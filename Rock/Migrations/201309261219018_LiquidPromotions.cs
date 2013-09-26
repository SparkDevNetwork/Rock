//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class LiquidPromotions : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
DECLARE @BlockTypeId int
SET @BlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '5A880084-7237-449A-9855-3FA02B6BD79F')

DELETE [BlockType]
WHERE [Path] = '~/Blocks/Cms/MarketingCampaignAds.ascx'

UPDATE [BlockType] SET
	[Name] = 'Cms - Marketing Campaign Ads',
	[Path] = '~/Blocks/Cms/MarketingCampaignAds.ascx'
WHERE [Id] = @BlockTypeId

DECLARE @AttributeId int
SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'D4B3F1F1-0714-44EF-B569-A981A8DAEF97')

DECLARE @MemoFieldId int
SET @MemoFieldId = (SELECT [Id] FROM [FieldType] WHERE [Guid] = 'C28C7BF3-A552-4D77-9408-DEDCF760CED0')

UPDATE [Attribute] SET
	[FieldTypeId] = @MemoFieldId,
	[Name] = 'Template',
	[Key] = 'Template',
	[Description] = 'The liquid template to use for rendering',
	[DefaultValue] = '
<div class=''ad-list''>
    {% include ''AdList'' with Ads %}
</div>
'
WHERE [Id] = @AttributeId
	
UPDATE AV SET
	[Value] = CASE B.Zone
		WHEN 'PromotionRotator' THEN '
<section class=""promo-slider"">
    <div class=""flexslider"">
        <ul class=""slides"">
        {% for Ad in Ads %}
            <li>
                <a href=""{{ Ad.DetailPageUrl }}"">
                {% for Attribute in Ad.Attributes %}
                    {% if Attribute.Key == ''PromotionImage'' %}
                        {{ Attribute.Value }}
                    {% endif %}
                {% endfor %}
                </a>
            </li>
        {% endfor %}
        </ul>
    </div>
    <img class=""slider-shadow"" src=""{{ ApplicationPath }}Themes/GrayFabric/Assets/Images/slider-shadow.png""/>
</section>'
		WHEN 'PromotionList' THEN '
<section class=""promo-secondary container"">
    <ul>
    {% for Ad in Ads %}
        <li>
            <a href=""{{ Ad.DetailPageUrl }}"">
            {% for Attribute in Ad.Attributes %}
                {% if Attribute.Key == ''PromotionImage'' %}
                    {{ Attribute.Value }}
                {% endif %}
            {% endfor %}
            </a>
        </li>
    {% endfor %}
    </ul>
</section>'
		END
FROM [AttributeValue] AV
INNER JOIN [Block] B
	ON B.[Id] = AV.[EntityId]
WHERE AV.[AttributeId] = @AttributeId
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
DECLARE @BlockTypeId int
SET @BlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '5A880084-7237-449A-9855-3FA02B6BD79F')

DELETE [BlockType]
WHERE [Path] = '~/Blocks/Cms/MarketingCampaignAdsXslt.ascx'

UPDATE [BlockType] SET
	[Name] = 'Cms - Marketing Campaign Ads - Xslt',
	[Path] = '~/Blocks/Cms/MarketingCampaignAdsXslt.ascx'
WHERE [Id] = @BlockTypeId

DECLARE @AttributeId int
SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'D4B3F1F1-0714-44EF-B569-A981A8DAEF97')

DECLARE @TextFieldId int
SET @TextFieldId = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '9C204CD0-1233-41C5-818A-C5DA439445AA')

UPDATE [Attribute] SET
	[FieldTypeId] = @TextFieldId,
	[Name] = 'XSLT File',
	[Key] = 'XSLTFile',
	[Description] = 'The path to the XSLT File ',
	[DefaultValue] = '~/Assets/XSLT/AdList.xslt'
WHERE [Id] = @AttributeId
	
UPDATE AV SET
	[Value] = CASE B.Zone
		WHEN 'PromotionRotator' THEN '~/Themes/GrayFabric/Assets/XSLT/HomepagePromotionRotator.xslt'
		WHEN 'PromotionList' THEN '~/Themes/GrayFabric/Assets/XSLT/HomepagePromotionList.xslt'
		END
FROM [AttributeValue] AV
INNER JOIN [Block] B
	ON B.[Id] = AV.[EntityId]
WHERE AV.[AttributeId] = @AttributeId
" );
        }
    }
}
