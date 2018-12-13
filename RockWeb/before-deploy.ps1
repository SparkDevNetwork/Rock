# --------------------------------------------------
# ./before-deploy.ps1
# This script is run by AppVeyor's deploy agent before the deploy
# --------------------------------------------------

$RootLocation = $env:APPLICATION_PATH;
$TempLocation = Join-Path $env:Temp $env:APPVEYOR_JOB_ID;
New-Item $TempLocation -ItemType Directory

function Backup-RockFile([string] $RockWebFile) {
    $RockLocation = Join-Path $RootLocation $RockWebFile;
    $BackupLocation = Join-Path $TempLocation $RockWebFile;
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


# 1. Put the app into maintenence mode

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


# 2. Save server-specifig files like static files, logs, plugin packages, and caches

Write-Host "Saving server-specific files";

Backup-RockFile "App_Data\Files";
Backup-RockFile "App_Data\Logs";
Backup-RockFile "App_Data\packages";
Backup-RockFile "App_Data\RockShop";
Backup-RockFile "App_Data\InstalledStorePackages.json";
Backup-RockFile "Content";
Backup-RockFile "wp-content";

# We'll also save the generated theme css since it takes forever to regenerate itself on startup.
Backup-RockFile "Themes\CheckinAdventureKids\Styles\checkin-theme.css"
Backup-RockFile "Themes\CheckinAtTheMovies\Styles\checkin-theme.css"
Backup-RockFile "Themes\CheckinBlueCrystal\Styles\checkin-theme.css"
Backup-RockFile "Themes\CheckinNewPointeOrange\Styles\checkin-theme.css"
Backup-RockFile "Themes\CheckinPark\Styles\checkin-theme.css"
Backup-RockFile "Themes\CheckinPointePark\Styles\checkin-theme.css"
Backup-RockFile "Themes\DashboardStark\Styles\bootstrap.css"
Backup-RockFile "Themes\DashboardStark\Styles\theme.css"
Backup-RockFile "Themes\Flat\Styles\bootstrap.css"
Backup-RockFile "Themes\Flat\Styles\theme.css"
Backup-RockFile "Themes\KioskStark\Styles\bootstrap.css"
Backup-RockFile "Themes\KioskStark\Styles\theme.css"
Backup-RockFile "Themes\LandingPage\Styles\bootstrap.css"
Backup-RockFile "Themes\LandingPage\Styles\theme.css"
Backup-RockFile "Themes\NewFamilyCheckin\Styles\bootstrap.css"
Backup-RockFile "Themes\NewFamilyCheckin\Styles\theme.css"
Backup-RockFile "Themes\NewPointeInstitute\Styles\bootstrap.css"
Backup-RockFile "Themes\NewPointeInstitute\Styles\theme.css"
Backup-RockFile "Themes\NewpointeLive\Styles\bootstrap.css"
Backup-RockFile "Themes\NewpointeLive\Styles\theme.css"
Backup-RockFile "Themes\NewpointeMain\Styles\bootstrap.css"
Backup-RockFile "Themes\NewpointeMain\Styles\theme.css"
Backup-RockFile "Themes\PointeBlank\Styles\bootstrap.css"
Backup-RockFile "Themes\PointeBlank\Styles\theme.css"
Backup-RockFile "Themes\Rock\Styles\bootstrap.css"
Backup-RockFile "Themes\Rock\Styles\email-editor.css"
Backup-RockFile "Themes\Rock\Styles\theme.css"
Backup-RockFile "Themes\RockOriginal\Styles\bootstrap.css"
Backup-RockFile "Themes\RockOriginal\Styles\email-editor.css"
Backup-RockFile "Themes\RockOriginal\Styles\font-awesome.css"
Backup-RockFile "Themes\RockOriginal\Styles\theme.css"
Backup-RockFile "Themes\Stark\Styles\bootstrap.css"
Backup-RockFile "Themes\Stark\Styles\theme.css"

# Done!

Write-Host "Deployment script finished successfully";