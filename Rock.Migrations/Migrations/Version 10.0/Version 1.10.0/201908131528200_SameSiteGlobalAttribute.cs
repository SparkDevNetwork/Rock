// <copyright>
// Copyright by the Spark Development Network
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
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class SameSiteGlobalAttribute : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddGlobalAttribute(
                "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0",
                string.Empty,
                string.Empty,
                "Same Site Cookie Setting",
                "SameSite Setting",
                @"This setting is to protect Rock from Cross Site Request Forgery attacks by restricting the websites that have access to the Rock cookie. If this setting is changed then it will be applied to each user as they log in.  The ""Strict"" setting will keep the browser from sending the Rock cookie to any other domains, while the ""Lax"" setting provides a balance between security and usability. In most cases the ""Lax"" setting is appropriate.",
                1033,
                "Lax",
                Rock.SystemGuid.Attribute.SAME_SITE_COOKIE_SETTING,
                "core_SameSiteCookieSetting",
                false
            );

            RockMigrationHelper.AddAttributeQualifier( Rock.SystemGuid.Attribute.SAME_SITE_COOKIE_SETTING, "values", "None,Lax,Strict", "BDDC44B7-DBBA-48BC-8EE5-D9D17D28FAB8" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( Rock.SystemGuid.Attribute.SAME_SITE_COOKIE_SETTING );
        }
    }
}
