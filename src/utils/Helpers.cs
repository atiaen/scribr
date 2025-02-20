using System.Text.Json;

public static class Helpers
{


    public static void WriteFileToPath(string fileNameLocation, string finalText)
    {
        try
        {
            // Create a new file 
            using (StreamWriter sw = File.CreateText(fileNameLocation))
            {
                sw.WriteLine(finalText);
            }
        }
        catch (Exception Ex)
        {
            Console.WriteLine(Ex.ToString());
        }
    }

    public static string ReadJSONString(string jsonString, string property)
    {
        var finalText = "";
        using (JsonDocument document = JsonDocument.Parse(jsonString))
        {
            JsonElement root = document.RootElement;
            JsonElement textElement = root.GetProperty(property);
            finalText = textElement.GetString();
            Console.WriteLine("Final: " + textElement.GetString());
        }

        return finalText;
    }

    public static string[] SeparateFromExtension(string path)
    {
        var firstSplit = path.Split(".")[0];
        var fileExtension = path.Split(".")[1];
        var secondSplit = firstSplit.Split("/");
        var final = secondSplit[secondSplit.Count() - 1];

        return [fileExtension, final];
    }

    public static string[] BreakdownQueueItem(string queueItem)
    {
        var splitText = queueItem.Split("|");
        var filePath = splitText[0];
        var fileName = splitText[1];
        var status = splitText[2];

        return [filePath, fileName, status];
    }
}