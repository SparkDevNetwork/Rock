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
    public partial class ContentOptions : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.ContentChannel", "ContentControlType", c => c.Int(nullable: false));
            AddColumn("dbo.ContentChannel", "RootImageDirectory", c => c.String(maxLength: 200));
            AddColumn("dbo.ContentChannelType", "DisablePriority", c => c.Boolean(nullable: false));
            AlterColumn("dbo.ContentChannel", "ChannelUrl", c => c.String(maxLength: 200));
            AlterColumn("dbo.ContentChannel", "ItemUrl", c => c.String(maxLength: 200));

            RockMigrationHelper.UpdateBlockTypeAttribute( "143A2345-3E26-4ED0-A2FE-42AAF11B4C0F", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "The page to navigate to for details.", 0, @"", "2D7E6F55-B25E-4EA2-8F7E-F7E138E39E21" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "143A2345-3E26-4ED0-A2FE-42AAF11B4C0F", "D835A0EC-C8DB-483A-A37C-E8FB6E956C3D", "Channel", "Channel", "", "The channel to display items from.", 1, @"", "34EACB0F-DBC4-4F18-85C9-0D3EDFDF46BE" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "143A2345-3E26-4ED0-A2FE-42AAF11B4C0F", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Status", "Status", "", "Include items with the following status.", 2, @"2", "DA1DEE5D-BCEF-4AA4-A9D9-EFD4DD64462B" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "143A2345-3E26-4ED0-A2FE-42AAF11B4C0F", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Template", "Template", "", "The template to use when formatting the list of items.", 3, @"", "8026FEA1-35C1-41CF-9D09-E8B1DB6CBDA8" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "143A2345-3E26-4ED0-A2FE-42AAF11B4C0F", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Count", "Count", "", "The maximum number of items to display.", 4, @"5", "25A501FC-E269-40B8-9904-E20FA7A1ADB6" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "143A2345-3E26-4ED0-A2FE-42AAF11B4C0F", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Cache Duration", "CacheDuration", "", "Number of seconds to cache the content.", 5, @"3600", "773BEFDD-EEBA-486C-98E6-AFD0D4156E22" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "143A2345-3E26-4ED0-A2FE-42AAF11B4C0F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Debug", "EnableDebug", "", "Enabling debug will display the fields of the first 5 items to help show you wants available for your liquid.", 6, @"False", "72F4232B-8D2A-4823-B9F1-ED68F182C1A4" );
            RockMigrationHelper.AddBlockTypeAttribute( "143A2345-3E26-4ED0-A2FE-42AAF11B4C0F", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Filter Id", "FilterId", "", "The data filter that is used to filter items", 7, @"0", "618EFBDA-941D-4F60-9AA8-54955B7A03A2" );
            RockMigrationHelper.AddBlockTypeAttribute( "143A2345-3E26-4ED0-A2FE-42AAF11B4C0F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Query Parameter Filtering", "QueryParameterFiltering", "", "Determines if block should evaluate the query string parameters for additional filter criteria.", 8, @"False", "AA9CD867-FA21-43C2-822B-CAC06E1F18B8" );
            RockMigrationHelper.AddBlockTypeAttribute( "143A2345-3E26-4ED0-A2FE-42AAF11B4C0F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Order", "Order", "", "The specifics of how items should be ordered. This value is set through configuration and should not be modified here.", 9, @"", "07ED420E-749C-4938-ADFD-1DDEA6B63014" );
            RockMigrationHelper.AddBlockTypeAttribute( "143A2345-3E26-4ED0-A2FE-42AAF11B4C0F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Merge Content", "MergeContent", "", "Should the content data and attribute values be merged using the liquid template engine.", 10, @"false", "20BE4E0A-E84C-4AA1-9368-9732A834E1DE" );
            RockMigrationHelper.AddBlockTypeAttribute( "143A2345-3E26-4ED0-A2FE-42AAF11B4C0F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Set Page Title", "SetPageTitle", "", "Determines if the block should set the page title with the channel name or content item.", 11, @"false", "97161D67-EF24-4F21-9E6A-74B696DD33DE" );
            RockMigrationHelper.AddBlockTypeAttribute( "143A2345-3E26-4ED0-A2FE-42AAF11B4C0F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Rss Autodiscover", "RssAutodiscover", "", "Determines if a RSS autodiscover link should be added to the page head.", 12, @"false", "ABA69AFC-2C5E-4CF6-A156-EDA2752CCC86" );
            RockMigrationHelper.AddBlockTypeAttribute( "143A2345-3E26-4ED0-A2FE-42AAF11B4C0F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Meta Description Attribute", "MetaDescriptionAttribute", "", "Attribute to use for storing the description attribute.", 13, @"", "E01AE3A7-2607-4DA5-AC98-3A368C900B64" );
            RockMigrationHelper.AddBlockTypeAttribute( "143A2345-3E26-4ED0-A2FE-42AAF11B4C0F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Meta Image Attribute", "MetaImageAttribute", "", "Attribute to use for storing the image attribute.", 14, @"", "A3510474-86E5-4AD2-BD4C-3C89E85795F5" );

            RockMigrationHelper.DeleteAttribute( "0FC5F418-FF53-4881-BB00-B67D23C5B4EC" ); // Old filter attribute

            Sql( @"
    DECLARE @PropertyFilterEntityId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Reporting.DataFilter.PropertyFilter' )
    DECLARE @AttributeId int = ( SELECT [Id] FROM [Attribute] WHERE [Guid] = '618EFBDA-941D-4F60-9AA8-54955B7A03A2' )
	IF @AttributeId IS NOT NULL
	BEGIN

		DECLARE @BlockId int = (SELECT [Id] FROM [Block] WHERE [Guid] = '095027CB-9114-4CD5-ABE8-1E8882422DCF')
		IF @BlockId IS NOT NULL
		BEGIN

			DECLARE @FilterId int

			INSERT INTO [DataViewFilter] ( [ExpressionType], [Guid] ) 
			VALUES ( 1, NEWID() )
			SET @FilterId = SCOPE_IDENTITY()

			-- If 'Rotator' audience exists, create a data filter for the first context item block on home page
			IF EXISTS ( SELECT [Id] FROM [DefinedValue] WHERE [Guid] = 'b364cdee-f000-4965-ae67-0c80dda365dc' )
			BEGIN

				INSERT INTO [DataViewFilter] ( [ExpressionType], [ParentId], [EntityTypeId], [Selection], [Guid] ) 
				VALUES ( 0, @FilterId, @PropertyFilterEntityId, '
[
	""PrimaryAudience"",
	""[\r\n  \""b364cdee-f000-4965-ae67-0c80dda365dc\""\r\n]""
]', NEWID() )

			END

			INSERT INTO [DataViewFilter] ( [ExpressionType], [ParentId], [EntityTypeId], [Selection], [Guid] ) 
			VALUES ( 0, @FilterId, @PropertyFilterEntityId, '
[
  ""StartDateTime"",
  ""1024"",
  ""CURRENT""
]', NEWID() )

			INSERT INTO [DataViewFilter] ( [ExpressionType], [ParentId], [EntityTypeId], [Selection], [Guid] ) 
			VALUES ( 0, @FilterId, @PropertyFilterEntityId, '
[
  ""ExpireDateTime"",
  ""128"",
  ""CURRENT""
]', NEWID() )

			INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
			VALUES ( 0, @AttributeId, @BlockId, CAST( @FilterId as varchar ), NEWID())
	
		END

		SET @BlockId = (SELECT [Id] FROM [Block] WHERE [Guid] = '2E0FFD29-B4AF-4A5E-B528-667168762ABC')
		IF @BlockId IS NOT NULL 
		BEGIN

			INSERT INTO [DataViewFilter] ( [ExpressionType], [Guid] ) 
			VALUES ( 1, NEWID() )
			SET @FilterId = SCOPE_IDENTITY()

			-- If 'sub-promotions' audience exists, create a data filter for the second context item block on home page
			IF EXISTS ( SELECT [Id] FROM [DefinedValue] WHERE [Guid] = 'b364cdee-f000-4965-ae67-0c80dda365dc' )
			BEGIN

				INSERT INTO [DataViewFilter] ( [ExpressionType], [ParentId], [EntityTypeId], [Selection], [Guid] ) 
				VALUES ( 0, @FilterId, @PropertyFilterEntityId, '
[
	""PrimaryAudience"",
	""[\r\n  \""57b2a23f-3b0c-43a8-9f45-332120dcd0ee\""\r\n]""
]', NEWID() )
	
			END

			INSERT INTO [DataViewFilter] ( [ExpressionType], [ParentId], [EntityTypeId], [Selection], [Guid] ) 
			VALUES ( 0, @FilterId, @PropertyFilterEntityId, '
[
  ""StartDateTime"",
  ""1024"",
  ""CURRENT""
]', NEWID() )

			INSERT INTO [DataViewFilter] ( [ExpressionType], [ParentId], [EntityTypeId], [Selection], [Guid] ) 
			VALUES ( 0, @FilterId, @PropertyFilterEntityId, '
[
  ""ExpireDateTime"",
  ""128"",
  ""CURRENT""
]', NEWID() )

			INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
			VALUES ( 0, @AttributeId, @BlockId, CAST( @FilterId as varchar ), NEWID())

		END

	END

    -- Update the content item detail lava to check for empty image
    SET @BlockId = ( SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = '7173AA95-15AF-49C5-933D-004717A3FF3C' )
    SET @AttributeId = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '8026FEA1-35C1-41CF-9D09-E8B1DB6CBDA8' )
    IF @BlockId IS NOT NULL AND @AttributeId IS NOT NULL 
    BEGIN
        DECLARE @Value varchar(max) =  ( SELECT TOP 1 [Value] FROM [AttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = @BlockId )
		IF @Value = '
{% for item in Items %}
    <img src=""/GetImage.ashx?Guid={{ item.DetailImage_unformatted }}"" class=""title-image""><h1>{{ item.Title }}</h1>{{ item.Content }}
{% endfor -%}
'		BEGIN
			UPDATE [AttributeValue] 
			SET [Value] = '
{% for item in Items %}
    {% if item.DetailImage_unformatted != '''' %}
        <img src=""/GetImage.ashx?Guid={{ item.DetailImage_unformatted }}"" class=""title-image"">
    {% endif %}
    <h1>{{ item.Title }}</h1>{{ item.Content }}
{% endfor -%}
'
			WHERE [AttributeId] = @AttributeId AND [EntityId] = @BlockId 
		END
	END


" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AlterColumn("dbo.ContentChannel", "ItemUrl", c => c.String());
            AlterColumn("dbo.ContentChannel", "ChannelUrl", c => c.String());
            DropColumn("dbo.ContentChannelType", "DisablePriority");
            DropColumn("dbo.ContentChannel", "RootImageDirectory");
            DropColumn("dbo.ContentChannel", "ContentControlType");
        }
    }
}
