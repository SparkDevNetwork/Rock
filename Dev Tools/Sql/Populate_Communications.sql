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
	,@maxCommunicationCount INT = 250000

    -- it does a batch communications around 5% of the time, and then a random number of recipients (up to 500) for each batch communication
    ,@maxCommunicationRecipientBatchCount INT = 500
    ,@personSampleSize int = 10000 -- number of people to use when randomly assigning a person to each attendance. You might want to set this lower or higher depending on what type of data you want
	,@communicationDateTime DATETIME
	,@futureSendDateTime DATETIME
	,@communicationSubject NVARCHAR(max)
	,@communicationId INT
	,@mediumEntityTypeId INT
    ,@isSmsEntityTypeId int
    ,@transportEntityTypeId int
    ,@transportEntityTypeName nvarchar(max)
	,@yearsBack INT = 4
	,@isbulk BIT
	,@communicationStatus INT = 0
	,@isCommunicationTemplate bit
	,@CommunicationTemplateId INT
	,@SystemCommunicationId INT

   	,@smsNumbersDefinedTypeId int = (select top 1 Id from DefinedType where [Guid] = '611BDE1F-7405-4D16-8626-CCFEDB0E62BE')

declare
    @relatedSmsFromDefinedValueId int = (select top 1 Id from DefinedValue where DefinedTypeId = @smsNumbersDefinedTypeId)

DECLARE @daysBack INT = @yearsBack * 366

DECLARE @CommunicationMediumEntities AS TABLE (Id int PRIMARY KEY, isSms bit);
DECLARE @CommunicationTransportEntities AS TABLE (Id int PRIMARY KEY, [Name] nvarchar(100));
DECLARE @twilioEntityTypeId int = (select top 1 Id from EntityType where Name like 'Rock.Communication.Transport.Twilio');

BEGIN

  	insert into @CommunicationMediumEntities 
  		SELECT Id, case when [Name] like '%.sms' then 1 else 0 end
		    FROM EntityType
		    WHERE Name LIKE 'Rock.Communication.Medium%'
   
	insert into @CommunicationTransportEntities 
		SELECT Id, [Name]
		    FROM EntityType
		    WHERE Name LIKE 'Rock.Communication.Transport%'
			and Id != @twilioEntityTypeId


/*
 select count(*) from Communication
 select count(*) from CommunicationRecipient
 delete [CommunicationRecipient]
 delete [Communication]
*/

    IF CURSOR_STATUS('global','senderPersonAliasIdCursor')>=-1
    BEGIN
     DEALLOCATE senderPersonAliasIdCursor;
    END

    -- put all personIds in randomly ordered cursor to speed up getting a random personAliasId for each communication
	declare senderPersonAliasIdCursor cursor LOCAL FAST_FORWARD for 
        select top (@personSampleSize) Id from PersonAlias pa where pa.PersonId 
        not in (select Id from Person where (IsDeceased = 1  and RecordStatusValueId != 3)) order by newid();
	open senderPersonAliasIdCursor;

    IF CURSOR_STATUS('global','recipientPersonAliasIdCursor')>=-1
    BEGIN
     DEALLOCATE recipientPersonAliasIdCursor;
    END

    -- put all personIds in randomly ordered cursor to speed up getting a random personAliasId for each communication
	declare recipientPersonAliasIdCursor cursor LOCAL FAST_FORWARD for 
        select top (@personSampleSize) Id from PersonAlias pa where pa.PersonId 
        not in (select Id from Person where (IsDeceased = 1  and RecordStatusValueId != 3)) order by newid();
	open recipientPersonAliasIdCursor;
	
    
    SET @communicationDateTime = DATEADD(DAY, - @daysBack, SYSDATETIME())

	WHILE @communicationCounter < @maxCommunicationCount
	BEGIN
		fetch next from recipientPersonAliasIdCursor into @recipientPersonAliasId;

		if (@@FETCH_STATUS != 0) begin
		   close recipientPersonAliasIdCursor;
		   open recipientPersonAliasIdCursor;
		   fetch next from recipientPersonAliasIdCursor into @recipientPersonAliasId;
		end

        fetch next from senderPersonAliasIdCursor into @senderPersonAliasId;

		if (@@FETCH_STATUS != 0) begin
		   close senderPersonAliasIdCursor;
		   open senderPersonAliasIdCursor;
		   fetch next from senderPersonAliasIdCursor into @senderPersonAliasId;
		end
        
        
        SET @communicationSubject = 'Random Subject ' + convert(NVARCHAR(max), rand());

		SET @futureSendDateTime = DATEADD(DAY, round(50 * rand(), 0) - 1, @communicationDateTime)

		IF (@futureSendDateTime = @communicationDateTime)
		BEGIN
			SET @futureSendDateTime = NULL
		END

		SET @communicationStatus = ROUND(5 * RAND(), 0)

		SELECT TOP 1 @mediumEntityTypeId = id, @isSmsEntityTypeId = isSms
		FROM @CommunicationMediumEntities
		ORDER BY newid()

        if (@isSmsEntityTypeId = 1) begin
		    set @transportEntityTypeId = @twilioEntityTypeId
			set @transportEntityTypeName = 'Rock.Communication.Transport.Twilio'
        end else begin
            SELECT TOP 1 @transportEntityTypeId = [Id], @transportEntityTypeName = [Name]
		    FROM @CommunicationTransportEntities
		    ORDER BY newid()
        end

		-- have it be from a SystemCommunication a about 1/10th of the time
		SELECT @isCommunicationTemplate = CAST(ROUND(RAND() * 10, 0) AS BIT);
		if (@isCommunicationTemplate = 1)  BEGIN
			select @CommunicationTemplateId = (select top 1 Id from CommunicationTemplate order by newid());	
			set @SystemCommunicationId = null;
		end else BEGIN
			select @SystemCommunicationId = (select top 1 Id from SystemCommunication order by newid());
			set @CommunicationTemplateId = null;
		end

		-- generate a bulk about 1/100th of the time
		SELECT @isbulk = CASE CAST(ROUND(RAND() * 100, 0) AS BIT)
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
			,[CommunicationTemplateId]
			,[SystemCommunicationId]
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
			,@CommunicationTemplateId
			,@SystemCommunicationId
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
        
        if @isSmsEntityTypeId = 1 begin
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

        if (@communicationCounter % 1000 = 0)
		begin
  		  print @communicationCounter  
        end

        SET @communicationCounter += 1;
        
		SET @communicationDateTime = DATEADD(ss, (86000 * @daysBack / @maxCommunicationCount), @communicationDateTime);
	END

    close senderPersonAliasIdCursor;
    close recipientPersonAliasIdCursor;
    
END;

select count(*) [Total Communications] from Communication 
select count(*) [CommunicationsFromTemplate] from Communication where CommunicationTemplateId is not null
select count(*) [CommunicationsFromSystemCommunication] from Communication where SystemCommunicationId is not null
select count(*) [Total Communication Recipients] from CommunicationRecipient

