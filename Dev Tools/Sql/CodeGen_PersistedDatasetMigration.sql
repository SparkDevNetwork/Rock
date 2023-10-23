/***************************************************************************************************************************************************************************************
****************************************************************************************************************************************************************************************
***************************************************************************WARNING READ THIS!!******************************************************************************************
****************************************************************************************************************************************************************************************
****************************************************************************************************************************************************************************************

1. Only run this script on a clean database that only has your changes in it (i.e. new PersistedDataset)
2. To copy and paste directly from the grid results turn on Options > Query Results > Sql Server > Results to Grid > 'Retain CR/LF on copy or save'
3. The results should not be blindly accepted. Make sure the output is correct.

***************************************************************************************************************************************************************************************/

SELECT [AccessKey] -- Helps identify migration results.
	, N'Sql( @"DECLARE @EntityTypeId AS INT

SELECT @EntityTypeId=[Id]
  FROM [dbo].[EntityType]
 WHERE [Guid] = ''' + CONVERT(VARCHAR(36), E.[Guid]) + '''

INSERT INTO [dbo].[PersistedDataset]
(
	[AccessKey]
	, [Name]
	, [Description]
	, [RefreshIntervalMinutes]
	, [AllowManualRefresh]
	, [ResultFormat]
	, [MemoryCacheDurationMS]
	, [BuildScript]
	, [BuildScriptType]
	, [IsSystem]
	, [IsActive]
	, [EntityTypeId]
	, [ExpireDateTime]
	, [Guid]
	, [ForeignId]
	, [ForeignGuid]
	, [ForeignKey]
	, [EnabledLavaCommands]
)
VALUES
(
	' + ISNULL('N''' + REPLACE(REPLACE(P.[AccessKey], '"', '""'), '''', '''''') + '''', 'NULL') + ' -- [AccessKey]
	, N''' + REPLACE(REPLACE(P.[Name], '"', '""'), '''', '''''') + ''' -- [Name]
	, ' + ISNULL('N''' + REPLACE(REPLACE(P.[Description], '"', '""'), '''', '''''') + '''', 'NULL') + ' -- [Description]
	, ' + ISNULL(CONVERT(VARCHAR(20), P.[RefreshIntervalMinutes]), 'NULL') + ' -- [RefreshIntervalMinutes]
	, ' + CONVERT(CHAR(1), P.[AllowManualRefresh]) + ' -- [AllowManualRefresh]
	, ' + CONVERT(VARCHAR(20), P.[ResultFormat]) + ' -- [ResultFormat]
	, ' + ISNULL(CONVERT(VARCHAR(20), P.[MemoryCacheDurationMS]), 'NULL') + ' -- [MemoryCacheDurationMS]
	, ' + ISNULL('N''' + REPLACE(REPLACE(P.[BuildScript], '"', '""'), '''', '''''') + '''', 'NULL') + ' -- [BuildScript]
	, ' + CONVERT(VARCHAR(20), P.[BuildScriptType]) + ' -- [BuildScriptType]
	, ' + CONVERT(CHAR(1), P.[IsSystem]) + ' -- [IsSystem]
	, ' + CONVERT(CHAR(1), P.[IsActive]) + ' -- [IsActive]
	, @EntityTypeId -- [EntityTypeId]
	, ' + ISNULL('''' + CONVERT(VARCHAR(25), P.[ExpireDateTime], 121) + '''', 'NULL') + ' -- [ExpireDateTime]
	, ''' + CONVERT(VARCHAR(36), P.[Guid]) + ''' -- [Guid]
	, ' + ISNULL(CONVERT(VARCHAR(20), P.[ForeignId]), 'NULL') + ' -- [ForeignId]
	, ' + ISNULL('''' + CONVERT(VARCHAR(36), P.[ForeignGuid]) + '''', 'NULL') + ' -- [ForeignGuid]
	, ' + ISNULL('N''' + REPLACE(REPLACE(P.[ForeignKey], '"', '""'), '''', '''''') + '''', 'NULL') + ' -- [ForeignKey]
	, ' + ISNULL('N''' + REPLACE(REPLACE(P.[EnabledLavaCommands], '"', '""'), '''', '''''') + '''', 'NULL') + ' -- [EnabledLavaCommands]
)" );' AS [Up],
	N'Sql( @"DELETE FROM [dbo].[PersistedDataset] WHERE [Guid] = ''' + CONVERT(VARCHAR(36), P.[Guid]) + '''" );' AS [Down]
  FROM [dbo].[PersistedDataset] P
  LEFT JOIN [dbo].[EntityType] E ON P.[EntityTypeId] = E.[Id]