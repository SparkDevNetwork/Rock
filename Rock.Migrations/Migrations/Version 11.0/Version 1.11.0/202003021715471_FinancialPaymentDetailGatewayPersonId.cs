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
    public partial class FinancialPaymentDetailGatewayPersonId : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.FinancialPaymentDetail", "GatewayPersonIdentifier", c => c.String(maxLength: 50));
            AddColumn("dbo.FinancialPaymentDetail", "FinancialPersonSavedAccountId", c => c.Int());
            CreateIndex("dbo.FinancialPaymentDetail", "FinancialPersonSavedAccountId");

            // Instead of the regular scaffolded AddForeignKey( "dbo.FinancialPaymentDetail", "FinancialPersonSavedAccountId", "dbo.FinancialPersonSavedAccount", "Id", cascadeDelete: true);
            // Create the FK manually so that we can add the 'ON DELETE SET NULL'
            Sql( @"
ALTER TABLE [dbo].[FinancialPaymentDetail]  WITH CHECK ADD CONSTRAINT [FK_dbo.FinancialPaymentDetail_dbo.FinancialPersonSavedAccount] FOREIGN KEY ([FinancialPersonSavedAccountId])
REFERENCES [dbo].[FinancialPersonSavedAccount] ([Id])
ON DELETE SET NULL
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.FinancialPaymentDetail", "FinancialPersonSavedAccountId", "dbo.FinancialPersonSavedAccount");
            DropIndex("dbo.FinancialPaymentDetail", new[] { "FinancialPersonSavedAccountId" });
            DropColumn("dbo.FinancialPaymentDetail", "FinancialPersonSavedAccountId");
            DropColumn("dbo.FinancialPaymentDetail", "GatewayPersonIdentifier");
        }
    }
}
