using Rock.Plugin;
using com.bemaservices.Security.SSO.SystemGuids;
using Rock.Web.Cache;

namespace com.bemaservices.Security.SSO.Migrations
{
    [MigrationNumber( 1, "1.8.0" )]
    public class SSO_AddPage : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            var pageExist = PageCache.Get( Page.SSOLoginPage ) != null ? true : false;

            RockMigrationHelper.AddPage( true, Rock.SystemGuid.Page.SECURITY_ADMIN_TOOLS, "DEC70939-E041-4C9E-A4AA-5A15C0C8391F", "SSO Login Page", "", Page.SSOLoginPage );
            RockMigrationHelper.AddPageRoute( Page.SSOLoginPage, "SSO" );

            if ( pageExist == false )
            {
                RockMigrationHelper.AddSecurityAuthForPage( Page.SSOLoginPage, 0, "View", true, null, 1, Auth.SSOLoginPageAuth );
            }
         }

        public override void Down()
        {
            RockMigrationHelper.DeleteSecurityAuthForPage( Auth.SSOLoginPageAuth );
            RockMigrationHelper.DeletePage( Page.SSOLoginPage );
        }
    }

}