
# Joins multiple paths together. Like Join-Path but with friends.
function Join-Paths {

    # Take the first path
    $resultPath, $remainingParts = $args;

    # Add each part
    $remainingParts | ForEach-Object { $resultPath = Join-Path -Path $resultPath -ChildPath $_ };

    # Return the result
    return $resultPath;

}

# Copies the contents of a directory to a new location
Function Copy-ContentsRecursively {

    Param(
        [parameter( Mandatory, Position = 0 )]
        [ValidateNotNullOrEmpty()]
        [string]
        $DirectoryToCopy,

        [parameter( Mandatory, Position = 1 )]
        [ValidateNotNullOrEmpty()]
        [string]
        $Destination,

        [scriptblock] $Filter,
        [switch] $Force
    )

    # Make sure the destination exists
    New-Item -ItemType Directory $Destination -Force | Out-Null;

    $Children = Get-ChildItem $DirectoryToCopy;

    if ($null -ne $Filter) { $Children = $Children | Where-Object $Filter };

    # For each item in the directory to copy
    foreach ($Child in $Children) {

        if ($Child.LinkType -eq "SymbolicLink") {

            # Re-create the symbolic link
            $LinkTarget, $OtherTargets = $Child.Target;

            if (Test-Path $LinkTarget) {
                New-Item -ItemType $Child.LinkType -Path $Destination -Name $Child.Name -Target $LinkTarget -Force | Out-Null;
            }
            else {
                Write-Warning "Skipping symbolic link '$($Child.FullName)' because it's target does not exist ($LinkTarget)."
            }

        }
        elseif ($Child.LinkType -eq "Junction") {

            # Re-create the junction
            $LinkTarget, $OtherTargets = $Child.Target;

            if (Test-Path $LinkTarget) {
                New-Item -ItemType $Child.LinkType -Path $Destination -Name $Child.Name -Target $LinkTarget -Force | Out-Null;
            }
            else {
                Write-Warning "Skipping junction '$($Child.FullName)' because it's target does not exist ($LinkTarget)."
            }

        }
        elseif ($Child.LinkType -eq "HardLink") {

            # Re-create the hard link
            New-Item -ItemType $Child.LinkType -Path $Destination -Name $Child.Name -Target $Child.FullName -Force | Out-Null;

        }
        elseif ($Child.PSIsContainer) {

            # Copy it's contents
            Copy-ContentsRecursively (Join-Path $DirectoryToCopy $Child.Name) (Join-Path $Destination $Child.Name) -Filter $Filter -Force:$Force;

        }
        else {

            # Copy the file
            Copy-Item (Join-Path $DirectoryToCopy $Child.Name) (Join-Path $Destination $Child.Name) -Force:$Force;

        }

    }
}

# Agressively clears a directory
function Remove-ContentsAgressively([string] $Path) {

    # Take ownership
    takeown.exe /F $Path /R /D y | Out-Null;

    # Grant full rights
    icacls.exe $Path /grant administrators:F /T | Out-Null;

    # Remove all of the contents
    Remove-Item -Recurse -Force "$Path/*" -ErrorAction SilentlyContinue | Out-Null;
    Start-Sleep -Seconds 2
    Remove-Item -Recurse -Force "$Path/*" -ErrorAction SilentlyContinue | Out-Null;

}


function Get-FilesystemLinks([string] $rootPath) {
    foreach ($child in Get-ChildItem $rootPath) {
        if ($Child.LinkType) {
            $child;
        }
        elseif ($Child.PSIsContainer) {
            Get-FilesystemLinks (Join-Path $rootPath $Child.Name);
        }
    }
}


function Get-TemplateFiles([string] $SearchDirectory) {
    Get-ChildItem $SearchDirectory -Recurse -Filter "*.template.*";
}


function Get-TemplateVariables([string] $TemplateContent) {

    # Get regex matches
    $Matches = ($TemplateContent | Select-String -AllMatches "\[\[(\w+)]]").Matches;

    # Get the matched strings
    $MatchValues = $Matches | ForEach-Object { $_.Groups[1].Value };

    # Return the unique results
    return $MatchValues | Sort-Object | Get-Unique;

}


