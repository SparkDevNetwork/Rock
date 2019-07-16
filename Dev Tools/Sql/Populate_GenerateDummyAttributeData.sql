/* Use this script to generate a large amount of data in the AttributeValue's table */
/* This will just be junk data for purposes of checking performance when there is a very large amount of data in the AttributeValues table */
/* Only use this on a test development database that you won't be keeping for very long since it might have unwanted side effects within the Rock application */


declare @maxAttributeValueRowsPerRun int = 500000;


/* This will double (up to 500000 rows at a time) the amount of Attribute Values in the system. Run this multiple times until you have as much Attribute value data as you want */
/* NOTE: You may occasionally get an index violation since the EntityIds are just random numbers, because AttributeId+EntityId must be unique. 
  If you do get the index error, just try again */
INSERT INTO [dbo].[AttributeValue] (
	[IsSystem],
	[AttributeId],
	[EntityId],
	[Value],
	[Guid],
	[CreatedDateTime],
	[ModifiedDateTime],
	[CreatedByPersonAliasId],
	[ModifiedByPersonAliasId],
	[ForeignKey],
	[ValueAsDateTime],
	[ForeignGuid],
	[ForeignId]
	)
SELECT TOP (@maxAttributeValueRowsPerRun) av.IsSystem,
	av.AttributeId,
	abs(Cast(checksum(newid()) AS INT)) EntityId,
	av.[Value],
	newid(),
	getdate(),
	getdate(),
	av.CreatedByPersonAliasId,
	av.ModifiedByPersonAliasId,
	av.ForeignKey,
	ValueAsDateTime,
	av.ForeignGuid,
	av.ForeignId
FROM AttributeValue av
