------------------------------------------------------------------------
-- This script will add data for the check-in system 
------------------------------------------------------------------------

/*
* This script will add data for the check-in system.
* It adds the following GroupTypes, Groups, needed attributes, Locations
* and Schedules.  These new GroupTypes will be placed under a new top level
* GroupType called "Weekly Service Check-in Area" and all the groups will be
* placed under a new top level Group called "Weekly Service Check-in" 
* 
* GROUPTYPE                ATTRIBUTES
*   - GROUP                                               LOCATION            GUID                                   Inherits From
* -------------------      ----------------------------   ------------------  -------------------------------------  ----------------------
* Weekly Service Check-in Area                                                FEDD389A-616F-4A53-906C-63D8255631C5
* 
* Check in by Age          Ages:                                              0572A5FE-20A4-4BF1-95CD-C71DB5281392
* Check in by AbilityLevel AbilityLevel: (AL:)                                02EA0D2A-B8E8-421F-83A1-CADFC2920AD8   Check in by Age
* Check in by Grade        Grades:                                            4F9565A7-DD5A-41C3-B4E8-13F0B872B10B   Check in by Age
*
* Nursery/Preschool Area                                                      CADB2D12-7836-44BC-8EEA-3C6AB22FD5E8   Check in by AbilityLevel
*   - Nursery              Ages: 0-3,   AL: Infants       Bunnies Room         DC1A2A83-1B5D-46BC-9E99-4571466827F5
*   - Crawlers/Walkers     Ages: -3.99, AL: Crawl-or-Walk Kittens Room         0650C672-1A08-4EEC-B3BC-2D00B744E371
*   - Preschool            Ages: -5.99, AL: Potty-Trained Puppies Room         366001D1-0E60-4AA1-875D-046286E29284
*
* Elementary Area                                                             E3C8F7D6-5CEB-43BB-802F-66C3E734049E   Check in by Grade
*   - Grades K-1           Ages: 4.75-8.75, Grades: K-1   Bears   Room         FB8AAAA2-9A57-4AA4-8543-10A053C4834F
*   - Grades 2-3           Ages: 6-10.99,   Grades: 2-3   Bobcats Room         24901861-14CF-474F-9FCE-7BA1D6C84BFF
*   - Grades 4-6           Ages: 8-13.99,   Grades: 4-6   Outpost Room         42C408CE-3D69-4D7D-B9EA-41087A8945A6
*
* Jr High Area                                                                7A17235B-69AD-439B-BAB0-1A0A472DB96F   Check in by Grade
*   - Grades 7-8           Ages: 12-15,     Grades: 7-8   the Warehouse        7B99F840-66AB-4C7A-99A2-104D9CC953F7
*
* High School Area                                                            9A88743B-F336-4404-B877-2A623689195D   Check in by Grade
*   - Grades 9-12          Ages: 13-19,      Grades: 9-12 the Garage           9B982B2A-24AB-4B82-AB49-84BDB4CF9E5F
*
* Check-in Test (Don't Use) Area                                              CAAF4F9B-58B9-45B4-AABC-9188347948B7 
*   - Check-in Test (Don't Use)                                                CBBBEEE0-DE95-4876-9FEF-5EB68FA67853
*
*/

IF NOT EXISTS(SELECT *  FROM [Category] WHERE Guid = '8F8B272D-D351-485E-86D6-3EE5B7C84D99' )
BEGIN
	PRINT( 'ERROR: This script requires that the Populate_WorkflowTypeAndCategory.sql be run first.' )
	RETURN
END

-- Group Type Entity Type
IF NOT EXISTS(SELECT Id FROM EntityType WHERE Name = 'Rock.Model.GroupType')
  INSERT INTO EntityType (Name, Guid, IsEntity, IsSecured, IsCommon)
  VALUES ('Rock.Model.GroupType', NEWID(), 0, 0, 0)

DECLARE @GroupTypeEntityTypeId int
SET @GroupTypeEntityTypeId = (SELECT Id FROM EntityType WHERE Name = 'Rock.Model.GroupType')

-- Get the related FieldType bits necessary for adding attributes later...
DECLARE @DecimalRangeFieldTypeId int
SET @DecimalRangeFieldTypeId = (SELECT Id FROM FieldType WHERE guid = '758D9648-573E-4800-B5AF-7CC29F4BE170')

DECLARE @DefinedValueFieldTypeId INT
SET @DefinedValueFieldTypeId = (SELECT Id FROM [FieldType] WHERE [Guid] = '59D5A94C-94A0-4630-B80A-BB25697D74C7')

DECLARE @IntegerRangeFieldTypeId INT
SET @IntegerRangeFieldTypeId = (SELECT Id FROM [FieldType] WHERE [Guid] = '9D5F21E0-DEA0-4E8E-BA42-71151F6A8ED4')

DECLARE @GroupEntityTypeId INT
SET @GroupEntityTypeId = (SELECT Id FROM [EntityType] WHERE Name = 'Rock.Model.Group')

-- Attribute Entity Type
DECLARE @AttributeEntityTypeId INT
SET @AttributeEntityTypeId = (SELECT Id FROM [EntityType] WHERE Name = 'Rock.Model.Attribute')

DECLARE @GroupTypePurposeCheckinTemplateId INT
SET @GroupTypePurposeCheckinTemplateId = (select [Id] from [DefinedValue] where [Guid] = '4A406CB0-495B-4795-B788-52BDFDE00B01');

DECLARE @GroupTypePurposeCheckinFilterId INT
SET @GroupTypePurposeCheckinFilterId = (select [Id] from [DefinedValue] where [Guid] = '6BCED84C-69AD-4F5A-9197-5C0F9C02DD34');

-- Group Type Check-in Category Id
DECLARE @GroupTypeCheckInCategoryId INT
SET @GroupTypeCheckInCategoryId = (
  SELECT Id FROM Category 
  WHERE EntityTypeId = @AttributeEntityTypeId 
  AND EntityTypeQualifierColumn = 'EntityTypeId' 
  AND EntityTypeQualifierValue = CAST(@GroupTypeEntityTypeId AS varchar)
  AND Name = 'Check-in'
)
IF @GroupTypeCheckInCategoryId IS NULL
BEGIN
  INSERT INTO Category (IsSystem, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, Name, Guid)
  VALUES (0, @AttributeEntityTypeId, 'EntityTypeId', CAST(@GroupTypeEntityTypeId AS varchar), 'Check-in', NEWID())
  SET @GroupTypeCheckInCategoryId = SCOPE_IDENTITY()
END

-- Table of all GroupType Guids
DECLARE @tGroupTypeGuids TABLE ( [Guid] uniqueidentifier );
INSERT INTO @tGroupTypeGuids
SELECT 'FEDD389A-616F-4A53-906C-63D8255631C5'
UNION ALL SELECT 'CADB2D12-7836-44BC-8EEA-3C6AB22FD5E8'
UNION ALL SELECT 'E3C8F7D6-5CEB-43BB-802F-66C3E734049E'
UNION ALL SELECT '7A17235B-69AD-439B-BAB0-1A0A472DB96F'
UNION ALL SELECT '9A88743B-F336-4404-B877-2A623689195D'
UNION ALL SELECT 'CAAF4F9B-58B9-45B4-AABC-9188347948B7'
UNION ALL SELECT '0572A5FE-20A4-4BF1-95CD-C71DB5281392'
UNION ALL SELECT '13A6139D-EEEC-412D-8572-773ECA1939CC'
UNION ALL SELECT '02EA0D2A-B8E8-421F-83A1-CADFC2920AD8'
UNION ALL SELECT '4F9565A7-DD5A-41C3-B4E8-13F0B872B10B'

