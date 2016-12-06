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
   INSERT INTO [Category]([IsSystem],[ParentCategoryId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],[Name],[Guid],[IconCssClass],[Order])
     VALUES
      (0,@parentCategoryId,@entityTypeIdDVType,null,null,'Person Views',NEWID(),'icon-user',0),
      (0,@parentCategoryId,@entityTypeIdDVType,null,null,'Group Views',NEWID(),'icon-group',0),
      (0,@parentCategoryId,@entityTypeIdDVType,null,null,'Financial Views',NEWID(),'icon-money',0) 

   -- 2nd level cats
   set @parentCategoryId = (select id from Category where Name = 'Person Views' and EntityTypeId = @entityTypeIdDVType)
   INSERT INTO [Category]([IsSystem],[ParentCategoryId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],[Name],[Guid],[IconCssClass],[Order])
     VALUES
      (0,@parentCategoryId,@entityTypeIdDVType,null,null,'Neighborhoods',NEWID(),'icon-building',0),
      (0,@parentCategoryId,@entityTypeIdDVType,null,null,'Missions',NEWID(),'icon-globe',0),
      (0,@parentCategoryId,@entityTypeIdDVType,null,null,'Jr. High',NEWID(),'icon-question-sign',0) 

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