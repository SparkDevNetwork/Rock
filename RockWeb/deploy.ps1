# --------------------------------------------------
# ./deploy.ps1
# This script is run by AppVeyor's deploy agent after the deploy
# --------------------------------------------------

$RootLocation = $env:APPLICATION_PATH;
$TempLocation = Join-Path $env:Temp $env:APPVEYOR_JOB_ID;


function Restore-RockFile([string] $RockWebFile) {
    $RockLocation = Join-Path $RootLocation $RockWebFile;
    $BackupLocation = Join-Path $TempLocation $RockWebFile;
    if (Test-Path $BackupLocation) {
        Write-Host "Restoring '$RockWebFile'";
        if(Test-Path $RockLocation) {
            Remove-Item $RockLocation -Recurse
        }
        Move-Item $BackupLocation $RockLocation;
    }
    else {
        Write-Warning "Could not restore '$RockWebFile': Location does not exist.";
    }
}

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


if(Test-Path "env:DEPLOY_DEBUG") {
    Write-Host "================= DEBUG ==================";
    Write-Host "Working Directories: $(Get-Location)";
    Write-Host "Environment:";
    Get-ChildItem "env:";
}

Write-Host "===== NP Rock Deployment script v0.1 =====";
Write-Host "Mode: Post-deploy";
Write-Host "Application: $env:APPVEYOR_PROJECT_NAME";
Write-Host "Build Number: $env:APPVEYOR_BUILD_VERSION";
Write-Host "Deploy Location: $RootLocation";
Write-Host "==========================================";

Write-Host "Restoring server-specific files";



# Restore server-specifig files like static files, logs, plugin packages, and caches
Restore-RockFile "App_Data\Files";
Restore-RockFile "App_Data\Logs";
Restore-RockFile "App_Data\packages";
Restore-RockFile "App_Data\RockShop";
Restore-RockFile "App_Data\InstalledStorePackages.json";
Restore-RockFile "Content";
Restore-RockFile "wp-content";

# We'll also restore the generated theme css since it takes forever to regenerate itself on startup.
Restore-RockFile "Themes\CheckinAdventureKids\Styles\checkin-theme.css"
Restore-RockFile "Themes\CheckinAtTheMovies\Styles\checkin-theme.css"
Restore-RockFile "Themes\CheckinBlueCrystal\Styles\checkin-theme.css"
Restore-RockFile "Themes\CheckinNewPointeOrange\Styles\checkin-theme.css"
Restore-RockFile "Themes\CheckinPark\Styles\checkin-theme.css"
Restore-RockFile "Themes\CheckinPointePark\Styles\checkin-theme.css"
Restore-RockFile "Themes\DashboardStark\Styles\bootstrap.css"
Restore-RockFile "Themes\DashboardStark\Styles\theme.css"
Restore-RockFile "Themes\Flat\Styles\bootstrap.css"
Restore-RockFile "Themes\Flat\Styles\theme.css"
Restore-RockFile "Themes\KioskStark\Styles\bootstrap.css"
Restore-RockFile "Themes\KioskStark\Styles\theme.css"
Restore-RockFile "Themes\LandingPage\Styles\bootstrap.css"
Restore-RockFile "Themes\LandingPage\Styles\theme.css"
Restore-RockFile "Themes\NewFamilyCheckin\Styles\bootstrap.css"
Restore-RockFile "Themes\NewFamilyCheckin\Styles\theme.css"
Restore-RockFile "Themes\NewPointeInstitute\Styles\bootstrap.css"
Restore-RockFile "Themes\NewPointeInstitute\Styles\theme.css"
Restore-RockFile "Themes\NewpointeLive\Styles\bootstrap.css"
Restore-RockFile "Themes\NewpointeLive\Styles\theme.css"
Restore-RockFile "Themes\NewpointeMain\Styles\bootstrap.css"
Restore-RockFile "Themes\NewpointeMain\Styles\theme.css"
Restore-RockFile "Themes\PointeBlank\Styles\bootstrap.css"
Restore-RockFile "Themes\PointeBlank\Styles\theme.css"
Restore-RockFile "Themes\Rock\Styles\bootstrap.css"
Restore-RockFile "Themes\Rock\Styles\email-editor.css"
Restore-RockFile "Themes\Rock\Styles\theme.css"
Restore-RockFile "Themes\RockOriginal\Styles\bootstrap.css"
Restore-RockFile "Themes\RockOriginal\Styles\email-editor.css"
Restore-RockFile "Themes\RockOriginal\Styles\font-awesome.css"
Restore-RockFile "Themes\RockOriginal\Styles\theme.css"
Restore-RockFile "Themes\Stark\Styles\bootstrap.css"
Restore-RockFile "Themes\Stark\Styles\theme.css"

Write-Host "Rewriting Templated Files";

$TemplateFilenamePattern = "*.template.*" # something.template.txt
$TemplateVariableRegex = "\[\[(\w+)]]";   # [[Variable_Name]]

# For each template file
$TemplateFiles = Get-ChildItem $RootLocation -Recurse -Include $TemplateFilenamePattern;
foreach($TemplateFile in $TemplateFiles) {

    Write-Host "Rewriting $TemplateFile";

    # Get the raw contents
    $TemplateContents = Get-Content $TemplateFile -Raw;

    # Get a list of all the variables
    $TemplateVariables = ($TemplateContents | Select-String -AllMatches $TemplateVariableRegex).Matches | ForEach-Object { $_.Groups[1].Value } | Sort-Object | Get-Unique;

    # For each needed variable
    foreach($TemplateVariable in $TemplateVariables) {

        $EnvVar = "DEPLOY_$TemplateVariable".ToUpper();

        # Check that it's in the environment
        if(Test-Path "env:$EnvVar") {
            
            # Update it's value
            $TemplateContents = $TemplateContents.Replace("[[$TemplateVariable]]", (Get-Item "env:$EnvVar").Value);
        }
        else {
            Write-Warning "Could not update '[[$TemplateVariable]]' in '$TemplateFile'. Environment variable '$EnvVar' is not set.";
        }

    }

    # Save the new file
    $TemplateTempLocation = $TemplateFile -replace ".template", "";
    Set-Content $TemplateTempLocation $TemplateContents;
}

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

Remove-Item $TempLocation -Recurse -Force;

Write-Host "Taking application out of maintenence mode";

Move-Item -Path (Join-Path $RootLocation "app_offline.htm") -Destination (Join-Path $RootLocation "app_offline-template.htm") -ErrorAction SilentlyContinue;
Remove-Item -Path (Join-Path $RootLocation "app_offline.htm") -ErrorAction SilentlyContinue;

Write-Host "Deployment script finished successfully";