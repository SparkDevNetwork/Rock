# This script is run by AppVeyor's deploy agent before the deploy
Import-Module WebAdministration

# Get the application (web root) and the root folder
$webroot = "c:\inetpub\wwwroot\rock"
$rootfolder = Split-Path -Parent $webroot

Write-Output "Running pre-deploy script"
Write-Output "--------------------------------------------------"
Write-Output "Root folder: $rootfolder"
Write-Output "Web root folder: $webroot"
Write-Output "Running script as: $env:userdomain\$env:username"

# stop execution of the deploy if the moves fail
$ErrorActionPreference = "Stop"

# stop web site and app pool
$Site = Get-WebSite -Name "$env:APPLICATION_SITE_NAME"
If ( $Site.State -eq "Started" ) {
	Write-Host "Stopping Website"
	Stop-Website -Name "$env:APPLICATION_SITE_NAME"
}
If ( (Get-WebAppPoolState -Name $Site.applicationPool).Value -eq "Started" ) {
	Write-Host "Stopping ApplicationPool"
	Stop-WebAppPool -Name $Site.applicationPool
}

# wait for 10 seconds before continuing
Start-Sleep 10

# delete the contents of the temp directory
If (Test-Path "$rootfolder\temp"){
	Remove-Item "$rootfolder\temp\*" -Force -Confirm:$False -Recurse
} Else {
  New-Item -ItemType Directory -Force -Path "$rootfolder\temp"
}

# Move any files or folders that are not in your git repo like this:

# move a robots file if it exists
If (Test-Path "$webroot\robots.txt"){
	Move-Item "$webroot\robots.txt" "$rootfolder\temp"
}