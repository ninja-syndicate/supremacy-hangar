using System;
using System.Collections.Generic;
using UnityEngine;

namespace BuildSystem
{
    public struct BuildParams
    {
        private const string ExecuteMethod = "executemethod";

        private const string DevelopmentModeEnable = "-development";

        public bool DevelopmentMode { get; private set; }

        public void UpdateFromEnvironment()
        {
            
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

                if (foundExecute && !foundMethod)
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
                case DevelopmentModeEnable:
                    DevelopmentMode = true;
                    break;
                default:
                    Debug.LogError($"Unknown CLI Parameter! {parameter}");
                    break;
            }
        }
    }
}