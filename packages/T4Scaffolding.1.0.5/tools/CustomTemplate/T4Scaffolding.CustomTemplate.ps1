[T4Scaffolding.Scaffolder(Description = "Allows you to modify the T4 template rendered by a scaffolder")][CmdletBinding()]
param(        
    [parameter(Mandatory = $true, ValueFromPipelineByPropertyName = $true)][string]$ScaffolderName,
	[parameter(Mandatory = $true, ValueFromPipelineByPropertyName = $true)][string]$TemplateName,
    [string]$Project,
    [string]$CodeLanguage,
	[switch]$Force = $false
)

# Ensure we can find a unique scaffolder
$resolvedScaffolder = Get-Scaffolder $ScaffolderName -Project $Project
if (!$resolvedScaffolder) {
	Write-Error "Could not find scaffolder '$ScaffolderName'"
	return
} elseif ($resolvedScaffolder -is [Array]) {
	Write-Error "More than one scaffolder matches the name '$ScaffolderName'"
	return
}

# Find its template
$scaffolderFolder = [System.IO.Path]::GetDirectoryName($resolvedScaffolder.Location)
$sourceTemplateFile = Find-ScaffolderTemplate $TemplateName -TemplateFolders $scaffolderFolder -Project $Project -CodeLanguage $CodeLanguage -ErrorIfNotFound
if (!$sourceTemplateFile) { return }

# Determine where the output will go in the project, and ensure we're not going to overwrite anything (except if Force is on)
$customScaffoldersPath = [T4Scaffolding.Core.ScaffoldingConstants]::CustomScaffoldersFolderPath
$outputPath = Join-Path (Join-Path $customScaffoldersPath $resolvedScaffolder.Name) ([System.IO.Path]::GetFileName($sourceTemplateFile))
$existingProjectItem = Get-ProjectItem $outputPath -Project $Project
if ($existingProjectItem -and -not $Force) {
	Write-Warning "$outputPath already exists! Pass -Force to overwrite. Skipping..."
	return
}

# Copy the source file to the output location in the project
if ($existingProjectItem) {
	$outputPath = $existingProjectItem.Properties.Item("FullPath").Value
	Set-IsCheckedOut $outputPath
	Copy-Item -Path $sourceTemplateFile -Destination $outputPath -Force
} else {
	$outputDir = [System.IO.Path]::GetDirectoryName($outputPath)
	$outputDirProjectItem = Get-ProjectFolder $outputDir -Create -Project $Project
	$outputDirProjectItem.AddFromFileCopy($sourceTemplateFile) | Out-Null
}

Write-Host "Added custom template '$outputPath'"