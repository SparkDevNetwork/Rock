------------------------------------------------------------------------
-- This script will add data for the check-in system 
------------------------------------------------------------------------

/*
* This script will add data for the check-in system.
* It adds the following GroupTypes, Groups, needed attributes, Locations
* and Schedules.  These new GroupTypes will be placed under a new top level
* GroupType called "Weekend Service Check-in Area" and all the groups will be
* placed under a new top level Group called "Weekend Service Check-in" 
* 
* GROUPTYPE				   ATTRIBUTES
*     - GROUP			                                  LOCATION           GUID
* -------------------     --------------------------      ------------------ ------------------------------------
* Weekend Service Check-in Area                                              FEDD389A-616F-4A53-906C-63D8255631C5
*     - Weekend Service Check-in                                               64F0F121-8E1E-4A24-B706-BA8E921FE623
* 
* Nursery/Preschool Area  Ages: 0-6                                          CADB2D12-7836-44BC-8EEA-3C6AB22FD5E8
*     - Nursery           Ages: 0-3                       Bunnies              DC1A2A83-1B5D-46BC-9E99-4571466827F5
*     - Preschool         Ages: 2.5-5.99                  Puppies              366001D1-0E60-4AA1-875D-046286E29284
*
* Elementary Area         Ages: 4.75-13.99  Grades: K-6                      E3C8F7D6-5CEB-43BB-802F-66C3E734049E
*     - Grades K-1        Ages: 4.75-8.75   Grades: K-1   Bears                FB8AAAA2-9A57-4AA4-8543-10A053C4834F
*     - Grades 2-3        Ages: 6-10.99     Grades: 2-3   Bobcats              24901861-14CF-474F-9FCE-7BA1D6C84BFF
*     - Grades 4-6        Ages: 8-13.99     Grades: 4-6   Outpost              42C408CE-3D69-4D7D-B9EA-41087A8945A6
*
* Jr High Area            Ages: 12-15 Grades: 7-8                            7A17235B-69AD-439B-BAB0-1A0A472DB96F
*     - Grades 7-8        n/a                             the Warehouse        7B99F840-66AB-4C7A-99A2-104D9CC953F7
*
* High School Area        Ages: 13-19 Grades: 9-12                           9A88743B-F336-4404-B877-2A623689195D
*     - Grades 9-12       n/a                             the Garage           9B982B2A-24AB-4B82-AB49-84BDB4CF9E5F
*
* Check-in Test (Don't Use) Area                                             CAAF4F9B-58B9-45B4-AABC-9188347948B7
*     - Check-in Test (Don't Use)                                              CBBBEEE0-DE95-4876-9FEF-5EB68FA67853
*     
*/

BEGIN TRANSACTION

-- Group Type Entity Type
IF NOT EXISTS(SELECT Id FROM EntityType WHERE Name = 'Rock.Model.GroupType')
	INSERT INTO EntityType (Name, Guid, IsEntity, IsSecured)
	VALUES ('Rock.Model.GroupType', NEWID(), 0, 0)

DECLARE @GroupTypeEntityTypeId int
SET @GroupTypeEntityTypeId = (SELECT Id FROM EntityType WHERE Name = 'Rock.Model.GroupType')

-- Get the related FieldType bits necessary for adding attributes later...
DECLARE @DecimalFieldTypeId int
SET @DecimalFieldTypeId = (SELECT Id FROM FieldType WHERE guid = 'c757a554-3009-4214-b05d-cea2b2ea6b8f')

DECLARE @IntFieldTypeId INT
SET @IntFieldTypeId = (SELECT Id FROM [FieldType] WHERE [Guid] = 'A75DFC58-7A1B-4799-BF31-451B2BBE38FF')

DECLARE @GroupEntityTypeId INT
SET @GroupEntityTypeId = (SELECT Id FROM [EntityType] WHERE Name = 'Rock.Model.Group')

-- Table of all GroupType Guids
DECLARE @tGroupTypeGuids TABLE ( [Guid] uniqueidentifier );
INSERT INTO @tGroupTypeGuids
SELECT 'FEDD389A-616F-4A53-906C-63D8255631C5'
UNION ALL SELECT 'CADB2D12-7836-44BC-8EEA-3C6AB22FD5E8'
UNION ALL SELECT 'E3C8F7D6-5CEB-43BB-802F-66C3E734049E'
UNION ALL SELECT '7A17235B-69AD-439B-BAB0-1A0A472DB96F'
UNION ALL SELECT '9A88743B-F336-4404-B877-2A623689195D'
UNION ALL SELECT 'CAAF4F9B-58B9-45B4-AABC-9188347948B7'

-- Table of all Group Guids
DECLARE @tGroupGuids TABLE ( [Guid] uniqueidentifier );
INSERT INTO @tGroupGuids
SELECT '64F0F121-8E1E-4A24-B706-BA8E921FE623'
UNION ALL SELECT 'DC1A2A83-1B5D-46BC-9E99-4571466827F5'
UNION ALL SELECT '366001D1-0E60-4AA1-875D-046286E29284'
UNION ALL SELECT 'FB8AAAA2-9A57-4AA4-8543-10A053C4834F'
UNION ALL SELECT '24901861-14CF-474F-9FCE-7BA1D6C84BFF'
UNION ALL SELECT '42C408CE-3D69-4D7D-B9EA-41087A8945A6'
UNION ALL SELECT '7B99F840-66AB-4C7A-99A2-104D9CC953F7'
UNION ALL SELECT '9B982B2A-24AB-4B82-AB49-84BDB4CF9E5F'
UNION ALL SELECT 'CBBBEEE0-DE95-4876-9FEF-5EB68FA67853'

-- Get all existing check-in GroupType Ids and Group Ids...
-- put them into two table variables for use below
DECLARE @tAllCheckinGroupTypeIds TABLE ( Id INT, IdString VARCHAR(16) );
INSERT INTO @tAllCheckinGroupTypeIds
SELECT Id, CONVERT(varchar, Id) FROM [GroupType] WHERE [Guid] in (SELECT [Guid] FROM @tGroupTypeGuids )

DECLARE @tAllCheckinGroupIds TABLE ( Id INT );
INSERT INTO @tAllCheckinGroupIds
SELECT Id FROM [Group] WHERE GroupTypeId in (SELECT Id FROM @tAllCheckinGroupTypeIds )

-- TODO COMMENT OUT -- the select will be empty on a new system/first run
--SELECT Id as AllCheckinGroupTypeIds from @tAllCheckinGroupTypeIds

------------------------------------------------------------------------
-- Cleanup the old data before we add the new stuff
------------------------------------------------------------------------

    -- Delete Attributes Values for Groups
	DELETE AV FROM [AttributeValue] AV WHERE AV.AttributeId IN
	(
		SELECT Id FROM [Attribute] A WHERE EntityTypeId = @GroupEntityTypeId AND EntityTypeQualifierColumn = 'GroupTypeId' AND EntityTypeQualifierValue IN (SELECT [Id] FROM @tAllCheckinGroupTypeIds )
    )
    
    -- Delete Attributes Values for GroupTypes
	DELETE AV FROM [AttributeValue] AV WHERE AV.AttributeId IN
	(
		SELECT Id FROM [Attribute] A WHERE EntityTypeId = @GroupEntityTypeId AND EntityTypeQualifierColumn = 'Id' AND EntityTypeQualifierValue IN (SELECT [Id] FROM @tAllCheckinGroupTypeIds )
    )
    
    -- Delete Attributes for Groups 
    DELETE A FROM [Attribute] A WHERE A.EntityTypeId = @GroupTypeEntityTypeId AND A.EntityTypeQualifierColumn = 'GroupTypeId' AND A.EntityTypeQualifierValue IN ( SELECT Id from @tAllCheckinGroupTypeIds )
	
    -- Delete Attributes for GroupTypes 
    DELETE A FROM [Attribute] A WHERE A.EntityTypeId = @GroupTypeEntityTypeId AND A.EntityTypeQualifierColumn = 'Id' AND A.EntityTypeQualifierValue IN ( SELECT Id from @tAllCheckinGroupTypeIds )
	   
	-- Delete existing GroupType Associations, Groups and GroupTypes
	DELETE GTA FROM [GroupTypeAssociation] GTA WHERE GTA.GroupTypeId IN ( SELECT Id from @tAllCheckinGroupTypeIds )
	 OR GTA.ChildGroupTypeId IN ( SELECT Id from @tAllCheckinGroupTypeIds )
	                                                                
	-- Unlink the groups from their parent group
	UPDATE [Group] SET ParentGroupId = null WHERE GroupTypeId IN ( SELECT Id from @tAllCheckinGroupTypeIds )
	DELETE [Group] WHERE GroupTypeId IN ( SELECT Id from [GroupType] where [Guid] IN (SELECT Guid from @tGroupTypeGuids ))
	DELETE [GroupType] WHERE [Guid] IN ( SELECT [Guid] from @tGroupTypeGuids )

	DELETE @tAllCheckinGroupTypeIds
	DELETE @tAllCheckinGroupIds
	
	-- Delete the legacy Min/Max Age attributes ?? 
	DELETE [Attribute] WHERE guid in ('63FA25AA-7796-4302-BF05-D96A1C390BD7','D05368C9-5069-49CD-B7E8-9CE8C46BB75D','8A5315C5-1431-4C48-9C91-0675D3BE87EE','47C14AF6-A259-4EE4-8BC6-4D735C2A1252')

