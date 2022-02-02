using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;
using ModMail.Data;
using ModMail.Data.Entities;
using Serilog;

namespace ModMail;

public class ModmailExtension : BaseExtension
{
    private readonly DataContext _dataContext;

    public ModmailExtension(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    protected override void Setup(DiscordClient client)
    {
        Client = client;
    }

    public async Task SendWebhookMessageInThread(ThreadEntity thread, DiscordMessage message, bool react = true)
    {
        try
        {
            var discordGuild = await Client.GetGuildAsync(thread.Guild);
            var channel = discordGuild.GetChannel(thread.Channel);
            var webhook = (await channel.GetWebhooksAsync()).FirstOrDefault(x => x.Name == "Modmail");
            if (webhook == null)
                webhook = await channel.CreateWebhookAsync("Modmail");
            var builder = new DiscordWebhookBuilder()
                .WithContent(message.Content.Replace("@", "\\@"))
                .WithAvatarUrl(message.Author.AvatarUrl)
                .WithUsername($"{message.Author.Username}#{message.Author.Discriminator}" + " (Recipient)");
            var client = new HttpClient();
            for (var index = 0; index < message.Attachments.Count; index++)
            {
                var attachment = message.Attachments[index];
                builder.AddFile(index + attachment.FileName, await client.GetStreamAsync(attachment.Url));
            }

            await webhook.ExecuteAsync(builder);
            if (react)
                await message.Success();
        }
        catch
        {
            if (react)
                await message.Error();
            throw; // log the message in console and stuff
        }
    }

    public async Task<bool> SendDmMessage(ThreadEntity thread, DiscordMessage discordMessage, string message,
        bool react = true, bool anonymous = true, DiscordMember mod = null!)
    {
        try
        {
            var member = await discordMessage.Channel.Guild.GetMemberAsync(thread.Recipient);
            if (member == null)
            {
                if (react)
                    await discordMessage.Error();
                await discordMessage.RespondAsync(new DiscordEmbedBuilder
                {
                    Title = "User not found",
                    Description = "The user you are trying to send a message to is not in the server anymore."
                });
                return false;
            }

            var embed = new DiscordEmbedBuilder()
                .WithDescription(message)
                .WithFooter($"Sent from {member.Guild.Name}")
                .WithColor(new DiscordColor("2F3136"));

            if (anonymous)
                embed
                    .WithAuthor("Anonymous", null, Client.CurrentUser.AvatarUrl);
            else
                embed
                    .WithAuthor(mod.Username, null, mod.AvatarUrl);

            var builder = new DiscordMessageBuilder()
                .AddEmbed(embed);
            if (discordMessage.Attachments.Count == 1 && discordMessage.Attachments[0].MediaType[..5] == "image")
            {
                embed.WithImageUrl(discordMessage.Attachments[0].Url);
                await member.SendMessageAsync(embed);
                return true;
            }

            var client = new HttpClient();
            var files = new Dictionary<string, Stream>();
            for (var index = 0; index < discordMessage.Attachments.Count; index++)
            {
                var attachment = discordMessage.Attachments[index];
                files.Add(index + attachment.FileName, await client.GetStreamAsync(attachment.Url));
            }

            await member.SendMessageAsync(files.Any() ? builder.WithFiles(files) : builder);
            if (react) 
                await discordMessage.Success();
            return true;
        }
        catch
        {
            if (react)
                await discordMessage.Error();
            return false;
        }
    }

    public async Task<DiscordChannel> CreateCategoryChannel(DiscordGuild guild,
        IEnumerable<DiscordOverwrite> overwrites)
    {
        Log.Information("Creating Category channel in {Name}", guild.Name);
        var overwritesList = new List<DiscordOverwriteBuilder>();
        foreach (var discordOverwrite in overwrites)
        {
#pragma warning disable CS0618
            var discordOverwriteBuilder = await new DiscordOverwriteBuilder().FromAsync(discordOverwrite);
#pragma warning restore CS0618
            overwritesList.Add(discordOverwriteBuilder);
        }

        var channel = await guild.CreateChannelAsync("Modmail", ChannelType.Category, overwrites: overwritesList);
        return channel;
    }

    public async Task<DiscordChannel> CreateLogChannel(DiscordChannel categoryChannel)
    {
        Log.Information("Creating Log channel in {Name}", categoryChannel.Guild.Name);
        var channel = await categoryChannel.Guild
            .CreateChannelAsync("Logs", ChannelType.Text, categoryChannel);
        await channel.ModifyAsync(x => x.Parent = categoryChannel);
        await channel.SendMessageAsync(new DiscordEmbedBuilder
        {
            Title = "Logs",
            Description = "This is now the logs channel for Modmail and you cant do nothing about it",
            Color = new DiscordColor("2F3136")
        });
        return channel;
    }

    public async Task<DiscordChannel> CreateThread(DiscordChannel logs, DiscordMember member, string openMessage,
        bool dm = true, DiscordMember mod = null!)
    {
        Log.Information("Creating thread for {Username} in {Name}", member.Username, logs.Guild.Name);
        var channel = await logs.Guild
            .CreateChannelAsync($"{member.Username}-{member.Discriminator}", ChannelType.Text, logs.Parent);
        await channel.CreateWebhookAsync("Modmail");
        var msg = await channel.SendMessageAsync(new DiscordEmbedBuilder
        {
            Title = $"{member.Username}#{member.Discriminator} `{member.Id}`",
            Description =
                $"{member.Mention} was created {Formatter.Timestamp(member.CreationTimestamp)}\nJoined on {Formatter.Timestamp(member.JoinedAt)}\n{string.Join("", member.Roles.Select(Formatter.Mention))}",
            Color = new DiscordColor("2F3136")
        });
        await msg.PinAsync();
        try
        {
            await member.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Thread Created",
                Description = openMessage,
                Color = new DiscordColor("2F3136")
            });
        }
        catch (UnauthorizedException)
        {
            await channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Can't DM",
                Description = "The user has either blocked Modmail or have their DMs disabled",
                Color = new DiscordColor("2F3136")
            });
        }

        await logs.SendMessageAsync(new DiscordEmbedBuilder
        {
            Title = "Thread Created",
            Description = dm
                ? $"{Formatter.Mention(channel)} `#{channel.Name}` was created by {Formatter.Mention(member)} `{member.Username}#{member.Discriminator} ({member.Id})` by DM"
                : $"{Formatter.Mention(channel)} `#{channel.Name}` was created by {Formatter.Mention(mod)} `{mod.Username}#{mod.Discriminator} ({mod.Id})` to contact {Formatter.Mention(member)} `{member.Username}#{member.Discriminator} ({member.Id})`",
            Color = new DiscordColor("2F3136")
        });
        return channel;
    }

    public async Task SendReplyWebhookMessageInThread(DiscordChannel channel, string message, bool anon = false, DiscordMember member = null!)
    {
        var webhook = (await channel.GetWebhooksAsync()).FirstOrDefault(x => x.Name == "Modmail") ?? await channel.CreateWebhookAsync("Modmail");
        await webhook.ExecuteAsync(new DiscordWebhookBuilder
        {
            Username = anon ? "Anonymous" : $"{member.Username}#{member.Discriminator}",
            AvatarUrl = anon ? Client.CurrentUser.AvatarUrl : member.AvatarUrl,
            Content = message
        });
    }

    public async Task CloseThread(ThreadEntity threadEntity, DiscordChannel log, InteractionContext ctx, string message)
    {
        Log.Information("Closing thread {@Thread:lj}", threadEntity);
        var member = await log.Guild.GetMemberAsync(threadEntity.Recipient);
        var user = await Client.GetUserAsync(threadEntity.Recipient);

        var embed = new DiscordEmbedBuilder()
            .WithTitle("Thread Closed")
            .WithDescription(
                message + $"{(string.IsNullOrEmpty(message) ? "" : "\n")}Replying will create a new thread")
            .WithFooter($"Sent from {log.Guild.Name}")
            .WithColor(new DiscordColor("2F3136"));

        if (member != null)
        {
            await member.SendMessageAsync(embed);
        }

        _dataContext.Remove(threadEntity);
        await _dataContext.SaveChangesAsync();
        await ctx.CreateResponseAsync("Thread closed getting all messages");
        await ctx.FollowUpAsync(
            new DiscordFollowupMessageBuilder().WithContent("Got all messages, will nuke the channel in 5 seconds"));
#pragma warning disable CS4014
        Task.Delay(5000).ContinueWith(async _ => await ctx.Channel.DeleteAsync());
#pragma warning restore CS4014
        var closeEmbed = new DiscordEmbedBuilder
        {
            Title = "Thread closed",
            Description = "The thread with the recipient " + user.Mention +
                          $" `{user.Username}#{user.Discriminator} ({user.Id})` has been closed by {ctx.Member.Mention} `{ctx.Member.Username}#{ctx.Member.Discriminator} ({ctx.Member.Id})` with the message `{(string.IsNullOrEmpty(message) ? "No message" : message)}`)",
            Color = new DiscordColor("2F3136")
        }; // TODO add the log url
        var logs = ctx.Guild.GetChannel(_dataContext.Find<GuildEntity>(ctx.Guild.Id)!.Log);
        await logs.SendMessageAsync(closeEmbed);
    }

    public async Task SetThreadParent(GuildEntity guildEntity)
    {
        var parent = (await Client.GetGuildAsync(guildEntity.GuildId)).GetChannel(guildEntity.Category);
        var threads = _dataContext.Threads
            .Where(x => x.Guild == guildEntity.GuildId)
            .Select(x => parent.Guild.GetChannel(x.Channel))
            .ToArray();
        foreach (var discordChannel in threads.Where(x => x != null).Where(x => (x?.Parent?.Id ?? 0) != guildEntity.Category))
        {
            Log.Information("Moving channel {Channel} to {Parent}", discordChannel.Id, guildEntity.Category);
            await discordChannel.ModifyAsync(x => x.Parent = parent);
        }
    }

    public async Task ThreadDeleted(ThreadEntity threadEntity, DiscordChannel log, DiscordMember? member,
        DiscordUser? user, string? reason)
    {
        Log.Information("{@Thread:lj} was deleted by {User} cuz {Reason}", threadEntity, user?.Username ?? "Unknown", reason ?? "No reason");
        try
        {
            await
                (await
                    (await Client.GetGuildAsync(threadEntity.Guild))
                    .GetMemberAsync(threadEntity.Recipient))
                .SendMessageAsync(new DiscordEmbedBuilder
                {
                    Title = "Thread closed",
                    Description = "Your thread channel was deleted",
                    Color = new DiscordColor("2F3136")
                });
            await log.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Thread closed",
                Description = "The thread with the recipient " + (member?.Mention ?? "idk they leave when i was slep") +
                              $" `{member?.Username ?? "--"}#{member?.Discriminator ?? "--"} ({member?.Id ?? 0})` has been closed (channel deleted by {user?.Mention ?? "@(idk they delete when i was slep)"} `{user?.Username ?? "--"}#{user?.Discriminator ?? "--"} ({user?.Id ?? 0})` with the reason `{(string.IsNullOrEmpty(reason) ? "No reason" : reason)}`)",
                Color = new DiscordColor("2F3136")
            });
        }
        catch
        {
            // ignored
        }
    }
}