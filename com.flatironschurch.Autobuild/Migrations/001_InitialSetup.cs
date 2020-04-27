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

namespace com.flatironschurch.Autobuild
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
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/FLCC/Finance/RunSQLOnClick.ascx',
                        Category = 'BEMA Services > Finance'
                    Where Path = '~/Plugins/com_bemadev/Finance/RunSQLOnClick.ascx'
            " );

            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/FLCC/Groups/GroupDetailLava.ascx'
                    Where Path = '~/Plugins/com_bemaservices/Groups/GroupDetailLava.ascx'
            " );

            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/BEMA/Communication/CommunicationEntryWizard.ascx'
                    Where Path = '~/Plugins/com_bemaservices/Communication/CommunicationEntryWizard.ascx'
            " );

            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/BEMA/Finance/ScheduledTransactionView.ascx'
                    Where Path = '~/Plugins/com_bemaservices/Finance/ScheduledTransactionView-Old.ascx'
            " );

            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/FLCC/Finance/ExportToGL.ascx'
                    Where Path = '~/Plugins/com_bemaservices/Finance/ExportToGL.ascx'
            " );

            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/BEMA/Fundraising/FundraisingLeaderToolbox.ascx'
                    Where Path = '~/Plugins/com_bemaservices/Fundraising/FundraisingLeaderToolbox.ascx'
            " );

            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/BEMA/Finance/TransactionEntry.ascx'
                    Where Path = '~/Plugins/com_bemaservices/Finance/TransactionEntry.ascx'
            " );

            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/BEMA/Event/RegistrationInstanceDetail.ascx'
                    Where Path = '~/Plugins/com_bemaservices/Event/RegistrationInstanceDetail.ascx'
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

