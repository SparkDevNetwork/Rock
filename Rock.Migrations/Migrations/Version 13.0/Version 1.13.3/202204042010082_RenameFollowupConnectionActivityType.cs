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
    /// Add or Update 'Follow-up Date Reached' Connection Activity Type
    /// Previously known as "Future Follow-up Complete"
    /// </summary>
    public partial class RenameFollowupConnectionActivityType : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // This is the name that the Connection Activity Type will have.
            string followupDateReached = "Follow-up Date Reached";

            Sql( $@"
            IF NOT EXISTS (
               SELECT [Id]
               FROM [ConnectionActivityType]
               WHERE [Guid] = '{SystemGuid.ConnectionActivityType.FOLLOWUP_DATE_REACHED}' )
            BEGIN
            INSERT INTO
            [ConnectionActivityType]
            ( [Name], [IsActive], [Guid])
            VALUES
            ( N'{followupDateReached}', 1, N'{SystemGuid.ConnectionActivityType.FOLLOWUP_DATE_REACHED}')
            END
            ELSE
            BEGIN
            IF NOT EXISTS (
                SELECT [Id]
                FROM [ConnectionActivityType]
                WHERE [Guid] = '{SystemGuid.ConnectionActivityType.FOLLOWUP_DATE_REACHED}' AND [Name] = '{followupDateReached}' )
                BEGIN
                    UPDATE [ConnectionActivityType]
                    SET
                        [Name] = N'{followupDateReached}'
                    WHERE
                        [Guid] = N'{SystemGuid.ConnectionActivityType.FOLLOWUP_DATE_REACHED}'
                END
            END
            " );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