-- TODO COMMENT OUT
SELECT * From [Attribute] where Category = 'Check-in'


DECLARE @ParentGroupTypeId int
DECLARE @ChildGroupTypeId int
DECLARE @GroupRoleId int

DECLARE @TestGroupTypeId int
DECLARE @NurseryPreschoolGroupTypeId int
DECLARE @ElementaryGroupTypeId int
DECLARE @JHGroupTypeId int
DECLARE @HSGroupTypeId int

---------------------------------------------------------------------------
-- new GroupTypes
---------------------------------------------------------------------------
-- Insert the new top level Check-in GroupType
INSERT INTO [GroupType] ( [IsSystem],[Name],[Guid],[AllowMultipleLocations],[TakesAttendance],[AttendanceRule],[AttendancePrintTo])
   VALUES (0, 'Weekend Service Check-in Area', 'FEDD389A-616F-4A53-906C-63D8255631C5', 0, 0, 0, 0)
SET @ParentGroupTypeId = SCOPE_IDENTITY()

-- Now insert the all the new GroupTypes under that one...
INSERT INTO [GroupType] ( [IsSystem],[Name],[Guid],[AllowMultipleLocations],[TakesAttendance],[AttendanceRule],[AttendancePrintTo])
   VALUES (0, 'Check-in Test (Don''t Use) Area', 'CAAF4F9B-58B9-45B4-AABC-9188347948B7', 1, 1, 0, 0)
SET @TestGroupTypeId = SCOPE_IDENTITY()
INSERT INTO [GroupTypeAssociation] VALUES (@ParentGroupTypeId, @TestGroupTypeId);

INSERT INTO [GroupType] ( [IsSystem],[Name],[Guid],[AllowMultipleLocations],[TakesAttendance],[AttendanceRule],[AttendancePrintTo])
   VALUES (0, 'Nursery/Preschool Area', 'CADB2D12-7836-44BC-8EEA-3C6AB22FD5E8', 1, 1, 0, 0)
SET @NurseryPreschoolGroupTypeId = SCOPE_IDENTITY()
INSERT INTO [GroupTypeAssociation] VALUES (@ParentGroupTypeId, @NurseryPreschoolGroupTypeId);

INSERT INTO [GroupType] ( [IsSystem],[Name],[Guid],[AllowMultipleLocations],[TakesAttendance],[AttendanceRule],[AttendancePrintTo])
   VALUES (0, 'Elementary Area', 'E3C8F7D6-5CEB-43BB-802F-66C3E734049E', 1, 1, 0, 0)
SET @ElementaryGroupTypeId = SCOPE_IDENTITY()
INSERT INTO [GroupTypeAssociation] VALUES (@ParentGroupTypeId, @ElementaryGroupTypeId);

INSERT INTO [GroupType] ( [IsSystem],[Name],[Guid],[AllowMultipleLocations],[TakesAttendance],[AttendanceRule],[AttendancePrintTo])
   VALUES (0, 'Jr High Area', '7A17235B-69AD-439B-BAB0-1A0A472DB96F', 1, 1, 0, 0)
SET @JHGroupTypeId = SCOPE_IDENTITY()
INSERT INTO [GroupTypeAssociation] VALUES (@ParentGroupTypeId, @JHGroupTypeId);

INSERT INTO [GroupType] ( [IsSystem],[Name],[Guid],[AllowMultipleLocations],[TakesAttendance],[AttendanceRule],[AttendancePrintTo])
   VALUES (0, 'High School Area', '9A88743B-F336-4404-B877-2A623689195D', 1, 1, 0, 0)
SET @HSGroupTypeId = SCOPE_IDENTITY()
INSERT INTO [GroupTypeAssociation] VALUES (@ParentGroupTypeId, @HSGroupTypeId);


SELECT * from [GroupType]  -- TODO COMMENT OUT

------------------------------------------------------------------------------
-- Re-Populate the temp table of GroupTypes
------------------------------------------------------------------------------
INSERT INTO @tAllCheckinGroupTypeIds
SELECT Id, CONVERT(varchar, Id) FROM [GroupType] WHERE [Guid] in (SELECT [Guid] FROM @tGroupTypeGuids )

-- TODO COMMENT OUT
SELECT IdString as AllCheckinGroupTypes from @tAllCheckinGroupTypeIds

-- Re-populate the temp tabel of Groups
--INSERT INTO @tAllCheckinGroupIds
--SELECT Id FROM [Group] WHERE GroupTypeId in (SELECT Id FROM @tAllCheckinGroupTypeIds )


------------------------------------------------------------------------------------
-- Add the GroupType attributes
--
--		EntityTypeId			EntityTypeQualifierColumn	EntityTypeQualifierValue
--		@GroupTypeEntityTypeId	'Id'						<the GroupType's ID>
------------------------------------------------------------------------------------
DECLARE @TheGroupTypeId int;

-- Nursery/Preschool GroupType Attributes
SET @TheGroupTypeId = @NurseryPreschoolGroupTypeId;
	-- Ages: 0-6
	INSERT INTO [Attribute] ( IsSystem, FieldTypeId, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, [Key], Name, [Order], IsGridColumn, IsMultiValue, IsRequired, Category, [Guid], [Description] )
	   VALUES ( 0, @DecimalFieldTypeId, @GroupTypeEntityTypeId, 'Id', @TheGroupTypeId, 'MinAge', 'Minimum Age', 0, 0, 0, 0, 'Check-in', '43511B8F-71D9-423A-85BF-D1CD08C1998E', 'The minimum age required to check into these group types.' )
	INSERT INTO [Attribute] ( IsSystem, FieldTypeId, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, [Key], Name, [Order], IsGridColumn, IsMultiValue, IsRequired, Category, [Guid], [Description] )
	   VALUES ( 0, @DecimalFieldTypeId, @GroupTypeEntityTypeId, 'Id', @TheGroupTypeId, 'MaxAge', 'Maximum Age', 1, 0, 0, 0, 'Check-in', 'BB85499E-3BD5-4C77-A711-DC4AE7F6115F', 'The maximum age allowed to check into these group types.' )

	INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], [Value], [Guid]) 
	   SELECT 0, Id, @TheGroupTypeId, 0, '0.0', newid() FROM Attribute WHERE guid = '43511B8F-71D9-423A-85BF-D1CD08C1998E'
	INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], [Value], [Guid])
	   SELECT 0, Id, @TheGroupTypeId, 0, '6.00', newid() FROM Attribute WHERE guid = 'BB85499E-3BD5-4C77-A711-DC4AE7F6115F'

