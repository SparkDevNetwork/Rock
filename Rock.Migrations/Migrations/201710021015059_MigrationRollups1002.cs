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
    public partial class MigrationRollups1002 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // MP: Set New Communication Templates Active
            Sql( @"
    UPDATE CommunicationTemplate SET IsActive = 1
    WHERE [Guid] = '88B7DF18-9C30-4BAC-8CA2-5AD253D57E4D';

    UPDATE CommunicationTemplate SET IsActive = 1
    WHERE [Guid] = 'A3C7F623-7F6F-4C48-B66F-CBEE2DF30B6A';
" );

            // MP: Set GroupListPage for CommunicationList Detail
            RockMigrationHelper.AddBlockAttributeValue( "3FF79A87-ABC1-4DE3-B25A-8111E5D05607", "4F55D7DD-27A4-4ABC-89AD-1160F5697FD2", @"002c9991-523a-478c-b19b-e9df2b977481" ); // Group List Page

            // DT: Set Transaction List Show Summary setting
            RockMigrationHelper.UpdateBlockTypeAttribute( "E04320BC-67C3-452D-9EF6-D74D8C177154", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Account Summary", "ShowAccountSummary", "", "Should the account summary be displayed at the bottom of the list?", 8, @"False", "4C92974B-FB99-4E89-B215-A457646D77E1" );
            RockMigrationHelper.AddBlockAttributeValue( "B447AB11-3A19-4527-921A-2266A6B4E181", "4C92974B-FB99-4E89-B215-A457646D77E1", "True" );
            RockMigrationHelper.AddBlockAttributeValue( "1133795B-3325-4D81-B603-F442F0AC892B", "4C92974B-FB99-4E89-B215-A457646D77E1", "True" );

            // DT: Domain level auth cookie
            RockMigrationHelper.AddDefinedType( "Global", "Domain Login Sharing", @"
By default, Rock does not share saved login information across domains. For example if a user logs in from <i>http://<strong>www</strong>.rocksolidchurch.com</i>, they would also have to login at <i>http://<strong>admin</strong>.rocksolidchurch.com</i>. You can override this behavior so that all hosts of common domain share their login status. So in the case above, if <i>rocksolidchurchdemo.com</i> was entered below, logging into the <strong>www</strong> site would also auto log you into the <strong>admin</strong> site.
", "6CE00E1B-FE09-45FE-BD9D-56C57A11BE1A", "Enter the high-level domain names that should share the authentication cookie between subdomains." );

            // DT: Background Check SSN Attribute
            Sql( MigrationSQL._201710021015059_BackgroundCheckSSN );

            // MP: Update 'Blank' communication template
            Sql( MigrationSQL._201710021015059_BlankCommTemplate );

            // MP: Update Communications to use Email Wizard by default
            // -----------------
            // Add a new 'Simple Communication' page under the 'New Communication' page to put the orig Communication Entry and Communication Detail blocks
            RockMigrationHelper.AddPage( "2A22D08D-73A8-4AAF-AC7E-220E8B2E7857", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Simple Communication", "", "7E8408B2-354C-4A5A-8707-36754AE80B9A", "" ); // Site:Rock RMS
            // move the original 'Communication Entry' and 'Communication Detail' blocks to the new 'Simple Communication' page
            Sql( @"
DECLARE 
-- 'New Communication' Page
@newCommunicationPageId int = (select top 1 Id from [Page] where [Guid] = '2A22D08D-73A8-4AAF-AC7E-220E8B2E7857'),
-- 'Simple Communication' Page
@simpleCommunicationPageId int = (select top 1 Id from [Page] where [Guid] = '7E8408B2-354C-4A5A-8707-36754AE80B9A'),
-- 'Communication Entry' block on 'New Communication Page'
@communicationEntryBlockId int = (select top 1 Id from [Block] where [Guid] = 'BD9B2F32-AB18-4761-80C9-FDA4DBEEA9EC'),
-- 'Communication Detail' block on 'New Communication Page'
@communicationDetailBlockId int = (select top 1 Id from [Block] where [Guid] = 'A02F7695-4C6E-44E9-84CB-42E6F51F285F')
UPDATE [Block] SET [PageId] = @simpleCommunicationPageId where [Id] = @communicationEntryBlockId;
UPDATE [Block] SET [PageId] = @simpleCommunicationPageId where [Id] = @communicationDetailBlockId;
" );

            RockMigrationHelper.AddPage( "7F79E512-B9DB-4780-9887-AD6D63A39050", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Email Analytics", "", "DF014200-72A3-48A0-A953-E594E5410E36", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPageRoute( "DF014200-72A3-48A0-A953-E594E5410E36", "EmailAnalytics", "DB116A39-BCE2-40D1-9604-FE955472DD35" );
            RockMigrationHelper.UpdateBlockType( "Communication Entry Wizard", "Used for creating and sending a new communications such as email, SMS, etc. to recipients.", "~/Blocks/Communication/CommunicationEntryWizard.ascx", "Communication", "F7D464E2-5F7C-47BA-84DB-7CC7B0B623C0" );
            RockMigrationHelper.UpdateBlockType( "Email Analytics", "Shows a graph of email statistics optionally limited to a specific communication or communication list.", "~/Blocks/Communication/EmailAnalytics.ascx", "Communication", "7B506760-93FA-4FBF-9FB5-0D9C3E36DCCD" );

            // Add Block to Page: New Communication, Site: Rock RMS
            RockMigrationHelper.AddBlock( "2A22D08D-73A8-4AAF-AC7E-220E8B2E7857", "", "F7D464E2-5F7C-47BA-84DB-7CC7B0B623C0", "New Communication", "Main", @"", @"", 0, "82D5B1A0-1C17-464E-9EC5-414549FB44C7" );
            // Add Block to Page: New Communication, Site: Rock RMS
            RockMigrationHelper.AddBlock( "2A22D08D-73A8-4AAF-AC7E-220E8B2E7857", "", "CEDC742C-0AB3-487D-ABC2-77A0A443AEBF", "Communication Detail", "Main", @"", @"", 1, "25D890B9-9609-4B63-AD25-4AE427205563" );

            // Add Block to Page: Email Analytics, Site: Rock RMS
            RockMigrationHelper.AddBlock( "DF014200-72A3-48A0-A953-E594E5410E36", "", "7B506760-93FA-4FBF-9FB5-0D9C3E36DCCD", "Email Analytics", "Main", @"", @"", 0, "DC951B4F-0F07-47C3-A279-D1AFA1C50549" );
            // update block order for pages with new blocks if the page,zone has multiple blocks
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '82D5B1A0-1C17-464E-9EC5-414549FB44C7'" );  // Page: New Communication,  Zone: Main,  Block: New Communication
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '25D890B9-9609-4B63-AD25-4AE427205563'" );  // Page: New Communication,  Zone: Main,  Block: Communication Detail

            // Attrib for BlockType: Communication Entry:Enabled Lava Commands
            RockMigrationHelper.UpdateBlockTypeAttribute( "D9834641-7F39-4CFA-8CB2-E64068127565", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "", "The Lava commands that should be enabled for this HTML block.", 0, @"", "924A35EE-9AFC-4BB7-8C51-2F0A7F8BBB07" );
            // Attrib for BlockType: Communication Entry:Allow CC/Bcc
            RockMigrationHelper.UpdateBlockTypeAttribute( "D9834641-7F39-4CFA-8CB2-E64068127565", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow CC/Bcc", "AllowCcBcc", "", "Allow CC and Bcc addresses to be entered for email communications?", 7, @"False", "B9CEE691-4E73-4EB4-BE15-A4FAC75C3FF6" );
            // Attrib for BlockType: Communication Entry:User Specific Folders
            RockMigrationHelper.UpdateBlockTypeAttribute( "D9834641-7F39-4CFA-8CB2-E64068127565", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "User Specific Folders", "UserSpecificFolders", "", "Should the root folders be specific to current user?", 2, @"False", "65E8E538-EA4C-45AF-96DD-4CBA8AC15D01" );
            // Attrib for BlockType: Communication Entry:Document Root Folder
            RockMigrationHelper.UpdateBlockTypeAttribute( "D9834641-7F39-4CFA-8CB2-E64068127565", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Document Root Folder", "DocumentRootFolder", "", "The folder to use as the root when browsing or uploading documents.", 0, @"~/Content", "AE8A1A13-20F3-4B1A-BD2A-AB97C94E0493" );
            // Attrib for BlockType: Communication Entry:Image Root Folder
            RockMigrationHelper.UpdateBlockTypeAttribute( "D9834641-7F39-4CFA-8CB2-E64068127565", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Image Root Folder", "ImageRootFolder", "", "The folder to use as the root when browsing or uploading images.", 1, @"~/Content", "9F19C549-6A04-4E89-98DF-AECBAEC02B82" );
            // Attrib for BlockType: Communication Entry Wizard:Communication Types
            RockMigrationHelper.UpdateBlockTypeAttribute( "F7D464E2-5F7C-47BA-84DB-7CC7B0B623C0", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Communication Types", "CommunicationTypes", "", "The communication types that should be available to user to send (If none are selected, all will be available).", 4, @"", "E8837FA3-D2C5-48B8-93CC-18236549212A" );
            // Attrib for BlockType: Communication Entry Wizard:Maximum Recipients
            RockMigrationHelper.UpdateBlockTypeAttribute( "F7D464E2-5F7C-47BA-84DB-7CC7B0B623C0", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Maximum Recipients", "MaximumRecipients", "", "The maximum number of recipients allowed before communication will need to be approved.", 5, @"300", "C9468757-5DDB-448A-BDB3-DE1AFCB4CFB5" );
            // Attrib for BlockType: Communication Entry Wizard:Send When Approved
            RockMigrationHelper.UpdateBlockTypeAttribute( "F7D464E2-5F7C-47BA-84DB-7CC7B0B623C0", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Send When Approved", "SendWhenApproved", "", "Should communication be sent once it's approved (vs. just being queued for scheduled job to send)?", 6, @"True", "A2D43F93-3200-41AC-A5DB-DAD7EB147873" );
            // Attrib for BlockType: Communication Entry Wizard:Max SMS Image Width
            RockMigrationHelper.UpdateBlockTypeAttribute( "F7D464E2-5F7C-47BA-84DB-7CC7B0B623C0", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max SMS Image Width", "MaxSMSImageWidth", "", "The maximum width (in pixels) of an image attached to a mobile communication. If its width is over the max, Rock will automatically resize image to the max width.", 7, @"600", "63A17952-6CEA-4ECC-A355-D430D176E816" );
            // Attrib for BlockType: Communication Entry Wizard:Simple Communication Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "F7D464E2-5F7C-47BA-84DB-7CC7B0B623C0", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Simple Communication Page", "SimpleCommunicationPage", "", "The page to use if the 'Use Simple Editor' panel heading icon is clicked. Leave this blank to not show the 'Use Simple Editor' heading icon", 8, @"", "E7D98A97-0287-4FFF-BE86-9FC52C874C29" );
            // Attrib for BlockType: Communication Entry Wizard:Binary File Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "F7D464E2-5F7C-47BA-84DB-7CC7B0B623C0", "09EC7F0D-3505-4090-B010-ABA68CB9B904", "Binary File Type", "BinaryFileType", "", "The FileType to use for images that are added to the email using the image component", 1, @"60B896C3-F00C-411C-A31C-2D5D4CCBB65F", "23AAAE89-DB39-47C2-80A0-800EE28866DA" );
            // Attrib for BlockType: Communication Entry Wizard:Character Limit
            RockMigrationHelper.UpdateBlockTypeAttribute( "F7D464E2-5F7C-47BA-84DB-7CC7B0B623C0", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Character Limit", "CharacterLimit", "", "Set this to show a character limit countdown for SMS communications. Set to 0 to disable", 2, @"160", "E01ABA64-DEAA-4929-80C8-4AF58C73A63B" );
            // Attrib for BlockType: Communication Entry Wizard:Enabled Lava Commands
            RockMigrationHelper.UpdateBlockTypeAttribute( "F7D464E2-5F7C-47BA-84DB-7CC7B0B623C0", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "", "The Lava commands that should be enabled for this HTML block.", 3, @"", "ADEF85D0-F870-4883-9694-396EC5BF8F52" );
            // Attrib Value for Block:New Communication, Attribute:Communication Types Page: New Communication, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "82D5B1A0-1C17-464E-9EC5-414549FB44C7", "E8837FA3-D2C5-48B8-93CC-18236549212A", @"" );
            // Attrib Value for Block:New Communication, Attribute:Maximum Recipients Page: New Communication, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "82D5B1A0-1C17-464E-9EC5-414549FB44C7", "C9468757-5DDB-448A-BDB3-DE1AFCB4CFB5", @"300" );
            // Attrib Value for Block:New Communication, Attribute:Send When Approved Page: New Communication, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "82D5B1A0-1C17-464E-9EC5-414549FB44C7", "A2D43F93-3200-41AC-A5DB-DAD7EB147873", @"True" );
            // Attrib Value for Block:New Communication, Attribute:Max SMS Image Width Page: New Communication, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "82D5B1A0-1C17-464E-9EC5-414549FB44C7", "63A17952-6CEA-4ECC-A355-D430D176E816", @"600" );
            // Attrib Value for Block:New Communication, Attribute:Simple Communication Page Page: New Communication, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "82D5B1A0-1C17-464E-9EC5-414549FB44C7", "E7D98A97-0287-4FFF-BE86-9FC52C874C29", @"7e8408b2-354c-4a5a-8707-36754ae80b9a" );
            // Attrib Value for Block:New Communication, Attribute:Binary File Type Page: New Communication, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "82D5B1A0-1C17-464E-9EC5-414549FB44C7", "23AAAE89-DB39-47C2-80A0-800EE28866DA", @"60b896c3-f00c-411c-a31c-2d5d4ccbb65f" );
            // Attrib Value for Block:New Communication, Attribute:Character Limit Page: New Communication, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "82D5B1A0-1C17-464E-9EC5-414549FB44C7", "E01ABA64-DEAA-4929-80C8-4AF58C73A63B", @"160" );
            // Attrib Value for Block:New Communication, Attribute:Enabled Lava Commands Page: New Communication, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "82D5B1A0-1C17-464E-9EC5-414549FB44C7", "ADEF85D0-F870-4883-9694-396EC5BF8F52", @"" );
            RockMigrationHelper.UpdateFieldType( "Interaction Channel", "", "Rock", "Rock.Field.Types.InteractionChannelFieldType", "5EE5D193-60B6-4808-9BE9-C5FFDDF444E4" );

            // Attrib for BlockType: Group List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "3D7FB6BE-6BBD-49F7-96B4-96310AF3048A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", "", 0, @"", "487A898B-2192-459D-9546-32AE3EE9A9C5" );

            // Attrib Value for Block:Group List, Attribute:core.CustomGridColumnsConfig Page: Communication Lists, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "426EC86B-5784-411D-94ED-DD007E6DF783", "487A898B-2192-459D-9546-32AE3EE9A9C5", @"{
  ""ColumnsConfig"": [
    {
      ""HeaderText"": """",
      ""HeaderClass"": ""grid-columncommand"",
      ""ItemClass"": ""grid-columncommand"",
      ""LavaTemplate"": ""<div class='text-center'>\n    <a href='~/EmailAnalytics?CommunicationListId={{Row.Id}}' class='btn btn-default btn-sm' title='Email Analytics'>\n        <i class='fa fa-line-chart'></i>\n    </a>\n</div>"",
      ""PositionOffsetType"": 1,
      ""PositionOffset"": 1
    }
  ]
}" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove Block: Email Analytics, from Page: Email Analytics, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "DC951B4F-0F07-47C3-A279-D1AFA1C50549" );
            // Remove Block: Communication Detail, from Page: New Communication, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "25D890B9-9609-4B63-AD25-4AE427205563" );

            // Remove Block: New Communication, from Page: New Communication, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "82D5B1A0-1C17-464E-9EC5-414549FB44C7" );

            RockMigrationHelper.DeletePage( "DF014200-72A3-48A0-A953-E594E5410E36" ); // Page: Email Analytics, Layout: Full Width, Site: Rock RMS
                                                                                      
            // move the original 'Communication Entry' and 'Communication Detail' blocks back to the orig 'New Communication' page
            Sql( @"
DECLARE 
-- 'New Communication' Page
@newCommunicationPageId int = (select top 1 Id from [Page] where [Guid] = '2A22D08D-73A8-4AAF-AC7E-220E8B2E7857'),
-- 'Simple Communication' Page
@simpleCommunicationPageId int = (select top 1 Id from [Page] where [Guid] = '7E8408B2-354C-4A5A-8707-36754AE80B9A'),
-- 'Communication Entry' block on 'New Communication Page'
@communicationEntryBlockId int = (select top 1 Id from [Block] where [Guid] = 'BD9B2F32-AB18-4761-80C9-FDA4DBEEA9EC'),
-- 'Communication Detail' block on 'New Communication Page'
@communicationDetailBlockId int = (select top 1 Id from [Block] where [Guid] = 'A02F7695-4C6E-44E9-84CB-42E6F51F285F')
UPDATE [Block] SET [PageId] = @newCommunicationPageId where [Id] = @communicationEntryBlockId;
UPDATE [Block] SET [PageId] = @newCommunicationPageId where [Id] = @communicationDetailBlockId;
" );

            RockMigrationHelper.DeletePage( "7E8408B2-354C-4A5A-8707-36754AE80B9A" ); // Page: Simple Communication, Layout: Full Width, Site: Rock RMS

        }
    }
}
