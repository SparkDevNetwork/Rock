/* !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

	Populate All FellowshipOne People & Groups

This script imports all the people and groups that are in FellowshipOne's SQL database.
There is another script to import people attendance once this script is run.

The following assumptions or actions are made:

1. Source database is named F1
2. Current database is RockChMS
3. The Populate_FellowshipOne_HelperFunctions has been run
4. Either a checkin workflow or migration has been run to add CheckIn properties
5. Households or people with a ".", "?", or "_" in their name are excluded. 
6. Person roles are assigned by their age: 19+ is an adult, otherwise a child
7. FellowshipOne database ID's are stored as a person's attribute after the import finishes.
8. Person graduation dates are calculated if a grade can be found in the location name 
	where they checked in. If not, the graduation date is 18 + birth date.

!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! */

BEGIN TRANSACTION

----------------------------------------------------------
-- Set up necessary Person attributes and properties
----------------------------------------------------------

DECLARE @IntFieldType int
SELECT @IntFieldType = [ID] from FieldType where guid = 'A75DFC58-7A1B-4799-BF31-451B2BBE38FF'

DECLARE @PersonRecordType int
SET @PersonRecordType = (SELECT id FROM DefinedValue WHERE guid = '36CF10D6-C695-413D-8E7C-4546EFEF385E')

DECLARE @ActiveRecordStatus int
SET @ActiveRecordStatus = (SELECT id FROM DefinedValue WHERE guid = '618F906C-C33D-4FA3-8AEF-E58CB7B63F1E')

DECLARE @PrimaryPhone int
SET @PrimaryPhone = (SELECT id FROM DefinedValue WHERE guid = '407E7E45-7B2E-4FCD-9605-ECB1339F2453')

DECLARE @LocationHome int 
SELECT @LocationHome = (select id from DefinedValue where guid = '8C52E53C-2A66-435A-AE6E-5EE307D9A0DC')

DECLARE @FamilyGroupType int
SET @FamilyGroupType = (SELECT id FROM GroupType WHERE guid = '790E3215-3B10-442B-AF69-616C0DCB998E')

DECLARE @KnownRelationshipsGroupType int
SELECT @KnownRelationshipsGroupType = [Id] FROM GroupType WHERE guid = 'E0C5A0E2-B7B3-4EF4-820D-BBF7F9A374EF'

DECLARE @AdultRole int
SET @AdultRole = (SELECT id FROM GroupTypeRole WHERE guid = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42')

DECLARE @ChildRole int
SET @ChildRole = (SELECT id FROM GroupTypeRole WHERE guid = 'C8B1814F-6AA7-4055-B2D7-48FE20429CB9')

DECLARE @AbilityAttributeId int
SET @AbilityAttributeId = (SELECT id FROM Attribute WHERE guid = '4ABF0BF2-49BA-4363-9D85-AC48A0F7E92A')

DECLARE @PersonEntityID int
SELECT @PersonEntityID = [ID] FROM [EntityType] WHERE [Guid] = '72657ED8-D16E-492E-AC12-144C5E7567E7'

DECLARE @GroupEntityID int
SELECT @GroupEntityID = [ID] FROM [EntityType] WHERE [Guid] = '9BBFDA11-0D22-40D5-902F-60ADFBC88987'

DECLARE @OwnerKnownRelationshipsRole int
SELECT @OwnerKnownRelationshipsRole = [ID] FROM [GroupTypeRole] WHERE [Guid] = '7BC6C12E-0CD1-4DFD-8D5B-1B35AE714C42'

DECLARE @CanCheckInRole int
SELECT @CanCheckInRole = [ID] FROM [GroupTypeRole] WHERE [Guid] = 'DC8E5C35-F37C-4B49-A5C6-DF3D94FC808F'

DECLARE @NextGradeTransitionDate date
SELECT @NextGradeTransitionDate = CASE 
	WHEN (REPLACE([DefaultValue], '/', '-' ) + '-') + CONVERT(varchar(4), YEAR(GETDATE())) <= GETDATE()
		THEN (REPLACE([DefaultValue], '/', '-' ) + '-') + CONVERT(varchar(4), YEAR(GETDATE()) + 1)
	ELSE (REPLACE([DefaultValue], '/', '-' ) + '-') + CONVERT(varchar(4), YEAR(GETDATE())) 
END
FROM Attribute 
WHERE [Key] = 'GradeTransitionDate'
--select @NextGradeTransitionDate

------------------------------------------------------------
-- Add Person Attributes for IndividualID and HouseholdID
------------------------------------------------------------
DECLARE @IndividualAttributeID int
DECLARE @HouseholdAttributeID int
DECLARE @GroupAttributeID int

MERGE INTO [Attribute] A
USING (
	VALUES ( 0,	@IntFieldType,	@PersonEntityID, '', '', 'IndividualID',	'IndividualID',	'A person''s FellowshipOne individual ID',	0,	0,	0,	0,	'0F9C847B-4302-421E-9980-51E4772E80F5')	
	, (0,	@IntFieldType,	@PersonEntityID, '', '', 'HouseholdID',	'HouseholdID',	'A person''s FellowshipOne household ID',	0,	0,	0,	0,	'CAB397A9-5E72-4970-AF27-E60967FD6D58')
	, (0, @IntFieldType, @GroupEntityID, 'GroupTypeId', @FamilyGroupType, 'HouseholdID', 'HouseholdID', 'A group''s FellowshipOne household ID', 0, 0, 0, 0, '1DF676F2-A1C9-4534-9F59-8ABCBDD1CDD3')
) AS E (IsSystem,	FieldTypeId, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, [Key], Name, [Description], [Order], IsGridColumn, IsMultiValue, IsRequired, [Guid])
ON A.[Guid] = E.[Guid]
WHEN NOT MATCHED THEN
	INSERT (IsSystem, FieldTypeId, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, [Key], Name, [Description], [Order], IsGridColumn, IsMultiValue, IsRequired, [Guid])
	VALUES (E.IsSystem,	E.FieldTypeId,	E.EntityTypeId,	E.EntityTypeQualifierColumn, E.EntityTypeQualifierValue, E.[Key], E.Name, E.[Description], E.[Order], E.IsGridColumn, E.IsMultiValue, E.IsRequired, E.[Guid]);
		 
SELECT @IndividualAttributeID = [ID] FROM [Attribute] WHERE [Guid] = '0F9C847B-4302-421E-9980-51E4772E80F5'
SELECT @HouseholdAttributeID = [ID] FROM [Attribute] WHERE [Guid] = 'CAB397A9-5E72-4970-AF27-E60967FD6D58'
SELECT @GroupAttributeID = [ID] FROM [Attribute] WHERE [Guid] = '1DF676F2-A1C9-4534-9F59-8ABCBDD1CDD3'

------------------------------------------------------------
-- Add Group for each Household in F1
------------------------------------------------------------
CREATE TABLE #InsertedGroups (
	HouseholdID int
	, GroupID int
	, GroupName nvarchar(500) 
);