-- Elementary GroupType Attributes
SET @TheGroupTypeId = @ElementaryGroupTypeId;
	-- Ages: 4.75-13.99
	INSERT INTO [Attribute] ( IsSystem, FieldTypeId, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, [Key], Name, [Order], IsGridColumn, IsMultiValue, IsRequired, Category, [Guid], [Description] )
	   VALUES ( 0, @DecimalFieldTypeId, @GroupTypeEntityTypeId, 'Id', @TheGroupTypeId, 'MinAge', 'Minimum Age', 0, 0, 0, 0, 'Check-in', 'AA140FB3-31D5-4DF1-B765-067817535CFC', 'The minimum age required to check into these group types.' )
	INSERT INTO [Attribute] ( IsSystem, FieldTypeId, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, [Key], Name, [Order], IsGridColumn, IsMultiValue, IsRequired, Category, [Guid], [Description] )
	   VALUES ( 0, @DecimalFieldTypeId, @GroupTypeEntityTypeId, 'Id', @TheGroupTypeId, 'MaxAge', 'Maximum Age', 1, 0, 0, 0, 'Check-in', '3AB8B255-8DCD-443A-955B-95578A09AFB5', 'The maximum age allowed to check into these group types.' )

	INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], [Value], [Guid])
	   SELECT 0, Id, @TheGroupTypeId, 0, '4.75',  newid() FROM [Attribute] WHERE guid = 'AA140FB3-31D5-4DF1-B765-067817535CFC'
	INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], [Value], [Guid])
	   SELECT 0, Id, @TheGroupTypeId, 0, '13.99', newid() FROM [Attribute] WHERE guid = '3AB8B255-8DCD-443A-955B-95578A09AFB5'

    -- Grades: K-6
	INSERT INTO [Attribute] ( IsSystem, FieldTypeId, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, [Key], Name, [Order], IsGridColumn, IsMultiValue, IsRequired, Category, [Guid], [Description] )
	   VALUES ( 0, @IntFieldTypeId, @GroupTypeEntityTypeId, 'Id', @TheGroupTypeId, 'MinGrade', 'Minimum Grade', 1, 0, 0, 0, 'Check-in', 'C7C028C2-6582-45E8-839D-5C4467C6FDF4', 'Defines the lower grade level boundary to check into these group types.' )
	INSERT INTO [Attribute] ( IsSystem, FieldTypeId, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, [Key], Name, [Order], IsGridColumn, IsMultiValue, IsRequired, Category, [Guid], [Description] )
	   VALUES ( 0, @IntFieldTypeId, @GroupTypeEntityTypeId, 'Id', @TheGroupTypeId, 'MaxGrade', 'Maximum Grade', 1, 0, 0, 0, 'Check-in', 'AD1FECFD-A6B8-4C76-AB5D-B5B91AAAEFA1', 'Defines the upper grade level boundary to check into these group types.' )

	INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], [Value], [Guid])
	   SELECT 0, Id, @TheGroupTypeId, 0, '0', newid() FROM [Attribute] WHERE guid = 'C7C028C2-6582-45E8-839D-5C4467C6FDF4'
	INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], [Value], [Guid])
	   SELECT 0, Id, @TheGroupTypeId, 0, '6', newid() FROM [Attribute] WHERE guid = 'AD1FECFD-A6B8-4C76-AB5D-B5B91AAAEFA1'

-- Jr High GroupType Attributes
SET @TheGroupTypeId = @JHGroupTypeId;

	-- Ages: 12-15
	INSERT INTO [Attribute] ( IsSystem, FieldTypeId, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, [Key], Name, [Order], IsGridColumn, IsMultiValue, IsRequired, Category, [Guid], [Description] )
	   VALUES ( 0, @DecimalFieldTypeId, @GroupTypeEntityTypeId, 'Id', @TheGroupTypeId, 'MinAge', 'Minimum Age', 0, 0, 0, 0, 'Check-in', 'E88E1E79-96E4-4F91-88A1-21B6230E083F', 'The minimum age required to check into these group types.' )
	INSERT INTO [Attribute] ( IsSystem, FieldTypeId, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, [Key], Name, [Order], IsGridColumn, IsMultiValue, IsRequired, Category, [Guid], [Description] )
	   VALUES ( 0, @DecimalFieldTypeId, @GroupTypeEntityTypeId, 'Id', @TheGroupTypeId, 'MaxAge', 'Maximum Age', 1, 0, 0, 0, 'Check-in', '360F533A-C502-4578-9410-B8487566E1E2', 'The maximum age allowed to check into these group types.' )

	INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], [Value], [Guid]) 
	   SELECT 0, Id, @TheGroupTypeId, 0, '12', newid() FROM Attribute WHERE guid = 'E88E1E79-96E4-4F91-88A1-21B6230E083F'
	INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], [Value], [Guid])
	   SELECT 0, Id, @TheGroupTypeId, 0, '15', newid() FROM Attribute WHERE guid = '360F533A-C502-4578-9410-B8487566E1E2'
	   
   -- Grades 7-8
	INSERT INTO [Attribute] ( IsSystem, FieldTypeId, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, [Key], Name, [Order], IsGridColumn, IsMultiValue, IsRequired, Category, [Guid], [Description] )
	   VALUES ( 0, @IntFieldTypeId, @GroupTypeEntityTypeId, 'Id', @TheGroupTypeId, 'MinGrade', 'Minimum Grade', 1, 0, 0, 0, 'Check-in', 'D4AC0C00-A5D1-405C-B2E3-F2F5F0C58009', 'Defines the lower grade level boundary to check into these group types.' )
	INSERT INTO [Attribute] ( IsSystem, FieldTypeId, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, [Key], Name, [Order], IsGridColumn, IsMultiValue, IsRequired, Category, [Guid], [Description] )
	   VALUES ( 0, @IntFieldTypeId, @GroupTypeEntityTypeId, 'Id', @TheGroupTypeId, 'MaxGrade', 'Maximum Grade', 1, 0, 0, 0, 'Check-in', '6C73D5C0-975E-486B-A56A-C7F9CFF05950', 'Defines the upper grade level boundary to check into these group types.' )

	INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], [Value], [Guid])
	   SELECT 0, Id, @TheGroupTypeId, 0, '7', newid() FROM [Attribute] WHERE guid = 'D4AC0C00-A5D1-405C-B2E3-F2F5F0C58009'
	INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], [Value], [Guid])
	   SELECT 0, Id, @TheGroupTypeId, 0, '8', newid() FROM [Attribute] WHERE guid = '6C73D5C0-975E-486B-A56A-C7F9CFF05950'
	   
