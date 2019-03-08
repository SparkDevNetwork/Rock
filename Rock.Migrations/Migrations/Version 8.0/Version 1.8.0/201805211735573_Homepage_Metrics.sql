DECLARE @SourceValueTypeId int = ( SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '6a1e1a1b-a636-4e12-b90c-d7fd1bdae764' )
DECLARE @CategoryId int = ( SELECT TOP 1 [Id] FROM [Category] WHERE [Guid] = '073add0c-b1f3-43ab-8360-89a1ce05a95d' )

INSERT INTO [Metric] ( [IsSystem], [Title], [SubTitle], [Description], [IconCssClass], [IsCumulative], [SourceValueTypeId], [SourceSql], [YAxisLabel], [Guid], [NumericDataType] )
VALUES 
	( 0
	, 'Active Records'
	, ''
	, 'The number of active person records in the database.'
	, 'fa fa-check-square'
	, 0
	, @SourceValueTypeId
	, 'SELECT COUNT(*) 
FROM [Person]
WHERE ([RecordTypeValueId] = 1) AND ([RecordStatusValueId] = 3)'
	, 'People'
	, 'ecb1b552-9a3d-46fc-952b-d57dbc4a329d'
	, 0 ),
	( 0
	, 'Active Families'
	, ''
	, 'The number of active families in the database.'
	, 'fa fa-users'
	, 0
	, @SourceValueTypeId
	, 'DECLARE @FamilyGroupTypeId int = (SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = ''790e3215-3b10-442b-af69-616c0dcb998e'')
SELECT COUNT( DISTINCT(g.[Id])) 
FROM [Person] p
    INNER JOIN [GroupMember] gm ON gm.[PersonId] = p.[Id]
    INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
WHERE ([RecordTypeValueId] = 1) AND ([RecordStatusValueId] = 3)'
	, 'Families'
	, '491061b7-1834-44da-8ea1-bb73b2d52ad3'
	, 0 ),
	( 0
	, 'Active Connection Requests'
	, ''
	, 'Active Connection Requests'
	, 'fa fa-plug'
	, 0
	, @SourceValueTypeId
	, 'SELECT COUNT(*) 
FROM [ConnectionRequest]
WHERE [ConnectionState] = 0'
	, 'Requests'
	, 'f0a24208-f8ac-4e04-8309-1a276885f6a6'
	, 0 )

DECLARE @ActiveRecordsMetricId int = ( SELECT TOP 1 [Id] FROM [Metric] WHERE [Guid] = 'ecb1b552-9a3d-46fc-952b-d57dbc4a329d' )
DECLARE @ActiveFamiliesMetricId int = ( SELECT TOP 1 [Id] FROM [Metric] WHERE [Guid] = '491061b7-1834-44da-8ea1-bb73b2d52ad3' )
DECLARE @ActiveConnectionRequestsMetricId int = ( SELECT TOP 1 [Id] FROM [Metric] WHERE [Guid] = 'f0a24208-f8ac-4e04-8309-1a276885f6a6' )

INSERT INTO [MetricCategory] ( [MetricId], [CategoryId], [Order], [Guid] )
VALUES 
	( @ActiveRecordsMetricId, @CategoryId, 0, 'aa35efa8-97cd-4124-9f7a-0032af56ec51' ),
	( @ActiveFamiliesMetricId, @CategoryId, 0, 'd7323374-0745-405e-a5be-4975783601e0' ),
	( @ActiveConnectionRequestsMetricId, @CategoryId, 0, '67686294-87ee-45f1-abf6-4273c9b524ca' )

INSERT INTO [MetricPartition] ( [MetricId], [Label], [EntityTypeId], [IsRequired], [Order], [Guid] )
VALUES ( @ActiveRecordsMetricId, NULL, NULL, 1, 0, '20bd0c1e-2faf-41ea-b443-839cbe2dce9a' )
DECLARE @ActiveRecordsMetricPartitionId int = SCOPE_IDENTITY()

INSERT INTO [MetricPartition] ( [MetricId], [Label], [EntityTypeId], [IsRequired], [Order], [Guid] )
VALUES ( @ActiveFamiliesMetricId, NULL, NULL, 1, 0, 'f879279d-3484-4f58-a16d-f64bdb277358' )
DECLARE @ActiveFamiliesMetricPartitionId int = SCOPE_IDENTITY()

INSERT INTO [MetricPartition] ( [MetricId], [Label], [EntityTypeId], [IsRequired], [Order], [Guid] )
VALUES ( @ActiveConnectionRequestsMetricId, NULL, NULL, 1, 0, 'bd1bd405-e6f0-439b-90b6-7bd97c76b637' )
DECLARE @ActiveConnectionRequestsMetricPartitionId int = SCOPE_IDENTITY()

INSERT INTO [dbo].[MetricValue] ([MetricValueType] ,[XValue] ,[YValue] ,[MetricId] ,[Note] ,[MetricValueDateTime] ,[Guid])
VALUES (0 , NULL, 49.00, @ActiveRecordsMetricId, NULL, '05/09/2018', '34325795-9016-47e9-a9d9-6283d1a84275')
DECLARE @ActiveRecordsMetricValueId int = SCOPE_IDENTITY()

INSERT INTO [dbo].[MetricValue] ([MetricValueType] ,[XValue] ,[YValue] ,[MetricId] ,[Note] ,[MetricValueDateTime] ,[Guid])
VALUES (0 , '', 0.00, @ActiveConnectionRequestsMetricId, '', '05/09/2018', '90cd5a83-3079-4656-b7ce-bfa21055c980')
DECLARE @ActiveConnectionMetricValueId int = SCOPE_IDENTITY()

INSERT INTO [dbo].[MetricValue] ([MetricValueType] ,[XValue] ,[YValue] ,[MetricId] ,[Note] ,[MetricValueDateTime] ,[Guid])
VALUES (0 , '', 1.00, @ActiveFamiliesMetricId, '', '05/09/2018', '932479dd-9612-4d07-b9cd-9227976cf5dd')
DECLARE @ActiveFamiliesMetricValueId int = SCOPE_IDENTITY()

INSERT INTO [dbo].[MetricValuePartition] ([MetricPartitionId] ,[MetricValueId] ,[EntityId] ,[Guid])
VALUES
	(@ActiveRecordsMetricPartitionId ,@ActiveRecordsMetricValueId ,NULL , 'd72e40cf-2e85-44b9-b864-140a37e85afd'),
	(@ActiveConnectionRequestsMetricPartitionId ,@ActiveConnectionMetricValueId ,NULL , '465d1cad-0798-4e3c-8671-b53c032a0eac'),
	(@ActiveFamiliesMetricPartitionId ,@ActiveFamiliesMetricValueId ,NULL , 'd7af2f13-8f7d-46a0-b91a-424c6b080afb')