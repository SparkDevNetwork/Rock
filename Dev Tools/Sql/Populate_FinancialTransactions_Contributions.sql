set nocount on

declare
  @authorizedPersonAliasId int = 1,
  @transactionCounter int = 0,
  @maxTransactionCount int = 200000, 
  @maxPersonAliasIdForTransactions int = (select max(Id) from (select top 4000 Id from PersonAlias order by Id) x),  /* limit to first 4000 persons in the database */ 
  @transactionDateTime datetime,
  @transactionAmount decimal(18,2),
  @transactionNote nvarchar(max),
  @transactionTypeValueId int = (select Id from DefinedValue where Guid = '2D607262-52D6-4724-910D-5C6E8FB89ACC'),
  @currencyTypeCash int = (select Id from DefinedValue where Guid = 'F3ADC889-1EE8-4EB6-B3FD-8C10F3C8AF93'),
  @creditCardTypeVisa int = (select Id from DefinedValue where Guid = 'FC66B5F8-634F-4800-A60D-436964D27B64'),
  @sourceTypeWeb int = (select Id from DefinedValue where Guid = '7D705CE7-7B11-4342-A58E-53617C5B4E69'),
  @accountId int,
  @transactionId int,
  @checkMicrEncrypted nvarchar(max),
  @checkMicrHash nvarchar(128),
  @yearsBack int = 4

declare
  @daysBack int = @yearsBack * 366


begin

begin transaction

/*
 truncate table FinancialTransactionDetail
 delete FinancialTransaction
*/

set @transactionDateTime = DATEADD(DAY, -@daysBack, SYSDATETIME())

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

        set @checkMicrEncrypted = replace(cast(NEWID() as nvarchar(36)), '-', '') + replace(cast(NEWID() as nvarchar(36)), '-', '');
        set @checkMicrHash = replace(cast(NEWID() as nvarchar(36)), '-', '') + replace(cast(NEWID() as nvarchar(36)), '-', '') + replace(cast(NEWID() as nvarchar(36)), '-', '');

        INSERT INTO [dbo].[FinancialTransaction]
                   ([AuthorizedPersonAliasId]
                   ,[BatchId]
                   ,[TransactionDateTime]
                   ,[TransactionCode]
                   ,[Summary]
                   ,[TransactionTypeValueId]
                   ,[CurrencyTypeValueId]
                   ,[CreditCardTypeValueId]
                   ,[SourceTypeValueId]
                   ,[CheckMicrEncrypted]
                   ,[CheckMicrHash]
                   ,[Guid])
             VALUES
                   (@authorizedPersonAliasId
                   ,null
                   ,@transactionDateTime
                   ,null
                   ,@transactionNote
                   ,@transactionTypeValueId
                   ,@currencyTypeCash
                   ,@creditCardTypeVisa
                   ,@sourceTypeWeb
                   ,@checkMicrEncrypted
                   ,@checkMicrHash
                   ,NEWID()
        )
        set @transactionId = SCOPE_IDENTITY()
        set @accountId = (select top 1 id from FinancialAccount where Id = round(RAND() * 2, 0) + 1)
 
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
    end

commit transaction

end;


