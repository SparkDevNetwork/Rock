USE RockChMS_AC

DECLARE @indivAttrID int
DECLARE @houseAttrID int
DECLARE @entityID int

-- GET Rock.Model.Person ENTITY TYPE ID
SELECT @entityID = [ID] FROM [EntityType] WHERE [Guid] = '72657ED8-D16E-492E-AC12-144C5E7567E7'

-- Insert Person Attributes for IndividualID and HouseholdID
MERGE INTO ATTRIBUTE A
USING (
	VALUES (0,	7,	@entityID, 'IndividualID',	'IndividualID',	'F1 Individual ID',	0,	0,	0,	0,	'0F9C847B-4302-421E-9980-51E4772E80F5')	
	, (0,	7,	@entityID,	'HouseholdID',	'HouseholdID',	'F1 Household ID',	0,	0,	0,	0,	'CAB397A9-5E72-4970-AF27-E60967FD6D58')
) AS E (IsSystem,	FieldTypeId,	EntityTypeId,	[Key], Name, [Description], [Order], IsGridColumn, IsMultiValue, IsRequired, [Guid])
ON A.[Guid] = E.[Guid]
WHEN NOT MATCHED THEN
	INSERT (IsSystem,	FieldTypeId,	EntityTypeId,	[Key], Name, [Description], [Order], IsGridColumn, IsMultiValue, IsRequired, [Guid])
	VALUES (E.IsSystem,	E.FieldTypeId,	E.EntityTypeId,	E.[Key], E.Name, E.[Description], E.[Order], E.IsGridColumn, E.IsMultiValue, E.IsRequired, E.[Guid]);
		 
SELECT @indivAttrID = [ID] FROM [Attribute] WHERE [Guid] = '0F9C847B-4302-421E-9980-51E4772E80F5'
SELECT @houseAttrID = [ID] FROM [Attribute] WHERE [Guid] = 'CAB397A9-5E72-4970-AF27-E60967FD6D58'

-- Insert Fellowship One ID's where matched
DROP table #resultset
SELECT p.Id, p.FirstName, p.LastName, ih.individual_id, ih.household_id
INTO #resultset
FROM person p
INNER JOIN [SVR].f1.dbo.individual_household ih
ON p.FirstName = ih.first_name
AND p.lastname = ih.last_name
AND p.birthyear = year(ih.date_of_birth)
AND p.birthmonth = month(ih.date_of_birth)
AND p.birthday = day(ih.date_of_birth)

INSERT attributevalue (IsSystem, attributeid, entityid, [order], value, [guid])
SELECT 0, @indivAttrID, Id, 0, individual_id, NEWID()
FROM #resultset r

INSERT attributevalue (IsSystem, attributeid, entityid, [order], value, [guid])
SELECT 0, @houseAttrID, Id, 0, household_id, NEWID()
FROM #resultset r

/* ================================================== 
	delete from attributevalue where id >= 685
	select * from attributevalue
   ================================================== */

-- Insert Fellowship One Attendance
INSERT Attendance (LocationId, ScheduleId, GroupId, PersonId, StartDateTime, Note, DidAttend, [Guid])
SELECT distinct l.id
, s.id
, g.id
, p.Id
, a.start_date_time
, 'Imported from F1', 1
, NEWID()
FROM Person p
INNER JOIN AttributeValue av
ON av.attributeid = 552 -- @indivAttrID
AND av.entityid = p.id
INNER JOIN [SVR].f1.dbo.Attendance a
ON a.individual_id = av.value
INNER JOIN [SVR].f1.dbo.RLC r
ON a.rlc_id = r.rlc_id
INNER JOIN [Group] g
ON r.rlc_name = g.name
INNER JOIN Location l
ON r.rlc_name = l.Name
INNER JOIN Schedule s
ON CAST(a.start_date_time as time) = CAST(s.name as time)
ORDER BY a.start_date_time
