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
namespace Rock.Plugin.HotFixes
{
    /// <summary>
    ///Migration MigrationRollupsForV8_7_6
    /// </summary>
    [MigrationNumber( 69, "1.8.6" )]
    public class MigrationRollupsForV8_7_6 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            //// Attrib for BlockType: Attendance Analytics:Filter Column Count
            //RockMigrationHelper.UpdateBlockTypeAttribute("3CD3411C-C076-4344-A9D5-8F3B4F01E31D","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Filter Column Count","FilterColumnCount","",@"The number of check boxes for each row.",14,@"1","244327E8-01EE-4860-9F12-4CF6144DFD61");
            //// Attrib for BlockType: Attendance Analytics:Filter Column Direction
            //RockMigrationHelper.UpdateBlockTypeAttribute("3CD3411C-C076-4344-A9D5-8F3B4F01E31D","7525C4CB-EE6B-41D4-9B64-A08048D5A5C0","Filter Column Direction","FilterColumnDirection","",@"Choose the direction for the checkboxes for filter selections.",13,@"vertical","0807FC61-26CE-41A1-9C54-8C4FA0DB6B5C");
            //// Attrib for BlockType: Giving Analytics:Filter Column Count
            //RockMigrationHelper.UpdateBlockTypeAttribute("48E4225F-8948-4FB0-8F00-1B43D3D9B3C3","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Filter Column Count","FilterColumnCount","",@"The number of check boxes for each row.",4,@"1","43B7025A-778A-4107-8243-D91A2FA74AA4");
            //// Attrib for BlockType: Giving Analytics:Filter Column Direction
            //RockMigrationHelper.UpdateBlockTypeAttribute("48E4225F-8948-4FB0-8F00-1B43D3D9B3C3","7525C4CB-EE6B-41D4-9B64-A08048D5A5C0","Filter Column Direction","FilterColumnDirection","",@"Choose the direction for the checkboxes for filter selections.",3,@"vertical","11C6953E-E176-40A2-9BF7-979344BD8FD8");
            //// Attrib Value for Block:Attendance Reporting, Attribute:Filter Column Count Page: Attendance Analytics, Site: Rock RMS
            //RockMigrationHelper.AddBlockAttributeValue("3EF007F1-6B46-4BCD-A435-345C03EBCA17","244327E8-01EE-4860-9F12-4CF6144DFD61",@"3");
            //// Attrib Value for Block:Attendance Reporting, Attribute:Filter Column Direction Page: Attendance Analytics, Site: Rock RMS
            //RockMigrationHelper.AddBlockAttributeValue("3EF007F1-6B46-4BCD-A435-345C03EBCA17","0807FC61-26CE-41A1-9C54-8C4FA0DB6B5C",@"horizontal");
            //// Attrib Value for Block:Giving Analysis, Attribute:Filter Column Count Page: Giving Analytics, Site: Rock RMS
            //RockMigrationHelper.AddBlockAttributeValue("784C58EF-B1B8-4237-BF12-E04DE8271A5A","43B7025A-778A-4107-8243-D91A2FA74AA4",@"3");
            //// Attrib Value for Block:Giving Analysis, Attribute:Filter Column Direction Page: Giving Analytics, Site: Rock RMS
            //RockMigrationHelper.AddBlockAttributeValue("784C58EF-B1B8-4237-BF12-E04DE8271A5A","11C6953E-E176-40A2-9BF7-979344BD8FD8",@"horizontal");

        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            // Not yet used by hotfix migrations.
        }



    }
}
