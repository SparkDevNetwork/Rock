param($installPath, $toolsPath, $package, $project)
   $project.Object.References | Where-Object { $_.Name -eq 'Microsoft.ScriptManager.MSAjax' } | ForEach-Object { $_.Remove() }  