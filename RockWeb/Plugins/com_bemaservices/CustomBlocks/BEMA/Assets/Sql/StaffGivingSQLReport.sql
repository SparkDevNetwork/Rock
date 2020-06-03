{% comment %}
  Checks groups 2 and 3 for staff (Rock Admins and Staff Security Roles)
{% endcomment %}

select  distinct
	p.id
	, p.GivingId
	, p.FirstName + ' ' + p.LastName as [Name]
	, p.Email
	, campus.ShortCode as [Campus]
	, '$' + CONVERT(varchar, CAST(GivingLast12Months.Total AS money), 1)  as [Giving Last 12 Months]
	, '$' + CONVERT(varchar, CAST(GivingLast24Months.Total AS money), 1)  as [Giving Last 24 Months]
from GroupMember m
inner join [Group] g on g.id = m.GroupId and g.id in (2,3)
inner join person p on p.id = m.PersonId
inner join GroupMember familyGroupM on familyGroupM.PersonId = p.Id
inner join [Group] familyGroupT on familyGroupT.id = familyGroupM.GroupId and familyGroupT.GroupTypeId = 10 
left outer join Campus campus on campus.Id = familyGroupT.CampusId
outer apply ( select sum(d1.amount) as Total
	, count(*) as TotalCount
	from FinancialTransaction t1
	inner join FinancialTransactionDetail d1 on d1.TransactionId = t1.Id
	inner join FinancialAccount a1 on a1.Id = d1.AccountId
	inner join PersonAlias pa1 on pa1.Id = t1.AuthorizedPersonAliasId
	inner join Person p1 on p1.id = pa1.PersonId
	where t1.TransactionDateTime > getdate()-360
	  and p1.GivingId = p.GivingId
	) as GivingLast12Months
outer apply ( select sum(d2.amount) as Total
	, count(*) as TotalCount
	from FinancialTransaction t2
	inner join FinancialTransactionDetail d2 on d2.TransactionId = t2.Id
	inner join FinancialAccount a2 on a2.Id = d2.AccountId
	inner join PersonAlias pa2 on pa2.Id = t2.AuthorizedPersonAliasId
	inner join Person p2 on p2.id = pa2.PersonId
	where t2.TransactionDateTime > getdate()-720
	  and p2.GivingId = p.GivingId
	) as GivingLast24Months
where m.GroupMemberStatus = 1 
and m.IsArchived = 0