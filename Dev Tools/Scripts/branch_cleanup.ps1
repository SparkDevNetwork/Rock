################################################################################
# This script should never modify editable files like C# code. Because it's run
# automatically on clean we never want to risk removing intentionally added 
# files - only files that we can reasonably assume are build artifacts.
################################################################################
# Remove directories from the project bin and obj directories.
Write-Host "Removing files from project bin directories"

# Read the list of projects from the solution.
$projectsPattern = '^Project\(\".* = \"(?<projectName>[^\"]+)\", \"[^\\]+(?<projectFile>[^\"]+).csproj\"'
$projectFiles = Get-Content "Rock.sln" | Where-Object { 
    $_ -match $projectsPattern
}

foreach($projectFile in $projectFiles) {
    $projectFileMatch = $projectFile -match $projectsPattern
    $projectPath = $Matches.projectFile

    # Remove the bin and obj directories for the project.
    Write-Host "Removing directories '$projectPath\Bin' and '$projectPath\Obj'."
    Remove-Item -Recurse -Force -ErrorAction SilentlyContinue `
        -Path ".\$projectPath\Bin", ".\$projectPath\Obj"
}

# Remove the packages directories for some projects.
Write-Host "Removing directories '\Applications\Wpf\packages' and '\Installers\StatementGeneratorInstall\packages'."
Remove-Item -Recurse -Force -ErrorAction SilentlyContinue `
    -Path ".\Applications\Wpf\packages", ".\Installers\StatementGeneratorInstall\packages"

# Change extensions for dll's that should not be removed.
Rename-Item -Path "RockWeb\Bin\Microsoft.CodeDom.Providers.DotNetCompilerPlatform.dll" -NewName "Microsoft.CodeDom.Providers.DotNetCompilerPlatform.bak" -ErrorAction SilentlyContinue
Rename-Item -Path "RockWeb\Bin\Rock.Common.Mobile.dll" -NewName "Rock.Common.Mobile.bak" -ErrorAction SilentlyContinue
Rename-Item -Path "RockWeb\Bin\grpc_csharp_ext.x64.dll" -NewName "grpc_csharp_ext.x64.bak" -ErrorAction SilentlyContinue
Rename-Item -Path "RockWeb\Bin\grpc_csharp_ext.x86.dll" -NewName "grpc_csharp_ext.x86.bak" -ErrorAction SilentlyContinue

Write-Host "Removing 'RockWeb\Bin' directory .dll files that should be created by the build"
Remove-Item -Path "RockWeb\Bin\*.dll", "RockWeb\Bin\*.pdb", "RockWeb\Bin\*.xml" -Force -ErrorAction SilentlyContinue

# Change the extensions back to .dll for those files that were earlier renamed.
Rename-Item -Path "RockWeb\Bin\Microsoft.CodeDom.Providers.DotNetCompilerPlatform.bak" -NewName "Microsoft.CodeDom.Providers.DotNetCompilerPlatform.dll" -ErrorAction SilentlyContinue
Rename-Item -Path "RockWeb\Bin\Rock.Common.Mobile.bak" -NewName "Rock.Common.Mobile.dll" -ErrorAction SilentlyContinue
Rename-Item -Path "RockWeb\Bin\grpc_csharp_ext.x64.bak" -NewName "grpc_csharp_ext.x64.dll" -ErrorAction SilentlyContinue
Rename-Item -Path "RockWeb\Bin\grpc_csharp_ext.x86.bak" -NewName "grpc_csharp_ext.x86.dll" -ErrorAction SilentlyContinue

# Remove the RockWeb\Obsidian directory which should be created by the build.
Write-Host "Removing the 'RockWeb\Obsidian' directory."
Remove-Item -Path ".\RockWeb\Obsidian" -Recurse -Force -ErrorAction SilentlyContinue

# Loop through all directories in packages - removing all files.
if ( Get-Item -Path "packages" ) {
    Write-Host "Removing Nuget Packages"
    Get-ChildItem -Path "packages\*" -Directory | ForEach-Object { Remove-Item -Path $_.FullName -Recurse -Force }
}

# Run npm ci for each Obsidian project.
Write-Host "Running 'npm ci' for the 'Rock.JavaScript.Obsidian' project"
Set-Location -Path ".\Rock.JavaScript.Obsidian"
npm ci
Write-Host "Running 'npm ci' for the 'Rock.JavaScript.Obsidian.Blocks' project"
Set-Location -Path "..\Rock.JavaScript.Obsidian.Blocks"
npm ci

# Set the location back to it's original value.
Set-Location ".."

Write-Host "Completed Branch Cleanup"