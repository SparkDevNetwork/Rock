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
    public partial class Rollup_0517 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Person", "DeceasedDate", c => c.DateTime()); 

            UpdateImpliedRelationshipGroupTypeAndGroups();

            Sql( MigrationSQL._201805171836577_Rollup_0517_CreatePresenceUser );

            Sql( MigrationSQL._201805171836577_Rollup_0517_spAnalytics_ETL_Attendance );

            Sql( MigrationSQL._201805171836577_Rollup_0517_vCheckin_Attendance );

            Sql( @"-- Move Person Pages to People Page and hide from navigation
                    DECLARE @ParentPageId int = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '97ECDC48-6DF6-492E-8C72-161F76AE111B') -- Internal Homepage > People
                    DECLARE @ChildPageId int = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = 'BF04BB7E-BE3A-4A38-A37C-386B55496303') -- Person Pages

                    UPDATE [Page]
                        SET [ParentPageId] = @ParentPageId,
                        [DisplayInNavWhen] = 2
                    WHERE
                        [Id] = @ChildPageId"
                );
            Sql( @"-- Move Person Search to Person Pages
                    DECLARE @ParentPageId int = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = 'BF04BB7E-BE3A-4A38-A37C-386B55496303') -- Person Pages
                    DECLARE @ChildPageId int = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '5E036ADE-C2A4-4988-B393-DAC58230F02E') -- Person Search

                    UPDATE [Page]
                        SET [ParentPageId] = @ParentPageId
                    WHERE
                        [Id] = @ChildPageId" 
                );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.Person", "DeceasedDate");
        }

        public void UpdateImpliedRelationshipGroupTypeAndGroups()
        {
            string sql = @"DECLARE @PeerNetworkGroupTypeId int = (SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = '8c0e5852-f08f-4327-9aa5-87800a6ab53e')

-- Update the name of the group type
UPDATE [GroupType]
    SET [Name] = 'Peer Network',
        [IconCssClass] = 'fa fa-user-friends'
    WHERE [Id] = @PeerNetworkGroupTypeId

-- Update the name of the current groups of this type
UPDATE [Group]
    SET [Name] = 'Peer Network'
    WHERE [GroupTypeId] = @PeerNetworkGroupTypeId";

            Sql( sql );
        }
    }
}
