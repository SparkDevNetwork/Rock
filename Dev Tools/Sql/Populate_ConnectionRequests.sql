/*
 * Creates @maxRequests Connection Requests for random people.
 * Be sure you have plenty of random people in your system first.
 */

DECLARE
  @maxRequests INT = 5000
  ,@StartDate AS DATE = DATEADD(m, -6, GETDATE())
  ,@EndDate AS DATE = GETDATE()

-------------------------------------------------------------------
DECLARE 
	 @coConnectionTypeId INT
	,@coConnectionOpportunityId INT
	,@coConnectionStatusId INT
	,@personAliasId INT
	,@recordStatusValueId INT
    ,@personRecordType INT = (
        SELECT id
        FROM DefinedValue
        WHERE guid = '36CF10D6-C695-413D-8E7C-4546EFEF385E'
        )
    ,@personRecordStatusDefinedTypeId INT = (
        SELECT id
        FROM DefinedType
        WHERE guid = '8522BADD-2871-45A5-81DD-C76DA07E2E7E'
        )
    ,@personConnectionStatusDefinedTypeId INT = (
        SELECT id
        FROM DefinedType
        WHERE guid = '2E6540EA-63F0-40FE-BE50-F2A84735E600'
    )
	,@personRecordStatusValueId INT
    ,@requestDate DATE

-- The "Active" person record status
SET @personRecordStatusValueId = (select top 1 id from DefinedValue where DefinedTypeId = @personRecordStatusDefinedTypeId AND [Value] = 'Active' )

-- Create a bunch of records
DECLARE @requestCounter int = 0
WHILE @requestCounter < @maxRequests
BEGIN

    SET @requestDate = (SELECT DATEADD(DAY, RAND(CHECKSUM(NEWID()))*(1+DATEDIFF(DAY, @StartDate, @EndDate)),@StartDate))
	SET @coConnectionTypeId = (SELECT TOP 1 Id FROM [ConnectionType] ORDER BY NEWID())
	SET @coConnectionOpportunityId = (SELECT TOP 1 Id FROM [ConnectionOpportunity] WHERE ConnectionTypeID = @coConnectionTypeId ORDER BY NEWID())
	SET @coConnectionStatusId = (SELECT TOP 1 Id FROM [ConnectionStatus] WHERE [ConnectionTypeId] = @coConnectionTypeId  ORDER BY NEWID())
	SET @personAliasId = (SELECT TOP 1 pa.[Id] FROM [PersonAlias] pa INNER JOIN [Person] p ON p.[Id] = pa.[PersonId] WHERE p.RecordTypeValueId = @personRecordType AND p.RecordStatusValueId = @personRecordStatusValueId ORDER BY NEWID() )

	INSERT 
		INTO [ConnectionRequest] 
			([ConnectionOpportunityId], [PersonAliasId], [Comments], [ConnectionStatusId], [ConnectionState], [Guid], [CreatedDateTime], [ModifiedDateTime], [CreatedDateKey], [ConnectionTypeId]) 
		VALUES 
			(@coConnectionOpportunityId, @personAliasId, '', @coConnectionStatusId, 0, NEWID(), @requestDate, @requestDate, convert(varchar, @requestDate, 112), @coConnectionTypeId )

	SET @requestCounter += 1;
END;