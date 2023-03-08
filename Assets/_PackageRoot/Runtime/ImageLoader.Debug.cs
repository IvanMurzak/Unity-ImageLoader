namespace Extensions.Unity.ImageLoader
{
    public static partial class ImageLoader
    {
        public static readonly Settings settings = new Settings();
    }
    public partial class Settings
    {
        public DebugMode debugMode = DebugMode.Error;
    }

    public enum DebugMode
    {
        Log         = 0,
        Warning     = 1,
        Error       = 2,
        Exception   = 3,
        None        = 4
    }
}