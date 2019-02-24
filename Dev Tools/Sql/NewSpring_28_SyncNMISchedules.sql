/*  

	Sync NMI Schedules:
	This script syncs Scheduled Transactions in Rock that were cancelled in NMI or vice versa. 
	
	1. Use the SELECT at the top to get the count of how many schedules are in NMI that don't 
		exist in Rock. For ease of inserting new schedules, the necessary fields are added
		via LEFT JOINs.
		
		Use the second SELECT to find how many active schedules are set in Rock but don't exist
		in NMI. This will exclude any accounts created today to prevent a time differential 
		between the NMI report and live data.

	2. Use the BEGIN TRANSACTION section to create new schedules in Rock. For historical purposes,
		the ForeignKey field will be set on newly inserted transactions.
		
	4. Use COMMIT TRANSACTION when you're sure the sync was correct, or ROLLBACK the transaction.

*/


--DROP TABLE #newSchedules

/* Get NMI schedules not in Rock */
SELECT s.*, pa.Id AuthorizedPersonAliasId, g.Id FinancialGatewayId, ct.Id CurrencyType, cc.Id CreditCardType, l.Id LocationId, tf.Id TransactionFrequency, CONVERT(DATE, '20' + RIGHT(s.account_expiration, 2) + '-' + LEFT(s.account_expiration, 2) + '-01') CardExpiration, fa.Id ParentAccountId, cfa.Id CampusAccountId
INTO #newSchedules
FROM [Imports]..nmiSchedules s
-- get the authorized person
LEFT JOIN Person p
	ON s.email = p.email	
	AND (s.first_name = p.FirstName OR s.first_name = p.NickName OR s.first_name = p.MiddleName)	
LEFT JOIN PersonAlias pa
	ON p.Id = pa.PersonId
	/* use the original alias */
	AND p.Id = pa.AliasPersonId
-- look for existing schedules
LEFT JOIN [Rock]..FinancialScheduledTransaction st
	ON s.subscription_id = st.GatewayScheduleId
-- get the currency type
LEFT JOIN DefinedValue ct
	/* Rock Currency Type */
	ON ct.DefinedTypeId = 10 
	AND ct.Value = replace(replace(s.payment_type, 'cc', 'Credit Card'), 'ck', 'ACH')
-- get the credit card type
LEFT JOIN DefinedValue cc
	ON cc.DefinedTypeId = 11
	AND s.payment_type = 'cc'
	/* parse the account prefix (only supported types) */
	AND cc.Value = CASE LEFT(s.account, 1)
	  WHEN '4' THEN 'Visa' 
	  WHEN '5' THEN 'MasterCard'  
	  WHEN '3' THEN 'American Express' 
	  WHEN '6' THEN 'Discover'  
	  ELSE NULL
	END
-- get trans frequency 
LEFT JOIN DefinedValue tf
	ON tf.DefinedTypeId = 23
	AND tf.Value = CASE
		WHEN s.billing_cycle LIKE '%7 day(s)' THEN 'Weekly'
		WHEN s.billing_cycle LIKE '%14 day(s)' THEN 'Bi-Weekly'
		WHEN s.billing_cycle LIKE '%day of the month' THEN 'Monthly'
		WHEN s.billing_cycle LIKE '%12 month(s)' THEN 'Yearly'
		ELSE NULL
	END
-- get gateway id
LEFT JOIN [Rock]..FinancialGateway g
	ON g.Name LIKE '%NMI%'
-- get billing location
LEFT JOIN Location l
	ON s.city = l.City
	AND s.start_date = l.State
	AND REPLACE(s.address1, '.', '') = REPLACE(l.Street1, '.', '')
-- get account id from merchant fields
LEFT JOIN FinancialAccount fa
	ON fa.id = SUBSTRING(s.merchant_defined_field_1, 0, LEN(s.merchant_defined_field_1) - CHARINDEX(',',s.merchant_defined_field_1) +1)
