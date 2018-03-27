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
    public partial class AddIsLockedAsChildToPerson : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Person", "IsLockedAsChild", c => c.Boolean(nullable: false));

            // Add Block to Page: Calendar Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "9C610805-BE44-42DF-A73F-2C6D0014AD49", "", "E5EA2F6D-43A2-48E0-B59C-4409B78AC830", "Event Occurrence Attributes", "Main", @"<div class=""panel-block panel-parent""  style=""margin-bottom: 15px;"">
    <div class=""panel-heading"">
        <h1 class=""panel-title"">
            <i class=""fa fa-fw fa fa-clock-o""></i>
            Event Occurrence Attributes
        </h1>
    </div>", @"</div>", 1, "DF4472A7-AC11-4245-B9D0-FBB8547B60B4" );

            // update block order for pages with new blocks if the page,zone has multiple blocks
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = 'F04979E2-33A2-4C0E-936E-5C8849BB98F4'" );  // Page: Calendar Attributes,  Zone: Main,  Block: Calendar Attributes
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = 'DF4472A7-AC11-4245-B9D0-FBB8547B60B4'" );  // Page: Calendar Attributes,  Zone: Main,  Block: Event Occurrence Attributes

            // Attrib Value for Block:Event Occurrence Attributes, Attribute:Entity Qualifier Column Page: Calendar Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "DF4472A7-AC11-4245-B9D0-FBB8547B60B4", "ECD5B86C-2B48-4548-9FE9-7AC6F6FA8106", @"" );

            // Attrib Value for Block:Event Occurrence Attributes, Attribute:Entity Qualifier Value Page: Calendar Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "DF4472A7-AC11-4245-B9D0-FBB8547B60B4", "FCE1E87D-F816-4AD5-AE60-1E71942C547C", @"" );

            // Attrib Value for Block:Event Occurrence Attributes, Attribute:Entity Page: Calendar Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "DF4472A7-AC11-4245-B9D0-FBB8547B60B4", "5B33FE25-6BF0-4890-91C6-49FB1629221E", @"71632e1a-1e7f-42b9-a630-ec99f375303a" );

            // Attrib Value for Block:Event Occurrence Attributes, Attribute:Entity Id Page: Calendar Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "DF4472A7-AC11-4245-B9D0-FBB8547B60B4", "CBB56D68-3727-42B9-BF13-0B2B593FB328", @"0" );

            // Attrib Value for Block:Event Occurrence Attributes, Attribute:Allow Setting of Values Page: Calendar Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "DF4472A7-AC11-4245-B9D0-FBB8547B60B4", "018C0016-C253-44E4-84DB-D166084C5CAD", @"False" );

            // Attrib Value for Block:Event Occurrence Attributes, Attribute:Enable Show In Grid Page: Calendar Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "DF4472A7-AC11-4245-B9D0-FBB8547B60B4", "920FE120-AD75-4D5C-BFE0-FA5745B1118B", @"False" );

            // Attrib Value for Block:Event Occurrence Attributes, Attribute:Category Filter Page: Calendar Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "DF4472A7-AC11-4245-B9D0-FBB8547B60B4", "0C2BCD33-05CC-4B03-9F57-C686B8911E64", @"" );

            // Attrib Value for Block:Event Occurrence Attributes, Attribute:core.CustomGridColumnsConfig Page: Calendar Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "DF4472A7-AC11-4245-B9D0-FBB8547B60B4", "11F74455-F71D-45C7-806B-0DB463D34DAB", @"" );
            // Update pre and post html for the existing Calendar Attribute block
            Sql( @"UPDATE [Block]
SET [PreHtml] = 
'<style>
    .panel-parent .panel-block .panel-heading { display: none; }
</style>
<div class=""panel-block panel-parent"" style=""margin-bottom: 15px;"">
<div class=""panel-heading"">
        <h1 class=""panel-title"">
            <i class=""fa fa-fw fa-calendar""></i>
            Calendar Attributes
        </h1>
    </div>',
 [PostHtml] = '</div>'
WHERE[Block].[Guid] = 'F04979E2-33A2-4C0E-936E-5C8849BB98F4'" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.Person", "IsLockedAsChild");
        }
    }
}
