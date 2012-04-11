[T4Scaffolding.Scaffolder(Description = "Makes an EF DbContext able to persist models of a given type, creating the DbContext first if necessary")][CmdletBinding()]
param(        
	[parameter(Mandatory = $true, ValueFromPipelineByPropertyName = $true)][string]$ModelType,
	[parameter(Mandatory = $true, ValueFromPipelineByPropertyName = $true)][string]$DbContextType,
	[string]$Area,
    [string]$Project,
	[string]$CodeLanguage,
	[string[]]$TemplateFolders
)

# Ensure we can find the model type
$foundModelType = Get-ProjectType $ModelType -Project $Project
if (!$foundModelType) { return }

# Find the DbContext class, or create it via a template if not already present
$foundDbContextType = Get-ProjectType $DbContextType -Project $Project -AllowMultiple
if (!$foundDbContextType) {
	# Determine where the DbContext class will go
	$defaultNamespace = (Get-Project $Project).Properties.Item("DefaultNamespace").Value
	if ($DbContextType.Contains(".")) {
		if ($DbContextType.StartsWith($defaultNamespace + ".", [System.StringComparison]::OrdinalIgnoreCase)) {
			$DbContextType = $DbContextType.Substring($defaultNamespace.Length + 1)
		}
		$outputPath = $DbContextType.Replace(".", [System.IO.Path]::DirectorySeparatorChar)
		$DbContextType = [System.IO.Path]::GetFileName($outputPath)
	} else {
		$outputPath = Join-Path Models $DbContextType
		if ($Area) {
			$areaFolder = Join-Path Areas $Area
			if (-not (Get-ProjectItem $areaFolder -Project $Project)) {
				Write-Error "Cannot find area '$Area'. Make sure it exists already."
				return
			}
			$outputPath = Join-Path $areaFolder $outputPath
		}
	}
	
	$dbContextNamespace = [T4Scaffolding.Namespaces]::Normalize($defaultNamespace + "." + [System.IO.Path]::GetDirectoryName($outputPath).Replace([System.IO.Path]::DirectorySeparatorChar, "."))
	Add-ProjectItemViaTemplate $outputPath -Template DbContext -Model @{
		DefaultNamespace = $defaultNamespace; 
		DbContextNamespace = $dbContextNamespace; 
		DbContextType = $DbContextType; 
	} -SuccessMessage "Added database context '{0}'" -TemplateFolders $TemplateFolders -Project $Project -CodeLanguage $CodeLanguage -Force:$Force

	$foundDbContextType = Get-ProjectType ($dbContextNamespace + "." + $DbContextType) -Project $Project
	if (!$foundDbContextType) { throw "Created database context $DbContextType, but could not find it as a project item" }
} elseif (($foundDbContextType | Measure-Object).Count -gt 1) {
	throw "Cannot find the database context class, because more than one type is called $DbContextType. Try specifying the fully-qualified type name, including namespace."
}

# Add a new property on the DbContext class
if ($foundDbContextType) {
	$propertyName = Get-PluralizedWord $foundModelType.Name

	# If this is not a DbContext, we can't add a new property, so ensure there is already one
	# Unfortunately we have to use the awkward "PowerShellInvoke" calling mechanism otherwise
	# the PowerShell COM wrapper objects can't be passed into the .NET code.
	$isAssignableToParamTypes = New-Object System.Type[] 2
	$isAssignableToParamTypes[0] = [EnvDTE.CodeType]
	$isAssignableToParamTypes[1] = [System.String]
	$isDbContext = [T4Scaffolding.Core.EnvDTE.EnvDTEExtensions]::PowerShellInvoke([T4Scaffolding.Core.EnvDTE.EnvDTEExtensions].GetMethod("IsAssignableTo", $isAssignableToParamTypes), $null, @($foundDbContextType, "System.Data.Entity.DbContext"))
	if (!$isDbContext) {
		$existingMembers = [T4Scaffolding.Core.EnvDTE.EnvDTEExtensions]::PowerShellInvoke([T4Scaffolding.Core.EnvDTE.EnvDTEExtensions].GetMethod("VisibleMembers"), $null, $foundDbContextType)
		$hasExistingPropertyForModel = $existingMembers | ?{ ($_.Name -eq $propertyName) -and ($_.Kind -eq [EnvDTE.vsCMElement]::vsCMElementProperty) }
		if ($hasExistingPropertyForModel) {
			Write-Warning "$($foundDbContextType.Name) already contains a '$propertyName' property."
		} else {
			throw "$($foundDbContextType.FullName) is not a System.Data.Entity.DbContext class and does not contain a '$propertyName' property, so it cannot be used as the database context."
		}
	} else {
		# This *is* a DbContext, so we can freely add a new property if there isn't already one for this model
		Add-ClassMemberViaTemplate -Name $propertyName -CodeClass $foundDbContextType -Template DbContextEntityMember -Model @{
			EntityType = $foundModelType;
			EntityTypeNamePluralized = $propertyName;
		} -SuccessMessage "Added '$propertyName' to database context '$($foundDbContextType.FullName)'" -TemplateFolders $TemplateFolders -Project $Project -CodeLanguage $CodeLanguage
	}
}

return @{
	DbContextType = $foundDbContextType
}