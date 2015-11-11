/* ====================================================== 
-- NewSpring Script #3: 
-- Inserts recurring giving from an Excel file -> SQL table
  
--  Assumptions:
--  People are imported on the destination
--  Cybersource Gateway is installed on the destination

   ====================================================== */
-- Make sure you're using the right Rock database:

USE Rock

/* ====================================================== */

SET NOCOUNT ON;

DECLARE @IsActive bit = 0;

-- Check the credit card type
DECLARE @CreditCardTypeId int;

SELECT
  @CreditCardTypeId = [Id]
FROM
  [DefinedType]
WHERE
  [Name] = 'Credit Card Type';

IF @CreditCardTypeId IS NULL
BEGIN
  RAISERROR('Could not find credit card type', 20, -1) WITH LOG;
END

-- Check the currency type
DECLARE @CurrencyTypeId int;

SELECT
  @CurrencyTypeId = [Id]
FROM
  [DefinedType]
WHERE
  [Name] = 'Tender Type';

IF @CurrencyTypeId IS NULL
BEGIN
  RAISERROR('Could not find currency type', 20, -1) WITH LOG;
END

-- Check the recurring defined type
DECLARE @RecurringFrequencyTypeId int;

SELECT
  @RecurringFrequencyTypeId = [Id]
FROM
  [DefinedType]
WHERE
  [Name] = 'Recurring Transaction Frequency';

IF @RecurringFrequencyTypeId IS NULL
BEGIN
  RAISERROR('Could not find recurring frequency type', 20, -1) WITH LOG;
END

-- Get the cyber source gateway id
DECLARE @FinancialGatewayId int;

SELECT 
  @FinancialGatewayId = [Id]
FROM 
  [beta].[dbo].[FinancialGateway]
WHERE
  [Name] = 'Cyber Source';

IF @FinancialGatewayId IS NULL
BEGIN
  RAISERROR('Could not find Cyber Source', 20, -1) WITH LOG;
END

-- Create temp table
if object_id('tempdb..#temp') is not null
begin
	drop table #temp;
end

create table #temp (
	ParentAccountId int,
	CampusId int,
	TransactionFrequencyValueId int,
	StartDate date,
	NumberOfPayments int,
	NextPaymentDate date,
	LastStatusUpdateDateTime datetime,
	CardReminderDate date,
	CreatedDateTime datetime,
	ModifiedDateTime datetime,
	CreatedByPersonAliasId int,
	CurrencyTypeValueId int,
	CreditCardTypeValueId int,
	AuthorizedPersonAliasId int,
	FinancialGatewayId int,
	Amount decimal(18, 2),
	[ScheduleGuid] uniqueidentifier,
	[DetailGuid] uniqueidentifier
);

