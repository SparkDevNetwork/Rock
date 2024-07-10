// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
namespace Rock.Migrations
{
    /// <summary>
    ///
    /// </summary>
    public partial class CodeGenerated_20240705 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Bus.QueueList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Bus.QueueList", "Queue List", "Rock.Blocks.Bus.QueueList, Rock.Blocks, Version=1.16.5.4, Culture=neutral, PublicKeyToken=null", false, false, "BE20153D-8462-403D-B18D-8E8AFC274EE5" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.CheckIn.CheckInKiosk
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.CheckIn.CheckInKiosk", "Check In Kiosk", "Rock.Blocks.CheckIn.CheckInKiosk, Rock.Blocks, Version=1.16.5.4, Culture=neutral, PublicKeyToken=null", false, false, "B208CAFE-2194-4308-AA52-A920C516805A" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.CheckIn.CheckInKioskSetup
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.CheckIn.CheckInKioskSetup", "Check In Kiosk Setup", "Rock.Blocks.CheckIn.CheckInKioskSetup, Rock.Blocks, Version=1.16.5.4, Culture=neutral, PublicKeyToken=null", false, false, "B252D7DF-D0C1-4126-A36A-F0B1E2063372" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.CheckIn.Configuration.CheckInLabelDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.CheckIn.Configuration.CheckInLabelDetail", "Check In Label Detail", "Rock.Blocks.CheckIn.Configuration.CheckInLabelDetail, Rock.Blocks, Version=1.16.5.4, Culture=neutral, PublicKeyToken=null", false, false, "E61908FC-EC33-4B55-B3B9-D83E32A1F064" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.CheckIn.Configuration.CheckInLabelList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.CheckIn.Configuration.CheckInLabelList", "Check In Label List", "Rock.Blocks.CheckIn.Configuration.CheckInLabelList, Rock.Blocks, Version=1.16.5.4, Culture=neutral, PublicKeyToken=null", false, false, "A20E29C7-83F6-46AD-87C3-4C1846022F8A" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.CheckIn.Configuration.CheckInSimulator
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.CheckIn.Configuration.CheckInSimulator", "Check In Simulator", "Rock.Blocks.CheckIn.Configuration.CheckInSimulator, Rock.Blocks, Version=1.16.5.4, Culture=neutral, PublicKeyToken=null", false, false, "23316388-EC1D-495A-8EFB-C1B5F6806041" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.CheckIn.Configuration.LabelDesigner
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.CheckIn.Configuration.LabelDesigner", "Label Designer", "Rock.Blocks.CheckIn.Configuration.LabelDesigner, Rock.Blocks, Version=1.16.5.4, Culture=neutral, PublicKeyToken=null", false, false, "3F477B52-6062-4AF4-ABB7-B8C153F6242A" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.LayoutList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.LayoutList", "Layout List", "Rock.Blocks.Cms.LayoutList, Rock.Blocks, Version=1.16.5.4, Culture=neutral, PublicKeyToken=null", false, false, "6E1D987D-DE38-4440-B54F-717C102795FE" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.ShortLink
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.ShortLink", "Short Link", "Rock.Blocks.Cms.ShortLink, Rock.Blocks, Version=1.16.5.4, Culture=neutral, PublicKeyToken=null", false, false, "026C6A93-5295-43E9-B67D-C3708ACB25B9" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Engagement.CampaignList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Engagement.CampaignList", "Campaign List", "Rock.Blocks.Engagement.CampaignList, Rock.Blocks, Version=1.16.5.4, Culture=neutral, PublicKeyToken=null", false, false, "68FF1164-17C0-4D30-A937-B2E628CCBFDE" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Event.EventItemOccurrenceList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Event.EventItemOccurrenceList", "Event Item Occurrence List", "Rock.Blocks.Event.EventItemOccurrenceList, Rock.Blocks, Version=1.16.5.4, Culture=neutral, PublicKeyToken=null", false, false, "AB765C53-424B-4824-AFD6-1174228FD92F" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Security.AuthClientList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Security.AuthClientList", "Auth Client List", "Rock.Blocks.Security.AuthClientList, Rock.Blocks, Version=1.16.5.4, Culture=neutral, PublicKeyToken=null", false, false, "FFA316A0-0508-4AD8-806B-D636A30386E7" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Security.AuthScopeList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Security.AuthScopeList", "Auth Scope List", "Rock.Blocks.Security.AuthScopeList, Rock.Blocks, Version=1.16.5.4, Culture=neutral, PublicKeyToken=null", false, false, "A4F6030A-C5A9-44F8-ABB2-22DF2FCB7D91" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Security.UserLoginList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Security.UserLoginList", "User Login List", "Rock.Blocks.Security.UserLoginList, Rock.Blocks, Version=1.16.5.4, Culture=neutral, PublicKeyToken=null", false, false, "B74114DA-830B-45CF-A04B-E96C5D27783F" );

            // Add/Update Obsidian Block Type
            //   Name:Queue List
            //   Category:Bus
            //   EntityType:Rock.Blocks.Bus.QueueList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Queue List", "Displays the details of bus queues.", "Rock.Blocks.Bus.QueueList", "Bus", "8A5785FC-3094-4C2C-929A-3FD6D21DA7F8" );

            // Add/Update Obsidian Block Type
            //   Name:Check-in Kiosk
            //   Category:Check-in
            //   EntityType:Rock.Blocks.CheckIn.CheckInKiosk
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Check-in Kiosk", "The standard Rock block for performing check-in at a kiosk.", "Rock.Blocks.CheckIn.CheckInKiosk", "Check-in", "A27FD0AA-67EE-44C3-9E5F-3289C6A210F3" );

            // Add/Update Obsidian Block Type
            //   Name:Check-in Kiosk Setup
            //   Category:Check-in
            //   EntityType:Rock.Blocks.CheckIn.CheckInKioskSetup
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Check-in Kiosk Setup", "Sets kiosk options and then starts the kiosk to allow self check-in.", "Rock.Blocks.CheckIn.CheckInKioskSetup", "Check-in", "D42352A2-C48D-443B-A51D-31EA4CE0C5A4" );

            // Add/Update Obsidian Block Type
            //   Name:Check-in Label Detail
            //   Category:Check-in > Configuration
            //   EntityType:Rock.Blocks.CheckIn.Configuration.CheckInLabelDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Check-in Label Detail", "Displays the details of a particular check in label.", "Rock.Blocks.CheckIn.Configuration.CheckInLabelDetail", "Check-in > Configuration", "3299706F-2BB8-49DB-831B-86A2B282BB02" );

            // Add/Update Obsidian Block Type
            //   Name:Check-in Label List
            //   Category:Check-in > Configuration
            //   EntityType:Rock.Blocks.CheckIn.Configuration.CheckInLabelList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Check-in Label List", "Displays a list of check in labels.", "Rock.Blocks.CheckIn.Configuration.CheckInLabelList", "Check-in > Configuration", "357014CB-376C-4957-A031-51A8371C3EBF" );

            // Add/Update Obsidian Block Type
            //   Name:Check-in Simulator
            //   Category:Check-in > Configuration
            //   EntityType:Rock.Blocks.CheckIn.Configuration.CheckInSimulator
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Check-in Simulator", "Simulates the check-in process in a UI that can be used to quickly test different configuration settings.", "Rock.Blocks.CheckIn.Configuration.CheckInSimulator", "Check-in > Configuration", "30002636-494B-4FDC-848C-A816F9291764" );

            // Add/Update Obsidian Block Type
            //   Name:Label Designer
            //   Category:Check-in > Configuration
            //   EntityType:Rock.Blocks.CheckIn.Configuration.LabelDesigner
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Label Designer", "Designs a check-in label with a nice drag and drop experience.", "Rock.Blocks.CheckIn.Configuration.LabelDesigner", "Check-in > Configuration", "8C4AD18F-9F81-4145-8AD0-AB90E451D0D6" );

            // Add/Update Obsidian Block Type
            //   Name:Layout List
            //   Category:CMS
            //   EntityType:Rock.Blocks.Cms.LayoutList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Layout List", "Displays a list of layouts.", "Rock.Blocks.Cms.LayoutList", "CMS", "6A10A280-65B8-4988-96B2-974FCD80604B" );

            // Add/Update Obsidian Block Type
            //   Name:Shortened Links
            //   Category:Administration
            //   EntityType:Rock.Blocks.Cms.ShortLink
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Shortened Links", "Displays a dialog for adding a short link to the current page.", "Rock.Blocks.Cms.ShortLink", "Administration", "C85551E8-A363-4AA6-9BFD-E6A1C9CEDE80" );

            // Add/Update Obsidian Block Type
            //   Name:Campaign List
            //   Category:Engagement
            //   EntityType:Rock.Blocks.Engagement.CampaignList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Campaign List", "Block for viewing list of campaign connection configurations.", "Rock.Blocks.Engagement.CampaignList", "Engagement", "9BD8B4B1-638E-4F35-9593-1E854BDA44DC" );

            // Add/Update Obsidian Block Type
            //   Name:Calendar Event Item Occurrence List
            //   Category:Event
            //   EntityType:Rock.Blocks.Event.EventItemOccurrenceList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Calendar Event Item Occurrence List", "Displays the occurrence details for a given calendar event item.", "Rock.Blocks.Event.EventItemOccurrenceList", "Event", "DDC28E7A-E6C0-4081-B4B9-7CD6475E9046" );

            // Add/Update Obsidian Block Type
            //   Name:OpenID Connect Clients
            //   Category:Security > OIDC
            //   EntityType:Rock.Blocks.Security.AuthClientList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "OpenID Connect Clients", "Block for displaying and editing available OpenID Connect clients.", "Rock.Blocks.Security.AuthClientList", "Security > OIDC", "53A34D60-31B8-4D22-BC42-E3B669ED152B" );

            // Add/Update Obsidian Block Type
            //   Name:OpenID Connect Scopes
            //   Category:Security > OIDC
            //   EntityType:Rock.Blocks.Security.AuthScopeList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "OpenID Connect Scopes", "Block for displaying and editing available OpenID Connect scopes.", "Rock.Blocks.Security.AuthScopeList", "Security > OIDC", "9FF39411-D9CE-4A5D-B04A-2DB169A688F4" );

            // Add/Update Obsidian Block Type
            //   Name:User Login List
            //   Category:Security
            //   EntityType:Rock.Blocks.Security.UserLoginList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "User Login List", "Block for displaying logins.  By default displays all logins, but can be configured to use person context to display logins for a specific person.", "Rock.Blocks.Security.UserLoginList", "Security", "DBFA9E41-FA62-4869-8A44-D03B561433B2" );

            // Attribute for BlockType
            //   BlockType: Group Tree View
            //   Category: Groups
            //   Attribute: Limit to Security Role Groups
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2D26A2C4-62DC-4680-8219-A52EB2BC0F65", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Limit to Security Role Groups", "LimittoSecurityRoleGroups", "Limit to Security Role Groups", @"", 5, @"False", "38BA8DBD-AAD9-4C91-AF45-7EA4E44C06BF" );

            // Attribute for BlockType
            //   BlockType: Group Detail Lava
            //   Category: Groups
            //   Attribute: Show 'Email Group Leaders' Button
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "218B057F-B214-4317-8E84-7A95CF88067E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show 'Email Group Leaders' Button", "ShowEmailGroupLeadersButton", "Show 'Email Group Leaders' Button", @"Determines if the 'Email Group Leaders' button should be displayed.", 20, @"False", "AB162002-82EF-4C8F-A390-B540A4C1B12A" );

            // Attribute for BlockType
            //   BlockType: Group Detail Lava
            //   Category: Groups
            //   Attribute: Show 'Email Roster Parents' Button
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "218B057F-B214-4317-8E84-7A95CF88067E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show 'Email Roster Parents' Button", "ShowEmailRosterParentsButton", "Show 'Email Roster Parents' Button", @"Determines if the 'Email Roster Parents' button should be displayed.", 21, @"False", "5E017E62-D864-48E6-BA1B-27168FCDB29B" );

            // Attribute for BlockType
            //   BlockType: Service Metrics Entry
            //   Category: Reporting
            //   Attribute: Default to Current Week
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "535E1879-CD4C-432B-9312-B27B3A668D88", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Default to Current Week", "DefaultToCurrentWeek", "Default to Current Week", @"When enabled, the block will bypass the step to select a week from a list and will instead skip right to the entry page with the current week selected.", 3, @"False", "9C4598C2-51EB-4F79-8B73-B707790597EC" );

            // Attribute for BlockType
            //   BlockType: Reminder Edit
            //   Category: Reminders
            //   Attribute: Show Assigned To
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BA26C29E-660C-470D-9FEA-5830DB15E935", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Assigned To", "ShowAssignedTo", "Show Assigned To", @"Whether to show the assigned to field. Otherwise defaults to the Current Person.", 2, @"False", "95F9F210-4EE3-4EEE-A322-D64F856256F0" );

            // Attribute for BlockType
            //   BlockType: Service Metrics Entry
            //   Category: Reporting
            //   Attribute: Default to Current Week
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E6144C7A-2E95-431B-AB75-C588D151ACA4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Default to Current Week", "DefaultToCurrentWeek", "Default to Current Week", @"When enabled, the block will bypass the step to select a week from a list and will instead skip right to the entry page with the current week selected.", 3, @"False", "847EEE85-9CEB-4CB0-AA56-7A43033BC041" );

            // Attribute for BlockType
            //   BlockType: Queue List
            //   Category: Bus
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8A5785FC-3094-4C2C-929A-3FD6D21DA7F8", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the page short link details.", 0, @"", "7568B370-DFBA-440B-8828-A755F5503242" );

            // Attribute for BlockType
            //   BlockType: Check-in Kiosk
            //   Category: Check-in
            //   Attribute: Setup Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A27FD0AA-67EE-44C3-9E5F-3289C6A210F3", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Setup Page", "SetupPage", "Setup Page", @"The page to use when kiosk setup is required.", 0, @"", "A380BDB9-96CF-4F90-A921-FED850C67B59" );

            // Attribute for BlockType
            //   BlockType: Check-in Kiosk Setup
            //   Category: Check-in
            //   Attribute: Kiosk Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D42352A2-C48D-443B-A51D-31EA4CE0C5A4", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Kiosk Page", "KioskPage", "Kiosk Page", @"The page to redirect to after configuration has been set.", 0, @"", "3BEAE186-4F7A-4F5A-AD37-8EDE2D939D36" );

            // Attribute for BlockType
            //   BlockType: Check-in Kiosk Setup
            //   Category: Check-in
            //   Attribute: Allow Manual Setup
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D42352A2-C48D-443B-A51D-31EA4CE0C5A4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Manual Setup", "AllowManualSetup", "Allow Manual Setup", @"If enabled, the block will allow the kiosk to be setup manually if it was not set via other means.", 1, @"True", "9EB01CA4-2E97-48DB-901C-F4E3240A26E8" );

            // Attribute for BlockType
            //   BlockType: Check-in Kiosk Setup
            //   Category: Check-in
            //   Attribute: Enable Location Sharing
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D42352A2-C48D-443B-A51D-31EA4CE0C5A4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Location Sharing", "EnableLocationSharing", "Enable Location Sharing", @"If enabled, the block will attempt to determine the kiosk's location via location sharing geocode.", 2, @"False", "AEBC9857-5360-4B4C-A4F4-1AED5B9CE69B" );

            // Attribute for BlockType
            //   BlockType: Check-in Kiosk Setup
            //   Category: Check-in
            //   Attribute: Time to Cache Kiosk GeoLocation
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D42352A2-C48D-443B-A51D-31EA4CE0C5A4", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Time to Cache Kiosk GeoLocation", "TimeToCacheKioskLocation", "Time to Cache Kiosk GeoLocation", @"Time in minutes to cache the coordinates of the kiosk. A value of zero (0) means cache forever. Default 20 minutes.", 3, @"20", "E39B6635-1083-4E3A-BAA1-E68DE5E720E6" );

            // Attribute for BlockType
            //   BlockType: Check-in Kiosk Setup
            //   Category: Check-in
            //   Attribute: Enable Kiosk Match By Name
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D42352A2-C48D-443B-A51D-31EA4CE0C5A4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Kiosk Match By Name", "EnableKioskMatchByName", "Enable Kiosk Match By Name", @"Enable a kiosk match by computer name by doing reverse IP lookup to get computer name based on IP address", 4, @"False", "AFEDFCE6-7A07-4EA9-93B4-C70A896B3CE3" );

            // Attribute for BlockType
            //   BlockType: Check-in Label Detail
            //   Category: Check-in > Configuration
            //   Attribute: Designer Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3299706F-2BB8-49DB-831B-86A2B282BB02", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Designer Page", "DesignerPage", "Designer Page", @"The page that will show the label designer.", 0, @"", "0E94E533-87B1-4654-946B-03EB640F1D41" );

            // Attribute for BlockType
            //   BlockType: Check-in Label List
            //   Category: Check-in > Configuration
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "357014CB-376C-4957-A031-51A8371C3EBF", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the check in label details.", 0, @"", "3C5684B3-C486-4FA9-BA05-19E02DC60B3D" );

            // Attribute for BlockType
            //   BlockType: Check-in Label List
            //   Category: Check-in > Configuration
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "357014CB-376C-4957-A031-51A8371C3EBF", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "88CE991D-CCB0-4421-9749-157982309D62" );

            // Attribute for BlockType
            //   BlockType: Check-in Label List
            //   Category: Check-in > Configuration
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "357014CB-376C-4957-A031-51A8371C3EBF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "5A5E49E9-B296-4174-92DC-AD60E7D8E45C" );

            // Attribute for BlockType
            //   BlockType: Layout List
            //   Category: CMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6A10A280-65B8-4988-96B2-974FCD80604B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the layout details.", 0, @"", "746F7B5F-9906-41D4-BD6E-E66653256B23" );

            // Attribute for BlockType
            //   BlockType: Layout List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6A10A280-65B8-4988-96B2-974FCD80604B", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "A6726E17-8B5C-4D2E-BEBC-5969282C04DD" );

            // Attribute for BlockType
            //   BlockType: Layout List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6A10A280-65B8-4988-96B2-974FCD80604B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "1610117B-CE51-4826-AB6C-73EA9AD762ED" );

            // Attribute for BlockType
            //   BlockType: Shortened Links
            //   Category: Administration
            //   Attribute: Minimum Token Length
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C85551E8-A363-4AA6-9BFD-E6A1C9CEDE80", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Minimum Token Length", "MinimumTokenLength", "Minimum Token Length", @"The minimum number of characters for the token.", 0, @"7", "944120C5-A44D-4BB6-B507-6F588EE7DD1F" );

            // Attribute for BlockType
            //   BlockType: Campaign List
            //   Category: Engagement
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9BD8B4B1-638E-4F35-9593-1E854BDA44DC", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the campaign details.", 0, @"", "E6EFF7B2-223C-4604-8589-BA036788C432" );

            // Attribute for BlockType
            //   BlockType: Campaign List
            //   Category: Engagement
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9BD8B4B1-638E-4F35-9593-1E854BDA44DC", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "BEC13803-83B0-44B1-898B-85E0D30FEF78" );

            // Attribute for BlockType
            //   BlockType: Campaign List
            //   Category: Engagement
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9BD8B4B1-638E-4F35-9593-1E854BDA44DC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "1DB49CFD-6672-4647-8963-0B433392319F" );

            // Attribute for BlockType
            //   BlockType: Calendar Event Item Occurrence List
            //   Category: Event
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DDC28E7A-E6C0-4081-B4B9-7CD6475E9046", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the event item occurrence details.", 0, @"", "0691F346-135C-43ED-80A4-79A5CCA2F722" );

            // Attribute for BlockType
            //   BlockType: Calendar Event Item Occurrence List
            //   Category: Event
            //   Attribute: Registration Instance Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DDC28E7A-E6C0-4081-B4B9-7CD6475E9046", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Registration Instance Page", "RegistrationInstancePage", "Registration Instance Page", @"The page to view registration details", 1, @"", "7ACB9AB8-86A0-4B2C-8357-7DB80CCFA736" );

            // Attribute for BlockType
            //   BlockType: Calendar Event Item Occurrence List
            //   Category: Event
            //   Attribute: Group Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DDC28E7A-E6C0-4081-B4B9-7CD6475E9046", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Detail Page", "GroupDetailPage", "Group Detail Page", @"The page for viewing details about a group", 2, @"", "552E00C7-D1B4-4F33-B1A7-23824C93687D" );

            // Attribute for BlockType
            //   BlockType: Calendar Event Item Occurrence List
            //   Category: Event
            //   Attribute: Content Item Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DDC28E7A-E6C0-4081-B4B9-7CD6475E9046", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Content Item Detail Page", "ContentItemDetailPage", "Content Item Detail Page", @"The page for viewing details about a content item", 3, @"", "7F073D99-70AB-433A-A185-09D43A4887A0" );

            // Attribute for BlockType
            //   BlockType: Calendar Event Item Occurrence List
            //   Category: Event
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DDC28E7A-E6C0-4081-B4B9-7CD6475E9046", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "CF360FE7-D5FA-4622-B419-233DF63B5342" );

            // Attribute for BlockType
            //   BlockType: Calendar Event Item Occurrence List
            //   Category: Event
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DDC28E7A-E6C0-4081-B4B9-7CD6475E9046", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "5D366EDA-0656-492B-BD22-09F282A50F44" );

            // Attribute for BlockType
            //   BlockType: OpenID Connect Clients
            //   Category: Security > OIDC
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "53A34D60-31B8-4D22-BC42-E3B669ED152B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the auth client details.", 0, @"", "C85871A7-883B-46C1-B654-3E80113580B8" );

            // Attribute for BlockType
            //   BlockType: OpenID Connect Clients
            //   Category: Security > OIDC
            //   Attribute: OpenID Connect Scopes Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "53A34D60-31B8-4D22-BC42-E3B669ED152B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "OpenID Connect Scopes Page", "ScopePage", "OpenID Connect Scopes Page", @"", 2, @"", "89EC3E11-D2EC-429E-9F3E-B99A7816F26A" );

            // Attribute for BlockType
            //   BlockType: OpenID Connect Clients
            //   Category: Security > OIDC
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "53A34D60-31B8-4D22-BC42-E3B669ED152B", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "76386DAF-307A-40D5-95B3-63BB6C2774E6" );

            // Attribute for BlockType
            //   BlockType: OpenID Connect Clients
            //   Category: Security > OIDC
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "53A34D60-31B8-4D22-BC42-E3B669ED152B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "FBB0D007-0BC5-4D31-909C-609D9EDD5934" );

            // Attribute for BlockType
            //   BlockType: OpenID Connect Scopes
            //   Category: Security > OIDC
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FF39411-D9CE-4A5D-B04A-2DB169A688F4", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the auth scope details.", 0, @"", "57EF285C-0D46-43E8-98BE-A208CD5B3F55" );

            // Attribute for BlockType
            //   BlockType: OpenID Connect Scopes
            //   Category: Security > OIDC
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FF39411-D9CE-4A5D-B04A-2DB169A688F4", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "54E351E0-479C-4CB0-B565-31BC0D4ACEE3" );

            // Attribute for BlockType
            //   BlockType: OpenID Connect Scopes
            //   Category: Security > OIDC
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FF39411-D9CE-4A5D-B04A-2DB169A688F4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "14F7BFF7-04DF-406B-B772-0C9908F76E3C" );

            // Attribute for BlockType
            //   BlockType: User Login List
            //   Category: Security
            //   Attribute: Entity Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DBFA9E41-FA62-4869-8A44-D03B561433B2", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "Entity Type", @"The type of entity that will provide context for this block", 0, @"", "CEDE12DC-C17A-490F-8DB7-5755D293DA28" );

            // Attribute for BlockType
            //   BlockType: User Login List
            //   Category: Security
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DBFA9E41-FA62-4869-8A44-D03B561433B2", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "B7FCA384-CCF9-44D9-BA43-6121672B5C5F" );

            // Attribute for BlockType
            //   BlockType: User Login List
            //   Category: Security
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DBFA9E41-FA62-4869-8A44-D03B561433B2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "D9562E1A-2C16-483D-8C58-67613C60F9D1" );

            // Add Block Attribute Value
            //   Block: Page Route List
            //   BlockType: Route List
            //   Category: CMS
            //   Block Location: Page=Routes, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            RockMigrationHelper.AddBlockAttributeValue( "36FFB74B-E093-4E92-96D8-E4A3E2A08233", "46E82B2C-68D1-4337-993B-7F46B1B7D445", @"" );

            // Add Block Attribute Value
            //   Block: Page Route List
            //   BlockType: Route List
            //   Category: CMS
            //   Block Location: Page=Routes, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "36FFB74B-E093-4E92-96D8-E4A3E2A08233", "BB272CDB-EA52-4BC7-A02B-0F89C50618FA", @"False" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Reminder Edit
            //   Category: Reminders
            //   Attribute: Show Assigned To
            RockMigrationHelper.DeleteAttribute( "95F9F210-4EE3-4EEE-A322-D64F856256F0" );

            // Attribute for BlockType
            //   BlockType: Service Metrics Entry
            //   Category: Reporting
            //   Attribute: Default to Current Week
            RockMigrationHelper.DeleteAttribute( "847EEE85-9CEB-4CB0-AA56-7A43033BC041" );

            // Attribute for BlockType
            //   BlockType: User Login List
            //   Category: Security
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "D9562E1A-2C16-483D-8C58-67613C60F9D1" );

            // Attribute for BlockType
            //   BlockType: User Login List
            //   Category: Security
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "B7FCA384-CCF9-44D9-BA43-6121672B5C5F" );

            // Attribute for BlockType
            //   BlockType: User Login List
            //   Category: Security
            //   Attribute: Entity Type
            RockMigrationHelper.DeleteAttribute( "CEDE12DC-C17A-490F-8DB7-5755D293DA28" );

            // Attribute for BlockType
            //   BlockType: OpenID Connect Scopes
            //   Category: Security > OIDC
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "14F7BFF7-04DF-406B-B772-0C9908F76E3C" );

            // Attribute for BlockType
            //   BlockType: OpenID Connect Scopes
            //   Category: Security > OIDC
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "54E351E0-479C-4CB0-B565-31BC0D4ACEE3" );

            // Attribute for BlockType
            //   BlockType: OpenID Connect Scopes
            //   Category: Security > OIDC
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "57EF285C-0D46-43E8-98BE-A208CD5B3F55" );

            // Attribute for BlockType
            //   BlockType: OpenID Connect Clients
            //   Category: Security > OIDC
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "FBB0D007-0BC5-4D31-909C-609D9EDD5934" );

            // Attribute for BlockType
            //   BlockType: OpenID Connect Clients
            //   Category: Security > OIDC
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "76386DAF-307A-40D5-95B3-63BB6C2774E6" );

            // Attribute for BlockType
            //   BlockType: OpenID Connect Clients
            //   Category: Security > OIDC
            //   Attribute: OpenID Connect Scopes Page
            RockMigrationHelper.DeleteAttribute( "89EC3E11-D2EC-429E-9F3E-B99A7816F26A" );

            // Attribute for BlockType
            //   BlockType: OpenID Connect Clients
            //   Category: Security > OIDC
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "C85871A7-883B-46C1-B654-3E80113580B8" );

            // Attribute for BlockType
            //   BlockType: Calendar Event Item Occurrence List
            //   Category: Event
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "5D366EDA-0656-492B-BD22-09F282A50F44" );

            // Attribute for BlockType
            //   BlockType: Calendar Event Item Occurrence List
            //   Category: Event
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "CF360FE7-D5FA-4622-B419-233DF63B5342" );

            // Attribute for BlockType
            //   BlockType: Calendar Event Item Occurrence List
            //   Category: Event
            //   Attribute: Content Item Detail Page
            RockMigrationHelper.DeleteAttribute( "7F073D99-70AB-433A-A185-09D43A4887A0" );

            // Attribute for BlockType
            //   BlockType: Calendar Event Item Occurrence List
            //   Category: Event
            //   Attribute: Group Detail Page
            RockMigrationHelper.DeleteAttribute( "552E00C7-D1B4-4F33-B1A7-23824C93687D" );

            // Attribute for BlockType
            //   BlockType: Calendar Event Item Occurrence List
            //   Category: Event
            //   Attribute: Registration Instance Page
            RockMigrationHelper.DeleteAttribute( "7ACB9AB8-86A0-4B2C-8357-7DB80CCFA736" );

            // Attribute for BlockType
            //   BlockType: Calendar Event Item Occurrence List
            //   Category: Event
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "0691F346-135C-43ED-80A4-79A5CCA2F722" );

            // Attribute for BlockType
            //   BlockType: Campaign List
            //   Category: Engagement
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "1DB49CFD-6672-4647-8963-0B433392319F" );

            // Attribute for BlockType
            //   BlockType: Campaign List
            //   Category: Engagement
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "BEC13803-83B0-44B1-898B-85E0D30FEF78" );

            // Attribute for BlockType
            //   BlockType: Campaign List
            //   Category: Engagement
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "E6EFF7B2-223C-4604-8589-BA036788C432" );

            // Attribute for BlockType
            //   BlockType: Shortened Links
            //   Category: Administration
            //   Attribute: Minimum Token Length
            RockMigrationHelper.DeleteAttribute( "944120C5-A44D-4BB6-B507-6F588EE7DD1F" );

            // Attribute for BlockType
            //   BlockType: Layout List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "1610117B-CE51-4826-AB6C-73EA9AD762ED" );

            // Attribute for BlockType
            //   BlockType: Layout List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "A6726E17-8B5C-4D2E-BEBC-5969282C04DD" );

            // Attribute for BlockType
            //   BlockType: Layout List
            //   Category: CMS
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "746F7B5F-9906-41D4-BD6E-E66653256B23" );

            // Attribute for BlockType
            //   BlockType: Check-in Label List
            //   Category: Check-in > Configuration
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "5A5E49E9-B296-4174-92DC-AD60E7D8E45C" );

            // Attribute for BlockType
            //   BlockType: Check-in Label List
            //   Category: Check-in > Configuration
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "88CE991D-CCB0-4421-9749-157982309D62" );

            // Attribute for BlockType
            //   BlockType: Check-in Label List
            //   Category: Check-in > Configuration
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "3C5684B3-C486-4FA9-BA05-19E02DC60B3D" );

            // Attribute for BlockType
            //   BlockType: Check-in Label Detail
            //   Category: Check-in > Configuration
            //   Attribute: Designer Page
            RockMigrationHelper.DeleteAttribute( "0E94E533-87B1-4654-946B-03EB640F1D41" );

            // Attribute for BlockType
            //   BlockType: Check-in Kiosk Setup
            //   Category: Check-in
            //   Attribute: Enable Kiosk Match By Name
            RockMigrationHelper.DeleteAttribute( "AFEDFCE6-7A07-4EA9-93B4-C70A896B3CE3" );

            // Attribute for BlockType
            //   BlockType: Check-in Kiosk Setup
            //   Category: Check-in
            //   Attribute: Time to Cache Kiosk GeoLocation
            RockMigrationHelper.DeleteAttribute( "E39B6635-1083-4E3A-BAA1-E68DE5E720E6" );

            // Attribute for BlockType
            //   BlockType: Check-in Kiosk Setup
            //   Category: Check-in
            //   Attribute: Enable Location Sharing
            RockMigrationHelper.DeleteAttribute( "AEBC9857-5360-4B4C-A4F4-1AED5B9CE69B" );

            // Attribute for BlockType
            //   BlockType: Check-in Kiosk Setup
            //   Category: Check-in
            //   Attribute: Allow Manual Setup
            RockMigrationHelper.DeleteAttribute( "9EB01CA4-2E97-48DB-901C-F4E3240A26E8" );

            // Attribute for BlockType
            //   BlockType: Check-in Kiosk Setup
            //   Category: Check-in
            //   Attribute: Kiosk Page
            RockMigrationHelper.DeleteAttribute( "3BEAE186-4F7A-4F5A-AD37-8EDE2D939D36" );

            // Attribute for BlockType
            //   BlockType: Check-in Kiosk
            //   Category: Check-in
            //   Attribute: Setup Page
            RockMigrationHelper.DeleteAttribute( "A380BDB9-96CF-4F90-A921-FED850C67B59" );

            // Attribute for BlockType
            //   BlockType: Queue List
            //   Category: Bus
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "7568B370-DFBA-440B-8828-A755F5503242" );

            // Attribute for BlockType
            //   BlockType: Service Metrics Entry
            //   Category: Reporting
            //   Attribute: Default to Current Week
            RockMigrationHelper.DeleteAttribute( "9C4598C2-51EB-4F79-8B73-B707790597EC" );

            // Attribute for BlockType
            //   BlockType: Group Detail Lava
            //   Category: Groups
            //   Attribute: Show 'Email Roster Parents' Button
            RockMigrationHelper.DeleteAttribute( "5E017E62-D864-48E6-BA1B-27168FCDB29B" );

            // Attribute for BlockType
            //   BlockType: Group Detail Lava
            //   Category: Groups
            //   Attribute: Show 'Email Group Leaders' Button
            RockMigrationHelper.DeleteAttribute( "AB162002-82EF-4C8F-A390-B540A4C1B12A" );

            // Delete BlockType 
            //   Name: User Login List
            //   Category: Security
            //   Path: -
            //   EntityType: User Login List
            RockMigrationHelper.DeleteBlockType( "DBFA9E41-FA62-4869-8A44-D03B561433B2" );

            // Delete BlockType 
            //   Name: OpenID Connect Scopes
            //   Category: Security > OIDC
            //   Path: -
            //   EntityType: Auth Scope List
            RockMigrationHelper.DeleteBlockType( "9FF39411-D9CE-4A5D-B04A-2DB169A688F4" );

            // Delete BlockType 
            //   Name: OpenID Connect Clients
            //   Category: Security > OIDC
            //   Path: -
            //   EntityType: Auth Client List
            RockMigrationHelper.DeleteBlockType( "53A34D60-31B8-4D22-BC42-E3B669ED152B" );

            // Delete BlockType 
            //   Name: Calendar Event Item Occurrence List
            //   Category: Event
            //   Path: -
            //   EntityType: Event Item Occurrence List
            RockMigrationHelper.DeleteBlockType( "DDC28E7A-E6C0-4081-B4B9-7CD6475E9046" );

            // Delete BlockType 
            //   Name: Campaign List
            //   Category: Engagement
            //   Path: -
            //   EntityType: Campaign List
            RockMigrationHelper.DeleteBlockType( "9BD8B4B1-638E-4F35-9593-1E854BDA44DC" );

            // Delete BlockType 
            //   Name: Shortened Links
            //   Category: Administration
            //   Path: -
            //   EntityType: Short Link
            RockMigrationHelper.DeleteBlockType( "C85551E8-A363-4AA6-9BFD-E6A1C9CEDE80" );

            // Delete BlockType 
            //   Name: Layout List
            //   Category: CMS
            //   Path: -
            //   EntityType: Layout List
            RockMigrationHelper.DeleteBlockType( "6A10A280-65B8-4988-96B2-974FCD80604B" );

            // Delete BlockType 
            //   Name: Label Designer
            //   Category: Check-in > Configuration
            //   Path: -
            //   EntityType: Label Designer
            RockMigrationHelper.DeleteBlockType( "8C4AD18F-9F81-4145-8AD0-AB90E451D0D6" );

            // Delete BlockType 
            //   Name: Check-in Simulator
            //   Category: Check-in > Configuration
            //   Path: -
            //   EntityType: Check In Simulator
            RockMigrationHelper.DeleteBlockType( "30002636-494B-4FDC-848C-A816F9291764" );

            // Delete BlockType 
            //   Name: Check-in Label List
            //   Category: Check-in > Configuration
            //   Path: -
            //   EntityType: Check In Label List
            RockMigrationHelper.DeleteBlockType( "357014CB-376C-4957-A031-51A8371C3EBF" );

            // Delete BlockType 
            //   Name: Check-in Label Detail
            //   Category: Check-in > Configuration
            //   Path: -
            //   EntityType: Check In Label Detail
            RockMigrationHelper.DeleteBlockType( "3299706F-2BB8-49DB-831B-86A2B282BB02" );

            // Delete BlockType 
            //   Name: Check-in Kiosk Setup
            //   Category: Check-in
            //   Path: -
            //   EntityType: Check In Kiosk Setup
            RockMigrationHelper.DeleteBlockType( "D42352A2-C48D-443B-A51D-31EA4CE0C5A4" );

            // Delete BlockType 
            //   Name: Check-in Kiosk
            //   Category: Check-in
            //   Path: -
            //   EntityType: Check In Kiosk
            RockMigrationHelper.DeleteBlockType( "A27FD0AA-67EE-44C3-9E5F-3289C6A210F3" );

            // Delete BlockType 
            //   Name: Queue List
            //   Category: Bus
            //   Path: -
            //   EntityType: Queue List
            RockMigrationHelper.DeleteBlockType( "8A5785FC-3094-4C2C-929A-3FD6D21DA7F8" );
        }
    }
}
