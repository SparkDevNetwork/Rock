CREATE FUNCTION [dbo].[ufnUtility_GetAttributesAsXML] (
    @EntityTypeId INT
    ,@EntityId INT
    )
RETURNS NVARCHAR(max)
AS
BEGIN
    RETURN (
            SELECT a.Id
                ,a.[Key]
                ,av.Value
            FROM AttributeValue av
            JOIN Attribute a ON av.AttributeId = a.Id
                AND av.EntityId = @EntityId
                AND a.EntityTypeId = @EntityTypeId
            FOR XML AUTO
                ,ROOT('AttributeValue')
            );
END
