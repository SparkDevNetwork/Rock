declare
    @entityTypeIdMetricCategoryType int = (select id from EntityType where Name = 'Rock.Model.MetricCategory'),
    @parentCategoryId int = null,
    @categoryName nvarchar(max),
    @iconCssClass nvarchar(max) = null,
    @categoryId int = null,
    @order int = 0,
    @metricId int = null,
    @metricValueTypeMeasure int = 0,
    @metricValueTypeGoal int = 1,
    @entityTypeIdCampus int = (select id from EntityType where Name = 'Rock.Model.Campus'),
    @entityTypeIdGroup int = (select id from EntityType where Name = 'Rock.Model.Group'),
    @campusIdMain int,
    @campusIdEast int,
    @campusIdWest int,
    @metricGuidFirstTimeVisitors uniqueidentifier = '41C1D067-0F4A-4D33-84A3-A8144D8F5418',
    @metricGuidFreeLunches uniqueidentifier = '5C52B8D2-C94F-43F3-AF22-28A0A1D3C791',
    @metricGuidAttendance uniqueidentifier = 'D4752628-DFC9-4681-ADB3-01936B8F38CA',
    @metricGuidCats uniqueidentifier = 'AAA21D1C-3F58-494D-B039-236E21C99595'
begin

    delete from MetricValue   
    delete from Metric
    delete from MetricCategory
    delete from Category where EntityTypeId in (@entityTypeIdMetricCategoryType)  

    if not exists (select * from Campus where Guid = '69EB5895-F1E7-4D73-ABBD-63DDDEC75582') begin
        insert into [Campus] ([Name], [IsSystem], [Guid]) values ('East', 0, '69EB5895-F1E7-4D73-ABBD-63DDDEC75582');
    end;

    if not exists (select * from Campus where Guid = 'B188D509-FAF6-4388-ADB8-478926427F8A') begin    
        insert into [Campus] ([Name], [IsSystem], [Guid]) values ('West', 0, 'B188D509-FAF6-4388-ADB8-478926427F8A');
    end;

    select @campusIdMain = id from [Campus] where [Guid]  = '76882AE3-1CE8-42A6-A2B6-8C0B29CF8CF8';
    select @campusIdEast = id from [Campus] where [Guid]  = '69EB5895-F1E7-4D73-ABBD-63DDDEC75582';
    select @campusIdWest = id from [Campus] where [Guid]  = 'B188D509-FAF6-4388-ADB8-478926427F8A';

   -- root cats
   INSERT INTO [Category]([IsSystem],[ParentCategoryId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],[Name],[Guid],[IconCssClass],[Order])
     VALUES
      (0,@parentCategoryId,@entityTypeIdMetricCategoryType,null,null,'Person Metrics',NEWID(),'fa fa-user',0),
      (0,@parentCategoryId,@entityTypeIdMetricCategoryType,null,null,'Group Metrics',NEWID(),'fa fa-users',0),
      (0,@parentCategoryId,@entityTypeIdMetricCategoryType,null,null,'Financial Metrics',NEWID(),'fa fa-money',0) 

   -- 2nd level cats
   set @parentCategoryId = (select id from Category where Name = 'Person Metrics' and EntityTypeId = @entityTypeIdMetricCategoryType)
   INSERT INTO [Category]([IsSystem],[ParentCategoryId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],[Name],[Guid],[IconCssClass],[Order])
     VALUES
      (0,@parentCategoryId,@entityTypeIdMetricCategoryType,null,null,'Neighborhoods',NEWID(),'fa fa-building-o',0),
      (0,@parentCategoryId,@entityTypeIdMetricCategoryType,null,null,'Missions',NEWID(),'fa fa-globe',0),
      (0,@parentCategoryId,@entityTypeIdMetricCategoryType,null,null,'Jr. High',NEWID(),'fa fa-question-circle',0) 


   -- example metric: First Time Visitors
   INSERT INTO [Metric]([IsSystem],[Title],[Description],[XAxisLabel],[YAxisLabel],[IconCssClass],[IsCumulative],[Guid])
     VALUES 
        (0,'First Time Visitors Per Week','First Time Visitors counts broken down into weeks', 'Weekend', 'Visitors', 'fa fa-gift', 0, @metricGuidFirstTimeVisitors)

    set @metricId = (select Id from Metric where Guid = @metricGuidFirstTimeVisitors);

    set @categoryId = (select id from Category where Name = 'Person Metrics' and EntityTypeId = @entityTypeIdMetricCategoryType )    
    insert into [MetricCategory] ([MetricId], [CategoryId], [Order], [Guid])
      values ( @metricId, @categoryId, 0, NEWID())

    set @categoryId = (select id from Category where Name = 'Group Metrics' and EntityTypeId = @entityTypeIdMetricCategoryType )    
    insert into [MetricCategory] ([MetricId], [CategoryId], [Order], [Guid])
      values ( @metricId, @categoryId, 0, NEWID())

    -- example metric: Free Lunches    
    set @categoryId = (select id from Category where Name = 'Person Metrics' and EntityTypeId = @entityTypeIdMetricCategoryType )    
    INSERT INTO [Metric]([IsSystem],[Title],[Description],[XAxisLabel],[YAxisLabel],[IconCssClass], [IsCumulative], [EntityTypeId],[Guid])
     VALUES 
        (0,'Free Lunches','Number of Free Lunches per day', 'Week', 'Meals Served', 'fa fa-thumbs-up', 1, @entityTypeIdGroup, @metricGuidFreeLunches)

    set @metricId = (select Id from Metric where Guid = @metricGuidFreeLunches);
    
    insert into [MetricCategory] ([MetricId], [CategoryId], [Order], [Guid])
        values ( @metricId, @categoryId, 0, NEWID())

INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [MetricValueDateTime], [XValue], [YValue], [Order], [Guid])
        values 
( @metricId, @metricValueTypeMeasure, '03/01/2013', 0, 400, 0, NEWID()),
( @metricId, @metricValueTypeMeasure, '04/01/2013', 0, 500, 0, NEWID()),
( @metricId, @metricValueTypeMeasure, '05/01/2013', 0, 600, 0, NEWID()),
( @metricId, @metricValueTypeMeasure, '06/01/2013', 0, 700, 0, NEWID()),
( @metricId, @metricValueTypeMeasure, '07/01/2013', 0, 300, 0, NEWID()),
( @metricId, @metricValueTypeMeasure, '08/01/2013', 0, 200, 0, NEWID()),
( @metricId, @metricValueTypeMeasure, '09/01/2013', 0, 100, 0, NEWID()),
( @metricId, @metricValueTypeMeasure, '10/01/2013', 0, 50, 0, NEWID()),
( @metricId, @metricValueTypeMeasure, '10/02/2013', 0, 1.9199065065406540, 0, NEWID()),
( @metricId, @metricValueTypeMeasure, '11/01/2013', 0, 9, 0, NEWID()),
( @metricId, @metricValueTypeMeasure, '12/01/2013', 0, 500, 0, NEWID()),
( @metricId, @metricValueTypeMeasure, '01/01/2014', 0, 150, 0, NEWID()),
( @metricId, @metricValueTypeMeasure, '02/01/2014', 0, 2, 0, NEWID()),
( @metricId, @metricValueTypeMeasure, '03/01/2014', 0, 613, 0, NEWID()),
( @metricId, @metricValueTypeMeasure, '04/01/2014', 0, 13, 0, NEWID()),
( @metricId, @metricValueTypeMeasure, '05/01/2014', 0, 245, 0, NEWID()),
( @metricId, @metricValueTypeMeasure, '06/01/2014', 0, 197, 0, NEWID()),
( @metricId, @metricValueTypeMeasure, '07/01/2014', 0, 42, 0, NEWID()),
( @metricId, @metricValueTypeMeasure, '08/01/2014', 0, 71, 0, NEWID()),
( @metricId, @metricValueTypeMeasure, '09/01/2014', 0, 49, 0, NEWID()),
( @metricId, @metricValueTypeMeasure, '10/01/2014', 0, 68, 0, NEWID()),
( @metricId, @metricValueTypeMeasure, '11/01/2014', 0, 17, 0, NEWID()),
( @metricId, @metricValueTypeMeasure, '11/01/2014', 0, -50, 0, NEWID()),
( @metricId, @metricValueTypeGoal, '03/01/2013', 0, 100, 0, NEWID()),
( @metricId, @metricValueTypeGoal, '04/01/2013', 0, 100, 0, NEWID()),
( @metricId, @metricValueTypeGoal, '05/01/2013', 0, 110, 0, NEWID()),
( @metricId, @metricValueTypeGoal, '06/01/2013', 0, 110, 0, NEWID()),
( @metricId, @metricValueTypeGoal, '07/01/2013', 0, 120, 0, NEWID()),
( @metricId, @metricValueTypeGoal, '08/01/2013', 0, 120, 0, NEWID()),
( @metricId, @metricValueTypeGoal, '09/01/2013', 0, 130, 0, NEWID()),
( @metricId, @metricValueTypeGoal, '10/01/2013', 0, 130, 0, NEWID()),
( @metricId, @metricValueTypeGoal, '11/01/2013', 0, 140, 0, NEWID()),
( @metricId, @metricValueTypeGoal, '12/01/2013', 0, 140, 0, NEWID()),
( @metricId, @metricValueTypeGoal, '01/01/2014', 0, 150, 0, NEWID()),
( @metricId, @metricValueTypeGoal, '02/01/2014', 0, 150, 0, NEWID()),
( @metricId, @metricValueTypeGoal, '03/01/2014', 0, 160, 0, NEWID()),
( @metricId, @metricValueTypeGoal, '04/01/2014', 0, 160, 0, NEWID()),
( @metricId, @metricValueTypeGoal, '05/01/2014', 0, 175, 0, NEWID()),
( @metricId, @metricValueTypeGoal, '06/01/2014', 0, 175, 0, NEWID()),
( @metricId, @metricValueTypeGoal, '07/01/2014', 0, 190, 0, NEWID()),
( @metricId, @metricValueTypeGoal, '08/01/2014', 0, 190, 0, NEWID()),
( @metricId, @metricValueTypeGoal, '09/01/2014', 0, 205, 0, NEWID()),
( @metricId, @metricValueTypeGoal, '10/01/2014', 0, 205, 0, NEWID()),
( @metricId, @metricValueTypeGoal, '11/01/2014', 0, 225, 0, NEWID()),
( @metricId, @metricValueTypeGoal, '12/01/2014', 0, 225, 0, NEWID()),
( @metricId, @metricValueTypeGoal, '01/01/2015', 0, 245, 0, NEWID()),
( @metricId, @metricValueTypeGoal, '02/01/2015', 0, 245, 0, NEWID()),
( @metricId, @metricValueTypeGoal, '03/01/2015', 0, 270, 0, NEWID()),
( @metricId, @metricValueTypeGoal, '04/01/2015', 0, 270, 0, NEWID())


