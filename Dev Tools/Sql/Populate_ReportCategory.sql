declare
   @entityTypeIdDVType int = (select id from EntityType where Name = 'Rock.Model.Report'),
   @entityTypeIdPersonType int = (select id from EntityType where Name = 'Rock.Model.Person'),
   @parentCategoryId int = null,
   @categoryName nvarchar(max),
   @iconCssClass nvarchar(max) = null,
   @categoryId int = null,
   @order int = 0
begin
   if (@entityTypeIdDVType is null) begin
     INSERT INTO [EntityType] ([Name],[FriendlyName],[Guid],[AssemblyName],[IsEntity],[IsSecured])
       VALUES ('Rock.Model.Report',null,NEWID(),null,0,0)
     set @entityTypeIdDVType = SCOPE_IDENTITY()
   end

   delete from Report where CategoryId is not null    
   delete from Category where EntityTypeId in (@entityTypeIdDVType)  
   

   -- root wf cats
   INSERT INTO [Category]([IsSystem],[ParentCategoryId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],[Name],[Guid],[IconCssClass],[Order])
     VALUES
      (0,@parentCategoryId,@entityTypeIdDVType,null,null,'Person Reports',NEWID(),'icon-user',0),
      (0,@parentCategoryId,@entityTypeIdDVType,null,null,'Group Reports',NEWID(),'icon-group',0),
      (0,@parentCategoryId,@entityTypeIdDVType,null,null,'Financial Reports',NEWID(),'icon-money',0) 

end