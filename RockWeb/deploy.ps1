# This script is run by AppVeyor's deploy agent after the deploy
Import-Module WebAdministration

# Get the application (web root) and the root folder
$webroot = "c:\inetpub\wwwroot\rock"
$rootfolder = Split-Path -Parent $webroot

Write-Output "Running post-deploy script"
Write-Output "--------------------------------------------------"
Write-Output "Root folder: $rootfolder"
Write-Output "Web root folder: $webroot"
Write-Output "Running script as: $env:userdomain\$env:username"

# ensure that the compilation debug is false
(Get-Content "$webroot\web.config").Replace('<compilation debug="true"', '<compilation debug="false"').replace('RunJobsInIISContext" value="false"', 'RunJobsInIISContext" value="true"') | Set-Content "$webroot\web.config"

# Now move your custom stuff back from the temp folder like this:

# move a robots file back from temp if it exists
If (Test-Path "$rootfolder\temp\robots.txt"){
	Write-Host "Moving robots.txt file back from temp directory"
	Move-Item "$rootfolder\temp\robots.txt" "$webroot"
}

# copy new connection string file
Write-Host "Copying web.config and web.ConnectionStrings.config to web dir"
Copy-Item "$rootfolder\web.ConnectionStrings.config" $webroot -force
Copy-Item "$rootfolder\web.config" $webroot -force

Start-Sleep 5

# start web site and app pool
Write-Host "Starting ApplicationPool and Website"
Start-WebAppPool -Name (Get-Website -Name "$env:APPLICATION_SITE_NAME").applicationPool
Start-Website -Name "$env:APPLICATION_SITE_NAME"

# create empty migration flag
New-Item "$webroot\App_Data\Run.Migration" -type file -force

# set acl on migration flag file so the app has permissions to delete it
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