// <copyright>
// Copyright by Central Christian Church
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock;
using Rock.Plugin;
namespace com.lcbcchurch.Care.Migrations
{
    [MigrationNumber( 3, "1.0.14" )]
    public class PrayerPage : Migration
    {
        public override void Up()
        {
            // Page: Prayer
            RockMigrationHelper.AddPage( "BF04BB7E-BE3A-4A38-A37C-386B55496303", "F66758C6-3E3D-4598-AF4C-B317047B5987", "Prayer", "", "C328DA17-AF71-418B-A297-51046C1628E4", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPageRoute( "C328DA17-AF71-418B-A297-51046C1628E4", "Person/{PersonId}/Prayer" );
            // Add Block to Page: Prayer, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "C328DA17-AF71-418B-A297-51046C1628E4", "", "4D6B686A-79DF-4EFC-A8BA-9841C248BF74", "Prayer Request List", "SectionC1", "", "", 1, "8D9A6E4B-3AEE-4B20-A633-EB8A003050E4" );
            // Add Block to Page: Prayer, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "C328DA17-AF71-418B-A297-51046C1628E4", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "HTML Content", "SectionC1", "", "", 0, "F018985D-04B3-4A8B-8C43-076461B63D63" );
            // Attrib Value for Block:Prayer Request List, Attribute:Detail Page Page: Prayer, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "8D9A6E4B-3AEE-4B20-A633-EB8A003050E4", "A1275318-F6B4-4C66-B782-99037E6E16C0", @"89c3db4a-bafd-45c8-88c6-45d8fec48b48" );
            // Attrib Value for Block:Prayer Request List, Attribute:core.CustomGridColumnsConfig Page: Prayer, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "8D9A6E4B-3AEE-4B20-A633-EB8A003050E4", "C90634D6-16F5-4741-95DB-E4B154979FFF", @"" );
            // Attrib Value for Block:Prayer Request List, Attribute:core.CustomGridEnableStickyHeaders Page: Prayer, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "8D9A6E4B-3AEE-4B20-A633-EB8A003050E4", "AF264718-2907-4177-B98D-28BC525571AB", @"False" );
            // Attrib Value for Block:Prayer Request List, Attribute:Expires After (Days) Page: Prayer, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "8D9A6E4B-3AEE-4B20-A633-EB8A003050E4", "7FEB9A26-6AE6-41F8-B6B5-D5E432C832D0", @"14" );
            // Attrib Value for Block:Prayer Request List, Attribute:Show Prayer Count Page: Prayer, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "8D9A6E4B-3AEE-4B20-A633-EB8A003050E4", "0BD12941-651E-421E-9A14-3740593C843F", @"True" );
            // Attrib Value for Block:Prayer Request List, Attribute:Show 'Approved' column Page: Prayer, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "8D9A6E4B-3AEE-4B20-A633-EB8A003050E4", "96DDF1FA-0345-4E54-AC02-95D5B4577471", @"True" );
            // Attrib Value for Block:Prayer Request List, Attribute:Show Grid Filter Page: Prayer, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "8D9A6E4B-3AEE-4B20-A633-EB8A003050E4", "210646DF-0588-489D-BA46-538754EC9D1F", @"True" );
            // Attrib Value for Block:Prayer Request List, Attribute:Show Public Only Page: Prayer, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "8D9A6E4B-3AEE-4B20-A633-EB8A003050E4", "C131C26B-1309-459D-8A94-0A3A6B01B3B6", @"False" );
            // Add/Update PageContext for Page:Prayer, Entity: Rock.Model.Person, Parameter: PersonId
            RockMigrationHelper.UpdatePageContext( "C328DA17-AF71-418B-A297-51046C1628E4", "Rock.Model.Person", "PersonId", "80FC3A89-4F0B-4493-AEE3-5C668BF3835C" );

            RockMigrationHelper.UpdateHtmlContentBlock( "F018985D-04B3-4A8B-8C43-076461B63D63", @"<a href=""/PrayerRequestDetail/0?PersonId={{ 'Global' | PageParameter:'PersonId' }}""
class=""btn btn-primary"" style=""margin-bottom:15px;"">
<i class=""fa fa-heart"" style=""margin-right: 8px; font-size: 12px;""></i>New Prayer Request
</a>
<br/>
", "DE397827-0A02-4D32-9AFE-4C26B7568A62" );

        }
        public override void Down()
        {

            RockMigrationHelper.DeleteBlock( "F018985D-04B3-4A8B-8C43-076461B63D63" );
            RockMigrationHelper.DeleteBlock( "8D9A6E4B-3AEE-4B20-A633-EB8A003050E4" );
            RockMigrationHelper.DeletePage( "C328DA17-AF71-418B-A297-51046C1628E4" ); //  Page: Prayer
            // Delete PageContext for Page:Prayer, Entity: Rock.Model.Person, Parameter: PersonId
            RockMigrationHelper.DeletePageContext( "80FC3A89-4F0B-4493-AEE3-5C668BF3835C" );

        }
    }
}
