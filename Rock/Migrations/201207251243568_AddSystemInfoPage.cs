namespace Rock.Migrations
{
    using System.Data.Entity.Migrations;
#pragma warning disable 1591
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
SELECT @Order = ISNULL(MAX([order])+1,0) FROM [cmsPage] WHERE [ParentPageId] = 74;
INSERT INTO [cmsPage] ([Name],[Title],[IsSystem],[ParentPageId],[SiteId],[Layout],[RequiresEncryption],[EnableViewState],[MenuDisplayDescription],[MenuDisplayIcon],[MenuDisplayChildPages],[DisplayInNavWhen],[Order],[OutputCacheDuration],[Description],[IncludeAdminFooter],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[IconUrl],[Guid])VALUES('System Information','System Information',1,74,1,'Dialog',0,1,0,0,0,2,ISNULL(@Order,0),0,'Displays status and performance information for the currently running instance of Rock ChMS',0,'Jul 25 2012 06:00:00:000AM','Jul 25 2012 06:00:00:000AM',1,1,NULL,'8A97CC93-3E93-4286-8440-E5217B65A904')
SET @PageId = SCOPE_IDENTITY()

-- cmsPageRoute
INSERT INTO [cmsPageRoute] ([IsSystem],[PageId],[Route],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,@PageId,'SystemInfo','Jul 25 2012 06:00:00:000AM','Jul 25 2012 06:00:00:000AM',1,1,'617CF50F-C199-470A-8B32-F9115BDD02C0')

-- cmsBlockInstance
INSERT INTO [cmsBlockInstance] ([IsSystem],[PageId],[Layout],[BlockId],[Zone],[Order],[Name],[OutputCacheDuration],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,@PageId,NULL,@BlockId,'Content',0,'SystemInfo',0,'Jul 25 2012 06:00:00:000AM','Jul 25 2012 06:00:00:000AM',1,1,'D1FD9A6B-C213-4074-8D84-EE5353635443')
" );
        }

        public override void Down()
        {
            // Remove the System Info page and block instance
            Sql( @"
-- cmsBlockInstance
DELETE [cmsBlockInstance] WHERE [Guid] = 'D1FD9A6B-C213-4074-8D84-EE5353635443'

-- cmsPageRoute
DELETE [cmsPageRoute] WHERE [Guid] = '617CF50F-C199-470A-8B32-F9115BDD02C0'

-- cmsPage --
DELETE [cmsPage] WHERE [Guid] = '8A97CC93-3E93-4286-8440-E5217B65A904'

-- cmsBlock --
DELETE [cmsBlock] WHERE [Guid] = 'DE08EFD7-4CF9-4BD5-9F72-C0151FD08523'
" );
        }
    }
}
