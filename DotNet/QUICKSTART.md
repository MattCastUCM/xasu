## Installation
Xasu is a library developed in .NET Standard 2.0. You can check the .NET implementations that support it [here](https://learn.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-0).

Xasu can be downloaded through the Unity Package Manager using the [repository link](https://github.com/e-ucm/xasu.git) of this project.

<!-- TODO: Build the dlls and provide them directly? -->
To add it to your proyect, just add the ```xasu.csproj``` to your solution and add it as a dependency in the project where you want to use it.

## Application configuration 

In order to store and retrieve information from files, Xasu defines ```ApplicationSettings``` (to store things such as the Application name or path to the assets folder) and ```PersistentPrefs``` (to persist and access data in a file).

By default, their values will be the following:
* ApplicationSettings:
    * ProductName: ```MyProject```
    * CompanyName: ```DefaultCompany```
    * TrackerConfigPath: ```./StreamingAssets```
    * AssetsPath: ```./Assets```
    * PersistentDataPath: 
        * Windows: ```%userprofile%/AppData/Roaming/Xasu/(CompanyName)/(ProductName)```
        * Linux: ```~/.config/Xasu/(CompanyName)/(ProductName)``` (via XDG standards)
        * macOS: ```~/Library/Application Support/Xasu/(CompanyName)/(ProductName)```
    * TemporaryCachePath: 
        * Windows: ```%userprofile%/AppData/Local/XasuCache/(CompanyName)/(ProductName)```
        * Linux: ```~/.local/share/XasuCache/(CompanyName)/(ProductName)``` (via XDG standards)
        * macOS: ```~/Library/Application Support/XasuCache/(CompanyName)/(ProductName)/cache``` or ```~/Library/Caches/XasuCache/(CompanyName)/(ProductName)```

* PersistentPrefs:
    * fileName: ```preferences.json```
    * filePath: ```ApplicationSettings.PersistentDataPath```

These values can be changed by replacing (before the tracker initialization and before accessing any other delegate class) their factory method in ```Factories``` so they call either the base class' constructor with different parameters or the constructor of a custom implementation:

```cs
Factories.factories[Factories.Id.APPLICATION_SETTINGS] = () =>
{
    // All parameters are optional
    return new BaseApplicationSettings(productName, companyName, trackerConfigPath, assetsPath, persistentDataPath, temporaryCachePath);
};
```
```cs
Factories.factories[Factories.Id.PERSISTENT_PREFS] = () =>
{
    // All parameters are optional
    return new BasePersistentPrefs(fileName, filePath);
};
```

    
## Configuration file

The tracker configuration can be provided either using a file or via scripting. By default, the file will be read from ```./StreamingAssets/tracker_config.json```, which must be in the application's root folder. 

The file name and path can be changed by replacing the ```TrackerConfig```'s factory method in [the same way](#application-configuration) as in the application configuration
```cs
Factories.factories[Factories.Id.TRACKER_CONFIG] = () =>
{
    // All parameters are optional
    return new TrackerConfig(fileName, filePath);
};
```

To check the minimal tracker configuration, check the main [README file](../README.md#minimal-tracker_configjson).

## Adding Xasu to your game

Once Xasu is added as a dependency, using it is as simple as accessing its methods and properties through the tracker's delegate ```XasuTracker```.

If you want to know more about how Xasu works, please check the Wiki:
* Working with Xasu: https://github.com/e-ucm/xasu/wiki/Working-with-Xasu

### Initializing Xasu

Before sending any traces, you must initialize Xasu by using the ```Init``` method:
```cs
    await XasuTracker.Init();
```

If you want to learn more about how to initialize Xasu please visit our Wiki.
* Initializing Xasu: https://github.com/e-ucm/xasu/wiki/Working-with-Xasu#initialization
