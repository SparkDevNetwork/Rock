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

select fa.id RockSavedAccountId, fa.Name, fa.PersonAliasId, fa.ReferenceNumber, fa.CreatedDateTime, fa.ModifiedDateTime, fa.TransactionCode, pd.AccountNumberMasked, p.email 'rockEmails'
from FinancialPersonSavedAccount fa
inner join FinancialPaymentDetail pd
on fa.FinancialPaymentDetailId = pd.id
inner join PersonAlias pa
on fa.PersonAliasId = pa.Id
inner join Person p
on pa.PersonId = p.id
left outer join [Imports]..nmiAccounts a
on fa.ReferenceNumber = a.vault_id
and pd.AccountNumberMasked = a.account
where a.vault_id is null
and fa.CreatedDateTime < CONVERT(DATE, getdate())

drop table [Imports]..nmiDeletes
select fa.id SavedAccountId, fa.Name, fa.PersonAliasId, fa.ReferenceNumber, fa.CreatedDateTime, fa.ModifiedDateTime, fa.TransactionCode, pd.AccountNumberMasked, p.email 'rockEmails'
--into [Imports]..nmiDeletes
from FinancialPersonSavedAccount fa
inner join FinancialPaymentDetail pd
on fa.FinancialPaymentDetailId = pd.id
inner join PersonAlias pa
on fa.PersonAliasId = pa.Id
inner join Person p
on pa.PersonId = p.id
left join [Imports]..nmiAccounts a
on fa.ReferenceNumber = a.vault_id
and pd.AccountNumberMasked = a.account
where a.vault_id is null
and fa.CreatedDateTime < CONVERT(DATE, getdate())

begin transaction

delete from FinancialPersonSavedAccount 
where Id IN (select SavedAccountId from [Imports]..nmiDeletes)


--rollback transaction

--commit transaction
