SET NOCOUNT ON

----------------------------------------------
-- README
-- Adds 25000 sample Communications with an avg of 8 Communication Recipients
-- So this end up with around 200000 CommunicationRecipient records
-- It could take a couple of minutes to run this script
-- This generates mostly garbage data, so it is really only useful for performance testing
----------------------------------------------
DECLARE @recipientPersonAliasId INT
	,@senderPersonAliasId INT
	,@communicationCounter INT = 0
	,@maxCommunicationCount INT = 25000

    -- it does a batch communications around 5% of the time, and then a random number of recipients (up to 500) for each batch communication
    ,@maxCommunicationRecipientBatchCount INT = 500
	,@maxPersonAliasIdForCommunications INT = (
		SELECT max(Id)
		FROM (
			SELECT TOP 4000 Id
			FROM PersonAlias
			ORDER BY Id
			) x
		)
	,/* limit to first 4000 persons in the database */
	@communicationDateTime DATETIME
	,@futureSendDateTime DATETIME
	,@communicationSubject NVARCHAR(max)
	,@communicationId INT
	,@mediumEntityTypeId INT
    ,@mediumEntityTypeName NVARCHAR(max)
    ,@transportEntityTypeId int
    ,@transportEntityTypeName nvarchar(max)
	,@yearsBack INT = 4
	,@isbulk BIT
	,@communicationStatus INT = 0

   	,@smsNumbersDefinedTypeId int = (select top 1 Id from DefinedType where [Guid] = '611BDE1F-7405-4D16-8626-CCFEDB0E62BE')

declare
    @relatedSmsFromDefinedValueId int = (select top 1 Id from DefinedValue where DefinedTypeId = @smsNumbersDefinedTypeId)

DECLARE @daysBack INT = @yearsBack * 366

BEGIN

