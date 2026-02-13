$scriptFile = [System.IO.Path]::GetFileName($MyInvocation.MyCommand.Definition)
$scriptName = [System.IO.Path]::GetFileNameWithoutExtension($scriptFile)
$solutionPath = $PSScriptRoot | Split-Path | Split-Path | Split-Path

function Show-Help {
    Write-Host "Usage: $scriptFile [options]"
    Write-Host ""
    Write-Host "Options:" -ForegroundColor Green
    Write-Host "  -a" -ForegroundColor Green -NoNewline
    Write-Host ", --all" -ForegroundColor DarkGray -NoNewline
    Write-Host "            Watch and build all default projects (Blocks, Enums, ViewModels)" -ForegroundColor White
    Write-Host "  -b" -ForegroundColor Green -NoNewline
    Write-Host ", --blocks" -ForegroundColor DarkGray -NoNewline
    Write-Host "         Watch and build Rock.Blocks" -ForegroundColor White
    Write-Host "  -e" -ForegroundColor Green -NoNewline
    Write-Host ", --enums" -ForegroundColor DarkGray -NoNewline
    Write-Host "          Watch and build Rock.Enums" -ForegroundColor White
    Write-Host "  -v" -ForegroundColor Green -NoNewline
    Write-Host ", --view-models" -ForegroundColor DarkGray -NoNewline
    Write-Host "    Watch and build Rock.ViewModels" -ForegroundColor White
    Write-Host "  -n" -ForegroundColor Green -NoNewline
    Write-Host ", --none" -ForegroundColor DarkGray -NoNewline
    Write-Host "           Do not watch any default projects" -ForegroundColor White
    Write-Host "  -p" -ForegroundColor Green -NoNewline
    Write-Host ", --custom-path" -ForegroundColor DarkGray -NoNewline
    Write-Host "    Custom projects to watch/build." -ForegroundColor White
    Write-Host "                       Format: -p `"ProjectFilePath1;ProjectFolderToWatch1|ProjectFilePath2;ProjectFolderToWatch2|...`""
    Write-Host "                       where ProjectFilePath is the .csproj path and ProjectFolderToWatch is the root folder to monitor for changes."
    Write-Host "  -s" -ForegroundColor Green -NoNewline
    Write-Host ", --solution-path" -ForegroundColor DarkGray -NoNewline
    Write-Host "  Override the default solution path (currently: $solutionPath)" -ForegroundColor White
    Write-Host "  -f" -ForegroundColor Green -NoNewline
    Write-Host ", --filter" -ForegroundColor DarkGray -NoNewline
    Write-Host "         Override the file filter for the watcher (default: *.cs). Accepts e.g. '*.cs', '*.cs;*.json'" -ForegroundColor White
    Write-Host "      --do-not-killall" -ForegroundColor DarkGray -NoNewline
    Write-Host " Prevents killing all other running $scriptFile PowerShell processes." -ForegroundColor White
    Write-Host "      --prerelease" -ForegroundColor DarkGray -NoNewline
    Write-Host "     Includes pre-release versions, like Insiders, of Visual Studio when looking for the msbuild tool." -ForegroundColor White
    Write-Host "  -h" -ForegroundColor Green -NoNewline
    Write-Host ", --help" -ForegroundColor DarkGray -NoNewline
    Write-Host "           Show this help message" -ForegroundColor White
    Write-Host ""
    Write-Host "[EXAMPLE - Default usage]" -ForegroundColor Green
    Write-Host "  $scriptFile -a" -ForegroundColor Yellow
    Write-Host "    Would kill other running instances of this script."
    Write-Host "    Would watch for .cs file changes in $solutionPath for Rock.Blocks project folder."
    Write-Host "    Would watch for .cs file changes in $solutionPath for Rock.Enums project folder."
    Write-Host "    Would watch for .cs file changes in $solutionPath for Rock.ViewModels project folder."
    Write-Host ""
    Write-Host "[Example - Change monitored file extensions]" -ForegroundColor Green
    Write-Host "  $scriptFile -a -f '*.cs;*.json'" -ForegroundColor Yellow
    Write-Host "    Would kill other running instances of this script."
    Write-Host "    Would watch for .cs and .json file changes in $solutionPath for Rock.Blocks project folder."
    Write-Host "    Would watch for .cs and .json file changes in $solutionPath for Rock.Enums project folder."
    Write-Host "    Would watch for .cs and .json file changes in $solutionPath for Rock.ViewModels project folder."
    Write-Host "[Example - Watch no default projects and specify a custom project]" -ForegroundColor Green
    Write-Host "  $scriptFile -n -p `"Rock.Rest\\Rock.Rest.csproj;Rock.Rest`"" -ForegroundColor Yellow
    Write-Host "    Would kill other running instances of this script."
    Write-Host "    Would watch for .cs file changes in $solutionPath for Rock.Rest project folder."
    Write-Host "[Example - Watch a custom solution path's Rock.Blocks and a custom project]" -ForegroundColor Green
    Write-Host "  $scriptFile -b -p `"Rock.Rest\\Rock.Rest.csproj;Rock.Rest`" -s `"C:\\dev\\Rock-v18`"" -ForegroundColor Yellow
    Write-Host "    Would kill other running instances of this script."
    Write-Host "    Would watch for .cs file changes in C:\dev\Rock-v18\ for Rock.Blocks project folder."
    Write-Host "    Would watch for .cs file changes in C:\dev\Rock-v18\ for Rock.Rest project folder."
    Write-Host "[Example - Watch Rock.ViewModels and multiple custom projects with a different solution path and file extensions]" -ForegroundColor Green
    Write-Host "  $scriptFile -v" -ForegroundColor Yellow
    Write-Host "  -p `"Rock.Rest\\Rock.Rest.csproj;Rock.Rest|Rock.Tests\\Rock.Tests.csproj;Rock.Tests`""
    Write-Host "  -s `"C:\\Github\\Rock-v17`""
    Write-Host "  -f '*.txt'"
    Write-Host "    Would watch for .txt file changes in C:\Github\Rock-v17\Rock.Blocks and build the Rock.Blocks project."
    Write-Host "    Would watch for .txt file changes in C:\Github\Rock-v17\Rock.Rest and build the Rock.Rest project."
    Write-Host "    Would watch for .txt file changes in C:\Github\Rock-v17\Rock.Tests and build the Rock.Tests project."
}

