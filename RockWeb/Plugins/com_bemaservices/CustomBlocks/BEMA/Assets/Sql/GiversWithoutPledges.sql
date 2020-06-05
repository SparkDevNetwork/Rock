DECLARE @start date = DateAdd(Year, -1, GetDate() )
DECLARE @end date = GetDate()

	SELECT 
	 personHOH.GivingId
	, personHOH.FirstName as [First Name]
	, personHOH.LastName as [Last Name]
	, personHOH.Id
	, personSpouse.FirstName as [Spouse]
	, personHOH.Email
	, c.Name as [Campus] 
	, '$' + CONVERT(varchar, CAST(sum(case when cast(ft.TransactionDateTime as date) between @start and @end then td.Amount else 0 end) AS money), 1) as [12 m]
	, '$' + CONVERT(varchar, CAST(sum(case when cast(ft.TransactionDateTime as date) between @start and @end then td.Amount  else 0 end) AS money), 1) as [24 m]

	FROM FinancialTransaction ft
	INNER JOIN FinancialTransactionDetail td on ft.id = td.TransactionId
	INNER JOIN FinancialAccount fa2 on fa2.id = td.AccountId
	INNER JOIN PersonAlias pa2 on pa2.Id = ft.AuthorizedPersonAliasId
	INNER JOIN Person person2 on person2.id = pa2.PersonId
	inner join Person personHOH on personHOH.id = dbo.ufnCrm_GetHeadOfHousePersonIdFromGivingId(person2.GivingID)
	left outer join Person personSpouse on personSpouse.id = dbo.ufnCrm_GetSpousePersonIdFromPersonId( personHOH.id)
	INNER JOIN GroupMember gm on gm.PersonId = personHOH.id
	INNER JOIN [Group] g on g.id = gm.GroupId and g.GroupTypeId = 10
	LEFT OUTER JOIN [Campus] c on c.Id = g.CampusId
	outer apply (select pledge.TotalAmount
		from FinancialPledge pledge
		inner join PersonAlias paPledge on paPledge.id = pledge.PersonAliasId
		inner join Person personPledge on personPledge.id = paPledge.PersonId
		where personPledge.GivingId = person2.GivingId
		and cast(pledge.StartDate as date) >= @start
	) as pledgeAmount
	WHERE cast(ft.TransactionDateTime as date) BETWEEN @start and @end
	and pledgeAmount.TotalAmount is null
	and person2.RecordStatusValueId = 3
	group by 	
	personHOH.FirstName 
	, personHOH.LastName 
	, personHOH.Id 
	, personHOH.GivingId
	, personSpouse.FirstName 
	, c.Name
	, personHOH.Email
	, person2.GivingId
	, pledgeAmount.TotalAmount
	order by personHOH.LastName 