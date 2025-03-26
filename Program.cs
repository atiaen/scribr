namespace Scribr;

class Program
{
    public static int screenWidth = 500;
    public static int screenHeight = 550;
    static void Main(string[] args)
    {
        Console.WriteLine("Starting Scribr");
        ScribrApp app = new ScribrApp();
        app.Start();
        app.Run();
        app.Stop();
    }
}
