select 
    'ALTER TABLE ' + t.name + ' ADD [' + c.name + '] AS ' + cc.definition + ';'
from 
    sys.computed_columns cc
join 
    sys.columns c on cc.column_id = c.column_id
join 
    sys.tables t on t.object_id = c.object_id
where SCHEMA_NAME(t.schema_id) = 'dbo'

