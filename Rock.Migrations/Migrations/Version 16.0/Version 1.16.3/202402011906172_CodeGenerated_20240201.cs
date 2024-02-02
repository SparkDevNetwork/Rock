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
    public partial class CodeGenerated_20240201 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attribute for BlockType              
            //   BlockType: Person Search              
            //   Category: CRM              
            //   Attribute: Highlight Indicators              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "764D3E67-2D01-437A-9F45-9F8C97878434", "F739BF5D-3FDC-45EC-A03C-1AE7C47E3883", "Highlight Indicators", "DataViewIcons", "Highlight Indicators", @"Select one or more Datae Views for Person search result icons. Note: More selections increase processing time.", 8, @"", "8FBF6FCC-B41F-4B1D-9FDD-6A5C60071BA5" );

            // Attribute for BlockType              
            //   BlockType: Log In              
            //   Category: Mobile > Cms              
            //   Attribute: Enable Microsoft Entra Login              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6006FE32-DC01-4B1C-A9B8-EE172451F4C5", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Microsoft Entra Login", "EnableEntraLogin", "Enable Microsoft Entra Login", @"Whether or not to enable Entra as an authentication provider. This must be configured in the application settings beforehand.", 9, @"False", "1D907103-CA35-4E16-BC3A-633CB3EA4B1C" );

            // Attribute for BlockType              
            //   BlockType: Log In              
            //   Category: Mobile > Cms              
            //   Attribute: Entra Login Button Text              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6006FE32-DC01-4B1C-A9B8-EE172451F4C5", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Entra Login Button Text", "EntraLoginButtonText", "Entra Login Button Text", @"The text of the Entra login button.", 12, @"Login With Entra", "D89CA35C-B607-49DD-A89B-46663E2300F7" );

            // Attribute for BlockType              
            //   BlockType: Account Entry              
            //   Category: Security              
            //   Attribute: Disable Captcha Support              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E5C34503-DDAD-4881-8463-0E1E20B1675D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Disable Captcha Support", "DisableCaptchaSupport", "Disable Captcha Support", @"If set to 'Yes' the CAPTCHA verification step will not be performed.", 30, @"False", "69683D94-406E-4645-BB5E-FB239A293AA7" );

            // Attribute for BlockType              
            //   BlockType: Account Entry              
            //   Category: Security              
            //   Attribute: Disable Captcha Support              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E5C34503-DDAD-4881-8463-0E1E20B1675D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Disable Captcha Support", "DisableCaptchaSupport", "Disable Captcha Support", @"If set to 'Yes' the CAPTCHA verification step will not be performed.", 30, @"False", "69683D94-406E-4645-BB5E-FB239A293AA7" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType              
            //   BlockType: Log In              
            //   Category: Mobile > Cms              
            //   Attribute: Entra Login Button Text              
            RockMigrationHelper.DeleteAttribute( "D89CA35C-B607-49DD-A89B-46663E2300F7" );

            // Attribute for BlockType              
            //   BlockType: Log In              
            //   Category: Mobile > Cms              
            //   Attribute: Enable Microsoft Entra Login              
            RockMigrationHelper.DeleteAttribute( "1D907103-CA35-4E16-BC3A-633CB3EA4B1C" );

            // Attribute for BlockType              
            //   BlockType: Account Entry              
            //   Category: Security              
            //   Attribute: Disable Captcha Support              
            RockMigrationHelper.DeleteAttribute( "69683D94-406E-4645-BB5E-FB239A293AA7" );

            // Attribute for BlockType              
            //   BlockType: Person Search              
            //   Category: CRM              
            //   Attribute: Highlight Indicators              
            RockMigrationHelper.DeleteAttribute( "8FBF6FCC-B41F-4B1D-9FDD-6A5C60071BA5" );
        }
    }
}
