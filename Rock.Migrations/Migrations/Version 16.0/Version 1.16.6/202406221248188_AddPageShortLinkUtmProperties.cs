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
    public partial class AddPageShortLinkUtmProperties : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateDataModels_Up();
            AddDefinedTypes_Up();
        }

        private void UpdateDataModels_Up()
        {
            AddColumn( "dbo.Interaction", "SourceValueId", c => c.Int() );
            AddColumn( "dbo.Interaction", "MediumValueId", c => c.Int() );
            AddColumn( "dbo.Interaction", "CampaignValueId", c => c.Int() );

            AddColumn( "dbo.PageShortLink", "AdditionalSettingsJson", c => c.String() );
        }

        private const string _DefinedTypeUtmSource = "3CFE43A9-5D0C-4BE4-B1EC-AFA06BBB7C32";
        private const string _DefinedTypeUtmMedium = "31693856-8553-4321-A302-B84CF1D22BAB";
        private const string _DefinedTypeUtmCampaign = "A2F452BB-39E8-40F8-9DAD-74DBD920FD2F";

        private void AddDefinedTypes_Up()
        {
            // Add Category
            RockMigrationHelper.UpdateCategory( SystemGuid.EntityType.DEFINED_TYPE, "Marketing",
                iconCssClass: string.Empty,
                description: string.Empty,
                guid: SystemGuid.Category.MARKETING );

            // Add Defined Type: UTM Source
            RockMigrationHelper.AddDefinedType( "Marketing", "UTM Source", "The UTM source parameter identifies the origin of your traffic, like a search engine, newsletter, or specific website, helping to pinpoint which platforms are directing visitors to your site.", _DefinedTypeUtmSource );
            RockMigrationHelper.AddDefinedValue( _DefinedTypeUtmSource, "google", "", "DAFAA4D6-E754-48AF-B374-5F1683CBE089" );
            RockMigrationHelper.AddDefinedValue( _DefinedTypeUtmSource, "youtube", "", "9FF58EE1-DADD-4524-9123-FE2D767845D1" );
            RockMigrationHelper.AddDefinedValue( _DefinedTypeUtmSource, "facebook", "", "ED3D6562-3F65-40DD-8C4F-A8726198B001" );
            RockMigrationHelper.AddDefinedValue( _DefinedTypeUtmSource, "instagram", "", "5DAB0DF9-778D-408A-87C4-1E4DDB8BA00B" );
            RockMigrationHelper.AddDefinedValue( _DefinedTypeUtmSource, "bing", "", "9B59AD12-7D5A-4D9A-8F61-B1169564A526" );
            RockMigrationHelper.AddDefinedValue( _DefinedTypeUtmSource, "website", "", "D08A34E7-0685-4BA5-8B55-EA3480A75220" );

            // Add Defined Type: UTM Medium
            RockMigrationHelper.AddDefinedType( "Marketing", "UTM Medium", "The UTM medium parameter is used to identify the marketing or advertising medium that directed a user to your site. Examples include \"email\", \"social\", \"cpc\" (cost per click), or \"organic\" for non-paid search engine traffic.", _DefinedTypeUtmMedium );

            RockMigrationHelper.AddDefinedValue( _DefinedTypeUtmMedium, "email", "", "D369D728-DEC2-4C53-9528-B4D2C78E2E40" );
            RockMigrationHelper.AddDefinedValue( _DefinedTypeUtmMedium, "social", "", "616764EC-28A5-45EF-ACE5-5BA579284A21" );
            RockMigrationHelper.AddDefinedValue( _DefinedTypeUtmMedium, "cpc", "", "3BDFC79A-6F08-40CD-9EB5-B652A935721D" );
            RockMigrationHelper.AddDefinedValue( _DefinedTypeUtmMedium, "organic", "", "8F5401AB-12D6-4837-851D-B785461BC74E" );
            RockMigrationHelper.AddDefinedValue( _DefinedTypeUtmMedium, "post", "", "084113DB-D162-4F82-A001-2A7A6A985E73" );
            RockMigrationHelper.AddDefinedValue( _DefinedTypeUtmMedium, "ppc", "", "6583CDD5-158E-447F-A662-A3FDB5990B89" );

            // Add Defined Type: UTM Campaign
            RockMigrationHelper.AddDefinedType( "Marketing", "UTM Campaign", "The UTM campaign parameter tags your traffic with specific campaign names, enabling you to measure the performance of individual marketing campaigns and understand their impact on your traffic.", _DefinedTypeUtmCampaign );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn( "dbo.PageShortLink", "AdditionalSettingsJson" );

            DropColumn( "dbo.Interaction", "CampaignValueId" );
            DropColumn( "dbo.Interaction", "MediumValueId" );
            DropColumn( "dbo.Interaction", "SourceValueId" );

            RockMigrationHelper.DeleteDefinedType( _DefinedTypeUtmSource );
            RockMigrationHelper.DeleteDefinedType( _DefinedTypeUtmMedium );
            RockMigrationHelper.DeleteDefinedType( _DefinedTypeUtmCampaign );
        }
    }
}
