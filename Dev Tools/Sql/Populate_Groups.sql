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
  @maxGroupsPerArea int = 50,
  @groupTypeNHRegionGuid nvarchar(36) = 'B31676B5-A004-4A72-AE0D-4D5E1C6BF5DB',
  @groupTypeNHAreaGuid nvarchar(36) = '30F8F34A-5E18-461E-BEFE-5563AA372574',
  @groupTypeNHGroupGuid nvarchar(36) = '4043A5D6-6C2F-49DB-92DD-BD6ED1937DA8'
begin

begin transaction

delete from [Group] where [GroupTypeId] in (select id from GroupType where Guid in (@groupTypeNHRegionGuid, @groupTypeNHAreaGuid, @groupTypeNHGroupGuid))
delete from [GroupTypeRole] where [GroupTypeId] in (select id from GroupType where Guid in (@groupTypeNHRegionGuid, @groupTypeNHAreaGuid, @groupTypeNHGroupGuid))
delete from [GroupTypeAssociation] where [GroupTypeId] in (select id from GroupType where Guid in (@groupTypeNHRegionGuid, @groupTypeNHAreaGuid, @groupTypeNHGroupGuid))
delete from [GroupType] where [Id] in (select id from GroupType where Guid in (@groupTypeNHRegionGuid, @groupTypeNHAreaGuid, @groupTypeNHGroupGuid))

select 'adding ' + CAST(@maxRegions AS VARCHAR(10)) + ' group regions';
select 'adding ' + CAST(@maxRegions*@maxAreasPerRegion AS VARCHAR(10)) + ' group areas';
select 'adding ' + CAST(@maxRegions*@maxAreasPerRegion*@maxGroupsPerArea AS VARCHAR(10)) + ' groups';

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
           ,0
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
           ,0
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
           ,0
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


select @campusId = null;

-- NG regions
while @regionCounter < @maxRegions
    begin
        select @groupGuid = NEWID();
        select @groupName = 'Region ' + REPLACE(str(@regionCounter, 2), ' ', '0');
		if @regionCounter = 0
		begin
		  set @groupName = 'Small Region';	
		end;

        select @groupDescription = 'Description of ' + @groupName;
        
        INSERT INTO [dbo].[Group] ([IsSystem],[ParentGroupId],[GroupTypeId],[CampusId],[Name],[Description],[IsSecurityRole],[IsActive],[Order],[Guid])
                            VALUES (0,null,@regionGroupTypeId,@campusId,@groupName,@groupDescription,0,1,0,@groupGuid);

        select @regionGroupId = @@IDENTITY;
        select @areaCounter = 0

        -- NG areas 
        while @areaCounter < @maxAreasPerRegion
            begin
                select @groupGuid = NEWID();
                select @groupName = 'Area ' + REPLACE(str(@regionCounter, 2), ' ', '0') + REPLACE(str(@areaCounter, 2), ' ', '0');
                select @groupDescription = 'Description of ' + @groupName;
        
				if @regionCounter = 0
				begin
					set @groupName = 'Small Area';
					set @groupDescription = 'This is a pretty small area';	
					set @areaCounter = 999; 
				end;

                INSERT INTO [dbo].[Group] ([IsSystem],[ParentGroupId],[GroupTypeId],[CampusId],[Name],[Description],[IsSecurityRole],[IsActive],[Order],[Guid])
                                    VALUES (0,@regionGroupId,@areaGroupTypeId,@campusId,@groupName,@groupDescription,0,1,0,@groupGuid);

                select @areaGroupId = @@IDENTITY;

                select @groupCounter = 0
				
        
                -- NG groups
                while @groupCounter < @maxGroupsPerArea
                    begin
                        select @groupGuid = NEWID();
                        select @groupName = 'Group ' + REPLACE(str(@regionCounter, 2), ' ', '0') + REPLACE(str(@areaCounter, 2), ' ', '0') + REPLACE(str(@groupCounter, 2), ' ', '0');
                        select @groupDescription = 'Description of ' + @groupName;

						if @regionCounter = 0
						begin
							set @groupName = 'Lone Group';	
							set @groupDescription = 'This is the only group in this area';
							set @groupCounter = 999; 
						end;
        
                        INSERT INTO [dbo].[Group] ([IsSystem],[ParentGroupId],[GroupTypeId],[CampusId],[Name],[Description],[IsSecurityRole],[IsActive],[Order],[Guid])
                                            VALUES (0,@areaGroupId,@groupTypeId,@campusId,@groupName,@groupDescription,0,1,0,@groupGuid);
       
                        set @groupCounter += 1;
                    end;
       
                set @areaCounter += 1;
            end;
        
       
        set @regionCounter += 1;
    end;

commit transaction

end