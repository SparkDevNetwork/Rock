# Mostly copied from NuGet's native tab expansion function
# Maybe a future version of NuGet will have some kind of pluggable tab expansion system
# so that 3rd-party packages can provide expansions without duplicating all this logic

if (!$global:scaffolderTabExpansion) { $global:scaffolderTabExpansion = @{ } }

# make sure we stop on exceptions
$ErrorActionPreference = "Stop"

# This object reprents the result value for tab expansion functions when no result is returned.
# This is so that we can distinguish it from $null, which has different semantics
$NoResultValue = New-Object 'System.Object'

# Backup the original tab expansion function
if ((Test-Path Function:\TabExpansionPreT4Scaffolding) -eq $false) {
    Rename-Item Function:\TabExpansion global:TabExpansionPreT4Scaffolding
}

function TabExpansion($line, $lastWord) {
    $filter = $lastWord.Trim()
    
    if ($filter.StartsWith('-')) {
       # if this is a parameter, let default PS tab expansion supply the list of parameters
       return (TabExpansionPreT4Scaffolding $line $lastWord)
    }
    
    # remove double quotes around last word
    $trimmedFilter = $filter.Trim( '"', "'" )
    if ($trimmedFilter.length -lt $filter.length) {
        $filter = $trimmedFilter
        $addQuote = $true
    }
    
    $e = $null
	$tokens = @([System.Management.Automation.PSParser]::Tokenize($line, [ref]$e)) | %{ $_.Content }
	if (-not($tokens -is [Array])) {
		$tokens = ,$tokens
	}
    if (!$filter) {
        $tokens = $tokens + $filter
    }	

    switch ($tokens[0]) {
        'Invoke-Scaffolder' {
			$choices = TabExpansionForInvokeScaffolder $filter $tokens
        }
        'Scaffold' {
			$choices = TabExpansionForInvokeScaffolder $filter $tokens
        }		    
		'Set-DefaultScaffolder' {
			$choices = TabExpansionForSetDefaultScaffolder $filter $tokens
		}    
        default {
            $choices = $NoResultValue
        }
    }
    
	if ($choices -eq $NoResultValue) {	
        # Fallback to the default tab expansion
        TabExpansionPreT4Scaffolding $line $lastWord 
	}
	elseif ($choices) {	
        # Return all the choices, do some filtering based on the last word, sort them and wrap each suggestion in a double quote if necessary
        $choices | 
            Where-Object { $_.StartsWith($filter, "OrdinalIgnoreCase") } | 
            Sort-Object |
            ForEach-Object { if ($addQuote -or $_.IndexOf(' ') -gt -1) { "'" + ($_ -replace "'", "''") + "'"} else { $_ } }
    }
    else {
        # return null here will tell the console not to show system file paths
        return $null
    }
}

function TabExpansionForInvokeScaffolder([string]$filter, $allTokens) {
	$secondLastToken = $allTokens[-2]	
    if (($secondLastToken -eq '-scaffolder') -or ($allTokens.length -eq 2) -or (($allTokens.length -eq 4) -and ($allTokens[-3] -eq '-project'))) {
		$project = FindNamedParameterValue $allTokens Project
		$filter += "*"
        return @() + (Get-DefaultScaffolder -Project $project | %{ $_.DefaultName }) + (Get-Scaffolder $filter -Project $project | %{ $_.Name })
    } elseif ($secondLastToken -eq '-project') {
		return (Get-Project -All) | %{ $_.Name }
    } else {		
        # We don't know how to complete custom scaffolder params here, so ask the scaffolder itself to do so
		$scaffolder = Get-Scaffolder $tokens[1]
		if ($scaffolder -is [T4Scaffolding.Core.ScaffolderLocators.ScaffolderInfo]) {
			$registeredScriptBlock = $global:scaffolderTabExpansion[$scaffolder.Name]
			if ($registeredScriptBlock -is [ScriptBlock]) {
				return . $registeredScriptBlock $filter $tokens
			}			
		}
		return $NoResultValue
    }
}

function TabExpansionForSetDefaultScaffolder([string]$filter, $allTokens) {
	$secondLastToken = $allTokens[-2]
	if (($secondLastToken -eq '-name') -or ($allTokens.length -eq 2)) {
		return (Get-DefaultScaffolder | %{ $_.DefaultName })
	} elseif (($secondLastToken -eq '-scaffolder') -or ($allTokens.length -eq 3)) {
		$project = FindNamedParameterValue $allTokens Project
		$filter += "*"
		return (Get-Scaffolder $filter -Project $project | %{ $_.Name })
	}
	return $NoResultValue
}

function FindNamedParameterValue($allTokens, $parameterName) {
	$foundAtPreviousToken = $false
	foreach($token in $allTokens) {
		if ($foundAtPreviousToken) { return $token }
		if ($token -eq ("-" + $parameterName)) { $foundAtPreviousToken = $true }
	}
}