# Default values
$watchBlocks = $false
$watchEnums = $false
$watchViewModels = $false
$customProjects = @()
$showHelp = $false
$serviceableArgsUsed = $false
$watcherFilter = "*.cs"
$killAll = $true
$prerelease = $false

# Parse arguments
for ($i = 0; $i -lt $args.Count; $i++) {
    switch ($args[$i]) {
        '-a' { $watchBlocks = $true; $watchEnums = $true; $watchViewModels = $true; $serviceableArgsUsed = $true }
        '--all' { $watchBlocks = $true; $watchEnums = $true; $watchViewModels = $true; $serviceableArgsUsed = $true }
        '-b' { $watchBlocks = $true; $serviceableArgsUsed = $true }
        '--blocks' { $watchBlocks = $true; $serviceableArgsUsed = $true }
        '-e' { $watchEnums = $true; $serviceableArgsUsed = $true }
        '--enums' { $watchEnums = $true; $serviceableArgsUsed = $true }
        '-v' { $watchViewModels = $true; $serviceableArgsUsed = $true }
        '--view-models' { $watchViewModels = $true; $serviceableArgsUsed = $true }
        '-n' { $watchBlocks = $false; $watchEnums = $false; $watchViewModels = $false; $serviceableArgsUsed = $true }
        '--none' { $watchBlocks = $false; $watchEnums = $false; $watchViewModels = $false; $serviceableArgsUsed = $true }
        '-p' {
            $serviceableArgsUsed = $true
            if ($i + 1 -lt $args.Count) {
                $customArg = $args[$i + 1]
                $i++
                $customProjects = $customArg -split '\|'
            }
        }
        '--custom-path' {
            $serviceableArgsUsed = $true
            if ($i + 1 -lt $args.Count) {
                $customArg = $args[$i + 1]
                $i++
                $customProjects = $customArg -split '\|'
            }
        }
        '-s' {
            if ($i + 1 -lt $args.Count) {
                $solutionPath = $args[$i + 1]
                $i++
            }
        }
        '--solution-path' {
            if ($i + 1 -lt $args.Count) {
                $solutionPath = $args[$i + 1]
                $i++
            }
        }
        '-f' {
            if ($i + 1 -lt $args.Count) {
                $watcherFilter = $args[$i + 1]
                $i++
            }
        }
        '--filter' {
            if ($i + 1 -lt $args.Count) {
                $watcherFilter = $args[$i + 1]
                $i++
            }
        }
        '--do-not-kill-all' { $killAll = $false }
        '--prerelease' { $prerelease = $true }
        '-h' { $showHelp = $true }
        '--help' { $showHelp = $true }
        default { }
    }
}

