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
    public partial class TransactionImageOrder : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey( "dbo.FinancialTransactionImage", "TransactionImageTypeValueId", "dbo.DefinedValue" );
            DropIndex( "dbo.FinancialTransactionImage", new[] { "TransactionImageTypeValueId" } );
            AddColumn( "dbo.FinancialTransactionImage", "Order", c => c.Int( nullable: false ) );

            /* Change "FrontOfCheck" to be Order=0, "BackOfCheck" to be Order=1
             FrontOfCheck = A52EDD34-D3A2-420F-AF45-21B323FB21D6 
             BackOfCheck = 87D9347D-64E6-4DD4-8F05-2AA17419B5E8
             */

            Sql( @"UPDATE [FinancialTransactionImage] SET [Order] = 0 WHERE [TransactionImageTypeValueId] = (SELECT TOP 1 [Id] FROM [DefinedValue] where [Guid] = 'A52EDD34-D3A2-420F-AF45-21B323FB21D6')" );
            Sql( @"UPDATE [FinancialTransactionImage] SET [Order] = 1 WHERE [TransactionImageTypeValueId] = (SELECT TOP 1 [Id] FROM [DefinedValue] where [Guid] = '87D9347D-64E6-4DD4-8F05-2AA17419B5E8')" );

            DropColumn( "dbo.FinancialTransactionImage", "TransactionImageTypeValueId" );

            // get rid of the FINANCIAL_TRANSACTION_IMAGE_TYPE defined type and associated defined values
            RockMigrationHelper.DeleteDefinedType( "0745D5DE-2D09-44B3-9017-40C1DA83CB39" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.FinancialTransactionImage", "TransactionImageTypeValueId", c => c.Int());
            DropColumn("dbo.FinancialTransactionImage", "Order");
            CreateIndex("dbo.FinancialTransactionImage", "TransactionImageTypeValueId");
            AddForeignKey("dbo.FinancialTransactionImage", "TransactionImageTypeValueId", "dbo.DefinedValue", "Id");
        }
    }
}
