namespace Rock.Migrations
{
    using System.Data.Entity.Migrations;
#pragma warning disable 1591
    public partial class AddCampusPage : DbMigration
    {
        public override void Up()
        {
            // add the Campus page to the admin section of the website.
            Sql( @"
-- cmsBlock --
DECLARE @BlockId int
INSERT INTO [cmsBlock] ([IsSystem],[Path],[Name],[Description],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,'~/Blocks/Administration/Campuses.ascx','Campuses','Allows for the administration of campuses.','Jul 23 2012 06:00:00:000AM','Jul 23 2012 06:00:00:000AM',1,1,'0D0DC731-E282-44EA-AD1E-89D16AB20192')
SET @BlockId = SCOPE_IDENTITY()

-- cmsPage --
DECLARE @PageId int
DECLARE @Order int
SELECT @Order = ISNULL(MAX([order])+1,0) FROM [cmsPage] WHERE [ParentPageId] = 77;
INSERT INTO [cmsPage] ([Name],[Title],[IsSystem],[ParentPageId],[SiteId],[Layout],[RequiresEncryption],[EnableViewState],[MenuDisplayDescription],[MenuDisplayIcon],[MenuDisplayChildPages],[DisplayInNavWhen],[Order],[OutputCacheDuration],[Description],[IncludeAdminFooter],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[IconUrl],[Guid])VALUES('Campuses','Campuses',1,77,1,'Default',0,1,1,0,0,0,ISNULL(@Order,0),0,'Screen to administrate campuses.',1,'Jul 23 2012 06:00:00:000AM','Jul 23 2012 06:00:00:000AM',1,1,NULL,'5EE91A54-C750-48DC-9392-F1F0F0581C3A')
SET @PageId = SCOPE_IDENTITY()

-- cmsBlockInstance
INSERT INTO [cmsBlockInstance] ([IsSystem],[PageId],[Layout],[BlockId],[Zone],[Order],[Name],[OutputCacheDuration],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,@PageId,NULL,@BlockId,'Content',0,'Campuses',0,'Jul 23 2012 06:00:00:000AM','Jul 23 2012 06:00:00:000AM',1,1,'CB71352B-C10B-453A-8879-0EFF8707355A')
" );
        }

        public override void Down()
        {
            // Remove the Campus page and block instance
            Sql( @"
-- cmsBlockInstance
DELETE [cmsBlockInstance] WHERE [Guid] = 'CB71352B-C10B-453A-8879-0EFF8707355A'

-- cmsPage --
DELETE [cmsPage] WHERE [Guid] = '5EE91A54-C750-48DC-9392-F1F0F0581C3A'

-- cmsBlock --
DELETE [cmsBlock] WHERE [Guid] = '0D0DC731-E282-44EA-AD1E-89D16AB20192'
" );
        }
    }
}
