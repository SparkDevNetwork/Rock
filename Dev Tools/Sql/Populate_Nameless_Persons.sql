DECLARE @MAXPEOPLE INT = 10;

-- Step 1: Define well-known DefinedValue IDs used for creating Person records
DECLARE @RecordTypeValueId INT = (
    SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '721300ED-1267-4DA0-B4F2-6C6B5B17B1C5'
);
DECLARE @ConnectionStatusValueId INT = (
    SELECT [Id] FROM [DefinedValue] WHERE [Guid] = 'B91BA046-BC1E-400C-B85D-638C1F4E0CE2'
);
DECLARE @MobileTypeValueId INT = (
    SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '407e7e45-7b2e-4fcd-9605-ecb1339f2453'
);

-- Step 2: Loop to create dummy people with related data
DECLARE @i INT = 1;
DECLARE @NewPersonId INT;
DECLARE @PersonGuid UNIQUEIDENTIFIER;
DECLARE @AliasGuid UNIQUEIDENTIFIER;
DECLARE @PhoneNumber NVARCHAR(10);
DECLARE @Formatted NVARCHAR(20);
DECLARE @FullNumber NVARCHAR(11);

WHILE @i <= @MAXPEOPLE
BEGIN
    -- Generate random 10-digit phone number (not truly random for production)
    SET @PhoneNumber = RIGHT('0000000000' + CAST((ABS(CHECKSUM(NEWID())) % 10000000000) AS VARCHAR), 10);
    SET @Formatted = '(' + SUBSTRING(@PhoneNumber, 1, 3) + ') ' + SUBSTRING(@PhoneNumber, 4, 3) + '-' + SUBSTRING(@PhoneNumber, 7, 4);
    SET @FullNumber = '1' + @PhoneNumber;

    SET @PersonGuid = NEWID();
    SET @AliasGuid = NEWID();

    -- Insert into Person
    INSERT INTO [Person] (
        [IsSystem],
        [RecordTypeValueId],
        [RecordStatusValueId],
        [ConnectionStatusValueId],
        [IsDeceased],
        [FirstName],
        [LastName],
        [IsEmailActive],
        [Guid],
        [CreatedDateTime],
        [ModifiedDateTime],
        [EmailPreference],
        [GivingId],
        [Gender]
    )
    VALUES (
        0,
        @RecordTypeValueId,
        3, -- Active
        @ConnectionStatusValueId,
        0,
        'TestFirst' + CAST(@i AS VARCHAR),
        'TestLast' + CAST(@i AS VARCHAR),
        1,
        @PersonGuid,
        GETDATE(),
        GETDATE(),
        0,
        '', -- GivingId will be set next
        0   -- Unknown gender
    );

    SET @NewPersonId = SCOPE_IDENTITY();

    -- Set GivingId to "P{PersonId}"
    UPDATE [Person]
    SET GivingId = 'P' + CAST(@NewPersonId AS VARCHAR)
    WHERE Id = @NewPersonId;

    -- Insert PersonAlias (AliasPersonGuid must match Person.Guid)
    INSERT INTO [PersonAlias] (
        [PersonId],
        [AliasPersonId],
        [AliasPersonGuid],
        [Guid]
    )
    VALUES (
        @NewPersonId,
        @NewPersonId,
        @PersonGuid,
        @AliasGuid
    );

    -- Insert PhoneNumber with generated number
    INSERT INTO [PhoneNumber] (
        [PersonId],
        [NumberTypeValueId],
        [IsMessagingEnabled],
        [IsUnlisted],
        [Number],
        [CountryCode],
        [NumberFormatted],
        [FullNumber],
        [IsMessagingOptedOut],
        [IsSystem],
        [Guid]
    )
    VALUES (
        @NewPersonId,
        @MobileTypeValueId,
        1,              -- IsMessagingEnabled
        0,              -- IsUnlisted
        @PhoneNumber,
        1,              -- CountryCode
        @Formatted,
        @FullNumber,
        0,              -- IsMessagingOptedOut
        0,              -- IsSystem
        NEWID()
    );

    SET @i += 1;
END