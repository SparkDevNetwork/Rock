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

DECLARE @AdultRole int
SET @AdultRole = (SELECT id FROM GroupRole WHERE guid = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42')

DECLARE @ChildRole int
SET @ChildRole = (SELECT id FROM GroupRole WHERE guid = 'C8B1814F-6AA7-4055-B2D7-48FE20429CB9')

DECLARE @AbilityAttributeId int
SET @AbilityAttributeId = (SELECT id FROM Attribute WHERE guid = '4ABF0BF2-49BA-4363-9D85-AC48A0F7E92A')

DECLARE @PersonEntityID int
SELECT @PersonEntityID = [ID] FROM [EntityType] WHERE [Guid] = '72657ED8-D16E-492E-AC12-144C5E7567E7'

DECLARE @GroupEntityID int
SELECT @GroupEntityID = [ID] FROM [EntityType] WHERE [Guid] = '9BBFDA11-0D22-40D5-902F-60ADFBC88987'

DECLARE @NextGradeTransitionDate date
SELECT @NextGradeTransitionDate = CASE 
	WHEN (REPLACE([DefaultValue], '/', '-' ) + '-') + CONVERT(varchar(4), YEAR(GETDATE())) <= GETDATE()
		THEN (REPLACE([DefaultValue], '/', '-' ) + '-') + CONVERT(varchar(4), YEAR(GETDATE()) + 1)
	ELSE (REPLACE([DefaultValue], '/', '-' ) + '-') + CONVERT(varchar(4), YEAR(GETDATE())) 
END
FROM Attribute 
WHERE [Key] = 'GradeTransitionDate'
--select @NextGradeTransitionDate

DECLARE @GroupId int
DECLARE @PersonId int
DECLARE @LocationId int
DECLARE @IndividualAttributeID int
DECLARE @HouseholdAttributeID int
DECLARE @GroupAttributeID int

------------------------------------------------------------
-- Add Person Attributes for IndividualID and HouseholdID
------------------------------------------------------------
MERGE INTO [Attribute] A
USING (
	VALUES ( 0,	@IntFieldType,	@PersonEntityID, 'IndividualID',	'IndividualID',	'F1 Individual ID',	0,	0,	0,	0,	'0F9C847B-4302-421E-9980-51E4772E80F5')	
	, (0,	@IntFieldType,	@PersonEntityID,	'HouseholdID',	'HouseholdID',	'F1 Household ID',	0,	0,	0,	0,	'CAB397A9-5E72-4970-AF27-E60967FD6D58')
	, (0, @IntFieldType, @GroupEntityID, 'HouseholdID', 'HouseholdID', 'F1 Household ID', 0, 0, 0, 0, '1DF676F2-A1C9-4534-9F59-8ABCBDD1CDD3')
) AS E (IsSystem,	FieldTypeId,	EntityTypeId,	[Key], Name, [Description], [Order], IsGridColumn, IsMultiValue, IsRequired, [Guid])
ON A.[Guid] = E.[Guid]
WHEN NOT MATCHED THEN
	INSERT (IsSystem,	FieldTypeId,	EntityTypeId,	[Key], Name, [Description], [Order], IsGridColumn, IsMultiValue, IsRequired, [Guid])
	VALUES (E.IsSystem,	E.FieldTypeId,	E.EntityTypeId,	E.[Key], E.Name, E.[Description], E.[Order], E.IsGridColumn, E.IsMultiValue, E.IsRequired, E.[Guid]);
		 
SELECT @IndividualAttributeID = [ID] FROM [Attribute] WHERE [Guid] = '0F9C847B-4302-421E-9980-51E4772E80F5'
SELECT @HouseholdAttributeID = [ID] FROM [Attribute] WHERE [Guid] = 'CAB397A9-5E72-4970-AF27-E60967FD6D58'
SELECT @GroupAttributeID = [ID] FROM [Attribute] WHERE [Guid] = '1DF676F2-A1C9-4534-9F59-8ABCBDD1CDD3'

------------------------------------------------------------
-- Add Group for each Household in F1
------------------------------------------------------------

IF EXISTS( SELECT * FROM SYS.TABLES WHERE NAME = 'InsertedGroups' )
BEGIN
	DROP TABLE #InsertedGroups
END

CREATE TABLE #InsertedGroups (
	HouseholdID int
	, GroupID int
	, GroupName nvarchar(max) 
);

