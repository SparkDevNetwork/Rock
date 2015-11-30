CREATE TRIGGER [dbo].[tgrAttributeValue_InsertUpdate]
   ON  [dbo].[AttributeValue]
   AFTER INSERT, UPDATE
AS 
BEGIN

    UPDATE [AttributeValue] SET ValueAsDateTime = 
		CASE WHEN 
			LEN(value) < 50 and 
			ISNULL(value,'') != '' and 
			ISNUMERIC([value]) = 0 THEN
				CASE WHEN [value] LIKE '____-__-__T%__:__:%' THEN 
					ISNULL( TRY_CAST( TRY_CAST( LEFT([value],19) AS datetimeoffset ) as datetime) , TRY_CAST( value as datetime ))
				ELSE
					TRY_CAST( [value] as datetime )
				END
		END
    WHERE [Id] IN ( SELECT [Id] FROM INSERTED )    

END