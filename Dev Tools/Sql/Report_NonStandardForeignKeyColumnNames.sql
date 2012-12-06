-- Standard Rule filtering out one-offs like DefinedValue
select * from
(
select 
  OBJECT_NAME([fk].[parent_object_id]) [parent_table], 
  [cc].[name] [column_name],
  OBJECT_NAME([fk].[referenced_object_id]) [reftable], 
  [fk].[delete_referential_action] [CascadeAction]
from 
sys.foreign_key_columns [fkc]
join sys.foreign_keys [fk]
on fkc.constraint_object_id = fk.object_id
join sys.columns cc
on fkc.parent_column_id = cc.column_id
where cc.object_id = fk.parent_object_id
--and [fk].[delete_referential_action_desc] != 'CASCADE'
) sub
where [column_name] not like '%' + [reftable] + 'Id'
and [reftable] not in ('DefinedValue', 'BinaryFile')
order by [parent_table], [column_name]

-- OneOff:  DefinedValue
select * from
(
select 
  OBJECT_NAME([fk].[parent_object_id]) [parent_table], 
  [cc].[name] [column_name],
  OBJECT_NAME([fk].[referenced_object_id]) [reftable], 
  [fk].[delete_referential_action] [CascadeAction]
from 
sys.foreign_key_columns [fkc]
join sys.foreign_keys [fk]
on fkc.constraint_object_id = fk.object_id
join sys.columns cc
on fkc.parent_column_id = cc.column_id
where cc.object_id = fk.parent_object_id
--and [fk].[delete_referential_action_desc] != 'CASCADE'
) sub
where [reftable] = 'DefinedValue'
and [column_name] not like '%ValueId'
order by [parent_table], [column_name]

-- OneOff:  BinaryFile
select * from
(
select 
  OBJECT_NAME([fk].[parent_object_id]) [parent_table], 
  [cc].[name] [column_name],
  OBJECT_NAME([fk].[referenced_object_id]) [reftable], 
  [fk].[delete_referential_action] [CascadeAction]
from 
sys.foreign_key_columns [fkc]
join sys.foreign_keys [fk]
on fkc.constraint_object_id = fk.object_id
join sys.columns cc
on fkc.parent_column_id = cc.column_id
where cc.object_id = fk.parent_object_id
--and [fk].[delete_referential_action_desc] != 'CASCADE'
) sub
where [reftable] = 'BinaryFile'
and [column_name] not like '%FileId'
order by [parent_table], [column_name]






