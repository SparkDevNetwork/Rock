declare
   @entityTypeIdMetricCategoryType int = (select id from EntityType where Name = 'Rock.Model.MetricCategory'),
   @parentCategoryId int = null,
   @categoryName nvarchar(max),
   @iconCssClass nvarchar(max) = null,
   @categoryId int = null,
   @order int = 0,
   @metricGuid uniqueidentifier = null,
   @metricId int = null;
begin

    delete from MetricValue   
    delete from Metric
    delete from MetricCategory
    delete from Category where EntityTypeId in (@entityTypeIdMetricCategoryType)  


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


   -- example metric
   
   set @metricGuid = NEWID();
   INSERT INTO [Metric]([IsSystem],[Title],[Description],[IconCssClass],[IsCumulative],[Guid])
     VALUES 
        (0,'First Time Visitors Per Week','First Time Visitors counts broken down into weeks', 'fa fa-gift', 0, @metricGuid)

    set @metricId = (select Id from Metric where Guid = @metricGuid);

    set @categoryId = (select id from Category where Name = 'Person Metrics' and EntityTypeId = @entityTypeIdMetricCategoryType )    
    insert into [MetricCategory] ([MetricId], [CategoryId], [Order], [Guid])
      values ( @metricId, @categoryId, 0, NEWID())

    set @categoryId = (select id from Category where Name = 'Group Metrics' and EntityTypeId = @entityTypeIdMetricCategoryType )    
    insert into [MetricCategory] ([MetricId], [CategoryId], [Order], [Guid])
      values ( @metricId, @categoryId, 0, NEWID())

    -- example metric    
    set @categoryId = (select id from Category where Name = 'Person Metrics' and EntityTypeId = @entityTypeIdMetricCategoryType )    
    set @metricGuid = NEWID();
    INSERT INTO [Metric]([IsSystem],[Title],[Description],[IconCssClass], [IsCumulative],[Guid])
     VALUES 
        (0,'Free Lunches','Number of Free Lunches per day', 'fa fa-thumbs-up', 1, @metricGuid)

    set @metricId = (select Id from Metric where Guid = @metricGuid);
    
    insert into [MetricCategory] ([MetricId], [CategoryId], [Order], [Guid])
        values ( @metricId, @categoryId, 0, NEWID())

    INSERT INTO [MetricValue] ([MetricId], [MetricValueType], [XValue], [YValue], [Order], [Guid])
        values 
( @metricId, 1, '12/11/2013', 500, 0, NEWID()),
( @metricId, 1, '12/18/2013', 200, 0, NEWID()),
( @metricId, 1, '12/25/2013', 0, 0, NEWID()),
( @metricId, 1, '01/03/2013', 0, 0, NEWID()),
( @metricId, 1, '01/10/2013', 999, 0, NEWID()),
( @metricId, 1, '01/17/2013', 5000, 0, NEWID()),
( @metricId, 1, '01/24/2013', 12, 0, NEWID()),
( @metricId, 1, '02/01/2013', 45, 0, NEWID()),
( @metricId, 1, '02/05/2013', 10, 0, NEWID()),
( @metricId, 1, '02/10/2013', 200, 0, NEWID()),
( @metricId, 1, '02/18/2013', 300, 0, NEWID()),
( @metricId, 1, '03/01/2013', 400, 0, NEWID()),
( @metricId, 1, '04/01/2013', 500, 0, NEWID()),
( @metricId, 1, '05/01/2013', 600, 0, NEWID()),
( @metricId, 1, '06/01/2013', 700, 0, NEWID()),
( @metricId, 1, '07/01/2013', 300, 0, NEWID()),
( @metricId, 1, '08/01/2013', 200, 0, NEWID()),
( @metricId, 1, '09/01/2013', 100, 0, NEWID()),
( @metricId, 1, '10/01/2013', 50, 0, NEWID()),
( @metricId, 1, '10/02/2013', 1.9199065065406540, 0, NEWID()),
( @metricId, 1, '11/01/2013', 9, 0, NEWID())

   

end