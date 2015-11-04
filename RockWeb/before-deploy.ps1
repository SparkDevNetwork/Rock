# This script is run by AppVeyor's deploy agent before the deploy
Import-Module WebAdministration


$rootfolder = "$env:application_path\..\"
$webroot = "$env:application_path"

Write-Output "Running pre-deploy script"
Write-Output "--------------------------------------------------"
Write-Output "Root folder: $rootfolder"
Write-Output "Web root folder: $webroot"
Write-Output "Running script as: $env:userdomain\$env:username"

# stop execution of the deploy if the moves fail
$ErrorActionPreference = "Stop"

# stop web publishing service - needed to allow the deploy to overwrite the sql server spatial types
Write-Host "Stopping Web Publishing Service"
Stop-Service -ServiceName w3logsvc
Stop-Service -ServiceName w3svc

# delete the content directory in temp
If (Test-Path "$rootfolder\temp\Content"){
	Remove-Item "$rootfolder\temp\Content" -Force -Confirm:$False -Recurse
}

# move content folder to temp
If (Test-Path "$webroot\Content"){
	Write-Host "Moving content folder to temp directory"
	Move-Item "$webroot\Content" "$rootfolder\temp\Content"
}

If (Test-Path "$webroot\checks"){
	Write-Host "Moving checks folder to temp directory"
	Move-Item "$webroot\checks" "$rootfolder\temp\checks"
}

If (Test-Path "$webroot\documents"){
	Write-Host "Moving documents folder to temp directory"
	Move-Item "$webroot\documents" "$rootfolder\temp\documents"
}

If (Test-Path "$webroot\profiles"){
	Write-Host "Moving profiles folder to temp directory"
	Move-Item "$webroot\profiles" "$rootfolder\temp\profiles"
}

# move connection string file
If (Test-Path "$webroot\web.connectionstrings.config"){
	Write-Host "Moving web.connectionstrings.config to temp dir"
	Copy-Item "$webroot\web.connectionstrings.config" "$rootfolder\temp" -force
}
