SELECT
	'            Sql( @"' + CHAR(10) + '    ' + 
	'sp_rename ''' + 
		case when [type] = 'U' THEN [name] ELSE 'dbo.[' + [name] + ']' end + ''', ' +
	'''' + REPLACE([name],'com_ccvonline','church_ccv') + '''' + CHAR(10) +
	'" );'
from sys.all_objects 
where name like '%Residency%'
order by [name]
