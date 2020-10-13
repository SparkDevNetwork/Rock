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
    public partial class AddWorkflowChangeLogNote : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateNoteType( "Workflow Type Change Log",
                "Rock.Model.WorkflowType",
                true,
                Rock.SystemGuid.NoteType.WORKFLOW_CHANGE_LOG_NOTE,
                true,
                "fa fa-gogs",
                true );

            AddNoteBlockToWorkflowConfiguration();
        }

        private void AddNoteBlockToWorkflowConfiguration()
        {
            // Add Block Change Log to Page: Workflow Configuration, Site: Rock RMS
            RockMigrationHelper.AddBlock( true,
                Rock.SystemGuid.Page.WORKFLOW_CONFIGURATION.AsGuid(),
                null,
                SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid(),
                SystemGuid.BlockType.NOTES.AsGuid(),
                "Change Log",
                "Main",
                string.Empty,
                string.Empty,
                2,
                SystemGuid.Block.WORKFLOW_CONFIGURATION_CHANGE_LOG_NOTES );


            // Block Attribute Value for Change Log ( Page: Workflow Configuration, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( SystemGuid.Block.WORKFLOW_CONFIGURATION_CHANGE_LOG_NOTES, "F1BCF615-FBCA-4BC2-A912-C35C0DC04174", SystemGuid.EntityType.WORKFLOW_TYPE );

            // Block Attribute Value for Change Log ( Page: Workflow Configuration, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( SystemGuid.Block.WORKFLOW_CONFIGURATION_CHANGE_LOG_NOTES, "D68EE1F5-D29F-404B-945D-AD0BE76594C3", @"False" );

            // Block Attribute Value for Change Log ( Page: Workflow Configuration, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( SystemGuid.Block.WORKFLOW_CONFIGURATION_CHANGE_LOG_NOTES, "00B6EBFF-786D-453E-8746-119D0B45CB3E", @"False" );

            // Block Attribute Value for Change Log ( Page: Workflow Configuration, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( SystemGuid.Block.WORKFLOW_CONFIGURATION_CHANGE_LOG_NOTES, "20243A98-4802-48E2-AF61-83956056AC65", @"False" );

            // Block Attribute Value for Change Log ( Page: Workflow Configuration, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( SystemGuid.Block.WORKFLOW_CONFIGURATION_CHANGE_LOG_NOTES, "3CB0A7DF-996B-4D6C-B3B6-9BBCC40BDC69", @"Change Log" );

            // Block Attribute Value for Change Log ( Page: Workflow Configuration, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( SystemGuid.Block.WORKFLOW_CONFIGURATION_CHANGE_LOG_NOTES, "B69937BE-000A-4B94-852F-16DE92344392", @"fa fa-sticky-note-o" );

            // Block Attribute Value for Change Log ( Page: Workflow Configuration, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( SystemGuid.Block.WORKFLOW_CONFIGURATION_CHANGE_LOG_NOTES, "FD0727DC-92F4-4765-82CB-3A08B7D864F8", @"Log Entry" );

            // Block Attribute Value for Change Log ( Page: Workflow Configuration, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( SystemGuid.Block.WORKFLOW_CONFIGURATION_CHANGE_LOG_NOTES, "5232BFAE-4DC8-4270-B38F-D29E1B00AB5E", @"Full" );

            // Block Attribute Value for Change Log ( Page: Workflow Configuration, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( SystemGuid.Block.WORKFLOW_CONFIGURATION_CHANGE_LOG_NOTES, "C05757C0-E83E-4170-8CBF-C4E1ABEC36E1", @"True" );

            // Block Attribute Value for Change Log ( Page: Workflow Configuration, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( SystemGuid.Block.WORKFLOW_CONFIGURATION_CHANGE_LOG_NOTES, "EB9CBD02-2B0F-4BA3-9112-BC73D54159E7", @"False" );

            // Block Attribute Value for Change Log ( Page: Workflow Configuration, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( SystemGuid.Block.WORKFLOW_CONFIGURATION_CHANGE_LOG_NOTES, "8E0BDD15-6B92-4BB0-9138-E9382B60F3A9", @"False" );

            // Block Attribute Value for Change Log ( Page: Workflow Configuration, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( SystemGuid.Block.WORKFLOW_CONFIGURATION_CHANGE_LOG_NOTES, "C9FC2C09-1BF5-4711-8F97-0B96633C46B1", @"Descending" );

            // Block Attribute Value for Change Log ( Page: Workflow Configuration, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( SystemGuid.Block.WORKFLOW_CONFIGURATION_CHANGE_LOG_NOTES, "6184511D-CC68-4FF2-90CB-3AD0AFD59D61", @"True" );

            // Block Attribute Value for Change Log ( Page: Workflow Configuration, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( SystemGuid.Block.WORKFLOW_CONFIGURATION_CHANGE_LOG_NOTES, "CB89C2A5-49DB-4108-B924-6C610CEDFBF4", SystemGuid.NoteType.WORKFLOW_CHANGE_LOG_NOTE );

            // Block Attribute Value for Change Log ( Page: Workflow Configuration, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( SystemGuid.Block.WORKFLOW_CONFIGURATION_CHANGE_LOG_NOTES, "C5FD0719-1E03-4C17-BE31-E02A3637C39A", @"False" );

            // Block Attribute Value for Change Log ( Page: Workflow Configuration, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( SystemGuid.Block.WORKFLOW_CONFIGURATION_CHANGE_LOG_NOTES, "328DDE3F-6FFF-4CA4-B6D0-C1BD4D643307", @"{% include '~~/Assets/Lava/NoteViewList.lava' %}" );

            // Block Attribute Value for Change Log ( Page: Workflow Configuration, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( SystemGuid.Block.WORKFLOW_CONFIGURATION_CHANGE_LOG_NOTES, "84E53A88-32D2-432C-8BB5-600BDBA10949", @"False" );

            // Add/Update PageContext for Page:Workflow Configuration, Entity: Rock.Model.WorkflowType, Parameter: workflowTypeId
            RockMigrationHelper.UpdatePageContext( SystemGuid.Page.WORKFLOW_CONFIGURATION, "Rock.Model.WorkflowType", "workflowTypeId", "B316CA90-0482-458B-BC5D-7E64FCB60A9D" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( $"DELETE [NoteType] WHERE [Guid] = '{Rock.SystemGuid.NoteType.WORKFLOW_CHANGE_LOG_NOTE}'" );
            RemoveNoteBlockFromWorkflowConfiguration();
        }

        private void RemoveNoteBlockFromWorkflowConfiguration()
        {

            RockMigrationHelper.DeleteBlockAttributeValue( SystemGuid.Block.WORKFLOW_CONFIGURATION_CHANGE_LOG_NOTES, "F1BCF615-FBCA-4BC2-A912-C35C0DC04174" );

            // Block Attribute Value for Change Log ( Page: Workflow Configuration, Site: Rock RMS )
            RockMigrationHelper.DeleteBlockAttributeValue( SystemGuid.Block.WORKFLOW_CONFIGURATION_CHANGE_LOG_NOTES, "D68EE1F5-D29F-404B-945D-AD0BE76594C3" );

            // Block Attribute Value for Change Log ( Page: Workflow Configuration, Site: Rock RMS )
            RockMigrationHelper.DeleteBlockAttributeValue( SystemGuid.Block.WORKFLOW_CONFIGURATION_CHANGE_LOG_NOTES, "00B6EBFF-786D-453E-8746-119D0B45CB3E" );

            // Block Attribute Value for Change Log ( Page: Workflow Configuration, Site: Rock RMS )
            RockMigrationHelper.DeleteBlockAttributeValue( SystemGuid.Block.WORKFLOW_CONFIGURATION_CHANGE_LOG_NOTES, "20243A98-4802-48E2-AF61-83956056AC65" );

            // Block Attribute Value for Change Log ( Page: Workflow Configuration, Site: Rock RMS )
            RockMigrationHelper.DeleteBlockAttributeValue( SystemGuid.Block.WORKFLOW_CONFIGURATION_CHANGE_LOG_NOTES, "3CB0A7DF-996B-4D6C-B3B6-9BBCC40BDC69" );

            // Block Attribute Value for Change Log ( Page: Workflow Configuration, Site: Rock RMS )
            RockMigrationHelper.DeleteBlockAttributeValue( SystemGuid.Block.WORKFLOW_CONFIGURATION_CHANGE_LOG_NOTES, "B69937BE-000A-4B94-852F-16DE92344392" );

            // Block Attribute Value for Change Log ( Page: Workflow Configuration, Site: Rock RMS )
            RockMigrationHelper.DeleteBlockAttributeValue( SystemGuid.Block.WORKFLOW_CONFIGURATION_CHANGE_LOG_NOTES, "FD0727DC-92F4-4765-82CB-3A08B7D864F8" );

            // Block Attribute Value for Change Log ( Page: Workflow Configuration, Site: Rock RMS )
            RockMigrationHelper.DeleteBlockAttributeValue( SystemGuid.Block.WORKFLOW_CONFIGURATION_CHANGE_LOG_NOTES, "5232BFAE-4DC8-4270-B38F-D29E1B00AB5E" );

            // Block Attribute Value for Change Log ( Page: Workflow Configuration, Site: Rock RMS )
            RockMigrationHelper.DeleteBlockAttributeValue( SystemGuid.Block.WORKFLOW_CONFIGURATION_CHANGE_LOG_NOTES, "C05757C0-E83E-4170-8CBF-C4E1ABEC36E1" );

            // Block Attribute Value for Change Log ( Page: Workflow Configuration, Site: Rock RMS )
            RockMigrationHelper.DeleteBlockAttributeValue( SystemGuid.Block.WORKFLOW_CONFIGURATION_CHANGE_LOG_NOTES, "EB9CBD02-2B0F-4BA3-9112-BC73D54159E7" );

            // Block Attribute Value for Change Log ( Page: Workflow Configuration, Site: Rock RMS )
            RockMigrationHelper.DeleteBlockAttributeValue( SystemGuid.Block.WORKFLOW_CONFIGURATION_CHANGE_LOG_NOTES, "8E0BDD15-6B92-4BB0-9138-E9382B60F3A9" );

            // Block Attribute Value for Change Log ( Page: Workflow Configuration, Site: Rock RMS )
            RockMigrationHelper.DeleteBlockAttributeValue( SystemGuid.Block.WORKFLOW_CONFIGURATION_CHANGE_LOG_NOTES, "C9FC2C09-1BF5-4711-8F97-0B96633C46B1" );

            // Block Attribute Value for Change Log ( Page: Workflow Configuration, Site: Rock RMS )
            RockMigrationHelper.DeleteBlockAttributeValue( SystemGuid.Block.WORKFLOW_CONFIGURATION_CHANGE_LOG_NOTES, "6184511D-CC68-4FF2-90CB-3AD0AFD59D61" );

            // Block Attribute Value for Change Log ( Page: Workflow Configuration, Site: Rock RMS )
            RockMigrationHelper.DeleteBlockAttributeValue( SystemGuid.Block.WORKFLOW_CONFIGURATION_CHANGE_LOG_NOTES, "CB89C2A5-49DB-4108-B924-6C610CEDFBF4" );

            // Block Attribute Value for Change Log ( Page: Workflow Configuration, Site: Rock RMS )
            RockMigrationHelper.DeleteBlockAttributeValue( SystemGuid.Block.WORKFLOW_CONFIGURATION_CHANGE_LOG_NOTES, "C5FD0719-1E03-4C17-BE31-E02A3637C39A" );

            // Block Attribute Value for Change Log ( Page: Workflow Configuration, Site: Rock RMS )
            RockMigrationHelper.DeleteBlockAttributeValue( SystemGuid.Block.WORKFLOW_CONFIGURATION_CHANGE_LOG_NOTES, "328DDE3F-6FFF-4CA4-B6D0-C1BD4D643307" );

            // Block Attribute Value for Change Log ( Page: Workflow Configuration, Site: Rock RMS )
            RockMigrationHelper.DeleteBlockAttributeValue( SystemGuid.Block.WORKFLOW_CONFIGURATION_CHANGE_LOG_NOTES, "84E53A88-32D2-432C-8BB5-600BDBA10949" );

            // Remove Block: Change Log, from Page: Workflow Configuration, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( SystemGuid.Block.WORKFLOW_CONFIGURATION_CHANGE_LOG_NOTES );

            // Delete PageContext for Page:Workflow Configuration, Entity: Rock.Model.WorkflowType, Parameter: workflowTypeId
            RockMigrationHelper.DeletePageContext( "B316CA90-0482-458B-BC5D-7E64FCB60A9D" );

        }
    }
}
