SET NOCOUNT ON

----------------------------------------------
-- README
-- Adds 250,000 sample communications (takes a long time, 20+ minutes)
----------------------------------------------
DECLARE @recipientPersonAliasId INT
	,@senderPersonAliasId INT
	,@communicationCounter INT = 0
	,@maxCommunicationCount INT = 250000
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
	,@yearsBack INT = 4
	,@isbulk BIT
	,@communicationStatus INT = 0

   	,@smsNumbersDefinedTypeId int = (select top 1 Id from DefinedType where [Guid] = '611BDE1F-7405-4D16-8626-CCFEDB0E62BE')

declare
    @relatedSmsFromDefinedValueId int = (select top 1 Id from DefinedValue where DefinedTypeId = @smsNumbersDefinedTypeId)

DECLARE @daysBack INT = @yearsBack * 366

BEGIN
	--begin transaction
	/*
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
            SELECT TOP 1 @transportEntityTypeId = id
		    FROM EntityType
		    WHERE Name LIKE 'Rock.Communication.Transport.Twilio'
		    ORDER BY newid()
        end else begin
            SELECT TOP 1 @transportEntityTypeId = id
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
				)
			VALUES (
				@communicationId
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
				,NULL
				,NULL
				,NULL
				,@recipientPersonAliasId
                ,@mediumEntityTypeId
				)
		END
		ELSE
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
				)
			SELECT TOP 40 @communicationId
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
				,NULL
				,NULL
				,NULL
				,Id
			FROM PersonAlias
			ORDER BY newid()
		END

		DECLARE @messageKey NVARCHAR(11) = (
				SELECT TOP 1 CONCAT (
						isnull(CountryCode, 1)
						,Number
						)
				FROM PhoneNumber
				WHERE IsMessagingEnabled = 1
				ORDER BY newid()
				)

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

		SET @communicationCounter += 1;
		SET @communicationDateTime = DATEADD(ss, (86000 * @daysBack / @maxCommunicationCount), @communicationDateTime);
	END
			--commit transaction
END;