# Kill other running instances of this script to prevent conflicts
if ($killAll -and -not $showHelp) {
    Write-Host "Killing all other running $scriptName PowerShell processes to prevent conflicts..."
    $currentPid = $PID
    $procs = Get-CimInstance Win32_Process | Where-Object {
        $_.Name -match 'powershell' -and $_.ProcessId -ne $currentPid -and $_.CommandLine -match "$scriptFile"
    }
    foreach ($proc in $procs) {
        try {
            Stop-Process -Id $proc.ProcessId -Force -ErrorAction Stop
            Write-Host "Killed process $($proc.ProcessId) : $($proc.CommandLine)"
        } catch {
            Write-Host "Failed to kill process $($proc.ProcessId): $_" -ForegroundColor Red
        }
    }
    Write-Host "Done killing old $scriptName processes."
}

# Show help if no args or -h/--help
if (-not $serviceableArgsUsed -or $showHelp) {
    Show-Help
    exit 0
}

# Build up projects array
$projects = @()
if ($watchBlocks) {
    $projects += @{ Path = "$solutionPath\Rock.Blocks\Rock.Blocks.csproj"; Watch = "$solutionPath\Rock.Blocks" }
}
if ($watchEnums) {
    $projects += @{ Path = "$solutionPath\Rock.Enums\Rock.Enums.csproj"; Watch = "$solutionPath\Rock.Enums" }
}
if ($watchViewModels) {
    $projects += @{ Path = "$solutionPath\Rock.ViewModels\Rock.ViewModels.csproj"; Watch = "$solutionPath\Rock.ViewModels" }
}
foreach ($custom in $customProjects) {
    $parts = $custom -split ';'
    if ($parts.Count -eq 2) {
        $projects += @{ Path = $parts[0]; Watch = $parts[1] }
    }
}

if ($projects.Count -eq 0) {
    Write-Host "No projects selected to watch/build. Exiting."
    exit 1
}

# Debounce and build state per project
$debounceSeconds = 3
$heartbeatSeconds = 60
$lastHeartbeat = Get-Date

# Track build state for each project
$projectStates = @{}
foreach ($proj in $projects) {
    $projectStates[$proj.Path] = @{ LastBuildTime = (Get-Date "2000-01-01"); PendingBuild = $false }
}

Write-Host "Watching for $watcherFilter changes in project folders. Building on change."

# Create a watcher for each unique watch path and filter
$watchers = @{}
foreach ($proj in $projects) {
    $watchPath = $proj.Watch
    $filters = $watcherFilter -split ';'
    foreach ($filter in $filters) {
        $key = "$watchPath|$filter"
        if (-not $watchers.ContainsKey($key)) {
            $watcher = New-Object System.IO.FileSystemWatcher
            $watcher.Path = $watchPath
            $watcher.Filter = $filter.Trim()
            $watcher.IncludeSubdirectories = $true
            $watcher.EnableRaisingEvents = $true
            $watchers[$key] = $watcher

            Register-ObjectEvent $watcher Changed -Action {
                foreach ($p in $projects) {
                    if ($p.Watch -eq $Event.Sender.Path) {
                        $global:projectStates[$p.Path].PendingBuild = $true
                    }
                }
            }
            Register-ObjectEvent $watcher Created -Action {
                foreach ($p in $projects) {
                    if ($p.Watch -eq $Event.Sender.Path) {
                        $global:projectStates[$p.Path].PendingBuild = $true
                    }
                }
            }
            Register-ObjectEvent $watcher Renamed -Action {
                foreach ($p in $projects) {
                    if ($p.Watch -eq $Event.Sender.Path) {
                        $global:projectStates[$p.Path].PendingBuild = $true
                    }
                }
            }
        }
    }
}

