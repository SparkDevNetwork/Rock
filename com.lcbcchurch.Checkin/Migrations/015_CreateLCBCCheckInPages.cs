// <copyright>
// Copyright by LCBC Church
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock;
using Rock.Plugin;
namespace com.lcbcchurch.Checkin.Migrations
{
    [MigrationNumber( 15, "1.6.0" )]
    public class CreateLCBCCheckInPages : Migration
    {
        public string LCBCCheckinSiteGuid = "30FB46F7-4814-4691-852A-04FB56CC07F0";
        public string LCBCCheckInRootPageGuid = "132DE89E-2556-45DE-A8CE-A77F4BC2EC79";
        public string CheckinGenericLCBCTheme_CheckinLayout = "23B01AB5-3AF2-46BC-8A75-BAF8CCC1C9E0";
        public override void Up()
        {
            // LCBC Check-in Site
            RockMigrationHelper.AddSite( "LCBC Check-in", "LCBC Check-In Site.", "CheckinGenericLCBC", LCBCCheckinSiteGuid );
            RockMigrationHelper.AddLayout( LCBCCheckinSiteGuid, "Checkin", "Checkin", "", CheckinGenericLCBCTheme_CheckinLayout );

            var customSiteUpdates = @"UPDATE [Site] SET [IsSystem] = 0 WHERE [Guid] = '{0}'";
            Sql( string.Format( customSiteUpdates, LCBCCheckinSiteGuid ) );

            // Page: LCBC Check-in root page (no blocks)
            RockMigrationHelper.AddPage("",CheckinGenericLCBCTheme_CheckinLayout,"LCBC Check-in","Screens for managing LCBC Check-in",LCBCCheckInRootPageGuid,""); // Site:LCBC Check-in
            
            // Add the attended check-in route to the default domain
            var customDomainUpdate = @"
                DECLARE @PageId int = (SELECT [Id] FROM [Page] WHERE [Guid] = '{0}')
                DECLARE @SiteId int = (SELECT [Id] FROM [Site] WHERE [Guid] = '{1}')
                DECLARE @SiteDomain varchar(200) = (SELECT TOP 1 [Domain] FROM [SiteDomain] WHERE [Domain] <> '')
                
                UPDATE [Site] SET [DefaultPageId] = @PageId, [AllowIndexing] = 0 WHERE [Id] = @SiteId
                
                SELECT @SiteDomain = @SiteDomain + '/lcbc-checkin'
                
                INSERT [SiteDomain] ([IsSystem], [SiteId], [Domain], [Guid])
                SELECT 0, @SiteId, @SiteDomain, NEWID()
            ";
            Sql( string.Format(customDomainUpdate, LCBCCheckInRootPageGuid, LCBCCheckinSiteGuid) );


            // Page: Admin
            RockMigrationHelper.AddPage("132DE89E-2556-45DE-A8CE-A77F4BC2EC79","23B01AB5-3AF2-46BC-8A75-BAF8CCC1C9E0","Admin","Admin screen for LCBC Check-in","44B1021C-B408-495F-8496-9DA333DC3C02",""); // Site:LCBC Check-in
            RockMigrationHelper.AddPageRoute("44B1021C-B408-495F-8496-9DA333DC3C02","lcbc-checkin");
            RockMigrationHelper.AddPageRoute("44B1021C-B408-495F-8496-9DA333DC3C02","lcbc-checkin/{KioskId}/{GroupTypeIds}");
            RockMigrationHelper.AddPageRoute("44B1021C-B408-495F-8496-9DA333DC3C02","lcbc-checkin/{KioskId}/{CheckinConfigId}/{GroupTypeIds}");
            RockMigrationHelper.UpdateBlockType("Administration","Check-in Administration block","~/Blocks/CheckIn/Admin.ascx","Check-in","3B5FBE9A-2904-4220-92F3-47DD16E805C0");
            // Add Block to Page: Admin, Site: LCBC Check-in
            RockMigrationHelper.AddBlock( true, "44B1021C-B408-495F-8496-9DA333DC3C02","","3B5FBE9A-2904-4220-92F3-47DD16E805C0","Admin","Main","","",0,"D13B8089-9AFD-4523-8E85-C89D8135A862"); 
            // Attrib for BlockType: Administration:Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute("3B5FBE9A-2904-4220-92F3-47DD16E805C0","46A03F59-55D3-4ACE-ADD5-B4642225DD20","Workflow Type","WorkflowType","","The workflow type to activate for check-in",0,@"","162A2B82-A71F-4B29-970A-047266FE696D");
            // Attrib for BlockType: Administration:Workflow Activity
            RockMigrationHelper.UpdateBlockTypeAttribute("3B5FBE9A-2904-4220-92F3-47DD16E805C0","9C204CD0-1233-41C5-818A-C5DA439445AA","Workflow Activity","WorkflowActivity","","The name of the workflow activity to run on selection.",1,@"","50DF7B49-FAF4-45D5-919F-14E589B37666");
            // Attrib for BlockType: Administration:Home Page
            RockMigrationHelper.UpdateBlockTypeAttribute("3B5FBE9A-2904-4220-92F3-47DD16E805C0","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Home Page","HomePage","","",2,@"","D46F3099-5700-4CCD-8B6C-F1F306BA02B8");
            // Attrib for BlockType: Administration:Previous Page
            RockMigrationHelper.UpdateBlockTypeAttribute("3B5FBE9A-2904-4220-92F3-47DD16E805C0","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Previous Page","PreviousPage","","",3,@"","7675AE35-1A61-460E-8FF6-B2A5C473F319");
            // Attrib for BlockType: Administration:Next Page
            RockMigrationHelper.UpdateBlockTypeAttribute("3B5FBE9A-2904-4220-92F3-47DD16E805C0","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Next Page","NextPage","","",4,@"","36C334EF-E723-4065-9C39-BD5663582751");
            // Attrib for BlockType: Administration:Allow Manual Setup
            RockMigrationHelper.UpdateBlockTypeAttribute("3B5FBE9A-2904-4220-92F3-47DD16E805C0","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Allow Manual Setup","AllowManualSetup","","If enabled, the block will allow the kiosk to be setup manually if it was not set via other means.",5,@"True","EBBC10ED-18E4-4E7D-9467-E7C27F12A745");
            // Attrib for BlockType: Administration:Enable Location Sharing
            RockMigrationHelper.UpdateBlockTypeAttribute("3B5FBE9A-2904-4220-92F3-47DD16E805C0","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Enable Location Sharing","EnableLocationSharing","","If enabled, the block will attempt to determine the kiosk's location via location sharing geocode.",6,@"False","992F693A-1019-468C-B7A7-B945A616BAF0");
            // Attrib for BlockType: Administration:Time to Cache Kiosk GeoLocation
            RockMigrationHelper.UpdateBlockTypeAttribute("3B5FBE9A-2904-4220-92F3-47DD16E805C0","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Time to Cache Kiosk GeoLocation","TimetoCacheKioskGeoLocation","","Time in minutes to cache the coordinates of the kiosk. A value of zero (0) means cache forever. Default 20 minutes.",7,@"20","C2A1F6A2-A801-4B52-8CFB-9B394CCA2D17");
            // Attrib for BlockType: Administration:Enable Kiosk Match By Name
            RockMigrationHelper.UpdateBlockTypeAttribute("3B5FBE9A-2904-4220-92F3-47DD16E805C0","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Enable Kiosk Match By Name","EnableReverseLookup","","Enable a kiosk match by computer name by doing reverseIP lookup to get computer name based on IP address",8,@"False","0E252443-86E1-4068-8B32-9943E0974C94");
            // Attrib Value for Block:Admin, Attribute:Workflow Type Page: Admin, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("D13B8089-9AFD-4523-8E85-C89D8135A862","162A2B82-A71F-4B29-970A-047266FE696D",@"a0bbc045-00e5-4485-88cb-69a73ac7c78d");
            // Attrib Value for Block:Admin, Attribute:Home Page Page: Admin, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("D13B8089-9AFD-4523-8E85-C89D8135A862","D46F3099-5700-4CCD-8B6C-F1F306BA02B8",@"1610b488-1ad8-44f5-aabd-96c259c02b09");
            // Attrib Value for Block:Admin, Attribute:Next Page Page: Admin, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("D13B8089-9AFD-4523-8E85-C89D8135A862","36C334EF-E723-4065-9C39-BD5663582751",@"1610b488-1ad8-44f5-aabd-96c259c02b09");
            // Attrib Value for Block:Admin, Attribute:Previous Page Page: Admin, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("D13B8089-9AFD-4523-8E85-C89D8135A862","7675AE35-1A61-460E-8FF6-B2A5C473F319",@"44b1021c-b408-495f-8496-9da333dc3c02");
            // Attrib Value for Block:Admin, Attribute:Enable Location Sharing Page: Admin, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("D13B8089-9AFD-4523-8E85-C89D8135A862","992F693A-1019-468C-B7A7-B945A616BAF0",@"False");
            // Attrib Value for Block:Admin, Attribute:Time to Cache Kiosk GeoLocation Page: Admin, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("D13B8089-9AFD-4523-8E85-C89D8135A862","C2A1F6A2-A801-4B52-8CFB-9B394CCA2D17",@"20");
            // Attrib Value for Block:Admin, Attribute:Allow Manual Setup Page: Admin, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("D13B8089-9AFD-4523-8E85-C89D8135A862","EBBC10ED-18E4-4E7D-9467-E7C27F12A745",@"True");
            // Attrib Value for Block:Admin, Attribute:Enable Kiosk Match By Name Page: Admin, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("D13B8089-9AFD-4523-8E85-C89D8135A862","0E252443-86E1-4068-8B32-9943E0974C94",@"False");



            // Page: Welcome
            RockMigrationHelper.AddPage("132DE89E-2556-45DE-A8CE-A77F4BC2EC79","23B01AB5-3AF2-46BC-8A75-BAF8CCC1C9E0","Welcome","","1610B488-1AD8-44F5-AABD-96C259C02B09",""); // Site:LCBC Check-in
            RockMigrationHelper.AddPageRoute("1610B488-1AD8-44F5-AABD-96C259C02B09","lcbc-checkin/welcome");
            RockMigrationHelper.UpdateBlockType("Welcome","Welcome screen for check-in.","~/Blocks/CheckIn/Welcome.ascx","Check-in","E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE");
            RockMigrationHelper.UpdateBlockType("Override","Override button for changing classrooms without a PIN","~/Plugins/com_bemadev/Checkin/Override.ascx","com_bemadev > Check-in","6B3F9708-16A0-4D01-B1D0-02FAD523B00F");
            RockMigrationHelper.UpdateBlockType("Roster Button","Displays a button to print rosters for location","~/Plugins/com_bemadev/Checkin/RosterButton.ascx","com_bemadev > Check-in","31308E57-85D5-4E93-BFFB-4066EB6FF90D");
            RockMigrationHelper.UpdateBlockType("Reprint Label Button","Displays a button to print rosters for location","~/Plugins/com_bemadev/Checkin/ReprintLabelButton.ascx","BEMA Services > Check-in","6781F30C-6F34-4E45-93A5-42DD7CD2132D");
            // Add Block to Page: Welcome, Site: LCBC Check-in
            RockMigrationHelper.AddBlock( true, "1610B488-1AD8-44F5-AABD-96C259C02B09","","E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE","Welcome","Main",@"
<script>
function startLocationListener() {
    var mutationObserver = new MutationObserver(function (mutations) {
        mutations.forEach(function (mutation) {
            for (var i = 0; i < mutation.addedNodes.length; i++) {
                if ($(""[id$='btnScheduleLocations']"").is(':visible')) {
                    $(""[id$='btnScheduleLocations']"").hide();
                }
            }
        });
    }); mutationObserver.observe(document.documentElement, {
        childList: true, subtree: true
    });
}

$(document).ready(function () {
    startLocationListener();
});
</script>","",0,"CD73D739-800B-472D-899A-6186495982B2"); 
            // Add Block to Page: Welcome, Site: LCBC Check-in
            RockMigrationHelper.AddBlock( true, "1610B488-1AD8-44F5-AABD-96C259C02B09","","6B3F9708-16A0-4D01-B1D0-02FAD523B00F","Override","Main","","",1,"07BB34B0-C354-4156-AED8-9BEE28AAC058"); 
            // Add Block to Page: Welcome, Site: LCBC Check-in
            RockMigrationHelper.AddBlock( true, "1610B488-1AD8-44F5-AABD-96C259C02B09","","31308E57-85D5-4E93-BFFB-4066EB6FF90D","Roster Button","Main","","",2,"C98A14C5-1D9D-4352-A257-007AC9AF7495"); 
            // Add Block to Page: Welcome, Site: LCBC Check-in
            RockMigrationHelper.AddBlock( true, "1610B488-1AD8-44F5-AABD-96C259C02B09","","6781F30C-6F34-4E45-93A5-42DD7CD2132D","Reprint Label Button","Main","","",3,"EF55E484-B4AD-44AA-BE0A-2D64E250B709"); 
            // Attrib for BlockType: Welcome:Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute("E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE","46A03F59-55D3-4ACE-ADD5-B4642225DD20","Workflow Type","WorkflowType","","The workflow type to activate for check-in",0,@"","9DBAC218-2498-4B94-B40D-45516C477C07");
            // Attrib for BlockType: Override:Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute("6B3F9708-16A0-4D01-B1D0-02FAD523B00F","46A03F59-55D3-4ACE-ADD5-B4642225DD20","Workflow Type","WorkflowType","","The workflow type to activate for check-in",0,@"","61CA26C0-BC7B-4C99-BBCF-557A5C451808");
            // Attrib for BlockType: Roster Button:Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute("31308E57-85D5-4E93-BFFB-4066EB6FF90D","46A03F59-55D3-4ACE-ADD5-B4642225DD20","Workflow Type","WorkflowType","","The workflow type to activate for check-in",0,@"","01F5204F-4806-4E42-B3CB-F17422476A52");
            // Attrib for BlockType: Reprint Label Button:Reprint Label Page
            RockMigrationHelper.UpdateBlockTypeAttribute("6781F30C-6F34-4E45-93A5-42DD7CD2132D","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Reprint Label Page","ReprintLabelPage","","",0,@"","FF39512E-D345-416A-AA83-69A83355B01A");
            // Attrib for BlockType: Reprint Label Button:Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute("6781F30C-6F34-4E45-93A5-42DD7CD2132D","46A03F59-55D3-4ACE-ADD5-B4642225DD20","Workflow Type","WorkflowType","","The workflow type to activate for check-in",0,@"","EB9C9C0B-B811-43D6-9CE8-23925074983A");
            // Attrib for BlockType: Reprint Label Button:Workflow Activity
            RockMigrationHelper.UpdateBlockTypeAttribute("6781F30C-6F34-4E45-93A5-42DD7CD2132D","9C204CD0-1233-41C5-818A-C5DA439445AA","Workflow Activity","WorkflowActivity","","The name of the workflow activity to run on selection.",1,@"","CA33C5AE-963B-430C-83A4-1D8C2C849320");
            // Attrib for BlockType: Roster Button:Workflow Activity
            RockMigrationHelper.UpdateBlockTypeAttribute("31308E57-85D5-4E93-BFFB-4066EB6FF90D","9C204CD0-1233-41C5-818A-C5DA439445AA","Workflow Activity","WorkflowActivity","","The name of the workflow activity to run on selection.",1,@"","3378C278-6E4A-46B5-A72B-30D23D9059DE");
            // Attrib for BlockType: Override:Workflow Activity
            RockMigrationHelper.UpdateBlockTypeAttribute("6B3F9708-16A0-4D01-B1D0-02FAD523B00F","9C204CD0-1233-41C5-818A-C5DA439445AA","Workflow Activity","WorkflowActivity","","The name of the workflow activity to run on selection.",1,@"","7F9D96B4-B667-454F-93DB-8806841E88BC");
            // Attrib for BlockType: Welcome:Workflow Activity
            RockMigrationHelper.UpdateBlockTypeAttribute("E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE","9C204CD0-1233-41C5-818A-C5DA439445AA","Workflow Activity","WorkflowActivity","","The name of the workflow activity to run on selection.",1,@"","169D731D-D7EE-42E6-9D5B-62E33E847A16");
            // Attrib for BlockType: Welcome:Home Page
            RockMigrationHelper.UpdateBlockTypeAttribute("E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Home Page","HomePage","","",2,@"","E7F05E05-3FAA-4332-9E06-2D69F35CA6D7");
            // Attrib for BlockType: Override:Home Page
            RockMigrationHelper.UpdateBlockTypeAttribute("6B3F9708-16A0-4D01-B1D0-02FAD523B00F","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Home Page","HomePage","","",2,@"","4E018CB8-8D5D-45EB-B257-EC5D36377CA7");
            // Attrib for BlockType: Roster Button:Home Page
            RockMigrationHelper.UpdateBlockTypeAttribute("31308E57-85D5-4E93-BFFB-4066EB6FF90D","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Home Page","HomePage","","",2,@"","68C1E145-01A8-4842-99D3-E91E5DBC2E8A");
            // Attrib for BlockType: Reprint Label Button:Home Page
            RockMigrationHelper.UpdateBlockTypeAttribute("6781F30C-6F34-4E45-93A5-42DD7CD2132D","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Home Page","HomePage","","",2,@"","71976F89-A374-4156-BBB2-4C3CF192E555");
            // Attrib for BlockType: Reprint Label Button:Previous Page
            RockMigrationHelper.UpdateBlockTypeAttribute("6781F30C-6F34-4E45-93A5-42DD7CD2132D","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Previous Page","PreviousPage","","",3,@"","2AEAF491-1241-47B5-8639-E3AEBDB26F90");
            // Attrib for BlockType: Roster Button:Previous Page
            RockMigrationHelper.UpdateBlockTypeAttribute("31308E57-85D5-4E93-BFFB-4066EB6FF90D","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Previous Page","PreviousPage","","",3,@"","0AA8150B-579F-456D-8BC6-0A16421226D7");
            // Attrib for BlockType: Override:Previous Page
            RockMigrationHelper.UpdateBlockTypeAttribute("6B3F9708-16A0-4D01-B1D0-02FAD523B00F","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Previous Page","PreviousPage","","",3,@"","16F1CDF4-E11D-4611-866F-163AB00D4D69");
            // Attrib for BlockType: Welcome:Previous Page
            RockMigrationHelper.UpdateBlockTypeAttribute("E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Previous Page","PreviousPage","","",3,@"","7F4B3918-25F4-4F36-B7BA-645AA8DA7F47");
            // Attrib for BlockType: Welcome:Next Page
            RockMigrationHelper.UpdateBlockTypeAttribute("E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Next Page","NextPage","","",4,@"","11D60BFC-383E-452D-8DC3-6575B54D8D23");
            // Attrib for BlockType: Override:Next Page
            RockMigrationHelper.UpdateBlockTypeAttribute("6B3F9708-16A0-4D01-B1D0-02FAD523B00F","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Next Page","NextPage","","",4,@"","7E4CF473-A9C3-451D-AD69-19A2D834C0A0");
            // Attrib for BlockType: Roster Button:Next Page
            RockMigrationHelper.UpdateBlockTypeAttribute("31308E57-85D5-4E93-BFFB-4066EB6FF90D","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Next Page","NextPage","","",4,@"","E785339B-3A10-42A0-9184-235DFEF33615");
            // Attrib for BlockType: Reprint Label Button:Next Page
            RockMigrationHelper.UpdateBlockTypeAttribute("6781F30C-6F34-4E45-93A5-42DD7CD2132D","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Next Page","NextPage","","",4,@"","69481D3C-079C-4283-9A30-CB4E78BB9CFF");
            // Attrib for BlockType: Reprint Label Button:Multi-Person First Page (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute("6781F30C-6F34-4E45-93A5-42DD7CD2132D","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Multi-Person First Page (Family Check-in)","MultiPersonFirstPage","","The first page for each person during family check-in.",5,@"","565214B3-80B8-4C4F-B7FF-C3649F06BBCC");
            // Attrib for BlockType: Roster Button:Multi-Person First Page (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute("31308E57-85D5-4E93-BFFB-4066EB6FF90D","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Multi-Person First Page (Family Check-in)","MultiPersonFirstPage","","The first page for each person during family check-in.",5,@"","059BB4A2-8F81-4F5D-847E-87FD0D4C4E33");
            // Attrib for BlockType: Welcome:Family Select Page
            RockMigrationHelper.UpdateBlockTypeAttribute("E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Family Select Page","FamilySelectPage","","",5,@"","A637E55F-830E-49AE-8924-E4103E6B9DB2");
            // Attrib for BlockType: Welcome:Scheduled Locations Page
            RockMigrationHelper.UpdateBlockTypeAttribute("E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Scheduled Locations Page","ScheduledLocationsPage","","",6,@"","C79544CC-B79A-4D05-BFC2-DF78CCC3D4F4");
            // Attrib for BlockType: Roster Button:Multi-Person Last Page  (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute("31308E57-85D5-4E93-BFFB-4066EB6FF90D","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Multi-Person Last Page  (Family Check-in)","MultiPersonLastPage","","The last page for each person during family check-in.",6,@"","DC541F93-7E99-4C9F-BCB6-7A41BC4BEA25");
            // Attrib for BlockType: Reprint Label Button:Multi-Person Last Page  (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute("6781F30C-6F34-4E45-93A5-42DD7CD2132D","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Multi-Person Last Page  (Family Check-in)","MultiPersonLastPage","","The last page for each person during family check-in.",6,@"","5E85FA42-F467-49DB-B61F-D85108BC7FED");
            // Attrib for BlockType: Reprint Label Button:Multi-Person Done Page (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute("6781F30C-6F34-4E45-93A5-42DD7CD2132D","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Multi-Person Done Page (Family Check-in)","MultiPersonDonePage","","The page to navigate to once all people have checked in during family check-in.",7,@"","56DAEDFB-0A8A-4533-B06C-D888D409D7D6");
            // Attrib for BlockType: Roster Button:Multi-Person Done Page (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute("31308E57-85D5-4E93-BFFB-4066EB6FF90D","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Multi-Person Done Page (Family Check-in)","MultiPersonDonePage","","The page to navigate to once all people have checked in during family check-in.",7,@"","1D0AD768-EF42-4C82-937B-CDCCCAA9183C");
            // Attrib for BlockType: Welcome:Not Active Title
            RockMigrationHelper.UpdateBlockTypeAttribute("E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE","9C204CD0-1233-41C5-818A-C5DA439445AA","Not Active Title","NotActiveTitle","","Title displayed when there are not any active options today.",7,@"Check-in Is Not Active","32470663-4CCE-4FA8-9AAC-CF7B5C6346D1");
            // Attrib for BlockType: Welcome:Not Active Caption
            RockMigrationHelper.UpdateBlockTypeAttribute("E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE","9C204CD0-1233-41C5-818A-C5DA439445AA","Not Active Caption","NotActiveCaption","","Caption displayed when there are not any active options today.",8,@"There are no current or future schedules for this kiosk today!","1D832AB9-DA71-47B3-B4E8-6661A316BD7B");
            // Attrib for BlockType: Welcome:Not Active Yet Title
            RockMigrationHelper.UpdateBlockTypeAttribute("E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE","9C204CD0-1233-41C5-818A-C5DA439445AA","Not Active Yet Title","NotActiveYetTitle","","Title displayed when there are active options today, but none are active now.",9,@"Check-in Is Not Active Yet","151B4CD4-C5CA-47F0-B2EB-348408FC8AE1");
            // Attrib for BlockType: Welcome:Not Active Yet Caption
            RockMigrationHelper.UpdateBlockTypeAttribute("E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE","9C204CD0-1233-41C5-818A-C5DA439445AA","Not Active Yet Caption","NotActiveYetCaption","","Caption displayed when there are active options today, but none are active now. Use {0} for a countdown timer.",10,@"This kiosk is not active yet.  Countdown until active: {0}.","4ACAEE3E-1388-485E-A641-07C69562D317");
            // Attrib for BlockType: Welcome:Closed Title
            RockMigrationHelper.UpdateBlockTypeAttribute("E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE","9C204CD0-1233-41C5-818A-C5DA439445AA","Closed Title","ClosedTitle","","",11,@"Closed","C4642C7B-049C-4A00-A2EE-1ABE7EF05E61");
            // Attrib for BlockType: Welcome:Closed Caption
            RockMigrationHelper.UpdateBlockTypeAttribute("E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE","9C204CD0-1233-41C5-818A-C5DA439445AA","Closed Caption","ClosedCaption","","",12,@"This location is currently closed.","4AC7E21F-7805-4E43-9301-597E45EA5211");
            // Attrib for BlockType: Welcome:Check-in Button Text
            RockMigrationHelper.UpdateBlockTypeAttribute("E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE","9C204CD0-1233-41C5-818A-C5DA439445AA","Check-in Button Text","CheckinButtonText","","The text to display on the check-in button. Defaults to 'Start' if left blank.",13,@"","C211328D-3F66-4F5D-902A-2A7AF1985209");
            // Attrib for BlockType: Welcome:No Option Caption
            RockMigrationHelper.UpdateBlockTypeAttribute("E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE","9C204CD0-1233-41C5-818A-C5DA439445AA","No Option Caption","NoOptionCaption","","The text to display when there are not any families found matching a scanned identifier (barcode, etc).",14,@"Sorry, there were not any families found with the selected identifier.","35727C8E-71A2-4272-ACB3-5D407194D728");
            // Attrib Value for Block:Welcome, Attribute:Workflow Type Page: Welcome, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("CD73D739-800B-472D-899A-6186495982B2","9DBAC218-2498-4B94-B40D-45516C477C07",@"a0bbc045-00e5-4485-88cb-69a73ac7c78d");
            // Attrib Value for Block:Welcome, Attribute:Home Page Page: Welcome, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("CD73D739-800B-472D-899A-6186495982B2","E7F05E05-3FAA-4332-9E06-2D69F35CA6D7",@"1610b488-1ad8-44f5-aabd-96c259c02b09");
            // Attrib Value for Block:Welcome, Attribute:Next Page Page: Welcome, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("CD73D739-800B-472D-899A-6186495982B2","11D60BFC-383E-452D-8DC3-6575B54D8D23",@"9ea706a9-31ca-47d4-b819-3b29a5ea7fc2");
            // Attrib Value for Block:Welcome, Attribute:Previous Page Page: Welcome, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("CD73D739-800B-472D-899A-6186495982B2","7F4B3918-25F4-4F36-B7BA-645AA8DA7F47",@"44b1021c-b408-495f-8496-9da333dc3c02");
            // Attrib Value for Block:Welcome, Attribute:Family Select Page Page: Welcome, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("CD73D739-800B-472D-899A-6186495982B2","A637E55F-830E-49AE-8924-E4103E6B9DB2",@"f8caf8f2-081b-4b45-bd45-1aa16de1ad81");
            // Attrib Value for Block:Welcome, Attribute:Not Active Yet Caption Page: Welcome, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("CD73D739-800B-472D-899A-6186495982B2","4ACAEE3E-1388-485E-A641-07C69562D317",@"This kiosk is not active yet.  Countdown until active: {0}.");
            // Attrib Value for Block:Welcome, Attribute:Not Active Yet Title Page: Welcome, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("CD73D739-800B-472D-899A-6186495982B2","151B4CD4-C5CA-47F0-B2EB-348408FC8AE1",@"Check-in Is Not Active Yet");
            // Attrib Value for Block:Welcome, Attribute:No Option Caption Page: Welcome, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("CD73D739-800B-472D-899A-6186495982B2","35727C8E-71A2-4272-ACB3-5D407194D728",@"Sorry, there were not any families found with the selected identifier.");
            // Attrib Value for Block:Welcome, Attribute:Not Active Title Page: Welcome, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("CD73D739-800B-472D-899A-6186495982B2","32470663-4CCE-4FA8-9AAC-CF7B5C6346D1",@"Check-in Is Not Active");
            // Attrib Value for Block:Welcome, Attribute:Not Active Caption Page: Welcome, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("CD73D739-800B-472D-899A-6186495982B2","1D832AB9-DA71-47B3-B4E8-6661A316BD7B",@"There are no current or future schedules for this kiosk today!");
            // Attrib Value for Block:Welcome, Attribute:Closed Title Page: Welcome, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("CD73D739-800B-472D-899A-6186495982B2","C4642C7B-049C-4A00-A2EE-1ABE7EF05E61",@"Closed");
            // Attrib Value for Block:Welcome, Attribute:Closed Caption Page: Welcome, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("CD73D739-800B-472D-899A-6186495982B2","4AC7E21F-7805-4E43-9301-597E45EA5211",@"This location is currently closed.");
            // Attrib Value for Block:Override, Attribute:Workflow Type Page: Welcome, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("07BB34B0-C354-4156-AED8-9BEE28AAC058","61CA26C0-BC7B-4C99-BBCF-557A5C451808",@"a0bbc045-00e5-4485-88cb-69a73ac7c78d");
            // Attrib Value for Block:Override, Attribute:Home Page Page: Welcome, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("07BB34B0-C354-4156-AED8-9BEE28AAC058","4E018CB8-8D5D-45EB-B257-EC5D36377CA7",@"1610b488-1ad8-44f5-aabd-96c259c02b09");
            // Attrib Value for Block:Override, Attribute:Previous Page Page: Welcome, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("07BB34B0-C354-4156-AED8-9BEE28AAC058","16F1CDF4-E11D-4611-866F-163AB00D4D69",@"44b1021c-b408-495f-8496-9da333dc3c02");
            // Attrib Value for Block:Override, Attribute:Next Page Page: Welcome, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("07BB34B0-C354-4156-AED8-9BEE28AAC058","7E4CF473-A9C3-451D-AD69-19A2D834C0A0",@"9ea706a9-31ca-47d4-b819-3b29a5ea7fc2");
            // Attrib Value for Block:Reprint Label Button, Attribute:Reprint Label Page Page: Welcome, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("EF55E484-B4AD-44AA-BE0A-2D64E250B709","FF39512E-D345-416A-AA83-69A83355B01A",@"30af1742-361e-4aab-a7b8-e258095aee61");
            // Attrib Value for Block:Reprint Label Button, Attribute:Workflow Type Page: Welcome, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("EF55E484-B4AD-44AA-BE0A-2D64E250B709","EB9C9C0B-B811-43D6-9CE8-23925074983A",@"a0bbc045-00e5-4485-88cb-69a73ac7c78d");



            // Page: Search
            RockMigrationHelper.AddPage("132DE89E-2556-45DE-A8CE-A77F4BC2EC79","23B01AB5-3AF2-46BC-8A75-BAF8CCC1C9E0","Search","","9EA706A9-31CA-47D4-B819-3B29A5EA7FC2",""); // Site:LCBC Check-in
            RockMigrationHelper.AddPageRoute("9EA706A9-31CA-47D4-B819-3B29A5EA7FC2","lcbc-checkin/search");
            RockMigrationHelper.UpdateBlockType("Search","Displays keypad for searching on phone numbers.","~/Blocks/CheckIn/Search.ascx","Check-in","E3A99534-6FD9-49AD-AC52-32D53B2CEDD7");
            RockMigrationHelper.UpdateBlockType("Idle Redirect","Redirects user to a new url after a specific number of idle seconds.","~/Blocks/Utility/IdleRedirect.ascx","Utility","49FC4B38-741E-4B0B-B395-7C1929340D88");
            RockMigrationHelper.UpdateBlockType("Edit Family","Block to Add or Edit a Family during the Check-in Process.","~/Blocks/CheckIn/EditFamily.ascx","Check-in","06DF448A-684E-4B64-8E1B-EA1727BA9233");
            // Add Block to Page: Search, Site: LCBC Check-in
            RockMigrationHelper.AddBlock( true, "9EA706A9-31CA-47D4-B819-3B29A5EA7FC2","","E3A99534-6FD9-49AD-AC52-32D53B2CEDD7","Search","Main","","",0,"DD8EAF37-C129-4C0F-9516-16790235B02C"); 
            // Add Block to Page: Search, Site: LCBC Check-in
            RockMigrationHelper.AddBlock( true, "9EA706A9-31CA-47D4-B819-3B29A5EA7FC2","","49FC4B38-741E-4B0B-B395-7C1929340D88","Idle Redirect","Main","","",1,"3B1817AC-4FF4-4B5F-BD4D-DEBB6E6B6E1D"); 
            // Add Block to Page: Search, Site: LCBC Check-in
            RockMigrationHelper.AddBlock( true, "9EA706A9-31CA-47D4-B819-3B29A5EA7FC2","","06DF448A-684E-4B64-8E1B-EA1727BA9233","Add Family","Main","","",2,"FC2EB397-09B4-4577-8983-1369417078FB"); 
            // Attrib for BlockType: Search:Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute("E3A99534-6FD9-49AD-AC52-32D53B2CEDD7","46A03F59-55D3-4ACE-ADD5-B4642225DD20","Workflow Type","WorkflowType","","The workflow type to activate for check-in",0,@"","8BAD4223-13E5-4A53-8BBC-483D8AE9AE61");
            // Attrib for BlockType: Idle Redirect:Idle Seconds
            RockMigrationHelper.UpdateBlockTypeAttribute("49FC4B38-741E-4B0B-B395-7C1929340D88","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Idle Seconds","IdleSeconds","","How many seconds of idle time to wait before redirecting user",0,@"20","1CAC7B16-041A-4F40-8AEE-A39DFA076C14");
            // Attrib for BlockType: Idle Redirect:New Location
            RockMigrationHelper.UpdateBlockTypeAttribute("49FC4B38-741E-4B0B-B395-7C1929340D88","9C204CD0-1233-41C5-818A-C5DA439445AA","New Location","NewLocation","","The new location URL to send user to after idle time",0,@"","2254B67B-9CB1-47DE-A63D-D0B56051ECD4");
            // Attrib for BlockType: Edit Family:Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute("06DF448A-684E-4B64-8E1B-EA1727BA9233","46A03F59-55D3-4ACE-ADD5-B4642225DD20","Workflow Type","WorkflowType","","The workflow type to activate for check-in",0,@"","C7C8C51E-B5A0-49A5-96F3-CB23BB5F81AB");
            // Attrib for BlockType: Search:Workflow Activity
            RockMigrationHelper.UpdateBlockTypeAttribute("E3A99534-6FD9-49AD-AC52-32D53B2CEDD7","9C204CD0-1233-41C5-818A-C5DA439445AA","Workflow Activity","WorkflowActivity","","The name of the workflow activity to run on selection.",1,@"","EDC6A39C-211D-429A-BB1E-6156F16B4618");
            // Attrib for BlockType: Edit Family:Workflow Activity
            RockMigrationHelper.UpdateBlockTypeAttribute("06DF448A-684E-4B64-8E1B-EA1727BA9233","9C204CD0-1233-41C5-818A-C5DA439445AA","Workflow Activity","WorkflowActivity","","The name of the workflow activity to run on selection.",1,@"","9B1B49A1-716D-4B5C-A75E-D39B681207AB");
            // Attrib for BlockType: Edit Family:Home Page
            RockMigrationHelper.UpdateBlockTypeAttribute("06DF448A-684E-4B64-8E1B-EA1727BA9233","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Home Page","HomePage","","",2,@"","808EA130-EDED-45FD-9683-A5A26859128F");
            // Attrib for BlockType: Search:Home Page
            RockMigrationHelper.UpdateBlockTypeAttribute("E3A99534-6FD9-49AD-AC52-32D53B2CEDD7","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Home Page","HomePage","","",2,@"","23EA5174-A5D8-4161-94C4-F70AB827FCF1");
            // Attrib for BlockType: Edit Family:Previous Page
            RockMigrationHelper.UpdateBlockTypeAttribute("06DF448A-684E-4B64-8E1B-EA1727BA9233","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Previous Page","PreviousPage","","",3,@"","FC3382DD-647F-4C2F-A06F-3A0C274B8B95");
            // Attrib for BlockType: Search:Previous Page
            RockMigrationHelper.UpdateBlockTypeAttribute("E3A99534-6FD9-49AD-AC52-32D53B2CEDD7","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Previous Page","PreviousPage","","",3,@"","17D71A46-A1D4-4CF3-9353-5339E487BA75");
            // Attrib for BlockType: Edit Family:Next Page
            RockMigrationHelper.UpdateBlockTypeAttribute("06DF448A-684E-4B64-8E1B-EA1727BA9233","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Next Page","NextPage","","",4,@"","B9F99590-3E58-4746-9884-14D2223D00F8");
            // Attrib for BlockType: Search:Next Page
            RockMigrationHelper.UpdateBlockTypeAttribute("E3A99534-6FD9-49AD-AC52-32D53B2CEDD7","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Next Page","NextPage","","",4,@"","5B25560D-2CA2-422B-89B1-928D17005CD3");
            // Attrib for BlockType: Search:Title
            RockMigrationHelper.UpdateBlockTypeAttribute("E3A99534-6FD9-49AD-AC52-32D53B2CEDD7","9C204CD0-1233-41C5-818A-C5DA439445AA","Title","Title","","Title to display. Use {0} for search type.",5,@"Search","837B34E3-D140-44CD-8456-9D222325E42E");
            // Attrib for BlockType: Search:No Option Message
            RockMigrationHelper.UpdateBlockTypeAttribute("E3A99534-6FD9-49AD-AC52-32D53B2CEDD7","9C204CD0-1233-41C5-818A-C5DA439445AA","No Option Message","NoOptionMessage","","",6,@"There were not any families that match the search criteria.","E4AFD216-5386-4E38-B98F-9436601F7B1B");
            // Attrib Value for Block:Search, Attribute:Workflow Type Page: Search, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("DD8EAF37-C129-4C0F-9516-16790235B02C","8BAD4223-13E5-4A53-8BBC-483D8AE9AE61",@"a0bbc045-00e5-4485-88cb-69a73ac7c78d");
            // Attrib Value for Block:Search, Attribute:Home Page Page: Search, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("DD8EAF37-C129-4C0F-9516-16790235B02C","23EA5174-A5D8-4161-94C4-F70AB827FCF1",@"1610b488-1ad8-44f5-aabd-96c259c02b09");
            // Attrib Value for Block:Search, Attribute:Next Page Page: Search, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("DD8EAF37-C129-4C0F-9516-16790235B02C","5B25560D-2CA2-422B-89B1-928D17005CD3",@"f8caf8f2-081b-4b45-bd45-1aa16de1ad81");
            // Attrib Value for Block:Search, Attribute:Previous Page Page: Search, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("DD8EAF37-C129-4C0F-9516-16790235B02C","17D71A46-A1D4-4CF3-9353-5339E487BA75",@"1610b488-1ad8-44f5-aabd-96c259c02b09");
            // Attrib Value for Block:Search, Attribute:Workflow Activity Page: Search, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("DD8EAF37-C129-4C0F-9516-16790235B02C","EDC6A39C-211D-429A-BB1E-6156F16B4618",@"Family Search");
            // Attrib Value for Block:Search, Attribute:No Option Message Page: Search, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("DD8EAF37-C129-4C0F-9516-16790235B02C","E4AFD216-5386-4E38-B98F-9436601F7B1B",@"There were not any families that match the search criteria.");
            // Attrib Value for Block:Search, Attribute:Title Page: Search, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("DD8EAF37-C129-4C0F-9516-16790235B02C","837B34E3-D140-44CD-8456-9D222325E42E",@"Search");
            // Attrib Value for Block:Idle Redirect, Attribute:Idle Seconds Page: Search, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("3B1817AC-4FF4-4B5F-BD4D-DEBB6E6B6E1D","1CAC7B16-041A-4F40-8AEE-A39DFA076C14",@"45");
            // Attrib Value for Block:Idle Redirect, Attribute:New Location Page: Search, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("3B1817AC-4FF4-4B5F-BD4D-DEBB6E6B6E1D","2254B67B-9CB1-47DE-A63D-D0B56051ECD4",@"/lcbc-checkin/welcome");
            // Attrib Value for Block:Add Family, Attribute:Workflow Activity Page: Search, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("FC2EB397-09B4-4577-8983-1369417078FB","9B1B49A1-716D-4B5C-A75E-D39B681207AB",@"Family Search");
            // Attrib Value for Block:Add Family, Attribute:Home Page Page: Search, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("FC2EB397-09B4-4577-8983-1369417078FB","808EA130-EDED-45FD-9683-A5A26859128F",@"1610b488-1ad8-44f5-aabd-96c259c02b09");
            // Attrib Value for Block:Add Family, Attribute:Workflow Type Page: Search, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("FC2EB397-09B4-4577-8983-1369417078FB","C7C8C51E-B5A0-49A5-96F3-CB23BB5F81AB",@"a0bbc045-00e5-4485-88cb-69a73ac7c78d");



            // Page: Family Select
            RockMigrationHelper.AddPage("132DE89E-2556-45DE-A8CE-A77F4BC2EC79","23B01AB5-3AF2-46BC-8A75-BAF8CCC1C9E0","Family Select","","F8CAF8F2-081B-4B45-BD45-1AA16DE1AD81",""); // Site:LCBC Check-in
            RockMigrationHelper.AddPageRoute("F8CAF8F2-081B-4B45-BD45-1AA16DE1AD81","lcbc-checkin/family");
            RockMigrationHelper.UpdateBlockType("Family Select","Displays a list of families to select for checkin.","~/Blocks/CheckIn/FamilySelect.ascx","Check-in","6B050E12-A232-41F6-94C5-B190F4520607");
            RockMigrationHelper.UpdateBlockType("Idle Redirect","Redirects user to a new url after a specific number of idle seconds.","~/Blocks/Utility/IdleRedirect.ascx","Utility","49FC4B38-741E-4B0B-B395-7C1929340D88");
            RockMigrationHelper.UpdateBlockType("Edit Family","Block to Add or Edit a Family during the Check-in Process.","~/Blocks/CheckIn/EditFamily.ascx","Check-in","06DF448A-684E-4B64-8E1B-EA1727BA9233");
            // Add Block to Page: Family Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlock( true, "F8CAF8F2-081B-4B45-BD45-1AA16DE1AD81","","6B050E12-A232-41F6-94C5-B190F4520607","Family Select","Main","","",0,"A4861138-D855-4BCC-83B9-4549AD92D347"); 
            // Add Block to Page: Family Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlock( true, "F8CAF8F2-081B-4B45-BD45-1AA16DE1AD81","","49FC4B38-741E-4B0B-B395-7C1929340D88","Idle Redirect","Main","","",1,"9B31FCF7-DA84-4E51-BE5F-DCE9182B6106"); 
            // Add Block to Page: Family Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlock( true, "F8CAF8F2-081B-4B45-BD45-1AA16DE1AD81","","06DF448A-684E-4B64-8E1B-EA1727BA9233","Add Family","Main","","",2,"C1815AC4-BA41-4867-AFEE-8FFDD4D068E6"); 
            // Attrib for BlockType: Family Select:Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute("6B050E12-A232-41F6-94C5-B190F4520607","46A03F59-55D3-4ACE-ADD5-B4642225DD20","Workflow Type","WorkflowType","","The workflow type to activate for check-in",0,@"","1701FD5B-4B65-4AC1-845C-3AA31DE621AE");
            // Attrib for BlockType: Idle Redirect:Idle Seconds
            RockMigrationHelper.UpdateBlockTypeAttribute("49FC4B38-741E-4B0B-B395-7C1929340D88","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Idle Seconds","IdleSeconds","","How many seconds of idle time to wait before redirecting user",0,@"20","1CAC7B16-041A-4F40-8AEE-A39DFA076C14");
            // Attrib for BlockType: Idle Redirect:New Location
            RockMigrationHelper.UpdateBlockTypeAttribute("49FC4B38-741E-4B0B-B395-7C1929340D88","9C204CD0-1233-41C5-818A-C5DA439445AA","New Location","NewLocation","","The new location URL to send user to after idle time",0,@"","2254B67B-9CB1-47DE-A63D-D0B56051ECD4");
            // Attrib for BlockType: Edit Family:Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute("06DF448A-684E-4B64-8E1B-EA1727BA9233","46A03F59-55D3-4ACE-ADD5-B4642225DD20","Workflow Type","WorkflowType","","The workflow type to activate for check-in",0,@"","C7C8C51E-B5A0-49A5-96F3-CB23BB5F81AB");
            // Attrib for BlockType: Edit Family:Workflow Activity
            RockMigrationHelper.UpdateBlockTypeAttribute("06DF448A-684E-4B64-8E1B-EA1727BA9233","9C204CD0-1233-41C5-818A-C5DA439445AA","Workflow Activity","WorkflowActivity","","The name of the workflow activity to run on selection.",1,@"","9B1B49A1-716D-4B5C-A75E-D39B681207AB");
            // Attrib for BlockType: Family Select:Workflow Activity
            RockMigrationHelper.UpdateBlockTypeAttribute("6B050E12-A232-41F6-94C5-B190F4520607","9C204CD0-1233-41C5-818A-C5DA439445AA","Workflow Activity","WorkflowActivity","","The name of the workflow activity to run on selection.",1,@"","B602450A-0C1F-401A-87BC-9A804461E887");
            // Attrib for BlockType: Family Select:Home Page
            RockMigrationHelper.UpdateBlockTypeAttribute("6B050E12-A232-41F6-94C5-B190F4520607","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Home Page","HomePage","","",2,@"","47EE6878-36A1-4A55-A634-584ADD852822");
            // Attrib for BlockType: Edit Family:Home Page
            RockMigrationHelper.UpdateBlockTypeAttribute("06DF448A-684E-4B64-8E1B-EA1727BA9233","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Home Page","HomePage","","",2,@"","808EA130-EDED-45FD-9683-A5A26859128F");
            // Attrib for BlockType: Edit Family:Previous Page
            RockMigrationHelper.UpdateBlockTypeAttribute("06DF448A-684E-4B64-8E1B-EA1727BA9233","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Previous Page","PreviousPage","","",3,@"","FC3382DD-647F-4C2F-A06F-3A0C274B8B95");
            // Attrib for BlockType: Family Select:Previous Page
            RockMigrationHelper.UpdateBlockTypeAttribute("6B050E12-A232-41F6-94C5-B190F4520607","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Previous Page","PreviousPage","","",3,@"","2578A0C1-3236-409D-AECE-154C98429628");
            // Attrib for BlockType: Edit Family:Next Page
            RockMigrationHelper.UpdateBlockTypeAttribute("06DF448A-684E-4B64-8E1B-EA1727BA9233","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Next Page","NextPage","","",4,@"","B9F99590-3E58-4746-9884-14D2223D00F8");
            // Attrib for BlockType: Family Select:Next Page
            RockMigrationHelper.UpdateBlockTypeAttribute("6B050E12-A232-41F6-94C5-B190F4520607","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Next Page","NextPage","","",4,@"","90ECD00A-9570-4986-B32F-02F32B656A2A");
            // Attrib for BlockType: Family Select:Title
            RockMigrationHelper.UpdateBlockTypeAttribute("6B050E12-A232-41F6-94C5-B190F4520607","9C204CD0-1233-41C5-818A-C5DA439445AA","Title","Title","","Title to display.",5,@"Families","AF6A6A8B-981A-4ACB-A42C-1D576917C724");
            // Attrib for BlockType: Family Select:Caption
            RockMigrationHelper.UpdateBlockTypeAttribute("6B050E12-A232-41F6-94C5-B190F4520607","9C204CD0-1233-41C5-818A-C5DA439445AA","Caption","Caption","","Caption to display.",6,@"Select Your Family","F1911A0A-AC69-474F-9D99-A5022D6E129C");
            // Attrib for BlockType: Family Select:No Option Message
            RockMigrationHelper.UpdateBlockTypeAttribute("6B050E12-A232-41F6-94C5-B190F4520607","9C204CD0-1233-41C5-818A-C5DA439445AA","No Option Message","NoOptionMessage","","Text to display when there is not anyone in the family that can check-in",7,@"Sorry, no one in your family is eligible to check-in at this location.","E2C9760F-D5CB-475B-BEDD-E2E249CAB1AF");
            // Attrib Value for Block:Family Select, Attribute:Workflow Type Page: Family Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("A4861138-D855-4BCC-83B9-4549AD92D347","1701FD5B-4B65-4AC1-845C-3AA31DE621AE",@"a0bbc045-00e5-4485-88cb-69a73ac7c78d");
            // Attrib Value for Block:Family Select, Attribute:Home Page Page: Family Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("A4861138-D855-4BCC-83B9-4549AD92D347","47EE6878-36A1-4A55-A634-584ADD852822",@"1610b488-1ad8-44f5-aabd-96c259c02b09");
            // Attrib Value for Block:Family Select, Attribute:Next Page Page: Family Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("A4861138-D855-4BCC-83B9-4549AD92D347","90ECD00A-9570-4986-B32F-02F32B656A2A",@"cc38ad88-b319-40ab-b9e7-d04f563224a5");
            // Attrib Value for Block:Family Select, Attribute:Previous Page Page: Family Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("A4861138-D855-4BCC-83B9-4549AD92D347","2578A0C1-3236-409D-AECE-154C98429628",@"9ea706a9-31ca-47d4-b819-3b29a5ea7fc2");
            // Attrib Value for Block:Family Select, Attribute:Workflow Activity Page: Family Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("A4861138-D855-4BCC-83B9-4549AD92D347","B602450A-0C1F-401A-87BC-9A804461E887",@"Person Search");
            // Attrib Value for Block:Family Select, Attribute:Caption Page: Family Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("A4861138-D855-4BCC-83B9-4549AD92D347","F1911A0A-AC69-474F-9D99-A5022D6E129C",@"Select Your Family");
            // Attrib Value for Block:Family Select, Attribute:Title Page: Family Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("A4861138-D855-4BCC-83B9-4549AD92D347","AF6A6A8B-981A-4ACB-A42C-1D576917C724",@"Families");
            // Attrib Value for Block:Family Select, Attribute:No Option Message Page: Family Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("A4861138-D855-4BCC-83B9-4549AD92D347","E2C9760F-D5CB-475B-BEDD-E2E249CAB1AF",@"Sorry, no one in your family is eligible to check-in at this location.");
            // Attrib Value for Block:Idle Redirect, Attribute:Idle Seconds Page: Family Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("9B31FCF7-DA84-4E51-BE5F-DCE9182B6106","1CAC7B16-041A-4F40-8AEE-A39DFA076C14",@"45");
            // Attrib Value for Block:Idle Redirect, Attribute:New Location Page: Family Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("9B31FCF7-DA84-4E51-BE5F-DCE9182B6106","2254B67B-9CB1-47DE-A63D-D0B56051ECD4",@"/lcbc-checkin/welcome");
            // Attrib Value for Block:Add Family, Attribute:Workflow Activity Page: Family Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("C1815AC4-BA41-4867-AFEE-8FFDD4D068E6","9B1B49A1-716D-4B5C-A75E-D39B681207AB",@"Family Search");
            // Attrib Value for Block:Add Family, Attribute:Home Page Page: Family Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("C1815AC4-BA41-4867-AFEE-8FFDD4D068E6","808EA130-EDED-45FD-9683-A5A26859128F",@"1610b488-1ad8-44f5-aabd-96c259c02b09");
            // Attrib Value for Block:Add Family, Attribute:Workflow Type Page: Family Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("C1815AC4-BA41-4867-AFEE-8FFDD4D068E6","C7C8C51E-B5A0-49A5-96F3-CB23BB5F81AB",@"a0bbc045-00e5-4485-88cb-69a73ac7c78d");



            // Page: Person Select
            RockMigrationHelper.AddPage("132DE89E-2556-45DE-A8CE-A77F4BC2EC79","23B01AB5-3AF2-46BC-8A75-BAF8CCC1C9E0","Person Select","","4A162CDD-FB14-4599-A8F7-C5CA9011E321",""); // Site:LCBC Check-in
            RockMigrationHelper.AddPageRoute("4A162CDD-FB14-4599-A8F7-C5CA9011E321","lcbc-checkin/person");
            RockMigrationHelper.UpdateBlockType("Person Select","Lists people who match the selected family to pick to checkin.","~/Blocks/CheckIn/PersonSelect.ascx","Check-in","34B48E0F-5E37-425E-9588-E612ED34DB03");
            RockMigrationHelper.UpdateBlockType("Idle Redirect","Redirects user to a new url after a specific number of idle seconds.","~/Blocks/Utility/IdleRedirect.ascx","Utility","49FC4B38-741E-4B0B-B395-7C1929340D88");
            RockMigrationHelper.UpdateBlockType("Edit Family","Block to Add or Edit a Family during the Check-in Process.","~/Blocks/CheckIn/EditFamily.ascx","Check-in","06DF448A-684E-4B64-8E1B-EA1727BA9233");
            // Add Block to Page: Person Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlock( true, "4A162CDD-FB14-4599-A8F7-C5CA9011E321","","34B48E0F-5E37-425E-9588-E612ED34DB03","Person Select","Main","","",0,"1CF6CFD1-3722-447E-922B-CF9C375A3C3F"); 
            // Add Block to Page: Person Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlock( true, "4A162CDD-FB14-4599-A8F7-C5CA9011E321","","49FC4B38-741E-4B0B-B395-7C1929340D88","Idle Redirect","Main","","",1,"F92C0EC3-5188-47E9-9386-FC585EBC7FEC"); 
            // Add Block to Page: Person Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlock( true, "4A162CDD-FB14-4599-A8F7-C5CA9011E321","","06DF448A-684E-4B64-8E1B-EA1727BA9233","Edit Family","Main","","",2,"22D846AF-62D0-4066-97FA-16B4DAF3F7DB"); 
            // Attrib for BlockType: Idle Redirect:Idle Seconds
            RockMigrationHelper.UpdateBlockTypeAttribute("49FC4B38-741E-4B0B-B395-7C1929340D88","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Idle Seconds","IdleSeconds","","How many seconds of idle time to wait before redirecting user",0,@"20","1CAC7B16-041A-4F40-8AEE-A39DFA076C14");
            // Attrib for BlockType: Idle Redirect:New Location
            RockMigrationHelper.UpdateBlockTypeAttribute("49FC4B38-741E-4B0B-B395-7C1929340D88","9C204CD0-1233-41C5-818A-C5DA439445AA","New Location","NewLocation","","The new location URL to send user to after idle time",0,@"","2254B67B-9CB1-47DE-A63D-D0B56051ECD4");
            // Attrib for BlockType: Person Select:Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute("34B48E0F-5E37-425E-9588-E612ED34DB03","46A03F59-55D3-4ACE-ADD5-B4642225DD20","Workflow Type","WorkflowType","","The workflow type to activate for check-in",0,@"","1B455A3D-58B0-4BB9-BF25-3F2CEAF6E49F");
            // Attrib for BlockType: Edit Family:Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute("06DF448A-684E-4B64-8E1B-EA1727BA9233","46A03F59-55D3-4ACE-ADD5-B4642225DD20","Workflow Type","WorkflowType","","The workflow type to activate for check-in",0,@"","C7C8C51E-B5A0-49A5-96F3-CB23BB5F81AB");
            // Attrib for BlockType: Edit Family:Workflow Activity
            RockMigrationHelper.UpdateBlockTypeAttribute("06DF448A-684E-4B64-8E1B-EA1727BA9233","9C204CD0-1233-41C5-818A-C5DA439445AA","Workflow Activity","WorkflowActivity","","The name of the workflow activity to run on selection.",1,@"","9B1B49A1-716D-4B5C-A75E-D39B681207AB");
            // Attrib for BlockType: Person Select:Workflow Activity
            RockMigrationHelper.UpdateBlockTypeAttribute("34B48E0F-5E37-425E-9588-E612ED34DB03","9C204CD0-1233-41C5-818A-C5DA439445AA","Workflow Activity","WorkflowActivity","","The name of the workflow activity to run on selection.",1,@"","3747242A-6852-4B60-BB63-49DEB8A20CF1");
            // Attrib for BlockType: Edit Family:Home Page
            RockMigrationHelper.UpdateBlockTypeAttribute("06DF448A-684E-4B64-8E1B-EA1727BA9233","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Home Page","HomePage","","",2,@"","808EA130-EDED-45FD-9683-A5A26859128F");
            // Attrib for BlockType: Person Select:Home Page
            RockMigrationHelper.UpdateBlockTypeAttribute("34B48E0F-5E37-425E-9588-E612ED34DB03","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Home Page","HomePage","","",2,@"","97433068-DC3F-461A-AAFB-3DF83B1E3B2F");
            // Attrib for BlockType: Person Select:Previous Page
            RockMigrationHelper.UpdateBlockTypeAttribute("34B48E0F-5E37-425E-9588-E612ED34DB03","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Previous Page","PreviousPage","","",3,@"","A3F0C33B-F380-46AF-BCDA-100F18F8889E");
            // Attrib for BlockType: Edit Family:Previous Page
            RockMigrationHelper.UpdateBlockTypeAttribute("06DF448A-684E-4B64-8E1B-EA1727BA9233","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Previous Page","PreviousPage","","",3,@"","FC3382DD-647F-4C2F-A06F-3A0C274B8B95");
            // Attrib for BlockType: Edit Family:Next Page
            RockMigrationHelper.UpdateBlockTypeAttribute("06DF448A-684E-4B64-8E1B-EA1727BA9233","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Next Page","NextPage","","",4,@"","B9F99590-3E58-4746-9884-14D2223D00F8");
            // Attrib for BlockType: Person Select:Next Page
            RockMigrationHelper.UpdateBlockTypeAttribute("34B48E0F-5E37-425E-9588-E612ED34DB03","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Next Page","NextPage","","",4,@"","F680429D-A228-43FE-A54E-927F95ACC030");
            // Attrib for BlockType: Person Select:Title
            RockMigrationHelper.UpdateBlockTypeAttribute("34B48E0F-5E37-425E-9588-E612ED34DB03","9C204CD0-1233-41C5-818A-C5DA439445AA","Title","Title","","Title to display. Use {0} for family name.",8,@"{0}","8ECB1E83-97BB-435E-BFE6-40B4A33ECC9B");
            // Attrib for BlockType: Person Select:Caption
            RockMigrationHelper.UpdateBlockTypeAttribute("34B48E0F-5E37-425E-9588-E612ED34DB03","9C204CD0-1233-41C5-818A-C5DA439445AA","Caption","Caption","","",9,@"Select Person","50595A53-E5FE-4515-9DBD-EA4B006A5AFF");
            // Attrib for BlockType: Person Select:No Option Message
            RockMigrationHelper.UpdateBlockTypeAttribute("34B48E0F-5E37-425E-9588-E612ED34DB03","9C204CD0-1233-41C5-818A-C5DA439445AA","No Option Message","NoOptionMessage","","The option to display when there are not any people that match. Use {0} for the current action ('into' or 'out of').",10,@"Sorry, there are currently not any available areas that the selected person can check {0}.","9B51C224-E03E-498F-A3A2-855FFF71A103");
            // Attrib Value for Block:Person Select, Attribute:Workflow Type Page: Person Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("1CF6CFD1-3722-447E-922B-CF9C375A3C3F","1B455A3D-58B0-4BB9-BF25-3F2CEAF6E49F",@"a0bbc045-00e5-4485-88cb-69a73ac7c78d");
            // Attrib Value for Block:Person Select, Attribute:Home Page Page: Person Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("1CF6CFD1-3722-447E-922B-CF9C375A3C3F","97433068-DC3F-461A-AAFB-3DF83B1E3B2F",@"1610b488-1ad8-44f5-aabd-96c259c02b09");
            // Attrib Value for Block:Person Select, Attribute:Next Page Page: Person Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("1CF6CFD1-3722-447E-922B-CF9C375A3C3F","F680429D-A228-43FE-A54E-927F95ACC030",@"b0799456-2597-4001-8a5b-58b0a58dd42a");
            // Attrib Value for Block:Person Select, Attribute:Previous Page Page: Person Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("1CF6CFD1-3722-447E-922B-CF9C375A3C3F","A3F0C33B-F380-46AF-BCDA-100F18F8889E",@"f8caf8f2-081b-4b45-bd45-1aa16de1ad81");
            // Attrib Value for Block:Person Select, Attribute:No Option Message Page: Person Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("1CF6CFD1-3722-447E-922B-CF9C375A3C3F","9B51C224-E03E-498F-A3A2-855FFF71A103",@"Sorry, there are currently not any available areas that the selected person can check {0}.");
            // Attrib Value for Block:Person Select, Attribute:Title Page: Person Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("1CF6CFD1-3722-447E-922B-CF9C375A3C3F","8ECB1E83-97BB-435E-BFE6-40B4A33ECC9B",@"{0}");
            // Attrib Value for Block:Person Select, Attribute:Caption Page: Person Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("1CF6CFD1-3722-447E-922B-CF9C375A3C3F","50595A53-E5FE-4515-9DBD-EA4B006A5AFF",@"Select Person");
            // Attrib Value for Block:Idle Redirect, Attribute:Idle Seconds Page: Person Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("F92C0EC3-5188-47E9-9386-FC585EBC7FEC","1CAC7B16-041A-4F40-8AEE-A39DFA076C14",@"45");
            // Attrib Value for Block:Idle Redirect, Attribute:New Location Page: Person Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("F92C0EC3-5188-47E9-9386-FC585EBC7FEC","2254B67B-9CB1-47DE-A63D-D0B56051ECD4",@"/lcbc-checkin/welcome");
            // Attrib Value for Block:Edit Family, Attribute:Workflow Activity Page: Person Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("22D846AF-62D0-4066-97FA-16B4DAF3F7DB","9B1B49A1-716D-4B5C-A75E-D39B681207AB",@"Person Search");
            // Attrib Value for Block:Edit Family, Attribute:Home Page Page: Person Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("22D846AF-62D0-4066-97FA-16B4DAF3F7DB","808EA130-EDED-45FD-9683-A5A26859128F",@"1610b488-1ad8-44f5-aabd-96c259c02b09");
            // Attrib Value for Block:Edit Family, Attribute:Workflow Type Page: Person Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("22D846AF-62D0-4066-97FA-16B4DAF3F7DB","C7C8C51E-B5A0-49A5-96F3-CB23BB5F81AB",@"a0bbc045-00e5-4485-88cb-69a73ac7c78d");



            // Page: Group Type Select
            RockMigrationHelper.AddPage("132DE89E-2556-45DE-A8CE-A77F4BC2EC79","23B01AB5-3AF2-46BC-8A75-BAF8CCC1C9E0","Group Type Select","","1F6E580F-2F38-411E-A52C-F14A89520CBD",""); // Site:LCBC Check-in
            RockMigrationHelper.AddPageRoute("1F6E580F-2F38-411E-A52C-F14A89520CBD","lcbc-checkin/grouptype");
            RockMigrationHelper.UpdateBlockType("Group Type Select","Displays a list of group types the person is configured to checkin to.","~/Blocks/CheckIn/GroupTypeSelect.ascx","Check-in","7E20E97E-63F2-413D-9C2C-16FF34023F70");
            RockMigrationHelper.UpdateBlockType("Idle Redirect","Redirects user to a new url after a specific number of idle seconds.","~/Blocks/Utility/IdleRedirect.ascx","Utility","49FC4B38-741E-4B0B-B395-7C1929340D88");
            // Add Block to Page: Group Type Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlock( true, "1F6E580F-2F38-411E-A52C-F14A89520CBD","","7E20E97E-63F2-413D-9C2C-16FF34023F70","Group Type Select","Main","","",0,"07F7E4B5-3DE3-4418-AB29-2C6E71F354F7"); 
            // Add Block to Page: Group Type Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlock( true, "1F6E580F-2F38-411E-A52C-F14A89520CBD","","49FC4B38-741E-4B0B-B395-7C1929340D88","Idle Redirect","Main","","",1,"B2E1266C-E672-4E7E-BF61-17ED9A82FF2D"); 
            // Attrib for BlockType: Group Type Select:Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute("7E20E97E-63F2-413D-9C2C-16FF34023F70","46A03F59-55D3-4ACE-ADD5-B4642225DD20","Workflow Type","WorkflowType","","The workflow type to activate for check-in",0,@"","7250DC20-6AE9-48CF-9173-74CB221AF79E");
            // Attrib for BlockType: Idle Redirect:Idle Seconds
            RockMigrationHelper.UpdateBlockTypeAttribute("49FC4B38-741E-4B0B-B395-7C1929340D88","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Idle Seconds","IdleSeconds","","How many seconds of idle time to wait before redirecting user",0,@"20","1CAC7B16-041A-4F40-8AEE-A39DFA076C14");
            // Attrib for BlockType: Idle Redirect:New Location
            RockMigrationHelper.UpdateBlockTypeAttribute("49FC4B38-741E-4B0B-B395-7C1929340D88","9C204CD0-1233-41C5-818A-C5DA439445AA","New Location","NewLocation","","The new location URL to send user to after idle time",0,@"","2254B67B-9CB1-47DE-A63D-D0B56051ECD4");
            // Attrib for BlockType: Group Type Select:Workflow Activity
            RockMigrationHelper.UpdateBlockTypeAttribute("7E20E97E-63F2-413D-9C2C-16FF34023F70","9C204CD0-1233-41C5-818A-C5DA439445AA","Workflow Activity","WorkflowActivity","","The name of the workflow activity to run on selection.",1,@"","11FB556B-3E88-4189-8E54-2B92E076F426");
            // Attrib for BlockType: Group Type Select:Home Page
            RockMigrationHelper.UpdateBlockTypeAttribute("7E20E97E-63F2-413D-9C2C-16FF34023F70","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Home Page","HomePage","","",2,@"","98DBFE23-80D5-47EB-AE3F-5381C024F23D");
            // Attrib for BlockType: Group Type Select:Previous Page
            RockMigrationHelper.UpdateBlockTypeAttribute("7E20E97E-63F2-413D-9C2C-16FF34023F70","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Previous Page","PreviousPage","","",3,@"","39D260A5-A976-4DA9-B3E0-7381E9B8F3D5");
            // Attrib for BlockType: Group Type Select:Next Page
            RockMigrationHelper.UpdateBlockTypeAttribute("7E20E97E-63F2-413D-9C2C-16FF34023F70","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Next Page","NextPage","","",4,@"","F3D66EC8-E1CF-4C28-B55A-C1F49E4633A0");
            // Attrib for BlockType: Group Type Select:Multi-Person First Page (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute("7E20E97E-63F2-413D-9C2C-16FF34023F70","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Multi-Person First Page (Family Check-in)","MultiPersonFirstPage","","The first page for each person during family check-in.",5,@"","52050592-25DF-4651-8B58-4DD7581F78A3");
            // Attrib for BlockType: Group Type Select:Multi-Person Last Page  (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute("7E20E97E-63F2-413D-9C2C-16FF34023F70","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Multi-Person Last Page  (Family Check-in)","MultiPersonLastPage","","The last page for each person during family check-in.",6,@"","D14CCB80-4155-4F64-92C7-14DDA4C53FC3");
            // Attrib for BlockType: Group Type Select:Multi-Person Done Page (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute("7E20E97E-63F2-413D-9C2C-16FF34023F70","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Multi-Person Done Page (Family Check-in)","MultiPersonDonePage","","The page to navigate to once all people have checked in during family check-in.",7,@"","E1E51DE0-7492-493B-8EBF-311DFD4925F6");
            // Attrib for BlockType: Group Type Select:Select All and Skip
            RockMigrationHelper.UpdateBlockTypeAttribute("7E20E97E-63F2-413D-9C2C-16FF34023F70","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Select All and Skip","SelectAll","","Select this option if end-user should never see screen to select group types, all group types will automatically be selected and all the groups in all types will be available.",8,@"False","41AFF704-87A8-4282-80D0-B7C40983B549");
            // Attrib for BlockType: Group Type Select:Title
            RockMigrationHelper.UpdateBlockTypeAttribute("7E20E97E-63F2-413D-9C2C-16FF34023F70","9C204CD0-1233-41C5-818A-C5DA439445AA","Title","Title","","Title to display. Use {0} for person/schedule.",9,@"{0}","9DDF3190-E07F-4964-9CDC-69AF675FCF2E");
            // Attrib for BlockType: Group Type Select:Caption
            RockMigrationHelper.UpdateBlockTypeAttribute("7E20E97E-63F2-413D-9C2C-16FF34023F70","9C204CD0-1233-41C5-818A-C5DA439445AA","Caption","Caption","","",10,@"Select Area","314506D3-FCC0-42AC-86A2-77EE921C0CCD");
            // Attrib for BlockType: Group Type Select:No Option Message
            RockMigrationHelper.UpdateBlockTypeAttribute("7E20E97E-63F2-413D-9C2C-16FF34023F70","9C204CD0-1233-41C5-818A-C5DA439445AA","No Option Message","NoOptionMessage","","Message to display when there are not any options available. Use {0} for person's name, and {1} for schedule name.",11,@"Sorry, there are currently not any available areas that {0} can check into at {1}.","444058FF-0C4D-4D2E-9FDA-6036AD572C7E");
            // Attrib for BlockType: Group Type Select:No Option After Select Message
            RockMigrationHelper.UpdateBlockTypeAttribute("7E20E97E-63F2-413D-9C2C-16FF34023F70","9C204CD0-1233-41C5-818A-C5DA439445AA","No Option After Select Message","NoOptionAfterSelectMessage","","Message to display when there are not any options available after group type is selected. Use {0} for person's name",12,@"Sorry, based on your selection, there are currently not any available times that {0} can check into.","E19DD14C-80DD-4427-B73D-58187E1BE8AD");
            // Attrib Value for Block:Group Type Select, Attribute:Select All and Skip Page: Group Type Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("07F7E4B5-3DE3-4418-AB29-2C6E71F354F7","41AFF704-87A8-4282-80D0-B7C40983B549",@"False");
            // Attrib Value for Block:Group Type Select, Attribute:Multi-Person First Page (Family Check-in) Page: Group Type Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("07F7E4B5-3DE3-4418-AB29-2C6E71F354F7","52050592-25DF-4651-8B58-4DD7581F78A3",@"b0799456-2597-4001-8a5b-58b0a58dd42a");
            // Attrib Value for Block:Group Type Select, Attribute:Multi-Person Last Page  (Family Check-in) Page: Group Type Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("07F7E4B5-3DE3-4418-AB29-2C6E71F354F7","D14CCB80-4155-4F64-92C7-14DDA4C53FC3",@"ca6b5be4-c193-4db7-8469-08429e6b2803");
            // Attrib Value for Block:Group Type Select, Attribute:Multi-Person Done Page (Family Check-in) Page: Group Type Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("07F7E4B5-3DE3-4418-AB29-2C6E71F354F7","E1E51DE0-7492-493B-8EBF-311DFD4925F6",@"b7e527b9-8c7e-4f6c-afbf-464dec3f63a4");
            // Attrib Value for Block:Group Type Select, Attribute:No Option After Select Message Page: Group Type Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("07F7E4B5-3DE3-4418-AB29-2C6E71F354F7","E19DD14C-80DD-4427-B73D-58187E1BE8AD",@"Sorry, based on your selection, there are currently not any available times that {0} can check into.");
            // Attrib Value for Block:Group Type Select, Attribute:Title Page: Group Type Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("07F7E4B5-3DE3-4418-AB29-2C6E71F354F7","9DDF3190-E07F-4964-9CDC-69AF675FCF2E",@"{0}");
            // Attrib Value for Block:Group Type Select, Attribute:Caption Page: Group Type Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("07F7E4B5-3DE3-4418-AB29-2C6E71F354F7","314506D3-FCC0-42AC-86A2-77EE921C0CCD",@"Select Area");
            // Attrib Value for Block:Group Type Select, Attribute:No Option Message Page: Group Type Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("07F7E4B5-3DE3-4418-AB29-2C6E71F354F7","444058FF-0C4D-4D2E-9FDA-6036AD572C7E",@"Sorry, there are currently not any available areas that {0} can check into at {1}.");
            // Attrib Value for Block:Group Type Select, Attribute:Home Page Page: Group Type Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("07F7E4B5-3DE3-4418-AB29-2C6E71F354F7","98DBFE23-80D5-47EB-AE3F-5381C024F23D",@"1610b488-1ad8-44f5-aabd-96c259c02b09");
            // Attrib Value for Block:Group Type Select, Attribute:Next Page Page: Group Type Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("07F7E4B5-3DE3-4418-AB29-2C6E71F354F7","F3D66EC8-E1CF-4C28-B55A-C1F49E4633A0",@"ac01b3bd-58e9-42ca-aa10-1a19abd78b3c");
            // Attrib Value for Block:Group Type Select, Attribute:Previous Page Page: Group Type Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("07F7E4B5-3DE3-4418-AB29-2C6E71F354F7","39D260A5-A976-4DA9-B3E0-7381E9B8F3D5",@"b0799456-2597-4001-8a5b-58b0a58dd42a");
            // Attrib Value for Block:Idle Redirect, Attribute:Idle Seconds Page: Group Type Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("B2E1266C-E672-4E7E-BF61-17ED9A82FF2D","1CAC7B16-041A-4F40-8AEE-A39DFA076C14",@"20");
            // Attrib Value for Block:Idle Redirect, Attribute:New Location Page: Group Type Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("B2E1266C-E672-4E7E-BF61-17ED9A82FF2D","2254B67B-9CB1-47DE-A63D-D0B56051ECD4",@"/lcbc-checkin/welcome");




            // Page: Ability Select
            RockMigrationHelper.AddPage("132DE89E-2556-45DE-A8CE-A77F4BC2EC79","23B01AB5-3AF2-46BC-8A75-BAF8CCC1C9E0","Ability Select","","B0799456-2597-4001-8A5B-58B0A58DD42A",""); // Site:LCBC Check-in
            RockMigrationHelper.AddPageRoute("B0799456-2597-4001-8A5B-58B0A58DD42A","lcbc-checkin/ability");
            RockMigrationHelper.UpdateBlockType("Ability Level Select","Check-in Ability Level Select block","~/Blocks/CheckIn/AbilityLevelSelect.ascx","Check-in","605389F5-5BC5-438F-8757-110328B0CED3");
            RockMigrationHelper.UpdateBlockType("Idle Redirect","Redirects user to a new url after a specific number of idle seconds.","~/Blocks/Utility/IdleRedirect.ascx","Utility","49FC4B38-741E-4B0B-B395-7C1929340D88");
            // Add Block to Page: Ability Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlock( true, "B0799456-2597-4001-8A5B-58B0A58DD42A","","605389F5-5BC5-438F-8757-110328B0CED3","Ability Level Select","Main","","",0,"150D1DFC-F57F-4830-85BC-D41B4E2EB0C8"); 
            // Add Block to Page: Ability Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlock( true, "B0799456-2597-4001-8A5B-58B0A58DD42A","","49FC4B38-741E-4B0B-B395-7C1929340D88","Idle Redirect","Main","","",1,"74B9D8CB-A5B4-43C8-816D-CDF3081B1B75"); 
            // Attrib for BlockType: Ability Level Select:Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute("605389F5-5BC5-438F-8757-110328B0CED3","46A03F59-55D3-4ACE-ADD5-B4642225DD20","Workflow Type","WorkflowType","","The workflow type to activate for check-in",0,@"","1ED29421-3F3B-4A5B-AC1D-FAA27B34D23E");
            // Attrib for BlockType: Idle Redirect:Idle Seconds
            RockMigrationHelper.UpdateBlockTypeAttribute("49FC4B38-741E-4B0B-B395-7C1929340D88","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Idle Seconds","IdleSeconds","","How many seconds of idle time to wait before redirecting user",0,@"20","1CAC7B16-041A-4F40-8AEE-A39DFA076C14");
            // Attrib for BlockType: Idle Redirect:New Location
            RockMigrationHelper.UpdateBlockTypeAttribute("49FC4B38-741E-4B0B-B395-7C1929340D88","9C204CD0-1233-41C5-818A-C5DA439445AA","New Location","NewLocation","","The new location URL to send user to after idle time",0,@"","2254B67B-9CB1-47DE-A63D-D0B56051ECD4");
            // Attrib for BlockType: Ability Level Select:Workflow Activity
            RockMigrationHelper.UpdateBlockTypeAttribute("605389F5-5BC5-438F-8757-110328B0CED3","9C204CD0-1233-41C5-818A-C5DA439445AA","Workflow Activity","WorkflowActivity","","The name of the workflow activity to run on selection.",1,@"","19E1825C-A722-470D-B8E6-2E96B250E39F");
            // Attrib for BlockType: Ability Level Select:Home Page
            RockMigrationHelper.UpdateBlockTypeAttribute("605389F5-5BC5-438F-8757-110328B0CED3","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Home Page","HomePage","","",2,@"","5CC185C4-2ACC-4EDD-8073-9B54A638B225");
            // Attrib for BlockType: Ability Level Select:Previous Page
            RockMigrationHelper.UpdateBlockTypeAttribute("605389F5-5BC5-438F-8757-110328B0CED3","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Previous Page","PreviousPage","","",3,@"","60572273-EC82-4F5E-9153-4ED79CAEFFE5");
            // Attrib for BlockType: Ability Level Select:Next Page
            RockMigrationHelper.UpdateBlockTypeAttribute("605389F5-5BC5-438F-8757-110328B0CED3","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Next Page","NextPage","","",4,@"","D5CA9580-D867-4539-9BC3-609F13D4CDE4");
            // Attrib for BlockType: Ability Level Select:Multi-Person First Page (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute("605389F5-5BC5-438F-8757-110328B0CED3","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Multi-Person First Page (Family Check-in)","MultiPersonFirstPage","","The first page for each person during family check-in.",5,@"","C0414290-0F05-4587-9BF8-9EB862FE3143");
            // Attrib for BlockType: Ability Level Select:Multi-Person Last Page  (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute("605389F5-5BC5-438F-8757-110328B0CED3","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Multi-Person Last Page  (Family Check-in)","MultiPersonLastPage","","The last page for each person during family check-in.",6,@"","E659796B-9C56-4668-B1AD-C1C9CDFAFF73");
            // Attrib for BlockType: Ability Level Select:Multi-Person Done Page (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute("605389F5-5BC5-438F-8757-110328B0CED3","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Multi-Person Done Page (Family Check-in)","MultiPersonDonePage","","The page to navigate to once all people have checked in during family check-in.",7,@"","B6F5457A-72CC-4288-9291-5046CFFC04B6");
            // Attrib for BlockType: Ability Level Select:Previous Page (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute("605389F5-5BC5-438F-8757-110328B0CED3","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Previous Page (Family Check-in)","FamilyPreviousPage","","The page to navigate back to if none of the people and schedules have been processed.",8,@"","3D24A4D2-90AF-4FDD-8CE2-7D1F9B76104B");
            // Attrib for BlockType: Ability Level Select:Title
            RockMigrationHelper.UpdateBlockTypeAttribute("605389F5-5BC5-438F-8757-110328B0CED3","9C204CD0-1233-41C5-818A-C5DA439445AA","Title","Title","","Title to display. Use {0} for person's name.",9,@"{0}","085D8CA9-82EE-40C2-8985-F3DB36DC4370");
            // Attrib for BlockType: Ability Level Select:Caption
            RockMigrationHelper.UpdateBlockTypeAttribute("605389F5-5BC5-438F-8757-110328B0CED3","9C204CD0-1233-41C5-818A-C5DA439445AA","Caption","Caption","","",10,@"Select Ability Level","DE63023F-10BC-4D71-94EA-A9E020016E97");
            // Attrib for BlockType: Ability Level Select:No Option Title
            RockMigrationHelper.UpdateBlockTypeAttribute("605389F5-5BC5-438F-8757-110328B0CED3","9C204CD0-1233-41C5-818A-C5DA439445AA","No Option Title","NoOptionTitle","","",11,@"Sorry","634FE416-500A-4C32-A1E0-123C19681574");
            // Attrib for BlockType: Ability Level Select:No Option Caption
            RockMigrationHelper.UpdateBlockTypeAttribute("605389F5-5BC5-438F-8757-110328B0CED3","9C204CD0-1233-41C5-818A-C5DA439445AA","No Option Caption","NoOptionCaption","","",12,@"Sorry, there are currently not any available options to check into.","6886E63B-F961-4088-9FD9-72D1A5C84DD7");
            // Attrib for BlockType: Ability Level Select:Selection No Option
            RockMigrationHelper.UpdateBlockTypeAttribute("605389F5-5BC5-438F-8757-110328B0CED3","9C204CD0-1233-41C5-818A-C5DA439445AA","Selection No Option","SelectionNoOption","","Text displayed if there are not any options after selecting an ability level. Use {0} for person's name.",13,@"Sorry, based on your selection, there are currently not any available locations that {0} can check into.","FDF2C27E-5D91-44EF-AEB7-B559A7711EE5");
            // Attrib Value for Block:Ability Level Select, Attribute:Previous Page (Family Check-in) Page: Ability Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("150D1DFC-F57F-4830-85BC-D41B4E2EB0C8","3D24A4D2-90AF-4FDD-8CE2-7D1F9B76104B",@"5a2f46ab-c2bb-4d8d-ab7b-55e83eeec5ef");
            // Attrib Value for Block:Ability Level Select, Attribute:Multi-Person First Page (Family Check-in) Page: Ability Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("150D1DFC-F57F-4830-85BC-D41B4E2EB0C8","C0414290-0F05-4587-9BF8-9EB862FE3143",@"b0799456-2597-4001-8a5b-58b0a58dd42a");
            // Attrib Value for Block:Ability Level Select, Attribute:Multi-Person Last Page  (Family Check-in) Page: Ability Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("150D1DFC-F57F-4830-85BC-D41B4E2EB0C8","E659796B-9C56-4668-B1AD-C1C9CDFAFF73",@"ca6b5be4-c193-4db7-8469-08429e6b2803");
            // Attrib Value for Block:Ability Level Select, Attribute:Multi-Person Done Page (Family Check-in) Page: Ability Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("150D1DFC-F57F-4830-85BC-D41B4E2EB0C8","B6F5457A-72CC-4288-9291-5046CFFC04B6",@"b7e527b9-8c7e-4f6c-afbf-464dec3f63a4");
            // Attrib Value for Block:Ability Level Select, Attribute:Title Page: Ability Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("150D1DFC-F57F-4830-85BC-D41B4E2EB0C8","085D8CA9-82EE-40C2-8985-F3DB36DC4370",@"{0}");
            // Attrib Value for Block:Ability Level Select, Attribute:No Option Caption Page: Ability Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("150D1DFC-F57F-4830-85BC-D41B4E2EB0C8","6886E63B-F961-4088-9FD9-72D1A5C84DD7",@"Sorry, there are currently not any available options to check into.");
            // Attrib Value for Block:Ability Level Select, Attribute:Selection No Option Page: Ability Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("150D1DFC-F57F-4830-85BC-D41B4E2EB0C8","FDF2C27E-5D91-44EF-AEB7-B559A7711EE5",@"Sorry, based on your selection, there are currently not any available locations that {0} can check into.");
            // Attrib Value for Block:Ability Level Select, Attribute:Caption Page: Ability Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("150D1DFC-F57F-4830-85BC-D41B4E2EB0C8","DE63023F-10BC-4D71-94EA-A9E020016E97",@"Select Ability Level");
            // Attrib Value for Block:Ability Level Select, Attribute:No Option Title Page: Ability Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("150D1DFC-F57F-4830-85BC-D41B4E2EB0C8","634FE416-500A-4C32-A1E0-123C19681574",@"Sorry");
            // Attrib Value for Block:Ability Level Select, Attribute:Previous Page Page: Ability Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("150D1DFC-F57F-4830-85BC-D41B4E2EB0C8","60572273-EC82-4F5E-9153-4ED79CAEFFE5",@"4a162cdd-fb14-4599-a8f7-c5ca9011e321");
            // Attrib Value for Block:Ability Level Select, Attribute:Workflow Type Page: Ability Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("150D1DFC-F57F-4830-85BC-D41B4E2EB0C8","1ED29421-3F3B-4A5B-AC1D-FAA27B34D23E",@"a0bbc045-00e5-4485-88cb-69a73ac7c78d");
            // Attrib Value for Block:Ability Level Select, Attribute:Home Page Page: Ability Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("150D1DFC-F57F-4830-85BC-D41B4E2EB0C8","5CC185C4-2ACC-4EDD-8073-9B54A638B225",@"1610b488-1ad8-44f5-aabd-96c259c02b09");
            // Attrib Value for Block:Ability Level Select, Attribute:Next Page Page: Ability Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("150D1DFC-F57F-4830-85BC-D41B4E2EB0C8","D5CA9580-D867-4539-9BC3-609F13D4CDE4",@"1f6e580f-2f38-411e-a52c-f14a89520cbd");
            // Attrib Value for Block:Ability Level Select, Attribute:Workflow Activity Page: Ability Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("150D1DFC-F57F-4830-85BC-D41B4E2EB0C8","19E1825C-A722-470D-B8E6-2E96B250E39F",@"Ability Level Search");
            // Attrib Value for Block:Idle Redirect, Attribute:Idle Seconds Page: Ability Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("74B9D8CB-A5B4-43C8-816D-CDF3081B1B75","1CAC7B16-041A-4F40-8AEE-A39DFA076C14",@"20");
            // Attrib Value for Block:Idle Redirect, Attribute:New Location Page: Ability Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("74B9D8CB-A5B4-43C8-816D-CDF3081B1B75","2254B67B-9CB1-47DE-A63D-D0B56051ECD4",@"/lcbc-checkin/welcome");



            // Page: Group Select
            RockMigrationHelper.AddPage("132DE89E-2556-45DE-A8CE-A77F4BC2EC79","23B01AB5-3AF2-46BC-8A75-BAF8CCC1C9E0","Group Select","","AC01B3BD-58E9-42CA-AA10-1A19ABD78B3C",""); // Site:LCBC Check-in
            RockMigrationHelper.AddPageRoute("AC01B3BD-58E9-42CA-AA10-1A19ABD78B3C","lcbc-checkin/group");
            RockMigrationHelper.UpdateBlockType("Group Select","Displays a list of groups that a person is configured to checkin to.","~/Blocks/CheckIn/GroupSelect.ascx","Check-in","933418C1-448E-4825-8D3D-BDE23E968483");
            RockMigrationHelper.UpdateBlockType("Idle Redirect","Redirects user to a new url after a specific number of idle seconds.","~/Blocks/Utility/IdleRedirect.ascx","Utility","49FC4B38-741E-4B0B-B395-7C1929340D88");
            // Add Block to Page: Group Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlock( true, "AC01B3BD-58E9-42CA-AA10-1A19ABD78B3C","","933418C1-448E-4825-8D3D-BDE23E968483","Group Select","Main","","",0,"D88AC05B-5F5E-41E0-992A-E9B43ACDDC99"); 
            // Add Block to Page: Group Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlock( true, "AC01B3BD-58E9-42CA-AA10-1A19ABD78B3C","","49FC4B38-741E-4B0B-B395-7C1929340D88","Idle Redirect","Main","","",1,"5DF94A0A-A965-47D5-BDDD-68DD02F880D5"); 
            // Attrib for BlockType: Group Select:Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute("933418C1-448E-4825-8D3D-BDE23E968483","46A03F59-55D3-4ACE-ADD5-B4642225DD20","Workflow Type","WorkflowType","","The workflow type to activate for check-in",0,@"","5C01B3D2-781B-4A64-8E9A-9987868AD709");
            // Attrib for BlockType: Idle Redirect:Idle Seconds
            RockMigrationHelper.UpdateBlockTypeAttribute("49FC4B38-741E-4B0B-B395-7C1929340D88","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Idle Seconds","IdleSeconds","","How many seconds of idle time to wait before redirecting user",0,@"20","1CAC7B16-041A-4F40-8AEE-A39DFA076C14");
            // Attrib for BlockType: Idle Redirect:New Location
            RockMigrationHelper.UpdateBlockTypeAttribute("49FC4B38-741E-4B0B-B395-7C1929340D88","9C204CD0-1233-41C5-818A-C5DA439445AA","New Location","NewLocation","","The new location URL to send user to after idle time",0,@"","2254B67B-9CB1-47DE-A63D-D0B56051ECD4");
            // Attrib for BlockType: Group Select:Workflow Activity
            RockMigrationHelper.UpdateBlockTypeAttribute("933418C1-448E-4825-8D3D-BDE23E968483","9C204CD0-1233-41C5-818A-C5DA439445AA","Workflow Activity","WorkflowActivity","","The name of the workflow activity to run on selection.",1,@"","868173D9-B662-4899-87EB-1F560917C787");
            // Attrib for BlockType: Group Select:Home Page
            RockMigrationHelper.UpdateBlockTypeAttribute("933418C1-448E-4825-8D3D-BDE23E968483","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Home Page","HomePage","","",2,@"","C318262A-1B9D-4B54-B2CC-3971F4E8636F");
            // Attrib for BlockType: Group Select:Previous Page
            RockMigrationHelper.UpdateBlockTypeAttribute("933418C1-448E-4825-8D3D-BDE23E968483","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Previous Page","PreviousPage","","",3,@"","795530E8-9395-4360-99B6-376A4BF40C5A");
            // Attrib for BlockType: Group Select:Next Page
            RockMigrationHelper.UpdateBlockTypeAttribute("933418C1-448E-4825-8D3D-BDE23E968483","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Next Page","NextPage","","",4,@"","E4F7B489-39B8-49F9-8C8C-533275FAACDF");
            // Attrib for BlockType: Group Select:Multi-Person First Page (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute("933418C1-448E-4825-8D3D-BDE23E968483","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Multi-Person First Page (Family Check-in)","MultiPersonFirstPage","","The first page for each person during family check-in.",5,@"","B3FA9F93-1338-4C38-A700-36AE29884C49");
            // Attrib for BlockType: Group Select:Multi-Person Last Page  (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute("933418C1-448E-4825-8D3D-BDE23E968483","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Multi-Person Last Page  (Family Check-in)","MultiPersonLastPage","","The last page for each person during family check-in.",6,@"","E4F7DEB6-4A3F-480C-B4E8-90F120E5804E");
            // Attrib for BlockType: Group Select:Multi-Person Done Page (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute("933418C1-448E-4825-8D3D-BDE23E968483","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Multi-Person Done Page (Family Check-in)","MultiPersonDonePage","","The page to navigate to once all people have checked in during family check-in.",7,@"","1A43B624-1FF9-44A8-BBB3-B6073A3C9688");
            // Attrib for BlockType: Group Select:Title
            RockMigrationHelper.UpdateBlockTypeAttribute("933418C1-448E-4825-8D3D-BDE23E968483","9C204CD0-1233-41C5-818A-C5DA439445AA","Title","Title","","Title to display. Use {0} for person/schedule.",8,@"{0}","256D4CC4-9F09-47D3-B167-46F876F0ACD3");
            // Attrib for BlockType: Group Select:Sub Title
            RockMigrationHelper.UpdateBlockTypeAttribute("933418C1-448E-4825-8D3D-BDE23E968483","9C204CD0-1233-41C5-818A-C5DA439445AA","Sub Title","SubTitle","","Sub-Title to display. Use {0} for selected group type name.",9,@"{0}","342CE8B6-7CB1-4D3D-BE1A-3F55CBC3F376");
            // Attrib for BlockType: Group Select:Caption
            RockMigrationHelper.UpdateBlockTypeAttribute("933418C1-448E-4825-8D3D-BDE23E968483","9C204CD0-1233-41C5-818A-C5DA439445AA","Caption","Caption","","",10,@"Select Group","B36D25F5-7A36-4901-9ACE-72ED355F5C6C");
            // Attrib for BlockType: Group Select:No Option Message
            RockMigrationHelper.UpdateBlockTypeAttribute("933418C1-448E-4825-8D3D-BDE23E968483","9C204CD0-1233-41C5-818A-C5DA439445AA","No Option Message","NoOptionMessage","","",11,@"Sorry, no one in your family is eligible to check-in at this location.","05FF57CD-A9D2-4E2E-9426-F02EAD95CAA4");
            // Attrib Value for Block:Group Select, Attribute:Workflow Type Page: Group Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("D88AC05B-5F5E-41E0-992A-E9B43ACDDC99","5C01B3D2-781B-4A64-8E9A-9987868AD709",@"a0bbc045-00e5-4485-88cb-69a73ac7c78d");
            // Attrib Value for Block:Group Select, Attribute:Home Page Page: Group Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("D88AC05B-5F5E-41E0-992A-E9B43ACDDC99","C318262A-1B9D-4B54-B2CC-3971F4E8636F",@"1610b488-1ad8-44f5-aabd-96c259c02b09");
            // Attrib Value for Block:Group Select, Attribute:Next Page Page: Group Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("D88AC05B-5F5E-41E0-992A-E9B43ACDDC99","E4F7B489-39B8-49F9-8C8C-533275FAACDF",@"7a6413bd-5338-425c-801e-4dbb12913c73");
            // Attrib Value for Block:Group Select, Attribute:Previous Page Page: Group Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("D88AC05B-5F5E-41E0-992A-E9B43ACDDC99","795530E8-9395-4360-99B6-376A4BF40C5A",@"1f6e580f-2f38-411e-a52c-f14a89520cbd");
            // Attrib Value for Block:Group Select, Attribute:Workflow Activity Page: Group Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("D88AC05B-5F5E-41E0-992A-E9B43ACDDC99","868173D9-B662-4899-87EB-1F560917C787",@"Location Search");
            // Attrib Value for Block:Group Select, Attribute:Multi-Person First Page (Family Check-in) Page: Group Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("D88AC05B-5F5E-41E0-992A-E9B43ACDDC99","B3FA9F93-1338-4C38-A700-36AE29884C49",@"b0799456-2597-4001-8a5b-58b0a58dd42a");
            // Attrib Value for Block:Group Select, Attribute:Multi-Person Last Page  (Family Check-in) Page: Group Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("D88AC05B-5F5E-41E0-992A-E9B43ACDDC99","E4F7DEB6-4A3F-480C-B4E8-90F120E5804E",@"ca6b5be4-c193-4db7-8469-08429e6b2803");
            // Attrib Value for Block:Group Select, Attribute:Multi-Person Done Page (Family Check-in) Page: Group Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("D88AC05B-5F5E-41E0-992A-E9B43ACDDC99","1A43B624-1FF9-44A8-BBB3-B6073A3C9688",@"b7e527b9-8c7e-4f6c-afbf-464dec3f63a4");
            // Attrib Value for Block:Group Select, Attribute:Title Page: Group Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("D88AC05B-5F5E-41E0-992A-E9B43ACDDC99","256D4CC4-9F09-47D3-B167-46F876F0ACD3",@"{0}");
            // Attrib Value for Block:Group Select, Attribute:Caption Page: Group Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("D88AC05B-5F5E-41E0-992A-E9B43ACDDC99","B36D25F5-7A36-4901-9ACE-72ED355F5C6C",@"Select Group");
            // Attrib Value for Block:Group Select, Attribute:No Option Message Page: Group Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("D88AC05B-5F5E-41E0-992A-E9B43ACDDC99","05FF57CD-A9D2-4E2E-9426-F02EAD95CAA4",@"Sorry, no one in your family is eligible to check-in at this location.");
            // Attrib Value for Block:Group Select, Attribute:Sub Title Page: Group Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("D88AC05B-5F5E-41E0-992A-E9B43ACDDC99","342CE8B6-7CB1-4D3D-BE1A-3F55CBC3F376",@"{0}");
            // Attrib Value for Block:Idle Redirect, Attribute:Idle Seconds Page: Group Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("5DF94A0A-A965-47D5-BDDD-68DD02F880D5","1CAC7B16-041A-4F40-8AEE-A39DFA076C14",@"20");
            // Attrib Value for Block:Idle Redirect, Attribute:New Location Page: Group Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("5DF94A0A-A965-47D5-BDDD-68DD02F880D5","2254B67B-9CB1-47DE-A63D-D0B56051ECD4",@"/lcbc-checkin/welcome");




            // Page: Location Select
            RockMigrationHelper.AddPage("132DE89E-2556-45DE-A8CE-A77F4BC2EC79","23B01AB5-3AF2-46BC-8A75-BAF8CCC1C9E0","Location Select","","CA6B5BE4-C193-4DB7-8469-08429E6B2803",""); // Site:LCBC Check-in
            RockMigrationHelper.AddPageRoute("CA6B5BE4-C193-4DB7-8469-08429E6B2803","lcbc-checkin/location");
            RockMigrationHelper.UpdateBlockType("Location Select","Displays a list of locations a person is able to checkin to.","~/Blocks/CheckIn/LocationSelect.ascx","Check-in","FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6");
            RockMigrationHelper.UpdateBlockType("Idle Redirect","Redirects user to a new url after a specific number of idle seconds.","~/Blocks/Utility/IdleRedirect.ascx","Utility","49FC4B38-741E-4B0B-B395-7C1929340D88");
            // Add Block to Page: Location Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlock( true, "CA6B5BE4-C193-4DB7-8469-08429E6B2803","","FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6","Location Select","Main","","",0,"DAB33126-F897-4FE4-8ECC-D9D03AA44A78"); 
            // Add Block to Page: Location Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlock( true, "CA6B5BE4-C193-4DB7-8469-08429E6B2803","","49FC4B38-741E-4B0B-B395-7C1929340D88","Idle Redirect","Main","","",1,"853C8EF2-AD66-4BDE-9487-D3A489136297"); 
            // Attrib for BlockType: Location Select:Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute("FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6","46A03F59-55D3-4ACE-ADD5-B4642225DD20","Workflow Type","WorkflowType","","The workflow type to activate for check-in",0,@"","236ABC89-D83B-4A78-BC9B-6E273E8DD81E");
            // Attrib for BlockType: Idle Redirect:Idle Seconds
            RockMigrationHelper.UpdateBlockTypeAttribute("49FC4B38-741E-4B0B-B395-7C1929340D88","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Idle Seconds","IdleSeconds","","How many seconds of idle time to wait before redirecting user",0,@"20","1CAC7B16-041A-4F40-8AEE-A39DFA076C14");
            // Attrib for BlockType: Idle Redirect:New Location
            RockMigrationHelper.UpdateBlockTypeAttribute("49FC4B38-741E-4B0B-B395-7C1929340D88","9C204CD0-1233-41C5-818A-C5DA439445AA","New Location","NewLocation","","The new location URL to send user to after idle time",0,@"","2254B67B-9CB1-47DE-A63D-D0B56051ECD4");
            // Attrib for BlockType: Location Select:Workflow Activity
            RockMigrationHelper.UpdateBlockTypeAttribute("FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6","9C204CD0-1233-41C5-818A-C5DA439445AA","Workflow Activity","WorkflowActivity","","The name of the workflow activity to run on selection.",1,@"","0E65FA3B-FB48-4DEC-B0F3-9394FBC21818");
            // Attrib for BlockType: Location Select:Home Page
            RockMigrationHelper.UpdateBlockTypeAttribute("FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Home Page","HomePage","","",2,@"","051CEDD9-2FB5-4873-8FBB-B5F5671EF044");
            // Attrib for BlockType: Location Select:Previous Page
            RockMigrationHelper.UpdateBlockTypeAttribute("FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Previous Page","PreviousPage","","",3,@"","569E033B-A2D5-4C15-8CD5-7F1336C22871");
            // Attrib for BlockType: Location Select:Next Page
            RockMigrationHelper.UpdateBlockTypeAttribute("FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Next Page","NextPage","","",4,@"","39246677-8451-4422-B384-C7AD9DA6C649");
            // Attrib for BlockType: Location Select:Multi-Person First Page (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute("FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Multi-Person First Page (Family Check-in)","MultiPersonFirstPage","","The first page for each person during family check-in.",5,@"","14F75C51-6176-4DBF-B1FC-6517E62E310F");
            // Attrib for BlockType: Location Select:Multi-Person Last Page  (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute("FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Multi-Person Last Page  (Family Check-in)","MultiPersonLastPage","","The last page for each person during family check-in.",6,@"","CEE736CA-7F05-4480-B34B-2A4A743F556C");
            // Attrib for BlockType: Location Select:Multi-Person Done Page (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute("FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Multi-Person Done Page (Family Check-in)","MultiPersonDonePage","","The page to navigate to once all people have checked in during family check-in.",7,@"","8EB048AF-3A8B-4D55-8045-861B9AE7DF4C");
            // Attrib for BlockType: Location Select:Title
            RockMigrationHelper.UpdateBlockTypeAttribute("FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6","9C204CD0-1233-41C5-818A-C5DA439445AA","Title","Title","","Title to display. Use {0} for person/schedule.",8,@"{0}","F95CAB1D-37A4-4A53-B63F-BF9D275FBA27");
            // Attrib for BlockType: Location Select:Sub Title
            RockMigrationHelper.UpdateBlockTypeAttribute("FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6","9C204CD0-1233-41C5-818A-C5DA439445AA","Sub Title","SubTitle","","Sub-Title to display. Use {0} for selected group name.",9,@"{0}","A85DEF5A-D6CE-41FE-891E-36880DE5CD9C");
            // Attrib for BlockType: Location Select:Caption
            RockMigrationHelper.UpdateBlockTypeAttribute("FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6","9C204CD0-1233-41C5-818A-C5DA439445AA","Caption","Caption","","",10,@"Select Location","3CD2A392-3A06-42BE-A3A9-8324D5FCC810");
            // Attrib for BlockType: Location Select:No Option Message
            RockMigrationHelper.UpdateBlockTypeAttribute("FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6","9C204CD0-1233-41C5-818A-C5DA439445AA","No Option Message","NoOptionMessage","","Message to display when there are not any options available. Use {0} for person's name, and {1} for schedule name.",11,@"Sorry, there are currently not any available locations that {0} can check into at {1}.","C94E5760-4EF1-40B4-84B9-B75EFAA1030B");
            // Attrib for BlockType: Location Select:No Option After Select Message
            RockMigrationHelper.UpdateBlockTypeAttribute("FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6","9C204CD0-1233-41C5-818A-C5DA439445AA","No Option After Select Message","NoOptionAfterSelectMessage","","Message to display when there are not any options available after location is selected. Use {0} for person's name",12,@"Sorry, based on your selection, there are currently not any available times that {0} can check into.","C14C7143-5D9C-463F-9C8B-2680509C22A5");
            // Attrib Value for Block:Location Select, Attribute:Multi-Person First Page (Family Check-in) Page: Location Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("DAB33126-F897-4FE4-8ECC-D9D03AA44A78","14F75C51-6176-4DBF-B1FC-6517E62E310F",@"b0799456-2597-4001-8a5b-58b0a58dd42a");
            // Attrib Value for Block:Location Select, Attribute:Multi-Person Last Page  (Family Check-in) Page: Location Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("DAB33126-F897-4FE4-8ECC-D9D03AA44A78","CEE736CA-7F05-4480-B34B-2A4A743F556C",@"ca6b5be4-c193-4db7-8469-08429e6b2803");
            // Attrib Value for Block:Location Select, Attribute:Multi-Person Done Page (Family Check-in) Page: Location Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("DAB33126-F897-4FE4-8ECC-D9D03AA44A78","8EB048AF-3A8B-4D55-8045-861B9AE7DF4C",@"b7e527b9-8c7e-4f6c-afbf-464dec3f63a4");
            // Attrib Value for Block:Location Select, Attribute:No Option After Select Message Page: Location Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("DAB33126-F897-4FE4-8ECC-D9D03AA44A78","C14C7143-5D9C-463F-9C8B-2680509C22A5",@"Sorry, based on your selection, there are currently not any available times that {0} can check into.");
            // Attrib Value for Block:Location Select, Attribute:No Option Message Page: Location Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("DAB33126-F897-4FE4-8ECC-D9D03AA44A78","C94E5760-4EF1-40B4-84B9-B75EFAA1030B",@"Sorry, there are currently not any available locations that {0} can check into at {1}.");
            // Attrib Value for Block:Location Select, Attribute:Title Page: Location Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("DAB33126-F897-4FE4-8ECC-D9D03AA44A78","F95CAB1D-37A4-4A53-B63F-BF9D275FBA27",@"{0}");
            // Attrib Value for Block:Location Select, Attribute:Sub Title Page: Location Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("DAB33126-F897-4FE4-8ECC-D9D03AA44A78","A85DEF5A-D6CE-41FE-891E-36880DE5CD9C",@"{0}");
            // Attrib Value for Block:Location Select, Attribute:Caption Page: Location Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("DAB33126-F897-4FE4-8ECC-D9D03AA44A78","3CD2A392-3A06-42BE-A3A9-8324D5FCC810",@"Select Location");
            // Attrib Value for Block:Location Select, Attribute:Workflow Type Page: Location Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("DAB33126-F897-4FE4-8ECC-D9D03AA44A78","236ABC89-D83B-4A78-BC9B-6E273E8DD81E",@"a0bbc045-00e5-4485-88cb-69a73ac7c78d");
            // Attrib Value for Block:Location Select, Attribute:Home Page Page: Location Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("DAB33126-F897-4FE4-8ECC-D9D03AA44A78","051CEDD9-2FB5-4873-8FBB-B5F5671EF044",@"1610b488-1ad8-44f5-aabd-96c259c02b09");
            // Attrib Value for Block:Location Select, Attribute:Next Page Page: Location Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("DAB33126-F897-4FE4-8ECC-D9D03AA44A78","39246677-8451-4422-B384-C7AD9DA6C649",@"c87bbf9b-7ed9-4ebc-99e0-354bd350f4a1");
            // Attrib Value for Block:Location Select, Attribute:Previous Page Page: Location Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("DAB33126-F897-4FE4-8ECC-D9D03AA44A78","569E033B-A2D5-4C15-8CD5-7F1336C22871",@"d07aacc7-f2b3-4bf2-96ef-91bb9394d47c");
            // Attrib Value for Block:Location Select, Attribute:Workflow Activity Page: Location Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("DAB33126-F897-4FE4-8ECC-D9D03AA44A78","0E65FA3B-FB48-4DEC-B0F3-9394FBC21818",@"Schedule Search");
            // Attrib Value for Block:Idle Redirect, Attribute:Idle Seconds Page: Location Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("853C8EF2-AD66-4BDE-9487-D3A489136297","1CAC7B16-041A-4F40-8AEE-A39DFA076C14",@"20");
            // Attrib Value for Block:Idle Redirect, Attribute:New Location Page: Location Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("853C8EF2-AD66-4BDE-9487-D3A489136297","2254B67B-9CB1-47DE-A63D-D0B56051ECD4",@"/lcbc-checkin/welcome");



            // Page: Time Select
            RockMigrationHelper.AddPage("132DE89E-2556-45DE-A8CE-A77F4BC2EC79","23B01AB5-3AF2-46BC-8A75-BAF8CCC1C9E0","Time Select","","C87BBF9B-7ED9-4EBC-99E0-354BD350F4A1",""); // Site:LCBC Check-in
            RockMigrationHelper.AddPageRoute("C87BBF9B-7ED9-4EBC-99E0-354BD350F4A1","lcbc-checkin/time");
            RockMigrationHelper.UpdateBlockType("Time Select","Displays a list of times to checkin for.","~/Blocks/CheckIn/TimeSelect.ascx","Check-in","D2348D51-B13A-4069-97AD-369D9615A711");
            RockMigrationHelper.UpdateBlockType("Idle Redirect","Redirects user to a new url after a specific number of idle seconds.","~/Blocks/Utility/IdleRedirect.ascx","Utility","49FC4B38-741E-4B0B-B395-7C1929340D88");
            // Add Block to Page: Time Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlock( true, "C87BBF9B-7ED9-4EBC-99E0-354BD350F4A1","","D2348D51-B13A-4069-97AD-369D9615A711","Time Select","Main","","",0,"FD613FF9-8610-4444-80E9-6C38DA796ED7"); 
            // Add Block to Page: Time Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlock( true, "C87BBF9B-7ED9-4EBC-99E0-354BD350F4A1","","49FC4B38-741E-4B0B-B395-7C1929340D88","Idle Redirect","Main","","",1,"F5AC0A96-5ED3-4D99-8C6F-4EA3CED3EBE4"); 
            // Attrib for BlockType: Time Select:Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute("D2348D51-B13A-4069-97AD-369D9615A711","46A03F59-55D3-4ACE-ADD5-B4642225DD20","Workflow Type","WorkflowType","","The workflow type to activate for check-in",0,@"","108E2E9E-DC18-4D5D-80FA-5D4A90FFCE65");
            // Attrib for BlockType: Idle Redirect:Idle Seconds
            RockMigrationHelper.UpdateBlockTypeAttribute("49FC4B38-741E-4B0B-B395-7C1929340D88","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Idle Seconds","IdleSeconds","","How many seconds of idle time to wait before redirecting user",0,@"20","1CAC7B16-041A-4F40-8AEE-A39DFA076C14");
            // Attrib for BlockType: Idle Redirect:New Location
            RockMigrationHelper.UpdateBlockTypeAttribute("49FC4B38-741E-4B0B-B395-7C1929340D88","9C204CD0-1233-41C5-818A-C5DA439445AA","New Location","NewLocation","","The new location URL to send user to after idle time",0,@"","2254B67B-9CB1-47DE-A63D-D0B56051ECD4");
            // Attrib for BlockType: Time Select:Workflow Activity
            RockMigrationHelper.UpdateBlockTypeAttribute("D2348D51-B13A-4069-97AD-369D9615A711","9C204CD0-1233-41C5-818A-C5DA439445AA","Workflow Activity","WorkflowActivity","","The name of the workflow activity to run on selection.",1,@"","12DF930E-6460-4A66-9326-E39BEAFC6F9D");
            // Attrib for BlockType: Time Select:Home Page
            RockMigrationHelper.UpdateBlockTypeAttribute("D2348D51-B13A-4069-97AD-369D9615A711","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Home Page","HomePage","","",2,@"","D5AFB471-3EE2-44D5-BC66-F4EFD26FD394");
            // Attrib for BlockType: Time Select:Previous Page
            RockMigrationHelper.UpdateBlockTypeAttribute("D2348D51-B13A-4069-97AD-369D9615A711","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Previous Page","PreviousPage","","",3,@"","DE808D50-0861-4E24-A483-F1C74C1FFDE8");
            // Attrib for BlockType: Time Select:Next Page
            RockMigrationHelper.UpdateBlockTypeAttribute("D2348D51-B13A-4069-97AD-369D9615A711","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Next Page","NextPage","","",4,@"","840898DB-A9AB-45C9-9894-0A1E816EFC4C");
            // Attrib for BlockType: Time Select:Title
            RockMigrationHelper.UpdateBlockTypeAttribute("D2348D51-B13A-4069-97AD-369D9615A711","9C204CD0-1233-41C5-818A-C5DA439445AA","Title","Title","","Title to display. Use {0} for family/person name.",5,@"{0}","B5CF8A58-92E8-4473-BE73-63FB3B6FF49E");
            // Attrib for BlockType: Time Select:Sub Title
            RockMigrationHelper.UpdateBlockTypeAttribute("D2348D51-B13A-4069-97AD-369D9615A711","9C204CD0-1233-41C5-818A-C5DA439445AA","Sub Title","SubTitle","","Sub-Title to display. Use {0} for selected group/location name.",6,@"{0}","2BE3BCC3-7307-4DA3-BF4A-4B8E4A5C9837");
            // Attrib for BlockType: Time Select:Caption
            RockMigrationHelper.UpdateBlockTypeAttribute("D2348D51-B13A-4069-97AD-369D9615A711","9C204CD0-1233-41C5-818A-C5DA439445AA","Caption","Caption","","",7,@"Select Time(s)","16091831-474A-4618-872F-E9257F7E9948");
            // Attrib Value for Block:Time Select, Attribute:Workflow Type Page: Time Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("FD613FF9-8610-4444-80E9-6C38DA796ED7","108E2E9E-DC18-4D5D-80FA-5D4A90FFCE65",@"a0bbc045-00e5-4485-88cb-69a73ac7c78d");
            // Attrib Value for Block:Time Select, Attribute:Home Page Page: Time Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("FD613FF9-8610-4444-80E9-6C38DA796ED7","D5AFB471-3EE2-44D5-BC66-F4EFD26FD394",@"1610b488-1ad8-44f5-aabd-96c259c02b09");
            // Attrib Value for Block:Time Select, Attribute:Next Page Page: Time Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("FD613FF9-8610-4444-80E9-6C38DA796ED7","840898DB-A9AB-45C9-9894-0A1E816EFC4C",@"16cc3f78-9149-457e-9718-099bf5278ed0");
            // Attrib Value for Block:Time Select, Attribute:Previous Page Page: Time Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("FD613FF9-8610-4444-80E9-6C38DA796ED7","DE808D50-0861-4E24-A483-F1C74C1FFDE8",@"ca6b5be4-c193-4db7-8469-08429e6b2803");
            // Attrib Value for Block:Time Select, Attribute:Workflow Activity Page: Time Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("FD613FF9-8610-4444-80E9-6C38DA796ED7","12DF930E-6460-4A66-9326-E39BEAFC6F9D",@"Save Attendance");
            // Attrib Value for Block:Time Select, Attribute:Title Page: Time Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("FD613FF9-8610-4444-80E9-6C38DA796ED7","B5CF8A58-92E8-4473-BE73-63FB3B6FF49E",@"{0}");
            // Attrib Value for Block:Time Select, Attribute:Sub Title Page: Time Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("FD613FF9-8610-4444-80E9-6C38DA796ED7","2BE3BCC3-7307-4DA3-BF4A-4B8E4A5C9837",@"{0}");
            // Attrib Value for Block:Time Select, Attribute:Caption Page: Time Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("FD613FF9-8610-4444-80E9-6C38DA796ED7","16091831-474A-4618-872F-E9257F7E9948",@"Select Time(s)");
            // Attrib Value for Block:Idle Redirect, Attribute:Idle Seconds Page: Time Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("F5AC0A96-5ED3-4D99-8C6F-4EA3CED3EBE4","1CAC7B16-041A-4F40-8AEE-A39DFA076C14",@"20");
            // Attrib Value for Block:Idle Redirect, Attribute:New Location Page: Time Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("F5AC0A96-5ED3-4D99-8C6F-4EA3CED3EBE4","2254B67B-9CB1-47DE-A63D-D0B56051ECD4",@"/lcbc-checkin/welcome");



            // Page: Success
            RockMigrationHelper.AddPage("132DE89E-2556-45DE-A8CE-A77F4BC2EC79","23B01AB5-3AF2-46BC-8A75-BAF8CCC1C9E0","Success","","16CC3F78-9149-457E-9718-099BF5278ED0",""); // Site:LCBC Check-in
            RockMigrationHelper.AddPageRoute("16CC3F78-9149-457E-9718-099BF5278ED0","lcbc-checkin/success");
            RockMigrationHelper.UpdateBlockType("Idle Redirect","Redirects user to a new url after a specific number of idle seconds.","~/Blocks/Utility/IdleRedirect.ascx","Utility","49FC4B38-741E-4B0B-B395-7C1929340D88");
            RockMigrationHelper.UpdateBlockType("Custom Checkin Success","Displays the details of a successful checkin.","~/Plugins/org_newpointe/CheckinBatchCut/CustomCheckinSuccess.ascx","NewPointe: Check-in Batch Cutting","52C24978-0DF0-45B2-995F-D9C1822EBCAD");
            // Add Block to Page: Success, Site: LCBC Check-in
            RockMigrationHelper.AddBlock( true, "16CC3F78-9149-457E-9718-099BF5278ED0","","52C24978-0DF0-45B2-995F-D9C1822EBCAD","Custom Checkin Success","Main","","",0,"0B7E6253-9FFC-4CAB-BC06-D9F99F49D5FC"); 
            // Add Block to Page: Success, Site: LCBC Check-in
            RockMigrationHelper.AddBlock( true, "16CC3F78-9149-457E-9718-099BF5278ED0","","49FC4B38-741E-4B0B-B395-7C1929340D88","Idle Redirect","Main","","",1,"689B0B1C-0A55-4717-A5D6-952F3313DFF3"); 
            // Attrib for BlockType: Idle Redirect:Idle Seconds
            RockMigrationHelper.UpdateBlockTypeAttribute("49FC4B38-741E-4B0B-B395-7C1929340D88","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Idle Seconds","IdleSeconds","","How many seconds of idle time to wait before redirecting user",0,@"20","1CAC7B16-041A-4F40-8AEE-A39DFA076C14");
            // Attrib for BlockType: Idle Redirect:New Location
            RockMigrationHelper.UpdateBlockTypeAttribute("49FC4B38-741E-4B0B-B395-7C1929340D88","9C204CD0-1233-41C5-818A-C5DA439445AA","New Location","NewLocation","","The new location URL to send user to after idle time",0,@"","2254B67B-9CB1-47DE-A63D-D0B56051ECD4");
            // Attrib for BlockType: Custom Checkin Success:Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute("52C24978-0DF0-45B2-995F-D9C1822EBCAD","46A03F59-55D3-4ACE-ADD5-B4642225DD20","Workflow Type","WorkflowType","","The workflow type to activate for check-in",0,@"","226199A6-4652-4E76-8458-F65AB7BD61F0");
            // Attrib for BlockType: Custom Checkin Success:Workflow Activity
            RockMigrationHelper.UpdateBlockTypeAttribute("52C24978-0DF0-45B2-995F-D9C1822EBCAD","9C204CD0-1233-41C5-818A-C5DA439445AA","Workflow Activity","WorkflowActivity","","The name of the workflow activity to run on selection.",1,@"","42F1885D-C21D-4C49-9415-6E36D330FD08");
            // Attrib for BlockType: Custom Checkin Success:Home Page
            RockMigrationHelper.UpdateBlockTypeAttribute("52C24978-0DF0-45B2-995F-D9C1822EBCAD","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Home Page","HomePage","","",2,@"","813F9853-2B70-4DED-8B86-1685B973264E");
            // Attrib for BlockType: Custom Checkin Success:Previous Page
            RockMigrationHelper.UpdateBlockTypeAttribute("52C24978-0DF0-45B2-995F-D9C1822EBCAD","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Previous Page","PreviousPage","","",3,@"","4B0036FD-8C4A-4AEF-BF85-0085C3840101");
            // Attrib for BlockType: Custom Checkin Success:Next Page
            RockMigrationHelper.UpdateBlockTypeAttribute("52C24978-0DF0-45B2-995F-D9C1822EBCAD","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Next Page","NextPage","","",4,@"","3FC12FA9-B03E-4186-8AC4-CB1696469D34");
            // Attrib for BlockType: Custom Checkin Success:Person Select Page
            RockMigrationHelper.UpdateBlockTypeAttribute("52C24978-0DF0-45B2-995F-D9C1822EBCAD","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Person Select Page","PersonSelectPage","","",5,@"","70E9D04E-3BB9-469F-961A-B99E56F4B510");
            // Attrib for BlockType: Custom Checkin Success:Title
            RockMigrationHelper.UpdateBlockTypeAttribute("52C24978-0DF0-45B2-995F-D9C1822EBCAD","9C204CD0-1233-41C5-818A-C5DA439445AA","Title","Title","","",6,@"Checked-in","12B32671-30DA-41D2-8626-2FF166E5D684");
            // Attrib for BlockType: Custom Checkin Success:Detail Message
            RockMigrationHelper.UpdateBlockTypeAttribute("52C24978-0DF0-45B2-995F-D9C1822EBCAD","9C204CD0-1233-41C5-818A-C5DA439445AA","Detail Message","DetailMessage","","The message to display indicating person has been checked in. Use {0} for person, {1} for group, {2} for schedule, and {3} for the security code",7,@"{0} was checked into {1} in {2} at {3}","F08FA1F3-0EE3-426D-868F-65A1C4E5C46D");
            // Attrib Value for Block:Custom Checkin Success, Attribute:Detail Message Page: Success, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("0B7E6253-9FFC-4CAB-BC06-D9F99F49D5FC","F08FA1F3-0EE3-426D-868F-65A1C4E5C46D",@"{0} was checked into {1} in {2} at {3}");
            // Attrib Value for Block:Custom Checkin Success, Attribute:Person Select Page Page: Success, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("0B7E6253-9FFC-4CAB-BC06-D9F99F49D5FC","70E9D04E-3BB9-469F-961A-B99E56F4B510",@"4a162cdd-fb14-4599-a8f7-c5ca9011e321");
            // Attrib Value for Block:Custom Checkin Success, Attribute:Title Page: Success, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("0B7E6253-9FFC-4CAB-BC06-D9F99F49D5FC","12B32671-30DA-41D2-8626-2FF166E5D684",@"Checked-in");
            // Attrib Value for Block:Custom Checkin Success, Attribute:Workflow Type Page: Success, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("0B7E6253-9FFC-4CAB-BC06-D9F99F49D5FC","226199A6-4652-4E76-8458-F65AB7BD61F0",@"a0bbc045-00e5-4485-88cb-69a73ac7c78d");
            // Attrib Value for Block:Custom Checkin Success, Attribute:Home Page Page: Success, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("0B7E6253-9FFC-4CAB-BC06-D9F99F49D5FC","813F9853-2B70-4DED-8B86-1685B973264E",@"1610b488-1ad8-44f5-aabd-96c259c02b09");
            // Attrib Value for Block:Custom Checkin Success, Attribute:Previous Page Page: Success, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("0B7E6253-9FFC-4CAB-BC06-D9F99F49D5FC","4B0036FD-8C4A-4AEF-BF85-0085C3840101",@"c87bbf9b-7ed9-4ebc-99e0-354bd350f4a1");
            // Attrib Value for Block:Custom Checkin Success, Attribute:Next Page Page: Success, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("0B7E6253-9FFC-4CAB-BC06-D9F99F49D5FC","3FC12FA9-B03E-4186-8AC4-CB1696469D34",@"16cc3f78-9149-457e-9718-099bf5278ed0");
            // Attrib Value for Block:Idle Redirect, Attribute:Idle Seconds Page: Success, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("689B0B1C-0A55-4717-A5D6-952F3313DFF3","1CAC7B16-041A-4F40-8AEE-A39DFA076C14",@"20");
            // Attrib Value for Block:Idle Redirect, Attribute:New Location Page: Success, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("689B0B1C-0A55-4717-A5D6-952F3313DFF3","2254B67B-9CB1-47DE-A63D-D0B56051ECD4",@"/lcbc-checkin/welcome");



            // Page: Scheduled Locations
            RockMigrationHelper.AddPage("132DE89E-2556-45DE-A8CE-A77F4BC2EC79","23B01AB5-3AF2-46BC-8A75-BAF8CCC1C9E0","Scheduled Locations","","9CD9CA88-87E8-4CBF-8E61-C277531E6F8C",""); // Site:LCBC Check-in
            RockMigrationHelper.AddPageRoute("9CD9CA88-87E8-4CBF-8E61-C277531E6F8C","lcbc-checkin/scheduledlocations");
            RockMigrationHelper.UpdateBlockType("Check-in Scheduled Locations","Helps to enable/disable schedules associated with the configured group types at a kiosk","~/Blocks/CheckIn/CheckinScheduledLocations.ascx","Check-in","C8C4E323-C227-4EAA-938F-4B962BC2DD7E");
            // Add Block to Page: Scheduled Locations, Site: LCBC Check-in
            RockMigrationHelper.AddBlock( true, "9CD9CA88-87E8-4CBF-8E61-C277531E6F8C","","C8C4E323-C227-4EAA-938F-4B962BC2DD7E","Check-in Scheduled Locations","Main","","",0,"8C6C3BEC-1A71-4C3F-AA7E-39C9D13DFEDD"); 
            // Attrib for BlockType: Check-in Scheduled Locations:Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute("C8C4E323-C227-4EAA-938F-4B962BC2DD7E","46A03F59-55D3-4ACE-ADD5-B4642225DD20","Workflow Type","WorkflowType","","The workflow type to activate for check-in",0,@"","2B9D7D0C-2027-4364-ACD4-22BD4BE0E8FE");
            // Attrib for BlockType: Check-in Scheduled Locations:Workflow Activity
            RockMigrationHelper.UpdateBlockTypeAttribute("C8C4E323-C227-4EAA-938F-4B962BC2DD7E","9C204CD0-1233-41C5-818A-C5DA439445AA","Workflow Activity","WorkflowActivity","","The name of the workflow activity to run on selection.",1,@"","E8503CEA-AA23-4F02-9B23-8F4018F0B553");
            // Attrib for BlockType: Check-in Scheduled Locations:Home Page
            RockMigrationHelper.UpdateBlockTypeAttribute("C8C4E323-C227-4EAA-938F-4B962BC2DD7E","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Home Page","HomePage","","",2,@"","93DDA73B-5672-4099-9CB0-61DFF17A48DC");
            // Attrib for BlockType: Check-in Scheduled Locations:Previous Page
            RockMigrationHelper.UpdateBlockTypeAttribute("C8C4E323-C227-4EAA-938F-4B962BC2DD7E","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Previous Page","PreviousPage","","",3,@"","49A01E13-F151-472D-9862-5E5BA64D6066");
            // Attrib for BlockType: Check-in Scheduled Locations:Next Page
            RockMigrationHelper.UpdateBlockTypeAttribute("C8C4E323-C227-4EAA-938F-4B962BC2DD7E","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Next Page","NextPage","","",4,@"","3E080271-B91D-43EC-93A5-FA15F60DFE5D");



            // Page: Person Select (Family Check-in)
            RockMigrationHelper.AddPage("132DE89E-2556-45DE-A8CE-A77F4BC2EC79","23B01AB5-3AF2-46BC-8A75-BAF8CCC1C9E0","Person Select (Family Check-in)","","BAED4080-0AFC-4D2D-8748-659AA9A28D24",""); // Site:LCBC Check-in
            RockMigrationHelper.AddPageRoute("BAED4080-0AFC-4D2D-8748-659AA9A28D24","lcbc-checkin/people");
            RockMigrationHelper.UpdateBlockType("Idle Redirect","Redirects user to a new url after a specific number of idle seconds.","~/Blocks/Utility/IdleRedirect.ascx","Utility","49FC4B38-741E-4B0B-B395-7C1929340D88");
            RockMigrationHelper.UpdateBlockType("Edit Family","Block to Add or Edit a Family during the Check-in Process.","~/Blocks/CheckIn/EditFamily.ascx","Check-in","06DF448A-684E-4B64-8E1B-EA1727BA9233");
            RockMigrationHelper.UpdateBlockType("Person Select (Family Check-in)","Lists people who match the selected family and provides option of selecting multiple.","~/Plugins/com_lcbcchurch/CheckIn/MultiPersonSelect.ascx","LCBC > Check-in","76A6D95E-A067-4E36-AE0D-10782836D69D");
            // Add Block to Page: Person Select (Family Check-in), Site: LCBC Check-in
            RockMigrationHelper.AddBlock( true, "BAED4080-0AFC-4D2D-8748-659AA9A28D24","","76A6D95E-A067-4E36-AE0D-10782836D69D","Person Select","Main","","",0,"A121AC90-9E70-4C8F-B57F-2256CF57714A"); 
            // Add Block to Page: Person Select (Family Check-in), Site: LCBC Check-in
            RockMigrationHelper.AddBlock( true, "BAED4080-0AFC-4D2D-8748-659AA9A28D24","","49FC4B38-741E-4B0B-B395-7C1929340D88","Idle Redirect","Main","","",1,"E316B493-B752-4B3F-AE92-F5D7BB1C47E2"); 
            // Add Block to Page: Person Select (Family Check-in), Site: LCBC Check-in
            RockMigrationHelper.AddBlock( true, "BAED4080-0AFC-4D2D-8748-659AA9A28D24","","06DF448A-684E-4B64-8E1B-EA1727BA9233","Edit Family","Main","","",2,"CEF09175-8B7A-4652-BDE7-39CEFE1CB548"); 
            // Attrib for BlockType: Idle Redirect:Idle Seconds
            RockMigrationHelper.UpdateBlockTypeAttribute("49FC4B38-741E-4B0B-B395-7C1929340D88","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Idle Seconds","IdleSeconds","","How many seconds of idle time to wait before redirecting user",0,@"20","1CAC7B16-041A-4F40-8AEE-A39DFA076C14");
            // Attrib for BlockType: Idle Redirect:New Location
            RockMigrationHelper.UpdateBlockTypeAttribute("49FC4B38-741E-4B0B-B395-7C1929340D88","9C204CD0-1233-41C5-818A-C5DA439445AA","New Location","NewLocation","","The new location URL to send user to after idle time",0,@"","2254B67B-9CB1-47DE-A63D-D0B56051ECD4");
            // Attrib for BlockType: Edit Family:Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute("06DF448A-684E-4B64-8E1B-EA1727BA9233","46A03F59-55D3-4ACE-ADD5-B4642225DD20","Workflow Type","WorkflowType","","The workflow type to activate for check-in",0,@"","C7C8C51E-B5A0-49A5-96F3-CB23BB5F81AB");
            // Attrib for BlockType: Person Select (Family Check-in):Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute("76A6D95E-A067-4E36-AE0D-10782836D69D","46A03F59-55D3-4ACE-ADD5-B4642225DD20","Workflow Type","WorkflowType","","The workflow type to activate for check-in",0,@"","E53CAF3F-70D8-4968-AB60-37146DF9D61F");
            // Attrib for BlockType: Person Select (Family Check-in):Workflow Activity
            RockMigrationHelper.UpdateBlockTypeAttribute("76A6D95E-A067-4E36-AE0D-10782836D69D","9C204CD0-1233-41C5-818A-C5DA439445AA","Workflow Activity","WorkflowActivity","","The name of the workflow activity to run on selection.",1,@"","C16FD48E-1D3E-4582-A7C3-BC54865EBDEF");
            // Attrib for BlockType: Edit Family:Workflow Activity
            RockMigrationHelper.UpdateBlockTypeAttribute("06DF448A-684E-4B64-8E1B-EA1727BA9233","9C204CD0-1233-41C5-818A-C5DA439445AA","Workflow Activity","WorkflowActivity","","The name of the workflow activity to run on selection.",1,@"","9B1B49A1-716D-4B5C-A75E-D39B681207AB");
            // Attrib for BlockType: Edit Family:Home Page
            RockMigrationHelper.UpdateBlockTypeAttribute("06DF448A-684E-4B64-8E1B-EA1727BA9233","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Home Page","HomePage","","",2,@"","808EA130-EDED-45FD-9683-A5A26859128F");
            // Attrib for BlockType: Person Select (Family Check-in):Home Page
            RockMigrationHelper.UpdateBlockTypeAttribute("76A6D95E-A067-4E36-AE0D-10782836D69D","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Home Page","HomePage","","",2,@"","E0B65EBE-7D9D-495C-94E0-B6D06C360038");
            // Attrib for BlockType: Person Select (Family Check-in):Previous Page
            RockMigrationHelper.UpdateBlockTypeAttribute("76A6D95E-A067-4E36-AE0D-10782836D69D","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Previous Page","PreviousPage","","",3,@"","91418009-9856-4CE4-8C18-CF2BA0EC1CCF");
            // Attrib for BlockType: Edit Family:Previous Page
            RockMigrationHelper.UpdateBlockTypeAttribute("06DF448A-684E-4B64-8E1B-EA1727BA9233","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Previous Page","PreviousPage","","",3,@"","FC3382DD-647F-4C2F-A06F-3A0C274B8B95");
            // Attrib for BlockType: Edit Family:Next Page
            RockMigrationHelper.UpdateBlockTypeAttribute("06DF448A-684E-4B64-8E1B-EA1727BA9233","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Next Page","NextPage","","",4,@"","B9F99590-3E58-4746-9884-14D2223D00F8");
            // Attrib for BlockType: Person Select (Family Check-in):Next Page
            RockMigrationHelper.UpdateBlockTypeAttribute("76A6D95E-A067-4E36-AE0D-10782836D69D","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Next Page","NextPage","","",4,@"","78B1BA12-B331-4A5E-B9CE-CD5F6592AB23");
            // Attrib for BlockType: Person Select (Family Check-in):Auto Select Next Page
            RockMigrationHelper.UpdateBlockTypeAttribute("76A6D95E-A067-4E36-AE0D-10782836D69D","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Auto Select Next Page","AutoSelectNextPage","","The page to navigate to after selecting people in auto-select mode.",5,@"","8DAB8E81-3C19-4DAB-8F9E-6A0892375D2B");
            // Attrib for BlockType: Person Select (Family Check-in):Auto Select Done Page
            RockMigrationHelper.UpdateBlockTypeAttribute("76A6D95E-A067-4E36-AE0D-10782836D69D","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Auto Select Done Page","AutoSelectDonePage","","The page to navigate to once all people have checked in during family check-in.",6,@"","0ADE3737-78D3-49BA-8B10-04223B127242");
            // Attrib for BlockType: Person Select (Family Check-in):Pre-Selected Options Format
            RockMigrationHelper.UpdateBlockTypeAttribute("76A6D95E-A067-4E36-AE0D-10782836D69D","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","Pre-Selected Options Format","OptionFormat","","The format to use when displaying auto-checkin options",7,@"
<span class='auto-select-schedule'>{{ Schedule.Name }}:</span>
<span class='auto-select-group'>{{ Group.Name }}</span>
<span class='auto-select-location'>{{ Location.Name }}</span>
","194F9B56-2A6D-4D0C-BC41-97B16946A81B");
            // Attrib for BlockType: Person Select (Family Check-in):Title
            RockMigrationHelper.UpdateBlockTypeAttribute("76A6D95E-A067-4E36-AE0D-10782836D69D","9C204CD0-1233-41C5-818A-C5DA439445AA","Title","Title","","Title to display. Use {0} for family name.",8,@"{0}","D2330B74-D772-4144-812C-1B35A080A033");
            // Attrib for BlockType: Person Select (Family Check-in):Caption
            RockMigrationHelper.UpdateBlockTypeAttribute("76A6D95E-A067-4E36-AE0D-10782836D69D","9C204CD0-1233-41C5-818A-C5DA439445AA","Caption","Caption","","",9,@"Select People","E831CC6C-78EE-4D6A-BAD4-44B9519BAA10");
            // Attrib for BlockType: Person Select (Family Check-in):Option Title
            RockMigrationHelper.UpdateBlockTypeAttribute("76A6D95E-A067-4E36-AE0D-10782836D69D","9C204CD0-1233-41C5-818A-C5DA439445AA","Option Title","OptionTitle","","Title to display on option screen. Use {0} for person's full name.",10,@"{0}","53B77FFC-E9C5-43C7-8D86-2DD51E62EA0B");
            // Attrib for BlockType: Person Select (Family Check-in):Option Sub Title
            RockMigrationHelper.UpdateBlockTypeAttribute("76A6D95E-A067-4E36-AE0D-10782836D69D","9C204CD0-1233-41C5-818A-C5DA439445AA","Option Sub Title","OptionSubTitle","","Subtitle to display on option screen. Use {0} for person's nickname.",11,@"Please select the options that {0} would like to attend.","D2345110-06FE-4964-A98C-BE897D3A325D");
            // Attrib for BlockType: Person Select (Family Check-in):No Option Message
            RockMigrationHelper.UpdateBlockTypeAttribute("76A6D95E-A067-4E36-AE0D-10782836D69D","9C204CD0-1233-41C5-818A-C5DA439445AA","No Option Message","NoOptionMessage","","",12,@"Sorry, there are currently not any available areas that the selected people can check into.","41D2B172-6CC5-480A-B76F-76A80A1135CD");
            // Attrib for BlockType: Person Select (Family Check-in):Next Button Text
            RockMigrationHelper.UpdateBlockTypeAttribute("76A6D95E-A067-4E36-AE0D-10782836D69D","9C204CD0-1233-41C5-818A-C5DA439445AA","Next Button Text","NextButtonText","","",13,@"Next","345583A1-BDBA-4642-9EFC-2DE07AC07561");
            // Attrib Value for Block:Person Select, Attribute:Next Button Text Page: Person Select (Family Check-in), Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("A121AC90-9E70-4C8F-B57F-2256CF57714A","345583A1-BDBA-4642-9EFC-2DE07AC07561",@"Next");
            // Attrib Value for Block:Person Select, Attribute:Title Page: Person Select (Family Check-in), Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("A121AC90-9E70-4C8F-B57F-2256CF57714A","D2330B74-D772-4144-812C-1B35A080A033",@"{0}");
            // Attrib Value for Block:Person Select, Attribute:Caption Page: Person Select (Family Check-in), Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("A121AC90-9E70-4C8F-B57F-2256CF57714A","E831CC6C-78EE-4D6A-BAD4-44B9519BAA10",@"Select People");
            // Attrib Value for Block:Person Select, Attribute:Option Title Page: Person Select (Family Check-in), Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("A121AC90-9E70-4C8F-B57F-2256CF57714A","53B77FFC-E9C5-43C7-8D86-2DD51E62EA0B",@"{0}");
            // Attrib Value for Block:Person Select, Attribute:Pre-Selected Options Format Page: Person Select (Family Check-in), Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("A121AC90-9E70-4C8F-B57F-2256CF57714A","194F9B56-2A6D-4D0C-BC41-97B16946A81B",@"
<div>{{ Location.Name }} - {{ Schedule.Name }}</div>
<div>{{ Group.Name }}</div>");
            // Attrib Value for Block:Person Select, Attribute:No Option Message Page: Person Select (Family Check-in), Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("A121AC90-9E70-4C8F-B57F-2256CF57714A","41D2B172-6CC5-480A-B76F-76A80A1135CD",@"Sorry, there are currently not any available areas that the selected people can check into.");
            // Attrib Value for Block:Person Select, Attribute:Auto Select Next Page Page: Person Select (Family Check-in), Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("A121AC90-9E70-4C8F-B57F-2256CF57714A","8DAB8E81-3C19-4DAB-8F9E-6A0892375D2B",@"7a6413bd-5338-425c-801e-4dbb12913c73");
            // Attrib Value for Block:Person Select, Attribute:Option Sub Title Page: Person Select (Family Check-in), Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("A121AC90-9E70-4C8F-B57F-2256CF57714A","D2345110-06FE-4964-A98C-BE897D3A325D",@"Please select the options that {0} would like to attend.");
            // Attrib Value for Block:Person Select, Attribute:Auto Select Done Page Page: Person Select (Family Check-in), Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("A121AC90-9E70-4C8F-B57F-2256CF57714A","0ADE3737-78D3-49BA-8B10-04223B127242",@"b7e527b9-8c7e-4f6c-afbf-464dec3f63a4");
            // Attrib Value for Block:Person Select, Attribute:Workflow Type Page: Person Select (Family Check-in), Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("A121AC90-9E70-4C8F-B57F-2256CF57714A","E53CAF3F-70D8-4968-AB60-37146DF9D61F",@"a0bbc045-00e5-4485-88cb-69a73ac7c78d");
            // Attrib Value for Block:Person Select, Attribute:Workflow Activity Page: Person Select (Family Check-in), Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("A121AC90-9E70-4C8F-B57F-2256CF57714A","C16FD48E-1D3E-4582-A7C3-BC54865EBDEF",@"Load Schedules");
            // Attrib Value for Block:Person Select, Attribute:Home Page Page: Person Select (Family Check-in), Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("A121AC90-9E70-4C8F-B57F-2256CF57714A","E0B65EBE-7D9D-495C-94E0-B6D06C360038",@"1610b488-1ad8-44f5-aabd-96c259c02b09");
            // Attrib Value for Block:Person Select, Attribute:Previous Page Page: Person Select (Family Check-in), Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("A121AC90-9E70-4C8F-B57F-2256CF57714A","91418009-9856-4CE4-8C18-CF2BA0EC1CCF",@"f8caf8f2-081b-4b45-bd45-1aa16de1ad81");
            // Attrib Value for Block:Person Select, Attribute:Next Page Page: Person Select (Family Check-in), Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("A121AC90-9E70-4C8F-B57F-2256CF57714A","78B1BA12-B331-4A5E-B9CE-CD5F6592AB23",@"5a2f46ab-c2bb-4d8d-ab7b-55e83eeec5ef");
            // Attrib Value for Block:Idle Redirect, Attribute:Idle Seconds Page: Person Select (Family Check-in), Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("E316B493-B752-4B3F-AE92-F5D7BB1C47E2","1CAC7B16-041A-4F40-8AEE-A39DFA076C14",@"45");
            // Attrib Value for Block:Idle Redirect, Attribute:New Location Page: Person Select (Family Check-in), Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("E316B493-B752-4B3F-AE92-F5D7BB1C47E2","2254B67B-9CB1-47DE-A63D-D0B56051ECD4",@"/lcbc-checkin/welcome");
            // Attrib Value for Block:Edit Family, Attribute:Workflow Activity Page: Person Select (Family Check-in), Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("CEF09175-8B7A-4652-BDE7-39CEFE1CB548","9B1B49A1-716D-4B5C-A75E-D39B681207AB",@"Person Search");
            // Attrib Value for Block:Edit Family, Attribute:Home Page Page: Person Select (Family Check-in), Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("CEF09175-8B7A-4652-BDE7-39CEFE1CB548","808EA130-EDED-45FD-9683-A5A26859128F",@"1610b488-1ad8-44f5-aabd-96c259c02b09");
            // Attrib Value for Block:Edit Family, Attribute:Workflow Type Page: Person Select (Family Check-in), Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("CEF09175-8B7A-4652-BDE7-39CEFE1CB548","C7C8C51E-B5A0-49A5-96F3-CB23BB5F81AB",@"a0bbc045-00e5-4485-88cb-69a73ac7c78d");



            // Page: Time Select (Family Check-in)
            RockMigrationHelper.AddPage("132DE89E-2556-45DE-A8CE-A77F4BC2EC79","23B01AB5-3AF2-46BC-8A75-BAF8CCC1C9E0","Time Select (Family Check-in)","","5A2F46AB-C2BB-4D8D-AB7B-55E83EEEC5EF",""); // Site:LCBC Check-in
            RockMigrationHelper.UpdateBlockType("Time Select","Displays a list of times to checkin for.","~/Blocks/CheckIn/TimeSelect.ascx","Check-in","D2348D51-B13A-4069-97AD-369D9615A711");
            RockMigrationHelper.UpdateBlockType("Idle Redirect","Redirects user to a new url after a specific number of idle seconds.","~/Blocks/Utility/IdleRedirect.ascx","Utility","49FC4B38-741E-4B0B-B395-7C1929340D88");
            // Add Block to Page: Time Select (Family Check-in), Site: LCBC Check-in
            RockMigrationHelper.AddBlock( true, "5A2F46AB-C2BB-4D8D-AB7B-55E83EEEC5EF","","D2348D51-B13A-4069-97AD-369D9615A711","Time Select","Main","","",0,"AACAC91D-E2DD-47F8-A4A4-A0932AAD5826"); 
            // Add Block to Page: Time Select (Family Check-in), Site: LCBC Check-in
            RockMigrationHelper.AddBlock( true, "5A2F46AB-C2BB-4D8D-AB7B-55E83EEEC5EF","","49FC4B38-741E-4B0B-B395-7C1929340D88","Idle Redirect","Main","","",1,"0A69DDAE-359F-48A2-9E2A-206AD11A27FA"); 
            // Attrib for BlockType: Time Select:Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute("D2348D51-B13A-4069-97AD-369D9615A711","46A03F59-55D3-4ACE-ADD5-B4642225DD20","Workflow Type","WorkflowType","","The workflow type to activate for check-in",0,@"","108E2E9E-DC18-4D5D-80FA-5D4A90FFCE65");
            // Attrib for BlockType: Idle Redirect:Idle Seconds
            RockMigrationHelper.UpdateBlockTypeAttribute("49FC4B38-741E-4B0B-B395-7C1929340D88","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Idle Seconds","IdleSeconds","","How many seconds of idle time to wait before redirecting user",0,@"20","1CAC7B16-041A-4F40-8AEE-A39DFA076C14");
            // Attrib for BlockType: Idle Redirect:New Location
            RockMigrationHelper.UpdateBlockTypeAttribute("49FC4B38-741E-4B0B-B395-7C1929340D88","9C204CD0-1233-41C5-818A-C5DA439445AA","New Location","NewLocation","","The new location URL to send user to after idle time",0,@"","2254B67B-9CB1-47DE-A63D-D0B56051ECD4");
            // Attrib for BlockType: Time Select:Workflow Activity
            RockMigrationHelper.UpdateBlockTypeAttribute("D2348D51-B13A-4069-97AD-369D9615A711","9C204CD0-1233-41C5-818A-C5DA439445AA","Workflow Activity","WorkflowActivity","","The name of the workflow activity to run on selection.",1,@"","12DF930E-6460-4A66-9326-E39BEAFC6F9D");
            // Attrib for BlockType: Time Select:Home Page
            RockMigrationHelper.UpdateBlockTypeAttribute("D2348D51-B13A-4069-97AD-369D9615A711","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Home Page","HomePage","","",2,@"","D5AFB471-3EE2-44D5-BC66-F4EFD26FD394");
            // Attrib for BlockType: Time Select:Previous Page
            RockMigrationHelper.UpdateBlockTypeAttribute("D2348D51-B13A-4069-97AD-369D9615A711","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Previous Page","PreviousPage","","",3,@"","DE808D50-0861-4E24-A483-F1C74C1FFDE8");
            // Attrib for BlockType: Time Select:Next Page
            RockMigrationHelper.UpdateBlockTypeAttribute("D2348D51-B13A-4069-97AD-369D9615A711","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Next Page","NextPage","","",4,@"","840898DB-A9AB-45C9-9894-0A1E816EFC4C");
            // Attrib for BlockType: Time Select:Title
            RockMigrationHelper.UpdateBlockTypeAttribute("D2348D51-B13A-4069-97AD-369D9615A711","9C204CD0-1233-41C5-818A-C5DA439445AA","Title","Title","","Title to display. Use {0} for family/person name.",5,@"{0}","B5CF8A58-92E8-4473-BE73-63FB3B6FF49E");
            // Attrib for BlockType: Time Select:Sub Title
            RockMigrationHelper.UpdateBlockTypeAttribute("D2348D51-B13A-4069-97AD-369D9615A711","9C204CD0-1233-41C5-818A-C5DA439445AA","Sub Title","SubTitle","","Sub-Title to display. Use {0} for selected group/location name.",6,@"{0}","2BE3BCC3-7307-4DA3-BF4A-4B8E4A5C9837");
            // Attrib for BlockType: Time Select:Caption
            RockMigrationHelper.UpdateBlockTypeAttribute("D2348D51-B13A-4069-97AD-369D9615A711","9C204CD0-1233-41C5-818A-C5DA439445AA","Caption","Caption","","",7,@"Select Time(s)","16091831-474A-4618-872F-E9257F7E9948");
            // Attrib Value for Block:Time Select, Attribute:Workflow Type Page: Time Select (Family Check-in), Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("AACAC91D-E2DD-47F8-A4A4-A0932AAD5826","108E2E9E-DC18-4D5D-80FA-5D4A90FFCE65",@"a0bbc045-00e5-4485-88cb-69a73ac7c78d");
            // Attrib Value for Block:Time Select, Attribute:Home Page Page: Time Select (Family Check-in), Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("AACAC91D-E2DD-47F8-A4A4-A0932AAD5826","D5AFB471-3EE2-44D5-BC66-F4EFD26FD394",@"1610b488-1ad8-44f5-aabd-96c259c02b09");
            // Attrib Value for Block:Time Select, Attribute:Next Page Page: Time Select (Family Check-in), Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("AACAC91D-E2DD-47F8-A4A4-A0932AAD5826","840898DB-A9AB-45C9-9894-0A1E816EFC4C",@"b0799456-2597-4001-8a5b-58b0a58dd42a");
            // Attrib Value for Block:Time Select, Attribute:Previous Page Page: Time Select (Family Check-in), Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("AACAC91D-E2DD-47F8-A4A4-A0932AAD5826","DE808D50-0861-4E24-A483-F1C74C1FFDE8",@"baed4080-0afc-4d2d-8748-659aa9a28d24");
            // Attrib Value for Block:Time Select, Attribute:Workflow Activity Page: Time Select (Family Check-in), Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("AACAC91D-E2DD-47F8-A4A4-A0932AAD5826","12DF930E-6460-4A66-9326-E39BEAFC6F9D",@"Schedule Select");
            // Attrib Value for Block:Time Select, Attribute:Title Page: Time Select (Family Check-in), Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("AACAC91D-E2DD-47F8-A4A4-A0932AAD5826","B5CF8A58-92E8-4473-BE73-63FB3B6FF49E",@"{0}");
            // Attrib Value for Block:Time Select, Attribute:Sub Title Page: Time Select (Family Check-in), Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("AACAC91D-E2DD-47F8-A4A4-A0932AAD5826","2BE3BCC3-7307-4DA3-BF4A-4B8E4A5C9837",@"{0}");
            // Attrib Value for Block:Time Select, Attribute:Caption Page: Time Select (Family Check-in), Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("AACAC91D-E2DD-47F8-A4A4-A0932AAD5826","16091831-474A-4618-872F-E9257F7E9948",@"Select Time(s)");
            // Attrib Value for Block:Idle Redirect, Attribute:Idle Seconds Page: Time Select (Family Check-in), Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("0A69DDAE-359F-48A2-9E2A-206AD11A27FA","1CAC7B16-041A-4F40-8AEE-A39DFA076C14",@"20");
            // Attrib Value for Block:Idle Redirect, Attribute:New Location Page: Time Select (Family Check-in), Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("0A69DDAE-359F-48A2-9E2A-206AD11A27FA","2254B67B-9CB1-47DE-A63D-D0B56051ECD4",@"/lcbc-checkin/welcome");



            // Page: Save Attendance (Family Check-in)
            RockMigrationHelper.AddPage("132DE89E-2556-45DE-A8CE-A77F4BC2EC79","23B01AB5-3AF2-46BC-8A75-BAF8CCC1C9E0","Save Attendance (Family Check-in)","","B7E527B9-8C7E-4F6C-AFBF-464DEC3F63A4",""); // Site:LCBC Check-in
            RockMigrationHelper.UpdateBlockType("Idle Redirect","Redirects user to a new url after a specific number of idle seconds.","~/Blocks/Utility/IdleRedirect.ascx","Utility","49FC4B38-741E-4B0B-B395-7C1929340D88");
            RockMigrationHelper.UpdateBlockType("Process Only","Provides a page for simply launching a check-in workflow action","~/Blocks/CheckIn/ProcessOnly.ascx","Check-in","F7B86942-9BF2-4132-B5EB-C7310952ECFF");
            // Add Block to Page: Save Attendance (Family Check-in), Site: LCBC Check-in
            RockMigrationHelper.AddBlock( true, "B7E527B9-8C7E-4F6C-AFBF-464DEC3F63A4","","F7B86942-9BF2-4132-B5EB-C7310952ECFF","Process Only","Main","","",0,"1DE48EAE-F9FA-429C-BC26-EE9699A148C1"); 
            // Add Block to Page: Save Attendance (Family Check-in), Site: LCBC Check-in
            RockMigrationHelper.AddBlock( true, "B7E527B9-8C7E-4F6C-AFBF-464DEC3F63A4","","49FC4B38-741E-4B0B-B395-7C1929340D88","Idle Redirect","Main","","",1,"D870749E-41F2-47F7-850C-0234A242087D"); 
            // Attrib for BlockType: Idle Redirect:Idle Seconds
            RockMigrationHelper.UpdateBlockTypeAttribute("49FC4B38-741E-4B0B-B395-7C1929340D88","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Idle Seconds","IdleSeconds","","How many seconds of idle time to wait before redirecting user",0,@"20","1CAC7B16-041A-4F40-8AEE-A39DFA076C14");
            // Attrib for BlockType: Idle Redirect:New Location
            RockMigrationHelper.UpdateBlockTypeAttribute("49FC4B38-741E-4B0B-B395-7C1929340D88","9C204CD0-1233-41C5-818A-C5DA439445AA","New Location","NewLocation","","The new location URL to send user to after idle time",0,@"","2254B67B-9CB1-47DE-A63D-D0B56051ECD4");
            // Attrib for BlockType: Process Only:Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute("F7B86942-9BF2-4132-B5EB-C7310952ECFF","46A03F59-55D3-4ACE-ADD5-B4642225DD20","Workflow Type","WorkflowType","","The workflow type to activate for check-in",0,@"","BA9AD11A-DB90-4BF6-ACDA-6FFB56C0358A");
            // Attrib for BlockType: Process Only:Workflow Activity
            RockMigrationHelper.UpdateBlockTypeAttribute("F7B86942-9BF2-4132-B5EB-C7310952ECFF","9C204CD0-1233-41C5-818A-C5DA439445AA","Workflow Activity","WorkflowActivity","","The name of the workflow activity to run on selection.",1,@"","21715BAA-59CE-41F7-8D7B-925C8DB4F3DD");
            // Attrib for BlockType: Process Only:Home Page
            RockMigrationHelper.UpdateBlockTypeAttribute("F7B86942-9BF2-4132-B5EB-C7310952ECFF","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Home Page","HomePage","","",2,@"","44646AA4-0E73-4AE2-B456-B2F7E9C96BAE");
            // Attrib for BlockType: Process Only:Previous Page
            RockMigrationHelper.UpdateBlockTypeAttribute("F7B86942-9BF2-4132-B5EB-C7310952ECFF","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Previous Page","PreviousPage","","",3,@"","0320C949-F06B-49FF-A9E1-F686CB14841C");
            // Attrib for BlockType: Process Only:Next Page
            RockMigrationHelper.UpdateBlockTypeAttribute("F7B86942-9BF2-4132-B5EB-C7310952ECFF","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Next Page","NextPage","","",4,@"","BA7AB351-CF98-4846-90C2-62F5EE8D799C");
            // Attrib Value for Block:Process Only, Attribute:Workflow Type Page: Save Attendance (Family Check-in), Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("1DE48EAE-F9FA-429C-BC26-EE9699A148C1","BA9AD11A-DB90-4BF6-ACDA-6FFB56C0358A",@"a0bbc045-00e5-4485-88cb-69a73ac7c78d");
            // Attrib Value for Block:Process Only, Attribute:Workflow Activity Page: Save Attendance (Family Check-in), Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("1DE48EAE-F9FA-429C-BC26-EE9699A148C1","21715BAA-59CE-41F7-8D7B-925C8DB4F3DD",@"Save Attendance");
            // Attrib Value for Block:Process Only, Attribute:Home Page Page: Save Attendance (Family Check-in), Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("1DE48EAE-F9FA-429C-BC26-EE9699A148C1","44646AA4-0E73-4AE2-B456-B2F7E9C96BAE",@"1610b488-1ad8-44f5-aabd-96c259c02b09");
            // Attrib Value for Block:Process Only, Attribute:Previous Page Page: Save Attendance (Family Check-in), Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("1DE48EAE-F9FA-429C-BC26-EE9699A148C1","0320C949-F06B-49FF-A9E1-F686CB14841C",@"ca6b5be4-c193-4db7-8469-08429e6b2803");
            // Attrib Value for Block:Process Only, Attribute:Next Page Page: Save Attendance (Family Check-in), Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("1DE48EAE-F9FA-429C-BC26-EE9699A148C1","BA7AB351-CF98-4846-90C2-62F5EE8D799C",@"16cc3f78-9149-457e-9718-099bf5278ed0");
            // Attrib Value for Block:Idle Redirect, Attribute:Idle Seconds Page: Save Attendance (Family Check-in), Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("D870749E-41F2-47F7-850C-0234A242087D","1CAC7B16-041A-4F40-8AEE-A39DFA076C14",@"20");
            // Attrib Value for Block:Idle Redirect, Attribute:New Location Page: Save Attendance (Family Check-in), Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("D870749E-41F2-47F7-850C-0234A242087D","2254B67B-9CB1-47DE-A63D-D0B56051ECD4",@"/lcbc-checkin/welcome");



            // Page: Action Select
            RockMigrationHelper.AddPage("132DE89E-2556-45DE-A8CE-A77F4BC2EC79","23B01AB5-3AF2-46BC-8A75-BAF8CCC1C9E0","Action Select","","CC38AD88-B319-40AB-B9E7-D04F563224A5",""); // Site:LCBC Check-in
            RockMigrationHelper.UpdateBlockType("Idle Redirect","Redirects user to a new url after a specific number of idle seconds.","~/Blocks/Utility/IdleRedirect.ascx","Utility","49FC4B38-741E-4B0B-B395-7C1929340D88");
            RockMigrationHelper.UpdateBlockType("Action Select","Displays option for family to Check In or Check Out.","~/Blocks/CheckIn/ActionSelect.ascx","Check-in","66DDB050-8F60-4DF3-9AED-5CE283E22350");
            // Add Block to Page: Action Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlock( true, "CC38AD88-B319-40AB-B9E7-D04F563224A5","","66DDB050-8F60-4DF3-9AED-5CE283E22350","Action Select","Main","","",0,"7D95FE6D-D168-4C21-9BE9-D62A65AC1D1F"); 
            // Add Block to Page: Action Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlock( true, "CC38AD88-B319-40AB-B9E7-D04F563224A5","","49FC4B38-741E-4B0B-B395-7C1929340D88","Idle Redirect","Main","","",1,"9F3BD3DB-F7EA-4A5E-B295-6FF7F6CF6854"); 
            // Attrib for BlockType: Idle Redirect:Idle Seconds
            RockMigrationHelper.UpdateBlockTypeAttribute("49FC4B38-741E-4B0B-B395-7C1929340D88","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Idle Seconds","IdleSeconds","","How many seconds of idle time to wait before redirecting user",0,@"20","1CAC7B16-041A-4F40-8AEE-A39DFA076C14");
            // Attrib for BlockType: Idle Redirect:New Location
            RockMigrationHelper.UpdateBlockTypeAttribute("49FC4B38-741E-4B0B-B395-7C1929340D88","9C204CD0-1233-41C5-818A-C5DA439445AA","New Location","NewLocation","","The new location URL to send user to after idle time",0,@"","2254B67B-9CB1-47DE-A63D-D0B56051ECD4");
            // Attrib for BlockType: Action Select:Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute("66DDB050-8F60-4DF3-9AED-5CE283E22350","46A03F59-55D3-4ACE-ADD5-B4642225DD20","Workflow Type","WorkflowType","","The workflow type to activate for check-in",0,@"","31BAADBE-0E12-4EC4-B05D-472EBAD9C1B5");
            // Attrib for BlockType: Action Select:Workflow Activity
            RockMigrationHelper.UpdateBlockTypeAttribute("66DDB050-8F60-4DF3-9AED-5CE283E22350","9C204CD0-1233-41C5-818A-C5DA439445AA","Workflow Activity","WorkflowActivity","","The name of the workflow activity to run on selection.",1,@"","C0EEDB49-6B69-47B0-98DE-2A1A28188C5D");
            // Attrib for BlockType: Action Select:Home Page
            RockMigrationHelper.UpdateBlockTypeAttribute("66DDB050-8F60-4DF3-9AED-5CE283E22350","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Home Page","HomePage","","",2,@"","DE27E0C8-5BEF-48FE-88D9-3E8300B4988E");
            // Attrib for BlockType: Action Select:Previous Page
            RockMigrationHelper.UpdateBlockTypeAttribute("66DDB050-8F60-4DF3-9AED-5CE283E22350","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Previous Page","PreviousPage","","",3,@"","5751A6B9-1155-4BAC-BA2D-84C6A419D6E7");
            // Attrib for BlockType: Action Select:Next Page
            RockMigrationHelper.UpdateBlockTypeAttribute("66DDB050-8F60-4DF3-9AED-5CE283E22350","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Next Page","NextPage","","",4,@"","A161CC4A-F521-49A3-B648-165A7AE4EFE0");
            // Attrib for BlockType: Action Select:Next Page (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute("66DDB050-8F60-4DF3-9AED-5CE283E22350","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Next Page (Family Check-in)","FamilyNextPage","","",5,@"","83450920-66B3-46FD-AEA5-35EC43F96C9D");
            // Attrib for BlockType: Action Select:Check Out Page
            RockMigrationHelper.UpdateBlockTypeAttribute("66DDB050-8F60-4DF3-9AED-5CE283E22350","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Check Out Page","CheckOutPage","","",6,@"","F70CFDEC-1131-4127-A6B8-A1A9AEE02D71");
            // Attrib for BlockType: Action Select:Title
            RockMigrationHelper.UpdateBlockTypeAttribute("66DDB050-8F60-4DF3-9AED-5CE283E22350","9C204CD0-1233-41C5-818A-C5DA439445AA","Title","Title","","Title to display. Use {0} for family name",7,@"{0}","0D5B5B30-E1CE-402E-B7FB-760F8E4975B2");
            // Attrib for BlockType: Action Select:Caption
            RockMigrationHelper.UpdateBlockTypeAttribute("66DDB050-8F60-4DF3-9AED-5CE283E22350","9C204CD0-1233-41C5-818A-C5DA439445AA","Caption","Caption","","",8,@"Select Action","1E7D1586-9636-4144-9019-61DE1CFF576F");
            // Attrib Value for Block:Action Select, Attribute:Home Page Page: Action Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("7D95FE6D-D168-4C21-9BE9-D62A65AC1D1F","DE27E0C8-5BEF-48FE-88D9-3E8300B4988E",@"1610b488-1ad8-44f5-aabd-96c259c02b09");
            // Attrib Value for Block:Action Select, Attribute:Previous Page Page: Action Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("7D95FE6D-D168-4C21-9BE9-D62A65AC1D1F","5751A6B9-1155-4BAC-BA2D-84C6A419D6E7",@"f8caf8f2-081b-4b45-bd45-1aa16de1ad81");
            // Attrib Value for Block:Action Select, Attribute:Next Page Page: Action Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("7D95FE6D-D168-4C21-9BE9-D62A65AC1D1F","A161CC4A-F521-49A3-B648-165A7AE4EFE0",@"4a162cdd-fb14-4599-a8f7-c5ca9011e321");
            // Attrib Value for Block:Action Select, Attribute:Next Page (Family Check-in) Page: Action Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("7D95FE6D-D168-4C21-9BE9-D62A65AC1D1F","83450920-66B3-46FD-AEA5-35EC43F96C9D",@"baed4080-0afc-4d2d-8748-659aa9a28d24");
            // Attrib Value for Block:Action Select, Attribute:Check Out Page Page: Action Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("7D95FE6D-D168-4C21-9BE9-D62A65AC1D1F","F70CFDEC-1131-4127-A6B8-A1A9AEE02D71",@"c094ca3f-1fd0-4e73-b1bb-f53f19029198");
            // Attrib Value for Block:Action Select, Attribute:Caption Page: Action Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("7D95FE6D-D168-4C21-9BE9-D62A65AC1D1F","1E7D1586-9636-4144-9019-61DE1CFF576F",@"Select Action");
            // Attrib Value for Block:Action Select, Attribute:Title Page: Action Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("7D95FE6D-D168-4C21-9BE9-D62A65AC1D1F","0D5B5B30-E1CE-402E-B7FB-760F8E4975B2",@"{0}");
            // Attrib Value for Block:Idle Redirect, Attribute:Idle Seconds Page: Action Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("9F3BD3DB-F7EA-4A5E-B295-6FF7F6CF6854","1CAC7B16-041A-4F40-8AEE-A39DFA076C14",@"20");
            // Attrib Value for Block:Idle Redirect, Attribute:New Location Page: Action Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("9F3BD3DB-F7EA-4A5E-B295-6FF7F6CF6854","2254B67B-9CB1-47DE-A63D-D0B56051ECD4",@"/lcbc-checkin/welcome");



            // Page: Check Out Person Select
            RockMigrationHelper.AddPage("132DE89E-2556-45DE-A8CE-A77F4BC2EC79","23B01AB5-3AF2-46BC-8A75-BAF8CCC1C9E0","Check Out Person Select","","C094CA3F-1FD0-4E73-B1BB-F53F19029198",""); // Site:LCBC Check-in
            RockMigrationHelper.UpdateBlockType("Idle Redirect","Redirects user to a new url after a specific number of idle seconds.","~/Blocks/Utility/IdleRedirect.ascx","Utility","49FC4B38-741E-4B0B-B395-7C1929340D88");
            RockMigrationHelper.UpdateBlockType("Check Out Person Select","Lists people who match the selected family and provides option of selecting multiple people to check-out.","~/Blocks/CheckIn/CheckOutPersonSelect.ascx","Check-in","54EB0252-6FE4-49C5-8716-14A3A06C3EC5");
            // Add Block to Page: Check Out Person Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlock( true, "C094CA3F-1FD0-4E73-B1BB-F53F19029198","","54EB0252-6FE4-49C5-8716-14A3A06C3EC5","Check Out Person Select","Main","","",0,"F23E2D2B-9A5F-4866-9EA1-7AB611A1E413"); 
            // Add Block to Page: Check Out Person Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlock( true, "C094CA3F-1FD0-4E73-B1BB-F53F19029198","","49FC4B38-741E-4B0B-B395-7C1929340D88","Idle Redirect","Main","","",1,"F6C3DC7B-9343-4C6D-8308-B33BE512A6DB"); 
            // Attrib for BlockType: Check Out Person Select:Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute("54EB0252-6FE4-49C5-8716-14A3A06C3EC5","46A03F59-55D3-4ACE-ADD5-B4642225DD20","Workflow Type","WorkflowType","","The workflow type to activate for check-in",0,@"","E7FCFB35-0172-46DB-A38F-6C54BCA49A6A");
            // Attrib for BlockType: Idle Redirect:Idle Seconds
            RockMigrationHelper.UpdateBlockTypeAttribute("49FC4B38-741E-4B0B-B395-7C1929340D88","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Idle Seconds","IdleSeconds","","How many seconds of idle time to wait before redirecting user",0,@"20","1CAC7B16-041A-4F40-8AEE-A39DFA076C14");
            // Attrib for BlockType: Idle Redirect:New Location
            RockMigrationHelper.UpdateBlockTypeAttribute("49FC4B38-741E-4B0B-B395-7C1929340D88","9C204CD0-1233-41C5-818A-C5DA439445AA","New Location","NewLocation","","The new location URL to send user to after idle time",0,@"","2254B67B-9CB1-47DE-A63D-D0B56051ECD4");
            // Attrib for BlockType: Check Out Person Select:Workflow Activity
            RockMigrationHelper.UpdateBlockTypeAttribute("54EB0252-6FE4-49C5-8716-14A3A06C3EC5","9C204CD0-1233-41C5-818A-C5DA439445AA","Workflow Activity","WorkflowActivity","","The name of the workflow activity to run on selection.",1,@"","A4FB35E5-8A62-47FE-AE49-6E447DA8CF82");
            // Attrib for BlockType: Check Out Person Select:Home Page
            RockMigrationHelper.UpdateBlockTypeAttribute("54EB0252-6FE4-49C5-8716-14A3A06C3EC5","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Home Page","HomePage","","",2,@"","843FB186-90E8-4DCE-B138-B23E891E2CFF");
            // Attrib for BlockType: Check Out Person Select:Previous Page
            RockMigrationHelper.UpdateBlockTypeAttribute("54EB0252-6FE4-49C5-8716-14A3A06C3EC5","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Previous Page","PreviousPage","","",3,@"","67C256C2-753F-410B-B683-F64368AC8497");
            // Attrib for BlockType: Check Out Person Select:Next Page
            RockMigrationHelper.UpdateBlockTypeAttribute("54EB0252-6FE4-49C5-8716-14A3A06C3EC5","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Next Page","NextPage","","",4,@"","18BAC460-2630-4651-A320-7927A3078A87");
            // Attrib for BlockType: Check Out Person Select:Title
            RockMigrationHelper.UpdateBlockTypeAttribute("54EB0252-6FE4-49C5-8716-14A3A06C3EC5","9C204CD0-1233-41C5-818A-C5DA439445AA","Title","Title","","Title to display. Use {0} for family name",5,@"{0} Check Out","05CA1B42-C5F1-407C-9F35-2CF4104BC96D");
            // Attrib for BlockType: Check Out Person Select:Caption
            RockMigrationHelper.UpdateBlockTypeAttribute("54EB0252-6FE4-49C5-8716-14A3A06C3EC5","9C204CD0-1233-41C5-818A-C5DA439445AA","Caption","Caption","","",6,@"Select People","45B1D388-9DBC-4A8A-8AC7-70423E672624");
            // Attrib Value for Block:Check Out Person Select, Attribute:Workflow Type Page: Check Out Person Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("F23E2D2B-9A5F-4866-9EA1-7AB611A1E413","E7FCFB35-0172-46DB-A38F-6C54BCA49A6A",@"a0bbc045-00e5-4485-88cb-69a73ac7c78d");
            // Attrib Value for Block:Check Out Person Select, Attribute:Workflow Activity Page: Check Out Person Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("F23E2D2B-9A5F-4866-9EA1-7AB611A1E413","A4FB35E5-8A62-47FE-AE49-6E447DA8CF82",@"Create Check-Out Labels");
            // Attrib Value for Block:Check Out Person Select, Attribute:Home Page Page: Check Out Person Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("F23E2D2B-9A5F-4866-9EA1-7AB611A1E413","843FB186-90E8-4DCE-B138-B23E891E2CFF",@"1610b488-1ad8-44f5-aabd-96c259c02b09");
            // Attrib Value for Block:Check Out Person Select, Attribute:Previous Page Page: Check Out Person Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("F23E2D2B-9A5F-4866-9EA1-7AB611A1E413","67C256C2-753F-410B-B683-F64368AC8497",@"cc38ad88-b319-40ab-b9e7-d04f563224a5");
            // Attrib Value for Block:Check Out Person Select, Attribute:Next Page Page: Check Out Person Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("F23E2D2B-9A5F-4866-9EA1-7AB611A1E413","18BAC460-2630-4651-A320-7927A3078A87",@"36a7f8ac-5a66-40cc-b175-ba0114e42622");
            // Attrib Value for Block:Check Out Person Select, Attribute:Caption Page: Check Out Person Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("F23E2D2B-9A5F-4866-9EA1-7AB611A1E413","45B1D388-9DBC-4A8A-8AC7-70423E672624",@"Select People");
            // Attrib Value for Block:Check Out Person Select, Attribute:Title Page: Check Out Person Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("F23E2D2B-9A5F-4866-9EA1-7AB611A1E413","05CA1B42-C5F1-407C-9F35-2CF4104BC96D",@"{0} Check Out");
            // Attrib Value for Block:Idle Redirect, Attribute:Idle Seconds Page: Check Out Person Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("F6C3DC7B-9343-4C6D-8308-B33BE512A6DB","1CAC7B16-041A-4F40-8AEE-A39DFA076C14",@"20");
            // Attrib Value for Block:Idle Redirect, Attribute:New Location Page: Check Out Person Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("F6C3DC7B-9343-4C6D-8308-B33BE512A6DB","2254B67B-9CB1-47DE-A63D-D0B56051ECD4",@"/lcbc-checkin/welcome");



            // Page: Check Out Success
            RockMigrationHelper.AddPage("132DE89E-2556-45DE-A8CE-A77F4BC2EC79","23B01AB5-3AF2-46BC-8A75-BAF8CCC1C9E0","Check Out Success","","36A7F8AC-5A66-40CC-B175-BA0114E42622",""); // Site:LCBC Check-in
            RockMigrationHelper.UpdateBlockType("Idle Redirect","Redirects user to a new url after a specific number of idle seconds.","~/Blocks/Utility/IdleRedirect.ascx","Utility","49FC4B38-741E-4B0B-B395-7C1929340D88");
            RockMigrationHelper.UpdateBlockType("Check Out Success","Displays the details of a successful check out.","~/Blocks/CheckIn/CheckoutSuccess.ascx","Check-in","F499C4A9-9A60-404B-9383-B950EE6D7821");
            // Add Block to Page: Check Out Success, Site: LCBC Check-in
            RockMigrationHelper.AddBlock( true, "36A7F8AC-5A66-40CC-B175-BA0114E42622","","49FC4B38-741E-4B0B-B395-7C1929340D88","Idle Redirect","Main","","",1,"D54699AB-CFCF-48F2-A225-4AD2A2DF8F92"); 
            // Add Block to Page: Check Out Success, Site: LCBC Check-in
            RockMigrationHelper.AddBlock( true, "36A7F8AC-5A66-40CC-B175-BA0114E42622","","F499C4A9-9A60-404B-9383-B950EE6D7821","Check Out Success","Main","","",0,"A614555F-1610-4BA6-8FEB-5DB0087DD90E"); 
            // Attrib for BlockType: Idle Redirect:Idle Seconds
            RockMigrationHelper.UpdateBlockTypeAttribute("49FC4B38-741E-4B0B-B395-7C1929340D88","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Idle Seconds","IdleSeconds","","How many seconds of idle time to wait before redirecting user",0,@"20","1CAC7B16-041A-4F40-8AEE-A39DFA076C14");
            // Attrib for BlockType: Idle Redirect:New Location
            RockMigrationHelper.UpdateBlockTypeAttribute("49FC4B38-741E-4B0B-B395-7C1929340D88","9C204CD0-1233-41C5-818A-C5DA439445AA","New Location","NewLocation","","The new location URL to send user to after idle time",0,@"","2254B67B-9CB1-47DE-A63D-D0B56051ECD4");
            // Attrib for BlockType: Check Out Success:Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute("F499C4A9-9A60-404B-9383-B950EE6D7821","46A03F59-55D3-4ACE-ADD5-B4642225DD20","Workflow Type","WorkflowType","","The workflow type to activate for check-in",0,@"","7328D95B-D9BB-49D0-943B-B374EBC664DD");
            // Attrib for BlockType: Check Out Success:Workflow Activity
            RockMigrationHelper.UpdateBlockTypeAttribute("F499C4A9-9A60-404B-9383-B950EE6D7821","9C204CD0-1233-41C5-818A-C5DA439445AA","Workflow Activity","WorkflowActivity","","The name of the workflow activity to run on selection.",1,@"","76C4F5AC-7EA8-45ED-8B7C-1974361FDEE5");
            // Attrib for BlockType: Check Out Success:Home Page
            RockMigrationHelper.UpdateBlockTypeAttribute("F499C4A9-9A60-404B-9383-B950EE6D7821","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Home Page","HomePage","","",2,@"","ADA3C354-42ED-4F28-8F68-38FBC2926CBF");
            // Attrib for BlockType: Check Out Success:Previous Page
            RockMigrationHelper.UpdateBlockTypeAttribute("F499C4A9-9A60-404B-9383-B950EE6D7821","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Previous Page","PreviousPage","","",3,@"","735D4AAB-F8F4-4388-9A00-2132356187A6");
            // Attrib for BlockType: Check Out Success:Next Page
            RockMigrationHelper.UpdateBlockTypeAttribute("F499C4A9-9A60-404B-9383-B950EE6D7821","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Next Page","NextPage","","",4,@"","223C5BA3-B6B0-4EC6-9D38-5607837410D6");
            // Attrib for BlockType: Check Out Success:Title
            RockMigrationHelper.UpdateBlockTypeAttribute("F499C4A9-9A60-404B-9383-B950EE6D7821","9C204CD0-1233-41C5-818A-C5DA439445AA","Title","Title","","Title to display.",5,@"Checked Out","3784B16E-BF23-42A4-8E04-0E93EB71C0D4");
            // Attrib for BlockType: Check Out Success:Detail Message
            RockMigrationHelper.UpdateBlockTypeAttribute("F499C4A9-9A60-404B-9383-B950EE6D7821","9C204CD0-1233-41C5-818A-C5DA439445AA","Detail Message","DetailMessage","","The message to display indicating person has been checked out. Use {0} for person, {1} for group, {2} for location, and {3} for schedule.",6,@"{0} was checked out of {1} in {2} at {3}.","A6C1FF95-43D8-4602-9175-B6F0B0523E61");
            // Attrib Value for Block:Idle Redirect, Attribute:Idle Seconds Page: Check Out Success, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("D54699AB-CFCF-48F2-A225-4AD2A2DF8F92","1CAC7B16-041A-4F40-8AEE-A39DFA076C14",@"20");
            // Attrib Value for Block:Idle Redirect, Attribute:New Location Page: Check Out Success, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("D54699AB-CFCF-48F2-A225-4AD2A2DF8F92","2254B67B-9CB1-47DE-A63D-D0B56051ECD4",@"/lcbc-checkin/welcome");
            // Attrib Value for Block:Check Out Success, Attribute:Home Page Page: Check Out Success, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("A614555F-1610-4BA6-8FEB-5DB0087DD90E","ADA3C354-42ED-4F28-8F68-38FBC2926CBF",@"1610b488-1ad8-44f5-aabd-96c259c02b09");
            // Attrib Value for Block:Check Out Success, Attribute:Previous Page Page: Check Out Success, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("A614555F-1610-4BA6-8FEB-5DB0087DD90E","735D4AAB-F8F4-4388-9A00-2132356187A6",@"c094ca3f-1fd0-4e73-b1bb-f53f19029198");
            // Attrib Value for Block:Check Out Success, Attribute:Title Page: Check Out Success, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("A614555F-1610-4BA6-8FEB-5DB0087DD90E","3784B16E-BF23-42A4-8E04-0E93EB71C0D4",@"Checked Out");
            // Attrib Value for Block:Check Out Success, Attribute:Detail Message Page: Check Out Success, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("A614555F-1610-4BA6-8FEB-5DB0087DD90E","A6C1FF95-43D8-4602-9175-B6F0B0523E61",@"{0} was checked out of {1} in {2} at {3}.");



            // Page: Pager Select
            RockMigrationHelper.AddPage("132DE89E-2556-45DE-A8CE-A77F4BC2EC79","23B01AB5-3AF2-46BC-8A75-BAF8CCC1C9E0","Pager Select","","D07AACC7-F2B3-4BF2-96EF-91BB9394D47C",""); // Site:LCBC Check-in
            RockMigrationHelper.UpdateBlockType("Idle Redirect","Redirects user to a new url after a specific number of idle seconds.","~/Blocks/Utility/IdleRedirect.ascx","Utility","49FC4B38-741E-4B0B-B395-7C1929340D88");
            RockMigrationHelper.UpdateBlockType("Pager Select","Displays a text box to enter a pager number","~/Plugins/com_bemadev/Checkin/PagerSelect.ascx","com_bemadev > Check-in","BF35D025-2BE8-4E10-B5F2-94F5E808326E");
            // Add Block to Page: Pager Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlock( true, "D07AACC7-F2B3-4BF2-96EF-91BB9394D47C","","BF35D025-2BE8-4E10-B5F2-94F5E808326E","Pager Select","Main","","",0,"80B948DE-D459-4D52-B1DA-A169BC628D7B"); 
            // Add Block to Page: Pager Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlock( true, "D07AACC7-F2B3-4BF2-96EF-91BB9394D47C","","49FC4B38-741E-4B0B-B395-7C1929340D88","Idle Redirect","Main","","",1,"3F5370A7-D7AE-4D20-BBF9-30DE7E926711"); 
            // Attrib for BlockType: Idle Redirect:Idle Seconds
            RockMigrationHelper.UpdateBlockTypeAttribute("49FC4B38-741E-4B0B-B395-7C1929340D88","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Idle Seconds","IdleSeconds","","How many seconds of idle time to wait before redirecting user",0,@"20","1CAC7B16-041A-4F40-8AEE-A39DFA076C14");
            // Attrib for BlockType: Idle Redirect:New Location
            RockMigrationHelper.UpdateBlockTypeAttribute("49FC4B38-741E-4B0B-B395-7C1929340D88","9C204CD0-1233-41C5-818A-C5DA439445AA","New Location","NewLocation","","The new location URL to send user to after idle time",0,@"","2254B67B-9CB1-47DE-A63D-D0B56051ECD4");
            // Attrib for BlockType: Pager Select:Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute("BF35D025-2BE8-4E10-B5F2-94F5E808326E","46A03F59-55D3-4ACE-ADD5-B4642225DD20","Workflow Type","WorkflowType","","The workflow type to activate for check-in",0,@"","60A53637-0F10-4A39-8A52-CFFE4EE8842B");
            // Attrib for BlockType: Pager Select:Workflow Activity
            RockMigrationHelper.UpdateBlockTypeAttribute("BF35D025-2BE8-4E10-B5F2-94F5E808326E","9C204CD0-1233-41C5-818A-C5DA439445AA","Workflow Activity","WorkflowActivity","","The name of the workflow activity to run on selection.",1,@"","58080898-0380-4AB4-8DE3-5D78249BD0AE");
            // Attrib for BlockType: Pager Select:Home Page
            RockMigrationHelper.UpdateBlockTypeAttribute("BF35D025-2BE8-4E10-B5F2-94F5E808326E","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Home Page","HomePage","","",2,@"","6AE01D26-E6DA-4284-802F-A62A51A3C0D8");
            // Attrib for BlockType: Pager Select:Previous Page
            RockMigrationHelper.UpdateBlockTypeAttribute("BF35D025-2BE8-4E10-B5F2-94F5E808326E","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Previous Page","PreviousPage","","",3,@"","1335CE7A-76CD-47AD-8BF3-D2A169F664EE");
            // Attrib for BlockType: Pager Select:Next Page
            RockMigrationHelper.UpdateBlockTypeAttribute("BF35D025-2BE8-4E10-B5F2-94F5E808326E","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Next Page","NextPage","","",4,@"","181F6788-F854-4176-80B7-BB70001B7BDC");
            // Attrib for BlockType: Pager Select:Title
            RockMigrationHelper.UpdateBlockTypeAttribute("BF35D025-2BE8-4E10-B5F2-94F5E808326E","9C204CD0-1233-41C5-818A-C5DA439445AA","Title","Title","","Title to display. Use {0} for person/schedule.",8,@"{0}","7CBCD581-E89A-48ED-A828-3B2C5A53B34A");
            // Attrib for BlockType: Pager Select:Sub Title
            RockMigrationHelper.UpdateBlockTypeAttribute("BF35D025-2BE8-4E10-B5F2-94F5E808326E","9C204CD0-1233-41C5-818A-C5DA439445AA","Sub Title","SubTitle","","Sub-Title to display. Use {0} for selected group name.",9,@"{0}","7DDEC250-E975-47A9-9DEC-61B9B8740EC7");
            // Attrib for BlockType: Pager Select:Caption
            RockMigrationHelper.UpdateBlockTypeAttribute("BF35D025-2BE8-4E10-B5F2-94F5E808326E","9C204CD0-1233-41C5-818A-C5DA439445AA","Caption","Caption","","",10,@"Select Location","A6D6BC40-858D-4020-99BA-9F1E28F9B73D");
            // Attrib for BlockType: Pager Select:No Option Message
            RockMigrationHelper.UpdateBlockTypeAttribute("BF35D025-2BE8-4E10-B5F2-94F5E808326E","9C204CD0-1233-41C5-818A-C5DA439445AA","No Option Message","NoOptionMessage","","Message to display when there are not any options available. Use {0} for person's name, and {1} for schedule name.",11,@"Sorry, there are currently not any available locations that {0} can check into at {1}.","98A73089-1A39-4C97-933B-48EF57C3BCA0");
            // Attrib for BlockType: Pager Select:No Option After Select Message
            RockMigrationHelper.UpdateBlockTypeAttribute("BF35D025-2BE8-4E10-B5F2-94F5E808326E","9C204CD0-1233-41C5-818A-C5DA439445AA","No Option After Select Message","NoOptionAfterSelectMessage","","Message to display when there are not any options available after location is selected. Use {0} for person's name",12,@"Sorry, based on your selection, there are currently not any available times that {0} can check into.","787246D1-9DE3-410F-9949-99D12B7C6F27");
            // Attrib for BlockType: Pager Select:Auto Select First Page
            RockMigrationHelper.UpdateBlockTypeAttribute("BF35D025-2BE8-4E10-B5F2-94F5E808326E","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Auto Select First Page","FamilyAutoSelectFirstPage","","The first page for each person during family check-in.",13,@"","8FE6374B-7E9E-49C7-A633-4B9B601F12DD");
            // Attrib for BlockType: Pager Select:Auto Select Done Page
            RockMigrationHelper.UpdateBlockTypeAttribute("BF35D025-2BE8-4E10-B5F2-94F5E808326E","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Auto Select Done Page","FamilyAutoSelectDonePage","","The page to navigate to once all people have checked in during family check-in.",14,@"","2ED7931E-CD8C-4F6D-B055-B38941277F2A");
            // Attrib Value for Block:Pager Select, Attribute:Workflow Type Page: Pager Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("80B948DE-D459-4D52-B1DA-A169BC628D7B","60A53637-0F10-4A39-8A52-CFFE4EE8842B",@"a0bbc045-00e5-4485-88cb-69a73ac7c78d");
            // Attrib Value for Block:Pager Select, Attribute:Home Page Page: Pager Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("80B948DE-D459-4D52-B1DA-A169BC628D7B","6AE01D26-E6DA-4284-802F-A62A51A3C0D8",@"1610b488-1ad8-44f5-aabd-96c259c02b09");
            // Attrib Value for Block:Pager Select, Attribute:Previous Page Page: Pager Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("80B948DE-D459-4D52-B1DA-A169BC628D7B","1335CE7A-76CD-47AD-8BF3-D2A169F664EE",@"7a6413bd-5338-425c-801e-4dbb12913c73");
            // Attrib Value for Block:Pager Select, Attribute:Next Page Page: Pager Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("80B948DE-D459-4D52-B1DA-A169BC628D7B","181F6788-F854-4176-80B7-BB70001B7BDC",@"ca6b5be4-c193-4db7-8469-08429e6b2803");
            // Attrib Value for Block:Pager Select, Attribute:Title Page: Pager Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("80B948DE-D459-4D52-B1DA-A169BC628D7B","7CBCD581-E89A-48ED-A828-3B2C5A53B34A",@"{0}");
            // Attrib Value for Block:Pager Select, Attribute:Sub Title Page: Pager Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("80B948DE-D459-4D52-B1DA-A169BC628D7B","7DDEC250-E975-47A9-9DEC-61B9B8740EC7",@"{0}");
            // Attrib Value for Block:Pager Select, Attribute:Caption Page: Pager Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("80B948DE-D459-4D52-B1DA-A169BC628D7B","A6D6BC40-858D-4020-99BA-9F1E28F9B73D",@"Enter Pager");
            // Attrib Value for Block:Pager Select, Attribute:No Option Message Page: Pager Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("80B948DE-D459-4D52-B1DA-A169BC628D7B","98A73089-1A39-4C97-933B-48EF57C3BCA0",@"Sorry, there are currently not any available locations that {0} can check into at {1}.");
            // Attrib Value for Block:Pager Select, Attribute:No Option After Select Message Page: Pager Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("80B948DE-D459-4D52-B1DA-A169BC628D7B","787246D1-9DE3-410F-9949-99D12B7C6F27",@"Sorry, based on your selection, there are currently not any available times that {0} can check into.");
            // Attrib Value for Block:Pager Select, Attribute:Auto Select First Page Page: Pager Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("80B948DE-D459-4D52-B1DA-A169BC628D7B","8FE6374B-7E9E-49C7-A633-4B9B601F12DD",@"7a6413bd-5338-425c-801e-4dbb12913c73");
            // Attrib Value for Block:Pager Select, Attribute:Auto Select Done Page Page: Pager Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("80B948DE-D459-4D52-B1DA-A169BC628D7B","2ED7931E-CD8C-4F6D-B055-B38941277F2A",@"b7e527b9-8c7e-4f6c-afbf-464dec3f63a4");
            // Attrib Value for Block:Idle Redirect, Attribute:Idle Seconds Page: Pager Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("3F5370A7-D7AE-4D20-BBF9-30DE7E926711","1CAC7B16-041A-4F40-8AEE-A39DFA076C14",@"120");
            // Attrib Value for Block:Idle Redirect, Attribute:New Location Page: Pager Select, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("3F5370A7-D7AE-4D20-BBF9-30DE7E926711","2254B67B-9CB1-47DE-A63D-D0B56051ECD4",@"/lcbc-checkin/welcome");



            // Page: Reprint Label
            RockMigrationHelper.AddPage("132DE89E-2556-45DE-A8CE-A77F4BC2EC79","8305704F-928D-4379-967A-253E576E0923","Reprint Label","","30AF1742-361E-4AAB-A7B8-E258095AEE61",""); // Site:Rock Check-in Manager
            RockMigrationHelper.UpdateBlockType("Reprint Labels","Used to quickly reprint a child's label","~/Plugins/com_bemadev/Checkin/ReprintLabel.ascx","BEMA Services > Check-in","03321528-2AD6-4F8D-8097-F923DD94532A");
            RockMigrationHelper.UpdateBlockType("Reprint Label Client","Used if the device prints from the client","~/Plugins/com_bemadev/Checkin/ReprintLabelClient.ascx","BEMA Services > Check-in","FEBA6349-7C21-462D-B834-3505BFAE2A26");
            // Add Block to Page: Reprint Label, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlock( true, "30AF1742-361E-4AAB-A7B8-E258095AEE61","","03321528-2AD6-4F8D-8097-F923DD94532A","Reprint Labels","Main","","",0,"9D31E0B1-1ED9-4E12-92AF-087D7463DDFC"); 
            // Add Block to Page: Reprint Label, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlock( true, "30AF1742-361E-4AAB-A7B8-E258095AEE61","","FEBA6349-7C21-462D-B834-3505BFAE2A26","Reprint Label Client","Main","","",1,"D71823F8-564D-4B0A-86E3-369F958F0A43"); 
            // Attrib for BlockType: Reprint Labels:Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute("03321528-2AD6-4F8D-8097-F923DD94532A","46A03F59-55D3-4ACE-ADD5-B4642225DD20","Workflow Type","WorkflowType","","The workflow type to activate for check-in",0,@"","B0CC25E3-7F21-4B05-A0FF-133FE0B2DB84");
            // Attrib for BlockType: Reprint Label Client:Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute("FEBA6349-7C21-462D-B834-3505BFAE2A26","46A03F59-55D3-4ACE-ADD5-B4642225DD20","Workflow Type","WorkflowType","","The workflow type to activate for check-in",0,@"","2EDDBD15-0897-43A4-BD16-3CE33F761F74");
            // Attrib for BlockType: Reprint Label Client:Workflow Activity
            RockMigrationHelper.UpdateBlockTypeAttribute("FEBA6349-7C21-462D-B834-3505BFAE2A26","9C204CD0-1233-41C5-818A-C5DA439445AA","Workflow Activity","WorkflowActivity","","The name of the workflow activity to run on selection.",1,@"","AE680D84-7950-45A3-95A0-2616177A2EC9");
            // Attrib for BlockType: Reprint Labels:Workflow Activity
            RockMigrationHelper.UpdateBlockTypeAttribute("03321528-2AD6-4F8D-8097-F923DD94532A","9C204CD0-1233-41C5-818A-C5DA439445AA","Workflow Activity","WorkflowActivity","","The name of the workflow activity to run on selection.",1,@"","A5BF7D1B-143A-4509-8DA1-2688BC2F381D");
            // Attrib for BlockType: Reprint Labels:Home Page
            RockMigrationHelper.UpdateBlockTypeAttribute("03321528-2AD6-4F8D-8097-F923DD94532A","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Home Page","HomePage","","",2,@"","26652D54-5C30-4E29-8759-3192A011A8C8");
            // Attrib for BlockType: Reprint Label Client:Home Page
            RockMigrationHelper.UpdateBlockTypeAttribute("FEBA6349-7C21-462D-B834-3505BFAE2A26","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Home Page","HomePage","","",2,@"","2FC73DFF-DEBF-4A55-8184-E499E8C15F67");
            // Attrib for BlockType: Reprint Label Client:Previous Page
            RockMigrationHelper.UpdateBlockTypeAttribute("FEBA6349-7C21-462D-B834-3505BFAE2A26","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Previous Page","PreviousPage","","",3,@"","2A81794C-C104-40D0-BBC6-CB199D106507");
            // Attrib for BlockType: Reprint Labels:Previous Page
            RockMigrationHelper.UpdateBlockTypeAttribute("03321528-2AD6-4F8D-8097-F923DD94532A","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Previous Page","PreviousPage","","",3,@"","782BB17E-4519-4958-A0AA-201C61922FE8");
            // Attrib for BlockType: Reprint Labels:Next Page
            RockMigrationHelper.UpdateBlockTypeAttribute("03321528-2AD6-4F8D-8097-F923DD94532A","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Next Page","NextPage","","",4,@"","03186D78-3B7C-49DB-AFB6-E605F8E2705D");
            // Attrib for BlockType: Reprint Label Client:Next Page
            RockMigrationHelper.UpdateBlockTypeAttribute("FEBA6349-7C21-462D-B834-3505BFAE2A26","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Next Page","NextPage","","",4,@"","F48EA93B-54C7-43D7-9E19-6CA94FA8A649");
            // Attrib for BlockType: Reprint Label Client:Multi-Person First Page (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute("FEBA6349-7C21-462D-B834-3505BFAE2A26","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Multi-Person First Page (Family Check-in)","MultiPersonFirstPage","","The first page for each person during family check-in.",5,@"","36149BD5-51BB-419E-B9E1-68536CEDEFEA");
            // Attrib for BlockType: Reprint Labels:Multi-Person First Page (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute("03321528-2AD6-4F8D-8097-F923DD94532A","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Multi-Person First Page (Family Check-in)","MultiPersonFirstPage","","The first page for each person during family check-in.",5,@"","6BB5F8B7-E58F-4DEF-A70F-8C57A7EB383B");
            // Attrib for BlockType: Reprint Labels:Multi-Person Last Page  (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute("03321528-2AD6-4F8D-8097-F923DD94532A","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Multi-Person Last Page  (Family Check-in)","MultiPersonLastPage","","The last page for each person during family check-in.",6,@"","F48A7930-8E72-4D2B-85E6-183B1A10B37C");
            // Attrib for BlockType: Reprint Label Client:Multi-Person Last Page  (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute("FEBA6349-7C21-462D-B834-3505BFAE2A26","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Multi-Person Last Page  (Family Check-in)","MultiPersonLastPage","","The last page for each person during family check-in.",6,@"","00EB5C99-7A3D-494E-9EBD-63EB3A11C9FC");
            // Attrib for BlockType: Reprint Label Client:Multi-Person Done Page (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute("FEBA6349-7C21-462D-B834-3505BFAE2A26","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Multi-Person Done Page (Family Check-in)","MultiPersonDonePage","","The page to navigate to once all people have checked in during family check-in.",7,@"","B33E44EA-D2BD-4E4D-960B-F18A6246697E");
            // Attrib for BlockType: Reprint Labels:Multi-Person Done Page (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute("03321528-2AD6-4F8D-8097-F923DD94532A","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Multi-Person Done Page (Family Check-in)","MultiPersonDonePage","","The page to navigate to once all people have checked in during family check-in.",7,@"","558B57E0-66B4-4C07-9881-10E6F81C6A9D");
            // Attrib Value for Block:Reprint Labels, Attribute:Home Page Page: Reprint Label, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlockAttributeValue("9D31E0B1-1ED9-4E12-92AF-087D7463DDFC","26652D54-5C30-4E29-8759-3192A011A8C8",@"1610b488-1ad8-44f5-aabd-96c259c02b09");



            // Page: Item Tags
            RockMigrationHelper.AddPage("132DE89E-2556-45DE-A8CE-A77F4BC2EC79","23B01AB5-3AF2-46BC-8A75-BAF8CCC1C9E0","Item Tags","","7A6413BD-5338-425C-801E-4DBB12913C73",""); // Site:LCBC Check-in
            RockMigrationHelper.UpdateBlockType("Idle Redirect","Redirects user to a new url after a specific number of idle seconds.","~/Blocks/Utility/IdleRedirect.ascx","Utility","49FC4B38-741E-4B0B-B395-7C1929340D88");
            RockMigrationHelper.UpdateBlockType("Item Tag Select","Displays a number box to enter how many item tags you would like printed.","~/Plugins/com_bemadev/Checkin/ItemTagSelect.ascx","com_bemadev > Check-in","09113783-54B7-4C9F-B626-218B0BA75646");
            // Add Block to Page: Item Tags, Site: LCBC Check-in
            RockMigrationHelper.AddBlock( true, "7A6413BD-5338-425C-801E-4DBB12913C73","","09113783-54B7-4C9F-B626-218B0BA75646","Item Tag Select","Main","","",0,"3589416D-4770-4C49-B3C8-718BDA7AC942"); 
            // Add Block to Page: Item Tags, Site: LCBC Check-in
            RockMigrationHelper.AddBlock( true, "7A6413BD-5338-425C-801E-4DBB12913C73","","49FC4B38-741E-4B0B-B395-7C1929340D88","Idle Redirect","Main","","",1,"1391B29F-2FC7-49DC-8A2C-2C9E4A73D274"); 
            // Attrib for BlockType: Idle Redirect:Idle Seconds
            RockMigrationHelper.UpdateBlockTypeAttribute("49FC4B38-741E-4B0B-B395-7C1929340D88","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Idle Seconds","IdleSeconds","","How many seconds of idle time to wait before redirecting user",0,@"20","1CAC7B16-041A-4F40-8AEE-A39DFA076C14");
            // Attrib for BlockType: Idle Redirect:New Location
            RockMigrationHelper.UpdateBlockTypeAttribute("49FC4B38-741E-4B0B-B395-7C1929340D88","9C204CD0-1233-41C5-818A-C5DA439445AA","New Location","NewLocation","","The new location URL to send user to after idle time",0,@"","2254B67B-9CB1-47DE-A63D-D0B56051ECD4");
            // Attrib for BlockType: Item Tag Select:Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute("09113783-54B7-4C9F-B626-218B0BA75646","46A03F59-55D3-4ACE-ADD5-B4642225DD20","Workflow Type","WorkflowType","","The workflow type to activate for check-in",0,@"","CAE5DBEF-AE31-42AF-BD8B-E6484F893A13");
            // Attrib for BlockType: Item Tag Select:Workflow Activity
            RockMigrationHelper.UpdateBlockTypeAttribute("09113783-54B7-4C9F-B626-218B0BA75646","9C204CD0-1233-41C5-818A-C5DA439445AA","Workflow Activity","WorkflowActivity","","The name of the workflow activity to run on selection.",1,@"","613FB891-9999-45FB-9AB9-A9D1719DB9C0");
            // Attrib for BlockType: Item Tag Select:Home Page
            RockMigrationHelper.UpdateBlockTypeAttribute("09113783-54B7-4C9F-B626-218B0BA75646","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Home Page","HomePage","","",2,@"","C01FAE24-5AA1-492C-9CC7-B8FC11E129DC");
            // Attrib for BlockType: Item Tag Select:Previous Page
            RockMigrationHelper.UpdateBlockTypeAttribute("09113783-54B7-4C9F-B626-218B0BA75646","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Previous Page","PreviousPage","","",3,@"","AC712866-CD2F-4ED5-9F89-A3736FBA1FB9");
            // Attrib for BlockType: Item Tag Select:Next Page
            RockMigrationHelper.UpdateBlockTypeAttribute("09113783-54B7-4C9F-B626-218B0BA75646","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Next Page","NextPage","","",4,@"","506E9720-83A5-48D0-AAFA-F8259E4C05EE");
            // Attrib for BlockType: Item Tag Select:Title
            RockMigrationHelper.UpdateBlockTypeAttribute("09113783-54B7-4C9F-B626-218B0BA75646","9C204CD0-1233-41C5-818A-C5DA439445AA","Title","Title","","Title to display. Use {0} for person/schedule.",8,@"{0}","91665645-2FC8-4A27-9CAF-D509092B0DDA");
            // Attrib for BlockType: Item Tag Select:Sub Title
            RockMigrationHelper.UpdateBlockTypeAttribute("09113783-54B7-4C9F-B626-218B0BA75646","9C204CD0-1233-41C5-818A-C5DA439445AA","Sub Title","SubTitle","","Sub-Title to display. Use {0} for selected group name.",9,@"{0}","5FD74F6F-FF46-4481-9A5A-CD3FA55E332C");
            // Attrib for BlockType: Item Tag Select:Caption
            RockMigrationHelper.UpdateBlockTypeAttribute("09113783-54B7-4C9F-B626-218B0BA75646","9C204CD0-1233-41C5-818A-C5DA439445AA","Caption","Caption","","",10,@"How many item tags would you like?","021B9635-938D-461D-9CE8-14B0ADDB65B0");
            // Attrib for BlockType: Item Tag Select:No Option Message
            RockMigrationHelper.UpdateBlockTypeAttribute("09113783-54B7-4C9F-B626-218B0BA75646","9C204CD0-1233-41C5-818A-C5DA439445AA","No Option Message","NoOptionMessage","","Message to display when there are not any options available. Use {0} for person's name, and {1} for schedule name.",11,@"Sorry, there are currently not any available locations that {0} can check into at {1}.","44D298CB-392D-4A53-86CA-1D9313632429");
            // Attrib for BlockType: Item Tag Select:No Option After Select Message
            RockMigrationHelper.UpdateBlockTypeAttribute("09113783-54B7-4C9F-B626-218B0BA75646","9C204CD0-1233-41C5-818A-C5DA439445AA","No Option After Select Message","NoOptionAfterSelectMessage","","Message to display when there are not any options available after location is selected. Use {0} for person's name",12,@"Sorry, based on your selection, there are currently not any available times that {0} can check into.","5D15A404-DC64-4A61-949E-4434F48B5CCD");
            // Attrib for BlockType: Item Tag Select:Auto Select Previous Page
            RockMigrationHelper.UpdateBlockTypeAttribute("09113783-54B7-4C9F-B626-218B0BA75646","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Auto Select Previous Page","FamilyAutoSelectPreviousPage","","The page to navigate back to if none of the people and schedules have been processed.",13,@"","377B732F-F1A8-498E-88A4-E2AC44595E31");
            // Attrib for BlockType: Item Tag Select:Auto Select Last Page
            RockMigrationHelper.UpdateBlockTypeAttribute("09113783-54B7-4C9F-B626-218B0BA75646","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Auto Select Last Page","FamilyAutoSelectLastPage","","The last page for each person during family check-in.",14,@"","A17972F1-F8DB-444F-9281-9F1AAF1ADBBA");
            // Attrib Value for Block:Item Tag Select, Attribute:Workflow Type Page: Item Tags, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("3589416D-4770-4C49-B3C8-718BDA7AC942","CAE5DBEF-AE31-42AF-BD8B-E6484F893A13",@"a0bbc045-00e5-4485-88cb-69a73ac7c78d");
            // Attrib Value for Block:Item Tag Select, Attribute:Home Page Page: Item Tags, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("3589416D-4770-4C49-B3C8-718BDA7AC942","C01FAE24-5AA1-492C-9CC7-B8FC11E129DC",@"1610b488-1ad8-44f5-aabd-96c259c02b09");
            // Attrib Value for Block:Item Tag Select, Attribute:Previous Page Page: Item Tags, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("3589416D-4770-4C49-B3C8-718BDA7AC942","AC712866-CD2F-4ED5-9F89-A3736FBA1FB9",@"ac01b3bd-58e9-42ca-aa10-1a19abd78b3c");
            // Attrib Value for Block:Item Tag Select, Attribute:Next Page Page: Item Tags, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("3589416D-4770-4C49-B3C8-718BDA7AC942","506E9720-83A5-48D0-AAFA-F8259E4C05EE",@"d07aacc7-f2b3-4bf2-96ef-91bb9394d47c");
            // Attrib Value for Block:Item Tag Select, Attribute:Title Page: Item Tags, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("3589416D-4770-4C49-B3C8-718BDA7AC942","91665645-2FC8-4A27-9CAF-D509092B0DDA",@"{0}");
            // Attrib Value for Block:Item Tag Select, Attribute:Sub Title Page: Item Tags, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("3589416D-4770-4C49-B3C8-718BDA7AC942","5FD74F6F-FF46-4481-9A5A-CD3FA55E332C",@"{0}");
            // Attrib Value for Block:Item Tag Select, Attribute:Caption Page: Item Tags, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("3589416D-4770-4C49-B3C8-718BDA7AC942","021B9635-938D-461D-9CE8-14B0ADDB65B0",@"How many item tags would you like?");
            // Attrib Value for Block:Item Tag Select, Attribute:No Option Message Page: Item Tags, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("3589416D-4770-4C49-B3C8-718BDA7AC942","44D298CB-392D-4A53-86CA-1D9313632429",@"Sorry, there are currently not any available locations that {0} can check into at {1}.");
            // Attrib Value for Block:Item Tag Select, Attribute:No Option After Select Message Page: Item Tags, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("3589416D-4770-4C49-B3C8-718BDA7AC942","5D15A404-DC64-4A61-949E-4434F48B5CCD",@"Sorry, based on your selection, there are currently not any available times that {0} can check into.");
            // Attrib Value for Block:Item Tag Select, Attribute:Auto Select Previous Page Page: Item Tags, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("3589416D-4770-4C49-B3C8-718BDA7AC942","377B732F-F1A8-498E-88A4-E2AC44595E31",@"baed4080-0afc-4d2d-8748-659aa9a28d24");
            // Attrib Value for Block:Item Tag Select, Attribute:Auto Select Last Page Page: Item Tags, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("3589416D-4770-4C49-B3C8-718BDA7AC942","A17972F1-F8DB-444F-9281-9F1AAF1ADBBA",@"d07aacc7-f2b3-4bf2-96ef-91bb9394d47c");
            // Attrib Value for Block:Idle Redirect, Attribute:Idle Seconds Page: Item Tags, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("1391B29F-2FC7-49DC-8A2C-2C9E4A73D274","1CAC7B16-041A-4F40-8AEE-A39DFA076C14",@"30");
            // Attrib Value for Block:Idle Redirect, Attribute:New Location Page: Item Tags, Site: LCBC Check-in
            RockMigrationHelper.AddBlockAttributeValue("1391B29F-2FC7-49DC-8A2C-2C9E4A73D274","2254B67B-9CB1-47DE-A63D-D0B56051ECD4",@"/lcbc-checkin/welcome");



            // Page: Launch
            RockMigrationHelper.AddPage("132DE89E-2556-45DE-A8CE-A77F4BC2EC79","8305704F-928D-4379-967A-253E576E0923","Launch","Choose the appropriate check-in configuration for your ministry below.","639A5FB7-A9BE-4771-AF03-F696FB896917",""); // Site:Rock Check-in Manager
            RockMigrationHelper.AddPageRoute("639A5FB7-A9BE-4771-AF03-F696FB896917","lcbc-checkin/launch");
            RockMigrationHelper.UpdateBlockType("HTML Content","Adds an editable HTML fragment to the page.","~/Blocks/Cms/HtmlContentDetail.ascx","CMS","19B61D65-37E3-459F-A44F-DEF0089118A3");
            // Add Block to Page: Launch, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlock( true, "639A5FB7-A9BE-4771-AF03-F696FB896917","","19B61D65-37E3-459F-A44F-DEF0089118A3","Launch Buttons","Main","","",0,"698FFAF5-1E3B-41AB-8E5C-7935D3638314"); 
            // Attrib for BlockType: HTML Content:Enabled Lava Commands
            RockMigrationHelper.UpdateBlockTypeAttribute("19B61D65-37E3-459F-A44F-DEF0089118A3","4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D","Enabled Lava Commands","EnabledLavaCommands","","The Lava commands that should be enabled for this HTML block.",0,@"","7146AC24-9250-4FC4-9DF2-9803B9A84299");
            // Attrib for BlockType: HTML Content:Entity Type
            RockMigrationHelper.UpdateBlockTypeAttribute("19B61D65-37E3-459F-A44F-DEF0089118A3","3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB","Entity Type","ContextEntityType","","The type of entity that will provide context for this block",0,@"","6783D47D-92F9-4F48-93C0-16111D675A0F");
            // Attrib for BlockType: HTML Content:Start in Code Editor mode
            RockMigrationHelper.UpdateBlockTypeAttribute("19B61D65-37E3-459F-A44F-DEF0089118A3","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Start in Code Editor mode","UseCodeEditor","","Start the editor in code editor mode instead of WYSIWYG editor mode.",1,@"True","0673E015-F8DD-4A52-B380-C758011331B2");
            // Attrib for BlockType: HTML Content:Document Root Folder
            RockMigrationHelper.UpdateBlockTypeAttribute("19B61D65-37E3-459F-A44F-DEF0089118A3","9C204CD0-1233-41C5-818A-C5DA439445AA","Document Root Folder","DocumentRootFolder","","The folder to use as the root when browsing or uploading documents.",2,@"~/Content","3BDB8AED-32C5-4879-B1CB-8FC7C8336534");
            // Attrib for BlockType: HTML Content:Image Root Folder
            RockMigrationHelper.UpdateBlockTypeAttribute("19B61D65-37E3-459F-A44F-DEF0089118A3","9C204CD0-1233-41C5-818A-C5DA439445AA","Image Root Folder","ImageRootFolder","","The folder to use as the root when browsing or uploading images.",3,@"~/Content","26F3AFC6-C05B-44A4-8593-AFE1D9969B0E");
            // Attrib for BlockType: HTML Content:User Specific Folders
            RockMigrationHelper.UpdateBlockTypeAttribute("19B61D65-37E3-459F-A44F-DEF0089118A3","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","User Specific Folders","UserSpecificFolders","","Should the root folders be specific to current user?",4,@"False","9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE");
            // Attrib for BlockType: HTML Content:Cache Duration
            RockMigrationHelper.UpdateBlockTypeAttribute("19B61D65-37E3-459F-A44F-DEF0089118A3","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Cache Duration","CacheDuration","","Number of seconds to cache the content.",5,@"0","4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4");
            // Attrib for BlockType: HTML Content:Context Parameter
            RockMigrationHelper.UpdateBlockTypeAttribute("19B61D65-37E3-459F-A44F-DEF0089118A3","9C204CD0-1233-41C5-818A-C5DA439445AA","Context Parameter","ContextParameter","","Query string parameter to use for 'personalizing' content based on unique values.",6,@"","3FFC512D-A576-4289-B648-905FD7A64ABB");
            // Attrib for BlockType: HTML Content:Context Name
            RockMigrationHelper.UpdateBlockTypeAttribute("19B61D65-37E3-459F-A44F-DEF0089118A3","9C204CD0-1233-41C5-818A-C5DA439445AA","Context Name","ContextName","","Name to use to further 'personalize' content.  Blocks with the same name, and referenced with the same context parameter will share html values.",7,@"","466993F7-D838-447A-97E7-8BBDA6A57289");
            // Attrib for BlockType: HTML Content:Enable Versioning
            RockMigrationHelper.UpdateBlockTypeAttribute("19B61D65-37E3-459F-A44F-DEF0089118A3","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Enable Versioning","SupportVersions","","If checked, previous versions of the content will be preserved. Versioning is required if you want to require approval.",8,@"False","7C1CE199-86CF-4EAE-8AB3-848416A72C58");
            // Attrib for BlockType: HTML Content:Require Approval
            RockMigrationHelper.UpdateBlockTypeAttribute("19B61D65-37E3-459F-A44F-DEF0089118A3","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Require Approval","RequireApproval","","Require that content be approved?",9,@"False","EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A");
            // Attrib for BlockType: HTML Content:Cache Tags
            RockMigrationHelper.UpdateBlockTypeAttribute("19B61D65-37E3-459F-A44F-DEF0089118A3","BD0D9B57-2A41-4490-89FF-F01DAB7D4904","Cache Tags","CacheTags","","Cached tags are used to link cached content so that it can be expired as a group",10,@"","522C18A9-C727-42A5-A0BA-13C673E8C4B6");
            // Attrib for BlockType: HTML Content:Is Secondary Block
            RockMigrationHelper.UpdateBlockTypeAttribute("19B61D65-37E3-459F-A44F-DEF0089118A3","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Is Secondary Block","IsSecondaryBlock","","Flag indicating whether this block is considered secondary and should be hidden when other secondary blocks are hidden.",11,@"False","04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4");
            // Attrib Value for Block:Launch Buttons, Attribute:Cache Duration Page: Launch, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlockAttributeValue("698FFAF5-1E3B-41AB-8E5C-7935D3638314","4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4",@"0");
            // Attrib Value for Block:Launch Buttons, Attribute:Require Approval Page: Launch, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlockAttributeValue("698FFAF5-1E3B-41AB-8E5C-7935D3638314","EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A",@"False");
            // Attrib Value for Block:Launch Buttons, Attribute:Enable Versioning Page: Launch, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlockAttributeValue("698FFAF5-1E3B-41AB-8E5C-7935D3638314","7C1CE199-86CF-4EAE-8AB3-848416A72C58",@"False");
            // Attrib Value for Block:Launch Buttons, Attribute:Start in Code Editor mode Page: Launch, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlockAttributeValue("698FFAF5-1E3B-41AB-8E5C-7935D3638314","0673E015-F8DD-4A52-B380-C758011331B2",@"True");
            // Attrib Value for Block:Launch Buttons, Attribute:Image Root Folder Page: Launch, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlockAttributeValue("698FFAF5-1E3B-41AB-8E5C-7935D3638314","26F3AFC6-C05B-44A4-8593-AFE1D9969B0E",@"~/Content");
            // Attrib Value for Block:Launch Buttons, Attribute:User Specific Folders Page: Launch, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlockAttributeValue("698FFAF5-1E3B-41AB-8E5C-7935D3638314","9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE",@"False");
            // Attrib Value for Block:Launch Buttons, Attribute:Document Root Folder Page: Launch, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlockAttributeValue("698FFAF5-1E3B-41AB-8E5C-7935D3638314","3BDB8AED-32C5-4879-B1CB-8FC7C8336534",@"~/Content");
            // Attrib Value for Block:Launch Buttons, Attribute:Enabled Lava Commands Page: Launch, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlockAttributeValue("698FFAF5-1E3B-41AB-8E5C-7935D3638314","7146AC24-9250-4FC4-9DF2-9803B9A84299",@"Sql");
            // Attrib Value for Block:Launch Buttons, Attribute:Is Secondary Block Page: Launch, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlockAttributeValue("698FFAF5-1E3B-41AB-8E5C-7935D3638314","04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4",@"False");
        }
        public override void Down()
        {
           
        }
    }
}
