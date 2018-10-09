/* ====================================================== 
-- NewSpring Script #777: 
-- Copies attributes from old groups to new groups
  
--  Assumptions:
--  Existing metrics structure exists according to script 7:

   ====================================================== */
-- Make sure you're using the right Rock database:

--USE Rock

-- Set common variables 
DECLARE @IsSystem bit = 0
DECLARE @True bit = 1
DECLARE @False bit = 0
DECLARE @Order int = 0
DECLARE @CreatedDateTime AS DATETIME = GETDATE();
DECLARE @foreignKey AS NVARCHAR(15) = 'Metrics 2.0';

-- Entity Type Ids
DECLARE @etidGroup AS INT = (SELECT Id FROM EntityType WHERE Name = 'Rock.Model.Group');
DECLARE @attributeIdAge AS INT = (SELECT Id FROM Attribute WHERE EntityTypeId = @etidGroup AND [Key] = 'AgeRange');
DECLARE @attributeIdGrade AS INT = (SELECT Id FROM Attribute WHERE EntityTypeId = @etidGroup AND [Key] = 'GradeRange');

/* ====================================================== */
-- create the group conversion table
/* ====================================================== */
IF object_id('tempdb..#groupConversion') IS NOT NULL
BEGIN
	drop table #groupConversion
END

select ogt.Name OldGroupType, og.id OldGroupId, og.name OldGroup, ogl.Id OldLocationId, ogl.name OldLocation, gt.Name GroupTypeName, ng.Id GroupId, ng.Name GroupName, ng.CampusId
into #groupConversion
from [group] og
	inner join grouptype ogt
	on og.grouptypeid = ogt.id
	and ogt.name not like 'NEW %'    
	inner join grouplocation gl
	on og.id = gl.groupid
	and og.isactive = 1    
	inner join location ogl
	on gl.LocationId = ogl.id
	and ogl.name is not null    
	inner join [group] ng
	on ng.name = ogl.name
	inner join grouptype gt
	on ng.grouptypeid = gt.id
	and gt.name like 'NEW %'
	inner join grouplocation ngl
	on ng.id = ngl.groupid
	and ogl.id = ngl.LocationId;

/* ====================================================== */
-- Copy age
/* ====================================================== */
INSERT INTO AttributeValue (EntityId, IsSystem, AttributeId, Value, [Guid], CreatedDateTime, ForeignKey)
SELECT 
	gc.GroupId AS EntityId,
	@IsSystem,
	@attributeIdAge,
	MAX(age.Value),
	NEWID(),
	@CreatedDateTime,
	@foreignKey
FROM 
	#groupConversion gc
	LEFT JOIN AttributeValue age ON gc.OldGroupId = age.EntityId AND age.AttributeId = @attributeIdAge
	LEFT JOIN AttributeValue newAge ON gc.GroupId = newAge.EntityId AND newAge.AttributeId = @attributeIdAge
WHERE
	age.Id IS NOT NULL
	AND newAge.Id IS NULL
GROUP BY
	gc.GroupId;

/* ====================================================== */
-- Copy grade
/* ====================================================== */
INSERT INTO AttributeValue (EntityId, IsSystem, AttributeId, Value, [Guid], CreatedDateTime, ForeignKey)
SELECT 
	gc.GroupId AS EntityId,
	@IsSystem,
	@attributeIdGrade,
	MAX(grade.Value),
	NEWID(),
	@CreatedDateTime,
	@foreignKey
FROM 
	#groupConversion gc
	LEFT JOIN AttributeValue grade ON gc.OldGroupId = grade.EntityId AND grade.AttributeId = @attributeIdGrade
	LEFT JOIN AttributeValue newGrade ON gc.GroupId = newGrade.EntityId AND newGrade.AttributeId = @attributeIdGrade
WHERE
	grade.Id IS NOT NULL
	AND newGrade.Id IS NULL
GROUP BY
	gc.GroupId;

-- DELETE FROM AttributeValue WHERE ForeignKey = 'Metrics 2.0'