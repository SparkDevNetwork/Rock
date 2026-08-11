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
    public partial class AddGroupFinderBlock : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Group", "MeetingStyle", c => c.Int());
            AddColumn("dbo.GroupType", "IsMeetingStyleEnabled", c => c.Boolean(nullable: false));

            // Enable Meeting Style on the built-in Small Group type so the Group Finder's
            // meeting-style filter and the per-group field are usable out of the box.
            Sql( @"
                UPDATE [GroupType]
                SET [IsMeetingStyleEnabled] = 1
                WHERE [Guid] = '50FCFB30-F51A-49DF-86F4-2B176EA1820B'" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.GroupType", "IsMeetingStyleEnabled");
            DropColumn("dbo.Group", "MeetingStyle");
        }
    }
}
