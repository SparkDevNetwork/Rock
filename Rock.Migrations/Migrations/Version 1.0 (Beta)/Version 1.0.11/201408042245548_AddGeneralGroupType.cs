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
    public partial class AddGeneralGroupType : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateGroupType( "General Group", "Used to track groups that do not warrant their own group type.", "Group", "Member", null, false, true, true, "fa fa-users", 1, null, 19, null, "8400497B-C52F-40AE-A529-3FCCB9587101", false );
            RockMigrationHelper.UpdateGroupTypeRole( "8400497B-C52F-40AE-A529-3FCCB9587101", "Member", "", 1, null, null, "A0BBF29D-AD9D-4D06-9E81-9DA080D53C10", false );
            RockMigrationHelper.UpdateGroup( null, "8400497B-C52F-40AE-A529-3FCCB9587101", "General Groups", "Parent group for all general groups", "57DC00A3-FF88-4D4C-9878-30AE309117E2", 1, "57DC00A3-FF88-4D4C-9878-30AE309117E2", false );

            // now set the inherited group type
            Sql( @"
                DECLARE @DefaultGroupRoleId int = ( SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid] = 'A0BBF29D-AD9D-4D06-9E81-9DA080D53C10' )

                UPDATE [GroupType]
	            SET [DefaultGroupRoleId] = @DefaultGroupRoleId
	            WHERE [Guid] = '8400497B-C52F-40AE-A529-3FCCB9587101'" );

            // set allowed child group types
            Sql( @"
                DECLARE @GroupTypeId int = ( SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = '8400497B-C52F-40AE-A529-3FCCB9587101' )
  
                INSERT INTO [GroupTypeAssociation]
	                ([GroupTypeId], [ChildGroupTypeId])
	                VALUES
	                (@GroupTypeId, @GroupTypeId)
            " );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteGroupTypeRole( "A0BBF29D-AD9D-4D06-9E81-9DA080D53C10" );
            RockMigrationHelper.DeleteGroupType( "8400497B-C52F-40AE-A529-3FCCB9587101" );
            RockMigrationHelper.DeleteGroup( "57DC00A3-FF88-4D4C-9878-30AE309117E2" );
        }
    }
}
