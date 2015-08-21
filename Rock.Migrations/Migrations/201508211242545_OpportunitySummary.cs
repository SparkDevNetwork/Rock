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
