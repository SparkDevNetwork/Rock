select 'alter table [' + TABLE_NAME + '] nocheck constraint all' [Code], 0 [ScriptOrder], [TABLE_NAME]  from INFORMATION_SCHEMA.TABLES where TABLE_NAME not in ( 'sysdiagrams')
union
select 'delete from [' + TABLE_NAME + ']'  [Code], 1 [ScriptOrder], [TABLE_NAME] from INFORMATION_SCHEMA.TABLES where TABLE_NAME not in ( 'sysdiagrams')
union
select 'alter table [' + TABLE_NAME + '] check constraint all' [Code], 2 [ScriptOrder], [TABLE_NAME] from INFORMATION_SCHEMA.TABLES where TABLE_NAME not in ( 'sysdiagrams')
order by ScriptOrder, TABLE_NAME


