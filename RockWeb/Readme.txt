+ Add ability to specify the port when using the Mandrill provider and update SMTP and Mandrill providers to share common code.
+ Fix the Site Map block so that the expanded tree view of pages/blocks is persisted through postback (Fixes #468).
+ Add functionality for specifying specific HTTP Server Variable values to evaluate before sending exception notifications and prevent the notification being sent (helpful for ignore exceptions caused by web crawlers using outdated links)
+ Add additional blocks for managing workflow and create sample workflows.
+ Add better error handling and reporting when an error occurs during application start-up
+ Added SignalR to the project per team discussion
+ Add ability to add images to a named location
+ Update campus detail block so that campus location is linked to named location with same name.
+ Merge branch 'origin/feature-dt-plugin'

Conflicts:
	Applications/Wpf/CheckinClient/bin/Debug/Newtonsoft.Json.xml
	Applications/Wpf/CheckinClient/packages/Newtonsoft.Json.6.0.3/lib/net20/Newtonsoft.Json.xml
	Applications/Wpf/CheckinClient/packages/Newtonsoft.Json.6.0.3/lib/net35/Newtonsoft.Json.xml
	Applications/Wpf/CheckinClient/packages/Newtonsoft.Json.6.0.3/lib/net40/Newtonsoft.Json.xml
	Applications/Wpf/CheckinClient/packages/Newtonsoft.Json.6.0.3/lib/portable-net40+sl4+wp7+win8/Newtonsoft.Json.xml
	Applications/Wpf/CheckinClient/packages/Newtonsoft.Json.6.0.3/lib/portable-net45+wp80+win8+wpa81/Newtonsoft.Json.xml
	Applications/Wpf/CheckinClient/packages/Newtonsoft.Json.6.0.3/tools/install.ps1
	Rock.Rest/Rock.Rest.csproj
	Rock.Rest/packages.config
	Rock/SystemGuid/DefinedType.cs
	RockInstaller/packages/Newtonsoft.Json.5.0.6/lib/net20/Newtonsoft.Json.xml
	RockInstaller/packages/Newtonsoft.Json.5.0.6/lib/portable-net40+sl4+wp7+win8/Newtonsoft.Json.xml
	RockWeb/packages.config
	packages/Newtonsoft.Json.5.0.6/lib/net35/Newtonsoft.Json.xml
	packages/Newtonsoft.Json.6.0.2/lib/net20/Newtonsoft.Json.xml
	packages/Newtonsoft.Json.6.0.2/lib/net35/Newtonsoft.Json.xml
	packages/Newtonsoft.Json.6.0.2/lib/net40/Newtonsoft.Json.xml
	packages/Newtonsoft.Json.6.0.2/lib/net45/Newtonsoft.Json.xml
	packages/Newtonsoft.Json.6.0.2/lib/netcore45/Newtonsoft.Json.xml
	packages/Newtonsoft.Json.6.0.2/lib/portable-net40+sl5+wp80+win8+monotouch+monoandroid/Newtonsoft.Json.xml
	packages/Newtonsoft.Json.6.0.2/lib/portable-net45+wp80+win8+wpa81/Newtonsoft.Json.xml
	packages/Newtonsoft.Json.6.0.2/tools/install.ps1
	packages/Newtonsoft.Json.6.0.3/lib/net20/Newtonsoft.Json.xml
	packages/Newtonsoft.Json.6.0.3/lib/net35/Newtonsoft.Json.xml
	packages/Newtonsoft.Json.6.0.3/lib/net40/Newtonsoft.Json.xml
	packages/Newtonsoft.Json.6.0.3/lib/net45/Newtonsoft.Json.xml
	packages/Newtonsoft.Json.6.0.3/lib/portable-net40+sl4+wp7+win8/Newtonsoft.Json.xml
	packages/Newtonsoft.Json.6.0.3/lib/portable-net45+wp80+win8+wpa81/Newtonsoft.Json.xml
	packages/Newtonsoft.Json.6.0.3/tools/install.ps1
+ Add support to REST API to allow cross-site API calls from trusted domains.
+ Person pickers now will select the person when you click on them instead of making you click the 'select' button
+ Group pickers, Page pickers, and all the other pickers will now select the item instead of making you click the 'select' button. (If the picker supports selecting multiple items, you still need to click the 'select' button)
+ Merge branch 'origin/feature-dt-plugin'
+ Merge branch 'origin/feature-dt-plugin'
+ Changed group tree to only show children of the configured root group and made the 'Add-Top Level' behave as you would expect in that case.
+ When creating new top level group from GroupTreeView, the cancel button now works correctly. [Fixes #473]
+ updated checkin footer so it doesn't overlap buttons (fixes #428)
+ Merge branch 'feature-mp-contextsetter' into develop
+ Updated to Font Awesome 4.1.0
+ Merge branch 'origin/feature-rd-restkeyadmin'
+ changed Html editor to automatically enable versioning if the Require Approval setting is enabled and also inform the editor that their changes won't be visible until the content is approved.
+ Update README.markdown

fixed link...+ Update README.markdown

added license info+ Added the ability to filter out inactive groups from the groups map block.
+ Fixed bug on the NewCommunications block which prevented showing when someone did not have an SMS number.  [Fixes #467]
+ Corrected Html content approval logic and now the block will be flushed from the cache as soon as it's approved. [Fixes #462]
+ Add PersonGetByEmail and PersonGetByPhoneNumber REST endpoints
+ Update GetByEmail and GetByPhoneNumber with feedback from @azturner
+ Fix bug with referencing batch.Id before batch was saved to the database
+ Update search with latest code from develop
+ Add 'Date', 'Time', and 'DayOfWeek' as avaialable merge fields to the Html Content Editor block.  Also added a merge field picker when using the Html Content Editor's Code Editor mode.
+ Add ability to 'Follow' specific people
+ Add person attributes for storing a person's social media links and add social media icons under person's photo
+ Merge branch 'origin/feature-dt-plugin'

Conflicts:
	Rock.Plugin/MigrationNumberAttribute.cs
	Rock.Plugin/Rock.Plugin.csproj
	Rock.Plugin/VersionAttribute.cs
	Rock/Plugin/VersionAttribute.cs
	Rock/Rock.csproj
+ Add People search by phone number & email
+ Merge branch 'origin/feature-dt-workflow-form-entry'

Conflicts:
	Rock/Rock.csproj
+ More updates to Workflow Type Detail block and controls to streamline editing.
+ Merge branch 'feature-mp-flotcharts' into develop
+ Changed DataView filters and Report field selection to internally store selected values for entities by Guid instead of Id.  This will cause any custom dataviews and reports to loose some of their settings. After upgrading, verify that your dataviews and reports are configured correctly.
+ Merge branch 'feature-dt-plugin'

Conflicts:
	Rock/Rock.csproj
+ Merge branch 'feature-dt-rock-plugin'