-- High School GroupType Attributes
SET @TheGroupTypeId = @HSGroupTypeId;

	-- Ages: 13-19
	INSERT INTO [Attribute] ( IsSystem, FieldTypeId, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, [Key], Name, [Order], IsGridColumn, IsMultiValue, IsRequired, Category, [Guid], [Description] )
	   VALUES ( 0, @DecimalFieldTypeId, @GroupTypeEntityTypeId, 'Id', @TheGroupTypeId, 'MinAge', 'Minimum Age', 0, 0, 0, 0, 'Check-in', '44CC7A63-3AAE-4645-8594-F8B3E1BFE961', 'The minimum age required to check into these group types.' )
	INSERT INTO [Attribute] ( IsSystem, FieldTypeId, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, [Key], Name, [Order], IsGridColumn, IsMultiValue, IsRequired, Category, [Guid], [Description] )
	   VALUES ( 0, @DecimalFieldTypeId, @GroupTypeEntityTypeId, 'Id', @TheGroupTypeId, 'MaxAge', 'Maximum Age', 1, 0, 0, 0, 'Check-in', '2C19E957-E049-4FF0-BB10-4FDF448EE712', 'The maximum age allowed for kids to check into these group types.' )

	INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], [Value], [Guid]) 
	   SELECT 0, Id, @TheGroupTypeId, 0, '13.0', newid() FROM Attribute WHERE guid = '44CC7A63-3AAE-4645-8594-F8B3E1BFE961'
	INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], [Value], [Guid])
	   SELECT 0, Id, @TheGroupTypeId, 0, '19.0', newid() FROM Attribute WHERE guid = '2C19E957-E049-4FF0-BB10-4FDF448EE712'

    -- Grades 9-12
	INSERT INTO [Attribute] ( IsSystem, FieldTypeId, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, [Key], Name, [Order], IsGridColumn, IsMultiValue, IsRequired, Category, [Guid], [Description] )
	   VALUES ( 0, @IntFieldTypeId, @GroupTypeEntityTypeId, 'Id', @TheGroupTypeId, 'MinGrade', 'Minimum Grade', 1, 0, 0, 0, 'Check-in', '92AE2841-44D4-4331-A255-E2FB900DAF4F', 'Defines the lower grade level boundary to check into these group types.' )
	INSERT INTO [Attribute] ( IsSystem, FieldTypeId, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, [Key], Name, [Order], IsGridColumn, IsMultiValue, IsRequired, Category, [Guid], [Description] )
	   VALUES ( 0, @IntFieldTypeId, @GroupTypeEntityTypeId, 'Id', @TheGroupTypeId, 'MaxGrade', 'Maximum Grade', 1, 0, 0, 0, 'Check-in', 'EB9DD62C-AF7C-4DA6-9A67-DA8BFE26BE0A', 'Defines the upper grade level boundary to check into these group types.' )

	INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], [Value], [Guid])
	   SELECT 0, Id, @TheGroupTypeId, 0, '9', newid() FROM [Attribute] WHERE guid = '92AE2841-44D4-4331-A255-E2FB900DAF4F'
	INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], [Value], [Guid])
	   SELECT 0, Id, @TheGroupTypeId, 0, '12', newid() FROM [Attribute] WHERE guid = 'EB9DD62C-AF7C-4DA6-9A67-DA8BFE26BE0A'

-- COMMENT OUT THIS SECTION
--/*
-- Verify GroupType attributes
SELECT 'Verify GroupType attributes' 
SELECT * from [Attribute] A WHERE A.EntityTypeId = @GroupTypeEntityTypeId AND A.EntityTypeQualifierColumn = 'Id'
AND A.EntityTypeQualifierValue
IN ( SELECT IdString from @tAllCheckinGroupTypeIds )

SELECT 'Verify GroupType attribute values'  
SELECT * from [AttributeValue] AV WHERE AV.AttributeId IN
(
	SELECT Id from [Attribute] A WHERE A.EntityTypeId = @GroupTypeEntityTypeId AND A.EntityTypeQualifierColumn = 'Id'
	AND A.EntityTypeQualifierValue
	IN ( SELECT IdString from @tAllCheckinGroupTypeIds )
)
--*/

---------------------------------------------------------------------------
-- Add the Groups
---------------------------------------------------------------------------
DECLARE @ParentGroupId int
DECLARE @TestGroupId int
DECLARE @NurseryGroupId int
DECLARE @PreschoolGroupId int
DECLARE @GradeK1GroupId int
DECLARE @Grade23GroupId int
DECLARE @Grade46GroupId int
DECLARE @JHGroupId int
DECLARE @HSGroupId int

INSERT INTO [Group] ( [IsSystem],[GroupTypeId],[Name],[IsSecurityRole],[IsActive],[Guid] )
   VALUES( 0, @ParentGroupTypeId, 'Weekend Service Check-in', 0, 1, '64F0F121-8E1E-4A24-B706-BA8E921FE623' )
SET @ParentGroupId = SCOPE_IDENTITY()

INSERT INTO [Group] ( [IsSystem],[ParentGroupId],[GroupTypeId],[Name],[IsSecurityRole],[IsActive],[Guid] )
   VALUES ( 0, @ParentGroupId, @ParentGroupTypeId, 'Check-in Test (Don''t Use)', 0, 1, 'CBBBEEE0-DE95-4876-9FEF-5EB68FA67853' )
SET @TestGroupId = SCOPE_IDENTITY()

INSERT INTO [Group] ( [IsSystem],[ParentGroupId],[GroupTypeId],[Name],[IsSecurityRole],[IsActive],[Guid] )
   VALUES ( 0, @ParentGroupId, @NurseryPreschoolGroupTypeId, 'Nursery', 0, 1, 'DC1A2A83-1B5D-46BC-9E99-4571466827F5' )
SET @NurseryGroupId = SCOPE_IDENTITY()

INSERT INTO [Group] ( [IsSystem],[ParentGroupId],[GroupTypeId],[Name],[IsSecurityRole],[IsActive],[Guid] )
   VALUES ( 0, @ParentGroupId, @NurseryPreschoolGroupTypeId, 'Preschool', 0, 1, '366001D1-0E60-4AA1-875D-046286E29284' )
SET @PreschoolGroupId = SCOPE_IDENTITY()

INSERT INTO [Group] ( [IsSystem],[ParentGroupId],[GroupTypeId],[Name],[IsSecurityRole],[IsActive],[Guid] )
   VALUES ( 0, @ParentGroupId, @ElementaryGroupTypeId, 'Grades K-1', 0, 1, 'FB8AAAA2-9A57-4AA4-8543-10A053C4834F' )
SET @GradeK1GroupId = SCOPE_IDENTITY()

INSERT INTO [Group] ( [IsSystem],[ParentGroupId],[GroupTypeId],[Name],[IsSecurityRole],[IsActive],[Guid] )
   VALUES ( 0, @ParentGroupId, @ElementaryGroupTypeId, 'Grades 2-3', 0, 1, '24901861-14CF-474F-9FCE-7BA1D6C84BFF' )
SET @Grade23GroupId = SCOPE_IDENTITY()

INSERT INTO [Group] ( [IsSystem],[ParentGroupId],[GroupTypeId],[Name],[IsSecurityRole],[IsActive],[Guid])
   VALUES ( 0, @ParentGroupId, @ElementaryGroupTypeId, 'Grades 4-6', 0, 1, '42C408CE-3D69-4D7D-B9EA-41087A8945A6' )
SET @Grade46GroupId = SCOPE_IDENTITY()

INSERT INTO [Group] ( [IsSystem],[ParentGroupId],[GroupTypeId],[Name],[IsSecurityRole],[IsActive],[Guid])
   VALUES ( 0, @ParentGroupId, @JHGroupTypeId, 'Grades 7-8', 0, 1, '7B99F840-66AB-4C7A-99A2-104D9CC953F7' )
SET @JHGroupId = SCOPE_IDENTITY()

INSERT INTO [Group] ( [IsSystem],[ParentGroupId],[GroupTypeId],[Name],[IsSecurityRole],[IsActive],[Guid])
   VALUES ( 0, @ParentGroupId, @HSGroupTypeId, 'Grades 9-12', 0, 1, '9B982B2A-24AB-4B82-AB49-84BDB4CF9E5F' )
SET @HSGroupId = SCOPE_IDENTITY()

-- TODO COMMENT OUT
--SELECT * FROM [Group] WHERE ParentGroupId = (SELECT Id FROM [Group] WHERE Name = 'Weekend Service Check-in' )

------------------------------------------------------------------------------------
-- Add the Group attributes ([Attributes] table)
--
--		EntityTypeId			EntityTypeQualifierColumn	EntityTypeQualifierValue
--		@GroupEntityTypeId		'GroupTypeId'				<the GroupType's ID>
--
-- NOTE: the use of @*Group*EntityTypeId and NOT @*GroupType*EntityTypeId in
--       the EntityTypeId column which is different than the way the Group Type
--       Attributes are added. 
-- 
-- However when adding the [AttributeValues] for thes attributes the
-- EntityId column value will be the <the GroupId>

------------------------------------------------------------------------------------
DECLARE @TheGroupId int;
DECLARE @AttributeId int;

