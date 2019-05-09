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

# load the app offline template
If (Test-Path "$webroot\app_offline-template.htm"){
	Write-Host "Loading the app offline template"
	Copy-Item "$webroot\app_offline-template.htm" "$webroot\app_offline.htm" -force
}

####################################################################################################
####################################################################################################
# Borrowed from NewPointe deploy script

if([string]::IsNullOrWhiteSpace($env:APPLICATION_PATH)) {
    Write-Error "APPLICATION_PATH is not set, aborting!";
    exit;
}
if([string]::IsNullOrWhiteSpace($env:APPVEYOR_JOB_ID)) {
    Write-Error "APPVEYOR_JOB_ID is not set, aborting!";
    exit;
}

$RootLocation = $env:APPLICATION_PATH;
$TempLocation = Join-Path $env:Temp $env:APPVEYOR_JOB_ID;
New-Item $TempLocation -ItemType Directory | Out-Null;

function Join-Paths {
    $path, $parts= $args;
    foreach ($part in $parts) {
        $path = Join-Path $path $part;
    }
    return $path;
}

$FileBackupLocation = Join-Path $TempLocation "SavedFiles";
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

# function Copy-DirectoryContentsRecursivelyWithSaneLinkHandling([string] $DirectoryToCopy, [string] $Destination) {
#     New-Item -ItemType Directory $Destination -Force | Out-Null;
#     foreach($Child in Get-ChildItem $DirectoryToCopy) {
#         if($Child.LinkType) {
#             $LinkTarget, $OtherTargets = $Child.Target;
#             New-Item -ItemType $Child.LinkType -Path $Destination -Name $Child.Name -Target $LinkTarget -Force | Out-Null;
#         }
#         elseif($Child.PSIsContainer) {
#             Copy-DirectoryContentsRecursivelyWithSaneLinkHandling (Join-Path $DirectoryToCopy $Child.Name) (Join-Path $Destination $Child.Name);
#         }
#         else {
#             Copy-Item (Join-Path $DirectoryToCopy $Child.Name) (Join-Path $Destination $Child.Name) -Force;
#         }
#     }
# }


# if(Test-Path "env:DEPLOY_DEBUG") {
#     Write-Host "================= DEBUG ==================";
#     Write-Host "Working Directories: $(Get-Location)";
#     Write-Host "Environment:";
#     Get-ChildItem "env:";
# }

Write-Host "Mode: Pre-deploy";
Write-Host "Application: $env:APPVEYOR_PROJECT_NAME";
Write-Host "Build Number: $env:APPVEYOR_BUILD_VERSION";
Write-Host "Deploy Location: $RootLocation";
Write-Host "==========================================";

# 1. Save or restore a backup of the website folder

# $InProgressBackupLocation = "$($RootLocation.TrimEnd("/\\")).backup.deploy-in-progress";
# if (Test-Path $InProgressBackupLocation) {
#     Write-Host "Detected a deployment backup, assuming old deployment failed and restoring...";
#     Remove-Item -Recurse -Force $RootLocation\*;
#     Copy-DirectoryContentsRecursivelyWithSaneLinkHandling $InProgressBackupLocation $RootLocation;
# }
# else {
#     Write-Host "Creating a deployment backup (If something fails please run the deployment again)...";
#     Copy-DirectoryContentsRecursivelyWithSaneLinkHandling $RootLocation $InProgressBackupLocation;
# }


# 2. Save server-specifig files like static files, logs, plugin packages, and caches

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


# # 3. Put the app into maintenence mode

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





# # backup web.config file
# If (Test-Path "$webroot\web.config"){
# 	Write-Host "Moving web.config to config dir"
# 	Copy-Item "$webroot\web.config" "$rootfolder\config" -force
# }

# # backup connection string file
# If (Test-Path "$webroot\web.connectionstrings.config"){
# 	Write-Host "Moving web.connectionstrings.config to config dir"
# 	Copy-Item "$webroot\web.connectionstrings.config" "$rootfolder\config" -force
# }


# # load the app offline template
# If (Test-Path "$webroot\app_offline-template.htm"){
# 	Write-Host "Loading the app offline template"
# 	Copy-Item "$webroot\app_offline-template.htm" "$webroot\app_offline.htm" -force
# }

# # delete the contents of the temp directory or create it
# If (Test-Path "$rootfolder\temp"){
# 	Remove-Item "$rootfolder\temp\*" -Force -Confirm:$False -Recurse
# } Else {
#   New-Item -ItemType Directory -Force -Path "$rootfolder\temp"
# }

# # delete the contents of the temp\Plugins directory or create it
# If (Test-Path "$rootfolder\temp\Plugins"){
# 	Remove-Item "$rootfolder\temp\Plugins\*" -Force -Confirm:$False -Recurse
# } Else {
#   New-Item -ItemType Directory -Force -Path "$rootfolder\temp\Plugins"
# }

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

# # move non com_lcbcchurch plugins to temp\Plugins folder
# If (Test-Path "$webroot\Plugins\"){
# 	Write-Host "Moving non com_lcbcchurch/com_bemadev/com_bemaservices/org_newpointe Plugins to temp Plugins directory"
# 	$files = GCI -path "$webroot\Plugins\" | Where-Object {$_.name -ne "com_lcbcchurch" -and $_.name -ne "com_bemadev" -and $_.name -ne "com_bemaservices" -and $_.name -ne "org_newpointe" -and $_.name -ne "com_shepherdchurch" -and $_.name -ne ".gitignore" -and $_.name -ne "readme.txt"}
# 	foreach ($file in $files) { Move-Item  "$webroot\Plugins\$file" -Destination "$rootfolder\temp\Plugins" -Force }
# }
# # move a robots file if it exists
# If (Test-Path "$webroot\robots.txt"){
# 	Move-Item "$webroot\robots.txt" "$rootfolder\temp"
# }
