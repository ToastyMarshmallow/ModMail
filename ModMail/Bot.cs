using System.ComponentModel.Design;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Microsoft.Extensions.Logging;
using ModMail.Commands;
using ModMail.Data;
using Serilog;

namespace ModMail;

public class Bot
{
    private readonly DiscordShardedClient _client;
    private DataContext _dataContext = null!;

    public Bot()
    {
        var token = Environment.GetEnvironmentVariable("TOKEN");
        var discordConfig = new DiscordConfiguration
        {
            Token = token,
            TokenType = TokenType.Bot,
            MinimumLogLevel = LogLevel.Trace,
            LoggerFactory = new LoggerFactory().AddSerilog(),
            Intents = DiscordIntents.All,
        };
        _client = new DiscordShardedClient(discordConfig);
        _client.ChannelDeleted += OnChannelDeleted;
        _client.ComponentInteractionCreated += async (_, args) =>
        {
            await args.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
            var exist = Extensions.Handlers.TryGetValue(args.Id, out var action);
            if (exist)
                await action!(args);
        };
        _client.GuildDownloadCompleted += ClientOnGuildDownloadCompleted;
        _client.MessageCreated += OnMessageCreated;
        _client.GuildMemberAdded += ClientOnGuildMemberAdded;
        _client.GuildMemberRemoved += ClientOnGuildMemberRemoved;
    }

    private async Task ClientOnGuildMemberRemoved(DiscordClient sender, GuildMemberRemoveEventArgs e)
    {
        var thread = _dataContext.Threads.FirstOrDefault(x => x.Recipient == e.Member.Id);
        if (thread == null)
            return;
        var channel = e.Guild.GetChannel(thread.Channel);
        await channel.SendMessageAsync(new DiscordEmbedBuilder
        {
            Title = "The recipient has left the server!",
            Color = new DiscordColor("2F3136")
        });
    }

