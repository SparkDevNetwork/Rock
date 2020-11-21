DECLARE @keepUpdating INT = 1

-- loop just in case the rckipid is in the URL multiple times
-- use 'rckipid_' to indicate which rows where updated in previous loops, then to a sweep and change them back to 'rckipid'
WHILE @keepUpdating > 0
BEGIN
    UPDATE PageView
    SET Url = stuff(Url, patindex('%rckipid=%', Url), CASE 
                WHEN charindex('&', SUBSTRING(Url, patindex('%rckipid=%', Url), 500)) > 0
                    THEN charindex('&', SUBSTRING(Url, patindex('%rckipid=%', Url), 500)) - 1
                ELSE 500
                END, 'rckipid_=XXXXXXXXXXXXXXXXXXXXXXXXXXXX')
    WHERE Url LIKE '%rckipid=%'

    SET @keepUpdating = @@ROWCOUNT
END

UPDATE PageView
SET Url = REPLACE(Url, 'rckipid_=', 'rckipid=')
WHERE Url LIKE '%rckipid_=%'
