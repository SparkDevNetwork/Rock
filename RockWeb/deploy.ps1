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

####################################################################################################
####################################################################################################
# Borrowed from NewPointe deploy script

if([string]::IsNullOrWhiteSpace($env:APPLICATION_PATH)) {
    Write-Error "APPLICATION_PATH is not set, aborting!";
    exit;
}
if([string]::IsNullOrWhiteSpace($env:APPVEYOR_JOB_ID)) {
    Write-Error "APPVEYOR_JOB_ID is not set, aborting!"
    exit;
}

$RootLocation = $env:APPLICATION_PATH;
$TempLocation = Join-Path $env:Temp $env:APPVEYOR_JOB_ID;

function Join-Paths {
    $path, $parts= $args;
    foreach ($part in $parts) {
        $path = Join-Path $path $part;
    }
    return $path;
}

function Get-VersionId([string] $FileName) {
    $Parts = $FileName -split "-";
    if($Parts.Length -gt 0) {
        return $Parts[0] -replace "\D+" -as [Int];
    }
    else {
        return 0;
    }
}

function Expand-RockPlugin([string] $PluginPath, [string] $DestinationPath) {
    
    $PackageHash = (Get-FileHash $PluginPath).Hash;
    $TempZip = Join-Paths $TempLocation "$PackageHash.zip";
    Copy-Item $PluginPath $TempZip;
    Expand-Archive $TempZip $DestinationPath -Force;
    Remove-Item $TempZip -Force;
}

function Restore-RockPlugin([string] $PluginPackagePath) {

    $PackageHash = (Get-FileHash $PluginPackagePath).Hash;
    $PackageTempLocation = Join-Path $TempLocation $PackageHash;

    New-Item $PackageTempLocation -ItemType Directory | Out-Null;
    Expand-RockPlugin $PluginPackagePath $PackageTempLocation;

    $ContentPath = Join-Path $PackageTempLocation "content";
    if(Test-Path $ContentPath) {
        Get-ChildItem $ContentPath | Copy-Item -Destination $RootLocation -Recurse -Container -Force
    }
    
    Remove-Item $PackageTempLocation -Recurse -Force;
}

function Copy-DirectoryContentsRecursivelyWithSaneLinkHandling([string] $DirectoryToCopy, [string] $Destination) {
    New-Item -ItemType Directory $Destination -Force | Out-Null;
    foreach($Child in Get-ChildItem $DirectoryToCopy) {
        if($Child.LinkType) {
            $Dest = Join-Path $Destination $Child.Name;
            if(Test-Path $Dest) {
                Remove-Item $Dest -Recurse -Force
            }
            $LinkTarget, $OtherTargets = $Child.Target;
            New-Item -ItemType $Child.LinkType $Dest -Target $LinkTarget -Force | Out-Null;
        }
        elseif($Child.PSIsContainer) {
            Copy-DirectoryContentsRecursivelyWithSaneLinkHandling (Join-Path $DirectoryToCopy $Child.Name) (Join-Path $Destination $Child.Name);
        }
        else {
            Copy-Item (Join-Path $DirectoryToCopy $Child.Name) (Join-Path $Destination $Child.Name) -Force;
        }
    }
}


# if(Test-Path "env:DEPLOY_DEBUG") {
#     Write-Host "================= DEBUG ==================";
#     Write-Host "Working Directories: $(Get-Location)";
#     Write-Host "Environment:";
#     Get-ChildItem "env:";
# }

Write-Host "Mode: Post-deploy";
Write-Host "Application: $env:APPVEYOR_PROJECT_NAME";
Write-Host "Build Number: $env:APPVEYOR_BUILD_VERSION";
Write-Host "Deploy Location: $RootLocation";
Write-Host "==========================================";


# 1. Restore server-specific files like static files, logs, plugin packages, and caches

Write-Host "Restoring server-specific files";
$FileBackupLocation = Join-Path $TempLocation "SavedFiles";
Copy-DirectoryContentsRecursivelyWithSaneLinkHandling $FileBackupLocation $RootLocation;

# 2. Reinstall plugins

Write-Host "Reinstalling Plugin Files";

$InstalledPluginsPath = Join-Paths $RootLocation "App_Data" "RockShop";
if(Test-Path $InstalledPluginsPath) {

    $InstalledPlugins = Get-ChildItem $InstalledPluginsPath;
    foreach ($Plugin in $InstalledPlugins) {

        $PluginVersions = Get-ChildItem $Plugin.FullName;
        if($PluginVersions.Count -gt 0) {

            $LatestVersion = $PluginVersions  | Sort-Object {Get-VersionId $_.BaseName} | Select-Object -Last 1;
            Write-Host "Restoring ${LatestVersion.Name}";
            Restore-RockPlugin $LatestVersion.FullName;
        }
    }
}

# 3. Clean up temp folder

#Remove-Item $TempLocation -Recurse -Force;

# # 4. Take the app out of maintenence mode

# Write-Host "Taking application out of maintenence mode";

# Move-Item -Path (Join-Path $RootLocation "app_offline.htm") -Destination (Join-Path $RootLocation "app_offline-template.htm") -ErrorAction SilentlyContinue;
# Remove-Item -Path (Join-Path $RootLocation "app_offline.htm") -ErrorAction SilentlyContinue;

# 5. Move the backup so we know we completed successfully

