/* ====================================================== 
-- NewSpring Script #29: 
-- Adds a schedule that was found in NMI, but missing in
Rock.

0. ONLY RUN INSIDE AN ENVIRONMENT THAT LETS YOU USE `transaction`
1. Find a person with missing schedule and get their ID (not alias)
2. Look up the schedule in NMI
3. Fill out the variables at the top of this script (Before `DONT CHANGE THESE`)
4. Run script
5. BEFORE COMMITTING, check the output to make sure everything looks right
6. Commit transaction
  - If you need to undo something after committing, you can run the `RESET` block at the bottom
  - That will remove all entries created by this script with the same Foreign Key (the same day)
7. Check rock for the transaction
====================================================== */

-- CHANGE PEOPLE
declare @YourPersonId int = 12345 -- not alias
declare @GiverPersonId int = 2222 -- not alias

-- CHANGE FOR SCHEDULES
declare @TransactionFrequencyString varchar(50) = 'Monthly' -- enum['One-Time', 'Weekly', 'Bi-Weekly', 'Twice a Month', 'Monthly', 'Quarterly', 'Twice a Year', 'Yearly']
declare @ScheduleStart date = '2017-01-01' -- in format YYYY-MM-DD
declare @ScheduleNextPayment date = '2017-02-01' -- in format YYYY-MM-DD
declare @GatewayScheduleId nvarchar(50) = 12345678 -- the NMI schedule id

-- SCHEDULE DETAILS (DONT CHANGE THIS LINE)
declare @ScheduleDetails table (MerchantField1 int, MerchantField2 int, Amount decimal(18,2), AccountId int)

-- merchant-defined-field-1, merchant-defined-field-2, amount
-- **DO NOT LOOK UP THE CORRECT ACCOUNT. USE THE MERCHANT FIELDS IN NMI**
insert into @ScheduleDetails(MerchantField1, MerchantField2, Amount) values(125, 8, 10.00) -- ONCE FOR EACH ACCOUNT ON A SCHEDULE

-- PAYMENT DETAILS
declare @AccountNumberMasked varchar(50) = '9999' -- LAST 4
declare @CurrencyTypeValueString varchar(50) = 'Credit Card' -- enum['Cash', 'Check', 'Credit Card', 'ACH', 'Non-Cash']
declare @CreditCardTypeString varchar(50) = 'MasterCard' -- enum[NULL, 'Visa','MasterCard','American Express','Discover','Diner''s Club','JCB']

-- DONT CHANGE THESE
declare @YourPrimaryAliasId int = (select top 1 Id from PersonAlias where PersonId=@YourPersonId and AliasPersonId=@YourPersonId)
declare @GiverPrimaryAliasId int = (select top 1 Id from PersonAlias where PersonId=@GiverPersonId and AliasPersonId=@GiverPersonId)
update @ScheduleDetails set AccountId = (select top 1 Id from FinancialAccount where (CampusId = MerchantField2 and ParentAccountId = MerchantField1) or (Id = MerchantField1) order by ParentAccountId desc)
declare @CurrencyTypeValueId int = (select Id from DefinedValue where DefinedTypeId = 10 and Value = @CurrencyTypeValueString)
declare @CreditCardTypeValueId int = (select Id from DefinedValue where DefinedTypeId = 11 and Value = @CreditCardTypeString)
declare @TransactionFrequencyValueId int = (select Id from DefinedValue where DefinedTypeId = 23 and Value = @TransactionFrequencyString)
DECLARE @ForeignKey AS NVARCHAR(MAX) = 'NMIManualReconciliation' + ' ' + CONVERT(varchar(20), GETDATE(), 1);	

begin transaction

-- PAYMENT DETAILS
-- declare guid separate for lookup
declare @PaymentDetailGuid uniqueidentifier = NEWID()
insert into FinancialPaymentDetail(
	AccountNumberMasked,
	CurrencyTypeValueId,
	CreditCardTypeValueId,
	CreatedByPersonAliasId,
	ModifiedByPersonAliasId,
	ForeignKey,
	Guid
) values (
	@AccountNumberMasked,
	@CurrencyTypeValueId,
	@CreditCardTypeValueId,
	@GiverPrimaryAliasId,
	@YourPrimaryAliasId,
	@ForeignKey,
	@PaymentDetailGuid
)

-- THE SCHEDULE
declare @ScheduleGuid uniqueidentifier = NEWID()
insert into FinancialScheduledTransaction (
	AuthorizedPersonAliasId,
	TransactionFrequencyValueId,
	StartDate,
	NextPaymentDate,
	IsActive,
	FinancialGatewayId,
	FinancialPaymentDetailId,
	GatewayScheduleId,
	CreatedByPersonAliasId,
	ModifiedByPersonAliasId,
	SourceTypeValueId,
	Guid,
	ForeignKey
) values (
	@GiverPrimaryAliasId,
	@TransactionFrequencyValueId,
	@ScheduleStart,
	@ScheduleNextPayment,
	1, -- is active
	3, -- NMI
	(select Id from FinancialPaymentDetail where Guid = @PaymentDetailGuid),
	@GatewayScheduleId,
	@GiverPrimaryAliasId,
	@YourPrimaryAliasId,
	798,
	@ScheduleGuid,
	@ForeignKey
)

-- THE SCHEDULE DETAILS
insert into FinancialScheduledTransactionDetail (
	AccountId,
	Amount,
	ScheduledTransactionId,
	CreatedByPersonAliasId,
	ModifiedByPersonAliasId,
	Guid,
	ForeignKey
) select
  AccountId,
  Amount,
  (select Id from FinancialScheduledTransaction where Guid = @ScheduleGuid),
  @YourPrimaryAliasId,
  @GiverPrimaryAliasId,
  NEWID(),
  @ForeignKey
from @ScheduleDetails

-- SHOW THE ADDED SCHEDULE DETAILS
select
	s.Id as ScheduleId,
	s.TransactionFrequencyValueId,
	s.StartDate,
	s.NextPaymentDate,
	s.GatewayScheduleId,
	s.CreatedByPersonAliasId,
	sd.Id as ScheduleDetailId,
	sd.AccountId,
	sd.Amount,
	pd.Id as PaymentDetailId,
	pd.AccountNumberMasked,
	pd.CurrencyTypeValueId
from 
	FinancialScheduledTransaction as s
	inner join FinancialScheduledTransactionDetail as sd on sd.ScheduledTransactionId = s.Id
	inner join FinancialPaymentDetail as pd on s.FinancialPaymentDetailId = pd.Id 
where s.Guid = @ScheduleGuid

declare @scheduleId int = (select Id from FinancialScheduledTransaction where Guid = @ScheduleGuid)
select concat('https://rock.newspring.cc/page/319?ScheduledTransactionId=',@scheduleId)

--rollback transaction

--commit transaction

--RESET
/*
begin transaction
DECLARE @ForeignKey AS NVARCHAR(MAX) = 'NMIManualReconciliation' + ' ' + CONVERT(varchar(20), GETDATE(), 1);	
--select * from FinancialScheduledTransactionDetail where ForeignKey like '%NMIManual%' 
delete from FinancialScheduledTransactionDetail where ForeignKey = @ForeignKey
delete from FinancialScheduledTransaction where ForeignKey = @ForeignKey
delete from FinancialPaymentDetail where ForeignKey = @ForeignKey
--commit transaction
*/


