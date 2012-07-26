namespace Rock.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddSystemInfoPage : DbMigration
    {
        public override void Up()
        {
            // add the System Info page to the admin section of the website.
            Sql( @"
-- cmsBlock --
DECLARE @BlockId int
INSERT INTO [cmsBlock] ([IsSystem],[Path],[Name],[Description],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,'~/Blocks/Administration/SystemInfo.ascx','System Information','Displays status and performance information for the currently running instance of Rock ChMS','Jul 25 2012 06:00:00:000AM','Jul 25 2012 06:00:00:000AM',1,1,'DE08EFD7-4CF9-4BD5-9F72-C0151FD08523')
SET @BlockId = SCOPE_IDENTITY()

-- cmsPage --
DECLARE @PageId int
DECLARE @Order int
SELECT @Order = ISNULL(MAX([order])+1,0) FROM [cmsPage] WHERE [ParentPageId] = 77;
INSERT INTO [cmsPage] ([Name],[Title],[IsSystem],[ParentPageId],[SiteId],[Layout],[RequiresEncryption],[EnableViewState],[MenuDisplayDescription],[MenuDisplayIcon],[MenuDisplayChildPages],[DisplayInNavWhen],[Order],[OutputCacheDuration],[Description],[IncludeAdminFooter],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[IconUrl],[Guid])VALUES('System Information','System Information',0,77,1,'Default',0,1,1,0,0,0,ISNULL(@Order,0),0,'Displays status and performance information for the currently running instance of Rock ChMS',1,'Jul 25 2012 06:00:00:000AM','Jul 25 2012 06:00:00:000AM',1,1,NULL,'8A97CC93-3E93-4286-8440-E5217B65A904')
SET @PageId = SCOPE_IDENTITY()

-- cmsBlockInstance
INSERT INTO [cmsBlockInstance] ([IsSystem],[PageId],[Layout],[BlockId],[Zone],[Order],[Name],[OutputCacheDuration],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(0,@PageId,NULL,@BlockId,'Content',0,'SystemInfo',0,'Jul 25 2012 06:00:00:000AM','Jul 25 2012 06:00:00:000AM',1,1,'D1FD9A6B-C213-4074-8D84-EE5353635443')
" );
        }

        public override void Down()
        {
            // Remove the System Info page and block instance
            Sql( @"
-- cmsBlockInstance
DELETE [cmsBlockInstance] WHERE [Guid] = 'D1FD9A6B-C213-4074-8D84-EE5353635443'

-- cmsPage --
DELETE [cmsPage] WHERE [Guid] = '8A97CC93-3E93-4286-8440-E5217B65A904'

-- cmsBlock --
DELETE [cmsBlock] WHERE [Guid] = 'DE08EFD7-4CF9-4BD5-9F72-C0151FD08523'
" );
        }
    }
}
