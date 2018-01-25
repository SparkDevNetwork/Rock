Declare @AttributeId int = 1962;
Declare @AttributeMatrixTemplateId int = 1;
-----------------------------------------------------------------------------
-- FLUSH THE CACHE AFTER RUNNING THIS
-----------------------------------------------------------------------------

Begin Transaction

-------------------------------------- Get Constants
Declare @AttributeMatrixFieldTypeId int  = (Select Top 1 Id From FieldType Where [Guid] = 'F16FC460-DC1E-4821-9012-5F21F974C677')
Declare @DateFieldTypeId int  = (Select Top 1 Id From FieldType Where [Guid] = '6B6AA175-4758-453F-8D83-FCD8044B5F36')
Declare @FileFieldTypeId int  = (Select Top 1 Id From FieldType Where [Guid] = '6F9E2DD0-E39E-4602-ADF9-EB710A75304A')

Declare @AttributeGuid uniqueidentifier= (Select top 1 [Guid] From Attribute where Id = @AttributeId and FieldTypeId = @FileFieldTypeId)
Declare @AttributeMatrixTemplateGuid uniqueidentifier = (select top 1 [Guid] from AttributeMatrixTemplate where Id = @AttributeMatrixTemplateId )

Declare @DateColumnAttributeId int = 
(
	Select Top 1 Id
	From Attribute
	Where EntityTypeQualifierColumn = 'AttributeMatrixTemplateId'
	And EntityTypeQualifierValue = Convert(nvarchar(max), @AttributeMatrixTemplateId)
	And FieldTypeId = @DateFieldTypeId
)

Declare @FileColumnAttributeId int = 
(
	Select Top 1 Id
	From Attribute
	Where EntityTypeQualifierColumn = 'AttributeMatrixTemplateId'
	And EntityTypeQualifierValue = Convert(nvarchar(max), @AttributeMatrixTemplateId)
	And FieldTypeId = @FileFieldTypeId
)

---------------------- If Check on attribute and template guids to verify the entities are in the database
If @AttributeGuid is not null and @AttributeMatrixTemplateGuid is not null and @DateColumnAttributeId is not null and @FileColumnAttributeId is not null
Begin

---------------------- Update Attribute to Type AttributeMatrix
Update Attribute
Set FieldTypeId = @AttributeMatrixFieldTypeId
Where Id = @AttributeId

---------------------- Set AttributeQualifier ( Removing file type qualifiers, etc)
Delete 
From AttributeQualifier
Where AttributeId = @AttributeId

INSERT INTO [AttributeQualifier]
           ([IsSystem]
           ,[AttributeId]
           ,[Key]
           ,[Value]
           ,[Guid])
     VALUES
           (0
           ,@AttributeId
           ,'attributematrixtemplate'
           ,Convert(nvarchar(max),@AttributeMatrixTemplateId)
           ,NEWID())

---------------------- Build Table to store values in
declare @DataTable table(
EntityId int,  -- Person Id
Value nvarchar(max), -- BinaryFile Guid
AttributeValueId int,
ModifiedDate datetime, -- ModifiedDate of the AttributeValue
AttributeMatrixGuid uniqueidentifier,
AttributeMatrixItemGuid uniqueidentifier
);

insert into @DataTable
Select 
av.EntityId,
av.Value,
av.Id,
av.ModifiedDateTime,
NewId(),
NewId()
From AttributeValue av
Where AttributeId = @AttributeId

---------------------- Create New AM ( The Grid)
INSERT INTO [AttributeMatrix]
           ([AttributeMatrixTemplateId]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[Guid])
     Select
           @AttributeMatrixTemplateId
           ,GETDATE()
           ,GETDATE()
           ,null
           ,null
           ,AttributeMatrixGuid
		From @DataTable

---------------------- Create New AMI ( The First Row) 
INSERT INTO [AttributeMatrixItem]
           ([AttributeMatrixId]
           ,[Order]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[Guid])
     Select
           (Select top 1 Id from AttributeMatrix where [Guid] = AttributeMatrixGuid)
           ,0
           ,GETDATE()
           ,GETDATE()
           , AttributeMatrixItemGuid
	From @DataTable

---------------------- Create New Date AV
INSERT INTO [AttributeValue]
           ([IsSystem]
           ,[AttributeId]
           ,[EntityId]
           ,[Value]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[ValueAsDateTime])
     Select
           0
           ,@DateColumnAttributeId
           ,(Select top 1 Id from AttributeMatrixItem where [Guid] = AttributeMatrixItemGuid)
           ,convert(nvarchar(max),convert(datetime, convert(date, ModifiedDate)), 126)
           ,NewId()
           ,GetDate()
           ,GetDate()
           ,convert(datetime, convert(date, ModifiedDate))
	From @DataTable

---------------------- Update old AV to point to AMI
Update  av
Set av.AttributeId = @FileColumnAttributeId,
	av.EntityId = (Select top 1 Id from AttributeMatrixItem where [Guid] = dt.AttributeMatrixItemGuid)
From
	AttributeValue av
	inner join @DataTable dt on av.Id = dt.AttributeValueId
Where av.AttributeId = @AttributeId

---------------------- Create New Attribute Value Pointing to AM
INSERT INTO [AttributeValue]
           ([IsSystem]
           ,[AttributeId]
           ,[EntityId]
           ,[Value]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[ValueAsDateTime])
     Select
           0
           ,@AttributeId
           ,EntityId
           ,Convert(nvarchar(max), AttributeMatrixGuid)
           ,NewId()
           ,GetDate()
           ,GetDate()
           ,NULL
	From @DataTable

End
rollback Transaction