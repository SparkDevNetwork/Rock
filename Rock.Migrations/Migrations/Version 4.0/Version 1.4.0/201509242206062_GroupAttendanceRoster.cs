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
    public partial class GroupAttendanceRoster : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // MP - Add Group Attendance Roster Template
            RockMigrationHelper.UpdateFieldType( "Merge Template", "", "Rock", "Rock.Field.Types.MergeTemplateFieldType", "890BAA33-5EBB-4343-A8AA-42E0E6C7467A" );

            // Add Groups MergeTemplate Category
            RockMigrationHelper.UpdateCategory( Rock.SystemGuid.EntityType.MERGE_TEMPLATE, "Groups", "fa fa-group", "", "8B792E4E-FA91-488E-B12A-B070EE3E5391" );

            Sql( MigrationSQL._201509242206062_GroupAttendanceRoster_AddGroupRosterMergeTemplate );

            // Attrib for BlockType: Group Attendance Detail:Attendance Roster Template
            RockMigrationHelper.AddBlockTypeAttribute( "FC6B5DC8-3A90-4D78-8DC2-7F7698A6E73B", "890BAA33-5EBB-4343-A8AA-42E0E6C7467A", "Attendance Roster Template", "AttendanceRosterTemplate", "", "", 0, @"", "D255FB71-5D55-490B-92A0-3C48F3AE95BF" );

            // Attrib Value for Block:Group Attendance Detail, Attribute:Attendance Roster Template Page: Attendance, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "FE956364-7B56-4E73-AD11-CB6DFB21B673", "D255FB71-5D55-490B-92A0-3C48F3AE95BF", @"69a8286b-fe23-45de-959b-7cc8c7a57911" );

            // JE: Gave Staff View/Edit rights to Website Ads and Bulletin Content Channels
            // Staff Bulletin View/Edit
            RockMigrationHelper.AddSecurityAuthForContentChannel( "3CBD6C7A-30B4-4CF5-B1B9-4216C4EEF371", 0, "Edit", true, "2C112948-FF4C-46E7-981A-0257681EADF4", Model.SpecialRole.None, "BE3EB179-CE8D-6680-46BB-4AFB9B91FF9E" );
            RockMigrationHelper.AddSecurityAuthForContentChannel( "3CBD6C7A-30B4-4CF5-B1B9-4216C4EEF371", 0, "View", true, "2C112948-FF4C-46E7-981A-0257681EADF4", Model.SpecialRole.None, "09273F89-9D62-06AB-4ED0-E8133507D2EE" );

            // StaffLike Bulletin View/Edit
            RockMigrationHelper.AddSecurityAuthForContentChannel( "3CBD6C7A-30B4-4CF5-B1B9-4216C4EEF371", 0, "Edit", true, "300BA2C8-49A3-44BA-A82A-82E3FD8C3745", Model.SpecialRole.None, "F0BCD3B9-D023-F8AB-45E0-2BA6D39E5C86" );
            RockMigrationHelper.AddSecurityAuthForContentChannel( "3CBD6C7A-30B4-4CF5-B1B9-4216C4EEF371", 0, "View", true, "300BA2C8-49A3-44BA-A82A-82E3FD8C3745", Model.SpecialRole.None, "240B36AE-CF53-2EAD-4DA3-0EEFC4AD0D3F" );

            // Staff Website Ads View/Edit
            RockMigrationHelper.AddSecurityAuthForContentChannel( "8E213BB1-9E6F-40C1-B468-B3F8A60D5D24", 0, "Edit", true, "2C112948-FF4C-46E7-981A-0257681EADF4", Model.SpecialRole.None, "B8D7E38A-2B33-F4AF-477D-9188B23C982A" );
            RockMigrationHelper.AddSecurityAuthForContentChannel( "8E213BB1-9E6F-40C1-B468-B3F8A60D5D24", 0, "View", true, "2C112948-FF4C-46E7-981A-0257681EADF4", Model.SpecialRole.None, "44B9C8E2-2F89-D09C-4B38-A864298D13D6" );

            // StaffLike Website Ads View/Edit
            RockMigrationHelper.AddSecurityAuthForContentChannel( "8E213BB1-9E6F-40C1-B468-B3F8A60D5D24", 0, "Edit", true, "300BA2C8-49A3-44BA-A82A-82E3FD8C3745", Model.SpecialRole.None, "2B9DF845-B094-C3B4-43D7-E325795F6EE9" );
            RockMigrationHelper.AddSecurityAuthForContentChannel( "8E213BB1-9E6F-40C1-B468-B3F8A60D5D24", 0, "View", true, "300BA2C8-49A3-44BA-A82A-82E3FD8C3745", Model.SpecialRole.None, "541E70B0-686A-8E84-49F9-F25152CAD24D" );

            // DT: Set output cache duration to 0
            Sql( @"UPDATE [Block] SET [OutputCacheDuration] = 0" );

            // DT: Add Secondary Audience to channel filters
            Sql( MigrationSQL._201509242206062_GroupAttendanceRoster_SecondaryAudience );

            // MP: fix for PersonDuplicateFinder stored proc sometimes getting errors on MERGE
            Sql( MigrationSQL._201509242206062_GroupAttendanceRoster_spCrm_PersonDuplicateFinder );

            //
            // DT: Add Transaction Code index to transaction table
            //
            Sql( @"
    IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_TransactionCode' AND object_id = OBJECT_ID('FinancialTransaction'))
    BEGIN
        CREATE NONCLUSTERED INDEX [IX_TransactionCode] ON [dbo].[FinancialTransaction] ( [TransactionCode] ASC ) ON [PRIMARY]
    END
" );
            // MP: Remove "Enable Page View Tracking" global attribute
            Sql( @"DELETE FROM [Attribute] WHERE [Guid] = '950096BE-1F3C-49B8-81F9-757D27C90310'" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // MP - (Down) Add Group Attendance Roster Template
            // Attrib for BlockType: Group Attendance Detail:Attendance Roster Template
            RockMigrationHelper.DeleteAttribute( "D255FB71-5D55-490B-92A0-3C48F3AE95BF" );

            Sql( @"
delete From MergeTemplate where [Guid] = '69A8286B-FE23-45DE-959B-7CC8C7A57911'
delete from BinaryFile where [Guid] = '2a81b6d1-4729-4a1c-a19b-8e5f02169e56'" );
        }
    }
}
