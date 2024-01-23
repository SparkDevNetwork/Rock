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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 188, "1.16.1" )]
    public class MigrationRollupsForV17_0_0 : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            var cmsConfigurationPageGuid = "B4A24AB7-9369-4055-883F-4F4892C39AE3";
            var fullWidthLayoutGuid = "D65F783D-87A9-4CC9-8110-E83466A0EADB";
            var pagelistBlockGuid = "BEDFF750-3EB8-4EE7-A8B4-23863FB0315D";

            // Add the Website Configurations Section
            RockMigrationHelper.AddPage( true, cmsConfigurationPageGuid, fullWidthLayoutGuid, "Website Configuration", "Configuration features, designed to simplify the process of managing and fine-tuning your website.", "CCDFEA8F-CF33-49C7-86C0-C4B10DCF1E89" );

            // Move the Pages to the Website Configuration Section
            RockMigrationHelper.MovePage( "7596D389-4EAB-4535-8BEE-229737F46F44", "CCDFEA8F-CF33-49C7-86C0-C4B10DCF1E89" ); // Sites Page
            RockMigrationHelper.MovePage( "EC7A06CD-AAB5-4455-962E-B4043EA2440E", "CCDFEA8F-CF33-49C7-86C0-C4B10DCF1E89" ); // Pages Page
            RockMigrationHelper.MovePage( "6F074DAA-BDCC-44C5-BB89-B899C1AAC6C1", "CCDFEA8F-CF33-49C7-86C0-C4B10DCF1E89" ); // File Manager Page
            RockMigrationHelper.MovePage( "4A833BE3-7D5E-4C38-AF60-5706260015EA", "CCDFEA8F-CF33-49C7-86C0-C4B10DCF1E89" ); // Routes Page
            RockMigrationHelper.MovePage( "5FBE9019-862A-41C6-ACDC-287D7934757D", "CCDFEA8F-CF33-49C7-86C0-C4B10DCF1E89" ); // Block Types Page
            RockMigrationHelper.MovePage( "BC2AFAEF-712C-4173-895E-81347F6B0B1C", "CCDFEA8F-CF33-49C7-86C0-C4B10DCF1E89" ); // Themes Page
            RockMigrationHelper.MovePage( "39F928A5-1374-4380-B807-EADF145F18A1", "CCDFEA8F-CF33-49C7-86C0-C4B10DCF1E89" ); // HTTP Modules Page

            // Add the Content Channels Section
            RockMigrationHelper.AddPage( true, cmsConfigurationPageGuid, fullWidthLayoutGuid, "Content Channels", "In this section, you'll find tools to effectively configure and manage all aspects of your content channels.", "889D7F7F-EB0F-40CD-9E80-E58A00EE69F7" );

            // Move the Pages to the Content Channels Section
            RockMigrationHelper.MovePage( "8ADCE4B2-8E95-4FA3-89C4-06A883E8145E", "889D7F7F-EB0F-40CD-9E80-E58A00EE69F7" ); // Content Channels Page
            RockMigrationHelper.MovePage( "37E3D602-5D7D-4818-BCAA-C67EBB301E55", "889D7F7F-EB0F-40CD-9E80-E58A00EE69F7" ); // Content Channel Types Page
            RockMigrationHelper.MovePage( "40875E7E-B912-43FF-892B-6161C21F130B", "889D7F7F-EB0F-40CD-9E80-E58A00EE69F7" ); // Content Collections Page
            RockMigrationHelper.MovePage( "F1ED10C2-A17D-4310-9F86-76E11A4A7ED2", "889D7F7F-EB0F-40CD-9E80-E58A00EE69F7" ); // Content Component Templates Page
            RockMigrationHelper.MovePage( "BBDE39C3-01C9-4C9E-9506-C2205508BC77", "889D7F7F-EB0F-40CD-9E80-E58A00EE69F7" ); // Content Channel Item Attribute Categories Page
            RockMigrationHelper.MovePage( "0F1B45B8-032D-4306-834D-670FA3933589", "889D7F7F-EB0F-40CD-9E80-E58A00EE69F7" ); // Content Channel Categories Page

            // Add the Personalization Section
            RockMigrationHelper.AddPage( true, cmsConfigurationPageGuid, fullWidthLayoutGuid, "Personalization", "Personalization features designed to tailor content and interactions to each individual's unique preferences and needs.", "B892DF6D-4789-4AC3-9E6C-2BFE0D9E30E4" );

            // Move the Pages to the Personalization Section
            RockMigrationHelper.MovePage( "905F6132-AE1C-4C85-9752-18D22E604C3A", "B892DF6D-4789-4AC3-9E6C-2BFE0D9E30E4" ); // Personalization Segments Page
            RockMigrationHelper.MovePage( "511FC29A-EAF2-4AC0-B9B3-8613739A9ACF", "B892DF6D-4789-4AC3-9E6C-2BFE0D9E30E4" ); // Request Filters Page

            // Add the Content Platform Section
            RockMigrationHelper.AddPage( true, cmsConfigurationPageGuid, fullWidthLayoutGuid, "Content Platform", "Tools designed for creating, managing, and seamlessly distributing digital content.", "04FE297E-D45E-44EC-B521-181423F05A1C" );

            // Move the Pages to the Content Platform Section
            RockMigrationHelper.MovePage( "6CFF2C81-6303-4477-A7EC-156DDBF8BE64", "04FE297E-D45E-44EC-B521-181423F05A1C" ); // Lava Shortcodes Page
            RockMigrationHelper.MovePage( "07CB7BB5-1465-4E75-8DD4-28FA6EA48222", "04FE297E-D45E-44EC-B521-181423F05A1C" ); // Media Accounts Page
            RockMigrationHelper.MovePage( "37C20B91-737B-42D1-907D-9868104DBA7B", "04FE297E-D45E-44EC-B521-181423F05A1C" ); // Persisted Datasets Page
            RockMigrationHelper.MovePage( "8C0114FF-31CF-443E-9278-3F9E6087140C", "04FE297E-D45E-44EC-B521-181423F05A1C" ); // Short Links Page
            RockMigrationHelper.MovePage( "C206A96E-6926-4EB9-A30F-E5FCE559D180", "04FE297E-D45E-44EC-B521-181423F05A1C" ); // Shared Links Page
            RockMigrationHelper.MovePage( "4B8691C7-537F-4B6E-9ED1-E3BA3FA0051E", "04FE297E-D45E-44EC-B521-181423F05A1C" ); // Cache Manager Page
            RockMigrationHelper.MovePage( "D2B919E2-3725-438F-8A86-AC87F81A72EB", "04FE297E-D45E-44EC-B521-181423F05A1C" ); // Asset Manager Page
            RockMigrationHelper.MovePage( "706C0584-285F-4014-BA61-EC42C8F6F76B", "04FE297E-D45E-44EC-B521-181423F05A1C" ); // Control Gallery Page
            RockMigrationHelper.MovePage( "BB2AF2B3-6D06-48C6-9895-EDF2BA254533", "04FE297E-D45E-44EC-B521-181423F05A1C" ); // Font Awesome Settings Page

            // Add the Digital Media Applications Section
            RockMigrationHelper.AddPage( true, cmsConfigurationPageGuid, fullWidthLayoutGuid, "Digital Media Applications", "Suite of tools and platforms designed to enhance digital content creation, distribution, and engagement.", "82726ACD-3480-4514-A920-FE920A71C046" );

            // Move the Pages to the Digital Media Applications Section
            RockMigrationHelper.MovePage( "784259EC-46B7-4DE3-AC37-E8BFDB0B90A6", "82726ACD-3480-4514-A920-FE920A71C046" ); // Mobile Applications Page
            RockMigrationHelper.MovePage( "C8B81EBE-E98F-43EF-9E39-0491685145E2", "82726ACD-3480-4514-A920-FE920A71C046" ); // Apple TV Apps Page

            // Update the CMS Page List Attributes
            RockMigrationHelper.AddBlockAttributeValue( pagelistBlockGuid, "1322186A-862A-4CF1-B349-28ECB67229BA", "{% include '~~/Assets/Lava/PageListAsBlocksSections.lava' %}" );
            RockMigrationHelper.AddBlockAttributeValue( pagelistBlockGuid, "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"2" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Move all the pages back to the CMS Configuration Page
            var cmsConfigurationPageGuid = "B4A24AB7-9369-4055-883F-4F4892C39AE3";
            var pagelistBlockGuid = "BEDFF750-3EB8-4EE7-A8B4-23863FB0315D";

            RockMigrationHelper.MovePage( "7596D389-4EAB-4535-8BEE-229737F46F44", cmsConfigurationPageGuid ); // Sites Page
            RockMigrationHelper.MovePage( "EC7A06CD-AAB5-4455-962E-B4043EA2440E", cmsConfigurationPageGuid ); // Pages Page
            RockMigrationHelper.MovePage( "6F074DAA-BDCC-44C5-BB89-B899C1AAC6C1", cmsConfigurationPageGuid ); // File Manager Page
            RockMigrationHelper.MovePage( "4A833BE3-7D5E-4C38-AF60-5706260015EA", cmsConfigurationPageGuid ); // Routes Page
            RockMigrationHelper.MovePage( "5FBE9019-862A-41C6-ACDC-287D7934757D", cmsConfigurationPageGuid ); // Block Types Page
            RockMigrationHelper.MovePage( "BC2AFAEF-712C-4173-895E-81347F6B0B1C", cmsConfigurationPageGuid ); // Themes Page
            RockMigrationHelper.MovePage( "39F928A5-1374-4380-B807-EADF145F18A1", cmsConfigurationPageGuid ); // HTTP Modules Page
            RockMigrationHelper.MovePage( "8ADCE4B2-8E95-4FA3-89C4-06A883E8145E", cmsConfigurationPageGuid ); // Content Channels Page
            RockMigrationHelper.MovePage( "37E3D602-5D7D-4818-BCAA-C67EBB301E55", cmsConfigurationPageGuid ); // Content Channel Types Page
            RockMigrationHelper.MovePage( "40875E7E-B912-43FF-892B-6161C21F130B", cmsConfigurationPageGuid ); // Content Collections Page
            RockMigrationHelper.MovePage( "F1ED10C2-A17D-4310-9F86-76E11A4A7ED2", cmsConfigurationPageGuid ); // Content Component Templates Page
            RockMigrationHelper.MovePage( "BBDE39C3-01C9-4C9E-9506-C2205508BC77", cmsConfigurationPageGuid ); // Content Channel Item Attribute Categories Page
            RockMigrationHelper.MovePage( "0F1B45B8-032D-4306-834D-670FA3933589", cmsConfigurationPageGuid ); // Content Channel Categories Page
            RockMigrationHelper.MovePage( "905F6132-AE1C-4C85-9752-18D22E604C3A", cmsConfigurationPageGuid ); // Personalization Segments Page
            RockMigrationHelper.MovePage( "511FC29A-EAF2-4AC0-B9B3-8613739A9ACF", cmsConfigurationPageGuid ); // Request Filters Page
            RockMigrationHelper.MovePage( "6CFF2C81-6303-4477-A7EC-156DDBF8BE64", cmsConfigurationPageGuid ); // Lava Shortcodes Page
            RockMigrationHelper.MovePage( "07CB7BB5-1465-4E75-8DD4-28FA6EA48222", cmsConfigurationPageGuid ); // Media Accounts Page
            RockMigrationHelper.MovePage( "37C20B91-737B-42D1-907D-9868104DBA7B", cmsConfigurationPageGuid ); // Persisted Datasets Page
            RockMigrationHelper.MovePage( "8C0114FF-31CF-443E-9278-3F9E6087140C", cmsConfigurationPageGuid ); // Short Links Page
            RockMigrationHelper.MovePage( "C206A96E-6926-4EB9-A30F-E5FCE559D180", cmsConfigurationPageGuid ); // Shared Links Page
            RockMigrationHelper.MovePage( "4B8691C7-537F-4B6E-9ED1-E3BA3FA0051E", cmsConfigurationPageGuid ); // Cache Manager Page
            RockMigrationHelper.MovePage( "D2B919E2-3725-438F-8A86-AC87F81A72EB", cmsConfigurationPageGuid ); // Asset Manager Page
            RockMigrationHelper.MovePage( "706C0584-285F-4014-BA61-EC42C8F6F76B", cmsConfigurationPageGuid ); // Control Gallery Page
            RockMigrationHelper.MovePage( "BB2AF2B3-6D06-48C6-9895-EDF2BA254533", cmsConfigurationPageGuid ); // Font Awesome Settings Page
            RockMigrationHelper.MovePage( "784259EC-46B7-4DE3-AC37-E8BFDB0B90A6", cmsConfigurationPageGuid ); // Mobile Applications Page
            RockMigrationHelper.MovePage( "C8B81EBE-E98F-43EF-9E39-0491685145E2", cmsConfigurationPageGuid ); // Apple TV Apps Page


            // Remove the Sections
            RockMigrationHelper.DeletePage( "CCDFEA8F-CF33-49C7-86C0-C4B10DCF1E89" ); // Website Configuration Section
            RockMigrationHelper.DeletePage( "889D7F7F-EB0F-40CD-9E80-E58A00EE69F7" ); // Content Channels Section
            RockMigrationHelper.DeletePage( "B892DF6D-4789-4AC3-9E6C-2BFE0D9E30E4" ); // Personalization Section
            RockMigrationHelper.DeletePage( "04FE297E-D45E-44EC-B521-181423F05A1C" ); // Content Platform Section
            RockMigrationHelper.DeletePage( "82726ACD-3480-4514-A920-FE920A71C046" ); // Digital Media Applications Section

            // Update the CMS Page List Attributes
            RockMigrationHelper.AddBlockAttributeValue( pagelistBlockGuid, "1322186A-862A-4CF1-B349-28ECB67229BA", "{% include '~~/Assets/Lava/PageListAsBlocksSections.lava' %}" );
            RockMigrationHelper.AddBlockAttributeValue( pagelistBlockGuid, "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"1" );
        }
    }
}
