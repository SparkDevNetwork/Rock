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
    public partial class Rollup_1127 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();
            FixPledgeAnalyticsGiftDateFilteringUp();
            AddFileEditorBlockUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddFileEditorBlockDown();
            FixPledgeAnalyticsGiftDateFilteringDown();
            CodeGenMigrationsDown();
        }

        private void CodeGenMigrationsUp()
        {
            // Attrib for BlockType: Batch List:Summary Lava Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "AB345CE7-5DC6-41AF-BBDC-8D23D52AFE25", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Summary Lava Template", "SummaryLavaTemplate", "", @"The lava template for display the content for summary", 3, @"
         <div class='panel panel-block'>
            <div class='panel-heading'>
                <h1 class='panel-title'>Total Results</h1>
            </div>
            <div class='panel-body'>
                {% assign totalAmount = 0 %}
                {% for batchSummary in BatchSummary %}
                <div class='row'>
                    <div class='col-xs-8'>{{ batchSummary.FinancialAccount.Name }}</div>
                    <div class='col-xs-4 text-right'>{{ batchSummary.TotalAmount | FormatAsCurrency }}</div>
                </div>
                {% assign totalAmount = totalAmount | Plus: batchSummary.TotalAmount %}
                {% endfor %}
                <div class='row'>
                    <div class='col-xs-8'><b>Total: </div>
                    <div class='col-xs-4 text-right'>
                        {{ totalAmount | FormatAsCurrency }}
                    </div>
                </div>
            </div>
        </div>
", "1D7430E7-27E5-4426-A07F-2AF591E5D9C4" );

        }

        private void CodeGenMigrationsDown()
        {
            // Attrib for BlockType: Batch List:Summary Lava Template
            RockMigrationHelper.DeleteAttribute( "1D7430E7-27E5-4426-A07F-2AF591E5D9C4" );
        }

        /// <summary>
        /// ED: Date filtering is missing while calculating Gifts in spFinance_PledgeAnalyticsQuery
        /// </summary>
        private void FixPledgeAnalyticsGiftDateFilteringUp()
        {
            Sql( MigrationSQL._201811271852316_Rollup_1127_spFinance_PledgeAnalyticsQuery_Up );
        }

        /// <summary>
        /// Reverts Change:
        /// ED: Date filtering is missing while calculating Gifts in spFinance_PledgeAnalyticsQuery
        /// </summary>
        private void FixPledgeAnalyticsGiftDateFilteringDown()
        {
            Sql( MigrationSQL._201811271852316_Rollup_1127_spFinance_PledgeAnalyticsQuery_Down );
        }

        /// <summary>
        /// SK: Add File Editor Block to File Manager
        /// </summary>
        private void AddFileEditorBlockUp()
        {
            Sql( @"
            UPDATE [BlockType]
            SET [Name] = 'File Editor'
            WHERE [Guid] = '0F1DADBC-6B12-4BAA-A828-FD1AA86AA387'" );

            RockMigrationHelper.AddPage( true, "6F074DAA-BDCC-44C5-BB89-B899C1AAC6C1", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "File Editor", "", "053C3F1D-8BF2-48B2-A8E6-55184F8A87F4", "" ); // Site:Rock RMS
            RockMigrationHelper.AddBlock( true, "053C3F1D-8BF2-48B2-A8E6-55184F8A87F4".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "0F1DADBC-6B12-4BAA-A828-FD1AA86AA387".AsGuid(), "File Editor", "Main", @"", @"", 0, "FB5BC320-1D3A-44F5-9585-628E7CC33737" );
            // Attrib Value for Block:File Manager, Attribute:File Editor Page Page: File Manager, Site: Rock RMS             
            RockMigrationHelper.AddBlockAttributeValue( "B4847448-371F-40B5-9EDA-5C376F233E48", "15F4E62D-FC89-4C7C-8E73-6A3D75A4FB19", @"053c3f1d-8bf2-48b2-a8e6-55184f8a87f4" );
        }

        /// <summary>
        /// Reverts change:
        /// SK: Add File Editor Block to File Manager
        /// </summary>
        private void AddFileEditorBlockDown()
        {
            // Remove Block: File Editor, from Page: File Editor, Site: Rock RMS              
            RockMigrationHelper.DeleteBlock( "FB5BC320-1D3A-44F5-9585-628E7CC33737" );
            RockMigrationHelper.DeletePage( "053C3F1D-8BF2-48B2-A8E6-55184F8A87F4" ); //  Page: File Editor, Layout: Full Width, Site: Rock RMS
        }
    }
}
