$pluginName = 'PastoralCare';

$scriptpath = $MyInvocation.MyCommand.Path
$dir = Split-Path $scriptpath
Push-Location $dir

# Clean Up Everything
$validPath = Test-Path ("bin\$pluginName");
if ($validPath -eq $True) {
    del -recurse  "bin\$pluginName"
}
$validPath = Test-Path ("bin\$pluginName.plugin")
if ($validPath -eq $True) {
    del "bin\$pluginName.plugin"
}

# Create the directory structure
mkdir -Path "bin\$pluginName\content\bin"
mkdir -Path "bin\$pluginName\install"
mkdir -Path "bin\$pluginName\uninstall"


# Copy the bin files
xcopy /Y /R "bin\Debug\org.secc.*.dll" "bin\$pluginName\content\bin"
#xcopy /Y /R "bin\Debug\org.secc.*.pdb" "bin\$pluginName\content\bin"

# Copy any blocks
$ValidBlocksPath = Test-Path ("org_secc")
if ($ValidBlocksPath -eq $True) {
    xcopy /Y /R /E /I "org_secc" "bin\$pluginName\content\Plugins\org_secc"
}

# Copy theme files
$ValidThemePath = Test-Path ("Themes")    
if ($ValidThemePath -eq $True) {
    xcopy /Y /R /E /I "Themes" "bin\$pluginName\content\Plugins\Themes"
}

# Output everything to the deletefile.txt
$currentDir = Get-Item "bin\$pluginName\content\"
$deleteFiles = "";
ForEach($file in (Get-ChildItem -Path "bin\$pluginName\content" –Recurse -File)) {
    $deleteFiles = $deleteFiles+$file.FullName.Replace($currentDir,"")+"`r`n";
}
$deleteFiles | Out-File "bin\$pluginName\uninstall\deletefile.txt"


# Zip it all up and name it appropriately
&"C:\Program Files\7-Zip\7z.exe" a "$dir\bin\$pluginName.zip" "$dir\bin\$pluginName\*"
mv "bin\$pluginName.zip" "bin\$pluginName.plugin"

Pop-Location