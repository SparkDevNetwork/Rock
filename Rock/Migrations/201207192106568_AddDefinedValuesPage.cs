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
                SET IDENTITY_INSERT cmsPage ON
                INSERT INTO [cmsPage] ([Id],[Name],[Title],[IsSystem],[ParentPageId],[SiteId],[Layout],[RequiresEncryption],[EnableViewState],[MenuDisplayDescription],[MenuDisplayIcon],[MenuDisplayChildPages],[DisplayInNavWhen],[Order],[OutputCacheDuration],[Description],[IncludeAdminFooter],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[IconUrl],[Guid])VALUES(84,'Defined Types','Defined Types',0,77,1,'Default',0,1,1,0,0,0,5,0,'Screen to administrate defined types and values that will be used throughout the application.',1,'Jul 19 2012  1:47:00:757PM','Jul 19 2012  1:48:14:613PM',1,1,NULL,'E0E1DE66-B825-4BFB-A0B3-6E069AA9AA40')
                SET IDENTITY_INSERT cmsPage OFF
                
                -- cmsBlockInstance --
                SET IDENTITY_INSERT cmsBlockInstance ON
                INSERT INTO [cmsBlockInstance] ([Id],[IsSystem],[PageId],[Layout],[BlockId],[Zone],[Order],[Name],[OutputCacheDuration],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(98,0,84,NULL,63,'Content',0,'Defined Types',0,'Jul 19 2012  1:55:50:203PM','Jul 19 2012  1:55:50:203PM',1,1,'B8D83A2C-31F2-48C6-BEBC-753BCDC2A30C')
                SET IDENTITY_INSERT cmsBlockInstance OFF
            " );
        }
        
        public override void Down()
        {
            // down not needed as we're just add some seed data and the classes have been here since release 0
        }
    }
}
