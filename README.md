# Captain of industry modding

Welcome to the official modding resource for video game [Captain of Industry](captain-of-industry.com).

## Status of modding support

The game is in alpha stage and modding is not yet fully supported and documented.
However, we decided to release some modding support very early to give our community an easy entry-point.
Full modding support and docummentation is planned in the second half of 2022.

## Prerequisities

1. Own the Captain of Industry on Steam and have the game installed.
2. Have .NET framework installed.

## Getting started

1. Locate the game files via right-click on the game title in the Steam client -> `Properies...` -> `Local Files` -> `Browse`.
2. Copy the game root path in your clipboard. In my case it is `D:\Steam\steamapps\common\Captain of Industry`.
3. Create an environmental variable `COI_ROOT` and paste the game root path from above. If you are on Windows, you can use handy `Edit environmental variables` tool, just open Start menu and type `Edit env` and you should see it.
4. Download this repo and open the solution with example mod. I like to use [Visual Studio](https://visualstudio.microsoft.com/) but feel free to use your favorite tools. In Visual Studio you should see all dependent assemblies linked correctly. If not and you are seeing a lot of errors, check you environmental `COI_ROOT` variable, try restarting.
5. Compile it for example in `Release` configuration.
6. Locate the resulting `ExampleMod.dll` in `\bin\Release\net46` and copy it in a clipboard.
6. Find your CoI game files folder in your Documents, mine is at `D:\%USERNAME%\Documents\CaptainOfIndustry`.
7. Open or create directory `Mods`. There, create a new directory that has the same name as your mod DLL, in our case it is `ExampleMod`, and inside it paste the compiled dll.
8. Launch the game and in Miscellaneous settings enable mod support.
9. Restart the game.
10. Launch the new game and locate a new node in the research tree (open using `G` key). In case of any errors, examine logs in the `Logs` directory.
11. Congratulations, you are now running your mod in Captain of Industry!

## Questions, issues

Note that mod support is experimental and APIs might change.
If you'd like to discuss modding topics with the devs and the community visit our [Discord channel #alpha-modiding](https://discord.gg/JxmUbGsNRU).
