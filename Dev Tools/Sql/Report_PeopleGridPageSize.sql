SELECT av.[Value]
    , av.[EntityId]
    , av.[ModifiedDateTime]
    , p.[NickName]
    , p.[LastName]
    , a.[Key]
    , b.[Name] [Block.Name]
    , pg.[InternalName] AS [Page.Name]
FROM [Attribute] a
JOIN [AttributeValue] av ON av.[AttributeId] = a.[Id]
JOIN [Person] p ON p.[Id] = av.[EntityId]
JOIN [Block] b ON b.[Id] = TRY_CAST(SUBSTRING(a.[Key], PATINDEX('%[0-9]%', a.[Key]), LEN(a.[Key])) AS INT)
JOIN [Page] pg ON pg.Id = b.PageId
WHERE a.[Key] LIKE '%grid-page-size-preference%'
ORDER BY [Value] DESC
    , p.[FirstName]
    , p.[LastName]
    , b.[Name]