-- Table of all Group Guids
DECLARE @tGroupGuids TABLE ( [Guid] uniqueidentifier );
INSERT INTO @tGroupGuids
SELECT '64F0F121-8E1E-4A24-B706-BA8E921FE623'       -- (old parent group, no longer added by script)
UNION ALL SELECT 'DC1A2A83-1B5D-46BC-9E99-4571466827F5'
UNION ALL SELECT '0650C672-1A08-4EEC-B3BC-2D00B744E371'
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

    -- Delete Attributes for Groups 
    DELETE A FROM [Attribute] A WHERE A.EntityTypeId = @GroupEntityTypeId AND A.EntityTypeQualifierColumn = 'GroupTypeId' AND A.EntityTypeQualifierValue IN ( SELECT Id from @tAllCheckinGroupTypeIds )
  
    -- Delete Attributes for GroupTypes 
    DELETE A FROM [Attribute] A WHERE A.EntityTypeId = @GroupTypeEntityTypeId AND A.EntityTypeQualifierColumn = 'Id' AND A.EntityTypeQualifierValue IN ( SELECT Id from @tAllCheckinGroupTypeIds )
     
  -- Delete existing GroupType Associations, Groups and GroupTypes
  DELETE GTA FROM [GroupTypeAssociation] GTA WHERE GTA.GroupTypeId IN ( SELECT Id from @tAllCheckinGroupTypeIds )
   OR GTA.ChildGroupTypeId IN ( SELECT Id from @tAllCheckinGroupTypeIds )
                                                                  
  -- Unlink the groups from their parent group
  UPDATE [Group] SET ParentGroupId = null WHERE GroupTypeId IN ( SELECT Id from @tAllCheckinGroupTypeIds )
  DELETE [Group] WHERE GroupTypeId IN ( SELECT Id from [GroupType] where [Guid] IN (SELECT Guid from @tGroupTypeGuids ))
  
  -- Unlink the grouptypes from their inherited grouptype
  UPDATE [GroupType] set [InheritedGroupTypeId] = null where [InheritedGroupTypeId] IN ( SELECT Id from [GroupType] where [Guid] IN (SELECT Guid from @tGroupTypeGuids ))
  DELETE [GroupType] WHERE [Guid] IN ( SELECT [Guid] from @tGroupTypeGuids )

  DELETE @tAllCheckinGroupTypeIds
  DELETE @tAllCheckinGroupIds
  
  -- Delete the legacy Min/Max Age attributes ?? 
  DELETE [Attribute] WHERE guid in (
    '63FA25AA-7796-4302-BF05-D96A1C390BD7','D05368C9-5069-49CD-B7E8-9CE8C46BB75D','8A5315C5-1431-4C48-9C91-0675D3BE87EE','47C14AF6-A259-4EE4-8BC6-4D735C2A1252',
    '43511B8F-71D9-423A-85BF-D1CD08C1998E','BB85499E-3BD5-4C77-A711-DC4AE7F6115F','C7C028C2-6582-45E8-839D-5C4467C6FDF4','AD1FECFD-A6B8-4C76-AB5D-B5B91AAAEFA1')


---------------------------------------------------------------------------
-- Add the GroupTypes
---------------------------------------------------------------------------
DECLARE @ParentGroupTypeId int

DECLARE @AgeGroupTypeId int
DECLARE @AbilityLevelGroupTypeId int
DECLARE @GradeGroupTypeId int
DECLARE @TestGroupTypeId int
DECLARE @NurseryPreschoolGroupTypeId int
DECLARE @ElementaryGroupTypeId int
DECLARE @JHGroupTypeId int
DECLARE @HSGroupTypeId int

-- Insert the new top level Check-in GroupType
INSERT INTO [GroupType] ( [IsSystem],[Name],[Guid],[AllowMultipleLocations],[TakesAttendance],[AttendanceRule],[AttendancePrintTo],[ShowInGroupList],[ShowInNavigation],[Order],[LocationSelectionMode],[GroupTypePurposeValueId],[GroupTerm],[GroupMemberTerm])
   VALUES (0, 'Weekly Service Check-in Area', 'FEDD389A-616F-4A53-906C-63D8255631C5', 0, 0, 0, 0, 0,0,0,0, @GroupTypePurposeCheckinTemplateId, 'Group', 'Member')
SET @ParentGroupTypeId = SCOPE_IDENTITY()

-- Now insert the all the new GroupTypes under that one...
INSERT INTO [GroupType] ( [IsSystem],[Name],[Guid],[AllowMultipleLocations],[TakesAttendance],[AttendanceRule],[AttendancePrintTo],[ShowInGroupList],[ShowInNavigation],[Order],[LocationSelectionMode],[GroupTypePurposeValueId],[GroupTerm],[GroupMemberTerm])
   VALUES (0, 'Check in by Age', '0572A5FE-20A4-4BF1-95CD-C71DB5281392', 0, 0, 0, 0, 0,0,0,0,@GroupTypePurposeCheckinFilterId, 'Group', 'Member')
SET @AgeGroupTypeId = SCOPE_IDENTITY()
INSERT INTO [GroupTypeAssociation] VALUES (@ParentGroupTypeId, @AgeGroupTypeId);

INSERT INTO [GroupType] ( [IsSystem],[Name],[Guid],[AllowMultipleLocations],[TakesAttendance],[AttendanceRule],[AttendancePrintTo],[ShowInGroupList],[ShowInNavigation],[Order],[LocationSelectionMode],[InheritedGroupTypeId],[GroupTypePurposeValueId],[GroupTerm],[GroupMemberTerm])
   VALUES (0, 'Check in by AbilityLevel', '13A6139D-EEEC-412D-8572-773ECA1939CC', 0, 0, 0, 0, 0,0,0,0,@AgeGroupTypeId, @GroupTypePurposeCheckinFilterId, 'Group', 'Member')
SET @AbilityLevelGroupTypeId = SCOPE_IDENTITY()
INSERT INTO [GroupTypeAssociation] VALUES (@ParentGroupTypeId, @AbilityLevelGroupTypeId);

INSERT INTO [GroupType] ( [IsSystem],[Name],[Guid],[AllowMultipleLocations],[TakesAttendance],[AttendanceRule],[AttendancePrintTo],[ShowInGroupList],[ShowInNavigation],[Order],[LocationSelectionMode],[InheritedGroupTypeId],[GroupTypePurposeValueId],[GroupTerm],[GroupMemberTerm])
   VALUES (0, 'Check in by Grade', '4F9565A7-DD5A-41C3-B4E8-13F0B872B10B', 0, 0, 0, 0, 0,0,0,0,@AgeGroupTypeId, @GroupTypePurposeCheckinFilterId, 'Group', 'Member')
SET @GradeGroupTypeId = SCOPE_IDENTITY()
INSERT INTO [GroupTypeAssociation] VALUES (@ParentGroupTypeId, @GradeGroupTypeId);

INSERT INTO [GroupType] ( [IsSystem],[Name],[Guid],[AllowMultipleLocations],[TakesAttendance],[AttendanceRule],[AttendancePrintTo],[ShowInGroupList],[ShowInNavigation],[Order],[LocationSelectionMode],[GroupTerm],[GroupMemberTerm])
   VALUES (0, 'Check-in Test (Don''t Use) Area', 'CAAF4F9B-58B9-45B4-AABC-9188347948B7', 1, 1, 0, 0,0,0,0,0, 'Group', 'Member')
