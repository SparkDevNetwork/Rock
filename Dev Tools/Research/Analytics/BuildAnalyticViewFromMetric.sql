DECLARE 
  -- Figure out which Metric you want to build a view for, in this case, the one with MetricCategoryId 100 (from the Metrics URL)
  @metricId int = (
                SELECT TOP 1 MetricId
                FROM MetricCategory
                WHERE Id = 100
                ),

@queryText NVARCHAR(max) = 'SELECT pvt.Id
  ,cast(pvt.MetricValueDateTime AS DATE) AS [MetricValueDateTime]
  ,pvt.YValue
';

BEGIN
    WITH ctePartition
    AS (
        SELECT mp.Label [Partition.Label]
            ,mp.EntityTypeId
            ,et.NAME [EntityType.Name]
            ,et.FriendlyName
            ,mp.[Order]
        FROM MetricPartition mp
        JOIN EntityType et ON mp.EntityTypeId = et.Id
        WHERE mp.MetricId = @MetricId
        )
    SELECT @queryText = @queryText + x.Result
    FROM (
        SELECT 1 [ResultOrder], CONCAT (
                '  ,pvt.['
                ,c.EntityTypeId
                ,'] AS ['
                ,Replace(c.[Partition.Label], ' ', '')
                ,case when EntityTypeId = 54 then 'ValueId]' else 'Id]' end
                ,CHAR(10)
                ) [Result], [Order]
        FROM ctePartition c
         -- x order by x.[Order]
		union 
		select 2 [ResultOrder], concat('FROM (
    SELECT 
	  mv.Id
      ,mv.YValue
      ,mv.MetricValueDateTime
      ,mvp.EntityId
      ,mp.EntityTypeId
    FROM MetricValue mv
    JOIN MetricValuePartition mvp ON mvp.MetricValueId = mv.Id
    JOIN MetricPartition mp ON mvp.MetricPartitionId = mp.Id
    WHERE mv.MetricId = ', @metricId, '
    ) src
pivot(min(EntityId) FOR EntityTypeId IN (#deleteme#') [Result], null
       union
	   select 3 [ResultOrder], concat(',[', c2.EntityTypeId, ']', char(10)) [Result], null from ctePartition c2
		) x
		order by x.ResultOrder, x.[Order]
    
    SELECT @queryText = REPLACE(@queryText, '#deleteme#,', '') + ')) pvt'

	-- this will output the SELECT statement for the View, but you still need to add the DROP/CREATE View manually
	SELECT @queryText

END

--drop view _church_ccv_v_HeadcountMetrics
--go
--CREATE VIEW _church_ccv_v_HeadcountMetrics
--AS

