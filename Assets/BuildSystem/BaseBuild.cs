using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BuildSystem
{
    public abstract class Base
    {
        protected BuildPlayerOptions BuildPlayerOptions = new BuildPlayerOptions();

        public void PreWarm()
        {
            
        }
        
        public void DoBuild(bool developerMode = false)
        {
            if (developerMode) {
                BuildPlayerOptions.options |= BuildOptions.Development;
                BuildPlayerOptions.options |= BuildOptions.AllowDebugging;
            }
            if (!PreBuild()) return;
            if (!Build()) return;
            if (!PostBuild()) return;
        }

        protected virtual bool PreBuild() => true;

        protected virtual bool PostBuild() => true;

        private bool Build()
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
            int sceneCount = SceneManager.sceneCountInBuildSettings;
            string[] sceneList = new string[sceneCount];
            for (int i = 0; i < sceneCount; i++) sceneList[i] = SceneManager.GetSceneByBuildIndex(i).path;
            return sceneList;
        }
    }
}