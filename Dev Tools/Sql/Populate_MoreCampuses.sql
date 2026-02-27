-- =====================================================================================================
-- Author:        Rock
-- Create Date:   03-11-2025
-- Description:   Populates more campuses into Rock for testing.
--
-- Change History:
--                 02-18-2026 NA - Added locations to the Peoria campus.
-- ======================================================================================================

DECLARE @campusLocationId INT;

DECLARE @campusLocationTypeValueId INT = (SELECT Id FROM [DefinedValue] WHERE [Guid] = 'C0D7AE35-7901-4396-870E-3AAF472AAE88');

DECLARE @campusStatusValueId INT = (SELECT Id FROM [DefinedValue] WHERE [Guid] = '10696fd8-d0c7-486f-b736-5fb3f5d69f1a'); -- open

DECLARE @campusTypeValueId INT = (SELECT Id FROM [DefinedValue] WHERE [Guid] = '5A61507B-79CB-4DA2-AF43-6F82260203B3'); -- physical
--------------------------------------------------------------------------------
-- Phoenix Central
DECLARE @PhoenixCentralLocationGuid uniqueidentifier = '11111111-1111-1111-1111-111111111111';

IF NOT EXISTS (SELECT 1 FROM [Location] WHERE [Guid] = @PhoenixCentralLocationGuid)
BEGIN
    INSERT INTO [Location] ([Guid], [Name], [IsActive], [Street1], [City], [State], [PostalCode], [GeoPoint], [LocationTypeValueId])
    VALUES (@PhoenixCentralLocationGuid, 'Phoenix Central', 1, '100 Church St', 'Phoenix', 'AZ', '85003', 'POINT(-112.0740 33.4484)', @campusLocationTypeValueId);

    SET @campusLocationId = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @campusLocationId = [Id]
    FROM [Location]
    WHERE [Guid] = @PhoenixCentralLocationGuid;
END;

IF NOT EXISTS (SELECT 1 FROM [Campus] WHERE [Guid] = 'A1B2C3D4-E5F6-7890-1234-56789ABCDEF0')
BEGIN
    INSERT INTO [Campus] ([Name], [IsSystem], [LocationId], [Guid], [IsActive], [CampusStatusValueId], [CampusTypeValueId])
    VALUES ('Phoenix Central', 0, @campusLocationId, 'A1B2C3D4-E5F6-7890-1234-56789ABCDEF0', 1, @campusStatusValueId, @campusTypeValueId);
END;

--------------------------------------------------------------------------------
-- Mesa East
DECLARE @MesaEastLocationGuid uniqueidentifier = '22222222-2222-2222-2222-222222222222';

IF NOT EXISTS (SELECT 1 FROM [Location] WHERE [Guid] = @MesaEastLocationGuid)
BEGIN
    INSERT INTO [Location] ([Guid], [Name], [IsActive], [Street1], [City], [State], [PostalCode], [GeoPoint], [LocationTypeValueId])
    VALUES (@MesaEastLocationGuid, 'Mesa East', 1, '200 Worship Way', 'Mesa', 'AZ', '85201', 'POINT(-111.8315 33.4152)', @campusLocationTypeValueId);

    SET @campusLocationId = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @campusLocationId = [Id]
    FROM [Location]
    WHERE [Guid] = @MesaEastLocationGuid;
END;

IF NOT EXISTS (SELECT 1 FROM [Campus] WHERE [Guid] = 'B2C3D4E5-F678-9012-3456-789ABCDE1234')
BEGIN
    INSERT INTO [Campus] ([Name], [IsSystem], [LocationId], [Guid], [IsActive], [CampusStatusValueId], [CampusTypeValueId])
    VALUES ('Mesa East', 0, @campusLocationId, 'B2C3D4E5-F678-9012-3456-789ABCDE1234', 1, @campusStatusValueId, @campusTypeValueId);
END;

--------------------------------------------------------------------------------
-- Scottsdale North
DECLARE @ScottsdaleNorthLocationGuid uniqueidentifier = '33333333-3333-3333-3333-333333333333';