MERGE [Group] AS G
USING (		
	SELECT DISTINCT Household_ID, dbo.FormatName(Household_Name, 'L') + ' Family'
	FROM F1.dbo.Individual_Household
	WHERE Household_Name NOT LIKE '%[_?]%'
	AND LEN(Last_Name) > 1
) AS source (HouseholdID, GroupName)
ON 0 = 1
WHEN NOT MATCHED THEN
	INSERT ( IsSystem, GroupTypeId, Name, IsSecurityRole, IsActive, [Order], [Guid] )
	VALUES ( 0, @FamilyGroupType, GroupName, 0, 0, 0, NEWID() )
	OUTPUT source.HouseholdID, Inserted.Id, Inserted.Name INTO #InsertedGroups;

INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], Value, [Guid])
SELECT 0, @GroupAttributeID, GroupID, 0, HouseholdID, NEWID()
FROM #InsertedGroups IG

DROP TABLE #InsertedGroups

------------------------------------------------------------
-- Add Person for each Individual in F1
------------------------------------------------------------
CREATE TABLE #InsertedPeople (
	HouseholdID int
	, IndividualID int
	, PersonID int
	, BirthDate date
);

MERGE Person AS P
USING (
	SELECT DISTINCT i.Household_ID, i.Individual_ID, dbo.CleanCase(i.First_Name), i.goes_by, dbo.CleanCase(i.middle_name), dbo.CleanCase(i.Last_Name), i.gender, i.date_of_birth
	FROM F1.dbo.Individual_Household i
	WHERE Household_Name NOT LIKE '%[_?]%'
	AND LEN(Last_Name) > 1
) AS source ( HouseholdID, IndividualID, FirstName, NickName, MiddleName, LastName, Gender, birthdate )
ON 0 = 1
WHEN NOT MATCHED THEN
	INSERT ( IsSystem, GivenName, NickName, MiddleName, LastName, BirthDay, BirthMonth, BirthYear, Gender, DoNotEmail, SystemNote, [Guid],[RecordTypeValueId],[RecordStatusValueId] )
	VALUES ( 0, FirstName, NickName, MiddleName, LastName, day(birthdate), month(birthdate), year(birthdate)
	, CASE Gender WHEN 'Male' THEN 1 WHEN 'Female' THEN 2 ELSE 0 END
	, 0, 'Imported from FellowshipOne', NEWID(), @PersonRecordType, @ActiveRecordStatus )
	OUTPUT source.HouseholdID, source.IndividualID, Inserted.Id, source.BirthDate INTO #InsertedPeople;

