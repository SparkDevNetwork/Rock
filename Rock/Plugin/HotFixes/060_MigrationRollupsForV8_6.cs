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

    }
}
