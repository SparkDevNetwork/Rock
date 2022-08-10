SET NOCOUNT ON

/* Change these settings to your liking*/
DECLARE @yearsBack INT = 10
    , @randomizePersonList BIT = 1
    , /* set to false to use a consistent set of X people (ordered by Person.Id) instead of using a randomized set */
    @maxPersonCount INT = 1000
    , /* limit to a count of X persons in the database */
    @maxTransactionCount INT = 100
DECLARE @authorizedPersonAliasId INT = 1
    , @transactionCounter INT = 0
    , @maxPersonAliasIdForTransactions INT = (
        SELECT max(Id)
        FROM (
            SELECT TOP (@maxPersonCount) Id
            FROM PersonAlias
            ORDER BY Id
            ) x
        )
    , @transactionDateTime DATETIME
    , @transactionAmount DECIMAL(18, 2)
    , @transactionTypeValueId INT = (
        SELECT TOP 1 Id
        FROM DefinedValue
        WHERE Guid = '2D607262-52D6-4724-910D-5C6E8FB89ACC'
        )
    , @currencyTypeCreditCard INT = (
        SELECT TOP 1 Id
        FROM DefinedValue
        WHERE Guid = '928A2E04-C77B-4282-888F-EC549CEE026A'
        )
    , @creditCardTypeVisa INT = (
        SELECT Id
        FROM DefinedValue
        WHERE Guid = 'FC66B5F8-634F-4800-A60D-436964D27B64'
        )
    , @sourceTypeDefinedTypeId INT = (
        SELECT TOP 1 Id
        FROM DefinedType
        WHERE [Guid] = '4F02B41E-AB7D-4345-8A97-3904DDD89B01'
        )
    , @transactionFrequencyDefinedTypeId INT = (
        SELECT TOP 1 Id
        FROM DefinedType
        WHERE [Guid] = '1f645cfb-5bbd-4465-b9ca-0d2104a1479b'
        )
    , @financialTestGatewayId INT = (
        SELECT TOP 1 Id
        FROM FinancialGateway
        WHERE [Guid] = '6432d2d2-32ff-443d-b5b3-fb6c8414c3ad'
        ) -- Test Gateway
DECLARE @sourceTypeValueId INT = (
        SELECT TOP 1 Id
        FROM DefinedValue
        WHERE DefinedTypeId = @sourceTypeDefinedTypeId
        ORDER BY NEWID()
        )
    , @accountId INT = (
        SELECT TOP 1 id
        FROM FinancialAccount
        WHERE IsTaxDeductible = 1
        )
    , @transactionId INT
    , @financialPaymentDetailId INT
    , @transactionFrequencyValueId INT
DECLARE @daysBack INT = @yearsBack * 366

