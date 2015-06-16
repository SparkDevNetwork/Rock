CREATE TRIGGER [dbo].[tgrAttributeValue_InsertUpdate]
   ON  [dbo].[AttributeValue]
   AFTER INSERT, UPDATE
AS 
BEGIN
    update [AttributeValue] set ValueAsDateTime = CASE WHEN len(value) < 50 and isnull(value,'') != '' and isnumeric([value]) = 0 THEN
        ISNULL(TRY_CONVERT([datetime], TRY_CONVERT([datetimeoffset], left([value], (19)), 126)), TRY_CONVERT(DATETIME, [value], 101))
    END where Id in (select Id from inserted)
END