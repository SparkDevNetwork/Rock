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
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class CommunicationListGroup : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateIndex("dbo.Communication", "ListGroupId");

            // Instead of AddForeignKey, do it manually so it can be a ON DELETE SET NULL
            // AddForeignKey("dbo.Communication", "ListGroupId", "dbo.Group", "Id");
            Sql( @"ALTER TABLE dbo.Communication ADD CONSTRAINT [FK_dbo.Communication_dbo.Group_ListGroupId] 
                  FOREIGN KEY (ListGroupId) REFERENCES dbo.[Group] (Id) ON DELETE SET NULL" );

            // MP: Add optional SecurityColumn to GroupList and enable for Communication List
            // Attrib for BlockType: Group List:Display Security Column
            RockMigrationHelper.UpdateBlockTypeAttribute( "3D7FB6BE-6BBD-49F7-96B4-96310AF3048A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Security Column", "DisplaySecurityColumn", "", "Should the Security column be displayed?", 10, @"False", "2DDD4FD0-5E03-4271-B8EF-728DECA10018" );
            // Attrib Value for Block:Group List, Attribute:core.CustomGridColumnsConfig Page: Communication Lists, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "426EC86B-5784-411D-94ED-DD007E6DF783", "487A898B-2192-459D-9546-32AE3EE9A9C5", @"{
 ""ColumnsConfig"": [
 {
 ""HeaderText"": """",
 ""HeaderClass"": ""grid-columncommand"",
 ""ItemClass"": ""grid-columncommand"",
 ""LavaTemplate"": ""<div class='text-center'>\n <a href='~/EmailAnalytics?CommunicationListId={{Row.Id}}' class='btn btn-default btn-sm' title='Email Analytics'>\n <i class='fa fa-line-chart'></i>\n </a>\n</div>"",
 ""PositionOffsetType"": 1,
 ""PositionOffset"": 2
 }
 ]
}" );
            // Attrib Value for Block:Group List, Attribute:Display Security Column Page: Communication Lists, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "426EC86B-5784-411D-94ED-DD007E6DF783", "2DDD4FD0-5E03-4271-B8EF-728DECA10018", @"True" );


            // MP: Update EmailMedium AttributeValues
            Sql( MigrationSQL._201710061805334_CommunicationListGroup_UpdateEmailMediumAttributeValues );

            // MP: Communication List Public Name
            // 'Public Name' group attribute for Communication List
            RockMigrationHelper.AddGroupTypeGroupAttribute( "D1D95777-FFA3-CBB3-4A6D-658706DAED33", SystemGuid.FieldType.TEXT, "Public Name", "The name of the communication list that is shown publicly.", 0, "", "086104F3-EE6B-4557-BBD1-9533C8023267" );

            // MP: More Catchups
            RockMigrationHelper.UpdateBlockType( "Bulk Import", "Block to import Slingshot files into Rock using BulkImport", "~/Blocks/BulkImport/BulkImportTool.ascx", "Bulk Import", "D9302E4A-C498-4CD7-8D3B-0E9DA9802DD5" );
            // Attrib for BlockType: Login:New Account Text
            RockMigrationHelper.UpdateBlockTypeAttribute( "7B83D513-1178-429E-93FF-E76430E038E4", "9C204CD0-1233-41C5-818A-C5DA439445AA", "New Account Text", "NewAccountButtonText", "", "The text to show on the New Account button.", 7, @"Register", "C76DC2DA-E1CB-4AAB-AD1F-573FFF726805" );
            // Attrib for BlockType: My Workflows Lava:Set Panel Icon
            RockMigrationHelper.UpdateBlockTypeAttribute( "4F217A7F-A34E-489E-AE0E-2B7EDCF69CD1", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Set Panel Icon", "SetPanelIcon", "", "The icon to display in the panel header.", 5, @"", "0BB2EE44-5790-4861-A556-25E280A8858C" );
            // Attrib for BlockType: My Workflows Lava:Set Panel Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "4F217A7F-A34E-489E-AE0E-2B7EDCF69CD1", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Set Panel Title", "SetPanelTitle", "", "The title to display in the panel header. Leave empty to have the block name.", 4, @"", "290E180A-0433-48D1-9BB2-4A9A46E2D60D" );
            // Attrib for BlockType: Group List:Set Panel Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "3D7FB6BE-6BBD-49F7-96B4-96310AF3048A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Set Panel Title", "SetPanelTitle", "", "The title to display in the panel header. Leave empty to have the title be set automatically based on the group type or block name.", 13, @"", "E861BE97-59F3-4A9C-8F9E-8F45798DF26C" );
            // Attrib for BlockType: Group List:Set Panel Icon
            RockMigrationHelper.UpdateBlockTypeAttribute( "3D7FB6BE-6BBD-49F7-96B4-96310AF3048A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Set Panel Icon", "SetPanelIcon", "", "The icon to display in the panel header. Leave empty to have the icon be set automatically based on the group type or default icon.", 14, @"", "A03E361B-3F13-41C4-B92F-59A42C261569" );
            // Attrib for BlockType: Email Analytics:Series Colors
            RockMigrationHelper.UpdateBlockTypeAttribute( "7B506760-93FA-4FBF-9FB5-0D9C3E36DCCD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Series Colors", "SeriesColors", "", "A comma-delimited list of colors that the charts will use.", 0, @"#5DA5DA,#60BD68,#FFBF2F,#F36F13,#C83013,#676766", "0EF39AAC-E6EA-426B-802A-3212CE52F245" );
            // Attrib for BlockType: Public Profile Edit:Request Changes Text
            RockMigrationHelper.UpdateBlockTypeAttribute( "841D1670-8BFD-4913-8409-FB47EB7A2AB9", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Request Changes Text", "RequestChangesText", "", "The text to use for the request changes button (only displayed if there is a 'Workflow Launch Page' configured).", 7, @"Request Additional Changes", "1B56C327-758E-401F-A9C8-5E414DDC6F7D" );
            // Attrib for BlockType: Person Bio:Display Anniversary Date
            RockMigrationHelper.UpdateBlockTypeAttribute( "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Anniversary Date", "DisplayAnniversaryDate", "", "Should the Anniversary Date be displayed?", 12, @"True", "D6B98FBE-A10E-4C29-B033-1BF949391212" );
            // Attrib for BlockType: Person Bio:Display Tags
            RockMigrationHelper.UpdateBlockTypeAttribute( "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Tags", "DisplayTags", "", "Should tags be displayed?", 10, @"True", "9217579B-C0DE-4D2F-BBE3-DE75E2D239E1" );
            // Attrib for BlockType: Person Bio:Display Graduation
            RockMigrationHelper.UpdateBlockTypeAttribute( "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Graduation", "DisplayGraduation", "", "Should the Grade/Graduation be displayed", 11, @"True", "59C6A71C-7064-4D23-A5D5-04FA8F1B3456" );
            // Attrib for BlockType: Notes:Display Note Type Heading
            RockMigrationHelper.UpdateBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Note Type Heading", "DisplayNoteTypeHeading", "", "Should each note's Note Type be displayed as a heading above each note?", 13, @"False", "C5FD0719-1E03-4C17-BE31-E02A3637C39A" );
            // Attrib for BlockType: Group List:Display Security Column
            RockMigrationHelper.UpdateBlockTypeAttribute( "3D7FB6BE-6BBD-49F7-96B4-96310AF3048A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Security Column", "DisplaySecurityColumn", "", "Should the Security column be displayed?", 10, @"False", "2DDD4FD0-5E03-4271-B8EF-728DECA10018" );
            // Attrib for BlockType: Group Detail:Show Location Addresses
            RockMigrationHelper.UpdateBlockTypeAttribute( "582BEEA1-5B27-444D-BC0A-F60CEB053981", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Location Addresses", "ShowLocationAddresses", "", "Determines if the location address should be shown when viewing the group details.", 13, @"True", "8A2D0F83-0856-4963-A600-095391603661" );
            // Attrib for BlockType: Group Detail:Prevent Selecting Inactive Campus
            RockMigrationHelper.UpdateBlockTypeAttribute( "582BEEA1-5B27-444D-BC0A-F60CEB053981", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Prevent Selecting Inactive Campus", "PreventSelectingInactiveCampus", "", "Should inactive campuses be excluded from the campus field when editing a group?.", 14, @"False", "7268FBA7-B88B-4CFD-B34B-4F6ACFE80BC2" );
            // Attrib for BlockType: Edit Person:Hide Anniversary Date
            RockMigrationHelper.UpdateBlockTypeAttribute( "0A15F28C-4828-4B38-AF66-58AC5BDE48E0", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide Anniversary Date", "HideAnniversaryDate", "", "Should the Anniversary Date field be hidden?", 1, @"False", "EA1869F0-1E2A-4DF9-AF2C-32A4733624D5" );
            // Attrib for BlockType: Edit Person:Hide Grade
            RockMigrationHelper.UpdateBlockTypeAttribute( "0A15F28C-4828-4B38-AF66-58AC5BDE48E0", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide Grade", "HideGrade", "", "Should the Grade (and Graduation Year) fields be hidden?", 0, @"False", "2B823000-A6DA-4105-8FAC-ADD09D3A7E3E" );
            // Attrib for BlockType: Content Channel View:Enable Legacy Global Attribute Lava
            RockMigrationHelper.UpdateBlockTypeAttribute( "143A2345-3E26-4ED0-A2FE-42AAF11B4C0F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Legacy Global Attribute Lava", "SupportLegacy", "", "This should only be enabled if your lava is using legacy Global Attributes. Enabling this option, will negatively affect the performance of this block.", 2, @"False", "B1A62B09-92DF-41D1-8698-6B6F7DE1DD36" );
            // Attrib for BlockType: Public Profile Edit:Disable Name Edit
            RockMigrationHelper.UpdateBlockTypeAttribute( "841D1670-8BFD-4913-8409-FB47EB7A2AB9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Disable Name Edit", "DisableNameEdit", "", "Whether the First and Last Names can be edited.", 0, @"False", "5B3ECBBE-293C-4BA6-A5D8-87846D4F641C" );
            // Attrib for BlockType: Public Profile Edit:View Only
            RockMigrationHelper.UpdateBlockTypeAttribute( "841D1670-8BFD-4913-8409-FB47EB7A2AB9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "View Only", "ViewOnly", "", "Should people be prevented from editing thier profile or family records?", 1, @"False", "13E4D341-CEEF-4B7E-BB3F-6FF5B3466817" );
            // Attrib for BlockType: Public Profile Edit:Show Communication Preference
            RockMigrationHelper.UpdateBlockTypeAttribute( "841D1670-8BFD-4913-8409-FB47EB7A2AB9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Communication Preference", "ShowCommunicationPreference", "", "Show the communication preference and allow it to be edited", 5, @"True", "8466A07F-4C0E-4B31-8AE5-F6BE96AB56F4" );
            // Attrib for BlockType: Workflow Entry:Show Summary View
            RockMigrationHelper.UpdateBlockTypeAttribute( "A8BD05C8-6F89-4628-845B-059E686F089A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Summary View", "ShowSummaryView", "", "If workflow has been completed, should the summary view be displayed?", 1, @"False", "1CFB44EE-4DF7-40DD-83DC-B7801909D259" );
            // Attrib for BlockType: Relationships:Max Relationships To Display
            RockMigrationHelper.UpdateBlockTypeAttribute( "77E409D4-11CD-4009-B4CD-4B75DF2CC9FD", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max Relationships To Display", "MaxRelationshipsToDisplay", "", "", 0, @"50", "15982494-746F-48F0-9634-F045583A05FC" );
            // Attrib for BlockType: Content Channel View:Output Cache Duration
            RockMigrationHelper.UpdateBlockTypeAttribute( "143A2345-3E26-4ED0-A2FE-42AAF11B4C0F", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Output Cache Duration", "OutputCacheDuration", "", "Number of seconds to cache the resolved output. Only cache the output if you are not personalizing the output based on current user, current page, or any other merge field value.", 0, @"0", "8B21B333-9875-4231-B83D-4E007409BF30" );
            // Attrib for BlockType: Group List Personalized Lava:Cache Duration
            RockMigrationHelper.UpdateBlockTypeAttribute( "1B172C33-8672-4C98-A995-8E123FF316BD", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Cache Duration", "CacheDuration", "", "Length of time in seconds to cache which groups are descendants of the parent group.", 3, @"3600", "CCA08E73-78F6-46A5-831F-7BEB8C213ACD" );
            // Attrib for BlockType: Communication List:Email Analytics
            RockMigrationHelper.UpdateBlockTypeAttribute( "56ABBD0F-8F62-4094-88B3-161E71F21419", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Email Analytics", "EmailAnalytics", "", "", 2, @"DF014200-72A3-48A0-A953-E594E5410E36", "0B28D804-634A-40CF-AF8B-BD37E1E7A7C6" );
            // Attrib for BlockType: Email Preference Entry:Unsubscribe from List Text
            RockMigrationHelper.UpdateBlockTypeAttribute( "B3C076C7-1325-4453-9549-456C23702069", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Unsubscribe from List Text", "UnsubscribefromListText", "", "Text to display for the 'Unsubscribe me from list' option.", 1, @"Please unsubscribe me from emails regarding '{{ Communication.ListGroup | Attribute:'PublicName' | Default:Communication.ListGroup.Name }}'", "A09E8C9D-F6CD-4BFC-9CF4-2C9B7DBB9953" );
            // Attrib for BlockType: Email Preference Entry:Unsubscribe Success Text
            RockMigrationHelper.UpdateBlockTypeAttribute( "B3C076C7-1325-4453-9549-456C23702069", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Unsubscribe Success Text", "UnsubscribeSuccessText", "", "Text to display after user unsubscribes from a list.", 7, @"<h4>Thank You</h4>We have saved your removed you from the '{{ Communication.ListGroup | Attribute:'PublicName' | Default:Communication.ListGroup.Name }}' list.", "267739E4-A231-4E4C-A58C-9BFDA3C2948C" );
            // Attrib for BlockType: Group List Personalized Lava:Parent Group
            RockMigrationHelper.UpdateBlockTypeAttribute( "1B172C33-8672-4C98-A995-8E123FF316BD", "F4399CEF-827B-48B2-A735-F7806FCFE8E8", "Parent Group", "ParentGroup", "", "If a group is chosen, only the groups under this group will be displayed.", 2, @"", "D5F64AB6-2296-491F-A2F0-1B6660F3E878" );
            // Attrib for BlockType: Login:No Account Text
            RockMigrationHelper.UpdateBlockTypeAttribute( "7B83D513-1178-429E-93FF-E76430E038E4", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "No Account Text", "NoAccountText", "", "The text to show when no account exists. <span class='tip tip-lava'></span>.", 8, @"Sorry, we couldn't find an account matching that username/password. Can we help you <a href='{{HelpPage}}'>recover your account information</a>?", "A33F3913-78E0-463B-9861-64611B4E7B31" );
            // Attrib for BlockType: Login:Invalid PersonToken Text
            RockMigrationHelper.UpdateBlockTypeAttribute( "7B83D513-1178-429E-93FF-E76430E038E4", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Invalid PersonToken Text", "InvalidPersonTokenText", "", "The text to show when a person is logged out due to an invalid persontoken. <span class='tip tip-lava'></span>.", 12, @"<div class='alert alert-warning'>The login token you provided is no longer valid. Please login below.</div>", "734324B5-AC54-408C-B9B7-ABC31745B0DE" );
            // Attrib for BlockType: Group Members:Group Footer Lava
            RockMigrationHelper.UpdateBlockTypeAttribute( "FC137BDA-4F05-4ECE-9899-A249C90D11FC", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Group Footer Lava", "GroupFooterLava", "", "Lava to put at the bottom of the block. Merge fields include Page, CurrentPerson, Group (the family) and GroupMembers.", 5, @"", "7CA36124-D14C-44E8-89EA-DE3845630B97" );
            // Attrib for BlockType: Group Members:Group Header Lava
            RockMigrationHelper.UpdateBlockTypeAttribute( "FC137BDA-4F05-4ECE-9899-A249C90D11FC", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Group Header Lava", "GroupHeaderLava", "", "Lava to put at the top of the block. Merge fields include Page, CurrentPerson, Group (the family) and GroupMembers.", 4, @"", "2B06CE66-DD84-47B9-9205-00A72DB75063" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.Communication", "ListGroupId", "dbo.Group");
            DropIndex("dbo.Communication", new[] { "ListGroupId" });
        }
    }
}
