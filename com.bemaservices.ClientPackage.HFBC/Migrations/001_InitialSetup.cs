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

namespace com.bemaservices.ClientPackage.HFBC
{
    [MigrationNumber( 1, "1.7.4" )]
    public class InitialSetup : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/HFBC/Groups/GroupDetailLava2.ascx'
                    Where Path = '~/Plugins/org_hfbc/Groups/GroupDetailLava2.ascx'
            " );
            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/HFBC/Finance/BatchListWithGLExport.ascx'
                    Where Path = '~/Plugins/com_bemaservices/Finance/BatchListWithGLExport.ascx'
            " );
            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/HFBC/SearchBySSN/SearchBySSN.ascx'
                    Where Path = '~/Plugins/com_bemadev/SearchBySSN/SearchBySSN.ascx'
            " );
            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/HFBC/Legacy685/FamilyMembers.ascx'
                    Where Path = '~/Plugins/org_hfbc/Legacy685/FamilyMembers.ascx'
            " );
            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/HFBC/Legacy685/FamilyDetailLava.ascx'
                    Where Path = '~/Plugins/org_hfbc/Legacy685/FamilyDetailLava.ascx'
            " );
            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/HFBC/Legacy685/PersonSearchWithRedirect.ascx'
                    Where Path = '~/Plugins/org_hfbc/Legacy685/PersonSearchWithRedirect.ascx'
            " );
            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/HFBC/Legacy685/FamilyAttendanceHistoryList.ascx'
                    Where Path = '~/Plugins/org_hfbc/Legacy685/FamilyAttendanceHistoryList.ascx'
            " );
            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/HFBC/Legacy685/FamilyGroupList.ascx'
                    Where Path = '~/Plugins/org_hfbc/Legacy685/FamilyGroupList.ascx'
            " );
            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/HFBC/Legacy685/FamilyAttributeValues.ascx'
                    Where Path = '~/Plugins/org_hfbc/Legacy685/FamilyAttributeValues.ascx'
            " );
            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/HFBC/Legacy685/FamilyGroupListPersonalizedLava.ascx'
                    Where Path = '~/Plugins/org_hfbc/Legacy685/FamilyGroupListPersonalizedLava.ascx'
            " );
            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/HFBC/Legacy685/FamilyNotes.ascx'
                    Where Path = '~/Plugins/org_hfbc/Legacy685/FamilyNotes.ascx'
            " );
            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/HFBC/Legacy685/FamilyCommunicationRecipientList.ascx'
                    Where Path = '~/Plugins/org_hfbc/Legacy685/FamilyCommunicationRecipientList.ascx'
            " );
            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/HFBC/Legacy685/FamilyPublicProfile.ascx'
                    Where Path = '~/Plugins/org_hfbc/Legacy685/FamilyPublicProfile.ascx'
            " );
            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/HFBC/Finance/BenevolenceRequestDetail.ascx'
                    Where Path = '~/Plugins/com_bemaservices/Finance/BenevolenceRequestDetail.ascx'
            " );
            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/HFBC/Finance/ExportToGL.ascx'
                    Where Path = '~/Plugins/com_bemaservices/Finance/ExportToGL.ascx'
            " );
            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/HFBC/Workflow/WorkFlowRegistration.ascx'
                    Where Path = '~/Plugins/com_bemaservices/Workflow/WorkFlowRegistration.ascx'
            " );
            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/HFBC/Finance/TransactionReconciler.ascx'
                    Where Path = '~/Plugins/com_bemaservices/Finance/TransactionReconciler.ascx'
            " );
            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/HFBC/Event/DaycationParentHub.ascx'
                    Where Path = '~/Plugins/com_bemaservices/Event/DaycationParentHub.ascx'
            " );
            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/HFBC/Event/MultiEventRegistrationWizard.ascx'
                    Where Path = '~/Plugins/com_bemaservices/Event/MultiEventRegistrationWizard.ascx'
            " );

            // Uses Universal BEMA Blocks
            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/BEMA/Finance/BusinessDetail.ascx'
                    Where Path = '~/Plugins/com_bemaservices/Finance/BusinessDetail.ascx'
            " );

            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/BEMA/Finance/ScheduledTransactionListLiquid.ascx'
                    Where Path = '~/Plugins/com_bemaservices/Event/ScheduledFilteredTransactionListLiquid.ascx'
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

