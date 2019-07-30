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
    public partial class Rollup_1002 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddContentChannelItemAttributeCategoriesblockUp();
            UpdatespCheckin_BadgeAttendance();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddContentChannelItemAttributeCategoriesblockDown();
        }

        /// <summary>
        /// [MP] - Add Content Channel Item Attribute Categories block (Up)
        /// </summary>
        private void AddContentChannelItemAttributeCategoriesblockUp()
        {
            RockMigrationHelper.AddPage( true, "B4A24AB7-9369-4055-883F-4F4892C39AE3", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Content Channel Item Attribute Categories", "", "BBDE39C3-01C9-4C9E-9506-C2205508BC77", "fa fa-folder" ); // Site:Rock RMS
            // Add Block to Page: Content Channel Item Attribute Categories Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "BBDE39C3-01C9-4C9E-9506-C2205508BC77".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "620FC4A2-6587-409F-8972-22065919D9AC".AsGuid(), "Categories", "Main", @"", @"", 0, "3B05004B-0EF8-41A2-B5B8-058BEDB9B3AD" );
            // Attrib Value for Block:Categories, Attribute:Entity Type Page: Content Channel Item Attribute Categories, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3B05004B-0EF8-41A2-B5B8-058BEDB9B3AD", "C405A507-7889-4287-8342-105B89710044", @"5997c8d3-8840-4591-99a5-552919f90cbd" );
            // Attrib Value for Block:Categories, Attribute:Enable Hierarchy Page: Content Channel Item Attribute Categories, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3B05004B-0EF8-41A2-B5B8-058BEDB9B3AD", "736C3F4B-34CC-4B4B-9811-7171C82DDC41", @"False" );
            // Attrib Value for Block:Categories, Attribute:Entity Qualifier Column Page: Content Channel Item Attribute Categories, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3B05004B-0EF8-41A2-B5B8-058BEDB9B3AD", "65C4A655-6E1D-4504-838B-28B91FCC6DF8", @"EntityTypeId" );
            // Attrib Value for Block:Categories, Attribute:Entity Qualifier Value Page: Content Channel Item Attribute Categories, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3B05004B-0EF8-41A2-B5B8-058BEDB9B3AD", "19A79376-3F07-45E4-95CB-5AD5D3C4DDCF", @"" );
            // Set the Entity Qualifier Value to whatever the EntityTypeId of CONTENT_CHANNEL_ITEM is
            Sql( $@"UPDATE AttributeValue
                SET [Value] = (
                        SELECT TOP 1 Id
                        FROM EntityType
                        WHERE [Guid] = '{Rock.SystemGuid.EntityType.CONTENT_CHANNEL_ITEM}'
                        )
                WHERE AttributeId = (
                        SELECT TOP 1 ID
                        FROM Attribute
                        WHERE [Guid] = '19A79376-3F07-45E4-95CB-5AD5D3C4DDCF'
                        )
                    AND EntityId = (
                        SELECT TOP 1 ID
                        FROM [Block]
                        WHERE [Guid] = '3B05004B-0EF8-41A2-B5B8-058BEDB9B3AD'
                        )" );
            // Attrib Value for Block:Categories, Attribute:core.CustomGridColumnsConfig Page: Content Channel Item Attribute Categories, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3B05004B-0EF8-41A2-B5B8-058BEDB9B3AD", "65AA8E36-9BAB-4BB7-B48A-FE0BF8A30CE3", @"" );
            // Attrib Value for Block:Categories, Attribute:core.CustomGridEnableStickyHeaders Page: Content Channel Item Attribute Categories, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3B05004B-0EF8-41A2-B5B8-058BEDB9B3AD", "6D9CC876-720F-44C8-A95D-FEFC91DC7E85", @"" );
        }

        /// <summary>
        /// [MP] - Add Content Channel Item Attribute Categories block (Down)
        /// </summary>
        private void AddContentChannelItemAttributeCategoriesblockDown()
        {
            // Attrib for BlockType: Categories:core.CustomGridEnableStickyHeaders
            RockMigrationHelper.DeleteAttribute( "6D9CC876-720F-44C8-A95D-FEFC91DC7E85" );
            // Remove Block: Categories, from Page: Content Channel Item Attribute Categories, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "3B05004B-0EF8-41A2-B5B8-058BEDB9B3AD" );
            RockMigrationHelper.DeletePage( "BBDE39C3-01C9-4C9E-9506-C2205508BC77" ); //  Page: Content Channel Item Attribute Categories, Layout: Full Width, Site: Rock RMS
        }

        /// <summary>
        /// [ED] - spCheckin_BadgeAttendance
        /// </summary>
        private void UpdatespCheckin_BadgeAttendance()
        {
            Sql( MigrationSQL._201810021839212_Rollup_1002_spCheckin_BadgeAttendance );
        }
    }
}
