namespace Rock.Migrations
    
    using System.Data.Entity.Migrations;
#pragma warning disable 1591
    public partial class RemoveGlobalValuesPage : DbMigration
        
        public override void Up()
            
            Sql( @"
DECLARE @PageId int
SET @PageId = (SELECT [Id] FROM [cmsPage] WHERE [Guid] = 'D5550020-0BD0-43E6-806B-25338830F244')

DELETE [cmsBlockInstance] WHERE [PageId] = @PageId
DELETE [cmsPage] WHERE [Id] = @PageId
DELETE [cmsAuth] WHERE [EntityType] = 'CMS.Page' AND [EntityId] = @PageId

-- attributes block
DECLARE @BlockId int
SET @BlockId = (SELECT [Id] FROM [cmsBlock] WHERE [Guid] = 'E5EA2F6D-43A2-48E0-B59C-4409B78AC830')

-- global attributes instance
DECLARE @BlockInstanceId int
SET @BlockInstanceId = (SELECT [Id] FROM [cmsBlockInstance] WHERE [Guid] = '3CBB177B-DBFB-4FB2-A1A7-957DC6C350EB')

-- add new attribute block attributes
DECLARE @AttributeId int
INSERT INTO [coreAttribute] ([IsSystem],[FieldTypeId],[Entity],[EntityQualifierColumn],[EntityQualifierValue],[Key],[Name],[Category],[Description],[Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,1,'Rock.CMS.BlockInstance','BlockId',CAST(@BlockId AS varchar),'EntityId','Entity Id','Set Values','The entity id that values apply to',4,0,'',0,0,'Aug 16 2012 06:00:00:000AM','Aug 16 2012 06:00:00:000AM',1,1,'CBB56D68-3727-42B9-BF13-0B2B593FB328')
SET @AttributeId = SCOPE_IDENTITY()
INSERT INTO [coreAttributeValue] ([IsSystem],[AttributeId],[EntityId],[Order],[Value],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,@AttributeId,@BlockInstanceId,0,'','Aug 16 2012 06:00:00:000AM','Aug 16 2012 06:00:00:000AM',1,1,'8AB246E3-5197-4F47-8C98-D4EB704FBC57')

INSERT INTO [coreAttribute] ([IsSystem],[FieldTypeId],[Entity],[EntityQualifierColumn],[EntityQualifierValue],[Key],[Name],[Category],[Description],[Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,3,'Rock.CMS.BlockInstance','BlockId',CAST(@BlockId AS varchar),'SetValues','Allow Setting of Values','Set Values','Should UI be available for setting values of the specified Entity ID?',3,0,'false',0,0,'Aug 16 2012 06:00:00:000AM','Aug 16 2012 06:00:00:000AM',1,1,'018C0016-C253-44E4-84DB-D166084C5CAD')
SET @AttributeId = SCOPE_IDENTITY()
INSERT INTO [coreAttributeValue] ([IsSystem],[AttributeId],[EntityId],[Order],[Value],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,@AttributeId,@BlockInstanceId,0,'True','Aug 16 2012 06:00:00:000AM','Aug 16 2012 06:00:00:000AM',1,1,'FC554EDD-2130-4FAE-94A9-C4D9691DEF9E')

" );
        }
        
        public override void Down()
            
            Sql( @"
-- delete new Attribute block attributes
DELETE [coreAttributeValue] WHERE [Guid] = '8AB246E3-5197-4F47-8C98-D4EB704FBC57'
DELETE [coreAttributeValue] WHERE [Guid] = 'FC554EDD-2130-4FAE-94A9-C4D9691DEF9E'
DELETE [coreAttribute] WHERE [Guid] = 'CBB56D68-3727-42B9-BF13-0B2B593FB328'
DELETE [coreAttribute] WHERE [Guid] = '018C0016-C253-44E4-84DB-D166084C5CAD'

-- attribute value block --
DECLARE @BlockId int
SET @BlockId = (SELECT [Id] FROM [cmsBlock] WHERE [Guid] = 'B084F060-ECE4-462A-B6D0-35B2A30AF3DF')

-- global attribute value page --
DECLARE @PageId int
DECLARE @Order int
SELECT @Order = ISNULL(MAX([order])+1,0) FROM [cmsPage] WHERE [ParentPageId] = 77;
INSERT INTO [cmsPage] ([Name],[Title],[IsSystem],[ParentPageId],[SiteId],[Layout],[RequiresEncryption],[EnableViewState],[MenuDisplayDescription],[MenuDisplayIcon],[MenuDisplayChildPages],[DisplayInNavWhen],[Order],[OutputCacheDuration],[Description],[IncludeAdminFooter],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[IconUrl],[Guid])VALUES('Global Values','Global Values',1,77,1,'Default',0,1,1,0,0,0,@Order,0,'Set the values of Global Attributes',1,'Aug 16 2012 06:00:00:000AM','Aug 16 2012 06:00:00:000AM',1,1,NULL,'D5550020-0BD0-43E6-806B-25338830F244')
SET @PageId = SCOPE_IDENTITY()

-- cmsBlockInstance
INSERT INTO [cmsBlockInstance] ([IsSystem],[PageId],[Layout],[BlockId],[Zone],[Order],[Name],[OutputCacheDuration],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,@PageId,NULL,@BlockId,'Content',0,'Org Values',0,'Aug 16 2012 06:00:00:000AM','Aug 16 2012 06:00:00:000AM',1,1,'89FE9265-3087-4334-B990-CE06E91925EE')
INSERT INTO [cmsAuth] ([EntityType],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[PersonId],[GroupId],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES('CMS.Page',@PageId,0,'Configure','A',0,NULL,2,'Aug 16 2012 06:00:00:000AM','Aug 16 2012 06:00:00:000AM',1,1,'228DB32F-DF56-403E-B31F-4163C6829119')
INSERT INTO [cmsAuth] ([EntityType],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[PersonId],[GroupId],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES('CMS.Page',@PageId,0,'Edit','A',0,NULL,2,'Aug 16 2012 06:00:00:000AM','Aug 16 2012 06:00:00:000AM',1,1,'1CEACC65-4140-4682-9D48-77F3D7FE1EDB')
" );
        
        }
    }
}
