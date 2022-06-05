using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace BuildSystem
{
    public abstract class Base
    {
        protected BuildPlayerOptions BuildPlayerOptions = new BuildPlayerOptions();
        
        public void DoBuild(BuildParams parameters)
        {
            if (parameters.DevelopmentMode) {
                BuildPlayerOptions.options |= BuildOptions.Development;
                BuildPlayerOptions.options |= BuildOptions.AllowDebugging;
            }
            if (!PreBuild(parameters)) return;
            if (!Build(parameters)) return;
            if (!PostBuild(parameters)) return;
        }

        protected virtual bool PreBuild(BuildParams parameters) => true;

        protected virtual bool PostBuild(BuildParams parameters) => true;

        private bool Build(BuildParams parameters)
        {
            BuildReport report = BuildPipeline.BuildPlayer(BuildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Build completed");
                return true;
            }

            return false;
        }
        
        protected string[] GatherAllScenes()
        {
            var enabledScenes = 
                from editorScene in EditorBuildSettings.scenes
                where editorScene.enabled
                select editorScene;
            return (from editorScene in enabledScenes select editorScene.path).ToArray();
        }
    }
}