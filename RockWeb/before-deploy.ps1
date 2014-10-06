# This script is run by AppVeyor's deploy agent before the deploy
Import-Module WebAdministration

# stop execution of the deploy if the moves fail
$ErrorActionPreference = "Stop"

# stop web publishing service - needed to allow the deploy to overwrite the sql server spatial types
Write-Host "Stopping Web Publishing Service"
stop-service -servicename w3svc
 
# move content folder to temp
Write-Host "Moving content folder to temp directory"
Move-Item c:\webdata\rock.ccvonline.com\docs\Content c:\webdata\rock.ccvonline.com\temp