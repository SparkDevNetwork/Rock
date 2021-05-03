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
    public partial class Rollup_0312 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Fix System.Data.SqlClient.SqlException "The conversion of a datetime2 data type to a datetime data type resulted in an out-of-range value. The statement has been terminated."
            AlterColumn( "dbo.Interaction", "InteractionEndDateTime", c => c.DateTime());

            // Attrib for BlockType: Email Form:CC Email(s)
            RockMigrationHelper.UpdateBlockTypeAttribute( "48253494-F8A0-4DD8-B645-6CB481CEB7BD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "CC Email(s)", "CCEmail", "", @"CC Email addresses (comma delimited) to send the contents to. <span class='tip tip-lava'></span>", 1, @"", "F8147243-92F3-4DAE-B132-206BA4C9DE75" );
            // Attrib for BlockType: Email Form:BCC Email(s)
            RockMigrationHelper.UpdateBlockTypeAttribute( "48253494-F8A0-4DD8-B645-6CB481CEB7BD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "BCC Email(s)", "BCCEmail", "", @"BCC Email addresses (comma delimited) to send the contents to. <span class='tip tip-lava'></span>", 2, @"", "F560B227-F517-4B04-870F-5DF6D723B4DA" );
            // Attrib for BlockType: Email Preference Entry:Available Options
            RockMigrationHelper.UpdateBlockTypeAttribute( "B3C076C7-1325-4453-9549-456C23702069", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Available Options", "AvailableOptions", "", @"Select the options that should be available to a user when they are updating their email preference.", 10, @"Unsubscribe,Update Email Address,Emails Allowed,No Mass Emails,No Emails,Not Involved", "F121A8A9-FCEF-4FBE-BCEC-B2FBAFF1BAF7" );
            // Attrib for BlockType: Attendance Analytics:Show Campus Filter
            RockMigrationHelper.UpdateBlockTypeAttribute( "3CD3411C-C076-4344-A9D5-8F3B4F01E31D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Campus Filter", "ShowCampusFilter", "", @"Should the Campus filter be displayed?", 9, @"True", "14D8F05F-3D63-4067-A437-EB1296EBF6B5" );
            // Attrib for BlockType: Attendance Analytics:Show Schedule Filter
            RockMigrationHelper.UpdateBlockTypeAttribute( "3CD3411C-C076-4344-A9D5-8F3B4F01E31D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Schedule Filter", "ShowScheduleFilter", "", @"Should the Schedules filter be displayed", 8, @"True", "5D84C6C3-ABBB-4FED-A72F-5060EB77DD4F" );
            // Attrib for BlockType: Attendance Analytics:Show View By Option
            RockMigrationHelper.UpdateBlockTypeAttribute( "3CD3411C-C076-4344-A9D5-8F3B4F01E31D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show View By Option", "ShowViewByOption", "", @"Should the option to view 'Attendees' vs 'Parents of Attendees' vs 'Children of Attendees' be displayed when viewing the grid? If not displayed, the grid will always show attendees.", 10, @"True", "9A85F7E3-89C5-473C-8DA5-ABE24E97A49F" );
            // Attrib for BlockType: Attendance Analytics:Group Specific
            RockMigrationHelper.UpdateBlockTypeAttribute( "3CD3411C-C076-4344-A9D5-8F3B4F01E31D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Group Specific", "GroupSpecific", "", @"Should this block display attendance only for the selected group?", 7, @"False", "BB582B2E-4A17-43CF-9FB2-0E54F78A7120" );
            // Attrib for BlockType: Prayer Session:Display Campus
            RockMigrationHelper.UpdateBlockTypeAttribute( "FD294789-3B72-4D83-8006-FA50B5087D06", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Campus", "DisplayCampus", "", @"Display the campus filter", 6, @"True", "CF395D2D-A20E-42B7-B10D-6ACC57A34422" );
            // Attrib for BlockType: Login:Redirect to Single External Auth Provider
            RockMigrationHelper.UpdateBlockTypeAttribute( "7B83D513-1178-429E-93FF-E76430E038E4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Redirect to Single External Auth Provider", "RedirecttoSingleExternalAuthProvider", "", @"Redirect straight to the external authentication provider if only one is configured and the internal login is disabled.", 12, @"False", "F794E74B-7834-40A3-8E16-71DDA2194DCD" );
            // Attrib for BlockType: Email Preference Entry:Update Email Address Text
            RockMigrationHelper.UpdateBlockTypeAttribute( "B3C076C7-1325-4453-9549-456C23702069", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Update Email Address Text", "UpdateEmailAddressText", "", @"Text to display for the 'Update Email Address' option.", 1, @"Update my email address.", "FAEC5E9D-134C-4288-ABE4-C680E400313C" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
