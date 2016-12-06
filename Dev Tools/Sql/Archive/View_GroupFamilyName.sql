select 
	g.Id [GroupId], 
	pn.PersonNames [FamilyName], 
	g.Name [GroupName] 
from [Group] [g]
cross apply [ufnPersonGroupToPersonName](null, g.Id) [pn]

