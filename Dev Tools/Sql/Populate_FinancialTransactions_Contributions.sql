set nocount on

/* Change these settings to your liking*/
declare
    @yearsBack int = 1,
    @maxPersonCount INT = 40000, /* limit to first X persons in the database (Handy for testing Statement Generator) */ 
    @maxTransactionCount int = 100000, 
    @maxBatchNumber INT = 1000

declare
  @authorizedPersonAliasId int = 1,
  @transactionCounter int = 0,
  @maxPersonAliasIdForTransactions int = (select max(Id) from (select top (@maxPersonCount) Id from PersonAlias order by Id) x),
  @transactionDateTime datetime,
  @transactionAmount decimal(18,2),
  @transactionNote nvarchar(max),
  @transactionTypeValueId int = (select top 1 Id from DefinedValue where Guid = '2D607262-52D6-4724-910D-5C6E8FB89ACC'),
  @currencyTypeCash int = (select top 1 Id from DefinedValue where Guid = 'F3ADC889-1EE8-4EB6-B3FD-8C10F3C8AF93'),
  @currencyTypeCheck int = (select top 1 Id from DefinedValue where Guid = '8B086A19-405A-451F-8D44-174E92D6B402'),
  @currencyTypeCreditCard int = (select top 1 Id from DefinedValue where Guid = '928A2E04-C77B-4282-888F-EC549CEE026A'),
  @creditCardTypeVisa int = (select Id from DefinedValue where Guid = 'FC66B5F8-634F-4800-A60D-436964D27B64'),
  @sourceTypeDefinedTypeId int = (select top 1 Id from DefinedType where [Guid] = '4F02B41E-AB7D-4345-8A97-3904DDD89B01') --FINANCIAL_SOURCE_TYPE 

declare
  @sourceTypeValueId int = (select top 1 Id from DefinedValue where DefinedTypeId = @sourceTypeDefinedTypeId order by NEWID()), 
  @accountId int = (select top 1 id from FinancialAccount where IsTaxDeductible = 1),
  @batchId int,
  @batchStatusOpen int = 1,
  @transactionId int,
  @financialPaymentDetailId int,
  @checkMicrEncrypted nvarchar(max) = null,
  @checkMicrHash nvarchar(128) = null,
  @checkMicrParts nvarchar(max) = null,
  @currencyType int,
  @creditCardType int

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

IF CURSOR_STATUS('global','personAliasIdCursor')>=-1
BEGIN
    DEALLOCATE personAliasIdCursor;
END

-- put all personIds in randomly ordered cursor to speed up getting a random personAliasId for each attendance
declare personAliasIdCursor cursor LOCAL FAST_FORWARD for select top( @maxPersonCount ) Id from PersonAlias order by newid();
open personAliasIdCursor;

IF CURSOR_STATUS('global','batchIdCursor')>=-1
BEGIN
    DEALLOCATE batchIdCursor;
END

declare batchIdCursor cursor LOCAL FAST_FORWARD for select top( @maxBatchNumber ) Id from FinancialBatch order by newid();
open batchIdCursor;

begin transaction

/*
 truncate table FinancialTransactionDetail
 delete FinancialTransaction
*/

set @transactionDateTime = DATEADD(DAY, -@daysBack, SYSDATETIME())

while @transactionCounter < @maxTransactionCount
begin
    fetch next from personAliasIdCursor into @authorizedPersonAliasId;

    if (@@FETCH_STATUS != 0) begin
        close personAliasIdCursor;
        open personAliasIdCursor;
        fetch next from personAliasIdCursor into @authorizedPersonAliasId;
    end

    -- do a random number of transactions per person so that we don't have exactly the same number of transactions per person
    DECLARE @MaxTransactionCountPerPerson INT = ROUND(rand() * 20, 0);

    DECLARE @TransactionCountPerPerson INT = 0;

    while (@TransactionCountPerPerson < @MaxTransactionCountPerPerson AND @transactionCounter < @maxTransactionCount)
    begin
        set @transactionAmount = ROUND(rand() * 5000, 2);

        SET @TransactionCountPerPerson = @TransactionCountPerPerson + 1;

        if (@transactionCounter % 100 = 0 ) begin
          set @transactionNote = 'Random Note ' + convert(nvarchar(max), rand());
        end else begin
          set @transactionNote =  null
        end

        if (rand() * 20 = 1) begin
          -- have around 5% be checks
          set @currencyType = @currencyTypeCheck
          set @checkMicrHash = replace(cast(NEWID() as nvarchar(36)), '-', '') + replace(cast(NEWID() as nvarchar(36)), '-', '') + replace(cast(NEWID() as nvarchar(36)), '-', '');
          set @creditCardType = null
        end
        else if (rand() * 20 = 1) begin
          -- have around 5% be cash
          set @currencyType = @currencyTypeCash
          set @checkMicrHash = null
          set @creditCardType = null
        end
        else begin
          -- have around  80% be creditcard
          set @currencyType = @currencyTypeCreditCard
          set @checkMicrHash = null
          set @creditCardType = @creditCardTypeVisa
        end

        insert into FinancialPaymentDetail ( CurrencyTypeValueId, CreditCardTypeValueId, [Guid] ) values (@currencyType, @creditCardType, NEWID());
        set @financialPaymentDetailId = SCOPE_IDENTITY()

        if (@transactionCounter % 100 = 0 )
        begin
            set @sourceTypeValueId = (select top 1 Id from DefinedValue where DefinedTypeId = @sourceTypeDefinedTypeId order by NEWID())
        end

        if (@transactionCounter % 17 = 0 )
        begin
            set @accountId = (select top 1 id from FinancialAccount where IsTaxDeductible = 1 order by NEWID());
        end

        if (@transactionCounter % 50 = 0 )
        begin
            fetch next from batchIdCursor into @BatchId;
            if (@@FETCH_STATUS != 0) begin
                close batchIdCursor;
                open batchIdCursor;
                fetch next from batchIdCursor into @BatchId;
            end
        end

        INSERT INTO [dbo].[FinancialTransaction]
                    ([AuthorizedPersonAliasId]
                    ,[BatchId]
                    ,[TransactionDateTime]
                    ,[TransactionDateKey]
                    ,[SundayDate]
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
                    ,CONVERT(INT, (CONVERT(CHAR(8), @transactionDateTime, 112)))
                    ,dbo.ufnUtility_GetSundayDate(@transactionDateTime)
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
        if (@transactionCounter % 10000 = 0)
        begin
            commit transaction
            begin transaction
            print @transactionCounter
        end
    end
end

commit transaction

close personAliasIdCursor;
close batchIdCursor;

end;


select count(*) from FinancialTransaction