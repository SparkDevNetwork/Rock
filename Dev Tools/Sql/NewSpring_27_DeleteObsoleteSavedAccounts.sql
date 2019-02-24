/*  

	Delete NMI Accounts:
	This script deletes saved accounts from Rock that don't exist in NMI. 
	
	1. Use the SELECT at the top to get the count of how many accounts no longer exist. 
		This will exclude any accounts created today to prevent a time differential 
		between the NMI report and live data.

	2. Use the DROP TABLE section to save as much data about the obsolete accounts 
		(in case a restore is needed!)
	
	3. Use the BEGIN TRANSACTION section to verify only the obsolete accounts are being deleted.

	4. Use COMMIT TRANSACTION when you're sure the delete was correct.

*/

SELECT fa.id RockSavedAccountId, fa.Name, fa.PersonAliasId, fa.ReferenceNumber, fa.CreatedDateTime, fa.ModifiedDateTime, fa.TransactionCode, pd.AccountNumberMasked, p.email 'rockEmails'
FROM FinancialPersonSavedAccount fa
INNER JOIN FinancialPaymentDetail pd
	ON fa.FinancialPaymentDetailId = pd.id
INNER JOIN PersonAlias pa
	ON fa.PersonAliasId = pa.Id
INNER JOIN Person p
	ON pa.PersonId = p.id
LEFT OUTER JOIN [Imports]..nmiAccounts a
	ON fa.ReferenceNumber = a.customer_vault_id
	AND pd.AccountNumberMasked = a.account
WHERE a.customer_vault_id IS NULL
	AND fa.CreatedDateTime < CONVERT(DATE, GETDATE())

--DROP TABLE [Imports]..nmiDeletes
SELECT fa.*, pd.AccountNumberMasked, pd.CurrencyTypeValueId, pd.CreditCardTypeValueId, p.email 'PersonEmail' 
--INTO [Imports]..nmiDeletes
FROM FinancialPersonSavedAccount fa
INNER JOIN FinancialPaymentDetail pd
	ON fa.FinancialPaymentDetailId = pd.id
INNER JOIN PersonAlias pa
	ON fa.PersonAliasId = pa.Id
INNER JOIN Person p
	ON pa.PersonId = p.id
LEFT JOIN [Imports]..nmiAccounts a
	ON fa.ReferenceNumber = a.customer_vault_id	
WHERE a.customer_vault_id IS NULL
	AND fa.CreatedDateTime < CONVERT(DATE, GETDATE())


BEGIN TRANSACTION

DELETE FROM FinancialPersonSavedAccount 
WHERE [Id] IN (SELECT Id from [Imports]..nmiDeletes)

--ROLLBACK TRANSACTION

--COMMIT TRANSACTION