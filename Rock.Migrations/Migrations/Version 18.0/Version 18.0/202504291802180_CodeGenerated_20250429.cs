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
    public partial class CodeGenerated_20250429 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attribute for BlockType
            //   BlockType: Log In
            //   Category: Mobile > Cms
            //   Attribute: Enable Enhanced Authentication Security
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6006FE32-DC01-4B1C-A9B8-EE172451F4C5", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Enhanced Authentication Security", "EnableEnhancedAuthenticationSecurity", "Enable Enhanced Authentication Security", @"Only applies to external authentication. Whether or not to enable enhanced authentication security. This will be automatically enabled in a future version of Rock, and the setting will be removed.", 13, @"True", "1BC33564-848F-43E3-885A-542DF57EC273" );

            // Attribute for BlockType
            //   BlockType: Form Builder Detail
            //   Category: WorkFlow > FormBuilder
            //   Attribute: Default Preview Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A61C5E3C-2267-4CF7-B305-D8AF0DB9660B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Default Preview Page", "DefaultPreviewPage", "Default Preview Page", @"The default page to use when previewing this form in the builder. The page must include a Workflow Entry block with 'Enable for Form Sharing' enabled. If not set, the first eligible page will be used.", 2, @"", "48283E94-1D87-468D-A123-34C865543B4C" );

            // Attribute for BlockType
            //   BlockType: Family Pre Registration
            //   Category: CRM
            //   Attribute: Record Source
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Source", "RecordSource", "Record Source", @"The record source to use for new individuals (default = 'Family Registration'). If a 'RecordSource' page parameter is found, it will be used instead.", 13, @"264C0969-55EA-4DF2-8FFD-2E3AAB311601", "08410DB0-FE41-495C-8928-97E4B6097238" );

            // Attribute for BlockType
            //   BlockType: Chat View
            //   Category: Communication
            //   Attribute: Minimum Age
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "723A3F70-87DC-4BA0-A6FB-0AC15B1865B0", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Minimum Age", "MinimumAge", "Minimum Age", @"The minimum age required to use chat. If the person does not have a birthdate, the verification template will show. Leave as empty to disable this check altogether.", 1, @"", "1CE3E2CA-7DF7-4759-8380-AC96ED79EF4B" );

            // Attribute for BlockType
            //   BlockType: Chat View
            //   Category: Communication
            //   Attribute: Age Verification Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "723A3F70-87DC-4BA0-A6FB-0AC15B1865B0", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Age Verification Template", "AgeVerificationTemplate", "Age Verification Template", @"The XAML template displayed when the person does not have a birthdate.", 2, @"<StackLayout StyleClass=""spacing-24, p-16"">
    <Rock:StyledBorder HorizontalOptions=""Center""
        StyleClass=""border-info-strong""
        BorderWidth=""4""
        CornerRadius=""70""
        HeightRequest=""140""
        WidthRequest=""140"">
        <Rock:Icon IconClass=""fa fa-shield-alt"" 
            FontSize=""84""
            HorizontalOptions=""Center""
            VerticalOptions=""Center""
            StyleClass=""text-info-strong"" />
    </Rock:StyledBorder>

    <StackLayout>
        <Label StyleClass=""title1, bold, text-interface-strongest"" 
            Text=""Let’s Verify Your Age"" />
    
        <Label StyleClass=""body, text-interface-stronger""
            Text=""The chat feature is only available to individuals above a certain age. Please confirm your birthdate to proceed."" />
    </StackLayout>
</StackLayout>", "B12A6843-856A-4C05-A24E-23D9F4883FEA" );

            // Attribute for BlockType
            //   BlockType: Chat View
            //   Category: Communication
            //   Attribute: Age Restriction Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "723A3F70-87DC-4BA0-A6FB-0AC15B1865B0", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Age Restriction Template", "AgeRestrictionTemplate", "Age Restriction Template", @"The XAML template displayed when the person is under the minimum age.", 3, @"<StackLayout StyleClass=""spacing-24, p-16"">
    <Rock:StyledBorder HorizontalOptions=""Center""
        StyleClass=""border-warning-strong""
        BorderWidth=""4""
        CornerRadius=""999""
        HeightRequest=""140""
        WidthRequest=""140"">
        <Rock:Icon IconClass=""fa fa-user-lock"" 
            FontSize=""72""
            StyleClass=""text-warning-strong""
            HorizontalOptions=""Center""
            VerticalOptions=""Center"" />
    </Rock:StyledBorder>

    <StackLayout>
        <Label StyleClass=""title1, bold, text-interface-strongest"" 
            Text=""Chat Unavailable"" />
    
        <Label StyleClass=""body, text-interface-stronger""
            Text=""We're sorry, but this feature is only available to individuals who are {{ MinimumAge }} years or older."" />
    </StackLayout>
</StackLayout>", "7D537F6C-39DD-4127-BD72-099B56CA51EB" );

            // Attribute for BlockType
            //   BlockType: Chat View
            //   Category: Communication
            //   Attribute: Age Verification Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "723A3F70-87DC-4BA0-A6FB-0AC15B1865B0", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Age Verification Template", "WebAgeVerificationTemplate", "Age Verification Template", @"The XAML template displayed when the person does not have a birthdate.", 4, @"<!-- Age Verification Prompt -->
