/* ====================================================== 
-- NewSpring Script #3: 
-- Inserts recurring giving from an Excel file -> SQL table
  
--  Assumptions:
--  People are imported on the destination
--  Cybersource Gateway is installed on the destination

   ====================================================== */

-- !!! Make sure you're using the right Rock database !!!

/*

How to import the XLS file:

1.) Create Imports DB.  If it already exists, drop it, then create a new one.
2.) Right click on the Imports DB and choose Tasks > Import Data
3.) Move through the wizard to Choose a Data Source and select Microsoft Excel
4.) Select the xls file with the browse button
5.) Click next
6.) Choose the destination, SQL Server Native Client 11.0
7.) Set the connection variables and credentials
8.) Select the Imports DB from the drop down list
9.) Click next
10.) Select the first option to copy all the data and click next
11.) Make sure the right source and destination are checked and click next
12.) Ensure Run immediately is checked and click finish
13.) Confirm that you are so really very sure and get out of the wizard
14.) Expand the Imports DB, right click tables and choose refresh
15.) You should see the new table in the expanded tables list
16.) Right click and rename the table to F1Schedules
17.) You can now run NS script 3
18.) Praise be

*/

/* ====================================================== */

-- Clean up data --

DECLARE @start AS DATETIME = '3-6-2016 23:00:00';
--DECLARE @end AS DATETIME = '3-8-2016 23:59:59';

DELETE FROM [Imports].[dbo].[F1Schedules]
WHERE
	[Created Date] < @start
	--OR [Created Date] > @end;

/* ====================================================== */

SET NOCOUNT ON;

DECLARE @foreignKey AS NVARCHAR(MAX) = 'FromF1PostLaunch-Schedule';
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
  [FinancialGateway]
WHERE
  [Name] = 'NMI Gateway';

IF @FinancialGatewayId IS NULL
BEGIN
  RAISERROR('Could not find NMI Gateway', 20, -1) WITH LOG;
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
	[DetailGuid] uniqueidentifier,
	PaymentDetailGuid uniqueidentifier
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
  , NEWID() AS [PaymentDetailGuid]
FROM 
  [Imports].[dbo].[F1Schedules] [f1s]
  LEFT JOIN [PersonAlias] [pa] ON [f1s].[Individual ID] = [pa].[ForeignId]
WHERE
  [pa].[Id] IS NOT NULL;

INSERT INTO [FinancialPaymentDetail] (
	CurrencyTypeValueId,
	CreditCardTypeValueId,
	[Guid],
	ForeignKey)
SELECT
	CurrencyTypeValueId,
	CreditCardTypeValueId,
	PaymentDetailGuid,
	@foreignKey
FROM #temp;

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
  AuthorizedPersonAliasId,
  FinancialGatewayId,
  FinancialPaymentDetailId,
  ForeignKey) 
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
  AuthorizedPersonAliasId,
  FinancialGatewayId,
  (SELECT Id FROM FinancialPaymentDetail WHERE [Guid] = PaymentDetailGuid),
  @foreignKey
FROM #temp;

INSERT INTO [FinancialScheduledTransactionDetail] (
  ScheduledTransactionId,
  AccountId,
  Amount,
  Guid,
  CreatedDateTime,
  ModifiedDateTime,
  CreatedByPersonAliasId,
  ForeignKey) 
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
  CreatedByPersonAliasId,
  @foreignKey
FROM #temp t;