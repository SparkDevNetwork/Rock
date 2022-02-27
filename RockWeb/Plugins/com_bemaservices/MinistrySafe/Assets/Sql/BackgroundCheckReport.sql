Select p.Id, 
	p.NickName,
	p.LastName,
	g.Id as GroupId,
	g.Name as [GroupName],
	backgroundCheckResult.Value as 'Background Check Result',
	backgroundCheckDate.ValueAsDateTime as 'BackgroundCheckDate',
	'<a href="/Person/'+convert(nvarchar(max),p.Id)+'"><div class="btn btn-default btn-sm"><i class="fa fa-user"></i></div></a>' as [Person],
	'<a href="/page/113?GroupId='+convert(nvarchar(max),g.Id)+'"><div class="btn btn-default btn-sm"><i class="fa fa-group"></i></div></a>' as [Group]
From [Group] g
Join GroupType gt on g.GroupTypeId = gt.Id
Join GroupRequirement gr on (gr.GroupId = g.Id or gr.GroupTypeId = gt.Id)
Join GroupRequirementType grt on grt.Id = gr.GroupRequirementTypeId
Join GroupMember gm on gm.GroupId = g.Id
Join Person p on p.Id = gm.PersonId
Left Join AttributeValue backgroundCheckDate on backgroundCheckDate.EntityId = p.Id and backgroundCheckDate.AttributeId = (Select top 1 Id From Attribute Where Guid = '3DAFF000-7F74-47D7-8CB0-E4A4E6C81F5F')
Left Join AttributeValue backgroundCheckResult on backgroundCheckResult.EntityId = p.Id and backgroundCheckResult.AttributeId = (Select top 1 Id From Attribute Where Guid = '44490089-E02C-4E54-A456-454845ABBC9D')

Where grt.Name = 'Background Check Required'