using UnityEngine;

namespace BuildSystem
{
    public static class CLI
    {
        public static void PreWarm()
        {
            GetBuildParams();
            Debug.Log("Prewarm Successful");
        }
        
        public static void BuildWebGL()
        {
            var builder = new WebGL();
            builder.DoBuild(GetBuildParams());
        }
        
        private static BuildParams GetBuildParams()
        {
            var buildParams = new BuildParams();
            buildParams.UpdateFromEnvironment();
            buildParams.UpdateFromCLI();
            return buildParams;
        }
    }
}