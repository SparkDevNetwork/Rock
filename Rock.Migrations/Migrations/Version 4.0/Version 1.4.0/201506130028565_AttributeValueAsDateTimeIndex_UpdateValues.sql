update AttributeValue set Value = Value WHERE CASE WHEN len(value) < 50 and isnull(value,'') != '' and isnumeric([value]) = 0 THEN
        ISNULL(TRY_CONVERT([datetime], TRY_CONVERT([datetimeoffset], left([value], (19)), 126)), TRY_CONVERT(DATETIME, [value], 101))
    END is not NULL

