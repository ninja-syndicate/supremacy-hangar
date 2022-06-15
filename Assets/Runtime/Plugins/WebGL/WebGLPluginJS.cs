using System.Runtime.InteropServices;

// Read more about creating JS plugins: https://www.patrykgalach.com/2020/04/27/unity-js-plugin/

/// <summary>
/// Class with a JS Plugin functions for WebGL.
/// </summary>

namespace SupremacyHangar.Runtime.Plugins.WebGL
{
    public static class WebGLPluginJS
    {
        // Importing "SiloReady"
        [DllImport("__Internal")]
        public static extern void SiloReady();
    }
}