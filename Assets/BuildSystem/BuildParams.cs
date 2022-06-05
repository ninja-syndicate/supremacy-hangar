using System;
using System.Collections.Generic;
using UnityEngine;

namespace BuildSystem
{
    public struct BuildParams
    {
        private const string ExecuteMethod = "executemethod";

        private static readonly string[] EnvTrueValues = new[] { "true", "t", "yes", "y" };

        private const string DevelopmentModeEnableCLI = "-development";
        private const string DevelopmentModeEnableEnv = "DEVELOPMENT_MODE";

        public bool DevelopmentMode { get; private set; }

        public void UpdateFromEnvironment()
        {
            var envVars = Environment.GetEnvironmentVariables();
            if (envVars.Contains(DevelopmentModeEnableEnv)) 
            {
                DevelopmentMode = StringIsTrue(envVars[DevelopmentModeEnableEnv].ToString());
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
        
        public static BuildParams WithValues(bool development)
        {
            return new BuildParams()
            {
                DevelopmentMode = development,
            };
        }

        private void ParseCLIParam(string parameter, Queue<string> paramQueue)
        {
            switch (parameter.ToLowerInvariant())
            {
                case DevelopmentModeEnableCLI:
                    DevelopmentMode = true;
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