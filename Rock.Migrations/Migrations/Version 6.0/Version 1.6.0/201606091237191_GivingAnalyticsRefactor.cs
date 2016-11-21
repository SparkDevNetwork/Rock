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
    public partial class GivingAnalyticsRefactor : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
    IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spFinance_GivingAnalyticsQuery]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[spFinance_GivingAnalyticsQuery]
" );

            Sql( MigrationSQL._201606091237191_GivingAnalyticsRefactor_AccountTotals );
            Sql( MigrationSQL._201606091237191_GivingAnalyticsRefactor_FirstLastEverDates );
            Sql( MigrationSQL._201606091237191_GivingAnalyticsRefactor_PersonSummary );
            Sql( MigrationSQL._201606091237191_GivingAnalyticsRefactor_TransactionData );

            // NA: Change "Default" BinaryFileType name to "Unsecured"
            Sql( @"
    UPDATE [BinaryFileType] SET [Name] = 'Unsecured' 
    WHERE [Guid] = 'c1142570-8cd6-4a20-83b1-acb47c1cd377'
	AND [Name] = 'Default'
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
