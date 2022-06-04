using UnityEditor;

namespace BuildSystem
{
    public class WebGL : Base
    {
        protected override bool PreBuild()
        {
            if (!base.PreBuild()) return false;
            BuildPlayerOptions.target = BuildTarget.WebGL;
            BuildPlayerOptions.scenes = GatherAllScenes();
            BuildPlayerOptions.locationPathName = "Builds/WebGL";
            return true;
        }
    }
}