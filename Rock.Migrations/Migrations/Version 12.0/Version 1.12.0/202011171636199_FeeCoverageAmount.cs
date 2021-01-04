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
    public partial class FeeCoverageAmount : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.FinancialScheduledTransactionDetail", "FeeCoverageAmount", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.FinancialTransactionDetail", "FeeCoverageAmount", c => c.Decimal(precision: 18, scale: 2));

            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "841D1670-8BFD-4913-8409-FB47EB7A2AB9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Gender", "RequireGender", "Require Gender", @"Controls whether or not the gender field is required.", 17, @"True", "F49B0742-D0D3-4384-8F37-F9EDE7B15A31" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.FinancialTransactionDetail", "FeeCoverageAmount");
            DropColumn("dbo.FinancialScheduledTransactionDetail", "FeeCoverageAmount");
        }
    }
}
