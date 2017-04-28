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
    public partial class WorkflowWebhookDefinedType : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddDefinedType( "Global", "Workflow Webhook", "Webhook to activate the Workflow", SystemGuid.DefinedType.WEBHOOK_TO_WORKFLOW );
            RockMigrationHelper.AddDefinedTypeAttribute( SystemGuid.DefinedType.WEBHOOK_TO_WORKFLOW, "c0d0d7e2-c3b0-4004-abea-4bbfad10d5d2", "Url", "Url", "Request Url to activate the Workflow", 1, "", "5d9ac172-0d2f-495e-b435-de1a010acdbf" );
            RockMigrationHelper.AddDefinedTypeAttribute( SystemGuid.DefinedType.WEBHOOK_TO_WORKFLOW, "9c204cd0-1233-41c5-818a-c5da439445aa", "Method", "Method", "Http Method Type allowed for the request", 2, "", "2d1693db-52aa-453c-b111-ebe9ca225317" );
            RockMigrationHelper.AddDefinedTypeAttribute( SystemGuid.DefinedType.WEBHOOK_TO_WORKFLOW, "1edafded-dfe6-4334-b019-6eecba89e05a", "Headers", "Headers", "Check for Headers", 3, @"False", "c1a5ca29-863a-4639-8e1f-301903016d52" );
            RockMigrationHelper.AddDefinedTypeAttribute( SystemGuid.DefinedType.WEBHOOK_TO_WORKFLOW, "1edafded-dfe6-4334-b019-6eecba89e05a", "Cookies", "Cookies", "Check for Cookies", 4, @"False", "0ec3cd49-db5d-4a7e-ab57-1a4ab68bf9c8" );
            RockMigrationHelper.AddDefinedTypeAttribute( SystemGuid.DefinedType.WEBHOOK_TO_WORKFLOW, "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow Type", "WorkflowType", "The type of workflow to launch.", 5, "", "307b0e9c-bd7b-4ca2-9616-254850736fb9" );
            RockMigrationHelper.AddDefinedTypeAttribute( SystemGuid.DefinedType.WEBHOOK_TO_WORKFLOW, "9C204CD0-1233-41C5-818A-C5DA439445AA", "Workflow Name Template", "WorkflowNameTemplate", "The lava template to use for setting the workflow name. See the defined type's help text for a listing of merge fields. <span class='tip tip-lava'></span>", 6, "", "399b3d34-8273-497c-8fb7-cb137dadd6c3" );
            RockMigrationHelper.AddDefinedTypeAttribute( SystemGuid.DefinedType.WEBHOOK_TO_WORKFLOW, "73b02051-0d38-4ad9-bf81-a2d477de4f70", "Workflow Attributes", "WorkflowAttributes", "Key/value list of workflow attributes to set with the given lava merge template. See the defined type’s help text for a listing of merge fields. <span class='tip tip-lava'></span>", 7, "", "c75b0443-de6c-4791-ab4a-018429a807c8" );
            RockMigrationHelper.AddAttributeQualifier( "c75b0443-de6c-4791-ab4a-018429a807c8", "keyprompt", "Attribute Key", "c2c293f5-30c4-48e1-b31b-12612d7e4cb1" );
            RockMigrationHelper.AddAttributeQualifier( "c75b0443-de6c-4791-ab4a-018429a807c8", "valueprompt", "Merge Template", "79137493-d103-4368-a2e8-942e70ee2c5c" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
