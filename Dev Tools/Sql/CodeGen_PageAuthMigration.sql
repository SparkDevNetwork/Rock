declare
@EntityTypeIdPage int = 2

select 
    p.InternalName + char(9) [Page],
    a.[Order],
    a.[Action],
    a.AllowOrDeny,
    a.SpecialRole,
    concat(g.[Guid], ' ( ', g.Name, ' ), ') [Group],
    a.[Guid]
    
 from Auth a
left join [Group] g on a.GroupId = g.Id
join [Page] p on a.EntityId = p.Id
where EntityTypeId = @EntityTypeIdPage
order by a.ModifiedDateTime desc