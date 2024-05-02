namespace Extensions.Unity.ImageLoader
{
    public static partial class ImageLoader
    {
        public static readonly Settings settings = new Settings();
    }
    public partial class Settings
    {
        public DebugLevel debugLevel = DebugLevel.Warning;
        public bool useMemoryCache = true;
#if UNITY_WEBGL 
        public bool useDiskCache = false; // default value for WebGL = false
#else
        public bool useDiskCache = true;  // default value for non WebGL = true
#endif
        public string diskSaveLocation { get; set; } = UnityEngine.Application.persistentDataPath + "/imageCache";
    }

    public enum DebugLevel
    {
        Log         = 0,
        Warning     = 1,
        Error       = 2,
        Exception   = 3,
        None        = 4
    }
}