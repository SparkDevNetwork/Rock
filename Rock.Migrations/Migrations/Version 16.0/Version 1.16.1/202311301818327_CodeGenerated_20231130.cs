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

    /// <summary>
    ///
    /// </summary>
    public partial class CodeGenerated_20231130 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attribute for BlockType              
            //   BlockType: Attendance Detail              
            //   Category: Check-in > Manager              
            //   Attribute: Allow Editing Start and End Times              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CA59CE67-9313-4B9F-8593-380044E5AE6A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Editing Start and End Times", "AllowEditingStartAndEndTimes", "Allow Editing Start and End Times", @"This allows editing the start and end datetime to be edited", 7, @"False", "2E10F8A0-4892-42D6-B03A-8179DC2750A8" );

            // Attribute for BlockType              
            //   BlockType: Group Tree View              
            //   Category: Groups              
            //   Attribute: Limit to Security Role Groups              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2D26A2C4-62DC-4680-8219-A52EB2BC0F65", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Limit to Security Role Groups", "LimitToSecurityRoleGroups", "Limit to Security Role Groups", @"", 5, @"False", "926F9CD7-2C0B-406D-A2BD-7B4A3A4F0E92" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {

            // Attribute for BlockType              
            //   BlockType: Group Tree View              
            //   Category: Groups              
            //   Attribute: Limit to Security Role Groups              
            RockMigrationHelper.DeleteAttribute( "926F9CD7-2C0B-406D-A2BD-7B4A3A4F0E92" );

            // Attribute for BlockType              
            //   BlockType: Attendance Detail              
            //   Category: Check-in > Manager              
            //   Attribute: Allow Editing Start and End Times              
            RockMigrationHelper.DeleteAttribute( "2E10F8A0-4892-42D6-B03A-8179DC2750A8" );
        }
    }
}
