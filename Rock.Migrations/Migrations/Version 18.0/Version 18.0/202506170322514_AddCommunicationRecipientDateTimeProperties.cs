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
    public partial class AddCommunicationRecipientDateTimeProperties : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Drop the IX_CommunicationId index that's referencing the CausedUnsubscribe field. We'll re-add the index
            // without this field in a post update job.
            DropIndex( "dbo.CommunicationRecipient", "IX_CommunicationId" );
            DropColumn( "dbo.CommunicationRecipient", "CausedUnsubscribe" );

            AddColumn( "dbo.CommunicationRecipient", "DeliveredDateTime", c => c.DateTime() );
            AddColumn( "dbo.CommunicationRecipient", "SpamComplaintDateTime", c => c.DateTime() );

            RockMigrationHelper.AddPostUpdateServiceJob(
                name: "Rock Update Helper v18.0 - Update CommunicationRecipient Index",
                description: "This job will update an existing index on the CommunicationRecipient table.",
                jobType: "Rock.Jobs.PostV18UpdateCommunicationRecipientIndex",
                cronExpression: "0 0 21 1/1 * ? *",
                guid: Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_180_UPDATE_COMMUNICATIONRECIPIENT_INDEX );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn( "dbo.CommunicationRecipient", "SpamComplaintDateTime" );
            DropColumn( "dbo.CommunicationRecipient", "DeliveredDateTime" );

            AddColumn( "dbo.CommunicationRecipient", "CausedUnsubscribe", c => c.Boolean( nullable: false ) );
        }
    }
}
