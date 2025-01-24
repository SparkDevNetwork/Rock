param(
    [string] $RepoRoot,
    [string] $VersionFilePath
)

# Set relative paths to the files

Set-Location $RepoRoot

Write-Host "Executing 'version_check.ps1' from: $RepoRoot"
$previousVersionFile = "last_built_version.txt"
$currentVersionFile = "Rock.Version\AssemblySharedInfo.cs"
$branchCleanupScriptPath = "Dev Tools\Scripts\branch_cleanup.ps1"

# Initialize lastBuiltVersion and currentVersion variables
$lastBuiltVersion = ""
$currentVersion = ""

# Check if the previous version file exists
if (Test-Path $previousVersionFile) {
    # Get the last built version from the file
    $lastBuiltVersion = Get-Content $previousVersionFile | Out-String
    $lastBuiltVersion = $lastBuiltVersion.Trim() # Remove any extra spaces or newlines
} else {
    Write-Host "No previous built version found."
    exit
}

# Read the content of the second file to find the version string
Get-Content $currentVersionFile | ForEach-Object {
    $line = $_
    
    # Check if the line contains the assembly informational version pattern
    if ($line -match '\[assembly: AssemblyInformationalVersion\( "Rock McKinley \d+\.\d+" \)\]') {
        # Extract the version number from the line using regular expression
        if ($line -match '\[assembly: AssemblyInformationalVersion\( "(.*?)" \)\]') {
            $currentVersion = $matches[1]
        }
        return # Exit the loop after finding the version
    }
}

# Compare the version of the previous version file to the content of currentVersionFile.
if ($lastBuiltVersion -eq $currentVersion) {
    Write-Host "Current and previous build versions match: '$currentVersion'"
    exit
} 

# Run the branch cleanup script if the versions don't match
Write-Host "Previously built Rock Version differs from current:"
Write-Host "Previous: '$lastBuiltVersion'"
Write-Host "Current:  '$currentVersion'"
Write-Host "Running Cleanup script: '$branchCleanupScriptPath'"
& ".\$branchCleanupScriptPath"