IF NOT EXISTS (SELECT 1 FROM [Location] WHERE [Guid] = @ScottsdaleNorthLocationGuid)
BEGIN
    INSERT INTO [Location] ([Guid], [Name], [IsActive], [Street1], [City], [State], [PostalCode], [GeoPoint], [LocationTypeValueId])
    VALUES (@ScottsdaleNorthLocationGuid, 'Scottsdale North', 1, '300 Grace Ave', 'Scottsdale', 'AZ', '85251', 'POINT(-111.9261 33.4942)', @campusLocationTypeValueId);

    SET @campusLocationId = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @campusLocationId = [Id]
    FROM [Location]
    WHERE [Guid] = @ScottsdaleNorthLocationGuid;
END;

IF NOT EXISTS (SELECT 1 FROM [Campus] WHERE [Guid] = 'C3D4E5F6-7890-1234-5678-9ABCDEF12345')
BEGIN
    INSERT INTO [Campus] ([Name], [IsSystem], [LocationId], [Guid], [IsActive], [CampusStatusValueId], [CampusTypeValueId])
    VALUES ('Scottsdale North', 0, @campusLocationId, 'C3D4E5F6-7890-1234-5678-9ABCDEF12345', 1, @campusStatusValueId, @campusTypeValueId);
END;

--------------------------------------------------------------------------------
-- Chandler South
DECLARE @ChandlerSouthLocationGuid uniqueidentifier = '44444444-4444-4444-4444-444444444444';

IF NOT EXISTS (SELECT 1 FROM [Location] WHERE [Guid] = @ChandlerSouthLocationGuid)
BEGIN
    INSERT INTO [Location] ([Guid], [Name], [IsActive], [Street1], [City], [State], [PostalCode], [GeoPoint], [LocationTypeValueId])
    VALUES (@ChandlerSouthLocationGuid, 'Chandler South', 1, '400 Faith Rd', 'Chandler', 'AZ', '85225', 'POINT(-111.8413 33.3062)', @campusLocationTypeValueId);

    SET @campusLocationId = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @campusLocationId = [Id]
    FROM [Location]
    WHERE [Guid] = @ChandlerSouthLocationGuid;
END;

IF NOT EXISTS (SELECT 1 FROM [Campus] WHERE [Guid] = 'D4E5F678-9012-3456-789A-BCDEF1234567')
BEGIN
    INSERT INTO [Campus] ([Name], [IsSystem], [LocationId], [Guid], [IsActive], [CampusStatusValueId], [CampusTypeValueId])
    VALUES ('Chandler South', 0, @campusLocationId, 'D4E5F678-9012-3456-789A-BCDEF1234567', 1, @campusStatusValueId, @campusTypeValueId);
END;

--------------------------------------------------------------------------------
-- Glendale West
DECLARE @GlendaleWestLocationGuid uniqueidentifier = '55555555-5555-5555-5555-555555555555';

IF NOT EXISTS (SELECT 1 FROM [Location] WHERE [Guid] = @GlendaleWestLocationGuid)
BEGIN
    INSERT INTO [Location] ([Guid], [Name], [IsActive], [Street1], [City], [State], [PostalCode], [GeoPoint], [LocationTypeValueId])
    VALUES (@GlendaleWestLocationGuid, 'Glendale West', 1, '500 Rock Ave', 'Glendale', 'AZ', '85301', 'POINT(-112.1860 33.5387)', @campusLocationTypeValueId);

    SET @campusLocationId = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @campusLocationId = [Id]
    FROM [Location]
    WHERE [Guid] = @GlendaleWestLocationGuid;
END;

IF NOT EXISTS (SELECT 1 FROM [Campus] WHERE [Guid] = 'E5F67890-1234-5678-9ABC-DEF123456789')
BEGIN
    INSERT INTO [Campus] ([Name], [IsSystem], [LocationId], [Guid], [IsActive], [CampusStatusValueId], [CampusTypeValueId])
    VALUES ('Glendale West', 0, @campusLocationId, 'E5F67890-1234-5678-9ABC-DEF123456789', 1, @campusStatusValueId, @campusTypeValueId);
END;

--------------------------------------------------------------------------------
-- Gilbert East
DECLARE @GilbertEastLocationGuid uniqueidentifier = '66666666-6666-6666-6666-666666666666';

