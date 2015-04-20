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
    public partial class OrganizationUnit : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // create group type
            RockMigrationHelper.UpdateGroupType( "Organization Unit", "Used for creating internal org charts for the organization.", "Unit", "Employee", null, false, true, true, "fa fa-sitemap", 99, null, 2, null, Rock.SystemGuid.GroupType.GROUPTYPE_ORGANIZATION_UNIT, true );

            // create group type roles
            RockMigrationHelper.UpdateGroupTypeRole( Rock.SystemGuid.GroupType.GROUPTYPE_ORGANIZATION_UNIT, "Leader", "", 1, null, null, "8438D6C5-DB92-4C99-947B-60E9100F223D", true, true, false );
            RockMigrationHelper.UpdateGroupTypeRole( Rock.SystemGuid.GroupType.GROUPTYPE_ORGANIZATION_UNIT, "Staff", "", 1, null, null, "17E516FC-76A4-4BF4-9B6F-0F859B13F563", true, false, true );

            // set the default role
            Sql( @"
                DECLARE @DefaultGroupRoleId int = ( SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid] = '17E516FC-76A4-4BF4-9B6F-0F859B13F563' )

                UPDATE [GroupType]
	            SET [DefaultGroupRoleId] = @DefaultGroupRoleId
	            WHERE [Guid] = 'AAB2E9F4-E828-4FEE-8467-73DC9DAB784C'" );

            // set allowed child group types
            Sql( @"
                DECLARE @GroupTypeId int = ( SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = 'AAB2E9F4-E828-4FEE-8467-73DC9DAB784C' )
  
                INSERT INTO [GroupTypeAssociation]
	                ([GroupTypeId], [ChildGroupTypeId])
	                VALUES
	                (@GroupTypeId, @GroupTypeId)
            " );

            // add some default groups
            RockMigrationHelper.UpdateGroup( null, Rock.SystemGuid.GroupType.GROUPTYPE_ORGANIZATION_UNIT, "Organization", "Parent unit for the entire organization.", null, 1, "EF41CD00-1266-4BE6-9130-453982014B79", true );
            RockMigrationHelper.UpdateGroup( "EF41CD00-1266-4BE6-9130-453982014B79", Rock.SystemGuid.GroupType.GROUPTYPE_ORGANIZATION_UNIT, "Childrens", "Department for childrens staff.", null, 1, "56AD5243-DED9-42F1-A922-5069A4EF7A7B", false );
            RockMigrationHelper.UpdateGroup( "EF41CD00-1266-4BE6-9130-453982014B79", Rock.SystemGuid.GroupType.GROUPTYPE_ORGANIZATION_UNIT, "Students", "Department for students staff.", null, 2, "44C85087-A452-4D13-B95B-EC92FF0F9A7F", false );
            RockMigrationHelper.UpdateGroup( "EF41CD00-1266-4BE6-9130-453982014B79", Rock.SystemGuid.GroupType.GROUPTYPE_ORGANIZATION_UNIT, "Connections", "Department for connections staff.", null, 3, "4737CD5D-DEA1-4F78-A581-D246B08CED6A", false );
            RockMigrationHelper.UpdateGroup( "EF41CD00-1266-4BE6-9130-453982014B79", Rock.SystemGuid.GroupType.GROUPTYPE_ORGANIZATION_UNIT, "Administration", "Department for administration staff.", null, 4, "5B6BE24F-349B-4630-B99C-20BB62BC8BD2", false );
        
            // add pages for the org chart
            RockMigrationHelper.AddPage( "7F2581A1-941E-4D51-8A9D-5BE9B881B003", "0CB60906-6B74-44FD-AB25-026050EF70EB", "Org Chart", "", "C3909F1A-6908-4035-BB93-EC4FBFDCC536", "fa fa-sitemap" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "C3909F1A-6908-4035-BB93-EC4FBFDCC536", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Employee Details", "", "DA8E33F3-2EEF-4C4B-87F3-715C3F107CAF", "fa fa-user" ); // Site:Rock RMS

            // Add Block to Page: Office Information, Site: Rock RMS
            RockMigrationHelper.AddBlock( "7F2581A1-941E-4D51-8A9D-5BE9B881B003", "", "2D26A2C4-62DC-4680-8219-A52EB2BC0F65", "Group Tree View", "Sidebar1", "", "", 0, "3BFEF2CC-AEA9-457E-A552-C14D69AD93FE" );

            // Add Block to Page: Org Chart, Site: Rock RMS
            RockMigrationHelper.AddBlock( "C3909F1A-6908-4035-BB93-EC4FBFDCC536", "", "2D26A2C4-62DC-4680-8219-A52EB2BC0F65", "Group Tree View", "Sidebar1", "", "", 0, "3FB66841-811D-4298-823B-06F8AFC95047" );

            // Add Block to Page: Org Chart, Site: Rock RMS
            RockMigrationHelper.AddBlock( "C3909F1A-6908-4035-BB93-EC4FBFDCC536", "", "582BEEA1-5B27-444D-BC0A-F60CEB053981", "Group Detail", "Main", "", "", 0, "C1B5CA27-8B5B-4D7F-9160-916EFAAA7D26" );

            // Add Block to Page: Org Chart, Site: Rock RMS
            RockMigrationHelper.AddBlock( "C3909F1A-6908-4035-BB93-EC4FBFDCC536", "", "88B7EFA9-7419-4D05-9F88-38B936E61EDD", "Group Member List", "Main", "", "", 1, "DB536038-A421-408F-8A33-1B95ADB8A51E" );

            // Add Block to Page: Employee Details, Site: Rock RMS
            RockMigrationHelper.AddBlock( "DA8E33F3-2EEF-4C4B-87F3-715C3F107CAF", "", "AAE2E5C3-9279-4AB0-9682-F4D19519D678", "Group Member Detail", "Main", "", "", 0, "9EFE36CD-6B19-421B-8844-B62A65B56D1C" );

            // Attrib Value for Block:Group Tree View, Attribute:Limit to Security Role Groups Page: Org Chart, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3FB66841-811D-4298-823B-06F8AFC95047", "1688837B-73CF-46C3-8880-74C46605807C", @"False" );

            // Attrib Value for Block:Group Tree View, Attribute:Group Types Page: Org Chart, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3FB66841-811D-4298-823B-06F8AFC95047", "12557F76-B0AF-4327-8884-C664B08453AE", @"aab2e9f4-e828-4fee-8467-73dc9dab784c" );

            // Attrib Value for Block:Group Tree View, Attribute:Treeview Title Page: Org Chart, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3FB66841-811D-4298-823B-06F8AFC95047", "D1583306-2504-48D2-98EE-3DE55C2806C7", @"" );

            // Attrib Value for Block:Group Member List, Attribute:Detail Page Page: Org Chart, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "DB536038-A421-408F-8A33-1B95ADB8A51E", "E4CCB79C-479F-4BEE-8156-969B2CE05973", @"da8e33f3-2eef-4c4b-87f3-715c3f107caf" );

            // Attrib Value for Block:Group Member List, Attribute:Person Profile Page Page: Org Chart, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "DB536038-A421-408F-8A33-1B95ADB8A51E", "9E139BB9-D87C-4C9F-A241-DC4620AD340B", @"08dbd8a5-2c35-4146-b4a8-0f7652348b25,7e97823a-78a8-4e8e-a337-7a20f2da9e52" );
        

            // migration roundup... yeehaa!
            //

            // DISC route
            RockMigrationHelper.AddPageRoute( "C8CEF4B0-4A09-46D2-9B6B-CD2B6D3078B1", "DISC" );

            // universal channel type
            Sql( @"INSERT INTO [ContentChannelType]
( [IsSystem], [Name], [DateRangeType], [Guid], [DisablePriority])
VALUES
( 1, 'Universal Channel Type', 1, '0A69DA05-F671-454F-A25D-99A01E10ADB8', 0)" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // delete groups
            RockMigrationHelper.DeleteGroup( "56AD5243-DED9-42F1-A922-5069A4EF7A7B" );
            RockMigrationHelper.DeleteGroup( "44C85087-A452-4D13-B95B-EC92FF0F9A7F" );
            RockMigrationHelper.DeleteGroup( "4737CD5D-DEA1-4F78-A581-D246B08CED6A" );
            RockMigrationHelper.DeleteGroup( "5B6BE24F-349B-4630-B99C-20BB62BC8BD2" );
            RockMigrationHelper.DeleteGroup( "EF41CD00-1266-4BE6-9130-453982014B79" );
            
            // delete group roles
            RockMigrationHelper.DeleteGroupTypeRole( "8438D6C5-DB92-4C99-947B-60E9100F223D" );
            RockMigrationHelper.DeleteGroupTypeRole( "17E516FC-76A4-4BF4-9B6F-0F859B13F563" );

            // delete group type
            RockMigrationHelper.DeleteGroupType( Rock.SystemGuid.GroupType.GROUPTYPE_ORGANIZATION_UNIT );

            // Remove Block: Group Member Detail, from Page: Employee Details, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "9EFE36CD-6B19-421B-8844-B62A65B56D1C" );
            // Remove Block: Group Member List, from Page: Org Chart, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "DB536038-A421-408F-8A33-1B95ADB8A51E" );
            // Remove Block: Group Detail, from Page: Org Chart, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "C1B5CA27-8B5B-4D7F-9160-916EFAAA7D26" );
            // Remove Block: Group Tree View, from Page: Org Chart, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "3FB66841-811D-4298-823B-06F8AFC95047" );
            // Remove Block: Group Tree View, from Page: Office Information, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "3BFEF2CC-AEA9-457E-A552-C14D69AD93FE" );

            RockMigrationHelper.DeletePage( "DA8E33F3-2EEF-4C4B-87F3-715C3F107CAF" ); //  Page: Employee Details, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "C3909F1A-6908-4035-BB93-EC4FBFDCC536" ); //  Page: Org Chart, Layout: Left Sidebar, Site: Rock RMS

            // delete DISC route
            Sql( @" DELETE [PageRoute] WHERE [Route] = 'DISC' " );

            // delete universal channel type
            // universal channel type
            Sql( @"DELETE [ContentChannelType] WHERE [Guid] =  '0A69DA05-F671-454F-A25D-99A01E10ADB8'" );

        }
    }
}
