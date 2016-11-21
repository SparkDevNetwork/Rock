UPDATE GroupLocation
SET IsMappedLocation = 1
WHERE Id IN (
        SELECT gl.Id
        FROM [Group] g
        JOIN GroupLocation gl ON g.Id = gl.GroupId
        JOIN Location l ON gl.LocationId = l.Id
        WHERE gl.GroupLocationTypeValueId = 19
            AND l.GeocodedDateTime IS NOT NULL
            AND g.GroupTypeId = 10
            AND g.Id IN (
                SELECT GroupId
                FROM GroupLocation
                WHERE GroupLocationTypeValueId = 19
                )
            AND g.Id NOT IN (
                SELECT g.Id
                FROM Location l
                JOIN GroupLocation gl ON gl.LocationId = l.Id
                JOIN [Group] g ON g.Id = gl.GroupId
                JOIN DefinedValue glv ON gl.GroupLocationTypeValueId = glv.Id
                WHERE gl.GroupLocationTypeValueId = 19
                    AND gl.IsMappedLocation = 1
                )
        )