-- Nursery/Preschool GroupType's Group Attributes

    -- MinAge
	INSERT INTO [Attribute] ( IsSystem, FieldTypeId, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, [Key], Name, [Order], IsGridColumn, IsMultiValue, IsRequired, Category, Guid, [Description])
	   VALUES ( 0, @DecimalFieldTypeId, @GroupEntityTypeId, 'GroupTypeId', @NurseryPreschoolGroupTypeId, 'MinAge', 'Minimum Age', 1, 0, 0, 0, 'Check-in', NEWID(), 'The minimum age required to check into this group.' )
	SET @AttributeId = SCOPE_IDENTITY();

		-- Nursery MinAge (0 - )
		INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], [Value], [Guid])
		   VALUES ( 0, @AttributeId, @NurseryGroupId , 0, '0.0',  newid() )
		   
		-- Preschool MinAge (2.5 - )
		INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], [Value], [Guid])
		   VALUES ( 0, @AttributeId, @PreschoolGroupId, 0, '2.5',  newid() )

	-- MaxAge
	INSERT INTO [Attribute] ( IsSystem, FieldTypeId, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, [Key], Name, [Order], IsGridColumn, IsMultiValue, IsRequired, Category, Guid, [Description])
	   VALUES ( 0, @DecimalFieldTypeId, @GroupEntityTypeId, 'GroupTypeId', @NurseryPreschoolGroupTypeId, 'MaxAge', 'Maximum Age', 1, 0, 0, 0, 'Check-in', NEWID(), 'The maximum age allowed to check into this group.' )
	SET @AttributeId = SCOPE_IDENTITY();
	
		-- Nursery MaxAge ( - 3)
		INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], [Value], [Guid])
		   VALUES ( 0, @AttributeId, @NurseryGroupId , 0, '3.0',  newid() )
		   
		-- Preschool MaxAge ( - 5.99)
		INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], [Value], [Guid])
		   VALUES ( 0, @AttributeId, @PreschoolGroupId, 0, '5.99',  newid() )

-- Elementary GroupType's Group Attributes

    -- MinAge
	INSERT INTO [Attribute] ( IsSystem, FieldTypeId, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, [Key], Name, [Order], IsGridColumn, IsMultiValue, IsRequired, Category, Guid, [Description])
	   VALUES ( 0, @DecimalFieldTypeId, @GroupEntityTypeId, 'GroupTypeId', @ElementaryGroupTypeId, 'MinAge', 'Minimum Age', 1, 0, 0, 0, 'Check-in', NEWID(), 'The minimum age required to check into this group.' )
	SET @AttributeId = SCOPE_IDENTITY();

		-- Grades K-1 MinAge ( 4.75 - )
		INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], [Value], [Guid])
		   VALUES ( 0, @AttributeId, @GradeK1GroupId , 0, '4.75',  newid() )

		-- Grades 2-3 MinAge ( 6 - )
		INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], [Value], [Guid])
		   VALUES ( 0, @AttributeId, @Grade23GroupId , 0, '6.0',  newid() )
		   
		-- Grades 4-6 MinAge ( 8 - )
		INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], [Value], [Guid])
		   VALUES ( 0, @AttributeId, @Grade46GroupId , 0, '8.0',  newid() )
		   
	-- MaxAge
	INSERT INTO [Attribute] ( IsSystem, FieldTypeId, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, [Key], Name, [Order], IsGridColumn, IsMultiValue, IsRequired, Category, Guid, [Description])
	   VALUES ( 0, @DecimalFieldTypeId, @GroupEntityTypeId, 'GroupTypeId', @ElementaryGroupTypeId, 'MaxAge', 'Maximum Age', 1, 0, 0, 0, 'Check-in', NEWID(), 'The maximum age allowed to check into this group.' )
	SET @AttributeId = SCOPE_IDENTITY();

		-- Grades K-1 MaxAge ( - 8.75 )
		INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], [Value], [Guid])
		   VALUES ( 0, @AttributeId, @GradeK1GroupId , 0, '8.75',  newid() )

		-- Grades 2-3 MaxAge ( - 10.99 )
		INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], [Value], [Guid])
		   VALUES ( 0, @AttributeId, @Grade23GroupId , 0, '10.99',  newid() )
		   
		-- Grades 4-6 MaxAge ( - 13.99 )
		INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], [Value], [Guid])
		   VALUES ( 0, @AttributeId, @Grade46GroupId , 0, '13.99',  newid() )

	-- MinGrade
	INSERT INTO [Attribute] ( IsSystem, FieldTypeId, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, [Key], Name, [Order], IsGridColumn, IsMultiValue, IsRequired, Category, Guid, [Description])
	   VALUES ( 0, @IntFieldTypeId, @GroupEntityTypeId, 'GroupTypeId', @ElementaryGroupTypeId, 'MinGrade', 'Minimum Grade', 1, 0, 0, 0, 'Check-in', NEWID(), 'Defines the lower grade boundary to check in to this group.' )
	SET @AttributeId = SCOPE_IDENTITY();

		-- Grades K-1 MinGrade ( 0 - )
		INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], [Value], [Guid])
		   VALUES ( 0, @AttributeId, @GradeK1GroupId , 0, '0',  newid() )

		-- Grades 2-3 MinGrade ( 2 - )
		INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], [Value], [Guid])
		   VALUES ( 0, @AttributeId, @Grade23GroupId , 0, '2',  newid() )
		   
		-- Grades 4-6 MinGrade ( 4 - )
		INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], [Value], [Guid])
		   VALUES ( 0, @AttributeId, @Grade46GroupId , 0, '4',  newid() )
		   
	-- MaxGrade
	INSERT INTO [Attribute] ( IsSystem, FieldTypeId, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, [Key], Name, [Order], IsGridColumn, IsMultiValue, IsRequired, Category, Guid, [Description])
	   VALUES ( 0, @IntFieldTypeId, @GroupEntityTypeId, 'GroupTypeId', @ElementaryGroupTypeId, 'MaxGrade', 'Maximum Grade', 1, 0, 0, 0, 'Check-in', NEWID(), 'Defines the upper grade boundary to check in to this group.' )
	SET @AttributeId = SCOPE_IDENTITY();

		-- Grades K-1 MaxGrade ( - 1 )
		INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], [Value], [Guid])
		   VALUES ( 0, @AttributeId, @GradeK1GroupId , 0, '1',  newid() )

		-- Grades 2-3 MaxGrade ( - 3 )
		INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], [Value], [Guid])
		   VALUES ( 0, @AttributeId, @Grade23GroupId , 0, '3',  newid() )
		   
		-- Grades 4-6 MaxGrade ( - 6 )
		INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], [Value], [Guid])
		   VALUES ( 0, @AttributeId, @Grade46GroupId , 0, '6',  newid() )


-- COMMENT OUT THIS SECTION
/*

SELECT 'Verify Group attributes for these groups' 
SELECT 'Groups:', @TestGroupId, @NurseryGroupId, @PreschoolGroupId, @GradeK1GroupId, @Grade23GroupId, @Grade46GroupId, @JHGroupId, @HSGroupId
SELECT * FROM [Attribute] A WHERE A.EntityTypeId = @GroupEntityTypeId AND A.EntityTypeQualifierColumn = 'GroupTypeId'
AND A.EntityTypeQualifierValue
IN ( SELECT IdString as AllCheckinGroupTypes from @tAllCheckinGroupTypeIds )

SELECT 'Verify Group attribute values'  
SELECT * FROM [AttributeValue] AV WHERE AV.AttributeId IN
(
	SELECT Id from [Attribute] A WHERE A.EntityTypeId = @GroupEntityTypeId AND A.EntityTypeQualifierColumn = 'GroupTypeId'
	AND A.EntityTypeQualifierValue
    IN ( SELECT IdString as AllCheckinGroupTypes from @tAllCheckinGroupTypeIds )
)
SELECT * FROM [AttributeValue]
*/

---------------------------------------------------------------------------
-- Create the Schedules
---------------------------------------------------------------------------