Write-Host "Starting $scriptName watch loop..."
Write-Host "To stop the watch, you can do any of the following:"
Write-Host "  Tools > (Stop) WhatYouNamedThisExternalTool"
Write-Host "  Run `"$scriptFile`" without arguments"
Write-Host "  End the Powershell process with Id $($PID)"
Write-Host "  Close Visual Studio."

while ($true) {
    # Heartbeat
    if ((Get-Date) - $lastHeartbeat -gt (New-TimeSpan -Seconds $heartbeatSeconds)) {
        Write-Host ("[{0}] $scriptName Heartbeat: Watching for changes..." -f (Get-Date -Format "HH:mm:ss"))
        $lastHeartbeat = Get-Date
    }

    foreach ($proj in $projects) {
        $state = $projectStates[$proj.Path]

        # Check if a build is pending and debounce time has passed
        if ($state.PendingBuild -and ((Get-Date) - $state.LastBuildTime -gt (New-TimeSpan -Seconds $debounceSeconds))) {
            Write-Host ("[{0}] Change detected. Building $($proj.Path)..." -f (Get-Date -Format "HH:mm:ss"))
            $state.PendingBuild = $false
            $state.LastBuildTime = Get-Date

            # Find msbuild.exe using vswhere if available
            $msbuild = $null
            $vswherePath = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
            if (Test-Path $vswherePath) {
                $vswhereArgs = @('-latest', '-products', '*', '-requires', 'Microsoft.Component.MSBuild', '-find', 'MSBuild\**\Bin\MSBuild.exe')

                if ($prerelease) {
                    $vswhereArgs = @('-prerelease') + $vswhereArgs
                }

                $msbuildPath = & $vswherePath @vswhereArgs | Select-Object -First 1

                if ($msbuildPath -and (Test-Path $msbuildPath)) {
                    $msbuild = $msbuildPath
                }
            }

            if (-not $msbuild) {
                $msbuild = "msbuild.exe" # fallback and pray they have it in their PATH
            }

            $buildArgs = "`"$($proj.Path)`" /m /t:Build /p:Configuration=Debug"
            $logFile = "Dev Tools\Scripts\obsidian-backend-watch\logs\build-$($proj.Path | Split-Path -Leaf).log"
            $errFile = $logFile + ".err"
            try {
                $proc = Start-Process -FilePath $msbuild -ArgumentList $buildArgs -PassThru -RedirectStandardOutput $logFile -RedirectStandardError $errFile -WindowStyle Hidden
                # Poll for process exit because -Wait sometimes doesn't work as expected
                $dots = @(".", "..", "...")
                $dotIndex = 0
                while ($true) {
                    Start-Sleep -Seconds 2
                    try {
                        $p = Get-Process -Id $proc.Id -ErrorAction Stop
                        Write-Host -NoNewline "`rBuilding$($dots[$dotIndex])"
                        $dotIndex = ($dotIndex + 1) % $dots.Length
                    } catch {
                        Write-Host "" # New line after build completes
                        break # Process has exited
                    }
                }
            } catch {
                if ($_.Exception.Message -like '*The system cannot find the file specified*') {
                    Write-Host "ERROR: msbuild.exe was not found." -ForegroundColor Red
                    Write-Host "If you are using a prerelease or Insiders version of Visual Studio," -ForegroundColor Red
                    Write-Host "try running this script with --prerelease." -ForegroundColor Red
                    Write-Host "Exiting..." -ForegroundColor Red
                } else {
                    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
                }
                exit(1)
            }
            if (Test-Path $errFile) {
                Get-Content $errFile | Add-Content $logFile
                Remove-Item $errFile -ErrorAction SilentlyContinue
            }
            if (Test-Path $logFile) {
                Get-Content $logFile -Tail 20
            } else {
                Write-Host "Build log file not found: $logFile" -ForegroundColor Yellow
            }

            # msbuild uses null exit code for success, but let's also accept 0
            if ($proc.ExitCode -eq 0 -or $proc.ExitCode -eq $null) {
                Write-Host ("[{0}] Build succeeded for $($proj.Path)." -f (Get-Date -Format "HH:mm:ss")) -ForegroundColor Green
            } else {
                Write-Host ("[{0}] Build failed for $($proj.Path)! See $logFile for details." -f (Get-Date -Format "HH:mm:ss")) -ForegroundColor Red
            }
        }
    }

    Start-Sleep -Milliseconds 500
}