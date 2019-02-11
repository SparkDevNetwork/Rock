# --------------------------------------------------
# ./before-deploy.ps1
# This script is run by AppVeyor's deploy agent before the deploy
# --------------------------------------------------

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
New-Item $TempLocation -ItemType Directory

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

if(Test-Path "env:DEPLOY_DEBUG") {
    Write-Host "================= DEBUG ==================";
    Write-Host "Working Directories: $(Get-Location)";
    Write-Host "Environment:";
    Get-ChildItem "env:";
}

Write-Host "===== NP Rock Deployment script v0.1 =====";
Write-Host "Mode: Pre-deploy";
Write-Host "Application: $env:APPVEYOR_PROJECT_NAME";
Write-Host "Build Number: $env:APPVEYOR_BUILD_VERSION";
Write-Host "Deploy Location: $RootLocation";
Write-Host "==========================================";

# 1. Save or restore a backup of the website folder

$InProgressBackupFile = "$($RootLocation.TrimEnd("/\\")).backup.deploy-in-progress.zip";
if (Test-Path $InProgressBackupFile) {
    Write-Host "Detected a deployment backup file, assuming old deployment failed and restoring...";
    Remove-Item -Recurse -Force $RootLocation\*;
    Expand-Archive $InProgressBackupFile $RootLocation -Force
}
else {
    Write-Host "Creating a deployment backup file (If something fails please run the deployment again)...";
    Compress-Archive -Path $RootLocation\* -DestinationPath $InProgressBackupFile;
}


# 2. Put the app into maintenence mode

# Apparently just adding an app_offline.htm file isn't enough
# See: https://web.archive.org/web/20160704222144/http://blog.kurtschindler.net/more-app_offline-htm-woes/

# Complete steps:
#  - Add app_offline.htm
#  - Replace web.config with an empty one w/ a high change notification setting
#  - Request a page from the site

Write-Host "Putting application in maintenence mode";

Move-Item -Path (Join-Path $RootLocation "app_offline-template.htm") -Destination (Join-Path $RootLocation "app_offline.htm") -ErrorAction SilentlyContinue;

Set-Content (Join-Path $RootLocation "app_offline-template.htm") @'
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
    <system.web>
        <httpRuntime waitChangeNotification="300" maxWaitChangeNotification="300"/>
    </system.web>
    <system.webServer>
        <modules runAllManagedModulesForAllRequests="true" />
    </system.webServer>
</configuration>
'@


# 3. Save server-specifig files like static files, logs, plugin packages, and caches

Write-Host "Saving server-specific files";

Backup-RockFile "App_Data\Files";
Backup-RockFile "App_Data\Logs";
Backup-RockFile "App_Data\packages";
Backup-RockFile "App_Data\RockShop";
Backup-RockFile "App_Data\InstalledStorePackages.json";
Backup-RockFile "Content";
Backup-RockFile "wp-content";

# Save theme customizations and generated theme css
$FilesToSave = "checkin-theme.css","bootstrap.css","theme.css","_css-overrides.less","_variable-overrides.less";

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

# Done!

Write-Host "Deployment script finished successfully";