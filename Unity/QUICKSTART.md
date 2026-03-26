## Installation
<!-- TODO: Check older versions -->
Xasu requires at least **Unity 2019.4 (LTS)**.

To add it to Unity, you can simply clone this repository, build the xasu .NET Standard 2.0 project (`DotNet/xasu/xasu.csproj`), and copy the built dlls (usually located in `DotNet/xasu/bin/Release/netstandard2.0`) and the `Unity` folder contents into your project. 

You can also copy the xasu Runtime folder(`DotNet/xasu/Runtime`) if you want to be able to modify the base functionality, but you will have to delete the `xasu.dll` from the built dlls to avoid conflicts.

You can automate this process (except for deleting the `xasu.dll`) by running the python script in the `PackageBuilder` folder. This will create the `xasu` folder, which includes the files mentioned above. You can then copy this folder to your Unity project or add it as a package using the Unity Package Manager:
* Go to `Window > Package Manager`
* Press the "+" icon on the top left.
* Select `Add package from disk...`.
* Open the `package.json` file in `PackageBuilder/xasu`


## Configuration file

The tracker configuration can be provided either using the `StreamingAssets` folder (recommended) or via scripting. We recommend using the `StreamingAssets` folder to allow configuration to be changed after the game is exported, allowing simpler adaptation of the game to different scenarios without having to recompila the whole game. It must be placed in:

```path
Assets/StreamingAssets/tracker_config.json
```

To check the minimal tracker configuration, check the main [README file](../README.md#minimal-tracker_configjson).

## Adding Xasu to your game

Once Xasu is installed, to add Xasu to your game you just have to create a new GameObject in Unity and include the Xasu Unity Component.

If you want to know more about how Xasu works, please check the Wiki:
* Working with Xasu: https://github.com/e-ucm/xasu/wiki/Working-with-Xasu

### Initializing Xasu

When Xasu is added to your scene it won't initialize and connect by default. 

To initialize it automatically, please check the `Auto Start` property in the object inspector. You can also check `Enable Debug Logging` to display debug logs in the Unity console.

![alt text](xasu-parameters.png)

You can also initialize Xasu manually by using the ```Init``` method:
```cs
    await XasuTracker.Init();
```

If you want to learn more about how to initialize Xasu please visit our Wiki.
* Initializing Xasu: https://github.com/e-ucm/xasu/wiki/Working-with-Xasu#initialization
