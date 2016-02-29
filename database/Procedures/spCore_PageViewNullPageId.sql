/*
<doc>
	<summary>
 		This function nulls out the page ids on the [PageView] table for a page that 
		is about to be deleted. This is done in SQL since a single page can have
		thousands of [PageView] records.
	</summary>

	<returns>
		
	</returns>
	<param name="PageId" datatype="int">Page Id of the page that is about to be deleted</param>
	
	<code>
		EXEC [dbo].[spCore_PageViewNullPageId] 2 
	</code>
</doc>
*/
ALTER PROCEDURE spCore_PageViewNullPageId 
	@PageId int 
AS
BEGIN

	SET NOCOUNT ON;

	UPDATE
		[PageView]
	SET
		[PageId] = null
	WHERE
		[PageId] = @PageId
    
END