# $InProgressBackupLocation = "$($RootLocation.TrimEnd("/\\")).backup.deploy-in-progress";
# $SuccessBackupLocation = "$($RootLocation.TrimEnd("/\\")).backup";
# Remove-Item $SuccessBackupLocation -Recurse -Force | Out-Null;
# Move-Item $InProgressBackupLocation $SuccessBackupLocation -Force;







## These steps taken care of by #1 above

# # move connection string file back from config
# Write-Host "Copying web.ConnectionStrings.config to web dir"
# If (Test-Path "$rootfolder\config\web.connectionstrings.config"){
# 	Write-Host "Moving web.connectionstrings.config from config dir"
# 	Copy-Item "$rootfolder\config\web.connectionstrings.config" "$webroot" -force
# }

# # move web.config file back from temp (restarts the app pool)
# If (Test-Path "$rootfolder\config\web.config"){
# 	Write-Host "Moving web.config from config dir"
# 	Copy-Item "$rootfolder\config\web.config" "$webroot" -force
# }


# ensure that the compilation debug is false
(Get-Content "$webroot\web.config").Replace('<compilation debug="true"', '<compilation debug="false"') | Set-Content "$webroot\web.config"

# delete the content directory if it exists as it was added by the deploy
# If (Test-Path "$webroot\Content"){
# 	Remove-Item "$webroot\Content" -Force -Confirm:$False -Recurse
# }

# move Content directory back from temp
# Write-Host "Moving Contents folder back from temp directory"
# Move-Item "$rootfolder\temp\Content" "$webroot\"

# move LEGACY directory back from temp
# Write-Host "Moving LEGACY ARENA folder back from temp directory"
# Move-Item "$rootfolder\temp\arena" "$webroot\"

# move App_Data directory back from temp
# Write-Host "Moving App_Data folder back from temp directory"
# Move-Item "$rootfolder\temp\App_Data" "$webroot\"

# move custom themes back from temp
# Write-Host "Moving Themes\Ulfberht folder back from temp directory"
# Move-Item "$rootfolder\temp\Ulfberht" "$webroot\Themes"

# remove any items that were deployed into the CheckinKids_CentralAZ Assets folder
# ...otherwise the move back from temp will not work correctly.
# If (Test-Path "$webroot\Themes\CheckinKids_CentralAZ\Assets"){
# 	Remove-Item "$webroot\Themes\CheckinKids_CentralAZ\Assets" -Force -Confirm:$False -Recurse
# }

# move custom Children's check-in theme Assets folder back from temp
# Write-Host "Moving Themes\CheckinKids_CentralAZ\Assets folder back from temp directory"
# Move-Item "$rootfolder\temp\CheckinKids_CentralAZ\Assets" "$webroot\Themes\CheckinKids_CentralAZ" -Force


## These steps taken care of by #2 above
# # move non com_lcbcchurch plugins back from temp\Plugins folder
# Write-Host "Moving non com_lcbcchurch/com_bemadev/com_bemaservices/org_newpointe Plugins back from temp Plugins directory"
# $files = GCI -path "$rootfolder\temp\Plugins" | Where-Object {$_.name -ne "com_lcbcchurch" -and $_.name -ne "com_bemadev" -and $_.name -ne "com_bemaservices" -and $_.name -ne "org_newpointe" -and $_.name -ne "com_shepherdchurch"}
# foreach ($file in $files) { Move-Item  "$rootfolder\temp\Plugins\$file" -Destination "$webroot\Plugins\" -Force }

# # move a robots file back from temp if it exists
# If (Test-Path "$rootfolder\temp\robots.txt"){
# 	Write-Host "Moving robots.txt file back from temp directory"
# 	Move-Item "$rootfolder\temp\robots.txt" "$webroot"
# }

# # copy any dlls from the manual-fix\bin folder to the web bin folder
# Write-Host "Copying any manual-fix\bin DLLs to web bin folder"
# $files = GCI -path "$rootfolder\manual-fixes\bin"
# foreach ($file in $files) { Copy-Item  "$rootfolder\manual-fixes\bin\$file" -Destination "$webroot\bin\" -Force}

# create empty migration flag
New-Item "$webroot\App_Data\Run.Migration" -type file -force

# set acl on migration flag file so the app has permissions to delete it
Write-Host "Setting read-write on the Run.Migration file"
$acl = Get-ACL "$webroot\App_Data\Run.Migration"
$accessRule= New-Object System.Security.AccessControl.FileSystemAccessRule("Everyone","FullControl","Allow")
$acl.AddAccessRule($accessRule)
Set-Acl "$webroot\App_Data\Run.Migration" $acl

# start web publishing service
Write-Host "Starting Web Publishing Service"
start-service -servicename w3svc

# start web site and app pool
Write-Host "Starting ApplicationPool and Website"
Start-WebAppPool -Name (Get-Website -Name "$env:APPLICATION_SITE_NAME").applicationPool
Start-Website -Name "$env:APPLICATION_SITE_NAME"

# delete deploy scripts
If (Test-Path "$webroot\deploy.ps1"){
	Remove-Item "$webroot\deploy.ps1"
}

If (Test-Path "$webroot\before-deploy.ps1"){
	Remove-Item "$webroot\before-deploy.ps1"
}

# delete the appveyor deploy cache
Write-Host "Deleting appveyor deploy cache"
If (Test-Path c:\appveyor){
	Remove-Item c:\appveyor -Force -Confirm:$False -Recurse
}

Write-Output "--------------------------------------------------"
Write-Output "Post-deploy script complete"
Write-Output "--------------------------------------------------"