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
using Rock;
using Rock.Plugin;
namespace com.lcbcchurch.Reporting.Migrations
{
    [MigrationNumber( 8, "1.0.14" )]
    public class AddedColumnsAndSignalReferencing : Migration
    {
        public override void Up()
        {
            // Attrib Value for Block:First Time Guests, Attribute:Query Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "37A9B4BC-FED2-4E09-BECA-2CAB9DE1D248", "CBE194E9-4F10-4505-93DE-31EABA918E85", @"--- First grab everyone for this campus and time period
Declare @CampusIds nvarchar(max) = 'null'
Declare @AttendanceNumbers nvarchar(max) = '1'
Declare @Ages nvarchar(max) = ''
DECLARE @FirstVisitAttributeId int = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '1859E6AE-DD4B-49D1-83B3-5EC6A9D029CB')
DECLARE @SecondVisitAttributeId int = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'FBF88BC2-F99F-45BF-85F9-1DE91C26C4BD')
DECLARE @ThirdVisitAttributeId int = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '4208456B-67B2-4B37-8F4B-547DF48A89FC')

{% assign queryParms = 'Global' | Page:'QueryString' %}

{% for item in queryParms %}
    {% assign kvItem = item | PropertyToKeyValue %}  

    {% if kvItem.Key == 'DateRange' %}
        {% if kvItem.Value != '' %}
            {% assign DateRange = kvItem.Value | Split:',' %}
        {% endif %}
    {% endif %} 

    {% if kvItem.Key == 'CampusIds' %}
        {% if kvItem.Value != ',' %}
            {% assign CampusIds = kvItem.Value %}
        {% endif %}
    {% endif %}

    {% if kvItem.Key == 'Visitors' %}
        {% if kvItem.Value != ',' %}
            {% assign AttendanceNumbers = kvItem.Value %}
        {% endif %}
    {% endif %}

    {% if kvItem.Key == 'Age' %}
        {% if kvItem.Value != ',' %}
            {% assign Age = kvItem.Value %}
        {% endif %}
    {% endif %}

{% endfor %}

{% if CampusIds != null %}
Set @CampusIds = '{{CampusIds}}'
{% endif %}

{% if AttendanceNumbers != null %}
Set @AttendanceNumbers = '{{AttendanceNumbers}}'
{% endif %}

{% if Age != null %}
Set @Ages = '{{Age}}'
{% endif %}

Declare @EndDateTime datetime =DateAdd(day, 1, [dbo].ufnUtility_GetPreviousSundayDate())
Declare @StartDateTime datetime = DateAdd(day, -7, @EndDateTime)

{% if DateRange[0] != null %}
Set @StartDateTime = '{{ DateRange[0] }} 12:00 AM'
{% endif %}

{% if DateRange[1] != null %}
Set @EndDateTime = DateAdd(day, 1,'{{ DateRange[1] }} 12:00 AM')
{% endif %}

Declare @TotalAttendanceCountTable table(
	PersonId int,
	Age int,
	AttendanceDateTime datetime,
	VisitNumber int,
	CampusName nvarchar(max)
);

Insert Into @TotalAttendanceCountTable
Select *
From (
	Select distinct p.Id,
	Case When p.BirthDate is null Then null
					When p.BirthDate > DateAdd(year,DatePart(Year,p.BirthDate) - DatePart(year, getDate()),GetDate()) Then (DatePart(year, getDate()) - DatePart(Year,p.BirthDate))-1
					Else  DatePart(year, getDate()) - DatePart(Year,p.BirthDate) End as 'Age',
	    a.StartDateTime,
		Case	When firstTime.ID is not null then 1
				When secondTime.ID is not null then 2
				When thirdTime.ID is not null then 3
				Else 0 End as VisitNumber,
	    c.Name
	From Attendance a
	Join AttendanceOccurrence ao on a.OccurrenceId = ao.Id
	Join [Group] g on ao.GroupId = g.Id
	Join GroupType gt on g.GroupTypeId = gt.Id
	Join PersonAlias pa on a.PersonAliasId = pa.Id
	Join Person p on pa.PersonId = p.Id
	Join Campus c on g.CampusId = c.Id
	Left Join AttributeValue firstTime on firstTime.EntityId = p.Id and firstTime.ValueAsDateTime = a.StartDateTime and firstTime.AttributeId = @FirstVisitAttributeId
	Left Join AttributeValue secondTime on secondTime.EntityId = p.Id and secondTime.ValueAsDateTime = a.StartDateTime and secondTime.AttributeId = @SecondVisitAttributeId
	Left Join AttributeValue thirdTime on thirdTime.EntityId = p.Id and thirdTime.ValueAsDateTime = a.StartDateTime and thirdTime.AttributeId = @ThirdVisitAttributeId
	Where gt.AttendanceCountsAsWeekendService = 1
	and a.StartDateTime >= @StartDateTime
	and a.StartDateTime < @EndDateTime
	and( @CampusIds = 'null' or c.Id in (Select * From ufnUtility_CsvToTable(@CampusIds)))
	and (firstTime.Id is not null or secondTime.Id is not null or thirdTime.id is not null)
) innerTable
Where (
		@Ages = '' or
		(@Ages like '%Child%' and Age >= 0 and Age <= 9) or
		(@Ages like '%Youth%' and Age >= 10 and Age <= 17) or
		(@Ages like '%Adult%' and ( Age >= 18 or Age is null))
		) 