SET @TestGroupTypeId = SCOPE_IDENTITY()
INSERT INTO [GroupTypeAssociation] VALUES (@ParentGroupTypeId, @TestGroupTypeId);

INSERT INTO [GroupType] ( [IsSystem],[Name],[Guid],[AllowMultipleLocations],[TakesAttendance],[AttendanceRule],[AttendancePrintTo],[ShowInGroupList],[ShowInNavigation],[Order],[LocationSelectionMode],[InheritedGroupTypeId],[GroupTerm],[GroupMemberTerm])
   VALUES (0, 'Nursery/Preschool Area', 'CADB2D12-7836-44BC-8EEA-3C6AB22FD5E8', 1, 1, 0, 0, 0,0,1,0,@AbilityLevelGroupTypeId, 'Group', 'Member')
SET @NurseryPreschoolGroupTypeId = SCOPE_IDENTITY()
INSERT INTO [GroupTypeAssociation] VALUES (@ParentGroupTypeId, @NurseryPreschoolGroupTypeId);

INSERT INTO [GroupType] ( [IsSystem],[Name],[Guid],[AllowMultipleLocations],[TakesAttendance],[AttendanceRule],[AttendancePrintTo],[ShowInGroupList],[ShowInNavigation],[Order],[LocationSelectionMode],[InheritedGroupTypeId],[GroupTerm],[GroupMemberTerm])
   VALUES (0, 'Elementary Area', 'E3C8F7D6-5CEB-43BB-802F-66C3E734049E', 1, 1, 0, 0, 0,0,2,0,@GradeGroupTypeId, 'Group', 'Member')
SET @ElementaryGroupTypeId = SCOPE_IDENTITY()
INSERT INTO [GroupTypeAssociation] VALUES (@ParentGroupTypeId, @ElementaryGroupTypeId);

INSERT INTO [GroupType] ( [IsSystem],[Name],[Guid],[AllowMultipleLocations],[TakesAttendance],[AttendanceRule],[AttendancePrintTo],[ShowInGroupList],[ShowInNavigation],[Order],[LocationSelectionMode],[InheritedGroupTypeId],[GroupTerm],[GroupMemberTerm])
   VALUES (0, 'Jr High Area', '7A17235B-69AD-439B-BAB0-1A0A472DB96F', 1, 1, 0, 0, 0,0,3,0,@GradeGroupTypeId, 'Group', 'Member')
SET @JHGroupTypeId = SCOPE_IDENTITY()
INSERT INTO [GroupTypeAssociation] VALUES (@ParentGroupTypeId, @JHGroupTypeId);

INSERT INTO [GroupType] ( [IsSystem],[Name],[Guid],[AllowMultipleLocations],[TakesAttendance],[AttendanceRule],[AttendancePrintTo],[ShowInGroupList],[ShowInNavigation],[Order],[LocationSelectionMode],[InheritedGroupTypeId],[GroupTerm],[GroupMemberTerm])
   VALUES (0, 'High School Area', '9A88743B-F336-4404-B877-2A623689195D', 1, 1, 0, 0, 0,0,4,0,@GradeGroupTypeId, 'Group', 'Member')
SET @HSGroupTypeId = SCOPE_IDENTITY()
INSERT INTO [GroupTypeAssociation] VALUES (@ParentGroupTypeId, @HSGroupTypeId);


---------------------------------------------------------------------------
-- Add the Groups
---------------------------------------------------------------------------
DECLARE @TestGroupId int
DECLARE @NurseryGroupId int
DECLARE @CrawlersWalkersGroupId int
DECLARE @PreschoolGroupId int
DECLARE @GradeK1GroupId int
DECLARE @Grade23GroupId int
DECLARE @Grade46GroupId int
DECLARE @JHGroupId int
DECLARE @HSGroupId int

INSERT INTO [Group] ( [IsSystem],[GroupTypeId],[Name],[IsSecurityRole],[IsActive],[Order],[Guid] )
   VALUES ( 0, @TestGroupTypeId, 'Check-in Test (Don''t Use)', 0, 1, 0,'CBBBEEE0-DE95-4876-9FEF-5EB68FA67853' )
SET @TestGroupId = SCOPE_IDENTITY()

INSERT INTO [Group] ( [IsSystem],[GroupTypeId],[Name],[IsSecurityRole],[IsActive],[Order],[Guid] )
   VALUES ( 0, @NurseryPreschoolGroupTypeId, 'Nursery', 0, 1, 1,'DC1A2A83-1B5D-46BC-9E99-4571466827F5' )
SET @NurseryGroupId = SCOPE_IDENTITY()

INSERT INTO [Group] ( [IsSystem],[GroupTypeId],[Name],[IsSecurityRole],[IsActive],[Order],[Guid] )
   VALUES ( 0, @NurseryPreschoolGroupTypeId, 'Crawlers/Walkers', 0, 1, 2,'0650C672-1A08-4EEC-B3BC-2D00B744E371' )
SET @CrawlersWalkersGroupId = SCOPE_IDENTITY()

INSERT INTO [Group] ( [IsSystem],[GroupTypeId],[Name],[IsSecurityRole],[IsActive],[Order],[Guid] )
   VALUES ( 0, @NurseryPreschoolGroupTypeId, 'Preschool', 0, 1, 3,'366001D1-0E60-4AA1-875D-046286E29284' )
SET @PreschoolGroupId = SCOPE_IDENTITY()

INSERT INTO [Group] ( [IsSystem],[GroupTypeId],[Name],[IsSecurityRole],[IsActive],[Order],[Guid] )
   VALUES ( 0, @ElementaryGroupTypeId, 'Grades K-1', 0, 1, 4,'FB8AAAA2-9A57-4AA4-8543-10A053C4834F' )
SET @GradeK1GroupId = SCOPE_IDENTITY()

INSERT INTO [Group] ( [IsSystem],[GroupTypeId],[Name],[IsSecurityRole],[IsActive],[Order],[Guid] )
   VALUES ( 0, @ElementaryGroupTypeId, 'Grades 2-3', 0, 1, 5,'24901861-14CF-474F-9FCE-7BA1D6C84BFF' )
SET @Grade23GroupId = SCOPE_IDENTITY()

INSERT INTO [Group] ( [IsSystem],[GroupTypeId],[Name],[IsSecurityRole],[IsActive],[Order],[Guid])
   VALUES ( 0, @ElementaryGroupTypeId, 'Grades 4-6', 0, 1, 6,'42C408CE-3D69-4D7D-B9EA-41087A8945A6' )
SET @Grade46GroupId = SCOPE_IDENTITY()

INSERT INTO [Group] ( [IsSystem],[GroupTypeId],[Name],[IsSecurityRole],[IsActive],[Order],[Guid])
   VALUES ( 0, @JHGroupTypeId, 'Grades 7-8', 0, 1, 7,'7B99F840-66AB-4C7A-99A2-104D9CC953F7' )
SET @JHGroupId = SCOPE_IDENTITY()

INSERT INTO [Group] ( [IsSystem],[GroupTypeId],[Name],[IsSecurityRole],[IsActive],[Order],[Guid])
   VALUES ( 0, @HSGroupTypeId, 'Grades 9-12', 0, 1, 8,'9B982B2A-24AB-4B82-AB49-84BDB4CF9E5F' )
SET @HSGroupId = SCOPE_IDENTITY()

------------------------------------------------------------------------------------
-- Add the attributes
------------------------------------------------------------------------------------
DECLARE @AttributeId int

