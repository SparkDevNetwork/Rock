# This script is run by AppVeyor's deploy agent before the deploy
Import-Module WebAdministration

# Get the application (web root) and the root folder
$webroot = $env:RockWebRootPath
$rootfolder = Split-Path -Parent $webroot

Write-Output "Running pre-deploy script"
Write-Output "--------------------------------------------------"
Write-Output "Root folder: $rootfolder"
Write-Output "Web root folder: $webroot"
Write-Output "Running script as: $env:userdomain\$env:username"

# stop execution of the deploy if the moves fail
$ErrorActionPreference = "Stop"

### stop web publishing service - needed to allow the deploy to overwrite the sql server spatial types
### Write-Host "Stopping Web Publishing Service"
### stop-service -servicename w3svc

# stop web site and app pool
Write-Host "Stopping Website and ApplicationPool"
Stop-Website -Name "$env:APPLICATION_SITE_NAME"
Stop-WebAppPool -Name (Get-Website -Name "$env:APPLICATION_SITE_NAME").applicationPool

# wait for 10 seconds before continuing
Start-Sleep 10

# delete the content directory in temp
If (Test-Path "$rootfolder\temp\Content"){
	Remove-Item "$rootfolder\temp\Content" -Force -Confirm:$False -Recurse
}

# move content folder to temp
Write-Host "Moving content folder to temp directory"
Move-Item "$webroot\Content" "$rootfolder\temp"

# move App_Data Cache to temp
Move-Item "$webroot\App_Data\Cache" "$rootfolder\temp"

# move App_Data Logs to temp
Move-Item "$webroot\App_Data\Logs" "$rootfolder\temp"

# move App_Data Packages to temp
Move-Item "$webroot\App_Data\Packages" "$rootfolder\temp"

# move App_Data Uploads to temp
Move-Item "$webroot\App_Data\Uploads" "$rootfolder\temp"

# move custom themes to temp
Move-Item "$webroot\Themes\Ulfberht" "$rootfolder\temp"

# move a robots file if it exists
If (Test-Path "$webroot\robots.txt"){
	Move-Item "$webroot\robots.txt" "$rootfolder\temp"
}
