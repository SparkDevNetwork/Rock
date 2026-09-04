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
using Rock.Plugin;
using Rock.Web;

namespace Rock.Migrations
{
    [MigrationNumber( 8, "1.8.0" )]
    public class Checkr_SetDefaultBackgroundCheckProvider : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            /*
                7/13/26 - NA

                Originally this migration's else branch fell back to setting Protect My
                Ministry (v1) as the default background check provider when the
                [BackgroundCheck] table already had rows (i.e., the install had been
                using PMM). PMM was removed in Rock v20, so we no longer want to
                (re)install it as the default under any condition. If [BackgroundCheck]
                already has rows, leave the currently-configured default provider alone;
                only fresh installs (count == 0) get Checkr auto-selected.

                Reason: Preserves the "auto-pick Checkr on brand-new installs" behavior
                without ever writing the removed PMM type name back into the system
                setting.
            */
            int count = ( int ) SqlScalar( "SELECT COUNT(Id) FROM [dbo].[BackgroundCheck]" );
            if ( count == 0 )
            {
                string checkrTypeName = ( typeof( Rock.Checkr.Checkr ) ).FullName;
                SystemSettings.SetValue( Rock.SystemKey.SystemSetting.DEFAULT_BACKGROUND_CHECK_PROVIDER, checkrTypeName );
            }
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}