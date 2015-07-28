SELECT 
    av.EntityId [Person.Id],
	p.FirstName,
	p.LastName,
	a.NAME [Attribute.Name]
    ,av.Value [AttributeValue.Value]
FROM AttributeValue av
JOIN Attribute a ON av.AttributeId = a.Id
join Person p on av.EntityId = p.Id
WHERE a.EntityTypeId = (
        SELECT TOP 1 Id
        FROM EntityType
        WHERE NAME LIKE 'Rock.Model.Person.Value'
        )
