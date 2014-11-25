# This script is run by AppVeyor's deploy agent before the deploy
Import-Module WebAdministration

$rootfolder = "c:\webdata\rock.ccvonline.com"
$webroot = "$rootfolder\docs"

Write-Output "Running pre-deploy script"
Write-Output "--------------------------------------------------"
Write-Output "Root folder: $rootfolder"
Write-Output "Web root folder: $webroot"
Write-Output "Running script as: $env:userdomain\$env:username"

# stop execution of the deploy if the moves fail
$ErrorActionPreference = "Stop"

# stop web publishing service - needed to allow the deploy to overwrite the sql server spatial types
Write-Host "Stopping Web Publishing Service"
stop-service -servicename w3svc

# delete the content directory in temp
If (Test-Path "$rootfolder\temp\Content"){
	Remove-Item "$rootfolder\temp\Content" -Force -Confirm:$False -Recurse
}
 
# move content folder to temp
Write-Host "Moving content folder to temp directory"
Move-Item "$webroot\Content" "$rootfolder\temp"