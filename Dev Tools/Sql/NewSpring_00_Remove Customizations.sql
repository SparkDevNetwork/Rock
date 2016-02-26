/* ====================================================== */
-- NewSpring Script #0:
-- Blanks out non-core grouptypes and groups 

-- Make sure you're using the right Rock database:

USE Rock

/* ====================================================== */

-- Enable production mode for performance
SET NOCOUNT ON

declare @True bit = 1
declare @False bit = 0

-- gather grouptypes to delete
if object_id('tempdb..#grouptypes') is not null
begin
	drop table #grouptypes
end
create table #grouptypes (
	ID int,
	areaName nvarchar(255)
)

insert #grouptypes
select ID, Name
from grouptype
where issystem = @False
and name not like 'Check in%'
and name not like 'Small Group%'
and name not like 'Serving Team%'
and name not like 'General Group%'

if object_id('tempdb..#groups') is not null
begin
	drop table #groups
end
create table #groups (
	ID int,
	name nvarchar(255)
)

insert #groups
select ID, Name
from [group]
where issystem = @False
and grouptypeid in (
	select id from #grouptypes
)

-- delete attendance
truncate table attendance

-- run this in a loop for large attendance tables
/*===============================================

delete from attendance 
where id in (
	select top 500000 id
	from attendance a
	where a.groupid in (
		select id from #groups
	)
)

=================================================*/

-- delete group members
delete gm 
from groupmember gm
inner join [group] g
on gm.groupid = g.id
and g.id in (
	select id from #groups
)

-- delete locations (and group locations)
delete l
from location l
inner join grouplocation gl
on l.id = gl.locationid
and gl.groupid in (
	select id from #groups
)

-- delete group attributes
delete from [attribute]
where entitytypeid = 90
and EntityTypeQualifierValue in (
	select id from #grouptypes
)

-- delete groups
delete from [group]
where parentgroupid in  (
	select id from #groups
)

delete from [group]
where id in  (
	select id from #groups
)

-- delete from grouptypes
delete GroupTypeAssociation
where grouptypeid in (
	select id from #grouptypes
)

delete grouptype
where id in (
	select id from #grouptypes
)

select isnull(lc.id, l.Id) as 'LocationId'
into #locations
from campus c
inner join location l
	on c.LocationId = l.ParentLocationId
left join location lc
	on lc.ParentLocationId = l.Id

-- delete child locations first
delete from location
where id in (
	select LocationId from #locations
)

insert #locations
select LocationId from campus

delete from campus
where id > 1

-- delete campus locations
delete from location
where id in (
	select LocationId from #locations
)


use master