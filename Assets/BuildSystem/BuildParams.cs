using System;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

namespace BuildSystem
{
    public struct BuildParams
    {
        public enum AddressablesLocationType
        {
            Local,
            Development,
            Staging,
            Production
        }
        
        private const string ExecuteMethod = "executemethod";

        private static readonly string[] EnvTrueValues = new[] { "true", "t", "yes", "y" };

        private const string DevelopmentModeEnableCLI = "-development";
        private const string DevelopmentModeEnableEnv = "DEVELOPMENT_MODE";

        private const string AddressablesLocationCLI = "-addressableslocation";
        private const string LocalAddressablesEnableENV = "ADDRESSABLES_LOCATION";

        public bool DevelopmentMode { get; private set; }
        public bool BuildAddressables { get; private set; } 

        public AddressablesLocationType AddressablesLocation { get; private set; }
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
                AddressablesLocation = ParseAddressablesLocation(envVars[DevelopmentModeEnableEnv].ToString());
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
        
        public static BuildParams WithValues(bool development, AddressablesLocationType addressablesLocation)
        {
            return new BuildParams()
            {
                DevelopmentMode = development,
                AddressablesLocation = addressablesLocation,
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
                case AddressablesLocationCLI:
                    AddressablesLocation = ParseAddressablesLocation(paramQueue.Dequeue());
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
        
        private AddressablesLocationType ParseAddressablesLocation(string locationString)
        {
            locationString = locationString.ToLowerInvariant();
            switch (locationString)
            {
                case "local":
                    return AddressablesLocationType.Local;
                case "dev":
                case "development":
                    return AddressablesLocationType.Development;
                case "staging":
                    return AddressablesLocationType.Staging;
                case "prod":
                case "production":
                    return AddressablesLocationType.Production;
                default:
                    Debug.LogError("Unknown addressables location, defaulting to local");
                    return AddressablesLocationType.Local;
            }
        }
    }
}