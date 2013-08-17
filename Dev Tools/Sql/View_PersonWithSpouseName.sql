-- This will return all Person records and include the Person's Spouse's name (if they have a spouse)

with AdultFamilyMembersCTE as
(
  select p.*, gm.PersonId, gm.GroupId 
  from [GroupMember] [gm]
  join [Group] [g]
  on [gm].GroupId = g.Id
  join [GroupType] [gt]
  on g.GroupTypeId = gt.Id
  join [Person] p
  on [gm].PersonId = [p].[Id]
  left outer join [GroupRole] [gr]
  on [gm].GroupRoleId = gr.Id
  where gt.Guid = '790E3215-3B10-442B-AF69-616C0DCB998E'  -- Family Group
  and gr.Guid = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42' -- Group Role Adult  
)

select 
   [p].*, 
   [spouse].[Id] [SpouseNamePersonId],
   [spouse].[FullNameLastFirst] [SpouseName] 
from Person [p]
left outer join [AdultFamilyMembersCTE] [pa]
on [pa].PersonId = p.Id
left outer join [AdultFamilyMembersCTE] [spouse]
on [spouse].GroupId = pa.GroupId
and [spouse].PersonId != pa.PersonId
and [spouse].Gender != pa.Gender
--and [Person].MaritalStatusValueId = (select Id from DefinedValue where Guid = 'ToDo: MarriedGuid') 
--and [Spouse].MaritalStatusValueId = (select Id from DefinedValue where Guid = 'ToDo: MarriedGuid') 

