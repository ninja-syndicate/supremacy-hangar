using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build.Pipeline.Utilities;
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

            if (parameters.BuildAddressables)
            {
                if (!PreAddressables(parameters)) return;
                if (!BuildAddressables(parameters)) return;
                if (!PostAddressables(parameters)) return;
            }
            
            if (!PreBuild(parameters)) return;
            if (!Build(parameters)) return;
            if (!PostBuild(parameters)) return;
        }

        
        protected virtual bool PreAddressables(BuildParams parameters) => true;
        protected virtual bool PostAddressables(BuildParams parameters) => true;

        protected virtual bool PreBuild(BuildParams parameters) => true;
        protected virtual bool PostBuild(BuildParams parameters) => true;


        private bool BuildAddressables(BuildParams parameters)
        {
            if (!SetAddressableBundleLocation(parameters)) return false;
            AddressableAssetSettings.CleanPlayerContent();
            BuildCache.PurgeCache(false);
            AddressableAssetSettings.BuildPlayerContent();
            
            return true;
        }

        private bool SetAddressableBundleLocation(BuildParams parameters)
        {
            var buildPath = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(parameters.BuildPath)) buildPath.Append(parameters.BuildPath);
            if (!string.IsNullOrWhiteSpace(parameters.BuildNumber)) buildPath.Append(parameters.BuildNumber);
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (parameters.LocalAddressables)
            {
                Debug.Log("Setting Build and Load paths to Local");
                SetAddressablePathTypes(settings, "LocalBuildPath", "LocalLoadPath");
                return true;
            }

            try
            {
                Debug.Log("Setting Build and Load paths to Remote");
                SetAddressablePathTypes(settings, "RemoteBuildPath", "RemoteLoadPath");
                if (buildPath.Length == 0) return true;
                string buildPathString = buildPath.ToString();
                List<string> profileNames = settings.profileSettings.GetAllProfileNames();
                foreach (string profileName in profileNames)
                {
                    Debug.Log($"Setting {profileName} build path to {buildPathString}");
                    string id = settings.profileSettings.GetProfileId(profileName);
                    settings.profileSettings.SetValue(id, "BuildPath", buildPathString);
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("Could not set build path");
                Debug.LogException(e);
                return false;
            }
        }

        private static bool SetAddressablePathTypes(AddressableAssetSettings settings, string buildPath,
            string loadPath)
        {
            bool success = true;
            settings.RemoteCatalogBuildPath.SetVariableByName(settings, buildPath);
            settings.RemoteCatalogLoadPath.SetVariableByName(settings, loadPath);

            foreach (var group in settings.groups)
            {
                if (group.ReadOnly) continue;
                var bundleSchema = group.GetSchema<BundledAssetGroupSchema>();
                if (bundleSchema == null)
                {
                    Debug.LogError($"Could not find group schema for group {group.name}");
                    success = false;
                    continue;
                }
                
                bundleSchema.BuildPath.SetVariableByName(settings, buildPath);
                bundleSchema.LoadPath.SetVariableByName(settings, loadPath);
            }

            return success;
        }
        
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