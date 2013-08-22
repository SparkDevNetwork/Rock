DECLARE @IndividualAttributeID int
DECLARE @HouseholdAttributeID int
DECLARE @PersonEntityID int

SELECT @PersonEntityID = [ID] FROM [EntityType] WHERE [Guid] = '72657ED8-D16E-492E-AC12-144C5E7567E7'

DECLARE @IntFieldType int
SELECT @IntFieldType = [ID] from FieldType where guid = 'A75DFC58-7A1B-4799-BF31-451B2BBE38FF'

-- Insert Person Attributes for IndividualID and HouseholdID
MERGE INTO [Attribute] A
USING (
	VALUES ( 0,	@IntFieldType,	@PersonEntityID, 'IndividualID',	'IndividualID',	'A person''s FellowshipOne individual_ID',	0,	0,	0,	0,	'0F9C847B-4302-421E-9980-51E4772E80F5')	
	, (0,	@IntFieldType,	@PersonEntityID,	'HouseholdID',	'HouseholdID',	'A person''s FellowshipOne household ID',	0,	0,	0,	0,	'CAB397A9-5E72-4970-AF27-E60967FD6D58')
) AS E (IsSystem,	FieldTypeId,	EntityTypeId,	[Key], Name, [Description], [Order], IsGridColumn, IsMultiValue, IsRequired, [Guid])
ON A.[Guid] = E.[Guid]
WHEN NOT MATCHED THEN
	INSERT (IsSystem,	FieldTypeId,	EntityTypeId,	[Key], Name, [Description], [Order], IsGridColumn, IsMultiValue, IsRequired, [Guid])
	VALUES (E.IsSystem,	E.FieldTypeId,	E.EntityTypeId,	E.[Key], E.Name, E.[Description], E.[Order], E.IsGridColumn, E.IsMultiValue, E.IsRequired, E.[Guid]);
	 
SELECT @IndividualAttributeID = [ID] FROM [Attribute] WHERE [Guid] = '0F9C847B-4302-421E-9980-51E4772E80F5'
SELECT @HouseholdAttributeID = [ID] FROM [Attribute] WHERE [Guid] = 'CAB397A9-5E72-4970-AF27-E60967FD6D58'

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
