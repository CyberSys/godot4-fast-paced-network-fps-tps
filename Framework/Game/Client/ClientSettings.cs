using System.Collections.Generic;
using Framework.Game.Server;
using System.Collections.ObjectModel;

namespace Framework.Game.Client
{
    /// <summary>
    /// Static class for client settings
    /// </summary>
    public static class ClientSettings
    {
        /// <summary>
        /// Contains all vars for the client instance
        /// </summary>
        /// <value></value>
        public static VarsCollection Variables { get; set; }

        public static readonly string[] Resolutions = new string[] {
           "640x480", "1280x1024", "1280x960", "1280x800","1280x768","1280x720", "1920x1080"
        };

        public enum WindowModes
        {
            Windowed,
            Borderless,
            Fullscreen,
            ExclusiveFullscreen
        };

        public static readonly Dictionary<string, int> ShadowQualities = new Dictionary<string, int> {
           {"Disabled", 0},
           {"UltraLow", 1024},
           {"Low", 2048},
           {"Middle", 4096},
           {"High", 8192},
        };
    }
}