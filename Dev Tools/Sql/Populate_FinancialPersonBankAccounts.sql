/* =============================================================================================
 Author:        Nick Airdo & Code Copilot
 Created:       2025-09-10
 Description:   SQL Server 2016–compatible populate script for [FinancialPersonBankAccount].
                - Inserts ~N fake rows for the first N People (configurable).
                - MAIN FIELDS: AccountNumberSecured, AccountNumberMasked, PersonAliasId.
                - Leaves ForeignKey, ForeignGuid, ForeignId NULL.
 Safety:        Set @DoCommit = 0 to preview (ROLLBACK). Set to 1 to COMMIT.
 ============================================================================================= */

SET NOCOUNT ON;

/* ===============================
   Configuration
   =============================== */
DECLARE @BankAccountCount INT = 50;           -- target number of rows to create
DECLARE @UseFirstNPeople  BIT = 1;            -- 1 = first N by Person.Id; 0 = random N
DECLARE @SkipExisting     BIT = 1;            -- 1 = do not insert if a row already exists for that PersonAliasId
DECLARE @CreatedDateStart DATE = '2025-01-01';
DECLARE @CreatedDateEnd   DATE = GETDATE();
DECLARE @DoCommit         BIT  = 1;           -- 0 = ROLLBACK, 1 = COMMIT

/* ===============================
   Preconditions
   =============================== */
IF OBJECT_ID('dbo.FinancialPersonBankAccount') IS NULL
BEGIN
    RAISERROR('dbo.FinancialPersonBankAccount not found.', 16, 1);
    RETURN;
END;

IF OBJECT_ID('dbo.Person') IS NULL OR OBJECT_ID('dbo.PersonAlias') IS NULL
BEGIN
    RAISERROR('dbo.Person or dbo.PersonAlias not found.', 16, 1);
    RETURN;
END;

/* ===============================
   Build seed of PersonAliasIds (primary alias only)
   =============================== */
IF OBJECT_ID('tempdb..#Seed') IS NOT NULL DROP TABLE #Seed;
CREATE TABLE #Seed (PersonAliasId INT NOT NULL PRIMARY KEY);

IF (@UseFirstNPeople = 1)
BEGIN
    INSERT INTO #Seed(PersonAliasId)
    SELECT TOP (@BankAccountCount) pa.Id
    FROM dbo.Person p
    JOIN dbo.PersonAlias pa
      ON pa.PersonId = p.Id AND pa.AliasPersonId = p.Id
    WHERE (@SkipExisting = 0)
       OR NOT EXISTS (SELECT 1 FROM dbo.FinancialPersonBankAccount f WHERE f.PersonAliasId = pa.Id)
    ORDER BY p.Id ASC;
END
ELSE
BEGIN
    INSERT INTO #Seed(PersonAliasId)
    SELECT TOP (@BankAccountCount) pa.Id
    FROM dbo.Person p
    JOIN dbo.PersonAlias pa
      ON pa.PersonId = p.Id AND pa.AliasPersonId = p.Id
    WHERE (@SkipExisting = 0)
       OR NOT EXISTS (SELECT 1 FROM dbo.FinancialPersonBankAccount f WHERE f.PersonAliasId = pa.Id)
    ORDER BY NEWID();
END

IF NOT EXISTS (SELECT 1 FROM #Seed)
BEGIN
    RAISERROR('No eligible PersonAliasIds found for insertion. Consider setting @SkipExisting = 0 or verify Person records.', 16, 1);
    RETURN;
END;

/* ===============================
   Prepare derived/account fields
   =============================== */
IF OBJECT_ID('tempdb..#Rows') IS NOT NULL DROP TABLE #Rows;
CREATE TABLE #Rows (
    PersonAliasId INT NOT NULL PRIMARY KEY,
    AccountNumberMasked NVARCHAR(MAX) NOT NULL,
    AccountNumberSecured NVARCHAR(128) NOT NULL,
    CreatedDateTime DATETIME NOT NULL
);

DECLARE @Days INT = DATEDIFF(DAY, @CreatedDateStart, @CreatedDateEnd);
IF @Days < 0 SET @Days = 0;

INSERT INTO #Rows (PersonAliasId, AccountNumberMasked, AccountNumberSecured, CreatedDateTime)
SELECT 
    s.PersonAliasId,
    -- Mask: 8 asterisks + last 4 digits, e.g., ********1234
    REPLICATE('*', 8) + Suffix4 AS AccountNumberMasked,
    -- Secured: SHA1 hex of 'bank:{aliasId}:{12-digit acct}' (as NVARCHAR hex)
    CAST(CONVERT(VARCHAR(128), HASHBYTES('SHA1', 'bank:' + CAST(s.PersonAliasId AS VARCHAR(20)) + ':' + (Prefix8 + Suffix4)), 2) AS NVARCHAR(128)) AS AccountNumberSecured,
    DATEADD(DAY, CASE WHEN @Days = 0 THEN 0 ELSE ABS(CHECKSUM(NEWID())) % (@Days + 1) END, CAST(@CreatedDateStart AS DATETIME)) AS CreatedDateTime
FROM #Seed s
CROSS APPLY (
    SELECT RIGHT('00000000' + CAST(ABS(CHECKSUM(NEWID())) % 100000000 AS VARCHAR(8)), 8) AS Prefix8,
           RIGHT('0000'     + CAST(ABS(CHECKSUM(NEWID())) % 10000      AS VARCHAR(4)), 4) AS Suffix4
) r;

/* ===============================
   Insert
   =============================== */
DECLARE @Inserted INT = 0;
BEGIN TRAN;
BEGIN TRY
    INSERT INTO dbo.FinancialPersonBankAccount (
          AccountNumberSecured
        , [Guid]
        , CreatedDateTime
        , ModifiedDateTime
        , CreatedByPersonAliasId
        , ModifiedByPersonAliasId
        , AccountNumberMasked
        , PersonAliasId
        -- ForeignKey/ForeignGuid/ForeignId intentionally omitted per request
    )
    SELECT 
          r.AccountNumberSecured
        , NEWID()
        , r.CreatedDateTime
        , r.CreatedDateTime
        , NULL
        , NULL
        , r.AccountNumberMasked
        , r.PersonAliasId
    FROM #Rows r;

    SET @Inserted = @@ROWCOUNT;

    IF (@DoCommit = 1)
    BEGIN
        COMMIT TRAN;
        PRINT CONCAT('Inserted FinancialPersonBankAccount rows: ', @Inserted);
    END
    ELSE
    BEGIN
        ROLLBACK TRAN;
        PRINT CONCAT('Preview only. Would have inserted FinancialPersonBankAccount rows: ', @Inserted);
    END
END TRY
BEGIN CATCH
    DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
    IF XACT_STATE() <> 0 ROLLBACK TRAN;
    RAISERROR('Populate_FinancialPersonBankAccount failed: %s', 16, 1, @ErrMsg);
    RETURN;
END CATCH;

/* ===============================
   Verify
   =============================== */
/*
SELECT TOP (50)
      f.Id
    , f.PersonAliasId
    , f.AccountNumberMasked
    , LEN(f.AccountNumberSecured) AS SecuredLen
    , f.CreatedDateTime
FROM dbo.FinancialPersonBankAccount f WITH (NOLOCK)
ORDER BY f.Id DESC;
*/