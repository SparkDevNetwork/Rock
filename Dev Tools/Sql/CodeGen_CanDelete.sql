/* Helps code gen a **Service.partial.cs CanDelete method based on an entity's fk relationships */
/* NOTE: change the CHANGEME!!! param to the table of interest :) */

select '
public bool CanDelete( int id, out string errorMessage )
{
    RockContext context = new RockContext();
    context.Database.Connection.Open();
    bool canDelete = true;
    errorMessage = string.Empty;
' [begincode]

select CONCAT('
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
}') [innercode] from (
select 
  OBJECT_NAME([fk].[parent_object_id]) [parent_table], 
  OBJECT_NAME([fk].[referenced_object_id]) [reftable], 
  [cc].[name] [column_name]
from 
sys.foreign_key_columns [fkc]
join sys.foreign_keys [fk]
on fkc.constraint_object_id = fk.object_id
join sys.columns cc
on fkc.parent_column_id = cc.column_id
where cc.object_id = fk.parent_object_id
) sub
where [reftable] = 'crmGroupType' /* <---- CHANGEME!!!  */

select 'return canDelete;
}'




