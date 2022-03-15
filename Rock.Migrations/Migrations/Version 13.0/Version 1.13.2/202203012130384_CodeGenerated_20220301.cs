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
    public partial class CodeGenerated_20220301 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
           // Add/Update BlockType 
           //   Name: System Communication Preview
           //   Category: Communication
           //   Path: ~/Blocks/Communication/CommunicationJobPreview.ascx
           //   EntityType: -
            RockMigrationHelper.UpdateBlockType("System Communication Preview","Create a preview and send a test message for the given system communication using the selected date and target person.","~/Blocks/Communication/CommunicationJobPreview.ascx","Communication","95366DA1-D878-4A9A-A26F-83160DBE784F");

            // Add/Update Obsidian Block Type
            //   Name:Attributes
            //   Category:Obsidian > Core
            //   EntityType:Rock.Blocks.Core.Attributes
            RockMigrationHelper.UpdateMobileBlockType("Attributes", "Allows for the managing of attributes.", "Rock.Blocks.Core.Attributes", "Obsidian > Core", "AD2AF533-CB26-4566-A537-62B84D5AFD4D");

            // Add/Update Obsidian Block Type
            //   Name:Form Builder Detail
            //   Category:Obsidian > Workflow > FormBuilder
            //   EntityType:Rock.Blocks.Workflow.FormBuilder.FormBuilderDetail
            RockMigrationHelper.UpdateMobileBlockType("Form Builder Detail", "Edits the details of a workflow Form Builder action.", "Rock.Blocks.Workflow.FormBuilder.FormBuilderDetail", "Obsidian > Workflow > FormBuilder", "A61C5E3C-2267-4CF7-B305-D8AF0DB9660B");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD2AF533-CB26-4566-A537-62B84D5AFD4D", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity", "Entity", "Entity", @"Entity Name", 0, @"", "5B8E3B95-C266-4608-86B3-75209B751192" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Qualifier Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD2AF533-CB26-4566-A537-62B84D5AFD4D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Entity Qualifier Column", "EntityQualifierColumn", "Entity Qualifier Column", @"The entity column to evaluate when determining if this attribute applies to the entity", 1, @"", "9DDFF152-B5E8-4E49-87DD-C1F02E4C6000" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Qualifier Value
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD2AF533-CB26-4566-A537-62B84D5AFD4D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Entity Qualifier Value", "EntityQualifierValue", "Entity Qualifier Value", @"The entity column value to evaluate.  Attributes will only apply to entities with this value", 2, @"", "557F8462-F8B9-4F25-8CF2-D52274A2B82E" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Allow Setting of Values
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD2AF533-CB26-4566-A537-62B84D5AFD4D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Setting of Values", "AllowSettingofValues", "Allow Setting of Values", @"Should UI be available for setting values of the specified Entity ID?", 3, @"false", "6EE1AE8C-67D8-449B-89CD-849E9BD820F9" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Id
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD2AF533-CB26-4566-A537-62B84D5AFD4D", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Entity Id", "EntityId", "Entity Id", @"The entity id that values apply to", 4, @"0", "3BB54764-C133-47C8-A0C0-C6BE1022FC42" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Enable Show In Grid
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD2AF533-CB26-4566-A537-62B84D5AFD4D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Show In Grid", "EnableShowInGrid", "Enable Show In Grid", @"Should the 'Show In Grid' option be displayed when editing attributes?", 5, @"false", "2056841F-7C35-4525-83F7-32DABBAAFD1A" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Category Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD2AF533-CB26-4566-A537-62B84D5AFD4D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Category Filter", "CategoryFilter", "Category Filter", @"A comma separated list of category GUIDs to limit the display of attributes to.", 6, @"", "E735E03E-8A88-4FFB-A6C4-753B13571523" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Hide Columns on Grid
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD2AF533-CB26-4566-A537-62B84D5AFD4D", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Hide Columns on Grid", "HideColumnsOnGrid", "Hide Columns on Grid", @"The grid columns that should be hidden.", 7, @"", "F0404477-FB90-4AC7-BC04-45E0212CB9A9" );

            // Attribute for BlockType
            //   BlockType: Form Builder Detail
            //   Category: Obsidian > Workflow > FormBuilder
            //   Attribute: Submissions Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A61C5E3C-2267-4CF7-B305-D8AF0DB9660B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Submissions Page", "SubmissionsPage", "Submissions Page", @"The page that contains the Submissions block to view submissions existing submissions for this form.", 0, @"", "E2B744B3-183D-458B-9080-1FA3A978EEE3" );

            // Attribute for BlockType
            //   BlockType: Form Builder Detail
            //   Category: Obsidian > Workflow > FormBuilder
            //   Attribute: Analytics Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A61C5E3C-2267-4CF7-B305-D8AF0DB9660B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Analytics Page", "AnalyticsPage", "Analytics Page", @"The page that contains the Analytics block to view statistics on existing submissions for this form.", 1, @"", "8F77C336-27F9-47EA-840D-A397982DD205" );

            // Attribute for BlockType
            //   BlockType: System Communication Preview
            //   Category: Communication
            //   Attribute: System Communication
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "95366DA1-D878-4A9A-A26F-83160DBE784F", "72ED40C7-4D64-4D60-9411-4FFB2B9E833E", "System Communication", "SystemCommunication", "System Communication", @"The system communication to use when previewing the message. When set as a block setting, it will not allow overriding by the query string", 0, @"", "E12A8468-C5B9-432E-97BA-77B3149E51E9" );

            // Attribute for BlockType
            //   BlockType: System Communication Preview
            //   Category: Communication
            //   Attribute: Send Day of the Week
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "95366DA1-D878-4A9A-A26F-83160DBE784F", "08943FF9-F2A8-4DB4-A72A-31938B200C8C", "Send Day of the Week", "SendDaysOfTheWeek", "Send Day of the Week", @"Used to determine which dates to list in the Message Date drop down. <i><strong>Note:</strong>If no day is selected the Message Date drop down control will not be shown</i>", 1, @"", "B99537B1-7DBD-4A38-A9E2-026A692C2C5C" );

            // Attribute for BlockType
            //   BlockType: System Communication Preview
            //   Category: Communication
            //   Attribute: Number of Previous Weeks to Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "95366DA1-D878-4A9A-A26F-83160DBE784F", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Number of Previous Weeks to Show", "PreviousWeeksToShow", "Number of Previous Weeks to Show", @"How many previous weeks to show in the drop down.", 3, @"6", "5E7D4C97-C6B3-4AC4-8ED3-84530A7D4ED1" );

            // Attribute for BlockType
            //   BlockType: System Communication Preview
            //   Category: Communication
            //   Attribute: Number of Future Weeks to Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "95366DA1-D878-4A9A-A26F-83160DBE784F", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Number of Future Weeks to Show", "FutureWeeksToShow", "Number of Future Weeks to Show", @"How many weeks ahead to show in the drop down.", 4, @"1", "92CC3216-3BD6-47F2-B3B6-D9A6937F4ECB" );

            // Attribute for BlockType
            //   BlockType: System Communication Preview
            //   Category: Communication
            //   Attribute: Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "95366DA1-D878-4A9A-A26F-83160DBE784F", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled.", 5, @"", "5166DDE7-0598-485F-AF20-4E1BF70F81A8" );

            // Attribute for BlockType
            //   BlockType: Related Entity List
            //   Category: Core
            //   Attribute: Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "28516B18-7423-4A97-9223-B97537BD0F79", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block.", 6, @"", "0E05D219-67A9-46C7-B519-3A06525E3DAB" );

            // Attribute for BlockType
            //   BlockType: Transaction Entry (V2)
            //   Category: Finance
            //   Attribute: Fee Coverage Message
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Fee Coverage Message", "FeeCoverageMessage", "Fee Coverage Message", @"The Lava template to use to provide the cover the fees prompt to the individual. <span class='tip tip-lava'></span>", 28, @"Make my gift go further. Please increase my gift by {{ Percentage }}% (${{ CalculatedAmount }}) to help cover the electronic transaction fees.", "6A757838-D924-4966-BF58-6542FC78FBA2" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Transaction Entry (V2)
            //   Category: Finance
            //   Attribute: Fee Coverage Message
            RockMigrationHelper.DeleteAttribute("6A757838-D924-4966-BF58-6542FC78FBA2");

            // Attribute for BlockType
            //   BlockType: Related Entity List
            //   Category: Core
            //   Attribute: Enabled Lava Commands
            RockMigrationHelper.DeleteAttribute("0E05D219-67A9-46C7-B519-3A06525E3DAB");

            // Attribute for BlockType
            //   BlockType: System Communication Preview
            //   Category: Communication
            //   Attribute: Enabled Lava Commands
            RockMigrationHelper.DeleteAttribute("5166DDE7-0598-485F-AF20-4E1BF70F81A8");

            // Attribute for BlockType
            //   BlockType: System Communication Preview
            //   Category: Communication
            //   Attribute: Number of Future Weeks to Show
            RockMigrationHelper.DeleteAttribute("92CC3216-3BD6-47F2-B3B6-D9A6937F4ECB");

            // Attribute for BlockType
            //   BlockType: System Communication Preview
            //   Category: Communication
            //   Attribute: Number of Previous Weeks to Show
            RockMigrationHelper.DeleteAttribute("5E7D4C97-C6B3-4AC4-8ED3-84530A7D4ED1");

            // Attribute for BlockType
            //   BlockType: System Communication Preview
            //   Category: Communication
            //   Attribute: Send Day of the Week
            RockMigrationHelper.DeleteAttribute("B99537B1-7DBD-4A38-A9E2-026A692C2C5C");

            // Attribute for BlockType
            //   BlockType: System Communication Preview
            //   Category: Communication
            //   Attribute: System Communication
            RockMigrationHelper.DeleteAttribute("E12A8468-C5B9-432E-97BA-77B3149E51E9");

            // Attribute for BlockType
            //   BlockType: Form Builder Detail
            //   Category: Obsidian > Workflow > FormBuilder
            //   Attribute: Analytics Page
            RockMigrationHelper.DeleteAttribute("8F77C336-27F9-47EA-840D-A397982DD205");

            // Attribute for BlockType
            //   BlockType: Form Builder Detail
            //   Category: Obsidian > Workflow > FormBuilder
            //   Attribute: Submissions Page
            RockMigrationHelper.DeleteAttribute("E2B744B3-183D-458B-9080-1FA3A978EEE3");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Hide Columns on Grid
            RockMigrationHelper.DeleteAttribute("F0404477-FB90-4AC7-BC04-45E0212CB9A9");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Category Filter
            RockMigrationHelper.DeleteAttribute("E735E03E-8A88-4FFB-A6C4-753B13571523");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Enable Show In Grid
            RockMigrationHelper.DeleteAttribute("2056841F-7C35-4525-83F7-32DABBAAFD1A");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Id
            RockMigrationHelper.DeleteAttribute("3BB54764-C133-47C8-A0C0-C6BE1022FC42");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Allow Setting of Values
            RockMigrationHelper.DeleteAttribute("6EE1AE8C-67D8-449B-89CD-849E9BD820F9");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Qualifier Value
            RockMigrationHelper.DeleteAttribute("557F8462-F8B9-4F25-8CF2-D52274A2B82E");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Qualifier Column
            RockMigrationHelper.DeleteAttribute("9DDFF152-B5E8-4E49-87DD-C1F02E4C6000");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity
            RockMigrationHelper.DeleteAttribute("5B8E3B95-C266-4608-86B3-75209B751192");

            // Delete BlockType 
            //   Name: System Communication Preview
            //   Category: Communication
            //   Path: ~/Blocks/Communication/CommunicationJobPreview.ascx
            //   EntityType: -
            RockMigrationHelper.DeleteBlockType("95366DA1-D878-4A9A-A26F-83160DBE784F");

            // Delete BlockType 
            //   Name: Form Template Detail
            //   Category: Workflow > FormBuilder
            //   Path: -
            //   EntityType: Form Template Detail
            RockMigrationHelper.DeleteBlockType("CA7A698B-2256-4F00-B0F3-E5A0396A5F81");

            // Delete BlockType 
            //   Name: Form Builder Detail
            //   Category: Obsidian > Workflow > FormBuilder
            //   Path: -
            //   EntityType: Form Builder Detail
            RockMigrationHelper.DeleteBlockType("A61C5E3C-2267-4CF7-B305-D8AF0DB9660B");

            // Delete BlockType 
            //   Name: Attributes
            //   Category: Obsidian > Core
            //   Path: -
            //   EntityType: Attributes
            RockMigrationHelper.DeleteBlockType("AD2AF533-CB26-4566-A537-62B84D5AFD4D");
        }
    }
}
