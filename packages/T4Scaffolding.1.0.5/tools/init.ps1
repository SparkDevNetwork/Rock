param($rootPath, $toolsPath, $package, $project)

# Don't support old NuGet versions as it's impractical to handle all their different sets of sematics
if ([NuGet.PackageManager].Assembly.GetName().Version -lt 1.4) 
{
	throw "T4Scaffolding requires NuGet (Package Manager Console) 1.4 or later"
} 

function GetLoadedT4ScaffoldingAssemblyVersion() {
	try {
		return [T4Scaffolding.ScaffolderAttribute].Assembly.GetName().Version
	} catch {
		return $null
	}
}
$t4ScaffoldingAssemblyVersion = GetLoadedT4ScaffoldingAssemblyVersion

if ($t4ScaffoldingAssemblyVersion -and ($t4ScaffoldingAssemblyVersion -ne "1.0.0.1"))
{
	# Prevent the use of scaffolding if you've got the wrong version of T4Scaffolding.dll loaded into your appdomain (we can't unload or update it until VS restarts)
	Write-Warning "---"
	Write-Warning "A different version of T4Scaffolding is already running in this instance of Visual Studio"
	Write-Warning "Please restart Visual Studio to avoid unexpected behavior."
	Write-Warning "You won't be able to use scaffolding until you restart Visual Studio."
	Write-Warning "---"	
	if (Get-Module T4Scaffolding) {
		# Disable scaffolding as much as possible until VS is restarted
		Remove-Module T4Scaffolding
	}
} 
else 
{
	# OK, we've got the correct .NET assembly version or none at all (in which case we can load the correct version)
	$dllPath = Join-Path $toolsPath T4Scaffolding.dll
	$tabExpansionPath = Join-Path $toolsPath "scaffoldingTabExpansion.psm1"
	$packagesRoot = [System.IO.Path]::GetDirectoryName($rootPath)

	if (Test-Path $dllPath) {
		# Enable shadow copying so the package can be uninstalled/upgraded later
		[System.AppDomain]::CurrentDomain.SetShadowCopyFiles()

		# Load the .NET PowerShell module and set up aliases, tab expansions, etc.
		Import-Module $dllPath
		[T4Scaffolding.NuGetServices.Services.ScaffoldingPackagePathResolver]::SetPackagesRootDirectory($packagesRoot)
		Set-Alias Scaffold Invoke-Scaffolder -Option AllScope -scope Global
		Update-FormatData -PrependPath (Join-Path $toolsPath T4Scaffolding.Format.ps1xml)
		Import-Module -Force -Global $tabExpansionPath

		# Ensure you've got some default settings for each of the included scaffolders
		Set-DefaultScaffolder -Name DbContext -Scaffolder T4Scaffolding.EFDbContext -SolutionWide -DoNotOverwriteExistingSetting
		Set-DefaultScaffolder -Name Repository -Scaffolder T4Scaffolding.EFRepository -SolutionWide -DoNotOverwriteExistingSetting
		Set-DefaultScaffolder -Name CustomTemplate -Scaffolder T4Scaffolding.CustomTemplate -SolutionWide -DoNotOverwriteExistingSetting
		Set-DefaultScaffolder -Name CustomScaffolder -Scaffolder T4Scaffolding.CustomScaffolder -SolutionWide -DoNotOverwriteExistingSetting
	} 
	else 
	{
		Write-Warning ("Could not find T4Scaffolding module. Looked for: " + $dllPath)
	}
}