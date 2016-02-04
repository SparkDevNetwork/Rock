DECLARE @today AS DATE = GETDATE();
DECLARE @reportEndDate AS DATE = CONVERT(DATE, DATEADD(DAY, 1 - DATEPART(WEEKDAY, @today), @today));
DECLARE @reportStartDate AS DATE = DATEADD(DAY, -6, @reportEndDate);

IF ISDATE(@StartDate) = 1 AND ISDATE(@EndDate) = 1
BEGIN
    SELECT @reportEndDate = @EndDate;
    SELECT @reportStartDate = @StartDate;
END

SELECT
    p.Id AS PersonId,
	CONCAT(p.LastName, ', ', p.FirstName) AS ContributorName,
	p.Email,
	fb.Name AS BatchName,
	fap.PublicName AS Fund,
	c.Name AS Campus,
	fa.GlCode AS GeneralLedger,
	stv.Value AS OriginatingSource,
	CONVERT(DATE, ft.TransactionDateTime) AS RecievedDate,
	CONVERT(TIME, ft.TransactionDateTime) AS ReceivedTime,
	ctv.Value AS [Type],
	cctv.Value AS [BankCardType],
	ftd.Amount,
	ft.TransactionCode
FROM
	FinancialTransaction ft
	LEFT JOIN FinancialBatch fb ON ft.BatchId = fb.Id
	LEFT JOIN FinancialPaymentDetail pd ON ft.FinancialPaymentDetailId = pd.Id
	JOIN FinancialTransactionDetail ftd ON ftd.TransactionId = ft.Id
	LEFT JOIN FinancialAccount fa ON fa.Id = ftd.AccountId
	LEFT JOIN FinancialAccount fap ON fap.Id = fa.ParentAccountId
	JOIN PersonAlias pa ON pa.Id = ft.AuthorizedPersonAliasId
	JOIN Person p ON p.Id = pa.PersonId
	LEFT JOIN DefinedValue stv ON stv.Id = ft.SourceTypeValueId
	LEFT JOIN DefinedValue ctv ON ctv.Id = pd.CurrencyTypeValueId
	LEFT JOIN DefinedValue cctv ON cctv.Id = pd.CreditCardTypeValueId
	LEFT JOIN Campus c ON c.Id = fa.CampusId
WHERE
	CONVERT(DATE, ft.TransactionDateTime) BETWEEN @reportStartDate AND @reportEndDate
ORDER BY
	ft.TransactionDateTime DESC
