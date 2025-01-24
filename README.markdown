![Rock RMS](https://raw.githubusercontent.com/SparkDevNetwork/Rock/develop/Images/github-banner.png)

Rock RMS is an open source Relationship Management System (RMS) and Application
Framework for 501c3 organizations[^1]. While Rock specializes in serving the unique needs of churches it's
useful in a wide range of service industries.  Rock is an ASP.NET 4.5 C# web application
that uses Entity Framework 6.0, jQuery, Bootstrap 3, and many other open source libraries.

Our main developer starting point site is [the wiki](https://github.com/SparkDevNetwork/Rock/wiki)
or to see how to setup your local environment check out [environment.markdown](environment.markdown)

## Learn More

Jump over to our [Rock website](https://www.rockrms.com/) to find out more. Keep up to date by:

* [Reading our blog](https://community.rockrms.com/connect)
* [Following us on Twitter](https://www.twitter.com/therockrms)
* [Liking us on Facebook](https://www.facebook.com/therockrms)
* [Reading the community Q & A](https://community.rockrms.com/ask)
* [Subscribing to our newsletter](https://www.rockrms.com/Rock/Subscribe)

## License
Rock released under the [Rock Community License](https://www.rockrms.com/license).

## Crafted By

A community of developers led by the [Spark Development Network](https://www.sparkdevnetwork.com/).

## Installer Note

Normally the [Rock installer](https://www.rockrms.com/Download) generates a unique `PasswordKey`
`DataEncryptionKey` and MachineKey's `validationKey` and `decryptionKey`. So if you decide
to clone the repo and run it directly, you will need to handle that aspect yourself.

## Troubleshooting Build Issues
If you run into build issues, try these steps to resolve common scenarios:

1. Clean the Visual Studio solution by right-clicking and choosing "Clean Solution". 
    * Using "Rebuild Solution" will not perform the same level of cleaning due to custom tasks added only to "Clean Solution"; therefore it's recommended that you use "Clean Solution" first.
2. Run the `BranchCleanup.bat` script at the root of the repo.
    * In Visual Studio open the Terminal (Developer PowerShell) ( ``CTRL + ` ``) and type `BranchCleanup.bat` then press enter (You can use tab completion after typing just a few characters).
    
### What does the BranchCleanup script do?
The script checks the last successfully built version and compares it with the current MAJOR.MINOR version. If they differ, it will:
1. Removes all files from project bin and obj directories.
2. Cleans the RockWeb bin directory:
    * Removing all .dll, .pdb, and .xml files except for the following checked-in files:
        * Microsoft.CodeDom.Providers.DotNetCompilerPlatform.dll
        * Rock.Common.Mobile.dll
        * grpc_csharp_ext.x64.dll
        * grpc_csharp_ext.x86.dll
3. Deletes the **RockWeb\Obsidian** directory.
4. Removes NuGet packages.
5. Runs `npm ci` for the **Rock.JavaScript.Obsidian** project.
6. Runs `npm ci` for the **Rock.JavaScript.Obsidian.Blocks** project.
 
### When is the BranchCleanup script triggered?
This script runs automatically when you clean the solution in Visual Studio, but only if the last build version differs from the current one. This helps avoid unnecessary cleanup when switching between branches with the same major and minor version.

You can also manually run the script as described above if needed.

 [^1]: [See our FAQ for details on our license](https://www.rockrms.com/faq)