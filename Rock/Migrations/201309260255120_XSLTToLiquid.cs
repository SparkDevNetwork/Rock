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
    public partial class XSLTToLiquid : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddBlockType( "Page Menu", "Block to render a page menu using Liquid templating syntax", "~/Blocks/Cms/PageLiquid.ascx", "CACB9D1A-A820-4587-986A-D66A69EE9948" );
            AddBlockTypeAttribute("CACB9D1A-A820-4587-986A-D66A69EE9948","9C204CD0-1233-41C5-818A-C5DA439445AA","CSS File","CSSFile","","Optional CSS file to add to the page for styling.",0,"","7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22");
            AddBlockTypeAttribute("CACB9D1A-A820-4587-986A-D66A69EE9948","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Include Current Parameters","IncludeCurrentParameters","","Flag indicating if current page's parameters should be used when building url for child pages",0,"False","EEE71DDE-C6BC-489B-BAA5-1753E322F183");
            AddBlockTypeAttribute("CACB9D1A-A820-4587-986A-D66A69EE9948","C28C7BF3-A552-4D77-9408-DEDCF760CED0","Template","Template","","The liquid template to use for rendering",0,@"
<ul>
    {% include ''PageMenu'' with page.pages %}
</ul>
","1322186A-862A-4CF1-B349-28ECB67229BA");
            AddBlockTypeAttribute("CACB9D1A-A820-4587-986A-D66A69EE9948","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Root Page","RootPage","","The root page to use for the page collection. Defaults to the current page instance if not set.",0,"","41F1C42E-2395-4063-BD4F-031DF8D5B231");
            AddBlockTypeAttribute("CACB9D1A-A820-4587-986A-D66A69EE9948","9C204CD0-1233-41C5-818A-C5DA439445AA","Number of Levels","NumberofLevels","","Number of parent-child page levels to display. Default 3.",0,"3","6C952052-BC79-41BA-8B88-AB8EA3E99648");
            AddBlockTypeAttribute("CACB9D1A-A820-4587-986A-D66A69EE9948","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Include Current QueryString","IncludeCurrentQueryString","","Flag indicating if current page's QueryString should be used when building url for child pages",0,"False","E4CF237D-1D12-4C93-AFD7-78EB296C4B69");

            Sql( @"
    DECLARE @NewBlockTypeId int
    SET @NewBlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = 'CACB9D1A-A820-4587-986A-D66A69EE9948');

    DECLARE @OldBlockTypeId int
    SET @OldBlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = 'F49AD5F8-1E45-41E7-A88E-8CD285815BD9');

    UPDATE [Block]
    SET [BlockTypeId] = @NewBlockTypeId
    WHERE [BlockTypeId] = @OldBlockTypeId

	UPDATE AV SET 
		[AttributeId] = NA.[Id],
		[Value] = CASE WHEN OA.[Key] <> 'XSLTFile' THEN AV.[Value] ELSE
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
			END
	FROM [Attribute] OA
	INNER JOIN [Attribute] NA
		ON NA.EntityTypeQualifierColumn = 'BlockTypeId'
		AND NA.EntityTypeQualifierValue = @NewBlockTypeId
		AND (NA.[Key] = OA.[Key]
			OR (NA.[Key] = 'Template' AND OA.[Key] = 'XSLTFile'))
	INNER JOIN [AttributeValue] AV
		ON AV.AttributeId = OA.[Id]
	INNER JOIN [Block] B
		ON B.[Id] = AV.EntityId
	WHERE OA.EntityTypeQualifierColumn = 'BlockTypeId'
	AND OA.EntityTypeQualifierValue = @OldBlockTypeId

" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
    DECLARE @NewBlockTypeId int
    SET @NewBlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = 'CACB9D1A-A820-4587-986A-D66A69EE9948');

    DECLARE @OldBlockTypeId int
    SET @OldBlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = 'F49AD5F8-1E45-41E7-A88E-8CD285815BD9');

    UPDATE [Block]
    SET [BlockTypeId] = @OldBlockTypeId
    WHERE [BlockTypeId] = @NewBlockTypeId

	UPDATE AV SET 
		[AttributeId] = OA.[Id],
		[Value] = CASE WHEN NA.[Key] <> 'Template' THEN AV.[Value] ELSE
			CASE B.Zone
				WHEN 'Content' THEN '~/Assets/XSLT/PageListAsBlocks.xslt'
				WHEN 'Menu' THEN '~/Assets/XSLT/PageNav.xslt'
				WHEN 'Navigation' THEN '~/Themes/GrayFabric/Assets/XSLT/PrimaryNavigation.xslt'
				WHEN 'TabsZone' THEN '~/Themes/RockChMS/Assets/Xslt/SubPageNav.xslt'
			END
			END
	FROM [Attribute] NA
	INNER JOIN [Attribute] OA
		ON OA.EntityTypeQualifierColumn = 'BlockTypeId'
		AND OA.EntityTypeQualifierValue = @OldBlockTypeId
		AND (OA.[Key] = NA.[Key]
			OR (NA.[Key] = 'Template' AND OA.[Key] = 'XSLTFile'))
	INNER JOIN [AttributeValue] AV
		ON AV.AttributeId = NA.[Id]
	INNER JOIN [Block] B
		ON B.[Id] = AV.EntityId
	WHERE NA.EntityTypeQualifierColumn = 'BlockTypeId'
	AND NA.EntityTypeQualifierValue = @NewBlockTypeId

" );

            DeleteAttribute("7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22");    // Rock.Model.Block: CSS File
            DeleteAttribute("EEE71DDE-C6BC-489B-BAA5-1753E322F183");    // Rock.Model.Block: Include Current Parameters
            DeleteAttribute("1322186A-862A-4CF1-B349-28ECB67229BA");    // Rock.Model.Block: Template
            DeleteAttribute("41F1C42E-2395-4063-BD4F-031DF8D5B231");    // Rock.Model.Block: Root Page
            DeleteAttribute("6C952052-BC79-41BA-8B88-AB8EA3E99648");    // Rock.Model.Block: Number of Levels
            DeleteAttribute("E4CF237D-1D12-4C93-AFD7-78EB296C4B69");    // Rock.Model.Block: Include Current QueryString

            DeleteBlockType( "CACB9D1A-A820-4587-986A-D66A69EE9948" );
        }
    }
}
