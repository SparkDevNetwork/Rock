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
    public partial class AddGenericExternalWorkflowPageBlockAndRoutes : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddGenericExternalWorkflowPageBlock_Up();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddGenericExternalWorkflowPageBlock_Down();
        }

        private void AddGenericExternalWorkflowPageBlock_Up()
        {
            RockMigrationHelper.AddPage( true, SystemGuid.Page.SUPPORT_PAGES_EXTERNAL_SITE, SystemGuid.Layout.FULL_WIDTH, "Generic Workflow Entry", string.Empty, SystemGuid.Page.EXTERNAL_WORKFLOW_ENTRY, "" );
#pragma warning disable CS0618 // Type or member is obsolete
            RockMigrationHelper.AddPageRoute( SystemGuid.Page.EXTERNAL_WORKFLOW_ENTRY, "WorkflowEntry/{WorkflowTypeGuid}/{WorkflowGuid}", SystemGuid.PageRoute.EXTERNAL_WORKFLOW_ENTRY_WITH_WORKFLOW );
            RockMigrationHelper.AddPageRoute( SystemGuid.Page.EXTERNAL_WORKFLOW_ENTRY, "WorkflowEntry/{WorkflowTypeGuid}", SystemGuid.PageRoute.EXTERNAL_WORKFLOW_ENTRY );
#pragma warning restore CS0618 // Type or member is obsolete

            RockMigrationHelper.AddBlock( true, SystemGuid.Page.EXTERNAL_WORKFLOW_ENTRY.AsGuid(), null, SystemGuid.Site.EXTERNAL_SITE.AsGuid(), SystemGuid.BlockType.WORKFLOW_ENTRY.AsGuid(), "Generic Workflow Entry", "Main", @"", @"", 0, SystemGuid.Block.EXTERNAL_WORKFLOW_ENTRY );

            RockMigrationHelper.AddBlockAttributeValue( SystemGuid.Block.EXTERNAL_WORKFLOW_ENTRY, SystemGuid.Attribute.WORKFLOW_ENTRY_BLOCK_DISABLE_PASSING_WORKFLOWID, "True" );
            RockMigrationHelper.AddBlockAttributeValue( SystemGuid.Block.EXTERNAL_WORKFLOW_ENTRY, SystemGuid.Attribute.WORKFLOW_ENTRY_BLOCK_DISABLE_PASSING_WORKFLOWTYPEID, "True" );
        }

        private void AddGenericExternalWorkflowPageBlock_Down()
        {
            RockMigrationHelper.DeleteBlockAttributeValue( SystemGuid.Block.EXTERNAL_WORKFLOW_ENTRY, SystemGuid.Attribute.WORKFLOW_ENTRY_BLOCK_DISABLE_PASSING_WORKFLOWTYPEID );
            RockMigrationHelper.DeleteBlockAttributeValue( SystemGuid.Block.EXTERNAL_WORKFLOW_ENTRY, SystemGuid.Attribute.WORKFLOW_ENTRY_BLOCK_DISABLE_PASSING_WORKFLOWID );
            RockMigrationHelper.DeleteBlock( SystemGuid.Block.EXTERNAL_WORKFLOW_ENTRY );
            RockMigrationHelper.DeletePageRoute( SystemGuid.PageRoute.EXTERNAL_WORKFLOW_ENTRY_WITH_WORKFLOW );
            RockMigrationHelper.DeletePageRoute( SystemGuid.PageRoute.EXTERNAL_WORKFLOW_ENTRY );
            RockMigrationHelper.DeletePage( SystemGuid.Page.EXTERNAL_WORKFLOW_ENTRY );
        }
    }
}
