using UnityEditor;

namespace BuildSystem
{
    public static class Menus
    {
        private static bool DevelopmentMode = true;

        [MenuItem("Builds/Settings/Enable Development Mode", true)]
        public static bool EnableDeveloperModeValidate()
        {
            return !DevelopmentMode;
        }

        [MenuItem("Builds/Settings/Enable Development Mode", false)]
        public static void EnableDeveloperMode()
        {
            DevelopmentMode = true;
        }

        [MenuItem("Builds/Settings/Disable Development Mode", true)]
        public static bool DisableDeveloperModeValidate()
        {
            return DevelopmentMode;
        }

        [MenuItem("Builds/Settings/Disable Development Mode", false)]
        public static void DisableDeveloperMode()
        {
            DevelopmentMode = false;
        }
        
        
        [MenuItem("Builds/Platforms/WebGL")]
        public static void BuildWebGL()
        {
            var builder = new WebGL();
            builder.DoBuild(DevelopmentMode);
        }
    }
}