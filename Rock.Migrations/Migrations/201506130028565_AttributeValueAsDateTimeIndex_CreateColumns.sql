ALTER TABLE AttributeValue DROP COLUMN ValueAsNumeric

ALTER TABLE AttributeValue ADD [ValueAsNumeric] AS (
    CASE 
        WHEN len([value]) < (100)
            AND isnumeric([value]) = (1)
            AND NOT [value] LIKE '%[^0-9.]%'
            AND NOT [value] LIKE '%[.]%'
            THEN TRY_CONVERT([numeric](38, 10), [value])
        END
    ) persisted

CREATE INDEX IX_ValueAsNumeric on AttributeValue (ValueAsNumeric)


ALTER TABLE AttributeValue DROP COLUMN ValueAsDateTime
ALTER TABLE AttributeValue ADD ValueAsDateTime datetime;
CREATE INDEX IX_ValueAsDateTime on AttributeValue (ValueAsDateTime)