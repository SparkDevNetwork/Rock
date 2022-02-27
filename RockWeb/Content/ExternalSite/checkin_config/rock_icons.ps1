#Create folders for config files if not previously created by software install
New-Item -ItemType Directory -Path "C:\Program Files (x86)\Spark Development Network\Rock RMS Check-in Client" -Force
New-Item -ItemType Directory -Path "C:\Program Files (x86)\Zebra Technologies\Zebra Setup Utilities\App" -Force

#Get config file for MS Windows dashboard icons
wget "https://rock.passion.team/Content/ExternalSite/checkin_config/CheckinClient.VisualElementsManifest.xml" -outfile "C:\Program Files (x86)\Spark Development Network\Rock RMS Check-in Client\CheckinClient.VisualElementsManifest.xml"
wget "https://rock.passion.team/Content/ExternalSite/checkin_config/PrnUtils.VisualElementsManifest.xml" -outfile "C:\Program Files (x86)\Zebra Technologies\Zebra Setup Utilities\App\PrnUtils.VisualElementsManifest.xml"

#Refresh icons
if (Test-Path "$env:ProgramData\Microsoft\Windows\Start Menu\Programs\Spark Development Network\RockRMS  Check-in Client\Rock RMS Check-in Client.lnk")
{
(ls "$env:ProgramData\Microsoft\Windows\Start Menu\Programs\Spark Development Network\RockRMS  Check-in Client\Rock RMS Check-in Client.lnk").lastwritetime = get-date
}
if (Test-Path "$env:ProgramData\Microsoft\Windows\Start Menu\Programs\Zebra Setup Utilities\Zebra Setup Utilities.lnk")
{
(ls "$env:ProgramData\Microsoft\Windows\Start Menu\Programs\Zebra Setup Utilities\Zebra Setup Utilities.lnk").lastwritetime = get-date
}
