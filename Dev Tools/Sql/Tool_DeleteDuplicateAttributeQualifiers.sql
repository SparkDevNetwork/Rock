
delete from AttributeQualifier where Id in
(
select a.Id from
(
select max(aq.id) [Id], aq.[AttributeId], aq.[Key], count(*) [Count] from AttributeQualifier aq group by aq.[AttributeId], aq.[Key]
) [a]
where a.Count > 1
)