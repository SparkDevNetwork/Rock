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
    public partial class AddFinancialTransactionAlertTypeRunDaysOfWeek : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.FinancialTransactionAlertType", "RunDaysOfWeek", c => c.Int());

            // Copy existing RunDays values to the new RunDaysOfWeek column
            // Note: The existing RunDays column uses a different bit flag format
            // than the new RunDaysOfWeek column.
            Sql( @"
UPDATE [FinancialTransactionAlertType]
SET [RunDaysOfWeek] = CASE WHEN [RunDays] IS NULL
    THEN NULL
    ELSE
        0
        + CASE WHEN ([RunDays] & 0x0000001) = 0x0000001 THEN 0x01 ELSE 0 END
        + CASE WHEN ([RunDays] & 0x0000010) = 0x0000010 THEN 0x02 ELSE 0 END
        + CASE WHEN ([RunDays] & 0x0000100) = 0x0000100 THEN 0x04 ELSE 0 END
        + CASE WHEN ([RunDays] & 0x0001000) = 0x0001000 THEN 0x08 ELSE 0 END
        + CASE WHEN ([RunDays] & 0x0010000) = 0x0010000 THEN 0x10 ELSE 0 END
        + CASE WHEN ([RunDays] & 0x0100000) = 0x0100000 THEN 0x20 ELSE 0 END
        + CASE WHEN ([RunDays] & 0x1000000) = 0x1000000 THEN 0x40 ELSE 0 END
    END
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.FinancialTransactionAlertType", "RunDaysOfWeek");
        }
    }
}
