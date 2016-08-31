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
    public partial class PageContextUniqueIndex : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Delete any duplicate PageContext values (just in case)
            Sql( @"
                ;WITH CTE
                AS
                (
	                SELECT 
		                [PageId],
		                [Entity],
                        [IdParameter],
		                MAX([Id]) AS [Id],
		                COUNT(*) AS [DupCount]
	                FROM [PageContext]
	                GROUP BY
                        [PageId],
		                [Entity],
                        [IdParameter]
                )

                DELETE PC
                FROM CTE
                INNER JOIN [PageContext] PC
	                ON PC.[PageId] = CTE.[PageId]
	                AND PC.[Entity] = CTE.[Entity]
                    AND PC.[IdParameter] = CTE.[IdParameter]
                WHERE PC.[Id] <> CTE.[Id]
                AND CTE.[DupCount] > 1" );
            
            CreateIndex("dbo.PageContext", new[] { "PageId", "Entity", "IdParameter" }, unique: true, name: "IX_PageIdEntityIdParameter");


            // GoogleKeyInstructions
            Sql( @"
        UPDATE [DefinedValue]
        SET [Description] = N'Rock uses Google Maps to help visualize where your ministry happens.  To enable this, each organization must get a Google Maps key. To complete this do the following:
        <ol>
        <li>Visit the <a target=""_blank"" href=""https://code.google.com/apis/console"">Google APIs Console</a> and log in with the Google Account you wish to tie the key to (perhaps not your personal account).</li>
        <li>Click the <em>APIs</em> link under APIs & auth from the left-hand menu.</li>
        <li>Activate the <em>Google Maps JavaScript API v3</em> service.</a></li>
        <li>Click the <em>Credentials</em> link from the left-hand menu. Your API key is available from the <em>Public API access</em> section. Press the ""Create new Key"" button if you don''t yet have a key. Maps API applications use Maps API applications use the Key for browser apps.</li>
        <li>Enter this key under <span class=""navigation-tip"">Administration > General Settings > Global Attributes > Google API Key</span>.</li>
        </ol>
        This key will allow your organization 25,000 transactions a day.  That''s quite a bit, but if you think you need more you can apply for a free <a href=""http://www.google.com/earth/outreach/grants/software/mapsapi.html"">Google Maps Grant</a>.'
        WHERE [Guid] = '92fa16fa-39e3-4364-9412-aa322c9ef01a'
" );

            //// PageContext for ScheduledTransactions
            // Add/Update PageContext for Page:Scheduled Transaction, Entity: Rock.Model.FinancialScheduledTransaction, Parameter: ScheduledTransactionId
            RockMigrationHelper.UpdatePageContext( "996F5541-D2E1-47E4-8078-80A388203CEC", "Rock.Model.FinancialScheduledTransaction", "ScheduledTransactionId", "1A89A743-8820-4F55-B2A9-B58A99E15F3E" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Delete PageContext for Page:Scheduled Transaction, Entity: Rock.Model.FinancialScheduledTransaction, Parameter: ScheduledTransactionId
            RockMigrationHelper.DeletePageContext( "1A89A743-8820-4F55-B2A9-B58A99E15F3E" );
            
            DropIndex("dbo.PageContext", "IX_PageIdEntityIdParameter");
        }
    }
}
