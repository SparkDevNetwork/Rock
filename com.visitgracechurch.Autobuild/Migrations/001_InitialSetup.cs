// <copyright>
// Copyright by BEMA Information Technologies
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
using Rock.Plugin;

namespace com.visitgracechurch.Autobuild
{
    [MigrationNumber( 1, "1.9.4" )]
    public class InitialSetup : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/GCKS/Connection/ConnectionCard.ascx',
                        Category = 'BEMA Services > Connection'
                    Where Path = '~/Plugins/com_visitgracechurch/Connection/ConnectionCard.ascx'
            " );

            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/GCKS/Reporting/ServiceMetricsEntryGraceK.ascx',
                        Category = 'BEMA Services > Reporting'
                    Where Path = '~/Plugins/com_visitgracechurch/Reporting/ServiceMetricsEntryGraceK.ascx'
            " );

            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/GCKS/Finance/ContributionStatementFromTemplate.ascx',
                        Category = 'BEMA Services > Finance'
                    Where Path = '~/Plugins/com_visitgracechurch/Finance/ContributionStatementFromTemplate.ascx'
            " );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {

        }
    }
}

