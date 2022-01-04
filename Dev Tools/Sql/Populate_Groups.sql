-- =====================================================================================================
-- Author:        Rock
-- Create Date: 
-- Modified Date: 01-03-2021
-- Description:   Populates groups for the specified GroupTypeIds that set using the Group GUID.
--
-- Change History:
--                 01-03-2021 COREYH - Add this script description. 
--                                     Also added the script requirements.
-- Requirement:
--                 Paramater [@groupTypeNHRegionGuid] must be set to a valid Group GUID.
--                 Paramater [@groupTypeNHAreaGuid] must be set to a valid Group GUID.
--                 Paramater [@groupTypeNHGroupGuid] must be set to a valid Group GUID.
-- ======================================================================================================
SET NOCOUNT ON
DECLARE
  @regionGroupId int = null,
  @areaGroupId int = null,
  @groupGroupId int = null,
  @regionGroupTypeId int,
  @areaGroupTypeId int,
  @groupTypeId int,
  @campusId int = null,
  @groupName nvarchar(100),
  @groupDescription nvarchar(max),
  @groupGuid nvarchar(max),
  @regionCounter int = 0,
  @maxRegions int = 10,  
  @areaCounter int = 0,
  @maxAreasPerRegion int = 20,
  @groupCounter int = 0,
  @childGroupCounter int = 0,
  @maxGroupsPerArea int = 25,
  @maxGroupsPerGroup int = 3000,
  @groupTypeNHRegionGuid nvarchar(36) = 'B31676B5-A004-4A72-AE0D-4D5E1C6BF5DB',
  @groupTypeNHAreaGuid nvarchar(36) = '30F8F34A-5E18-461E-BEFE-5563AA372574',
  @groupTypeNHGroupGuid nvarchar(36) = '4043A5D6-6C2F-49DB-92DD-BD6ED1937DA8'
BEGIN

/*delete from [Group] where [GroupTypeId] in (select id from GroupType where Guid in (@groupTypeNHRegionGuid, @groupTypeNHAreaGuid, @groupTypeNHGroupGuid))
delete from [GroupTypeRole] where [GroupTypeId] in (select id from GroupType where Guid in (@groupTypeNHRegionGuid, @groupTypeNHAreaGuid, @groupTypeNHGroupGuid))
delete from [GroupTypeAssociation] where [GroupTypeId] in (select id from GroupType where Guid in (@groupTypeNHRegionGuid, @groupTypeNHAreaGuid, @groupTypeNHGroupGuid))
delete from [GroupType] where [Id] in (select id from GroupType where Guid in (@groupTypeNHRegionGuid, @groupTypeNHAreaGuid, @groupTypeNHGroupGuid))
*/

SELECT 'adding ' + CAST(@maxRegions AS VARCHAR(10)) + ' group regions';
SELECT 'adding ' + CAST(@maxRegions*@maxAreasPerRegion AS VARCHAR(10)) + ' group areas';
SELECT 'adding ' + CAST(@maxRegions*@maxAreasPerRegion*@maxGroupsPerArea AS VARCHAR(10)) + ' groups';
/*
INSERT INTO [dbo].[GroupType]
           ([IsSystem]
           ,[Name]
           ,[Description]
           ,[GroupTerm]
           ,[GroupMemberTerm]
           ,[DefaultGroupRoleId]
           ,[AllowMultipleLocations]
           ,[ShowInGroupList]
           ,[ShowInNavigation]
           ,[IconCssClass]
           ,[TakesAttendance]
           ,[AttendanceRule]
           ,[AttendancePrintTo]
           ,[Order]
           ,[LocationSelectionMode]
           ,[Guid])
     VALUES
           (0
           ,'Neighborhood Group Region'
           ,'The Neighborhood Group Regions'
           ,'Group'
           ,'Member' 
           ,null
           ,0
           ,1
           ,1
           ,'icon-globe'
           ,0
           ,0
           ,0
           ,0
           ,0
           ,@groupTypeNHRegionGuid)

select @regionGroupTypeId = @@IDENTITY

INSERT INTO [dbo].[GroupType]
           ([IsSystem]
           ,[Name]
           ,[Description]
           ,[GroupTerm]
           ,[GroupMemberTerm]
           ,[DefaultGroupRoleId]
           ,[AllowMultipleLocations]
           ,[ShowInGroupList]
           ,[ShowInNavigation]
           ,[IconCssClass]
           ,[TakesAttendance]
           ,[AttendanceRule]
           ,[AttendancePrintTo]
           ,[Order]
           ,[LocationSelectionMode]
           ,[Guid])
     VALUES
           (0
           ,'Neighborhood Group Area'
           ,'The Neighborhood Group Areas'
           ,'Group'
           ,'Member' 
           ,null
           ,0
           ,1
           ,1
           ,'icon-group'
           ,0
           ,0
           ,0
           ,0
           ,0
           ,@groupTypeNHAreaGuid)

select @areaGroupTypeId = @@IDENTITY

INSERT INTO [dbo].[GroupType]
           ([IsSystem]
           ,[Name]
           ,[Description]
           ,[GroupTerm]
           ,[GroupMemberTerm]
           ,[DefaultGroupRoleId]
           ,[AllowMultipleLocations]
           ,[ShowInGroupList]
           ,[ShowInNavigation]
           ,[IconCssClass]
           ,[TakesAttendance]
           ,[AttendanceRule]
           ,[AttendancePrintTo]
           ,[Order]
           ,[LocationSelectionMode]
           ,[Guid])
     VALUES
           (0
           ,'Neighborhood Group'
           ,'The Neighborhood Groups'
           ,'Group'
           ,'Member' 
           ,null
           ,0
           ,1
           ,1
           ,'icon-home'
           ,0
           ,0
           ,0
           ,0
           ,0
           ,@groupTypeNHGroupGuid)

select @groupTypeId = @@IDENTITY

INSERT INTO [dbo].[GroupTypeRole] 
    ([IsSystem] ,[GroupTypeId] ,[Name] ,[Description] ,[Order] ,[MaxCount] ,[MinCount] ,[Guid] ,[IsLeader])
     VALUES
    (0, @groupTypeId, 'Leader', '', 0, null, null, NEWID(), 1),
    (0, @groupTypeId, 'Assistant Leader', '', 0, null, null, NEWID(), 0),
    (0, @groupTypeId, 'Host', '', 0, null, null, NEWID(), 0),
    (0, @groupTypeId, 'Member', '', 0, null, null, NEWID(), 0)

-- setup valid child group types
insert into [dbo].[GroupTypeAssociation] 
    (GroupTypeId, ChildGroupTypeId)
values
    (@regionGroupTypeId, @areaGroupTypeId),
    (@areaGroupTypeId, @groupTypeId)
*/

