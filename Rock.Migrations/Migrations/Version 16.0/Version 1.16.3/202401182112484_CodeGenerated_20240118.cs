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
    public partial class CodeGenerated_20240118 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attribute for BlockType              
            //   BlockType: Change Password              
            //   Category: Security              
            //   Attribute: Disable Captcha Support              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3C12DE99-2D1B-40F2-A9B8-6FE7C2524B37", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Disable Captcha Support", "DisableCaptchaSupport", "Disable Captcha Support", @"If set to 'Yes' the CAPTCHA verification step will not be performed.", 3, @"False", "EAD19C9B-73FB-4B31-A039-55142F36E743" );

            // Attribute for BlockType              
            //   BlockType: Forgot Username              
            //   Category: Security              
            //   Attribute: Disable Captcha Support              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "02B3D7D1-23CE-4154-B602-F4A15B321757", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Disable Captcha Support", "DisableCaptchaSupport", "Disable Captcha Support", @"If set to 'Yes' the CAPTCHA verification step will not be performed.", 6, @"False", "E717640D-C618-41F3-A388-4D3E7EBF73BB" );

            // Attribute for BlockType              
            //   BlockType: Pledge List              
            //   Category: Finance              
            //   Attribute: Hide Amount              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7011E792-A75F-4F22-B17E-D3A58C0EDB6D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide Amount", "HideAmount", "Hide Amount", @"Allows the amount column to be hidden.", 6, @"False", "0903AD1F-AF53-4901-981D-9FB6ACF93036" );

            // Attribute for BlockType              
            //   BlockType: System Communication Preview              
            //   Category: Communication              
            //   Attribute: Lava Template Append              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "95366DA1-D878-4A9A-A26F-83160DBE784F", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template Append", "LavaTemplateAppend", "Lava Template Append", @"This Lava will be appended to the system communication template to help setup any data that the template needs. This data would typically be passed to the template by a job or other means.", 6, @"", "C84598CD-6CC9-4B25-A8D7-32B5728C0EB8" );

            // Attribute for BlockType              
            //   BlockType: Forgot Username              
            //   Category: Security              
            //   Attribute: Disable Captcha Support              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "16CD7562-BE31-4823-9C4D-F365AB0AA5C4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Disable Captcha Support", "DisableCaptchaSupport", "Disable Captcha Support", @"If set to 'Yes' the CAPTCHA verification step will not be performed.", 6, @"False", "4F4EE17A-C031-42E4-AABE-80BF048F9085" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType              
            //   BlockType: Forgot Username              
            //   Category: Security              
            //   Attribute: Disable Captcha Support              
            RockMigrationHelper.DeleteAttribute( "4F4EE17A-C031-42E4-AABE-80BF048F9085" );

            // Attribute for BlockType              
            //   BlockType: Forgot Username              
            //   Category: Security              
            //   Attribute: Disable Captcha Support              
            RockMigrationHelper.DeleteAttribute( "E717640D-C618-41F3-A388-4D3E7EBF73BB" );

            // Attribute for BlockType              
            //   BlockType: Change Password              
            //   Category: Security              
            //   Attribute: Disable Captcha Support              
            RockMigrationHelper.DeleteAttribute( "EAD19C9B-73FB-4B31-A039-55142F36E743" );

            // Attribute for BlockType              
            //   BlockType: Pledge List              
            //   Category: Finance              
            //   Attribute: Hide Amount              
            RockMigrationHelper.DeleteAttribute( "0903AD1F-AF53-4901-981D-9FB6ACF93036" );

            // Attribute for BlockType              
            //   BlockType: System Communication Preview              
            //   Category: Communication              
            //   Attribute: Lava Template Append              
            RockMigrationHelper.DeleteAttribute( "C84598CD-6CC9-4B25-A8D7-32B5728C0EB8" );
        }
    }
}
