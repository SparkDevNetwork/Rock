// <copyright>
// Copyright by LCBC Church
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

using Rock.Plugin;
namespace com.lcbcchurch.Reporting.Migrations
{
    [MigrationNumber( 2, "1.0.14" )]
    public class AddAbsenteePage : Migration
    {
        public override void Up()
        {
            // Page: Absentee Report
            RockMigrationHelper.AddPage( "7F1F4130-CB98-473B-9DE1-7A886D2283ED", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Absentee Report", "", "23B27982-B2F2-4D18-A4BF-598E41DC8FFC", "fa fa-file-invoice" ); // Site:Rock RMS
            // Add Block to Page: Absentee Report, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "23B27982-B2F2-4D18-A4BF-598E41DC8FFC", "", "FE667841-D12D-4F12-A171-A3855A7CE254", "Page Parameter Filter", "Main", "", "", 1, "A2C2DE76-C8A0-48B0-ADE6-C8B7A2754CF3" );
            // Add Block to Page: Absentee Report, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "23B27982-B2F2-4D18-A4BF-598E41DC8FFC", "", "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "Dynamic Data", "Main", "", "", 0, "5D6128E5-8F4C-4166-A0D9-0F10E54887C7" );
            // Add Block to Page: Absentee Report, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "23B27982-B2F2-4D18-A4BF-598E41DC8FFC", "", "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "Dynamic Data", "Main", "", "", 2, "4A4A37D9-991A-485A-A961-BB9F31B9EAD9" );
            // Attrib Value for Block:Page Parameter Filter, Attribute:Date Range Label Page: Absentee Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A2C2DE76-C8A0-48B0-ADE6-C8B7A2754CF3", "F4D72961-F212-4D49-956F-CE42C3B86B8F", @"" );
            // Attrib Value for Block:Page Parameter Filter, Attribute:Multi Select Label Page: Absentee Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A2C2DE76-C8A0-48B0-ADE6-C8B7A2754CF3", "32CA03D1-7DB7-4922-B426-3131D91F96AC", @"Week Filter" );
            // Attrib Value for Block:Page Parameter Filter, Attribute:Number Range Label Page: Absentee Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A2C2DE76-C8A0-48B0-ADE6-C8B7A2754CF3", "AACC30CA-06F8-426B-8DBB-43301A5E5122", @"" );
            // Attrib Value for Block:Page Parameter Filter, Attribute:Enable Campuses Filter Page: Absentee Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A2C2DE76-C8A0-48B0-ADE6-C8B7A2754CF3", "204FB5CA-4650-4D82-8E55-035AA7C73304", @"True" );
            // Attrib Value for Block:Page Parameter Filter, Attribute:Page Redirect Page: Absentee Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A2C2DE76-C8A0-48B0-ADE6-C8B7A2754CF3", "41B4D32D-ED66-402D-8B3C-FC5DCE53DBA8", @"0" );
            // Attrib Value for Block:Page Parameter Filter, Attribute:Boolean Select Page: Absentee Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A2C2DE76-C8A0-48B0-ADE6-C8B7A2754CF3", "B29D445D-4447-4137-A33F-FF0C63A6E813", @"" );
            // Attrib Value for Block:Page Parameter Filter, Attribute:Date Range Label 2 Page: Absentee Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A2C2DE76-C8A0-48B0-ADE6-C8B7A2754CF3", "BEAB0FC4-AB78-4E65-9157-D31390F531A1", @"" );
            // Attrib Value for Block:Page Parameter Filter, Attribute:Multi Select List Page: Absentee Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A2C2DE76-C8A0-48B0-ADE6-C8B7A2754CF3", "A5EAFA44-1844-41A6-A359-5E19BEF3249F", @"Absent 4 weeks^4week|Absent 8 weeks^8week|Absent 16 weeks^16week|Absent 26 weeks^26week" );
            // Attrib Value for Block:Page Parameter Filter, Attribute:Number Range Label 2 Page: Absentee Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A2C2DE76-C8A0-48B0-ADE6-C8B7A2754CF3", "20102155-99D0-484F-86A1-39262994EBCF", @"" );
            // Attrib Value for Block:Page Parameter Filter, Attribute:Time Range Label Page: Absentee Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A2C2DE76-C8A0-48B0-ADE6-C8B7A2754CF3", "D4570AD0-97D6-40D2-A511-F44F0B34F7F6", @"" );
            // Attrib Value for Block:Page Parameter Filter, Attribute:Button Text Page: Absentee Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A2C2DE76-C8A0-48B0-ADE6-C8B7A2754CF3", "9AE98186-E466-4196-A272-31852322A4BA", @"Filter" );
            // Attrib Value for Block:Page Parameter Filter, Attribute:Boolean Select 2 Page: Absentee Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A2C2DE76-C8A0-48B0-ADE6-C8B7A2754CF3", "B3BEA2E6-31F9-4615-B452-681F3887023B", @"" );
            // Attrib Value for Block:Page Parameter Filter, Attribute:Enable Account Filter Page: Absentee Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A2C2DE76-C8A0-48B0-ADE6-C8B7A2754CF3", "E7DF5865-7973-45AF-A090-9D28E9747B7F", @"False" );
            // Attrib Value for Block:Page Parameter Filter, Attribute:Number Range Label 3 Page: Absentee Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A2C2DE76-C8A0-48B0-ADE6-C8B7A2754CF3", "3D33B8AC-BFBD-4498-B8A1-882BBEB64BE2", @"" );
            // Attrib Value for Block:Page Parameter Filter, Attribute:Date Label Page: Absentee Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A2C2DE76-C8A0-48B0-ADE6-C8B7A2754CF3", "9E3699DA-DABC-4D8F-BB03-30F7E336BC22", @"" );
            // Attrib Value for Block:Page Parameter Filter, Attribute:Multi Select Label 2 Page: Absentee Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A2C2DE76-C8A0-48B0-ADE6-C8B7A2754CF3", "9172F1C2-932B-4F06-91D8-F1A254862A3E", @"" );
            // Attrib Value for Block:Page Parameter Filter, Attribute:Text Entry Page: Absentee Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A2C2DE76-C8A0-48B0-ADE6-C8B7A2754CF3", "552A47D7-A6BE-4634-A1AC-C947147AE6CA", @"" );
            // Attrib Value for Block:Page Parameter Filter, Attribute:Enable Person Filter Page: Absentee Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A2C2DE76-C8A0-48B0-ADE6-C8B7A2754CF3", "60F58D52-1C66-45C6-B3B9-85A213A53EEF", @"False" );
            // Attrib Value for Block:Page Parameter Filter, Attribute:Block Text Page: Absentee Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A2C2DE76-C8A0-48B0-ADE6-C8B7A2754CF3", "86EFBF55-2E2F-42BD-AEEF-34E9769AE44F", @"Filter" );
            // Attrib Value for Block:Page Parameter Filter, Attribute:Multi Select List 2 Page: Absentee Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A2C2DE76-C8A0-48B0-ADE6-C8B7A2754CF3", "1512BB91-F120-409A-A5D9-3F912911AD60", @"" );
            // Attrib Value for Block:Page Parameter Filter, Attribute:Enable Groups Filter Page: Absentee Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A2C2DE76-C8A0-48B0-ADE6-C8B7A2754CF3", "AA948207-54F5-4CCC-AB4B-4B3582C7C4D1", @"False" );
            // Attrib Value for Block:Dynamic Data, Attribute:Update Page Page: Absentee Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4A4A37D9-991A-485A-A961-BB9F31B9EAD9", "230EDFE8-33CA-478D-8C9A-572323AF3466", @"True" );
            // Attrib Value for Block:Dynamic Data, Attribute:Query Params Page: Absentee Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4A4A37D9-991A-485A-A961-BB9F31B9EAD9", "B0EC41B9-37C0-48FD-8E4E-37A8CA305012", @"" );
            // Attrib Value for Block:Dynamic Data, Attribute:Columns Page: Absentee Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4A4A37D9-991A-485A-A961-BB9F31B9EAD9", "90B0E6AF-B2F4-4397-953B-737A40D4023B", @"Id" );
            // Attrib Value for Block:Dynamic Data, Attribute:Query Page: Absentee Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4A4A37D9-991A-485A-A961-BB9F31B9EAD9", "71C8BA4E-8EF2-416B-BFE9-D1D88D9AA356", @"Declare @WeekFilter nvarchar(max) = '4week'
Declare @CampusIds nvarchar(max) = 'null'

{% assign queryParms = 'Global' | Page:'QueryString' %}

{% for item in queryParms %}
    {% assign kvItem = item | PropertyToKeyValue %}  

    {% if kvItem.Key == 'CampusIds' %}
        {% if kvItem.Value != ',' %}
            {% assign CampusIds = kvItem.Value %}
        {% endif %}
    {% endif %}
    
    {% if kvItem.Key == 'WeekFilter' %}
        {% if kvItem.Value != ',' %}
            {% assign WeekFilter = kvItem.Value %}
        {% endif %}
    {% endif %}
    
{% endfor %}

{% if CampusIds != null %}
Set @CampusIds = '{{CampusIds}}'
{% endif %}

{% if WeekFilter != null %}
Set @WeekFilter = '{{WeekFilter}}'
{% endif %}

Declare @0WeekMark datetime = [dbo].[ufnUtility_GetPreviousSundayDate]();
Declare @4WeekMark datetime = DateAdd(week, -4, @0WeekMark)
Declare @8WeekMark datetime = DateAdd(week, -8, @0WeekMark)
Declare @16WeekMark datetime = DateAdd(week, -16, @0WeekMark)
Declare @26WeekMark datetime = DateAdd(week, -26, @0WeekMark)

Declare @ReferenceTable table(
	LastAttendanceId int,
	PersonId int
)
Insert into @ReferenceTable
Select max(a.Id)[LastAttendanceId], pa.PersonId
From Attendance a
Join AttendanceOccurrence ao on a.OccurrenceId = ao.Id
Join PersonAlias pa on a.PersonAliasId = pa.Id
Join [Group] g on ao.GroupId = g.Id
Join GroupType gt on g.GroupTypeId = gt.Id
Join (
	Select max(a.StartDateTime) [StartDateTime], pa.PersonId
	From Attendance a
Join AttendanceOccurrence ao on a.OccurrenceId = ao.Id
	Join [Group] g on ao.GroupId = g.Id
	Join GroupType gt on g.GroupTypeId = gt.Id
	Join PersonAlias pa on a.PersonAliasId = pa.Id
	Join Person p on pa.PersonId = p.Id
	Where p.ConnectionStatusValueId = 146
	and gt.AttendanceCountsAsWeekendService = 1
	Group by pa.PersonId
	) as latestAttendanceTime on latestAttendanceTime.PersonId = pa.PersonId and latestAttendanceTime.StartDateTime = a.StartDateTime
Where (
		(@WeekFilter like '%4week%' and latestAttendanceTime.StartDateTime <= @4WeekMark and latestAttendanceTime.StartDateTime > @8WeekMark) or
		(@WeekFilter like '%8week%' and latestAttendanceTime.StartDateTime <= @8WeekMark and latestAttendanceTime.StartDateTime > @16WeekMark) or
		(@WeekFilter like '%16week%' and latestAttendanceTime.StartDateTime <= @16WeekMark and latestAttendanceTime.StartDateTime > @26WeekMark) or
		(@WeekFilter like '%26week%' and latestAttendanceTime.StartDateTime <= @26WeekMark)
		)
and gt.AttendanceCountsAsWeekendService = 1
Group by pa.PersonId

Select	p.Id,
		p.NickName as 'First Name',
		p.LastName,
		f.Name as 'Household Name',
		(SELECT ISNULL([Street1], '') + ' ' + ISNULL([Street2], '') 
			+ ' ' + ISNULL([City], '') + ', ' + ISNULL([State], '') 
			+ ' ' + ISNULL([PostalCode], '')
			 FROM [Location] 
			 WHERE [Id] = (SELECT TOP 1 [LocationId] 
							FROM [GroupLocation] 
							WHERE  [GroupLocationTypeValueId] = 19 
							AND  [GroupId] = f.Id)) as 'Address',
		STUFF(
				(   SELECT ',' + pn.NumberFormatted 
					From PhoneNumber pn
					Where PersonId = p.Id
					FOR xml path('')
				)
				, 1
				, 1
				, '') as 'Phone #''s',
		p.Email,
		Case When p.BirthDate is null Then null
			When p.BirthDate > DateAdd(year,DatePart(Year,p.BirthDate) - DatePart(year, getDate()),GetDate()) Then (DatePart(year, getDate()) - DatePart(Year,p.BirthDate))-1
			Else  DatePart(year, getDate()) - DatePart(Year,p.BirthDate) End as 'Age',
		c.Name as 'Campus',
		a.StartDateTime as 'Last Visit Date',
		DATEDIFF(week, a.StartDateTime, @0WeekMark) as 'Weeks Since Last Visit'
From @ReferenceTable referenceTable
Join Person p on referenceTable.PersonId = p.Id
Join GroupMember fm on p.Id = fm.PersonId
Join [Group] f on fm.GroupId = f.Id and f.Id = (SELECT TOP 1 [GroupId] 
									FROM [GroupMember] gm 
									INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] 
									WHERE [PersonId] = p.Id AND g.[GroupTypeId] = 10)
Join Attendance a on referenceTable.LastAttendanceId = a.Id
Join AttendanceOccurrence ao on a.OccurrenceId = ao.Id
Join [Group] g on ao.GroupId = g.Id
Join Campus c on f.CampusId = c.Id
Where ( @CampusIds = 'null' or c.Id in (Select * From ufnUtility_CsvToTable(@CampusIds)))" );
            // Attrib Value for Block:Dynamic Data, Attribute:Url Mask Page: Absentee Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4A4A37D9-991A-485A-A961-BB9F31B9EAD9", "B9163A35-E09C-466D-8A2D-4ED81DF0114C", @"~/Person/{Id}" );
            // Attrib Value for Block:Dynamic Data, Attribute:Show Columns Page: Absentee Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4A4A37D9-991A-485A-A961-BB9F31B9EAD9", "202A82BF-7772-481C-8419-600012607972", @"False" );
            // Attrib Value for Block:Dynamic Data, Attribute:Merge Fields Page: Absentee Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4A4A37D9-991A-485A-A961-BB9F31B9EAD9", "8EB882CE-5BB1-4844-9C28-10190903EECD", @"" );
            // Attrib Value for Block:Dynamic Data, Attribute:Formatted Output Page: Absentee Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4A4A37D9-991A-485A-A961-BB9F31B9EAD9", "6A233402-446C-47E9-94A5-6A247C29BC21", @"" );
            // Attrib Value for Block:Dynamic Data, Attribute:Person Report Page: Absentee Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4A4A37D9-991A-485A-A961-BB9F31B9EAD9", "8104CE53-FDB3-4E9F-B8E7-FD9E06E7551C", @"True" );
            // Attrib Value for Block:Dynamic Data, Attribute:Communication Recipient Person Id Columns Page: Absentee Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4A4A37D9-991A-485A-A961-BB9F31B9EAD9", "75DDB977-9E71-44E8-924B-27134659D3A4", @"" );
            // Attrib Value for Block:Dynamic Data, Attribute:Show Excel Export Page: Absentee Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4A4A37D9-991A-485A-A961-BB9F31B9EAD9", "E11B57E5-EC7D-4C42-9ADA-37594D71F145", @"True" );
            // Attrib Value for Block:Dynamic Data, Attribute:Show Communicate Page: Absentee Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4A4A37D9-991A-485A-A961-BB9F31B9EAD9", "5B2C115A-C187-4AB3-93AE-7010644B39DA", @"True" );
            // Attrib Value for Block:Dynamic Data, Attribute:Show Merge Person Page: Absentee Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4A4A37D9-991A-485A-A961-BB9F31B9EAD9", "8762ABE3-726E-4629-BD4D-3E42E1FBCC9E", @"False" );
            // Attrib Value for Block:Dynamic Data, Attribute:Show Bulk Update Page: Absentee Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4A4A37D9-991A-485A-A961-BB9F31B9EAD9", "D01510AA-1B8D-467C-AFC6-F7554CB7CF78", @"False" );
            // Attrib Value for Block:Dynamic Data, Attribute:Stored Procedure Page: Absentee Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4A4A37D9-991A-485A-A961-BB9F31B9EAD9", "A4439703-5432-489A-9C14-155903D6A43E", @"False" );
            // Attrib Value for Block:Dynamic Data, Attribute:Show Merge Template Page: Absentee Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4A4A37D9-991A-485A-A961-BB9F31B9EAD9", "6697B0A2-C8FE-497A-B5B4-A9D459474338", @"True" );
            // Attrib Value for Block:Dynamic Data, Attribute:Paneled Grid Page: Absentee Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4A4A37D9-991A-485A-A961-BB9F31B9EAD9", "5449CB61-2DFC-4B55-A697-38F1C2AF128B", @"False" );
            // Attrib Value for Block:Dynamic Data, Attribute:Show Grid Filter Page: Absentee Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4A4A37D9-991A-485A-A961-BB9F31B9EAD9", "E582FD3C-9990-47D1-A57F-A3DB753B1D0C", @"True" );
            // Attrib Value for Block:Dynamic Data, Attribute:Timeout Page: Absentee Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4A4A37D9-991A-485A-A961-BB9F31B9EAD9", "BEEE38DD-2791-4242-84B6-0495904143CC", @"900" );

            RockMigrationHelper.AddSecurityAuthForPage( "23B27982-B2F2-4D18-A4BF-598E41DC8FFC", 0, "View", true, "628C51A8-4613-43ED-A18D-4A6FB999273E", 0, "5DC0859E-66A9-4686-9E28-E9321FBD4A71" ); // Page:Absentee Report
            RockMigrationHelper.AddSecurityAuthForPage( "23B27982-B2F2-4D18-A4BF-598E41DC8FFC", 1, "View", false, "", 1, "5A362D9D-1E8F-4C2D-9C23-E410A7F2D305" ); // Page:Absentee Report
        }
        public override void Down()
        {
            RockMigrationHelper.DeleteSecurityAuth( "5A362D9D-1E8F-4C2D-9C23-E410A7F2D305" ); // Page:Absentee Report Group: <all users>
            RockMigrationHelper.DeleteSecurityAuth( "5DC0859E-66A9-4686-9E28-E9321FBD4A71" ); // Page:Absentee Report Group: 628C51A8-4613-43ED-A18D-4A6FB999273E ( RSR - Rock Administration ),
           
            RockMigrationHelper.DeleteBlock( "4A4A37D9-991A-485A-A961-BB9F31B9EAD9" );
            RockMigrationHelper.DeleteBlock( "5D6128E5-8F4C-4166-A0D9-0F10E54887C7" );
            RockMigrationHelper.DeleteBlock( "A2C2DE76-C8A0-48B0-ADE6-C8B7A2754CF3" );
            RockMigrationHelper.DeletePage( "23B27982-B2F2-4D18-A4BF-598E41DC8FFC" ); //  Page: Absentee Report
        }
    }
}
