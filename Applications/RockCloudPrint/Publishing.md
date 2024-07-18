# Overview

Publishing involves a few steps to ensure everything is properly signed and bundled.
This is a pre-release of the documentation as some steps (such as code signing) have not been tested yet.

## Version Upgrades

To release a new version, a few things need to be updated.
These must be done prior to building and publishing.

Double check Rock.CloudPrint.Desktop and Rock.CloudPrint.Service projects.
Change the `Version` node to the new version number.

In the Rock.CloudPrint.Installer project, open the `Package.wxs` file.
Change the version number at the top to match the version set in the other two projects.

## Building Projects

In Visual Studio, set your build mode to Release.
Then publish both Rock.CloudPrint.Desktop and Rock.CloudPrint.Service.
Publishing can be done by right clicking the project and selecting the Publish item, then click the Publish button in the window that opens up.

Now the desktop application needs to be code signed.
This is because it will self-elevate to administrator when launched and we want it to show a proper UAC dialog.

```
signtool.exe /f SparkDevelopmentNetwork.cer /d "Rock Cloud Print" Rock.CloudPrint.Desktop\bin\Release\net8.0-windows\publish\Rock.CloudPrint.Desktop.exe
```

## Building Installer

In Visual Studio, make sure build mode is set to Release.
Then build the Rock.CloudPrint.Installer project.

Now the MSI needs to be code signed.

```
signtool.exe /f SparkDevelopmentNetwork.cer /d "Rock Cloud Print" Rock.CloudPrint.Installer\bin\x64\Rock.CloudPrint.Installer.msi
```
