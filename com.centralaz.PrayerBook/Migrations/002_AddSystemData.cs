// <copyright>
// Copyright by Central Christian Church
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
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

            //Add security to App Management pages (UP admin can view but not Contributors)
            // allow Rock admins
            RockMigrationHelper.AddSecurityAuthForPage( com.centralaz.Prayerbook.SystemGuid.Page.APP_MANAGEMENT_PAGE, 0, Rock.Security.Authorization.VIEW, true, "628C51A8-4613-43ED-A18D-4A6FB999273E" /* Rock Administrators */, 0, "1901672F-BEC8-40E1-83EE-CCD08311C322" );
            RockMigrationHelper.AddSecurityAuthForPage( com.centralaz.Prayerbook.SystemGuid.Page.APP_MANAGEMENT_PAGE, 0, Rock.Security.Authorization.EDIT, true, "628C51A8-4613-43ED-A18D-4A6FB999273E" /* Rock Administrators */, 0, "819CFB65-55FF-4B57-A1E4-6D07B0E96F9A" );
            // allow UP admins
            RockMigrationHelper.AddSecurityAuthForPage( com.centralaz.Prayerbook.SystemGuid.Page.APP_MANAGEMENT_PAGE, 1, Rock.Security.Authorization.VIEW, true, com.centralaz.Prayerbook.SystemGuid.Group.ADMINISTRATORS_GROUP, 0, "6B5C1F99-7FCD-48ED-BDAA-DD012EB1C62F" );
            RockMigrationHelper.AddSecurityAuthForPage( com.centralaz.Prayerbook.SystemGuid.Page.APP_MANAGEMENT_PAGE, 1, Rock.Security.Authorization.EDIT, true, com.centralaz.Prayerbook.SystemGuid.Group.ADMINISTRATORS_GROUP, 0, "6B5FCF8A-53C6-42E9-AD76-2FEB8D4C2BF6" );
            // deny contributors
            RockMigrationHelper.AddSecurityAuthForPage( com.centralaz.Prayerbook.SystemGuid.Page.APP_MANAGEMENT_PAGE, 2, Rock.Security.Authorization.VIEW, false, com.centralaz.Prayerbook.SystemGuid.Group.CONTRIBUTORS_GROUP, 0, com.centralaz.Prayerbook.SystemGuid.Auth.APPMANAGEMENT_UPTEAMCONTRIBUTORS_DENY_VIEW_AUTH );
            RockMigrationHelper.AddSecurityAuthForPage( com.centralaz.Prayerbook.SystemGuid.Page.APP_MANAGEMENT_PAGE, 2, Rock.Security.Authorization.EDIT, false, com.centralaz.Prayerbook.SystemGuid.Group.CONTRIBUTORS_GROUP, 0, com.centralaz.Prayerbook.SystemGuid.Auth.APPMANAGEMENT_UPTEAMCONTRIBUTORS_DENY_EDIT_AUTH );

            //Add security to Management Page Navigation Block (UP admin can view but not Contributors)
            // allow Rock admins
            RockMigrationHelper.AddSecurityAuthForBlock( com.centralaz.Prayerbook.SystemGuid.Block.HOMEPAGE_PAGEMENU_BLOCK, 0, Rock.Security.Authorization.VIEW, true, "628C51A8-4613-43ED-A18D-4A6FB999273E" /* Rock Administrators */, Rock.Model.SpecialRole.None, "16C392B4-A2B2-4E30-A903-1003AD427D69" );
            RockMigrationHelper.AddSecurityAuthForBlock( com.centralaz.Prayerbook.SystemGuid.Block.HOMEPAGE_PAGEMENU_BLOCK, 0, Rock.Security.Authorization.EDIT, true, "628C51A8-4613-43ED-A18D-4A6FB999273E" /* Rock Administrators */, Rock.Model.SpecialRole.None, "E346C4F9-F836-4674-91F6-B8A25A390BB4" );
            // allow UP admins
            RockMigrationHelper.AddSecurityAuthForBlock( com.centralaz.Prayerbook.SystemGuid.Block.HOMEPAGE_PAGEMENU_BLOCK, 1, Rock.Security.Authorization.VIEW, true, com.centralaz.Prayerbook.SystemGuid.Group.ADMINISTRATORS_GROUP, Rock.Model.SpecialRole.None, com.centralaz.Prayerbook.SystemGuid.Auth.HOMEPAGE_UPTEAMADMINS_ALLOW_VIEW_PAGEMENUBLOCK_AUTH );
            RockMigrationHelper.AddSecurityAuthForBlock( com.centralaz.Prayerbook.SystemGuid.Block.HOMEPAGE_PAGEMENU_BLOCK, 1, Rock.Security.Authorization.EDIT, true, com.centralaz.Prayerbook.SystemGuid.Group.ADMINISTRATORS_GROUP, Rock.Model.SpecialRole.None, com.centralaz.Prayerbook.SystemGuid.Auth.HOMEPAGE_UPTEAMADMINS_ALLOW_EDIT_PAGEMENUBLOCK_AUTH );
            // deny contributors
            RockMigrationHelper.AddSecurityAuthForBlock( com.centralaz.Prayerbook.SystemGuid.Block.HOMEPAGE_PAGEMENU_BLOCK, 2, Rock.Security.Authorization.VIEW, false, com.centralaz.Prayerbook.SystemGuid.Group.CONTRIBUTORS_GROUP, Rock.Model.SpecialRole.None, com.centralaz.Prayerbook.SystemGuid.Auth.HOMEPAGE_UPTEAMCONTRIBUTORS_DENY_VIEW_PAGEMENUBLOCK_AUTH );
            RockMigrationHelper.AddSecurityAuthForBlock( com.centralaz.Prayerbook.SystemGuid.Block.HOMEPAGE_PAGEMENU_BLOCK, 2, Rock.Security.Authorization.EDIT, false, com.centralaz.Prayerbook.SystemGuid.Group.CONTRIBUTORS_GROUP, Rock.Model.SpecialRole.None, com.centralaz.Prayerbook.SystemGuid.Auth.HOMEPAGE_UPTEAMCONTRIBUTORS_DENY_EDIT_PAGEMENUBLOCK_AUTH );
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}
