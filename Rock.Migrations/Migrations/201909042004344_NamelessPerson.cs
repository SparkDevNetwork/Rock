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
    public partial class NamelessPerson : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateDefinedValue( "26BE73A6-A9C5-4E94-AE00-3AFDCF8C9275", "Nameless Person", "A presumed transient, unknown person record with limited data not yet matched to an existing person record.", "721300ED-1267-4DA0-B4F2-6C6B5B17B1C5" );

            RockMigrationHelper.AddPage( true, "199DC522-F4D6-4D82-AF44-3C16EE9D2CDA", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Nameless People", "", "62F18233-0395-4BEA-ADC7-BC08271EDAF1", "fa fa-question-circle" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Nameless Person List", "List unmatched phone numbers with an option to link to a person that has the same phone number.", "~/Blocks/Communication/NamelessPersonList.ascx", "Communication", "41AE0574-BE1E-4656-B45D-2CB734D1BE30" );

            // Add Block to Page: Nameless People Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "62F18233-0395-4BEA-ADC7-BC08271EDAF1".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "41AE0574-BE1E-4656-B45D-2CB734D1BE30".AsGuid(), "Nameless Person List", "Main", @"", @"", 0, "5AC75928-3B05-4AED-96ED-38D018F9982A" );

            // Attrib for BlockType: SMS Conversations:Max Conversations
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3497603B-3BE6-4262-B7E9-EC01FC7140EB", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max Conversations", "MaxConversations", "Max Conversations", @"Limits the number of conversations shown in the left pane. This does not affect the actual messages shown on the right.", 5, @"100", "FA074D66-14ED-4D68-B6F5-A62D8BC091EA" );

            // Attrib for BlockType: Person Bio:Nameless Person Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Nameless Person Detail Page", "NamelessPersonDetailPage", "Nameless Person Detail Page", @"The page to redirect user to if the person record is a Nameless Person record type.", 6, @"", "43F5A0D5-8942-454A-867C-5F329A908615" );

            // Attrib Value for Block:Bio, Attribute:Nameless Person Detail Page , Layout: PersonDetail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B5C1FDB6-0224-43E4-8E26-6B2EAF86253A", "43F5A0D5-8942-454A-867C-5F329A908615", @"62f18233-0395-4bea-adc7-bc08271edaf1" );

            // other catch ups
            RockMigrationHelper.UpdateFieldType( "Persisted Dataset", "", "Rock", "Rock.Field.Types.PersistedDatasetFieldType", "392865C4-F17B-4832-AB59-20F72BB1C9F6" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove Block: Nameless Person List, from Page: Nameless People, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "5AC75928-3B05-4AED-96ED-38D018F9982A" );
            RockMigrationHelper.DeleteBlockType( "41AE0574-BE1E-4656-B45D-2CB734D1BE30" ); // Nameless Person List
            RockMigrationHelper.DeletePage( "62F18233-0395-4BEA-ADC7-BC08271EDAF1" ); //  Page: Nameless People, Layout: Full Width, Site: Rock RMS
        }
    }
}
