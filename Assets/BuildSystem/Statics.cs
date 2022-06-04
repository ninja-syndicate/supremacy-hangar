using UnityEditor;

namespace BuildSystem
{
    public static class Statics
    {
        [MenuItem("Builds/WebGL")]
        public static void BuildWebGLTest()
        {
            var builder = new WebGL();
            builder.DoBuild();
        }
        
    }
}