using AstralModMail;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .MinimumLevel.Debug()
    .CreateLogger();

var bot = new Bot();
await bot.StartAsync();
await Task.Delay(-1);