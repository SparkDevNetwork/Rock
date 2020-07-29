Import-Module "$PSScriptRoot\lib\Util";
Import-Module "$PSScriptRoot\lib\Rock";


# Make sure we know where the app root is

if( -not (Test-Path "Env:\APPLICATION_PATH" )){
    throw "Could not determine the application install path. Make sure the APPLICATION_PATH environment variable is set.";
}


# Set up some variables

$AppRoot       = $env:APPLICATION_PATH;
$OldBackupRoot = "$AppRoot.backup";
$NewBackupRoot = "$AppRoot.backup.new";

$REMOTE_STORE_ROOT = "\\newpointe.file.core.windows.net\rock-content-files";


# Restore per-instance files

Write-Host "Restoring instance files";
try { Copy-ContentsRecursively "$NewBackupRoot\App_Data\Cache"             "$AppRoot\App_Data\Cache" -ErrorAction SilentlyContinue; } catch { }
try { Copy-ContentsRecursively "$NewBackupRoot\App_Data\Logs"              "$AppRoot\App_Data\Logs" -ErrorAction SilentlyContinue; } catch { }
try { Copy-ContentsRecursively "$NewBackupRoot\App_Data\LuceneSearchIndex" "$AppRoot\App_Data\LuceneSearchIndex" -ErrorAction SilentlyContinue; } catch { }
try { Copy-ContentsRecursively "$NewBackupRoot\App_Data\Uploads"           "$AppRoot\App_Data\Uploads" -ErrorAction SilentlyContinue; } catch { }


# Restore old compiled theme files (It takes a while to compile them on startup, so we'll use the old ones until they're done)

Write-Host "Restoring compiled theme files";

$Themes = Get-RockThemes $AppRoot;

foreach ($Theme in $Themes) {

    if($Theme.AllowsCompile) {

        $StylesRelativePath = Join-Path $Theme.RelativePath "Styles";

        $NewStylesPath = Join-Path $AppRoot $StylesRelativePath;
        $OldStylesPath = Join-Path $NewBackupRoot $StylesRelativePath;

        $CompilableFiles = Get-ChildItem $NewStylesPath | Where-Object { ($_.Name.EndsWith(".less")) -and (-not ($_.Name.StartsWith("_"))) }
        $CompiledFiles = $CompilableFiles -replace ".less", ".css";

        foreach($File in $CompiledFiles) {

            $OldFilePath = Join-Path $OldStylesPath $File.Name;
            $NewFilePath = Join-Path $NewStylesPath $File.Name;

            if( Test-Path $OldFilePath) {
                Copy-Item $NewFilePath $NewFilePath;
            }

        }

    }

}


# Get files needing templated
# Do this before linking to remote files so it doesn't take forever searching through all of them
$TemplatedFiles = Get-TemplateFiles $AppRoot;

# Recreate links

Write-Host "Recreating links";

# Clean out any pre-existing links (There shouldn't be any, but just in case)
Get-FilesystemLinks $AppRoot | ForEach-Object { $_.Delete() }

# Connect the various content files
Connect-RemoteFile $AppRoot             $REMOTE_STORE_ROOT "Content";
Connect-RemoteFile $AppRoot             $REMOTE_STORE_ROOT "wp-content";
Connect-RemoteFile "$AppRoot\App_Data"  $REMOTE_STORE_ROOT "Files";
Connect-RemoteFile "$AppRoot\App_Data"  $REMOTE_STORE_ROOT "RockShop";
Connect-RemoteFile "$AppRoot\App_Data"  $REMOTE_STORE_ROOT "InstalledStorePackages.json";

# Connect the various theme customizations
$Themes = Get-RockThemes $AppRoot;

foreach ($Theme in $Themes) {

    $LocalStylesPath = Join-Path $Theme.AbsolutePath "Styles";

    $OverrideFiles = Get-ChildItem $LocalStylesPath | Where-Object { $_.Name.EndsWith("-overrides.less") };

    foreach ($File in $OverrideFiles) {

        $LocalRoot = Join-Paths $AppRoot "Themes" $Theme.Name "Styles";
        $RemoteRoot = Join-Paths $REMOTE_STORE_ROOT "Themes" $Theme.Name "Styles";

        Connect-RemoteFile $LocalRoot $RemoteRoot $File.Name;

    }

}


# Re-install plugins

Write-Host "Installing plugins";
Get-RockInstalledStorePackages $AppRoot | Get-RockCachedPluginFile $AppRoot | Install-RockPlugin $AppRoot;


# Apply file templating

Write-Host "Applying file templates";
$TemplatedFiles | Install-TemplatedFile;


# Disable the app_offline.htm file

Write-Host "Bringing site online";
try {
    Move-Item -Path (Join-Path $AppRoot "app_offline.htm") -Destination (Join-Path $AppRoot "app_offline-template.htm") -Force -ErrorAction SilentlyContinue | Out-Null;
}
catch { }


# Try to clear the asp.net temporary files
# This seems to keep some weird things from happening. For example, we had a
# problem with plugin Rest APIs disappearing for no reason with no way of
# reproducing the issue - but this fixed it so ¯\_(ツ)_/¯

Write-Host "Clearing temp files";
try {
    Remove-ContentsAgressively "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\Temporary ASP.NET Files\root";
}
catch { }


# Start the IIS site

Start-IISSite "Default Web Site";


# Move the new backup

Write-Host "Finishing up";

Get-FilesystemLinks $OldBackupRoot | ForEach-Object { $_.Delete() };
Remove-Item -Recurse -Force $OldBackupRoot;

Move-Item -Force $NewBackupRoot $OldBackupRoot;
