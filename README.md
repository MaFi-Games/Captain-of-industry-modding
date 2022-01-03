# Captain of industry modding

Welcome to the official modding resource for video game [Captain of Industry](captain-of-industry.com).

## Status of modding support

The game is in closed beta stage and modding is not yet fully supported and documented.
However, we decided to release experimental modding support early to give our community an easy entry-point for experimentation.
Full modding support and documentation is planned in the second half of 2022.

## Prerequisites

1. Own Captain of Industry on Steam and have the game installed.
2. Have .NET framework installed.
3. (optional) Have Unity 2019.4 installed (only for asset creation)

## Getting started

1. Locate the CoI game files via right-click on the game title in the Steam client -> `Properties...` -> `Local Files` -> `Browse`.
2. Copy the game root path in your clipboard. In my case it is `D:\Steam\steamapps\common\Captain of Industry`.
3. Create an environmental variable `COI_ROOT` and paste the game root path from above. If you are on Windows, you can use the handy `Edit environmental variables` tool, just open the Start menu and type `Edit env` and you should see it.
4. Download this repo and open the solution with example mod. I like to use [Visual Studio](https://visualstudio.microsoft.com/) but feel free to use your favorite tools. In Visual Studio you should see all dependent assemblies linked correctly. If not and you are seeing a lot of errors, check you environmental `COI_ROOT` variable, try restarting.
5. Compile it, for example in `Release` configuration.
6. Locate the resulting `ExampleMod.dll` in `\bin\Release\net46` and copy it in a clipboard.
7. Find your CoI game files folder in your Documents, mine is at `D:\%USERNAME%\Documents\CaptainOfIndustry`.
8. Open or create directory `Mods`. There, create a new directory that has the same name as your mod DLL, in our case it is `ExampleMod`, and inside it paste the compiled dll.
9. Launch the game and in `Miscellaneous` settings enable mod support.
10. Restart the game.
11. Launch the new game and observe that your mod is loaded by locating a new node in the research tree (open using `G` key). In case of any errors, examine logs in the `Logs` directory. You can also examine logs where all loaded mods are listed.
12. Congratulations, you are now running your mod in Captain of Industry!

Note: Mods are currently bound to save files. It is not possible to add a mod to older save, or remove a mod from existing save. If you delete a mod DLL, any saves that were started with it won't be loadable.

## Assets creation

Assets such as icons or 3D models can be created using Unity editor. We currently use Unity 2019.4 and it is recommended to use the same version to avoid incompatibilities.

### Unity setup

1. Download and install Unity 2019.4 from https://unity3d.com/unity/qa/lts-releases.
2. Create a new Unity project from the 3D template, I usually use Unity Hub for this and select `New -> 2019.4.xx -> 3D`.
3. In the Unity project, under `Assets`, create a new directory called `DLLs` (use project tab). 
4. Locate the CoI game files via right-click on the game title in the Steam client -> `Properties...` -> `Local Files` -> `Browse`.
5. Locate game DLLS in the `Captain of Industry_Data\Managed` subfolder of the game files. From this folder copy following DLLs to the newly created `DLLs` directory, ideally all at once: `Mafi.dll`, `Mafi.Core.dll`, `Mafi.Base.dll`, `Mafi.Unity.dll`, `Facepunch.Steamworks.Win64.dll`, `Assembly-CSharp.dll`, `Mafi.ModsAuthoringSupport.dll`. If there are any issues with the dependency order, try restarting Unity.
6. Once all DLLs are loaded, you should see a new option `[MaFi] Build asset bundles` when right clicking in the project window (on any file or empty area). Now you are ready to start creating assets.

### Creation of asset bundle with 3D model

Following steps describe how to package a 3D model.

1. Ensure that your Unity project has a new directory under `Assets` with your mod name, in our case that is `ExampleMod`, create it if necessary.
2. Under the mod directory `Assets/ExampleMod`, import 3D model (incl. textures) to Unity by drag & drop of your files. Creating a separate folder for each asset is recommended.
3. Drag the 3D model file to the Unity scene, set its position and rotation to all zeros using Inspector and make it look as you want (add materials, etc.). Note: it is important that your asset has zero position and rotation. If you need to reposition it, edit the 3D model or make it a child that can be moved.
4. Create a prefab by dragging the root object and dropping it to the project pane. This will make a new `.prefab` file.
5. Assign the newly created prefab to any asset bundle from the drop-down menu on the bottom of the Inspector tab. You can create a new bundle called `asdf` or pick any existing one.
6. Right click anywhere in the project window (on any file or empty area) and select `[MaFi] Build asset bundles`. After Unity is done processing, notice that the AssetBundle  name of your prefab was set to a new value.
7. To use prefabs in your mod, simply right-click any prefab and select `Copy Path`. That path can be used to load your prefab in the game.

If you do any changes to your prefabs, simply repeat step #6 to re-compile asset bundles.

### Creation of asset bundle with texture or icon

Packaging icons (for example for new products) as an asset bundle is very easy.

1. Ensure that your Unity project has a new directory under `Assets` with your mod name, in our case that is `ExampleMod`, create it if necessary.
2. Under the mod directory `Assets/ExampleMod`, import images as png or jpg file by drag & drop. Creating a separate folder for each asset is recommended.
3. Configure newly imported textures to have `Sprite (2D and UI)` type.
4. Assign the newly imported textures to any asset bundle from the drop-down menu on the bottom of the Inspector tab. You can create a new bundle called `asdf` or pick any existing one.
5. Right click anywhere in the project window (on any file or empty area) and select `[MaFi] Build asset bundles`. After Unity is done processing, notice that the AssetBundle  name of your prefab was set to a new value.
6. To use an icon in your mod (for example as a product icon), simply right-click on a texture and select `Copy Path`. That path can be used to load your prefab in the game.

Note: Unlike 3D models, textures do not need to have `prefab` created.

### Packaging asset bundles to your mod

Once your Asset bundles are ready, follow these steps to package them with your mod.

1. Mods are published in a directory with the same name as your mod DLL, in our case we already have `ExampleMod/ExampleMod.dll`. Now we can add asset bundles to the same directory.
2. Locate the `AssetBundles` directory from in your Unity project root and copy it to the root of your mod dir, in our example `ExampleMod/AssetBundles`.
3. (optional) If you want to make it neat, you really only need the asset bundles (files in format `YourPrefabName_xxxx` without extension) and the `bundles.manifest` file. All other `.manifest` files could be removed as well as the `AssetBundles` file.

And that's it, all asset bundles will be now loadable in the game.

## Questions, issues

Note that mod support is experimental and APIs might change.
If you are having issues, always examine logs, they contain a lot of useful information.
If you'd like to discuss modding topics with the devs and the community visit our [Discord channel #alpha-modiding](https://discord.gg/JxmUbGsNRU).
