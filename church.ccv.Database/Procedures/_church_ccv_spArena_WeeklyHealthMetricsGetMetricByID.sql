CREATE  PROC [dbo].[_church_ccv_GetMetricByID]
@MetricID int

AS

SELECT 
	M.Id AS [metric_id],
	M.CreatedDateTime AS [date_created],
	M.ModifiedDateTime AS [date_modified],
	M.CreatedByPersonAliasId AS [created_by],
	M.ModifiedByPersonAliasId AS [modified_by],
	M.[Guid] AS [guid],
	MC.CategoryId AS [parent_metric_id],
	MC.[Order] AS [metric_order],
	0 AS [graph_type],
	M.Title,
	M.Subtitle AS [series_caption],
	M.[Description],
	M.Subtitle AS [source_summary],
	0 AS [collection_frequency],
	M.LastRunDateTime AS [collection_last_date],
	M.SourceSql AS [collection_sql_statement],
	M.IsCumulative AS [aggregate_type],
	M.SourceValueTypeId AS [collection_list_source]
FROM Metric M
INNER JOIN MetricCategory MC ON MC.MetricId = M.Id
WHERE M.Id = @MetricID