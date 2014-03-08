The files in this directory are used to install the Rock ChMS.




CREATING INSTALLER ZIP FILE
<><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><>


1. Download the source from the GitHub release tag
2. Build solution in Visual Studio (IMPORTANT: build in Release mode)
3. Edit web.config.  Delete the line:   <add key="AutoMigrateDatabase" value="False"/>
4. Edit web.config. set RunJobsInIISContext = true
5. Turn off debug <compilation debug="false"…
6. Update NuGet Package called Rock.x.y.z.nupkg (where x.y.z is version being built) in the App_Data/Packages folder.
7. Update NuGet Package called RockUpdate-x-y-z.x.y.z.nupkg (RockUpdate-x-y-z) in the App_Data/Packages folder.
8. Zip up the RockWeb directory leaving out the following files:
	* web.ConnectionStrings.config  !IMPORTANT!
	* .gitignore
	* Settings.StyleCop
9. Rename zip file 'rock-install-latest.zip'
10. Move copy of zip to ./RockInstaller/Install Versions/vX.Y.Z/ so that it will be in source control
11. Overwrite with snapshot zip file to Azure Blog storage
