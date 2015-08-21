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
    public partial class OpportunitySummary : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.ConnectionOpportunity", "Summary", c => c.String());

            Sql( @"
UPDATE [ConnectionOpportunity] SET [Summary] = [Description]
UPDATE [ConnectionOpportunity] SET [Description] = '<p>Ut omnis accumsan moderatius vis. Ad sit simul ocurreret. Cu pri mollis delectus molestiae. Usu mutat postea ea, 
ad laoreet accusata consequat eum, ex probo philosophia has. Ex impetus fabulas indoctum nam, sed in quando labore. His eu cetero delenit detraxit.</b>
<p>Ut eam atqui maluisset. Eam ei nulla soleat aperiam, ne docendi dignissim posidonium per. Veri ponderum moderatius ei nam, vix legere legimus abhorreant eu. Id 
numquam noluisse pri. Ea eum quem homero suscipiantur, ad verterem postulant vim. Ex eros audire sit, in natum maiorum eum, cum eruditi volumus electram at. Sea 
ei nobis intellegam, ea ius hinc putent scaevola.</p>
<p>Prima partiendo deseruisse ut his, feugait probatus petentium te sea. Quando neglegentur in eam, per ut minim quando appetere, mea at molestiae maiestatis. At 
nihil nemore denique vim. Qui minim gubergren ne, atqui legendos et cum. Option persequeris te vix, ne pri sale homero graecis, mea ea facer solet.</p>'
" );
            // JE: Add New Route for Registration
            RockMigrationHelper.AddPageRoute( "F7CA6E0F-C319-47AB-9A6D-247C5716D846", "Registration" );

            // DT: New block attribute/values
            // Attrib for BlockType: Registration Instance Detail:Content Item Page
            RockMigrationHelper.AddBlockTypeAttribute( "22B67EDB-6D13-4D29-B722-DF45367AA3CB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Content Item Page", "ContentItemPage", "", "The page for viewing details about a content channel item", 5, @"", "95580BE4-2348-409D-B401-F2B68208FCBC" );
            // Attrib for BlockType: Content Item Detail:Event Occurrence Page
            RockMigrationHelper.AddBlockTypeAttribute( "5B99687B-5FE9-4EE2-8679-5040CAEB9E2E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Event Occurrence Page", "EventOccurrencePage", "", "", 0, @"", "8924F60C-509D-4242-8653-CE8E18DAB9BC" );

            // Attrib Value for Block:Content Item Detail, Attribute:Event Occurrence Page Page: Content Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "AA00DFCB-5C02-418A-A2DC-C0DEA910DA52", "8924F60C-509D-4242-8653-CE8E18DAB9BC", @"4b0c44ee-28e3-4753-a95b-8c57cd958fd1" );
            // Attrib Value for Block:Registration Instance Detail, Attribute:Content Item Page Page: Registration Instance, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5F44A3A8-500B-4C89-95CA-8C4246B53C3F", "95580BE4-2348-409D-B401-F2B68208FCBC", @"d18e837c-9e65-4a38-8647-dff04a595d97" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.ConnectionOpportunity", "Summary");
        }
    }
}
