using System;

namespace Extensions.Unity.ImageLoader
{
    public static partial class ImageLoader
    {
        /// <summary>
        /// The global settings for the ImageLoader
        /// </summary>
        public static readonly Settings settings = new Settings();
    }
    public partial class Settings
    {
        /// <summary>
        /// The level of debug messages that will be shown in the console.
        /// Default value is DebugLevel.Warning
        ///
        /// Trace ----- show all messages
        /// Log ------- show only Log, Warning, Error, Exception messages
        /// Warning --- show only Warning, Error, Exception messages
        /// Error ----- show only Error, Exception messages
        /// Exception - show only Exception messages
        /// None ------ show no messages
        /// </summary>
        public DebugLevel debugLevel = DebugLevel.Warning;
        public bool useMemoryCache = true;
#if UNITY_WEBGL
        public bool useDiskCache = false; // default value for WebGL = false
#else
        public bool useDiskCache = true;  // default value for non WebGL = true
#endif
        /// <summary>
        /// The location where the images will be saved on disk.
        /// If not set, it will default to UnityEngine.Application.persistentDataPath + "/ImageLoader"
        /// </summary>
        public string diskSaveLocation { get; set; } = UnityEngine.Application.persistentDataPath + "/ImageLoader";

        /// <summary>
        /// The timeout for the web requests
        /// Default value is 30 seconds
        /// </summary>
        public TimeSpan timeout = TimeSpan.FromSeconds(30);
    }

    public enum DebugLevel
    {
        Trace       = -1, // show all messages
        Log         = 0,  // show only Log, Warning, Error, Exception messages
        Warning     = 1,  // show only Warning, Error, Exception messages
        Error       = 2,  // show only Error, Exception messages
        Exception   = 3,  // show only Exception messages
        None        = 4   // show no messages
    }
    public static class DebugLevelEx
    {
        /// <summary>
        /// Check if the DebugLevel is active
        /// If it is active the related message will be shown in the console
        /// </summary>
        public static bool IsActive(this DebugLevel debugLevel, DebugLevel level) => debugLevel <= level;
    }
}