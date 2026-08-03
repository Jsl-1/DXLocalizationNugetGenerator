# DX Localization NuGet Generator
This project aims to create localization NuGet packages from provided DevExpress NuGet packages and localization libraries provided by DevExpress Localization service.

# Usage

The DXLocalizationNugetGenerator executable has two commands. The CreateNuspec must precede the CreateNuget command.

### Create nuspec files

Create nuspec files before the NuGet packages creation. You will need:
* --inputDXNuGetPath - path to DevExpress Nuget packages
* --inputLocalizationNetFrameworkPath - path to the .NET Framework DevExpress localization libraries from DevExpress Localization Service
* --inputLocalizationNetCorePath - path to the .NET Core DevExpress localization libraries from DevExpress Localization Service
* --outputLanguageCode - target two letter language code
* --outputNuspecPath - target nuspec path
* --revision (-r) - optional revision version number, appended to the DevExpress version (e.g. `25.2.3.1`). It allows publishing several localization package versions for the same DevExpress version, and makes the generated packages compatible with DevExpress hot fixes and minor updates
* --packageIdPrefix (-p) - optional prefix added to the generated package ids (e.g. `ApiAndyou` produces `ApiAndyou.DevExpress.Win.fr`). It keeps the generated packages distinguishable from the official DevExpress ones. Empty by default.

```
.\DXLocalizationNugetGenerator.exe CreateNuspec --inputDXNuGetPath="C:\Program Files\DevExpress 25.2\Components\System\Components\packages" --inputLocalizationNetFrameworkPath=D:\Playground\devexpress-nuget-localization\source\localization\Framework --inputLocalizationNetCorePath=D:\Playground\devexpress-nuget-localization\source\localization\NetCore --outputLanguageCode=cs --outputNuspecPath=D:\Playground\devexpress-nuget-localization\target\nuspec -r=1 -p=ApiAndyou
```

### Create NuGet packages

Create NuGet packages. This command takes all nuspec files from the given input path and creates NuGet packages. If the NuGet executable does not exist in PATH, the latest stable NuGet executable is downloaded and used for the creation of the NuGet package. To use this command, you will need:
* --i - path to the directory that consists of nuspec files
* --o - path to output directory that will contain the generated NuGet files.

```
.\DXLocalizationNugetGenerator.exe CreateNuget -i D:\Playground\devexpress-nuget-localization\target\nuspec -o D:\Playground\devexpress-nuget-localization\target\nuget
```

# Contributing

Contributions are welcome.  Feel free to file issues and pull requests on the repo.