INSERT [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], Value, [Guid])
SELECT 0, @IndividualAttributeID, PersonId, 0, IndividualID, NEWID()
FROM #InsertedPeople

INSERT [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], Value, [Guid])
SELECT 0, @HouseholdAttributeID, PersonId, 0, HouseholdID, NEWID()
FROM #InsertedPeople

DROP TABLE #InsertedPeople

------------------------------------------------------------
-- Add Family member group memberships 
------------------------------------------------------------
;WITH F1FamilyMembers AS (
	SELECT DISTINCT Household_ID
	, Individual_ID
	, CASE WHEN (Household_Position = 'Head' OR Household_Position = 'Spouse')
		AND DATEDIFF(YEAR, Date_Of_Birth, @NextGradeTransitionDate ) >= 19
		THEN @AdultRole
	WHEN DATEDIFF(YEAR, Date_Of_Birth, @NextGradeTransitionDate ) < 19
		OR Household_Position = 'Child'
		THEN @ChildRole
	ELSE @AdultRole
	END AS PersonRole
	FROM F1.dbo.individual_household
	WHERE Household_Name NOT LIKE '%[_?]%'
	AND LEN(Last_Name) > 1
	AND Household_Position <> 'Visitor' 		
)
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, GroupMemberStatus, Guid)
SELECT 0, Household.EntityID, Individual.EntityId, FGM.PersonRole, 1, NEWID()
FROM F1FamilyMembers FGM
LEFT JOIN AttributeValue AS Household
	ON Household.AttributeId = @GroupAttributeID
	AND Household.Value = FGM.Household_ID
LEFT JOIN AttributeValue AS Individual
	ON Individual.AttributeId = @IndividualAttributeID
	AND Individual.Value = FGM.Individual_ID

------------------------------------------------------------
-- Add Visitor group memberships 
------------------------------------------------------------
CREATE TABLE #InsertedVisitors (
	HouseholdID int
	, IndividualID int
	, CanCheckInGroupID int
);

-- Create group for visitor relationship
MERGE [Group] AS G
USING (
	SELECT DISTINCT Household_ID
	, Individual_ID
	, 'Known Relationships'	
	FROM F1.dbo.individual_household
	WHERE Household_Name NOT LIKE '%[_?]%'
	AND LEN(Last_Name) > 1
	AND Household_Position = 'Visitor'
) AS source (HouseholdID, IndividualID, GroupName)
ON 0 = 1
WHEN NOT MATCHED THEN
	INSERT ( IsSystem, GroupTypeId, Name, IsSecurityRole, IsActive, [Order], [Guid] )
	VALUES ( 0, @KnownRelationshipsGroupType, GroupName, 0, 0, 0, NEWID() )
	OUTPUT source.HouseholdID, source.IndividualID, Inserted.Id
	INTO #InsertedVisitors ( HouseholdID, IndividualID, CanCheckInGroupID );

-- Add household members with Owner role
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, GroupMemberStatus, Guid)
SELECT 0, IV.CanCheckInGroupID, GM.PersonId, @OwnerKnownRelationshipsRole, 1, NEWID()
FROM #InsertedVisitors IV
INNER JOIN AttributeValue AV
	ON AV.AttributeId = @HouseholdAttributeID
	AND AV.Value = IV.HouseholdID
INNER JOIN GroupMember GM
	ON GM.PersonId = AV.EntityId

-- Add visitor as member of the group with CanCheckIn role
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, GroupMemberStatus, Guid)
SELECT 0, IV.CanCheckInGroupID, AV.EntityId, @CanCheckInRole, 1, NEWID()
FROM #InsertedVisitors IV
INNER JOIN AttributeValue AS AV
	ON AV.AttributeId = @IndividualAttributeID
	AND AV.Value = IV.IndividualID

--select * from #InsertedVisitors
		
------------------------------------------------------------
-- Set person email
------------------------------------------------------------
;WITH RankedContactInformation AS (
	SELECT Household_ID, Individual_ID, Communication_Value, Listed, Communication_Comment, Communication_Type
	, ROW_NUMBER() OVER (
		PARTITION BY Household_ID, Individual_ID, Communication_Type
		ORDER BY LastUpdatedDate DESC
	) as 'EmailRank'
	FROM F1.dbo.Communication
	WHERE Communication_Value LIKE '%@%'
)
UPDATE P
SET Email = RCI.Communication_Value
, IsEmailActive = RCI.Listed
, EmailNote = ISNULL(RCI.Communication_Comment, RCI.Communication_Type)
, DoNotEmail = RCI.Listed
FROM Person P
INNER JOIN AttributeValue AV
	ON AV.AttributeId = @IndividualAttributeID
	AND P.Id = AV.EntityId
