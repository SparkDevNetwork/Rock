delete cmsAuth
where entityType in ('Cms.Page', 'Cms.BlockInstance')

insert into cmsAuth (
	EntityType,
	EntityId,
	[Order],
	AllowOrDeny,
	UserOrRole,
	Action,
	UserOrRoleName )
select 
	'CMS.Page',
	p.id,
	0,
	'A',
	'R',
	'Configure',
	'628c51a8-4613-43ed-a18d-4a6fb999273e'
from cmsPage p

insert into cmsAuth (
	EntityType,
	EntityId,
	[Order],
	AllowOrDeny,
	UserOrRole,
	Action,
	UserOrRoleName )
select 
	'CMS.Page',
	p.id,
	0,
	'A',
	'R',
	'Edit',
	'628c51a8-4613-43ed-a18d-4a6fb999273e'
from cmsPage p

insert into cmsAuth (
	EntityType,
	EntityId,
	[Order],
	AllowOrDeny,
	UserOrRole,
	Action,
	UserOrRoleName )
select 
	'CMS.BlockInstance',
	bi.id,
	0,
	'A',
	'R',
	'Configure',
	'628c51a8-4613-43ed-a18d-4a6fb999273e'
from cmsBlockInstance bi

insert into cmsAuth (
	EntityType,
	EntityId,
	[Order],
	AllowOrDeny,
	UserOrRole,
	Action,
	UserOrRoleName )
select 
	'CMS.BlockInstance',
	bi.id,
	0,
	'A',
	'R',
	'Edit',
	'628c51a8-4613-43ed-a18d-4a6fb999273e'
from cmsBlockInstance bi

