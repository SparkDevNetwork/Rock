select 
    t.[name] [TableName],
	c.[name] [ColumnName],
	cc.is_persisted,
	'ALTER TABLE ' + t.name + ' ADD [' + c.name + '] AS ' + cc.definition + ';' [CreateScript]
from 
    sys.computed_columns cc
inner join 
    sys.columns c on cc.column_id = c.column_id and cc.object_id = c.object_id
inner join 
    sys.tables t on t.object_id = c.object_id
where SCHEMA_NAME(t.schema_id) = 'dbo'
