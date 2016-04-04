/* ====================================================== 
-- NewSpring Script #16: 
-- Inserts transactions from an Excel file -> SQL table
  
--  Assumptions:
--  People are imported on the destination

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
16.) Right click and rename the table to F1Transactions
17.) You can now run NS script 16
18.) Praise be

*/

-- Combine time with date column
UPDATE [Imports].[dbo].[F1Transactions]
SET Received_Date = Received_Date + CONVERT(DATETIME, CONVERT(TIME, Received_Time))
WHERE Received_Time IS NOT NULL;

/* ====================================================== */

-- Clean up data --

DECLARE @start AS DATETIME = '3-14-2016 00:00:00';
DECLARE @end AS DATETIME = '3-14-2016 23:59:59';

DELETE FROM [Imports].[dbo].[F1Transactions]
WHERE
	--Received_Time IS NULL
	Received_Date < @start
	OR Received_Date > @end;

/* ====================================================== */


DECLARE @foreignKey AS NVARCHAR(MAX) = 'FromF1PostLaunch';
DECLARE @createdDate AS DATE = GETDATE();
DECLARE @contribution AS INT = (SELECT dv.Id FROM DefinedValue dv JOIN DefinedType dt ON dt.Id = dv.DefinedTypeId WHERE dt.Name='Transaction Type' AND dv.Value = 'Contribution');
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

-- Supplement Bank Card Type from Reference Col
UPDATE f1t 
SET
	[Bank Card_Type] =
		CASE WHEN Reference = 'amex' THEN
			'American Express'
		WHEN Reference = 'discover' THEN 
			'Discover'
		WHEN Reference = 'mc' THEN 
			'MasterCard'
		WHEN Reference = 'visa' THEN 
			'Visa'
		END
FROM [Imports].[dbo].[F1Transactions] f1t
WHERE [Bank Card_Type] IS NULL AND Reference IN ('amex', 'mc', 'visa', 'discover');

-- Create temp table
if object_id('tempdb..#temp') is not null
begin
	drop table #temp;
end

create table #temp (
	ParentAccountId int,
	CampusId int,
	TransactionDateTime datetime,
	CurrencyTypeValueId int,
	CreditCardTypeValueId int,
	AuthorizedPersonAliasId int,
	FinancialGatewayId int,
	Amount decimal(18, 2),
	[TransactionGuid] uniqueidentifier,
	[DetailGuid] uniqueidentifier,
	PaymentDetailGuid uniqueidentifier,
	LastFour NVARCHAR(MAX),
	Reference NVARCHAR(MAX)
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
	  AND [Name] = [f1t].[Fund]
  ) AS [ParentAccountId]
  , (
      SELECT
        [Id]
      FROM
        [Campus]
      WHERE
	    [Name] = REPLACE([f1t].[SubFund], ' Campus', '')
    ) AS [CampusId]
  , Received_Date AS TransactionDateTime
  , (
	  SELECT 
	    [Id] 
	  FROM 
	    [DefinedValue] 
	  WHERE
	   [DefinedTypeId] = @CurrencyTypeId
	   AND (
	     ([f1t].[Bank Card_Type] = 'eCheck' AND [Value] = 'ACH')
		 OR ([f1t].[Bank Card_Type] <> 'eCheck' AND [Value] = 'Credit Card')
	   )
	) AS [CurrencyTypeValueId]
	, (
		  SELECT 
			[Id] 
		  FROM 
			[DefinedValue] 
		  WHERE
		   [DefinedTypeId] = @CreditCardTypeId
		   AND [f1t].[Bank Card_Type] <> 'eCheck'
		   AND [f1t].[Bank Card_Type] = [Value]
		) AS [CreditCardTypeValueId]
  , CASE WHEN [pa].[Id] IS NULL THEN
		(
			SELECT TOP 1
				pa2.Id
			FROM
				[Group] g
				JOIN Person p ON p.GivingGroupId = g.Id AND p.Id = p.GivingLeaderId
				JOIN PersonAlias pa2 ON pa2.PersonId = p.Id
			WHERE
				g.ForeignId = f1t.[Giving Unit_ID]
		)
	ELSE
		pa.Id
	END AS [AuthorizedPersonAliasId]
  , @FinancialGatewayId AS [FinancialGatewayId]
  , [f1t].[Amount]
  , NEWID() AS [TransactionGuid]
  , NEWID() AS [DetailGuid]
  , NEWID() AS [PaymentDetailGuid]
  , f1t.[Bank Card # Last 4]
  , f1t.Reference
FROM 
  [Imports].[dbo].[F1Transactions] [f1t]
  LEFT JOIN [PersonAlias] [pa] ON [f1t].[Contributor_ID] = [pa].[ForeignId];

DELETE FROM #Temp WHERE AuthorizedPersonAliasId IS NULL;

INSERT INTO [FinancialPaymentDetail] (
	CurrencyTypeValueId,
	CreditCardTypeValueId,
	[Guid],
	CreatedDateTime,
	AccountNumberMasked,
	ForeignKey)
SELECT
	CurrencyTypeValueId,
	CreditCardTypeValueId,
	PaymentDetailGuid,
	@createdDate,
	CASE WHEN LastFour IS NULL THEN NULL ELSE CONCAT('***', LastFour) END,
	@foreignKey
FROM #temp;

INSERT INTO [FinancialTransaction] (
	[Guid],
	TransactionDateTime,
	AuthorizedPersonAliasId,
	FinancialGatewayId,
	FinancialPaymentDetailId,
	TransactionTypeValueId,
	CreatedDateTime,
	ForeignKey,
	SourceTypeValueId) 
SELECT
	TransactionGuid,
	TransactionDateTime,
	AuthorizedPersonAliasId,
	@FinancialGatewayId,
	(SELECT Id FROM FinancialPaymentDetail WHERE [Guid] = PaymentDetailGuid),
	@contribution,
	@createdDate,
	@foreignKey,
	10
FROM #temp;

INSERT INTO [FinancialTransactionDetail] (
  TransactionId,
  AccountId,
  Amount,
  Guid,
  CreatedDateTime,
  ForeignKey)
SELECT
  (SELECT Id FROM [FinancialTransaction] WHERE [Guid] = t.[TransactionGuid]),
  a.Id,
  Amount,
  DetailGuid,
  @createdDate,
  @foreignKey
FROM 
	#temp t
	JOIN FinancialAccount a ON a.ParentAccountId = t.ParentAccountId AND a.CampusId = t.CampusId;
