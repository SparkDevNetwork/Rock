# This script is run by AppVeyor's deploy agent before the deploy
 
 # move content folder to temp
 Move-Item Content $env:temp