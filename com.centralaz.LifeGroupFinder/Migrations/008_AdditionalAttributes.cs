// <copyright>
// Copyright by Central Christian Church
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

using Rock.Plugin;
using Rock.Web.Cache;

namespace com.centralaz.LifeGroupFinder.Migrations
{
    [MigrationNumber( 8, "1.4.0" )]
    public class AdditionalAttributes : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            //Life Group Launch Workflow button
            RockMigrationHelper.UpdateBlockType( "Fire Workflow Button", "Allows a user to fire off a workflow", "~/Plugins/com_centralaz/LifeGroupFinder/FireWorkflowButton.ascx", "com_centralaz > Groups", "33677897-2A3C-46BF-81C7-F5A61788B63C" );
            RockMigrationHelper.AddBlockTypeAttribute( "33677897-2A3C-46BF-81C7-F5A61788B63C", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow Actions", "WorkflowActions", "", "The workflow to make available as an action.", 1, @"", "B662ADD2-96DE-43CA-91F4-B2B2EE431B71" );
            RockMigrationHelper.AddBlockTypeAttribute( "33677897-2A3C-46BF-81C7-F5A61788B63C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Button Text", "ButtonText", "", "The text the workflow button will display.", 0, @"", "96DF2C04-8977-4D49-A1EB-5935849BE436" );

            //Life Group Show Map 
            RockMigrationHelper.AddBlockTypeAttribute( "205531A1-C1BC-494C-911E-EE88D29969FB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Map Link", "ShowMapLink", "", "Whether to show the map link", 0, @"False", "D128D295-5372-4EC8-9429-F0A99615FA49" );
            RockMigrationHelper.AddBlockAttributeValue( "6E830C1F-E5BB-4BCB-AA59-807204134E3E", "D128D295-5372-4EC8-9429-F0A99615FA49", @"False" ); // Show Map Link

            //New Life Group Detail Lava Block
            RockMigrationHelper.UpdateBlockType( "Life Group Detail Lava", "Presents the details of a group using Lava", "~/Plugins/com_centralaz/LifeGroupFinder/LifeGroupDetailLava.ascx", "com_centralaz > Groups", "987853FB-549B-4DEE-905F-9FD588D00DC6" );
            RockMigrationHelper.AddBlockTypeAttribute( "987853FB-549B-4DEE-905F-9FD588D00DC6", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Detail Page", "PersonDetailPage", "", "Page to link to for more information on a group member.", 0, @"", "769829DC-9E9F-4C9B-A199-4E62FD2BCAE6" );
            RockMigrationHelper.AddBlockTypeAttribute( "987853FB-549B-4DEE-905F-9FD588D00DC6", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Member Add Page", "GroupMemberAddPage", "", "Page to use for adding a new group member. If no page is provided the built in group member edit panel will be used. This panel allows the individual to search the database.", 1, @"", "82E78813-B999-4830-B332-A773C269C42F" );
            RockMigrationHelper.AddBlockTypeAttribute( "987853FB-549B-4DEE-905F-9FD588D00DC6", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Roster Page", "RosterPage", "", "The page to link to to view the roster.", 2, @"", "6DC1A19F-E409-4DB4-9188-B4FC437383F3" );
            RockMigrationHelper.AddBlockTypeAttribute( "987853FB-549B-4DEE-905F-9FD588D00DC6", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Attendance Page", "AttendancePage", "", "The page to link to to manage the group's attendance.", 3, @"", "15BD52B5-2B07-4C15-9661-69F011016AF0" );
            RockMigrationHelper.AddBlockTypeAttribute( "987853FB-549B-4DEE-905F-9FD588D00DC6", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Communication Page", "CommunicationPage", "", "The communication page to use for sending emails to the group members.", 4, @"", "365BB9A4-940C-4291-8231-D9FC0DD4095D" );
            RockMigrationHelper.AddBlockTypeAttribute( "987853FB-549B-4DEE-905F-9FD588D00DC6", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "", "The lava template to use to format the group details.", 5, @"{% include '~~/Assets/Lava/GroupDetail.lava' %}", "95641A2E-EE45-489E-8E59-F0B73DE3CEB5" );
            RockMigrationHelper.AddBlockTypeAttribute( "987853FB-549B-4DEE-905F-9FD588D00DC6", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Location Edit", "EnableLocationEdit", "", "Enables changing locations when editing a group.", 6, @"False", "139F5813-26A5-4D4F-8EBC-404EA60BD40F" );
            RockMigrationHelper.AddBlockTypeAttribute( "987853FB-549B-4DEE-905F-9FD588D00DC6", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Debug", "EnableDebug", "", "Shows the fields available to merge in lava.", 7, @"False", "067525BF-B4CA-406A-99EC-72170ED2E601" );
            RockMigrationHelper.AddBlockTypeAttribute( "987853FB-549B-4DEE-905F-9FD588D00DC6", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Edit Group Pre-HTML", "EditGroupPre-HTML", "", "HTML to display before the edit group panel.", 8, @"", "2198493A-D929-4C84-8E1D-078A84807FB8" );
            RockMigrationHelper.AddBlockTypeAttribute( "987853FB-549B-4DEE-905F-9FD588D00DC6", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Edit Group Post-HTML", "EditGroupPost-HTML", "", "HTML to display after the edit group panel.", 9, @"", "F2AAF0F6-F7D4-4A7C-8481-7C85438E5848" );
            RockMigrationHelper.AddBlockTypeAttribute( "987853FB-549B-4DEE-905F-9FD588D00DC6", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Edit Group Member Pre-HTML", "EditGroupMemberPre-HTML", "", "HTML to display before the edit group member panel.", 10, @"", "ABF0E163-DEC9-4EC4-ABB0-C7E1E4DE5008" );
            RockMigrationHelper.AddBlockTypeAttribute( "987853FB-549B-4DEE-905F-9FD588D00DC6", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Edit Group Member Post-HTML", "EditGroupMemberPost-HTML", "", "HTML to display after the edit group member panel.", 11, @"", "446455C6-3381-4299-8882-8992E8A694D1" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            //Life Group Launch Workflow Button
            RockMigrationHelper.DeleteAttribute( "B662ADD2-96DE-43CA-91F4-B2B2EE431B71" );
            RockMigrationHelper.DeleteAttribute( "96DF2C04-8977-4D49-A1EB-5935849BE436" );
            RockMigrationHelper.DeleteBlockType( "33677897-2A3C-46BF-81C7-F5A61788B63C" );
            
            //Life Group Show Map
            RockMigrationHelper.DeleteAttribute( "D128D295-5372-4EC8-9429-F0A99615FA49" );

            //New Life Group Detail Lava Block
            RockMigrationHelper.DeleteAttribute( "446455C6-3381-4299-8882-8992E8A694D1" );
            RockMigrationHelper.DeleteAttribute( "ABF0E163-DEC9-4EC4-ABB0-C7E1E4DE5008" );
            RockMigrationHelper.DeleteAttribute( "F2AAF0F6-F7D4-4A7C-8481-7C85438E5848" );
            RockMigrationHelper.DeleteAttribute( "2198493A-D929-4C84-8E1D-078A84807FB8" );
            RockMigrationHelper.DeleteAttribute( "067525BF-B4CA-406A-99EC-72170ED2E601" );
            RockMigrationHelper.DeleteAttribute( "139F5813-26A5-4D4F-8EBC-404EA60BD40F" );
            RockMigrationHelper.DeleteAttribute( "95641A2E-EE45-489E-8E59-F0B73DE3CEB5" );
            RockMigrationHelper.DeleteAttribute( "365BB9A4-940C-4291-8231-D9FC0DD4095D" );
            RockMigrationHelper.DeleteAttribute( "15BD52B5-2B07-4C15-9661-69F011016AF0" );
            RockMigrationHelper.DeleteAttribute( "6DC1A19F-E409-4DB4-9188-B4FC437383F3" );
            RockMigrationHelper.DeleteAttribute( "82E78813-B999-4830-B332-A773C269C42F" );
            RockMigrationHelper.DeleteAttribute( "769829DC-9E9F-4C9B-A199-4E62FD2BCAE6" );
            RockMigrationHelper.DeleteBlockType( "987853FB-549B-4DEE-905F-9FD588D00DC6" );
        }
    }
}
