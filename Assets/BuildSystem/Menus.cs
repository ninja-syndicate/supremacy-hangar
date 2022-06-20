using UnityEditor;

namespace BuildSystem
{
    public static class Menus
    {
        private static bool DevelopmentMode = true;
        private static BuildParams.AddressablesLocationType AddressablesLocation =
            BuildParams.AddressablesLocationType.Local;

#region Development Mode        
        
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

#endregion        

#region AddressablesLocation 

#region Local Addressables Location
        [MenuItem("Builds/Settings/Addressable Location/Local", true)]
        public static bool EnableLocalAddressablesValidate()
        {
            return AddressablesLocation != BuildParams.AddressablesLocationType.Local;
        }

        [MenuItem("Builds/Settings/Addressable Location/Local", false)]
        public static void EnableLocalAddressables()
        {
            AddressablesLocation = BuildParams.AddressablesLocationType.Local;
        }
#endregion
        
#region Dev Addressables Location
        [MenuItem("Builds/Settings/Addressable Location/Dev", true)]
        public static bool EnableDevlAddressablesValidate()
        {
            return AddressablesLocation != BuildParams.AddressablesLocationType.Development;
        }

        [MenuItem("Builds/Settings/Addressable Location/Dev", false)]
        public static void EnableDevAddressables()
        {
            AddressablesLocation = BuildParams.AddressablesLocationType.Development;
        }
#endregion

#region Staging Addressables Location
        [MenuItem("Builds/Settings/Addressable Location/Staging", true)]
        public static bool EnableStagingAddressablesValidate()
        {
            return AddressablesLocation != BuildParams.AddressablesLocationType.Staging;
        }

        [MenuItem("Builds/Settings/Addressable Location/Staging", false)]
        public static void EnableStagingAddressables()
        {
            AddressablesLocation = BuildParams.AddressablesLocationType.Staging;
        }
#endregion

#region Production Addressables Location
        [MenuItem("Builds/Settings/Addressable Location/Prod", true)]
        public static bool EnableProdAddressablesValidate()
        {
            return AddressablesLocation != BuildParams.AddressablesLocationType.Production;
        }

        [MenuItem("Builds/Settings/Addressable Location/Prod", false)]
        public static void EnableProdAddressables()
        {
            AddressablesLocation = BuildParams.AddressablesLocationType.Production;
        }
#endregion

#endregion

        [MenuItem("Builds/Platforms/WebGL")]
        public static void BuildWebGL()
        {
            var builder = new WebGL();
            builder.DoBuild(GetBuildParams());
        }

        private static BuildParams GetBuildParams()
        {
            return BuildParams.WithValues(DevelopmentMode, AddressablesLocation);
        }
    }
}