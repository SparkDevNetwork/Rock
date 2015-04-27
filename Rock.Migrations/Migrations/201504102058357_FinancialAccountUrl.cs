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
    using System.Linq;
    
    /// <summary>
    ///
    /// </summary>
    public partial class FinancialAccountUrl : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.FinancialAccount", "ImageBinaryFileId", c => c.Int());
            AddColumn("dbo.FinancialAccount", "Url", c => c.String());
            CreateIndex("dbo.FinancialAccount", "ImageBinaryFileId");
            AddForeignKey("dbo.FinancialAccount", "ImageBinaryFileId", "dbo.BinaryFile", "Id");

            // update TransactionCode to be the CheckNumber for transactions that are a result of scanning checks from the Rock Check Scanner app
            try
            {
                var rockContext = new Rock.Data.RockContext();
                var financialTransactionService = new Rock.Model.FinancialTransactionService( rockContext );
                Guid currencyTypeCheckGuid = Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CHECK.AsGuid();
                var currencyTypeCheckId = Rock.Web.Cache.DefinedValueCache.Read( currencyTypeCheckGuid ).Id;
                var tran = rockContext.Database.BeginTransaction();
                try
                {
                    foreach ( var financialTransaction in financialTransactionService.Queryable().Where( a =>
                        ( a.CheckMicrEncrypted != null && a.CheckMicrEncrypted != string.Empty )
                        && ( a.TransactionCode == null || a.TransactionCode == string.Empty )
                        && a.CurrencyTypeValueId == currencyTypeCheckId )
                        .Select( a => new { a.Id, a.CheckMicrEncrypted } ).ToList() )
                    {
                        string checkMicrDecrypted = Rock.Security.Encryption.DecryptString( financialTransaction.CheckMicrEncrypted ) ?? string.Empty;
                        var parts = checkMicrDecrypted.Split( '_' );
                        if ( parts.Length == 3 )
                        {
                            string transactionCode = parts[2];

                            string sql = string.Format(
                                "update [FinancialTransaction] set [TransactionCode] = '{0}' where [Id] = {1}"
                                , transactionCode
                                , financialTransaction.Id );
                            rockContext.Database.ExecuteSqlCommand( sql );
                        }
                    }
                }
                finally
                {
                    tran.Commit();
                }
            }
            catch
            {
                // ignore if transaction TransactionCode can't be updated
            }
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.FinancialAccount", "ImageBinaryFileId", "dbo.BinaryFile");
            DropIndex("dbo.FinancialAccount", new[] { "ImageBinaryFileId" });
            DropColumn("dbo.FinancialAccount", "Url");
            DropColumn("dbo.FinancialAccount", "ImageBinaryFileId");
        }
    }
}
