set nocount on

declare
  @recipientPersonAliasId int,
  @senderPersonAliasId int,
  @communicationCounter int = 0,
  @maxCommunicationCount int = 250000, 
  @maxPersonAliasIdForCommunications int = (select max(Id) from (select top 4000 Id from PersonAlias order by Id) x),  /* limit to first 4000 persons in the database */ 
  @communicationDateTime datetime,
  @futureSendDateTime datetime,
  @communicationSubject nvarchar(max),
  @communicationId int,
  @mediumId int,
  @yearsBack int = 4,
  @isbulk bit,
  @communicationStatus int = 0

declare
  @daysBack int = @yearsBack * 366


begin

begin transaction

/*
 delete [CommunicationRecipient]
 delete [Communication]
*/

set @communicationDateTime = DATEADD(DAY, -@daysBack, SYSDATETIME())

while @communicationCounter < @maxCommunicationCount
    begin
        set @communicationSubject = 'Random Subject ' + convert(nvarchar(max), rand());
        set @recipientPersonAliasId =  (select top 1 Id from PersonAlias where Id <= rand() * @maxPersonAliasIdForCommunications order by Id desc);
        while @recipientPersonAliasId is null
        begin
          -- Try again just in case we didn't get anybody
          set @recipientPersonAliasId =  (select top 1 Id from PersonAlias where Id <= rand() * @maxPersonAliasIdForCommunications order by Id desc);
        end

		set @senderPersonAliasId =  (select top 1 Id from PersonAlias where Id <= rand() * @maxPersonAliasIdForCommunications order by Id desc);
        while @senderPersonAliasId is null
        begin
          -- Try again just in case we didn't get anybody
          set @senderPersonAliasId =  (select top 1 Id from PersonAlias where Id <= rand() * @maxPersonAliasIdForCommunications order by Id desc);
        end

		set @futureSendDateTime = DATEADD(DAY, round(50*rand(), 0)-1, @communicationDateTime)
		if (@futureSendDateTime = @communicationDateTime) begin
			set @futureSendDateTime = null
		end

		set @communicationStatus = ROUND(5 * RAND(), 0) 
		select top 1 @mediumId = id from EntityType where Name like 'Rock.Communication.Medium%' order by newid()

		-- generate a bulk about 1/20th of the time
		select @isbulk = case CAST(ROUND(RAND()*20,0) AS BIT) when 1 then 0 else 1 end

        INSERT INTO [dbo].[Communication]
           ([Subject]
           ,[FutureSendDateTime]
           ,[Status]
           ,[ReviewedDateTime]
           ,[ReviewerNote]
           ,[MediumEntityTypeId]
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
           ,[MediumDataJson])
		VALUES
           (@communicationSubject
           ,@futureSendDateTime
           ,@communicationStatus
           ,DATEADD(DAY, round(50*rand(), 0), @communicationDateTime)
           ,'some note'
           ,@mediumId
           ,''
           ,newid()
           ,@communicationDateTime
           ,@communicationDateTime
           ,null
           ,null
           ,null
           ,@isbulk
           ,@senderPersonAliasId
           ,null
           ,null)

		   set @communicationId = SCOPE_IDENTITY()

		if (@isbulk = 0) begin
			INSERT INTO [dbo].[CommunicationRecipient]
				   ([CommunicationId]
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
				   ,[PersonAliasId])
			 VALUES
				   (@communicationId
				   ,ROUND(5 * RAND(), 0) 
				   ,null
				   ,null
				   ,newid()
				   ,@communicationDateTime
				   ,@communicationDateTime
				   ,null
				   ,null
				   ,DATEADD(day, 5, @communicationDateTime)
				   ,null
				   ,null
				   ,null
				   ,null
				   ,null
				   ,@recipientPersonAliasId)
		end else begin
			INSERT INTO [dbo].[CommunicationRecipient]
				   ([CommunicationId]
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
				   ,[PersonAliasId])
			 select top 40
				   @communicationId
				   ,ROUND(5 * RAND(), 0) 
				   ,null
				   ,null
				   ,newid()
				   ,@communicationDateTime
				   ,@communicationDateTime
				   ,null
				   ,null
				   ,DATEADD(day, 5, @communicationDateTime)
				   ,null
				   ,null
				   ,null
				   ,null
				   ,null
				   ,Id from PersonAlias order by newid()
		end

        set @communicationCounter += 1;
        set @communicationDateTime = DATEADD(ss, (86000*@daysBack/@maxCommunicationCount), @communicationDateTime);
    end

commit transaction

end;


