----------------------------------------------------------
-- Set up necessary Person attributes and properties
----------------------------------------------------------

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

DECLARE @AdultRole int
SET @AdultRole = (SELECT id FROM GroupRole WHERE guid = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42')

DECLARE @ChildRole int
SET @ChildRole = (SELECT id FROM GroupRole WHERE guid = 'C8B1814F-6AA7-4055-B2D7-48FE20429CB9')

DECLARE @AbilityAttributeId int
SET @AbilityAttributeId = (SELECT id FROM Attribute WHERE guid = '4ABF0BF2-49BA-4363-9D85-AC48A0F7E92A')

DECLARE @PersonEntityID int
SELECT @PersonEntityID = [ID] FROM [EntityType] WHERE [Guid] = '72657ED8-D16E-492E-AC12-144C5E7567E7'

DECLARE @GradeTransitionDate date
SELECT @GradeTransitionDate = (replace([DefaultValue], '/', '-' ) + '-') + convert(varchar(4), year(getdate()))
FROM Attribute WHERE [Key] = 'GradeTransitionDate'

DECLARE @GroupId int
DECLARE @PersonId int
DECLARE @LocationId int
DECLARE @IndividualAttrID int
DECLARE @HouseholdAttrID int

------------------------------------------------------------
-- Add Person Attributes for IndividualID and HouseholdID
------------------------------------------------------------
MERGE INTO [Attribute] A
USING (
	VALUES (0,	7,	@PersonEntityID, 'IndividualID',	'IndividualID',	'F1 Individual ID',	0,	0,	0,	0,	'0F9C847B-4302-421E-9980-51E4772E80F5')	
	, (0,	7,	@PersonEntityID,	'HouseholdID',	'HouseholdID',	'F1 Household ID',	0,	0,	0,	0,	'CAB397A9-5E72-4970-AF27-E60967FD6D58')
) AS E (IsSystem,	FieldTypeId,	EntityTypeId,	[Key], Name, [Description], [Order], IsGridColumn, IsMultiValue, IsRequired, [Guid])
ON A.[Guid] = E.[Guid]
WHEN NOT MATCHED THEN
	INSERT (IsSystem,	FieldTypeId,	EntityTypeId,	[Key], Name, [Description], [Order], IsGridColumn, IsMultiValue, IsRequired, [Guid])
	VALUES (E.IsSystem,	E.FieldTypeId,	E.EntityTypeId,	E.[Key], E.Name, E.[Description], E.[Order], E.IsGridColumn, E.IsMultiValue, E.IsRequired, E.[Guid]);
		 
SELECT @IndividualAttrID = [ID] FROM [Attribute] WHERE [Guid] = '0F9C847B-4302-421E-9980-51E4772E80F5'
SELECT @HouseholdAttrID = [ID] FROM [Attribute] WHERE [Guid] = 'CAB397A9-5E72-4970-AF27-E60967FD6D58'

------------------------------------------------------------
-- Add Group for each Household in F1
------------------------------------------------------------

MERGE INTO [Group] G
USING (
	SELECT distinct household_id, household_name
	from F1.dbo.Individual_Household i	
	WHERE Household_Name not like '%[_.?]%'		
) AS R ( h_id, Name )
ON G.Name = R.Name
WHEN NOT MATCHED THEN
	INSERT ( IsSystem, GroupTypeId, Name, IsSecurityRole, IsActive, [Guid])
	VALUES ( 0, @FamilyGroupType, R.Name, 0, 1, newid() );

------------------------------------------------------------
-- Add Person for each Individual in F1
------------------------------------------------------------

MERGE INTO Person P
USING (
	SELECT i.First_Name, i.goes_by, i.middle_name, i.Last_Name, i.gender, i.date_of_birth
	from F1.dbo.Individual_Household i
	WHERE Household_Name not like '%[_.?]%'	
) AS R ( FirstName, NickName, MiddleName, LastName, Gender, birthdate )
ON P.FirstName = R.FirstName
AND P.LastName = R.LastName
AND p.birthyear = year(R.birthdate)
AND p.birthmonth = month(R.birthdate)
AND p.birthday = day(R.birthdate)
WHEN NOT MATCHED THEN
	INSERT ( IsSystem, GivenName, NickName, MiddleName, LastName, BirthDay, BirthMonth, BirthYear, Gender, SystemNote, [Guid],[RecordTypeValueId],[RecordStatusValueId] )
	VALUES ( 0, R.FirstName, R.NickName, R.MiddleName, R.LastName, day(R.birthdate), month(R.birthdate), year(R.birthdate)
	, CASE R.Gender WHEN 'Male' THEN 1 WHEN 'Female' THEN 2 ELSE 0 END
	, 'Imported from FellowshipOne', newid(), @PersonRecordType, @ActiveRecordStatus );

------------------------------------------------------------
-- Add Person attribute values for the F1 ID's
------------------------------------------------------------

DECLARE @PersonAttributeIDs
TABLE ( PersonId int, Individual_id int, Household_id int )
INSERT @PersonAttributeIDs
	SELECT p.Id, ih.individual_id, ih.household_id
	FROM Person p
	INNER JOIN F1.dbo.individual_household ih
	ON p.FirstName = ih.first_name
	AND p.lastname = ih.last_name
	AND p.birthyear = year(ih.date_of_birth)
	AND p.birthmonth = month(ih.date_of_birth)
	AND p.birthday = day(ih.date_of_birth)

INSERT [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], Value, [Guid])
SELECT 0, @IndividualAttrID, PersonId, 0, Individual_id, NEWID()
FROM @PersonAttributeIDs

