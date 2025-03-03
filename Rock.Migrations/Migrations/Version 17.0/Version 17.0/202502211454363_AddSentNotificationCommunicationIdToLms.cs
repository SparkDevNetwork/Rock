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
    public partial class AddSentNotificationCommunicationIdToLms : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey("dbo.LearningActivityCompletion", "NotificationCommunicationId", "dbo.SystemCommunication");
            DropIndex("dbo.LearningActivityCompletion", new[] { "NotificationCommunicationId" });
            AddColumn("dbo.LearningActivityCompletion", "SentNotificationCommunicationId", c => c.Int());
            CreateIndex("dbo.LearningActivityCompletion", "SentNotificationCommunicationId");
            AddForeignKey("dbo.LearningActivityCompletion", "SentNotificationCommunicationId", "dbo.Communication", "Id");
            DropColumn("dbo.LearningActivityCompletion", "NotificationCommunicationId");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.LearningActivityCompletion", "NotificationCommunicationId", c => c.Int());
            DropForeignKey("dbo.LearningActivityCompletion", "SentNotificationCommunicationId", "dbo.Communication");
            DropIndex("dbo.LearningActivityCompletion", new[] { "SentNotificationCommunicationId" });
            DropColumn("dbo.LearningActivityCompletion", "SentNotificationCommunicationId");
            CreateIndex("dbo.LearningActivityCompletion", "NotificationCommunicationId");
            AddForeignKey("dbo.LearningActivityCompletion", "NotificationCommunicationId", "dbo.SystemCommunication", "Id");
        }
    }
}
