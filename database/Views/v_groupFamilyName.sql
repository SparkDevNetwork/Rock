create view v_groupFamilyName as
select 
	g.Id [GroupId], 
	pn.PersonNames [FamilyName], 
	g.Name [GroupName] 
from [Group] [g]
cross apply [ufn_person_group_to_person_names](null, g.Id) [pn]

