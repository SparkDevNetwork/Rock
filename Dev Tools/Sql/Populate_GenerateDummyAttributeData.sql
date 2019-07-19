/* Use this script to generate a large amount of data in the AttributeValue's table */
/* This will just be junk data for purposes of checking performance when there is a very large amount of data in the AttributeValues table */
/* Only use this on a test development database that you won't be keeping for very long since it might have unwanted side effects within the Rock application */


declare @maxAttributeValues int = 5000000;
declare @currentAttributeCount int = (select count(*) from Attribute)
declare @currentAttributeValueCount int = (select count(*) from AttributeValue)

if (@currentAttributeCount < 100000)
begin
INSERT INTO [dbo].[Attribute]
           ([IsSystem]
           ,[FieldTypeId]
           ,[EntityTypeId]
           ,[EntityTypeQualifierColumn]
           ,[EntityTypeQualifierValue]
           ,[Key]
           ,[Name]
           ,[Description]
           ,[Order]
           ,[IsGridColumn]
           ,[DefaultValue]
           ,[IsMultiValue]
           ,[IsRequired]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[IconCssClass]
           ,[AllowSearch]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[IsIndexEnabled]
           ,[IsAnalytic]
           ,[IsAnalyticHistory]
           ,[IsActive]
           ,[EnableHistory])
           select
		   IsSystem,
           FieldTypeId,
           EntityTypeId,
           EntityTypeQualifierColumn,
           EntityTypeQualifierValue,
           [Key],
           [Name],
           [Description],
           [Order],
           IsGridColumn,
           DefaultValue,
           IsMultiValue,
           IsRequired,
           newid(),
           CreatedDateTime,
           ModifiedDateTime,
           CreatedByPersonAliasId,
           ModifiedByPersonAliasId,
           ForeignKey,
           IconCssClass,
           AllowSearch,
           ForeignGuid,
           ForeignId,
           IsIndexEnabled,
           IsAnalytic,
           IsAnalyticHistory,
           IsActive,
           EnableHistory from Attribute
end



/* This will double (up to 500000 rows at a time) the amount of Attribute Values in the system. Run this multiple times until you have as much Attribute value data as you want */
/* NOTE: You may occasionally get an index violation since the EntityIds are just random numbers, because AttributeId+EntityId must be unique. 
  If you do get the index error, just try again */
 while (@currentAttributeValueCount < @maxAttributeValues)
 begin
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
SELECT TOP (100000) av.IsSystem,
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
set @currentAttributeValueCount = (select count(*) from AttributeValue)
end
