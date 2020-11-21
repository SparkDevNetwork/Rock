/*
 * Creates two new buildings ("Auditorium" and "Youth Bldg") with set number of rooms
 * and creates a new "Bldg 2" with N number of rooms defined below.
 * 
 * Connects all the Serving Team type groups with various locations and schedules, then
 * sets up some min, desired and max values.
 */

DECLARE
  @maxRooms int = 50

------------------------------------------------------------------------------------------------------------
BEGIN TRANSACTION

DECLARE @CampusLocationTypeId int = (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = 'C0D7AE35-7901-4396-870E-3AAF472AAE88' )
DECLARE @BuildingLocationTypeId int = (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = 'D9646A93-1667-4A44-82DA-12E1229B4695' )
DECLARE @RoomLocationTypeId int = (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '107C6DA1-266D-4E1C-A443-1CD37064601D' )
DECLARE @AuditoriumGuid uniqueidentifier = 'A1B0A314-EF23-4FBF-B7E8-9C73A0B69FF7'
DECLARE @YouthBldgGuid uniqueidentifier = 'B2BAE69F-2F00-49FA-BD90-CF7AFC087D73'

-- Get Main Campus
DECLARE @MainCampusLocationId int = (SELECT [Id] FROM [Location] WHERE [Name] = 'Main Campus' AND [LocationTypeValueId] = @CampusLocationTypeId )

-- Create Locations
DECLARE @LocationId int

-- Create Auditorium
IF NOT EXISTS ( SELECT 1 FROM [Location] WHERE [Guid] = @AuditoriumGuid )
BEGIN
	INSERT INTO [Location] ([Name], [ParentLocationId], [LocationTypeValueId], [IsActive], [Guid])	VALUES ( 'Auditorium', @MainCampusLocationId, @BuildingLocationTypeId, 1, @AuditoriumGuid )
	SET @LocationId = SCOPE_IDENTITY()

	-- Create Auditorium Locations
	INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid])	VALUES (@LocationId, 'Auditorium Sec. A', @RoomLocationTypeId, 1, NEWID())
	INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid])	VALUES (@LocationId, 'Auditorium Sec. B', @RoomLocationTypeId, 1, NEWID())
	INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid])	VALUES (@LocationId, 'Auditorium Sec. C', @RoomLocationTypeId, 1, NEWID())
	INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid])	VALUES (@LocationId, 'Auditorium Sec. D', @RoomLocationTypeId, 1, NEWID())
	INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid])	VALUES (@LocationId, 'Auditorium Sec. E', @RoomLocationTypeId, 1, NEWID())
	INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid])	VALUES (@LocationId, 'Courtyard', @RoomLocationTypeId, 1, NEWID())
	INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid])	VALUES (@LocationId, 'East Entrance', @RoomLocationTypeId, 1, NEWID())
	INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid])	VALUES (@LocationId, 'West Entrance', @RoomLocationTypeId, 1, NEWID())
	INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid])	VALUES (@LocationId, 'Chapel', @RoomLocationTypeId, 1, NEWID())
	INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid])	VALUES (@LocationId, 'Lobby', @RoomLocationTypeId, 1, NEWID())
	INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid])	VALUES (@LocationId, 'Prayer Room', @RoomLocationTypeId, 1, NEWID())
	INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid])	VALUES (@LocationId, 'Baptismal', @RoomLocationTypeId, 1, NEWID())
	INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid])	VALUES (@LocationId, 'Stage', @RoomLocationTypeId, 1, NEWID())
	INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid])	VALUES (@LocationId, 'A/V Studio', @RoomLocationTypeId, 1, NEWID())
	INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid])	VALUES (@LocationId, 'Multipurpose', @RoomLocationTypeId, 1, NEWID())
	INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid])	VALUES (@LocationId, 'Cafe', @RoomLocationTypeId, 1, NEWID())
	INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid])	VALUES (@LocationId, 'Bookstore', @RoomLocationTypeId, 1, NEWID())
END