-- Map the payments
INSERT INTO #temp
SELECT
  (
    SELECT
	  [Id]
    FROM 
	  [FinancialAccount]
    WHERE 
	  [ParentAccountId] IS NULL 
	  AND [Name] = [f1s].[Fund]
  ) AS [ParentAccountId]
  , (
      SELECT
        [Id]
      FROM
        [Campus]
      WHERE
	    [Name] = REPLACE([f1s].[Sub Fund], ' Campus', '')
    ) AS [CampusId]
  , (
	  SELECT 
	    [Id] 
	  FROM 
	    [DefinedValue] 
	  WHERE
	   [DefinedTypeId] = @RecurringFrequencyTypeId
	   AND (
	     [f1s].[Gift Frequency] = [Value]
		 OR [f1s].[Gift Frequency] = [Description]
	     OR ([f1s].[Gift Frequency] = 'Twice Monthly - 1st & 16th' AND [Value] = 'Twice a Month')
		 OR ([f1s].[Gift Frequency] = 'Monthly - Last Day of Month' AND [Value] = 'Monthly')
	   )
	) AS [TransactionFrequencyValueId]
  , CAST([f1s].[Start Date] AS date) AS [StartDate]
  , CASE WHEN [f1s].[Payments Remaining] = 999 THEN NULL ELSE [f1s].[Payments Remaining] END AS [NumberOfPayments]
  , CAST([f1s].[Next Payment Date] AS date) AS [NextPaymentDate]
  , CAST([f1s].[Last Update Date] AS datetime) AS [LastStatusUpdateDateTime]
  , CAST([f1s].[Credit Card Expiration Date] AS date) AS [CardReminderDate]
  , CAST([f1s].[Created Date] AS datetime) AS [CreatedDateTime]
  , CAST([f1s].[Last Update Date] AS datetime) AS [ModifiedDateTime]
  , [pa].[Id] AS [CreatedByPersonAliasId]
  , (
	  SELECT 
	    [Id] 
	  FROM 
	    [DefinedValue] 
	  WHERE
	   [DefinedTypeId] = @CurrencyTypeId
	   AND (
	     ([f1s].[Payment Type] = 'eCheck' AND [Value] = 'ACH')
		 OR ([f1s].[Payment Type] <> 'eCheck' AND [Value] = 'Credit Card')
	   )
	) AS [CurrencyTypeValueId]
  , (
	  SELECT 
	    [Id] 
	  FROM 
	    [DefinedValue] 
	  WHERE
	   [DefinedTypeId] = @CreditCardTypeId
	   AND [f1s].[Payment Type] <> 'eCheck'
	   AND [f1s].[Payment Type] = [Value]
	) AS [CreditCardTypeValueId]
  , [pa].[Id] AS [AuthorizedPersonAliasId]
  , @FinancialGatewayId AS [FinancialGatewayId]
  , [f1s].[Amount]
  , NEWID() AS [ScheduleGuid]
  , NEWID() AS [DetailGuid]
FROM 
  [RecurringGiving].[dbo].[F1Schedules] [f1s]
  LEFT JOIN [PersonAlias] [pa] ON [f1s].[Individual ID] = [pa].[ForeignId]
WHERE
  [pa].[Id] IS NOT NULL;

INSERT INTO [FinancialScheduledTransaction] (
  TransactionFrequencyValueId,
  StartDate,
  NumberOfPayments,
  NextPaymentDate,
  LastStatusUpdateDateTime,
  IsActive,
  CardReminderDate,
  Guid,
  CreatedDateTime,
  ModifiedDateTime,
  CreatedByPersonAliasId,
  CurrencyTypeValueId,
  CreditCardTypeValueId,
  AuthorizedPersonAliasId,
  FinancialGatewayId) 
SELECT
  TransactionFrequencyValueId,
  StartDate,
  NumberOfPayments,
  NextPaymentDate,
  LastStatusUpdateDateTime,
  @IsActive,
  CardReminderDate,
  ScheduleGuid,
  CreatedDateTime,
  ModifiedDateTime,
  CreatedByPersonAliasId,
  CurrencyTypeValueId,
  CreditCardTypeValueId,
  AuthorizedPersonAliasId,
  FinancialGatewayId
FROM #temp;

INSERT INTO [FinancialScheduledTransactionDetail] (
  ScheduledTransactionId,
  AccountId,
  Amount,
  Guid,
  CreatedDateTime,
  ModifiedDateTime,
  CreatedByPersonAliasId) 
SELECT
  (SELECT Id FROM [FinancialScheduledTransaction] WHERE [Guid] = t.[ScheduleGuid]),
  CASE WHEN t.[CampusId] IS NULL 
    THEN (SELECT Id FROM [FinancialAccount] WHERE [CampusId] IS NULL AND [ParentAccountId] = t.[ParentAccountId] AND [Name] LIKE 'Web %' )
    ELSE (SELECT Id FROM [FinancialAccount] WHERE [CampusId] IS NOT NULL AND [ParentAccountId] = t.[ParentAccountId] AND [CampusId] = t.[CampusId]) 
  END,
  Amount,
  DetailGuid,
  CreatedDateTime,
  ModifiedDateTime,
  CreatedByPersonAliasId
FROM #temp t;

SELECT * 
FROM [FinancialScheduledTransactionDetail] d 
JOIN [FinancialScheduledTransaction] t ON t.Id = d.ScheduledTransactionId;

USE [master];
