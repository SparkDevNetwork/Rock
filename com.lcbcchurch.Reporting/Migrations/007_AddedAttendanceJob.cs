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
    [MigrationNumber( 7, "1.0.14" )]
    public class AddAttendanceJob : Migration
    {
        public override void Up()
        {
            RockMigrationHelper.AddGroupType( "F1 Weekend Gathering", "", "Group", "Member", false, true, true, "", 0, "", 0, "", "EA3530C1-7A0C-45A3-9899-CF184F2B8D42" );
            RockMigrationHelper.UpdateGroupTypeRole( "EA3530C1-7A0C-45A3-9899-CF184F2B8D42", "Member", "", 0, null, null, "EB951E95-BD73-4228-AFCC-DDB49CBC7538", false, false, true );

            RockMigrationHelper.UpdatePersonAttributeCategory( "Next Steps", "", "", "FAC12B3C-CB73-4DC6-8A54-3C3F5F3A312D" );
            RockMigrationHelper.UpdatePersonAttribute( "6B6AA175-4758-453F-8D83-FCD8044B5F36", "FAC12B3C-CB73-4DC6-8A54-3C3F5F3A312D", "First Weekend Attendance", "LCBCFirstVisit", "", "", 1009, "", "1859E6AE-DD4B-49D1-83B3-5EC6A9D029CB" );
            RockMigrationHelper.UpdatePersonAttribute( "6B6AA175-4758-453F-8D83-FCD8044B5F36", "FAC12B3C-CB73-4DC6-8A54-3C3F5F3A312D", "Second Weekend Attendance", "LCBCSecondVisit", "", "", 1010, "", "FBF88BC2-F99F-45BF-85F9-1DE91C26C4BD" );
            RockMigrationHelper.UpdatePersonAttribute( "6B6AA175-4758-453F-8D83-FCD8044B5F36", "FAC12B3C-CB73-4DC6-8A54-3C3F5F3A312D", "Third Weekend Attendance", "LCBCThirdVisit", "", "", 1011, "", "4208456B-67B2-4B37-8F4B-547DF48A89FC" );
            RockMigrationHelper.UpdatePersonAttribute( "6B6AA175-4758-453F-8D83-FCD8044B5F36", "FAC12B3C-CB73-4DC6-8A54-3C3F5F3A312D", "Last Weekend Attendance", "LCBCLastVisit", "", "", 1012, "", "9ABC5D6B-4840-495B-9F5E-123F9AAF2F59" );
            // add job service
            Sql( @"  INSERT INTO [ServiceJob]
                          ([IsSystem], [IsActive], [Name], [Description], [Class], [CronExpression], [NotificationStatus], [Guid])
                          VALUES 
                          (0, 0, 'Calculate Weekend Gathering Attendance', 'Job that populates the Next Steps Visit attributes', 'Rock.Jobs.RunSQL', '0 0 20 ? * SUN *', 1, '86C2F5FE-FCD1-4E07-83B1-7C7E621F06C6')" );

            var systemJobId = SqlScalar( "Select Top 1 Id From ServiceJob Where Guid = '86C2F5FE-FCD1-4E07-83B1-7C7E621F06C6'" ).ToString().AsInteger();
            RockMigrationHelper.AddAttributeValue( "FF66ABF1-B01D-4AE7-814E-95D842B2EA99", systemJobId, "3600", "AC25616F-90DD-4162-97DF-C6EBD73E2B2A" );
            RockMigrationHelper.AddAttributeValue( "7AD0C57A-D40E-4A14-81D8-8ACA68600FF5", systemJobId, @"
DECLARE @FirstVisitAttributeId int = (SELECT [Id] FROM [Attribute] WHERE [Guid] = ''1859E6AE-DD4B-49D1-83B3-5EC6A9D029CB'')
DECLARE @SecondVisitAttributeId int = (SELECT [Id] FROM [Attribute] WHERE [Guid] = ''FBF88BC2-F99F-45BF-85F9-1DE91C26C4BD'')
DECLARE @ThirdVisitAttributeId int = (SELECT [Id] FROM [Attribute] WHERE [Guid] = ''4208456B-67B2-4B37-8F4B-547DF48A89FC'')
DECLARE @LastAttendedAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = ''9ABC5D6B-4840-495B-9F5E-123F9AAF2F59'')

DELETE FROM [AttributeValue] WHERE ([Value] = '''' OR [Value] IS NULL) AND AttributeId = @FirstVisitAttributeId
DELETE FROM [AttributeValue] WHERE ([Value] = '''' OR [Value] IS NULL) AND AttributeId = @SecondVisitAttributeId
DELETE FROM [AttributeValue] WHERE ([Value] = '''' OR [Value] IS NULL) AND AttributeId = @ThirdVisitAttributeId
DELETE FROM [AttributeValue] WHERE ([Value] = '''' OR [Value] IS NULL) AND AttributeId = @LastAttendedAttributeId
IF @FirstVisitAttributeId IS NOT NULL
BEGIN
    INSERT INTO [AttributeValue]
    ([IsSystem], [AttributeId], [EntityId], [Value], [Guid], [CreatedDateTime])
    SELECT 1, @FirstVisitAttributeId, p.Id, CONVERT(varchar(50), a.FirstVisit, 126), newid(), getdate()
    FROM [Person] p
    CROSS APPLY (
        SELECT
            MIN([StartDateTime]) [FirstVisit]
        FROM
            [Attendance] a
        INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
        INNER JOIN [Group] g ON g.Id = O.GroupId
        INNER JOIN [GroupType] gt ON gt.Id = g.GroupTypeId
        INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
        WHERE
            gt.Guid in (''531320FA-9BD1-494A-B512-EDBA1262B93D'',''EA3530C1-7A0C-45A3-9899-CF184F2B8D42'')
            AND a.[DidAttend] = 1
            AND pa.[PersonId] = p.[Id]
    ) a
    WHERE
        p.[Id] NOT IN (SELECT [EntityId] FROM [AttributeValue] WHERE [AttributeId] = @FirstVisitAttributeId )
END

IF @SecondVisitAttributeId IS NOT NULL
BEGIN
    INSERT INTO [AttributeValue]
    ([IsSystem], [AttributeId], [EntityId], [Value], [Guid], [CreatedDateTime])
    SELECT 1, @SecondVisitAttributeId, p.Id, CONVERT(varchar(50), a.[SecondVisit], 126), newid(), getdate()
    FROM [Person] p
    CROSS APPLY (
        SELECT
        MIN([StartDateTime]) [SecondVisit]
        FROM
        [Attendance] a
        INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
        INNER JOIN [Group] g ON g.Id = O.GroupId
        INNER JOIN [GroupType] gt ON gt.Id = g.GroupTypeId
        INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
        WHERE
            gt.Guid in (''531320FA-9BD1-494A-B512-EDBA1262B93D'',''EA3530C1-7A0C-45A3-9899-CF184F2B8D42'')
            AND pa.[PersonId] = p.[Id]
            AND a.[DidAttend] = 1
            AND CONVERT(date, a.[StartDateTime]) > (SELECT MAX([ValueAsDateTime]) FROM [AttributeValue] WHERE [AttributeId] = @FirstVisitAttributeId AND [EntityId] = pa.[PersonId] AND [IsSystem] = 1)
    ) a
    WHERE
        p.[Id] NOT IN (SELECT [EntityId] FROM [AttributeValue] WHERE [AttributeId] = @SecondVisitAttributeId )
        AND p.[Id] IN (SELECT [EntityId] FROM [AttributeValue] WHERE [AttributeId] = @FirstVisitAttributeId AND [ValueAsDateTime] IS NOT NULL )
        AND a.[SecondVisit] IS NOT NULL
END

IF @ThirdVisitAttributeId IS NOT NULL
BEGIN
    INSERT INTO [AttributeValue]
    ([IsSystem], [AttributeId], [EntityId], [Value], [Guid], [CreatedDateTime])
    SELECT 1, @ThirdVisitAttributeId, p.Id, CONVERT(varchar(50), a.[SecondVisit], 126), newid(), getdate()
    FROM [Person] p
    CROSS APPLY (
        SELECT
        MIN([StartDateTime]) [SecondVisit]
        FROM
        [Attendance] a
        INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
        INNER JOIN [Group] g ON g.Id = O.GroupId
        INNER JOIN [GroupType] gt ON gt.Id = g.GroupTypeId
        INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
        WHERE
            gt.Guid in (''531320FA-9BD1-494A-B512-EDBA1262B93D'',''EA3530C1-7A0C-45A3-9899-CF184F2B8D42'')
            AND pa.[PersonId] = p.[Id]
            AND a.[DidAttend] = 1
            AND CONVERT(date, a.[StartDateTime]) > (SELECT MAX([ValueAsDateTime]) FROM [AttributeValue] WHERE [AttributeId] = @SecondVisitAttributeId AND [EntityId] = pa.[PersonId] AND [IsSystem] = 1)
    ) a
    WHERE
        p.[Id] NOT IN (SELECT [EntityId] FROM [AttributeValue] WHERE [AttributeId] = @ThirdVisitAttributeId )
        AND p.[Id] IN (SELECT [EntityId] FROM [AttributeValue] WHERE [AttributeId] = @SecondVisitAttributeId AND [ValueAsDateTime] IS NOT NULL )
        AND a.[SecondVisit] IS NOT NULL
END

DELETE FROM [AttributeValue] WHERE [AttributeId] = @LastAttendedAttributeId;
INSERT INTO AttributeValue ([EntityId], [AttributeId], [Value], [IsSystem], [Guid], [CreatedDateTime])

SELECT *
FROM (
    SELECT
        p.Id
        , @LastAttendedAttributeId AS [AttributeId]
        , (SELECT
            MAX(a.StartDateTime )
            FROM
                [Attendance] a
            INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
            INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
            INNER JOIN [Group] g ON g.Id = O.GroupId
            INNER JOIN [GroupType] gt ON gt.Id = g.GroupTypeId
            WHERE
            gt.Guid in (''531320FA-9BD1-494A-B512-EDBA1262B93D'',''EA3530C1-7A0C-45A3-9899-CF184F2B8D42'')
                AND a.[DidAttend] = 1
                AND pa.[PersonId] = p.Id)
        AS [LastAttendedDate]
        , 0 AS [IsSystem]
        , newid() AS [Guid]
        , getdate() AS [CreateDate]
    FROM Person p
) AS a
WHERE a.[LastAttendedDate] IS NOT NULL", "A30EFA82-0221-4982-9DFB-37A73986A247" );

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
        countTable.AttendanceDateTime as 'Visit Date',
        countTable.VisitNumber as 'Visit #'
From @TotalAttendanceCountTable countTable
Join Person p on countTable.PersonId = p.Id
Join GroupMember gm on p.Id = gm.PersonId
Join [Group] g on gm.GroupId = g.Id and g.Id = (SELECT TOP 1 [GroupId] 
					                FROM [GroupMember] gm 
					                INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] 
					                WHERE [PersonId] = p.Id AND g.[GroupTypeId] = 10)
Where countTable.VisitNumber in (Select * From ufnUtility_CsvToTable(@AttendanceNumbers))

Order By LastName, [First Name], [Visit #]" );

            // Attrib Value for Block:Dynamic Data, Attribute:Query Page: Absentee Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4A4A37D9-991A-485A-A961-BB9F31B9EAD9", "71C8BA4E-8EF2-416B-BFE9-D1D88D9AA356", @"Declare @WeekFilter nvarchar(max) = '4week'
Declare @CampusIds nvarchar(max) = 'null'
DECLARE @LastAttendedAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '9ABC5D6B-4840-495B-9F5E-123F9AAF2F59')

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
Where ( @CampusIds = 'null' or c.Id in (Select * From ufnUtility_CsvToTable(@CampusIds)))" );
        }
        public override void Down()
        {
        }
    }
}
