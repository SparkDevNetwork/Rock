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
    public partial class MoveLiquidToTheme : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
    DECLARE @AttributeId int

    -- Page Menu
    SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '1322186A-862A-4CF1-B349-28ECB67229BA');
	UPDATE AV SET
		[Value] = 
			CASE B.Zone
				WHEN 'Content' THEN '{% include ''PageListAsBlocks'' %}' 
				WHEN 'Menu' THEN '{% include ''PageMenu'' %}'
				WHEN 'Navigation' THEN '{% include ''PageMenu'' %}'
				WHEN 'TabsZone' THEN '{% include ''PageListAsTabs'' %}'
			END
	FROM [AttributeValue] AV
	INNER JOIN [Block] B
		ON B.[Id] = AV.EntityId
	WHERE AV.[AttributeID] = @AttributeId 

    -- Promotions
    SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'D4B3F1F1-0714-44EF-B569-A981A8DAEF97');
	UPDATE AV SET
		[Value] = 
			CASE B.Zone
				WHEN 'PromotionRotator' THEN '{% include ''PromoSlider'' %}' 
				WHEN 'PromotionList' THEN '{% include ''PromoList'' %}'
			END
	FROM [AttributeValue] AV
	INNER JOIN [Block] B
		ON B.[Id] = AV.EntityId
	WHERE AV.[AttributeID] = @AttributeId

" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
    DECLARE @AttributeId int

    -- Page Menu
    SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '1322186A-862A-4CF1-B349-28ECB67229BA');
	UPDATE AV SET
		[Value] = 
			CASE B.Zone
				WHEN 'Content' THEN '
<div class=""panel panel-default page-list-as-blocks clearfix"">
    <div class=""panel-body"">
        <ul>
            {% for childPage in page.pages %}
                <li>
                    <a href=""{{ childPage.id }}"" {% if childPage.display-description != ''true'' %} title=""{{ childPage.description }}""{% endif %}>
                        {% if childPage.icon-css-class != '' %}
                            <i class=""{{ childPage.icon-css-class }} icon-large""></i>
                        {% endif %}
                        <h3>{{ childPage.title }}</h3>
                    </a>
                </li>
            {% endfor %}
        </ul>
    </div>
</div>
' 
				WHEN 'Menu' THEN '
{% if page.display-child-pages == ''true'' && page.pages != empty %}
    <ul class=""nav navbar-nav"">
        {% for topPage in page.pages %}
            <li class=""dropdown pagenav-item"">
                <a class=""dropdown-toggle"" data-toggle=""dropdown"" href=""#"">
                    <i class=""{{ topPage.icon-css-class }}""></i>
                    {{ topPage.title }}
                    <b class=""caret""></b>
                </a>
                {% if topPage.display-child-pages == ''true'' && topPage.pages != empty %}
                    <ul class=""dropdown-menu"" role=""menu"">
                        {% for sectionPage in topPage.pages %}
                            <li class=""dropdown-header"">{{ sectionPage.title }}</li>
                            {% if sectionPage.display-child-pages == ''true'' %}
                                {% for childpage in sectionPage.pages %}
                                    <li role=""presentation"">
                                        <a role=""menu-item"" href=""{{ childpage.url }}"">{{ childpage.title }}</a>
                                    </li>
                                {% endfor %}
                                <li class=""divider""></li>
                            {% endif %}
                        {% endfor %}
                    </ul>
                {% endif %}
            </li>
        {% endfor %}
    </ul>
{% endif %}
'
				WHEN 'Navigation' THEN '
<div class=""navbar"">
    <div class=""navbar-header"">
        <a class=""btn btn-navbar"" data-toggle=""collapse"" data-target="".nav-collapse"">
            <span class=""icon-bar""></span>
            <span class=""icon-bar""></span>
            <span class=""icon-bar""></span>
        </a>
    </div>
    <div class=""navbar-collapse collapse"">
        <ul class=""navbar-nav nav"">
            {% for childPage in page.pages %}
                <li {% if childPage.current == ''True'' %}class=''active''{% endif %}>
                    <a href=""{{ childPage.url }}"">{{ childPage.title }}</a>
                </li>
            {% endfor %}
        </ul>
    </div>
</div>'
				WHEN 'TabsZone' THEN '
<ul class=""nav nav-pills"">
    {% for childPage in page.pages %}
        <li {% if childPage.current == ''True'' %}class=''active''{% endif %}>
            <a href=""{{ childPage.url }}"">{{ childPage.title }}</a>
        </li>
    {% endfor %}
</ul>
'
			END
	FROM [AttributeValue] AV
	INNER JOIN [Block] B
		ON B.[Id] = AV.EntityId
	WHERE AV.[AttributeID] = @AttributeId 

    SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'D4B3F1F1-0714-44EF-B569-A981A8DAEF97');
	UPDATE AV SET
		[Value] = 
			CASE B.Zone
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
</section>
' 
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
</section>
'
			END
	FROM [AttributeValue] AV
	INNER JOIN [Block] B
		ON B.[Id] = AV.EntityId
	WHERE AV.[AttributeID] = @AttributeId

" );
        }
    }
}
