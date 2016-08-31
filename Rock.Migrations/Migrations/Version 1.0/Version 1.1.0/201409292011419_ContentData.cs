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
    public partial class ContentData : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateEntityType( "Rock.Model.ContentChannelItem", "BF12AE64-21FB-433B-A8A4-E40E8C426DDA", true, true );

            RockMigrationHelper.AddBlockAttributeValue( "095027CB-9114-4CD5-ABE8-1E8882422DCF", "25A501FC-E269-40B8-9904-E20FA7A1ADB6", @"0" ); // Count
            RockMigrationHelper.AddBlockAttributeValue( "095027CB-9114-4CD5-ABE8-1E8882422DCF", "2D7E6F55-B25E-4EA2-8F7E-F7E138E39E21", @"56f1dc05-3d7d-49b6-9a30-5cf271c687f4" ); // Detail Page
            RockMigrationHelper.AddBlockAttributeValue( "095027CB-9114-4CD5-ABE8-1E8882422DCF", "72F4232B-8D2A-4823-B9F1-ED68F182C1A4", @"False" ); // Enable Debug
            RockMigrationHelper.AddBlockAttributeValue( "095027CB-9114-4CD5-ABE8-1E8882422DCF", "773BEFDD-EEBA-486C-98E6-AFD0D4156E22", @"60" ); // Cache Duration
            RockMigrationHelper.AddBlockAttributeValue( "095027CB-9114-4CD5-ABE8-1E8882422DCF", "34EACB0F-DBC4-4F18-85C9-0D3EDFDF46BE", @"8e213bb1-9e6f-40c1-b468-b3f8a60d5d24" ); // Channel
            RockMigrationHelper.AddBlockAttributeValue( "095027CB-9114-4CD5-ABE8-1E8882422DCF", "DA1DEE5D-BCEF-4AA4-A9D9-EFD4DD64462B", @"2" ); // Status
            RockMigrationHelper.AddBlockAttributeValue( "095027CB-9114-4CD5-ABE8-1E8882422DCF", "8026FEA1-35C1-41CF-9D09-E8B1DB6CBDA8", @"
<div id=""announcement-rotator"" class=""carousel slide"" data-ride=""carousel"">
    <!-- Wrapper for slides -->
    <div class=""carousel-inner"">
        {% for item in Items %}
              {% if forloop.index == 1 -%}
                <div class=""item active"">
              {% else -%}
                <div class=""item"">
              {% endif -%}
                  <a href=""{{ LinkedPages.DetailPage }}?Item={{ item.Id }}"">{{ item.Image }}</a>
                </div>
        {% endfor %}
    </div>
    
    <!-- Controls -->
    <a class=""left carousel-control"" href=""#announcement-rotator"" data-slide=""prev"">
        <span class=""fa fa-chevron-left""></span>
    </a>
    <a class=""right carousel-control"" href=""#announcement-rotator"" data-slide=""next"">
        <span class=""fa fa-chevron-right""></span>
    </a>
    
</div>
" ); // Template
            RockMigrationHelper.AddBlockAttributeValue( "095027CB-9114-4CD5-ABE8-1E8882422DCF", "0FC5F418-FF53-4881-BB00-B67D23C5B4EC", @"PrimaryAudience^B364CDEE-F000-4965-AE67-0C80DDA365DC|" ); // Filters

            // Add Block to Page: External Homepage, Site: External Website
            RockMigrationHelper.AddBlock( "85F25819-E948-4960-9DDF-00F54D32444E", "", "143A2345-3E26-4ED0-A2FE-42AAF11B4C0F", "Content Channel Items", "SubFeature", "", "", 0, "2E0FFD29-B4AF-4A5E-B528-667168762ABC" );
            // Attrib Value for Block:Content Channel Items, Attribute:Count Page: External Homepage, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "2E0FFD29-B4AF-4A5E-B528-667168762ABC", "25A501FC-E269-40B8-9904-E20FA7A1ADB6", @"3" );
            // Attrib Value for Block:Content Channel Items, Attribute:Detail Page Page: External Homepage, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "2E0FFD29-B4AF-4A5E-B528-667168762ABC", "2D7E6F55-B25E-4EA2-8F7E-F7E138E39E21", @"56f1dc05-3d7d-49b6-9a30-5cf271c687f4" );
            // Attrib Value for Block:Content Channel Items, Attribute:Enable Debug Page: External Homepage, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "2E0FFD29-B4AF-4A5E-B528-667168762ABC", "72F4232B-8D2A-4823-B9F1-ED68F182C1A4", @"False" );
            // Attrib Value for Block:Content Channel Items, Attribute:Cache Duration Page: External Homepage, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "2E0FFD29-B4AF-4A5E-B528-667168762ABC", "773BEFDD-EEBA-486C-98E6-AFD0D4156E22", @"60" );
            // Attrib Value for Block:Content Channel Items, Attribute:Channel Page: External Homepage, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "2E0FFD29-B4AF-4A5E-B528-667168762ABC", "34EACB0F-DBC4-4F18-85C9-0D3EDFDF46BE", @"8e213bb1-9e6f-40c1-b468-b3f8a60d5d24" );
            // Attrib Value for Block:Content Channel Items, Attribute:Status Page: External Homepage, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "2E0FFD29-B4AF-4A5E-B528-667168762ABC", "DA1DEE5D-BCEF-4AA4-A9D9-EFD4DD64462B", @"2" );
            // Attrib Value for Block:Content Channel Items, Attribute:Template Page: External Homepage, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "2E0FFD29-B4AF-4A5E-B528-667168762ABC", "8026FEA1-35C1-41CF-9D09-E8B1DB6CBDA8", @"
<div class=""row announcement-list"">
    {% for item in Items %}
        <div class=""col-md-4 col-sm-6 announcement"">
            <a href=""{{ LinkedPages.DetailPage }}?Item={{ item.Id }}"">{{ item.Image }}</a>
            <h2 class=""announcement-title"">{{ item.Title }}</h2>
            <p class=""announcement-text"">{{ item.SummaryText }}</p>
            <p><a class=""view-details btn btn-default"" href=""{{ LinkedPages.DetailPage }}?Item={{ item.Id }}"" role=""button"">View details �</a></p>
        </div>
        {% capture breakNow %}{{ forloop.index | Modulo:3 }}{% endcapture %}
        {% if breakNow == 0 -%}
            </div>
            <div class=""row announcement-list"">
        {% endif -%}
    {% endfor -%}
</div>" );
            // Attrib Value for Block:Content Channel Items, Attribute:Filters Page: External Homepage, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "2E0FFD29-B4AF-4A5E-B528-667168762ABC", "0FC5F418-FF53-4881-BB00-B67D23C5B4EC", @"PrimaryAudience^57B2A23F-3B0C-43A8-9F45-332120DCD0EE|" );

            // Add Block to Page: Item Detail, Site: External Website
            RockMigrationHelper.AddBlock( "56F1DC05-3D7D-49B6-9A30-5CF271C687F4", "", "143A2345-3E26-4ED0-A2FE-42AAF11B4C0F", "Content Channel Dynamic", "Main", "", "", 0, "7173AA95-15AF-49C5-933D-004717A3FF3C" );
            // Attrib Value for Block:Content Channel Dynamic, Attribute:Filters Page: Item Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "7173AA95-15AF-49C5-933D-004717A3FF3C", "0FC5F418-FF53-4881-BB00-B67D23C5B4EC", @"" );
            // Attrib Value for Block:Content Channel Dynamic, Attribute:Status Page: Item Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "7173AA95-15AF-49C5-933D-004717A3FF3C", "DA1DEE5D-BCEF-4AA4-A9D9-EFD4DD64462B", @"2" );
            // Attrib Value for Block:Content Channel Dynamic, Attribute:Template Page: Item Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "7173AA95-15AF-49C5-933D-004717A3FF3C", "8026FEA1-35C1-41CF-9D09-E8B1DB6CBDA8", @"
{% for item in Items %}
    <img src=""/GetImage.ashx?Guid={{ item.DetailImage_unformatted }}"" class=""title-image""><h1>{{ item.Title }}</h1>{{ item.Content }}
{% endfor -%}
" );
            // Attrib Value for Block:Content Channel Dynamic, Attribute:Channel Page: Item Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "7173AA95-15AF-49C5-933D-004717A3FF3C", "34EACB0F-DBC4-4F18-85C9-0D3EDFDF46BE", @"8e213bb1-9e6f-40c1-b468-b3f8a60d5d24" );
            // Attrib Value for Block:Content Channel Dynamic, Attribute:Enable Debug Page: Item Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "7173AA95-15AF-49C5-933D-004717A3FF3C", "72F4232B-8D2A-4823-B9F1-ED68F182C1A4", @"False" );
            // Attrib Value for Block:Content Channel Dynamic, Attribute:Cache Duration Page: Item Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "7173AA95-15AF-49C5-933D-004717A3FF3C", "773BEFDD-EEBA-486C-98E6-AFD0D4156E22", @"0" );
            // Attrib Value for Block:Content Channel Dynamic, Attribute:Count Page: Item Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "7173AA95-15AF-49C5-933D-004717A3FF3C", "25A501FC-E269-40B8-9904-E20FA7A1ADB6", @"1" );
            // Attrib Value for Block:Content Channel Dynamic, Attribute:Detail Page Page: Item Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "7173AA95-15AF-49C5-933D-004717A3FF3C", "2D7E6F55-B25E-4EA2-8F7E-F7E138E39E21", @"" );

            Sql( @"
-- Update Content Channel Type page title
UPDATE [Page] SET
	  [InternalName] = 'Content Channel Types'
	, [PageTitle] = 'Content Channel Types'
	, [BrowserTitle] = 'Content Channel Types'
WHERE [Guid] = '37E3D602-5D7D-4818-BCAA-C67EBB301E55'

-- Change the binary file type for content channel items
DECLARE @BinaryFileTypeId int = ( SELECT [Id] FROM [BinaryFileType] WHERE [Guid] = '8DBF874C-F3C2-4848-8137-C963C431EB0B' )
UPDATE [BinaryFileType] SET
	  [Name] = 'Content Channel Item Image'
	, [Description] = 'Image used for Content Channel Items.'
WHERE [Id] = @BinaryFileTypeId

-- Update the 'Marketing Campaign' defined type category to 'Content Channel'
DECLARE @DefinedTypeEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.DefinedType' ) 
UPDATE [Category] SET [Name] = 'Content Channel'
WHERE [EntityTypeId] = @DefinedTypeEntityTypeId AND [Name] = 'Marketing Campaign'

-- Change the name of the 'Audience' defined type
DECLARE @AudienceDefinedTypeId int = ( SELECT [Id] FROM [DefinedType] WHERE [Guid] = '799301A3-2026-4977-994E-45DC68502559' )
UPDATE [DefinedType] SET [Description] = 'Determines which audience(s) (adults, preschool, etc.) a content channel item should be targeted towards.'
WHERE [Id] = @AudienceDefinedTypeId

-- Create channel types
INSERT INTO [ContentChannelType] ( [IsSystem], [Name], [DateRangeType], [Guid] ) VALUES 
	( 0, 'Website Ads', 2, '7D2FAE46-16C6-47B5-93E6-B647BA182D3A' ),
	( 0, 'Bulletin', 1, '206CFC34-1C86-46F5-A1EA-6D71B25A8D33' )  

-- Add the Web Ads Channel
DECLARE @WebAdsTypeId int = ( SELECT TOP 1 [Id] FROM [ContentChannelType] WHERE [Guid] = '7D2FAE46-16C6-47B5-93E6-B647BA182D3A' )
DECLARE @AdminPersonAliasId int = ( SELECT TOP 1 [Id] FROM [PersonAlias] ORDER BY [Id] )
INSERT INTO [ContentChannel] ( [ContentChannelTypeId], [Name], [Description], [IconCssClass], [RequiresApproval], [EnableRss], [ChannelUrl], [ItemUrl] ,[TimeToLive], [Guid] )
VALUES ( @WebAdsTypeId, 'External Website Ads', 'Ads that should be promoted on the external website.', 'fa fa-laptop', 1, 0, '', '', 0, '8E213BB1-9E6F-40C1-B468-B3F8A60D5D24' )
DECLARE @WebAdsChannelId int = SCOPE_IDENTITY()

-- Add the Web Ads items
INSERT INTO [ContentChannelItem] ( [ContentChannelId], [ContentChannelTypeId], [Title], [Priority], [Status], [ApprovedByPersonAliasId], [ApprovedDateTime], [StartDateTime], [ExpireDateTime], [Guid], [Content] )
VALUES
    ( @WebAdsChannelId, @WebAdsTypeId, 'SAMPLE: Easter', 100, 2, CAST(@AdminPersonAliasId as varchar), '2013-08-01 00:00:00.000', '2013-08-01 00:00:00.000', '2020-08-02 00:00:00.000', '3B8E1859-E42F-4F01-9007-D3E04429F17D', '<p>
	Lorem ipsum dolor sit amet, consectetur adipiscing elit. Proin sollicitudin condimentum aliquet. In est nulla, lacinia ac dictum et, laoreet vitae elit. Proin tempus tellus ligula, a consequat diam consectetur a. Phasellus luctus velit sed lorem mollis commodo. Nunc sit amet blandit velit. Donec tincidunt congue facilisis. Sed iaculis at neque non porttitor. Phasellus ultrices egestas erat feugiat pellentesque. Duis venenatis, dolor quis fringilla tempus, sem lorem euismod lectus, sed egestas felis magna at felis. Pellentesque ut rhoncus erat, a pulvinar purus. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Ut sit amet consequat est. Maecenas et porta dui, non condimentum lectus.</p>
<p>
	Suspendisse vel nibh odio. Pellentesque porta sapien ligula, in laoreet diam tempus sed. Morbi nunc erat, mattis eu pulvinar blandit, adipiscing quis magna. Ut quis dui lobortis velit suscipit consectetur. Nulla iaculis fermentum egestas. Aenean venenatis sagittis mauris, sed rhoncus purus accumsan ac. Suspendisse potenti. Sed sed tempor turpis. Duis sit amet nisi nec purus fringilla condimentum. Phasellus non lacus arcu. Donec scelerisque, erat sed tempor elementum, nulla risus scelerisque ante, ac imperdiet velit magna ut quam. Nam tristique orci auctor consequat laoreet. Quisque malesuada metus sed sodales eleifend. Aenean rhoncus, mi sit amet ullamcorper tincidunt, sem sem rutrum felis, in semper enim massa ut sem.</p>
<p>
	Vivamus diam urna, cursus in sapien in, porta gravida enim. Cras non fringilla arcu, tincidunt laoreet lacus. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Aliquam volutpat felis quis augue faucibus ultrices. Morbi lobortis vestibulum sodales. Sed tincidunt urna vitae felis ultrices, pharetra placerat quam dignissim. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Morbi vel adipiscing tellus. In vitae sodales diam. Aliquam pharetra orci a porta molestie. In et neque bibendum, viverra leo sit amet, auctor magna. Morbi posuere massa sed metus euismod, et adipiscing sem dictum. Cras eget elementum risus, non imperdiet ligula.</p>
'),
    ( @WebAdsChannelId, @WebAdsTypeId, 'SAMPLE: Starting Point for Students', 50, 2, CAST(@AdminPersonAliasId as varchar), '2013-08-01 00:00:00.000', '2013-08-01 00:00:00.000', '2020-08-02 00:00:00.000', '41DC3262-1588-4142-AC09-3266FDD6AA17', '<p>
	Suspendisse vel nibh odio. Pellentesque porta sapien ligula, in laoreet diam tempus sed. Morbi nunc erat, mattis eu pulvinar blandit, adipiscing quis magna. Ut quis dui lobortis velit suscipit consectetur. Nulla iaculis fermentum egestas. Aenean venenatis sagittis mauris, sed rhoncus purus accumsan ac. Suspendisse potenti. Sed sed tempor turpis. Duis sit amet nisi nec purus fringilla condimentum. Phasellus non lacus arcu. Donec scelerisque, erat sed tempor elementum, nulla risus scelerisque ante, ac imperdiet velit magna ut quam. Nam tristique orci auctor consequat laoreet. Quisque malesuada metus sed sodales eleifend. Aenean rhoncus, mi sit amet ullamcorper tincidunt, sem sem rutrum felis, in semper enim massa ut sem.</p>
<p>
	Vivamus diam urna, cursus in sapien in, porta gravida enim. Cras non fringilla arcu, tincidunt laoreet lacus. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Aliquam volutpat felis quis augue faucibus ultrices. Morbi lobortis vestibulum sodales. Sed tincidunt urna vitae felis ultrices, pharetra placerat quam dignissim. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Morbi vel adipiscing tellus. In vitae sodales diam. Aliquam pharetra orci a porta molestie. In et neque bibendum, viverra leo sit amet, auctor magna. Morbi posuere massa sed metus euismod, et adipiscing sem dictum. Cras eget elementum risus, non imperdiet ligula.</p>
<p>
	In hac habitasse platea dictumst. Praesent quis imperdiet eros. Integer nec tellus ipsum. Cras sed nisl vel lectus ultricies cursus eget ut est. Phasellus sed blandit urna. Proin nulla nisl, facilisis a commodo non, facilisis sit amet odio. Sed pharetra nibh non luctus consequat. Vivamus tempor urna lectus, at posuere magna rhoncus eget. Mauris pretium sem quis fringilla molestie. Aliquam hendrerit odio eu dolor bibendum, eu dignissim sem accumsan. Nam vel risus fermentum, accumsan odio a, accumsan nisi. Aliquam ac lacus tempus, pellentesque lectus semper, facilisis orci. Maecenas dolor mi, pharetra a neque tincidunt, auctor aliquet sapien. Donec sagittis, metus ut rutrum posuere, purus enim bibendum tellus, eget porttitor turpis lectus ac dolor.</p>
'),
    ( @WebAdsChannelId, @WebAdsTypeId, 'SAMPLE: Amazing Race', 70, 2, CAST(@AdminPersonAliasId as varchar), '2013-08-01 00:00:00.000', '2013-08-01 00:00:00.000', '2020-08-02 00:00:00.000', '462FDDA7-2FB6-41A7-9671-30865D991A33', '<p>
	Vivamus diam urna, cursus in sapien in, porta gravida enim. Cras non fringilla arcu, tincidunt laoreet lacus. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Aliquam volutpat felis quis augue faucibus ultrices. Morbi lobortis vestibulum sodales. Sed tincidunt urna vitae felis ultrices, pharetra placerat quam dignissim. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Morbi vel adipiscing tellus. In vitae sodales diam. Aliquam pharetra orci a porta molestie. In et neque bibendum, viverra leo sit amet, auctor magna. Morbi posuere massa sed metus euismod, et adipiscing sem dictum. Cras eget elementum risus, non imperdiet ligula.</p>
<p>
	In hac habitasse platea dictumst. Praesent quis imperdiet eros. Integer nec tellus ipsum. Cras sed nisl vel lectus ultricies cursus eget ut est. Phasellus sed blandit urna. Proin nulla nisl, facilisis a commodo non, facilisis sit amet odio. Sed pharetra nibh non luctus consequat. Vivamus tempor urna lectus, at posuere magna rhoncus eget. Mauris pretium sem quis fringilla molestie. Aliquam hendrerit odio eu dolor bibendum, eu dignissim sem accumsan. Nam vel risus fermentum, accumsan odio a, accumsan nisi. Aliquam ac lacus tempus, pellentesque lectus semper, facilisis orci. Maecenas dolor mi, pharetra a neque tincidunt, auctor aliquet sapien. Donec sagittis, metus ut rutrum posuere, purus enim bibendum tellus, eget porttitor turpis lectus ac dolor.</p>
'),
    ( @WebAdsChannelId, @WebAdsTypeId, 'SAMPLE: Warrior Youth Event', 70, 2, CAST(@AdminPersonAliasId as varchar), '2013-08-01 00:00:00.000', '2013-08-01 00:00:00.000', '2020-08-02 00:00:00.000', '6B5E7F2B-F55B-42E1-BCA4-64206BC9E276', '<p>
	In hac habitasse platea dictumst. Praesent quis imperdiet eros. Integer nec tellus ipsum. Cras sed nisl vel lectus ultricies cursus eget ut est. Phasellus sed blandit urna. Proin nulla nisl, facilisis a commodo non, facilisis sit amet odio. Sed pharetra nibh non luctus consequat. Vivamus tempor urna lectus, at posuere magna rhoncus eget. Mauris pretium sem quis fringilla molestie. Aliquam hendrerit odio eu dolor bibendum, eu dignissim sem accumsan. Nam vel risus fermentum, accumsan odio a, accumsan nisi. Aliquam ac lacus tempus, pellentesque lectus semper, facilisis orci. Maecenas dolor mi, pharetra a neque tincidunt, auctor aliquet sapien. Donec sagittis, metus ut rutrum posuere, purus enim bibendum tellus, eget porttitor turpis lectus ac dolor.</p>
<p>
	Morbi sit amet enim sit amet ipsum pretium fringilla. Sed eget sem aliquam, sollicitudin risus vel, adipiscing dui. Vestibulum et tempus libero. Mauris dignissim venenatis mattis. Aliquam sagittis vulputate purus id auctor. Curabitur varius ligula lacinia placerat lobortis. Sed ornare feugiat odio eu blandit. Pellentesque nec odio scelerisque, feugiat elit quis, porta ipsum. Suspendisse blandit at enim consequat aliquet. Morbi euismod nec tellus vel consequat. Curabitur euismod dapibus urna, sit amet eleifend enim porta non. Praesent interdum vehicula ultrices. Aliquam erat volutpat. Donec quis lectus at nulla consequat cursus at ac est.</p>
'),
    ( @WebAdsChannelId, @WebAdsTypeId, 'SAMPLE: Car Show', 80, 2, CAST(@AdminPersonAliasId as varchar), '2013-08-01 00:00:00.000', '2013-08-01 00:00:00.000', '2020-08-02 00:00:00.000', '8C5D4D99-7FF0-4CBF-AE5C-B7C9075EBE05', '<p>
	Curabitur a neque in nibh pretium rutrum nec pharetra ligula. Nulla molestie imperdiet rhoncus. Nulla non semper sapien. Phasellus vel nisi vel ante imperdiet lacinia eu quis odio. In et felis eu sem luctus lacinia. Donec et purus eu dui luctus vehicula. Proin malesuada arcu at ipsum volutpat ullamcorper. Donec facilisis eros a turpis volutpat, at faucibus turpis bibendum. Praesent faucibus mauris sit amet erat lobortis faucibus at rutrum nunc. Phasellus dapibus sed quam eu sodales. Nulla ornare venenatis venenatis.</p>
<p>
	Duis vel massa egestas, cursus odio vestibulum, pulvinar felis. Quisque mattis enim nec libero euismod venenatis id nec arcu. Donec quis lectus leo. Nullam nec enim a massa placerat fermentum. Pellentesque dolor turpis, imperdiet nec nisl sed, ultricies condimentum sapien. Proin facilisis quam diam, quis varius risus aliquam eu. Suspendisse sed neque interdum nulla egestas molestie eget sed est. Mauris sed eros in neque scelerisque consequat. Ut commodo semper pharetra.</p>
<p>
	Maecenas eget elit dui. Nullam eu elementum ante. Morbi placerat in nisi eget hendrerit. Cras facilisis massa sit amet luctus dictum. Quisque pretium sapien vitae tincidunt molestie. Etiam eu lacinia odio. Nullam sit amet interdum lectus. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Etiam tempor et tortor et tristique. Ut sagittis neque non metus molestie, et dignissim massa porta.</p>
'),
    ( @WebAdsChannelId, @WebAdsTypeId, 'SAMPLE: Bible in One Year', 70, 2, CAST(@AdminPersonAliasId as varchar), '2013-08-01 00:00:00.000', '2013-08-01 00:00:00.000', '2020-08-02 00:00:00.000', '699C7C21-5798-4150-AE28-379D335FABE4', '<p>
	Maecenas eget elit dui. Nullam eu elementum ante. Morbi placerat in nisi eget hendrerit. Cras facilisis massa sit amet luctus dictum. Quisque pretium sapien vitae tincidunt molestie. Etiam eu lacinia odio. Nullam sit amet interdum lectus. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Etiam tempor et tortor et tristique. Ut sagittis neque non metus molestie, et dignissim massa porta.</p>
<p>
	Nulla vitae imperdiet lorem. Curabitur adipiscing lectus magna, vel pellentesque lorem semper quis. Nullam blandit neque sapien, ac lobortis quam mollis ac. Mauris scelerisque erat sit amet tellus iaculis aliquam. Duis lectus massa, dapibus eget mauris at, fringilla mattis lorem. Suspendisse eget est vitae odio eleifend auctor ac et leo. Praesent malesuada, felis at dignissim condimentum, urna mi vestibulum dui, a sagittis nibh mauris at libero. Nunc venenatis massa vel ultricies lobortis. Ut et urna justo. Nunc cursus eget dolor eget condimentum. Vivamus eu pulvinar sapien, a fringilla nisi. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Aenean in ultricies dui. Nulla vel lorem lacus.</p>
'),
    ( @WebAdsChannelId, @WebAdsTypeId, 'SAMPLE: Rock Solid Finances', 65, 2, CAST(@AdminPersonAliasId as varchar), '2013-08-01 00:00:00.000', '2013-08-01 00:00:00.000', '2020-08-02 00:00:00.000', 'E78148EA-35C8-43A8-B4C2-3FAB7925DF53', '<p>
	Nulla vitae imperdiet lorem. Curabitur adipiscing lectus magna, vel pellentesque lorem semper quis. Nullam blandit neque sapien, ac lobortis quam mollis ac. Mauris scelerisque erat sit amet tellus iaculis aliquam. Duis lectus massa, dapibus eget mauris at, fringilla mattis lorem. Suspendisse eget est vitae odio eleifend auctor ac et leo. Praesent malesuada, felis at dignissim condimentum, urna mi vestibulum dui, a sagittis nibh mauris at libero. Nunc venenatis massa vel ultricies lobortis. Ut et urna justo. Nunc cursus eget dolor eget condimentum. Vivamus eu pulvinar sapien, a fringilla nisi. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Aenean in ultricies dui. Nulla vel lorem lacus.</p>
<p>
	Praesent enim justo, aliquam eget consequat vel, gravida vitae tellus. Sed metus diam, accumsan ut velit at, auctor rhoncus massa. In neque nisi, volutpat et tristique pulvinar, fermentum id nulla. Vivamus sodales pharetra dui sit amet ultricies. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Pellentesque sit amet auctor magna, nec ultrices felis. In vitae purus neque. Nullam rhoncus condimentum elit, vitae rutrum nulla fermentum ut. Aenean a condimentum metus. Donec non hendrerit arcu. Pellentesque placerat nisi sed nisi accumsan, at scelerisque diam condimentum. Curabitur et arcu quis mi tincidunt pretium vel nec justo. Fusce vehicula erat et velit congue, ut dapibus dolor convallis.</p>
'),
    ( @WebAdsChannelId, @WebAdsTypeId, 'SAMPLE: Glow', 70, 2, CAST(@AdminPersonAliasId as varchar), '2013-08-01 00:00:00.000', '2013-08-01 00:00:00.000', '2020-08-02 00:00:00.000', 'A9A15E0E-F736-4F84-9527-50217B4E9091', '<p>
	Sed sagittis dui quis faucibus molestie. Quisque mattis pellentesque lacinia. Mauris risus ipsum, molestie ut purus eget, imperdiet commodo nunc. Praesent venenatis libero lectus, eu rutrum sapien congue quis. Phasellus vel urna aliquet, aliquam nibh in, semper velit. Quisque pellentesque ultrices eros et placerat. Duis tempor adipiscing ligula, at adipiscing neque condimentum vitae. Donec congue a erat eu molestie.</p>
<p>
	Sed auctor et nisl eu sagittis. Sed tellus erat, gravida nec vestibulum non, accumsan nec magna. Etiam mattis est eget magna rhoncus, sit amet posuere lorem elementum. Fusce non libero nec ipsum posuere vehicula a vel justo. In sagittis sem eu sem aliquam lobortis. Vestibulum lacinia erat in nibh molestie pretium. Etiam auctor, risus ut sodales commodo, velit metus condimentum libero, non dapibus erat diam eu lorem. Aenean malesuada sapien sed purus feugiat mollis. In vel leo ipsum. Phasellus a magna elit. Nulla fringilla dolor sapien, sodales feugiat metus vehicula vel.</p>
')

-- Delete any existing channel item attributes 
DECLARE @ChannelItemEntityId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.ContentChannelItem' )
DELETE [Attribute] WHERE [EntityTypeId] = @ChannelItemEntityId

-- Create summary text attribute for web ad type
DECLARE @MemoFieldTypeId int = ( SELECT TOP 1 [Id] FROM [FieldType] WHERE [Guid] = 'C28C7BF3-A552-4D77-9408-DEDCF760CED0' )
DECLARE @ImageFieldTypeId int = ( SELECT TOP 1 [Id] FROM [FieldType] WHERE [Guid] = '97F8157D-A8C8-4AB3-96A2-9CB2A9049E6D' )
DECLARE @CampusesFieldTypeId int = ( SELECT TOP 1 [Id] FROM [FieldType] WHERE [Guid] = '69254F91-C97F-4C2D-9ACB-1683B088097B' )
DECLARE @DefinedValueFieldTypeId int = ( SELECT TOP 1 [Id] FROM [FieldType] WHERE [Guid] = '59D5A94C-94A0-4630-B80A-BB25697D74C7' )

INSERT INTO [Attribute] ([IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],
	[Key],[Name],[Description],[Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],[Guid])
VALUES (0, @MemoFieldTypeId, @ChannelItemEntityId, 'ContentChannelTypeId', CAST(@WebAdsTypeId as varchar),
	'SummaryText', 'Summary Text', 'Short description',0,0,'',0,0,'35993D3B-57D3-4F41-88A5-A83EE380D2DD')
DECLARE @AttributeId int = SCOPE_IDENTITy()

INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
VALUES 
	( 0, @AttributeId, ( SELECT TOP 1 [Id] FROM [ContentChannelItem] WHERE [Guid] = '3B8E1859-E42F-4F01-9007-D3E04429F17D'), 'Lorem ipsum dolor sit amet, consectetur adipiscing elit. Proin sollicitudin condimentum aliquet. In est nulla, lacinia ac dictum et, laoreet vitae elit. Proin tempus tellus ligula, a consequat diam consectetur a.', NEWID() ),
	( 0, @AttributeId, ( SELECT TOP 1 [Id] FROM [ContentChannelItem] WHERE [Guid] = '41DC3262-1588-4142-AC09-3266FDD6AA17'), 'Suspendisse vel nibh odio. Pellentesque porta sapien ligula, in laoreet diam tempus sed. Morbi nunc erat, mattis eu pulvinar blandit, adipiscing quis magna. Ut quis dui lobortis velit suscipit consectetur. Nulla iaculis fermentum egestas.', NEWID() ),
	( 0, @AttributeId, ( SELECT TOP 1 [Id] FROM [ContentChannelItem] WHERE [Guid] = '462FDDA7-2FB6-41A7-9671-30865D991A33'), 'Vivamus diam urna, cursus in sapien in, porta gravida enim. Cras non fringilla arcu, tincidunt laoreet lacus. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos.', NEWID() ),
	( 0, @AttributeId, ( SELECT TOP 1 [Id] FROM [ContentChannelItem] WHERE [Guid] = '6B5E7F2B-F55B-42E1-BCA4-64206BC9E276'), 'In hac habitasse platea dictumst. Praesent quis imperdiet eros. Integer nec tellus ipsum. Cras sed nisl vel lectus ultricies cursus eget ut est. Phasellus sed blandit urna.', NEWID() ),
	( 0, @AttributeId, ( SELECT TOP 1 [Id] FROM [ContentChannelItem] WHERE [Guid] = '8C5D4D99-7FF0-4CBF-AE5C-B7C9075EBE05'), 'Curabitur a neque in nibh pretium rutrum nec pharetra ligula. Nulla molestie imperdiet rhoncus. Nulla non semper sapien. Phasellus vel nisi vel ante imperdiet lacinia eu quis odio. In et felis eu sem luctus lacinia. Donec et purus eu dui luctus vehicula.', NEWID() ),
	( 0, @AttributeId, ( SELECT TOP 1 [Id] FROM [ContentChannelItem] WHERE [Guid] = '699C7C21-5798-4150-AE28-379D335FABE4'), 'Maecenas eget elit dui. Nullam eu elementum ante. Morbi placerat in nisi eget hendrerit. Cras facilisis massa sit amet luctus dictum. Quisque pretium sapien vitae tincidunt molestie. Etiam eu lacinia odio. Nullam sit amet interdum lectus.', NEWID() ),
	( 0, @AttributeId, ( SELECT TOP 1 [Id] FROM [ContentChannelItem] WHERE [Guid] = 'E78148EA-35C8-43A8-B4C2-3FAB7925DF53'), 'Nulla vitae imperdiet lorem. Curabitur adipiscing lectus magna, vel pellentesque lorem semper quis. Nullam blandit neque sapien, ac lobortis quam mollis ac.', NEWID() ),
	( 0, @AttributeId, ( SELECT TOP 1 [Id] FROM [ContentChannelItem] WHERE [Guid] = 'A9A15E0E-F736-4F84-9527-50217B4E9091'), 'Sed sagittis dui quis faucibus molestie. Quisque mattis pellentesque lacinia. Mauris risus ipsum, molestie ut purus eget, imperdiet commodo nunc. Praesent venenatis libero lectus, eu rutrum sapien congue quis.', NEWID() )

-- Create image attribute for web ad type
INSERT INTO [Attribute] ([IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],
	[Key],[Name],[Description],[Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],[Guid])
VALUES (0, @ImageFieldTypeId, @ChannelItemEntityId, 'ContentChannelTypeId', CAST(@WebAdsTypeId as varchar),
	'Image', 'Image', '',1,0,'',0,0,'FFDF621C-ECFF-4199-AB90-D678C36DCE38')
SET @AttributeId = SCOPE_IDENTITy()

INSERT INTO [AttributeQualifier] ([IsSystem], [AttributeId], [Key], [Value], [Guid] )
VALUES (0, @AttributeId, 'binaryFileType', CAST(@BinaryFileTypeId AS varchar), NEWID() )

INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
VALUES 
	( 0, @AttributeId, ( SELECT TOP 1 [Id] FROM [ContentChannelItem] WHERE [Guid] = '3B8E1859-E42F-4F01-9007-D3E04429F17D'), '0241ED2F-B527-424C-917C-1142A398711F', NEWID() ),
	( 0, @AttributeId, ( SELECT TOP 1 [Id] FROM [ContentChannelItem] WHERE [Guid] = '41DC3262-1588-4142-AC09-3266FDD6AA17'), '14956C3D-DE89-4FE6-90E7-4A863760BE09', NEWID() ),
	( 0, @AttributeId, ( SELECT TOP 1 [Id] FROM [ContentChannelItem] WHERE [Guid] = '462FDDA7-2FB6-41A7-9671-30865D991A33'), 'E300EF10-1E76-4370-9209-8CCC0DC7D75A', NEWID() ),
	( 0, @AttributeId, ( SELECT TOP 1 [Id] FROM [ContentChannelItem] WHERE [Guid] = '6B5E7F2B-F55B-42E1-BCA4-64206BC9E276'), 'AF17CB8E-BA9C-4D2E-AB49-1FBE8A94A49D', NEWID() ),
	( 0, @AttributeId, ( SELECT TOP 1 [Id] FROM [ContentChannelItem] WHERE [Guid] = '8C5D4D99-7FF0-4CBF-AE5C-B7C9075EBE05'), '923329F4-819E-4EAA-8D96-9611624736E8', NEWID() ),
	( 0, @AttributeId, ( SELECT TOP 1 [Id] FROM [ContentChannelItem] WHERE [Guid] = '699C7C21-5798-4150-AE28-379D335FABE4'), '62D14425-9570-42CF-BB7C-4B78D8FBCD24', NEWID() ),
	( 0, @AttributeId, ( SELECT TOP 1 [Id] FROM [ContentChannelItem] WHERE [Guid] = 'E78148EA-35C8-43A8-B4C2-3FAB7925DF53'), '92DC752E-0BEA-446E-90C2-265F94FF17B1', NEWID() ),
	( 0, @AttributeId, ( SELECT TOP 1 [Id] FROM [ContentChannelItem] WHERE [Guid] = 'A9A15E0E-F736-4F84-9527-50217B4E9091'), 'A79B7F88-BE1B-4048-AAAA-76155CA82670', NEWID() )

-- Create detail image attribute for web ad type
INSERT INTO [Attribute] ([IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],
	[Key],[Name],[Description],[Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],[Guid])
VALUES (0, @ImageFieldTypeId, @ChannelItemEntityId, 'ContentChannelTypeId', CAST(@WebAdsTypeId as varchar),
	'DetailImage', 'Detail Image', '',2,0,'',0,0,'43758FC4-906E-46CD-A6FB-8F21176C1CC5')
SET @AttributeId = SCOPE_IDENTITy()

INSERT INTO [AttributeQualifier] ([IsSystem], [AttributeId], [Key], [Value], [Guid] )
VALUES (0, @AttributeId, 'binaryFileType', CAST(@BinaryFileTypeId AS varchar), NEWID() )

INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
VALUES 
	( 0, @AttributeId, ( SELECT TOP 1 [Id] FROM [ContentChannelItem] WHERE [Guid] = '3B8E1859-E42F-4F01-9007-D3E04429F17D'), '3DA90982-118A-4BFE-9A32-58D9F610090A', NEWID() ),
	( 0, @AttributeId, ( SELECT TOP 1 [Id] FROM [ContentChannelItem] WHERE [Guid] = '41DC3262-1588-4142-AC09-3266FDD6AA17'), '5AE6B225-4E48-4AE4-86A2-C17E882FE468', NEWID() ),
	( 0, @AttributeId, ( SELECT TOP 1 [Id] FROM [ContentChannelItem] WHERE [Guid] = '462FDDA7-2FB6-41A7-9671-30865D991A33'), 'AB031AA5-5DC1-4E18-87BF-4B0C319ED450', NEWID() ),
	( 0, @AttributeId, ( SELECT TOP 1 [Id] FROM [ContentChannelItem] WHERE [Guid] = '6B5E7F2B-F55B-42E1-BCA4-64206BC9E276'), 'D00FCEAA-2D16-40BF-9BA4-352D42605E28', NEWID() ),
	( 0, @AttributeId, ( SELECT TOP 1 [Id] FROM [ContentChannelItem] WHERE [Guid] = '8C5D4D99-7FF0-4CBF-AE5C-B7C9075EBE05'), '8EE5A840-0A10-44E2-9DBE-214AF27C234B', NEWID() ),
	( 0, @AttributeId, ( SELECT TOP 1 [Id] FROM [ContentChannelItem] WHERE [Guid] = '699C7C21-5798-4150-AE28-379D335FABE4'), '8EAAD843-1EEF-40B2-B00B-543BC5723CF7', NEWID() ),
	( 0, @AttributeId, ( SELECT TOP 1 [Id] FROM [ContentChannelItem] WHERE [Guid] = 'E78148EA-35C8-43A8-B4C2-3FAB7925DF53'), '5047381D-0CA9-49F7-9894-699C296BAAB6', NEWID() ),
	( 0, @AttributeId, ( SELECT TOP 1 [Id] FROM [ContentChannelItem] WHERE [Guid] = 'A9A15E0E-F736-4F84-9527-50217B4E9091'), '2ECD09FA-431E-495A-89C1-CFC418A36C3F', NEWID() )

-- Create campuses attribute for web ad type
INSERT INTO [Attribute] ([IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],
	[Key],[Name],[Description],[Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],[Guid])
VALUES (0, @CampusesFieldTypeId, @ChannelItemEntityId, 'ContentChannelTypeId', CAST(@WebAdsTypeId as varchar),
	'Campuses', 'Campuses', 'The campus or campuses that the item is associated with',3,0,'',0,0,'FD8DF8AC-0AF2-4738-8DDA-3B1030C3E0CE')
SET @AttributeId = SCOPE_IDENTITy()

-- Create primary audience attribute for web ad type
INSERT INTO [Attribute] ([IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],
	[Key],[Name],[Description],[Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],[Guid])
VALUES (0, @DefinedValueFieldTypeId, @ChannelItemEntityId, 'ContentChannelTypeId', CAST(@WebAdsTypeId as varchar),
	'PrimaryAudience', 'Primary Audience', 'The primary audience that the item is targeted towards.',4,0,'',0,0,'CA97AADD-9795-434A-B924-250CC15CD7A5')
SET @AttributeId = SCOPE_IDENTITy()

INSERT INTO [AttributeQualifier] ([IsSystem], [AttributeId], [Key], [Value], [Guid] )
VALUES 
    (0, @AttributeId, 'definedtype', CAST(@AudienceDefinedTypeId AS varchar), NEWID() ),
    (0, @AttributeId, 'allowmultiple', 'False', NEWID() )

INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
VALUES 
    ( 0, @AttributeId, ( SELECT TOP 1 [Id] FROM [ContentChannelItem] WHERE [Guid] = '3B8E1859-E42F-4F01-9007-D3E04429F17D'), 'B364CDEE-F000-4965-AE67-0C80DDA365DC', NEWID() ),
    ( 0, @AttributeId, ( SELECT TOP 1 [Id] FROM [ContentChannelItem] WHERE [Guid] = '41DC3262-1588-4142-AC09-3266FDD6AA17'), 'B364CDEE-F000-4965-AE67-0C80DDA365DC', NEWID() ),
    ( 0, @AttributeId, ( SELECT TOP 1 [Id] FROM [ContentChannelItem] WHERE [Guid] = '462FDDA7-2FB6-41A7-9671-30865D991A33'), 'B364CDEE-F000-4965-AE67-0C80DDA365DC', NEWID() ),
    ( 0, @AttributeId, ( SELECT TOP 1 [Id] FROM [ContentChannelItem] WHERE [Guid] = '6B5E7F2B-F55B-42E1-BCA4-64206BC9E276'), 'B364CDEE-F000-4965-AE67-0C80DDA365DC', NEWID() ),
    ( 0, @AttributeId, ( SELECT TOP 1 [Id] FROM [ContentChannelItem] WHERE [Guid] = '8C5D4D99-7FF0-4CBF-AE5C-B7C9075EBE05'), '57B2A23F-3B0C-43A8-9F45-332120DCD0EE', NEWID() ),
    ( 0, @AttributeId, ( SELECT TOP 1 [Id] FROM [ContentChannelItem] WHERE [Guid] = '699C7C21-5798-4150-AE28-379D335FABE4'), '57B2A23F-3B0C-43A8-9F45-332120DCD0EE', NEWID() ),
    ( 0, @AttributeId, ( SELECT TOP 1 [Id] FROM [ContentChannelItem] WHERE [Guid] = 'E78148EA-35C8-43A8-B4C2-3FAB7925DF53'), '57B2A23F-3B0C-43A8-9F45-332120DCD0EE', NEWID() ),
    ( 0, @AttributeId, ( SELECT TOP 1 [Id] FROM [ContentChannelItem] WHERE [Guid] = 'A9A15E0E-F736-4F84-9527-50217B4E9091'), '57B2A23F-3B0C-43A8-9F45-332120DCD0EE', NEWID() )

-- Create secondary audience attribute for web ad type
INSERT INTO [Attribute] ([IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],
	[Key],[Name],[Description],[Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],[Guid])
VALUES (0, @DefinedValueFieldTypeId, @ChannelItemEntityId, 'ContentChannelTypeId', CAST(@WebAdsTypeId as varchar),
	'SecondaryAudiences', 'Secondary Audiences', 'Any secondary audiences that the item is targeted towards.',5,0,'',0,0,'95E68A40-C987-4B53-8D9A-0C5605284E4C')
SET @AttributeId = SCOPE_IDENTITy()

INSERT INTO [AttributeQualifier] ([IsSystem], [AttributeId], [Key], [Value], [Guid] )
VALUES 
    (0, @AttributeId, 'definedtype', CAST(@AudienceDefinedTypeId AS varchar), NEWID() ),
    (0, @AttributeId, 'allowmultiple', 'True', NEWID() )

" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove Block: Content Channel Dynamic, from Page: Item Detail, Site: External Website
            RockMigrationHelper.DeleteBlock( "7173AA95-15AF-49C5-933D-004717A3FF3C" );
            // Remove Block: Content Channel Items, from Page: External Homepage, Site: External Website
            RockMigrationHelper.DeleteBlock( "2E0FFD29-B4AF-4A5E-B528-667168762ABC" );
        }
    }
}
