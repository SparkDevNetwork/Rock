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
    public partial class CodeGenerated_20211214 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            
            // Attribute for BlockType
            //   BlockType: Onboard Person
            //   Category: Mobile > Security
            //   Attribute: Disable Matching for the Following Protection Profiles
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Disable Matching for the Following Protection Profiles", "DisableMatchingProtectionProfiles", "Disable Matching for the Following Protection Profiles", @"This disables matching on people with one of the selected protection profiles. A person with a selected protection profile will be required to login by username and password.", 8, @"2,3", "3D8F0BF8-7C5D-4043-9DB9-E053413914D9" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            
            // Attribute for BlockType
            //   BlockType: Onboard Person
            //   Category: Mobile > Security
            //   Attribute: Disable Matching for the Following Protection Profiles
            RockMigrationHelper.DeleteAttribute("3D8F0BF8-7C5D-4043-9DB9-E053413914D9");
        }
    }
}
