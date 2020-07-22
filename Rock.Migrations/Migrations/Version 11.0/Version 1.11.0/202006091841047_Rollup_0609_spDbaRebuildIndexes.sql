/*
<doc>
	<summary>
 		This function rebuilds the database indexes.
	</summary>

	<returns>
	</returns>
	<param name='PageCountLimit' datatype='int'>The number of page counts the index must have in order to be considered for re-indexing (default 100).</param>
	<param name='MinFragmentation' datatype='int'>The minimum amount of fragmentation a index must have to be considered for re-indexing (default 10).</param>
	<param name='MinFragmentationRebuild' datatype='int'>The minimum amount of fragmentation a index must have to be considered for a full rebuild. Otherwise a reorganize will be done. (default 30).</param>
	<remarks>	
	</remarks>
	<code>
		EXEC [dbo].[spDbaRebuildIndexes]
		EXEC [dbo].[spDbaRebuildIndexes] default, default, default, 1
		EXEC [dbo].[spDbaRebuildIndexes] default, 0, 0, 1
	</code>
</doc>
*/

ALTER PROCEDURE [dbo].[spDbaRebuildIndexes]
	  @PageCountLimit int = 100
	, @MinFragmentation int = 10
	, @MinFragmentationRebuild int = 30
	, @UseONLINEIndexRebuild bit = 0
	WITH RECOMPILE
AS

BEGIN

DECLARE @SchemaName AS varchar(100);
DECLARE @TableName AS varchar(100);
DECLARE @IndexName AS varchar(100);
DECLARE @IndexType AS varchar(100);
DECLARE @FragmentationPercent AS varchar(100);
DECLARE @PageCount AS varchar(100);
DECLARE @MaintenanceCursor AS CURSOR;
DECLARE @CommandOption varchar(100) = 'FILLFACTOR = 80';
DECLARE @SqlCommand AS nvarchar(2000);

SET @MaintenanceCursor = CURSOR FOR
			SELECT 
				dbschemas.[name] as 'Schema', 
				dbtables.[name] as 'Table', 
				dbindexes.[name] as 'Index',
				dbindexes.[type_desc] as 'IndexType',
				CONVERT(INT, indexstats.avg_fragmentation_in_percent),
				indexstats.page_count
			FROM 
				sys.dm_db_index_physical_stats (DB_ID(), NULL, NULL, NULL, NULL) AS indexstats
				INNER JOIN sys.tables dbtables on dbtables.[object_id] = indexstats.[object_id]
				INNER JOIN sys.schemas dbschemas on dbtables.[schema_id] = dbschemas.[schema_id]
				INNER JOIN sys.indexes AS dbindexes ON dbindexes.[object_id] = indexstats.[object_id]
				AND indexstats.index_id = dbindexes.index_id
			WHERE 
				indexstats.database_id = DB_ID() 
				AND indexstats.page_count > @PageCountLimit
				AND indexstats.avg_fragmentation_in_percent > @MinFragmentation
			ORDER BY 
				indexstats.avg_fragmentation_in_percent DESC
 
OPEN @MaintenanceCursor;
FETCH NEXT FROM @MaintenanceCursor INTO @SchemaName, @TableName, @IndexName, @IndexType, @FragmentationPercent, @PageCount;
 
WHILE @@FETCH_STATUS = 0
BEGIN
	IF (@FragmentationPercent > @MinFragmentationRebuild)
	BEGIN
		IF ( @UseONLINEIndexRebuild = 1 AND @IndexType NOT IN ('SPATIAL','XML') )
		BEGIN
			SELECT @CommandOption = @CommandOption + ', ONLINE = ON';
		END

		SET @SqlCommand = N'ALTER INDEX [' + @IndexName + N'] ON [' +  @SchemaName + N'].[' + @TableName + '] REBUILD WITH (' + @CommandOption + ')';
	END
	ELSE BEGIN
		SET @SqlCommand = N'ALTER INDEX [' + @IndexName + N'] ON [' +  @SchemaName + N'].[' + @TableName + '] REORGANIZE';
	END

	PRINT @SqlCommand;
	EXECUTE sp_executeSQL @SqlCommand;

	FETCH NEXT FROM @MaintenanceCursor INTO @SchemaName, @TableName, @IndexName, @IndexType, @FragmentationPercent, @PageCount;
END
 
CLOSE @MaintenanceCursor;
DEALLOCATE @MaintenanceCursor;

END