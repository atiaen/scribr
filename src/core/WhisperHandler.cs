using System.Text;
using Whisper.net;
using Whisper.net.Logger;

public static class WhisperHandler
{


    public static string UseWhisper(string fileName)
    {
        using var whisperLogger = LogProvider.AddConsoleLogging(WhisperLogLevel.Debug);

        using var whisperFactory = WhisperFactory.FromPath("ggml-base.bin");
        var sb = new StringBuilder();

        // This section creates the processor object which is used to process the audio file, it uses language `auto` to detect the language of the audio file.
        // It also sets the segment event handler, which is called every time a new segment is detected.
        using var processor = whisperFactory.CreateBuilder()
            .WithLanguage("auto")
            .WithSegmentEventHandler((segment) =>
            {
                // Do whetever you want with your segment here.
                sb.AppendLine($"{segment.Text}");

            })
            .Build();

        using var fileStream = File.OpenRead(fileName);
        processor.Process(fileStream);

        return sb.ToString();
    }

    public static async Task UseWhisperWithAsync(string fileName)
    {
        using var whisperLogger = LogProvider.AddConsoleLogging(WhisperLogLevel.Debug);

        using var whisperFactory = WhisperFactory.FromPath("ggml-base.bin");

        using var processor = whisperFactory.CreateBuilder()
            .WithLanguage("auto")
            .Build();

        using var fileStream = File.OpenRead(fileName);
        var sb = new StringBuilder();
        await foreach (var result in processor.ProcessAsync(fileStream))
        {
            Console.WriteLine($"{result.Start}->{result.End}: {result.Text}");
            sb.AppendLine($"{result.Text}");

        }
        // processor.ProcessAsync(fileStream);
    }
}