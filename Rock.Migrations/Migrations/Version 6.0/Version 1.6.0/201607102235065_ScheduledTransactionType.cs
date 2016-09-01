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
    public partial class ScheduledTransactionType : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.FinancialScheduledTransaction", "TransactionTypeValueId", c => c.Int());
            CreateIndex("dbo.FinancialScheduledTransaction", "TransactionTypeValueId");
            AddForeignKey("dbo.FinancialScheduledTransaction", "TransactionTypeValueId", "dbo.DefinedValue", "Id");

            // JE: Update Icons for Attribute Pages
            Sql( @"
    UPDATE[Page]
    SET[IconCssClass] = 'fa fa-users'
    WHERE[Guid] = '59AB771D-194A-494E-87F5-7E00404C354A'

    UPDATE[Page]
    SET[IconCssClass] = 'fa fa-user'
    WHERE[Guid] = '7BA1FAF4-B63C-4423-A818-CC794DDB14E3'
" );

            // MP: Fix spCrm_PersonDuplicateFinder #Temp table issue
            Sql( MigrationSQL._201607102235065_ScheduledTransactionType );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.FinancialScheduledTransaction", "TransactionTypeValueId", "dbo.DefinedValue");
            DropIndex("dbo.FinancialScheduledTransaction", new[] { "TransactionTypeValueId" });
            DropColumn("dbo.FinancialScheduledTransaction", "TransactionTypeValueId");
        }
    }
}
