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
    /// <summary>
    ///
    /// </summary>
    public partial class AddRSVPSystemCommunicationForeignKeyToGroupType : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CleanupInvalidRsvpReminderSystemCommunicationIds();
            CreateIndex("dbo.GroupType", "RSVPReminderSystemCommunicationId");
            AddForeignKey("dbo.GroupType", "RSVPReminderSystemCommunicationId", "dbo.SystemCommunication", "Id");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.GroupType", "RSVPReminderSystemCommunicationId", "dbo.SystemCommunication");
            DropIndex("dbo.GroupType", new[] { "RSVPReminderSystemCommunicationId" });
        }

        #region Helper Methods

        private void CleanupInvalidRsvpReminderSystemCommunicationIds()
        {
            Sql( @"
        UPDATE [GT]
        SET [GT].[RSVPReminderSystemCommunicationId] = NULL
        FROM [dbo].[GroupType] AS [GT]
        LEFT JOIN [dbo].[SystemCommunication] AS [SC]
            ON [SC].[Id] = [GT].[RSVPReminderSystemCommunicationId]
        WHERE [GT].[RSVPReminderSystemCommunicationId] IS NOT NULL
          AND [SC].[Id] IS NULL
    " );
        }

        #endregion Helper Methods
    }
}
