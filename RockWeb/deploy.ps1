# This script is run by AppVeyor's deploy agent after the deploy
Import-Module WebAdministration


$rootfolder = "$env:application_path\..\"
$webroot = "$env:application_path"

Write-Output "Running post-deploy script"
Write-Output "--------------------------------------------------"
Write-Output "Root folder: $rootfolder"
Write-Output "Web root folder: $webroot"
Write-Output "Running script as: $env:userdomain\$env:username"

# delete the content directory if it exists as it was added by the deploy
If (Test-Path "$webroot\Content"){
	Remove-Item "$webroot\Content" -Force -Confirm:$False -Recurse
}

# move content directory back from temp
If (Test-Path "$rootfolder\temp\Content"){
	Write-Host "Moving Contents folder back from temp directory"
	Move-Item "$rootfolder\temp\Content" "$webroot"
}

If (Test-Path "$rootfolder\temp\checks"){
	Write-Host "Moving checks folder back from temp directory"
	Move-Item "$rootfolder\temp\checks" "$webroot"
}

If (Test-Path "$rootfolder\temp\documents"){
	Write-Host "Moving documents folder back from temp directory"
	Move-Item "$rootfolder\temp\documents" "$webroot"
}

If (Test-Path "$rootfolder\temp\profiles"){
	Write-Host "Moving profiles folder back from temp directory"
	Move-Item "$rootfolder\temp\profiles" "$webroot"
}

# start web publishing service
#Write-Host "Starting Web Publishing Service"
#Start-Service -ServiceName w3svc
#Start-Service -ServiceName w3logsvc

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
# If (Test-Path c:\appveyor){
# 	Remove-Item c:\appveyor -Force -Confirm:$False -Recurse
# }

# move connection string file back from temp
If (Test-Path "$rootfolder\temp\web.connectionstrings.config"){
	Write-Host "Moving web.connectionstrings.config from temp dir"
	Copy-Item "$rootfolder\temp\web.connectionstrings.config" "$webroot" -force
}

# move core Rock css overrides file back from temp
If (Test-Path "$rootfolder\temp\_css-overrides.less"){
	Write-Host "Moving _css-overrides.less from temp dir"
	Copy-Item "$rootfolder\temp\_css-overrides.less" "$webroot\Themes\Rock\Styles" -force
}

# remove the app offline flag
If (Test-Path "$webroot\app_offline.htm"){
	Write-Host "Removing app offline template"
	Remove-Item "$webroot\app_offline.htm"
}

# move web.config file back from temp (restarts the app pool)
If (Test-Path "$rootfolder\temp\web.config"){
	Write-Host "Moving web.config from temp dir"
	Copy-Item "$rootfolder\temp\web.config" "$webroot" -force
}
