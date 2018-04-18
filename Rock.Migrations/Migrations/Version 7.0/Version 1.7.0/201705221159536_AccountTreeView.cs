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
    public partial class AccountTreeView : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // DT: Update Accounts Page to use Tree View
            RockMigrationHelper.DeletePage( "75130E27-405A-4935-AB27-0EDE11F6E8B3" );

            Sql( @"
    DECLARE @LayoutId int = ( SELECT TOP 1 [Id] FROM [Layout] WHERE [Guid] = '0CB60906-6B74-44FD-AB25-026050EF70EB' )
    UPDATE [Page] SET [LayoutId] = @LayoutId WHERE [Guid] = '2B630A3B-E081-4204-A3E4-17BB3A5F063D'
" );
            RockMigrationHelper.UpdateBlockType( "Account Tree View", "Creates a navigation tree for accounts", "~/Blocks/Finance/AccountTreeView.ascx", "Finance", "EC6ECB2B-665F-43FC-9FF4-A6B1CD5F2AE6" );

            // Add Block to Page: Accounts, Site: Rock RMS
            RockMigrationHelper.AddBlock( "2B630A3B-E081-4204-A3E4-17BB3A5F063D", "", "EC6ECB2B-665F-43FC-9FF4-A6B1CD5F2AE6", "Account Tree View", "Sidebar1", @"", @"", 0, "7F52E6B6-B4ED-4ECE-BE19-98BD3B939965" );
            // Add Block to Page: Accounts, Site: Rock RMS
            RockMigrationHelper.AddBlock( "2B630A3B-E081-4204-A3E4-17BB3A5F063D", "", "DCD63280-B661-48AA-8DEB-F5ED63C7AB77", "Account Detail", "Main", @"", @"", 0, "4305FFD9-48FD-4918-8D7B-957429B78C87" );
            // update block order for pages with new blocks if the page,zone has multiple blocks
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '4305FFD9-48FD-4918-8D7B-957429B78C87'" );  // Page: Accounts,  Zone: Main,  Block: Account Detail
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '7F52E6B6-B4ED-4ECE-BE19-98BD3B939965'" );  // Page: Accounts,  Zone: Sidebar1,  Block: Account Tree View
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '52D2D2E1-43F7-4259-8F9A-936694DAEF76'" );  // Page: Accounts,  Zone: Main,  Block: Account List

            // Attrib for BlockType: Account Tree View:Initial Active Setting
            RockMigrationHelper.UpdateBlockTypeAttribute( "EC6ECB2B-665F-43FC-9FF4-A6B1CD5F2AE6", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Initial Active Setting", "InitialActiveSetting", "", "Select whether to initially show all or just active accounts in the treeview", 3, @"1", "8FD1D784-6EAA-496A-B579-CE38C43E3391" );
            // Attrib for BlockType: Account Tree View:Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "EC6ECB2B-665F-43FC-9FF4-A6B1CD5F2AE6", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 4, @"", "C6966A37-0DFB-4D1F-9569-3230663C9651" );
            // Attrib for BlockType: Account Tree View:Treeview Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "EC6ECB2B-665F-43FC-9FF4-A6B1CD5F2AE6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Treeview Title", "TreeviewTitle", "", "Account Tree View", 1, @"", "20A26493-96D8-4F58-B6D0-601E10DD964C" );
            // Attrib for BlockType: Account Tree View:Show Settings Panel
            RockMigrationHelper.UpdateBlockTypeAttribute( "EC6ECB2B-665F-43FC-9FF4-A6B1CD5F2AE6", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Settings Panel", "ShowFilterOption", "", "", 2, @"True", "7F89D075-A63D-4901-8A8D-A16000889B37" );

            // Attrib Value for Block:Account Tree View, Attribute:Initial Active Setting Page: Accounts, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7F52E6B6-B4ED-4ECE-BE19-98BD3B939965", "8FD1D784-6EAA-496A-B579-CE38C43E3391", @"1" );
            // Attrib Value for Block:Account Tree View, Attribute:Detail Page Page: Accounts, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7F52E6B6-B4ED-4ECE-BE19-98BD3B939965", "C6966A37-0DFB-4D1F-9569-3230663C9651", @"2b630a3b-e081-4204-a3e4-17bb3a5f063d" );
            // Attrib Value for Block:Account Tree View, Attribute:Treeview Title Page: Accounts, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7F52E6B6-B4ED-4ECE-BE19-98BD3B939965", "20A26493-96D8-4F58-B6D0-601E10DD964C", @"" );
            // Attrib Value for Block:Account Tree View, Attribute:Show Settings Panel Page: Accounts, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7F52E6B6-B4ED-4ECE-BE19-98BD3B939965", "7F89D075-A63D-4901-8A8D-A16000889B37", @"True" );


            RockMigrationHelper.AddBlockAttributeValue( "52D2D2E1-43F7-4259-8F9A-936694DAEF76", "EB24B424-2E36-4769-BE92-64B9673AC469", "2b630a3b-e081-4204-a3e4-17bb3a5f063d" );

            //NA: Add GroupDetailPage block setting to DataViewDetail block
            // Attrib for BlockType: Data View Detail: Group Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "EB279DF9-D817-4905-B6AC-D9883F0DA2E4", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Detail Page", "GroupDetailPage", "", "The page to display a group.", 2, @"", "D890811A-6A7E-4911-A8C3-A6FFD3B3E61D" );

            // Attrib value for Block: Data View Detail, Attribute: Group Detail Page, Page: Data Views, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7868AF5C-6512-4F33-B127-93B159E08A56", "D890811A-6A7E-4911-A8C3-A6FFD3B3E61D", @"4e237286-b715-4109-a578-c1445ec02707" );

            //DT: Add Staff "Edit" to System Group Types
            SetGroupTypeSecurity( Rock.SystemGuid.GroupType.GROUPTYPE_GENERAL );
            SetGroupTypeSecurity( Rock.SystemGuid.GroupType.GROUPTYPE_ORGANIZATION_UNIT );
            SetGroupTypeSecurity( Rock.SystemGuid.GroupType.GROUPTYPE_SERVING_TEAM );
            SetGroupTypeSecurity( Rock.SystemGuid.GroupType.GROUPTYPE_SMALL_GROUP );
            SetGroupTypeSecurity( Rock.SystemGuid.GroupType.GROUPTYPE_SMALL_GROUP_SECTION );
        }

        /// <summary>
        /// Gives 'Staff' and 'StaffLike' roles 'Edit' access to a group type if there is not curretnly any other explicit roles with edit security.
        /// </summary>
        /// <param name="groupTypeGuid">The group type unique identifier.</param>
        private void SetGroupTypeSecurity( string groupTypeGuid )
        {
            Sql( string.Format( @"

    DECLARE @GroupTypeId int = ( SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = '{0}' )
    DECLARE @GroupTypeEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.GroupType' )

    IF @GroupTypeId IS NOT NULL AND @GroupTypeEntityTypeId IS NOT NULL
    BEGIN
	
	    IF NOT EXISTS ( 
		    SELECT [Id] 
		    FROM [Auth] 
		    WHERE [EntityTypeId] = @GroupTypeEntityTypeId
		    AND [EntityId] = @GroupTypeId
		    AND [Action] = 'Edit'
	    )
	    BEGIN

		    DECLARE @Order int = 0

		    DECLARE @StaffRoleId int = ( SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = '2C112948-FF4C-46E7-981A-0257681EADF4' )
		    IF @StaffRoleId IS NOT NULL 
		    BEGIN
			    INSERT INTO [Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] )
			    VALUES ( @GroupTypeEntityTypeId, @GroupTypeId, @Order, 'Edit', 'A', 0, @StaffRoleId, NEWID() )
			    SET @Order = 1
		    END

		    DECLARE @StaffLikeRoleId int = ( SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = '300BA2C8-49A3-44BA-A82A-82E3FD8C3745' )
		    IF @StaffLikeRoleId IS NOT NULL 
		    BEGIN
			    INSERT INTO [Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] )
			    VALUES ( @GroupTypeEntityTypeId, @GroupTypeId, @Order, 'Edit', 'A', 0, @StaffRoleId, NEWID() )
			    SET @Order = 1
		    END

	    END

    END

", groupTypeGuid ) );
        }
    

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            //NA: Add GroupDetailPage block setting to DataViewDetail block
            // Attrib for BlockType: Data View Detail: Group Detail Page
            RockMigrationHelper.DeleteAttribute( "D890811A-6A7E-4911-A8C3-A6FFD3B3E61D" );
        }
    }
}

