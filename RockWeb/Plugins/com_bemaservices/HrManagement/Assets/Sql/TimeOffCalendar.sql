{% assign supervisor = 'Global' | PageParameter:'Supervisor' | SanatizeSql %}
{% assign ministryArea = 'Global' | PageParameter:'MinistryArea' | SanatizeSql %}

;with CTE as (
Select
	r.RequestDate
	, PtoAllocationId
	, p.Id as PersonId
	, r.[Hours]
	,r.PtoRequestApprovalState
	, r.Id as RequestId
	, avr.ValueAsDateTime as [ReturnDate]
	, avc.[Value] as ContactWhileImOut
From _com_bemaservices_HrManagement_PtoRequest r
Join _com_bemaservices_HrManagement_PtoAllocation a on r.PtoAllocationId = a.Id
Join Person p on p.Id = dbo.ufnUtility_GetPersonIdFromPersonAlias( a.PersonAliasId )
Left Join AttributeValue avr on avr.EntityId = r.Id and avr.AttributeId = 9605 -- Return Date
Left Join AttributeValue avc on avc.EntityId = r.Id and avc.AttributeId = 9607 -- Contact While I'm Out
And r.PtoRequestApprovalState in (1)
)


Select
    '<a href="/Person/' + Cast(p.Id as nvarchar(11)) + '/HR">' + p.FirstName + ' ' + p.LastName + '</a>' as [PersonLink]
	, p.Id as PersonId
	,p.FirstName + ' ' + p.LastName as PersonName
	, RequestDate
	, [ReturnDate]
	,PtoRequestApprovalState
	, ContactWhileImOut
	, sp.FirstName + ' ' + sp.LastName as [Supervisor]
	, avm.Value as [MinistryArea]
From CTE
Join Person p on p.Id = CTE.PersonId
Left Join AttributeValue avs on avs.EntityId = p.Id and avs.AttributeId = 3754 -- Supervisor
Left Join AttributeValue avm on avm.EntityId = p.Id and avm.AttributeId = 6320 -- Ministry Area
Left Join Person sp on sp.Id = dbo.ufnUtility_GetPersonIdFromPersonAliasGuid( avs.Value )

WHERE 1=1

{% if supervisor != null and supervisor != empty %}
    AND sp.Id = dbo.ufnUtility_GetPersonIdFromPersonAliasGuid( '{{ supervisor }}' )
{% endif %}

{% if ministryArea != null and ministryArea != empty %}
    AND avm.Value like '%{{ ministryArea | UnescapeDataString }}%'
{% endif %}

Order by CTE.RequestDate, p.LastName, p.FirstName
