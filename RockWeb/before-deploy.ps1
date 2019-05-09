# This script is run by AppVeyor's deploy agent before the deploy
Import-Module WebAdministration

if([string]::IsNullOrWhiteSpace($env:APPLICATION_PATH)) {
    Write-Error "APPLICATION_PATH is not set, aborting!";
    exit;
}
if([string]::IsNullOrWhiteSpace($env:APPVEYOR_JOB_ID)) {
    Write-Error "APPVEYOR_JOB_ID is not set, aborting!";
    exit;
}

# Get the application (web root), application_path, and tempLocation for use in copying files around
$webroot = $env:RockWebRootPath
$RootLocation = $env:APPLICATION_PATH;
$TempLocation = Join-Path $env:Temp $env:APPVEYOR_JOB_ID;
New-Item $TempLocation -ItemType Directory | Out-Null;
$FileBackupLocation = Join-Path $TempLocation "SavedFiles";


Write-Output "Running pre-deploy script"
Write-Output "--------------------------------------------------"
Write-Host "Application: $env:APPVEYOR_PROJECT_NAME";
Write-Host "Build Number: $env:APPVEYOR_BUILD_VERSION";
Write-Host "Job ID: $env:APPVEYOR_JOB_ID";
Write-Host "Deploy Location: $RootLocation";
Write-Host "Temp Location: $TempLocation";
Write-Host "File Backup Location: $FileBackupLocation";
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

function Backup-RockFile([string] $RockWebFile) {
    $RockLocation = Join-Path $RootLocation $RockWebFile;
    $BackupLocation = Join-Path $FileBackupLocation $RockWebFile;
    if (Test-Path $RockLocation) {
        Write-Host "Backing up '$RockWebFile'";
        $BackupParentLocation = Split-Path $BackupLocation;
        New-Item $BackupParentLocation -ItemType Directory -Force | Out-Null
        Move-Item $RockLocation $BackupLocation;
    }
    else {
        Write-Warning "Could not backup '$RockWebFile': Location does not exist.";
    }
}
####################################################################################################
####################################################################################################

### 1. Stop Server and App Pool

# stop execution of the deploy if the moves fail
$ErrorActionPreference = "Stop"

# stop web publishing service - needed to allow the deploy to overwrite the sql server spatial types
Write-Host "Stopping Web Publishing Service"
stop-service -servicename w3svc

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
write-output "$(Get-Date -Format G) Waiting 10 seconds for IIS to shutdown..."
Start-Sleep -s 10
write-output "$(Get-Date -Format G) Continuing on..."

# TODO: Figure out if this really works...
# load the app offline template
# If (Test-Path "$webroot\app_offline-template.htm"){
# 	Write-Host "Loading the app offline template"
# 	Copy-Item "$webroot\app_offline-template.htm" "$webroot\app_offline.htm" -force
# }


### 2. Save server-specifig files like configs, FontAwesome assets, and built theme files

Write-Host "Saving server-specific files";

Backup-RockFile "web.config"
Backup-RockFile "web.connectionstrings.config"
Backup-RockFile "Assets\Fonts\FontAwesome"
Backup-RockFile "Styles\FontAwesome"

# Save theme customizations and generated theme css
$FilesToSave = "checkin-theme.css","bootstrap.css","theme.css","email-editor.css","_css-overrides.less","_variable-overrides.less";

$ThemesLocation = Join-Path $RootLocation "Themes";
foreach ($Theme in Get-ChildItem $ThemesLocation) {

    if (-Not (Test-Path (Join-Paths $Theme.Name "Styles" ".nocompile"))) {

        foreach($File in $FilesToSave) {
            $LocalPath = Join-Paths "Themes" $Theme.Name "Styles" $File;
            if (Test-Path (Join-Path $RootLocation $LocalPath)) {
                Backup-RockFile $LocalPath;
            }
        }
    }
}

Write-Host "Before-Deploy script finished successfully";


####################################################################################################
####################################################################################################
### 3. Put the app into maintenence mode ???

# # Apparently just adding an app_offline.htm file isn't enough
# # See: https://web.archive.org/web/20160704222144/http://blog.kurtschindler.net/more-app_offline-htm-woes/

# # Complete steps:
# #  - Add app_offline.htm
# #  - Replace web.config with an empty one w/ a high change notification setting
# #  - Request a page from the site

# Write-Host "Putting application in maintenence mode";

# Move-Item -Path (Join-Path $RootLocation "app_offline-template.htm") -Destination (Join-Path $RootLocation "app_offline.htm") -ErrorAction SilentlyContinue;

# Set-Content (Join-Path $RootLocation "web.config") @'
# <?xml version="1.0" encoding="utf-8" ?>
# <configuration>
#     <system.web>
#         <httpRuntime waitChangeNotification="300" maxWaitChangeNotification="300"/>
#     </system.web>
#     <system.webServer>
#         <modules runAllManagedModulesForAllRequests="true" />
#     </system.webServer>
# </configuration>
# '@

# Invoke-WebRequest "http://localhost/"

####################################################################################################
####################################################################################################