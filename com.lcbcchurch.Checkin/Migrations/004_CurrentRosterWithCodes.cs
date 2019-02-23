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
            RockMigrationHelper.UpdateHtmlContentBlock( "7632DC74-0534-4167-A025-93135AF53E38", @"
{% assign navPath = CurrentPerson | GetUserPreference:'CurrentNavPath' %}
{% assign navPaths = navPath | Split:'|' %}
{% for path in navPaths %}
	{% assign pathSize = path | Size %}
	{%if pathSize > 1 %}
		{% if path contains 'T'%}
			{% assign groupTypeId = path | Remove:'T' %}
		{% elseif path contains 'G' %}
			{% assign groupId = path | Remove:'G' %}
		{% elseif path contains 'L' %}
			{% assign locationId = path | Remove:'L' %}
		{% endif %}
	{% endif %}
{% endfor %}

{% assign campusId = 'Global' | PageParameter:'CampusId' %}
{% assign locationIds = 'Global' | PageParameter:'LocationIds' %}
{% assign areaGuid = 'Global' | PageParameter:'Area' %}
{% sql %} 
{% if areaGuid != '' %} Declare @ParentGroupTypeId int = (Select Top 1 Id From GroupType Where Guid = '{{areaGuid}}');{% endif %}

{% if campusId != '' %}
Declare @CampusId int = {{campusId}}
Declare @CampusLocationId int = (Select LocationId From Campus Where Id = @CampusId)
Declare @LocationIdTable table(
LocationId int
)
Insert into @LocationIdTable (LocationId) Values (@CampusLocationId);

   WITH CTE AS (
                    SELECT Id FROM [Location] WHERE [ParentLocationId]=@CampusLocationId
                    UNION ALL
                    SELECT [a].Id FROM [Location] [a]
                    INNER JOIN CTE pcte ON pcte.Id = [a].[ParentLocationId]
                )
				Insert Into @LocationIdTable (LocationId) 
                SELECT L.Id FROM CTE
                INNER JOIN [Location] L ON L.[Id] = CTE.[Id]

{% endif %}

SELECT 	AC.Code AS Code ,
		P.NickName + ' ' + P.LastName AS Name,
		AV.Value AS Pager,
        L.Name AS LocationName,
		G.Name AS GroupName
FROM [Attendance] A 
INNER JOIN [AttendanceOccurrence] AO ON A.OccurrenceId = AO.Id 
INNER JOIN [Location] L ON AO.LocationId = L.Id
INNER JOIN [Group] G ON AO.GroupId = G.Id
INNER JOIN [GroupType] GT ON GT.Id = G.GroupTypeId
INNER JOIN [PersonAlias] PA ON A.PersonAliasId = PA.Id 
INNER JOIN [Person] P ON PA.PersonId = P.Id
{% if areaGuid != '' %} INNER JOIN GroupTypeAssociation GTA ON GT.ID = GTA.ChildGroupTypeId AND GTA.GroupTypeId = @ParentGroupTypeId {% endif %}
LEFT OUTER JOIN [AttendanceCode] AC ON A.AttendanceCodeId = AC.Id
LEFT OUTER JOIN [Attribute] AT ON AT.Guid = '791A4DC9-BB89-41E6-95E9-D377ED4C2F0B'
LEFT OUTER JOIN [AttributeValue] AV ON AV.AttributeId = AT.Id AND AV.EntityId = A.Id
WHERE A.DidAttend = 1 AND DATEDIFF(day, AO.OccurrenceDate, GetDate()) = 0
AND GetDate() < ISNULL(A.EndDateTime,GetDate()+1)
{% if campusId != '' %} AND AO.LocationId in (Select * From @LocationIdTable) {% endif %}
{% if groupTypeId %} AND G.GroupTypeId = {{groupTypeId}} {% endif %}
{% if groupId %} AND G.Id = {{groupId}} {% endif %}
{% if locationId %} AND AO.LocationId = {{locationId}} {% endif %}
{% if locationIds != '' %} AND AO.LocationId in ({{locationIds}}){% endif %}
ORDER BY L.Name, G.GroupTypeId, G.Name, P.NickName, P.LastName 
{% endsql %}

<style>
body { font-family: Arial, Helvetica, sans-serif; font-size: 14px; padding: .2in; }
table tr td, table tr th { page-break-inside: avoid; border-bottom: .5px solid black; }
.tr-header { border: none; background: #2E2E2E; }
.tr-header th { border: none !important; color: #fff !important; background: #2E2E2E !important; }
</style>

{% assign newLocation = '' %} 
    {% for item in results %} 
    	{% if newLocation != item.LocationName %}
			{% if newLocation != '' %} 
				</tbody></table> <div style='page -break-after:always;'></div>
            {% endif %}

            <table class='table table-striped'> 
				<thead> 
					<tr class='tr-header'>
						<th colspan='8' style='padding-left:15px'> 
							<h3 style='color:white;'>{{item.LocationName}}</h3> 
						</th> 
					</tr>
					<tr class='tr-header'> 
						<th>Attendee Name</th>
						<th>Group Name</th>
						<th>Security Code</th> 
						<th>Pager</th>
					</tr> 
				</thead> 
			<tbody> 

        {% endif %}
		{% assign newLocation = item.LocationName %} 
		<tr> 
			<td>{{item.Name}}</td>
			<td>{{item.GroupName}}</td> 
			<td>{{item.Code}}</td> 
			<td>{{item.Pager}}</td>
		</tr> 
	{% endfor %} 
	</tbody> 
</table>", "21263d20-7d72-4381-b490-476728124849" );



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
