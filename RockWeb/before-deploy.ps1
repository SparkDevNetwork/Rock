# This script is run by AppVeyor's deploy agent before the deploy
Import-Module WebAdministration

# stop website - needed to allow the deploy to overwrite the sql server spatial types
Stop-WebSite 'RockRMS'
 
# move content folder to temp
Move-Item c:\webdata\rock.ccvonline.com\docs\Content c:\webdata\rock.ccvonline.com\temp