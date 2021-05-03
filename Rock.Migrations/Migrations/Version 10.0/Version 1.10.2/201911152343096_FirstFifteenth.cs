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
    public partial class FirstFifteenth : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_FREQUENCY, "Twice a Month", "Twice a Month", Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_TWICEMONTHLY );
            RockMigrationHelper.UpdateDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_FREQUENCY, "1st and 15th", "1st and 15th", Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_FIRST_AND_FIFTEENTH );

            // Put the FINANCIAL_FREQUENCY in the desired order
            Sql( @"DECLARE @transactionFrequencyDefinedTypeId INT = (
		SELECT TOP 1 Id
		FROM DefinedType
		WHERE [Guid] = '1F645CFB-5BBD-4465-B9CA-0D2104A1479B'
		)

SELECT *
FROM dbo.DefinedValue
WHERE DefinedTypeId = @transactionFrequencyDefinedTypeId
ORDER BY [Order]

UPDATE DefinedValue
SET [Order] = [Order] + 1
WHERE DefinedTypeId = @transactionFrequencyDefinedTypeId AND [Order] > 3 AND [Guid] != 'C752403C-0F88-45CD-B574-069355C01D77'

UPDATE DefinedValue
SET [Order] = 4
WHERE [Guid] = 'C752403C-0F88-45CD-B574-069355C01D77'" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
