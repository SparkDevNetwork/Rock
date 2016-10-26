drop view vAnalytics_Dim_FinancialTransactionType
go
create view vAnalytics_Dim_FinancialTransactionType
as
select 
	dv.Id [TransactionTypeId]
	,dv.Value [Name]
	,dv.[Description]
	,dv.[Order] [SortOrder]
from 
	DefinedValue dv where dv.DefinedTypeId = 25
	