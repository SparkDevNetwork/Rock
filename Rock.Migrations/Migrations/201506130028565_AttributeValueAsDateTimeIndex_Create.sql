ALTER TABLE AttributeValue
DROP COLUMN ValueAsNumeric

ALTER TABLE AttributeValue
DROP COLUMN ValueAsDateTime

ALTER TABLE AttributeValue ADD [ValueAsNumeric] AS (
    CASE 
        WHEN LEN([value]) < (100)
            AND ISNUMERIC([value]) = (1)
            AND NOT [value] LIKE '%[^0-9.]%'
            AND NOT [value] LIKE '%[.]%'
            THEN TRY_CONVERT([numeric](38, 10), [value])
        END
    ) PERSISTED

ALTER TABLE AttributeValue ADD [ValueAsDateTime] AS ( 
    CASE WHEN ISNULL(value,'') != '' AND ISNUMERIC([value]) = 0 THEN
        ISNULL(TRY_CONVERT(datetime, TRY_CONVERT(datetimeoffset, left([value], 19), 126)), TRY_CONVERT(datetime, LEFT([value], 30), 101))
    END
) PERSISTED

CREATE INDEX IX_ValueAsDateTime on AttributeValue (ValueAsDateTime)
CREATE INDEX IX_ValueAsNumeric on AttributeValue (ValueAsNumeric)
