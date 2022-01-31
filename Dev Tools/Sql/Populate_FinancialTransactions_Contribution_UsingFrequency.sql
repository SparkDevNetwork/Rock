/* Adjusts the TransactionDateTime in FinancialTransactions to follow the specified frequency pattern  */

SET NOCOUNT ON

/* 14 - Update Transactions a Bi-Weekly patter*/
/* 7 - Update Transactions a Weekly pattern */
/* 30 - Magic number to create a monthly pattern */
DECLARE @frequencyDays INT = 14;


DECLARE @transactionDateTime DATETIME = SYSDATETIME();
DECLARE @transactionTypeId INT = (
        SELECT TOP 1 Id
        FROM DefinedValue
        WHERE [Guid] = '2D607262-52D6-4724-910D-5C6E8FB89ACC'
        )
DECLARE @givingId NVARCHAR(50)
    , @financialTransactionId INT
    , @previousGivingId NVARCHAR(50) = N'0';
DECLARE @progress INT = 0;


-- Sort the transactions by GivingId, then FinancialTransactionId
-- We'll re-set the TransactionDateTime to today every time we get to a new GivingId
DECLARE financialTransaction_cursor CURSOR FAST_FORWARD
FOR
SELECT ft.Id, p.GivingId
FROM FinancialTransaction ft
JOIN PersonAlias pa on ft.AuthorizedPersonAliasId = pa.Id
JOIN Person p on pa.PersonId = p.Id
WHERE ft.AuthorizedPersonAliasId IS NOT NULL
AND TransactionTypeValueId = @transactionTypeId
ORDER BY p.GivingId ASC, ft.Id DESC

OPEN financialTransaction_cursor

FETCH NEXT
FROM financialTransaction_cursor
INTO @financialTransactionId, @givingId

WHILE @@FETCH_STATUS = 0
BEGIN

    SET @progress = @progress + 1

    IF (@progress % 1000 = 0)
    BEGIN
        PRINT @progress
    END
    
    IF (@givingId != @previousGivingId) BEGIN
        SET @transactionDateTime = SYSDATETIME();
        set @previousGivingId = @givingId; 
    END
    
    UPDATE FinancialTransaction
    SET TransactionDateTime = @transactionDateTime
        , TransactionDateKey = CONVERT(INT, (CONVERT(CHAR(8), @transactionDateTime, 112)))
        , SundayDate = dbo.ufnUtility_GetSundayDate(@transactionDateTime)
    WHERE Id = @financialTransactionId

    if (@frequencyDays = 30) BEGIN
        -- magic number to indicate generating a monthly pattern (every 30 to 31 days)
        SET @transactionDateTime = DATEADD(MONTH, - 1, @transactionDateTime);
    END ELSE BEGIN
        SET @transactionDateTime = DATEADD(DAY, - @frequencyDays, @transactionDateTime);
    END
    
    FETCH NEXT
    FROM financialTransaction_cursor
    INTO @financialTransactionId, @givingId
END

CLOSE financialTransaction_cursor

DEALLOCATE financialTransaction_cursor;