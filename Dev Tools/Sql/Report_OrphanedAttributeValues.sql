select count(*) [TotalCount], sum(len(value)) [TotalSizeB], sum(len(value)) / 1024 [TotalSizeKB] from AttributeValue

select top 10 et.Name, count(*), sum(len(value)) / 1024 [TotalSizeKB] from AttributeValue av
join Attribute a on av.AttributeId = a.Id
join EntityType et on a.EntityTypeId = et.Id
group by et.Name
order by count(*) desc

-- Workflow - Orphaned Attribute Values
select et.Name, count(*) [Count], sum(len(value)) [TotalSizeB], sum(len(value)) / 1024 [TotalSizeKB]  from AttributeValue av
join Attribute a on av.AttributeId = a.Id
join EntityType et on a.EntityTypeId = et.Id
where (et.Name = 'Rock.Model.Workflow' and av.EntityId not in (Select Id from Workflow))
group by et.Name

-- RegistrationRegistrant - Orphaned Attribute Values
select et.Name, count(*) [Count], sum(len(value)) [TotalSizeB], sum(len(value)) / 1024 [TotalSizeKB]  from AttributeValue av
join Attribute a on av.AttributeId = a.Id
join EntityType et on a.EntityTypeId = et.Id
where (et.Name = 'Rock.Model.RegistrationRegistrant' and av.EntityId not in (Select Id from RegistrationRegistrant))
group by et.Name

-- Block - Orphaned Attribute Values
select et.Name, count(*) [Count], sum(len(value)) [TotalSizeB], sum(len(value)) / 1024 [TotalSizeKB]  from AttributeValue av
join Attribute a on av.AttributeId = a.Id
join EntityType et on a.EntityTypeId = et.Id
where (et.Name = 'Rock.Model.Block' and av.EntityId not in (Select Id from Block))
group by et.Name

-- Person - Orphaned Attribute Values 
select et.Name, count(*) [Count], sum(len(value)) [TotalSizeB], sum(len(value)) / 1024 [TotalSizeKB]  from AttributeValue av
join Attribute a on av.AttributeId = a.Id
join EntityType et on a.EntityTypeId = et.Id
where (et.Name = 'Rock.Model.Person' and av.EntityId not in (Select Id from Person))
group by et.Name




