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
using Rock.Checkr.Constants;
using Rock.Plugin;
using Rock.SystemGuid;

namespace Rock.Migrations
{
    [MigrationNumber( 2, "1.8.0" )]
    public partial class Checkr_CreatePages : Migration
    {
        /// <summary>
        /// The new Protect My Ministry workflow name
        /// </summary>
        public const string NEW_PMM_WORKFLOW_TYPE_NAME = "Background Check (PMM)";

        /// <summary>
        /// Makes the Checkr the default workflow action in Bio block.
        /// </summary>
        public void MakeCheckrDefaultWorkflowAction()
        {
            // Remove Checkr background check workflow from bio
            RockMigrationHelper.DeleteBlockAttributeValue( Block.BIO, SystemGuid.Attribute.BIO_WORKFLOWACTION, WorkflowType.PROTECTMYMINISTRY );

            // Add PMM background check workflow to bio
            RockMigrationHelper.AddBlockAttributeValue( Block.BIO, SystemGuid.Attribute.BIO_WORKFLOWACTION, CheckrSystemGuid.CHECKR_WORKFLOW_TYPE, appendToExisting: true );
            // Sql( string.Format( "UPDATE [dbo].[WorkflowType] SET [Name] = '{0}' WHERE [Guid] = '{1}'", NEW_PMM_WORKFLOW_TYPE_NAME, PMM_WORKFLOW_TYPE ) );
            Sql( string.Format( "UPDATE [dbo].[WorkflowType] SET [Name] = '{0}' WHERE [Guid] = '{1}'", CheckrConstants.CHECKR_WORKFLOW_TYPE_NAME, CheckrSystemGuid.CHECKR_WORKFLOW_TYPE ) );
        }

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddPage( "C831428A-6ACD-4D49-9B2D-046D399E3123", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Checkr", "", CheckrSystemGuid.CHECKR_PAGE, "fa fa-shield", "E7F4B733-60FF-4FA3-AB17-0832E123F6F2" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Checkr Settings", "Block for updating the settings used by the Checkr integration.", "~/Blocks/Security/BackgroundCheck/CheckrSettings.ascx", "Security  > Background Check", CheckrSystemGuid.CHECKR_SETTINGS_BLOCKTYPE );
            RockMigrationHelper.UpdateBlockType( "Checkr Request List", "Lists all the Checkr background check requests.", "~/Blocks/Security/BackgroundCheck/CheckrRequestList.ascx", "Security > Background Check", CheckrSystemGuid.CHECKR_REQUESTLIST_BLOCKTYPE );
            RockMigrationHelper.AddBlock( CheckrSystemGuid.CHECKR_PAGE, "", CheckrSystemGuid.CHECKR_SETTINGS_BLOCKTYPE, "Checkr Settings", "Main", "", "", 0, CheckrSystemGuid.CHECKR_SETTINGS_BLOCK );
            RockMigrationHelper.AddBlock( CheckrSystemGuid.CHECKR_PAGE, "", CheckrSystemGuid.CHECKR_REQUESTLIST_BLOCKTYPE, "Checkr Request List", "Main", "", "", 1, CheckrSystemGuid.CHECKR_REQUESTLIST_BLOCK );

            // Attrib for BlockType: Request List:Workflow Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( CheckrSystemGuid.CHECKR_REQUESTLIST_BLOCKTYPE, FieldType.PAGE_REFERENCE, "Workflow Detail Page", "WorkflowDetailPage", "", "The page to view details about the background check workflow", 0, @"", CheckrSystemGuid.CHECKR_REQUESTLIST_WORKFLOWDETAILPAGE_ATTRIBUTE );

            // Attrib Value for Block:Request List, Attribute:Workflow Detail Page Page: Protect My Ministry, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( CheckrSystemGuid.CHECKR_REQUESTLIST_WORKFLOWDETAILPAGE_ATTRIBUTE, "EBD0D19C-E73D-41AE-82D4-C89C21C35998", Rock.SystemGuid.Page.WORKFLOW_DETAIL );

            int count = (int)SqlScalar( "SELECT COUNT(Id) FROM [dbo].[BackgroundCheck]" );
            if ( count == 0 )
            {
                MakeCheckrDefaultWorkflowAction();
            }
            else
            {
                // Do nothing if PMM have been used.
            }
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteBlockAttributeValue( Block.BIO, CheckrSystemGuid.CHECKR_REQUESTLIST_WORKFLOWDETAILPAGE_ATTRIBUTE );
            RockMigrationHelper.DeleteBlockType( CheckrSystemGuid.CHECKR_SETTINGS_BLOCKTYPE );
            RockMigrationHelper.DeleteBlockType( CheckrSystemGuid.CHECKR_REQUESTLIST_BLOCKTYPE );
            RockMigrationHelper.DeleteBlock( CheckrSystemGuid.CHECKR_SETTINGS_BLOCK );
            RockMigrationHelper.DeleteBlock( CheckrSystemGuid.CHECKR_REQUESTLIST_BLOCK );
            RockMigrationHelper.DeletePage( CheckrSystemGuid.CHECKR_PAGE );
        }
    }
}