/* Lists Indexes that don't follow the naming pattern of
 Prefix: 'IX_' +
 '_' delimited list of Columns in order by their position in the index, followed by any 'include' columns 
*/

SELECT *
FROM (
	SELECT 
        t.name [TableName], 
        ind.name [ActualIndexName], 
        'IX_' + STRING_AGG(col.name, '_') within
	        GROUP ( ORDER BY is_included_column, ic.key_ordinal, col.Name ) [StandardizedIndexName]
	FROM sys.indexes ind
	INNER JOIN sys.index_columns ic
		ON ind.object_id = ic.object_id AND ind.index_id = ic.index_id
	INNER JOIN sys.columns col
		ON ic.object_id = col.object_id AND ic.column_id = col.column_id
	INNER JOIN sys.tables t
		ON ind.object_id = t.object_id
	WHERE ind.is_primary_key = 0
	GROUP BY t.Name, ind.name
	) x
WHERE x.[ActualIndexName] != x.[StandardizedIndexName]
ORDER BY x.TableName, x.ActualIndexName