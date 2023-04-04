SET NOCOUNT ON

-- Configuration for populating random attribute values.
DECLARE
    @personCounter INT = 0
    ,@personCountForAttributePopulation INT = 500
    ,@populateRandomAttributeValueDate DATE
   
    -- Guid given is well-known for Background Checked attribute. 
    ,@attributeForAddingRandomValue INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'DAF87B87-3D1E-463D-A197-52227FE4EA28')

    -- Guid given is well-known for Background Checked Date attribute. 
    ,@attributeForAddingRandomDateValue INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '3DAFF000-7F74-47D7-8CB0-E4A4E6C81F5F')

    -- Guid given is well-known for Background Checked Result attribute. 
    ,@attributeForAddingRandomResultValue INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '44490089-E02C-4E54-A456-454845ABBC9D')
    
DECLARE
    @personId int;

    WHILE (@personCounter < @personCountForAttributePopulation)
    BEGIN
    -- Get a random person from the database.
    SELECT @personId = (SELECT TOP 1 [Id] FROM [Person] ORDER BY NEWID());
        SET
    @populateRandomAttributeValueDate = CONVERT(VARCHAR, DATEADD(DAY, -ROUND(RAND() * 4000, 0), GETDATE()), 23);

    DECLARE @attributeCount INT = (SELECT COUNT(1) FROM [AttributeValue] WHERE [AttributeId] = @attributeForAddingRandomValue AND [EntityId] = @personId);
    DECLARE @attributeDateCount INT = (SELECT COUNT(1) FROM [AttributeValue] WHERE [AttributeId] = @attributeForAddingRandomDateValue AND [EntityId] = @personId);
    DECLARE @attributeResultCount INT = (SELECT COUNT(1) FROM [AttributeValue] WHERE [AttributeId] = @attributeForAddingRandomResultValue AND [EntityId] = @personId);
    IF (@attributeCount = 0 AND @attributeDateCount = 0 AND @attributeResultCount = 0)
        BEGIN
        INSERT INTO [AttributeValue]
        (
        [IsSystem],
        [Guid],
        [AttributeId],
        [EntityId],
        [Value]
        )
        VALUES
        (
         0,
         NEWID(),
         @attributeForAddingRandomDateValue,
         @personId,
         @populateRandomAttributeValueDate
        )

         INSERT INTO [AttributeValue]
        (
        [IsSystem],
        [Guid],
        [AttributeId],
        [EntityId],
        [Value]
        )
        VALUES
        (
         0,
         NEWID(),
         @attributeForAddingRandomValue,
         @personId,
         'True'
        )

        INSERT INTO [AttributeValue]
        (
        [IsSystem],
        [Guid],
        [AttributeId],
        [EntityId],
        [Value]
        )
        VALUES
        (
         0,
         NEWID(),
         @attributeForAddingRandomResultValue,
         @personId,
         'Pass'
        )

        SET @personCounter += 1;
        END
    END