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


# Restore per-instance files

Write-Host "Restoring instance files";
try { Copy-ContentsRecursively "$NewBackupRoot\App_Data\Cache"             "$AppRoot\App_Data\Cache" -ErrorAction SilentlyContinue; } catch { }
try { Copy-ContentsRecursively "$NewBackupRoot\App_Data\Logs"              "$AppRoot\App_Data\Logs" -ErrorAction SilentlyContinue; } catch { }
try { Copy-ContentsRecursively "$NewBackupRoot\App_Data\LuceneSearchIndex" "$AppRoot\App_Data\LuceneSearchIndex" -ErrorAction SilentlyContinue; } catch { }
try { Copy-ContentsRecursively "$NewBackupRoot\App_Data\Uploads"           "$AppRoot\App_Data\Uploads" -ErrorAction SilentlyContinue; } catch { }


# Get files needing templated
$TemplatedFiles = Get-TemplateFiles $AppRoot;

# Recreate links

Write-Host "Recreating links";

Get-FilesystemLinks $AppRoot | ForEach-Object { $_.Delete() }

Remove-Item -Path "$AppRoot\Content" -Force -Recurse -ErrorAction SilentlyContinue; ;
Remove-Item -Path "$AppRoot\wp-content" -Force -Recurse -ErrorAction SilentlyContinue; ;
Remove-Item -Path "$AppRoot\App_Data\Files" -Force -Recurse -ErrorAction SilentlyContinue; ;
Remove-Item -Path "$AppRoot\App_Data\RockShop" -Force -Recurse -ErrorAction SilentlyContinue; ;
Remove-Item -Path "$AppRoot\App_Data\InstalledStorePackages.json" -Force -Recurse -ErrorAction SilentlyContinue; ;

New-Item -ItemType SymbolicLink -Path $AppRoot            -Name "Content"                     -Target "\\newpointe.file.core.windows.net\rock-content-files\Content"                     | Out-Null;
New-Item -ItemType SymbolicLink -Path $AppRoot            -Name "wp-content"                  -Target "\\newpointe.file.core.windows.net\rock-content-files\wp-content"                  | Out-Null;
New-Item -ItemType SymbolicLink -Path "$AppRoot\App_Data" -Name "Files"                       -Target "\\newpointe.file.core.windows.net\rock-content-files\Files"                       | Out-Null;
New-Item -ItemType SymbolicLink -Path "$AppRoot\App_Data" -Name "RockShop"                    -Target "\\newpointe.file.core.windows.net\rock-content-files\RockShop"                    | Out-Null;
New-Item -ItemType SymbolicLink -Path "$AppRoot\App_Data" -Name "InstalledStorePackages.json" -Target "\\newpointe.file.core.windows.net\rock-content-files\InstalledStorePackages.json" | Out-Null;


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


Start-IISSite "Default Web Site";


# Move the new backup

Write-Host "Finishing up";
Remove-Item -Recurse -Force $OldBackupRoot;
Copy-ContentsRecursively -Force $NewBackupRoot $OldBackupRoot;
Remove-Item -Recurse -Force $NewBackupRoot;
