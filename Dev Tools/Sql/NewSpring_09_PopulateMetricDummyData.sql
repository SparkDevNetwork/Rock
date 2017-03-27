/* ====================================================== 
-- NewSpring Populate Metrics: 
-- Populates all metrics with values
  
--  Assumptions:
--  Values only calculated for the previous Sunday. {Total} 
--  metrics will receive one value, while {Service} metrics 
--  will receive values for each service.

   ====================================================== */
-- Make sure you're using the right Rock database:

--USE Rock

SET NOCOUNT ON

DECLARE @msg nvarchar(255)
DECLARE @IsSystem bit = 0
DECLARE @True bit = 1
DECLARE @False bit = 0
DECLARE @Order int = 0
DECLARE @CampusId int
DECLARE @MetricName nvarchar(255)
DECLARE @MetricDescription nvarchar(255)
DECLARE @MetricId int

-- pick the first campus in the db
SELECT TOP 1 @CampusId = [Id] FROM [Campus]	

DECLARE @9am varchar(5) = '09:15'
DECLARE @11am varchar(5) = '11:15'
DECLARE @4pm varchar(5) = '16:00'
DECLARE @6pm varchar(5) = '18:00'

DECLARE @SomeSunday datetime
DECLARE @LastSunday date = DATEADD( 
	DAY, -((DATEPART(DW, GETDATE()) + 6) % 7), GETDATE()
)

-- don't randomize the date for now
--SELECT @SomeSunday = DATEADD(dd, DATEDIFF(dd, @scopeIndex * 7, @LastSunday), 0)

SELECT @SomeSunday = @LastSunday

DECLARE @scopeIndex int, @numItems int, @numSundays int = 52
SELECT @scopeIndex = min(Id) FROM Metric
SELECT @numItems = count(1) + @scopeIndex FROM Metric

WHILE @scopeIndex < @numItems
BEGIN
	
	SELECT @MetricId = @scopeIndex

	-- log the description since a lot of metrics have the same name
	SELECT @MetricName = Title, @MetricDescription = SUBSTRING(Description, 17, LEN(Description) - 16)
	FROM Metric
	WHERE Id = @MetricId

	IF @MetricName <> ''
	BEGIN
		
		SELECT @msg = 'Creating values for ' + @MetricDescription
		RAISERROR ( @msg, 0, 0 ) WITH NOWAIT

		-- create a random, single, large number for totals
		IF @MetricName LIKE '%Total%'
		BEGIN

			INSERT [MetricValue] (MetricId, MetricValueType, YValue, [Order], MetricValueDateTime, EntityId, [Guid])
			VALUES (@MetricId, @False, CAST(ABS(RAND() * 1000) AS INT), @Order, @SomeSunday + '00:00', @CampusId, NEWID() )

		END
		-- create a random, smaller number for uniques
		ELSE IF @MetricName LIKE '%Unique%'
		BEGIN

			INSERT [MetricValue] (MetricId, MetricValueType, YValue, [Order], MetricValueDateTime, EntityId, [Guid])
			VALUES (@MetricId, @False, CAST(ABS(RAND() * 100) AS INT), @Order, @SomeSunday + '00:00', @CampusId, NEWID() )

		END
		-- create random, smaller numbers for each service time
		ELSE BEGIN

			INSERT [MetricValue] (MetricId, MetricValueType, YValue, [Order], MetricValueDateTime, EntityId, [Guid])
			VALUES (@MetricId, @False, CAST(ABS(RAND() * 100) AS INT), @Order, @SomeSunday + @9am, @CampusId, NEWID() )

			INSERT [MetricValue] (MetricId, MetricValueType, YValue, [Order], MetricValueDateTime, EntityId, [Guid])
			VALUES (@MetricId, @False, CAST(ABS(RAND() * 100) AS INT), @Order, @SomeSunday + @11am, @CampusId, NEWID() )
	
			INSERT [MetricValue] (MetricId, MetricValueType, YValue, [Order], MetricValueDateTime, EntityId, [Guid])
			VALUES (@MetricId, @False, CAST(ABS(RAND() * 100) AS INT), @Order, @SomeSunday + @4pm, @CampusId, NEWID() )

			INSERT [MetricValue] (MetricId, MetricValueType, YValue, [Order], MetricValueDateTime, EntityId, [Guid])
			VALUES (@MetricId, @False, CAST(ABS(RAND() * 100) AS INT), @Order, @SomeSunday + @6pm, @CampusId, NEWID() )	

		END

	END
	ELSE BEGIN
		SELECT @msg = 'Could not find metric ' + @MetricName
		RAISERROR ( @msg, 0, 0 ) WITH NOWAIT
	END
	
	SELECT @scopeIndex = @scopeIndex + 1
END






	
		
