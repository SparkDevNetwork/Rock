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
    public partial class BackgroundCheckExpiringSoon : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( MigrationSQL._201508010008536_BackgroundCheckExpiringSoon );

            // MP - Attrib Value for Block:Communication History, Attribute:Entity Type Page: History, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "27F84ADB-AA13-439E-A130-FBF73698B172", "B359D61E-6644-4BB3-BF12-6D4F9CFD3CE1", @"72657ed8-d16e-492e-ac12-144c5e7567e7" );

            // JE: Add block setting for the person profile page to the Connection Request Detail.
            // Attrib for BlockType: Connection Request Detail:Person Profile Page
            RockMigrationHelper.AddBlockTypeAttribute( "A7961C9C-2EF5-44DF-BEA5-C334B42A90E2", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Profile Page", "PersonProfilePage", "", "Page used for viewing a person's profile. If set a view profile button will show for each group member.", 2, @"", "291790F9-DC24-4675-B9BA-97DBA268D474" );

            // Attrib Value for Block:Connection Request Detail, Attribute:Person Profile Page Page: Connection Request Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "94187C5A-7F6A-4D45-B5C2-C3C8673E8817", "291790F9-DC24-4675-B9BA-97DBA268D474", @"08dbd8a5-2c35-4146-b4a8-0f7652348b25" );


            // MP: Fix for ValueAsNumeric not working for Decimals
            Sql( @"
    DROP INDEX [IX_ValueAsNumeric] ON [AttributeValue]
    ALTER TABLE AttributeValue DROP COLUMN ValueAsNumeric
    ALTER TABLE AttributeValue ADD ValueAsNumeric AS (
        CASE 
            WHEN len([value]) < (100)
                THEN CASE 
                        WHEN (
                                isnumeric([value]) = (1)
                                AND NOT [value] LIKE '%[^0-9.]%'
                                )
                            THEN TRY_CAST([value] AS [numeric](38, 10))
                        END
            END
        ) PERSISTED
    CREATE NONCLUSTERED INDEX [IX_ValueAsNumeric] ON [AttributeValue] ([ValueAsNumeric] )
" );

            // JE: Alter entity security for content items. Currently a web admin can't add items to new content channels.
            const string WEB_ADMIN_ROLE = "1918E74F-C00D-4DDD-94C4-2E7209CE12C3";
            const string COMM_ADMIN_ROLE = "B1906B7D-1A1E-41B9-BBA4-F4482CECAF7B";

            //
            // Content Channel

            // Add security for the Web Admin 
            RockMigrationHelper.AddSecurityAuthForEntityType( typeof( Rock.Model.ContentChannel ).FullName, 0, Rock.Security.Authorization.VIEW, true, WEB_ADMIN_ROLE, Rock.Model.SpecialRole.None.ConvertToInt(), "029754CE-619A-37BC-4125-5D2596359844" );
            RockMigrationHelper.AddSecurityAuthForEntityType( typeof( Rock.Model.ContentChannel ).FullName, 0, Rock.Security.Authorization.EDIT, true, WEB_ADMIN_ROLE, Rock.Model.SpecialRole.None.ConvertToInt(), "B1DA36B9-1676-5189-4489-08F735162656" );
            RockMigrationHelper.AddSecurityAuthForEntityType( typeof( Rock.Model.ContentChannel ).FullName, 0, Rock.Security.Authorization.ADMINISTRATE, true, WEB_ADMIN_ROLE, Rock.Model.SpecialRole.None.ConvertToInt(), "259BDAD7-EA66-B9B8-447F-5338A6C573B7" );
            RockMigrationHelper.AddSecurityAuthForEntityType( typeof( Rock.Model.ContentChannel ).FullName, 0, Rock.Security.Authorization.APPROVE, true, WEB_ADMIN_ROLE, Rock.Model.SpecialRole.None.ConvertToInt(), "7A79A971-9D94-BC9C-47BE-003C8D7D2AEA" );

            // Add security for the Comm Admin 
            RockMigrationHelper.AddSecurityAuthForEntityType( typeof( Rock.Model.ContentChannel ).FullName, 0, Rock.Security.Authorization.VIEW, true, COMM_ADMIN_ROLE, Rock.Model.SpecialRole.None.ConvertToInt(), "B4C0EDF3-3624-5E88-4DED-B6DE486DE585" );
            RockMigrationHelper.AddSecurityAuthForEntityType( typeof( Rock.Model.ContentChannel ).FullName, 0, Rock.Security.Authorization.EDIT, true, COMM_ADMIN_ROLE, Rock.Model.SpecialRole.None.ConvertToInt(), "E223895B-CA29-C596-4B3A-A90D832EF80D" );
            RockMigrationHelper.AddSecurityAuthForEntityType( typeof( Rock.Model.ContentChannel ).FullName, 0, Rock.Security.Authorization.ADMINISTRATE, true, COMM_ADMIN_ROLE, Rock.Model.SpecialRole.None.ConvertToInt(), "D7D05690-D0AE-08B9-4C00-42A95A5E75C2" );
            RockMigrationHelper.AddSecurityAuthForEntityType( typeof( Rock.Model.ContentChannel ).FullName, 0, Rock.Security.Authorization.APPROVE, true, COMM_ADMIN_ROLE, Rock.Model.SpecialRole.None.ConvertToInt(), "C87148FD-1A93-3E96-43B1-A9E74537334E" );

            //
            // Content Channel Item

            // Add security for the Web Admin 
            RockMigrationHelper.AddSecurityAuthForEntityType( typeof( Rock.Model.ContentChannelItem ).FullName, 0, Rock.Security.Authorization.VIEW, true, WEB_ADMIN_ROLE, Rock.Model.SpecialRole.None.ConvertToInt(), "24BA1C8C-3A88-D3AE-4183-90AE5B3C9BEE" );
            RockMigrationHelper.AddSecurityAuthForEntityType( typeof( Rock.Model.ContentChannelItem ).FullName, 0, Rock.Security.Authorization.EDIT, true, WEB_ADMIN_ROLE, Rock.Model.SpecialRole.None.ConvertToInt(), "382A03DE-8536-D5B4-4D5F-ED88734C2B1F" );
            RockMigrationHelper.AddSecurityAuthForEntityType( typeof( Rock.Model.ContentChannelItem ).FullName, 0, Rock.Security.Authorization.ADMINISTRATE, true, WEB_ADMIN_ROLE, Rock.Model.SpecialRole.None.ConvertToInt(), "E085C006-625A-B794-4388-834998432F1B" );

            // Add security for the Comm Admin
            RockMigrationHelper.AddSecurityAuthForEntityType( typeof( Rock.Model.ContentChannelItem ).FullName, 0, Rock.Security.Authorization.VIEW, true, COMM_ADMIN_ROLE, Rock.Model.SpecialRole.None.ConvertToInt(), "19B83413-1C52-07A2-4E83-DD925622470B" );
            RockMigrationHelper.AddSecurityAuthForEntityType( typeof( Rock.Model.ContentChannelItem ).FullName, 0, Rock.Security.Authorization.EDIT, true, COMM_ADMIN_ROLE, Rock.Model.SpecialRole.None.ConvertToInt(), "80D085C0-08C1-6199-43CE-918E45D27EBA" );
            RockMigrationHelper.AddSecurityAuthForEntityType( typeof( Rock.Model.ContentChannelItem ).FullName, 0, Rock.Security.Authorization.ADMINISTRATE, true, COMM_ADMIN_ROLE, Rock.Model.SpecialRole.None.ConvertToInt(), "11C595E9-74B3-2E85-4EEB-A82CCF13A059" );

            //
            // Content Channel Type

            // Add security for the Web Admin 
            RockMigrationHelper.AddSecurityAuthForEntityType( typeof( Rock.Model.ContentChannelType ).FullName, 0, Rock.Security.Authorization.VIEW, true, WEB_ADMIN_ROLE, Rock.Model.SpecialRole.None.ConvertToInt(), "B2D15F06-6927-48B9-44B6-44B26A459856" );
            RockMigrationHelper.AddSecurityAuthForEntityType( typeof( Rock.Model.ContentChannelType ).FullName, 0, Rock.Security.Authorization.EDIT, true, WEB_ADMIN_ROLE, Rock.Model.SpecialRole.None.ConvertToInt(), "5E8EB756-1AA0-DDB0-4497-785CB80BF7C5" );
            RockMigrationHelper.AddSecurityAuthForEntityType( typeof( Rock.Model.ContentChannelType ).FullName, 0, Rock.Security.Authorization.ADMINISTRATE, true, WEB_ADMIN_ROLE, Rock.Model.SpecialRole.None.ConvertToInt(), "D240FD48-0BEA-0980-407A-5E9DD3937446" );

            // Add security for the Comm Admin
            RockMigrationHelper.AddSecurityAuthForEntityType( typeof( Rock.Model.ContentChannelType ).FullName, 0, Rock.Security.Authorization.VIEW, true, COMM_ADMIN_ROLE, Rock.Model.SpecialRole.None.ConvertToInt(), "28C110FC-FD6F-62B2-429A-965296B65996" );
            RockMigrationHelper.AddSecurityAuthForEntityType( typeof( Rock.Model.ContentChannelType ).FullName, 0, Rock.Security.Authorization.EDIT, true, COMM_ADMIN_ROLE, Rock.Model.SpecialRole.None.ConvertToInt(), "EB61DE5A-3236-ECAB-4788-3C000133A9C1" );
            RockMigrationHelper.AddSecurityAuthForEntityType( typeof( Rock.Model.ContentChannelType ).FullName, 0, Rock.Security.Authorization.ADMINISTRATE, true, COMM_ADMIN_ROLE, Rock.Model.SpecialRole.None.ConvertToInt(), "00DEFAF8-233C-9689-4732-EA58608E4AE6" );




        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
