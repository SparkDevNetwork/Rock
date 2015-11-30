UPDATE AttributeValue 
SET Value = Value 
WHERE 
	CASE WHEN 
		LEN(value) < 50 and 
		ISNULL(value,'') != '' and 
		ISNUMERIC([value]) = 0 THEN
			CASE WHEN [value] LIKE '____-__-__T%__:__:%' THEN 
				ISNULL( TRY_CAST( TRY_CAST( LEFT([value],19) AS datetimeoffset ) as datetime) , TRY_CAST( value as datetime ))
			ELSE
				TRY_CAST( [value] as datetime )
			END
	END IS NOT NULL


