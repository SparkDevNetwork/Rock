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
    public partial class Podcasting : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( MigrationSQL._201608172242214_Podcasting );

            RockMigrationHelper.AddPage( "85F25819-E948-4960-9DDF-00F54D32444E", "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Watch", "", "7F4916E3-1CCA-4721-91C1-D0A54ED93AD3", "" ); // Site:External Website
            RockMigrationHelper.AddPage( "7F4916E3-1CCA-4721-91C1-D0A54ED93AD3", "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Series Detail", "", "7669A501-4075-431A-9828-565C47FD21C8", "" ); // Site:External Website
            RockMigrationHelper.AddPage( "7669A501-4075-431A-9828-565C47FD21C8", "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Message Detail", "", "BB83C51D-65C7-4F6C-BA24-A496167C9B11", "" ); // Site:External Website

            // hide page title and breadcrumbs
            Sql( @"UPDATE [Page]
SET [PageDisplayTitle] = 0,
[PageDisplayBreadCrumb] = 0
WHERE
	[Guid] = '7F4916E3-1CCA-4721-91C1-D0A54ED93AD3'

UPDATE [Page]
SET [PageDisplayTitle] = 0
WHERE
	[Guid] = '7669A501-4075-431A-9828-565C47FD21C8'

UPDATE [Page]
SET [PageDisplayTitle] = 0
WHERE
	[Guid] = 'BB83C51D-65C7-4F6C-BA24-A496167C9B11' " );

            RockMigrationHelper.AddPageRoute( "7F4916E3-1CCA-4721-91C1-D0A54ED93AD3", "watch", "A564AF1A-7294-4AE6-B9B5-04EDD4F1AF49" );// for Page:Watch
            RockMigrationHelper.AddPageRoute( "7669A501-4075-431A-9828-565C47FD21C8", "series/{Item}", "D4518DDA-4152-454A-9B7F-CAB1067AF0C7" );// for Page:Series Detail
            RockMigrationHelper.AddPageRoute( "BB83C51D-65C7-4F6C-BA24-A496167C9B11", "message/{Item}", "38F927EA-1586-4B32-BF0D-93E4C068C956" );// for Page:Message Detail


            // Add Block to Page: Watch, Site: External Website
            RockMigrationHelper.AddBlock( "7F4916E3-1CCA-4721-91C1-D0A54ED93AD3", "", "143A2345-3E26-4ED0-A2FE-42AAF11B4C0F", "Content Channel View", "Main", "", "", 0, "F3DE9793-89CE-4890-BD01-CD4CA5B6F07C" );
            // Add Block to Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlock( "7669A501-4075-431A-9828-565C47FD21C8", "", "143A2345-3E26-4ED0-A2FE-42AAF11B4C0F", "Content Channel View", "Main", "", "", 0, "B35B99F5-7479-44C7-9C58-54B3A56BC233" );
            // Add Block to Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlock( "BB83C51D-65C7-4F6C-BA24-A496167C9B11", "", "143A2345-3E26-4ED0-A2FE-42AAF11B4C0F", "Content Channel View", "Main", "", "", 0, "353BF738-D47A-4799-886D-F3055C98DBDC" );

            // Attrib Value for Block:Content Channel View, Attribute:Meta Image Attribute Page: Watch, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "F3DE9793-89CE-4890-BD01-CD4CA5B6F07C", "A3510474-86E5-4AD2-BD4C-3C89E85795F5", @"" );
            // Attrib Value for Block:Content Channel View, Attribute:Query Parameter Filtering Page: Watch, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "F3DE9793-89CE-4890-BD01-CD4CA5B6F07C", "AA9CD867-FA21-43C2-822B-CAC06E1F18B8", @"False" );
            // Attrib Value for Block:Content Channel View, Attribute:Order Page: Watch, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "F3DE9793-89CE-4890-BD01-CD4CA5B6F07C", "07ED420E-749C-4938-ADFD-1DDEA6B63014", @"StartDateTime^1|" );
            // Attrib Value for Block:Content Channel View, Attribute:Merge Content Page: Watch, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "F3DE9793-89CE-4890-BD01-CD4CA5B6F07C", "20BE4E0A-E84C-4AA1-9368-9732A834E1DE", @"False" );
            // Attrib Value for Block:Content Channel View, Attribute:Set Page Title Page: Watch, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "F3DE9793-89CE-4890-BD01-CD4CA5B6F07C", "97161D67-EF24-4F21-9E6A-74B696DD33DE", @"False" );
            // Attrib Value for Block:Content Channel View, Attribute:Rss Autodiscover Page: Watch, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "F3DE9793-89CE-4890-BD01-CD4CA5B6F07C", "ABA69AFC-2C5E-4CF6-A156-EDA2752CCC86", @"False" );
            // Attrib Value for Block:Content Channel View, Attribute:Meta Description Attribute Page: Watch, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "F3DE9793-89CE-4890-BD01-CD4CA5B6F07C", "E01AE3A7-2607-4DA5-AC98-3A368C900B64", @"" );
            // Attrib Value for Block:Content Channel View, Attribute:Count Page: Watch, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "F3DE9793-89CE-4890-BD01-CD4CA5B6F07C", "25A501FC-E269-40B8-9904-E20FA7A1ADB6", @"6" );
            // Attrib Value for Block:Content Channel View, Attribute:Detail Page Page: Watch, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "F3DE9793-89CE-4890-BD01-CD4CA5B6F07C", "2D7E6F55-B25E-4EA2-8F7E-F7E138E39E21", @"7669a501-4075-431a-9828-565c47fd21c8" );
            // Attrib Value for Block:Content Channel View, Attribute:Enable Debug Page: Watch, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "F3DE9793-89CE-4890-BD01-CD4CA5B6F07C", "72F4232B-8D2A-4823-B9F1-ED68F182C1A4", @"False" );
            // Attrib Value for Block:Content Channel View, Attribute:Cache Duration Page: Watch, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "F3DE9793-89CE-4890-BD01-CD4CA5B6F07C", "773BEFDD-EEBA-486C-98E6-AFD0D4156E22", @"0" );
            // Attrib Value for Block:Content Channel View, Attribute:Channel Page: Watch, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "F3DE9793-89CE-4890-BD01-CD4CA5B6F07C", "34EACB0F-DBC4-4F18-85C9-0D3EDFDF46BE", @"E2C598F1-D299-1BAA-4873-8B679E3C1998" );
            // Attrib Value for Block:Content Channel View, Attribute:Status Page: Watch, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "F3DE9793-89CE-4890-BD01-CD4CA5B6F07C", "DA1DEE5D-BCEF-4AA4-A9D9-EFD4DD64462B", @"2" );
            // Attrib Value for Block:Content Channel View, Attribute:Template Page: Watch, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "F3DE9793-89CE-4890-BD01-CD4CA5B6F07C", "8026FEA1-35C1-41CF-9D09-E8B1DB6CBDA8", @"{% include '~~/Assets/Lava/PodcastSeriesList.lava' %}" );
            // Attrib Value for Block:Content Channel View, Attribute:Filter Id Page: Watch, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "F3DE9793-89CE-4890-BD01-CD4CA5B6F07C", "618EFBDA-941D-4F60-9AA8-54955B7A03A2", @"79" );
            // Attrib Value for Block:Content Channel View, Attribute:Enabled Lava Commands Page: Watch, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "F3DE9793-89CE-4890-BD01-CD4CA5B6F07C", "E6A73336-D7EF-4814-8146-C4C1F59D9C91", @"" );
            // Attrib Value for Block:Content Channel View, Attribute:Filter Id Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "B35B99F5-7479-44C7-9C58-54B3A56BC233", "618EFBDA-941D-4F60-9AA8-54955B7A03A2", @"69" );
            // Attrib Value for Block:Content Channel View, Attribute:Template Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "B35B99F5-7479-44C7-9C58-54B3A56BC233", "8026FEA1-35C1-41CF-9D09-E8B1DB6CBDA8", @"{% include '~~/Assets/Lava/PodcastSeriesDetail.lava' %}" );
            // Attrib Value for Block:Content Channel View, Attribute:Query Parameter Filtering Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "B35B99F5-7479-44C7-9C58-54B3A56BC233", "AA9CD867-FA21-43C2-822B-CAC06E1F18B8", @"True" );
            // Attrib Value for Block:Content Channel View, Attribute:Status Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "B35B99F5-7479-44C7-9C58-54B3A56BC233", "DA1DEE5D-BCEF-4AA4-A9D9-EFD4DD64462B", @"2" );
            // Attrib Value for Block:Content Channel View, Attribute:Enable Debug Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "B35B99F5-7479-44C7-9C58-54B3A56BC233", "72F4232B-8D2A-4823-B9F1-ED68F182C1A4", @"False" );
            // Attrib Value for Block:Content Channel View, Attribute:Channel Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "B35B99F5-7479-44C7-9C58-54B3A56BC233", "34EACB0F-DBC4-4F18-85C9-0D3EDFDF46BE", @"E2C598F1-D299-1BAA-4873-8B679E3C1998" );
            // Attrib Value for Block:Content Channel View, Attribute:Cache Duration Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "B35B99F5-7479-44C7-9C58-54B3A56BC233", "773BEFDD-EEBA-486C-98E6-AFD0D4156E22", @"0" );
            // Attrib Value for Block:Content Channel View, Attribute:Detail Page Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "B35B99F5-7479-44C7-9C58-54B3A56BC233", "2D7E6F55-B25E-4EA2-8F7E-F7E138E39E21", @"bb83c51d-65c7-4f6c-ba24-a496167c9b11" );
            // Attrib Value for Block:Content Channel View, Attribute:Count Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "B35B99F5-7479-44C7-9C58-54B3A56BC233", "25A501FC-E269-40B8-9904-E20FA7A1ADB6", @"1" );
            // Attrib Value for Block:Content Channel View, Attribute:Meta Description Attribute Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "B35B99F5-7479-44C7-9C58-54B3A56BC233", "E01AE3A7-2607-4DA5-AC98-3A368C900B64", @"" );
            // Attrib Value for Block:Content Channel View, Attribute:Merge Content Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "B35B99F5-7479-44C7-9C58-54B3A56BC233", "20BE4E0A-E84C-4AA1-9368-9732A834E1DE", @"False" );
            // Attrib Value for Block:Content Channel View, Attribute:Rss Autodiscover Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "B35B99F5-7479-44C7-9C58-54B3A56BC233", "ABA69AFC-2C5E-4CF6-A156-EDA2752CCC86", @"False" );
            // Attrib Value for Block:Content Channel View, Attribute:Set Page Title Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "B35B99F5-7479-44C7-9C58-54B3A56BC233", "97161D67-EF24-4F21-9E6A-74B696DD33DE", @"True" );
            // Attrib Value for Block:Content Channel View, Attribute:Order Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "B35B99F5-7479-44C7-9C58-54B3A56BC233", "07ED420E-749C-4938-ADFD-1DDEA6B63014", @"" );
            // Attrib Value for Block:Content Channel View, Attribute:Meta Image Attribute Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "B35B99F5-7479-44C7-9C58-54B3A56BC233", "A3510474-86E5-4AD2-BD4C-3C89E85795F5", @"" );
            // Attrib Value for Block:Content Channel View, Attribute:Meta Image Attribute Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "353BF738-D47A-4799-886D-F3055C98DBDC", "A3510474-86E5-4AD2-BD4C-3C89E85795F5", @"" );
            // Attrib Value for Block:Content Channel View, Attribute:Order Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "353BF738-D47A-4799-886D-F3055C98DBDC", "07ED420E-749C-4938-ADFD-1DDEA6B63014", @"" );
            // Attrib Value for Block:Content Channel View, Attribute:Set Page Title Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "353BF738-D47A-4799-886D-F3055C98DBDC", "97161D67-EF24-4F21-9E6A-74B696DD33DE", @"True" );
            // Attrib Value for Block:Content Channel View, Attribute:Rss Autodiscover Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "353BF738-D47A-4799-886D-F3055C98DBDC", "ABA69AFC-2C5E-4CF6-A156-EDA2752CCC86", @"False" );
            // Attrib Value for Block:Content Channel View, Attribute:Merge Content Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "353BF738-D47A-4799-886D-F3055C98DBDC", "20BE4E0A-E84C-4AA1-9368-9732A834E1DE", @"False" );
            // Attrib Value for Block:Content Channel View, Attribute:Meta Description Attribute Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "353BF738-D47A-4799-886D-F3055C98DBDC", "E01AE3A7-2607-4DA5-AC98-3A368C900B64", @"" );
            // Attrib Value for Block:Content Channel View, Attribute:Count Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "353BF738-D47A-4799-886D-F3055C98DBDC", "25A501FC-E269-40B8-9904-E20FA7A1ADB6", @"1" );
            // Attrib Value for Block:Content Channel View, Attribute:Cache Duration Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "353BF738-D47A-4799-886D-F3055C98DBDC", "773BEFDD-EEBA-486C-98E6-AFD0D4156E22", @"0" );
            // Attrib Value for Block:Content Channel View, Attribute:Channel Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "353BF738-D47A-4799-886D-F3055C98DBDC", "34EACB0F-DBC4-4F18-85C9-0D3EDFDF46BE", @"0A63A427-E6B5-2284-45B3-789B293C02EA" );
            // Attrib Value for Block:Content Channel View, Attribute:Enable Debug Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "353BF738-D47A-4799-886D-F3055C98DBDC", "72F4232B-8D2A-4823-B9F1-ED68F182C1A4", @"False" );
            // Attrib Value for Block:Content Channel View, Attribute:Status Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "353BF738-D47A-4799-886D-F3055C98DBDC", "DA1DEE5D-BCEF-4AA4-A9D9-EFD4DD64462B", @"2" );
            // Attrib Value for Block:Content Channel View, Attribute:Query Parameter Filtering Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "353BF738-D47A-4799-886D-F3055C98DBDC", "AA9CD867-FA21-43C2-822B-CAC06E1F18B8", @"True" );
            // Attrib Value for Block:Content Channel View, Attribute:Template Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "353BF738-D47A-4799-886D-F3055C98DBDC", "8026FEA1-35C1-41CF-9D09-E8B1DB6CBDA8", @"{% include '~~/Assets/Lava/PodcastMessageDetail.lava' %}" );
            // Attrib Value for Block:Content Channel View, Attribute:Filter Id Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "353BF738-D47A-4799-886D-F3055C98DBDC", "618EFBDA-941D-4F60-9AA8-54955B7A03A2", @"76" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove Block: Content Channel View, from Page: Message Detail, Site: External Website
            RockMigrationHelper.DeleteBlock( "353BF738-D47A-4799-886D-F3055C98DBDC" );
            // Remove Block: Content Channel View, from Page: Series Detail, Site: External Website
            RockMigrationHelper.DeleteBlock( "B35B99F5-7479-44C7-9C58-54B3A56BC233" );
            // Remove Block: Content Channel View, from Page: Watch, Site: External Website
            RockMigrationHelper.DeleteBlock( "F3DE9793-89CE-4890-BD01-CD4CA5B6F07C" );

            RockMigrationHelper.DeletePage( "BB83C51D-65C7-4F6C-BA24-A496167C9B11" ); //  Page: Message Detail, Layout: FullWidth, Site: External Website
            RockMigrationHelper.DeletePage( "7669A501-4075-431A-9828-565C47FD21C8" ); //  Page: Series Detail, Layout: FullWidth, Site: External Website
            RockMigrationHelper.DeletePage( "7F4916E3-1CCA-4721-91C1-D0A54ED93AD3" ); //  Page: Watch, Layout: FullWidth, Site: External Website

        }
    }
}
