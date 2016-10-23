/* ====================================================== 
-- NewSpring Script #777: 
-- Copies attributes from old group members to new group members
  
--  Assumptions:
--  Existing metrics structure exists according to script 7:

   ====================================================== */
-- Make sure you're using the right Rock database:

--USE Rock

-- Set common variables 
DECLARE @IsSystem bit = 0
DECLARE @True bit = 1
DECLARE @False bit = 0
DECLARE @Order int = 0
DECLARE @CreatedDateTime AS DATETIME = GETDATE();
DECLARE @foreignKey AS NVARCHAR(15) = 'Metrics 2.0';

-- Entity Type Ids
DECLARE @etidGroup AS INT = (SELECT Id FROM EntityType WHERE Name = 'Rock.Model.Group');
DECLARE @attributeIdAge AS INT = (SELECT Id FROM Attribute WHERE EntityTypeId = @etidGroup AND [Key] = 'AgeRange');
DECLARE @attributeIdGrade AS INT = (SELECT Id FROM Attribute WHERE EntityTypeId = @etidGroup AND [Key] = 'GradeRange');

/* ====================================================== */
-- create the location matchup
/* ====================================================== */
IF object_id('tempdb..#allLocations') IS NOT NULL
BEGIN
	drop table #allLocations
END

select *
into #allLocations
from (
	SELECT
	lev01.[id] campusLocationId, lev01.name campusLocationName,
	lev02.[id] ministryLocationId, lev02.name ministryLocationName,
	lev03.[id] groupLocationId, lev03.name groupLocationName,
	lev04.[id] childLocationId, lev04.name childLocationName
	FROM
	location lev01
	LEFT OUTER JOIN location lev02 
	ON lev01.[id] = lev02.parentlocationid 
	AND lev02.name is not null
	LEFT OUTER JOIN location lev03 
	ON lev02.[id] = lev03.parentlocationid 
	AND lev03.name is not null
	LEFT OUTER JOIN location lev04 
	ON lev03.[id] = lev04.parentlocationid 
	AND lev04.name is not null
	WHERE
	lev01.parentlocationid IS NULL 
	AND lev01.name is not null
) l

/* ====================================================== */
-- create the group matchup
/* ====================================================== */
IF object_id('tempdb..#newGroups') IS NOT NULL
BEGIN
	drop table #newGroups
END

select *
into #newGroups 
from (
	select c.id CampusId, c.name CampusName, gt.Id GroupTypeId, gt.Name GroupTypeName, gt.DefaultGroupRoleId, pg.id ParentId, pg.name ParentName, g.id GroupId, g.name GroupName, l.id LocationId, l.name LocationName
	from [group] g
	inner join [group] pg
	on g.ParentGroupId = pg.id
	and pg.ParentGroupId is not null
	inner join grouptype gt
	on pg.grouptypeid = gt.id
	and gt.name like 'NEW %'
	inner join campus c
	on g.campusid = c.id
	left join grouplocation gl
	on g.id = gl.groupid
	left join location l
	on gl.LocationId = l.id
) g


/* ====================================================== */
-- create the group matchup
/* ====================================================== */
IF object_id('tempdb..#matchup') IS NOT NULL
BEGIN
	drop table #matchup
END

select * 
into #matchup
from (
	select gm.Id GroupMemberId, g.GroupTypeId OldGroupTypeId, gm.GroupId OldGroupId, gm.GroupRoleId, gtr.Name GroupRoleName, gm.personid, ng.GroupTypeId, ng.GroupId, ng.GroupName, ng.DefaultGroupRoleId, row_number() over 
		( partition by g.GroupTypeId, gm.GroupId, gm.personid, ng.GroupTypeId, ng.GroupId, ng.DefaultGroupRoleId order by gm.Id ) as membershipRole
	from [group] g
	inner join grouptype gt
	on g.grouptypeid = gt.id
	and (gt.name like '%attendee' or gt.name like '%volunteer')
	and gt.name not like 'new%'
	and gt.name not like 'event%'
	and g.name <> gt.name
	and g.isactive = 1
	inner join [groupmember] gm
		on gm.groupid = g.id
	inner join grouptyperole gtr
		on gm.grouproleid = gtr.id
		and gt.id = gtr.grouptypeid
	inner join attribute a
		on a.EntityTypeQualifierColumn = 'GroupTypeId'
		and a.EntityTypeQualifierValue = g.grouptypeid
		and a.Name = 'Campus'
	inner join attributevalue av
		on av.attributeid = a.id
		and av.entityid = gm.id
		and av.value <> ''
	inner join campus c
		on convert(uniqueidentifier, av.value) = c.[guid]
	inner join grouplocation gl
		on g.id = gl.groupid
	inner join #allLocations al
		on gl.locationid = al.groupLocationId
		and c.name = al.campusLocationName
	inner join #newGroups ng
		on ng.CampusId = c.id
		and ng.groupname = al.groupLocationName
		and ng.locationid = al.groupLocationId
) s
where membershipRole = 1


