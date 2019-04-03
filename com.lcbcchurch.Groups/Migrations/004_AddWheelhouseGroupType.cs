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
    [MigrationNumber( 4, "1.0.14" )]
    public class AddWheelhouseGroupType : Migration
    {
        public override void Up()
        {
            // Add Group Types
            RockMigrationHelper.AddGroupType( "Wheelhouse", "", "Group", "Member", false, true, true, "", 0, "", 0, "", "947373D1-0EF6-416F-96CE-872DBDA9B518" );
            // Add Group Type Associations
            AddGroupTypeAssociation( "F12057FA-F0CE-41A7-A199-7C7754E01E10", "947373D1-0EF6-416F-96CE-872DBDA9B518" );

            // Add Group Type Roles
            RockMigrationHelper.UpdateGroupTypeRole( "947373D1-0EF6-416F-96CE-872DBDA9B518", "Member", "", 0, null, null, "1DD59761-1560-41DF-B99E-DD0EB5285DCE", false, false, true );
            
        }
        public override void Down()
        {
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
