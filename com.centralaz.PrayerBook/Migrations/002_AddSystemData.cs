using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Constants;

namespace com.centralaz.Prayerbook.Migrations
{
    [MigrationNumber( 2, "1.1.0" )]
    public class AddSystemData2 : Migration
    {
        public override void Up()
        {
            //Hide App Management Page from BreadCrumbs
            Sql( string.Format( @"
                    BEGIN
                        UPDATE [Page] SET
                            [BreadCrumbDisplayName] = 0,
                            [BreadCrumbDisplayIcon] = 0
                        WHERE
                            [Guid] = '{0}'
                    END
                ", 
                 com.centralaz.Prayerbook.SystemGuid.Page.APP_MANAGEMENT_PAGE ) );
          
            //Add isActive AttributeValue to sample Ministry and sample Subministry
            var rockContext = new RockContext();
            var definedValueService = new DefinedValueService( rockContext );
            var sampleMinistryDefinedValue = definedValueService.Get( Guid.Parse( com.centralaz.Prayerbook.SystemGuid.DefinedValue.SAMPLE_MINISTRY_DEFINEDVALUE ) );
            var sampleSubministryDefinedValue = definedValueService.Get( Guid.Parse( com.centralaz.Prayerbook.SystemGuid.DefinedValue.SAMPLE_SUBMINISTRY_DEFINEDVALUE ) );
            RockMigrationHelper.AddAttributeValue( com.centralaz.Prayerbook.SystemGuid.Attribute.MINISTRY_ISACTIVE_ATTRIBTUTE, sampleMinistryDefinedValue.Id, "True", com.centralaz.Prayerbook.SystemGuid.AttributeValue.SAMPLE_MINISTRY_ISACTIVE_ATTRIBUTE_VALUE );
            RockMigrationHelper.AddAttributeValue( com.centralaz.Prayerbook.SystemGuid.Attribute.SUBMINISTRY_ISACTIVE_ATTRIBTUTE, sampleSubministryDefinedValue.Id, "True", com.centralaz.Prayerbook.SystemGuid.AttributeValue.SAMPLE_SUBMINISTRY_ISACTIVE_ATTRIBTUE_VALUE );

            //Add security to App Homepage
            RockMigrationHelper.AddSecurityAuthForPage( com.centralaz.Prayerbook.SystemGuid.Page.HOME_PAGE, 0, Rock.Security.Authorization.ADMINISTRATE, true, Rock.SystemGuid.Group.GROUP_ADMINISTRATORS, 0, com.centralaz.Prayerbook.SystemGuid.Auth.HOMEPAGE_ROCKADMINS_ALLOW_ADMINISTRATE_AUTH );
            RockMigrationHelper.AddSecurityAuthForPage( com.centralaz.Prayerbook.SystemGuid.Page.HOME_PAGE, 1, Rock.Security.Authorization.ADMINISTRATE, true, com.centralaz.Prayerbook.SystemGuid.Group.ADMINISTRATORS_GROUP, 0, com.centralaz.Prayerbook.SystemGuid.Auth.HOMEPAGE_UPTEAMADMINS_ALLOW_ADMINISTRATE_AUTH );

            RockMigrationHelper.AddSecurityAuthForPage( com.centralaz.Prayerbook.SystemGuid.Page.HOME_PAGE, 0, Rock.Security.Authorization.EDIT, true, com.centralaz.Prayerbook.SystemGuid.Group.ADMINISTRATORS_GROUP, 0, com.centralaz.Prayerbook.SystemGuid.Auth.HOMEPAGE_UPTEAMADMINS_ALLOW_EDIT_AUTH );
            RockMigrationHelper.AddSecurityAuthForPage( com.centralaz.Prayerbook.SystemGuid.Page.HOME_PAGE, 1, Rock.Security.Authorization.EDIT, true, com.centralaz.Prayerbook.SystemGuid.Group.CONTRIBUTORS_GROUP, 0, com.centralaz.Prayerbook.SystemGuid.Auth.HOMEPAGE_UPTEAMCONTRIBUTORS_ALLOW_EDIT_AUTH );

            RockMigrationHelper.AddSecurityAuthForPage( com.centralaz.Prayerbook.SystemGuid.Page.HOME_PAGE, 0, Rock.Security.Authorization.VIEW, true, Rock.SystemGuid.Group.GROUP_ADMINISTRATORS, 0, com.centralaz.Prayerbook.SystemGuid.Auth.HOMEPAGE_ROCKADMINS_ALLOW_VIEW_AUTH );
            RockMigrationHelper.AddSecurityAuthForPage( com.centralaz.Prayerbook.SystemGuid.Page.HOME_PAGE, 1, Rock.Security.Authorization.VIEW, true, com.centralaz.Prayerbook.SystemGuid.Group.ADMINISTRATORS_GROUP, 0, com.centralaz.Prayerbook.SystemGuid.Auth.HOMEPAGE_UPTEAMADMINS_ALLOW_VIEW_AUTH );
            RockMigrationHelper.AddSecurityAuthForPage( com.centralaz.Prayerbook.SystemGuid.Page.HOME_PAGE, 2, Rock.Security.Authorization.VIEW, true, com.centralaz.Prayerbook.SystemGuid.Group.CONTRIBUTORS_GROUP, 0, com.centralaz.Prayerbook.SystemGuid.Auth.HOMEPAGE_UPTEAMCONTRIBUTORS_ALLOW_VIEW_AUTH );
            RockMigrationHelper.AddSecurityAuthForPage( com.centralaz.Prayerbook.SystemGuid.Page.HOME_PAGE, 3, Rock.Security.Authorization.VIEW, false, null, Rock.Model.SpecialRole.AllUsers.ConvertToInt(), com.centralaz.Prayerbook.SystemGuid.Auth.HOMEPAGE_ALLUSERS_DENY_VIEW_AUTH );

            //Add security to App Management Pages
            RockMigrationHelper.AddSecurityAuthForPage( com.centralaz.Prayerbook.SystemGuid.Page.APP_MANAGEMENT_PAGE, 0, Rock.Security.Authorization.VIEW, false, com.centralaz.Prayerbook.SystemGuid.Group.CONTRIBUTORS_GROUP, 0, com.centralaz.Prayerbook.SystemGuid.Auth.APPMANAGEMENT_UPTEAMCONTRIBUTORS_DENY_VIEW_AUTH );
            RockMigrationHelper.AddSecurityAuthForPage( com.centralaz.Prayerbook.SystemGuid.Page.APP_MANAGEMENT_PAGE, 0, Rock.Security.Authorization.EDIT, false, com.centralaz.Prayerbook.SystemGuid.Group.CONTRIBUTORS_GROUP, 0, com.centralaz.Prayerbook.SystemGuid.Auth.APPMANAGEMENT_UPTEAMCONTRIBUTORS_DENY_EDIT_AUTH );

            RockMigrationHelper.AddSecurityAuthForPage( com.centralaz.Prayerbook.SystemGuid.Page.ADMINISTRATORS_GROUP_MANAGEMENT_PAGE, 0, Rock.Security.Authorization.VIEW, false, com.centralaz.Prayerbook.SystemGuid.Group.CONTRIBUTORS_GROUP, 0, com.centralaz.Prayerbook.SystemGuid.Auth.ADMINGROUPMGT_UPTEAMCONTRIBUTORS_DENY_VIEW_AUTH );
            RockMigrationHelper.AddSecurityAuthForPage( com.centralaz.Prayerbook.SystemGuid.Page.ADMINISTRATORS_GROUP_MANAGEMENT_PAGE, 0, Rock.Security.Authorization.EDIT, false, com.centralaz.Prayerbook.SystemGuid.Group.CONTRIBUTORS_GROUP, 0, com.centralaz.Prayerbook.SystemGuid.Auth.ADMINGROUPMGT_UPTEAMCONTRIBUTORS_DENY_EDIT_AUTH );

            RockMigrationHelper.AddSecurityAuthForPage( com.centralaz.Prayerbook.SystemGuid.Page.ADMINISTRATORS_GROUP_MEMBER_DETAIL_PAGE, 0, Rock.Security.Authorization.VIEW, false, com.centralaz.Prayerbook.SystemGuid.Group.CONTRIBUTORS_GROUP, 0, com.centralaz.Prayerbook.SystemGuid.Auth.ADMINGROUPMGTMEMBERDETAIL_UPTEAMCONTRIBUTORS_DENY_VIEW_AUTH );
            RockMigrationHelper.AddSecurityAuthForPage( com.centralaz.Prayerbook.SystemGuid.Page.ADMINISTRATORS_GROUP_MEMBER_DETAIL_PAGE, 0, Rock.Security.Authorization.EDIT, false, com.centralaz.Prayerbook.SystemGuid.Group.CONTRIBUTORS_GROUP, 0, com.centralaz.Prayerbook.SystemGuid.Auth.ADMINGROUPMGTMEMBERDETAIL_UPTEAMCONTRIBUTORS_DENY_EDIT_AUTH );

            RockMigrationHelper.AddSecurityAuthForPage( com.centralaz.Prayerbook.SystemGuid.Page.CONTRIBUTORS_GROUP_MANAGEMENT_PAGE, 0, Rock.Security.Authorization.VIEW, false, com.centralaz.Prayerbook.SystemGuid.Group.CONTRIBUTORS_GROUP, 0, com.centralaz.Prayerbook.SystemGuid.Auth.CONTRIBROUPMGT_UPTEAMCONTRIBUTORS_DENY_VIEW_AUTH );
            RockMigrationHelper.AddSecurityAuthForPage( com.centralaz.Prayerbook.SystemGuid.Page.CONTRIBUTORS_GROUP_MANAGEMENT_PAGE, 0, Rock.Security.Authorization.EDIT, false, com.centralaz.Prayerbook.SystemGuid.Group.CONTRIBUTORS_GROUP, 0, com.centralaz.Prayerbook.SystemGuid.Auth.CONTRIBROUPMGT_UPTEAMCONTRIBUTORS_DENY_EDIT_AUTH );

            RockMigrationHelper.AddSecurityAuthForPage( com.centralaz.Prayerbook.SystemGuid.Page.CONTRIBUTORS_GROUP_MEMBER_DETAIL_PAGE, 0, Rock.Security.Authorization.VIEW, false, com.centralaz.Prayerbook.SystemGuid.Group.CONTRIBUTORS_GROUP, 0, com.centralaz.Prayerbook.SystemGuid.Auth.CONTRIBROUPMGTMEMBERDETAIL_UPTEAMCONTRIBUTORS_DENY_VIEW_AUTH );
            RockMigrationHelper.AddSecurityAuthForPage( com.centralaz.Prayerbook.SystemGuid.Page.CONTRIBUTORS_GROUP_MEMBER_DETAIL_PAGE, 0, Rock.Security.Authorization.EDIT, false, com.centralaz.Prayerbook.SystemGuid.Group.CONTRIBUTORS_GROUP, 0, com.centralaz.Prayerbook.SystemGuid.Auth.CONTRIBROUPMGTMEMBERDETAIL_UPTEAMCONTRIBUTORS_DENY_EDIT_AUTH );

            RockMigrationHelper.AddSecurityAuthForPage( com.centralaz.Prayerbook.SystemGuid.Page.MINISTRY_LIST_MANAGEMENT_PAGE, 0, Rock.Security.Authorization.VIEW, false, com.centralaz.Prayerbook.SystemGuid.Group.CONTRIBUTORS_GROUP, 0, com.centralaz.Prayerbook.SystemGuid.Auth.MINISTRYLISTMGT_UPTEAMCONTRIBUTORS_DENY_VIEW_AUTH );
            RockMigrationHelper.AddSecurityAuthForPage( com.centralaz.Prayerbook.SystemGuid.Page.MINISTRY_LIST_MANAGEMENT_PAGE, 0, Rock.Security.Authorization.EDIT, false, com.centralaz.Prayerbook.SystemGuid.Group.CONTRIBUTORS_GROUP, 0, com.centralaz.Prayerbook.SystemGuid.Auth.MINISTRYLISTMGT_UPTEAMCONTRIBUTORS_DENY_EDIT_AUTH );

            RockMigrationHelper.AddSecurityAuthForPage( com.centralaz.Prayerbook.SystemGuid.Page.SUBMINISTRY_LIST_MANAGEMENT_PAGE, 0, Rock.Security.Authorization.VIEW, false, com.centralaz.Prayerbook.SystemGuid.Group.CONTRIBUTORS_GROUP, 0, com.centralaz.Prayerbook.SystemGuid.Auth.SUBMINISTRYLISTMGT_UPTEAMCONTRIBUTORS_DENY_VIEW_AUTH );
            RockMigrationHelper.AddSecurityAuthForPage( com.centralaz.Prayerbook.SystemGuid.Page.SUBMINISTRY_LIST_MANAGEMENT_PAGE, 0, Rock.Security.Authorization.EDIT, false, com.centralaz.Prayerbook.SystemGuid.Group.CONTRIBUTORS_GROUP, 0, com.centralaz.Prayerbook.SystemGuid.Auth.SUBMINISTRYLISTMGT_UPTEAMCONTRIBUTORS_DENY_EDIT_AUTH );

            RockMigrationHelper.AddSecurityAuthForPage( com.centralaz.Prayerbook.SystemGuid.Page.BOOK_MANAGEMENT_PAGE, 0, Rock.Security.Authorization.VIEW, false, com.centralaz.Prayerbook.SystemGuid.Group.CONTRIBUTORS_GROUP, 0, com.centralaz.Prayerbook.SystemGuid.Auth.BOOKMGT_UPTEAMCONTRIBUTORS_DENY_VIEW_AUTH );
            RockMigrationHelper.AddSecurityAuthForPage( com.centralaz.Prayerbook.SystemGuid.Page.BOOK_MANAGEMENT_PAGE, 0, Rock.Security.Authorization.EDIT, false, com.centralaz.Prayerbook.SystemGuid.Group.CONTRIBUTORS_GROUP, 0, com.centralaz.Prayerbook.SystemGuid.Auth.BOOKMGT_UPTEAMCONTRIBUTORS_DENY_EDIT_AUTH );
            
            //Add security to Management Page Navigation Block
            RockMigrationHelper.AddSecurityAuthForBlock( com.centralaz.Prayerbook.SystemGuid.Block.HOMEPAGE_PAGEMENU_BLOCK, 0, Rock.Security.Authorization.VIEW, false, com.centralaz.Prayerbook.SystemGuid.Group.CONTRIBUTORS_GROUP, Rock.Model.SpecialRole.None, com.centralaz.Prayerbook.SystemGuid.Auth.HOMEPAGE_UPTEAMCONTRIBUTORS_DENY_VIEW_PAGEMENUBLOCK_AUTH );
            RockMigrationHelper.AddSecurityAuthForBlock( com.centralaz.Prayerbook.SystemGuid.Block.HOMEPAGE_PAGEMENU_BLOCK, 0, Rock.Security.Authorization.EDIT, false, com.centralaz.Prayerbook.SystemGuid.Group.CONTRIBUTORS_GROUP, Rock.Model.SpecialRole.None, com.centralaz.Prayerbook.SystemGuid.Auth.HOMEPAGE_UPTEAMCONTRIBUTORS_DENY_EDIT_PAGEMENUBLOCK_AUTH );
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}
