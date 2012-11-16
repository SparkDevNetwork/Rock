/* Ad Hoc code gen to help rename foreign keys to correct naming conventions. Handy for a migration. */

/* Up */
SELECT 
	concat('Sql( @"sp_rename ''[', name, ']''',', ''FK_', OBJECT_NAME(k.parent_object_id) , '_' , OBJECT_NAME(k.referenced_object_id) , '_', COL_NAME(c.parent_object_id, c.parent_column_id), ''' ");') [Up]
FROM 
	SYS.foreign_keys k
join
	sys.foreign_key_columns c
on c.constraint_object_id = k.object_id
WHERE 
	OBJECT_NAME(k.parent_object_id) like 'crmGroup%'
or	 
	OBJECT_NAME(k.referenced_object_id)  like 'crmGroup%'
ORDER BY K.name

/* Down */
SELECT 
	concat('Sql( @"sp_rename ''', '[FK_', OBJECT_NAME(k.parent_object_id) , '_' , OBJECT_NAME(k.referenced_object_id) , '_', COL_NAME(c.parent_object_id, c.parent_column_id), ']'',''', name, ''' ");') [Down]
FROM 
	SYS.foreign_keys k
join
	sys.foreign_key_columns c
on c.constraint_object_id = k.object_id
WHERE 
	OBJECT_NAME(k.parent_object_id) like 'crmGroup%'
or	 
	OBJECT_NAME(k.referenced_object_id)  like 'crmGroup%'
ORDER BY K.name