-- Age Range
INSERT INTO [Attribute] ( IsSystem, FieldTypeId, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, [Key], Name, [Order], IsGridColumn, IsMultiValue, IsRequired, [Guid], [Description] )
  VALUES ( 0, @DecimalRangeFieldTypeId, @GroupEntityTypeId, 'GroupTypeId', @AgeGroupTypeId, 'AgeRange', 'Age Range', 0, 0, 0, 0, '43511B8F-71D9-423A-85BF-D1CD08C1998E', 'The age range allowed to check in to these group types.' )
SET @AttributeId = SCOPE_IDENTITY()

  INSERT INTO [AttributeCategory] (AttributeId, CategoryId)
    VALUES (@AttributeId, @GroupTypeCheckInCategoryId)

  -- Nursery AgeRange (0 - 3.00)
  INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], [Value], [Guid])
    VALUES ( 0, @AttributeId, @NurseryGroupId , 0, '0.0,3.00',  newid() )

  -- Crawlers and Walkers AgeRange (0 - 3.99)
  INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], [Value], [Guid])
    VALUES ( 0, @AttributeId, @CrawlersWalkersGroupId , 0, '0.0,3.99',  newid() )
    
  -- Preschool AgeRange (0 - 5.99)
  INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], [Value], [Guid])
    VALUES ( 0, @AttributeId, @PreschoolGroupId , 0, '0.0,5.99',  newid() )
       
  -- Grades K-1 AgeRange ( 4.75 - 8.75)
  INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], [Value], [Guid])
    VALUES ( 0, @AttributeId, @GradeK1GroupId , 0, '4.75,8.75',  newid() )

  -- Grades 2-3 AgeRange ( 6 - 10.99)
  INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], [Value], [Guid])
    VALUES ( 0, @AttributeId, @Grade23GroupId , 0, '6.0,10.99',  newid() )
       
  -- Grades 4-6 AgeRange ( 8 - 13.99)
  INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], [Value], [Guid])
    VALUES ( 0, @AttributeId, @Grade46GroupId , 0, '8.0,13.99',  newid() )
       
  -- Jr High AgeRange ( 12 - 15.0)
  INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], [Value], [Guid])
    VALUES ( 0, @AttributeId, @JHGroupId , 0, '12.0,15.0',  newid() )
       
  -- High School AgeRange ( 13 - 19.0)
  INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], [Value], [Guid])
    VALUES ( 0, @AttributeId, @HSGroupId , 0, '13.0,19.0',  newid() )

-- AbilityLevel attribute and values
SET @AttributeId = ( SELECT id from Attribute where Guid = 'A6898019-5954-4E48-A46C-A76A23639956' )
IF @AttributeId IS NULL
BEGIN
	INSERT INTO [Attribute] ( IsSystem, FieldTypeId, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, [Key], Name, [Order], IsGridColumn, IsMultiValue, IsRequired, [Guid], [Description] )
	  VALUES ( 0, @DefinedValueFieldTypeId, @GroupEntityTypeId, 'GroupTypeId', @AbilityLevelGroupTypeId, 'AbilityLevel', 'Ability Level', 0, 0, 0, 0, 'A6898019-5954-4E48-A46C-A76A23639956', 'The ability level allowed to check in to these group types.' )
	SET @AttributeId = SCOPE_IDENTITY()


    -- ... and two attribute qualifiers:
    DECLARE @AttributeLevelDefinedTypeId int
    SET @AttributeLevelDefinedTypeId = (SELECT [Id] FROM [DefinedType] WHERE [Guid] = '7BEEF4D4-0860-4913-9A3D-857634D1BF7C')
    INSERT INTO [AttributeQualifier] ([IsSystem], [AttributeId], [Key], [Value], [Guid])
        VALUES (1, @AttributeId, 'definedtype', CAST(@AttributeLevelDefinedTypeId as varchar), '32ADFB54-B48D-4E59-8A79-F3FBA558509D')
    INSERT INTO [AttributeQualifier] ([IsSystem], [AttributeId], [Key], [Value], [Guid])
        VALUES (1, @AttributeId, 'allowmultiple', 'False', '4D84191A-79DD-4CBD-8029-A13B98BE243C')

  INSERT INTO [AttributeCategory] (AttributeId, CategoryId)
    VALUES (@AttributeId, @GroupTypeCheckInCategoryId)

  -- Nursery AbilityLevel (Infant)
  INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], [Value], [Guid])
    VALUES ( 0, @AttributeId, @NurseryGroupId , 0, 'C4550426-ED87-4CB0-957E-C6E0BC96080F',  newid() )
    
  -- Crawlers/Walkers AbilityLevel (Crawling-or-Walking)
  INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], [Value], [Guid])
    VALUES ( 0, @AttributeId, @CrawlersWalkersGroupId , 0, 'F78D64D3-6BA1-4ECA-A9EC-058FBDF8E586',  newid() )

  -- Preschool AbilityLevel (Potty-Trained)
  INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], [Value], [Guid])
    VALUES ( 0, @AttributeId, @PreschoolGroupId , 0, 'E6905502-4C23-4879-A60F-8C4CEB3EE2E9',  newid() )

END

-- Grade Range
INSERT INTO [Attribute] ( IsSystem, FieldTypeId, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, [Key], Name, [Order], IsGridColumn, IsMultiValue, IsRequired, [Guid], [Description] )
  VALUES ( 0, @IntegerRangeFieldTypeId, @GroupEntityTypeId, 'GroupTypeId', @GradeGroupTypeId, 'GradeRange', 'Grade Range', 0, 0, 0, 0, 'C7C028C2-6582-45E8-839D-5C4467C6FDF4', 'Defines the grade level range that is allowed to check in to these group types.' )
SET @AttributeId = SCOPE_IDENTITY()

  INSERT INTO [AttributeCategory] (AttributeId, CategoryId)
    VALUES (@AttributeId, @GroupTypeCheckInCategoryId)

  -- Grades K-1 MinGrade ( 0 - 1)
  INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], [Value], [Guid])
    VALUES ( 0, @AttributeId, @GradeK1GroupId , 0, '0,1',  newid() )

  -- Grades 2-3 MinGrade ( 2 - 3)
  INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], [Value], [Guid])
    VALUES ( 0, @AttributeId, @Grade23GroupId , 0, '2,3',  newid() )
       
  -- Grades 4-6 MinGrade ( 4 - 6)
  INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], [Value], [Guid])
    VALUES ( 0, @AttributeId, @Grade46GroupId , 0, '4,6',  newid() )
       
  -- Jr High MinGrade ( 7 - 8)
  INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], [Value], [Guid])
    VALUES ( 0, @AttributeId, @JHGroupId , 0, '7,8',  newid() )
       
  -- High School MinGrade ( 9 - 12)
  INSERT INTO [AttributeValue] (IsSystem, AttributeId, EntityId, [Order], [Value], [Guid])
    VALUES ( 0, @AttributeId, @HSGroupId , 0, '9,12',  newid() )
     
---------------------------------------------------------------------------
-- Create the Schedules
---------------------------------------------------------------------------
-- "Service Times" Check-in Category Id
DECLARE @ServiceTimesCategoryId INT
SET @ServiceTimesCategoryId = (SELECT Id FROM Category WHERE [Guid] = '4FECC91B-83F9-4269-AE03-A006F401C47E' )

