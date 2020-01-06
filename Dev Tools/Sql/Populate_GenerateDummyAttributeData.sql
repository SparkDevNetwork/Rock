/* Use this script to generate a large amount of data in the AttributeValue's table */
/* This will just be junk data for purposes of checking performance when there is a very large amount of data in the AttributeValues table */
/* Only use this on a test development database that you won't be keeping for very long since it might have unwanted side effects within the Rock application */


declare @maxAttributeValueRowsPerRun int = 500000;
declare @totalAttributeValueCount int = 0;

while (@totalAttributeValueCount < 9500500)
begin

declare @attributeId int = (select top 1 Id from Attribute order by NEWID())

/* This will double (up to 500000 rows at a time) the amount of Attribute Values in the system. Run this multiple times until you have as much Attribute value data as you want */
/* NOTE: You may occasionally get an index violation since the EntityIds are just random numbers, because AttributeId+EntityId must be unique. 
  If you do get the index error, just try again */
INSERT INTO [dbo].[AttributeValue] (
	[IsSystem],
	[AttributeId],
	[EntityId],
	[Value],
	[Guid]
	)
SELECT TOP (@maxAttributeValueRowsPerRun) 
    0,
	@attributeId,
	p.Id EntityId,
	'456.456',
	newid()
FROM Person p
where p.Id not in (select EntityId from AttributeValue where AttributeId = @attributeId)
    set @totalAttributeValueCount = (select count(*) from AttributeValue)
end






