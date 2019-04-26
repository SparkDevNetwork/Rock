UPDATE FinancialTransaction
SET CheckMicrHash = NULL
	,CheckMicrEncrypted = NULL
WHERE CheckMicrHash IS NOT NULL
	OR CheckMicrEncrypted IS NOT NULL
