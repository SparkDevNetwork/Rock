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
    using Rock.SystemGuid;

    /// <summary>
    /// NOTE: ContentChannelViewDetail got renamed to ContentChannelItemView in v9.0
    /// </summary>
    public partial class ContentChannelViewDetail : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            #region Add ContentChannelViewDetail blocktype
            // Attrib Value for BlockType: Content Channel View Detail
            RockMigrationHelper.AddBlockType( "Content Channel View Detail", "Block to display a specific content channel item.", "~/Blocks/Cms/ContentChannelViewDetail.ascx", "CMS", "63659EBE-C5AF-4157-804A-55C7D565110E" );

            // Attrib Value for BlockType: Content Channel View Detail
            RockMigrationHelper.AddBlockTypeAttribute( "63659EBE-C5AF-4157-804A-55C7D565110E", FieldType.WORKFLOW_TYPE, "Workflow Type", "WorkflowType", "", @"The workflow type to launch when the content is viewed.", 0, @"", "61361765-4762-4017-A58D-6CFCDD3CADC1", false );
            // Attrib Value for BlockType: Content Channel View Detail
            RockMigrationHelper.AddBlockTypeAttribute( "63659EBE-C5AF-4157-804A-55C7D565110E", FieldType.TEXT, "Twitter Title Attribute", "TwitterTitleAttribute", "", @"", 0, @"", "CE43C275-44CA-4DA6-92CB-FAAFB1F886CF", false );
            // Attrib Value for BlockType: Content Channel View Detail
            RockMigrationHelper.AddBlockTypeAttribute( "63659EBE-C5AF-4157-804A-55C7D565110E", FieldType.TEXT, "Twitter Description Attribute", "TwitterDescriptionAttribute", "", @"", 0, @"", "32DE419C-062E-45FE-9BBE-CAE104A11491", false );
            // Attrib Value for BlockType: Content Channel View Detail
            RockMigrationHelper.AddBlockTypeAttribute( "63659EBE-C5AF-4157-804A-55C7D565110E", FieldType.TEXT, "Twitter Card", "TwitterCard", "", @"", 0, @"none", "D0C4618E-1F92-4107-A22F-8D638FD73E19", false );
            // Attrib Value for BlockType: Content Channel View Detail
            RockMigrationHelper.AddBlockTypeAttribute( "63659EBE-C5AF-4157-804A-55C7D565110E", FieldType.TEXT, "Twitter Image Attribute", "TwitterImageAttribute", "", @"", 0, @"", "4CEFDE01-A056-4DBE-BEC2-979DCE0F4D39", false );
            // Attrib Value for BlockType: Content Channel View Detail
            RockMigrationHelper.AddBlockTypeAttribute( "63659EBE-C5AF-4157-804A-55C7D565110E", FieldType.LAVA_COMMANDS, "Enabled Lava Commands", "EnabledLavaCommands", "", @"The Lava commands that should be enabled for this content channel item block.", 0, @"", "8E741F29-A5D1-433B-A520-25C65B349216", false );
            // Attrib Value for BlockType: Content Channel View Detail
            RockMigrationHelper.AddBlockTypeAttribute( "63659EBE-C5AF-4157-804A-55C7D565110E", FieldType.CONTENT_CHANNEL, "Content Channel", "ContentChannel", "", @"Limits content channel items to a specific channel, or leave blank to leave unrestricted.", 0, @"", "E8921151-6392-4FFD-A1F4-67A6AAD69776", false );
            // Attrib Value for BlockType: Content Channel View Detail
            RockMigrationHelper.AddBlockTypeAttribute( "63659EBE-C5AF-4157-804A-55C7D565110E", FieldType.TEXT, "Content Channel Query Parameter", "ContentChannelQueryParameter", "", @"
Specify the URL parameter to use to determine which Content Channel Item to show, or leave blank to use whatever the first parameter is. 
The type of the value will determine how the content channel item will be determined as follows:

Integer - ContentChannelItem Id
String - ContentChannelItem Slug
Guid - ContentChannelItem Guid

", 0, @"", "39CC148D-B905-4560-96DD-C5151DC344DE", false );
            // Attrib Value for BlockType: Content Channel View Detail
            RockMigrationHelper.AddBlockTypeAttribute( "63659EBE-C5AF-4157-804A-55C7D565110E", FieldType.CODE_EDITOR, "Lava Template", "LavaTemplate", "", @"The template to use when formatting the content channel item.", 0, @"
<h1>{{ Item.Title }}</h1>
{{ Item.Content }}", "47C56661-FB70-4703-9781-8651B8B49485", false );
            // Attrib Value for BlockType: Content Channel View Detail
            RockMigrationHelper.AddBlockTypeAttribute( "63659EBE-C5AF-4157-804A-55C7D565110E", FieldType.INTEGER, "Output Cache Duration", "OutputCacheDuration", "", @"Number of seconds to cache the resolved output. Only cache the output if you are not personalizing the output based on current user, current page, or any other merge field value.", 0, @"", "7A9CBC44-FF60-464D-983A-61BD009F9C95", false );
            // Attrib Value for BlockType: Content Channel View Detail
            RockMigrationHelper.AddBlockTypeAttribute( "63659EBE-C5AF-4157-804A-55C7D565110E", FieldType.BOOLEAN, "Set Page Title", "SetPageTitle", "", @"Determines if the block should set the page title with the channel name or content item.", 0, @"False", "406D4BB0-9BE3-4047-99C9-EAB5592B0942", false );
            // Attrib Value for BlockType: Content Channel View Detail
            RockMigrationHelper.AddBlockTypeAttribute( "63659EBE-C5AF-4157-804A-55C7D565110E", FieldType.BOOLEAN, "Log Interactions", "LogInteractions", "", @"", 0, @"False", "3503170E-DD5E-4F51-9699-DCEA80C8C64C", false );
            // Attrib Value for BlockType: Content Channel View Detail
            RockMigrationHelper.AddBlockTypeAttribute( "63659EBE-C5AF-4157-804A-55C7D565110E", FieldType.BOOLEAN, "Write Interaction Only If Individual Logged In", "WriteInteractionOnlyIfIndividualLoggedIn", "", @"Set to true to only write interactions for logged in users, or set to false to write interactions for both logged in and anonymous users.", 0, @"False", "63B254F7-E19C-48FD-A93F-AFEE19C1ED21", false );
            // Attrib Value for BlockType: Content Channel View Detail
            RockMigrationHelper.AddBlockTypeAttribute( "63659EBE-C5AF-4157-804A-55C7D565110E", FieldType.BOOLEAN, "Launch Workflow Only If Individual Logged In", "LaunchWorkflowOnlyIfIndividualLoggedIn", "", @"Set to true to only launch a workflow for logged in users, or set to false to launch for both logged in and anonymous users.", 0, @"False", "EB298724-07D5-42AF-B4BF-82420AF6A657", false );
            // Attrib Value for BlockType: Content Channel View Detail
            RockMigrationHelper.AddBlockTypeAttribute( "63659EBE-C5AF-4157-804A-55C7D565110E", FieldType.SINGLE_SELECT, "Launch Workflow Condition", "LaunchWorkflowCondition", "", @"", 0, @"1", "E5EFC23D-E030-496C-A9A4-D2BF4181CB49", false );
            // Attrib Value for BlockType: Content Channel View Detail
            RockMigrationHelper.AddBlockTypeAttribute( "63659EBE-C5AF-4157-804A-55C7D565110E", FieldType.TEXT, "Meta Description Attribute", "MetaDescriptionAttribute", "", @"", 0, @"", "DEECC65E-97B6-42B9-A1EF-9FD0CCD25CC0", false );
            // Attrib Value for BlockType: Content Channel View Detail
            RockMigrationHelper.AddBlockTypeAttribute( "63659EBE-C5AF-4157-804A-55C7D565110E", FieldType.TEXT, "Open Graph Type", "OpenGraphType", "", @"", 0, @"", "07C9EB02-98CF-4BB5-9C0A-6D616E35C8F0", false );
            // Attrib Value for BlockType: Content Channel View Detail
            RockMigrationHelper.AddBlockTypeAttribute( "63659EBE-C5AF-4157-804A-55C7D565110E", FieldType.TEXT, "Open Graph Title Attribute", "OpenGraphTitleAttribute", "", @"", 0, @"", "BF87BEEA-B1FB-40F1-AA29-1F214057BCE3", false );
            // Attrib Value for BlockType: Content Channel View Detail
            RockMigrationHelper.AddBlockTypeAttribute( "63659EBE-C5AF-4157-804A-55C7D565110E", FieldType.TEXT, "Open Graph Description Attribute", "OpenGraphDescriptionAttribute", "", @"", 0, @"", "7690BF49-E522-4A30-947A-0C410C4A7B42", false );
            // Attrib Value for BlockType: Content Channel View Detail
            RockMigrationHelper.AddBlockTypeAttribute( "63659EBE-C5AF-4157-804A-55C7D565110E", FieldType.TEXT, "Open Graph Image Attribute", "OpenGraphImageAttribute", "", @"", 0, @"", "EC5EFDA3-5F6D-46F9-830B-D1D0B3577AB0", false );

            #endregion

            Sql( MigrationSQL._201806111950515_ContentChannelViewDetail );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteBlockType( "63659ebe-c5af-4157-804a-55c7d565110e" );
            RockMigrationHelper.DeleteBlock( "0C828414-8AEF-4B43-AAEF-B200544A2197" );
            RockMigrationHelper.DeleteBlock( "70DCBB50-0978-4B24-9382-CDD9BEED5ADB" );
            RockMigrationHelper.DeleteBlock( "71D998C7-9F27-4B8A-937A-64C5EFC4783A" );
            RockMigrationHelper.DeleteBlock( "847E12E0-A7FC-4BD5-BD7E-1E9D435510E7" );            
        }
    }
}
