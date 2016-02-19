/*
<doc>
	<summary>
 		This stored procedure gets podcast information from Arena to help speed the Rock integration.
	</summary>

	<returns></returns>
	<param name="PageNum" datatype="int">The current page number.</param>
	<param name="MessagesPerPage" datatype="int">The number of messages to return per page</param>
	<remarks>	
	</remarks>
	<code>
		EXEC [dbo].[_church_ccv_spArenaPodcastSeriesList] 1, 3
		
	</code>
</doc>
*/

ALTER PROCEDURE [dbo].[_church_ccv_spArenaPodcastSeriesList]
	@PageNum int = 1
	, @SeriesPerPage int = 9

AS
SET @PageNum = @PageNum -1
BEGIN
		
		WITH
		  ctePodcasts (topic_id, title, description, image_blob_id, image2_blob_id, image3_blob_id, image4_blob_id, extra_details)
		  AS
		  (
			SELECT t.topic_id, t.title, t.description, t.image_blob_id, image2_blob_id, image3_blob_id, image4_blob_id, extra_details
			FROM [Arena].[dbo].feed_topic t
			WHERE t.active = 1 AND t.channel_id = 1
			ORDER BY topic_id DESC
			OFFSET (@PageNum * @SeriesPerPage) ROWS
			FETCH NEXT @SeriesPerPage ROWS ONLY
		  )
			SELECT 
				t.topic_id AS [SeriesId]
				,t.title AS [SeriesTitle]
				,i.item_id AS [MessageId]
				,t.description AS [MessageDescription]
				,(SELECT [guid] FROM [Arena].[dbo].[util_blob] WHERE [blob_id] = t.image_blob_id) AS [MessageImageBlobId1]
				,(SELECT [guid] FROM [Arena].[dbo].[util_blob] WHERE [blob_id] = t.image2_blob_id) AS [MessageImageBlobId2]
				,(SELECT [guid] FROM [Arena].[dbo].[util_blob] WHERE [blob_id] = t.image3_blob_id) AS [MessageImageBlobId3]
				,(SELECT [guid] FROM [Arena].[dbo].[util_blob] WHERE [blob_id] = t.image4_blob_id) AS [MessageImageBlobId4]
				,i.title AS [MessageTitle]
				,i.subtitle AS [MessageSubtitle]
				,i.link AS [MessageLink]
				,i.description AS [MessageDescription]
				,i.active AS [MessageActive]
				,i.keywords AS [MessageKeywords]
				,a.name AS [MessageAuthor]
				, (@PageNum + 1) AS [CurrentPage]
			FROM 
				[Arena].[dbo].feed_item i 
				INNER JOIN ctePodcasts t ON i.topic_id = t.topic_id
				LEFT OUTER JOIN [Arena].[dbo].feed_author a ON a.author_id = i.author_id
			WHERE i.active = 1
			ORDER BY publish_date DESC
	
	SELECT CEILING(CAST(COUNT(*) AS float) / CAST(@SeriesPerPage AS float))
			FROM [Arena].[dbo].feed_topic t
			WHERE t.active = 1 AND t.channel_id = 1
END