<div class=""age-verification-wrapper"" style=""text-align: center; padding: 1.5rem;"">
  <!-- Font Awesome shield icon -->
  <div class=""icon"" style=""margin-bottom: 1.5rem;"">
    <i class=""fas fa-shield-alt"" style=""font-size: 4rem; color: var(--color-info-strong);""></i>
  </div>
  
  <h2 style=""color: var(--color-interface-strongest)"">
      Let's Verify Your Age
  </h2>
  <!-- Instructional text -->
  <p style=""font-size: 1rem; max-width: 400px; margin: 0 auto; color: var(--color-interface-strong);"">
    The chat feature is only available to individuals above a certain age. Please confirm your birthdate to proceed.
  </p>
</div>", "6580AB8D-9686-4E0B-93BA-A891A84A497E" );

            // Attribute for BlockType
            //   BlockType: Chat View
            //   Category: Communication
            //   Attribute: Age Restriction Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "723A3F70-87DC-4BA0-A6FB-0AC15B1865B0", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Age Restriction Template", "WebAgeRestrictionTemplate", "Age Restriction Template", @"The XAML template displayed when the person is under the minimum age.", 5, @"<!-- Age Verification Prompt -->
<div class=""age-verification-wrapper"" style=""text-align: center; padding: 1.5rem;"">
  <!-- Font Awesome shield icon -->
  <div class=""icon"" style=""margin-bottom: 1.5rem;"">
    <i class=""fas fa-user-lock"" style=""font-size: 4rem; color: var(--color-warning-strong);""></i>
  </div>
  
  <h2 style=""color: var(--color-interface-strongest)"">
      Chat Unavailable
  </h2>
  <!-- Instructional text -->
  <p style=""font-size: 1rem; max-width: 400px; margin: 0 auto; color: var(--color-interface-strong);"">
    We're sorry, but this feature is only available to individuals who are {{ MinimumAge }} years or older.
  </p>
</div>", "54FA94D0-004C-40F9-A478-3CC121DE791F" );

            // Add Block Attribute Value
            //   Block: Scheduled Job List
            //   BlockType: Scheduled Job List
            //   Category: Core
            //   Block Location: Page=Jobs Administration, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "D89D08B5-7F2E-41F0-A274-7FF36332A50E", "B2148F3B-7D9E-44CE-8005-AB100C6797F6", @"" );

            // Add Block Attribute Value
            //   Block: Scheduled Job List
            //   BlockType: Scheduled Job List
            //   Category: Core
            //   Block Location: Page=Jobs Administration, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "D89D08B5-7F2E-41F0-A274-7FF36332A50E", "28E8B399-F22E-4767-811C-3B4B3EB313F8", @"False" );

            // Add Block Attribute Value
            //   Block: Person Following List
            //   BlockType: Person Following List
            //   Category: Follow
            //   Block Location: Page=Following, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "81AC060F-C2A6-430E-A678-388D95308391", "0B3C01F6-390C-4EED-A595-83E73C942706", @"" );

            // Add Block Attribute Value
            //   Block: Person Following List
            //   BlockType: Person Following List
            //   Category: Follow
            //   Block Location: Page=Following, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "81AC060F-C2A6-430E-A678-388D95308391", "EA2D373D-7F29-431F-91B9-28A5D85E9533", @"False" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Log In
            //   Category: Mobile > Cms
            //   Attribute: Enable Enhanced Authentication Security
            RockMigrationHelper.DeleteAttribute( "1BC33564-848F-43E3-885A-542DF57EC273" );

            // Attribute for BlockType
            //   BlockType: Chat View
            //   Category: Communication
            //   Attribute: Age Restriction Template
            RockMigrationHelper.DeleteAttribute( "54FA94D0-004C-40F9-A478-3CC121DE791F" );

            // Attribute for BlockType
            //   BlockType: Chat View
            //   Category: Communication
            //   Attribute: Age Verification Template
            RockMigrationHelper.DeleteAttribute( "6580AB8D-9686-4E0B-93BA-A891A84A497E" );

            // Attribute for BlockType
            //   BlockType: Chat View
            //   Category: Communication
            //   Attribute: Age Restriction Template
            RockMigrationHelper.DeleteAttribute( "7D537F6C-39DD-4127-BD72-099B56CA51EB" );

            // Attribute for BlockType
            //   BlockType: Chat View
            //   Category: Communication
            //   Attribute: Age Verification Template
            RockMigrationHelper.DeleteAttribute( "B12A6843-856A-4C05-A24E-23D9F4883FEA" );

            // Attribute for BlockType
            //   BlockType: Chat View
            //   Category: Communication
            //   Attribute: Minimum Age
            RockMigrationHelper.DeleteAttribute( "1CE3E2CA-7DF7-4759-8380-AC96ED79EF4B" );

            // Attribute for BlockType
            //   BlockType: Form Builder Detail
            //   Category: WorkFlow > FormBuilder
            //   Attribute: Default Preview Page
            RockMigrationHelper.DeleteAttribute( "48283E94-1D87-468D-A123-34C865543B4C" );

            // Attribute for BlockType
            //   BlockType: Family Pre Registration
            //   Category: CRM
            //   Attribute: Record Source
            RockMigrationHelper.DeleteAttribute( "08410DB0-FE41-495C-8928-97E4B6097238" );
        }
    }
}
