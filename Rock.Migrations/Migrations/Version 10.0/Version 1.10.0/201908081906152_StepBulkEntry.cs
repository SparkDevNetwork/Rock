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
    /// <summary>
    ///
    /// </summary>
    public partial class StepBulkEntry : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddPage( true, "8E78F9DC-657D-41BF-BE0F-56916B6BF92F", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Bulk Entry", "", "8224D858-04B3-4DCD-9C73-F9868DF29C95", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Step Bulk Entry", "Displays a form to add multiple steps at a time.", "~/Blocks/Steps/StepBulkEntry.ascx", "Steps", "6535FA22-9630-49A3-B8FF-A672CD91B8EE" );
            // Add Block to Page: Bulk Entry Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "8224D858-04B3-4DCD-9C73-F9868DF29C95".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "6535FA22-9630-49A3-B8FF-A672CD91B8EE".AsGuid(), "Step Bulk Entry", "Main", @"", @"", 0, "1F88CE2B-9ADC-43C0-8CFB-C7FC05BE0B83" );
            // Attrib for BlockType: Step Type Detail:Bulk Entry Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "84DEAB14-70B3-4DA4-9CC2-0E0A301EE0FD", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Bulk Entry Page", "BulkEntryPage", "Bulk Entry Page", @"The page to use for bulk entry of steps data", 8, @"", "69A3C6F9-15CC-40D5-AD03-F96B45B6822B" );
            // Attrib Value for Block:Step Type Detail, Attribute:Bulk Entry Page Page: Type, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B572C7D7-3989-4BEA-AC63-3447B5CF7ED8", "69A3C6F9-15CC-40D5-AD03-F96B45B6822B", @"8224d858-04b3-4dcd-9c73-f9868df29c95" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Step Type Detail:Bulk Entry Page
            RockMigrationHelper.DeleteAttribute( "69A3C6F9-15CC-40D5-AD03-F96B45B6822B" );
            // Remove Block: Step Bulk Entry, from Page: Bulk Entry, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "1F88CE2B-9ADC-43C0-8CFB-C7FC05BE0B83" );
            RockMigrationHelper.DeleteBlockType( "6535FA22-9630-49A3-B8FF-A672CD91B8EE" ); // Step Bulk Entry
            RockMigrationHelper.DeletePage( "8224D858-04B3-4DCD-9C73-F9868DF29C95" ); //  Page: Bulk Entry, Layout: Full Width, Site: Rock RMS
        }
    }
}