IF NOT EXISTS (SELECT 1 FROM [Location] WHERE [Guid] = @GilbertEastLocationGuid)
BEGIN
    INSERT INTO [Location] ([Guid], [Name], [IsActive], [Street1], [City], [State], [PostalCode], [GeoPoint], [LocationTypeValueId])
    VALUES (@GilbertEastLocationGuid, 'Gilbert East', 1, '600 Hope Blvd', 'Gilbert', 'AZ', '85234', 'POINT(-111.7890 33.3528)', @campusLocationTypeValueId);

    SET @campusLocationId = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @campusLocationId = [Id]
    FROM [Location]
    WHERE [Guid] = @GilbertEastLocationGuid;
END;

IF NOT EXISTS (SELECT 1 FROM [Campus] WHERE [Guid] = 'F6789012-3456-789A-BCDE-F123456789AB')
BEGIN
    INSERT INTO [Campus] ([Name], [IsSystem], [LocationId], [Guid], [IsActive], [CampusStatusValueId], [CampusTypeValueId])
    VALUES ('Gilbert East', 0, @campusLocationId, 'F6789012-3456-789A-BCDE-F123456789AB', 1, @campusStatusValueId, @campusTypeValueId);
END;

--------------------------------------------------------------------------------
-- Peoria
DECLARE @PeoriaCampusGuid uniqueidentifier = '77777777-7777-7777-7777-777777777777';

IF NOT EXISTS (SELECT 1 FROM [Location] WHERE [Guid] = @PeoriaCampusGuid)
BEGIN
    INSERT INTO [Location] ([Guid], [Name], [IsActive], [Street1], [City], [State], [PostalCode], [GeoPoint], [LocationTypeValueId])
    VALUES (@PeoriaCampusGuid, 'Peoria', 1, '700 Worship Dr', 'Peoria', 'AZ', '85382', 'POINT(-112.2374 33.5806)', @campusLocationTypeValueId);

    SET @campusLocationId = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @campusLocationId = [Id]
    FROM [Location]
    WHERE [Guid] = @PeoriaCampusGuid;
END;

IF NOT EXISTS (SELECT 1 FROM [Campus] WHERE [Guid] = '67890123-4567-89AB-CDEF-123456789ABC')
BEGIN
    INSERT INTO [Campus] ([Name], [IsSystem], [LocationId], [Guid], [IsActive], [CampusStatusValueId], [CampusTypeValueId])
    VALUES ('Peoria', 0, @campusLocationId, '67890123-4567-89AB-CDEF-123456789ABC', 1, @campusStatusValueId, @campusTypeValueId);
END;

--------------------------------------------------------------------------------
-- Tempe University
DECLARE @TempeUniversityLocationGuid uniqueidentifier = '88888888-8888-8888-8888-888888888888';

IF NOT EXISTS (SELECT 1 FROM [Location] WHERE [Guid] = @TempeUniversityLocationGuid)
BEGIN
    INSERT INTO [Location] ([Guid], [Name], [IsActive], [Street1], [City], [State], [PostalCode], [GeoPoint], [LocationTypeValueId])
    VALUES (@TempeUniversityLocationGuid, 'Tempe University', 1, '800 Spirit St', 'Tempe', 'AZ', '85281', 'POINT(-111.9400 33.4255)', @campusLocationTypeValueId);

    SET @campusLocationId = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @campusLocationId = [Id]
    FROM [Location]
    WHERE [Guid] = @TempeUniversityLocationGuid;
END;

IF NOT EXISTS (SELECT 1 FROM [Campus] WHERE [Guid] = '78901234-5678-9ABC-DEF1-23456789ABCD')
BEGIN
    INSERT INTO [Campus] ([Name], [IsSystem], [LocationId], [Guid], [IsActive], [CampusStatusValueId], [CampusTypeValueId])
    VALUES ('Tempe University', 0, @campusLocationId, '78901234-5678-9ABC-DEF1-23456789ABCD', 1, @campusStatusValueId, @campusTypeValueId);
END;

--------------------------------------------------------------------------------
-- Avondale Southwest
DECLARE @AvondaleSouthwestLocationGuid uniqueidentifier = '99999999-9999-9999-9999-999999999999';

