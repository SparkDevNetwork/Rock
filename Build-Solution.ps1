#
# This is based on the NuGetGallery build scripts which I like and which use MigratorDotNet.
# This does not mean we'll use this forever, just for now...
#

param($connectionString = "")

$scriptPath = Split-Path $MyInvocation.MyCommand.Path

function Get-ConnectionString($configPath, $connectionStringName) 
{
  $config = [xml](get-content $configPath)
  
  $connectionString = ($config.configuration.connectionStrings.add | where { $_.name -eq $connectionStringName }).connectionString
  
  $connectionString = $connectionString.Replace("=", "%3D")
  $connectionString = $connectionString.Replace(";", "%3B")

  return $connectionString
}

if ($connectionString.Trim() -eq "") 
{
  $connectionString = Get-ConnectionString -configPath (join-path $scriptPath RockWeb\web.config) -connectionStringName RockContext
}

# Next line is to be created...
$projFile = join-path $scriptPath RockChMS.msbuild 
& "$(get-content env:windir)\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" $projFile /p:DbConnection=$connectionString /t:FullBuild