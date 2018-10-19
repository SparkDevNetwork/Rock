# --------------------------------------------------
# ./deploy.ps1
# This script is run by AppVeyor's deploy agent after the deploy
# --------------------------------------------------


# Helper functions
function Coalesce([string[]] $Values) {
    return ($Values | Where-Object { $_ } | Select-Object -first 1);
}

# Make sure we have the WebAdministration commands
Import-Module WebAdministration;

# Try to figure out the app path
$webRoot = Coalesce $env:APPLICATION_PATH "$env:SystemDrive\inetpub\www";
if( -not (Test-Path $webRoot)) { throw "Could not reliably determine app root directory.";}

# Let's go!
Write-Output "Running post-deploy script";
Write-Output "--------------------------------------------------";
Write-Output "Web root folder: $rootfolder";
Write-Output "Running script as: $env:userdomain\$env:username";



Write-Output "Post-deploy script finished successfully";