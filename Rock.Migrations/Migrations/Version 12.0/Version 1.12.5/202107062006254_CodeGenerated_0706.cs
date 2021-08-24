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
    public partial class CodeGenerated_0706 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add/Update Mobile Block Type:Daily Challenge Entry
            RockMigrationHelper.UpdateMobileBlockType("Daily Challenge Entry", "Displays a set of challenges for the individual to complete today.", "Rock.Blocks.Types.Mobile.Cms.DailyChallengeEntry", "Mobile > Cms", "B702FF5B-2488-42C7-AAE8-2DD99E82326D");

            // Attribute for BlockType: Service Metrics Entry:Limit Campus Selection to Campus Team Membership
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "535E1879-CD4C-432B-9312-B27B3A668D88", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Limit Campus Selection to Campus Team Membership", "LimitCampusByCampusTeam", "Limit Campus Selection to Campus Team Membership", @"When enabled, this would limit the campuses shown to only those where the individual was on the Campus Team.", 7, @"False", "6CC2B44F-A3FF-4AC8-A50A-0AE660EF158F" );

            // Attribute for BlockType: Service Metrics Entry:Filter Schedules by Campus
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "535E1879-CD4C-432B-9312-B27B3A668D88", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Filter Schedules by Campus", "FilterByCampus", "Filter Schedules by Campus", @"When enabled, only schedules that are included in the Campus Schedules will be included.", 10, @"False", "8E60D466-C351-4DAC-A843-048B4FFE58CB" );

            // Attribute for BlockType: Service Metrics Entry:Campus Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "535E1879-CD4C-432B-9312-B27B3A668D88", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Types", "CampusTypes", "Campus Types", @"Note: setting this can override the selected Campuses block setting.", 8, @"", "ABF0087F-39CC-40F1-AF75-6BBBB2C6D5BA" );

            // Attribute for BlockType: Service Metrics Entry:Campus Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "535E1879-CD4C-432B-9312-B27B3A668D88", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Status", "CampusStatus", "Campus Status", @"Note: setting this can override the selected Campuses block setting.", 9, @"", "3C9C7CC8-38BC-4EB0-8ACC-4B0B6E0693BE" );

            // Attribute for BlockType: Daily Challenge Entry:Content Channel
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B702FF5B-2488-42C7-AAE8-2DD99E82326D", "D835A0EC-C8DB-483A-A37C-E8FB6E956C3D", "Content Channel", "ContentChannel", "Content Channel", @"The content channel that describes the challenge.", 0, @"", "6A969EF1-7A3C-4BF2-B6C3-4808A337C0BB" );

            // Attribute for BlockType: Daily Challenge Entry:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B702FF5B-2488-42C7-AAE8-2DD99E82326D", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the block.", 3, @"08009450-92A5-4D4A-8E31-FCC1E4CBDF16", "A1E961D6-310F-4581-B52E-5410939D39F5" );

            // Attribute for BlockType: Daily Challenge Entry:Allow Backfill in Days
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B702FF5B-2488-42C7-AAE8-2DD99E82326D", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Allow Backfill in Days", "AllowBackfillInDays", "Allow Backfill in Days", @"The number of days the individual should be allowed to go back and fill in if they missed some days.", 1, @"", "89D015B1-33A7-4CCA-A8F7-1BCB6945C7A1" );

            // Attribute for BlockType: Daily Challenge Entry:Challenge Cache Duration
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B702FF5B-2488-42C7-AAE8-2DD99E82326D", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Challenge Cache Duration", "CacheDuration", "Challenge Cache Duration", @"Number of seconds to cache the challenge configuration.", 2, @"0", "D1304C5E-A9E1-4353-8601-6E5C7B3CF618" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Template Attribute for BlockType: Daily Challenge Entry
            RockMigrationHelper.DeleteAttribute("A1E961D6-310F-4581-B52E-5410939D39F5");

            // Challenge Cache Duration Attribute for BlockType: Daily Challenge Entry
            RockMigrationHelper.DeleteAttribute("D1304C5E-A9E1-4353-8601-6E5C7B3CF618");

            // Allow Backfill in Days Attribute for BlockType: Daily Challenge Entry
            RockMigrationHelper.DeleteAttribute("89D015B1-33A7-4CCA-A8F7-1BCB6945C7A1");

            // Content Channel Attribute for BlockType: Daily Challenge Entry
            RockMigrationHelper.DeleteAttribute("6A969EF1-7A3C-4BF2-B6C3-4808A337C0BB");

            // Filter Schedules by Campus Attribute for BlockType: Service Metrics Entry
            RockMigrationHelper.DeleteAttribute("8E60D466-C351-4DAC-A843-048B4FFE58CB");

            // Campus Status Attribute for BlockType: Service Metrics Entry
            RockMigrationHelper.DeleteAttribute("3C9C7CC8-38BC-4EB0-8ACC-4B0B6E0693BE");

            // Campus Types Attribute for BlockType: Service Metrics Entry
            RockMigrationHelper.DeleteAttribute("ABF0087F-39CC-40F1-AF75-6BBBB2C6D5BA");

            // Limit Campus Selection to Campus Team Membership Attribute for BlockType: Service Metrics Entry
            RockMigrationHelper.DeleteAttribute("6CC2B44F-A3FF-4AC8-A50A-0AE660EF158F");

            // Delete BlockType Daily Challenge Entry
            RockMigrationHelper.DeleteBlockType("B702FF5B-2488-42C7-AAE8-2DD99E82326D"); // Daily Challenge Entry
        }
    }
}
