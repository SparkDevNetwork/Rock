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
    public partial class LabelEditor : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddPage( "B565FDF8-959F-4AC8-ACDF-3B1B5CFE79F5", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Edit Label", "", "15D3766A-6026-4F29-B5C6-5944204642F3", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Edit Label", "Allows editing contents of a label and printing test labels.", "~/Blocks/CheckIn/Config/EditLabel.ascx", "Check-in > Configuration", "5ACB281A-CE85-426F-92A6-771F3B8AEF8A" );
            // Add Block to Page: Edit Label, Site: Rock RMS
            RockMigrationHelper.AddBlock( "15D3766A-6026-4F29-B5C6-5944204642F3", "", "5ACB281A-CE85-426F-92A6-771F3B8AEF8A", "Edit Label", "Main", "", "", 0, "29C9C9B7-B175-47F8-9649-A0BB84D64624" );
            // Attrib for BlockType: Binary File Detail:Edit Label Page
            RockMigrationHelper.AddBlockTypeAttribute( "B97B2E51-5C9C-459B-999F-C7797DAD8B69", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Edit Label Page", "EditLabelPage", "", "Page used to edit and test the contents of a label file.", 0, @"", "DCDDDCD1-09B6-46F5-A596-8261613D0A50" );
            // Attrib Value for Block:Binary File Detail, Attribute:Edit Label Page Page: Check-in Label, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "F52CEDB1-F822-485C-9A1C-BA6D05383FAA", "DCDDDCD1-09B6-46F5-A596-8261613D0A50", @"15d3766a-6026-4f29-b5c6-5944204642f3" );

            //**********************
            //* New Action Type
            //**********************
            // Set Group Attribute
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SetGroupAttribute", "F4F59DF6-2F6A-40A0-9BFD-8427F7DD3D87", false, true );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "F4F59DF6-2F6A-40A0-9BFD-8427F7DD3D87", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "8D294784-52DD-4371-9076-6BAE9BA3872B" ); // Rock.Workflow.Action.SetGroupAttribute:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "F4F59DF6-2F6A-40A0-9BFD-8427F7DD3D87", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Group", "Group", "The attribute containing the group whose attribute will be set.", 0, @"", "BC0FF490-BA02-4B6A-948D-5DA253942083" ); // Rock.Workflow.Action.SetGroupAttribute:Group
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "F4F59DF6-2F6A-40A0-9BFD-8427F7DD3D87", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Text Value|Attribute Value", "AttributeValue", "The text or attribute to set the value from. <span class='tip tip-lava'></span>", 4, @"", "44B658CC-304E-41D9-A69C-B1029D07E21F" ); // Rock.Workflow.Action.SetGroupAttribute:Text Value|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "F4F59DF6-2F6A-40A0-9BFD-8427F7DD3D87", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Group Attribute Key", "GroupAttributeKey", "The attribute key to use for the group attribute.", 2, @"", "7A395334-BC94-486C-9E32-7E72A74D5F5B" ); // Rock.Workflow.Action.SetGroupAttribute:Group Attribute Key
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "F4F59DF6-2F6A-40A0-9BFD-8427F7DD3D87", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "F93AE95D-6A1B-4E6D-AB82-ADDB4F82CF3C" ); // Rock.Workflow.Action.SetGroupAttribute:Order
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Binary File Detail:Edit Label Page
            RockMigrationHelper.DeleteAttribute( "DCDDDCD1-09B6-46F5-A596-8261613D0A50" );
            RockMigrationHelper.DeleteBlock( "29C9C9B7-B175-47F8-9649-A0BB84D64624" );
            RockMigrationHelper.DeleteBlockType( "5ACB281A-CE85-426F-92A6-771F3B8AEF8A" ); // Edit Label
            RockMigrationHelper.DeletePage( "15D3766A-6026-4F29-B5C6-5944204642F3" ); //  Page: Edit Label, Layout: Full Width, Site: Rock RMS        }
        }
    }
}
