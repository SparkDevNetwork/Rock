# This script is run by AppVeyor's deploy agent after the deploy
Import-Module WebAdministration

if([string]::IsNullOrWhiteSpace($env:APPLICATION_PATH)) {
    Write-Error "APPLICATION_PATH is not set, aborting!";
    exit;
}
if([string]::IsNullOrWhiteSpace($env:APPVEYOR_JOB_ID)) {
    Write-Error "APPVEYOR_JOB_ID is not set, aborting!"
    exit;
}

# Get the application (web root), application_path, and tempLocation for use in copying files around
$webroot = $env:RockWebRootPath
$RootLocation = $env:APPLICATION_PATH;
$TempLocation = Join-Path $env:Temp $env:APPVEYOR_JOB_ID;
$FileBackupLocation = Join-Path $TempLocation "SavedFiles";


Write-Output "Running post-deploy script"
Write-Output "--------------------------------------------------"
Write-Host "Application: $env:APPVEYOR_PROJECT_NAME";
Write-Host "Build Number: $env:APPVEYOR_BUILD_VERSION";
Write-Host "Job ID: $env:APPVEYOR_JOB_ID";
Write-Host "Deploy Location: $RootLocation";
Write-Host "Temp Location: $TempLocation";
Write-Host "File Backup Location: $FileBackupLocation";
# Write-Output "Root folder: $rootfolder"
Write-Output "Web root folder: $webroot"
Write-Output "Running script as: $env:userdomain\$env:username"
Write-Host "====================================================";

####################################################################################################
####################################################################################################
# Functions borrowed from NewPointe deploy script
#################################################
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
####################################################################################################
####################################################################################################


### 1. Restore server-specific files like configs, FontAwesome assets, and built theme files

Write-Host "Restoring server-specific files";
Copy-DirectoryContentsRecursivelyWithSaneLinkHandling $FileBackupLocation $RootLocation;

### 2. Reinstall plugins

Write-Host "Reinstalling Plugin Files";

$InstalledPluginsPath = Join-Paths $RootLocation "App_Data" "RockShop";
if(Test-Path $InstalledPluginsPath) {

    $InstalledPlugins = Get-ChildItem $InstalledPluginsPath;
    foreach ($Plugin in $InstalledPlugins) {

        $PluginVersions = Get-ChildItem $Plugin.FullName;
        if($PluginVersions.Count -gt 0) {

            $LatestVersion = $PluginVersions  | Sort-Object {Get-VersionId $_.BaseName} | Select-Object -Last 1;
            Write-Host "Restoring $($LatestVersion.FullName)";
            Restore-RockPlugin $LatestVersion.FullName;
        }
    }
}

### 3. Clean up temp folder

Remove-Item $TempLocation -Recurse -Force;

####################################################################################################
####################################################################################################
### 4. Take the app out of maintenence mode ?

# Write-Host "Taking application out of maintenence mode";

# Move-Item -Path (Join-Path $RootLocation "app_offline.htm") -Destination (Join-Path $RootLocation "app_offline-template.htm") -ErrorAction SilentlyContinue;
# Remove-Item -Path (Join-Path $RootLocation "app_offline.htm") -ErrorAction SilentlyContinue;
####################################################################################################
####################################################################################################


### 4. ensure that the compilation debug is false
(Get-Content "$webroot\web.config").Replace('<compilation debug="true"', '<compilation debug="false"') | Set-Content "$webroot\web.config"

### 5. Create empty migration file and set permissions
New-Item "$webroot\App_Data\Run.Migration" -type file -force

# set acl on migration flag file so the app has permissions to delete it
Write-Host "Setting read-write on the Run.Migration file"
$acl = Get-ACL "$webroot\App_Data\Run.Migration"
$accessRule= New-Object System.Security.AccessControl.FileSystemAccessRule("Everyone","FullControl","Allow")
$acl.AddAccessRule($accessRule)
Set-Acl "$webroot\App_Data\Run.Migration" $acl

### 6. Restart Server and App Pool

# start web publishing service
Write-Host "Starting Web Publishing Service"
start-service -servicename w3svc

# start web site and app pool
Write-Host "Starting ApplicationPool and Website"
Start-WebAppPool -Name (Get-Website -Name "$env:APPLICATION_SITE_NAME").applicationPool
Start-Website -Name "$env:APPLICATION_SITE_NAME"

### 7. Cleanup

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