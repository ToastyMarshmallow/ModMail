using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using ModMail.Data;
using ModMail.Data.Entities;

namespace ModMail.Commands;

public class ThreadCommands : ApplicationCommandModule
{
    public DataContext DataContext { get; set; } = null!;
    public ModmailExtension ModmailExtension { get; set; } = null!;

    [SlashCommand("reply", "Replies to a thread")]
    [SlashRequireGuild]
    public async Task Reply(InteractionContext ctx, [Option("message", "the message to send")] string message)
    {
        var thread = DataContext.Find<ThreadEntity>(ctx.Channel.Id);
        await ctx.DeferAsync(true);
        var res = await ctx.GetOriginalResponseAsync();
        var e = await ModmailExtension.SendDmMessage(thread!, res, message, anonymous: false, mod: ctx.Member, react:false);
        await ModmailExtension.SendReplyWebhookMessageInThread(ctx.Channel, message, false, ctx.Member);
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(e ? "Message Sent!" : "Idk something shat itself"));
    }
    
    [SlashCommand("areply", "Replies to a thread anonymously")]
    [SlashRequireGuild]
    public async Task AReply(InteractionContext ctx, [Option("message", "the message to send")] string message)
    {
        var thread = DataContext.Find<ThreadEntity>(ctx.Channel.Id);
        await ctx.DeferAsync(true);
        var res = await ctx.GetOriginalResponseAsync();
        var e = await ModmailExtension.SendDmMessage(thread!, res, message, anonymous: true, react:false);
        await ModmailExtension.SendReplyWebhookMessageInThread(ctx.Channel, message, true);
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(e ? "Message Sent!" : "Idk something shat itself"));
    }
    
    [SlashCommand("close", "Closes the thread")]
    [SlashRequireGuild]
    public async Task Close(InteractionContext ctx,
        [Option("message", "The message to close the thread with")] string message = null!)
    {
        var threadEntity = DataContext.Find<ThreadEntity>(ctx.Channel.Id);
        var log = DataContext.Find<GuildEntity>(ctx.Guild.Id)!.Log;
        await ModmailExtension.CloseThread(threadEntity!, ctx.Guild.GetChannel(log), ctx, message);
    }

    public override async Task<bool> BeforeSlashExecutionAsync(InteractionContext ctx)
    {
        var thread = DataContext.Find<ThreadEntity>(ctx.Channel.Id);
        var invoke = thread != null;
        if (!invoke)
            await ctx.CreateResponseAsync("Not a thread");
        return invoke;
    }
}