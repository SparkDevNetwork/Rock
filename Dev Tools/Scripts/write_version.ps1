param(
    [string] $RepoRoot,
    [string] $VersionFilePath
)

# Set relative paths to the files

Set-Location $RepoRoot

Write-Host "Executing 'write_version.ps1' from: $RepoRoot"
$currentVersionFile = "Rock.Version\AssemblySharedInfo.cs"

# Initialize lastBuiltVersion and currentVersion variables
$lastBuiltVersion = ""
$currentVersion = ""

# Check if the previous version file exists
if (Test-Path $VersionFilePath) {
    # Get the last built version from the file
    $lastBuiltVersion = Get-Content $VersionFilePath | Out-String
    $lastBuiltVersion = $lastBuiltVersion.Trim() # Remove any extra spaces or newlines
} else {
    $lastBuiltVersion = ""
}

# Display the version of the first file
Write-Host "Last Built Version: '$lastBuiltVersion'"

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

# Display the content (version) from the currentVersionFile
Write-Host "Current Version: '$currentVersion'"

# Write the last built version to the text file.
Set-Content $VersionFilePath "$currentVersion"
Write-Host "Version '$currentVersion' has been written to: '$VersionFilePath'."