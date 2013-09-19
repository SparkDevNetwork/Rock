declare
   @entityTypeIdWFType int = (select id from EntityType where Name = 'Rock.Model.WorkflowType'),
   @parentCategoryId int = null,
   @categoryName nvarchar(max),
   @iconCssClass nvarchar(max) = null,
   @categoryId int = null,
   @order int = 0
begin
   if (@entityTypeIdWFType is null) begin
     INSERT INTO [EntityType] ([Name],[FriendlyName],[Guid],[AssemblyName],[IsEntity],[IsSecured])
       VALUES ('Rock.Model.WorkflowType',null,NEWID(),null,0,0)
     set @entityTypeIdWFType = SCOPE_IDENTITY()
   end

   delete from WorkflowType where CategoryId is not null    
   delete from Category where EntityTypeId in (@entityTypeIdWFType)

   -- root wf cats
   INSERT INTO [Category]([IsSystem],[ParentCategoryId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],[Name],[Guid],[IconSmallFileId],[IconLargeFileId],[IconCssClass])
     VALUES
      (0,@parentCategoryId,@entityTypeIdWFType,null,null,'CT Requests',NEWID(),null,null,'icon-beaker'),
      (0,@parentCategoryId,@entityTypeIdWFType,null,null,'HR Stuff',NEWID(),null,null,'icon-user-md'),
      (0,@parentCategoryId,@entityTypeIdWFType,null,null,'Random Stuff',NEWID(),null,null,'icon-lightbulb'), 
      (0,@parentCategoryId,@entityTypeIdWFType,null,null,'Check-in','8F8B272D-D351-485E-86D6-3EE5B7C84D99',null,null,'icon-check-sign') 
      
   -- 2nd level cats
   set @parentCategoryId = (select id from Category where Name = 'CT Requests')
   INSERT INTO [Category]([IsSystem],[ParentCategoryId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],[Name],[Guid],[IconSmallFileId],[IconLargeFileId],[IconCssClass])
     VALUES
      (0,@parentCategoryId,@entityTypeIdWFType,null,null,'IT Requests',NEWID(),null,null,'icon-wrench'),
      (0,@parentCategoryId,@entityTypeIdWFType,null,null,'IT Wishlists',NEWID(),null,null,'icon-truck'),
      (0,@parentCategoryId,@entityTypeIdWFType,null,null,'IT Secrets',NEWID(),null,null,'icon-thumbs-up') 

   set @parentCategoryId = (select id from Category where Name = 'HR Stuff')
   INSERT INTO [Category]([IsSystem],[ParentCategoryId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],[Name],[Guid],[IconSmallFileId],[IconLargeFileId],[IconCssClass])
     VALUES
      (0,@parentCategoryId,@entityTypeIdWFType,null,null,'New Hire Processes',NEWID(),null,null,'icon-book'),
      (0,@parentCategoryId,@entityTypeIdWFType,null,null,'Volunteer Processes',NEWID(),null,null,'icon-film'),
      (0,@parentCategoryId,@entityTypeIdWFType,null,null,'Severe Reprimand Processes',NEWID(),null,null,'icon-ok'),
      (0,@parentCategoryId,@entityTypeIdWFType,null,null,'Just Kidding Processes',NEWID(),null,null,'icon-pencil') 

   set @parentCategoryId = (select id from Category where Name = 'Random Stuff')
   INSERT INTO [Category]([IsSystem],[ParentCategoryId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],[Name],[Guid],[IconSmallFileId],[IconLargeFileId],[IconCssClass])
     VALUES
      (0,@parentCategoryId,@entityTypeIdWFType,null,null,'asdf',NEWID(),null,null,@iconCssClass),
      (0,@parentCategoryId,@entityTypeIdWFType,null,null,'12354',NEWID(),null,null,'icon-random'),
      (0,@parentCategoryId,@entityTypeIdWFType,null,null,'qwerty',NEWID(),null,null,@iconCssClass),
      (0,@parentCategoryId,@entityTypeIdWFType,null,null,'zxcvzxcvzxcv',NEWID(),null,null,@iconCssClass) 

   -- example workflow types
   set @categoryId = (select id from Category where Name = 'IT Requests')
   INSERT INTO [WorkflowType]([IsSystem],[IsActive],[Name],[Description],[CategoryId],[Order],[WorkTerm],[ProcessingIntervalSeconds],[IsPersisted],[LoggingLevel],[Guid])
     VALUES 
        (0,1,'Desktop','Desktop Workflow Type Description',@categoryId,0,'SomeWorkTermA','60',0,0,NEWID()),
        (0,1,'Reports','Reports Workflow Type Description',@categoryId,1,'SomeWorkTermB','60',0,0,NEWID()),
        (0,1,'ExampleWorkflowType asdf','Some Description',@categoryId,2,'SomeWorkTermC','60',0,0,NEWID()),
        (0,1,'ExampleWorkflowType qwert','Some Description',@categoryId,3,'SomeWorkTermD','60',0,0,NEWID())

    set @categoryId = (select id from Category where Name = 'Severe Reprimand Processes')
   INSERT INTO [WorkflowType]([IsSystem],[IsActive],[Name],[Description],[CategoryId],[Order],[WorkTerm],[ProcessingIntervalSeconds],[IsPersisted],[LoggingLevel],[Guid])
     VALUES 
        (0,1,'Make Example','Some Description',@categoryId,0,'SomeWorkTermEE','60',0,0,NEWID()),
        (0,1,'Another Example','Some Description',@categoryId,1,'SomeWorkTermFF','60',0,0,NEWID()),
        (0,1,'Blue Example ','Some Description',@categoryId,2,'SomeWorkTermGG','60',0,0,NEWID()),
        (0,1,'Purple Example','Some Description',@categoryId,3,'SomeWorkTermHH','60',0,0,NEWID())

end