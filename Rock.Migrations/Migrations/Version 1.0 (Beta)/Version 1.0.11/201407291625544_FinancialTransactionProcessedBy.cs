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
    public partial class FinancialTransactionProcessedBy : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.FinancialTransaction", "ProcessedByPersonAliasId", c => c.Int());
            AddColumn("dbo.FinancialTransaction", "ProcessedDateTime", c => c.DateTime());
            CreateIndex("dbo.FinancialTransaction", "ProcessedByPersonAliasId");
            AddForeignKey("dbo.FinancialTransaction", "ProcessedByPersonAliasId", "dbo.PersonAlias", "Id");

            // Update Batch Detail page not to diplay name in breadcrumbs (block now does it)
            Sql( @"
    UPDATE [Page] SET [BreadCrumbDisplayName] = 0 WHERE [Guid] = '606BDA31-A8FE-473A-B3F8-A00ECF7E06EC'

    UPDATE C SET C.[IdParameter] = 'batchId'
    FROM [PageContext] C
	    INNER JOIN [Page] P ON P.[Id] = C.[PageId]
    WHERE P.[Guid] = '606BDA31-A8FE-473A-B3F8-A00ECF7E06EC'
    AND C.[IdParameter] = 'financialBatchId'
" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.FinancialTransaction", "ProcessedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.FinancialTransaction", new[] { "ProcessedByPersonAliasId" });
            DropColumn("dbo.FinancialTransaction", "ProcessedDateTime");
            DropColumn("dbo.FinancialTransaction", "ProcessedByPersonAliasId");
        }
    }
}
