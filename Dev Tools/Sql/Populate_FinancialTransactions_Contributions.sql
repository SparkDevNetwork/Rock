set nocount on

/* Change these settings to your liking*/
declare
	@yearsBack int = 10,
	@maxPersonCount INT = 40000, /* limit to first X persons in the database */ 
	@maxTransactionCount int = 200000, 
	@maxBatchNumber INT = 1000

declare
  @authorizedPersonAliasId int = 1,
  @transactionCounter int = 0,
  @maxPersonAliasIdForTransactions int = (select max(Id) from (select top (@maxPersonCount) Id from PersonAlias order by Id) x),
  @transactionDateTime datetime,
  @transactionAmount decimal(18,2),
  @transactionNote nvarchar(max),
  @transactionTypeValueId int = (select Id from DefinedValue where Guid = '2D607262-52D6-4724-910D-5C6E8FB89ACC'),
  @currencyTypeCash int = (select Id from DefinedValue where Guid = 'F3ADC889-1EE8-4EB6-B3FD-8C10F3C8AF93'),
  @creditCardTypeVisa int = (select Id from DefinedValue where Guid = 'FC66B5F8-634F-4800-A60D-436964D27B64'),
  @sourceTypeValueId int, 
  @sourceTypeDefinedTypeId int = (select top 1 Id from DefinedType where [Guid] = '4F02B41E-AB7D-4345-8A97-3904DDD89B01'), --FINANCIAL_SOURCE_TYPE 
  @accountId int,
  @batchId int,
  @batchStatusOpen int = 1,
  @transactionId int,
  @financialPaymentDetailId int,
  @checkMicrEncrypted nvarchar(max) = null,
  @checkMicrHash nvarchar(128) = null,
  @checkMicrParts nvarchar(max) = null

declare
  @daysBack int = @yearsBack * 366

DECLARE 
	@BatchNumber INT = 1
	,@BatchName NVARCHAR(max)
	,@BatchDate DATETIME = sysdatetime()

begin

-- create a bunch of batches
WHILE @BatchNumber < @MaxBatchNumber
BEGIN
	SET @BatchName = CONCAT (
			'Batch '
			,@BatchNumber
			);

	if not exists(select * from FinancialBatch where Name = @BatchName)
	begin
	INSERT INTO [dbo].[FinancialBatch] (
		[Name]
		,[BatchStartDateTime]
		,[Status]
		,[ControlAmount]
		,[Guid]
		)
	VALUES (
		@BatchName
		,@BatchDate
		,1
		,0.00
		,newid()
		)
     end
	SET @BatchNumber = @BatchNumber + 1;
	SET @BatchDate = DATEADD(day, - 1, @BatchDate)
END


begin transaction

/*
 truncate table FinancialTransactionDetail
 delete FinancialTransaction
*/

set @transactionDateTime = DATEADD(DAY, -@daysBack, SYSDATETIME())
declare 
  @MaxBatchId int = (select max(Id) from FinancialBatch),
  @MaxAccountId int = (select max(Id) from FinancialAccount)

while @transactionCounter < @maxTransactionCount
    begin
        set @transactionAmount = ROUND(rand() * 5000, 2);
        set @transactionNote = 'Random Note ' + convert(nvarchar(max), rand());
        set @authorizedPersonAliasId =  (select top 1 Id from PersonAlias where Id <= rand() * @maxPersonAliasIdForTransactions order by Id desc);
        while @authorizedPersonAliasId is null
        begin
          -- Try again just in case we didn't get anybody
          set @authorizedPersonAliasId =  (select top 1 Id from PersonAlias where Id <= rand() * @maxPersonAliasIdForTransactions order by Id desc);
        end

        set @sourceTypeValueId = (select top 1 Id from DefinedValue where DefinedTypeId = @sourceTypeDefinedTypeId order by NEWID())

        --set @checkMicrEncrypted = replace(cast(NEWID() as nvarchar(36)), '-', '') + replace(cast(NEWID() as nvarchar(36)), '-', '');
        set @checkMicrHash = replace(cast(NEWID() as nvarchar(36)), '-', '') + replace(cast(NEWID() as nvarchar(36)), '-', '') + replace(cast(NEWID() as nvarchar(36)), '-', '');

		insert into FinancialPaymentDetail ( CurrencyTypeValueId, CreditCardTypeValueId, [Guid] ) values (@currencyTypeCash, @creditCardTypeVisa, NEWID());
		set @financialPaymentDetailId = SCOPE_IDENTITY()

	   set @BatchId = null
	   while @BatchId is null
	   begin
		  set @BatchId = (select top 1 id from FinancialBatch where Id = round(rand()*@MaxBatchId, 0))
	   end

        INSERT INTO [dbo].[FinancialTransaction]
                   ([AuthorizedPersonAliasId]
                   ,[BatchId]
                   ,[TransactionDateTime]
                   ,[TransactionCode]
                   ,[Summary]
                   ,[TransactionTypeValueId]
				   ,[FinancialPaymentDetailId]
                   ,[SourceTypeValueId]
                   ,[CheckMicrEncrypted]
                   ,[CheckMicrHash]
				   ,[CheckMicrParts]
                   ,[Guid])
             VALUES
                   (@authorizedPersonAliasId
                   ,@batchId
                   ,@transactionDateTime
                   ,null
                   ,@transactionNote
                   ,@transactionTypeValueId
				   ,@financialPaymentDetailId
                   ,@sourceTypeValueId
                   ,@checkMicrEncrypted
                   ,@checkMicrHash
				   ,@checkMicrParts
                   ,NEWID()
        )
        set @transactionId = SCOPE_IDENTITY()

        set @accountId = null
	   while @accountId is null
	   begin
		  set @accountId = (select top 1 id from FinancialAccount where Id = round(rand()*@MaxAccountId, 0))
	   end
 
        -- For contributions, we just need to put in the AccountId (entitytype/entityid would be null)
        INSERT INTO [dbo].[FinancialTransactionDetail]
                   ([TransactionId]
                   ,[AccountId]
                   ,[Amount]
                   ,[Summary]
                   ,[EntityTypeId]
                   ,[EntityId]
                   ,[Guid])
             VALUES
                   (@transactionId
                   ,@accountId
                   ,@transactionAmount
                   ,null
                   ,null
                   ,null
                   ,NEWID())

        set @transactionCounter += 1;
        set @transactionDateTime = DATEADD(ss, (86000*@daysBack/@maxTransactionCount), @transactionDateTime);
		if (@transactionCounter % 1000 = 0)
		begin
			print @transactionCounter
		end
    end

commit transaction

end;


