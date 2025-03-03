using Vosk;

public class VoskHandler
{

    public void Start()
    {

    }
    public static string ReadFile(string pathToFile)
    {
        // Model model = new Model("/home/deck/Documents/Projects/AI&LLMS/Scriber/src/models/model");
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

    //Todo: Allow users to import models first. What I mean is open a dialog or something that allows users to select a zip file
    // that zip file will then be extracted into a folder and we just read from that folder rather than expecting a models folder to be there.
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