/*  
<doc>  
<summary>  
This stored procedure returns the data for the communication saturation report.  
</summary>  
<param name='StartDate' datatype='date'>The starting date.</param>  
<param name='EndDate' datatype='date'>The ending date.</param>  
<param name='BucketSize' datatype='int'>The number of messages in the buckets.</param>  
<param name='CommunicationType' datatype='int'>The type of communications to filter on (1 = Email, 2 = SMS, 3 = Push).</param>  
<param name='@IncludeNonBulk' datatype='bool'>Determines if only bulk emails should be considered.</param>  
<param name='DataViewId' datatype='int'>The data view to filter people on.</param>  
<param name='ConnectionStatusValueId' datatype='int'>The connection type to filter on.</param>  
<remarks>  
  
</remarks>  
<code>  
EXEC [dbo].[spCommunication_SaturationReport] '1/1/2025', '2/1/2025'  
EXEC [dbo].[spCommunication_SaturationReport] '1/1/2025', '2/1/2025', 5, 1, 1  
</code>  
</doc>  
*/  
CREATE PROCEDURE [dbo].[spCommunication_SaturationReport]  
  @StartDate DATE  
, @EndDate DATE  
, @BucketSize int = 3 -- null means don't filter by start date  
, @CommunicationType varchar(10) = '1' -- default to email, 2 is sms, 3 is push, formatted as comma-delimited list for multi-select  
, @IncludeNonBulk bit = 0 -- Default to only consider bulk email  
, @DataViewId int = null -- null means don't filter by a data view  
, @ConnectionStatusValueId int = null -- null means all connection statuses  
AS  
BEGIN  
  
/*  
Create a temp table to hold our buckets. This will ensure that we return all buckets.  
It's possible that if there are no individuals with the count in the buckets that the  
bucket would not show otherwise. This will also help us to order the buckets and return  
information about the bucket ranges.  
*/  
CREATE TABLE #ReportBuckets (  
[Id] INT,  
[LowerRange] INT,  
[UpperRange] INT,  
[Name] NVARCHAR(25),  
[Order] INT  
);  
  
-- Get the active record status  
DECLARE @ActiveRecordStatusValueId INT = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '618F906C-C33D-4FA3-8AEF-E58CB7B63F1E')  
  
-- Add bucket ranges  
INSERT INTO #ReportBuckets ([Id], [LowerRange], [UpperRange], [Name], [Order])  
VALUES (1, 0, 0, '0', 1)  
  
--1  
INSERT INTO #ReportBuckets ([Id], [LowerRange], [UpperRange], [Name], [Order])  
VALUES (2, 1, @BucketSize,'1-' + CAST(@BucketSize AS nvarchar(5)) , 2)  
  
--2  
INSERT INTO #ReportBuckets ([Id], [LowerRange], [UpperRange], [Name], [Order])  
VALUES (3, (@BucketSize + 1),(@BucketSize * 2), CAST((@BucketSize + 1) AS nvarchar(5)) + '-' + CAST((@BucketSize * 2) AS nvarchar(5)), 3)  
  
--3  
INSERT INTO #ReportBuckets ([Id], [LowerRange], [UpperRange], [Name], [Order])  
VALUES (4, ((@BucketSize * 2) + 1), (@BucketSize * 3),CAST(((@BucketSize * 2) + 1) AS nvarchar(5)) + '-' + CAST((@BucketSize * 3) AS nvarchar(5)), 4)  
  
--4  
INSERT INTO #ReportBuckets ([Id], [LowerRange], [UpperRange], [Name], [Order])  
VALUES (5, ((@BucketSize * 3) + 1), (@BucketSize * 4),CAST(((@BucketSize * 3) + 1) AS nvarchar(5)) + '-' + CAST((@BucketSize * 4) AS nvarchar(5)), 5)  
  
--5  
INSERT INTO #ReportBuckets ([Id], [LowerRange], [UpperRange], [Name], [Order])  
VALUES (6, ((@BucketSize * 4) + 1), (@BucketSize * 5),CAST(((@BucketSize * 4) + 1) AS nvarchar(5)) + '-' + CAST((@BucketSize * 5) AS nvarchar(5)), 6)  
  
--6  
INSERT INTO #ReportBuckets ([Id], [LowerRange], [UpperRange], [Name], [Order])  
VALUES (7, ((@BucketSize * 5) + 1),(@BucketSize * 6), CAST(((@BucketSize * 5) + 1) AS nvarchar(5)) + '-' + CAST((@BucketSize * 6) AS nvarchar(5)), 7)  
  
--7  
INSERT INTO #ReportBuckets ([Id], [LowerRange], [UpperRange], [Name], [Order])  
VALUES (8, ((@BucketSize * 6) + 1),999999999, CAST(((@BucketSize * 6) + 1) AS nvarchar(5)) + '+', 8)  
  
-- Query the data  
SELECT  
	[Name] 
	, ISNULL([NumberOfRecipients], 0) AS [NumberOfRecipients] -- Ensure NULL values are replaced with 0  
FROM (  
	SELECT  
		[MessageBucketId]  
		, COUNT(*) AS [NumberOfRecipients]  
	FROM (  
		SELECT  
			rb.[Id] AS [MessageBucketId]  
		FROM (  
			SELECT  
				pa.[PersonId]  
				, COUNT( c.[Id] ) AS [MessageCount]  
			FROM [Person] p  
			INNER JOIN [PersonAlias] pa ON pa.[PersonId] = p.[Id]  
			LEFT OUTER JOIN [CommunicationRecipient] cr ON pa.[Id] = cr.[PersonAliasId]
			LEFT OUTER JOIN [Communication] c ON c.[Id] = cr.[CommunicationId] 
				AND (c.[IsBulkCommunication] = 1 OR @IncludeNonBulk = 1)
				AND c.[SendDateTime] >= @StartDate
				AND c.[SendDateTime] < @EndDate
				AND c.[CommunicationType] IN (SELECT value FROM STRING_SPLIT(@CommunicationType, ','))
			WHERE  
				p.[RecordStatusValueId] = @ActiveRecordStatusValueId  
				AND p.[AgeClassification] = 1  
				AND (p.[ConnectionStatusValueId] = @ConnectionStatusValueId OR @ConnectionStatusValueId IS NULL)  
				AND (@DataViewId IS NULL OR p.[Id] IN (SELECT [EntityId] FROM [DataViewPersistedValue] dvp WHERE dvp.[DataViewId] = @DataViewId))  
			GROUP BY pa.[PersonId]
		) AS [RecipientMessageCounts]  
		INNER JOIN #ReportBuckets rb ON rb.[LowerRange] <= [RecipientMessageCounts].[MessageCount] AND rb.UpperRange >= [RecipientMessageCounts].[MessageCount]  
	) AS [Buckets]
	GROUP BY [MessageBucketId]  
) AS [BucketCounts]  
RIGHT OUTER JOIN #ReportBuckets rb ON rb.[Id] = [BucketCounts].MessageBucketId  
ORDER BY rb.[Order]  
END