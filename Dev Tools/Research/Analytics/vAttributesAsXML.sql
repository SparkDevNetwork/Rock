CREATE VIEW vAttributesAsXML
AS
SELECT a.EntityTypeId
    ,av.EntityId
    ,(
        SELECT ax.Id
            ,ax.[Key]
            ,avx.Value
        FROM AttributeValue avx
        JOIN Attribute ax ON avx.AttributeId = ax.Id
            AND avx.EntityId = av.EntityId
            AND ax.EntityTypeId = a.EntityTypeId
        FOR XML AUTO
            ,ROOT('AttributeValue')
        ) [AttributeValuesXML]
FROM AttributeValue av
JOIN Attribute a ON av.AttributeId = a.Id
WHERE a.EntityTypeId IS NOT NULL
    AND av.EntityId IS NOT NULL
GROUP BY a.EntityTypeId
    ,av.EntityId

