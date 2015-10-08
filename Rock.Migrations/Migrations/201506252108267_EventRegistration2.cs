// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class EventRegistration2 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attrib for BlockType: Group Detail:Registration Instance Page
            RockMigrationHelper.AddBlockTypeAttribute( "582BEEA1-5B27-444D-BC0A-F60CEB053981", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Registration Instance Page", "RegistrationInstancePage", "", "The page to display registration details.", 7, @"", "36643FFE-C49F-443E-8C3D-E83324A45822" );
            // Attrib for BlockType: Registration Instance Detail:Group Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "22B67EDB-6D13-4D29-B722-DF45367AA3CB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Detail Page", "GroupDetailPage", "", "The page for viewing details about a group", 4, @"", "A70FDBD7-54F1-45E6-909A-621609FCA0E0" );
            // Attrib for BlockType: Registration Instance Detail:Calendar Item Page
            RockMigrationHelper.AddBlockTypeAttribute( "22B67EDB-6D13-4D29-B722-DF45367AA3CB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Calendar Item Page", "CalendarItemPage", "", "The page to view calendar item details", 3, @"", "EE4B4850-F076-4928-BE23-39464FB07B7D" );
            // Attrib for BlockType: Calendar Item Campus List:Group Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "94230E7A-8EB7-4407-9B8E-888B54C71E39", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Detail Page", "GroupDetailPage", "", "The page for viewing details about a group", 2, @"", "0C606E72-EFC6-49C0-87E1-60C309E39E3B" );
            // Attrib for BlockType: Calendar Item Campus List:Registration Instance Page
            RockMigrationHelper.AddBlockTypeAttribute( "94230E7A-8EB7-4407-9B8E-888B54C71E39", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Registration Instance Page", "RegistrationInstancePage", "", "The page to view registration details", 1, @"", "A289C30C-0112-4477-A287-0B2BE17F179C" );

            // Attrib Value for Block:GroupDetailRight, Attribute:Registration Instance Page Page: Group Viewer, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "88344FE3-737E-4741-A38D-D2D3A1653818", "36643FFE-C49F-443E-8C3D-E83324A45822", @"844dc54b-daec-47b3-a63a-712dd6d57793" );
            // Attrib Value for Block:Calendar Item Campus List, Attribute:Group Detail Page Page: Calendar Item, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "828C8FE3-D5F8-4C22-BA81-844D704842EA", "0C606E72-EFC6-49C0-87E1-60C309E39E3B", @"4e237286-b715-4109-a578-c1445ec02707" );
            // Attrib Value for Block:Calendar Item Campus List, Attribute:Registration Instance Page Page: Calendar Item, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "828C8FE3-D5F8-4C22-BA81-844D704842EA", "A289C30C-0112-4477-A287-0B2BE17F179C", @"844dc54b-daec-47b3-a63a-712dd6d57793" );
            // Attrib Value for Block:Registration Instance Detail, Attribute:Group Detail Page Page: Registration Instance, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5F44A3A8-500B-4C89-95CA-8C4246B53C3F", "A70FDBD7-54F1-45E6-909A-621609FCA0E0", @"4e237286-b715-4109-a578-c1445ec02707" );
            // Attrib Value for Block:Registration Instance Detail, Attribute:Calendar Item Page Page: Registration Instance, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5F44A3A8-500B-4C89-95CA-8C4246B53C3F", "EE4B4850-F076-4928-BE23-39464FB07B7D", @"7fb33834-f40a-4221-8849-bb8c06903b04" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Calendar Item Campus List:Registration Instance Page
            RockMigrationHelper.DeleteAttribute( "A289C30C-0112-4477-A287-0B2BE17F179C" );
            // Attrib for BlockType: Calendar Item Campus List:Group Detail Page
            RockMigrationHelper.DeleteAttribute( "0C606E72-EFC6-49C0-87E1-60C309E39E3B" );
            // Attrib for BlockType: Registration Instance Detail:Calendar Item Page
            RockMigrationHelper.DeleteAttribute( "EE4B4850-F076-4928-BE23-39464FB07B7D" );
            // Attrib for BlockType: Registration Instance Detail:Group Detail Page
            RockMigrationHelper.DeleteAttribute( "A70FDBD7-54F1-45E6-909A-621609FCA0E0" );
            // Attrib for BlockType: Group Detail:Registration Instance Page
            RockMigrationHelper.DeleteAttribute( "36643FFE-C49F-443E-8C3D-E83324A45822" );
        }
    }
}
