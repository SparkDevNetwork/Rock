/* Helps code gen a **Service.partial.cs CanDelete method based on an entity's fk relationships */
/* NOTE: change the CHANGEME!!! param to the table of interest :) */

declare @tableName sysname = 'BinaryFile';  /* <-- CHANGEME!!! */

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
where [reftable] = @tableName
order by [parent_table]



