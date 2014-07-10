# This script is run by AppVeyor's deploy agent after the deploy
Import-Module WebAdministration
 
# start web publishing service
Write-Host "Starting Web Publishing Service"
start-service -servicename w3svc

# move content directory back from temp
Write-Host "Moving Contents folder back from temp directory"
Move-Item c:\webdata\rock.ccvonline.com\temp\Content c:\webdata\rock.ccvonline.com\docs 

# copy new connection string file
Write-Host "Copying new web.ConnectionStrings.config to web dir"
Copy-Item c:\webdata\rock.ccvonline.com\config\web.ConnectionStrings.config c:\webdata\rock.ccvonline.com\docs