Select	p.Id,
        p.NickName as 'First Name',
        p.LastName,
        g.[Name] as 'Household Name',
        (SELECT ISNULL([Street1], '') + ' ' + ISNULL([Street2], '') 
            + ' ' + ISNULL([City], '') + ', ' + ISNULL([State], '') 
            + ' ' + ISNULL([PostalCode], '')
                FROM [Location] 
                WHERE [Id] = (SELECT TOP 1 [LocationId] 
			                FROM [GroupLocation] 
			                WHERE  [GroupLocationTypeValueId] = 19 
			                AND  [GroupId] = g.Id)) as 'Address',
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
        countTable.Age,
        countTable.CampusName as 'Campus',
        firstTime.ValueAsDateTime as 'First Visit',
        secondTime.ValueAsDateTime as 'Second Visit',
        thirdTime.ValueAsDateTime as 'Third Visit'
From @TotalAttendanceCountTable countTable
Join Person p on countTable.PersonId = p.Id
Join GroupMember gm on p.Id = gm.PersonId
Join [Group] g on gm.GroupId = g.Id and g.Id = (SELECT TOP 1 [GroupId] 
					                FROM [GroupMember] gm 
					                INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] 
					                WHERE [PersonId] = p.Id AND g.[GroupTypeId] = 10)
Left Join AttributeValue firstTime on firstTime.EntityId = p.Id  and firstTime.AttributeId = @FirstVisitAttributeId
Left Join AttributeValue secondTime on secondTime.EntityId = p.Id and secondTime.AttributeId = @SecondVisitAttributeId
Left Join AttributeValue thirdTime on thirdTime.EntityId = p.Id and thirdTime.AttributeId = @ThirdVisitAttributeId
Where countTable.VisitNumber in (Select * From ufnUtility_CsvToTable(@AttendanceNumbers))

Order By LastName, [First Name], [First Visit], [Second Visit], [Third Visit]" );

            // Attrib Value for Block:Dynamic Data, Attribute:Query Page: Absentee Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4A4A37D9-991A-485A-A961-BB9F31B9EAD9", "71C8BA4E-8EF2-416B-BFE9-D1D88D9AA356", @"Declare @WeekFilter nvarchar(max) = '4week'
Declare @CampusIds nvarchar(max) = 'null'
DECLARE @LastAttendedAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '9ABC5D6B-4840-495B-9F5E-123F9AAF2F59')

Declare @SignalTypeId int = null;
Set @SignalTypeId = (Select Top 1 Id from SignalType Where Name = 'No Absentee Communication')
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
Join Person p on pa.PersonId = p.Id
Join AttributeValue lastTime on lastTime.EntityId = p.Id and lastTime.ValueAsDateTime = a.StartDateTime and lastTime.AttributeId = @LastAttendedAttributeId
Where (
		(@WeekFilter like '%4week%' and a.StartDateTime <= @4WeekMark and a.StartDateTime > @8WeekMark) or
		(@WeekFilter like '%8week%' and a.StartDateTime <= @8WeekMark and a.StartDateTime > @16WeekMark) or
		(@WeekFilter like '%16week%' and a.StartDateTime <= @16WeekMark and a.StartDateTime > @26WeekMark) or
		(@WeekFilter like '%26week%' and a.StartDateTime <= @26WeekMark)
		)
and gt.AttendanceCountsAsWeekendService = 1
and p.ConnectionStatusValueId = 146
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
Left Join PersonSignal ps on ps.PersonId = p.Id and ps.SignalTypeId = @SignalTypeId and ps.ExpirationDate > GETDATE()
Where ( @CampusIds = 'null' or c.Id in (Select * From ufnUtility_CsvToTable(@CampusIds)))
And ps.Id is null" );
        }
        public override void Down()
        {
            }
    }
}
