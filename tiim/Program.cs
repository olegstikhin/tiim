using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.VoiceNext;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using tiim.Commands;

namespace tiim
{
  class Program
  {
    static void Main(string[] args)
    {
      MainAsync().GetAwaiter().GetResult();
    }

    static async Task MainAsync()
    {
      var discord = new DiscordClient(new DiscordConfiguration()
      {
        Token = Environment.GetEnvironmentVariable("DISCORD_BOT"),
        TokenType = TokenType.Bot,
        MinimumLogLevel = LogLevel.Debug
      });

      var services = new ServiceCollection()
        .AddSingleton<Random>()
        .BuildServiceProvider();

      var commands = discord.UseCommandsNext(new CommandsNextConfiguration()
      {
        Services = services,
        StringPrefixes = new[] { "!" }
      });

      discord.UseVoiceNext(new VoiceNextConfiguration()
      {
        EnableIncoming = true
      });

      commands.RegisterCommands<MyModule>();

      await discord.ConnectAsync();
      await Task.Delay(-1);
    }

  }
}
