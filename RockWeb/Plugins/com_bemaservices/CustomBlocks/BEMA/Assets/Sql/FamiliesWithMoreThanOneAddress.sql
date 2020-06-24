With CTE AS ( 
    Select  g.Id, 
            Count(Distinct gl.Id ) as HomeAddresses 
    From [Group] g 
    Join [GroupLocation] gl on gl.GroupId = g.Id and gl.IsMailingLocation = 1
    Join [DefinedValue] dv on dv.Id = gl.GroupLocationTypeValueId and dv.Guid = '8C52E53C-2A66-435A-AE6E-5EE307D9A0DC' -- Only look at Home Locations 
    Where g.GroupTypeId = 10 
    Group By g.Id 
    Having Count( Distinct gl.Id ) > 1 
), CTE2 AS ( 
    Select  p.Id, 
            ROW_NUMBER() Over (Partition By p.PrimaryFamilyId Order by IIF(p.Id = p.GivingLeaderId ,0,1), p.Gender, [dbo].[ufnCrm_GetAge](p.Birthdate) Desc ) as [Rank], 
            p.PrimaryFamilyId 
    From Person p 
) 

Select  p.Id, 
        p.LastName + ' Family' as Family, 
        CTE.HomeAddresses as NumberOfAddresses 
From [Group] g 
Join CTE on g.Id = CTE.Id 
Join CTE2 CTE2 on CTE2.PrimaryFamilyId = g.Id and CTE2.[Rank] = 1 
Join Person p on p.Id = CTE2.Id 
Order by g.Id 
/* ---- Block Settings --- Hide Columns: Id Select Url: /person/{Id} Person Report: Checked Show Grid Actions: Communicate, Merge Person, Bulk Update, Excel Export, Merge Template */