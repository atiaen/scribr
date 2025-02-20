namespace Scriber;

class Program
{
    public static int screenWidth = 500;
    public static int screenHeight = 550;
    static void Main(string[] args)
    {
        Console.WriteLine("Starting Scribr");
        Scribr app = new Scribr();
        app.Start();
        app.Run();
        app.Stop();
    }
}
