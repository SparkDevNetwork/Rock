?The files in this directory are used to install the Rock RMS.



CREATING INSTALLER ZIP FILE

<><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><>



1. Download the source from the GitHub release tag

2. Build solution in Visual Studio (IMPORTANT: build in Release mode)

3. Edit web.config
    Delete the line:   <add key="AutoMigrateDatabase" value="False"/>

    Set RunJobsInIISContext = true
    Turn off debug <compilation debug="false"…
4. Update NuGet Package called Rock.x.y.z.nupkg (where x.y.z is version being built) in the App_Data/Packages folder.

5. Update NuGet Package called RockUpdate-x-y-z.x.y.z.nupkg (RockUpdate-x-y-z) in the App_Data/Packages folder.

6. Zip up the RockWeb directory leaving out the following files:

    * .gitignore
	
    *.pdb 
	
    * Settings.StyleCop

7. Rename zip file 'rock-install-latest.zip'

8. Move copy of zip to ./Installers/RockInstaller/Install Versions/vX.Y.Z/ so that it will be in source control

9. Overwrite with snapshot zip file to Azure Blog storage
 (rockrms/install/<version>/Data)