    private async Task ClientOnGuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs e)
    {
        var thread = _dataContext.Threads.FirstOrDefault(x => x.Recipient == e.Member.Id);
        if (thread == null)
            return;
        var channel = e.Guild.GetChannel(thread.Channel);
        await channel.SendMessageAsync(new DiscordEmbedBuilder
        {
            Title = "The recipient has joined the server!",
            Color = new DiscordColor("2F3136")
        });
    }

    private async Task OnMessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        var guild = _dataContext.Find<GuildEntity>(Convert.ToUInt64(Environment.GetEnvironmentVariable("GUILD_ID")));
        if (e.Author.IsBot || guild == null)
            return;
        if (!(e.Channel.IsPrivate || (e.Channel.Parent?.Id ?? 0) == guild.Category))
            return;
        var thread = e.Channel.IsPrivate ? _dataContext.Threads.FirstOrDefault(x => x.Recipient == e.Author.Id)! : _dataContext.Find<ThreadEntity>(e.Channel.Id)!;
        if (thread == null! && e.Channel.IsPrivate)
        {

            try
            {
                var user = e.Author;
                var g = sender.Guilds[Convert.ToUInt64(Environment.GetEnvironmentVariable("GUILD_ID"))];
                // TODO remove hardcode
                var member = await g.GetMemberAsync(user.Id);
                var ext = sender.GetModmail();
                var threadChannel = await ext.CreateThread(g.GetChannel(guild.Log), member,
                    "The moderation team has been notified of your ticket");
                var threadEntity = new ThreadEntity
                {
                    Channel = threadChannel.Id,
                    Recipient = user.Id,
                    Guild = g.Id
                };
                await ext.SendWebhookMessageInThread(threadEntity, e.Message);
                _dataContext.Add(threadEntity);
                await _dataContext.SaveChangesAsync();
                return;
            }
            catch
            {
                await e.Message.Error();
                throw;
            }
        }

        if (thread != null && e.Channel.IsPrivate)
        { 
            var ext = sender.GetModmail();
            await ext.SendWebhookMessageInThread(thread, e.Message);
        }
    }

    private async Task ClientOnGuildDownloadCompleted(DiscordClient sender, GuildDownloadCompletedEventArgs e)
    {
        await sender.UpdateStatusAsync(new DiscordActivity("with your soul", ActivityType.Playing), UserStatus.Idle, DateTimeOffset.Now);
        
        // check for deleted channels

        Log.Information("Checking for deleted channels");
        foreach (var discordGuild in e.Guilds.Values)
        {
            var guild = _dataContext.Guilds.FirstOrDefault(x => x.GuildId == discordGuild.Id);
            if (guild == null) return;
            var category = discordGuild.GetChannel(guild.Category);
            var logs = discordGuild.GetChannel(guild.Log);
            var ext = sender.GetModmail();
            var threads = _dataContext.Threads.Where(x => x.Guild == discordGuild.Id).ToArray();
            if (logs == null && category != null)
            {
                logs = await ext.CreateLogChannel(category);
                guild.Log = logs.Id;
                _dataContext.Update(guild);
                await _dataContext.SaveChangesAsync();
                Log.Information("Logs channel was deleted in {Name}", discordGuild.Name);
            }
            else if (logs != null && category == null)
            {
                category = await ext.CreateCategoryChannel(discordGuild, logs.PermissionOverwrites);
                guild.Category = category.Id;
                _dataContext.Update(guild);
                await _dataContext.SaveChangesAsync();
                Log.Information("Category channel was deleted in {Name}", discordGuild.Name);
            }

            if (category == null && logs == null)
            {
                var discordChannel = threads.Select(x => discordGuild.GetChannel(x.Channel)).FirstOrDefault(x => x != null);
                if (discordChannel == null)
                {
                    foreach (var thread in threads)
                    {
                        _dataContext.Remove(thread);
                    }

                    _dataContext.Remove(guild);
                    await _dataContext.SaveChangesAsync();
                    Log.Information("{Name} got wiped completely", discordGuild.Name);
                    return;
                }
                Log.Information("Category and logs were deleted in {Name}", discordGuild.Name);
                category = await ext.CreateCategoryChannel(discordGuild, discordChannel.PermissionOverwrites);
                guild.Category = category.Id;
                logs = await ext.CreateLogChannel(category);
                guild.Log = logs.Id;
                _dataContext.Update(guild);
                await _dataContext.SaveChangesAsync();
            }
            foreach (var thread in threads)
            {
                var channel = discordGuild.GetChannel(thread.Channel);
                if (channel != null) continue;
                await ext.ThreadDeleted(thread, logs!, await discordGuild.GetMemberAsync(thread.Recipient), null!,
                    null!);
                _dataContext.Threads.Remove(thread);
                await _dataContext.SaveChangesAsync();
            }
            await ext.SetThreadParent(guild);
        }
    }

    private async Task OnChannelDeleted(DiscordClient sender, ChannelDeleteEventArgs e)
    {
        var guild = _dataContext.Guilds.FirstOrDefault(x => x.GuildId == e.Guild.Id);
        if (guild == null)
            return;
        var ext = sender.GetModmail();
        if (e.Channel.Id == guild.Log)
        {
            Log.Information("Log channel was deleted in {Name}", e.Guild.Name);
            var log = await ext.CreateLogChannel(e.Guild.GetChannel(guild.Category));
            guild.Log = log.Id;
            _dataContext.Update(guild);
            await _dataContext.SaveChangesAsync();
            return;
        }

        if (e.Channel.Id == guild.Category)
        {
            Log.Information("Category channel was deleted in {Name}", e.Guild.Name);
            var logs = e.Guild.GetChannel(guild.Log);
            var category = await ext.CreateCategoryChannel(e.Guild, logs.PermissionOverwrites);
            guild.Category = category.Id;
            _dataContext.Update(guild);
            await _dataContext.SaveChangesAsync();
            await logs.ModifyAsync(x => x.Parent = category);
            await ext.SetThreadParent(guild);
            return;
        }

        var thread = _dataContext.Find<ThreadEntity>(e.Channel.Id);
        if (thread != null)
        {
            _dataContext.Remove(thread);
            var member = await e.Guild.GetMemberAsync(thread.Recipient);
            var log = e.Guild.GetChannel(guild.Log);
            var audit = (await e.Guild.GetAuditLogsAsync(1, action_type: AuditLogActionType.ChannelDelete))[0];
            var user = audit.UserResponsible;
            var reason = audit.Reason;
            await ext.ThreadDeleted(thread, log, member, user, reason);
            await _dataContext.SaveChangesAsync();
        }
    }

    public async Task StartAsync()
    {
        await _client.StartAsync();
        _dataContext = new DataContext();
        var service = new ServiceContainer();
        service.AddService(typeof(DataContext), _dataContext);
        foreach (var (_, client) in _client.ShardClients)
        {
            var modmailExt = new ModmailExtension(_dataContext);
            client.AddExtension(modmailExt);
            service.AddService(typeof(ModmailExtension), modmailExt);
            var slashCommandConfig = new SlashCommandsConfiguration
            {
                Services = service,
            };
            var slashCommandsExtension = client.UseSlashCommands(slashCommandConfig);
            slashCommandsExtension.SlashCommandErrored += async (_, args) =>
            {
                var reply = args.Exception switch
                {
                    SlashExecutionChecksFailedException checksException => checksException.FailedChecks[0] switch
                    {
                        SlashRequireBotPermissionsAttribute => "Missing Bot permissions",
                        SlashRequireDirectMessageAttribute => "DM only",
                        SlashRequireGuildAttribute => "Guild only",
                        SlashRequireOwnerAttribute => "Owner only",
                        SlashRequirePermissionsAttribute => "Missing Bot or User permissions",
                        SlashRequireUserPermissionsAttribute => "Missing User Permissions",
                        _ => throw new ArgumentOutOfRangeException(),
                    },
                    _ => "Unknown Error"
                };
                await args.Context.CreateResponseAsync(reply);
            };
            slashCommandsExtension.ContextMenuErrored += async (_, args) =>
            {
                var reply = args.Exception switch
                {
                    SlashExecutionChecksFailedException checksException => checksException.FailedChecks[0] switch
                    {
                        SlashRequireBotPermissionsAttribute => "Missing Bot permissions",
                        SlashRequireDirectMessageAttribute => "DM only",
                        SlashRequireGuildAttribute => "Guild only",
                        SlashRequireOwnerAttribute => "Owner only",
                        SlashRequirePermissionsAttribute => "Missing Bot or User permissions",
                        SlashRequireUserPermissionsAttribute => "Missing User Permissions",
                        _ => throw new ArgumentOutOfRangeException(),
                    },
                    _ => "Unknown Error"
                };
                await args.Context.CreateResponseAsync(reply);
            };
            slashCommandsExtension.RegisterCommands<CoreCommands>(Convert.ToUInt64(Environment.GetEnvironmentVariable("GUILD_ID")));
            slashCommandsExtension.RegisterCommands<ThreadCommands>(Convert.ToUInt64(Environment.GetEnvironmentVariable("GUILD_ID")));
        }
    }
}