using UnityEngine;

namespace BuildSystem
{
    public static class CLI
    {
        public static void PreWarm()
        {
            Debug.Log("Prewarm Successful");
        }
        
        public static void BuildWebGL()
        {
            var builder = new WebGL();
            builder.DoBuild();
        }
    }
}