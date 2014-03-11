DECLARE @campusLocationId int
DECLARE @buildingLocationId int
DECLARE @roomLocationId int

/*
DECLARE @downtownCampus uniqueidentifier = '252BCFB7-219C-4384-8C60-0668B1A8A5FB'
DECLARE @northCampus uniqueidentifier = 'C8BE1C2C-9D9D-4A8F-A19B-D3ECD20B4EED'
DECLARE @southCampus uniqueidentifier = 'DD415EC2-9471-45AB-8C21-FF3107B3E9AE'
*/

DECLARE @staffBuilding42Guid uniqueidentifier = 'DD415EC2-9471-45AB-8C21-FF3107B3E9AE'

begin transaction

delete from Location where ParentLocationId = (select [Id] from Location where [Guid] = @staffBuilding42Guid)
delete from Location where [Guid] = @staffBuilding42Guid

-- Main Campus "Location"
select @campusLocationId = (select Id from Location where Name = 'Main Campus');
if @campusLocationId is null
begin
  INSERT INTO [Location] ([Guid], [Name], [IsActive],[IsNamedLocation]) VALUES (NEWID(), 'Main Campus', 1, 1)  
  SET @campusLocationId = SCOPE_IDENTITY()
end

-- Staff building
INSERT INTO [Location] ([ParentLocationId], [Name], [IsActive], [IsNamedLocation], [Guid]) VALUES (@campusLocationId, 'Staff Building 42', 1, 1, @staffBuilding42Guid)
SET @buildingLocationId = SCOPE_IDENTITY()

-- Staff Rooms
INSERT INTO [Location] ([ParentLocationId], [Name], [IsActive], [IsNamedLocation],[Guid]) VALUES (@buildingLocationId, 'Designers Room', 1, 1, NEWID())
INSERT INTO [Location] ([ParentLocationId], [Name], [IsActive], [IsNamedLocation],[Guid]) VALUES (@buildingLocationId, 'North Cubes', 1, 1, NEWID())
INSERT INTO [Location] ([ParentLocationId], [Name], [IsActive], [IsNamedLocation],[Guid]) VALUES (@buildingLocationId, 'South Cubes', 1, 1, NEWID())
INSERT INTO [Location] ([ParentLocationId], [Name], [IsActive], [IsNamedLocation],[Guid]) VALUES (@buildingLocationId, 'Server Room', 1, 1, NEWID())
INSERT INTO [Location] ([ParentLocationId], [Name], [IsActive], [IsNamedLocation],[Guid]) VALUES (@buildingLocationId, 'West Kitchen', 1, 1, NEWID())
INSERT INTO [Location] ([ParentLocationId], [Name], [IsActive], [IsNamedLocation],[Guid]) VALUES (@buildingLocationId, 'East Kitchen', 1, 1, NEWID())
INSERT INTO [Location] ([ParentLocationId], [Name], [IsActive], [IsNamedLocation],[Guid]) VALUES (@buildingLocationId, 'Prayer Room', 1, 1, NEWID())
INSERT INTO [Location] ([ParentLocationId], [Name], [IsActive], [IsNamedLocation],[Guid]) VALUES (@buildingLocationId, 'Green Room', 1, 1, NEWID())
INSERT INTO [Location] ([ParentLocationId], [Name], [IsActive], [IsNamedLocation],[Guid]) VALUES (@buildingLocationId, 'Waiting Room', 1, 1, NEWID())
INSERT INTO [Location] ([ParentLocationId], [Name], [IsActive], [IsNamedLocation],[Guid]) VALUES (@buildingLocationId, 'Secretary Room', 1, 1, NEWID())

commit