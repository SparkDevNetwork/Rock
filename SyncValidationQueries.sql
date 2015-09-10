-- contributions in Arena, not in Rock (should not return any records )
select *
from ctrb_contribution c
left outer join rockrms.dbo.FinancialTransaction t on t.Id = c.foreign_key
where t.id is null

-- batches in Arena, not in Rock (should not return any records )
select *
from ctrb_batch b
left outer join rockrms.dbo.FinancialBatch rb on rb.id = b.foreign_key
where rb.id is null

-- matched transactions with different or missing batch in Rock (should not return any)
select *
from ctrb_contribution c
inner join ctrb_batch b on b.batch_id = c.batch_id
inner join rockrms.dbo.FinancialTransaction t on t.Id = c.foreign_key
inner join rockrms.dbo.FinancialBatch rb on rb.id = b.foreign_key
where t.BatchId is null or t.BatchId <> rb.Id 

-- Check to make sure transaction images are correct (should not return any)
DECLARE @binaryFileTypeContributionId INT = (
	SELECT TOP 1 Id
	FROM rockrms.dbo.BinaryFileType
	WHERE [Guid] = '6D18A9C4-34AB-444A-B95B-C644019465AC' /* Contribution */
)
SELECT * FROM rockrms.dbo.BinaryFile
WHERE Id IN (
	SELECT BinaryFileId
	FROM rockrms.dbo.FinancialTransactionImage
)
AND BinaryFileTypeId != @binaryFileTypeContributionId 

-- Make sure giving group is correct (should not return any)
select distinct p.person_id, f.family_id, f.foreign_key, rp.givinggroupid
from core_person p 
inner join rockrms.dbo.person rp on rp.id = p.foreign_key
inner join core_family_member fm on fm.person_id = p.person_id
inner join core_family f on f.family_id = fm.family_id
where p.giving_unit_id like 'F%'
and rp.givinggroupid <> f.foreign_key