-- Create Youth Bldg
IF NOT EXISTS ( SELECT 1 FROM [Location] WHERE [Guid] = @YouthBldgGuid )
BEGIN
	INSERT INTO [Location] ([Name], [ParentLocationId], [LocationTypeValueId], [IsActive], [Guid])	VALUES ( 'Youth Bldg', @MainCampusLocationId, @BuildingLocationTypeId, 1, @YouthBldgGuid )
	SET @LocationId = SCOPE_IDENTITY()

	-- Create Youth Bldg Locations
	INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid])	VALUES (@LocationId, 'Blue Jays', @RoomLocationTypeId, 1, NEWID())
	INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid])	VALUES (@LocationId, 'Cardinals', @RoomLocationTypeId, 1, NEWID())
	INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid])	VALUES (@LocationId, 'Deer', @RoomLocationTypeId, 1, NEWID())
	INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid])	VALUES (@LocationId, 'Foxes', @RoomLocationTypeId, 1, NEWID())
	INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid])	VALUES (@LocationId, 'Hawks', @RoomLocationTypeId, 1, NEWID())
	INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid])	VALUES (@LocationId, 'Otters', @RoomLocationTypeId, 1, NEWID())
	INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid])	VALUES (@LocationId, 'Owls', @RoomLocationTypeId, 1, NEWID())
	INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid])	VALUES (@LocationId, 'Porcupines', @RoomLocationTypeId, 1, NEWID())
	INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid])	VALUES (@LocationId, 'Possums', @RoomLocationTypeId, 1, NEWID())
	INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid])	VALUES (@LocationId, 'Quails', @RoomLocationTypeId, 1, NEWID())
	INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid])	VALUES (@LocationId, 'Raccoons', @RoomLocationTypeId, 1, NEWID())
	INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid])	VALUES (@LocationId, 'Ravens', @RoomLocationTypeId, 1, NEWID())
	INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid])	VALUES (@LocationId, 'Road Runners', @RoomLocationTypeId, 1, NEWID())
	INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid])	VALUES (@LocationId, 'Wolves', @RoomLocationTypeId, 1, NEWID())
END


-- Create Bldg2
IF NOT EXISTS ( SELECT 1 FROM [Location] WHERE [Guid] = 'C3B5DA51-B2CF-4649-8515-D63DFC49CD8F' )
BEGIN
	DECLARE
		@roomName nvarchar(100),
		@roomCounter int = 1
  
	INSERT INTO [Location] ([Name], [ParentLocationId], [LocationTypeValueId], [IsActive], [Guid])	VALUES ( 'Bldg 2', @MainCampusLocationId, @BuildingLocationTypeId, 1, 'C3B5DA51-B2CF-4649-8515-D63DFC49CD8F' )
	SET @LocationId = SCOPE_IDENTITY()

	-- Create a bunch of rooms in Bldg2
	WHILE @roomCounter < @maxRooms
	BEGIN
		SELECT @roomName = 'Room ' + REPLACE(str(@roomCounter, 3), ' ', '0')
		INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid])	VALUES (@LocationId, @roomName, @RoomLocationTypeId, 1, NEWID())
		SET @roomCounter += 1;
	END;
END


/**************************************************
 * Create Group Location Schedules
 **************************************************/

DECLARE @servingGroupTypeId  INT = (SELECT [Id] FROM [GroupType] WHERE [Guid] = '2C42B2D4-1C5F-4AD5-A9AD-08631B872AC4' ) -- Serving Team group type
DECLARE @MeetingLocationTypeId INT = (SELECT [Id] FROM DefinedValue WHERE [Guid] = '96D540F5-071D-4BBD-9906-28F0A64D39C4')
DECLARE @availableUsherGreeterLocations TABLE(
    Id INT NOT NULL
);

DECLARE @availableChildrenLocations TABLE(
    Id INT NOT NULL
);

DECLARE @availableSchedules TABLE(
    Id INT NOT NULL
);

-- Populate our temp table variables
INSERT INTO @availableUsherGreeterLocations (Id)
	SELECT [Id] FROM [Location] WHERE ParentLocationId IN (SELECT [Id] FROM [Location] WHERE [Guid] = 'A1B0A314-EF23-4FBF-B7E8-9C73A0B69FF7')

INSERT INTO @availableChildrenLocations (Id)
	SELECT [Id] FROM [Location] WHERE ParentLocationId IN (SELECT [Id] FROM [Location] WHERE [Guid] = 'B2BAE69F-2F00-49FA-BD90-CF7AFC087D73')

INSERT INTO @availableSchedules (Id)
	SELECT [Id] FROM [Schedule] WHERE [Name] IN ('Saturday 4:30pm','Saturday 6:00pm','Sunday 9:00am','Sunday 10:30am','Sunday 12:00pm')

