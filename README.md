# Supremacy Hangar

This is the Unity hangar (because: mobile and WebGL) for players to see their mechs.

## Deployment Targets
  - WebGL

## Pre-requisites
  1. Git client with LFS support
  2. Unity 2021.3.3f1 ([Unity Hub Link](unityhub://2021.3.4f1/cb45f9cae8b7))

## Building from the command line

### Unity build "application"
Unity applications are built via the installed editor. This will usually be located at:
 - Windows
    - Hub: `C:\Program Files\Unity\Hub\Editor\<VERSION>\Editor\Unity.exe`
 - macOS
    - Hub: `/Applications/Unity/Hub/Editor/<VERSION>/Unity/Contents/MacOS/Unity`

Unity documents the command line parameters [here](https://docs.unity3d.com/Manual/EditorCommandLineArguments.html)

Unless the intention is to spawn an interactive session, the `-batchmode` and `-quit` parameters should be specified. Beware of specifying `-nographics` as while the documentation implies only Enlighten won't work - it should be thoroughly tested.

Despite the documentation implying otherwise `-buildTarget` should always be specified with the target platform.

The `-logFile` method should be specified on windows, as the log will not go to the console output, and must be logged to a file.

The `-executeMethod` parameter should be specified last with the desired static method to execute. The method cannot take arguments and you must omit the `()` from it.


### Build System Methods in Project

All the build functions are located in `BuildSystem.CLI`.

#### PreWarm

If run as part of a build script, the `PreWarm()` method should be run in a small loop before moving onto the main build to ensure all code is built properly for the target platform.

#### Build for Platform

| Method     | Matching buildTarget | Output Directory/File | Description            |
|------------|----------------------|-----------------------|------------------------|
| BuildWebGL | WebGL                | Builds/WebGL          | Build the WebGL Player |

### Build System Parameters / Environment Variables

Parameters should be specified _after_ the -executeMethod argument. 

| CLI Param               | Env. Var             | Default  | Description                                                                                                                                                            |
|-------------------------|----------------------|----------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `-development`          | `DEVELOPMENT_MODE`   | disabled | If specified (CLI) or true (ENV), build the project in development mode with debugging support enabled                                                                 |
| `-addressablesLocation` | `LOCAL_ADDRESSABLES` | local    | Addressables can come from:<ul><li>the build itself "local"</li><li>the dev server "dev"</li><li>the staging server "staging"</li><li>the prod server "prod"</li></ul> |
  
### Examples

 * Windows Editor 2021.3.4f1 installed via Hub
 * WebGL Target
 * Project in "E:\Ninja\supremacy-hangar"
 * log to log.txt in current dir
 * Run pre-warm
```
"C:\Program Files\Unity\Hub\Editor\2021.3.4f1\Editor\Unity.exe" -batchmode -quit -buildTarget WebGL -projectPath "E:\Ninja\supremacy-hangar" -logFile log.txt -executeMethod BuildSystem.CLI.PreWarm
```

* Windows Editor 2021.3.4f1 installed via Hub
* WebGL Target
* Project in "E:\Ninja\supremacy-hangar"
* log to log.txt in current dir
* Build for WebGL
* Build a development mode player
```
"C:\Program Files\Unity\Hub\Editor\2021.3.4f1\Editor\Unity.exe" -batchmode -quit -buildTarget WebGL -projectPath "E:\Ninja\supremacy-hangar" -logFile log.txt -executeMethod BuildSystem.CLI.BuildWebGL -development
```