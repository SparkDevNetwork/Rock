
Declare @tableGivers table ( GivingID varchar(20), TotalGiving int, campus int) 
insert into @tableGivers 
select  person.GivingId, 
        sum(d.amount) as [Total], 
        campus.id 
from FinancialTransaction t 
inner join FinancialTransactionDetail d on d.TransactionId = t.id 
inner join PersonAlias pa on pa.id = t.AuthorizedPersonAliasId 
inner join Person person on person.Id = pa.PersonId 
outer apply ( 
    select  c.Name, 
            c.id 
    from GroupMember FM 
    left outer join [Group] F ON F.Id = FM.GroupId AND F.GroupTypeId = 10 
    left outer join Campus c on c.id = f.CampusId 
    where fm.PersonId = person.Id and c.id is not null 
    ) campus 
where cast(t.TransactionDateTime as date) > getdate()-120 
and campus.id is not null 
group by person.GivingId, campus.id 

Select  person.id, 
        person.FirstName + ' ' + person.LastName as [Person], 
        lastGivingDate.[LastGiftDate] as [Last Gift Date], 
        personC.Name as [Person Campus], 
        giftC.Name as [Gift Campus] 
from @tableGivers tg 
inner join Person person on person.id = dbo.ufnCrm_GetHeadOfHousePersonIdFromGivingId(tg.GivingID) 
OUTER APPLY ( 
    SELECT TOP (1)  cast(ft.TransactionDateTime as date) AS [LastGiftDate], 
                    fa2.CampusId 
    FROM FinancialTransaction ft 
    INNER JOIN FinancialTransactionDetail td on ft.id = td.TransactionId 
    INNER JOIN FinancialAccount fa2 on fa2.id = td.AccountId 
    INNER JOIN PersonAlias pa2 on pa2.Id = ft.AuthorizedPersonAliasId 
    INNER JOIN Person person2 on person2.id = pa2.PersonId AND person2.GivingId = person.GivingId 
    where fa2.CampusId is not null 
    and cast(ft.TransactionDateTime as date) > getdate()-120 
    ORDER BY ft.TransactionDateTime desc 
    ) as lastGivingDate 
join Campus personC on personC.id = tg.campus 
join Campus giftC on giftC.id = lastGivingDate.CampusId 
where lastGivingDate.CampusId != tg.campus