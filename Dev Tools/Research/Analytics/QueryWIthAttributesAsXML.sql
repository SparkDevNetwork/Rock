SELECT x.*
    ,(
        SELECT a.Id
            ,a.[Key]
            ,av.Value
        FROM AttributeValue av
        JOIN Attribute a ON av.AttributeId = a.Id
		--JOIN EntityType et on a.EntityTypeId = et.Id
          --  AND et.Name = 'Rock.Model.Block'
            AND av.EntityId = x.Id
			and a.EntityTypeId = 9
        FOR XML AUTO, ROOT('AttributeValue') 
        ) [XML]
FROM Block x

--select * from EntityType


