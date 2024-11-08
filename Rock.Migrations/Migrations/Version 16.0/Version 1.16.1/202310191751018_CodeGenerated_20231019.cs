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
    public partial class CodeGenerated_20231019 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {

            // Attribute for BlockType              
            //   BlockType: Group Schedule Communication              
            //   Category: Group Scheduling              
            //   Attribute: Communications Page              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9F813A6C-25A7-491F-9C01-6D6EE6A7CA04", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Communications Page", "CommunicationsPage", "Communications Page", @"The page that the person will be pushed to when selecting a conversation.", 1, @"", "C707CEA5-3AE9-4215-A569-652DEBFF5D1E" );

            // Attribute for BlockType              
            //   BlockType: Group Schedule Communication              
            //   Category: Group Scheduling              
            //   Attribute: Require Group in URL              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9F813A6C-25A7-491F-9C01-6D6EE6A7CA04", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Group in URL", "RequireGroupInURL", "Require Group in URL", @"This will require that the group Id be passed in through the query string via the group GroupId parameter", 2, @"False", "5B1000AF-682B-4573-A903-FEECECE2C0C2" );

            // Attribute for BlockType              
            //   BlockType: Group Schedule Communication              
            //   Category: Group Scheduling              
            //   Attribute: Allow Including Child Groups              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9F813A6C-25A7-491F-9C01-6D6EE6A7CA04", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Including Child Groups", "AllowIncludingChildGroups", "Allow Including Child Groups", @"This allows the showing/hiding of the Include Child Groups option.", 3, @"True", "23823FDF-99B7-466F-A21A-ABA2C81D3A41" );

            // Attribute for BlockType              
            //   BlockType: Group Tree View              
            //   Category: Groups              
            //   Attribute: Limit to Security Role Groups              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2D26A2C4-62DC-4680-8219-A52EB2BC0F65", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Limit to Security Role Groups", "LimitToSecurityRoleGroups", "Limit to Security Role Groups", @"", 5, @"False", "04B11694-A59B-4366-9360-1919AD8A6963" );

            // Attribute for BlockType              
            //   BlockType: Login              
            //   Category: Security              
            //   Attribute: Two-Factor Email or Mobile Phone Required              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5437C991-536D-4D9C-BE58-CBDB59D1BBB3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Two-Factor Email or Mobile Phone Required", "TwoFactorEmailPhoneRequired", "Two-Factor Email or Mobile Phone Required", @"Lava template to show when email or mobile phone is required for 2FA. <span class='tip tip-lava'></span>", 19, @"<div class=""alert alert-warning"">Your current security access level requires you to complete a Two-Factor Authentication login in order to proceed. This additional layer of security is necessary to ensure the protection of your account and the sensitive data it contains.<br><br>To continue, please provide your email or mobile phone below.</div>", "82DB5F2E-B31C-43AF-8DB8-080F106B63EB" );

            // Attribute for BlockType              
            //   BlockType: Login              
            //   Category: Security              
            //   Attribute: Two-Factor Email and Mobile Phone Not Available              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5437C991-536D-4D9C-BE58-CBDB59D1BBB3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Two-Factor Email and Mobile Phone Not Available", "TwoFactorEmailPhoneNotAvailable", "Two-Factor Email and Mobile Phone Not Available", @"Lava template to show when email or mobile phone is required for 2FA but missing on the account. <span class='tip tip-lava'></span>", 20, @"<div class=""alert alert-warning"">Your current security access level requires you to complete a Two-Factor Authentication login in order to proceed. This additional layer of security is necessary to ensure the protection of your account and the sensitive data it contains.<br><br>Your account does not currently have an email address or mobile phone. Please contact us to assist you in configuring this.</div>", "9D012E56-889A-42A8-A085-5305CA4C6F6E" );

            // Attribute for BlockType              
            //   BlockType: Login              
            //   Category: Security              
            //   Attribute: Two-Factor Login Required              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5437C991-536D-4D9C-BE58-CBDB59D1BBB3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Two-Factor Login Required", "TwoFactorLoginRequired", "Two-Factor Login Required", @"Lava template to show when database login is required for 2FA. <span class='tip tip-lava'></span>", 21, @"<div class=""alert alert-warning"">Your current security access level requires you to complete a Two-Factor Authentication login in order to proceed. This additional layer of security is necessary to ensure the protection of your account and the sensitive data it contains.<br><br>To continue, please provide your email or mobile phone below.</div>", "97B4ACD1-B929-4E5B-B05D-0DC06DB193D3" );

            // Attribute for BlockType              
            //   BlockType: Login              
            //   Category: Security              
            //   Attribute: Two-Factor Login Not Available              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5437C991-536D-4D9C-BE58-CBDB59D1BBB3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Two-Factor Login Not Available", "TwoFactorLoginNotAvailable", "Two-Factor Login Not Available", @"Lava template to show when database login is required for 2FA but not available. <span class='tip tip-lava'></span>", 22, @"<div class=""alert alert-warning"">Your current security access level requires you to complete a Two-Factor Authentication login in order to proceed. This additional layer of security is necessary to ensure the protection of your account and the sensitive data it contains.<br><br>Your account does not currently have an email address or mobile phone. Please contact us to assist you in configuring this.</div>", "2D73A45E-2C43-4F85-896C-DC5BCF82E1FE" );

            // Attribute for BlockType              
            //   BlockType: Login              
            //   Category: Security              
            //   Attribute: Two-Factor Not Supported by Authorization Component              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5437C991-536D-4D9C-BE58-CBDB59D1BBB3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Two-Factor Not Supported by Authorization Component", "TwoFactorNotSupportedByAuthenticationMethod", "Two-Factor Not Supported by Authorization Component", @"Lava template to show when 2FA is not supported by the authentication method. <span class='tip tip-lava'></span>", 23, @"<div class=""alert alert-warning"">Your login attempt requires Two-Factor Authentication for enhanced security. However, the authentication method you used does not support 2FA. Please use a supported method or contact us for assistance.</div>", "4EF9D541-44E4-4FF5-B6E4-BC088E8CABED" );

            // Attribute for BlockType              
            //   BlockType: Log In              
            //   Category: Mobile > Cms              
            //   Attribute: Header Content              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6006FE32-DC01-4B1C-A9B8-EE172451F4C5", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Header Content", "HeaderContent", "Header Content", @"The content to display for the header. This only works if the block isn't in a ScrollView.", 6, @"", "718192A2-DCCB-4B7A-8A47-3749FD63B5AD" );

            // Attribute for BlockType              
            //   BlockType: Log In              
            //   Category: Mobile > Cms              
            //   Attribute: Footer Content              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6006FE32-DC01-4B1C-A9B8-EE172451F4C5", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Footer Content", "FooterContent", "Footer Content", @"The content to display for the footer. This only works if the block isn't in a ScrollView. Disappears when the keyboard is shown.", 7, @"", "E64CD914-44CF-4B54-9149-9AAC3AA92407" );

            // Attribute for BlockType              
            //   BlockType: Connection Request Detail              
            //   Category: Mobile > Connection              
            //   Attribute: Reminder Page              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EF537CC9-5E53-4832-A473-0D5EA439C296", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Reminder Page", "ReminderPage", "Reminder Page", @"Page to link to when the reminder button is tapped.", 5, @"", "116B5EB7-71DD-4CAA-BE35-0ED4CBA1150E" );

            // Attribute for BlockType              
            //   BlockType: Person Profile              
            //   Category: Mobile > Crm              
            //   Attribute: Reminder Page              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F97E2359-BB2D-4534-821D-870F853CA5CC", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Reminder Page", "ReminderPage", "Reminder Page", @"Page to link to when the reminder button is tapped.", 6, @"", "89BB1A61-7BFA-468A-A266-E9D412208229" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {

            // Attribute for BlockType              
            //   BlockType: Person Profile              
            //   Category: Mobile > Crm              
            //   Attribute: Reminder Page              
            RockMigrationHelper.DeleteAttribute( "89BB1A61-7BFA-468A-A266-E9D412208229" );

            // Attribute for BlockType              
            //   BlockType: Connection Request Detail              
            //   Category: Mobile > Connection              
            //   Attribute: Reminder Page              
            RockMigrationHelper.DeleteAttribute( "116B5EB7-71DD-4CAA-BE35-0ED4CBA1150E" );

            // Attribute for BlockType              
            //   BlockType: Log In              
            //   Category: Mobile > Cms              
            //   Attribute: Footer Content              
            RockMigrationHelper.DeleteAttribute( "E64CD914-44CF-4B54-9149-9AAC3AA92407" );

            // Attribute for BlockType              
            //   BlockType: Log In              
            //   Category: Mobile > Cms              
            //   Attribute: Header Content              
            RockMigrationHelper.DeleteAttribute( "718192A2-DCCB-4B7A-8A47-3749FD63B5AD" );

            // Attribute for BlockType              
            //   BlockType: Login              
            //   Category: Security              
            //   Attribute: Two-Factor Not Supported by Authorization Component              
            RockMigrationHelper.DeleteAttribute( "4EF9D541-44E4-4FF5-B6E4-BC088E8CABED" );

            // Attribute for BlockType              
            //   BlockType: Login              
            //   Category: Security              
            //   Attribute: Two-Factor Login Not Available              
            RockMigrationHelper.DeleteAttribute( "2D73A45E-2C43-4F85-896C-DC5BCF82E1FE" );

            // Attribute for BlockType              
            //   BlockType: Login              
            //   Category: Security              
            //   Attribute: Two-Factor Login Required              
            RockMigrationHelper.DeleteAttribute( "97B4ACD1-B929-4E5B-B05D-0DC06DB193D3" );

            // Attribute for BlockType              
            //   BlockType: Login              
            //   Category: Security              
            //   Attribute: Two-Factor Email and Mobile Phone Not Available              
            RockMigrationHelper.DeleteAttribute( "9D012E56-889A-42A8-A085-5305CA4C6F6E" );

            // Attribute for BlockType              
            //   BlockType: Login              
            //   Category: Security              
            //   Attribute: Two-Factor Email or Mobile Phone Required              
            RockMigrationHelper.DeleteAttribute( "82DB5F2E-B31C-43AF-8DB8-080F106B63EB" );

            // Attribute for BlockType              
            //   BlockType: Group Tree View              
            //   Category: Groups              
            //   Attribute: Limit to Security Role Groups              
            RockMigrationHelper.DeleteAttribute( "04B11694-A59B-4366-9360-1919AD8A6963" );

            // Attribute for BlockType              
            //   BlockType: Group Schedule Communication              
            //   Category: Group Scheduling              
            //   Attribute: Allow Including Child Groups              
            RockMigrationHelper.DeleteAttribute( "23823FDF-99B7-466F-A21A-ABA2C81D3A41" );

            // Attribute for BlockType              
            //   BlockType: Group Schedule Communication              
            //   Category: Group Scheduling              
            //   Attribute: Require Group in URL              
            RockMigrationHelper.DeleteAttribute( "5B1000AF-682B-4573-A903-FEECECE2C0C2" );

            // Attribute for BlockType              
            //   BlockType: Group Schedule Communication              
            //   Category: Group Scheduling              
            //   Attribute: Communications Page              
            RockMigrationHelper.DeleteAttribute( "C707CEA5-3AE9-4215-A569-652DEBFF5D1E" );
        }
    }
}
