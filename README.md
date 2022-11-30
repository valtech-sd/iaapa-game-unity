# iaapa-game-unity
Unity project repository for the "BEAT THE BUZZ" game for IAAPA 2022 TES After-Party

## Included In This Project
1. TextMeshPro
2. DoozyUI
3. RabbitMQ ([See Note](#rabbitmq))

## Software Requirements
1. [Git](https://git-scm.com/book/en/v2/Getting-Started-Installing-Git) to use the repository.
2. [Git LFS](https://git-lfs.github.com/) to access the large asset files in the repository.
3. [Unity Hub](https://unity3d.com/get-unity/download) to manage Unity versions and projects and to install the Unity editor.
4. [a script editor](https://www.dunebook.com/best-unity-ide/) to edit scripts.

## Additional Reqquirements
1. [IAAPA Game Rules Engine](https://github.com/valtech-sd/iaapa-game-rules-engine)

## How To Get
### Clone (To Edit and Run)
1. Download all needed tools from listed software requirements above.
2. [Clone the repository](https://docs.github.com/en/repositories/creating-and-managing-repositories/cloning-a-repository) from GitHub.
3. If previously cloned and just need to get code updates, then `git pull`.  
4. Make sure to use LFS to pull all files: `git lfs pull`.  Otherwise, you may get errors from missing expected files.
5. In Unity Hub > Installs, install the Unity Editor for the LTS version used by the repository currently, 2021.3.11f1.
### Build (To Only Run)
1. TBD -- Builds are currently not published / saved to Git.

## How To Finish Setting Up (For BOTH Clone OR Build)
This unity needs to subscribe to message broker queues in the [IAAPA Game Rules Engine](https://github.com/valtech-sd/iaapa-game-rules-engine) for game info. 
1. In your local copy or build of the project, go to Assets/StreamingAssets.
2. Copy `config.template.json` onto `config.json` in the same directory.
3. Edit the new `config.json` file to enter secret values we are not committing to version control:
	- message broker / rabbit mq:
	  - user
	  - pass

## How To Run
### From Clone
1. In Unity Hub > Projects, open the local repository for the project.
2. This should open the Unity Editor scene view by default.
3. In Unity Editor > bottom section > Project tab > Scenes folder, click on "Main" to open the main scene.
4. In Unity Editor > top middle section, click on Play button to start the scene. NOTE: You will get ERRORS if you did not finish setting up the config file above.
5. Click on the same button to end the scene.
### From Build
1. Click on the build executable.

## How To Test with Simulator
Running this app does not do much without receiving messages published to the message broker.  To receive a message sequence, run the simulator.
1. Go to local directory for [IAAPA Game Rules Engine](https://github.com/valtech-sd/iaapa-game-rules-engine).
2. `cd simulator`
3. `yarn sim:showcontrol`

## How To Edit
1. In Unity Hub > Projects, open the local [clone](#clone-to-edit-and-run) of the project.
2. Edit the scenes and components via the Unity Editor.
3. Edit the scripts via your script editor.

## How To Upload Changes
1. In Terminal, go to the [clone](#clone-to-edit-and-run) directory of the project.
2. `git status` to check file changed.
3. `git restore {file}` to unstage files that should not be committed.
4. `git add {file}` to stage files that should be committed.
5. `git commit -m "{change info}"` to commit the files.
6. `git push` to push changes to GitHub.

## How To Build 
1. In Unity Editor, go to File > Build Settings...
2. Select "Windows, Mac, Linux" as Platform.
3. Select desired "Target Platform" from dropdown.
4. Select desired platform "Architecture" from dropdown.
5. Click "Build" button.
6. Save build file to the "Builds" subdirectory of the project.


## Notes
### RabbitMQ
This project currently uses .NET standard 2.1.  
In case of upgrades, the setting is in Unity Editor > Edit > Project Settings > Player > Other Settings > Configuration.
For more info: see [Scripting Runtime in Unity](https://learn.microsoft.com/en-us/visualstudio/gamedev/unity/unity-scripting-upgrade#enabling-the-net-4x-scripting-runtime-in-unity).

CymaticLabs created a [Unity wrapper for the .NET RabbitMQ client](https://github.com/CymaticLabs/Unity3D.Amqp).
However, we are not using any of their sample scripts or actual wrapper methods because it is for an outdated version of Unity.

Instead, we have added just [their DLLs](https://github.com/CymaticLabs/Unity3D.Amqp/tree/master/unity/CymaticLabs.UnityAmqp/Assets/CymaticLabs/Amqp/Plugins) as plugins as per [Unity Scripting Upgrade](https://learn.microsoft.com/en-us/visualstudio/gamedev/unity/unity-scripting-upgrade).

We are not using the latest (6.0) .NET RabbitMQ client DLL directly because it:

- requires also adding additional libs:

	- System.Runtime.CompilerServices.Unsafe
	- System.Threading.Channels

- requires updating some method signatures
- still seems to present the threading issues the old CymaticLabs wrapper tried to fix

## Useful References
- [How to set up a Unity project in GitHub](https://unityatscale.com/unity-version-control-guide/how-to-setup-unity-project-on-github/)
- [Unity SmartMerge Instructions](https://github.com/anacat/unity-mergetool)
- [Unity SmartMerge](https://docs.unity3d.com/Manual/SmartMerge.html)
