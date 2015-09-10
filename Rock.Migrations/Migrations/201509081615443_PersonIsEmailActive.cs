// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class PersonIsEmailActive : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( "update [Person] set [IsEmailActive] = 1 where [IsEmailActive] is null" );
            AlterColumn( "dbo.Person", "IsEmailActive", c => c.Boolean( nullable: false ) );

            // DT: Add Data View Page Setting to Report Detail
            // Attrib for BlockType: Report Detail:Data View Page
            RockMigrationHelper.AddBlockTypeAttribute( "E431DBDF-5C65-45DC-ADC5-157A02045CCD", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Data View Page", "DataViewPage", "", "The page to edit data views", 1, @"", "8B156612-B469-4B96-93C3-0767D9E91152" );
            // Attrib Value for Block:Report Detail, Attribute:Data View Page Page: Reports, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "1B7D7C5C-A201-4FFD-BCEC-762D126DFC2F", "8B156612-B469-4B96-93C3-0767D9E91152", @"4011cb37-28aa-46c4-99d5-826f4a9cadf5" );

            // JE: Move Following Page
            Sql( @"DECLARE @MySettingsPageId int = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = 'CF54E680-2E02-4F16-B54B-A2F2D29CD932')
UPDATE [Page]	
	SET [ParentPageId] = @MySettingsPageId
	WHERE [Guid] = 'A6AE67F7-0B46-4F9A-9C96-054E1E82F784'" );

            // MP: Fix Security for Tags
            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.Tag", 3, "Edit", false, "", 1, "83325D2B-0FF0-4641-9DC4-7933C19644D7" ); // EntityType:Rock.Model.Tag Group: <all users>
            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.Tag", 2, "Edit", true, "2C112948-FF4C-46E7-981A-0257681EADF4", 0, "EFEA738B-0949-4735-A1F3-5BB188056F70" ); // EntityType:Rock.Model.Tag Group: 2C112948-FF4C-46E7-981A-0257681EADF4 ( RSR - Staff Workers ), 
            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.Tag", 1, "Edit", true, "300BA2C8-49A3-44BA-A82A-82E3FD8C3745", 0, "A94DCA4F-FDD7-4850-A922-4CB7D1566FD1" ); // EntityType:Rock.Model.Tag Group: 300BA2C8-49A3-44BA-A82A-82E3FD8C3745 ( RSR - Staff Like Workers ), 
            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.Tag", 0, "Edit", true, "628C51A8-4613-43ED-A18D-4A6FB999273E", 0, "FF0ABEA6-6156-4B15-96D8-A7129AF825C0" ); // EntityType:Rock.Model.Tag Group: 628C51A8-4613-43ED-A18D-4A6FB999273E ( RSR - Rock Administration ), 


            // MP: Migration catchups
            // Attrib for BlockType: Group List:Display Member Count Column
            RockMigrationHelper.AddBlockTypeAttribute( "3D7FB6BE-6BBD-49F7-96B4-96310AF3048A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Member Count Column", "DisplayMemberCountColumn", "", "Should the Member Count column be displayed? Does not affect lists with a person context.", 7, @"True", "FDD84597-E3E8-4E91-A72F-C6538B085310" );

            // Attrib for BlockType: Group List:Limit to Active Status
            RockMigrationHelper.AddBlockTypeAttribute( "3D7FB6BE-6BBD-49F7-96B4-96310AF3048A", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Limit to Active Status", "LimittoActiveStatus", "", "Select which groups to show, based on active status. Select [All] to let the user filter by active status.", 10, @"all", "B4133552-42B6-4053-90B9-33B882B72D2D" );

            // Attrib for BlockType: Benevolence Request List:Case Worker Role
            RockMigrationHelper.AddBlockTypeAttribute( "3131C55A-8753-435F-85F3-DF777EFBD1C8", "7BD25DC9-F34A-478D-BEF9-0C787F5D39B8", "Case Worker Role", "CaseWorkerRole", "", "The security role to draw case workers from", 0, @"02FA0881-3552-42B8-A519-D021139B800F", "1D3A87F6-1A27-407E-9218-F92B2E08AEB8" );

            // Attrib for BlockType: Report Detail:Database Timeout
            RockMigrationHelper.AddBlockTypeAttribute( "E431DBDF-5C65-45DC-ADC5-157A02045CCD", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Database Timeout", "DatabaseTimeout", "", "The number of seconds to wait before reporting a database timeout.", 0, @"180", "3DB8E441-CEC9-476E-A5E7-4D5AABE6FD6F" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // MP: Fix Security for Tags
            RockMigrationHelper.DeleteSecurityAuth( "83325D2B-0FF0-4641-9DC4-7933C19644D7" ); // EntityType:Rock.Model.Tag Group: <all users>
            RockMigrationHelper.DeleteSecurityAuth( "EFEA738B-0949-4735-A1F3-5BB188056F70" ); // EntityType:Rock.Model.Tag Group: 2C112948-FF4C-46E7-981A-0257681EADF4 ( RSR - Staff Workers ), 
            RockMigrationHelper.DeleteSecurityAuth( "A94DCA4F-FDD7-4850-A922-4CB7D1566FD1" ); // EntityType:Rock.Model.Tag Group: 300BA2C8-49A3-44BA-A82A-82E3FD8C3745 ( RSR - Staff Like Workers ), 
            RockMigrationHelper.DeleteSecurityAuth( "FF0ABEA6-6156-4B15-96D8-A7129AF825C0" ); // EntityType:Rock.Model.Tag Group: 628C51A8-4613-43ED-A18D-4A6FB999273E ( RSR - Rock Administration ), 

            // DT: Add Data View Page Setting to Report Detail
            // Attrib for BlockType: Report Detail:Data View Page
            RockMigrationHelper.DeleteAttribute( "8B156612-B469-4B96-93C3-0767D9E91152" );

            AlterColumn( "dbo.Person", "IsEmailActive", c => c.Boolean() );
        }
    }
}
