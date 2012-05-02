
DECLARE @AdminGroupId int
SELECT @AdminGroupId = [Id] from groupsGroup where [Guid] = '628C51A8-4613-43ED-A18D-4A6FB999273E'


delete cmsAuth
where entityType in ('Cms.Page', 'Cms.BlockInstance')

insert into cmsAuth (
	EntityType,
	EntityId,
	[Order],
	AllowOrDeny,
	SpecialRole,
	Action,
	Guid,
	GroupId)
select 
	'CMS.Page',
	p.id,
	0,
	'A',
	0,
	'Configure',
	NEWID(),
	@AdminGroupId
from cmsPage p

insert into cmsAuth (
	EntityType,
	EntityId,
	[Order],
	AllowOrDeny,
	SpecialRole,
	Action,
	Guid,
	GroupId )
select 
	'CMS.Page',
	p.id,
	0,
	'A',
	0,
	'Edit',
	NEWID(),
	@AdminGroupId
from cmsPage p

insert into cmsAuth (
	EntityType,
	EntityId,
	[Order],
	AllowOrDeny,
	SpecialRole,
	Action,
	Guid,
	GroupId )
select 
	'CMS.BlockInstance',
	bi.id,
	0,
	'A',
	0,
	'Configure',
	NEWID(),
	@AdminGroupId
from cmsBlockInstance bi

insert into cmsAuth (
	EntityType,
	EntityId,
	[Order],
	AllowOrDeny,
	SpecialRole,
	Action,
	Guid,
	GroupId )
select 
	'CMS.BlockInstance',
	bi.id,
	0,
	'A',
	0,
	'Edit',
	NEWID(),
	@AdminGroupId
from cmsBlockInstance bi

