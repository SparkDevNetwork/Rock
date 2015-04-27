select [Code] from (
select concat('
/// <summary>
/// Gets the ', p.InternalName  ,' page guid
/// </summary>
public const string ', Replace(Replace(Upper(p.InternalName), ' ', '_'),'-', '_'),'= "', p.[Guid], '";', '') [Code],
p.InternalName
from [Page] [p]
left outer join [Page] pp on p.ParentPageId = pp.id
 where (p.IsSystem = 1 or p.[Guid] in ('1AFDA740-8119-45B8-AF4D-58856D469BE5', 'B13FCF9A-FAE5-4E53-AF7C-32DF9CA5AAE3'))
 and p.Id in (select Id from (select count(*) [count], [InternalName], max([Id]) [Id] from [Page] group by [InternalName]) pc where pc.[count] = 1)


union


select concat('
/// <summary>
/// Gets the ', p.InternalName  ,' page guid
/// ParentPage: ', pp.InternalName, '
/// </summary>
public const string ', Replace(Replace(Upper(p.InternalName + '_' + ISNULL(pp.InternalName, 'ROOT')), ' ', '_'),'-', '_'),'= "', p.[Guid], '";', '') [Code],
p.InternalName
from [Page] [p]
left outer join [Page] pp on p.ParentPageId = pp.id
 where (p.IsSystem = 1 or p.[Guid] in ('1AFDA740-8119-45B8-AF4D-58856D469BE5', 'B13FCF9A-FAE5-4E53-AF7C-32DF9CA5AAE3'))
 and p.Id not in (select Id from (select count(*) [count], [InternalName], max([Id]) [Id] from [Page] group by [InternalName]) pc where pc.[count] = 1)
 ) u
order by replace(InternalName, '(','')