BEGIN
    IF CURSOR_STATUS('global', 'personAliasIdCursor') >= - 1
    BEGIN
        DEALLOCATE personAliasIdCursor;
    END

    DECLARE @personAliasIds TABLE (id INT NOT NULL);

    -- Get a list of person alias ids into a temporary table so that the loop uses the same set of PersonAliasIds every time
    -- Exclude Giver Anonymous and Anonymous Visitor
    INSERT INTO @personAliasIds
    SELECT TOP (@maxPersonCount) pa.Id
    FROM PersonAlias pa
    INNER JOIN Person p
        ON pa.PersonId = p.Id
    WHERE p.[Guid] NOT IN ('7ebc167b-512d-4683-9d80-98b6bb02e1b9', '802235dc-3ca5-94b0-4326-aace71180f48')
    ORDER BY CASE 
            WHEN @randomizePersonList = 1
                THEN CHECKSUM(NEWID())
            ELSE PersonId
            END

    -- put all personIds in randomly ordered cursor to speed up getting a random personAliasId for each attendance
    DECLARE personAliasIdCursor CURSOR LOCAL FAST_FORWARD
    FOR
    SELECT Id
    FROM @personAliasIds

    OPEN personAliasIdCursor;

    BEGIN TRANSACTION

    SET @transactionDateTime = DATEADD(DAY, - @daysBack, SYSDATETIME())

    WHILE @transactionCounter < @maxTransactionCount
    BEGIN
        FETCH NEXT
        FROM personAliasIdCursor
        INTO @authorizedPersonAliasId;

        IF (@@FETCH_STATUS != 0)
        BEGIN
            CLOSE personAliasIdCursor;

            OPEN personAliasIdCursor;

            FETCH NEXT
            FROM personAliasIdCursor
            INTO @authorizedPersonAliasId;
        END

        -- do a random number of transactions per person so that we don't have exactly the same number of transactions per person
        DECLARE @MaxTransactionCountPerPerson INT = ROUND(rand() * 20, 0);

        -- select a random amount with smaller rounded off amounts being more common than random amounts
        SET @transactionAmount = (
                SELECT round(w.r, 1)
                FROM (
                    SELECT (
                            CASE floor(rand(CHECKSUM(newid())) * 12)
                                WHEN 1
                                    THEN 100.00
                                WHEN 2
                                    THEN 100.00
                                WHEN 3
                                    THEN 150.00
                                WHEN 4
                                    THEN 200.00
                                WHEN 5
                                    THEN 20.00
                                WHEN 6
                                    THEN 20.00
                                WHEN 7
                                    THEN 25.00
                                WHEN 8
                                    THEN 250.00
                                ELSE RAND() * 1000
                                END
                            ) [r]
                    ) w
                )

        INSERT INTO FinancialPaymentDetail (
            CurrencyTypeValueId
            , CreditCardTypeValueId
            , [Guid]
            , [CreatedDateTime]
            , [ModifiedDateTime]
            )
        VALUES (
            @currencyTypeCreditCard
            , @creditCardTypeVisa
            , NEWID()
            , @transactionDateTime
            , SYSDATETIME()
            );

        SET @financialPaymentDetailId = SCOPE_IDENTITY()

        IF (@transactionCounter % 100 = 0)
        BEGIN
            SET @sourceTypeValueId = (
                    SELECT TOP 1 Id
                    FROM DefinedValue
                    WHERE DefinedTypeId = @sourceTypeDefinedTypeId
                    ORDER BY NEWID()
                    )
        END

        IF (@transactionCounter % 17 = 0)
        BEGIN
            SET @accountId = (
                    SELECT TOP 1 id
                    FROM FinancialAccount
                    WHERE IsTaxDeductible = 1
                    ORDER BY NEWID()
                    );
        END

        SET @transactionFrequencyValueId = (
                SELECT TOP 1 Id
                FROM DefinedValue
                WHERE DefinedTypeId = @transactionFrequencyDefinedTypeId
                ORDER BY NEWID()
                )

        INSERT INTO [dbo].[FinancialScheduledTransaction] (
            [TransactionFrequencyValueId]
            , [StartDate]
            , [EndDate]
            , [NumberOfPayments]
            , [NextPaymentDate]
            , [LastStatusUpdateDateTime]
            , [IsActive]
            , [TransactionCode]
            , [GatewayScheduleId]
            , [AuthorizedPersonAliasId]
            , [FinancialGatewayId]
            , [FinancialPaymentDetailId]
            , [SourceTypeValueId]
            , [TransactionTypeValueId]
            , [Guid]
            , [CreatedDateTime]
            , [ModifiedDateTime]
            )
        VALUES (
            @transactionFrequencyValueId -- TransactionFrequencyValueId
            , DATEADD(day, 1, @transactionDateTime) -- EndDate
            , NULL -- EndDate
            , NULL -- NumberOfPayments
            , DATEADD(day, 1, @transactionDateTime) -- NextPaymentDate
            , NULL -- LastStatusUpdateDateTime
            , 1 -- IsActive
            , CONCAT (
                'CODEGEN_TC_'
                , ABS(CHECKSUM(newid()))
                ) -- TransactionCode
            , CONCAT (
                'CODEGEN_GS_'
                , ABS(CHECKSUM(newid()))
                ) -- GatewayScheduleId
            , @authorizedPersonAliasId -- AuthorizedPersonAliasId
            , @financialTestGatewayId -- FinancialGatewayId
            , @financialPaymentDetailId -- FinancialPaymentDetailId
            , @sourceTypeValueId -- SourceTypeValueId
            , @transactionTypeValueId -- TransactionTypeValueId
            , NEWID() -- Guid
            , @transactionDateTime -- CreatedDateTime
            , SYSDATETIME() -- ModifiedDateTime
            )

        SET @transactionId = SCOPE_IDENTITY()

        -- For contributions, we just need to put in the AccountId (entitytype/entityid would be null)
        INSERT INTO [dbo].[FinancialScheduledTransactionDetail] (
            [ScheduledTransactionId]
            , [AccountId]
            , [Amount]
            , [Summary]
            , [EntityTypeId]
            , [EntityId]
            , [Guid]
            , [CreatedDateTime]
            , [ModifiedDateTime]
            )
        VALUES (
            @transactionId
            , @accountId
            , @transactionAmount
            , NULL
            , NULL
            , NULL
            , NEWID()
            , @transactionDateTime
            , SYSDATETIME()
            )

        SET @transactionCounter += 1;

        DECLARE @randomSecondsAgo INT = (RAND(CHECKSUM(newid()))) * @daysBack * 86400;

        SET @transactionDateTime = DATEADD(ss, - @randomSecondsAgo, GetDate());

        IF (@transactionCounter % 10000 = 0)
        BEGIN
            COMMIT TRANSACTION

            BEGIN TRANSACTION

            PRINT @transactionCounter
        END
    END

    COMMIT TRANSACTION

    CLOSE personAliasIdCursor;
END;

SELECT count(*)
FROM FinancialScheduledTransaction
