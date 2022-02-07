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
    public partial class ElevatedPrivilege : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.Person", "AccountProtectionProfile", c => c.Int( nullable: false ) );
            AddColumn( "dbo.Group", "ElevatedSecurityLevel", c => c.Int( nullable: false ) );

            Sql( $@"
-- Update all existing IsSystem Security Role groups to ElevatedSecurityLevel - High (but then change RSR Prayer Access ElevatedSecurityLevel to low)
UPDATE [Group]
SET ElevatedSecurityLevel = 2
WHERE ([GroupTypeId] IN (
        SELECT Id
        FROM [GroupType]
        WHERE [Guid] = '{Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE}'
        )
    OR IsSecurityRole = 1) AND [IsSystem] = 1

-- SET RSR Prayer Access ElevatedSecurityLevel to low
UPDATE [Group]
SET ElevatedSecurityLevel = 1
WHERE [Guid] = '{Rock.SystemGuid.Group.GROUP_RSR_PRAYER_ACCESS}'
" );

            PagesBlocks_Up();
        }

        private void PagesBlocks_Up()
        {
            // Add Page 
            //  Internal Name: Security Settings
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "91CCB1C9-5F9F-44F5-8BE2-9EC3A3CFD46F", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Security Settings", "", "0EF3DE1C-CB97-431E-A066-DDF8BD2D8283", "fa fa-shield-alt" );

            // Add/Update BlockType 
            //   Name: Rock Security Settings
            //   Category: Security
            //   Path: ~/Blocks/Security/RockSecuritySettings.ascx
            //   EntityType: -
            RockMigrationHelper.UpdateBlockType( "Rock Security Settings", "Block for displaying and editing Rock's security settings.", "~/Blocks/Security/RockSecuritySettings.ascx", "Security", "186490CD-4132-43BD-9BDF-DD04C6CD2432" );

            // Add Block 
            //  Block Name: Rock Security Settings
            //  Page Name: Security Settings
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "0EF3DE1C-CB97-431E-A066-DDF8BD2D8283".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "186490CD-4132-43BD-9BDF-DD04C6CD2432".AsGuid(), "Rock Security Settings", "Main", @"", @"", 0, "CC957809-5DA9-47A2-A55D-63F612478722" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn( "dbo.Group", "ElevatedSecurityLevel" );
            DropColumn( "dbo.Person", "AccountProtectionProfile" );

            PagesBlocks_Down();
        }

        private void PagesBlocks_Down()
        {
            // Remove Block
            //  Name: Rock Security Settings, from Page: Security Settings, Site: Rock RMS
            //  from Page: Security Settings, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "CC957809-5DA9-47A2-A55D-63F612478722" );

            // Delete BlockType 
            //   Name: Rock Security Settings
            //   Category: Security
            //   Path: ~/Blocks/Security/RockSecuritySettings.ascx
            //   EntityType: -
            RockMigrationHelper.DeleteBlockType( "186490CD-4132-43BD-9BDF-DD04C6CD2432" );

            // Delete Page 
            //  Internal Name: Security Settings
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "0EF3DE1C-CB97-431E-A066-DDF8BD2D8283" );
        }
    }
}
