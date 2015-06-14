ALTER TABLE AttributeValue

DROP COLUMN ValueAsNumeric

ALTER TABLE AttributeValue

DROP COLUMN ValueAsDateTime

ALTER TABLE AttributeValue ADD [ValueAsNumeric] AS (
    CASE 
        WHEN len([value]) < (100)
            AND isnumeric([value]) = (1)
            AND NOT [value] LIKE '%[^0-9.]%'
            AND NOT [value] LIKE '%[.]%'
            THEN TRY_CONVERT([numeric](38, 10), [value])
        END
    ) persisted

ALTER TABLE AttributeValue ADD [ValueAsDateTime] AS ( 
    CASE WHEN isnull(value,'') != '' and isnumeric([value]) = 0 THEN
        ISNULL(TRY_CONVERT([datetime], TRY_CONVERT([datetimeoffset], left([value], (19)), 126)), TRY_CONVERT(DATETIME, [value], 101))
    END
) persisted

CREATE INDEX IX_ValueAsDateTime on AttributeValue (ValueAsDateTime)
CREATE INDEX IX_ValueAsNumeric on AttributeValue (ValueAsNumeric)
