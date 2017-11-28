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
    public partial class RemoveTransactionKeys : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
    IF EXISTS ( SELECT * FROM sys.indexes WHERE name='IX_GatewayTransactionKey' AND object_id = OBJECT_ID(N'[dbo].[FinancialTransaction]') )
    DROP INDEX [IX_GatewayTransactionKey] ON [dbo].[FinancialTransaction]

    IF EXISTS ( SELECT * FROM sys.indexes WHERE name='IX_GatewayTransactionKey' AND object_id = OBJECT_ID(N'[dbo].[FinancialScheduledTransaction]') )
    DROP INDEX [IX_GatewayTransactionKey] ON [dbo].[FinancialScheduledTransaction]

    IF EXISTS ( SELECT * FROM sys.indexes WHERE name='IX_GatewayTransactionKey' AND object_id = OBJECT_ID(N'[dbo].[FinancialPersonSavedAccount]') )
    DROP INDEX [IX_GatewayTransactionKey] ON [dbo].[FinancialPersonSavedAccount]

    IF EXISTS ( SELECT * FROM sys.columns WHERE name='GatewayTransactionKey' AND object_id = OBJECT_ID(N'[dbo].[FinancialTransaction]') )
    ALTER TABLE [dbo].[FinancialTransaction] DROP COLUMN [GatewayTransactionKey]

    IF EXISTS ( SELECT * FROM sys.columns WHERE name='GatewayTransactionKey' AND object_id = OBJECT_ID(N'[dbo].[FinancialScheduledTransaction]') )
    ALTER TABLE [dbo].[FinancialScheduledTransaction] DROP COLUMN [GatewayTransactionKey]

    IF EXISTS ( SELECT * FROM sys.columns WHERE name='GatewayTransactionKey' AND object_id = OBJECT_ID(N'[dbo].[FinancialPersonSavedAccount]') )
    ALTER TABLE [dbo].[FinancialPersonSavedAccount] DROP COLUMN [GatewayTransactionKey]
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
