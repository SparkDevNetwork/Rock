# --------------------------------------------------
# ./deploy.ps1
# This script is run by AppVeyor's deploy agent after the deploy
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


# 1. Restore server-specific files like static files, logs, plugin packages, and caches

Write-Host "Restoring server-specific files";
$FileBackupLocation = Join-Path $TempLocation "SavedFiles";
Copy-DirectoryContentsRecursivelyWithSaneLinkHandling $FileBackupLocation $RootLocation;


# 2. Rewrite templated files

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


# 3. Reinstall plugins

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


# 4. Clean up temp folder

Remove-Item $TempLocation -Recurse -Force;


# 5. Take the app out of maintenence mode

Write-Host "Taking application out of maintenence mode";

Move-Item -Path (Join-Path $RootLocation "app_offline.htm") -Destination (Join-Path $RootLocation "app_offline-template.htm") -ErrorAction SilentlyContinue;
Remove-Item -Path (Join-Path $RootLocation "app_offline.htm") -ErrorAction SilentlyContinue;


# 6. Move the backup so we know we completed successfully

$InProgressBackupLocation = "$($RootLocation.TrimEnd("/\\")).backup.deploy-in-progress";
$SuccessBackupLocation = "$($RootLocation.TrimEnd("/\\")).backup";
Remove-Item $SuccessBackupLocation -Recurse -Force | Out-Null;
Move-Item $InProgressBackupLocation $SuccessBackupLocation -Force;


Write-Host "Deployment script finished successfully";