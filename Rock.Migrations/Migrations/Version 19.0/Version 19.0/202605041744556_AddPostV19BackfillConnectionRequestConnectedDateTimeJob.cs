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
    public partial class AddPostV19BackfillConnectionRequestConnectedDateTimeJob : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Schedule a one-shot post-update job (runs nightly at 2 AM until it self-deletes)
            // that backfills ConnectionRequest.ConnectedDateTime and WasCompletedOnTime from the
            // History table for requests that were already in the Connected state prior to v19.
            RockMigrationHelper.AddPostUpdateServiceJob(
                name: "Rock Update Helper v19.0 - Backfill Connection Request Connected DateTime",
                description: "This job backfills the ConnectedDateTime and WasCompletedOnTime columns on Connection Requests completed before v19.",
                jobType: "Rock.Jobs.PostV19BackfillConnectionRequestConnectedDateTime",
                cronExpression: "0 0 2 1/1 * ? *",
                guid: Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_190_BACKFILL_CONNECTION_REQUEST_CONNECTED_DATETIME );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
