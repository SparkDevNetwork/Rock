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
    public partial class Rollup_0608 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateStatementGeneratorDownloadLinkUp();
            AddHiddenRockLegacyPageUp();
            UpdateExternalEnquiryWorkflowAction();
            LavaHeatmapShortcode();
            UpdateIndexDocumentUrlForGroup();
            UpdateTrendChartShortcode();
            UpdateTagColors();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            UpdateStatementGeneratorDownloadLinkDown();
            AddHiddenRockLegacyPageDown();
        }

        /// <summary>
        /// MP: Statement Generator Download Location - Updates the statement generator download link up.
        /// </summary>
        private void UpdateStatementGeneratorDownloadLinkUp()
        {
            Sql( @"
                DECLARE @statementGeneratorDefinedValueId INT = (SELECT Id FROM [DefinedValue] WHERE [Guid] = '54E1EBCC-5A5A-4B26-9CCB-36E7CEB49C3C')
                DECLARE @downloadUrlAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'E0AF9B30-15EA-413B-BAC4-25B286D91FD9')

                UPDATE [AttributeValue]
                SET [Value] = 'https://storage.rockrms.com/externalapplications/sparkdevnetwork/statementgenerator/1.12.5/statementgenerator.msi'
                WHERE AttributeId = @downloadUrlAttributeId and EntityId = @statementGeneratorDefinedValueId" );
        }

        /// <summary>
        /// MP: Statement Generator Download Location - Updates the statement generator download link down.
        /// </summary>
        private void UpdateStatementGeneratorDownloadLinkDown()
        {
            Sql( @"
                DECLARE @statementGeneratorDefinedValueId INT = (SELECT Id FROM [DefinedValue] WHERE [Guid] = '54E1EBCC-5A5A-4B26-9CCB-36E7CEB49C3C')
                DECLARE @downloadUrlAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'E0AF9B30-15EA-413B-BAC4-25B286D91FD9')

                UPDATE [AttributeValue]
                SET [Value] = 'https://storage.rockrms.com/externalapplications/sparkdevnetwork/statementgenerator/1.12.1/statementgenerator.msi'
                WHERE AttributeId = @downloadUrlAttributeId and EntityId = @statementGeneratorDefinedValueId" );
        }

        /// <summary>
        /// MB: Add hidden legacy rock update page up.
        /// </summary>
        private void AddHiddenRockLegacyPageUp()
        {
            // Add Page Legacy Rock Updater to Site:Rock RMS
            RockMigrationHelper.AddPage( true, "0B213645-FA4E-44A5-8E4C-B2D8EF054985","D65F783D-87A9-4CC9-8110-E83466A0EADB","Legacy Rock Updater","","EA9AE18F-3DBF-494D-947D-31BCE363DF39","");

            // Hide page from nav
            Sql( "UPDATE [Page] SET [DisplayInNavWhen] = 2 WHERE [Guid] = 'EA9AE18F-3DBF-494D-947D-31BCE363DF39'" );

            // Add Page Route for Legacy Rock Updater
#pragma warning disable CS0618 // Type or member is obsolete
            RockMigrationHelper.AddPageRoute("EA9AE18F-3DBF-494D-947D-31BCE363DF39","RockUpdateLegacy","F6896AB7-8964-4CF2-A66B-E574C5BB8C1B");
#pragma warning restore CS0618 // Type or member is obsolete
            // Add/Update BlockType Rock Update Legacy
            RockMigrationHelper.UpdateBlockType("Rock Update Legacy","Handles checking for and performing upgrades to the Rock system.","~/Blocks/Core/RockUpdateLegacy.ascx","Core","F3F2EFAD-1C25-444B-832C-4FB97F5B5D4F");
            // Add Block Rock Update Legacy to Page: Legacy Rock Updater, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "EA9AE18F-3DBF-494D-947D-31BCE363DF39".AsGuid(),null,"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"F3F2EFAD-1C25-444B-832C-4FB97F5B5D4F".AsGuid(), "Rock Update Legacy","Main",@"",@"",0,"12FF0356-27B5-431F-8C6A-7A08394EBFB5");   
        }

        /// <summary>
        /// MB: Add hidden legacy rock update page down.
        /// </summary>
        private void AddHiddenRockLegacyPageDown()
        {
            // Remove Block: Rock Update Legacy, from Page: Legacy Rock Updater, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("12FF0356-27B5-431F-8C6A-7A08394EBFB5");  
            // Delete BlockType Rock Update Legacy              
            RockMigrationHelper.DeleteBlockType("F3F2EFAD-1C25-444B-832C-4FB97F5B5D4F"); // Rock Update Legacy  
            // Delete Page Legacy Rock Updater from Site:Rock RMS              
            RockMigrationHelper.DeletePage("EA9AE18F-3DBF-494D-947D-31BCE363DF39"); //  Page: Legacy Rock Updater, Layout: Full Width, Site: Rock RMS  
            // Delete Page Workflow Entity Test Page from Site:External Website              
            RockMigrationHelper.DeletePage("4DC0E769-2D02-479A-AEE6-B2D340C4B59F"); //  Page: Workflow Entity Test Page, Layout: Blank, Site: External Website  
        }

        /// <summary>
        /// SK: (migration) Update External Enquiry Workflow Action
        /// </summary>
        private void UpdateExternalEnquiryWorkflowAction()
        {
            string lavaTemplate = @"{% if 'Global' | Attribute:'OrganizationPhone' != Empty %}If you need assistance right 
away, please call <strong>{{ 'Global' | Attribute:'OrganizationPhone' }}</strong> to speak with someone.{% endif %}";

            string newLavaTemplate = @"If you need assistance right 
away, please call <strong>{{ 'Global' | Attribute:'OrganizationPhone' }}</strong> to speak with someone.";

            lavaTemplate = lavaTemplate.Replace( "'", "''" );
            newLavaTemplate = newLavaTemplate.Replace( "'", "''" );

            var targetColumn = RockMigrationHelper.NormalizeColumnCRLF( "Header" );

            Sql( $@"
                UPDATE [WorkflowActionForm]
                SET [Header] = REPLACE( {targetColumn}
                    ,'{lavaTemplate}'
                    ,'{newLavaTemplate}' )
                WHERE [Guid] = '11D4769F-5B93-4605-8BCA-D21C14B0CEBA' AND {targetColumn} LIKE '%{lavaTemplate}%'"
            );
        }

        /// <summary>
        /// GJ: Lava Heatmap Shortcode
        /// </summary>
        private void LavaHeatmapShortcode()
        {
            Sql( MigrationSQL._202106081803525_Rollup_0608_LavaHeatMap );
        }

        /// <summary>
        /// SK: Update Index Document Url For Rock.Model.Group
        /// </summary>
        private void UpdateIndexDocumentUrlForGroup()
        {
            string lavaTemplate = @"DisplayOptions.Group-Url != ' %";

            string newLavaTemplate = @"DisplayOptions.Group-Url != '' %";

            lavaTemplate = lavaTemplate.Replace( "'", "''" );
            newLavaTemplate = newLavaTemplate.Replace( "'", "''" );

            var targetColumn = RockMigrationHelper.NormalizeColumnCRLF( "IndexDocumentUrl" );

            Sql( $@"
            UPDATE [EntityType]
            SET [IndexDocumentUrl] = REPLACE( {targetColumn}
                ,'{lavaTemplate}'
                ,'{newLavaTemplate}' )
            WHERE [Name] = 'Rock.Model.Group' AND {targetColumn} LIKE '%{lavaTemplate}%'" 
            );
        }

        /// <summary>
        /// GJ: Update Trend Chart Shortcode
        /// </summary>
        private void UpdateTrendChartShortcode()
        {
            Sql( MigrationSQL._202106081803525_Rollup_0608_UpdateTrendChart );
        }

        /// <summary>
        /// GJ: Update Tag Colors
        /// </summary>
        private void UpdateTagColors()
        {
            Sql( @"
                UPDATE [Tag] SET [BackgroundColor]=N'#9E9EA0' WHERE ([BackgroundColor]='#bababa');
                UPDATE [Tag] SET [BackgroundColor]=N'#BCBCBD' WHERE ([BackgroundColor]='#e0e0e0');" );
        }
    }
}
