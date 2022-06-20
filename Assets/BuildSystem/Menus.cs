using UnityEditor;

namespace BuildSystem
{
    public static class Menus
    {
        private static bool DevelopmentMode = true;
        private static bool LocalAddressables = true;

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

        [MenuItem("Builds/Settings/Local Addressables", true)]
        public static bool EnableLocalAddressablesValidate()
        {
            return !LocalAddressables;
        }

        [MenuItem("Builds/Settings/Local Addressables", false)]
        public static void EnableLocalAddressables()
        {
            LocalAddressables = true;
        }

        [MenuItem("Builds/Settings/Remote Addressables", true)]
        public static bool DisableLocalAddressablesValidate()
        {
            return LocalAddressables;
        }

        [MenuItem("Builds/Settings/Remote Addressables", false)]
        public static void DisableLocalAddressables()
        {
            LocalAddressables = false;
        }        
        
        [MenuItem("Builds/Platforms/WebGL")]
        public static void BuildWebGL()
        {
            var builder = new WebGL();
            builder.DoBuild(GetBuildParams());
        }

        private static BuildParams GetBuildParams()
        {
            return BuildParams.WithValues(DevelopmentMode, LocalAddressables);
        }
    }
}