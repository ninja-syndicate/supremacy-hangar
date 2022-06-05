using UnityEditor;

namespace BuildSystem
{
    public class WebGL : Base
    {
        protected override bool PreBuild(BuildParams parameters)
        {
            if (!base.PreBuild(parameters)) return false;
            BuildPlayerOptions.target = BuildTarget.WebGL;
            BuildPlayerOptions.scenes = GatherAllScenes();
            BuildPlayerOptions.locationPathName = "Builds/WebGL";
            return true;
        }
    }
}