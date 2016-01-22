The files in this directory are used to install the Rock RMS.

CREATING INSTALLER ZIP FILE
<><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><>


___ 1. Download the source from the GitHub release tag

___ 2. Build solution in Visual Studio (IMPORTANT: build in Release mode) After downloading you may
       need to make the RockWeb project the startup project.

___ 3. Edit web.config

        * Set RunJobsInIISContext = true
        * Turn off debug <compilation debug="false"

___ 4. Delete all the files out of ~/App_Data/packages

___ 5. Copy the Rock.x.y.z.nupkg file from
       https://github.com/SparkDevNetwork/Rock-UpdatePackageBuilder/tree/master/InstallerArtifacts
       to the App_Data/Packages folder.  Remove any earlier versions of the Rock.*.nupkg file.
       
___ 6. Copy the RockUpdate-X-Y-Z.x.y.z.nupkg file from
       https://github.com/SparkDevNetwork/Rock-UpdatePackageBuilder/tree/master/InstallerArtifacts
       to the App_Data/Packages folder.
       
___ 7. Delete the following files from the RockWeb directory

       * .gitignore (do a search as there are files in several directory)
        *.pdb (do a search as there are several files )
        * Settings.StyleCop
        * .editorconfig

___ 8. Zip up the RockWeb directory

___ 9. Rename zip file 'rock-install-latest.zip'

___ 10. Move copy of zip to ./Installers/RockInstaller/Install Versions/vX.Y.Z/ so that it  
       will be in source control

___ 11. Overwrite with snapshot zip file to Azure Blog storage (rockrms/install/<version>/Data)
        Note the <version> label is the installer version not the Rock version. This should not
        be incremented except when the installer scripts get updated.

CREATING SQL FILE
<><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><>

___ 1. Add a web.ConnectionStrings.config to the downloaded project above and give it a 
       fresh database name.

___ 2. Run 'update-database' in Visual Studio package manager so that a new database is made.

___ 3. Open SQL Server Manager

___ 4. Run the SQL below to purge old migrations out (this greatly reduces the size of the SQL script)
       UPDATE [__MigrationHistory] 
       SET [Model] = 0x
       WHERE [MigrationId] != (SELECT MAX(MigrationId) FROM [__MigrationHistory])

___ 5. Right-click on the new database and select 'Tasks > Generate Scripts'

___ 6. Click 'Next' until you see the screen with the 'Advanced' button.

___ 7. Save the file as 'sql-install.sql'

___ 8. Click the 'Advanced' button and change the setting for 'Types of data to script' to 
       'schema and data'.

___ 9. Click 'Next' then 'Finished'

___ 10. Open the SQL file and make the following edits:
         * delete from the start of the file to the beginning of the first comment of the first stored proc
         * remove the follow four lines from the end of the script
              USE [master]
              GO
              ALTER DATABASE [RockRMS_NewDbName] SET  READ_WRITE 
              GO
         * Remove the following strings from the file in the order below:
            'TEXTIMAGE_ON [PRIMARY]'
            'ON [PRIMARY]'


___ 11. Zip the file into a new file named 'sql-latest.zip'

___ 12. Move copy of zip to ./Installers/RockInstaller/Install Versions/vX.Y.Z/ so that 
        it will be in source control

___ 13. Overwrite with snapshot zip file to Azure Blog storage
        (rockrms/install/<version>/Data)
