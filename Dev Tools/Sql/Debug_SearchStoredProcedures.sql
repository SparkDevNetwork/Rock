DECLARE @Search varchar(255)
SET @Search='2519'

SELECT DISTINCT
    o.name AS Object_Name,o.type_desc
    FROM sys.sql_modules        m 
        INNER JOIN sys.objects  o ON m.object_id=o.object_id
    WHERE m.definition Like '%'+@Search+'%'
    ORDER BY 2,1