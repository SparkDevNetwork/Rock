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
    public partial class AddReminderPropertiesToGroupEntities : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Group", "ReminderSystemCommunicationId", c => c.Int());
            AddColumn("dbo.Group", "ReminderOffsetDays", c => c.Int());
            AddColumn("dbo.Group", "ReminderAdditionalDetails", c => c.String());
            AddColumn("dbo.GroupMemberAssignment", "LastReminderSentDateTime", c => c.DateTime());
            AddColumn("dbo.GroupLocationScheduleConfig", "ReminderAdditionalDetails", c => c.String());
            DropColumn("dbo.Group", "RSVPReminderAdditionalDetails");
            DropColumn("dbo.GroupMemberAssignment", "LastRSVPReminderSentDateTime");
            DropColumn("dbo.GroupLocationScheduleConfig", "RSVPReminderAdditionalDetails");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.GroupLocationScheduleConfig", "RSVPReminderAdditionalDetails", c => c.String());
            AddColumn("dbo.GroupMemberAssignment", "LastRSVPReminderSentDateTime", c => c.DateTime());
            AddColumn("dbo.Group", "RSVPReminderAdditionalDetails", c => c.String());
            DropColumn("dbo.GroupLocationScheduleConfig", "ReminderAdditionalDetails");
            DropColumn("dbo.GroupMemberAssignment", "LastReminderSentDateTime");
            DropColumn("dbo.Group", "ReminderAdditionalDetails");
            DropColumn("dbo.Group", "ReminderOffsetDays");
            DropColumn("dbo.Group", "ReminderSystemCommunicationId");
        }
    }
}
