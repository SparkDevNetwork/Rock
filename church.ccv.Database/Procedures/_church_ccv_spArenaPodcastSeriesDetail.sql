/*
<doc>
	<summary>
 		This stored procedure gets podcast series information from Arena to help speed the Rock integration.
	</summary>

	<returns></returns>
	<param name="SeriesId" datatype="int">The series id to load</param>
	<remarks>	
	</remarks>
	<code>
		EXEC [dbo].[_church_ccv_spArenaPodcastSeriesDetail] 125
		
	</code>
</doc>
*/

ALTER PROCEDURE [dbo].[_church_ccv_spArenaPodcastSeriesDetail]
	@SeriesId int 

AS

BEGIN
		SELECT 
			[topic_id] AS [SeriesId]
			, [title] AS [Title]
			, ISNULL([description], '') AS [Description]
			, ISNULL([extra_details], '') AS [ExtraDetails]
			,ISNULL(CAST((SELECT [guid] FROM [Arena].[dbo].[util_blob] WHERE [blob_id] = t.image_blob_id) AS VARCHAR(100)), '') AS [MessageImageBlobId1]
			,ISNULL(CAST((SELECT [guid] FROM [Arena].[dbo].[util_blob] WHERE [blob_id] = t.image2_blob_id) AS VARCHAR(100)), '') AS [MessageImageBlobId2]
			,ISNULL(CAST((SELECT [guid] FROM [Arena].[dbo].[util_blob] WHERE [blob_id] = t.image3_blob_id) AS VARCHAR(100)), '') AS [MessageImageBlobId3]
			,ISNULL(CAST((SELECT [guid] FROM [Arena].[dbo].[util_blob] WHERE [blob_id] = t.image4_blob_id) AS VARCHAR(100)), '') AS [MessageImageBlobId4]
		FROM 
			[Arena].[dbo].[feed_topic] t
		WHERE
			[topic_id] = @SeriesId
		
		
		SELECT 
			[item_id] AS [MessageId]
			, [title] AS [Title]
			, i.subtitle AS [MessageSubtitle]
			, i.[Description]
			, i.[publish_date] AS [PublishDate]
			, i.[active] AS [IsActive]
			, a.name AS [MessageAuthor]
		FROM	
			[Arena].[dbo].[feed_item] i
			LEFT OUTER JOIN [Arena].[dbo].feed_author a ON a.author_id = i.author_id
		WHERE
			[topic_id] = @SeriesId
		ORDER BY
			i.[publish_date]
END