-- example metric: Cat Owners Per Week
    set @categoryId = (select id from Category where Name = 'Person Metrics' and EntityTypeId = @entityTypeIdMetricCategoryType )    
    INSERT INTO [Metric]([IsSystem],[Title],[Description],[XAxisLabel],[YAxisLabel],[IconCssClass], [IsCumulative], [EntityTypeId],[Guid])
     VALUES 
        (0,'Cat Owners','Number of cat owners that showed up for church', 'Week', 'Cat Owners', 'fa fa-thumbs-up', 1, @entityTypeIdCampus, @metricGuidCats)

    set @metricId = (select Id from Metric where Guid = @metricGuidCats);
    
    insert into [MetricCategory] ([MetricId], [CategoryId], [Order], [Guid])
        values ( @metricId, @categoryId, 0, NEWID())

INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [MetricValueDateTime], [XValue], [YValue], [Order], [EntityId], [Guid])
        values 
( @metricId, @metricValueTypeMeasure, '03/01/2013', 0, floor(rand() * 1000), 0, @campusIdMain, NEWID()),
( @metricId, @metricValueTypeMeasure, '04/01/2013', 0, floor(rand() * 1000), 0, @campusIdMain, NEWID()),
( @metricId, @metricValueTypeMeasure, '05/01/2013', 0, floor(rand() * 1000), 0, @campusIdMain, NEWID()),
( @metricId, @metricValueTypeMeasure, '06/01/2013', 0, floor(rand() * 1000), 0, @campusIdMain, NEWID()),
( @metricId, @metricValueTypeMeasure, '07/01/2013', 0, floor(rand() * 1000), 0, @campusIdMain, NEWID()),
( @metricId, @metricValueTypeMeasure, '08/01/2013', 0, floor(rand() * 1000), 0, @campusIdMain, NEWID()),
( @metricId, @metricValueTypeMeasure, '09/01/2013', 0, floor(rand() * 1000), 0, @campusIdMain, NEWID()),
( @metricId, @metricValueTypeMeasure, '10/01/2013', 0, floor(rand() * 1000), 0, @campusIdMain, NEWID()),
( @metricId, @metricValueTypeMeasure, '10/02/2013', 0, floor(rand() * 1000), 0, @campusIdMain, NEWID()),
( @metricId, @metricValueTypeMeasure, '11/01/2013', 0, floor(rand() * 1000), 0, @campusIdMain, NEWID()),
( @metricId, @metricValueTypeMeasure, '12/01/2013', 0, floor(rand() * 1000), 0, @campusIdMain, NEWID()),
( @metricId, @metricValueTypeMeasure, '01/01/2014', 0, floor(rand() * 1000), 0, @campusIdMain, NEWID()),
( @metricId, @metricValueTypeMeasure, '02/01/2014', 0, floor(rand() * 1000), 0, @campusIdMain, NEWID()),
( @metricId, @metricValueTypeMeasure, '03/01/2014', 0, floor(rand() * 1000), 0, @campusIdMain, NEWID()),
( @metricId, @metricValueTypeMeasure, '04/01/2014', 0, floor(rand() * 1000), 0, @campusIdMain, NEWID()),
( @metricId, @metricValueTypeMeasure, '05/01/2014', 0, floor(rand() * 1000), 0, @campusIdMain, NEWID()),
( @metricId, @metricValueTypeMeasure, '06/01/2014', 0, floor(rand() * 1000), 0, @campusIdMain, NEWID()),
( @metricId, @metricValueTypeMeasure, '07/01/2014', 0, floor(rand() * 1000), 0, @campusIdMain, NEWID()),
( @metricId, @metricValueTypeMeasure, '08/01/2014', 0, floor(rand() * 1000), 0, @campusIdMain, NEWID()),
( @metricId, @metricValueTypeMeasure, '09/01/2014', 0, floor(rand() * 1000), 0, @campusIdMain, NEWID()),
( @metricId, @metricValueTypeMeasure, '10/01/2014', 0, floor(rand() * 1000), 0, @campusIdMain, NEWID()),
( @metricId, @metricValueTypeMeasure, '11/01/2014', 0, floor(rand() * 1000), 0, @campusIdMain, NEWID()),
( @metricId, @metricValueTypeMeasure, '11/01/2014', 0, floor(rand() * 1000), 0, @campusIdMain, NEWID()),
( @metricId, @metricValueTypeMeasure, '03/01/2013', 0, floor(rand() * 1000), 0, @campusIdEast, NEWID()),
( @metricId, @metricValueTypeMeasure, '04/01/2013', 0, floor(rand() * 1000), 0, @campusIdEast, NEWID()),
( @metricId, @metricValueTypeMeasure, '05/01/2013', 0, floor(rand() * 1000), 0, @campusIdEast, NEWID()),
( @metricId, @metricValueTypeMeasure, '06/01/2013', 0, floor(rand() * 1000), 0, @campusIdEast, NEWID()),
( @metricId, @metricValueTypeMeasure, '07/01/2013', 0, floor(rand() * 1000), 0, @campusIdEast, NEWID()),
( @metricId, @metricValueTypeMeasure, '08/01/2013', 0, floor(rand() * 1000), 0, @campusIdEast, NEWID()),
( @metricId, @metricValueTypeMeasure, '09/01/2013', 0, floor(rand() * 1000), 0, @campusIdEast, NEWID()),
( @metricId, @metricValueTypeMeasure, '10/01/2013', 0, floor(rand() * 1000), 0, @campusIdEast, NEWID()),
( @metricId, @metricValueTypeMeasure, '10/02/2013', 0, floor(rand() * 1000), 0, @campusIdEast, NEWID()),
( @metricId, @metricValueTypeMeasure, '11/01/2013', 0, floor(rand() * 1000), 0, @campusIdEast, NEWID()),
( @metricId, @metricValueTypeMeasure, '12/01/2013', 0, floor(rand() * 1000), 0, @campusIdEast, NEWID()),
( @metricId, @metricValueTypeMeasure, '01/01/2014', 0, floor(rand() * 1000), 0, @campusIdEast, NEWID()),
( @metricId, @metricValueTypeMeasure, '02/01/2014', 0, floor(rand() * 1000), 0, @campusIdEast, NEWID()),
( @metricId, @metricValueTypeMeasure, '03/01/2014', 0, floor(rand() * 1000), 0, @campusIdEast, NEWID()),
( @metricId, @metricValueTypeMeasure, '04/01/2014', 0, floor(rand() * 1000), 0, @campusIdEast, NEWID()),
( @metricId, @metricValueTypeMeasure, '05/01/2014', 0, floor(rand() * 1000), 0, @campusIdEast, NEWID()),
( @metricId, @metricValueTypeMeasure, '06/01/2014', 0, floor(rand() * 1000), 0, @campusIdEast, NEWID()),
( @metricId, @metricValueTypeMeasure, '07/01/2014', 0, floor(rand() * 1000), 0, @campusIdEast, NEWID()),
( @metricId, @metricValueTypeMeasure, '08/01/2014', 0, floor(rand() * 1000), 0, @campusIdEast, NEWID()),
( @metricId, @metricValueTypeMeasure, '09/01/2014', 0, floor(rand() * 1000), 0, @campusIdEast, NEWID()),
( @metricId, @metricValueTypeMeasure, '10/01/2014', 0, floor(rand() * 1000), 0, @campusIdEast, NEWID()),
( @metricId, @metricValueTypeMeasure, '11/01/2014', 0, floor(rand() * 1000), 0, @campusIdEast, NEWID()),
( @metricId, @metricValueTypeMeasure, '11/01/2014', 0, floor(rand() * 1000), 0, @campusIdEast, NEWID()),
( @metricId, @metricValueTypeMeasure, '03/01/2013', 0, floor(rand() * 1000), 0, @campusIdWest, NEWID()),
( @metricId, @metricValueTypeMeasure, '04/01/2013', 0, floor(rand() * 1000), 0, @campusIdWest, NEWID()),
( @metricId, @metricValueTypeMeasure, '05/01/2013', 0, floor(rand() * 1000), 0, @campusIdWest, NEWID()),
( @metricId, @metricValueTypeMeasure, '06/01/2013', 0, floor(rand() * 1000), 0, @campusIdWest, NEWID()),
( @metricId, @metricValueTypeMeasure, '07/01/2013', 0, floor(rand() * 1000), 0, @campusIdWest, NEWID()),
( @metricId, @metricValueTypeMeasure, '08/01/2013', 0, floor(rand() * 1000), 0, @campusIdWest, NEWID()),
( @metricId, @metricValueTypeMeasure, '09/01/2013', 0, floor(rand() * 1000), 0, @campusIdWest, NEWID()),
( @metricId, @metricValueTypeMeasure, '10/01/2013', 0, floor(rand() * 1000), 0, @campusIdWest, NEWID()),
( @metricId, @metricValueTypeMeasure, '10/02/2013', 0, floor(rand() * 1000), 0, @campusIdWest, NEWID()),
( @metricId, @metricValueTypeMeasure, '11/01/2013', 0, floor(rand() * 1000), 0, @campusIdWest, NEWID()),
( @metricId, @metricValueTypeMeasure, '12/01/2013', 0, floor(rand() * 1000), 0, @campusIdWest, NEWID()),
( @metricId, @metricValueTypeMeasure, '01/01/2014', 0, floor(rand() * 1000), 0, @campusIdWest, NEWID()),
( @metricId, @metricValueTypeMeasure, '02/01/2014', 0, floor(rand() * 1000), 0, @campusIdWest, NEWID()),
( @metricId, @metricValueTypeMeasure, '03/01/2014', 0, floor(rand() * 1000), 0, @campusIdWest, NEWID()),
( @metricId, @metricValueTypeMeasure, '04/01/2014', 0, floor(rand() * 1000), 0, @campusIdWest, NEWID()),
( @metricId, @metricValueTypeMeasure, '05/01/2014', 0, floor(rand() * 1000), 0, @campusIdWest, NEWID()),
( @metricId, @metricValueTypeMeasure, '06/01/2014', 0, floor(rand() * 1000), 0, @campusIdWest, NEWID()),
( @metricId, @metricValueTypeMeasure, '07/01/2014', 0, floor(rand() * 1000), 0, @campusIdWest, NEWID()),
( @metricId, @metricValueTypeMeasure, '08/01/2014', 0, floor(rand() * 1000), 0, @campusIdWest, NEWID()),
( @metricId, @metricValueTypeMeasure, '09/01/2014', 0, floor(rand() * 1000), 0, @campusIdWest, NEWID()),
( @metricId, @metricValueTypeMeasure, '10/01/2014', 0, floor(rand() * 1000), 0, @campusIdWest, NEWID()),
( @metricId, @metricValueTypeMeasure, '11/01/2014', 0, floor(rand() * 1000), 0, @campusIdWest, NEWID()),
( @metricId, @metricValueTypeMeasure, '11/01/2014', 0, floor(rand() * 1000), 0, @campusIdWest, NEWID()),
( @metricId, @metricValueTypeGoal, '03/01/2013', 0, floor(rand() * 1000), 0, null, NEWID()),
( @metricId, @metricValueTypeGoal, '04/01/2013', 0, floor(rand() * 1000), 0, null, NEWID()),
( @metricId, @metricValueTypeGoal, '05/01/2013', 0, floor(rand() * 1000), 0, null, NEWID()),
( @metricId, @metricValueTypeGoal, '06/01/2013', 0, floor(rand() * 1000), 0, null, NEWID()),
( @metricId, @metricValueTypeGoal, '07/01/2013', 0, floor(rand() * 1000), 0, null, NEWID()),
( @metricId, @metricValueTypeGoal, '08/01/2013', 0, floor(rand() * 1000), 0, null, NEWID()),
( @metricId, @metricValueTypeGoal, '09/01/2013', 0, floor(rand() * 1000), 0, null, NEWID()),
( @metricId, @metricValueTypeGoal, '10/01/2013', 0, floor(rand() * 1000), 0, null, NEWID()),
( @metricId, @metricValueTypeGoal, '10/02/2013', 0, floor(rand() * 1000), 0, null, NEWID()),
( @metricId, @metricValueTypeGoal, '11/01/2013', 0, floor(rand() * 1000), 0, null, NEWID()),
( @metricId, @metricValueTypeGoal, '12/01/2013', 0, floor(rand() * 1000), 0, null, NEWID()),
( @metricId, @metricValueTypeGoal, '01/01/2014', 0, floor(rand() * 1000), 0, null, NEWID()),
( @metricId, @metricValueTypeGoal, '02/01/2014', 0, floor(rand() * 1000), 0, null, NEWID()),
( @metricId, @metricValueTypeGoal, '03/01/2014', 0, floor(rand() * 1000), 0, null, NEWID()),
( @metricId, @metricValueTypeGoal, '04/01/2014', 0, floor(rand() * 1000), 0, null, NEWID()),
( @metricId, @metricValueTypeGoal, '05/01/2014', 0, floor(rand() * 1000), 0, null, NEWID()),
( @metricId, @metricValueTypeGoal, '06/01/2014', 0, floor(rand() * 1000), 0, null, NEWID()),
( @metricId, @metricValueTypeGoal, '07/01/2014', 0, floor(rand() * 1000), 0, null, NEWID()),
( @metricId, @metricValueTypeGoal, '08/01/2014', 0, floor(rand() * 1000), 0, null, NEWID()),
( @metricId, @metricValueTypeGoal, '09/01/2014', 0, floor(rand() * 1000), 0, null, NEWID()),
( @metricId, @metricValueTypeGoal, '10/01/2014', 0, floor(rand() * 1000), 0, null, NEWID()),
( @metricId, @metricValueTypeGoal, '11/01/2014', 0, floor(rand() * 1000), 0, null, NEWID()),
( @metricId, @metricValueTypeGoal, '11/01/2014', 0, floor(rand() * 1000), 0, null, NEWID())


