using AstralModMail.Data;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Serilog;

namespace AstralModMail.Commands;

public class CoreCommands : ApplicationCommandModule
{
    public DataContext DataContext { get; set; } = null!;
    
    public ModmailExtension ModmailExtension { get; set; } = null!;

    
    [SlashCommand("ping", "Pong!")]
    public async Task Ping(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync("Pong! The latency is " + ctx.Client.Ping + "ms.");
    }
    
    [SlashCommand("setup", "Makes channels and stuff")]
    [SlashRequireUserPermissions(Permissions.Administrator)]
    public async Task Setup(InteractionContext ctx)
    {
        Log.Information("Setting up {Name}", ctx.Guild.Name);
        try
        {
            await ctx.DeferAsync();
            var db = DataContext;
            var exists = db.Guilds.Any(x => x.GuildId == ctx.Guild.Id);
            if (exists)
            {
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder()
                    .WithContent("Already setup!"));
                return;
            }

            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder()
                .WithContent("Setting up..."));
            var oOverwrites = ctx.Channel.PermissionOverwrites;
            var categoryChannel = await ModmailExtension.CreateCategoryChannel(ctx.Guild, oOverwrites);
            var logsChannel = await ModmailExtension.CreateLogChannel(categoryChannel);
            var guild = new GuildEntity
            {
                Category = categoryChannel.Id,
                GuildId = ctx.Guild.Id,
                Log = logsChannel.Id,
            };
            db.Add(guild);
            await db.SaveChangesAsync();
            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder()
                .WithContent("Setup complete!"));
        }
        catch
        {
            var msg = await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder()
                .WithContent("Setup failed!"));
            await msg.Error();
            throw;
        }
    }

    [SlashCommand("contact", "Start a thread with a user")]
    [SlashRequireUserPermissions(Permissions.ManageChannels)]
    public async Task Contact(InteractionContext ctx, [Option("user", "The user to contact")] DiscordUser user)
    {
        var db = DataContext;
        var guild = db.Guilds.FirstOrDefault(x => x.GuildId == ctx.Guild.Id);
        if (guild == null)
        {
            await ctx.CreateResponseAsync("You need to run the setup command first!");
            var res = await ctx.GetOriginalResponseAsync();
            await res.Error();
            return;
        }
        
        if (db.Threads.Any(x => x.Guild == ctx.Guild.Id && x.Recipient == user.Id))
        {
            await ctx.CreateResponseAsync("That user already has a thread!");
            var res = await ctx.GetOriginalResponseAsync();
            await res.Error();
            return;
        }

        if (user.IsBot)
        {
            await ctx.CreateResponseAsync("r/therewasanattempt");
            var res = await ctx.GetOriginalResponseAsync();
            await res.Error();
            return;
        }

        DiscordMember member;
        try
        {
            member = await ctx.Guild.GetMemberAsync(user.Id);
            if (member == null)
                throw new Exception("User not found");
        }
        catch
        {
            await ctx.CreateResponseAsync("User not found");
            var res = await ctx.GetOriginalResponseAsync();
            await res.Error();
            return;
        }

        try
        {
            await ctx.DeferAsync();
            var logs = ctx.Guild.GetChannel(guild.Log);
            var thread =
                await ModmailExtension.CreateThread(logs, member, $"A mod has contacted you from {ctx.Guild.Name}");
            var threadEntity = new ThreadEntity
            {
                Channel = thread.Id,
                Recipient = member.Id,
                Guild = guild.GuildId,
            };
            var msg = await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder()
                .WithContent(Formatter.Mention(thread) + $" `#{thread.Name} ({thread.Id})`"));
            await msg.Success();
            db.Add(threadEntity);
            await db.SaveChangesAsync();
        }
        catch
        {
            var msg = await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder()
                .WithContent("Something went wrong"));
            await msg.Error();
            throw;
        }
    }
    
    [ContextMenu(ApplicationCommandType.UserContextMenu,"Contact")]
    [SlashRequireUserPermissions(Permissions.ManageChannels)]
    public async Task Contact(ContextMenuContext ctx)
    {
        var member = ctx.TargetMember;
        var db = DataContext;
        var guild = db.Guilds.FirstOrDefault(x => x.GuildId == ctx.Guild.Id);
        if (guild == null)
        {
            await ctx.CreateResponseAsync("You need to run the setup command first!");
            var res = await ctx.GetOriginalResponseAsync();
            await res.Error();
            return;
        }

        if (member.IsBot)
        {
            await ctx.CreateResponseAsync("r/therewasanattempt");
            var res = await ctx.GetOriginalResponseAsync();
            await res.Error();
            return;
        }
        
        if (db.Threads.Any(x => x.Guild == ctx.Guild.Id && x.Recipient == member.Id))
        {
            await ctx.CreateResponseAsync("That user already has a thread!");
            var res = await ctx.GetOriginalResponseAsync();
            await res.Error();
            return;
        }

        try
        {
            await ctx.DeferAsync();
            var logs = ctx.Guild.GetChannel(guild.Log);
            var thread =
                await ModmailExtension.CreateThread(logs, member, $"A mod has contacted you from {ctx.Guild.Name}");
            var threadEntity = new ThreadEntity
            {
                Channel = thread.Id,
                Recipient = member.Id,
                Guild = guild.GuildId,
            };
            var msg = await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder()
                .WithContent(Formatter.Mention(thread) + $" `#{thread.Name} ({thread.Id})`"));
            await msg.Success();
            db.Add(threadEntity);
            await db.SaveChangesAsync();
        }
        catch
        {
            var msg = await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder()
                .WithContent("Something went wrong"));
            await msg.Error();
            throw;
        }
    }
}