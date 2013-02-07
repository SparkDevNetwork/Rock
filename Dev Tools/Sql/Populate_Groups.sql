delete from [Group] where IsSystem = 0
delete from [GroupType] where IsSystem = 0 and [Id] not in (select GroupTypeId from GroupTypeAssociation union select ChildGroupTypeId from GroupTypeAssociation)

declare
  @regionGroupId int = null,
  @areaGroupId int = null,
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
  @maxAreasPerRegion int = 25,
  @groupCounter int = 0,
  @maxGroupsPerArea int = 50

begin

select CONCAT('adding ', @maxRegions, ' group regions');
select CONCAT('adding ', @maxRegions*@maxAreasPerRegion, ' group areas');
select CONCAT('adding ', @maxRegions*@maxAreasPerRegion*@maxGroupsPerArea, ' groups');

INSERT INTO [dbo].[GroupType]
           ([IsSystem]
           ,[Name]
           ,[Description]
           ,[DefaultGroupRoleId]
           ,[Guid])
     VALUES
           (0
           ,'Neighborhood Group Region'
           ,'The Neighborhood Group Regions'
           ,null
           ,newid())

select @regionGroupTypeId = @@IDENTITY

INSERT INTO [dbo].[GroupType]
           ([IsSystem]
           ,[Name]
           ,[Description]
           ,[DefaultGroupRoleId]
           ,[Guid])
     VALUES
           (0
           ,'Neighborhood Group Area'
           ,'The Neighborhood Group Areas'
           ,null
           ,newid())

select @areaGroupTypeId = @@IDENTITY

INSERT INTO [dbo].[GroupType]
           ([IsSystem]
           ,[Name]
           ,[Description]
           ,[DefaultGroupRoleId]
           ,[Guid])
     VALUES
           (0
           ,'Neighborhood Group'
           ,'The Neighborhood Groups'
           ,null
           ,newid())

select @groupTypeId = @@IDENTITY
select @campusId = null;

-- NG regions
while @regionCounter < @maxRegions
    begin
        select @groupGuid = NEWID();
        select @groupName = 'Region ' + REPLACE(str(@regionCounter, 2), ' ', '0');
        select @groupDescription = 'Description of ' + @groupName;
        
        INSERT INTO [dbo].[Group] ([IsSystem],[ParentGroupId],[GroupTypeId],[CampusId],[Name],[Description],[IsSecurityRole],[IsActive],[Guid])
                            VALUES (0,null,@regionGroupTypeId,@campusId,@groupName,@groupDescription,0,1,@groupGuid);

        select @regionGroupId = @@IDENTITY;
        select @areaCounter = 0

        -- NG areas 
        while @areaCounter < @maxAreasPerRegion
            begin
                select @groupGuid = NEWID();
                select @groupName = 'Area ' + REPLACE(str(@regionCounter, 2), ' ', '0') + REPLACE(str(@areaCounter, 2), ' ', '0');
                select @groupDescription = 'Description of ' + @groupName;
        
                INSERT INTO [dbo].[Group] ([IsSystem],[ParentGroupId],[GroupTypeId],[CampusId],[Name],[Description],[IsSecurityRole],[IsActive],[Guid])
                                    VALUES (0,@regionGroupId,@areaGroupTypeId,@campusId,@groupName,@groupDescription,0,1,@groupGuid);

                select @areaGroupId = @@IDENTITY;
                select @groupCounter = 0
        
                -- NG groups
                while @groupCounter < @maxGroupsPerArea
                    begin
                        select @groupGuid = NEWID();
                        select @groupName = 'Group ' + REPLACE(str(@regionCounter, 2), ' ', '0') + REPLACE(str(@areaCounter, 2), ' ', '0') + REPLACE(str(@groupCounter, 2), ' ', '0');
                        select @groupDescription = 'Description of ' + @groupName;
        
                        INSERT INTO [dbo].[Group] ([IsSystem],[ParentGroupId],[GroupTypeId],[CampusId],[Name],[Description],[IsSecurityRole],[IsActive],[Guid])
                                            VALUES (0,@areaGroupId,@groupTypeId,@campusId,@groupName,@groupDescription,0,1,@groupGuid);
       
                        set @groupCounter += 1;
                    end;
       
                set @areaCounter += 1;
            end;
        
       
        set @regionCounter += 1;
    end;

end

/*
select * from [Group]
delete from [Group] where IsSystem = 0
delete from [GroupType] where Id not in (select GroupTypeId from [Group])
*/