MERGE [Group] AS G
USING (		
	SELECT DISTINCT Household_ID, Household_Name
	FROM F1.dbo.Individual_Household
	WHERE Household_Name not like '%[_.?]%'
) AS source (HouseholdID, GroupName)
ON source.GroupName = G.Name
WHEN NOT MATCHED THEN
	INSERT ( IsSystem, GroupTypeId, Name, IsSecurityRole, IsActive, [Guid] )
	VALUES ( 0, @FamilyGroupType, GroupName, 0, 0, NEWID() )
	OUTPUT source.HouseholdID, Inserted.Id, Inserted.Name INTO #InsertedGroups;

INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], Value, [Guid])
SELECT 0, @GroupAttributeID, GroupID, 0, HouseholdID, NEWID()
FROM #InsertedGroups IG

------------------------------------------------------------
-- Add Person for each Individual in F1
------------------------------------------------------------

IF EXISTS( SELECT * FROM SYS.TABLES WHERE NAME = 'InsertedGroups' )
BEGIN
	DROP TABLE #InsertedPeople
END

CREATE TABLE #InsertedPeople (
	HouseholdID int
	, IndividualID int
	, PersonID int
	, BirthDate date
);

MERGE Person AS P
USING (
	SELECT DISTINCT i.Household_ID, i.Individual_ID, i.First_Name, i.goes_by, i.middle_name, i.Last_Name, i.gender, i.date_of_birth	
	FROM F1.dbo.Individual_Household i
	WHERE Household_Name not like '%[_.?]%'	
) AS source ( HouseholdID, IndividualID, FirstName, NickName, MiddleName, LastName, Gender, birthdate )
ON P.FirstName = source.FirstName
AND P.LastName = source.LastName
AND p.birthyear = YEAR(source.birthdate)
AND p.birthmonth = MONTH(source.birthdate)
AND p.birthday = DAY(source.birthdate)
WHEN NOT MATCHED THEN
	INSERT ( IsSystem, GivenName, NickName, MiddleName, LastName, BirthDay, BirthMonth, BirthYear, Gender, DoNotEmail, SystemNote, [Guid],[RecordTypeValueId],[RecordStatusValueId] )
	VALUES ( 0, FirstName, NickName, MiddleName, LastName, day(birthdate), month(birthdate), year(birthdate)
	, CASE Gender WHEN 'Male' THEN 1 WHEN 'Female' THEN 2 ELSE 0 END
	, 0, 'Imported from FellowshipOne', NEWID(), @PersonRecordType, @ActiveRecordStatus )
	OUTPUT source.HouseholdID, source.IndividualID, source.BirthDate INTO #InsertedPeople;

INSERT [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], Value, [Guid])
SELECT 0, @IndividualAttributeID, PersonId, 0, IndividualID, NEWID()
FROM #InsertedPeople

INSERT [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], Value, [Guid])
SELECT 0, @HouseholdAttributeID, PersonId, 0, HouseholdID, NEWID()
FROM #InsertedPeople

------------------------------------------------------------
-- Add Person group memberships 
------------------------------------------------------------
;WITH F1GroupMembers AS (
	SELECT Household_Name
	, Household_ID
	, Individual_ID
	, CASE WHEN (Household_Position = 'Head' OR Household_Position = 'Spouse')
		AND DATEDIFF(YEAR, Date_Of_Birth, @NextGradeTransitionDate ) >= 19
		THEN @AdultRole
	WHEN (Household_Position = 'Child' OR Household_Position = 'Visitor')
		AND DATEDIFF(YEAR, Date_Of_Birth, @NextGradeTransitionDate ) < 19
		THEN @ChildRole	
	WHEN DATEDIFF(YEAR, Date_Of_Birth, @NextGradeTransitionDate ) < 19
		THEN @ChildRole
	ELSE @AdultRole
	END as PersonRole 
	FROM F1.dbo.individual_household
)
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
SELECT 0, Household.EntityID, Individual.EntityId, FGM.PersonRole, NEWID()
FROM F1GroupMembers FGM
LEFT JOIN AttributeValue AS Household
	ON Household.AttributeId = @GroupAttributeID
	AND Household.Value = FGM.Household_ID
LEFT JOIN AttributeValue AS Individual
	ON Individual.AttributeId = @IndividualAttributeID
	AND Individual.Value = FGM.Individual_ID

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
ON AV.AttributeId = 592
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
