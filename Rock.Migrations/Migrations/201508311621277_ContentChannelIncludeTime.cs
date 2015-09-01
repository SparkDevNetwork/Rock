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
    public partial class ContentChannelIncludeTime : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.ContentChannelType", "IncludeTime", c => c.Boolean(nullable: false));
            Sql( "UPDATE [ContentChannelType] SET [IncludeTime] = 1" );

            // MP: Bank Checks DefinedValue
            RockMigrationHelper.AddDefinedTypeAttribute( "4F02B41E-AB7D-4345-8A97-3904DDD89B01", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show in Check Scanner", "core.ShowInCheckScanner", "", 37, "False", "B4D4CCD3-94CD-4584-94FD-C677213A1A51" );
            RockMigrationHelper.AddAttributeQualifier( "B4D4CCD3-94CD-4584-94FD-C677213A1A51", "falsetext", "No", "2CDE5641-4FF8-422B-8607-EFA28550B06A" );
            RockMigrationHelper.AddAttributeQualifier( "B4D4CCD3-94CD-4584-94FD-C677213A1A51", "truetext", "Yes", "581EE78A-C122-413F-91C2-D30B2D9DA937" );
            RockMigrationHelper.AddDefinedValue( "4F02B41E-AB7D-4345-8A97-3904DDD89B01", "Bank Checks", "Transactions that originated from a bank's bill pay system", "61E46A46-7399-4817-A6EC-3D8495E2316E", false );
            RockMigrationHelper.AddDefinedValueAttributeValue( "61E46A46-7399-4817-A6EC-3D8495E2316E", "B4D4CCD3-94CD-4584-94FD-C677213A1A51", @"True" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "BE7ECF50-52BC-4774-808D-574BA842DB98", "B4D4CCD3-94CD-4584-94FD-C677213A1A51", @"True" );

            // MP: GroupMember PageRoute
            RockMigrationHelper.AddPageRoute( Rock.SystemGuid.Page.GROUP_MEMBER_DETAIL_GROUP_VIEWER, "GroupMember/{GroupMemberId}" );

            // DT: New Transaction List Settings
            // Attrib for BlockType: Transaction List:Show Only Active Accounts on Filter
            RockMigrationHelper.AddBlockTypeAttribute( "E04320BC-67C3-452D-9EF6-D74D8C177154", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Only Active Accounts on Filter", "ActiveAccountsOnlyFilter", "", "If account filter is displayed, only list active accounts", 2, @"False", "81AD58EA-F94B-42A1-AC57-16902B717092" );
            // Attrib for BlockType: Transaction List:Transaction Types
            RockMigrationHelper.AddBlockTypeAttribute( "E04320BC-67C3-452D-9EF6-D74D8C177154", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Transaction Types", "TransactionTypes", "", "Optional list of transation types to limit the list to (if none are selected all types will be included).", 3, @"", "293F8A3E-020A-4260-8817-3E368CF31ABB" );
            // Attrib Value for Block:Transaction List, Attribute:Transaction Types Page: Contributions, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9382B285-3EF6-47F7-94BB-A47C498196A3", "293F8A3E-020A-4260-8817-3E368CF31ABB", @"2d607262-52d6-4724-910d-5c6e8fb89acc" );

            // MP: Event Calendar Security Updates
            // add entity APPROVE security for Event Calendar
            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.EventCalendar", 0, Rock.Security.Authorization.APPROVE, true, Rock.SystemGuid.Group.GROUP_CALENDAR_ADMINISTRATORS, 0, "25EB8621-70C5-4467-B161-72A067880560" );
            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.EventCalendar", 1, Rock.Security.Authorization.APPROVE, true, Rock.SystemGuid.Group.GROUP_COMMUNICATION_ADMINISTRATORS, 0, "A7B792E5-1367-4966-9795-7566A1E936C3" );
            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.EventCalendar", 2, Rock.Security.Authorization.APPROVE, false, null, Rock.Model.SpecialRole.AllUsers.ConvertToInt(), "4F8D78AD-4161-4124-874C-C23F8500474F" );
            // add page ADMINISTRATE security for Calendars 
            RockMigrationHelper.AddSecurityAuthForPage( Rock.SystemGuid.Page.CALENDARS, 1, Rock.Security.Authorization.ADMINISTRATE, true, Rock.SystemGuid.Group.GROUP_COMMUNICATION_ADMINISTRATORS, Rock.Model.SpecialRole.None.ConvertToInt(), "C76D94F5-96C8-4371-80F5-99C50089D430" );
            RockMigrationHelper.AddSecurityAuthForPage( Rock.SystemGuid.Page.CALENDARS, 0, Rock.Security.Authorization.ADMINISTRATE, true, Rock.SystemGuid.Group.GROUP_CALENDAR_ADMINISTRATORS, Rock.Model.SpecialRole.None.ConvertToInt(), "2DEE6ECD-461D-487F-9932-9DDDE08A641D" );
            // add page EDIT security for Calendars 
            RockMigrationHelper.AddSecurityAuthForPage( Rock.SystemGuid.Page.CALENDARS, 1, Rock.Security.Authorization.EDIT, true, Rock.SystemGuid.Group.GROUP_STAFF_MEMBERS, Rock.Model.SpecialRole.None.ConvertToInt(), "A796B28F-E0A6-4E66-9981-CDB0F8A24565" );
            RockMigrationHelper.AddSecurityAuthForPage( Rock.SystemGuid.Page.CALENDARS, 2, Rock.Security.Authorization.EDIT, true, Rock.SystemGuid.Group.GROUP_STAFF_LIKE_MEMBERS, Rock.Model.SpecialRole.None.ConvertToInt(), "32E66F8B-A145-488B-A8CD-D76B334C3FC2" );

            // MK: New block attribute/vales
            // Attrib for BlockType: Data View Detail: Data View Page
            RockMigrationHelper.AddBlockTypeAttribute( "EB279DF9-D817-4905-B6AC-D9883F0DA2E4", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Data View Detail Page", "DataViewDetailPage", "", "Page used to view a data view.", 0, @"", "11C7EF84-14D5-4CAF-940B-B0A2AAD9DFE9" );
            // Attrib for BlockType: Report Detail: Report Page
            RockMigrationHelper.AddBlockTypeAttribute( "EB279DF9-D817-4905-B6AC-D9883F0DA2E4", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Report Detail Page", "ReportDetailPage", "", "Page used to view a report.", 1, @"", "2BBE2EF5-9FD7-411D-8C36-8F0DAF5A73C8" );
            // Attrib value for Block: Data View Detail, Attribute: Data View Detail Page, Page: Data Views, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7868AF5C-6512-4F33-B127-93B159E08A56", "11C7EF84-14D5-4CAF-940B-B0A2AAD9DFE9", "4011CB37-28AA-46C4-99D5-826F4A9CADF5" );
            // Attrib value for Block: Data View Detail, Attribute: Report Detail Page, Page: Data Views, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7868AF5C-6512-4F33-B127-93B159E08A56", "2BBE2EF5-9FD7-411D-8C36-8F0DAF5A73C8", "0FDF1F63-CFB3-4F8E-AC5D-A5312B522D6D" );

            // JE: Add RSR - Family Manager security group
            Sql( @"
    IF NOT EXISTS(SELECT * FROM [Group] WHERE [Guid] = 'D832E933-1972-4482-B24D-6AF0AC6BDF20' )
    BEGIN
	    DECLARE @SecurityGroupTypeId int = (SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = 'AECE949F-704C-483E-A4FB-93D5E4720C4C')
	    INSERT INTO [Group] ([IsSystem], [GroupTypeId], [Name], [Description], [IsSecurityRole], [IsActive], [Order], [Guid], [IsPublic])
	    VALUES (1, @SecurityGroupTypeId, 'RSR - Family Manager', 'Used to grant access for use of the Family Registration application.', 1, 1, 100, 'D832E933-1972-4482-B24D-6AF0AC6BDF20', 0)
    END
" );

            // JE: Disable Name Search on Connection Search
            RockMigrationHelper.AddBlockAttributeValue( "2B60FE4A-5AD8-4F0A-9205-0744485C3DDD", "4361D6EE-B95F-4DE7-88C5-F53A5494A1F6", @"False" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // MK: New block attribute/vales
            // Attrib for BlockType: Data View Detail: Data View Page
            RockMigrationHelper.DeleteAttribute( "11C7EF84-14D5-4CAF-940B-B0A2AAD9DFE9" );
            // Attrib for BlockType: Report Detail: Report Page
            RockMigrationHelper.DeleteAttribute( "2BBE2EF5-9FD7-411D-8C36-8F0DAF5A73C8" );
        
            // MP: Event Calendar Security Updates
            RockMigrationHelper.DeleteSecurityAuth( "25EB8621-70C5-4467-B161-72A067880560" );
            RockMigrationHelper.DeleteSecurityAuth( "A7B792E5-1367-4966-9795-7566A1E936C3" );
            RockMigrationHelper.DeleteSecurityAuth( "4F8D78AD-4161-4124-874C-C23F8500474F" );
            RockMigrationHelper.DeleteSecurityAuth( "C76D94F5-96C8-4371-80F5-99C50089D430" );
            RockMigrationHelper.DeleteSecurityAuth( "2DEE6ECD-461D-487F-9932-9DDDE08A641D" );
            RockMigrationHelper.DeleteSecurityAuth( "A796B28F-E0A6-4E66-9981-CDB0F8A24565" );
            RockMigrationHelper.DeleteSecurityAuth( "32E66F8B-A145-488B-A8CD-D76B334C3FC2" );

            // DT: New Transaction List Settings
            // Attrib for BlockType: Transaction List:Transaction Types
            RockMigrationHelper.DeleteAttribute( "293F8A3E-020A-4260-8817-3E368CF31ABB" );
            // Attrib for BlockType: Transaction List:Show Only Active Accounts on Filter
            RockMigrationHelper.DeleteAttribute( "81AD58EA-F94B-42A1-AC57-16902B717092" );

            // MP: Bank Checks DefinedValue
            RockMigrationHelper.DeleteAttribute( "B4D4CCD3-94CD-4584-94FD-C677213A1A51" ); // core.ShowInCheckScanner
            RockMigrationHelper.DeleteDefinedValue( "61E46A46-7399-4817-A6EC-3D8495E2316E" ); // Bank Checks

            DropColumn("dbo.ContentChannelType", "IncludeTime");
        }
    }
}