-- example metric : Adult Attendence    
    set @categoryId = (select id from Category where Name = 'Person Metrics' and EntityTypeId = @entityTypeIdMetricCategoryType )    
    INSERT INTO [Metric]([IsSystem],[Title],[Description],[XAxisLabel],[YAxisLabel],[IconCssClass], [IsCumulative],[EntityTypeId], [Guid])
     VALUES 
        (0,'Adult Attendence','Number of adults in the weekend service', 'Week Date', 'Adults', 'fa fa-thumbs-up', 1, @entityTypeIdCampus, @metricGuidAttendance)

    set @metricId = (select Id from Metric where Guid = @metricGuidAttendance);
    
    insert into [MetricCategory] ([MetricId], [CategoryId], [Order], [Guid])
        values ( @metricId, @categoryId, 0, NEWID())



INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',5376,0,'','Dec  4 2006 12:00AM','E22B3616-3516-4E23-A3D2-090D2905A2B8')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',5146,0,'','Dec 10 2006 12:00AM','7D46B549-31A8-4BF5-B99A-4D96C9CEAD4D')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',5569,0,'','Dec 17 2006 12:00AM','72DA0F64-091E-4259-B318-D658D22E940F')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',5379,0,'','Dec 30 2006 12:00AM','D70F622D-0AFA-489A-9D92-D1C8662B60B8')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6055,0,'','Jan  7 2007 12:00AM','3F4C5C45-D5FA-40F4-9FF4-55816DC7D7DE')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',5757,0,'','Jan 14 2007 12:00AM','4CE974F0-F754-4E23-9D81-EB3D273DEC24')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',5649,0,'','Jan 21 2007 12:00AM','4DD954E8-0F3E-4C6D-AEFF-2A96CE2B8E08')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',5755,0,'','Jan 28 2007 12:00AM','3F6BD03E-682F-4DD8-AD90-A33A1CE45A14')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',5690,0,'','Feb  4 2007 12:00AM','D0227E56-0EA1-49E2-AB4E-94C9364A5D32')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',5848,0,'','Feb 11 2007 12:00AM','E6F323B6-263D-4AD1-80B5-8025AA72BA21')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',5895,0,'','Feb 18 2007 12:00AM','4F7FE708-F9F6-459C-AF98-5814D06E0F87')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6338,0,'','Feb 25 2007 12:00AM','D8EC5771-7854-4BA5-BB70-490BEDE49D94')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6105,0,'','Mar  4 2007 12:00AM','6FFEF7BE-50D1-4039-8623-F2E9AA512127')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',5683,0,'','Mar 11 2007 12:00AM','6A52F1D0-B3DE-4D1C-9F11-19DCAFB2E114')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',5653,0,'','Mar 18 2007 12:00AM','290D55A9-B921-4FA9-B6FB-5F295C10DE2B')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',5869,0,'','Mar 25 2007 12:00AM','7D874489-7664-4700-9769-5598CDF4E4E4')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6275,0,'','Apr  1 2007 12:00AM','251996D1-87BA-4692-B254-A13721856E79')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',14477,0,'Easter - Included Good Friday service w/ 1250, 8am service w/ 1494 and Sunrise 942','Apr  8 2007 12:00AM','9B7D3396-0574-4AC5-A067-96FB724515CF')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6130,0,'','Apr 15 2007 12:00AM','F1E338A6-E31F-4FE7-A092-4A1310E97050')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6207,0,'','Apr 22 2007 12:00AM','0AD3ED4F-4B16-4602-BE65-EC4EDDA5AF7A')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',5579,0,'','Apr 29 2007 12:00AM','648643D8-BF98-49E2-922F-EDE8082F6D81')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',5292,0,'','May  6 2007 12:00AM','1B831AF7-FA6C-4EB0-937B-34773C6533BE')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6090,0,'','May 13 2007 12:00AM','0DC3FCF9-F1F5-4AEE-B3E0-4ECDBB9801E9')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',5862,0,'','May 20 2007 12:00AM','06EAE71F-5A00-4FC5-95C0-4F322CD22D20')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',5388,0,'','May 27 2007 12:00AM','92F65699-62ED-4088-87D5-977B3CB466B0')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',5996,0,'','Jun  3 2007 12:00AM','F1F72830-315C-4701-AF20-5FAD4F787D2F')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',5914,0,'','Jun 10 2007 12:00AM','58F1A048-2BBF-4422-8E47-D8CCEF24602E')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',5740,0,'','Jun 17 2007 12:00AM','2B867D3B-7FD7-4AD2-8E73-58A1D6EF0F27')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',5741,0,'','Jun 24 2007 12:00AM','3D90CF89-1FBB-4E9C-8724-476046E02B50')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',5770,0,'','Jul  1 2007 12:00AM','540AB8F8-4FDA-48C5-B57E-802D3F3F45F2')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6113,0,'','Jul  8 2007 12:00AM','C222067D-0D1E-4AC3-9862-0E2E27BE05E9')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6233,0,'','Jul 15 2007 12:00AM','E63A8E95-8F0F-4340-BB2D-17E00B588C10')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',5143,0,'','Jul 22 2007 12:00AM','18587E8F-8B1F-4C00-A08F-C21B99537278')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6129,0,'','Jul 29 2007 12:00AM','88A70615-2FCA-422F-B9C6-6B4C7B1F6584')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6234,0,'','Aug  5 2007 12:00AM','E90E1E95-EF2B-4DCB-82BC-137586D4CCD2')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',7087,0,'','Aug 12 2007 12:00AM','CB7718CE-B62D-4AAB-B8CB-3CEFA2CBC383')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6742,0,'','Aug 19 2007 12:00AM','2323563B-BAF7-4DC7-AD3B-786F8F06F8A0')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6662,0,'','Aug 26 2007 12:00AM','E8BC315F-8806-4722-A798-6CE1386E24FB')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6059,0,'','Sep  2 2007 12:00AM','D3641089-5623-4BBC-8833-7AF86E10D3D6')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',11384,0,'','Sep  9 2007 12:00AM','0C2E9A4D-BD91-44F3-85D0-E7D6B2A961E6')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6330,0,'','Sep 16 2007 12:00AM','FD01ADE4-1448-4FB8-84A6-B1FB24D366CD')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',5940,0,'','Sep 23 2007 12:00AM','99F554F7-EC24-4A74-84BD-62BDA1709482')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',5704,0,'','Sep 30 2007 12:00AM','A0E7B9CF-6354-426B-AE87-CAA84FEF190D')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',5458,0,'','Oct  7 2007 12:00AM','B5E00BDF-3CDA-444B-A6E7-FDC352914B27')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6764,0,'','Oct 14 2007 12:00AM','5F24E9E2-F978-46F9-A95E-D7D1E6E57E0B')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',5986,0,'','Oct 21 2007 12:00AM','23338A1C-891A-460D-A58D-89C1A8F1C5BA')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',5755,0,'','Oct 28 2007 12:00AM','379C4ADA-6E67-45B0-9CCC-2C82DCFC522C')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',5637,0,'','Nov  4 2007 12:00AM','CCB4115C-EB09-4F52-92A5-84FB3C323D82')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',5550,0,'','Nov 11 2007 12:00AM','3716579F-04EE-4147-8ED6-39522C379B72')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',5588,0,'','Nov 18 2007 12:00AM','FCA3FA2B-9939-431A-90AD-157165358A19')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',5527,0,'','Nov 25 2007 12:00AM','49ECAE13-268C-4E07-A357-01E0AFE08935')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',5119,0,'','Dec  2 2007 12:00AM','6515D772-5DE9-4032-A7A8-41A8DABC036D')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',5939,0,'','Dec  9 2007 12:00AM','243900F9-9220-43DD-9229-04BDC68F86EB')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',5891,0,'','Dec 16 2007 12:00AM','18E1A070-9515-4ADF-A218-79BF7F9A44F1')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',5204,0,'','Dec 23 2007 12:00AM','AEB70A78-6573-4057-9B4B-7DD1EA543C34')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',5872,0,'','Dec 30 2007 12:00AM','8D9B6CED-4AED-40E5-9E83-DD4B24E454F8')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6366,0,'','Jan  6 2008 12:00AM','E6F2ECB4-301E-4319-8E62-2EFEE9A56DD5')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',5830,0,'','Jan 13 2008 12:00AM','155315E6-D7D4-4645-A372-064DBCD2D08E')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6523,0,'','Jan 20 2008 12:00AM','941B066F-C39D-4FC1-8837-284E43746A7E')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6011,0,'','Jan 27 2008 12:00AM','93333CDE-53C2-4CD1-A282-63C17CD6DBE6')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6306,0,'','Feb  3 2008 12:00AM','EF0BB5CB-0A74-4E42-B2C5-3107D8F7E721')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',5717,0,'','Feb 10 2008 12:00AM','E5C9EA7B-E741-4427-B81D-026E0EDFB4FA')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',5796,0,'','Feb 17 2008 12:00AM','68C8EB75-268F-4F53-89AE-6C49E15ADF30')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6226,0,'','Feb 24 2008 12:00AM','B1AA2835-9B4B-4C17-A0C0-495F8E7AEE37')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6195,0,'','Mar  2 2008 12:00AM','79DBFE0B-71B4-4704-836E-2895C9326D59')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6372,0,'','Mar  9 2008 12:00AM','AB449EB2-DED2-463A-A56A-0AD65E21029A')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6143,0,'','Mar 16 2008 12:00AM','99299565-DC45-4318-BD12-B4A0AC91B240')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',14821,0,'Easter - Included Good Friday service w/ 1095, 8am service w/ 1436 and Sunrise w/ 908','Mar 23 2008 12:00AM','1C25A430-DFE6-4591-B970-3438C40135CA')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6319,0,'','Mar 30 2008 12:00AM','371423F9-A9AF-4F56-9A4E-80CF878BF9D6')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6590,0,'','Apr  6 2008 12:00AM','FDB250CB-3243-49E5-9AE1-26CFCB6E2E87')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6714,0,'','Apr 13 2008 12:00AM','2ECEA764-EA0D-4E0F-A2D5-0A59C6CC8713')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6249,0,'','Apr 20 2008 12:00AM','DE58CF92-AA57-443A-9790-644586BA3099')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6390,0,'','Apr 27 2008 12:00AM','0DCE56F1-34C8-45D9-9CE6-08D1341C31BF')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6223,0,'','May  4 2008 12:00AM','F8B424DB-E6CE-428C-A92D-F4AC8025474B')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6260,0,'','May 11 2008 12:00AM','B32B573B-0FBF-4C6A-9F04-10EAB268EDBD')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',5998,0,'','May 18 2008 12:00AM','E4A1B30D-14EC-4455-8CD7-C2E8629E50D9')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6536,0,'','May 25 2008 12:00AM','661743B1-539F-4E09-A9CB-B76EFBB2DDC8')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6060,0,'','Jun  1 2008 12:00AM','9B52A50C-5DCD-4B8E-969C-6FF50E45AB6B')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',5912,0,'','Jun  8 2008 12:00AM','8876C2C9-ECB4-417D-8262-E83E892E5C06')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6165,0,'','Jun 15 2008 12:00AM','086084C8-3067-4D7A-9D5A-BF7738418ED0')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',8290,0,'Ken Ham - Answers in Genesis speaking','Jun 22 2008 12:00AM','E21CF080-2EFB-4E3D-ADB4-3A570B15C725')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6241,0,'','Jun 29 2008 12:00AM','259EEC09-D952-4D85-BBD7-9B4D3904F5F9')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',5751,0,'4th of July Weekend','Jul  6 2008 12:00AM','53D6203B-18AB-431F-A2FB-779274D71CAE')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',7014,0,'','Jul 13 2008 12:00AM','D25BE075-40BD-4615-A17B-CE1C778A5DB3')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6686,0,'Paul Wilson preached','Jul 20 2008 12:00AM','3039D242-A0D6-4D9B-AEA0-ECD8A1BA03A5')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6626,0,'','Jul 27 2008 12:00AM','DC04D0AB-B880-445C-8A19-73E47270A9D8')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6381,0,'','Aug  3 2008 12:00AM','30D1534E-0AED-433C-BEEE-886785F3A182')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',7790,0,'','Aug 10 2008 12:00AM','D54B2B2A-C31F-4969-8032-BC2636CAF8F8')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',7549,0,'','Aug 17 2008 12:00AM','29094ECE-EBB1-4740-B71D-1436349DDD1A')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',7740,0,'','Aug 24 2008 12:00AM','1231F166-6D40-4A9C-B1C9-C83EEE9B690D')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6624,0,'Labor Day weekend','Aug 31 2008 12:00AM','1876B5C9-AD23-4D2C-AA06-A1AA1C868E6B')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',7605,0,'','Sep  7 2008 12:00AM','806CF443-835D-49C7-AD24-426268F24DD8')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',5941,0,'World View Series began','Sep 14 2008 12:00AM','846A12BF-8B85-4963-887C-DFB5B5B0CE45')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6582,0,'','Sep 21 2008 12:00AM','CCDC6901-2D95-4317-879A-AC2C264CDCA1')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6326,0,'','Sep 28 2008 12:00AM','4BC98AE6-7176-4963-A4AF-47945D56F90B')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6295,0,'','Oct  5 2008 12:00AM','AC76D6D6-5698-4679-A6FD-CFD5D7C900AD')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6768,0,'','Oct 12 2008 12:00AM','BAEDD21E-604E-4A02-A6E9-E1EA48BED5DD')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',7140,0,'','Oct 19 2008 12:00AM','D27F9CBE-B76D-4D8D-A1A2-485B49982438')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6039,0,'','Oct 26 2008 12:00AM','C599FBCD-3D9E-4F31-BC5D-B0A9AF157A45')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6312,0,'','Nov  2 2008 12:00AM','FEECCCD9-DB69-4AFD-BF97-3EF5C7AAD425')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',7021,0,'','Nov  9 2008 12:00AM','2825E478-D9E3-4D0A-903C-8B73ABD23DC5')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',7506,0,'','Nov 16 2008 12:00AM','F991D4F1-1D01-4A78-BFDF-A44A2BA8CBED')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',7321,0,'','Nov 23 2008 12:00AM','6936953E-F000-4794-87D1-0F74C78A1775')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',7098,0,'','Nov 30 2008 12:00AM','AD6C2920-8641-43EE-BA9A-8E5ECA5BCF94')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6826,0,'','Dec  7 2008 12:00AM','6660CB16-739D-486C-A1A6-E335ED5588D0')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6676,0,'','Dec 14 2008 12:00AM','9D208460-2050-4D2B-A33D-A0616980A553')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6469,0,'','Dec 21 2008 12:00AM','F91D059A-F6BA-4A04-86BF-1A376B855702')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',15542,0,'2008 Christmas Services','Dec 25 2008 12:00AM','E3614D43-45E2-4373-9FE7-D6991232852A')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6534,0,'Formula for a Peaceful New Year - Bob Russell','Dec 28 2008 12:00AM','460016D1-8AEF-4E2F-91E2-18F49A5B8DB6')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6632,0,'','Jan  4 2009 12:00AM','9C57CD58-5617-47EC-92AC-BF37994F354E')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',7546,0,'Encounters (Satan) Dennis','Jan 11 2009 12:00AM','2B9E9B75-8D0C-4CBF-9094-FF656D5C55A5')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',7053,0,'','Jan 18 2009 12:00AM','88F36FCD-A0D8-4C07-8B7B-3EEC1C7200D2')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',7632,0,'Encounters (The Pharisees) Don','Jan 25 2009 12:00AM','014E1550-74FA-4364-9E2E-C33C7EFCD7F5')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',7751,0,'Superbowl Weekend','Feb  1 2009 12:00AM','A1C86E75-40E8-4D6B-9B8B-11C5C7807135')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',7213,0,'Encounters Feeding 5000 - PW','Feb  8 2009 12:00AM','280C5EE1-042C-4795-BB1B-E23C7BF94009')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',7081,0,'Valentines / Presidents Day Weekend - DW','Feb 15 2009 12:00AM','82DFEF7E-F4EF-469C-A6A0-E3833CB70732')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',7617,0,'Encounters (The Rich Man) Don','Feb 22 2009 12:00AM','CFFF68C1-AF12-4D5C-A9F8-9DE4AFC06EE3')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',7685,0,'Encounters (Zaccheus) Dennis','Mar  1 2009 12:00AM','7474772A-EB01-4375-B92F-A6A27781170C')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',7551,0,'Encounters (Peter & Judas) Don','Mar  8 2009 12:00AM','B0F8F1D7-5CCB-455F-A335-39507E991D93')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',7884,0,'','Mar 15 2009 12:00AM','07605420-9CF2-4E19-B9E2-A741B77B85E5')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',8077,0,'','Mar 22 2009 12:00AM','C1A4F3E1-1052-4477-8908-A698F25FA14F')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',7953,0,'','Mar 29 2009 12:00AM','CD333372-D1F9-475A-8BFB-D05BC3AC410F')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',8691,0,'Passionate Warrior - PW','Apr  5 2009 12:00AM','464C529B-C36F-4277-9EE0-1C44B55A3AD8')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',18571,0,'2009 EASTER - Includes Good Friday service w/ 1626, 8am service w/ 1686 and Sunrise service 1072','Apr 12 2009 12:00AM','AEB9CCB1-491A-4B07-8A3A-DF14A5433E38')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',11007,0,'INFLATE - Fireproof (Discovering Differences) Don','Apr 19 2009 12:00AM','5F9A969E-6E89-448A-A4F5-D8DA7EA5C87E')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10300,0,'Fireproof (Breaking Free) Dennis','Apr 26 2009 12:00AM','89AEF732-464B-44C4-A64E-12A1EDCBA956')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9858,0,'Fireproof (Letting Go) Don','May  3 2009 12:00AM','0EAD067E-2B52-44F4-B6F6-429ABE8016E3')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9449,0,'Mother''s Day','May 10 2009 12:00AM','E915C5C7-4886-47EF-88D4-ADDCF62F47E5')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9150,0,'','May 17 2009 12:00AM','9A13F52B-37C1-443C-8574-AC4928ECF5B4')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',8337,0,'Angels & Demons - Don (Memorial Day Weekend)','May 24 2009 12:00AM','5EAB8C63-2CC7-4E8F-B554-DDCC5B36B31F')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',8829,0,'Angels & Demons (B. Fesmire)','May 31 2009 12:00AM','C762A95C-53E5-489D-AA99-228F8C85472C')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',8353,0,'Heaven & Hell (D. Hinkle)','Jun  7 2009 12:00AM','131D442D-C13E-4509-881B-05059538A531')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',7794,0,'Heaven & Hell (D. Bloodworth)','Jun 14 2009 12:00AM','0C7ED82E-F4F5-4BF0-9285-99E0D6D528E0')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',7900,0,'Father''s Day','Jun 21 2009 12:00AM','8FC56162-B64A-44AF-9A63-1507F71BBE11')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',7610,0,'Missions Weekend 360','Jun 28 2009 12:00AM','FA56474C-F3B6-48B8-BFCB-75A661D783C7')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6315,0,'4th of July - Freedom','Jul  5 2009 12:00AM','A34E9529-FA20-4D59-898B-BCA56304A24A')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',7431,0,'','Jul 12 2009 12:00AM','EE071465-BC57-4BB6-B4D1-D440E5CDED0D')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',7827,0,'','Jul 19 2009 12:00AM','E8BE1EFA-25D6-483A-A630-4ADBBD87EE3C')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',8029,0,'','Jul 26 2009 12:00AM','F93B794F-7410-47D0-A036-D66A54BBCD04')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',7964,0,'','Aug  2 2009 12:00AM','86C38882-B053-4A19-889E-CAA81E9ADDE2')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',8050,0,'International Justice Mission - Gary Haugen','Aug  9 2009 12:00AM','BF3FE86F-2232-4979-9D3D-337C4B338629')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9442,0,'Revolutionary - Fall Kick Off','Aug 16 2009 12:00AM','AA13650A-CE47-464E-BA75-FD5F74BE0A7F')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',8712,0,'Revolutionary Christian - Don','Aug 23 2009 12:00AM','250E0210-9DAE-41DD-8238-EB4198CAADCA')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9026,0,'David Murrow - Why Men Hate Going to Church','Aug 30 2009 12:00AM','13A9644E-43A6-4864-B1F6-C3E1BA296326')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',7691,0,'Labor Day Weekend','Sep  6 2009 12:00AM','F5105D8C-B000-4977-B09C-83813465A42C')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',8637,0,'Overcome - Loneliness (Paul)','Sep 13 2009 12:00AM','5D9862E3-B2CC-48E0-83BE-D1509F5D1A9C')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',8684,0,'Overcome - Adversity (Don)','Sep 20 2009 12:00AM','5733B32C-824A-4712-A226-5F2269DC450A')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',16633,0,'Nick Vujicic - Overcoming Limitations','Sep 27 2009 12:00AM','AB20F964-63F0-40B0-A1FD-CAC743DAE8E1')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',8977,0,'WEST FEST - Overcoming Pride (Don)','Oct  4 2009 12:00AM','292C99A8-E1EF-47B6-96D1-3746599ABFFA')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',8401,0,'Overcome - Complacency (Don)','Oct 11 2009 12:00AM','6D40EF9C-7431-4CDE-B7F1-E0434BA61147')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9607,0,'Jim Caviezel - Hollywood','Oct 18 2009 12:00AM','315C6B17-E574-4CFA-A61F-F4AF440F6E12')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',8138,0,'Overcome - Distractions (Don)','Oct 25 2009 12:00AM','33206441-F9EA-4640-8E03-706173D25AB6')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6649,0,'Overcome - Worry (Paul Wilson) Halloween','Nov  1 2009 12:00AM','859DB3EE-7803-4121-B125-8843CCE1763B')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',7615,0,'Contagious Generousity - Barry Cameron','Nov  8 2009 12:00AM','34761C68-4A85-44DD-9E0A-E81999EA2EEC')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',8175,0,'Contagious Generousity (Don)','Nov 15 2009 12:00AM','45DD5C79-858F-48A5-8CFF-A69798064756')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',7860,0,'Contagious Generousity (Don)','Nov 22 2009 12:00AM','29E3498C-6670-444B-B139-F511BF3F61ED')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',7729,0,'Contagious Generousity (Don)','Nov 29 2009 12:00AM','CEA6EE42-CA19-4B50-8818-AD0804305155')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',7808,0,'Messiah - Prophecy Fulfilled (Don)','Dec  6 2009 12:00AM','77BD4735-3970-40FA-BD43-52A7871DF082')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',7946,0,'Messiah - Hope Proclaimed (Don)','Dec 13 2009 12:00AM','D0731B88-71EE-478C-8444-72D5104F40E9')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',8459,0,'Messiah - King Annointed (Don)','Dec 20 2009 12:00AM','A5A1F04B-253B-489F-9883-4516DC34557C')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',22803,0,'Christmas Symphony
12/23 6,889
12/24 15,314
','Dec 24 2009 12:00AM','099C4551-D636-4532-90CE-57FCE35B275F')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6540,0,'Resolution - Looking Back (Kevin Ingram)','Dec 27 2009 12:00AM','52779DB7-49E9-4D17-9C87-2F1D54ACCBEA')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',8752,0,'Resolution - Don W','Jan  3 2010 12:00AM','E7A33CA1-C7F3-4F6F-96F8-312978CA346B')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',8542,0,'State of the Church - DW','Jan 10 2010 12:00AM','8CB8BFD1-EF7A-4764-BB91-71930DF3971E')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',8271,0,'The Light - Receive It (Don)','Jan 17 2010 12:00AM','158904E1-2140-47D2-9F20-BF8F0ADA25A5')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9919,0,'The Light - Live It (Don)','Jan 24 2010 12:00AM','2313D708-8325-488B-872C-1D02A098C92E')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9126,0,'','Jan 31 2010 12:00AM','D02C73CD-824A-4836-BB59-F3CD050A4ECA')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9226,0,'','Feb  7 2010 12:00AM','428F4109-D514-4AD0-A3B4-84CAE5F8D704')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',8759,0,'Counterfeit Gods - Sex (DW)','Feb 14 2010 12:00AM','CBEE7C68-AD6F-4B30-A9D5-F9FBCA1A4127')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9243,0,'Counterfeit GODS - Power (Don)','Feb 21 2010 12:00AM','49B19092-1F20-469E-900D-9328C9A1A3A5')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9340,0,'Fearless - Fear (Paul W)','Feb 28 2010 12:00AM','2D05C1AE-A90F-4F9A-B47E-6EF4759D9EFA')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9014,0,'','Mar  7 2010 12:00AM','1D9783D8-9A69-4DF1-9CD6-4CB3BC990D09')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',8958,0,'Fearless/Fear of Overwhelming Challenges, part 3 - Paul Wilson','Mar 14 2010 12:00AM','BB67AF02-86A2-4DA7-9915-4C9F054CBBD7')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9267,0,'Fearless - DW','Mar 21 2010 12:00AM','2FA5EAEC-29EA-44DF-8D5A-78609A21E7D0')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9981,0,'Start 5 Services - Fearless','Mar 28 2010 12:00AM','15BCFD95-A14D-4792-8E6F-89E552D69056')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',22224,0,'2010 EASTER - Includes Good Friday service w/ 1666 and Sunrise service 2411','Apr  4 2010 12:00AM','E083930A-E99D-4D17-B121-4D3314FAE166')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',11442,0,'Inflate & George Barna - Revolutionary Parenting','Apr 11 2010 12:00AM','F9C1D37F-6DDA-484A-B893-695AD2EDC5FE')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',16691,0,'Kurt Warner - Putting First Things Firs - Revolutionary Parenting','Apr 18 2010 12:00AM','4143CAD8-E29E-481A-A816-9E4D0BA06952')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9143,0,'Drawing Lines in the Sand','Apr 25 2010 12:00AM','C83CBA29-617D-4675-858B-003B86180E5C')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9171,0,'','May  2 2010 12:00AM','99971F99-A055-42FD-9DB3-7B7E3E43CD1E')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9485,0,'Mother''s Day','May  9 2010 12:00AM','947CEEBE-D43F-40CE-A03D-2AD6B9819199')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9229,0,'Faith Under Pressure','May 16 2010 12:00AM','38016AED-B345-443D-A85A-425DAC520CE0')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9563,0,'Faith in Action','May 23 2010 12:00AM','B357C76B-4D90-4439-B565-F0DE013B5948')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',7704,0,'Memorial Day Weekend','May 30 2010 12:00AM','20168871-7D61-4520-8555-D3F4454107EE')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',8938,0,'Faith that Surrenders','Jun  6 2010 12:00AM','B5923ED3-B9B6-44EF-9FC4-FF5861066FAB')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',8775,0,'Faith that Prays','Jun 13 2010 12:00AM','222ACB3E-31DE-4068-9310-A975CF520832')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',8475,0,'Father''s Day (Don)','Jun 20 2010 12:00AM','7898B9F1-C186-478A-90F8-2F107731BADE')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',7953,0,'Missions Weekend','Jun 27 2010 12:00AM','44566965-C340-421E-9B08-5FDE3AA9B86C')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',7488,0,'This is Freedom - July 4th (Don)','Jul  4 2010 12:00AM','8418464F-EBC1-4144-9B84-377358B6F431')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',12015,0,'Power of One (Don) - includes youth in service','Jul 11 2010 12:00AM','D773DE20-CB60-4855-9737-F18C46B8C95F')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9145,0,'Bondservant - Trent Renner','Jul 18 2010 12:00AM','674AF6F9-5564-44FF-9A08-95E8DDDC4092')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',8729,0,'Re:Think (Don)','Jul 25 2010 12:00AM','BEE4263B-1F9A-40A8-BEC6-4C7E0EDF3B5E')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',8788,0,'RE:Wind - Dr. Jim Garlow','Aug  1 2010 12:00AM','29C85282-F2FF-4A46-88EA-5B1D9473EFDF')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9314,0,'','Aug  8 2010 12:00AM','A168D9BB-2352-4B2F-AD6C-9EA773B51F88')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10767,0,'Heroes - Fall Kick Off','Aug 15 2010 12:00AM','A642E030-536A-4BF0-8C83-AE15ABBD8E95')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10987,0,'Heroes - Abraham','Aug 22 2010 12:00AM','9481C73C-3D77-436D-83BC-A8C7468189F8')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10978,0,'Heroes - Joseph (Temptation)','Aug 29 2010 12:00AM','B031A576-0075-4918-B506-D65CAAA3E63B')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',8486,0,'Labor Day Weekend - Heroes Moses (Don)','Sep  5 2010 12:00AM','CC9954A8-370B-466D-BAFE-C3AC331593D7')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10400,0,'Heroes - Joshua - Law Enforcement (Don)','Sep 12 2010 12:00AM','97FEA14E-B77E-4C35-923E-54737E7EC354')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9798,0,'Heroes - David & Goliath','Sep 19 2010 12:00AM','3CF3A6DE-8687-43EA-AE81-34D96ACE8976')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10267,0,'Heroes - Elijah (Don)','Sep 26 2010 12:00AM','9026D901-99EF-40BD-8EC1-B5A04AD1EA9D')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10350,0,'Heroes - Nehemiah','Oct  3 2010 12:00AM','8BA99CC9-E805-43C1-B04D-7F194309C5FF')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9509,0,'Heroes - Ester','Oct 10 2010 12:00AM','9DBB52FE-007F-467B-9A39-91278C7C21CA')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10021,0,'Heroes - Daniel (Paul)','Oct 17 2010 12:00AM','73DC79A7-CA78-401A-883B-7C4D98220379')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10450,0,'Christian Atheist (Don)','Oct 24 2010 12:00AM','92FAE832-01AB-43A6-A797-A188B90A2E77')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9656,0,'Christian Atheist','Oct 31 2010 12:00AM','549C0FE7-6AF6-4470-8DFE-540F1152C3FF')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10470,0,'Christian Athiest: Forgiveness','Nov  7 2010 12:00AM','6CFD12FC-2F0A-4EB5-BDA2-3A5958CD8925')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10290,0,'Christian Athiest: Don','Nov 14 2010 12:00AM','CC370367-22AE-41C8-8E6F-7F385952EE23')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10019,0,'Christian Atheist','Nov 21 2010 12:00AM','E88979C5-7C58-409E-9ED9-6897078DA676')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10362,0,'Christian Atheist - Share Faith','Nov 28 2010 12:00AM','2B26221B-7F62-4735-AA40-677593DCCDA0')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9650,0,'','Dec  5 2010 12:00AM','7F45B250-9A1F-4EA6-B025-541FFAFAD1E7')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10119,0,'','Dec 12 2010 12:00AM','6AB10F67-C0DC-48AF-ABAA-D28F3313591A')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10629,0,'','Dec 19 2010 12:00AM','84B031F8-F6B8-4F65-B973-367B5416F3FE')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',28227,0,'Christmas Services - 12/22 5,713  12/23 6,096  12/24 16,418','Dec 24 2010 12:00AM','BACC50ED-AADB-4BEE-B466-441E16FE9DE8')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',4712,0,'No Services Saturday - Christmas Day','Dec 26 2010 12:00AM','12C506AF-D5C1-4DA0-8088-7ADECDEEFC3B')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10194,0,'','Jan  2 2011 12:00AM','018981F0-43B9-4BD1-9B24-51A2BAC75A7A')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',11124,0,'State of the Church (Don)','Jan  9 2011 12:00AM','1A1B1DAF-0783-49CF-92CA-6C1E95BFB56D')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10884,0,'','Jan 16 2011 12:00AM','97794990-B785-451C-8759-5E170F63C6C7')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',12089,0,'Momentum - Don','Jan 23 2011 12:00AM','FFE939A0-F624-4092-BF6C-2CA744771368')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',11757,0,'','Jan 30 2011 12:00AM','02A8F6DD-D3BC-4517-9BB8-02214B098069')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10961,0,'Momentum - Step Out','Feb  6 2011 12:00AM','472487BB-FF73-4834-9047-859A34F80460')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',11168,0,'','Feb 13 2011 12:00AM','04322AE6-192E-43D8-A7AA-67D48A8963EA')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10831,0,'','Feb 20 2011 12:00AM','AE75EA16-8A0B-4E43-BD59-4227CA0A23B5')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',11130,0,'','Feb 27 2011 12:00AM','30834427-64B1-4F0F-B0AE-9B8CE52D681D')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',11495,0,'Baggage - Jealousy (Brian Beltramo)','Mar  6 2011 12:00AM','9B0B846B-88D8-489B-BE5B-56335D9A8E5B')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10423,0,'','Mar 13 2011 12:00AM','B58AEA93-D644-4155-AE9E-B97C44A3E04B')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10697,0,'The Last 24 - Passover (Don)','Mar 20 2011 12:00AM','8C043588-8B47-408B-AC4D-C2FCC6E95F00')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',11109,0,'The Last 24 - The Garden (Trent)','Mar 27 2011 12:00AM','3C016B72-B54F-4A22-8C1A-07F1BE59D074')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10567,0,'The Last 24 - Betrayal (Don)','Apr  3 2011 12:00AM','8636E822-D45A-4087-BBD9-65E102F242A5')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9967,0,'The Last 24 - The Trial (First Video)','Apr 10 2011 12:00AM','3D115CD7-FF69-446F-82C0-6A900EF3ACED')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9764,0,'','Apr 17 2011 12:00AM','043443DD-2E69-40CF-8C20-58A3E1159567')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',23488,0,'Good Friday 3,473 | Saturday 3:00 2,476 | 4:30 2,447 | 6:00 2,942 | Sunday Sunrise 2,906 | 9:00 2,809 | 10:30 3,346 | 12:00 3,089','Apr 24 2011 12:00AM','03519953-558A-47BD-9812-F8A76A3261AF')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10919,0,'Before & After (Don)','May  1 2011 12:00AM','DC643CCC-00F9-4E70-9164-B7DB54151A5D')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',11754,0,'Mother''s Day','May  8 2011 12:00AM','141492BE-751F-48DC-B0F3-4C4E5D5D2013')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10055,0,'','May 15 2011 12:00AM','ADDCF574-F889-4BB3-9263-A045D6172E73')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9966,0,'','May 22 2011 12:00AM','BE90EA26-2C22-4BD5-9F92-9B7F3F677D24')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9159,0,'Honour Found - Guest Speaker Robert Barriger','May 29 2011 12:00AM','AA439A2D-14A4-4B6E-9E2D-5FEA722CD2CA')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9764,0,'Red Letters - The Sower (Don)','Jun  5 2011 12:00AM','49EB85B6-54B0-48E7-B988-596207B20B07')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9889,0,'','Jun 12 2011 12:00AM','201FA271-6A6C-46FA-A760-22A59C96C037')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9074,0,'Prodigal Son - Trent (Father''s Day)','Jun 19 2011 12:00AM','7B94DC2C-5466-4230-8A8E-15CCFD7A6508')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',8814,0,'','Jun 26 2011 12:00AM','0A96893F-806B-4CFA-9D1D-6AEC76F2A773')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9197,0,'4th of July','Jul  3 2011 12:00AM','A0DA30DD-686E-4851-9A0B-7C02FB86347F')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10001,0,'','Jul 10 2011 12:00AM','1D0CD75D-C608-4E1F-B90F-11BA62A051E5')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',11462,0,'Youth Weekend (The Lost Sheep - Dustin T)','Jul 17 2011 12:00AM','DE593A13-DFE2-472F-B19A-051F9C78D3AF')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',8931,0,'Trent Renner','Jul 24 2011 12:00AM','506C11FE-0D92-4004-860B-26FCF9A8ADB3')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9149,0,'Red Letters (Forgiveness) - Trent Renner','Jul 31 2011 12:00AM','BD29E9B0-83EC-4726-9D9F-DF2AD9BD75D7')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10163,0,'All In - Giving','Aug  7 2011 12:00AM','5A1A7903-CF3A-40AE-9BBC-14A78D2CFA23')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10679,0,'Fall Kick Off - All In (Loving - Don)','Aug 14 2011 12:00AM','4AA4EBF6-2225-4146-8BF8-55802AA7C097')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10900,0,'All In','Aug 21 2011 12:00AM','743608E4-9AB9-4A0F-B8B5-6B5F4E334A05')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9959,0,'','Aug 28 2011 12:00AM','A723792F-6DA4-4D4A-BD68-A07A941127B1')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9209,0,'Mark Moore - Growing (Labor Day Weekend)','Sep  4 2011 12:00AM','20A965EC-71FA-49A9-87E7-CB386A45AFFD')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',11533,0,'9/11 Weekend','Sep 11 2011 12:00AM','A8FEC8D6-180B-46F7-80DA-D6224F000F61')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9940,0,'Where do we go after we die? Ingram','Sep 18 2011 12:00AM','B1046407-DFFA-4993-8A6B-567BF5FE0EDC')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10697,0,'Travis Brown NFL','Sep 25 2011 12:00AM','61B4B851-9D57-4DBB-8F26-0938F74E2D78')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9602,0,'Questions: Are All Lifestyles Okay? Mark Moore','Oct  2 2011 12:00AM','0E41CD51-A9AE-496E-A22E-ADDFF2816A08')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9258,0,'Questions: What is the Point of Church? George Barna','Oct  9 2011 12:00AM','3F740C88-5048-4246-8369-D9231C4C5BE9')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9289,0,'Joseph - Adversity (Don)','Oct 16 2011 12:00AM','30CBE326-C358-4887-8393-D0888ECA3A25')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9545,0,'Joseph - Temptation (Don)','Oct 23 2011 12:00AM','1A504315-B687-4433-8DC4-2867237144DB')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9290,0,'Joseph','Oct 30 2011 12:00AM','1489FF10-BF89-4629-9B65-F748FAFD0B41')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9316,0,'Joseph - Materialism (Don)','Nov  6 2011 12:00AM','8B1A37AE-46AA-49AB-A742-EA6CAA3C164F')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',8931,0,'Bob Fesmire','Nov 13 2011 12:00AM','10C486D2-3276-42E0-901F-83CDD8F0C7F4')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',8635,0,'Missions Weekend','Nov 20 2011 12:00AM','A5330987-753B-4CB2-B944-0AE70D94119B')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9572,0,'Volunteer Appreciation 2011','Nov 27 2011 12:00AM','86CD6850-8C7F-4075-B328-14A839A09CD6')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9499,0,'The Other "F" Word - Focus on the End','Dec  4 2011 12:00AM','F1F9A304-F695-48BA-8530-E5B518EBDF98')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',8930,0,'The Other "F" Word - Fight for the Heart','Dec 11 2011 12:00AM','E2115494-23E7-439B-A46E-37423515E5DE')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10155,0,'The other "F" Word - Create a Rhythm (Don)','Dec 18 2011 12:00AM','0F0FBEFD-243C-4EA9-BECE-0E3E5F029786')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',28633,0,'Christmas Services | Wed 5:30 2,397 | 7:00 2,135 | Thurs 5:30 2,727 | 7:00 2,697 | Fri 5:30 2,908 | 7:00 2,851 | Sat 2:30 3,467 | 4:00 3,422 | 5:30 3,150 | 7:00 2,879','Dec 24 2011 12:00AM','D67C2090-993A-4BBE-9CD8-3D2AF3835028')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',6850,0,'New Year''s Day','Jan  1 2012 12:00AM','48B2A299-B9BE-4D89-ABD7-B771EB424574')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10213,0,'State of the Church (Don)','Jan  8 2012 12:00AM','B6F9C8C9-3D5E-4525-8774-1197A14287CD')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9932,0,'Unleashed - Don','Jan 15 2012 12:00AM','2F349B6E-8045-469A-B608-30BF23AEF959')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10522,0,'Unleashed - Church (Don)','Jan 22 2012 12:00AM','833E562B-1802-41F2-A5FE-21A0EFAFA07F')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10930,0,'Unleashed','Jan 29 2012 12:00AM','3F59D0EA-7019-469A-8F8F-96EC8B4C2B54')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9914,0,'Unleashed (Don) Superbowl','Feb  5 2012 12:00AM','B30AD2BA-A90E-4661-98F6-6EF9C46F503E')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9947,0,'Unleashed','Feb 12 2012 12:00AM','BD16ED1B-7920-47BC-B43D-6A10B998AD04')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9565,0,'Unleashed (Mark Moore)','Feb 19 2012 12:00AM','A11C4448-9C8E-4DF6-A418-5B94B0567DE0')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9891,0,'Unleashed - Prayer (Don)','Feb 26 2012 12:00AM','7B43CB87-B769-48F6-BA52-6461A1E42C2D')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9835,0,'','Mar  4 2012 12:00AM','528AEE8C-3909-4C56-99C1-8ABA1013D5C2')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9756,0,'Unleashed','Mar 11 2012 12:00AM','9BDB3AD6-52DF-477A-8717-182EFC807A39')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',8977,0,'Unleashed - Don (it was raining)','Mar 18 2012 12:00AM','A1ADA8EB-21B3-4AA8-AE85-04576B11B40E')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',11195,0,'Unleashed - "Bring a Friend" | Car Show | Operation Support our Schools','Mar 25 2012 12:00AM','A7568F12-B8A5-4CBA-B606-8A98A9616B85')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9588,0,'Unleashed - CCV 30 Year Anniversary','Apr  1 2012 12:00AM','F7DFA9C4-693E-4C30-B7E9-E92BFE9D0FB3')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',22864,0,'Good Friday 5:30 1,800 | 7:00 1,487 | Saturday 4:30 3,516 | 6:00 3,110 | Sunday Sunrise 3,512 | 9:00 3,307 | 10:30 3,836 | 12:00 2,296','Apr  8 2012 12:00AM','3A441270-E8BF-404D-AD1F-79CF0D928F64')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10224,0,'The Blessed Life - The Principle of First','Apr 15 2012 12:00AM','7CE5B792-799A-4084-8616-081D34D4FA89')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9431,0,'','Apr 22 2012 12:00AM','CA6FBB6F-1025-42EA-98E8-4C02D893D471')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10200,0,'The Blessed Life - Breaking the Spirit of Mammon','Apr 29 2012 12:00AM','B82D24F2-AA1B-414F-A0CD-F4DC24753C5F')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9440,0,'The Blessed Life (Don)','May  6 2012 12:00AM','58A6F8D9-1D47-408A-9B81-B921AC211E48')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9893,0,'Mother''s Day (Sue W)','May 13 2012 12:00AM','16091396-D7DF-462F-98FA-5E5B8511FBDF')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9751,0,'X - Only Worship God (Don)','May 20 2012 12:00AM','487D87BA-AD87-4570-AB76-CAE5E04F09D5')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9378,0,'Memorial Day Weekend - X - Accept No Substitutes - Todd Clark','May 27 2012 12:00AM','35769B0D-0BDC-4B9C-8206-8053A84DEECB')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9971,0,'X - Respect God''s Name (Ingram)','Jun  3 2012 12:00AM','668E2AE8-A615-43AC-A31F-9891DADEDD1C')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10040,0,'X - Remember the Sabbath (Mark Moore)','Jun 10 2012 12:00AM','6A953AD5-A992-4E32-B9C7-FEA0279DC1F4')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10127,0,'X - Honor Your Father & Mother (Don)','Jun 17 2012 12:00AM','FDFFA360-6169-4A8A-BB69-DC268580A9D6')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10255,0,'X - Celebrate Life (Mark Moore)','Jun 24 2012 12:00AM','30469106-925F-4372-AF9B-D4599E25D32A')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9841,0,'X - Honor Your Vows (Mark Moore)','Jul  1 2012 12:00AM','7C613CDD-5AD3-4160-94CB-0ADDF4C053A2')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10385,0,'X - Live With Integrity (Mark)','Jul  8 2012 12:00AM','2A2DB75A-67A0-47BB-A0DB-4CFD58680084')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10560,0,'X - Tell The Truth (Mark)','Jul 15 2012 12:00AM','DF8ACB46-423C-4000-93E5-D1F599D79C33')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10041,0,'X - Be Content (Don)','Jul 22 2012 12:00AM','52C99935-46AE-4E5E-8C3D-F732A6897BFC')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',11437,0,'Lazarus ','Jul 29 2012 12:00AM','128FE424-ECBE-4CF2-AD8F-B23E92390957')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',11281,0,'Youth Weekend - Dustin Tappan','Aug  5 2012 12:00AM','486BF298-2E30-4419-9BB3-25579A9B5302')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',11938,0,'Fall Kickoff - The Story - Introduction (Don)','Aug 12 2012 12:00AM','ABE8AC97-3ECA-4983-BFB3-27E922F080A3')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',13745,0,'The Story - Ch 1 Creation (Mark)','Aug 19 2012 12:00AM','7E17344D-BC55-4B9C-96EB-92D1DA3F34E2')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',13050,0,'The Story - Ch 2 Abraham (Don)','Aug 26 2012 12:00AM','CE02FF23-FEFE-47D3-BC6A-01488DDDC68E')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',11177,0,'The Story - Ch 3 Joseph (Don)','Sep  2 2012 12:00AM','514CA05B-7FDE-4132-8B54-4438E81EF1D1')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',12471,0,'The Story - Ch 4 & 5 Moses (Don)','Sep  9 2012 12:00AM','CC55CA72-8A3F-4B70-A8BA-F729BC8631BC')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',11459,0,'The Story - Ch 6 & 7 (Mark)','Sep 16 2012 12:00AM','F21F44F1-3B13-40D5-A5B9-0FE9D8DFB726')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',11383,0,'The Story - Ch 8 & 9 (Mark)','Sep 23 2012 12:00AM','81C7C347-72FD-494E-B655-6A388F6E5BFB')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10858,0,'The Story - Ch 10 - Standing Tall, Falling Hard (Don)','Sep 30 2012 12:00AM','44E65410-616C-41C2-891C-7F75F83BB499')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10366,0,'The Story - Ch 11 (Mark)','Oct  7 2012 12:00AM','09F294F9-62D4-46AD-ACA7-6EF2A26EC52C')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9887,0,'The Story - David Part II (Mark)','Oct 14 2012 12:00AM','C1539C1D-DCE0-4846-8C21-E5E75F426048')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10049,0,'The Story - Ch 13 - The King Who Had It All (Mark)','Oct 21 2012 12:00AM','AE77A35E-E686-4678-9227-E962F6478D2C')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10359,0,'The Story - Ch 14 (Ingram)','Oct 28 2012 12:00AM','9B7799DF-D330-495A-889B-8CA993B40913')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10166,0,'The Story Ch 15 - Messengers of God (Don)','Nov  4 2012 12:00AM','E1DA715A-514D-4FBA-B11F-FC514E7B1A90')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10498,0,'The Story - Ch 16 (Don)','Nov 11 2012 12:00AM','9B0962E5-3053-4641-9B06-65E099FAF7AE')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',11857,0,'The Story - Ch 17 - The Kingdom''s Fall (Todd Clark)','Nov 18 2012 12:00AM','8CF2549F-71B3-468B-BA1E-8682EF37AED2')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10060,0,'The Story - Ch 18 - Daniel In Exile (Don)','Nov 25 2012 12:00AM','B3D4A935-AAF8-4E7E-B7E5-9D4BA620133E')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9927,0,'The Story - Ch 19 - The Return Home (Mark)','Dec  2 2012 12:00AM','74151930-0156-447C-B02F-D0156FF42E26')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10574,0,'The Story - Ch 20 Ester (Don)','Dec  9 2012 12:00AM','A631326F-4BED-48BF-919E-72D532DB51F8')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',11050,0,'The Story - Ch 21 - Rebuilding The Walls (Don)','Dec 16 2012 12:00AM','A7B4CE29-29A6-4A3F-BF31-AD89311A655B')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',31095,0,'12/22 4:00 2772, 5:30 2678 | 12/23 9:00 2239, 10:30 2589, 12:00 2205, 2:30 1642, 4:00 2573, 5:30 2629 | 12/24 2:30 2988, 4:00 3258, 5:30 2968, 7:00 2554','Dec 23 2012 12:00AM','16DEDFB1-10D8-4213-AA7F-D37FCF954DB9')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9781,0,'The Story - Ch 23 - Jesus'' Ministry Begins (Mark)','Dec 30 2012 12:00AM','F84FD791-A522-4F86-9548-65B1E26CB99E')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',11223,0,'The Story - Ch 24 (Don)','Jan  6 2013 12:00AM','52A96BD4-1A18-4AFA-95EB-443A360C8655')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10801,0,'The Story','Jan 13 2013 12:00AM','A75923EA-5217-4A97-B964-4D5DC22919E4')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10482,0,'The Story Ch 26 (Mark)','Jan 20 2013 12:00AM','43381779-201F-491C-AF4C-4DA40710B309')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10907,0,'The Story - Ch 27: Resurrection (Todd Clark)','Jan 27 2013 12:00AM','2701C3E0-8581-4E9E-A44A-3A63D0D13CB9')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10420,0,'State of the Church - Don','Feb  3 2013 12:00AM','C8151927-4181-4F69-AE74-A4ABF4338892')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10932,0,'The Story - Pauls Mission (Mark)','Feb 10 2013 12:00AM','9BB0C037-BB45-4F78-A181-834B90DA0315')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10617,0,'The Story - Paul''s Final Days (Mark)','Feb 17 2013 12:00AM','9BC14A49-5103-4A43-A021-FBDA72878775')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',11068,0,'The Story - End (Todd)','Feb 24 2013 12:00AM','AA2D8643-2909-44E5-B81C-1FC8D6879D0B')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',11691,0,'The Fighter - Get in the Ring (Don)','Mar  3 2013 12:00AM','20283C30-DB00-42A0-828C-3EDE072E3CE5')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',11692,0,'The Fighter - Eye of the Tiger (Don)','Mar 10 2013 12:00AM','F892349C-D565-4711-8DF2-79D8F8F3CD92')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',11525,0,'The Fighter - Knocked Out (Todd)','Mar 17 2013 12:00AM','A02E50E5-876E-4BBD-99E1-F69C70420554')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',11267,0,'The Fighter - Fighting to Win (Don)','Mar 24 2013 12:00AM','39BD8036-EE31-4B34-9808-AAE2F4065AD3')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',24654,0,'Good Friday 5:30 1937 | 7:00 1711 | Saturday 4:30 3584 | 6:00 3047 | Sunday Sunrise 3859 | 9:00 3348 | 10:30 4159 | 12:00 3009','Mar 31 2013 12:00AM','FABABA04-3012-4FE8-81B3-7FDA5CE9BA78')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',12110,0,'Search - Why Does God Allow Suffering? (Don)','Apr  7 2013 12:00AM','D2801E4B-4166-49A3-A2BF-2D0CFAD9C8E2')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',11874,0,'Search - Is the Bible (Mark)','Apr 14 2013 12:00AM','90E5C11C-9553-4E1B-AF30-957693787BC6')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',12718,0,'Search - What does God say about Homosexuality? (Todd) + Car Show','Apr 21 2013 12:00AM','87191BC9-8A53-4A4D-8478-B6F5C40FAA8D')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',11636,0,'Search - Questions with Don, Mark, Todd','Apr 28 2013 12:00AM','CB10C754-A2D0-44AF-B573-A92EE081F570')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',11555,0,'Think Pink - Think Change (Don)','May  5 2013 12:00AM','01E4D091-E1A3-42F7-A9DD-794A0AA33CBC')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',11640,0,'Think Pink - Mother''s Day (Sue Wilson) ','May 12 2013 12:00AM','66F87096-7D27-43EC-BEBB-929C27017196')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',11290,0,'','May 19 2013 12:00AM','2FC03AAE-E646-4241-AE51-B6BB91470296')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9995,0,'Think Pink - Priorities (Don) - Memorial Day Weekend','May 26 2013 12:00AM','35076F17-4C6F-4800-B0B0-B37E392BEFD9')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',11039,0,'At the Movies (Todd)','Jun  2 2013 12:00AM','EF573E81-726C-41A6-B3F7-DC6C9B9410B5')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10732,0,'At the Movies - Hunger Games (Todd)','Jun  9 2013 12:00AM','5B5E4CF0-3933-4105-B2D9-CA35263C346D')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10713,0,'At the Movies - Hunger Games (Todd)','Jun 16 2013 12:00AM','E5CC18CE-4579-413B-B32C-98AB280AD78D')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10598,0,'At the Movies (Iron Man) Todd','Jun 23 2013 12:00AM','69FF30B0-600C-4F8F-A8CB-AE7235354CC4')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9815,0,'At the Movies - Despicable Me (Todd)','Jun 30 2013 12:00AM','69789FDE-050E-42BE-BF74-EC2846C4FDF4')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10458,0,'Identity Theft - Phishing (Mark)','Jul  7 2013 12:00AM','B885EC46-01B1-44B8-9169-3A2F242CBF43')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10834,0,'Identity Theft - Mark','Jul 14 2013 12:00AM','A774080F-26BF-496A-A19D-854843C065B9')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10462,0,'','Jul 21 2013 12:00AM','89B0CCAE-13AF-4B0C-81A6-4231D2AE57FE')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10404,0,'Identity Theft - Spam - Mark Moore','Jul 28 2013 12:00AM','EA78F108-8BA4-42AF-ADD3-29E7C6C243AC')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10913,0,'Identity Theft - Hacking (Mark)','Aug  4 2013 12:00AM','4BF6EC43-B86C-41C5-A6C8-C22513145D7B')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',11511,0,'Step Up - Intercede (Don)','Aug 11 2013 12:00AM','7E09FB18-F585-41E6-BFCA-EA2941B34EE6')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',11149,0,'Step Up - Invite (Don)','Aug 18 2013 12:00AM','9E1C2D0C-8430-4CF2-A933-4699D68DC65B')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10811,0,'Step Up - Invest (Don)','Aug 25 2013 12:00AM','53449125-0B77-4A36-99B8-CAF8F143E275')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9057,0,'Step Up (Involved) - Don','Sep  1 2013 12:00AM','0B296E95-8C2C-4097-BA54-452363BF5149')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10268,0,'STEP UP Impact (Offering) - Don','Sep  8 2013 12:00AM','484D1DCA-D97E-4D55-BCE7-3C0E802F4BD6')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',11302,0,'My Family - Technology Trap (Todd Clark)','Sep 15 2013 12:00AM','F2EC22AC-1A1F-4469-929C-5127CC22BBD8')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9877,0,'My Family (Transfer Biblical Values) - Don','Sep 22 2013 12:00AM','7DB01FEC-62FC-483F-A670-E079EEAA49C4')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10156,0,'My Family - Distractions (Todd)','Sep 29 2013 12:00AM','E356B3F5-EEE2-4762-94E8-C6115C5B39D9')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9921,0,'My Family - Mixed Blessings (Mark)','Oct  6 2013 12:00AM','6939072F-7AD6-4804-9D5B-134DBCB48F5A')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9871,0,'Ghost Stories "In Us" (Todd)','Oct 13 2013 12:00AM','C0302AEA-9BCA-46F7-87CE-125C5ECA6F9A')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10788,0,'Ghost Stories (Through Us) - Todd Clark

