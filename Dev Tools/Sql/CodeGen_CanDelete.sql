/* Helps code gen a **Service.partial.cs CanDelete method based on an entity's fk relationships */
/* NOTE: change the CHANGEME!!! param to the table of interest :) */

declare @tableName sysname = 'GroupType';  /* <-- CHANGEME!!! */

select '
public bool CanDelete( int id, out string errorMessage )
{
    // partially code generated from Dev Tools/Sql/CodeGen_CanDelete.sql

    RockContext context = new RockContext();
    context.Database.Connection.Open();
    bool canDelete = true;
    errorMessage = string.Empty;' [beginCanDelete]

select CONCAT(case CascadeAction when 1 then '// NOTE: Cascade on delete.  Check might not be needed...' else null end, '
using ( var cmdCheckRef = context.Database.Connection.CreateCommand() )
{
    cmdCheckRef.CommandText = string.Format( "select count(*) from ', [parent_table], ' where ', [column_name], ' = {0} ", id );
    var result = cmdCheckRef.ExecuteScalar();
    int? refCount = result as int?;
    if ( refCount > 0 )
    {
        canDelete = false;
        errorMessage = "This ENTITYFRIENDLYNAME is assigned to a ', UPPER([parent_table]), '.";
    }
}') [innerCanDelete] from (
select 
  OBJECT_NAME([fk].[parent_object_id]) [parent_table], 
  OBJECT_NAME([fk].[referenced_object_id]) [reftable], 
  [cc].[name] [column_name],
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

select 'return canDelete; 
}' [endCanDelete]

select 'public override bool Delete( CHANGEME item, int? personId )
        {
            string message;
            if ( !CanDelete( item.Id, out message ) )
            {
                return false;
            }
            
            return base.Delete( item, personId );
        }' [deleteOverride]