INNER JOIN RankedContactInformation RCI
	ON AV.Value = RCI.Individual_ID
	AND RCI.EmailRank = 1

------------------------------------------------------------
-- Calculate and set Person graduation date
------------------------------------------------------------
;WITH LastAttendedAge AS (
	SELECT IH.Individual_ID
	, Date_Of_Birth AS 'BirthDate'
	, R.RLC_Name AS 'Location'
	, A.Start_Date_Time AS 'CheckInDate'	
	, ROW_NUMBER() OVER (
		PARTITION BY A.Individual_ID
		ORDER BY A.Start_Date_Time DESC
	) AS 'AttendanceRank'
	FROM F1.dbo.Attendance A
	INNER JOIN F1.dbo.Individual_Household IH
	ON A.Individual_ID = IH.Individual_ID
	AND Household_Name NOT LIKE '%[_?]%'
	AND LEN(Last_Name) > 1
	INNER JOIN F1.dbo.RLC R
	ON A.RLC_ID = R.RLC_ID
)
, LastAttendedGrade AS (
	SELECT Individual_ID, BirthDate, CheckInDate, Location
	, DATEDIFF(year, BirthDate, GETDATE()) AS 'CurrentAge'	
	, DATEDIFF(year, BirthDate, CheckInDate) AS 'CheckInAge'		
	-- Look for a grade in the location name if it contains a number
	, CASE WHEN Location LIKE '%[10-12]th%'
			THEN SUBSTRING(Location, PATINDEX('%[10-12]%', Location), 2)		
		WHEN Location LIKE '%[4-9]th%'
			THEN SUBSTRING(Location, PATINDEX('%[1-9]%', Location), 1)
		WHEN Location LIKE '%[2-3]_d%'
			THEN SUBSTRING(Location, PATINDEX('%[1-9]%', Location), 1)		
		WHEN Location LIKE '%[1]st%'
			THEN SUBSTRING(Location, PATINDEX('%[1-9]%', Location), 1)
		ELSE NULL
	END AS 'CheckInGrade'
	FROM LastAttendedAge	
	WHERE PATINDEX('%[1-9]%', Location) > 0
)
UPDATE P
SET GraduationDate = CASE
	WHEN LAG.CheckInGrade IS NOT NULL
	THEN -- calculate by check-in grade
		DATEADD(year, (12 - LAG.CheckInGrade) - DATEDIFF(year, LAG.CheckInDate, @NextGradeTransitionDate), @NextGradeTransitionDate)
	ELSE -- calculate by age (approximate)
		DATEADD(year, 18 - datediff(year, LAA.BirthDate, @NextGradeTransitionDate), @NextGradeTransitionDate )
END
FROM Person P
INNER JOIN AttributeValue AV
ON AV.AttributeId = @IndividualAttributeID
AND P.Id = AV.EntityId
INNER JOIN LastAttendedAge LAA
ON AV.Value = LAA.Individual_ID
AND LAA.AttendanceRank = 1
LEFT JOIN LastAttendedGrade LAG
ON AV.Value = LAG.Individual_ID
WHERE DATEDIFF(YEAR, LAA.BirthDate, @NextGradeTransitionDate ) <= 19

------------------------------------------------------------
-- Set person status
------------------------------------------------------------

/* !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!


	        NOT YET IMPLEMENTED


!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! */


------------------------------------------------------------
-- Add Person attendance
------------------------------------------------------------
;WITH F1Attendance AS (
	SELECT A.Individual_ID AS 'IndividualID'
	, A.Start_Date_Time AS 'ScheduleDate'
	, A.Check_In_Time AS 'CheckInTime'
	, R.RLC_Name AS 'GroupName'
	, A.CheckedInAs	
	FROM F1.dbo.Attendance A
	INNER JOIN F1.dbo.RLC R
	ON A.RLC_ID = R.RLC_ID
)
INSERT Attendance (LocationId, ScheduleId, GroupId, PersonId, StartDateTime, Note, DidAttend, [Guid])
SELECT L.Id, S.Id, G.Id, P.Id, ISNULL(F.CheckInTime, F.ScheduleDate), 'Imported from F1', 1, NEWID()
FROM Person P
INNER JOIN AttributeValue AV
	ON AV.AttributeId = @IndividualAttributeID
	AND AV.EntityId = P.Id
INNER JOIN F1Attendance F
	ON F.IndividualID = AV.Value
INNER JOIN [Group] G
	ON F.GroupName = G.Name
INNER JOIN Location L
	ON F.GroupName = L.Name
INNER JOIN Schedule S
	ON CAST(F.ScheduleDate as time) = CAST(S.Name as time)
GROUP BY L.Id, S.Id, G.Id, P.Id, ISNULL(F.CheckInTime, F.ScheduleDate)
ORDER BY ISNULL(F.CheckInTime, F.ScheduleDate)

COMMIT