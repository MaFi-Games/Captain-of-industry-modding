# Captain of industry modding

Welcome to the official modding resource for [Captain of Industry](https://captain-of-industry.com).
COI is currently [available on Steam](https://store.steampowered.com/app/1594320/Captain_of_Industry/?utm_source=GitHubModding) ans is in the Early Access stage.
While modding is possible, it is not officially supported yet, and updates to the game may break mods at any time as we are continuously working on improving the game, modding APIs, and documentation.

## Prerequisites

In order to start modding COI, you will need to:

1. Own Captain of Industry on Steam and have the game installed.
2. Read and agree to the Modding Policy at https://coigame.com/modding-policy
3. Have .NET framework 4.8 installed.
4. (optional) Have Unity 6000.0.50f1 installed (needed for asset creation and testing).

## Getting started

1. Create a new environmental variable `COI_ROOT` and set its value to the directory where the game is installed, for example `C:/Steam/steamapps/common/Captain of Industry`. You can get this path from the Steam client via `Properties...` -> `Local Files` -> `Browse`.
2. Fork/download this repo.
3. Compile the `ExampleMod` in the `Release` configuration located at `src/ExampleMod/ExampleMod.sln` via tools such as Visual Studio or `dotnet build` console command.
  - If there are issues with missing dependencies, check the `COI_ROOT` environmental variable, and try restarting (change to environmental variables is only applied after program restart).
4. Locate the resulting `ExampleMod.dll` in `/bin/Release/net48`.
5. Locate your COI user data folder, by default at `%APPDATA%/Captain of Industry`.
6. In the data folder, open or create directory `Mods`. Inside it, create a new directory that has the same name as your mod DLL, in our case it's `ExampleMod`. Copy the compiled `ExampleMod.dll` there, so that it is at `%APPDATA%/Captain of Industry/Mods/ExampleMod/ExampleMod.dll`. Note that the directory name and DLL name must match.
7. Launch the game and in `Miscellaneous` settings enable mod support and restart the game so that the setting takes effect.
8. Start a new game and see your new mod listed as available.

Congratulations, you are now running your mod in Captain of Industry!

## Troubleshooting

In case of issues, examine the logs in the `%APPDATA%/Captain of Industry/Logs` directory, they contain a lot of useful information. One log file is created for each game launch, and errors/exceptions are flushed to disk immediately, so you should see them without terminating the game. You can log your own entities using the `Mafi.Log` class.

If you'd like to discuss modding topics with the community and devs, visit our [Discord channel #modding-dev-general](https://discord.gg/JxmUbGsNRU) (you will need to assign yourself a `Mod creation` role in `#pick-your-roles-here` channel).
You can also file issues here on Github, but the response time from our team might be delayed.

## Assets creation

Assets such as icons or 3D models can be created using the Unity editor. It can also be used for debugging.

### Unity setup

One-time Unity setup needed for MaFi tools to function properly.

1. Download and install Unity 6000.0.50f1 from https://unity3d.com/unity/qa/lts-releases.
2. Locate the test scene in `src/ExampleMod.Unity`. Do not open it yet.
3. Create hard-links for necessary DLLs from your Steam installation by running the `src/ExampleMod.Unity/Assets/DLLs/create_dll_hardlinks.bat` batch file. You will need to run it under admin privileges (right-click, Run as Administrator).
   - Alternatively, you could also copy the DLLs in question to this directory but a hard link is better since any update to the original files will propagate.
4. Open the project in Unity. This can be done via Unity Hub by selecting `Open project from disk` in the `Projects` tab. Make sure you select the right Unity version if you have multiple installed.
5. Open the `ExampleModScene` by double-clicking on it in the `Project` pane (it's under `Assets` directory).
6. (optional) In Unity preferences (`Edit` -> `Preferences`), change the `External Script Editor` (under `External tools`) to the editor of your choosing. This should also generate the project files in case you need to open the .sln file.

### Creation of icon assets

Following steps describe how to package icons, for example for new products.

1. We recommend organizing assets in directories. Under the `Assets` directory create `<mod name>/<icons category>` directory, in our case that is `ExampleMod/ProductIcons`.
2. Copy images as png or jpg files to the newly created directory.
3. Configure newly imported textures to have `Sprite (2D and UI)` type and apply the change.
4. Assign the newly imported textures to any asset bundle from the drop-down menu on the bottom of the Inspector tab. You can create a new bundle called `asdf` or pick any existing one.
5. To use an icon in your mod (for example as a product icon), simply right-click on a texture and select `Copy Path`. That path can be used to load your prefab in the game.
6. Follow steps under the [Packaging asset bundles](#packaging-asset-bundles) to package the created assets.

Note: Unlike 3D models, textures do not need to have a `prefab` created.

### Creation of 3D model asset

Following steps describe how to create a 3D model template that is very beneficial in creation of 3D models of buildings.

1. Define a layout entity in your mod with desired layout specified using ASCII (see `ExampleMachineData.cs`).
2. Set the prefab path to `"TODO"` since we don't have a prefab yet.
3. Compile and deploy the mod.
4. Launch game with the newly created machine and run console command `generate_layout_entity_mesh_template` followed by your entity ID. This will generate an OBJ file in `%APPDATA%/Captain of Industry/Misc/GeneratedMeshTemplates` which represents a 3D bounding box of layout of your new entity with exact port locations.
5. If you don't have a 3D model, load the newly created template model to a 3D editor of your choice and create a 3D model that fits it. If you already have a 3D model, you can compare it to the generated template and edit the ASCII layout accordingly.
6. When the 3D model is complete, export FBX or OBJ and follow the next steps.

Following steps describe how to package a 3D model.

1. We recommend organizing assets in directories. Under the `Assets` directory create `<mod name>/<model name>` directory, in our case that is `ExampleMod/ExampleModel`.
2. Import 3D model files (OBJ, FBX, etc.) and textures (PNG, JPG, etc) to the model directory. You can simply use drag & drop. Creating a separate folder for each asset is recommended.
3. Drag the 3D model from Project pane to the Unity scene. Reset its position and rotation to all zeros using Inspector (you can use the three dots menu next to transform) and make it look as you want (add materials, etc.). Note: it is important that your asset has zero position and rotation. If you need to reposition it, change the 3D model or make a child object that can be moved.
4. Create a prefab by dragging the root object and dropping it to the project pane. This will make a new `.prefab` file. In our example we created the prefab in the `<mod name>` directory. 
5. Assign the newly created prefab to any asset bundle from the drop-down menu on the bottom of the Inspector tab. You can create a new temporary bundle called `asdf` or pick any existing one.
6. To use prefabs in your mod, simply right-click any prefab and select `Copy Path`. That path can be used to load your prefab in the game.
7. Follow steps under the [Packaging asset bundles](#packaging-asset-bundles) to package the created assets.

### Packaging asset bundles

Once your assets are ready, follow these steps to package them with your mod.
Mods are published in a directory with the same name as your mod DLL, in our case we have `ExampleMod/ExampleMod.dll`.
Now we can add asset bundles to the same directory.

1. Ensure that everything in Unity is saved (use `Ctrl+S`).
2. Build asset bundles by right-clicking anywhere in the project pane (on any file or empty area) and select `[MaFi] Build asset bundles`. After Unity is done processing, asset bundle files can be found in the `src/ExampleMod.Unity/AssetBundles` directory.
3. Copy contents of the `src/ExampleMod.Unity/AssetBundles` to the `AssetBundles` folder next to mod DLL, for example `%APPDATA%/Captain of Industry/Mods/ExampleMod/AssetBundles`.
4. (optional) If you want to make it neat, you really only need the asset bundles (files in format `YourPrefabName_xxxx`, without extension) and the `mafi_bundles.manifest` file. All other `.manifest` files could be removed as well as the `AssetBundles` file.

If you make any changes to your prefabs, simply rebuild asset bundles and copy the new files from the `AssetBundles` directory.

## Running game directly in Unity

As part of Update 3, we’ve spent a lot of work to make it possible for modders to run the game directly from Unity. This is how to make it work for you:

1. Complete the Unity Setup guide in Assets creation above.
2. Select the `Game` object in the Hierarchy tab.
3. In the Inspector tab, you should see 3 entries with missing game objects, replace them as follows (if not, just add new ones):
  - First: `GameMainMb`
  - Second: `GameDebugConfigStatelessMb`
  - Third: `GameDebugConfig`
4. In the GameMainMb, set the `Working Dir Path Override` to `%COI_ROOT%`, and the `Core Asset Bundle Path` to `%COI_ROOT%\AssetBundles`.
5. In the `Game Debug Config Stateless Mb` options, check `Skip splash screen`.
6. Select the Game tab and make sure that under Aspect ratio settings (top bar), the `Low Resolution Aspect Ratios` is unchecked and the Scale is set to 1x.
7. Press the Play button to start the game. Press play again to terminate it.

### Notes
- Unity does not reload static context on each run. Avoid usage of static variables, or refresh them with valid values on each launch.
- Most options from Game Debug Config won’t work properly, sorry!