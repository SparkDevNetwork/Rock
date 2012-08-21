namespace Rock.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddDefinedValuesPage : DbMigration
    {
        public override void Up()
        {
            // add the page Defined Values to the admin section of the website.
            Sql( @"
-- cmsPage --
DECLARE @PageId int
DECLARE @Order int
SELECT @Order = ISNULL(MAX([order])+1,0) FROM [cmsPage] WHERE [ParentPageId] = 77;
INSERT INTO [cmsPage] ([Name],[Title],[IsSystem],[ParentPageId],[SiteId],[Layout],[RequiresEncryption],[EnableViewState],[MenuDisplayDescription],[MenuDisplayIcon],[MenuDisplayChildPages],[DisplayInNavWhen],[Order],[OutputCacheDuration],[Description],[IncludeAdminFooter],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[IconUrl],[Guid])VALUES('Defined Types','Defined Types',0,77,1,'Default',0,1,1,0,0,0,ISNULL(@Order,0),0,'Screen to administrate defined types and values that will be used throughout the application.',1,'Jul 19 2012  1:47:00:757PM','Jul 19 2012  1:48:14:613PM',1,1,NULL,'E0E1DE66-B825-4BFB-A0B3-6E069AA9AA40')
SET @PageId = SCOPE_IDENTITY()

-- cmsBlockInstance
INSERT INTO [cmsBlockInstance] ([IsSystem],[PageId],[Layout],[BlockId],[Zone],[Order],[Name],[OutputCacheDuration],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(0,@PageId,NULL,63,'Content',0,'Defined Types',0,'Jul 19 2012  1:55:50:203PM','Jul 19 2012  1:55:50:203PM',1,1,'B8D83A2C-31F2-48C6-BEBC-753BCDC2A30C')
" );
        }
        
        public override void Down()
        {
            // Remove the Defined Values page and block instance
            Sql( @"
-- cmsBlockInstance
DELETE [cmsBlockInstance] WHERE [Guid] = 'B8D83A2C-31F2-48C6-BEBC-753BCDC2A30C'

-- cmsPage --
DELETE [cmsPage] WHERE [Guid] = 'E0E1DE66-B825-4BFB-A0B3-6E069AA9AA40'
" );
        }
    }
}
