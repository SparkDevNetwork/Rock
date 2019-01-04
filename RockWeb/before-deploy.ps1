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
# $Site = Get-WebSite -Name "$env:APPLICATION_SITE_NAME"
# If ( $Site.State -eq "Started" ) {
# 	Write-Host "Stopping Website"
# 	Stop-Website -Name "$env:APPLICATION_SITE_NAME"
# }
# If ( (Get-WebAppPoolState -Name $Site.applicationPool).Value -eq "Started" ) {
# 	Write-Host "Stopping ApplicationPool"
# 	Stop-WebAppPool -Name $Site.applicationPool
# }

# wait for 10 seconds before continuing
# Start-Sleep 10

# backup web.config file
If (Test-Path "$webroot\web.config"){
	Write-Host "Moving web.config to config dir"
	Copy-Item "$webroot\web.config" "$rootfolder\config" -force
}

# backup connection string file
If (Test-Path "$webroot\web.connectionstrings.config"){
	Write-Host "Moving web.connectionstrings.config to config dir"
	Copy-Item "$webroot\web.connectionstrings.config" "$rootfolder\config" -force
}

# delete the contents of the temp directory or create it
If (Test-Path "$rootfolder\temp"){
	Remove-Item "$rootfolder\temp\*" -Force -Confirm:$False -Recurse
} Else {
  New-Item -ItemType Directory -Force -Path "$rootfolder\temp"
}

# delete the contents of the temp\Plugins directory or create it
If (Test-Path "$rootfolder\temp\Plugins"){
	Remove-Item "$rootfolder\temp\Plugins\*" -Force -Confirm:$False -Recurse
} Else {
  New-Item -ItemType Directory -Force -Path "$rootfolder\temp\Plugins"
}

# move content folder to temp
#Write-Host "Moving content folder to temp directory"
#Move-Item "$webroot\Content" "$rootfolder\temp\" -Force

# move LEGACY content folder to temp
#Write-Host "Moving LEGACY ARENA folder to temp directory"
#Move-Item "$webroot\arena" "$rootfolder\temp\" -Force

# move App_Data to temp
#Write-Host "Moving App_Data folder to temp directory"
#Move-Item "$webroot\App_Data" "$rootfolder\temp\" -Force

# move custom themes to temp
#Write-Host "Moving Themes\Ulfberht folder to temp directory"
#Move-Item "$webroot\Themes\Ulfberht" "$rootfolder\temp\" -Force

# move custom Children's check-in theme to temp (since it contains live files)
# Write-Host "Moving Themes\CheckinKids_CentralAZ folder to temp directory"
# If (Test-Path "$rootfolder\temp\CheckinKids_CentralAZ"){
# 	Remove-Item "$rootfolder\temp\CheckinKids_CentralAZ\*" -Force -Confirm:$False -Recurse
# } Else {
#   New-Item -ItemType Directory -Force -Path "$rootfolder\temp\CheckinKids_CentralAZ"
# }
#Move-Item "$webroot\Themes\CheckinKids_CentralAZ\Assets" "$rootfolder\temp\CheckinKids_CentralAZ\Assets" -Force

# move non com_lcbcchurch plugins to temp\Plugins folder
If (Test-Path "$webroot\Plugins\"){
	Write-Host "Moving non com_lcbcchurch/com_bemadev Plugins to temp Plugins directory"
	$files = GCI -path "$webroot\Plugins\" | Where-Object {$_.name -ne "com_lcbcchurch" -and $_.name -ne "com_bemadev" -and $_.name -ne ".gitignore" -and $_.name -ne "readme.txt"}
	foreach ($file in $files) { Move-Item  "$webroot\Plugins\$file" -Destination "$rootfolder\temp\Plugins" -Force }
}
# move a robots file if it exists
If (Test-Path "$webroot\robots.txt"){
	Move-Item "$webroot\robots.txt" "$rootfolder\temp"
}
