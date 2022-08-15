# Captain of industry modding

Welcome to the official modding resource for the [Captain of Industry](https://captain-of-industry.com) game.

## Status of modding support

COI is currently [available on Steam](https://store.steampowered.com/app/1594320/Captain_of_Industry/?utm_source=GitHubModding) and it is in Early Access stage.
While modding is possible, it is not officially supported and documented yet and APIs may change.
Improved modding support and documentation is planned by the end of 2022.

## Prerequisites

In order to start modding COI, you will need to:

1. Own Captain of Industry on Steam and have the game installed.
2. Have .NET framework installed.
3. (optional) Have Unity 2021.6.f1 installed (needed only for asset creation).

## Getting started

1. Locate the COI game files via right-click on the game title in the Steam client -> `Properties...` -> `Local Files` -> `Browse`.
2. Copy the game root path to your clipboard. The path in our case is `D:/Steam/steamapps/common/Captain of Industry`.
3. Create a new environmental variable `COI_ROOT` equal to the game root path copied above. On Windows, use the handy `Edit environmental variables` tool, just open the Start menu and type `Edit env` and you should see it.
4. Fork/download this repo.
5. Compile the `ExampleMod` in `Release` configuration located at `src/ExampleMod/ExampleMod.sln`. We recommend to use [Visual Studio](https://visualstudio.microsoft.com/) but feel free to use any other tools, such as the `dotnet build` console command. In Visual Studio you should see all dependent assemblies linked correctly. If not, and you are seeing a lot of errors, check you environmental `COI_ROOT` variable, try restarting.
5. Locate the resulting `ExampleMod.dll` in `/bin/Release/net46`.
7. Find your COI game files folder (where saves or screenshots are). By default it is at `%USERPROFILE%/Documents/Captain of Industry`.
8. Open or create directory `Mods`. There, create a new directory that has the same name as your mod DLL, in our case it is `ExampleMod`. Copy the compuled `ExampleMod.dll` there, so that it is at `Captain of Industry/Mods/ExampleMod/ExampleMod.dll`. Note that the directory name and DLL name must match.
9. Launch the game and in `Miscellaneous` settings enable mod support.
10. Restart the game.
11. Launch a new game and observe that the ExampleMod is loaded by locating a new node in the research tree (open using `G` key). In case of any errors, examine logs in the `Logs` directory.
12. Congratulations, you are now running your mod in Captain of Industry!

Note: Mods are currently bound to save files. It is not possible to add a mod to an older save, or remove a mod from an existing save. If you delete a mod DLL, any saves that were started with it won't be loadable.

## Assets creation

Assets such as icons or 3D models can be created using Unity editor. We currently use Unity 2021.6.f1 and it is recommended to use the same version to avoid incompatibilities.

### Unity setup

One-time Unity setup needed for MaFi tools to function properly.

1. Download and install Unity 2021.6.f1 from https://unity3d.com/unity/qa/lts-releases.
2. Locate the test scene in `src/ExampleMod.Unity`. Do not open it yet.
3. Create a directory link called `UnityRootSymlink` in `src\ExampleMod.Unity\Library` that points to the Unity installation folder (e.g. `C:\Program Files\Unity`). This folder should have the `Library` directory in it. This can be done by invoking `mklink \D <target> <srouce>` command in console window with admin priviliges. For example: `mklink /D "D:\CaptainOfIndustryModding\src\ExampleMod.Unity\Library\UnityRootSymlink" "C:\Program Files\Unity"`.
4. Create hard-links for necessary DLLs from your Steam installation by running the `src/ExampleMod.Unity/Assets/DLLs/create_dll_hardlinks.bat` batch file. You will need to run it under admin privileges (right-click, Run as Administrator).
   - It is a good practice to look at any code you are running under admin privileges, so feel free to inspect the batch file first.
   - Alternatively, you could also copy the DLLs in question to this directory but hard link is better since any update to the original files will propagate.
5. Open the test scene from `src/ExampleMod.Unity/Assets/ExampleModScene.unity`.
6. Verify that you can see `MaFi` in the top menu. If not, linked DLLs were not properly loaded and you will not be able to create assets.

Unity is very aggressive in changing paths in project files from relative absolute. We recommend to mark the project files as "unchanged" in GIT to not commit them. However, if you even make significant changes, you will need to manually replace paths before commiting them.

```
git update-index --assume-unchanged src/unity/Assembly-CSharp.csproj
git update-index --assume-unchanged src/unity/Assembly-CSharp-Editor.csproj
```

### Creation of icon assets

Following steps describe how to package icons, for example for new products.

1. We recommend organizing assets in directories. Under the `Assets` directory create `<mod name>/<icons categor>` directory, in our case that is `ExampleMod/ProductIcons`.
2. Import images as png or jpg files to the newly created directory.
3. Configure newly imported textures to have `Sprite (2D and UI)` type and apply the change.
4. Assign the newly imported textures to any asset bundle from the drop-down menu on the bottom of the Inspector tab. You can create a new bundle called `asdf` or pick any existing one.
5. To use an icon in your mod (for example as a product icon), simply right-click on a texture and select `Copy Path`. That path can be used to load your prefab in the game.
6. Follow steps under the [Packaging asset bundles](#packaging-asset-bundles) to package the created assets.

Note: Unlike 3D models, textures do not need to have a `prefab` created.

### Creation of 3D model asset

Following steps describe how to create a 3D model template that is very benefitial in creation of 3D models of buildings.

1. Define a layout entity in your mod with desired layout specified using ASCII (see `ExampleMachineData.cs`).
2. Set prefab path to `"TODO"` since we don't have a prefab yet.
3. Compile and deploy the mod.
4. Launch game with the newly created machine and run console command `generate_layout_entity_mesh_template` followed by your entity ID. This will generate an OBJ file in `Captain of Industry/Misc/GeneratedMeshTemplates` which represents a 3D bounding box of layout of your new entity with exact port locations.
5. If you don't have a 3D model, load the newly created template model to a 3D editor of your choice and create 3D model that fits it. If you already have a 3D model, you can compare it to the generated template and edit the ASCII layout accordingly.
6. When 3D model is complete, export FBX or OBJ and follow the next steps.

Following steps describe how to package a 3D model.

1. We recommend organizing assets in directories. Under the `Assets` directory create `<mod name>/<model name>` directory, in our case that is `ExampleMod/ExampleModel`.
2. Import 3D model files (OBJ, FBX, etc.) and textures (PNG, JPG, etc) to the model directory. You can simply use drag & drop. Creating a separate folder for each asset is recommended.
3. Drag the 3D model from Project pane to the Unity scene. Reset its position and rotation to all zeros using Inspector (you can use the three dots menu next to transform) and make it look as you want (add materials, etc.). Note: it is important that your asset has zero position and rotation. If you need to reposition it, change the 3D model or make a child object that can be moved.
4. Create a prefab by dragging the root object and dropping it to the project pane. This will make a new `.prefab` file. In our example we created the prefab in the `<mod name>` directory. 
5. Assign the newly created prefab to any asset bundle from the drop-down menu on the bottom of the Inspector tab. You can create a new bundle called `asdf` or pick any existing one.
6. To use prefabs in your mod, simply right-click any prefab and select `Copy Path`. That path can be used to load your prefab in the game.
7. Follow steps under the [Packaging asset bundles](#packaging-asset-bundles) to package the created assets.

### Packaging asset bundles

Once your assets are ready, follow these steps to package them with your mod.
Mods are published in a directory with the same name as your mod DLL, in our case we have `ExampleMod/ExampleMod.dll`.
Now we can add asset bundles to the same directory.

1. Ensure that everyting in Unity is saved (use `Ctrl+S`).
2. Build asset bundles by right-clicking anywhere in the project pane (on any file or empty area) and select `[MaFi] Build asset bundles`. After Unity is done processing, asset bundle files were created in `src/ExampleMod.Unity/AssetBundles` directory.
3. Copy contents of `src/ExampleMod.Unity/AssetBundles` to the `AssetBundles` folder next to mod DLL, for example `Captain of Industry/Mods/ExampleMod/AssetBundles`.
4. (optional) If you want to make it neat, you really only need the asset bundles (files in format `YourPrefabName_xxxx`, without extension) and the `mafi_bundles.manifest` file. All other `.manifest` files could be removed as well as the `AssetBundles` file.

If you do any changes to your prefabs, simply rebuild asset bundles and copy files from `AssetBundles` directory.

## Questions, issues

Note that mod support is experimental and APIs might change.
If you are having issues, always examine logs, they contain a lot of useful information.
If you'd like to discuss modding topics with the community and devs, visit our [Discord channel #modding-dev-general](https://discord.gg/JxmUbGsNRU) (you will need to assign yourself a `Mod creation` role in `#pick-your-roles-here` channel).
