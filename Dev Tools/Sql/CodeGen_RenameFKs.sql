/* Ad Hoc code gen to help rename foreign keys to correct naming conventions. Handy for a migration. */

/* Up */
SELECT 
	concat('Sql( @"sp_rename ''[', name, ']''',', ''' 
      + substring('FK_dbo.' + OBJECT_NAME(k.parent_object_id) + '_dbo.' + OBJECT_NAME(k.referenced_object_id) + '_' + COL_NAME(c.parent_object_id, c.parent_column_id), 1, 128) + ''' ");') [Up]
FROM 
	SYS.foreign_keys k
join
	sys.foreign_key_columns c
on c.constraint_object_id = k.object_id
WHERE 
	OBJECT_NAME(k.parent_object_id) like '_com_ccvonline_Residency%'
or	 
	OBJECT_NAME(k.referenced_object_id)  like '_com_ccvonline_Residency%'
ORDER BY K.name


--sp_rename '[FK_dbo._com_ccvonline_ResidencyCompetency_dbo._com_ccvonline_ResidencyTrack_ResidencyTrackId]', '[FK_dbo._com_ccvonline_Residency_Competency_dbo._com_ccvonline_Residency_Track_TrackId]';


/* Down */
SELECT 
	concat('Sql( @"sp_rename ''', '[' + substring('FK_dbo.' + OBJECT_NAME(k.parent_object_id) + '_dbo.' + OBJECT_NAME(k.referenced_object_id) + '_' + COL_NAME(c.parent_object_id, c.parent_column_id), 1, 128) + ']'',''', name, ''' ");') [Down]
FROM 
	SYS.foreign_keys k
join
	sys.foreign_key_columns c
on c.constraint_object_id = k.object_id
WHERE 
	OBJECT_NAME(k.parent_object_id) like '_com_ccvonline_Residency%'
or	 
	OBJECT_NAME(k.referenced_object_id)  like '_com_ccvonline_Residency%'
ORDER BY K.name


