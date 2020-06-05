Select Distinct
	P.Id as 'BusinessId',
	P2.Id as 'ContactId',
	P.LastName as 'Business Name',
	P2.FirstName +' '+P2.LastName As [Contact Name], --Business Contact
	P.Email As [Email],
	Case When Concat(L.Street1,' ',L.Street2,', ',L.[City],', ',L.[State],', ',L.PostalCode) != ' , , , '
		Then Concat(L.Street1,' ',L.Street2,', ',L.[City],', ',L.[State],', ',L.PostalCode) 
		Else Null End As [Address],
	C.Name As [Campus]
From Person P
Inner Join GroupMember GM On P.Id = GM.PersonId
Inner JOIN [Group] G ON GM.[GroupId] = G.[Id] AND G.[GroupTypeId] = 11 -- Known Relationship
Inner Join GroupMember FM on P.Id = FM.PersonId
Inner Join [Group] F on FM.GroupId = F.Id and F.GroupTypeId = 10 -- Family Group Type
Left Outer JOIN [GroupLocation] GL ON F.[Id] = GL.[GroupId] And GL.GroupLocationTypeValueId = 20 -- Work Location
Left Outer JOIN [Location] L ON GL.[LocationId] = L.[Id]
Left Join GroupMember GM2 on GM2.GroupId = G.Id and GM2.GroupRoleId = 25
Left Join Person P2 on GM2.PersonId = P2.Id
Left Outer Join Campus C On F.CampusId = C.Id
Where P.RecordTypeValueId = 2 --Business
And GM.GroupRoleId = 5