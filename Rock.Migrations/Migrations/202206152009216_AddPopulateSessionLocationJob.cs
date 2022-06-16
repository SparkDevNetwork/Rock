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
    public partial class AddPopulateSessionLocationJob : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( $@"
            IF NOT EXISTS (
                SELECT 1
                FROM [ServiceJob]
                WHERE [Class] = 'Rock.Jobs.PostV14DataMigrationsUpdateCurrentSessions'
                    AND [Guid] = '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_140_UPDATE_CURRENT_SESSIONS}'
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
                    , 'Rock Update Helper v14.0 - Update current sessions'
                    , 'This job will update the current sessions to have the duration of the session as well as the interaction count.'
                    , 'Rock.Jobs.PostV14DataMigrationsUpdateCurrentSessions'
                    , '0 0 21 1/1 * ? *'
                    , 1
                    , '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_140_UPDATE_CURRENT_SESSIONS}'
                );
            END" );

            // Attribute: Rock.Jobs.PopulateInteractionSessionData: IP Address Geocoding Component
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.ServiceJob", "A7486B0E-4CA2-4E00-A987-5544C7DABA76", "Class", "Rock.Jobs.PopulateInteractionSessionData", "IP Address Geocoding Component", "IP Address Geocoding Component", @"The service that will perform the IP GeoCoding lookup for any new IPs that have not been GeoCoded. Not required to be set here because the job will use the first active component if one is not configured here.", 0, @"", "B58B9B93-779D-46DE-8308-E8BCAE7DC352", "IPAddressGeoCodingComponent" );
            // Attribute: Rock.Jobs.PopulateInteractionSessionData: Command Timeout
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.ServiceJob", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Class", "Rock.Jobs.PopulateInteractionSessionData", "Command Timeout", "Command Timeout", @"Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of Interactions, this could take several hours or more.", 0, @"3600", "7A02A62F-3B6E-495D-983B-F888B971B7E8", "CommandTimeout" );
            // Attribute: Rock.Jobs.PopulateInteractionSessionData: Lookback Maximum (days)
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.ServiceJob", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Class", "Rock.Jobs.PopulateInteractionSessionData", "Lookback Maximum (days)", "Lookback Maximum (days)", @"The number of days into the past the job should look for unmatched addresses in the InteractionSession table. (default 30 days)", 1, @"30", "BF23E452-603F-41F9-A7BF-FB68E8296686", "LookbackMaximumInDays" );
            // Attribute: Rock.Jobs.PopulateInteractionSessionData: Max Records To Process Per Run
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.ServiceJob", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Class", "Rock.Jobs.PopulateInteractionSessionData", "Max Records To Process Per Run", "Max Records To Process Per Run", @"The number of unique IP addresses to process on each run of this job.", 2, @"50000", "C1096F04-0ECF-4519-9ABF-0CBB58BFFEA8", "HowManyRecords" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
