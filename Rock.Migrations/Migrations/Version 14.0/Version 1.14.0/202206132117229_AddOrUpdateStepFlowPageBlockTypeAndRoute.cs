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
    public partial class AddOrUpdateStepFlowPageBlockTypeAndRoute : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Engagement.Steps.StepFlow", SystemGuid.EntityType.STEP_FLOW, false, true );

            // Add Page - Internal Name: Step Flow - Site: Rock RMS
            RockMigrationHelper.AddPage( true, SystemGuid.Page.STEP_PROGRAM_DETAIL, SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE, "Step Flow", string.Empty, SystemGuid.Page.STEP_FLOW, "fa-project-diagram" );

            // Add/Update BlockType - Name: Step Flow - Category: Steps - Path: - EntityType: -
            RockMigrationHelper.UpdateMobileBlockType( "Step Flow", "Show the flow of individuals as they move through different steps in a given step program.", "Rock.Blocks.Engagement.Steps.StepFlow", "Steps", SystemGuid.BlockType.STEP_FLOW );
#pragma warning disable CS0618 // Type or member is obsolete
            // Add Page Route - Page:Step Flow - Route: steps/program/{ProgramId}/flow
            RockMigrationHelper.AddPageRoute( SystemGuid.Page.STEP_FLOW, "steps/program/{ProgramId}/flow", SystemGuid.PageRoute.STEP_FLOW );
#pragma warning restore CS0618 // Type or member is obsolete
            
            // Add Block - Block Name: Step Flow - Page Name: Step Flow Layout: - Site: Rock RMS
            RockMigrationHelper.AddBlock( true, SystemGuid.Page.STEP_FLOW.AsGuid(), null, SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid(), SystemGuid.BlockType.STEP_FLOW.AsGuid(), "Step Flow", "Main", @"", @"", 0, SystemGuid.Block.STEP_FLOW );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove Block - Name: Step Flow, from Page: Step Flow, Site: Rock RMS - from Page: Step Flow, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( SystemGuid.Block.STEP_FLOW );
            
            // Delete BlockType - Name: Step Flow - Category: Steps - Path: - EntityType: -
            RockMigrationHelper.DeleteBlockType( SystemGuid.BlockType.STEP_FLOW );
            
            // Delete Page Internal Name: Step Flow Site: Rock RMS Layout: Full Width
            RockMigrationHelper.DeletePage( SystemGuid.Page.STEP_FLOW );
        }
    }
}
