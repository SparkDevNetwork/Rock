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
namespace com.lcbcchurch.Checkin.Migrations
{
    [MigrationNumber( 4, "1.0.14" )]
    public class CurrentRosterWithCodes : Migration
    {
        public override void Up()
        {
            //Add New Page for Lava Report
            // Page: Roster Report
            RockMigrationHelper.AddPage("A4DCE339-9C11-40CA-9A02-D2FE64EA164B","01158948-1DC3-4A92-B213-9E26BE194F68","Roster Report","","B3FAE8FA-4023-4239-9C28-F9D2C5285612",""); // Site:Rock Check-in Manager
            // Add Block to Page: Roster Report, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlock( true, "B3FAE8FA-4023-4239-9C28-F9D2C5285612","","19B61D65-37E3-459F-A44F-DEF0089118A3","Roster Report HTML","Main","","",0,"7632DC74-0534-4167-A025-93135AF53E38");
            // Attrib Value for Block:Roster Report HTML, Attribute:Enabled Lava Commands Page: Roster Report, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlockAttributeValue("7632DC74-0534-4167-A025-93135AF53E38","7146AC24-9250-4FC4-9DF2-9803B9A84299",@"RockEntity,Sql");
            //Add HTML to HTML Block
            RockMigrationHelper.UpdateHtmlContentBlock( "7632DC74-0534-4167-A025-93135AF53E38", @"{% include '~/Plugins/com_bemadev/Checkin/Assets/Lava/Roster.lava' %}", "21263d20-7d72-4381-b490-476728124849" );



            // Add Button to the Checkin Manager Page
            RockMigrationHelper.AddBlock( true, "A4DCE339-9C11-40CA-9A02-D2FE64EA164B", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "All Groups Rosters", "Main", "", "", 2, "ED0D88E2-3175-4C46-97DC-AC18E8E874D9" );
            RockMigrationHelper.AddBlockAttributeValue( "ED0D88E2-3175-4C46-97DC-AC18E8E874D9", "7146AC24-9250-4FC4-9DF2-9803B9A84299", @"RockEntity,Sql" );
            RockMigrationHelper.UpdateHtmlContentBlock( "ED0D88E2-3175-4C46-97DC-AC18E8E874D9", @"
                    {% assign pageId = 546 %}
                    {% page where:'Guid == ""B3FAE8FA-4023-4239-9C28-F9D2C5285612""' %}
                    {% for pageItem in pageItems %}
                     {% assign pageId = pageItem.Id %}
                    {% endfor %}
                    {% endpage %}
                    {% assign campusId = Context.Campus.Id %}
                    {% assign checkinArea = 'Global' | PageParameter:'Area' %}
                    {% if checkinArea == '' or !checkinArea or checkinArea == empty %}
                        {% assign checkinArea = 'dad0e772-6736-4635-92a0-931f9e94e082' %}
                    {% endif %}  
                    <a class=""btn btn-primary"" href=""/page/{{pageId}}/?CampusId={{campusId}}&Area={{checkinArea}}"" target=""_blank"">All Groups Rosters</a>
            ", "BFAFA6B9-DC18-4E9C-A66D-90D6036F8144" );

        }
        public override void Down()
        {
            
        }
    }
}
