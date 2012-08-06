namespace Rock.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddPackageManager : DbMigration
    {
        public override void Up()
        {
			// add the Plugin Manager page to the admin section of the website.
			Sql( string.Format( @"
DECLARE @PluginSettingsPageGuid uniqueidentifier
SET @PluginSettingsPageGuid = '{0}'
DECLARE @PageGuid uniqueidentifier
SET @PageGuid = '{1}'
DECLARE @BlockGuid uniqueidentifier
SET @BlockGuid = '{2}'
DECLARE @BlockInstanceGuid uniqueidentifier
SET @BlockInstanceGuid = '8B083CEA-0548-4AF2-86F7-46A88FDE07D5'

-- cmsBlock --
DECLARE @BlockId int
INSERT INTO [cmsBlock] ([IsSystem],[Path],[Name],[Description],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,'~/Blocks/Administration/PluginManager.ascx','Plugin Manager','Allows installed plugins to be viewed or removed and new ones to be added from the RockQuary.','Jul 13 2012 12:50:50:000PM','Jul 13 2012 12:50:50:000PM',1,1,@BlockGuid)
SET @BlockId = SCOPE_IDENTITY()

-- find parent page --
DECLARE @ParentPageId int
SELECT @ParentPageId = [Id] FROM cmsPage WHERE [Guid] = @PluginSettingsPageGuid

-- cmsPage --
DECLARE @PageId int
DECLARE @Order int
SELECT @Order = ISNULL(MAX([order])+1,0) FROM [cmsPage] WHERE [ParentPageId] = @ParentPageId;
INSERT INTO [cmsPage] ([Name],[Title],[IsSystem],[ParentPageId],[SiteId],[Layout],[RequiresEncryption],[EnableViewState],[MenuDisplayDescription],[MenuDisplayIcon],[MenuDisplayChildPages],[DisplayInNavWhen],[Order],[OutputCacheDuration],[Description],[IncludeAdminFooter],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[IconUrl],[Guid])
	VALUES('Plugin Manager','Plugin Manager',1,@ParentPageId,1,'Default',0,1,0,0,0,0,ISNULL(@Order,0),0,'Screen to administrate plugins.',1,'Jul 13 2012 12:50:50:000PM','Jul 13 2012 12:50:50:000PM',1,1,NULL,@PageGuid)
SET @PageId = SCOPE_IDENTITY()

-- cmsBlockInstance
INSERT INTO [cmsBlockInstance] ([IsSystem],[PageId],[Layout],[BlockId],[Zone],[Order],[Name],[OutputCacheDuration],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])
VALUES(1,@PageId,NULL,@BlockId,'Content',0,'Plugin Manager',0,'Jul 13 2012 12:50:50:000PM','Jul 13 2012 12:50:50:000PM',1,1,@BlockInstanceGuid)

-- coreAttribute --
DECLARE @AttributeId int
INSERT INTO [coreAttribute] ([IsSystem],[FieldTypeId],[Entity],[EntityQualifierColumn],[EntityQualifierValue],[Key],[Name],[Category],[Description],[Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])
	VALUES(1,1,'','','','PackageSourceUrl','Package Source URL','Config','URL to the Rock Quarry plugin source repository API (v2)',0,0,'http://quarry.rockchms.com/api/v2/',0,0,'Jul 13 2012 12:50:50:000PM','Jul 13 2012 12:50:50:000PM',1,1,'306E7E7C-9416-4098-9C25-488380B940A5')
SET @AttributeId = SCOPE_IDENTITY()

-- coreAttributeValue --
INSERT INTO [coreAttributeValue] ([IsSystem],[AttributeId],[EntityId],[Order],[Value],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])
	VALUES(0,@AttributeId,NULL,NULL,'http://quarry.rockchms.com/api/v2/','Jul 13 2012 12:50:00:000PM','Jul 13 2012 12:50:00:000PM',1,1,'5CB48974-6BB6-435B-A04A-2BF9B7CD778E')

-- cmsAuth --
INSERT INTO [cmsAuth] ([EntityType],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[PersonId],[GroupId],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])
	VALUES('CMS.Page',@PageId,0,'Edit','A',0,NULL,2,'Jul 13 2012 12:50:00:000PM','Jul 13 2012 12:50:00:000PM',1,1,'632E474F-29D0-4D7D-A51A-5FD2D8DE74FE')

", Rock.SystemGuid.Page.PLUGIN_SETTINGS, Rock.SystemGuid.Page.PLUGIN_MANAGER, Rock.SystemGuid.Block.PLUGIN_MANAGER ) );
        }
        
        public override void Down()
        {

			// Remove the block instance, the page and the block
			Sql( string.Format( @"
-- cmsBlockInstance --
DELETE [cmsBlockInstance] WHERE [Guid] = '8B083CEA-0548-4AF2-86F7-46A88FDE07D5'

-- cmsPage --
DELETE [cmsPage] WHERE [Guid] = '{0}'

-- cmsBlock --
DELETE [cmsBlock] WHERE [Guid] = '{1}'

-- coreAttribute --
DELETE [coreAttribute] WHERE [Guid] = '306E7E7C-9416-4098-9C25-488380B940A5'

-- coreAttributeValue --
DELETE [coreAttributeValue] WHERE [Guid] = '5CB48974-6BB6-435B-A04A-2BF9B7CD778E'


", Rock.SystemGuid.Page.PLUGIN_MANAGER, Rock.SystemGuid.Block.PLUGIN_MANAGER ));
        }
    }
}