IF NOT EXISTS (SELECT 1 FROM [Location] WHERE [Guid] = @AvondaleSouthwestLocationGuid)
BEGIN
    INSERT INTO [Location] ([Guid], [Name], [IsActive], [Street1], [City], [State], [PostalCode], [GeoPoint], [LocationTypeValueId])
    VALUES (@AvondaleSouthwestLocationGuid, 'Avondale Southwest', 1, '900 Faithful Way', 'Avondale', 'AZ', '85323', 'POINT(-112.3496 33.4356)', @campusLocationTypeValueId);

    SET @campusLocationId = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @campusLocationId = [Id]
    FROM [Location]
    WHERE [Guid] = @AvondaleSouthwestLocationGuid;
END;

IF NOT EXISTS (SELECT 1 FROM [Campus] WHERE [Guid] = '89012345-6789-ABCD-EF12-3456789ABCDE')
BEGIN
    INSERT INTO [Campus] ([Name], [IsSystem], [LocationId], [Guid], [IsActive], [CampusStatusValueId], [CampusTypeValueId])
    VALUES ('Avondale Southwest', 0, @campusLocationId, '89012345-6789-ABCD-EF12-3456789ABCDE', 1, @campusStatusValueId, @campusTypeValueId);
END;

--------------------------------------------------------------------------------
-- Surprise North
DECLARE @SurpriseNorthLocationGuid uniqueidentifier = 'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA';

IF NOT EXISTS (SELECT 1 FROM [Location] WHERE [Guid] = @SurpriseNorthLocationGuid)
BEGIN
    INSERT INTO [Location] ([Guid], [Name], [IsActive], [Street1], [City], [State], [PostalCode], [GeoPoint], [LocationTypeValueId])
    VALUES (@SurpriseNorthLocationGuid, 'Surprise North', 1, '1000 Trinity Ct', 'Surprise', 'AZ', '85374', 'POINT(-112.3679 33.6292)', @campusLocationTypeValueId);

    SET @campusLocationId = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @campusLocationId = [Id]
    FROM [Location]
    WHERE [Guid] = @SurpriseNorthLocationGuid;
END;

IF NOT EXISTS (SELECT 1 FROM [Campus] WHERE [Guid] = '90123456-789A-BCDE-F123-456789ABCDEF')
BEGIN
    INSERT INTO [Campus] ([Name], [IsSystem], [LocationId], [Guid], [IsActive], [CampusStatusValueId], [CampusTypeValueId])
    VALUES ('Surprise North', 0, @campusLocationId, '90123456-789A-BCDE-F123-456789ABCDEF', 1, @campusStatusValueId, @campusTypeValueId);
END;

--------------------------------------------------------------------------------
--
-- Insert some Peoria Locations
--
--------------------------------------------------------------------------------

DECLARE @BuildingLocationTypeId int = (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = 'D9646A93-1667-4A44-82DA-12E1229B4695');
DECLARE @RoomLocationTypeId int = (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '107C6DA1-266D-4E1C-A443-1CD37064601D');

DECLARE @AuditoriumGuid uniqueidentifier = 'A1B0A314-EF23-4FBF-B7E8-9C73A0B69FF7';
DECLARE @YouthBldgGuid uniqueidentifier = 'B2BAE69F-2F00-49FA-BD90-CF7AFC087D73';

-- Prefer GUID-based lookup so we match even if name/type changed.
DECLARE @PeoriaCampusLocationId int =
(
    SELECT [Id]
    FROM [Location]
    WHERE [Guid] = @PeoriaCampusGuid
);

-- Create Locations
DECLARE @LocationId int;

