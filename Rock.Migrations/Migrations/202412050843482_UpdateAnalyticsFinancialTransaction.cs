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
    public partial class UpdateAnalyticsFinancialTransaction : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
                IF NOT EXISTS (SELECT * 
                FROM sys.indexes 
                WHERE name = 'IX_TransactionTypeValueId_TransactionDateTime_AuthorizedPersonAliasId_AccountId' 
                    AND object_id = OBJECT_ID('dbo.AnalyticsSourceFinancialTransaction'))
                BEGIN
	                -- Create Index IX_TransactionTypeValueId_TransactionDateTime_AuthorizedPersonAliasId_AccountId
                    CREATE NONCLUSTERED INDEX [IX_TransactionTypeValueId_TransactionDateTime_AuthorizedPersonAliasId_AccountId] ON [dbo].[AnalyticsSourceFinancialTransaction]
	                (
		                [TransactionTypeValueId],
		                [TransactionDateTime],
                        [AuthorizedPersonAliasId],
                        [AccountId]
	                ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON);
                END" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {

        }
    }
}
