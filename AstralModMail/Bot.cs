using AstralModMail.Commands;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;

namespace AstralModMail;

public class Bot
{
    private readonly DiscordClient _client;

    public Bot()
    {
        var json = File.ReadAllText("Config.json");
        var config = JsonConvert.DeserializeObject<Config>(json) ?? throw new InvalidOperationException("No Config");
        var discordConfig = new DiscordConfiguration
        {
            Token = config.Token,
            TokenType = TokenType.Bot,
            MinimumLogLevel = LogLevel.Debug,
            LoggerFactory = new LoggerFactory().AddSerilog()
        };
        _client = new DiscordClient(discordConfig);
        _client.ChannelDeleted += OnChannelDeleted;
        var commandsNextConfig = new CommandsNextConfiguration
        {
            CaseSensitive = false,
            StringPrefixes = new []{"m!"},
            EnableDms = false,
            EnableMentionPrefix = true,
            DmHelp = false,
            IgnoreExtraArguments = true,
            EnableDefaultHelp = false,
        };
        var commandsNextExtension = _client.UseCommandsNext(commandsNextConfig);
        commandsNextExtension.RegisterCommands<ReplyCommands>();
    }

    private Task OnChannelDeleted(DiscordClient sender, ChannelDeleteEventArgs e)
    {
        throw new NotImplementedException();
    }

    public async Task StartAsync()
    {
        var slashCommandConfig = new SlashCommandsConfiguration
        {
            
        };
        var slashCommandsExtension = _client.UseSlashCommands(slashCommandConfig);
        slashCommandsExtension.RegisterCommands(GetType().Assembly, 841890589640359946);
        await _client.ConnectAsync();
        await _client.UpdateStatusAsync(new DiscordActivity("with your soul", ActivityType.Playing), UserStatus.Online);
    }
}