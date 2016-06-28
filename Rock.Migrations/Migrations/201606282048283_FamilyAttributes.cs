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
    public partial class FamilyAttributes : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddPage( "0B213645-FA4E-44A5-8E4C-B2D8EF054985", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Family Attributes", "", "59AB771D-194A-494E-87F5-7E00404C354A", "fa fa-list-ul", "FA2A1171-9308-41C7-948C-C9EBEA5BD668" ); // Site:Rock RMS
            // Add Block to Page: Family Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlock( "59AB771D-194A-494E-87F5-7E00404C354A", "", "E5EA2F6D-43A2-48E0-B59C-4409B78AC830", "Attributes", "Main", "", "", 0, "96A4C4C5-96F7-4ABE-946E-D552EEBD82ED" );
            // Attrib for BlockType: Attributes:Enable Ordering
            RockMigrationHelper.AddBlockTypeAttribute( "E5EA2F6D-43A2-48E0-B59C-4409B78AC830", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Ordering", "EnableOrdering", "", "Should the attributes be allowed to be sorted?", 3, @"False", "889B9C2B-7101-43BB-B3E2-EF82BD1CDAAF" );
            // Attrib Value for Block:Attributes, Attribute:Entity Qualifier Column Page: Family Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "96A4C4C5-96F7-4ABE-946E-D552EEBD82ED", "ECD5B86C-2B48-4548-9FE9-7AC6F6FA8106", @"GroupTypeId" );
            // Attrib Value for Block:Attributes, Attribute:Entity Qualifier Value Page: Family Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "96A4C4C5-96F7-4ABE-946E-D552EEBD82ED", "FCE1E87D-F816-4AD5-AE60-1E71942C547C", @"10" );
            // Attrib Value for Block:Attributes, Attribute:Entity Page: Family Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "96A4C4C5-96F7-4ABE-946E-D552EEBD82ED", "5B33FE25-6BF0-4890-91C6-49FB1629221E", @"9bbfda11-0d22-40d5-902f-60adfbc88987" );
            // Attrib Value for Block:Attributes, Attribute:Entity Id Page: Family Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "96A4C4C5-96F7-4ABE-946E-D552EEBD82ED", "CBB56D68-3727-42B9-BF13-0B2B593FB328", @"0" );
            // Attrib Value for Block:Attributes, Attribute:Allow Setting of Values Page: Family Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "96A4C4C5-96F7-4ABE-946E-D552EEBD82ED", "018C0016-C253-44E4-84DB-D166084C5CAD", @"False" );
            // Attrib Value for Block:Attributes, Attribute:Configure Type Page: Family Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "96A4C4C5-96F7-4ABE-946E-D552EEBD82ED", "D4132497-18BE-4D1F-8913-468E33DE63C4", @"True" );
            // Attrib Value for Block:Attributes, Attribute:Enable Show In Grid Page: Family Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "96A4C4C5-96F7-4ABE-946E-D552EEBD82ED", "920FE120-AD75-4D5C-BFE0-FA5745B1118B", @"True" );
            // Attrib Value for Block:Attributes, Attribute:Enable Ordering Page: Family Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "96A4C4C5-96F7-4ABE-946E-D552EEBD82ED", "889B9C2B-7101-43BB-B3E2-EF82BD1CDAAF", @"True" );

            // Make sure qualifier has correct id.
            Sql( @"
    DECLARE @FamilyGroupTypeId int = ( SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = '790E3215-3B10-442B-AF69-616C0DCB998E' )
    DECLARE @AttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'FCE1E87D-F816-4AD5-AE60-1E71942C547C' )
    DECLARE @BlockId int = ( SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = '96A4C4C5-96F7-4ABE-946E-D552EEBD82ED' )

    UPDATE [AttributeValue]
    SET [Value] = CAST( @FamilyGroupTypeId AS VARCHAR )
    WHERE [AttributeId] = @AttributeId
    AND [EntityId] = @BlockId
" );


        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteBlock( "96A4C4C5-96F7-4ABE-946E-D552EEBD82ED" );
            RockMigrationHelper.DeletePage( "59AB771D-194A-494E-87F5-7E00404C354A" ); //  Page: Family Attributes, Layout: Full Width, Site: Rock RMS

        }
    }
}
