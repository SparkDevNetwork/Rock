-- DECLARE @StartDate AS NVARCHAR(MAX) = '01/01/20';
-- DECLARE @EndDate AS NVARCHAR(MAX) = '12/31/20';

DECLARE @EntityId AS NVARCHAR(MAX);
DECLARE @PersonId AS INT = {{ CurrentPerson.Id }};
DECLARE @GivingGroupId AS INT;

-- default start date is a year ago
IF ISDATE(@StartDate) = 0
BEGIN
    SELECT @StartDate = DATEADD(yy, DATEDIFF(yy, 0, GETDATE()), 0)
END;

-- default end date is today
IF ISDATE(@EndDate) = 0
BEGIN
    SELECT @EndDate = GETDATE()
END;

-- set entity id based on business id being present
IF @BusinessId > 0
    BEGIN
        SET @EntityId = @BusinessId;
    END;
ELSE
    BEGIN
        SET @EntityId = @PersonId;
    END;

-- set giving group id based on entity
SET @GivingGroupId = (
    SELECT GivingGroupId
    FROM Person p
    WHERE p.Id = @EntityId
)

-- get transactions
IF(ISNULL(@GivingGroupId, null) IS null)
    SELECT *
    FROM FinancialTransaction ft
    JOIN PersonAlias pa
    ON ft.AuthorizedPersonAliasId = pa.Id
    JOIN Person p
    ON pa.PersonId = p.Id
    JOIN FinancialTransactionDetail ftd
    ON ftd.TransactionId = ft.Id
    JOIN FinancialAccount fa
    ON ftd.AccountId = fa.Id
    JOIN FinancialAccount pfa
    ON fa.ParentAccountId = pfa.Id
    WHERE ft.AuthorizedPersonAliasId IN
    (
        SELECT pa.Id
        FROM PersonAlias pa
        WHERE pa.PersonId = @EntityId
    )
    AND ft.TransactionDateTime >= @StartDate
    AND ft.TransactionDateTime <= @EndDate
    AND pfa.IsTaxDeductible = 1
    ORDER BY ft.TransactionDateTime DESC
ELSE
    SELECT *
    FROM FinancialTransaction ft
    JOIN PersonAlias pa
    ON ft.AuthorizedPersonAliasId = pa.Id
    JOIN Person p
    ON pa.PersonId = p.Id
    JOIN FinancialTransactionDetail ftd
    ON ftd.TransactionId = ft.Id
    JOIN FinancialAccount fa
    ON ftd.AccountId = fa.Id
    JOIN FinancialAccount pfa
    ON fa.ParentAccountId = pfa.Id
    WHERE ft.AuthorizedPersonAliasId IN
    (
        SELECT pa.Id
        FROM GroupMember gm
        JOIN Person p
        ON gm.PersonId = p.Id
        JOIN PersonAlias pa
        ON pa.PersonId = p.Id
        WHERE GroupId IN (
            SELECT g.Id
            FROM GroupMember gm
            JOIN [Group] g
            ON gm.GroupId = g.Id
            WHERE gm.PersonId = @EntityId
            AND g.GroupTypeId = 10
        ) AND
        (
            p.GivingGroupId = @GivingGroupId
            OR
            p.AgeClassification = 2
        )
    )
    AND ft.TransactionDateTime >= @StartDate
    AND ft.TransactionDateTime <= @EndDate
    AND pfa.IsTaxDeductible = 1
    ORDER BY ft.TransactionDateTime DESC