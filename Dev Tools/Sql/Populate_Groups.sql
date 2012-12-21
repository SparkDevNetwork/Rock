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
  @groupCount int = 0,
  @childGroupCount int = 0,
  @childChildGroupCount int = 0

begin

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
while @groupCount < 10
    begin
        select @groupGuid = NEWID();
        select @groupName = 'Region ' + REPLACE(str(@groupCount, 2), ' ', '0');
        select @groupDescription = 'Description of ' + @groupName;
        
        INSERT INTO [dbo].[Group] ([IsSystem],[ParentGroupId],[GroupTypeId],[CampusId],[Name],[Description],[IsSecurityRole],[IsActive],[Guid])
                            VALUES (0,null,@regionGroupTypeId,@campusId,@groupName,@groupDescription,0,1,@groupGuid);

        select @regionGroupId = @@IDENTITY;
        select @childGroupCount = 0

        -- NG areas 
        while @childGroupCount < 25
            begin
                select @groupGuid = NEWID();
                select @groupName = 'Area ' + REPLACE(str(@groupCount, 2), ' ', '0') + REPLACE(str(@childGroupCount, 2), ' ', '0');
                select @groupDescription = 'Description of ' + @groupName;
        
                INSERT INTO [dbo].[Group] ([IsSystem],[ParentGroupId],[GroupTypeId],[CampusId],[Name],[Description],[IsSecurityRole],[IsActive],[Guid])
                                    VALUES (0,@regionGroupId,@areaGroupTypeId,@campusId,@groupName,@groupDescription,0,1,@groupGuid);

                select @areaGroupId = @@IDENTITY;
                select @childChildGroupCount = 0
        
                -- NG groups
                while @childChildGroupCount < 5
                    begin
                        select @groupGuid = NEWID();
                        select @groupName = 'Group ' + REPLACE(str(@groupCount, 2), ' ', '0') + REPLACE(str(@childGroupCount, 2), ' ', '0') + REPLACE(str(@childChildGroupCount, 2), ' ', '0');
                        select @groupDescription = 'Description of ' + @groupName;
        
                        INSERT INTO [dbo].[Group] ([IsSystem],[ParentGroupId],[GroupTypeId],[CampusId],[Name],[Description],[IsSecurityRole],[IsActive],[Guid])
                                            VALUES (0,@areaGroupId,@groupTypeId,@campusId,@groupName,@groupDescription,0,1,@groupGuid);
       
                        set @childChildGroupCount += 1;
                    end;
       
                set @childGroupCount += 1;
            end;
        
       
        set @groupCount += 1;
    end;

end

/*
select * from [Group]
delete from [Group] where IsSystem = 0
delete from [GroupType] where Id not in (select GroupTypeId from [Group])
*/

