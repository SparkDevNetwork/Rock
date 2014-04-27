declare
   @entityTypeIdMetricCategoryType int = (select id from EntityType where Name = 'Rock.Model.MetricCategory'),
   @parentCategoryId int = null,
   @categoryName nvarchar(max),
   @iconCssClass nvarchar(max) = null,
   @categoryId int = null,
   @order int = 0,
   @metricGuid uniqueidentifier = null; 
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

    set @categoryId = (select id from Category where Name = 'Person Metrics' and EntityTypeId = @entityTypeIdMetricCategoryType )    
    insert into [MetricCategory] ([MetricId], [CategoryId], [Order], [Guid])
      values ( (select Id from Metric where Guid = @metricGuid), @categoryId, 0, NEWID())

    set @categoryId = (select id from Category where Name = 'Group Metrics' and EntityTypeId = @entityTypeIdMetricCategoryType )    
    insert into [MetricCategory] ([MetricId], [CategoryId], [Order], [Guid])
      values ( (select Id from Metric where Guid = @metricGuid), @categoryId, 0, NEWID())

    -- example metric    
    set @categoryId = (select id from Category where Name = 'Person Metrics' and EntityTypeId = @entityTypeIdMetricCategoryType )    
    set @metricGuid = NEWID();
    INSERT INTO [Metric]([IsSystem],[Title],[Description],[IconCssClass], [IsCumulative],[Guid])
     VALUES 
        (0,'Free Lunches','Number of Free Lunches per day', 'fa fa-thumbs-up', 1, @metricGuid)
    
    insert into [MetricCategory] ([MetricId], [CategoryId], [Order], [Guid])
      values ( (select Id from Metric where Guid = @metricGuid), @categoryId, 0, NEWID())
   

end