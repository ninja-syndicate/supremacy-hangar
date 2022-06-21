using UnityEditor;
using UnityEditorInternal;

namespace BuildSystem
{
    public class WebGL : Base
    {
        protected override bool PreBuild(BuildParams parameters)
        {
            if (!base.PreBuild(parameters)) return false;
            if (parameters.DevelopmentMode) {
                BuildPlayerOptions.options ^= BuildOptions.AllowDebugging;
            }
            BuildPlayerOptions.target = BuildTarget.WebGL;
            BuildPlayerOptions.scenes = GatherAllScenes();
            BuildPlayerOptions.locationPathName = "Builds/WebGL";
            return true;
        }

        protected override bool PreAddressables(BuildParams parameters)
        {
            if (!base.PreAddressables(parameters)) return false;
            EditorUserBuildSettings.SwitchActiveBuildTarget( BuildTargetGroup.WebGL, BuildTarget.WebGL );
            EditorUserBuildSettings.selectedStandaloneTarget = BuildTarget.WebGL;
            return true;
        }
    }
}