/* ====================================================== */
-- create the group members
/* ====================================================== */
begin transaction
insert groupmember (issystem, groupid, personid, grouproleid, groupmemberstatus, guid, CreatedDateTime, ModifiedDateTime, CreatedByPersonAliasId, ModifiedByPersonAliasId, ForeignKey, DateTimeAdded, note)
select 0, m.GroupId, gm.personid, ISNULL(gtr.id, m.DefaultGroupRoleId), gm.GroupMemberStatus, newid(), gm.CreatedDateTime, gm.ModifiedDateTime, gm.CreatedByPersonAliasId, gm.ModifiedByPersonAliasId, 'GroupMember16', gm.DateTimeAdded, gm.note
from #matchup m
inner join groupmember gm
on m.GroupMemberId = gm.id
left join grouptyperole gtr
on m.grouptypeid = gtr.id
and m.GroupRoleName = gtr.name

--commit transaction

/* ====================================================== */
-- insert schedule attributes
/* ====================================================== */
--begin transaction
insert attributevalue (issystem, attributeid, entityid, value, [guid], CreatedDateTime, ModifiedDateTime, CreatedByPersonAliasId, ModifiedByPersonAliasId, ForeignKey)
select 0, a.id, gm.id, av.value, newid(), av.CreatedDateTime, av.ModifiedDateTime, av.CreatedByPersonAliasId, av.ModifiedByPersonAliasId, 'GroupMemberAttribute16'
--select m.*, oa.id, oa.name, a.id, a.name
from #matchup m
inner join groupmember gm
	on m.personid = gm.personid
	and m.groupid = gm.groupid
	and gm.GroupMemberStatus = 1
	and m.DefaultGroupRoleId = gm.GroupRoleId
-- get the old attribute
inner join attribute oa
	on m.oldgrouptypeid = oa.EntityTypeQualifierValue
	and oa.EntityTypeQualifierColumn = 'GroupTypeId'
	and oa.Name = 'Schedule'
-- get the new attribute
inner join attribute a
	on m.GroupTypeId = a.EntityTypeQualifierValue
	and a.EntityTypeQualifierColumn = 'GroupTypeId'
	and a.Name = 'Schedule'
inner join attributevalue av
	on oa.id = av.AttributeId
	and av.EntityId = m.GroupMemberId
	and av.value <> ''

--commit transaction
--rollback transaction


/* ====================================================== */
-- insert team connector attributes
/* ====================================================== */
--begin transaction
insert attributevalue (issystem, attributeid, entityid, value, [guid], CreatedDateTime, ModifiedDateTime, CreatedByPersonAliasId, ModifiedByPersonAliasId, ForeignKey)
select 0, a.id, gm.id, oav.value, newid(), oav.CreatedDateTime, oav.ModifiedDateTime, oav.CreatedByPersonAliasId, oav.ModifiedByPersonAliasId, '07/16 Grestructure'
--select m.*, oa.id, oa.name, a.id, a.name, oav.value, av.value
from #matchup m
inner join groupmember gm
	on m.personid = gm.personid
	and m.groupid = gm.groupid
	and gm.GroupMemberStatus = 1
	and m.DefaultGroupRoleId = gm.GroupRoleId
-- get the old attribute
inner join attribute oa
	on m.oldgrouptypeid = oa.EntityTypeQualifierValue
	and oa.EntityTypeQualifierColumn = 'GroupTypeId'
	and oa.Name = 'Team Connector'
-- get the new attribute
inner join attribute a
	on m.GroupTypeId = a.EntityTypeQualifierValue
	and a.EntityTypeQualifierColumn = 'GroupTypeId'
	and a.Name = 'Team Connector'
inner join attributevalue oav
	on oa.id = oav.AttributeId
	and oav.EntityId = m.GroupMemberId
	and oav.value <> ''
left join attributevalue av
	on a.id = av.AttributeId
	and av.EntityId = m.GroupMemberId
	and av.value <> ''

--commit transaction
--rollback transaction

-- DELETE FROM AttributeValue WHERE ForeignKey = '07/16 Grestructure'