INSERT [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], Value, [Guid])
SELECT 0, @HouseholdAttrID, PersonId, 0, Household_id, NEWID()
FROM @PersonAttributeIDs

------------------------------------------------------------
-- Add Person group memberships 
------------------------------------------------------------

;WITH F1GroupMembers AS (
	SELECT Household_Name
	, Household_ID
	, Individual_ID
	, CASE WHEN (Household_Position = 'Head' OR Household_Position = 'Spouse')
		AND DATEDIFF(YEAR, Date_Of_Birth, GETDATE() ) >= 18 
		THEN @AdultRole
	WHEN (Household_Position = 'Child' OR Household_Position = 'Visitor')
		AND DATEDIFF(YEAR, Date_Of_Birth, GETDATE() ) < 18
		THEN @ChildRole	
	WHEN DATEDIFF(YEAR, Date_Of_Birth, GETDATE() ) < 18
		THEN @ChildRole
		ELSE @AdultRole
	END as PersonRole 
	FROM F1.dbo.individual_household	
)
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
SELECT 0, G.Id, AV.EntityId, FGM.PersonRole, newid()
FROM [Group] G
INNER JOIN F1GroupMembers FGM
ON G.Name = FGM.Household_Name
INNER JOIN AttributeValue AV
ON FGM.Individual_ID = AV.Value

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
	ON AV.AttributeId = @IndividualAttrID
	AND P.Id = AV.EntityId
INNER JOIN RankedContactInformation RCI
	ON AV.Value = RCI.Individual_ID
	AND RCI.EmailRank = 1

------------------------------------------------------------
-- Set person status
------------------------------------------------------------

/* !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!


	        NOT YET IMPLEMENTED


!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! */


------------------------------------------------------------
-- Calculate and set Person graduation date
------------------------------------------------------------

;WITH LastAttendedAge AS (
	SELECT IH.Individual_ID
	, Date_Of_Birth AS 'BirthDate'
	, R.RLC_Name AS 'Location'
	, A.Start_Date_Time AS 'CheckInDate'
	, R.Start_Age_Date AS 'MinAge'
	, R.End_Age_date AS 'MaxAge'
	FROM F1.dbo.Attendance A
	INNER JOIN F1.dbo.Individual_Household IH
	ON A.Individual_ID = IH.Individual_ID
	INNER JOIN F1.dbo.RLC R
	ON A.RLC_ID = R.RLC_ID
	AND R.Start_Age_Date IS NOT NULL
)
, LastAttendedGrade AS (
	SELECT Individual_ID, BirthDate, CheckInDate
	, DATEDIFF(year, BirthDate, GETDATE()) AS 'CurrentAge'	
	, DATEDIFF(year, BirthDate, CheckInDate) AS 'CheckInAge'		
	-- assume the location name has a Grade if it contains a number
	, CASE WHEN PATINDEX('%[0-9][0-2]%', Location) > 0
			THEN SUBSTRING(Location, PATINDEX('%[0-9]%', Location), 2)
		WHEN PATINDEX('%[0-9]%', Location) > 0
			THEN SUBSTRING(Location, PATINDEX('%[0-9]%', Location), 1)
		ELSE NULL
	END AS 'CheckInGrade'
	, ROW_NUMBER() OVER (
		PARTITION BY Individual_ID
		ORDER BY CheckInDate DESC
	) AS 'AttendanceRank'
	FROM LastAttendedAge	
	WHERE PATINDEX('%[0-9]%', Location) > 0
)
UPDATE P
SET GraduationDate = @GradeTransitionDate -- + CurrentGrade
FROM Person P
INNER JOIN AttributeValue AV
ON AV.AttributeId = @IndividualAttrID
AND P.Id = AV.EntityId
INNER JOIN LastAttendedGrade LAG
ON AV.Value = LAG.Individual_ID



