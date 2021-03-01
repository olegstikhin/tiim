using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using DSharpPlus.VoiceNext.EventArgs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace tiim.Commands
{
  public class MyModule : BaseCommandModule
  {
    public Random Rng { private get; set; } // Implied public setter.

    [Command("greet")]
    public async Task GreetCommand(CommandContext ctx, string name)
    {
      await ctx.RespondAsync($"Hello {name}! Are you tim?");
    }

    [Command("random")]
    public async Task RandomCommand(CommandContext ctx, int min, int max)
    {
      await ctx.RespondAsync($"Your number is: {Rng.Next(min, max)}");
    }

    [Command("join")]
    public async Task JoinCommand(CommandContext ctx, DiscordChannel channel = null)
    {
      channel ??= ctx.Member.VoiceState?.Channel;
      await channel.ConnectAsync();
    }

    [Command("play")]
    public async Task PlayCommand(CommandContext ctx, string path)
    {
      var vnext = ctx.Client.GetVoiceNext();
      var connection = vnext.GetConnection(ctx.Guild);

      var transmit = connection.GetTransmitSink();

      var pcm = ConvertAudioToPcm(path);
      await pcm.CopyToAsync(transmit);
      await pcm.DisposeAsync();
    }

    [Command("leave")]
    public async Task LeaveCommand(CommandContext ctx)
    {
      var vnext = ctx.Client.GetVoiceNext();
      var connection = vnext.GetConnection(ctx.Guild);

      connection.Disconnect();
    }

    private Stream ConvertAudioToPcm(string filePath)
    {
      var ffmpeg = Process.Start(new ProcessStartInfo
      {
        FileName = "ffmpeg",
        Arguments = $@"-i ""{filePath}"" -ac 2 -f s16le -ar 48000 pipe:1",
        RedirectStandardOutput = true,
        UseShellExecute = false
      });

      return ffmpeg.StandardOutput.BaseStream;
    }


    [Command("quickplay")]
    public async Task QuickPlayCommand(CommandContext ctx, DiscordChannel channel = null)
    {
      channel ??= ctx.Member.VoiceState?.Channel;
      var connection = await channel.ConnectAsync();

      var transmit = connection.GetTransmitSink();

      var pcm = ConvertAudioToPcm("mellan.wav");
      await pcm.CopyToAsync(transmit);
    }

    [Command("start")]
    public async Task StartCommand(CommandContext ctx, DiscordChannel channel = null)
    {
      channel ??= ctx.Member.VoiceState?.Channel;
      var connection = await channel.ConnectAsync();

      Directory.CreateDirectory("Output");
      connection.VoiceReceived += VoiceReceiveHandler;
    }


    [Command("stop")]
    public async Task StopCommand(CommandContext ctx)
    {
      var vnext = ctx.Client.GetVoiceNext();

      var connection = vnext.GetConnection(ctx.Guild);
      connection.VoiceReceived -= VoiceReceiveHandler;
      //connection.Dispose();

      //return Task.CompletedTask;
    }

    private async Task VoiceReceiveHandler(VoiceNextConnection connection, VoiceReceiveEventArgs args)
    {
      var fileName = DateTimeOffset.Now.ToUnixTimeMilliseconds();
      var ffmpeg = Process.Start(new ProcessStartInfo
      {
        FileName = "ffmpeg",
        Arguments = $@"-ac 1 -f s16le -ar 48000 -i pipe:0 -ac 2 -ar 44100 Output/{fileName}.wav",
        RedirectStandardInput = true
      });

      await ffmpeg.StandardInput.BaseStream.WriteAsync(args.PcmData);
      ffmpeg.Dispose();
    }
  }
}
