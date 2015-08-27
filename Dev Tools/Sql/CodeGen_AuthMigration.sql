-- list of recently modified Auth records for Page, Block and EntityType to help write a Migration

declare
@EntityTypeIdPage int = 2

select 
    p.[InternalName] [Page],
	p.[Guid] [Page.Guid],
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
    a.[Guid] [Auth.Guid]
    
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
    a.[Guid] [Auth.Guid]
    
 from Auth a
left join [Group] g on a.GroupId = g.Id
join [Block] b on a.EntityId = b.Id
where EntityTypeId = @EntityTypeIdBlock
order by a.ModifiedDateTime desc

-- Entity Security
select 
    b.[Name] + char(9) [EntityType],
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
    a.[Guid] [Auth.Guid]
    
 from Auth a
left join [Group] g on a.GroupId = g.Id
join [EntityType] b on a.EntityTypeId = b.Id
where a.EntityId = 0
order by isnull(a.ModifiedDateTime, '1900-01-01') desc