STARS CELEBRATION WEKEND','Oct 20 2013 12:00AM','99FFC756-BBEA-47A5-9ED4-750742E62C9F')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9587,0,'Ghost Stories - Around Us (Mark Moore)','Oct 27 2013 12:00AM','42B082C3-E06C-43B8-833D-31EE5E837295')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10352,0,'Great Expectations (Marriage) - Panel w/Don','Nov  3 2013 12:00AM','7BB7DE41-9C43-4CA2-9580-7E3F62FA49D1')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10147,0,'Great Expectations (Friendship) - Mark','Nov 10 2013 12:00AM','0A41A71A-CC34-4C44-A875-79F47DCA799C')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10175,0,'Great Expectations (In the Marketplace) Don','Nov 17 2013 12:00AM','806D1CB8-1C01-4DAA-AEC8-C1CC4BF5CA8E')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9757,0,'Great Expectations - Community (Todd)','Nov 24 2013 12:00AM','414434E3-234B-4C4E-8FAA-9CF0C6B40E24')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9950,0,'Colors of Christmas (Green) - Don','Dec  1 2013 12:00AM','4DDCA0D7-076F-4DF6-A170-FE2FF74D75A3')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9536,0,'Colors of Christmas - Blue (Todd)','Dec  8 2013 12:00AM','9A49F680-FD6F-4F82-B217-599F4889B066')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10757,0,'Colors of Christmas (Red) Don','Dec 15 2013 12:00AM','785CD71C-21F7-403F-B06B-475DB306C994')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',32564,0,'Christmas Services 12/21 3610, 2696 | 12/22 2508, 2964, 2425 | 12/23 2051, 2139, 2042 | 12/24 3917, 4261, 2030, 1921','Dec 24 2013 12:00AM','384E2F9D-2C94-45FC-9AE1-6A6C9B97CEFD')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9888,0,'The Silence of God - Mark Moore','Dec 29 2013 12:00AM','D7ECBFF1-7B6A-453E-A3CE-5053B81CF1D6')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',11537,0,'Power of One (Don)','Jan  5 2014 12:00AM','5360D448-57C6-48E8-A15D-4B39E486C8D3')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',11443,0,'The Power of One Decision (Don)','Jan 12 2014 12:00AM','E78A60C8-54AE-408F-B04F-4C61298194BD')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',11007,0,'Power of One (Prayer) Mark Moore','Jan 19 2014 12:00AM','92DA17B2-FEF0-4E8F-9E6D-32114C9299D2')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',11674,0,'The Power of One Invitation - Todd Clark','Jan 26 2014 12:00AM','36B9F806-E93C-40EA-A9A7-B8A57C1BDF18')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10689,0,'Power of One - Sabbath (Mark)','Feb  2 2014 12:00AM','FBDA9BC4-8A62-4D69-9679-2B6E0AE21DE4')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10606,0,'State of the Church (Don)','Feb  9 2014 12:00AM','024CE1EF-3722-45C5-8B54-B6ECAB7BE0C5')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10856,0,'Elevate Your Heart (Don)','Feb 16 2014 12:00AM','4C5A1F79-2A5A-4A46-BC30-B0E033EE0E52')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10663,0,'Elevate Your Trust (Todd)','Feb 23 2014 12:00AM','C809EE5E-0E9B-4972-B70C-5CD0E15B0F61')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9605,0,'Elevate Your Generosity (Ashley Wooldridge) ...rained Saturday','Mar  2 2014 12:00AM','ADD02568-4CB1-4013-92D4-3EC0647F45DC')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',9476,0,'Elevate Your Ownership (Don)','Mar  9 2014 12:00AM','7BB335BD-42C2-4182-ABEC-22D24396DB5F')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10443,0,'Encounters - Mary (Mark Moore)','Mar 16 2014 12:00AM','2062EFBD-0580-4707-9E3C-74D64D8142E8')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',11453,0,'Encounters (Peter) - Don W
CARSHOW','Mar 23 2014 12:00AM','15A94ED5-29B6-4DDF-8D65-7B74EBBE8CCF')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10480,0,'Encounters - Judas (Todd)','Mar 30 2014 12:00AM','979460B2-C11E-4959-B08D-0D2747B6F754')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10553,0,'Encounters (Pilate) - Don','Apr  6 2014 12:00AM','4C695633-64EF-4D0E-8210-3875870D3DE6')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',10139,0,'Encounters (The Thief on the Cross) - Mark','Apr 13 2014 12:00AM','8D1719AA-6259-428A-A056-6D36F2F0C847')
INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Note], [MetricValueDateTime], [Guid])
    VALUES (@metricId, 0, '0',23499,0,'Good Friday 5:30 1985 | 7:00 1627 | Saturday 4:30 3153 | 6:00 2985 | Sunday Sunrise 4128 | 9:00 3479 | 10:30 3838 | 12:00 2304','Apr 20 2014 12:00AM','8E90A661-5789-418B-B1D2-ED63765AEBB4')    
   

end