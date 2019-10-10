-- Set this to the number of records to include in the JSON output.  The API documentation says to do 1000, but there isn't a hard limit
declare @maxNumberOfRecords int = 1000


declare @crlf varchar(2) = char(13) + char(10)
declare @output varchar(max) = ''

select count(*) from Attendance
select count(*), PersonAliasId from Attendance group by PersonAliasId

select TOP (@maxNumberOfRecords) @output += concat(
	'      {',
    '"GroupId" : ', o.GroupId, ',',
	'"LocationId" : ', o.LocationId, ',',
	'"ScheduleId" : ', o.ScheduleId, ',',
	'"OccurrenceDate" : "', convert(varchar(33),dateadd(month, 12, o.OccurrenceDate) , 127), '",',
	'"StartDateTime" : "', convert(varchar(33),dateadd(month, 12, a.StartDateTime), 127), '",',
	'"PersonId" : ', pa.PersonId, ',',
    '"PersonAliasId" : ', pa.Id,
	'},
')
from 
Attendance a
join AttendanceOccurrence o on a.OccurrenceId = o.Id
join PersonAlias pa on a.PersonAliasId = pa.Id and a.PersonAliasId is not null


select 1 as tag,
   null as parent,concat('
   
#####STARTCOPY#####
   
',  '{
  "Attendances": 
    [
' + @output
	  + '    ]
}' , '
      
      
#####ENDCOPY#####') [str!1!!cdata] for xml explicit


