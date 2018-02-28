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
    public partial class ReCaptchaGlobalAttributes : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddGlobalAttribute( Rock.SystemGuid.FieldType.TEXT,
                string.Empty, string.Empty,
                "Google ReCaptcha Site Key", "The Secret Key provided by the Google ReCaptcha admin console.",
                0, string.Empty, SystemGuid.Attribute.GLOBAL_GOOGLE_RECAPTCHA_SITE_KEY );

            RockMigrationHelper.AddGlobalAttribute( Rock.SystemGuid.FieldType.TEXT,
                string.Empty, string.Empty,
                "Google ReCaptcha Secret Key", "The Secret Key provided by the Google ReCaptcha admin console.",
                0, string.Empty, SystemGuid.Attribute.GLOBAL_GOOGLE_RECAPTCHA_SECRET_KEY );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
