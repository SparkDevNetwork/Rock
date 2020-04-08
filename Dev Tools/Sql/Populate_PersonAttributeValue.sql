/* TODO: Come up with a way to make more realistic data for Attribute.Value */

-- NOTE: This might help  of Person AttributeValues, but just junk for the Value. So, only really useful if you just need some junk data for AttributeValues

INSERT INTO [dbo].[AttributeValue]
           ([IsSystem]
           ,[AttributeId]
           ,[EntityId]
           ,[Value]
           ,[Guid])
select top 100000
0 [IsSystem],
av.AttributeId [AttributeId],
p.Id [EntityId],
av.[Value] [Value], 
newid() [Guid]
from AttributeValue av,
Person p where EntityId not in (select EntityId from AttributeValue av1 where av1.AttributeId = av.AttributeId )


 