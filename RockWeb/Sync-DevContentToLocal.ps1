##
##  Script that will sync the content folder on the Development server
##     to the content folder on the local computer
##
##  TODO: Update the $localRockProjectFolder variable with the location of
##     the Rock project on your local computer
##

# Update this variable before running the first time
$LocalRockProjectFolder = 'D:\github\Rock'

# Development content folder location
$sourceContentFolder = '\\ccvdev\c$\inetpub\wwwroot\Content'

# Local content folder location
$destinationContentFolder = $LocalRockProjectFolder + '\RockWeb\Content'

# Command to sync the two locations
robocopy $sourceContentFolder $destinationContentFolder /MIR