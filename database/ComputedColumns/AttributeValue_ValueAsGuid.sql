ALTER TABLE AttributeValue

DROP COLUMN [ValueAsGuid]
GO

ALTER TABLE AttributeValue ADD [ValueAsGuid] AS (
    CASE 
        WHEN [Value] LIKE '________-____-____-____-____________'
            THEN try_convert(UNIQUEIDENTIFIER, Value)
        ELSE NULL
        END
    ) PERSISTED
GO


