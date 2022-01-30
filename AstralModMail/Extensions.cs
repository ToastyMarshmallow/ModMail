using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace AstralModMail;

public static class Extensions
{
    public delegate Task ComponentHandler(ComponentInteractionCreateEventArgs e);
    
    public static readonly Dictionary<string, ComponentHandler> Handlers = new();

    public static DiscordComponent AddHandler(this DiscordComponent component, ComponentHandler handler)
    {
        Handlers.Add(component.CustomId, handler);
        return component;
    }
    
    public static ModmailExtension GetModmail(this DiscordClient client)
    {
        return client.GetExtension<ModmailExtension>();
    }

    public static async Task Success(this DiscordMessage message)
    {
        await message.CreateReactionAsync(DiscordEmoji.FromUnicode("✅"));
    }
    
    public static async Task Error(this DiscordMessage message)
    {
        await message.CreateReactionAsync(DiscordEmoji.FromUnicode("❌"));
    }
}