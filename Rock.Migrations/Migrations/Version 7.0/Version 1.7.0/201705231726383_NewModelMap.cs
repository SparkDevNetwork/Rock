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
    public partial class NewModelMap : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddPage( "7F1F4130-CB98-473B-9DE1-7A886D2283ED", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Model Map", "", "67DBC902-BCD5-449E-8A1F-888A3CF9875E", "fa fa-object-ungroup" ); // Site:Rock RMS
            // Add Block to Page: Model Map, Site: Rock RMS
            RockMigrationHelper.AddBlock( "67DBC902-BCD5-449E-8A1F-888A3CF9875E", "", "DA2AAD13-209B-4885-8739-B7BE99F6510D", "Model Map", "Main", @"", @"", 0, "2583DE89-F028-4ACE-9E1F-2873340726AC" );
            // Attrib for BlockType: Model Map:Category Icons
            RockMigrationHelper.UpdateBlockTypeAttribute( "DA2AAD13-209B-4885-8739-B7BE99F6510D", "73B02051-0D38-4AD9-BF81-A2D477DE4F70", "Category Icons", "CategoryIcons", "", "The Icon Class to use for each category.", 0, @"", "75A0C88F-7F5B-48A2-88A4-C3A62F0EDF9A" );
            // Attrib Value for Block:Model Map, Attribute:Category Icons Page: Model Map, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "2583DE89-F028-4ACE-9E1F-2873340726AC", "75A0C88F-7F5B-48A2-88A4-C3A62F0EDF9A", @"CMS^fa fa-code|Communication^fa fa-comment|Connection^fa fa-plug|Core^fa fa-gear|Event^fa fa-clipboard|Finance^fa fa-money|Group^fa fa-users|Prayer^fa fa-cloud-upload|Reporting^fa fa-list-alt|Workflow^fa fa-gears|Other^fa fa-question-circle|CRM^fa fa-user" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Model Map:Category Icons
            RockMigrationHelper.DeleteAttribute( "75A0C88F-7F5B-48A2-88A4-C3A62F0EDF9A" );
            // Remove Block: Model Map, from Page: Model Map, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "2583DE89-F028-4ACE-9E1F-2873340726AC" );
            RockMigrationHelper.DeletePage( "67DBC902-BCD5-449E-8A1F-888A3CF9875E" ); //  Page: Model Map, Layout: Full Width, Site: Rock RMS
        }
    }
}
