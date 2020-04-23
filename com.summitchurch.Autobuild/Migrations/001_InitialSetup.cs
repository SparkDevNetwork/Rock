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

namespace com.summitchurch.Autobuild
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
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/SUCH/Finance/BatchListWithGLExport.ascx',
                        Category = 'BEMA Services > Finance'
                    Where Path = '~/Plugins/com_bemadev/Finance/BatchListWithGLExport.ascx'
            " );

            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/SUCH/Finance/TransactionImport.ascx',
                        Category = 'BEMA Services > Finance'
                    Where Path = '~/Plugins/com_bemadev/Finance/TransactionImport.ascx'
            " );

            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/SUCH/CRM/FamilyPreRegistration.ascx',
                        Category = 'BEMA Services > Crm'
                    Where Path = '~/Plugins/com_bemadev/CRM/FamilyPreRegistration.ascx'
            " );

            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/SUCH/Finance/PledgeDetail.ascx',
                        Category = 'BEMA Services > Finance'
                    Where Path = '~/Plugins/com_bemadev/Finance/PledgeDetail.ascx'
            " );

            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/SUCH/Event/CalendarLava.ascx',
                        Category = 'BEMA Services > Event'
                    Where Path = '~/Plugins/com_bemadev/Events/CalendarLavaCust.ascx'
            " );

            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/BEMA/Finance/ContributionStatementLava.ascx',
                        Category = 'BEMA Services > Finance'
                    Where Path = '~/Plugins/com_bemadev/Finance/ContributionStatementLava.ascx'
            " );

            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/SUCH/Finance/TransactionImportServiceU.ascx',
                        Category = 'BEMA Services > Finance'
                    Where Path = '~/Plugins/com_bemadev/Finance/TransactionImportServiceU.ascx'
            " );

            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/SUCH/Security/AccountEntry.ascx',
                        Category = 'BEMA Services > Security'
                    Where Path = '~/Plugins/com_bemadev/Security/AccountEntry.ascx'
            " );

            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/SUCH/Cms/PublicProfileEdit.ascx',
                        Category = 'BEMA Services > Cms'
                    Where Path = '~/Plugins/com_bemadev/CRM/PublicProfileEdit.ascx'
            " );

            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/BEMA/Fundraising/FundraisingDonationEntry.ascx',
                        Category = 'BEMA Services > Fundraising'
                    Where Path = '~/Plugins/com_bemadev/Fundraising/FundraisingDonationEntry.ascx'
            " );

            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/BEMA/Finance/TransactionMatching.ascx',
                        Category = 'BEMA Services > Finance'
                    Where Path = '~/Plugins/com_bemadev/Finance/TransactionMatching.ascx'
            " );

            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/BEMA/Event/RegistrationEntryWithSlots.ascx',
                        Category = 'BEMA Services > Event'
                    Where Path = '~/Plugins/com_bemaservices/Event/RegistrationEntryWithSlots.ascx'
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

