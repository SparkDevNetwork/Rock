The files in this directory are used to install the Rock ChMS.


FILE OVERVIEW
<><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><>
Start.aspx - 		The file that the user places on their webserver to 
			initiate the install.

Rock Website - 		Files and directories that are hosted on the Rock website
			to faciliate the install process. These files should be
			placed under the /installer/ folder of the web server.

rock-chms-latest.zip - 	The zip file that contains all of the files needed to run
			the Rock ChMS. This files should be placed on the Rock web
			server in the /downloads/ directory. Instructions on assembling
			this file can be found below.


CREATING INSTALLER ZIP FILE
<><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><>


1. Pull latest from GitHub
2. Build solution in Visual Studio
3. Edit web.config.  Delete the line:   <add key="AutoMigrateDatabase" value="False"/>
4. Edit web.config. set RunJobsInIISContext = true
5. Zip up the RockWeb directory leaving out the following files:
	* web.ConnectionStrings.config  !IMPORTANT!
	* .gitignore
	* .DS_Store (might just be on my Mac)
