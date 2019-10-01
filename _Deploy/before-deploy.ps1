Import-Module "$PSScriptRoot\lib\Util";
Import-Module "$PSScriptRoot\lib\Rock";
Import-Module IISAdministration;


# Make sure we know where the app root is

if ( -not (Test-Path "Env:\APPLICATION_PATH" )) {
    throw "Could not determine the application install path. Make sure the APPLICATION_PATH environment variable is set.";
}


# Set up some variables

$AppRoot = $env:APPLICATION_PATH;
$NewBackupRoot = "$AppRoot.backup.new";


# Stop the website
Stop-IISSite "Default Web Site" -Confirm:$false;

# Kill IIS so it stops locking msvcr120.dll and failing the deploy
$process = Get-Process -Name w3wp -ErrorAction SilentlyContinue;
if( -not $null -eq $process ) {
    $process.Kill();
}

# See if the new backup already exists (Indicates the previous deploy script failed to complete)

if (Test-Path $NewBackupRoot) {

    # Restore the web root
    Write-Host "Restoring files";
    Copy-ContentsRecursively $NewBackupRoot $AppRoot -Force;

}
else {

    # Backup the web root
    Write-Host "Backing up files";
    Copy-ContentsRecursively $AppRoot $NewBackupRoot -Force;

}


# Enable the app_offline.htm file

Write-Host "Taking site offline";
Move-Item -Path (Join-Path $AppRoot "app_offline-template.htm") -Destination (Join-Path $AppRoot "app_offline.htm") -Force -ErrorAction SilentlyContinue | Out-Null;


# Rewrite the web.config so app_offline works right and we can replace bin files

[xml]$ConfigTemplateXML = Get-Content -Raw "$AppRoot/web.template.config";
$NeededAssemblies = $ConfigTemplateXML.configuration.'system.web'.compilation.assemblies.InnerXml;

Set-Content "$AppRoot/web.config" @"
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <system.web>
    <customErrors mode="Off"/>
    <httpRuntime waitChangeNotification="3000" maxWaitChangeNotification="3000"/>
    <compilation debug="false" targetFramework="4.7.2" optimizeCompilations="true" batch="false">
      <assemblies>$NeededAssemblies</assemblies>
    </compilation>
  </system.web>
  <system.webServer>
     <modules runAllManagedModulesForAllRequests="true" />
  </system.webServer>
</configuration>
"@


# Make a request to load the new web.config

try {
    Invoke-WebRequest -UseBasicParsing -Uri "http://localhost" -ErrorAction SilentlyContinue | Out-Null;
}
catch { }

# Remove any filesystem links since they break the web deploy

Write-Host "Removing links";
Get-FilesystemLinks $AppRoot | ForEach-Object { $_.Delete() };


# Try to clear the asp.net temporary files
# This seems to keep some weird things from happening. For example, we had a
# problem with plugin Rest APIs dissapearing for no reason with no way of
# reproducing the issue - but this fixed it so ¯\_(ツ)_/¯

Write-Host "Clearing temp files";
try {
    Remove-ContentsAgressively "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\Temporary ASP.NET Files\root";
}
catch { }
