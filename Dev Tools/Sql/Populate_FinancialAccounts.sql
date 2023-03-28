SET NOCOUNT ON

-- Creates random Financial Accounts.  The Child Accounts will be randomly assigned parent accounts.
--
-- Run this to delete previously generated accounts
 -- DELETE FROM [FinancialAccount] where [Name] like 'CodeGen Account%' and Id not in (select AccountId from FinancialTransactionDetail)
--
-- Configurable parameters
DECLARE @maxAccounts INT = 255
    , @maxRootAccounts INT = 10
--
-- non-configurable    
DECLARE @campusId INT = NULL
    , @parentAccountId INT = NULL
    , @financialAccountName NVARCHAR(100)
    , @financialAccountDescription NVARCHAR(max)
    , @financialAccountGuid UNIQUEIDENTIFIER = newid()
    , @rootAccountCount INT = 0
    , @isTaxDeductible BIT = 1
    , @showInGivingOverview BIT = 1
    , @isActive BIT = 1
    , @glCode NVARCHAR(50) = NULL
    , @accountTypeValueId INT = NULL
    , @accountOrder INT = 0
    , @startDate DATE = NULL
    , @endDate DATE = NULL
    , @url NVARCHAR(100) = NULL
    , @isPublic BIT = 1
    , @accountsAdded INT = 0
    , @accountNameNumber INT = (select count(*) from FinancialTransaction)
    , @parentFinancialAccountName NVARCHAR(100)

BEGIN
    SELECT @campusId = NULL;

    WHILE @accountsAdded < @maxAccounts
    BEGIN
        SET @accountsAdded += 1;
        set @accountNameNumber += 1;

        SELECT @parentAccountId = (
                SELECT TOP 1 ID
                FROM FinancialAccount
                ORDER BY newid()
                )

        -- have it be from a root account a about 1/17th of the time, but no more than @maxRootAccounts
        IF (@rootAccountCount < @maxRootAccounts)
        BEGIN
            DECLARE @keepParentAccount BIT = CAST(ROUND(RAND() * 17, 0) AS BIT);

            IF (@keepParentAccount = 0)
            BEGIN
                SET @parentAccountId = NULL
                SET @rootAccountCount += 1;
            END
        END

        SELECT @financialAccountGuid = NEWID();

        SELECT @financialAccountName = substring(CONCAT (
                    'CodeGen Account '
                    , @accountNameNumber
                    ), 0, 35)

        SELECT @financialAccountDescription = 'Description of ' + @financialAccountName;

        INSERT INTO [FinancialAccount] (
            [ParentAccountId]
            , [CampusId]
            , [Name]
            , [PublicName]
            , [Description]
            , [IsTaxDeductible]
            , [ShowInGivingOverview]
            , [GlCode]
            , [Order]
            , [IsActive]
            , [StartDate]
            , [EndDate]
            , [AccountTypeValueId]
            , [Guid]
            , [Url]
            , [PublicDescription]
            , [IsPublic]
            )
        VALUES (
            @parentAccountId
            , @campusId
            , @financialAccountName
            , @financialAccountName
            , @financialAccountDescription
            , @isTaxDeductible
            , @showInGivingOverview
            , @glCode
            , @accountOrder
            , @isActive
            , @startDate
            , @endDate
            , @accountTypeValueId
            , @financialAccountGuid
            , @url
            , @financialAccountDescription
            , @isPublic
            )
    END

    SELECT count(*)
    FROM FinancialAccount
END
