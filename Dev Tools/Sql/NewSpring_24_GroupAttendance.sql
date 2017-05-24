/* ====================================================== 
-- NewSpring Script #24: 
-- Update group attendance to the new structure

   ====================================================== */

IF object_id('tempdb..#newGroups') IS NOT NULL
BEGIN
	drop table #newGroups
END

select *
into #newGroups 
from (
	select row_number() over ( order by ogt.name ) Id, ogt.Name OldGroupType, og.id OldGroupId, 
	og.name OldGroup, ogl.Id OldLocationId, ogl.name OldLocation, c.Name CampusName, 
	gt.Name GroupTypeName, ng.Id GroupId, ng.Name GroupName, ng.CampusId
	from [group] og
	inner join grouptype ogt
	on og.grouptypeid = ogt.id
	and ogt.name not like 'NEW %'	
	inner join grouplocation gl
	on og.id = gl.groupid
	and og.isactive = 1	
	inner join location ogl
	on gl.LocationId = ogl.id
	and ogl.name is not null	
	inner join [group] ng
	on ng.name = ogl.name
	inner join grouptype gt
	on ng.grouptypeid = gt.id
	and gt.name like 'NEW %'
	inner join grouplocation ngl
	on ng.id = ngl.groupid
	and ogl.id = ngl.LocationId
	inner join campus c
	on ng.campusid = c.id
) g

declare @scopeIndex int, @numItems int, @msg nvarchar(max)
declare @OldGroupId int, @LocationId int, @GroupId int, @CampusId int, 
	@CampusName nvarchar(50), @GroupName nvarchar(50), @GroupTypeName nvarchar(50)

select @scopeIndex = min(ID) from #newGroups
select @numItems = count(1) + @scopeIndex from #newGroups

while @scopeIndex <= @numItems
begin
	select @OldGroupId = null, @LocationId = null, @GroupId = null, @CampusId = null, 
		@CampusName = null, @GroupName = null, @GroupTypeName = null, @msg = null
	
	select @OldGroupId = OldGroupId, @LocationId = OldLocationId, @GroupId = GroupId,
		@CampusId = CampusId, @CampusName = CampusName, @GroupName = GroupName, @GroupTypeName = GroupTypeName
	from #newGroups
	where Id = @scopeIndex

	select @msg = 'Updating ' + @CampusName + ' / ' + @GroupTypeName + ' / ' + @GroupName
	RAISERROR ( @msg, 0, 0 ) WITH NOWAIT
	
	update a
	set groupid = @GroupId
	from attendance a
	where GroupId = @OldGroupId
	and LocationId = @LocationId
	and CampusId = @CampusId
	
	set @scopeIndex = @scopeIndex + 1
end