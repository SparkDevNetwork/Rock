{% if PageParameter.DateRange %}
    {% assign dateRange = PageParameter.DateRange | SanitizeSql | Split:',' %}
{% endif %}

{% if dateRange %}

Declare @start date = '{{ dateRange[0] }}'
Declare @end date = '{{ dateRange[1] }}'

{% else dateRange %}
Declare @start date = DateAdd(Year, -1, GetDate() )
Declare @end date = GetDate()

{% endif %}

{% if amount != null and amount != empty %}
Declare @minAmount int = {{amount}}
{% elseif PageParameter.Amount %}
Declare @minAmount int = {{PageParameter.Amount}}
{% else %}
Declare @minAmount int = 5000
{% endif %}

select 
	person.Id
	, person.GivingId
	, person.LastName
	, personHOh.NickName as [HOH]
	, personSpouse.NickName as [Spouse]
	, familyAddress.Street1
	, familyAddress.Street2
	, familyAddress.City
	, familyAddress.State
	, familyAddress.PostalCode
	, familyAddress.Name as [Campus]
	, person.Email
	, '$' + CONVERT(varchar, CAST( d.Amount  AS money), 1) as [Gift Amount]
	, cast(t.TransactionDateTime as date) as [Gift Date]
	, '$' + CONVERT(varchar, CAST(pledgeAmount.TotalAmount  AS money), 1)  as [Pledge Amount]
	, '$' + CONVERT(varchar, CAST(Giving.Amount  AS money), 1)   as [Giving]
	, '$' + CONVERT(varchar, CAST(Giving12m.Amount  AS money), 1)  as [Giving 12m]

from FinancialTransaction t
inner join FinancialTransactionDetail d on  d.TransactionId = t.Id
inner join PersonAlias pa on pa.id = t.AuthorizedPersonAliasId
inner join Person person on person.id = pa.PersonId
INNER JOIN person personHOH on personHOH.id = [dbo].[ufnCrm_GetHeadOfHousePersonIdFromGivingId](person.GivingId)
LEFT OUTER JOIN person personSpouse on personSpouse.id = [dbo].[ufnCrm_GetSpousePersonIdFromPersonId](personHOH.id)
inner join FinancialAccount fa on fa.Id = d.AccountId
OUTER APPLY ( SELECT SUM(td.Amount) as [Amount]
	FROM FinancialTransaction ft
	INNER JOIN FinancialTransactionDetail td on ft.id = td.TransactionId
	INNER JOIN FinancialAccount fa2 on fa2.id = td.AccountId
	INNER JOIN PersonAlias pa2 on pa2.Id = ft.AuthorizedPersonAliasId
	INNER JOIN Person person2 on person2.id = pa2.PersonId
	WHERE person2.GivingId = person.GivingId
) as Giving
OUTER APPLY ( SELECT SUM(td.Amount) as [Amount]
	FROM FinancialTransaction ft
	INNER JOIN FinancialTransactionDetail td on ft.id = td.TransactionId
	INNER JOIN FinancialAccount fa2 on fa2.id = td.AccountId
	INNER JOIN PersonAlias pa2 on pa2.Id = ft.AuthorizedPersonAliasId
	INNER JOIN Person person2 on person2.id = pa2.PersonId
	WHERE person2.GivingId = person.GivingId
	and ft.TransactionDateTime > DateAdd(Year, -1, GetDate() )
) as Giving12m
outer apply( select top(1) l.Street1, l.Street2, l.City, l.State, l.PostalCode, c.Name
	from GroupMember FM 
	left outer join [Group] F ON F.Id = FM.GroupId AND F.GroupTypeId = 10
	left outer join GroupLocation GL ON GL.GroupId = F.Id 
	left outer join Location L ON L.Id = GL.LocationId
	left outer join Campus c on c.id = f.CampusId
	where fm.PersonId = personHOH.id
	and gl.IsMailingLocation = 1
	) familyAddress
	outer apply (select pledge.TotalAmount
		from FinancialPledge pledge
		inner join PersonAlias paPledge on paPledge.id = pledge.PersonAliasId
		inner join Person personPledge on personPledge.id = paPledge.PersonId
		where personPledge.GivingId = person.GivingId
		and cast(pledge.StartDate as date) > DateAdd(Year, -1, GetDate() )
	) as pledgeAmount
where d.Amount >= @minAmount
and cast(t.TransactionDateTime as date) between @start and @end
and personHOH.id != 1
