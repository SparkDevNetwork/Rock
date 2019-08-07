-- If you are testing the CheckScannerUtility, and want to use the same checks multiple times, run this script to clear out MICR data so that the Duplicate Scan Check warning doesn't come up

UPDATE FinancialTransaction
SET CheckMicrEncrypted = NULL
	,CheckMicrHash = NULL
WHERE CheckMicrEncrypted IS NOT NULL
	OR CheckMicrHash IS NOT NULL
