# This script is run by AppVeyor's deploy agent after the deploy
Import-Module WebAdministration
 
# start website
Write-Host "Starting IIS website"
Start-WebSite 'RockRMS'

# move content directory back from temp
Write-Host "Moving Contents folder back from temp directory"
Move-Item c:\webdata\rock.ccvonline.com\temp\Content c:\webdata\rock.ccvonline.com\docs 

# copy new connection string file
Write-Host "Copying new web.ConnectionStrings.config to web dir"
Copy-Item c:\webdata\rock.ccvonline.com\config\web.ConnectionStrings.config c:\webdata\rock.ccvonline.com\docs