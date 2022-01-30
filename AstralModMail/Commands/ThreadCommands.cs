using AstralModMail.Data;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace AstralModMail.Commands;

public class ThreadCommands : ApplicationCommandModule
{
    public DataContext DataContext { get; set; } = null!;
    public ModmailExtension ModmailExtension { get; set; } = null!;

    [SlashCommand("reply", "Replies to a thread")]
    public async Task Reply(InteractionContext ctx, [Option("message", "the message to send")] string message)
    {
        var thread = DataContext.Find<ThreadEntity>(ctx.Channel.Id);
        await ctx.DeferAsync();
        var res = await ctx.GetOriginalResponseAsync();
        var e = await ModmailExtension.SendDmMessage(thread!, res, message, anonymous: false, mod: ctx.Member);
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(e ? "Message Sent!" : "Idk something shat itself"));
    }
    
    [SlashCommand("areply", "Replies to a thread anonymously")]
    public async Task AReply(InteractionContext ctx, [Option("message", "the message to send")] string message)
    {
        var thread = DataContext.Find<ThreadEntity>(ctx.Channel.Id);
        await ctx.DeferAsync();
        var res = await ctx.GetOriginalResponseAsync();
        var e = await ModmailExtension.SendDmMessage(thread!, res, message, anonymous: true);
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(e ? "Message Sent!" : "Idk something shat itself"));
    }
    
    [SlashCommand("close", "Closes the thread")]
    public async Task Close(InteractionContext ctx,
        [Option("message", "The message to close the thread with")] string message = null!)
    {
        var threadEntity = DataContext.Find<ThreadEntity>(ctx.Channel.Id);
        var log = DataContext.Find<GuildEntity>(ctx.Guild.Id)!.Log;
        await ModmailExtension.CloseThread(threadEntity!, ctx.Guild.GetChannel(log), ctx, message);
    }

    public override Task<bool> BeforeSlashExecutionAsync(InteractionContext ctx)
    {
        var thread = DataContext.Find<ThreadEntity>(ctx.Channel.Id);
        return Task.FromResult(thread != null);
    }
}