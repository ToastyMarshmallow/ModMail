// Gone cuz slash commands

/*using System.Reflection;
using AstralModMail.Providers;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;

namespace AstralModMail.Commands;

public partial class CoreCommands
{
    [SlashCommandGroup("help", "Lists all commands")]
    public class Help
    {
        [SlashCommand("group", "Lists all commands in a group")]
        public async Task Group(InteractionContext ctx, 
            [ChoiceProvider(typeof(GroupChoiceProvider))]
            [Option("group", "Group to list commands for")] string group)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = $"Group: {group}",
                Color = new DiscordColor("#2F3136"),
                Description = "Here are the commands in the " + group + " group:"
            };
            var ext = ctx.Client.GetCommandsNext();
            var c = ext.RegisteredCommands.Values
                .First(x => x.Module.ModuleType.Name.ToLower() == group);
            var methods = c.Module.ModuleType.GetMethods().Where(x => x.GetCustomAttribute<CommandAttribute>() is not null);
            foreach (var method in methods)
            {
                var name = method.GetCustomAttribute<CommandAttribute>()!.Name;
                var description = method.GetCustomAttribute<DescriptionAttribute>()!.Description;
                embed.AddField(name, description, true);
            }

            var select = new DiscordSelectComponent("helpselect", "Command",
                    embed.Fields.Select(x => new DiscordSelectComponentOption(x.Name, x.Name)))
                .AddHandler(args => Handler(args, ext));
            
            var builder = new DiscordInteractionResponseBuilder()
                .AddEmbed(embed)
                .AddComponents(select);
            await ctx.CreateResponseAsync(builder);
        }

        private async Task Handler(ComponentInteractionCreateEventArgs obj, CommandsNextExtension ext)
        {
            var command = obj.Values[0];
            var embed = GetCommandEmbed(command, ext);
            await obj.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                .AddEmbed(embed)
                .AsEphemeral(true));
        }

        private DiscordEmbed GetCommandEmbed(string command, CommandsNextExtension ext)
        {
            var method = ext.RegisteredCommands.Values.Select(x => x.Module.ModuleType)
                .SelectMany(x => x.GetMethods())
                .First(x => x.GetCustomAttribute<CommandAttribute>()?.Name == command);
            var param = method.GetParameters();
            var args = param.Length > 1 ? param[1..] : null;
            var embed = new DiscordEmbedBuilder
            {
                Title = $"Command {command}",
                Color = new DiscordColor("#2F3136"),
                Description = "Information for the " + command + " command:"
            };
            var description = method.GetCustomAttribute<DescriptionAttribute>()!.Description;
            embed.AddField("Description", description + "\n**Arguments**:");
            foreach (var parameterInfo in args!)
            {
                var name = parameterInfo.Name![0].ToString().ToUpper() + parameterInfo.Name[1..];
                var argDescription = "> **Description**: " + (parameterInfo.GetCustomAttribute<DescriptionAttribute>()?.Description ?? "No description");
                argDescription += "\n> **Type**: " + parameterInfo.ParameterType.Name;
                argDescription += "\n> **Optional**: " + (parameterInfo.HasDefaultValue ? "Yes" : "No");
                if (parameterInfo.ParameterType.IsEnum)
                {
                    var oneOf = parameterInfo.ParameterType.GetEnumNames().Select(x => x.ToLower()).ToList();
                    argDescription += "\n> **OneOf**: " + string.Join(", ", oneOf);
                }
                argDescription += "\n> **Infinite**: " + (parameterInfo.GetCustomAttribute<RemainingTextAttribute>() is not null ? "Yes" : "No");
                
                embed.AddField(name, argDescription, true);
            }
            return embed;
        }

        [SlashCommand("command", "Displays information about specific command")]
        public async Task Command(InteractionContext ctx,
            [ChoiceProvider(typeof(CommandChoiceProvider))]
            [Option("command", "Command you want info for")] string command)
        {
            var embed = GetCommandEmbed(command, ctx.Client.GetCommandsNext());
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .AddEmbed(embed));
        }
    }
}*/