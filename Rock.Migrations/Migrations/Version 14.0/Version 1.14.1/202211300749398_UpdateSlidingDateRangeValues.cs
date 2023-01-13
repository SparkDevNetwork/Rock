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
    public partial class UpdateSlidingDateRangeValues : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up( )
        {
            Sql( $@"
            IF NOT EXISTS (
                SELECT 1
                FROM [ServiceJob]
                WHERE [Class] = 'Rock.Jobs.PostV141DataMigrationsUpdateSlidingDateRangeFormat'
                    AND [Guid] = '{SystemGuid.ServiceJob.DATA_MIGRATIONS_141_UPDATE_SLIDING_DATE_RANGE_VALUE}'
            )
            BEGIN
                INSERT INTO [ServiceJob] (
                      [IsSystem]
                    , [IsActive]
                    , [Name]
                    , [Description]
                    , [Class]
                    , [CronExpression]
                    , [NotificationStatus]
                    , [Guid]
                ) VALUES (
                      1
                    , 1
                    , 'Rock Update Helper v14.1 - Update SlidingDateRange Attribute Values.'
                    , 'Updates attribute values of SlidingDateRangeFieldType to RoundTrip format.'
                    , 'Rock.Jobs.PostV141DataMigrationsUpdateSlidingDateRangeFormat'
                    , '0 0 21 1/1 * ? *'
                    , 1
                    , '{SystemGuid.ServiceJob.DATA_MIGRATIONS_141_UPDATE_SLIDING_DATE_RANGE_VALUE}'
                );
            END" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down( )
        {
            Sql( $"DELETE FROM [ServiceJob] WHERE [Guid] = '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_141_UPDATE_SLIDING_DATE_RANGE_VALUE}'" );
        }
    }
}