LEFT JOIN FinancialAccount cfa
	ON cfa.ParentAccountId = SUBSTRING(s.merchant_defined_field_1, 0, LEN(s.merchant_defined_field_1) - CHARINDEX(',',s.merchant_defined_field_1) +1)
	AND cfa.CampusId = s.merchant_defined_field_2
WHERE st.Id IS NULL
	AND s.status = 'active'
	AND s.email IS NOT NULL



/* Get Rock schedules not in NMI */
DECLARE @maxCreatedSchedule DATETIME = (SELECT MAX(created) FROM [Imports]..nmiSchedules)

SELECT st.*, std.Amount
FROM [Rock]..FinancialScheduledTransaction st
INNER JOIN [Rock]..FinancialScheduledTransactionDetail std
	ON st.Id = std.ScheduledTransactionId
LEFT JOIN [Imports]..nmiSchedules s
	ON s.subscription_id = st.GatewayScheduleId
WHERE s.recurring_sku IS NULL
	AND st.IsActive = 1
	AND st.TransactionFrequencyValueId <> 130
	AND st.CreatedDateTime >= @maxCreatedSchedule	
	

/* Create missing records */
BEGIN TRANSACTION

/* Create Rock payment detail */
INSERT FinancialPaymentDetail (AccountNumberMasked, CurrencyTypeValueId, CreditCardTypeValueId, BillingLocationId, CreatedDateTime, ModifiedDateTime, CreatedByPersonAliasId, ModifiedByPersonAliasId, [Guid], ForeignKey)
SELECT RIGHT(s.account, 4), s.CurrencyType, s.CreditCardType, s.LocationId, s.Created, s.Created, s.AuthorizedPersonAliasId, s.AuthorizedPersonAliasId, NEWID(), 'NMI Sync 1/17/17'
FROM #newSchedules s

/* Create Rock scheduled transaction */
INSERT FinancialScheduledTransaction (TransactionFrequencyValueId, StartDate, NextPaymentDate, IsActive, GatewayScheduleId, CardReminderDate, [Guid], CreatedDateTime, ModifiedDateTime, CreatedByPersonAliasId, ModifiedByPersonAliasId, ForeignKey, AuthorizedPersonAliasId, FinancialGatewayId, FinancialPaymentDetailId)
SELECT s.TransactionFrequency, REPLACE(REPLACE(s.start_date, 'Started ', ''), 'Starts ', ''), s.next_charge_date, 1, s.subscription_id, s.CardExpiration, NEWID(), s.Created, s.Created, s.AuthorizedPersonAliasId, s.AuthorizedPersonAliasId, 'NMI Sync 1/17/17', s.AuthorizedPersonAliasId, s.FinancialGatewayId, d.Id
FROM #newSchedules s
INNER JOIN FinancialPaymentDetail d
	ON s.CurrencyType = d.CurrencyTypeValueId
	AND s.created = d.CreatedDateTime
	AND s.AuthorizedPersonAliasId = d.CreatedByPersonAliasId
	AND s.AuthorizedPersonAliasId = d.ModifiedByPersonAliasId
	AND d.ForeignKey = 'NMI Sync 1/17/17'


/* Create Rock scheduled transaction detail */
INSERT FinancialScheduledTransactionDetail (ScheduledTransactionId, AccountId, Amount, [Guid], CreatedDateTime, ModifiedDateTime, CreatedByPersonAliasId, ModifiedByPersonAliasId, ForeignKey) 
SELECT st.Id, ISNULL(s.CampusAccountId, s.ParentAccountId), s.amount, NEWID(), s.created, s.created, s.AuthorizedPersonAliasId, s.AuthorizedPersonAliasId, 'NMI Sync 1/17/17'
FROM #newSchedules s
INNER JOIN FinancialScheduledTransaction st
	ON s.TransactionFrequency = st.TransactionFrequencyValueId
	AND s.created = st.CreatedDateTime
	AND s.AuthorizedPersonAliasId = st.CreatedByPersonAliasId
	AND s.AuthorizedPersonAliasId = st.ModifiedByPersonAliasId
	AND st.ForeignKey = 'NMI Sync 1/17/17'

--ROLLBACK TRANSACTION

--COMMIT TRANSACTION