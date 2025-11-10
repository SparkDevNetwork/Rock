# Overview

Publishing involves a few steps to ensure everything is properly signed and bundled.
This is a pre-release of the documentation as some steps (such as code signing) have not been tested yet.

## Version Upgrades

To release a new version, a few things need to be updated.
These must be done prior to building.

Double click the StatementGenerator project.
Change the `Version` node to the new version number.

In the StatementGenerator.Installer project, open the `Package.wxs` file.
Change the version number at the top to match the version set in the other project.

## Building Projects

In Visual Studio, set your build mode to Release.
Rebuild the StatementGenerator project.

Then publish StatementGenerator.
Publishing can be done by right clicking the project and selecting the Publish item, then click the Publish button in the window that opens up.

Now the desktop application needs to be code signed.

```
signtool.exe sign /f SparkDevelopmentNetwork.cer /d "Spark Development Network" bin\Release\net472\publish\StatementGenerator.exe
```

## Building Installer

In Visual Studio, make sure build mode is set to Release.
Then build the StatementGenerator.Installer project.

Now the MSI needs to be code signed.
NOTE: This is just an example, see the Loop document for more information on how to sign your MSI.

```
signtool.exe sign /f SparkDevelopmentNetwork.cer /d "Spark Development Network" Checkin.Installer\bin\x64\Release\checkinclient.msi
```
