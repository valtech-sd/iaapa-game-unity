# uni-iaapa
Unity project repository for the (BEAT THE BUZZ) mini games for the IAAPA event 2022 

## Included In This Project
1. TextMeshPro
2. DoozyUI

## Software Requirements
1. [Git](https://git-scm.com/book/en/v2/Getting-Started-Installing-Git) to use the repository.
2. [Git LFS](https://git-lfs.github.com/) to access the large asset files in the repository.
3. [Unity Hub](https://unity3d.com/get-unity/download) to manage Unity versions and projects and to install the Unity editor.
4. [a script editor](https://www.dunebook.com/best-unity-ide/) to edit scripts.

## How To Pull the Project
1. Download all needed tools from listed software requirements above.
2. [Clone the repository](https://docs.github.com/en/repositories/creating-and-managing-repositories/cloning-a-repository) from GitHub.
3. If previously cloned and just need to get code updates, then `git pull`.  
4. Make sure to use LFS to pull all files: `git lfs pull`.  Otherwise, you may get errors from missing expected files.
5. In Unity Hub > Installs, install the Unity Editor for the LTS version used by the repository currently, 2021.3.11f1.

## How To Finish Setting Up the Project
1. In your local copy of the project, go to Assets/StreamingAssets.
2. Copy `config.template.json` onto `config.json` in the same directory.
3. Edit the new `config.json` file to enter secret values we are not committing to version control:
    - message broker / rabbit mq:
      - user
      - pass

## How To View the Game
1. In Unity Hub > Projects, open the local repository for the project.
2. This should open the Unity Editor scene view by default.
3. In Unity Editor > bottom section > Project tab > Scenes folder, click on "Main" to open the main scene.
4. In Unity Editor > top middle section, click on Play button to start the scene. NOTE: You will get ERRORS if you did not finish setting up the config file above.
5. Click on the same button to end the scene.

## How To Make Changes to the Project
1. In Unity Hub > Projects, open the local repository for the project.
2. Edit the scenes and components via the Unity Editor.
3. Edit the scripts via your script editor.

## How To Push Changes to the Project
1. `git status` to check file changed.
2. `git restore {file}` to unstage files that should not be committed.
3. `git add {file}` to stage files that should be committed.
4. `git commit -m "{change info}"` to commit the files.
5. `git push` to push changes to GitHub.

## Useful References
- [How to set up a Unity project in GitHub](https://unityatscale.com/unity-version-control-guide/how-to-setup-unity-project-on-github/)
- [Unity SmartMerge Instructions](https://github.com/anacat/unity-mergetool)
- [Unity SmartMerge](https://docs.unity3d.com/Manual/SmartMerge.html)
