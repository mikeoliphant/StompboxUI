namespace StompboxAPITest
{
    using StompboxAPI;

    internal class Program
    {
        static void Main(string[] args)
        {
            StompboxProcessor processor = new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "stompbox"), dawMode: true);
        }
    }
}