/*
 select count(*) from Communication
 select count(*) from CommunicationRecipient
 delete [CommunicationRecipient]
 delete [Communication]
*/
	
    
    SET @communicationDateTime = DATEADD(DAY, - @daysBack, SYSDATETIME())

	WHILE @communicationCounter < @maxCommunicationCount
	BEGIN
		SET @communicationSubject = 'Random Subject ' + convert(NVARCHAR(max), rand());
		SET @recipientPersonAliasId = (
				SELECT TOP 1 Id
				FROM PersonAlias
				WHERE Id <= rand() * @maxPersonAliasIdForCommunications
				ORDER BY Id DESC
				);

		WHILE @recipientPersonAliasId IS NULL
		BEGIN
			-- Try again just in case we didn't get anybody
			SET @recipientPersonAliasId = (
					SELECT TOP 1 Id
					FROM PersonAlias
					WHERE Id <= rand() * @maxPersonAliasIdForCommunications
					ORDER BY Id DESC
					);
		END

		SET @senderPersonAliasId = (
				SELECT TOP 1 Id
				FROM PersonAlias
				WHERE Id <= rand() * @maxPersonAliasIdForCommunications
				ORDER BY Id DESC
				);

		WHILE @senderPersonAliasId IS NULL
		BEGIN
			-- Try again just in case we didn't get anybody
			SET @senderPersonAliasId = (
					SELECT TOP 1 Id
					FROM PersonAlias
					WHERE Id <= rand() * @maxPersonAliasIdForCommunications
					ORDER BY Id DESC
					);
		END

		SET @futureSendDateTime = DATEADD(DAY, round(50 * rand(), 0) - 1, @communicationDateTime)

		IF (@futureSendDateTime = @communicationDateTime)
		BEGIN
			SET @futureSendDateTime = NULL
		END

		SET @communicationStatus = ROUND(5 * RAND(), 0)

		SELECT TOP 1 @mediumEntityTypeId = id, @mediumEntityTypeName = et.Name
		FROM EntityType et
		WHERE Name LIKE 'Rock.Communication.Medium%'
		ORDER BY newid()

        if (@mediumEntityTypeName like '%.Sms') begin
            SELECT TOP 1 @transportEntityTypeId = id, @transportEntityTypeName = [Name]
		    FROM EntityType
		    WHERE Name LIKE 'Rock.Communication.Transport.Twilio'
		    ORDER BY newid()
        end else begin
            SELECT TOP 1 @transportEntityTypeId = id, @transportEntityTypeName = [Name]
		    FROM EntityType
		    WHERE Name LIKE 'Rock.Communication.Transport%'
		    ORDER BY newid()
        end

		-- generate a bulk about 1/20th of the time
		SELECT @isbulk = CASE CAST(ROUND(RAND() * 20, 0) AS BIT)
				WHEN 1
					THEN 0
				ELSE 1
				END

		INSERT INTO [dbo].[Communication] (
			[Subject]
			,[FutureSendDateTime]
			,[Status]
			,[ReviewedDateTime]
			,[ReviewerNote]
			,[AdditionalMergeFieldsJson]
			,[Guid]
			,[CreatedDateTime]
			,[ModifiedDateTime]
			,[CreatedByPersonAliasId]
			,[ModifiedByPersonAliasId]
			,[ForeignId]
			,[IsBulkCommunication]
			,[SenderPersonAliasId]
			,[ReviewerPersonAliasId]
			,[MediumDataJson]
            ,[SMSFromDefinedValueId]
			)
		VALUES (
			@communicationSubject
			,@futureSendDateTime
			,@communicationStatus
			,DATEADD(DAY, round(50 * rand(), 0), @communicationDateTime)
			,'some note'
			,''
			,newid()
			,@communicationDateTime
			,@communicationDateTime
			,NULL
			,NULL
			,NULL
			,@isbulk
			,@senderPersonAliasId
			,NULL
			,NULL
            ,@relatedSmsFromDefinedValueId
			)

		SET @communicationId = SCOPE_IDENTITY()

		IF (@isbulk = 0)
		BEGIN
			INSERT INTO [dbo].[CommunicationRecipient] (
				[CommunicationId]
				,[Status]
				,[StatusNote]
				,[AdditionalMergeValuesJson]
				,[Guid]
				,[CreatedDateTime]
				,[ModifiedDateTime]
				,[CreatedByPersonAliasId]
				,[ModifiedByPersonAliasId]
				,[OpenedDateTime]
				,[OpenedClient]
				,[ForeignId]
				,[TransportEntityTypeName]
				,[UniqueMessageId]
				,[ResponseCode]
				,[PersonAliasId]
                ,[MediumEntityTypeId]
                ,[SentMessage]
				)
			VALUES (
				@communicationId
				,ROUND(5 * RAND(), 0) -- [Status]
				,NULL -- [StatusNote]
				,CONCAT(newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid()) -- [AdditionalMergeValuesJson]
				,newid()
				,@communicationDateTime -- CreatedDateTime
				,@communicationDateTime -- ModifiedDateTIme
				,NULL -- [CreatedByPersonAliasId]
				,NULL -- [ModifiedByPersonAliasId]
				,DATEADD(day, 5, @communicationDateTime) -- [OpenedDateTime]
				,NULL --OpenedClient
				,NULL -- ForeignId
				,@transportEntityTypeName
				,NULL
				,ABS(CHECKSUM(NewId())) % 99999 -- ResponseCode
				,@recipientPersonAliasId
                ,@mediumEntityTypeId
                ,CONCAT(newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid()) -- [SentMessage]
				)
		END
		ELSE
		BEGIN
            -- get a random number of recipients between 1 and @maxCommunicationRecipientBatchCount, but have it tend to be on the lower side
            declare @recipientCount int = round(SQUARE(rand() * SQRT(@maxCommunicationRecipientBatchCount)), 0);

			INSERT INTO [dbo].[CommunicationRecipient] (
				[CommunicationId]
				,[Status]
				,[StatusNote]
				,[AdditionalMergeValuesJson]
				,[Guid]
				,[CreatedDateTime]
				,[ModifiedDateTime]
				,[CreatedByPersonAliasId]
				,[ModifiedByPersonAliasId]
				,[OpenedDateTime]
				,[OpenedClient]
				,[ForeignId]
				,[TransportEntityTypeName]
				,[UniqueMessageId]
				,[ResponseCode]
				,[PersonAliasId]
                ,[MediumEntityTypeId]
                ,[SentMessage]
				)
			SELECT top (@recipientCount) @communicationId
				,ROUND(5 * RAND(), 0)
				,NULL
				,NULL
				,newid()
				,@communicationDateTime
				,@communicationDateTime
				,NULL
				,NULL
				,DATEADD(day, 5, @communicationDateTime)
				,NULL
				,NULL
				, @transportEntityTypeName
				,NULL
				,ABS(CHECKSUM(NewId())) % 99999 -- ResponseCode
				,Id
                ,@mediumEntityTypeId
                ,CONCAT(newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid(), newid())
			FROM PersonAlias tablesample( 2500 rows)
		END

		DECLARE @messageKey NVARCHAR(11)
        
        if @mediumEntityTypeName like '%.Sms' begin
			select top 1 @messageKey = FullNumber
			FROM PhoneNumber tablesample(10 percent)
			WHERE IsMessagingEnabled = 1
        end
        else begin
            set @messageKey =  ''
        end

		INSERT INTO [dbo].[CommunicationResponse] (
			[MessageKey]
			,[FromPersonAliasId]
			,[ToPersonAliasId]
			,[IsRead]
			,[RelatedSmsFromDefinedValueId]
			,[RelatedTransportEntityTypeId]
			,[RelatedMediumEntityTypeId]
			,[Response]
			,[CreatedDateTime]
			,[Guid]
			)
		VALUES (
			@messageKey
			,null -- @recipientPersonAliasId
			,@senderPersonAliasId
			,0
			,@relatedSmsFromDefinedValueId
			,@transportEntityTypeId
			,@mediumEntityTypeId
			,CONCAT (
				'Some Message'
				,rand()
				)
			,DATEADD(DAY, - round(50 * rand(), 0), getdate())
			,NEWID()
			)

        if (@communicationCounter % 50 = 0)
		begin
  		  print @communicationCounter  
        end

        SET @communicationCounter += 1;
        
		SET @communicationDateTime = DATEADD(ss, (86000 * @daysBack / @maxCommunicationCount), @communicationDateTime);
	END
END;

select count(*) [Total Communications] from Communication 
select count(*) [Total Communication Recipients] from CommunicationRecipient