-- Insert Usher and Greeter group locations
INSERT INTO [GroupLocation] (
		GroupId
		,LocationId
		,GroupLocationTypeValueId
		,IsMailingLocation
		,IsMappedLocation
		,[Guid]
		,[CreatedDateTime]
		,[ModifiedDateTime]
		,[Order]
	)
	SELECT G.Id, L.Id,@MeetingLocationTypeId, 0, 0, NEWID(), GETDATE(), GETDATE(), 0
	FROM [Group] G, @availableUsherGreeterLocations L 
	WHERE G.ID IN ( SELECT [Id] FROM [Group] WHERE [GroupTypeId] = @servingGroupTypeId AND ParentGroupId IS NOT NULL AND Name IN ('Ushers','Greeters') )
	AND NOT EXISTS
	(
		SELECT * FROM  [GroupLocation] GL WHERE GL.[Groupid] IN ( SELECT [Id] FROM [Group] WHERE G.[Id] = [Id] ) AND GL.[LocationId] = L.[Id]
	)

-- Insert all Children's serving team group locations
INSERT INTO [GroupLocation] (
		GroupId
		,LocationId
		,GroupLocationTypeValueId
		,IsMailingLocation
		,IsMappedLocation
		,[Guid]
		,[CreatedDateTime]
		,[ModifiedDateTime]
		,[Order]
	)
	SELECT G.Id, L.Id,@MeetingLocationTypeId, 0, 0, NEWID(), GETDATE(), GETDATE(), 0
	FROM [Group] G, @availableChildrenLocations L 
	WHERE G.ID IN ( SELECT [Id] FROM [Group] WHERE [GroupTypeId] = @servingGroupTypeId AND ParentGroupId IS NOT NULL AND Name IN ('Children''s') )
	AND NOT EXISTS
	(
		SELECT * FROM  [GroupLocation] GL WHERE GL.[GroupId] IN ( SELECT [Id] FROM [Group] WHERE G.[Id] = [Id] ) AND GL.[LocationId] = L.[Id]
	)

-- Insert Usher, Greeter and Children's group location schedules
INSERT INTO [GroupLocationSchedule] (
	GroupLocationId
	,ScheduleId
	)
	SELECT GL.[Id], S.[Id]
	FROM [GroupLocation] GL, @availableSchedules S
	WHERE GL.[GroupId] IN ( SELECT [Id] FROM [Group] WHERE [GroupTypeId] = @servingGroupTypeId AND ParentGroupId IS NOT NULL AND Name IN ('Ushers','Greeters', 'Children''s') )
	AND NOT EXISTS
	(
		SELECT * FROM  [GroupLocationSchedule] GLS WHERE GLS.[GroupLocationId] = GL.[Id] AND GLS.[ScheduleId] IN (SELECT [Id] FROM @availableSchedules)
	)

-- Insert some random GroupLocationScheduleConfig for those Usher, Greeter and Children's group location schedules
INSERT INTO GroupLocationScheduleConfig (
	GroupLocationId
	,ScheduleId
	,MinimumCapacity
	,DesiredCapacity
	,MaximumCapacity
	)
	SELECT 
		GLS.[GroupLocationId]
		,GLS.[ScheduleId]
		,ROUND( RAND( CAST( NEWID() AS varbinary ) ) * 2, 0 ) + 1
		,ROUND( RAND( CAST( NEWID() AS varbinary ) ) * 3, 0 ) + 4
		,ROUND( RAND( CAST( NEWID() AS varbinary ) ) * 7, 0 ) + 7
	FROM [GroupLocationSchedule] GLS
	WHERE GLS.[GroupLocationId] IN 
		( SELECT [Id] FROM [GroupLocation] WHERE 
		[GroupId] IN 
			( SELECT [Id] FROM [Group] WHERE [GroupTypeId] = @servingGroupTypeId AND ParentGroupId IS NOT NULL AND Name IN ('Ushers','Greeters', 'Children''s') )
		)
	AND NOT EXISTS
	(
		SELECT * FROM  [GroupLocationScheduleConfig] GLSC WHERE GLSC.[GroupLocationId] = GLS.[GroupLocationId] AND GLSC.[ScheduleId] = GLS.[ScheduleId]
	)

-- check work
SELECT COUNT(1) AS 'Group Location Schedule Configurations' FROM [GroupLocationScheduleConfig]

COMMIT TRANSACTION