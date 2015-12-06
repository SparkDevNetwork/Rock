// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class FinancialTransactionMICRUpdates : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.FinancialTransaction", "MICRStatus", c => c.Int());
            AddColumn("dbo.FinancialTransaction", "CheckMicrParts", c => c.String());

            // Move the data from CheckMicrEncrypted to the new CheckMicrParts
            Sql( "UPDATE FinancialTransaction set CheckMicrParts = CheckMicrEncrypted where CheckMicrParts is null and  CheckMicrEncrypted is not null" );
            Sql( "UPDATE FinancialTransaction set CheckMicrEncrypted = null where CheckMicrEncrypted is not null" );

            // Set the MICRStatus of all previous scanned transactions to MICRStatus.Success (since MICRStatus.Fail is new 4.0 feature)
            Sql( "UPDATE FinancialTransaction set MICRStatus = 0 where CheckMicrParts is not null" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // revert the data for CheckMicrEncrypted back to the "Parts" format
            Sql( "UPDATE FinancialTransaction set CheckMicrEncrypted = CheckMicrParts where CheckMicrParts is not null" );
            DropColumn("dbo.FinancialTransaction", "CheckMicrParts");
            DropColumn("dbo.FinancialTransaction", "MICRStatus");
        }
    }
}
