# This script is run by AppVeyor's deploy agent after the deploy
Import-Module WebAdministration

# Get the application (web root) and the root folder
$webroot = $env:RockWebRootPath
$rootfolder = Split-Path -Parent $webroot

Write-Output "Running post-deploy script"
Write-Output "--------------------------------------------------"
Write-Output "Root folder: $rootfolder"
Write-Output "Web root folder: $webroot"
Write-Output "Running script as: $env:userdomain\$env:username"

# ensure that the compilation debug is false
(Get-Content "$webroot\web.config").Replace('<compilation debug="true"', '<compilation debug="false"') | Set-Content "$webroot\web.config"

# delete the content directory if it exists as it was added by the deploy
If (Test-Path "$webroot\Content"){
	Remove-Item "$webroot\Content" -Force -Confirm:$False -Recurse
}

# move Content directory back from temp
Write-Host "Moving Contents folder back from temp directory"
Move-Item "$rootfolder\temp\Content" "$webroot\"

# move LEGACY directory back from temp
Write-Host "Moving LEGACY ARENA folder back from temp directory"
Move-Item "$rootfolder\temp\arena" "$webroot\"

# move App_Data directory back from temp
Write-Host "Moving App_Data folder back from temp directory"
Move-Item "$rootfolder\temp\App_Data" "$webroot\"

# move custom themes back from temp
Write-Host "Moving Themes\Ulfberht folder back from temp directory"
Move-Item "$rootfolder\temp\Ulfberht" "$webroot\Themes"

# move custom Children's check-in theme back from temp
Write-Host "Moving Themes\CheckinKids_CentralAZ folder back from temp directory"
Move-Item "$rootfolder\temp\CheckinKids_CentralAZ" "$webroot\Themes" -Force

# move a robots file back from temp if it exists
If (Test-Path "$rootfolder\temp\robots.txt"){
	Write-Host "Moving robots.txt file back from temp directory"
	Move-Item "$rootfolder\temp\robots.txt" "$webroot"
}

# copy new connection string file
Write-Host "Copying new web.ConnectionStrings.config to web dir"
Copy-Item "$rootfolder\config\web.ConnectionStrings.config" $webroot -force

# start web publishing service
Write-Host "Starting Web Publishing Service"
start-service -servicename w3svc

# start web site and app pool
Write-Host "Starting ApplicationPool and Website"
Start-WebAppPool -Name (Get-Website -Name "$env:APPLICATION_SITE_NAME").applicationPool
Start-Website -Name "$env:APPLICATION_SITE_NAME"

# create empty migration flag
New-Item "$webroot\App_Data\Run.Migration" -type file -force

# set acl on migration flag file so the app has permissions to delete it
Write-Host "Setting read-write on the Run.Migration file"
$acl = Get-ACL "$webroot\App_Data\Run.Migration"
$accessRule= New-Object System.Security.AccessControl.FileSystemAccessRule("Everyone","FullControl","Allow")
$acl.AddAccessRule($accessRule)
Set-Acl "$webroot\App_Data\Run.Migration" $acl

# delete deploy scripts
If (Test-Path "$webroot\deploy.ps1"){
	Remove-Item "$webroot\deploy.ps1"
}

If (Test-Path "$webroot\before-deploy.ps1"){
	Remove-Item "$webroot\before-deploy.ps1"
}

# delete the appveyor deploy cache
If (Test-Path c:\appveyor){
	Remove-Item c:\appveyor -Force -Confirm:$False -Recurse
}
