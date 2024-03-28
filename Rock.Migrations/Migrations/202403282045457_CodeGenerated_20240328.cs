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
    public partial class CodeGenerated_20240328 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Engagement.StepProgramDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Engagement.StepProgramDetail", "Step Program Detail", "Rock.Blocks.Engagement.StepProgramDetail, Rock.Blocks, Version=1.16.4.2, Culture=neutral, PublicKeyToken=null", false, false, "7260278E-EFB7-4B98-A862-15BF0A40BA2E" );

            // Add/Update Obsidian Block Type
            //   Name:Step Program Detail
            //   Category:Steps
            //   EntityType:Rock.Blocks.Engagement.StepProgramDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Step Program Detail", "Displays the details of the given Step Program for editing.", "Rock.Blocks.Engagement.StepProgramDetail", "Steps", "E2F965D1-7419-4062-9568-08613BB696E3" );

            // Attribute for BlockType
            //   BlockType: Step Program Detail
            //   Category: Steps
            //   Attribute: Show Chart
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E2F965D1-7419-4062-9568-08613BB696E3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Chart", "Show Chart", "Show Chart", @"", 0, @"true", "B03F0821-699E-45D6-AB5C-50DFF3568D9F" );

            // Attribute for BlockType
            //   BlockType: Step Program Detail
            //   Category: Steps
            //   Attribute: Default Chart Date Range
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E2F965D1-7419-4062-9568-08613BB696E3", "55810BC5-45EA-4044-B783-0CCE0A445C6F", "Default Chart Date Range", "SlidingDateRange", "Default Chart Date Range", @"", 1, @"Current||Year||", "5D5C48E5-32A8-4D4E-9098-ED30FB05C3E0" );

            // Attribute for BlockType
            //   BlockType: Step Program Detail
            //   Category: Steps
            //   Attribute: Key Performance Indicator Lava
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E2F965D1-7419-4062-9568-08613BB696E3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Key Performance Indicator Lava", "KpiLava", "Key Performance Indicator Lava", @"The Lava used to render the Key Performance Indicators bar. <span class='tip tip-lava'></span>", 2, @"{[kpis style:'card' iconbackground:'true']}
  [[ kpi icon:'fa-user' value:'{{IndividualsCompleting | Format:'N0'}}' label:'Individuals Completing Program' color:'blue-700']][[ endkpi ]]
  [[ kpi icon:'fa-calendar' value:'{{AvgDaysToComplete | Format:'N0'}}' label:'Average Days to Complete Program' color:'green-600']][[ endkpi ]]
  [[ kpi icon:'fa-map-marker' value:'{{StepsStarted | Format:'N0'}}' label:'Steps Started' color:'#FF385C']][[ endkpi ]]
  [[ kpi icon:'fa-check-square' value:'{{StepsCompleted | Format:'N0'}}' label:'Steps Completed' color:'indigo-700']][[ endkpi ]]
{[endkpis]}", "742AFD42-B520-49CE-9A0C-C66B2467773F" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Step Program Detail
            //   Category: Steps
            //   Attribute: Key Performance Indicator Lava
            RockMigrationHelper.DeleteAttribute( "742AFD42-B520-49CE-9A0C-C66B2467773F" );

            // Attribute for BlockType
            //   BlockType: Step Program Detail
            //   Category: Steps
            //   Attribute: Default Chart Date Range
            RockMigrationHelper.DeleteAttribute( "5D5C48E5-32A8-4D4E-9098-ED30FB05C3E0" );

            // Attribute for BlockType
            //   BlockType: Step Program Detail
            //   Category: Steps
            //   Attribute: Show Chart
            RockMigrationHelper.DeleteAttribute( "B03F0821-699E-45D6-AB5C-50DFF3568D9F" );

            // Delete BlockType 
            //   Name: Step Program Detail
            //   Category: Steps
            //   Path: -
            //   EntityType: Step Program Detail
            RockMigrationHelper.DeleteBlockType( "E2F965D1-7419-4062-9568-08613BB696E3" );
        }
    }
}
