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
    public partial class GridLaunchWorkflows : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddPage( true, "6510AB6B-DFB4-4DBF-9F0F-7EA598E4AC54", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Workflow Launch", "", "DCD0E0A3-F115-4609-860F-B5BF678AA41E", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPageRoute( "DCD0E0A3-F115-4609-860F-B5BF678AA41E", "LaunchWorkflows/{EntitySetId}", "D497A92F-AA0E-40BF-997F-D512FF0357CE" );// for Page:Workflow Launch
            RockMigrationHelper.UpdateBlockType( "Workflow Launch", "Block that enables previewing an entity set and then launching a workflow for each item within the set.", "~/Blocks/WorkFlow/WorkflowLaunch.ascx", "Workflow", "D7C15C1B-7487-42C3-A485-AD154F46558A" );
            
            // Add Block to Page: Workflow Launch Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "DCD0E0A3-F115-4609-860F-B5BF678AA41E".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "D7C15C1B-7487-42C3-A485-AD154F46558A".AsGuid(), "Workflow Launch", "Main", @"", @"", 0, "5AAAB4FD-6A07-47BD-A08E-F7DB68DD7F33" );
            
            // Attrib for BlockType: Workflow Launch:Default Number of Items to Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D7C15C1B-7487-42C3-A485-AD154F46558A", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Default Number of Items to Show", "DefaultNumberOfItemsToShow", "Default Number of Items to Show", @"The number of entities to list on screen before summarizing ('...and xx more').", 5, @"50", "FF5A5181-F48E-4D75-B0FA-0CC626FDBBEE" );
            // Attrib for BlockType: Workflow Launch:Workflow Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D7C15C1B-7487-42C3-A485-AD154F46558A", "1D50399B-0D6E-480B-A71A-E7BD37DD83F0", "Workflow Types", "WorkflowTypes", "Workflow Types", @"Only the selected workflow types will be shown. If left blank, any workflow type can be launched.", 1, @"", "EAD99415-02EA-4D67-BC8B-1B7E186D6057" );
            // Attrib for BlockType: Workflow Launch:Allow Multiple Workflow Launches
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D7C15C1B-7487-42C3-A485-AD154F46558A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Multiple Workflow Launches", "AllowMultipleWorkflowLaunches", "Allow Multiple Workflow Launches", @"If set to yes, allows launching multiple different types of workflows. After one is launched, the block will allow the individual to select another type to be launched. This will only show if more than one type is configured.", 2, @"True", "A9F130BF-F793-4755-BAC2-8B685E1176B0" );
            // Attrib for BlockType: Workflow Launch:Panel Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D7C15C1B-7487-42C3-A485-AD154F46558A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Panel Title", "PanelTitle", "Panel Title", @"The title to display in the block panel.", 3, @"Workflow Launch", "C291BDE3-5689-45E6-B092-87EEF0782ABE" );
            // Attrib for BlockType: Workflow Launch:Panel Title Icon CSS Class
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D7C15C1B-7487-42C3-A485-AD154F46558A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Panel Title Icon CSS Class", "PanelIcon", "Panel Title Icon CSS Class", @"The icon to use before the panel title.", 4, @"fa fa-cog", "8EF7E051-F5ED-4C6B-A314-04FE8D97CC89" );
            
            // Attrib Value for Block:Workflow Launch, Attribute:Default Number of Items to Show Page: Workflow Launch, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5AAAB4FD-6A07-47BD-A08E-F7DB68DD7F33", "FF5A5181-F48E-4D75-B0FA-0CC626FDBBEE", @"" );
            // Attrib Value for Block:Workflow Launch, Attribute:Workflow Types Page: Workflow Launch, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5AAAB4FD-6A07-47BD-A08E-F7DB68DD7F33", "EAD99415-02EA-4D67-BC8B-1B7E186D6057", @"" );
            // Attrib Value for Block:Workflow Launch, Attribute:Allow Multiple Workflow Launches Page: Workflow Launch, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5AAAB4FD-6A07-47BD-A08E-F7DB68DD7F33", "A9F130BF-F793-4755-BAC2-8B685E1176B0", @"True" );
            // Attrib Value for Block:Workflow Launch, Attribute:Panel Title Page: Workflow Launch, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5AAAB4FD-6A07-47BD-A08E-F7DB68DD7F33", "C291BDE3-5689-45E6-B092-87EEF0782ABE", @"" );
            // Attrib Value for Block:Workflow Launch, Attribute:Panel Title Icon CSS Class Page: Workflow Launch, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5AAAB4FD-6A07-47BD-A08E-F7DB68DD7F33", "8EF7E051-F5ED-4C6B-A314-04FE8D97CC89", @"" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Workflow Launch:Panel Title Icon CSS Class
            RockMigrationHelper.DeleteAttribute( "8EF7E051-F5ED-4C6B-A314-04FE8D97CC89" );
            // Attrib for BlockType: Workflow Launch:Panel Title
            RockMigrationHelper.DeleteAttribute( "C291BDE3-5689-45E6-B092-87EEF0782ABE" );
            // Attrib for BlockType: Workflow Launch:Allow Multiple Workflow Launches
            RockMigrationHelper.DeleteAttribute( "A9F130BF-F793-4755-BAC2-8B685E1176B0" );
            // Attrib for BlockType: Workflow Launch:Workflow Types
            RockMigrationHelper.DeleteAttribute( "EAD99415-02EA-4D67-BC8B-1B7E186D6057" );
            // Attrib for BlockType: Workflow Launch:Default Number of Items to Show
            RockMigrationHelper.DeleteAttribute( "FF5A5181-F48E-4D75-B0FA-0CC626FDBBEE" );

            // Remove Block: Workflow Launch, from Page: Workflow Launch, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "5AAAB4FD-6A07-47BD-A08E-F7DB68DD7F33" );
            RockMigrationHelper.DeleteBlockType( "D7C15C1B-7487-42C3-A485-AD154F46558A" ); // Workflow Launch
            RockMigrationHelper.DeletePage( "DCD0E0A3-F115-4609-860F-B5BF678AA41E" ); //  Page: Workflow Launch, Layout: Full Width, Site: Rock RMS
        }
    }
}
