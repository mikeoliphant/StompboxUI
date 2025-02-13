namespace StompboxAPITest
{
    using StompboxAPI;

    internal class Program
    {
        static void Main(string[] args)
        {
            StompboxProcessor processor = new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "stompbox"), dawMode: true);

            var pluginNames = processor.GetAllPlugins();

            UnmanagedAudioPlugin tuner = processor.CreatePlugin("Tuner");

            if (tuner != null)
            {
                tuner.Enabled = true;

                bool enabled = tuner.Enabled;

                string id = tuner.ID;
            }
        }
    }
}
