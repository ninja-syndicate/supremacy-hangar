# Supremacy Hangar

This is the Unity hangar (because: mobile and WebGL) for players to see their mechs.

## Deployment Targets
  - WebGL

## Pre-requisites
  1. Git client with LFS support
  2. Unity 2021.3.3f1 ([Unity Hub Link](unityhub://2021.3.3f1/af2e63e8f9bd))

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

The `-executeMethod` parameter should be specified last with the desired static method to execute. The method cannot take arguments and you must omit the `()` from it.

### Build System Methods in Project

All the build functions are located in `BuildSystem.CLI`.

#### PreWarm

If run as part of a build script, the `PreWarm()` method should be run in a small loop before moving onto the main build to ensure all code is built properly for the target platform.

#### Build for Platform

| Method | Matching buildTarget | Output Directory/File | Description |
|--------|----------------------|-----------------------|-------------|
| 

### Build System Parameters / Environment Variables

Parameters should be specified _after_ the -executeMethod argument. 

| CLI Param    | Env. Var         | Default  | Description                                                                                |
|--------------|------------------|----------|--------------------------------------------------------------------------------------------|
| -development | DEVELOPMENT_MODE | disabled | If enabled/specified, build the project in development mode with debugging support enabled |
|              |                  |          |                                                                                            |
|              |                  |          |                                                                                            |

### Examples

 - Editor 2021.3.4f1, windows, installed via Hub, Prewarm for WebGL, project in "C:\My Funky Project", run pre-warm
```
"C:\Program Files\Unity\Hub\Editor\2021.3.4f1\Editor\Unity.exe" -batchmode -quit -buildTarget WebGL -projectPath "C:\My Funky Project" -executeMethod BuildSystem.CLI.PreWarm()
```