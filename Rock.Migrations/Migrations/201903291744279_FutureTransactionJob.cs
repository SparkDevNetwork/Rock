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
    public partial class FutureTransactionJob : RockMigration
    {
        private string FutureTransactionJobGuid = "123ADD3C-8A58-4A4D-9370-5E9C6CD3760B";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( string.Format( @"
                INSERT INTO ServiceJob (
                    IsSystem,
                    IsActive,
                    Name,
                    Description,
                    Class,
                    CronExpression,
                    Guid,
                    CreatedDateTime,
                    NotificationStatus
                ) VALUES (
                    0, --IsSystem
                    1, --IsActive
                    'Charge Future Transactions', --Name
                    'Charge future transactions where the FutureProcessingDateTime is now or has passed.', --Description
                    'Rock.Jobs.ChargeFutureTransactions', --Class
                    '0 0/10 * 1/1 * ? *', --Cron
                    '{0}', -- Guid
                    GETDATE(), -- Created
                    1 -- All notifications
                );", FutureTransactionJobGuid ) );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( string.Format( "DELETE FROM ServiceJob WHERE Guid = '{0}';", FutureTransactionJobGuid ) );
        }
    }
}