DELETE [Schedule]
INSERT INTO [Schedule] ([Name],[iCalendarContent], [CheckInStartOffsetMinutes],[CheckInEndOffsetMinutes],[EffectiveStartDate],[Guid],[CategoryId]) VALUES 
    ('Saturday 4:30pm',        
'BEGIN:VCALENDAR
BEGIN:VEVENT
DTEND:20130501T173000
DTSTART:20130501T163000
RRULE:FREQ=WEEKLY;BYDAY=SA
END:VEVENT
END:VCALENDAR', '30', '30', '05/01/2013', NEWID(), @ServiceTimesCategoryId ),
    
    ('Saturday 6:00pm',        
'BEGIN:VCALENDAR
BEGIN:VEVENT
DTEND:20130501T190000
DTSTART:20130501T180000
RRULE:FREQ=WEEKLY;BYDAY=SA
END:VEVENT
END:VCALENDAR', '30', '30', '05/01/2013', NEWID(), @ServiceTimesCategoryId ),

    ('Sunday 9:00am',        
'BEGIN:VCALENDAR
BEGIN:VEVENT
DTEND:20130501T100000
DTSTART:20130501T090000
RRULE:FREQ=WEEKLY;BYDAY=SU
END:VEVENT
END:VCALENDAR', '30', '30', '05/01/2013', NEWID(), @ServiceTimesCategoryId ),

    ('Sunday 10:30am',        
'BEGIN:VCALENDAR
BEGIN:VEVENT
DTEND:20130501T113000
DTSTART:20130501T103000
RRULE:FREQ=WEEKLY;BYDAY=SU
END:VEVENT
END:VCALENDAR', '30', '30', '05/01/2013', NEWID(), @ServiceTimesCategoryId ),

    ('Sunday 12:00pm',        
'BEGIN:VCALENDAR
BEGIN:VEVENT
DTEND:20130501T130000
DTSTART:20130501T120000
RRULE:FREQ=WEEKLY;BYDAY=SU
END:VEVENT
END:VCALENDAR', '30', '30', '05/01/2013', NEWID(), @ServiceTimesCategoryId ),

    ('4:30 (test)',        
'BEGIN:VCALENDAR
BEGIN:VEVENT
DTEND:20130501T235900
DTSTART:20130501T000100
RRULE:FREQ=DAILY
END:VEVENT
END:VCALENDAR', '0', '1439', '05/01/2013', NEWID(), @ServiceTimesCategoryId ),

    ('6:00 (test)',        
'BEGIN:VCALENDAR
BEGIN:VEVENT
DTEND:20130501T235900
DTSTART:20130501T000100
RRULE:FREQ=DAILY
END:VEVENT
END:VCALENDAR', '0', '1439', '05/01/2013', NEWID(), @ServiceTimesCategoryId )

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


DELETE CL
FROM [Location] PL
INNER JOIN [Location] CL
  ON CL.ParentLocationId = PL.Id
WHERE PL.Name = 'Bldg 1'
DELETE [Location] WHERE [Name] IN ('Bldg 1', 'Main Campus')

DECLARE @CampusLocationId int
DECLARE @BuildingLocationId int
DECLARE @RoomLocationId int

-- Main Campus "Location"
INSERT INTO [Location] ([Guid], [Name], [IsActive],[IsNamedLocation]) VALUES (NEWID(), 'Main Campus', 1, 1)
SET @CampusLocationId = SCOPE_IDENTITY()

-- Main building
INSERT INTO [Location] ([ParentLocationId], [Name], [IsActive], [IsNamedLocation], [Guid]) VALUES (@CampusLocationId, 'Bldg 1', 1, 1, NEWID())
SET @BuildingLocationId = SCOPE_IDENTITY()

-- Check in Rooms
INSERT INTO [Location] ([ParentLocationId], [Name], [IsActive], [IsNamedLocation],[Guid]) VALUES (@BuildingLocationId, 'Bunnies Room', 1, 1, NEWID())
INSERT INTO [Location] ([ParentLocationId], [Name], [IsActive], [IsNamedLocation],[Guid]) VALUES (@BuildingLocationId, 'Kittens Room', 1, 1,NEWID())
INSERT INTO [Location] ([ParentLocationId], [Name], [IsActive], [IsNamedLocation],[Guid]) VALUES (@BuildingLocationId, 'Puppies Room', 1, 1,NEWID())
INSERT INTO [Location] ([ParentLocationId], [Name], [IsActive], [IsNamedLocation],[Guid]) VALUES (@BuildingLocationId, 'Bears Room', 1, 1,NEWID())
INSERT INTO [Location] ([ParentLocationId], [Name], [IsActive], [IsNamedLocation],[Guid]) VALUES (@BuildingLocationId, 'Bobcats Room', 1, 1,NEWID())
INSERT INTO [Location] ([ParentLocationId], [Name], [IsActive], [IsNamedLocation],[Guid]) VALUES (@BuildingLocationId, 'Outpost Room', 1, 1,NEWID())
INSERT INTO [Location] ([ParentLocationId], [Name], [IsActive], [IsNamedLocation],[Guid]) VALUES (@BuildingLocationId, 'the Warehouse', 1, 1,NEWID())
INSERT INTO [Location] ([ParentLocationId], [Name], [IsActive], [IsNamedLocation],[Guid]) VALUES (@BuildingLocationId, 'the Garage', 1, 1,NEWID())

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
SELECT C.Name + ': Central Kiosk', @DeviceTypeValueId, @PrinterDeviceId, 0, 1, NEWID()
FROM Location C WHERE C.Name = 'Main Campus'
  
-- kiosks for each campus/building/room
INSERT INTO [Device] ([Name],[DeviceTypeValueId],[PrinterDeviceId],[PrintFrom],[PrintToOverride],[Guid])
SELECT C.Name + ': ' + B.Name + ': ' + R.Name, @DeviceTypeValueId, @PrinterDeviceId, 0, 1, NEWID()
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
  ON D.Name = C.Name + ': ' + B.Name + ': ' + R.Name

-- Add centralized kiosk to each Location
INSERT INTO [DeviceLocation] (DeviceId, LocationId)
SELECT D.Id, R.Id
FROM Location C
INNER JOIN Location B
  ON B.ParentLocationId = C.Id
INNER JOIN Location R
  ON R.ParentLocationId = B.Id
INNER JOIN Device D 
  ON D.Name = C.Name + ': Central Kiosk'
WHERE C.Name = 'Main Campus'


DECLARE @tGroupLocationGuids TABLE ( [Guid] uniqueidentifier );
INSERT INTO @tGroupLocationGuids
SELECT '95363486-2A66-4916-8DFF-7A4C67200A4A'
UNION ALL SELECT '9FEC12C8-1131-4881-8EB5-3A40FFC1D4C8'
UNION ALL SELECT '36B7B587-44B1-44DE-9BD5-64D1BF73C25E'
UNION ALL SELECT 'D3DA525C-65DC-4DDF-BCE9-2697753736AC'
UNION ALL SELECT 'FEFDEE04-65BF-4F38-9203-0872BFE17D7C'
UNION ALL SELECT 'F281B5AF-250F-4329-BCEC-3F00B3B4761C'
UNION ALL SELECT '1F1F3271-8E18-4AB7-8F1B-D7C32AB0E328'
UNION ALL SELECT '2F421E6E-D5C4-4910-80F6-E76EF7CBBB2E'
UNION ALL SELECT '5E8B611A-F84A-45E1-8CB7-2584FC66F3C3'

---------------------------------------------------------------------------
-- Add Groups to Locations
---------------------------------------------------------------------------
DELETE [GroupLocation] where [Guid] in (select * from @tGroupLocationGuids)

  INSERT INTO [GroupLocation] (GroupId, LocationId, IsMailingLocation, IsMappedLocation, Guid)
   SELECT @NurseryGroupId, L.Id, 0,0,'95363486-2A66-4916-8DFF-7A4C67200A4A' FROM Location L WHERE L.Name = 'Bunnies Room'

  INSERT INTO [GroupLocation] (GroupId, LocationId, IsMailingLocation, IsMappedLocation, Guid)
   SELECT @CrawlersWalkersGroupId, L.Id, 0,0,'9FEC12C8-1131-4881-8EB5-3A40FFC1D4C8' FROM Location L WHERE L.Name = 'Kittens Room'
   
  INSERT INTO [GroupLocation] (GroupId, LocationId, IsMailingLocation, IsMappedLocation, Guid)
   SELECT @PreschoolGroupId, L.Id, 0,0,'36B7B587-44B1-44DE-9BD5-64D1BF73C25E' FROM Location L WHERE L.Name = 'Puppies Room'
   
  INSERT INTO [GroupLocation] (GroupId, LocationId, IsMailingLocation, IsMappedLocation, Guid)
   SELECT @GradeK1GroupId, L.Id, 0,0,'D3DA525C-65DC-4DDF-BCE9-2697753736AC' FROM Location L WHERE L.Name = 'Bears Room'

  INSERT INTO [GroupLocation] (GroupId, LocationId, IsMailingLocation, IsMappedLocation, Guid)
   SELECT @Grade23GroupId, L.Id, 0,0,'FEFDEE04-65BF-4F38-9203-0872BFE17D7C' FROM Location L WHERE L.Name = 'Bobcats Room'
   
  INSERT INTO [GroupLocation] (GroupId, LocationId, IsMailingLocation, IsMappedLocation, Guid)
   SELECT @Grade46GroupId, L.Id, 0,0,'F281B5AF-250F-4329-BCEC-3F00B3B4761C' FROM Location L WHERE L.Name = 'Outpost Room'  

  INSERT INTO [GroupLocation] (GroupId, LocationId, IsMailingLocation, IsMappedLocation, Guid)
   SELECT @JHGroupId, L.Id, 0,0,'1F1F3271-8E18-4AB7-8F1B-D7C32AB0E328' FROM Location L WHERE L.Name = 'the Warehouse' 
   
  INSERT INTO [GroupLocation] (GroupId, LocationId, IsMailingLocation, IsMappedLocation, Guid)
   SELECT @HSGroupId, L.Id, 0,0,'2F421E6E-D5C4-4910-80F6-E76EF7CBBB2E' FROM Location L WHERE L.Name = 'the Garage' 
   
  -- Add Test group to each location
  INSERT INTO [GroupLocation] (GroupId, LocationId, IsMailingLocation, IsMappedLocation, Guid)
   SELECT @TestGroupId, L.Id, 0,0, NEWID() FROM Location L WHERE L.ParentLocationId = @BuildingLocationId

---------------------------------------------------------------------------
-- Add Group Locations to Schedules
---------------------------------------------------------------------------
DELETE [GroupLocationSchedule]

INSERT INTO [GroupLocationSchedule] (GroupLocationId, ScheduleId) 
SELECT GL.Id, S.Id
FROM GroupLocation GL
INNER JOIN [Group] G ON G.Id = GL.GroupId AND G.Id IN ( @NurseryGroupId, @PreschoolGroupId, @CrawlersWalkersGroupId, @GradeK1GroupId, @Grade23GroupId, @Grade46GroupId, @JHGroupId, @HSGroupId )
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
INSERT INTO EntityType (Name, Guid, IsEntity, IsSecured, IsCommon)
VALUES ('Rock.Workflow.Action.CheckIn.FilterActiveLocations', NEWID(), 0, 0, 0)

IF NOT EXISTS(SELECT Id FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.FilterByAge')
INSERT INTO EntityType (Name, Guid, IsEntity, IsSecured, IsCommon)
VALUES ('Rock.Workflow.Action.CheckIn.FilterByAge', NEWID(), 0, 0, 0)

IF NOT EXISTS(SELECT Id FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.FilterGroupsByAge')
INSERT INTO EntityType (Name, Guid, IsEntity, IsSecured, IsCommon)
VALUES ('Rock.Workflow.Action.CheckIn.FilterGroupsByAge', NEWID(), 0, 0, 0)

IF NOT EXISTS(SELECT Id FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.FilterByGrade')
INSERT INTO EntityType (Name, Guid, IsEntity, IsSecured, IsCommon)
VALUES ('Rock.Workflow.Action.CheckIn.FilterByGrade', NEWID(), 0, 0, 0)

IF NOT EXISTS(SELECT Id FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.FilterGroupsByGrade')
INSERT INTO EntityType (Name, Guid, IsEntity, IsSecured, IsCommon)
VALUES ('Rock.Workflow.Action.CheckIn.FilterGroupsByGrade', NEWID(), 0, 0, 0)

IF NOT EXISTS(SELECT Id FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.FilterGroupsByAbilityLevel')
INSERT INTO EntityType (Name, Guid, IsEntity, IsSecured, IsCommon)
VALUES ('Rock.Workflow.Action.CheckIn.FilterGroupsByAbilityLevel', NEWID(), 0, 0, 0)

IF NOT EXISTS(SELECT Id FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.FilterGroupsByLastName')
INSERT INTO EntityType (Name, Guid, IsEntity, IsSecured, IsCommon)
VALUES ('Rock.Workflow.Action.CheckIn.FilterGroupsByLastName', NEWID(), 0, 0, 0)

IF NOT EXISTS(SELECT Id FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.FilterGroupsBySpecialNeeds')
INSERT INTO EntityType (Name, Guid, IsEntity, IsSecured, IsCommon)
VALUES ('Rock.Workflow.Action.CheckIn.FilterGroupsBySpecialNeeds', NEWID(), 0, 0, 0)

IF NOT EXISTS(SELECT Id FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.FindFamilies')
INSERT INTO EntityType (Name, Guid, IsEntity, IsSecured, IsCommon)
VALUES ('Rock.Workflow.Action.CheckIn.FindFamilies', NEWID(), 0, 0, 0)

IF NOT EXISTS(SELECT Id FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.FindFamilyMembers')
INSERT INTO EntityType (Name, Guid, IsEntity, IsSecured, IsCommon)
VALUES ('Rock.Workflow.Action.CheckIn.FindFamilyMembers', NEWID(), 0, 0, 0)

IF NOT EXISTS(SELECT Id FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.FindRelationships')
INSERT INTO EntityType (Name, Guid, IsEntity, IsSecured, IsCommon)
VALUES ('Rock.Workflow.Action.CheckIn.FindRelationships', NEWID(), 0, 0, 0)

IF NOT EXISTS(SELECT Id FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.LoadGroups')
INSERT INTO EntityType (Name, Guid, IsEntity, IsSecured, IsCommon)
VALUES ('Rock.Workflow.Action.CheckIn.LoadGroups', NEWID(), 0, 0, 0)

IF NOT EXISTS(SELECT Id FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.LoadGroupTypes')
INSERT INTO EntityType (Name, Guid, IsEntity, IsSecured, IsCommon)
VALUES ('Rock.Workflow.Action.CheckIn.LoadGroupTypes', NEWID(), 0, 0, 0)

IF NOT EXISTS(SELECT Id FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.LoadLocations')
INSERT INTO EntityType (Name, Guid, IsEntity, IsSecured, IsCommon)
VALUES ('Rock.Workflow.Action.CheckIn.LoadLocations', NEWID(), 0, 0, 0)

IF NOT EXISTS(SELECT Id FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.LoadSchedules')
INSERT INTO EntityType (Name, Guid, IsEntity, IsSecured, IsCommon)
VALUES ('Rock.Workflow.Action.CheckIn.LoadSchedules', NEWID(), 0, 0, 0)

IF NOT EXISTS(SELECT Id FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.RemoveEmptyGroups')
INSERT INTO EntityType (Name, Guid, IsEntity, IsSecured, IsCommon)
VALUES ('Rock.Workflow.Action.CheckIn.RemoveEmptyGroups', NEWID(), 0, 0, 0)

IF NOT EXISTS(SELECT Id FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.RemoveEmptyGroupTypes')
INSERT INTO EntityType (Name, Guid, IsEntity, IsSecured, IsCommon)
VALUES ('Rock.Workflow.Action.CheckIn.RemoveEmptyGroupTypes', NEWID(), 0, 0, 0)

IF NOT EXISTS(SELECT Id FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.RemoveEmptyLocations')
INSERT INTO EntityType (Name, Guid, IsEntity, IsSecured, IsCommon)
VALUES ('Rock.Workflow.Action.CheckIn.RemoveEmptyLocations', NEWID(), 0, 0, 0)

IF NOT EXISTS(SELECT Id FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.RemoveEmptyPeople')
INSERT INTO EntityType (Name, Guid, IsEntity, IsSecured, IsCommon)
VALUES ('Rock.Workflow.Action.CheckIn.RemoveEmptyPeople', NEWID(), 0, 0, 0)

IF NOT EXISTS(SELECT Id FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.SaveAttendance')
INSERT INTO EntityType (Name, Guid, IsEntity, IsSecured, IsCommon)
VALUES ('Rock.Workflow.Action.CheckIn.SaveAttendance', NEWID(), 0, 0, 0)

IF NOT EXISTS(SELECT Id FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.CreateLabels')
INSERT INTO EntityType (Name, Guid, IsEntity, IsSecured, IsCommon)
VALUES ('Rock.Workflow.Action.CheckIn.CreateLabels', NEWID(), 0, 0, 0)

IF NOT EXISTS(SELECT Id FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.CalculateLastAttended')
INSERT INTO EntityType (Name, Guid, IsEntity, IsSecured, IsCommon)
VALUES ('Rock.Workflow.Action.CheckIn.CalculateLastAttended', NEWID(), 0, 0, 0)

-- Workflow Entity Type
IF NOT EXISTS(SELECT Id FROM EntityType WHERE Name = 'Rock.Model.Workflow')
INSERT INTO EntityType (Name, Guid, IsEntity, IsSecured, IsCommon)
VALUES ('Rock.Model.Workflow', NEWID(), 0, 0, 0)
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

DECLARE @CheckInCategoryId int
SET @CheckInCategoryId = (SELECT Id FROM [Category] WHERE Guid = '8F8B272D-D351-485E-86D6-3EE5B7C84D99')

INSERT INTO [WorkflowType] (IsSystem, IsActive, Name, [CategoryId], [Order], WorkTerm, IsPersisted, LoggingLevel, Guid)
VALUES (0, 1, 'Unattended Check-in', @CheckInCategoryId, 0, 'Check-in', 0, 3, '011E9F5A-60D4-4FF5-912A-290881E37EAF')
SET @WorkflowTypeId = SCOPE_IDENTITY()

UPDATE AV
SET [Value] = @WorkflowTypeId
FROM AttributeValue AV
INNER JOIN Attribute A ON A.Id = AV.AttributeId
WHERE A.[Key] = 'WorkflowTypeId' AND A.[Description] = 'The Id of the workflow type to activate for Check-in'

DECLARE @TextFieldTypeId int
SET @TextFieldTypeId = (SELECT Id FROM FieldType WHERE guid = '9C204CD0-1233-41C5-818A-C5DA439445AA')
DELETE [Attribute] WHERE guid = '9D2BFE8A-41F3-4A02-B3CF-9193F0C8419E'  -- old attribute, no longer added by script

-- Family Search Activity
DECLARE @WorkflowActivityTypeId int
DECLARE @WorkflowActionTypeId int
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
SELECT @WorkflowActivityTypeId, 'Find Family Members', 1, Id, 1, 0, NEWID() FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.FindFamilyMembers'
INSERT INTO [WorkflowActionType] (ActivityTypeId, Name, [Order], [EntityTypeId], IsActionCompletedOnSuccess, IsActivityCompletedOnSuccess, Guid)
SELECT @WorkflowActivityTypeId, 'Find Relationships', 2, Id, 1, 0, NEWID() FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.FindRelationships'
INSERT INTO [WorkflowActionType] (ActivityTypeId, Name, [Order], [EntityTypeId], IsActionCompletedOnSuccess, IsActivityCompletedOnSuccess, Guid)
SELECT @WorkflowActivityTypeId, 'Load Group Types', 3, Id, 1, 0, NEWID() FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.LoadGroupTypes'
INSERT INTO [WorkflowActionType] (ActivityTypeId, Name, [Order], [EntityTypeId], IsActionCompletedOnSuccess, IsActivityCompletedOnSuccess, Guid)
SELECT @WorkflowActivityTypeId, 'Filter GroupTypes by Age', 4, Id, 1, 0, NEWID() FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.FilterByAge'
INSERT INTO [WorkflowActionType] (ActivityTypeId, Name, [Order], [EntityTypeId], IsActionCompletedOnSuccess, IsActivityCompletedOnSuccess, Guid)
SELECT @WorkflowActivityTypeId, 'Filter GroupTypes by Grade', 5, Id, 1, 0, NEWID() FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.FilterByGrade'
INSERT INTO [WorkflowActionType] (ActivityTypeId, Name, [Order], [EntityTypeId], IsActionCompletedOnSuccess, IsActivityCompletedOnSuccess, Guid)
SELECT @WorkflowActivityTypeId, 'Load Groups', 6, Id, 1, 0, NEWID() FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.LoadGroups'
SET @WorkflowActionTypeId = SCOPE_IDENTITY()
	-- set the Load All attribute to true
	INSERT INTO [AttributeValue] ( IsSystem, AttributeId, [EntityId], [Order], [Value], Guid )
	SELECT 1, Id, @WorkflowActionTypeId, 0, 'True', NEWID() FROM [Attribute] WHERE Guid = '39762EF0-91D5-4B13-BD34-FC3AC3C24897'

INSERT INTO [WorkflowActionType] (ActivityTypeId, Name, [Order], [EntityTypeId], IsActionCompletedOnSuccess, IsActivityCompletedOnSuccess, Guid)
SELECT @WorkflowActivityTypeId, 'Filter Groups by Age', 6, Id, 1, 0, NEWID() FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.FilterGroupsByAge'
INSERT INTO [WorkflowActionType] (ActivityTypeId, Name, [Order], [EntityTypeId], IsActionCompletedOnSuccess, IsActivityCompletedOnSuccess, Guid)
SELECT @WorkflowActivityTypeId, 'Filter Groups by Grade', 8, Id, 1, 0, NEWID() FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.FilterGroupsByGrade'
INSERT INTO [WorkflowActionType] (ActivityTypeId, Name, [Order], [EntityTypeId], IsActionCompletedOnSuccess, IsActivityCompletedOnSuccess, Guid)
SELECT @WorkflowActivityTypeId, 'Load Locations', 9, Id, 1, 0, NEWID() FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.LoadLocations'
SET @WorkflowActionTypeId = SCOPE_IDENTITY()
	-- set the Load All attribute to true
	INSERT INTO [AttributeValue] ( IsSystem, AttributeId, [EntityId], [Order], [Value], Guid )
	SELECT 1, Id, @WorkflowActionTypeId, 0, 'True', NEWID() FROM [Attribute] WHERE Guid = '70203A96-AE70-47AD-A086-FD84792DF2B6'
	
INSERT INTO [WorkflowActionType] (ActivityTypeId, Name, [Order], [EntityTypeId], IsActionCompletedOnSuccess, IsActivityCompletedOnSuccess, Guid)
SELECT @WorkflowActivityTypeId, 'Remove Empty Groups', 10, Id, 1, 0, NEWID() FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.RemoveEmptyGroups'
INSERT INTO [WorkflowActionType] (ActivityTypeId, Name, [Order], [EntityTypeId], IsActionCompletedOnSuccess, IsActivityCompletedOnSuccess, Guid)
SELECT @WorkflowActivityTypeId, 'Remove Empty Group Types', 11, Id, 1, 0, NEWID() FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.RemoveEmptyGroupTypes'
INSERT INTO [WorkflowActionType] (ActivityTypeId, Name, [Order], [EntityTypeId], IsActionCompletedOnSuccess, IsActivityCompletedOnSuccess, Guid)
SELECT @WorkflowActivityTypeId, 'Remove Empty People', 12, Id, 1, 0, NEWID() FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.RemoveEmptyPeople'
INSERT INTO [WorkflowActionType] (ActivityTypeId, Name, [Order], [EntityTypeId], IsActionCompletedOnSuccess, IsActivityCompletedOnSuccess, Guid)
SELECT @WorkflowActivityTypeId, 'Update Last Attended', 13, Id, 1, 0, NEWID() FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.CalculateLastAttended'

-- Ability Level Search
INSERT INTO [WorkflowActivityType] (IsActive, WorkflowTypeId, Name, IsActivatedWithWorkflow, [Order], Guid)
VALUES (1, @WorkflowTypeId, 'Ability Level Search', 0, 3, NEWID())
SET @WorkflowActivityTypeId = SCOPE_IDENTITY()

INSERT INTO [WorkflowActionType] (ActivityTypeId, Name, [Order], [EntityTypeId], IsActionCompletedOnSuccess, IsActivityCompletedOnSuccess, Guid)
SELECT @WorkflowActivityTypeId, 'Filter Groups By Ability Level', 1, Id, 1, 0, NEWID() FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.FilterGroupsByAbilityLevel'
INSERT INTO [WorkflowActionType] (ActivityTypeId, Name, [Order], [EntityTypeId], IsActionCompletedOnSuccess, IsActivityCompletedOnSuccess, Guid)
SELECT @WorkflowActivityTypeId, 'Remove Empty Groups', 2, Id, 1, 0, NEWID() FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.RemoveEmptyGroups'
INSERT INTO [WorkflowActionType] (ActivityTypeId, Name, [Order], [EntityTypeId], IsActionCompletedOnSuccess, IsActivityCompletedOnSuccess, Guid)
SELECT @WorkflowActivityTypeId, 'Remove Empty Group Types', 3, Id, 1, 0, NEWID() FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.RemoveEmptyGroupTypes'

-- Group Search
INSERT INTO [WorkflowActivityType] (IsActive, WorkflowTypeId, Name, IsActivatedWithWorkflow, [Order], Guid)
VALUES (1, @WorkflowTypeId, 'Group Search', 0, 2, NEWID())
SET @WorkflowActivityTypeId = SCOPE_IDENTITY()

INSERT INTO [WorkflowActionType] (ActivityTypeId, Name, [Order], [EntityTypeId], IsActionCompletedOnSuccess, IsActivityCompletedOnSuccess, Guid)
SELECT @WorkflowActivityTypeId, 'Update Last Attended', 10, Id, 1, 0, NEWID() FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.CalculateLastAttended'

-- Location Search
INSERT INTO [WorkflowActivityType] (IsActive, WorkflowTypeId, Name, IsActivatedWithWorkflow, [Order], Guid)
VALUES (1, @WorkflowTypeId, 'Location Search', 0, 4, NEWID())
SET @WorkflowActivityTypeId = SCOPE_IDENTITY()

INSERT INTO [WorkflowActionType] (ActivityTypeId, Name, [Order], [EntityTypeId], IsActionCompletedOnSuccess, IsActivityCompletedOnSuccess, Guid)
SELECT @WorkflowActivityTypeId, 'Load Locations', 0, Id, 1, 0, NEWID() FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.LoadLocations'
INSERT INTO [WorkflowActionType] (ActivityTypeId, Name, [Order], [EntityTypeId], IsActionCompletedOnSuccess, IsActivityCompletedOnSuccess, Guid)
SELECT @WorkflowActivityTypeId, 'Filter Active Locations', 1, Id, 1, 0, NEWID() FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.FilterActiveLocations'
INSERT INTO [WorkflowActionType] (ActivityTypeId, Name, [Order], [EntityTypeId], IsActionCompletedOnSuccess, IsActivityCompletedOnSuccess, Guid)
SELECT @WorkflowActivityTypeId, 'Remove Empty Groups', 2, Id, 1, 0, NEWID() FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.RemoveEmptyGroups'
INSERT INTO [WorkflowActionType] (ActivityTypeId, Name, [Order], [EntityTypeId], IsActionCompletedOnSuccess, IsActivityCompletedOnSuccess, Guid)
SELECT @WorkflowActivityTypeId, 'Update Last Attended', 3, Id, 1, 0, NEWID() FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.CalculateLastAttended'

-- Schedule Search
INSERT INTO [WorkflowActivityType] (IsActive, WorkflowTypeId, Name, IsActivatedWithWorkflow, [Order], Guid)
VALUES (1, @WorkflowTypeId, 'Schedule Search', 0, 5, NEWID())
SET @WorkflowActivityTypeId = SCOPE_IDENTITY()

INSERT INTO [WorkflowActionType] (ActivityTypeId, Name, [Order], [EntityTypeId], IsActionCompletedOnSuccess, IsActivityCompletedOnSuccess, Guid)
SELECT @WorkflowActivityTypeId, 'Load Schedules', 0, Id, 1, 0, NEWID() FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.LoadSchedules'

-- Save Attendance
INSERT INTO [WorkflowActivityType] (IsActive, WorkflowTypeId, Name, IsActivatedWithWorkflow, [Order], Guid)
VALUES (1, @WorkflowTypeId, 'Save Attendance', 0, 6, NEWID())
SET @WorkflowActivityTypeId = SCOPE_IDENTITY()

INSERT INTO [WorkflowActionType] (ActivityTypeId, Name, [Order], [EntityTypeId], IsActionCompletedOnSuccess, IsActivityCompletedOnSuccess, Guid)
SELECT @WorkflowActivityTypeId, 'Save Attendance', 0, Id, 1, 0, NEWID() FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.SaveAttendance'
INSERT INTO [WorkflowActionType] (ActivityTypeId, Name, [Order], [EntityTypeId], IsActionCompletedOnSuccess, IsActivityCompletedOnSuccess, Guid)
SELECT @WorkflowActivityTypeId, 'Create Labels', 0, Id, 1, 0, NEWID() FROM EntityType WHERE Name = 'Rock.Workflow.Action.CheckIn.CreateLabels'