DELETE [Schedule]
INSERT INTO [Schedule] ([Name],[iCalendarContent], [CheckInStartOffsetMinutes],[CheckInEndOffsetMinutes],[EffectiveStartDate],[Guid]) VALUES 
    ('4:30',        
'BEGIN:VCALENDAR
BEGIN:VEVENT
DTEND:20130501T173000
DTSTART:20130501T163000
RRULE:FREQ=WEEKLY;BYDAY=SA
END:VEVENT
END:VCALENDAR', '30', '30', '05/01/2013', NEWID() ),
    
    ('6:00',        
'BEGIN:VCALENDAR
BEGIN:VEVENT
DTEND:20130501T190000
DTSTART:20130501T180000
RRULE:FREQ=WEEKLY;BYDAY=SA
END:VEVENT
END:VCALENDAR', '30', '30', '05/01/2013', NEWID() ),

    ('9:00',        
'BEGIN:VCALENDAR
BEGIN:VEVENT
DTEND:20130501T100000
DTSTART:20130501T090000
RRULE:FREQ=WEEKLY;BYDAY=SU
END:VEVENT
END:VCALENDAR', '30', '30', '05/01/2013', NEWID() ),

    ('10:30',        
'BEGIN:VCALENDAR
BEGIN:VEVENT
DTEND:20130501T113000
DTSTART:20130501T103000
RRULE:FREQ=WEEKLY;BYDAY=SU
END:VEVENT
END:VCALENDAR', '30', '30', '05/01/2013', NEWID() ),

    ('12:00',        
'BEGIN:VCALENDAR
BEGIN:VEVENT
DTEND:20130501T130000
DTSTART:20130501T120000
RRULE:FREQ=WEEKLY;BYDAY=SU
END:VEVENT
END:VCALENDAR', '30', '30', '05/01/2013', NEWID() ),

    ('4:30 (test)',        
'BEGIN:VCALENDAR
BEGIN:VEVENT
DTEND:20130501T235900
DTSTART:20130501T000100
RRULE:FREQ=DAILY
END:VEVENT
END:VCALENDAR', '30', '30', '05/01/2013', NEWID() ),

    ('6:00 (test)',        
'BEGIN:VCALENDAR
BEGIN:VEVENT
DTEND:20130501T235900
DTSTART:20130501T000100
RRULE:FREQ=DAILY
END:VEVENT
END:VCALENDAR', '30', '30', '05/01/2013', NEWID() )

---------------------------------------------------------------------------
-- Create Locations
--   * Bunnies
--   * Puppies
--   * Bears
--   * Bobcats
--   * Outpost
--   * the Warehouse
--   * the Garage
---------------------------------------------------------------------------

DELETE [Location]
DECLARE @CampusLocationId int
DECLARE @BuildingLocationId int
DECLARE @RoomLocationId int

-- Main Campus "Location"
INSERT INTO [Location] ([Guid], [Name], [IsActive])	VALUES (NEWID(), 'Main Campus', 1)
SET @CampusLocationId = SCOPE_IDENTITY()

-- Main building
INSERT INTO [Location] ([ParentLocationId], [Name], [IsActive], [Guid])	VALUES (@CampusLocationId, 'Bldg 1', 1, NEWID())
SET @BuildingLocationId = SCOPE_IDENTITY()

-- Check-in Rooms
INSERT INTO [Location] ([ParentLocationId], [Name], [IsActive], [Guid])	VALUES (@BuildingLocationId, 'Bunnies', 1, NEWID())
INSERT INTO [Location] ([ParentLocationId], [Name], [IsActive], [Guid])	VALUES (@BuildingLocationId, 'Puppies', 1, NEWID())
INSERT INTO [Location] ([ParentLocationId], [Name], [IsActive], [Guid])	VALUES (@BuildingLocationId, 'Bears', 1, NEWID())
INSERT INTO [Location] ([ParentLocationId], [Name], [IsActive], [Guid])	VALUES (@BuildingLocationId, 'Bobcats', 1, NEWID())
INSERT INTO [Location] ([ParentLocationId], [Name], [IsActive], [Guid])	VALUES (@BuildingLocationId, 'Outpost', 1, NEWID())
INSERT INTO [Location] ([ParentLocationId], [Name], [IsActive], [Guid])	VALUES (@BuildingLocationId, 'the Warehouse', 1, NEWID())
INSERT INTO [Location] ([ParentLocationId], [Name], [IsActive], [Guid])	VALUES (@BuildingLocationId, 'the Garage', 1, NEWID())


DELETE [DeviceLocation]
DELETE [Device]


DECLARE @fieldTypeIdText int = (select Id from FieldType where Guid = '9C204CD0-1233-41C5-818A-C5DA439445AA')


---------------------------------------------------------------------------
-- Create Device Types
---------------------------------------------------------------------------

DECLARE @DeviceTypeValueId int
SET @DeviceTypeValueId = (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = 'BC809626-1389-4543-B8BB-6FAC79C27AFD')
DECLARE @PrinterTypevalueId int
SET @PrinterTypevalueId = (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '8284B128-E73B-4863-9FC2-43E6827B65E6')

DECLARE @PrinterDeviceId int
INSERT INTO [Device] ([Name],[DeviceTypeValueId],[IPAddress],[PrintFrom],[PrintToOverride],[Guid])
VALUES ('Test Label Printer',@PrinterTypevalueId, '10.1.20.200',0,1,NEWID())
SET @PrinterDeviceId = SCOPE_IDENTITY()

-- A centralized kiosk
INSERT INTO [Device] ([Name],[DeviceTypeValueId],[PrinterDeviceId],[PrintFrom],[PrintToOverride],[Guid])
SELECT C.Name + ': Centralized', @DeviceTypeValueId, @PrinterDeviceId, 0, 1, NEWID()
FROM Location C WHERE C.Name = 'Main Campus'
	
-- kiosks for each campus/building/room
INSERT INTO [Device] ([Name],[DeviceTypeValueId],[PrinterDeviceId],[PrintFrom],[PrintToOverride],[Guid])
SELECT C.Name + ':' + B.Name + ':' + R.Name, @DeviceTypeValueId, @PrinterDeviceId, 0, 1, NEWID()
FROM Location C
INNER JOIN Location B
	ON B.ParentLocationId = C.Id
INNER JOIN Location R
	ON R.ParentLocationId = B.Id
WHERE C.Name = 'Main Campus'

-- Add Devices to their Locations
INSERT INTO [DeviceLocation] (DeviceId, LocationId)
SELECT D.Id, R.Id
FROM Location C
INNER JOIN Location B
	ON B.ParentLocationId = C.Id
INNER JOIN Location R
	ON R.ParentLocationId = B.Id
INNER JOIN Device D 
	ON D.Name = C.Name + ':' + B.Name + ':' + R.Name

-- Add centralized kiosk to each Location
INSERT INTO [DeviceLocation] (DeviceId, LocationId)
SELECT D.Id, R.Id
FROM Location C
INNER JOIN Location B
	ON B.ParentLocationId = C.Id
INNER JOIN Location R
	ON R.ParentLocationId = B.Id
INNER JOIN Device D 
	ON D.Name = C.Name + ': Centralized'
WHERE C.Name = 'Main Campus'


---------------------------------------------------------------------------
-- Add Groups to Locations
---------------------------------------------------------------------------
DELETE [GroupLocation]

	INSERT INTO [GroupLocation] (GroupId, LocationId, Guid) 
	SELECT @TestGroupId, DL.LocationId, NEWID()
	FROM DeviceLocation DL

	INSERT INTO [GroupLocation] (GroupId, LocationId, Guid)
	 SELECT @NurseryGroupId, L.Id, NEWID() FROM Location L WHERE L.Name = 'Bunnies'

	INSERT INTO [GroupLocation] (GroupId, LocationId, Guid)
	 SELECT @PreschoolGroupId, L.Id, NEWID() FROM Location L WHERE L.Name = 'Puppies'
	 
	INSERT INTO [GroupLocation] (GroupId, LocationId, Guid)
	 SELECT @GradeK1GroupId, L.Id, NEWID() FROM Location L WHERE L.Name = 'Bears'

	INSERT INTO [GroupLocation] (GroupId, LocationId, Guid)
	 SELECT @Grade23GroupId, L.Id, NEWID() FROM Location L WHERE L.Name = 'Bobcats'
	 
	INSERT INTO [GroupLocation] (GroupId, LocationId, Guid)
	 SELECT @Grade46GroupId, L.Id, NEWID() FROM Location L WHERE L.Name = 'Outpost'  

	INSERT INTO [GroupLocation] (GroupId, LocationId, Guid)
	 SELECT @JHGroupId, L.Id, NEWID() FROM Location L WHERE L.Name = 'the Warehouse' 
	 
	INSERT INTO [GroupLocation] (GroupId, LocationId, Guid)
	 SELECT @HSGroupId, L.Id, NEWID() FROM Location L WHERE L.Name = 'the Garage' 
	 
---------------------------------------------------------------------------
-- Add Group Locations to Schedules
---------------------------------------------------------------------------
DELETE [GroupLocationSchedule]

INSERT INTO [GroupLocationSchedule] (GroupLocationId, ScheduleId) 
SELECT GL.Id, S.Id
FROM GroupLocation GL
INNER JOIN [Group] G ON G.Id = GL.GroupId AND G.Id IN ( @NurseryGroupId, @PreschoolGroupId, @GradeK1GroupId, @Grade23GroupId, @Grade46GroupId, @JHGroupId, @HSGroupId )
INNER JOIN Schedule S ON S.[Name] NOT LIKE '%(test)'

INSERT INTO [GroupLocationSchedule] (GroupLocationId, ScheduleId) 
SELECT GL.Id, S.Id
FROM GroupLocation GL
INNER JOIN [Group] G ON G.Id = GL.GroupId AND G.Name = 'Check-in Test (Don''t Use)'
INNER JOIN Schedule S ON S.[Name] LIKE '%(test)'

---------------------------------------------------------------------------
-- Workflow
---------------------------------------------------------------------------

-- Workflow Action Entity Types
IF NOT EXISTS(SELECT Id FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.FilterActiveLocations')
INSERT INTO EntityType (Name, Guid, IsEntity, IsSecured)
VALUES ('Rock.Workflow.Action.CheckIn.FilterActiveLocations', NEWID(), 0, 0)

IF NOT EXISTS(SELECT Id FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.FilterByAge')
INSERT INTO EntityType (Name, Guid, IsEntity, IsSecured)
VALUES ('Rock.Workflow.Action.CheckIn.FilterByAge', NEWID(), 0, 0)

IF NOT EXISTS(SELECT Id FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.FindFamilies')
INSERT INTO EntityType (Name, Guid, IsEntity, IsSecured)
VALUES ('Rock.Workflow.Action.CheckIn.FindFamilies', NEWID(), 0, 0)

IF NOT EXISTS(SELECT Id FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.FindFamilyMembers')
INSERT INTO EntityType (Name, Guid, IsEntity, IsSecured)
VALUES ('Rock.Workflow.Action.CheckIn.FindFamilyMembers', NEWID(), 0, 0)

IF NOT EXISTS(SELECT Id FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.FindRelationships')
INSERT INTO EntityType (Name, Guid, IsEntity, IsSecured)
VALUES ('Rock.Workflow.Action.CheckIn.FindRelationships', NEWID(), 0, 0)

IF NOT EXISTS(SELECT Id FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.LoadGroups')
INSERT INTO EntityType (Name, Guid, IsEntity, IsSecured)
VALUES ('Rock.Workflow.Action.CheckIn.LoadGroups', NEWID(), 0, 0)

IF NOT EXISTS(SELECT Id FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.LoadGroupTypes')
INSERT INTO EntityType (Name, Guid, IsEntity, IsSecured)
VALUES ('Rock.Workflow.Action.CheckIn.LoadGroupTypes', NEWID(), 0, 0)

IF NOT EXISTS(SELECT Id FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.LoadLocations')
INSERT INTO EntityType (Name, Guid, IsEntity, IsSecured)
VALUES ('Rock.Workflow.Action.CheckIn.LoadLocations', NEWID(), 0, 0)

IF NOT EXISTS(SELECT Id FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.LoadSchedules')
INSERT INTO EntityType (Name, Guid, IsEntity, IsSecured)
VALUES ('Rock.Workflow.Action.CheckIn.LoadSchedules', NEWID(), 0, 0)

IF NOT EXISTS(SELECT Id FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.RemoveEmptyGroups')
INSERT INTO EntityType (Name, Guid, IsEntity, IsSecured)
VALUES ('Rock.Workflow.Action.CheckIn.RemoveEmptyGroups', NEWID(), 0, 0)

IF NOT EXISTS(SELECT Id FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.RemoveEmptyGroupTypes')
INSERT INTO EntityType (Name, Guid, IsEntity, IsSecured)
VALUES ('Rock.Workflow.Action.CheckIn.RemoveEmptyGroupTypes', NEWID(), 0, 0)

IF NOT EXISTS(SELECT Id FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.RemoveEmptyLocations')
INSERT INTO EntityType (Name, Guid, IsEntity, IsSecured)
VALUES ('Rock.Workflow.Action.CheckIn.RemoveEmptyLocations', NEWID(), 0, 0)

IF NOT EXISTS(SELECT Id FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.RemoveEmptyPeople')
INSERT INTO EntityType (Name, Guid, IsEntity, IsSecured)
VALUES ('Rock.Workflow.Action.CheckIn.RemoveEmptyPeople', NEWID(), 0, 0)

IF NOT EXISTS(SELECT Id FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.SaveAttendance')
INSERT INTO EntityType (Name, Guid, IsEntity, IsSecured)
VALUES ('Rock.Workflow.Action.CheckIn.SaveAttendance', NEWID(), 0, 0)

IF NOT EXISTS(SELECT Id FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.CreateLabels')
INSERT INTO EntityType (Name, Guid, IsEntity, IsSecured)
VALUES ('Rock.Workflow.Action.CheckIn.CreateLabels', NEWID(), 0, 0)

IF NOT EXISTS(SELECT Id FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.CalculateLastAttended')
INSERT INTO EntityType (Name, Guid, IsEntity, IsSecured)
VALUES ('Rock.Workflow.Action.CheckIn.CalculateLastAttended', NEWID(), 0, 0)

-- Workflow Entity Type
IF NOT EXISTS(SELECT Id FROM EntityType WHERE Name = 'Rock.Model.Workflow')
INSERT INTO EntityType (Name, Guid, IsEntity, IsSecured)
VALUES ('Rock.Model.Workflow', NEWID(), 0, 0)
DECLARE @WorkflowEntityTypeId int
SET @WorkflowEntityTypeId = (SELECT Id FROM EntityType WHERE Name = 'Rock.Model.Workflow')

-- Workflow Type
DECLARE @WorkflowTypeId int
SET @WorkflowTypeId = (SELECT Id FROM [WorkflowType] WHERE Guid = '011E9F5A-60D4-4FF5-912A-290881E37EAF')
IF @WorkflowTypeId IS NOT NULL
BEGIN
	DELETE [Workflow] WHERE Id = @WorkflowTypeId
	DELETE [WorkflowType] WHERE Id = @WorkflowTypeId
END

INSERT INTO [WorkflowType] (IsSystem, IsActive, Name, [Order], WorkTerm, IsPersisted, LoggingLevel, Guid)
VALUES (0, 1, 'Children''s Checkin', 0, 'Checkin', 0, 3, '011E9F5A-60D4-4FF5-912A-290881E37EAF')
SET @WorkflowTypeId = SCOPE_IDENTITY()

UPDATE AV
SET [Value] = @WorkflowTypeId
FROM AttributeValue AV
INNER JOIN Attribute A ON A.Id = AV.AttributeId
WHERE A.[Key] = 'WorkflowTypeId'
DECLARE @TextFieldTypeId int
SET @TextFieldTypeId = (SELECT Id FROM FieldType WHERE guid = '9C204CD0-1233-41C5-818A-C5DA439445AA')
DELETE [Attribute] WHERE guid = '9D2BFE8A-41F3-4A02-B3CF-9193F0C8419E'
INSERT INTO [Attribute] ( IsSystem, FieldTypeId, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, [Key], Name, [Order], IsGridColumn, IsMultiValue, IsRequired, Category, Guid)
VALUES ( 0, @TextFieldTypeId, @WorkflowEntityTypeId, 'WorkflowTypeId', CAST(@WorkflowTypeId as varchar), 'CheckInState', 'Check In State', 0, 0, 0, 0, '', '9D2BFE8A-41F3-4A02-B3CF-9193F0C8419E')

-- Family Search Activity
DECLARE @WorkflowActivityTypeId int
INSERT INTO [WorkflowActivityType] (IsActive, WorkflowTypeId, Name, IsActivatedWithWorkflow, [Order], Guid)
VALUES (1, @WorkflowTypeId, 'Family Search', 0, 0, NEWID())
SET @WorkflowActivityTypeId = SCOPE_IDENTITY()

INSERT INTO [WorkflowActionType] (ActivityTypeId, Name, [Order], [EntityTypeId], IsActionCompletedOnSuccess, IsActivityCompletedOnSuccess, Guid)
SELECT @WorkflowActivityTypeId, 'Find Families', 0, Id, 1, 0, NEWID() FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.FindFamilies'

-- Person Search Activity
INSERT INTO [WorkflowActivityType] (IsActive, WorkflowTypeId, Name, IsActivatedWithWorkflow, [Order], Guid)
VALUES (1, @WorkflowTypeId, 'Person Search', 0, 1, NEWID())
SET @WorkflowActivityTypeId = SCOPE_IDENTITY()

INSERT INTO [WorkflowActionType] (ActivityTypeId, Name, [Order], [EntityTypeId], IsActionCompletedOnSuccess, IsActivityCompletedOnSuccess, Guid)
SELECT @WorkflowActivityTypeId, 'Find Family Members', 0, Id, 1, 0, NEWID() FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.FindFamilyMembers'
INSERT INTO [WorkflowActionType] (ActivityTypeId, Name, [Order], [EntityTypeId], IsActionCompletedOnSuccess, IsActivityCompletedOnSuccess, Guid)
SELECT @WorkflowActivityTypeId, 'Find Relationships', 1, Id, 1, 0, NEWID() FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.FindRelationships'
INSERT INTO [WorkflowActionType] (ActivityTypeId, Name, [Order], [EntityTypeId], IsActionCompletedOnSuccess, IsActivityCompletedOnSuccess, Guid)
SELECT @WorkflowActivityTypeId, 'Load Group Types', 2, Id, 1, 0, NEWID() FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.LoadGroupTypes'
INSERT INTO [WorkflowActionType] (ActivityTypeId, Name, [Order], [EntityTypeId], IsActionCompletedOnSuccess, IsActivityCompletedOnSuccess, Guid)
SELECT @WorkflowActivityTypeId, 'Filter by Age', 3, Id, 1, 0, NEWID() FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.FilterByAge'
INSERT INTO [WorkflowActionType] (ActivityTypeId, Name, [Order], [EntityTypeId], IsActionCompletedOnSuccess, IsActivityCompletedOnSuccess, Guid)
SELECT @WorkflowActivityTypeId, 'Remove Empty People', 4, Id, 1, 0, NEWID() FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.RemoveEmptyPeople'
INSERT INTO [WorkflowActionType] (ActivityTypeId, Name, [Order], [EntityTypeId], IsActionCompletedOnSuccess, IsActivityCompletedOnSuccess, Guid)
SELECT @WorkflowActivityTypeId, 'Update Last Attended', 5, Id, 1, 0, NEWID() FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.CalculateLastAttended'

-- Location Search
INSERT INTO [WorkflowActivityType] (IsActive, WorkflowTypeId, Name, IsActivatedWithWorkflow, [Order], Guid)
VALUES (1, @WorkflowTypeId, 'Location Search', 0, 2, NEWID())
SET @WorkflowActivityTypeId = SCOPE_IDENTITY()

INSERT INTO [WorkflowActionType] (ActivityTypeId, Name, [Order], [EntityTypeId], IsActionCompletedOnSuccess, IsActivityCompletedOnSuccess, Guid)
SELECT @WorkflowActivityTypeId, 'Load Locations', 0, Id, 1, 0, NEWID() FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.LoadLocations'
INSERT INTO [WorkflowActionType] (ActivityTypeId, Name, [Order], [EntityTypeId], IsActionCompletedOnSuccess, IsActivityCompletedOnSuccess, Guid)
SELECT @WorkflowActivityTypeId, 'Filter Active Locations', 1, Id, 1, 0, NEWID() FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.FilterActiveLocations'
INSERT INTO [WorkflowActionType] (ActivityTypeId, Name, [Order], [EntityTypeId], IsActionCompletedOnSuccess, IsActivityCompletedOnSuccess, Guid)
SELECT @WorkflowActivityTypeId, 'Update Last Attended', 2, Id, 1, 0, NEWID() FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.CalculateLastAttended'

-- Group Search
INSERT INTO [WorkflowActivityType] (IsActive, WorkflowTypeId, Name, IsActivatedWithWorkflow, [Order], Guid)
VALUES (1, @WorkflowTypeId, 'Group Search', 0, 3, NEWID())
SET @WorkflowActivityTypeId = SCOPE_IDENTITY()

INSERT INTO [WorkflowActionType] (ActivityTypeId, Name, [Order], [EntityTypeId], IsActionCompletedOnSuccess, IsActivityCompletedOnSuccess, Guid)
SELECT @WorkflowActivityTypeId, 'Load Groups', 0, Id, 1, 0, NEWID() FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.LoadGroups'
INSERT INTO [WorkflowActionType] (ActivityTypeId, Name, [Order], [EntityTypeId], IsActionCompletedOnSuccess, IsActivityCompletedOnSuccess, Guid)
SELECT @WorkflowActivityTypeId, 'Update Last Attended', 2, Id, 1, 0, NEWID() FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.CalculateLastAttended'

-- Schedule Search
INSERT INTO [WorkflowActivityType] (IsActive, WorkflowTypeId, Name, IsActivatedWithWorkflow, [Order], Guid)
VALUES (1, @WorkflowTypeId, 'Schedule Search', 0, 4, NEWID())
SET @WorkflowActivityTypeId = SCOPE_IDENTITY()

INSERT INTO [WorkflowActionType] (ActivityTypeId, Name, [Order], [EntityTypeId], IsActionCompletedOnSuccess, IsActivityCompletedOnSuccess, Guid)
SELECT @WorkflowActivityTypeId, 'Load Schedules', 0, Id, 1, 0, NEWID() FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.LoadSchedules'

-- Save Attendance
INSERT INTO [WorkflowActivityType] (IsActive, WorkflowTypeId, Name, IsActivatedWithWorkflow, [Order], Guid)
VALUES (1, @WorkflowTypeId, 'Save Attendance', 0, 5, NEWID())
SET @WorkflowActivityTypeId = SCOPE_IDENTITY()

INSERT INTO [WorkflowActionType] (ActivityTypeId, Name, [Order], [EntityTypeId], IsActionCompletedOnSuccess, IsActivityCompletedOnSuccess, Guid)
SELECT @WorkflowActivityTypeId, 'Save Attendance', 0, Id, 1, 0, NEWID() FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.SaveAttendance'
INSERT INTO [WorkflowActionType] (ActivityTypeId, Name, [Order], [EntityTypeId], IsActionCompletedOnSuccess, IsActivityCompletedOnSuccess, Guid)
SELECT @WorkflowActivityTypeId, 'Create Labels', 0, Id, 1, 0, NEWID() FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.CreateLabels'

COMMIT
