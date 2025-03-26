using Vosk;

public class VoskHandler
{

    public static string ReadFile(string pathToFile)
    {
        Model model = new Model(Directory.GetCurrentDirectory() + "/src/models/model");
        SpkModel spkModel = new SpkModel(Directory.GetCurrentDirectory() + "/src/models/model/conf");
        VoskRecognizer rec = new VoskRecognizer(model, 48000.0f);
        rec.SetSpkModel(spkModel);

        using (Stream source = File.OpenRead(pathToFile))
        {
            byte[] buffer = new byte[4096];
            int bytesRead;
            while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                if (rec.AcceptWaveform(buffer, bytesRead))
                {
                    // Console.WriteLine(rec.Result());
                }
                else
                {
                    // Console.WriteLine(rec.PartialResult());
                }
            }
        }
        Console.WriteLine(rec.FinalResult());

        return rec.FinalResult();
    }

    public static string ReadFile2(string pathToFile)
    {
        Model model = new Model(Directory.GetCurrentDirectory() + "/model");

        // Demo byte buffer
        VoskRecognizer rec = new VoskRecognizer(model, 16000);
        rec.SetMaxAlternatives(0);
        rec.SetWords(true);
        using (Stream source = File.OpenRead(pathToFile))
        {
            byte[] buffer = new byte[4096];
            int bytesRead;
            while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                if (rec.AcceptWaveform(buffer, bytesRead))
                {
                    // Console.WriteLine(rec.Result());
                }
                else
                {
                    // Console.WriteLine(rec.PartialResult());
                }
            }
        }
        var FinalResult = rec.FinalResult();
        // Console.WriteLine("Final Result: \n" + FinalResult);
        return FinalResult;

    }
}