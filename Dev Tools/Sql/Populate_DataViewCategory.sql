declare
   @entityTypeIdDVType int = (select id from EntityType where Name = 'Rock.Model.DataView'),
   @entityTypeIdPersonType int = (select id from EntityType where Name = 'Rock.Model.Person'),
   @parentCategoryId int = null,
   @categoryName nvarchar(max),
   @iconCssClass nvarchar(max) = null,
   @categoryId int = null,
   @order int = 0
begin
   if (@entityTypeIdDVType is null) begin
     INSERT INTO [EntityType] ([Name],[FriendlyName],[Guid],[AssemblyName],[IsEntity],[IsSecured])
       VALUES ('Rock.Model.DataView',null,NEWID(),null,0,0)
     set @entityTypeIdDVType = SCOPE_IDENTITY()
   end

   delete from DataView where CategoryId is not null    
   delete from Category where EntityTypeId in (@entityTypeIdDVType)  
   

   -- root wf cats
   INSERT INTO [Category]([IsSystem],[ParentCategoryId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],[Name],[Guid],[IconSmallFileId],[IconLargeFileId],[IconCssClass])
     VALUES
      (0,@parentCategoryId,@entityTypeIdDVType,null,null,'Person Views',NEWID(),null,null,'icon-user'),
      (0,@parentCategoryId,@entityTypeIdDVType,null,null,'Group Views',NEWID(),null,null,'icon-group'),
      (0,@parentCategoryId,@entityTypeIdDVType,null,null,'Financial Views',NEWID(),null,null,'icon-money') 

   -- 2nd level cats
   set @parentCategoryId = (select id from Category where Name = 'Person Views' and EntityTypeId = @entityTypeIdDVType)
   INSERT INTO [Category]([IsSystem],[ParentCategoryId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],[Name],[Guid],[IconSmallFileId],[IconLargeFileId],[IconCssClass])
     VALUES
      (0,@parentCategoryId,@entityTypeIdDVType,null,null,'Neighborhoods',NEWID(),null,null,'icon-building'),
      (0,@parentCategoryId,@entityTypeIdDVType,null,null,'Missions',NEWID(),null,null,'icon-globe'),
      (0,@parentCategoryId,@entityTypeIdDVType,null,null,'Jr. High',NEWID(),null,null,'icon-question-sign') 

   -- example data views
   set @categoryId = (select id from Category where Name = 'Neighborhoods' and EntityTypeId = @entityTypeIdDVType )
   INSERT INTO [DataView]([IsSystem],[Name],[Description],[CategoryId],[EntityTypeId],[Guid])
     VALUES 
        (0,'Men','All Men',@categoryId,@entityTypeIdPersonType,NEWID()),
        (0,'Women','All Women',@categoryId,@entityTypeIdPersonType,NEWID())

   -- example data views
   set @categoryId = (select id from Category where Name = 'Jr. High' and EntityTypeId = @entityTypeIdDVType )
   INSERT INTO [DataView]([IsSystem],[Name],[Description],[CategoryId],[EntityTypeId],[Guid])
     VALUES 
        (0,'David''s','All David''s',@categoryId,@entityTypeIdPersonType,NEWID())

end