declare
@EntityTypeIdPage int = 2

select 
    p.[InternalName] [Page],
    a.[Order],
    a.[Action],
    a.AllowOrDeny,
    a.SpecialRole,
    case a.SpecialRole when 0 then
        concat(g.[Guid], ' ( ', g.Name, ' ), ')
        when 1 then
            '<all users>'
            when 2 then
            '<all authenticated users>'
            when 3 then
            '<all un-authenticated users>'
        end
         [Group],
    a.[Guid]
    
 from Auth a
left join [Group] g on a.GroupId = g.Id
join [Page] p on a.EntityId = p.Id
where EntityTypeId = @EntityTypeIdPage
order by a.ModifiedDateTime desc


declare
@EntityTypeIdBlock int = 9

select 
    b.[Name] + char(9) [Block],
    a.[Order],
    a.[Action],
    a.AllowOrDeny,
    a.SpecialRole,
    case a.SpecialRole when 0 then
        concat(g.[Guid], ' ( ', g.Name, ' ), ')
        when 1 then
            '<all users>'
            when 2 then
            '<all authenticated users>'
            when 3 then
            '<all un-authenticated users>'
        end
         [Group],
    a.[Guid]
    
 from Auth a
left join [Group] g on a.GroupId = g.Id
join [Block] b on a.EntityId = b.Id
where EntityTypeId = @EntityTypeIdBlock
order by a.ModifiedDateTime desc