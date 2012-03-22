[T4Scaffolding.Scaffolder(Description = "Creates an entirely new scaffolder with a PS1 script and a T4 template")][CmdletBinding()]
param(        
    [parameter(Mandatory = $true, ValueFromPipelineByPropertyName = $true)][string]$CustomScaffolderName,
    [string]$Project,
	[string]$CodeLanguage,
	[string[]]$TemplateFolders,
	[switch]$Force = $false
)

$templateName = $CustomScaffolderName + "Template"

# PS1 script
$customScaffoldersPath = [T4Scaffolding.Core.ScaffoldingConstants]::CustomScaffoldersFolderPath
$outputPath = Join-Path (Join-Path $customScaffoldersPath $CustomScaffolderName) $CustomScaffolderName
Add-ProjectItemViaTemplate $outputPath -Template DefaultPs1Script -Model @{
	Scaffolder = $CustomScaffolderName; 
	TemplateName = $templateName;
} -SuccessMessage "Added scaffolder script '{0}'" -TemplateFolders $TemplateFolders -Project $Project -CodeLanguage "ps1" -Force:$Force

# T4 template
$outputPath = Join-Path (Join-Path $customScaffoldersPath $CustomScaffolderName) $templateName
Add-ProjectItemViaTemplate $outputPath -Template DefaultT4Template -Model @{
} -SuccessMessage "Added scaffolder template '{0}'" -TemplateFolders $TemplateFolders -Project $Project -CodeLanguage $CodeLanguage -Force:$Force