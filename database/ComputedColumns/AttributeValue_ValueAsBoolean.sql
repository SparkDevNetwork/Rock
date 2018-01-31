ALTER TABLE AttributeValue
DROP COLUMN [ValueAsBoolean]
GO

ALTER TABLE AttributeValue ADD [ValueAsBoolean] AS (
    CASE 
        WHEN (Value IS NULL)
            OR (Value = '')
            OR (len(Value) > len('false'))
            THEN NULL
        WHEN lower(Value) IN (
                'true'
                ,'yes'
                ,'t'
                ,'y'
                ,'1'
                )
            THEN convert(BIT, 1)
        ELSE convert(BIT, 0)
        END
    ) PERSISTED
GO


CREATE NONCLUSTERED INDEX [IX_ValueAsBoolean] ON [dbo].[AttributeValue]
(
	[ValueAsBoolean] ASC
)
GO

