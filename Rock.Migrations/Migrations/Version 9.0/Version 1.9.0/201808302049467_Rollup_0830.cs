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
    public partial class Rollup_0830 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateHomePage();

            RockMigrationHelper.UpdateBlockType( "File Manager", "Block that can be used to browse and manage files on the web server", "~/Blocks/Cms/FileEditor.ascx", "CMS", "0F1DADBC-6B12-4BAA-A828-FD1AA86AA387" );
            // Attrib for BlockType: File Manager:Relative File Path
            RockMigrationHelper.UpdateBlockTypeAttribute( "0F1DADBC-6B12-4BAA-A828-FD1AA86AA387", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Relative File Path", "RelativeFilePath", "", @"The relative path to file", 0, @"", "145CF0AB-1FF7-46ED-8DCE-DD425CBF3F15" );
            // Attrib for BlockType: File Manager:File Editor Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "BA327D25-BD8A-4B67-B04C-17B499DDA4B6", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "File Editor Page", "FileEditorPage", "", @"Page used to edit  the contents of a file.", 2, @"", "15F4E62D-FC89-4C7C-8E73-6A3D75A4FB19" );
            // Attrib for BlockType: Person Bio:Communication Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Communication Page", "CommunicationPage", "", @"The communication page to use for when the person's email address is clicked. Leave this blank to use the default.", 15, @"", "747AC5B1-A535-4414-B97E-523AA4DCE0AB" );
            // Attrib for BlockType: Transaction Detail:Transaction Source Required
            RockMigrationHelper.UpdateBlockTypeAttribute( "1DE16F87-4A49-4A3C-A03E-B8488ECBEEBE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Transaction Source Required", "TransactionSourceRequired", "", @"Determine if Transaction Source should be required.", 6, @"False", "914A2F41-385F-43EB-A3A4-082F84152519" );
            // Attrib for BlockType: Prayer Request Entry:Enable Person Matching
            RockMigrationHelper.UpdateBlockTypeAttribute( "4C32F2CD-5A88-4C3A-ADEA-CF94E85D20A6", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Person Matching", "EnablePersonMatching", "", @"If enabled, requester detail will be matched with all existing person to see if it's already exists.", 14, @"False", "59E648F8-1C25-47B2-8D07-517BA001093A" );
            // Attrib for BlockType: Dynamic Data:Enabled Lava Commands
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "", @"The Lava commands that should be enabled for this dynamic data block.", 1, @"", "824634D6-7F75-465B-A2D2-BA3CE1662CAC" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Dynamic Data:Enabled Lava Commands
            RockMigrationHelper.DeleteAttribute( "824634D6-7F75-465B-A2D2-BA3CE1662CAC" );
            // Attrib for BlockType: Prayer Request Entry:Enable Person Matching
            RockMigrationHelper.DeleteAttribute( "59E648F8-1C25-47B2-8D07-517BA001093A" );
            // Attrib for BlockType: Transaction Detail:Transaction Source Required
            RockMigrationHelper.DeleteAttribute( "914A2F41-385F-43EB-A3A4-082F84152519" );
            // Attrib for BlockType: Person Bio:Communication Page
            RockMigrationHelper.DeleteAttribute( "747AC5B1-A535-4414-B97E-523AA4DCE0AB" );
            // Attrib for BlockType: File Manager:File Editor Page
            RockMigrationHelper.DeleteAttribute( "15F4E62D-FC89-4C7C-8E73-6A3D75A4FB19" );
            // Attrib for BlockType: File Manager:Relative File Path
            RockMigrationHelper.DeleteAttribute( "145CF0AB-1FF7-46ED-8DCE-DD425CBF3F15" );
            RockMigrationHelper.DeleteBlockType( "0F1DADBC-6B12-4BAA-A828-FD1AA86AA387" ); // File Manager
        }

        /// <summary>
        /// GP: Update Homepage Migration
        /// </summary>
        private void UpdateHomePage()
        {
            Sql( @"UPDATE [dbo].[Metric]
SET [SourceSql] = 'SELECT COUNT(*) 
FROM [Person]
WHERE ([RecordTypeValueId] = 1) AND ([RecordStatusValueId] = 3)'
WHERE GUID = 'ecb1b552-9a3d-46fc-952b-d57dbc4a329d'

UPDATE [dbo].[Metric]
SET [SourceSql] = 'DECLARE @FamilyGroupTypeId int = (SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = ''790e3215-3b10-442b-af69-616c0dcb998e'')
SELECT COUNT( DISTINCT(g.[Id])) 
FROM [Person] p
    INNER JOIN [GroupMember] gm ON gm.[PersonId] = p.[Id]
    INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
WHERE ([RecordTypeValueId] = 1) AND ([RecordStatusValueId] = 3)'
WHERE GUID = '491061b7-1834-44da-8ea1-bb73b2d52ad3'" );

            Sql( @"UPDATE [dbo].[MetricValue]
SET [YValue] = (SELECT COUNT(*) 
FROM [Person]
WHERE ([RecordTypeValueId] = 1) AND ([RecordStatusValueId] = 3)),
[MetricValueDateTime] = '" + RockDateTime.Now.ToString( "s" ) + @"'
WHERE [Guid] = '34325795-9016-47e9-a9d9-6283d1a84275'" ); // Active Records

            Sql( @"UPDATE [dbo].[MetricValue]
SET [YValue] = (SELECT COUNT( DISTINCT(g.[Id])) 
FROM [Person] p
    INNER JOIN [GroupMember] gm ON gm.[PersonId] = p.[Id]
    INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = (SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = '790e3215-3b10-442b-af69-616c0dcb998e')
WHERE ([RecordTypeValueId] = 1) AND ([RecordStatusValueId] = 3)),
[MetricValueDateTime] = '" + RockDateTime.Now.ToString( "s" ) + @"'
WHERE [Guid] = '932479dd-9612-4d07-b9cd-9227976cf5dd'" ); //Active Families

            Sql( @"UPDATE [dbo].[MetricValue]
SET [YValue] = (SELECT COUNT(*) 
FROM [ConnectionRequest]
WHERE [ConnectionState] = 0),
[MetricValueDateTime] = '" + RockDateTime.Now.ToString( "s" ) + @"'
WHERE [Guid] = '90cd5a83-3079-4656-b7ce-bfa21055c980'" ); // Active Connection Requests
        }

    }
}
