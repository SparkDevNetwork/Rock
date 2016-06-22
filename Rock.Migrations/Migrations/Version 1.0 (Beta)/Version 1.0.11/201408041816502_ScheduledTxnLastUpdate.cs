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
    public partial class ScheduledTxnLastUpdate : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AlterColumn("dbo.FinancialScheduledTransaction", "LastStatusUpdateDateTime", c => c.DateTime());

            // Add ContextAware attribute for HTML Content Block
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", "The type of entity that will provide context for this block", 0, "", "6783D47D-92F9-4F48-93C0-16111D675A0F" );

            // Remove prehtml headers from blocks on person detail pages now that grid blocks have a nice header.
            Sql( @"
    UPDATE [Block] SET [PreHtml] = ''
    WHERE [Guid] IN (
        'B33DF8C4-29B2-4DC5-B182-61FC255B01C0',
        '9382B285-3EF6-47F7-94BB-A47C498196A3',
        '1CBE10C7-5E64-4385-BEE3-81DCA43DC47F',
        '68D34EC2-0A10-4344-89E3-E6DF99951FDB',
        'F98649D7-E522-46CB-8F67-01DB7F59E3AA'
    )
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AlterColumn("dbo.FinancialScheduledTransaction", "LastStatusUpdateDateTime", c => c.DateTime(storeType: "date"));
        }
    }
}
