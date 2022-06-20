using System;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

namespace BuildSystem
{
    public struct BuildParams
    {
        private const string ExecuteMethod = "executemethod";

        private static readonly string[] EnvTrueValues = new[] { "true", "t", "yes", "y" };

        private const string DevelopmentModeEnableCLI = "-development";
        private const string DevelopmentModeEnableEnv = "DEVELOPMENT_MODE";

        private const string LocalAddressablesEnableCLI = "-localaddressables";
        private const string LocalAddressablesEnableENV = "LOCAL_ADDRESSABLES";

        public bool DevelopmentMode { get; private set; }
        public bool BuildAddressables { get; private set; } 
        public bool LocalAddressables { get; private set; }
        
        public string BuildPath { get; private set; }
        public string BuildNumber { get; private set; }

        public void UpdateFromEnvironment()
        {
            var envVars = Environment.GetEnvironmentVariables();
            if (envVars.Contains(DevelopmentModeEnableEnv)) 
            {
                DevelopmentMode = StringIsTrue(envVars[DevelopmentModeEnableEnv].ToString());
            }

            if (envVars.Contains(LocalAddressablesEnableENV))
            {
                LocalAddressables = StringIsTrue(envVars[DevelopmentModeEnableEnv].ToString());
            }
        }

        public void UpdateFromCLI()
        {
            bool foundExecute = false;
            bool foundMethod = false;

            var argQueue = new Queue<string>(Environment.GetCommandLineArgs());
            while (argQueue.Count > 0)
            {
                var parameter = argQueue.Dequeue();
                if (!foundExecute)
                {
                    if (parameter.ToLowerInvariant().EndsWith(ExecuteMethod)) foundExecute = true;
                    continue;
                }

                if (!foundMethod)
                {
                    foundMethod = true;
                    continue;
                }
                
                ParseCLIParam(parameter, argQueue);
            }
        }
        
        public static BuildParams WithValues(bool development, bool localAddressables)
        {
            return new BuildParams()
            {
                DevelopmentMode = development,
                LocalAddressables = localAddressables,
                //TODO: this should come from a project config file...
                BuildAddressables = true,
            };
        }

        private void ParseCLIParam(string parameter, Queue<string> paramQueue)
        {
            switch (parameter.ToLowerInvariant())
            {
                case DevelopmentModeEnableCLI:
                    DevelopmentMode = true;
                    break;
                case LocalAddressablesEnableCLI:
                    LocalAddressables = true;
                    break;
                default:
                    Debug.LogError($"Unknown CLI Parameter! {parameter}");
                    break;
            }
        }

        private bool StringIsTrue(string value)
        {
            foreach (var envTrueValue in EnvTrueValues)
            {
                if (value.Equals(envTrueValue, StringComparison.InvariantCultureIgnoreCase)) return true;
            }

            return false;
        }
    }
}