SELECT @regionGroupTypeId = (SELECT TOP 1 [Id] FROM GroupType WHERE [Guid] = @groupTypeNHRegionGuid);
SELECT @areaGroupTypeId =   (SELECT TOP 1 [Id] FROM GroupType WHERE [Guid] = @groupTypeNHAreaGuid);
SELECT @groupTypeId =       (SELECT TOP 1 [Id] FROM GroupType WHERE [Guid] = @groupTypeNHGroupGuid);

SELECT @campusId = null;

-- NG regions
WHILE @regionCounter < @maxRegions
    BEGIN
        
        SELECT @groupGuid = NEWID();
        SELECT @groupName = 'Region ' + REPLACE(str(@regionCounter, 2), ' ', '0');
		IF @regionCounter = 0
		BEGIN
		  SET @groupName = 'Small Region';	
		END;

        SELECT @groupDescription = 'Description of ' + @groupName;
        
        INSERT INTO [dbo].[Group] ([IsSystem],[ParentGroupId],[GroupTypeId],[CampusId],[Name],[Description],[IsSecurityRole],[IsActive],[Order],[Guid])
                            VALUES (0,null,@regionGroupTypeId,@campusId,@groupName,@groupDescription,0,1,0,@groupGuid);

        SELECT @regionGroupId = @@IDENTITY;
        SELECT @areaCounter = 0

        -- NG areas 
        WHILE @areaCounter < @maxAreasPerRegion
            BEGIN
                SELECT @groupGuid = NEWID();
                SELECT @groupName = 'Area ' + REPLACE(str(@regionCounter, 2), ' ', '0') + REPLACE(str(@areaCounter, 2), ' ', '0');
                SELECT @groupDescription = 'Description of ' + @groupName;
        
				IF @regionCounter = 0
				BEGIN
					SET @groupName = 'Small Area';
					SET @groupDescription = 'This is a pretty small area';	
					SET @areaCounter = 999; 
				END;

                INSERT INTO [dbo].[Group] ([IsSystem],[ParentGroupId],[GroupTypeId],[CampusId],[Name],[Description],[IsSecurityRole],[IsActive],[Order],[Guid])
                                    VALUES (0,@regionGroupId,@areaGroupTypeId,@campusId,@groupName,@groupDescription,0,1,0,@groupGuid);

                SELECT @areaGroupId = @@IDENTITY;

                SELECT @groupCounter = 0
        
                -- NG groups
                WHILE @groupCounter < @maxGroupsPerArea
                    BEGIN
                        SELECT @groupGuid = NEWID();
                        SELECT @groupName = 'Group ' + REPLACE(str(@regionCounter, 2), ' ', '0') + REPLACE(str(@areaCounter, 2), ' ', '0') + REPLACE(str(@groupCounter, 2), ' ', '0');
                        SELECT @groupDescription = 'Description of ' + @groupName;

						IF @regionCounter = 0
						BEGIN
							SET @groupName = 'Lone Group';	
							SET @groupDescription = 'This is the only group in this area';
							SET @groupCounter = 999; 
						END;
        
                        INSERT INTO [dbo].[Group] ([IsSystem],[ParentGroupId],[GroupTypeId],[CampusId],[Name],[Description],[IsSecurityRole],[IsActive],[Order],[Guid])
                                            VALUES (0,@areaGroupId,@groupTypeId,@campusId,@groupName,@groupDescription,0,1,0,@groupGuid);

                        SET @groupGroupId = @@IDENTITY

                         SELECT @childGroupCounter = 0
                         WHILE @childGroupCounter < @maxGroupsPerGroup
                         BEGIN
                            SELECT @groupGuid = NEWID();
                            SELECT @groupName = 'ChildGroup ' + REPLACE(str(@regionCounter, 2), ' ', '0') + REPLACE(str(@areaCounter, 2), ' ', '0') + REPLACE(str(@childGroupCounter, 2), ' ', '0');
                            SELECT @groupDescription = 'Description of ' + @groupName;
        
                            INSERT INTO [dbo].[Group] ([IsSystem],[ParentGroupId],[GroupTypeId],[CampusId],[Name],[Description],[IsSecurityRole],[IsActive],[Order],[Guid])
                                                VALUES (0,@groupGroupId,@groupTypeId,@campusId,@groupName,@groupDescription,0,1,0,@groupGuid);
                             SET @childGroupCounter += 1
                         END
       
                        SET @groupCounter += 1;
                        IF (@groupCounter%50 = 0)
                        BEGIN
                          PRINT @groupCounter
                          PRINT @areaCounter
                          PRINT @regionCounter
                          PRINT '----'
                        END
                    END;
       
                SET @areaCounter += 1;
            END;
       
        SET @regionCounter += 1;
    END;
END