Function Install-TemplatedFile {

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
        $LiteralPath
    )

    process {

        # Resolve path(s)
        if ($PSCmdlet.ParameterSetName -eq 'Path') {
            $TemplatePaths = Resolve-Path -Path $Path | Convert-Path;
        }
        elseif ($PSCmdlet.ParameterSetName -eq 'LiteralPath') {
            $TemplatePaths = Resolve-Path -LiteralPath $LiteralPath | Convert-Path;
        }

        foreach ( $TemplatePath in $TemplatePaths) {

            $Content = Get-Content -Raw -LiteralPath $TemplatePath;

            $Variables = Get-TemplateVariables $Content;

            foreach ($Variable in $Variables) {

                $EnvironmentVariableName = "DEPLOY_$Variable".ToUpper();
                $EnvironmentVariablePath = "env:$EnvironmentVariableName";

                # Check that it's in the environment
                if (Test-Path $EnvironmentVariablePath) {

                    # Get the value
                    $VariableValue = (Get-Item $EnvironmentVariablePath).Value;

                    # Update the content
                    $Content = $Content.Replace("[[$Variable]]", $VariableValue);

                }
                else {

                    # Oops
                    Write-Warning "Could not update '[[$Variable]]' in '$TemplatePath'. Environment variable '$EnvironmentVariableName' is not set.";

                }

            }

            $InstallPath = $TemplatePath.Replace(".template", "");

            Set-Content -LiteralPath $InstallPath -Value $Content;

        }

    }

}

Function Connect-RemoteFile {

    param(
        [parameter( Mandatory, Position = 0 )]
        [ValidateNotNullOrEmpty()]
        [string]
        $LocalRootPath,

        [parameter( Mandatory, Position = 2 )]
        [ValidateNotNullOrEmpty()]
        [string]
        $RemoteRootPath,

        [parameter( Mandatory, Position = 3 )]
        [ValidateNotNullOrEmpty()]
        [string]
        $ItemName
    )

    $LocalPath = Join-Path $LocalRootPath $ItemName;
    $RemotePath = Join-Path $RemoteRootPath $ItemName;

    # Make sure the remote root exists
    New-Item -ItemType Directory $RemoteRootPath -Force | Out-Null;

    # Make sure the remote path exists
    if( -not (Test-Path $RemotePath)) {

        if( Test-Path -PathType Container $LocalPath ) {

            # Copy the local folder to the remote location
            Copy-ContentsRecursively $LocalPath $RemotePath;

        }
        elseif (Test-Path -PathType Leaf $LocalPath) {

            # Copy the local file to the remote location
            Copy-Item $LocalPath $RemotePath -Force;

        }
        else {

            # Create a new file in the remote location
            New-Item -ItemType File -Path $RemotePath | Out-Null;

        }

    }

    # Check if the local path exists
    if(Test-Path $LocalPath) {

        # Remove all links inside the local path (Remove-Item might throw an error if it finds one with an invalid target)
        Get-FilesystemLinks $LocalPath | ForEach-Object { $_.Delete() };

        # Remove the local path
        Remove-Item -Path $LocalPath -Force -Recurse -ErrorAction SilentlyContinue | Out-Null;

    }

    # Link the local path to the remote path
    Write-Host "Linking new file:"
    Write-Host "    Path: '$LocalRootPath'"
    Write-Host "    Name: '$ItemName'"
    Write-Host "    Target: '$RemotePath'"
    If (Test-Path $LocalRootPath) {
        Write-Host "Local Path Root Exists"
    }
    If (Test-Path $LocalPath) {
        Write-Warning "Local Path Still Exists (it shouldn't)"
    }
    If (Test-Path $RemotePath) {
        Write-Host "Remote Path Exists"
    }
    New-Item -ItemType SymbolicLink -Path $LocalRootPath -Name $ItemName -Target $RemotePath | Out-Null;

}
