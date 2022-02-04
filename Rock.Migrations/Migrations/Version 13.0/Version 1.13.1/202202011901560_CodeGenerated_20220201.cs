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
    public partial class CodeGenerated_20220201 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
           // Add/Update BlockType 
           //   Name: Remote Authentication
           //   Category: TV > TV Apps
           //   Path: ~/Blocks/Tv/RemoteAuthentication.ascx
           //   EntityType: -
            RockMigrationHelper.UpdateBlockType("Remote Authentication","Authenticates an individual for a remote system.","~/Blocks/Tv/RemoteAuthentication.ascx","TV > TV Apps","3080C707-4594-4DDD-95B5-DEF82141DE6A");

            // Add/Update Obsidian Block Type
            //   Name:Attributes
            //   Category:Obsidian > Core
            //   EntityType:Rock.Blocks.Core.Attributes
            RockMigrationHelper.UpdateMobileBlockType("Attributes", "Allows for the managing of attributes.", "Rock.Blocks.Core.Attributes", "Obsidian > Core", "F0D01746-8CFF-48E0-8A62-030305191FB6");

            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: Disable Location Services
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FA4D15E6-4C85-4247-A374-5E592E711CFD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Disable Location Services", "DisableLocationServices", "Disable Location Services", @"If disabled, the mobile device’s location services will not be used and instead a list of active campuses will be shown. The selected campus will be used to find a matching device from the Devices block setting.", 0, @"False", "E478D72F-3D00-4190-82C9-66306DF851DE" );

            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: No Campuses Found <span class='tip tip-lava'></span>
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FA4D15E6-4C85-4247-A374-5E592E711CFD", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "No Campuses Found <span class='tip tip-lava'></span>", "NoCampusesFoundTemplate", "No Campuses Found <span class='tip tip-lava'></span>", @"", 9, @"Hi {{ CurrentPerson.NickName }}! There are currently no active campuses ready for check-in at this time.", "B885B4E1-45A4-41D5-8B5C-909A94E270BD" );

            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: Log In Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FA4D15E6-4C85-4247-A374-5E592E711CFD", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Log In Page", "LogInPage", "Log In Page", @"The page to use for logging in the person. If blank the log in button will not be shown", 100, @"", "5380F54F-F857-4D40-8B55-0058A1CF6028" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F0D01746-8CFF-48E0-8A62-030305191FB6", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity", "Entity", "Entity", @"Entity Name", 0, @"", "A58DE6F0-FA5D-438B-BBA9-09B95CE12038" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Qualifier Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F0D01746-8CFF-48E0-8A62-030305191FB6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Entity Qualifier Column", "EntityQualifierColumn", "Entity Qualifier Column", @"The entity column to evaluate when determining if this attribute applies to the entity", 1, @"", "8AAEBF8E-8BA8-4F25-8CFF-703E8446F516" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Qualifier Value
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F0D01746-8CFF-48E0-8A62-030305191FB6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Entity Qualifier Value", "EntityQualifierValue", "Entity Qualifier Value", @"The entity column value to evaluate.  Attributes will only apply to entities with this value", 2, @"", "4331180D-D601-4413-844B-3906817B3649" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Allow Setting of Values
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F0D01746-8CFF-48E0-8A62-030305191FB6", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Setting of Values", "AllowSettingofValues", "Allow Setting of Values", @"Should UI be available for setting values of the specified Entity ID?", 3, @"false", "4863908A-0996-473A-B28E-DEEA63772875" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Id
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F0D01746-8CFF-48E0-8A62-030305191FB6", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Entity Id", "EntityId", "Entity Id", @"The entity id that values apply to", 4, @"0", "A1212642-EF59-4B88-8406-090833AA39CC" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Enable Show In Grid
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F0D01746-8CFF-48E0-8A62-030305191FB6", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Show In Grid", "EnableShowInGrid", "Enable Show In Grid", @"Should the 'Show In Grid' option be displayed when editing attributes?", 5, @"false", "19FA9A3E-7D28-498E-8E9B-901865CA40F6" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Category Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F0D01746-8CFF-48E0-8A62-030305191FB6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Category Filter", "CategoryFilter", "Category Filter", @"A comma separated list of category GUIDs to limit the display of attributes to.", 6, @"", "5963F489-2EE7-4680-B840-9B414F82DA66" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Hide Columns on Grid
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F0D01746-8CFF-48E0-8A62-030305191FB6", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Hide Columns on Grid", "HideColumnsOnGrid", "Hide Columns on Grid", @"The grid columns that should be hidden.", 7, @"", "AA085A78-2A16-4CBF-BB2C-9BE773761245" );

            // Attribute for BlockType
            //   BlockType: Remote Authentication
            //   Category: TV > TV Apps
            //   Attribute: Site
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3080C707-4594-4DDD-95B5-DEF82141DE6A", "BB7AB90F-4DE9-4804-A852-F5593A35C8A0", "Site", "Site", "Site", @"The optional site that the remote authentication is tied to.", 0, @"", "AA1AF180-D327-4A87-A42B-0298FF9C8E7F" );

            // Attribute for BlockType
            //   BlockType: Remote Authentication
            //   Category: TV > TV Apps
            //   Attribute: Header Content
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3080C707-4594-4DDD-95B5-DEF82141DE6A", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Header Content", "HeaderContent", "Header Content", @"Lava template to create the header.", 1, @"default", "E5F4964B-548F-4892-9369-9FE586127308" );

            // Attribute for BlockType
            //   BlockType: Remote Authentication
            //   Category: TV > TV Apps
            //   Attribute: Footer Content
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3080C707-4594-4DDD-95B5-DEF82141DE6A", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Footer Content", "FooterContent", "Footer Content", @"Lava template to create the footer.", 2, @"", "90868508-5115-49E2-A25C-3680E887F68F" );

            // Attribute for BlockType
            //   BlockType: Remote Authentication
            //   Category: TV > TV Apps
            //   Attribute: Success Message
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3080C707-4594-4DDD-95B5-DEF82141DE6A", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Success Message", "SuccessMessage", "Success Message", @"Lava template that will be displayed after a successful authentication.", 3, @"default", "4B6C8CC9-D6B1-4516-9762-06EB744E656C" );

            // Attribute for BlockType
            //   BlockType: Remote Authentication
            //   Category: TV > TV Apps
            //   Attribute: Code Expiration Duration
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3080C707-4594-4DDD-95B5-DEF82141DE6A", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Code Expiration Duration", "CodeExpirationDuration", "Code Expiration Duration", @"The length of time in minutes that a code is good for.", 4, @"10", "38AA78BA-9063-4E1A-85BB-33CC618AFD79" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Remote Authentication
            //   Category: TV > TV Apps
            //   Attribute: Code Expiration Duration
            RockMigrationHelper.DeleteAttribute("38AA78BA-9063-4E1A-85BB-33CC618AFD79");

            // Attribute for BlockType
            //   BlockType: Remote Authentication
            //   Category: TV > TV Apps
            //   Attribute: Success Message
            RockMigrationHelper.DeleteAttribute("4B6C8CC9-D6B1-4516-9762-06EB744E656C");

            // Attribute for BlockType
            //   BlockType: Remote Authentication
            //   Category: TV > TV Apps
            //   Attribute: Footer Content
            RockMigrationHelper.DeleteAttribute("90868508-5115-49E2-A25C-3680E887F68F");

            // Attribute for BlockType
            //   BlockType: Remote Authentication
            //   Category: TV > TV Apps
            //   Attribute: Header Content
            RockMigrationHelper.DeleteAttribute("E5F4964B-548F-4892-9369-9FE586127308");

            // Attribute for BlockType
            //   BlockType: Remote Authentication
            //   Category: TV > TV Apps
            //   Attribute: Site
            RockMigrationHelper.DeleteAttribute("AA1AF180-D327-4A87-A42B-0298FF9C8E7F");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Hide Columns on Grid
            RockMigrationHelper.DeleteAttribute("AA085A78-2A16-4CBF-BB2C-9BE773761245");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Category Filter
            RockMigrationHelper.DeleteAttribute("5963F489-2EE7-4680-B840-9B414F82DA66");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Enable Show In Grid
            RockMigrationHelper.DeleteAttribute("19FA9A3E-7D28-498E-8E9B-901865CA40F6");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Id
            RockMigrationHelper.DeleteAttribute("A1212642-EF59-4B88-8406-090833AA39CC");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Allow Setting of Values
            RockMigrationHelper.DeleteAttribute("4863908A-0996-473A-B28E-DEEA63772875");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Qualifier Value
            RockMigrationHelper.DeleteAttribute("4331180D-D601-4413-844B-3906817B3649");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Qualifier Column
            RockMigrationHelper.DeleteAttribute("8AAEBF8E-8BA8-4F25-8CFF-703E8446F516");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity
            RockMigrationHelper.DeleteAttribute("A58DE6F0-FA5D-438B-BBA9-09B95CE12038");

            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: Log In Page
            RockMigrationHelper.DeleteAttribute("5380F54F-F857-4D40-8B55-0058A1CF6028");

            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: No Campuses Found <span class='tip tip-lava'></span>
            RockMigrationHelper.DeleteAttribute("B885B4E1-45A4-41D5-8B5C-909A94E270BD");

            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: Disable Location Services
            RockMigrationHelper.DeleteAttribute("E478D72F-3D00-4190-82C9-66306DF851DE");

            // Delete BlockType 
            //   Name: Remote Authentication
            //   Category: TV > TV Apps
            //   Path: ~/Blocks/Tv/RemoteAuthentication.ascx
            //   EntityType: -
            RockMigrationHelper.DeleteBlockType("3080C707-4594-4DDD-95B5-DEF82141DE6A");

            // Delete BlockType 
            //   Name: Attributes
            //   Category: Obsidian > Core
            //   Path: -
            //   EntityType: Attributes
            RockMigrationHelper.DeleteBlockType("F0D01746-8CFF-48E0-8A62-030305191FB6");
        }
    }
}
