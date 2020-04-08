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
namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 24, "1.6.3" )]
    public class PrayerRequestAttributes : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Moved to core migration: 201705011932514_CampusOrder
            //RockMigrationHelper.AddPage( true, "1A3437C8-D4CB-4329-A366-8D6A4CBF79BF", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Prayer Request Attributes", "", "C39C3E88-F423-424D-AA21-EB5CA7871A7B", "fa fa-list-ul" ); // Site:Rock RMS
            //// Add Block to Page: Prayer Request Attributes, Site: Rock RMS
            //RockMigrationHelper.AddBlock( true, "C39C3E88-F423-424D-AA21-EB5CA7871A7B", "", "E5EA2F6D-43A2-48E0-B59C-4409B78AC830", "Prayer Request Attributes", "Main", @"", @"", 0, "B6BFDEBD-BBB5-4EC8-A0B2-EB2AEC119FEF" );
            
            //// Attrib Value for Block:Prayer Request Attributes, Attribute:Enable Show In Grid Page: Prayer Request Attributes, Site: Rock RMS
            //RockMigrationHelper.AddBlockAttributeValue( true, "B6BFDEBD-BBB5-4EC8-A0B2-EB2AEC119FEF", "920FE120-AD75-4D5C-BFE0-FA5745B1118B", @"True" );
            //// Attrib Value for Block:Prayer Request Attributes, Attribute:Enable Ordering Page: Prayer Request Attributes, Site: Rock RMS
            //RockMigrationHelper.AddBlockAttributeValue( true, "B6BFDEBD-BBB5-4EC8-A0B2-EB2AEC119FEF", "5372DFB0-1884-49CD-BB62-BFFCCE33DF86", @"True" );
            //// Attrib Value for Block:Prayer Request Attributes, Attribute:Entity Qualifier Column Page: Prayer Request Attributes, Site: Rock RMS
            //RockMigrationHelper.AddBlockAttributeValue( true, "B6BFDEBD-BBB5-4EC8-A0B2-EB2AEC119FEF", "ECD5B86C-2B48-4548-9FE9-7AC6F6FA8106", @"" );
            //// Attrib Value for Block:Prayer Request Attributes, Attribute:Entity Qualifier Value Page: Prayer Request Attributes, Site: Rock RMS
            //RockMigrationHelper.AddBlockAttributeValue( true, "B6BFDEBD-BBB5-4EC8-A0B2-EB2AEC119FEF", "FCE1E87D-F816-4AD5-AE60-1E71942C547C", @"" );
            //// Attrib Value for Block:Prayer Request Attributes, Attribute:Entity Page: Prayer Request Attributes, Site: Rock RMS
            //RockMigrationHelper.AddBlockAttributeValue( true, "B6BFDEBD-BBB5-4EC8-A0B2-EB2AEC119FEF", "5B33FE25-6BF0-4890-91C6-49FB1629221E", @"f13c8fd2-7702-4c79-a6a9-86440dd5de13" );
            //// Attrib Value for Block:Prayer Request Attributes, Attribute:Entity Id Page: Prayer Request Attributes, Site: Rock RMS
            //RockMigrationHelper.AddBlockAttributeValue( true, "B6BFDEBD-BBB5-4EC8-A0B2-EB2AEC119FEF", "CBB56D68-3727-42B9-BF13-0B2B593FB328", @"0" );
            //// Attrib Value for Block:Prayer Request Attributes, Attribute:Allow Setting of Values Page: Prayer Request Attributes, Site: Rock RMS
            //RockMigrationHelper.AddBlockAttributeValue( true, "B6BFDEBD-BBB5-4EC8-A0B2-EB2AEC119FEF", "018C0016-C253-44E4-84DB-D166084C5CAD", @"False" );
            //// Attrib Value for Block:Prayer Request Attributes, Attribute:Configure Type Page: Prayer Request Attributes, Site: Rock RMS
            //RockMigrationHelper.AddBlockAttributeValue( true, "B6BFDEBD-BBB5-4EC8-A0B2-EB2AEC119FEF", "D4132497-18BE-4D1F-8913-468E33DE63C4", @"True" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
