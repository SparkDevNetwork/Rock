delete from [Group] where IsSystem = 0
delete from [GroupTypeAssociation] where [GroupTypeId] in (select id from GroupType where IsSystem = 0)
delete from [GroupRole] where [GroupTypeId] in (select id from GroupType where IsSystem = 0)
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

begin transaction

select 'adding ' + CAST(@maxRegions AS VARCHAR(10)) + ' group regions';
select 'adding ' + CAST(@maxRegions*@maxAreasPerRegion AS VARCHAR(10)) + ' group areas';
select 'adding ' + CAST(@maxRegions*@maxAreasPerRegion*@maxGroupsPerArea AS VARCHAR(10)) + ' groups';

INSERT INTO [dbo].[GroupType]
           ([IsSystem]
           ,[Name]
           ,[Description]
           ,[DefaultGroupRoleId]
           ,[IconCssClass]
           ,[Guid])
     VALUES
           (0
           ,'Neighborhood Group Region'
           ,'The Neighborhood Group Regions'
           ,null
           ,'icon-heart-empty'
           ,newid())

select @regionGroupTypeId = @@IDENTITY

INSERT INTO [dbo].[GroupType]
           ([IsSystem]
           ,[Name]
           ,[Description]
           ,[DefaultGroupRoleId]
           ,[IconCssClass]
           ,[Guid])
     VALUES
           (0
           ,'Neighborhood Group Area'
           ,'The Neighborhood Group Areas'
           ,null
           ,'icon-random'
           ,newid())

select @areaGroupTypeId = @@IDENTITY

INSERT INTO [dbo].[GroupType]
           ([IsSystem]
           ,[Name]
           ,[Description]
           ,[DefaultGroupRoleId]
           ,[IconCssClass]
           ,[Guid])
     VALUES
           (0
           ,'Neighborhood Group'
           ,'The Neighborhood Groups'
           ,null
           ,'icon-home'
           ,newid())

select @groupTypeId = @@IDENTITY

INSERT INTO [dbo].[GroupRole] 
    ([IsSystem] ,[GroupTypeId] ,[Name] ,[Description] ,[SortOrder] ,[MaxCount] ,[MinCount] ,[Guid] ,[IsLeader])
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

commit transaction

end