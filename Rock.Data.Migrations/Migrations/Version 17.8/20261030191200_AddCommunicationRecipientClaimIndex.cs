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
namespace Rock.Data.Migrations;

/// <summary>
/// Adds a post update job to add an index that keeps claiming the next
/// communication recipient fast during large sends. Fix for issue #7092.
/// </summary>
/// <seealso cref="Rock.Data.Migration" />
[SqlMigration( 20261030191200, "17.8", ReplacesHotFixNumber = 340 )]
internal class AddCommunicationRecipientClaimIndex : Migration
{
    /// <summary>
    /// Operations to be performed during the upgrade process.
    /// </summary>
    public override void Up()
    {
        AddCommunicationRecipientClaimIndexJobUp();
    }

    /// <summary>
    /// Adds a post update job to add the communication recipient claim index.
    /// This is done in a job rather than in the migration because building an
    /// index on a large CommunicationRecipient table can take several minutes.
    /// It runs at 2 AM (like the v19.0 index job on this table) because the
    /// build blocks writes to the table, and large sends often run in the evening.
    /// </summary>
    private void AddCommunicationRecipientClaimIndexJobUp()
    {
        RockMigrationHelper.AddPostUpdateServiceJob(
            name: "Rock Update Helper v20.1 - Add Communication Recipient Claim Index",
            description: "This job will add an index to keep large communications sending at a steady speed.",
            jobType: "Rock.Jobs.PostV201AddCommunicationRecipientClaimIndex",
            cronExpression: "0 0 2 1/1 * ? *",
            guid: Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_201_ADD_COMMUNICATION_RECIPIENT_CLAIM_INDEX );
    }
}
