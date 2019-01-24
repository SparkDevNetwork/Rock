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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 60, "1.8.5" )]
    public class MigrationRollupsForV8_6 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            //FixPledgeAnalyticsGiftDateFilteringUp();
            //FixChartShortcode();
            //UpdateIsSystemForCategories();
            //FixTypeInTemplate();
            //FixPageRouteForContentPage();
        }


        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            //FixPledgeAnalyticsGiftDateFilteringDown();
        }

        /// <summary>
        /// ED: Date filtering is missing while calculating Gifts in spFinance_PledgeAnalyticsQuery
        /// </summary>
        private void FixPledgeAnalyticsGiftDateFilteringUp()
        {
            Sql( HotFixMigrationResource._060_MigrationRollupsForV8_6_spFinance_PledgeAnalyticsQuery_Up );
        }

        /// <summary>
        /// Reverts Change:
        /// ED: Date filtering is missing while calculating Gifts in spFinance_PledgeAnalyticsQuery
        /// </summary>
        private void FixPledgeAnalyticsGiftDateFilteringDown()
        {
            Sql( HotFixMigrationResource._060_MigrationRollupsForV8_6_spFinance_PledgeAnalyticsQuery_Down );
        }

        /// <summary>
        /// GJ: Fix Chart Shortcode
        /// </summary>
        private void FixChartShortcode()
        {
            Sql( HotFixMigrationResource._060_MigrationRollupsForV8_6_FixChartShortcode );
        }

        /// <summary>
        /// SK: Set IsSystem = 1 in a new migration roll-up (so system items cannot be accidentally deleted) 
        /// </summary>
        private void UpdateIsSystemForCategories()
        {
                        Sql( @"
              UPDATE [Category] SET [IsSystem] = 1 WHERE [IsSystem] = 0 AND [CreatedDateTime] IS NULL AND [Guid] = 'E919E722-F895-44A4-B86D-38DB8FBA1844'
              UPDATE [Category] SET [IsSystem] = 1 WHERE [IsSystem] = 0 AND [CreatedDateTime] IS NULL AND [Guid] = 'F6B98D0C-197D-433A-917B-0C39A80A79E8'
              UPDATE [Category] SET [IsSystem] = 1 WHERE [IsSystem] = 0 AND [CreatedDateTime] IS NULL AND [Guid] = '9AF28593-E631-41E4-B696-78015A4D6F7B'
              UPDATE [Category] SET [IsSystem] = 1 WHERE [IsSystem] = 0 AND [CreatedDateTime] IS NULL AND [Guid] = '7B879922-5DA6-41EE-AC0B-45CEFFB99458'
              UPDATE [Category] SET [IsSystem] = 1 WHERE [IsSystem] = 0 AND [CreatedDateTime] IS NULL AND [Guid] = 'BA5B3BFE-C6C2-4B13-89A2-83F0A67486DA'
              UPDATE [Category] SET [IsSystem] = 1 WHERE [IsSystem] = 0 AND [CreatedDateTime] IS NULL AND [Guid] = '41BFFA70-905A-4A88-B8E7-9F1227760183'
              UPDATE [Category] SET [IsSystem] = 1 WHERE [IsSystem] = 0 AND [CreatedDateTime] IS NULL AND [Guid] = 'FD1AF609-6907-47D1-A11D-C9FE19AFD585'
              UPDATE [Category] SET [IsSystem] = 1 WHERE [IsSystem] = 0 AND [CreatedDateTime] IS NULL AND [Guid] = 'AE55DDF5-C81A-44C7-B5AC-AC8A2768CDA4'
              UPDATE [Category] SET [IsSystem] = 1 WHERE [IsSystem] = 0 AND [CreatedDateTime] IS NULL AND [Guid] = '4FEDD93A-940E-4414-BEAB-67A384D6CD35'
              UPDATE [Category] SET [IsSystem] = 1 WHERE [IsSystem] = 0 AND [CreatedDateTime] IS NULL AND [Guid] = 'F5FF6C2E-925C-4D02-A6CD-37185C7FFC66'
              UPDATE [Category] SET [IsSystem] = 1 WHERE [IsSystem] = 0 AND [CreatedDateTime] IS NULL AND [Guid] = '54F8D7D6-700A-4F6A-BB5A-2E2FEF3E4244'
              UPDATE [Category] SET [IsSystem] = 1 WHERE [IsSystem] = 0 AND [CreatedDateTime] IS NULL AND [Guid] = '6FE3AFD4-CA1B-4C84-9C89-252063EBA755'
              UPDATE [Category] SET [IsSystem] = 1 WHERE [IsSystem] = 0 AND [CreatedDateTime] IS NULL AND [Guid] = 'CD2621DC-5DAD-4142-9144-AF2301C353A9'
              UPDATE [Category] SET [IsSystem] = 1 WHERE [IsSystem] = 0 AND [CreatedDateTime] IS NULL AND [Guid] = 'C8E0FD8D-3032-4ACD-9DB9-FF70B11D6BCC'
            " );
        }

        /// <summary>
        /// MP: Fix article typo in Content Channel View Detail block in the Lava Template fixes #3350
        /// </summary>
        private void FixTypeInTemplate()
        {
            Sql( @"UPDATE AttributeValue
                SET Value = REPLACE(Value, '<artcile class=""message-detail""', '<article class=""message-detail""')
                WHERE Value LIKE '%<artcile class=""message-detail""%'
	                AND AttributeId IN (
		                SELECT Id
		                FROM Attribute
		                WHERE [Guid] = '47C56661-FB70-4703-9781-8651B8B49485'
		                )");
        }

        /// <summary>
        /// SK: Fixed Page Route for content page.
        /// </summary>
        private void FixPageRouteForContentPage()
        {
            Sql( @"

                DECLARE @PageId int
                SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '117B547B-9D71-4EE9-8047-176676F5DC8C')

                UPDATE
                   [PageRoute]
                SET 
                   [Route] = 'ContentChannel/{contentChannelGuid}'
                WHERE [PageId] = @PageId" );
        }

    }
}
