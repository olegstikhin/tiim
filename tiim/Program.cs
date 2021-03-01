using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace tiim
{
  class Program
  {
    public static void Main(string[] args)
    => new Program().MainAsync().GetAwaiter().GetResult();

    private DiscordSocketClient _client;
    public async Task MainAsync()
    {
      using (var services = ConfigureServices())
      {
        var client = services.GetRequiredService<DiscordSocketClient>();

        client.Log += Log;
        services.GetRequiredService<CommandService>().Log += Log;

        // Tokens should be considered secret data and never hard-coded.
        // We can read from the environment variable to avoid hardcoding.
        await client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("DISCORD_BOT"));
        await client.StartAsync();

        // Here we initialize the logic required to register our commands.
        await services.GetRequiredService<CommandHandler>().InstallCommandsAsync();

        await Task.Delay(Timeout.Infinite);
      }
    }
    private Task Log(LogMessage msg)
    {
      Console.WriteLine(msg.ToString());
      return Task.CompletedTask;
    }

    private ServiceProvider ConfigureServices()
    {
      return new ServiceCollection()
          .AddSingleton<DiscordSocketClient>()
          .AddSingleton<CommandService>()
          .AddSingleton<CommandHandler>()
          .BuildServiceProvider();
    }
  }
}
