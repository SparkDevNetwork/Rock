# This script is run by AppVeyor's deploy agent after the deploy
Import-Module WebAdministration
 
# move content directory back from temp
Write-Host "Moving Contents folder back from temp directory"
Move-Item c:\webdata\rock.ccvonline.com\temp\Content c:\webdata\rock.ccvonline.com\docs 

# copy new connection string file
Write-Host "Copying new web.ConnectionStrings.config to web dir"
Copy-Item c:\webdata\rock.ccvonline.com\config\web.ConnectionStrings.config c:\webdata\rock.ccvonline.com\docs

# start web publishing service
Write-Host "Starting Web Publishing Service"
start-service -servicename w3svc

# create empty migration flag
New-Item c:\webdata\rock.ccvonline.com\docs\App_Data\Run.Migration -type file -force

# delete deploy scripts
If (Test-Path c:\webdata\rock.ccvonline.com\docs\deploy.ps1){
	Remove-Item c:\webdata\rock.ccvonline.com\docs\deploy.ps1
}

If (Test-Path c:\webdata\rock.ccvonline.com\docs\before-deploy.ps1){
	Remove-Item c:\webdata\rock.ccvonline.com\docs\before-deploy.ps1
}

# delete the appveyor deploy cache
If (Test-Path c:\appveyor){
	Remove-Item c:\appveyor -Force -Confirm:$False -Recurse
}