# --------------------------------------------------
# ./before-deploy.ps1
# This script is run by AppVeyor's deploy agent before the deploy
# --------------------------------------------------

# --------------------------- #
#      Utility Functions      #
# --------------------------- #

function Join-Paths {
    $initialPath, $remainingParts= $args;
    $remainingParts | ForEach-Object { $initialPath = Join-Path -Path $initialPath -ChildPath $_ };
    return $initialPath;
}

function Assert-Exists($ItemPath) {
    if(Test-Path $ItemPath -and [string]::IsNullOrWhiteSpace((Get-Item $ItemPath).Value)) {
        Write-Error "$ItemPath is empty or not set, aborting!";
        exit;
    }
}

function Backup-RockFile([string] $RockWebFile) {
    $RockLocation = Join-Path $RootLocation $RockWebFile;
    $BackupLocation = Join-Paths $TempLocation "SavedFiles" $RockWebFile;
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

function Get-RockThemePaths() {
    return Get-ChildItem (Join-Path $RootLocation "Themes");
}

function Test-ThemeCanCompile([string] $Path) {
    return -Not (Join-Paths $Path "Styles" ".nocompile" | Test-Path);
}

function Copy-DirectoryContentsRecursivelyWithSaneLinkHandling([string] $DirectoryToCopy, [string] $Destination) {
    New-Item -ItemType Directory $Destination -Force | Out-Null;
    foreach($Child in Get-ChildItem $DirectoryToCopy) {
        if($Child.LinkType) {
            $LinkTarget, $OtherTargets = $Child.Target;
            New-Item -ItemType $Child.LinkType -Path $Destination -Name $Child.Name -Target $LinkTarget -Force | Out-Null;
        }
        elseif($Child.PSIsContainer) {
            Copy-DirectoryContentsRecursivelyWithSaneLinkHandling (Join-Path $DirectoryToCopy $Child.Name) (Join-Path $Destination $Child.Name);
        }
        else {
            Copy-Item (Join-Path $DirectoryToCopy $Child.Name) (Join-Path $Destination $Child.Name) -Force;
        }
    }
}
function Remove-ItemsAgressively([string] $Path) {

    # Move to the directory
    Push-Location $Path;

    # Take ownership of it
    cmd.exe /C "takeown /F . /R /D y"  | Out-Null;

    # Grant ourselves rights to it
    cmd.exe /C "icacls . /grant administrators:F /T" | Out-Null;

    # Remove all of the contents
    Remove-Item -Recurse -Force "$Path\*" | Out-Null;

    # Go back to the original directory
    Pop-Location;

}

# ------------------------------ #
#      Before-Deploy Script      #
# ------------------------------ #

# Make sure the app path and job id exist
Assert-Exists "env:APPLICATION_PATH";
Assert-Exists "env:APPVEYOR_JOB_ID";

# Build the paths for things
$RootPath = $env:APPLICATION_PATH;
$RootBackupPath = "$($RootPath.TrimEnd("/\\")).backup";
$InProgressRootBackupPath = "$($RootBackupPath.TrimEnd("/\\")).deploy-in-progress";
$TempLocation = Join-Path $env:Temp $env:APPVEYOR_JOB_ID;

# Make sure the temp location exists
New-Item $TempLocation -ItemType Directory | Out-Null;

# Some fancy output
Write-Host "===== NP Rock Deployment script v0.1 =====";
Write-Host "Mode: Pre-deploy";
Write-Host "Application: $env:APPVEYOR_PROJECT_NAME";
Write-Host "Build Number: $env:APPVEYOR_BUILD_VERSION";
Write-Host "Deploy Location: $RootPath";

if(Test-Path "env:DEPLOY_DEBUG") {
    Write-Host "================= DEBUG ==================";
    Write-Host "Working Directory: $(Get-Location)";
    Write-Host "Environment:";
    Get-ChildItem "env:";
}

Write-Host "==========================================";

# Save or restore a backup of the website folder
if (Test-Path $InProgressRootBackupPath) {

    Write-Host "Detected a deployment backup, assuming old deployment failed and restoring...";

    # Stop the website
    Stop-IISSite -Name "Default Web Site" -Confirm:$false;

    # Remove everything
    Remove-Item -Recurse -Force $RootPath\* | Out-Null;

    # Copy everything from the backup to the root directory
    Copy-DirectoryContentsRecursivelyWithSaneLinkHandling $InProgressRootBackupPath $RootPath;

}
else {

    Write-Host "Creating a deployment backup (If something fails please run the deployment again)...";

    # Copy everything from the root directory to a backup
    Copy-DirectoryContentsRecursivelyWithSaneLinkHandling $RootPath $InProgressRootBackupPath;

}

# Try to put the app into maintenence mode

# Apparently just adding an app_offline.htm file isn't enough
# See: https://web.archive.org/web/20160704222144/http://blog.kurtschindler.net/more-app_offline-htm-woes/

# Complete steps:
#  - Add app_offline.htm
#  - Replace web.config with an empty one w/ a high change notification setting
#  - Request a page from the site

Write-Host "Putting application in maintenence mode";

Move-Item -Path (Join-Path $RootPath "app_offline-template.htm") -Destination (Join-Path $RootPath "app_offline.htm") -ErrorAction SilentlyContinue;

Set-Content (Join-Path $RootPath "web.config") @'
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <system.web>
    <customErrors mode="Off"/>
    <httpRuntime waitChangeNotification="3000" maxWaitChangeNotification="3000"/>
    <compilation debug="false" targetFramework="4.7.2" optimizeCompilations="true" batch="true">
      <assemblies>
        <add assembly="System.Globalization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" />
        <add assembly="System.Runtime, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" />
        <add assembly="System.Runtime.Caching, Version=4.0.0.0, Culture=neutral, PublicKeyToken=B03F5F7F11D50A3A" />
        <add assembly="System.ComponentModel.Composition, Version=4.0.0.0, Culture=neutral, PublicKeyToken=B77A5C561934E089" />
        <add assembly="System.Net.Http, Version=4.0.0.0, Culture=neutral, PublicKeyToken=B03F5F7F11D50A3A" />
        <add assembly="System.IO.Compression, Version=4.0.0.0, Culture=neutral, PublicKeyToken=B77A5C561934E089" />
        <add assembly="System.IO.Compression.FileSystem, Version=4.0.0.0, Culture=neutral, PublicKeyToken=B77A5C561934E089" />
        <add assembly="WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35" />
        <remove assembly="ClearScriptV8-64" />
        <remove assembly="ClearScriptV8-32" />
      </assemblies>
    </compilation>
  </system.web>
  <system.webServer>
     <modules runAllManagedModulesForAllRequests="true"  />
  </system.webServer>
</configuration>
'@

Invoke-WebRequest "http://localhost" | Out-Null


# Backup server-generated files

Write-Host "Backing up server generated files";

Backup-RockFile "App_Data\Cache";
Backup-RockFile "App_Data\Logs";
Backup-RockFile "App_Data\LuceneSearchIndex";
Backup-RockFile "App_Data\Uploads";

# Save theme customizations and generated theme css
$FilesToSave = "checkin-theme.css","bootstrap.css","theme.css";

foreach ($ThemePath in Get-RockThemePaths) {

    if (Test-ThemeCanCompile $ThemePath.FullName) {

        foreach($File in $FilesToSave) {
            $LocalPath = Join-Paths "Themes" $Theme.Name "Styles" $File;
            if (Test-Path (Join-Path $RootPath $LocalPath)) {
                Backup-RockFile $LocalPath;
            }
        }

    }

}


# 4. Clear out the asp.net temp files

Write-Host "Clearing ASP.Net temporary files";

Remove-ItemsAgressively "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\Temporary ASP.NET Files\root";

# Done!

Write-Host "Deployment script finished successfully";