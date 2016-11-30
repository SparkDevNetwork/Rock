/*
<doc>
	<summary>
 		This stored procedure gets podcast message information from Arena to help speed the Rock integration.
	</summary>

	<returns></returns>
	<param name="MessageId" datatype="int">The series id to load</param>
	<remarks>	
	</remarks>
	<code>
		EXEC [dbo].[_church_ccv_spArenaPodcastMessageDetail] 526
	</code>
</doc>
*/

ALTER PROCEDURE [dbo].[_church_ccv_spArenaPodcastMessageDetail]
	@MessageId int 

AS

BEGIN
		SELECT 
			[item_id] AS [MessageId]
			, i.[title] AS [Title]
			, i.[description] AS [Description]
			, ISNULL(a.name,'') AS [MessageAuthor]
			, i.publish_date AS [PublishDate]
			, t.[title] AS [SeriesTitle]
			, t.[topic_id] AS [SeriesId]
			, ISNULL((SELECT TOP 1 [enclosure_url] FROM [Arena].[dbo].[feed_item_format] WHERE [item_id] = @MessageId AND [format_id] = 1), '') AS [VideoDownload]
			, ISNULL((SELECT TOP 1 [enclosure_url] FROM [Arena].[dbo].[feed_item_format] WHERE [item_id] = @MessageId AND [format_id] = 2), '') AS [VideoMessageOnly]
			, ISNULL((SELECT TOP 1 [enclosure_url] FROM [Arena].[dbo].[feed_item_format] WHERE [item_id] = @MessageId AND [format_id] = 5), '') AS [VideoFullService]
			, ISNULL((SELECT TOP 1 [enclosure_url] FROM [Arena].[dbo].[feed_item_format] WHERE [item_id] = @MessageId AND [format_id] = 3), '') AS [Audio]
			, ISNULL((SELECT TOP 1 [enclosure_url] FROM [Arena].[dbo].[feed_item_format] WHERE [item_id] = @MessageId AND [format_id] = 4), '') AS [SermonNotes]	
			,(SELECT [guid] FROM [Arena].[dbo].[util_blob] WHERE [blob_id] = t.image_blob_id) AS [MessageImageBlobId1]
			,(SELECT [guid] FROM [Arena].[dbo].[util_blob] WHERE [blob_id] = t.image2_blob_id) AS [MessageImageBlobId2]
			,(SELECT [guid] FROM [Arena].[dbo].[util_blob] WHERE [blob_id] = t.image3_blob_id) AS [MessageImageBlobId3]
			,(SELECT [guid] FROM [Arena].[dbo].[util_blob] WHERE [blob_id] = t.image4_blob_id) AS [MessageImageBlobId4]
		FROM 
			[Arena].[dbo].[feed_item] i
			INNER JOIN [Arena].[dbo].[feed_topic] t ON t.[topic_id] = i.[topic_id]
			LEFT OUTER JOIN [Arena].[dbo].[feed_author] a ON a.author_id = i.author_id
		WHERE
			[item_id] = @MessageId


		SELECT 
			[item_id] AS [MessageId]
			, [title] AS [Title]
			, i.subtitle AS [MessageSubtitle]
			, i.[description] AS [Description]
			, a.name AS [MessageAuthor]
			, i.publish_date AS [PublishDate]
		FROM	
			[Arena].[dbo].[feed_item] i
			LEFT OUTER JOIN [Arena].[dbo].feed_author a ON a.author_id = i.author_id
		WHERE
			[topic_id] = (SELECT TOP 1 [topic_id] FROM [Arena].[dbo].[feed_item] WHERE [item_id] = @MessageId)
		ORDER BY [publish_date]
END