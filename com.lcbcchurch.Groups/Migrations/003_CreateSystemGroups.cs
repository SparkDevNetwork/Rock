// <copyright>
// Copyright by LCBC Church
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;

namespace com.lcbcchurch.Groups.Migrations
{
    [MigrationNumber( 3, "1.0.14" )]
    class CreateSystemGroups : Migration
    {
        public override void Up()
        {
            // Add Group Types 15708E27-CC26-435F-B8B8-5E6CC48DDDA6
            RockMigrationHelper.AddGroupType( "System", "Holds system-level group types.", "Group", "Member", false, true, true, "fa fa-cogs", 0, null, 0, null, "15708E27-CC26-435F-B8B8-5E6CC48DDDA6" );
            RockMigrationHelper.AddGroupType( "Workflow Routing", "Holds groups that control routing of a Workflow to a worker.", "Group", "Member", false, true, true, "fa fa-code-fork", 0, null, 0, null, "943708E0-170B-4935-AA3A-3DF958D8D2C8" );

            // Add Group Type Associations
            AddGroupTypeAssociation( "15708E27-CC26-435F-B8B8-5E6CC48DDDA6", "15708E27-CC26-435F-B8B8-5E6CC48DDDA6" );  // System > System
            AddGroupTypeAssociation( "15708E27-CC26-435F-B8B8-5E6CC48DDDA6", "943708E0-170B-4935-AA3A-3DF958D8D2C8" );  // System > Workflow Routing

            // Add Group Type Roles
            RockMigrationHelper.UpdateGroupTypeRole( "15708E27-CC26-435F-B8B8-5E6CC48DDDA6", "Member", "", 0, null, null, "88C1908F-7FAD-421D-B9F6-272E7C2DEF5B", false, false, true ); // System > Member
            RockMigrationHelper.UpdateGroupTypeRole( "943708E0-170B-4935-AA3A-3DF958D8D2C8", "Member", "", 0, null, null, "AF296AD9-CAC8-4941-8645-30DBDB8B6E4B", false, false, true ); // Workflow Routing > Member

            // Add Group Type Member Attributes
            RockMigrationHelper.AddGroupTypeGroupMemberAttribute( "943708E0-170B-4935-AA3A-3DF958D8D2C8", "69254F91-C97F-4C2D-9ACB-1683B088097B", "Campus", @"", 0, "Campus", "66AE33E8-B374-4F07-BBE3-4096F1CBCE88" );

            // Create Groups
            RockMigrationHelper.UpdateGroup( null, "15708E27-CC26-435F-B8B8-5E6CC48DDDA6", "System", "", null, 0, "D791D411-2434-4D2B-A9AC-E621BFAEC6E5", false, false, true ); // System
            RockMigrationHelper.UpdateGroup( "D791D411-2434-4D2B-A9AC-E621BFAEC6E5", "15708E27-CC26-435F-B8B8-5E6CC48DDDA6", "Connections", "", null, 0, "ED3CF5DF-E4B0-4D18-A3C1-4FE9BBA83EB0", false, false, true ); // Connections
            RockMigrationHelper.UpdateGroup( "ED3CF5DF-E4B0-4D18-A3C1-4FE9BBA83EB0", "15708E27-CC26-435F-B8B8-5E6CC48DDDA6", "Care Requests", "", null, 0, "4890D767-15C6-465B-9D77-4A6078C59444", false, false, true ); // Care Requests
            
            RockMigrationHelper.UpdateGroup( "4890D767-15C6-465B-9D77-4A6078C59444", "15708E27-CC26-435F-B8B8-5E6CC48DDDA6", "Berks PRT", "", null, 0, "286A2DD3-84E5-4D05-9D3E-66D2B66E26F6", false, false, true ); // Berks PRT
            RockMigrationHelper.UpdateGroup( "4890D767-15C6-465B-9D77-4A6078C59444", "15708E27-CC26-435F-B8B8-5E6CC48DDDA6", "BranchCreek PRT", "", null, 0, "83258E8C-ACA4-4CC8-B1FA-B7C36083535B", false, false, true ); // BranchCreek PRT
            RockMigrationHelper.UpdateGroup( "4890D767-15C6-465B-9D77-4A6078C59444", "15708E27-CC26-435F-B8B8-5E6CC48DDDA6", "Coal Township PRT", "", null, 0, "C1281FB4-AF5B-4D73-A7F4-BCE0F182B8CC", false, false, true ); // Coal Township PRT
            RockMigrationHelper.UpdateGroup( "4890D767-15C6-465B-9D77-4A6078C59444", "15708E27-CC26-435F-B8B8-5E6CC48DDDA6", "Columbia-Montour PRT", "", null, 0, "F01A37C7-08A5-49B5-B392-1D5943D4BE83", false, false, true ); // Columbia-Montour PRT
            RockMigrationHelper.UpdateGroup( "4890D767-15C6-465B-9D77-4A6078C59444", "15708E27-CC26-435F-B8B8-5E6CC48DDDA6", "Ephrata PRT", "", null, 0, "4B68A2D5-90BA-4224-A5AA-4AB9CEBD16ED", false, false, true ); // Ephrata PRT
            RockMigrationHelper.UpdateGroup( "4890D767-15C6-465B-9D77-4A6078C59444", "15708E27-CC26-435F-B8B8-5E6CC48DDDA6", "Hanover PRT", "", null, 0, "DB6021D5-1F2F-4B1A-A7F4-49AD3A86BAEA", false, false, true ); // Hanover PRT
            RockMigrationHelper.UpdateGroup( "4890D767-15C6-465B-9D77-4A6078C59444", "15708E27-CC26-435F-B8B8-5E6CC48DDDA6", "Harrisburg PRT", "", null, 0, "4CBA8124-6EF5-4A9C-B74A-88C1D94AF334", false, false, true ); // Harrisburg PRT
            RockMigrationHelper.UpdateGroup( "4890D767-15C6-465B-9D77-4A6078C59444", "15708E27-CC26-435F-B8B8-5E6CC48DDDA6", "Hazleton PRT", "", null, 0, "7DA4D575-0738-4B1B-A3BF-014D97A8A397", false, false, true ); // Hazleton PRT
            RockMigrationHelper.UpdateGroup( "4890D767-15C6-465B-9D77-4A6078C59444", "15708E27-CC26-435F-B8B8-5E6CC48DDDA6", "Lancaster City PRT", "", null, 0, "69B22D74-B087-452E-9C0B-117A821458B7", false, false, true ); // Lancaster City PRT
            RockMigrationHelper.UpdateGroup( "4890D767-15C6-465B-9D77-4A6078C59444", "15708E27-CC26-435F-B8B8-5E6CC48DDDA6", "Lebanon PRT", "", null, 0, "4E33BB52-AA8E-46BA-92D9-F4283557D772", false, false, true ); // Lebanon PRT
            RockMigrationHelper.UpdateGroup( "4890D767-15C6-465B-9D77-4A6078C59444", "15708E27-CC26-435F-B8B8-5E6CC48DDDA6", "Manheim PRT", "", null, 0, "1570BEAB-F23B-49FA-A4B8-3CD3AD26C124", false, false, true ); // Manheim PRT
            RockMigrationHelper.UpdateGroup( "4890D767-15C6-465B-9D77-4A6078C59444", "15708E27-CC26-435F-B8B8-5E6CC48DDDA6", "Online PRT", "", null, 0, "EB9A9EEE-821E-4971-AC67-404B24C5C9D7", false, false, true ); // Online PRT
            RockMigrationHelper.UpdateGroup( "4890D767-15C6-465B-9D77-4A6078C59444", "15708E27-CC26-435F-B8B8-5E6CC48DDDA6", "Waynesboro PRT", "", null, 0, "74AA6A28-5CF4-4A38-AC5A-D6C51BC5C5D3", false, false, true ); // Waynesboro PRT
            RockMigrationHelper.UpdateGroup( "4890D767-15C6-465B-9D77-4A6078C59444", "15708E27-CC26-435F-B8B8-5E6CC48DDDA6", "West Shore PRT", "", null, 0, "E85C5DEE-3B98-4F1D-8D2F-CB7DE719D040", false, false, true ); // West Shore PRT
            RockMigrationHelper.UpdateGroup( "4890D767-15C6-465B-9D77-4A6078C59444", "15708E27-CC26-435F-B8B8-5E6CC48DDDA6", "York PRT", "", null, 0, "D78EB9A3-669A-46E0-B63E-3512A93E888A", false, false, true ); // York PRT

            RockMigrationHelper.UpdateGroup( "D791D411-2434-4D2B-A9AC-E621BFAEC6E5", "15708E27-CC26-435F-B8B8-5E6CC48DDDA6", "Workflows", "", null, 0, "351E56B1-5ED6-4C38-9D0E-E7EFAFD4DCA9", false, false, true ); // Workflows
            RockMigrationHelper.UpdateGroup( "351E56B1-5ED6-4C38-9D0E-E7EFAFD4DCA9", "943708E0-170B-4935-AA3A-3DF958D8D2C8", "About You Form Entry Routing", "Group for routing the workflows found on the About You Entry. Mostly Campus Admins.", null, 0, "5579B44A-AA02-4E9B-BAB7-29E702E773BF", false, false, true ); // About You Form Entry Routing
            RockMigrationHelper.UpdateGroup( "351E56B1-5ED6-4C38-9D0E-E7EFAFD4DCA9", "943708E0-170B-4935-AA3A-3DF958D8D2C8", "General Contact Routing", "Group to hold people the General Contact Workflow can be assigned to. Mostly Campus Admins.", null, 0, "CDA9CAE4-0674-4CD2-A275-8104F953D5CB", false, false, true ); // General Contact Routing

            // Set the attributes as IsGridColumn = true
            Sql( @" Update Attribute
                    Set IsGridColumn = 1
                    Where Guid in ('66AE33E8-B374-4F07-BBE3-4096F1CBCE88')
            " );

        }

        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "66AE33E8-B374-4F07-BBE3-4096F1CBCE88" );    // GroupType - Group Member Attribute, Workflow Routing: Campus
        }

        public void AddGroupTypeAssociation( string parentGroupTypeGuid, string childGroupTypeGuid )
        {
            Sql( string.Format( @"

                -- Insert a group type association...

                DECLARE @ParentGroupTypeId int = ( SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = '{0}' )
                DECLARE @ChildGroupTypeId int = ( SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = '{1}' )

                IF NOT EXISTS (
                    SELECT [GroupTypeId]
                    FROM [GroupTypeAssociation]
                    WHERE [GroupTypeId] = @ParentGroupTypeId
                    AND [ChildGroupTypeId] = @ChildGroupTypeId)
                BEGIN
                    INSERT INTO [GroupTypeAssociation] (
                        [GroupTypeId]
                        ,[ChildGroupTypeId])
                    VALUES(
                        @ParentGroupTypeId
                        ,@ChildGroupTypeId)
                END
            ",
                parentGroupTypeGuid,
                childGroupTypeGuid
           ) );
        }

    }
}
