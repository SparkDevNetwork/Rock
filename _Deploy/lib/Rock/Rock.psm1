Import-Module "$PSScriptRoot/../Util";

Function Get-RockThemes([string] $RootPath) {
    $ThemesRoot = Join-Path $RootPath "Themes";
    $ThemesFolders = Get-ChildItem -Path $ThemesRoot -Directory;
    return $ThemesFolders | Get-RockTheme;
}

Function Get-RockTheme {

    [cmdletbinding(DefaultParameterSetName = 'Path')]
    param(
        [parameter( Mandatory, ParameterSetName = 'Path', Position = 0, ValueFromPipeline, ValueFromPipelineByPropertyName )]
        [ValidateNotNullOrEmpty()]
        [SupportsWildcards()]
        [string[]]
        $Path,

        [parameter( Mandatory, ParameterSetName = 'LiteralPath', Position = 0, ValueFromPipelineByPropertyName )]
        [ValidateNotNullOrEmpty()]
        [Alias('PSPath')]
        [string[]]
        $LiteralPath,

        [parameter( Mandatory, ParameterSetName = 'DirectoryInfo', Position = 0, ValueFromPipeline, ValueFromPipelineByPropertyName )]
        [ValidateNotNullOrEmpty()]
        [System.IO.DirectoryInfo]
        $DirectoryInfo
    )

    process {

        # Resolve path(s)
        if ($PSCmdlet.ParameterSetName -eq 'Path') {
            $Directories = Get-Item -Path $Path;
        }
        elseif ($PSCmdlet.ParameterSetName -eq 'LiteralPath') {
            $Directories = Get-Item -LiteralPath $LiteralPath;
        }
        elseif ($PSCmdlet.ParameterSetName -eq 'DirectoryInfo') {
            $Directories = $DirectoryInfo;
        }

        foreach ($Directory in $Directories) {

            [pscustomobject]@{
                DirectoryInfo = $Directory
                Name          = $Directory.Name
                RelativePath  = (Join-Path "Themes" $Directory.Name)
                AbsolutePath  = $Directory.FullName
                IsSystem      = Test-Path (Join-Path $Directory.FullName ".system")
                AllowsCompile = Test-Path (Join-Paths $Directory.FullName "Styles" ".nocompile")
            }

        }

    }

}

# The OnlineGiving plugin includes old versions of these which break our build
$InstallSkipFiles = @(
    "Microsoft.IdentityModel.Logging.dll"
    "Microsoft.IdentityModel.Tokens.dll"
    "System.IdentityModel.Tokens.Jwt.dll"
)

Function Install-RockPlugin() {

    [cmdletbinding(DefaultParameterSetName = 'Path')]
    param(
        [parameter( Mandatory, Position = 0 )]
        [ValidateNotNullOrEmpty()]
        [string]
        $RootPath,

        [parameter( Mandatory, ParameterSetName = 'Path', Position = 1, ValueFromPipeline, ValueFromPipelineByPropertyName )]
        [ValidateNotNullOrEmpty()]
        [SupportsWildcards()]
        [string[]]
        $Path,

        [parameter( Mandatory, ParameterSetName = 'LiteralPath', Position = 1, ValueFromPipelineByPropertyName )]
        [ValidateNotNullOrEmpty()]
        [Alias('PSPath')]
        [string[]]
        $LiteralPath
    )

    process {

        # Resolve path(s)
        if ($PSCmdlet.ParameterSetName -eq 'Path') {
            $PluginPaths = Resolve-Path -Path $Path | Convert-Path;
        }
        elseif ($PSCmdlet.ParameterSetName -eq 'LiteralPath') {
            $PluginPaths = Resolve-Path -LiteralPath $LiteralPath | Convert-Path;
        }

        foreach ( $PluginPath in $PluginPaths) {

            Write-Host "Installing Plugin from '$PluginPath'..."

            $TempDir = [System.IO.Path]::GetTempPath();

            $PackageHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $PluginPath).Hash;

            $ZipPath = Join-Path $TempDir "$PackageHash.zip";
            $ExtractPath = Join-Path $TempDir "$PackageHash";
            $ContentPath = Join-Path $ExtractPath "content";

            # We have to copy it to a .zip otherwise Expand-Archive complains
            Copy-Item -LiteralPath $PluginPath -Destination $ZipPath -Force | Out-Null;

            # Unzip the package
            Expand-Archive $ZipPath $ExtractPath -Force | Out-Null;

            if (Test-Path $ContentPath) {

                # Copy the content files to Rock
                Copy-ContentsRecursively $ContentPath $RootPath -Filter { -not ($InstallSkipFiles -contains $_.Name) } -Force;
            }
            else {
                if (Test-Path $ExtractPath) {
                    Write-Warning "Plugin '$PluginPath' has no content files to install.";
                }
                else {
                    Write-Error "Failed to extract '$PluginPath'";
                }
            }

            # Cleanup
            Remove-Item $ExtractPath -Recurse -Force | Out-Null;
            Remove-Item $ZipPath -Force | Out-Null;

        }
    }

}

Function Get-RockInstalledStorePackages([string] $RootPath) {
    $InstalledPackagesJsonPath = Join-Paths $RootPath "App_Data" "InstalledStorePackages.json";
    $InstalledPackagesJson = Get-Content -Raw $InstalledPackagesJsonPath;
    return ConvertFrom-Json $InstalledPackagesJson;
}

Function Get-RockCachedPluginFile {

    [cmdletbinding()]
    param(
        [parameter( Mandatory, Position = 0 )]
        [ValidateNotNullOrEmpty()]
        [string]
        $RootPath,

        [parameter( Mandatory, Position = 1, ValueFromPipeline )]
        [ValidateNotNullOrEmpty()]
        [pscustomobject]
        $Plugin
    )

    process {

        foreach ( $Item in $Plugin) {

            $PluginsRootPath = Join-Paths $RootPath "App_Data" "RockShop";

            $PackagePathName = [string]::Format("{0} - {1}", $Item.PackageId, $Item.PackageName);
            $VersionPathName = [string]::Format("{0} - {1}.plugin", $Item.VersionId, $Item.VersionLabel);

            $PluginPath = Join-Paths $PluginsRootPath $PackagePathName $VersionPathName;

            Get-Item $PluginPath;

        }

    }

}