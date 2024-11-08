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
    public partial class CodeGenerated_20240104 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attribute for BlockType              
            //   BlockType: Campus Context Setter              
            //   Category: Core              
            //   Attribute: Update Family Campus on Change              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4A5AAFFC-B1C7-4EFD-A9E4-84363242EA85", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Update Family Campus on Change", "UpdateFamilyCampusOnChange", "Update Family Campus on Change", @"When the individual changes the selected campus, should their family's campus (primary family) be updated?", 10, @"False", "EDFC3ADA-916A-4B0E-990E-23A3F539655B" );

            // Attribute for BlockType              
            //   BlockType: Campus Context Setter              
            //   Category: Core              
            //   Attribute: Campus Types              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4A5AAFFC-B1C7-4EFD-A9E4-84363242EA85", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Types", "CampusTypes", "Campus Types", @"This setting filters the list of campuses by type that are displayed in the campus drop-down.", 11, @"", "CF170C12-55D4-43D6-A8D6-6340D028E204" );

            // Attribute for BlockType              
            //   BlockType: Campus Context Setter              
            //   Category: Core              
            //   Attribute: Campus Statuses              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4A5AAFFC-B1C7-4EFD-A9E4-84363242EA85", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Statuses", "CampusStatuses", "Campus Statuses", @"This setting filters the list of campuses by statuses that are displayed in the campus drop-down.", 12, @"", "F0336B47-C29B-4143-9F07-2AA3F1AC042F" );

            // Attribute for BlockType              
            //   BlockType: Campus Context Setter              
            //   Category: Core              
            //   Attribute: Default Campus              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4A5AAFFC-B1C7-4EFD-A9E4-84363242EA85", "1B71FEF4-201F-4D53-8C60-2DF21F1985ED", "Default Campus", "DefaultCampus", "Default Campus", @"When there is no campus value, what campus should be displayed?", 9, @"", "B541147A-67FA-45BA-B5F9-00CF747AD575" );

            // Attribute for BlockType              
            //   BlockType: Power Bi Report Viewer              
            //   Category: Reporting              
            //   Attribute: URL Append Lava Template              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "76A64656-7BAB-4ADC-82DD-9CD207F548F9", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "URL Append Lava Template", "UrlAppendLavaTemplate", "URL Append Lava Template", @"The Lava Template for the Append Url", 6, @"", "6E5DA7FB-BBBE-43C5-9506-9541FD8B7DFF" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {

            // Attribute for BlockType              
            //   BlockType: Power Bi Report Viewer              
            //   Category: Reporting              
            //   Attribute: URL Append Lava Template              
            RockMigrationHelper.DeleteAttribute( "6E5DA7FB-BBBE-43C5-9506-9541FD8B7DFF" );

            // Attribute for BlockType              
            //   BlockType: Campus Context Setter              
            //   Category: Core              
            //   Attribute: Campus Statuses              
            RockMigrationHelper.DeleteAttribute( "F0336B47-C29B-4143-9F07-2AA3F1AC042F" );

            // Attribute for BlockType              
            //   BlockType: Campus Context Setter              
            //   Category: Core              
            //   Attribute: Campus Types              
            RockMigrationHelper.DeleteAttribute( "CF170C12-55D4-43D6-A8D6-6340D028E204" );

            // Attribute for BlockType              
            //   BlockType: Campus Context Setter              
            //   Category: Core              
            //   Attribute: Update Family Campus on Change              
            RockMigrationHelper.DeleteAttribute( "EDFC3ADA-916A-4B0E-990E-23A3F539655B" );

            // Attribute for BlockType              
            //   BlockType: Campus Context Setter              
            //   Category: Core              
            //   Attribute: Default Campus              
            RockMigrationHelper.DeleteAttribute( "B541147A-67FA-45BA-B5F9-00CF747AD575" );
        }
    }
}
