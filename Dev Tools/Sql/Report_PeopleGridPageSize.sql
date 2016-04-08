SELECT av.Value
    ,av.EntityId
    ,av.ModifiedDateTime
    ,p.NickName
    ,p.LastName
    ,a.[Key]
    ,b.NAME [Block.Name]
    ,pg.InternalName [Page.Name]
FROM Attribute a
JOIN AttributeValue av ON av.AttributeId = a.Id
JOIN PersonAlias pa ON pa.Id = av.EntityId
JOIN Person p ON p.Id = pa.PersonId
JOIN Block b ON b.Id = try_cast(SUBSTRING(a.[Key], PATINDEX('%[0-9]%', a.[Key]), LEN(a.[Key])) AS INT)
JOIN [Page] pg ON pg.Id = b.PageId
WHERE a.[Key] LIKE '%grid-page-size-preference%'
ORDER BY Value DESC
    ,FirstName
    ,LastName
    ,b.NAME
