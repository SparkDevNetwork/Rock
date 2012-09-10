namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSearchPlugins : DbMigration
    {
        public override void Up()
        {
            // add the Search services page to the general settings section of the website.
            CreateIndex( "crmPhoneNumber", "Number" );

            Sql( @"
-- Add new index to crmPerson table to facilitate faster name searches
CREATE NONCLUSTERED INDEX [ix_crmPerson_LastName] ON [dbo].[crmPerson] ([LastName]) INCLUDE ([GivenName],[NickName])

-- Add new page for managing Search MEF components
DECLARE @PageId int
DECLARE @Order int
SELECT @Order = ISNULL(MAX([order])+1,0) FROM [cmsPage] WHERE [ParentPageId] = 77;
INSERT INTO [cmsPage] ([Name],[Title],[IsSystem],[ParentPageId],[SiteId],[Layout],[RequiresEncryption],[EnableViewState],[MenuDisplayDescription],[MenuDisplayIcon],[MenuDisplayChildPages],[DisplayInNavWhen],[Order],[OutputCacheDuration],[Description],[IncludeAdminFooter],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[IconUrl],[Guid])VALUES('Search Services','Search Services',1,77,1,'Default',0,1,1,0,0,0,ISNULL(@Order,0),0,'Manage the search interfaces',1,'Aug 24 2012 06:00:00:000AM','Aug 24 2012 06:00:00:000AM',1,1,NULL,'1719F597-5BA9-458D-9362-9C3E558E5C82')
SET @PageId = SCOPE_IDENTITY()

DECLARE @BlockId int
SET @BlockId = (SELECT [Id] FROM [cmsBlock] WHERE [Guid] = '21F5F466-59BC-40B2-8D73-7314D936C3CB')

DECLARE @BlockInstanceId int
INSERT INTO [cmsBlockInstance] ([IsSystem],[PageId],[Layout],[BlockId],[Zone],[Order],[Name],[OutputCacheDuration],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,@PageId,NULL,@BlockId,'Content',0,'Search Components',0,'Aug 24 2012 06:00:00:000AM','Aug 24 2012 06:00:00:000AM',1,1,'73CE9F13-43F1-4DD4-AA5B-70A48C5A6D85')
SET @BlockInstanceId = SCOPE_IDENTITY()

DECLARE @AttributeId int
SET @AttributeId = (SELECT [Id] FROM [coreAttribute] WHERE [Guid] = '259AF14D-0214-4BE4-A7BF-40423EA07C99')
INSERT INTO [coreAttributeValue] ([IsSystem],[AttributeId],[EntityId],[Order],[Value],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,@AttributeId,@BlockInstanceId,0,'Rock.Search.SearchContainer, Rock','Aug 24 2012 06:00:00:000AM','Aug 24 2012 06:00:00:000AM',1,1,'7DC40BA8-5ECC-40CE-99C9-72A72A87D835')

-- Set Attributes for Person Name Search Plugin
INSERT INTO [coreAttribute] ([IsSystem],[FieldTypeId],[Entity],[EntityQualifierColumn],[EntityQualifierValue],[Key],[Name],[Category],[Description],[Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,1,'Rock.Search.Person.Name','','','SearchLabel','Search Label','Behavior','The text to display in the search type dropdown',1,0,'Name',0,0,'Aug 24 2012 06:00:00:000AM','Aug 24 2012 06:00:00:000AM',1,1,'3fdd185d-3fe4-4be9-b769-26c6fc5c15f9')
SET @AttributeId = SCOPE_IDENTITY()
INSERT INTO [coreAttributeValue] ([IsSystem],[AttributeId],[EntityId],[Order],[Value],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,@AttributeId,0,0,'Name','Aug 24 2012 06:00:00:000AM','Aug 24 2012 06:00:00:000AM',1,1,NEWID())
INSERT INTO [coreAttribute] ([IsSystem],[FieldTypeId],[Entity],[EntityQualifierColumn],[EntityQualifierValue],[Key],[Name],[Category],[Description],[Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,1,'Rock.Search.Person.Name','','','ResultURL','Result URL','Behavior','The url to redirect user to after they have entered search text.  (use ''{0}'' for the search text)',2,0,'',0,1,'Aug 24 2012 06:00:00:000AM','Aug 24 2012 06:00:00:000AM',1,1,'62fe83a4-e36f-4810-b650-6f8d75f53f54')
SET @AttributeId = SCOPE_IDENTITY()
INSERT INTO [coreAttributeValue] ([IsSystem],[AttributeId],[EntityId],[Order],[Value],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,@AttributeId,0,0,'Person/Search/name/{0}','Aug 24 2012 06:00:00:000AM','Aug 24 2012 06:00:00:000AM',1,1,NEWID())
INSERT INTO [coreAttribute] ([IsSystem],[FieldTypeId],[Entity],[EntityQualifierColumn],[EntityQualifierValue],[Key],[Name],[Category],[Description],[Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,1,'Rock.Search.Person.Name','','','Order','Order','','The order that this service should be used (priority)',0,0,'0',0,0,'Aug 24 2012 06:00:00:000AM','Aug 24 2012 06:00:00:000AM',1,1,'c9df58f0-49a6-4f09-8174-08dcb38c95c7')
SET @AttributeId = SCOPE_IDENTITY()
INSERT INTO [coreAttributeValue] ([IsSystem],[AttributeId],[EntityId],[Order],[Value],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,@AttributeId,0,0,'0','Aug 24 2012 06:00:00:000AM','Aug 24 2012 06:00:00:000AM',1,1,NEWID())
INSERT INTO [coreAttribute] ([IsSystem],[FieldTypeId],[Entity],[EntityQualifierColumn],[EntityQualifierValue],[Key],[Name],[Category],[Description],[Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,1,'Rock.Search.Person.Name','','','Active','Active','','Should Service be used?',0,0,'False',0,0,'Aug 24 2012 06:00:00:000AM','Aug 24 2012 06:00:00:000AM',1,1,'655a2303-4c31-4ae4-a3eb-bce266f7266b')
SET @AttributeId = SCOPE_IDENTITY()
INSERT INTO [coreAttributeValue] ([IsSystem],[AttributeId],[EntityId],[Order],[Value],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,@AttributeId,0,0,'True','Aug 24 2012 06:00:00:000AM','Aug 24 2012 06:00:00:000AM',1,1,NEWID())

-- Set Attributes for Person Phone Search Plugin
INSERT INTO [coreAttribute] ([IsSystem],[FieldTypeId],[Entity],[EntityQualifierColumn],[EntityQualifierValue],[Key],[Name],[Category],[Description],[Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,1,'Rock.Search.Person.Phone','','','SearchLabel','Search Label','Behavior','The text to display in the search type dropdown',1,0,'Phone',0,0,'Aug 24 2012 06:00:00:000AM','Aug 24 2012 06:00:00:000AM',1,1,'02f3eb66-fddc-497b-a288-27a96b75bf42')
SET @AttributeId = SCOPE_IDENTITY()
INSERT INTO [coreAttributeValue] ([IsSystem],[AttributeId],[EntityId],[Order],[Value],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,@AttributeId,0,0,'Phone','Aug 24 2012 06:00:00:000AM','Aug 24 2012 06:00:00:000AM',1,1,NEWID())
INSERT INTO [coreAttribute] ([IsSystem],[FieldTypeId],[Entity],[EntityQualifierColumn],[EntityQualifierValue],[Key],[Name],[Category],[Description],[Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,1,'Rock.Search.Person.Phone','','','ResultURL','Result URL','Behavior','The url to redirect user to after they have entered search text.  (use ''{0}'' for the search text)',2,0,'',0,1,'Aug 24 2012 06:00:00:000AM','Aug 24 2012 06:00:00:000AM',1,1,'9bda29dd-f408-4e5a-983f-d053c9bba5ad')
SET @AttributeId = SCOPE_IDENTITY()
INSERT INTO [coreAttributeValue] ([IsSystem],[AttributeId],[EntityId],[Order],[Value],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,@AttributeId,0,0,'Person/Search/phone/{0}','Aug 24 2012 06:00:00:000AM','Aug 24 2012 06:00:00:000AM',1,1,NEWID())
INSERT INTO [coreAttribute] ([IsSystem],[FieldTypeId],[Entity],[EntityQualifierColumn],[EntityQualifierValue],[Key],[Name],[Category],[Description],[Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,1,'Rock.Search.Person.Phone','','','Order','Order','','The order that this service should be used (priority)',0,0,'0',0,0,'Aug 24 2012 06:00:00:000AM','Aug 24 2012 06:00:00:000AM',1,1,'67b770a1-7e61-4a60-a894-ba23ec9c85d1')
SET @AttributeId = SCOPE_IDENTITY()
INSERT INTO [coreAttributeValue] ([IsSystem],[AttributeId],[EntityId],[Order],[Value],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,@AttributeId,0,0,'1','Aug 24 2012 06:00:00:000AM','Aug 24 2012 06:00:00:000AM',1,1,NEWID())
INSERT INTO [coreAttribute] ([IsSystem],[FieldTypeId],[Entity],[EntityQualifierColumn],[EntityQualifierValue],[Key],[Name],[Category],[Description],[Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,1,'Rock.Search.Person.Phone','','','Active','Active','','Should Service be used?',0,0,'False',0,0,'Aug 24 2012 06:00:00:000AM','Aug 24 2012 06:00:00:000AM',1,1,'8a652104-6031-4e87-ba03-8fc3dafe4eee')
SET @AttributeId = SCOPE_IDENTITY()
INSERT INTO [coreAttributeValue] ([IsSystem],[AttributeId],[EntityId],[Order],[Value],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,@AttributeId,0,0,'True','Aug 24 2012 06:00:00:000AM','Aug 24 2012 06:00:00:000AM',1,1,NEWID())

-- Set Attributes for Person Email Search Plugin
INSERT INTO [coreAttribute] ([IsSystem],[FieldTypeId],[Entity],[EntityQualifierColumn],[EntityQualifierValue],[Key],[Name],[Category],[Description],[Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,1,'Rock.Search.Person.Email','','','SearchLabel','Search Label','Behavior','The text to display in the search type dropdown',1,0,'Email',0,0,'Aug 24 2012 06:00:00:000AM','Aug 24 2012 06:00:00:000AM',1,1,'047de9db-8b67-4318-a4cb-baac2dc5e132')
SET @AttributeId = SCOPE_IDENTITY()
INSERT INTO [coreAttributeValue] ([IsSystem],[AttributeId],[EntityId],[Order],[Value],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,@AttributeId,0,0,'Email','Aug 24 2012 06:00:00:000AM','Aug 24 2012 06:00:00:000AM',1,1,NEWID())
INSERT INTO [coreAttribute] ([IsSystem],[FieldTypeId],[Entity],[EntityQualifierColumn],[EntityQualifierValue],[Key],[Name],[Category],[Description],[Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,1,'Rock.Search.Person.Email','','','ResultURL','Result URL','Behavior','The url to redirect user to after they have entered search text.  (use ''{0}'' for the search text)',2,0,'',0,1,'Aug 24 2012 06:00:00:000AM','Aug 24 2012 06:00:00:000AM',1,1,'73429747-d55e-43f1-8ca4-e1def3ecf9a7')
SET @AttributeId = SCOPE_IDENTITY()
INSERT INTO [coreAttributeValue] ([IsSystem],[AttributeId],[EntityId],[Order],[Value],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,@AttributeId,0,0,'Person/Search/email/{0}','Aug 24 2012 06:00:00:000AM','Aug 24 2012 06:00:00:000AM',1,1,NEWID())
INSERT INTO [coreAttribute] ([IsSystem],[FieldTypeId],[Entity],[EntityQualifierColumn],[EntityQualifierValue],[Key],[Name],[Category],[Description],[Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,1,'Rock.Search.Person.Email','','','Order','Order','','The order that this service should be used (priority)',0,0,'0',0,0,'Aug 24 2012 06:00:00:000AM','Aug 24 2012 06:00:00:000AM',1,1,'1a01a90f-14dd-4a48-9311-88112acd7a66')
SET @AttributeId = SCOPE_IDENTITY()
INSERT INTO [coreAttributeValue] ([IsSystem],[AttributeId],[EntityId],[Order],[Value],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,@AttributeId,0,0,'2','Aug 24 2012 06:00:00:000AM','Aug 24 2012 06:00:00:000AM',1,1,NEWID())
INSERT INTO [coreAttribute] ([IsSystem],[FieldTypeId],[Entity],[EntityQualifierColumn],[EntityQualifierValue],[Key],[Name],[Category],[Description],[Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,1,'Rock.Search.Person.Email','','','Active','Active','','Should Service be used?',0,0,'False',0,0,'Aug 24 2012 06:00:00:000AM','Aug 24 2012 06:00:00:000AM',1,1,'3bb014b2-c8f5-4480-9db4-a740d6a53716')
SET @AttributeId = SCOPE_IDENTITY()
INSERT INTO [coreAttributeValue] ([IsSystem],[AttributeId],[EntityId],[Order],[Value],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,@AttributeId,0,0,'True','Aug 24 2012 06:00:00:000AM','Aug 24 2012 06:00:00:000AM',1,1,NEWID())

-- Set Attributes for Person Address Search Plugin
INSERT INTO [coreAttribute] ([IsSystem],[FieldTypeId],[Entity],[EntityQualifierColumn],[EntityQualifierValue],[Key],[Name],[Category],[Description],[Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,1,'Rock.Search.Person.Address','','','SearchLabel','Search Label','Behavior','The text to display in the search type dropdown',1,0,'Address',0,0,'Aug 24 2012 06:00:00:000AM','Aug 24 2012 06:00:00:000AM',1,1,'A179F2BA-CE19-4BC7-A926-AF36E8103364')
SET @AttributeId = SCOPE_IDENTITY()
INSERT INTO [coreAttributeValue] ([IsSystem],[AttributeId],[EntityId],[Order],[Value],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,@AttributeId,0,0,'Address','Aug 24 2012 06:00:00:000AM','Aug 24 2012 06:00:00:000AM',1,1,NEWID())
INSERT INTO [coreAttribute] ([IsSystem],[FieldTypeId],[Entity],[EntityQualifierColumn],[EntityQualifierValue],[Key],[Name],[Category],[Description],[Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,1,'Rock.Search.Person.Address','','','ResultURL','Result URL','Behavior','The url to redirect user to after they have entered search text.  (use ''{0}'' for the search text)',2,0,'',0,1,'Aug 24 2012 06:00:00:000AM','Aug 24 2012 06:00:00:000AM',1,1,'087E5D6E-7662-4D47-BD10-A22E113C3ED2')
SET @AttributeId = SCOPE_IDENTITY()
INSERT INTO [coreAttributeValue] ([IsSystem],[AttributeId],[EntityId],[Order],[Value],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,@AttributeId,0,0,'Person/Search/address/{0}','Aug 24 2012 06:00:00:000AM','Aug 24 2012 06:00:00:000AM',1,1,NEWID())
INSERT INTO [coreAttribute] ([IsSystem],[FieldTypeId],[Entity],[EntityQualifierColumn],[EntityQualifierValue],[Key],[Name],[Category],[Description],[Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,1,'Rock.Search.Person.Address','','','Order','Order','','The order that this service should be used (priority)',0,0,'0',0,0,'Aug 24 2012 06:00:00:000AM','Aug 24 2012 06:00:00:000AM',1,1,'C377E604-EF85-4B15-BD16-B7C96BCF0DEE')
SET @AttributeId = SCOPE_IDENTITY()
INSERT INTO [coreAttributeValue] ([IsSystem],[AttributeId],[EntityId],[Order],[Value],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,@AttributeId,0,0,'3','Aug 24 2012 06:00:00:000AM','Aug 24 2012 06:00:00:000AM',1,1,NEWID())
INSERT INTO [coreAttribute] ([IsSystem],[FieldTypeId],[Entity],[EntityQualifierColumn],[EntityQualifierValue],[Key],[Name],[Category],[Description],[Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,1,'Rock.Search.Person.Address','','','Active','Active','','Should Service be used?',0,0,'False',0,0,'Aug 24 2012 06:00:00:000AM','Aug 24 2012 06:00:00:000AM',1,1,'EBD8D448-6DF2-4D0A-823D-CA969501510F')
SET @AttributeId = SCOPE_IDENTITY()
INSERT INTO [coreAttributeValue] ([IsSystem],[AttributeId],[EntityId],[Order],[Value],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,@AttributeId,0,0,'True','Aug 24 2012 06:00:00:000AM','Aug 24 2012 06:00:00:000AM',1,1,NEWID())

-- Add new Person Search Block/Page --
INSERT INTO [cmsBlock] ([IsSystem],[Path],[Name],[Description],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,'~/Blocks/CRM/PersonSearch.ascx','Person Search','Displays list of people that match a given search type and term.','Aug 24 2012 06:00:00:000AM','Aug 24 2012 06:00:00:000AM',1,1,'764D3E67-2D01-437A-9F45-9F8C97878434')
SET @BlockId = SCOPE_IDENTITY()
INSERT INTO [cmsPage] ([Name],[Title],[IsSystem],[SiteId],[Layout],[RequiresEncryption],[EnableViewState],[MenuDisplayDescription],[MenuDisplayIcon],[MenuDisplayChildPages],[DisplayInNavWhen],[Order],[OutputCacheDuration],[Description],[IncludeAdminFooter],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[IconUrl],[Guid])VALUES('Person Search','Person Search',1,1,'Default',0,1,1,0,0,0,0,0,'Screen to administrate campuses.',1,'Aug 24 2012 06:00:00:000AM','Aug 24 2012 06:00:00:000AM',1,1,NULL,'5E036ADE-C2A4-4988-B393-DAC58230F02E')
SET @PageId = SCOPE_IDENTITY()
INSERT INTO [cmsBlockInstance] ([IsSystem],[PageId],[Layout],[BlockId],[Zone],[Order],[Name],[OutputCacheDuration],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,@PageId,NULL,@BlockId,'Content',0,'Person Search',0,'Aug 24 2012 06:00:00:000AM','Aug 24 2012 06:00:00:000AM',1,1,'434CB505-016B-418A-B27A-D0FDD07DD928')
INSERT INTO [cmsPageRoute] ([IsSystem],[PageId],[Route],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,@PageId,'Person/Search/{SearchType}/{SearchTerm}','Aug 24 2012 06:00:00:000AM','Aug 24 2012 06:00:00:000AM',1,1,'1D9A7766-71D4-4CC8-A8A5-71C2D100922C')


" );
        }
        
        public override void Down()
        {
            // Remove the  Search services page and block instance
            Sql( @"
-- Delete Person Search Page
DELETE [cmsPageRoute] WHERE [Guid] = '1D9A7766-71D4-4CC8-A8A5-71C2D100922C'
DELETE [cmsBlockInstance] WHERE [Guid] = '434CB505-016B-418A-B27A-D0FDD07DD928'
DELETE [cmsPage] WHERE [Guid] = '5E036ADE-C2A4-4988-B393-DAC58230F02E'
DELETE [cmsBlock] WHERE [Guid] = '764D3E67-2D01-437A-9F45-9F8C97878434'

-- Remove Componenent Attributes
DELETE [coreAttribute] WHERE [Guid] = '3fdd185d-3fe4-4be9-b769-26c6fc5c15f9'
DELETE [coreAttribute] WHERE [Guid] = '62fe83a4-e36f-4810-b650-6f8d75f53f54'
DELETE [coreAttribute] WHERE [Guid] = 'c9df58f0-49a6-4f09-8174-08dcb38c95c7'
DELETE [coreAttribute] WHERE [Guid] = '655a2303-4c31-4ae4-a3eb-bce266f7266b'

DELETE [coreAttribute] WHERE [Guid] = '02f3eb66-fddc-497b-a288-27a96b75bf42'
DELETE [coreAttribute] WHERE [Guid] = '9bda29dd-f408-4e5a-983f-d053c9bba5ad'
DELETE [coreAttribute] WHERE [Guid] = '67b770a1-7e61-4a60-a894-ba23ec9c85d1'
DELETE [coreAttribute] WHERE [Guid] = '8a652104-6031-4e87-ba03-8fc3dafe4eee'

DELETE [coreAttribute] WHERE [Guid] = '047de9db-8b67-4318-a4cb-baac2dc5e132'
DELETE [coreAttribute] WHERE [Guid] = '73429747-d55e-43f1-8ca4-e1def3ecf9a7'
DELETE [coreAttribute] WHERE [Guid] = '1a01a90f-14dd-4a48-9311-88112acd7a66'
DELETE [coreAttribute] WHERE [Guid] = '3bb014b2-c8f5-4480-9db4-a740d6a53716'

DELETE [coreAttribute] WHERE [Guid] = 'A179F2BA-CE19-4BC7-A926-AF36E8103364'
DELETE [coreAttribute] WHERE [Guid] = '087E5D6E-7662-4D47-BD10-A22E113C3ED2'
DELETE [coreAttribute] WHERE [Guid] = 'C377E604-EF85-4B15-BD16-B7C96BCF0DEE'
DELETE [coreAttribute] WHERE [Guid] = 'EBD8D448-6DF2-4D0A-823D-CA969501510F'

-- Remove Person Search MEF Componenent Management Page
DELETE [coreAttributeValue] WHERE [Guid] = '7DC40BA8-5ECC-40CE-99C9-72A72A87D835'
DELETE [cmsBlockInstance] WHERE [Guid] = '73CE9F13-43F1-4DD4-AA5B-70A48C5A6D85'
DELETE [cmsPage] WHERE [Guid] = '1719F597-5BA9-458D-9362-9C3E558E5C82'

-- Drop index on person table for name
DROP INDEX [dbo].[crmPerson].[ix_crmPerson_LastName] 

" );
            DropIndex( "crmPhoneNumber", "Number" );
        }
    }
}