-- Create Auditorium
IF NOT EXISTS ( SELECT 1 FROM [Location] WHERE [Guid] = @AuditoriumGuid )
BEGIN
    INSERT INTO [Location] ([Name], [ParentLocationId], [LocationTypeValueId], [IsActive], [Guid])
    VALUES ('Auditorium', @PeoriaCampusLocationId, @BuildingLocationTypeId, 1, @AuditoriumGuid);

    SET @LocationId = SCOPE_IDENTITY();

    -- Create Auditorium Locations
    INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid]) VALUES (@LocationId, 'Auditorium Sec. A', @RoomLocationTypeId, 1, NEWID());
    INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid]) VALUES (@LocationId, 'Auditorium Sec. B', @RoomLocationTypeId, 1, NEWID());
    INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid]) VALUES (@LocationId, 'Auditorium Sec. C', @RoomLocationTypeId, 1, NEWID());
    INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid]) VALUES (@LocationId, 'Auditorium Sec. D', @RoomLocationTypeId, 1, NEWID());
    INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid]) VALUES (@LocationId, 'Auditorium Sec. E', @RoomLocationTypeId, 1, NEWID());
    INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid]) VALUES (@LocationId, 'Courtyard', @RoomLocationTypeId, 1, NEWID());
    INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid]) VALUES (@LocationId, 'East Entrance', @RoomLocationTypeId, 1, NEWID());
    INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid]) VALUES (@LocationId, 'West Entrance', @RoomLocationTypeId, 1, NEWID());
    INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid]) VALUES (@LocationId, 'Chapel', @RoomLocationTypeId, 1, NEWID());
    INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid]) VALUES (@LocationId, 'Lobby', @RoomLocationTypeId, 1, NEWID());
    INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid]) VALUES (@LocationId, 'Prayer Room', @RoomLocationTypeId, 1, NEWID());
    INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid]) VALUES (@LocationId, 'Baptismal', @RoomLocationTypeId, 1, NEWID());
    INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid]) VALUES (@LocationId, 'Stage', @RoomLocationTypeId, 1, NEWID());
    INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid]) VALUES (@LocationId, 'A/V Studio', @RoomLocationTypeId, 1, NEWID());
    INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid]) VALUES (@LocationId, 'Multipurpose', @RoomLocationTypeId, 1, NEWID());
    INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid]) VALUES (@LocationId, 'Cafe', @RoomLocationTypeId, 1, NEWID());
    INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid]) VALUES (@LocationId, 'Bookstore', @RoomLocationTypeId, 1, NEWID());
END;

-- Create Youth Bldg
IF NOT EXISTS ( SELECT 1 FROM [Location] WHERE [Guid] = @YouthBldgGuid )
BEGIN
    INSERT INTO [Location] ([Name], [ParentLocationId], [LocationTypeValueId], [IsActive], [Guid])
    VALUES ('Youth Bldg', @PeoriaCampusLocationId, @BuildingLocationTypeId, 1, @YouthBldgGuid);

    SET @LocationId = SCOPE_IDENTITY();

    -- Create Youth Bldg Locations
    INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid]) VALUES (@LocationId, 'Blue Jays', @RoomLocationTypeId, 1, NEWID());
    INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid]) VALUES (@LocationId, 'Cardinals', @RoomLocationTypeId, 1, NEWID());
    INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid]) VALUES (@LocationId, 'Deer', @RoomLocationTypeId, 1, NEWID());
    INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid]) VALUES (@LocationId, 'Foxes', @RoomLocationTypeId, 1, NEWID());
    INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid]) VALUES (@LocationId, 'Hawks', @RoomLocationTypeId, 1, NEWID());
    INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid]) VALUES (@LocationId, 'Otters', @RoomLocationTypeId, 1, NEWID());
    INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid]) VALUES (@LocationId, 'Owls', @RoomLocationTypeId, 1, NEWID());
    INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid]) VALUES (@LocationId, 'Porcupines', @RoomLocationTypeId, 1, NEWID());
    INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid]) VALUES (@LocationId, 'Possums', @RoomLocationTypeId, 1, NEWID());
    INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid]) VALUES (@LocationId, 'Quails', @RoomLocationTypeId, 1, NEWID());
    INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid]) VALUES (@LocationId, 'Raccoons', @RoomLocationTypeId, 1, NEWID());
    INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid]) VALUES (@LocationId, 'Ravens', @RoomLocationTypeId, 1, NEWID());
    INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid]) VALUES (@LocationId, 'Road Runners', @RoomLocationTypeId, 1, NEWID());
    INSERT INTO [Location] ([ParentLocationId], [Name], [LocationTypeValueId], [IsActive], [Guid]) VALUES (@LocationId, 'Wolves', @RoomLocationTypeId, 1, NEWID());
END;
