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
    public partial class AddApiThrottleSettingToChatSyncJob : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddEntityAttributeIfMissing(
                entityTypeName: "Rock.Model.ServiceJob",
                fieldTypeGuid: SystemGuid.FieldType.INTEGER,
                entityTypeQualifierColumn: "Class",
                entityTypeQualifierValue: "Rock.Jobs.ChatSync",
                name: "API Throttle",
                description: "The number of milliseconds to wait between tightly-consecutive API calls to the external Chat system, helping to proactively avoid rate limiting.",
                order: 6,
                defaultValue: 100.ToString(),
                guid: "93344D10-3FEC-4C04-916C-BF730C7E8118",
                key: "ApiThrottle",
                isRequired: false
            );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "93344D10-3FEC-4C04-916C-BF730C7E8118" );
        }
    }
}
