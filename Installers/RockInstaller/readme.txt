?The files in this directory are used to install the Rock RMS.



CREATING INSTALLER ZIP FILE

<><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><>


___ 
1. Download the source from the GitHub release tag


___ 
2. Build solution in Visual Studio (IMPORTANT: build in Release mode)


___ 
3. Edit web.config
         * Delete the line:   <add key="AutoMigrateDatabase" value="False"/>

         * Set RunJobsInIISContext = true
         * Turn off debug <compilation debug="false"…

___ 
4. Update NuGet Package called Rock.x.y.z.nupkg (where x.y.z is version being built) in the App_Data/Packages folder.


___ 
5. Update NuGet Package called RockUpdate-x-y-z.x.y.z.nupkg (RockUpdate-x-y-z) in the App_Data/Packages folder.


___ 
6. Zip up the RockWeb directory leaving out the following files:

        * .gitignore
	
        *.pdb 
	
        * Settings.StyleCop


___ 
7. Rename zip file 'rock-install-latest.zip'


___ 
8. Move copy of zip to ./Installers/RockInstaller/Install Versions/vX.Y.Z/ so that it will be in source control


___ 
9. Overwrite with snapshot zip file to Azure Blog storage
 (rockrms/install/<version>/Data)

CREATING SQL FILE
<><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><>


___ 
1. Add a web.ConnectionStrings.config to the downloaded project above and give it a 
       fresh database name.

___ 
2. Run 'update-database' in Visual Studio package manager so that a new database is made.

___ 
3. Open SQL Server Manager

___ 
4. Right-click on the new database and select 'Tasks > Generate Scripts'

___ 
5. Click 'Next' until you see the screen with the 'Advanced' button.

___ 
6. Click the 'Advanced' button and change the setting for 'Types of data to script' to 'schema and data'.

___ 
7. Click 'Next' then 'Finished'

___ 
8. Open the SQL file and make the following edits:
         * delete from the start of the file to the beginning of the first comment of the first stored proc
         * remove the follow four lines from the end of the script
              USE [master]
              GO
              ALTER DATABASE [RockRMS_NewDbName] SET  READ_WRITE 
              GO

___ 
9. Zip the file into a new file named 'sql-latest.sql'

___ 
10. Move copy of zip to ./Installers/RockInstaller/Install Versions/vX.Y.Z/ so that it will be in source control


___ 
11. Overwrite with snapshot zip file to Azure Blog storage
 (rockrms/install/<version>/Data)