//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.Caching;
using System.Web.UI;

using DotLiquid;

using Rock;
using Rock.Attribute;

namespace RockWeb.Blocks.Cms
{
    [MemoField( "Template", "The liquid template to use for rendering", true, @"
<ul>
    {% include 'PageMenu' with page.pages %}
</ul>
" )]
    [LinkedPage( "Root Page", "The root page to use for the page collection. Defaults to the current page instance if not set.", false, "" )]
    [TextField( "Number of Levels", "Number of parent-child page levels to display. Default 3.", false, "3" )]
    [TextField("CSS File", "Optional CSS file to add to the page for styling.", false, "")]
    [BooleanField( "Include Current Parameters", "Flag indicating if current page's parameters should be used when building url for child pages", false )]
    [BooleanField( "Include Current QueryString", "Flag indicating if current page's QueryString should be used when building url for child pages", false )]

    public partial class PageLiquid : Rock.Web.UI.RockBlock
    {
        private static readonly string ROOT_PAGE = "RootPage";
        private static readonly string NUM_LEVELS = "NumberofLevels";

        protected override void OnInit( EventArgs e )
        {
            this.EnableViewState = false;

            base.OnInit( e );

            this.AttributesUpdated += PageLiquid_AttributesUpdated;

            // add css file to page
            if (GetAttributeValue( "CSSFile" ).Trim() != string.Empty)
                CurrentPage.AddCSSLink( Page, ResolveUrl("~/CSS/jquery.tagsinput.css"));  //todo why is this hardcoding? JME

            // add css file to page
            if (GetAttributeValue( "CSSFile" ).Trim() != string.Empty)
                CurrentPage.AddCSSLink( Page, ResolveUrl("~/CSS/jquery.tagsinput.css"));

            Render();

        }

        void PageLiquid_AttributesUpdated( object sender, EventArgs e )
        {
            ObjectCache cache = MemoryCache.Default;
            cache.Remove( CacheKey() );

            Render();
        }

        private void Render()
        {
            Rock.Web.Cache.PageCache rootPage = null;

            Guid pageGuid = Guid.Empty;
            if (Guid.TryParse(GetAttributeValue(ROOT_PAGE), out pageGuid))
            {
                rootPage = Rock.Web.Cache.PageCache.Read(pageGuid);
            }

            if (rootPage == null)
            {
                rootPage = CurrentPage;
            }

            int levelsDeep = Convert.ToInt32(GetAttributeValue(NUM_LEVELS));

            Dictionary<string, string> pageParameters = null;
            if (GetAttributeValue("IncludeCurrentParameters").AsBoolean())
            {
                pageParameters = CurrentPageReference.Parameters;
            }

            NameValueCollection queryString = null;
            if (GetAttributeValue("IncludeCurrentQueryString").AsBoolean())
            {
                queryString = CurrentPageReference.QueryString;
            }

            var pageProperties = new Dictionary<string, object>();
            pageProperties.Add( "page", rootPage.GetMenuProperties( levelsDeep, CurrentPerson, CurrentPage, pageParameters, queryString ) );
            string content = GetTemplate().Render( Hash.FromDictionary( pageProperties ) );

            phContent.Controls.Clear();
            phContent.Controls.Add(new LiteralControl(content));

        }

        private string CacheKey()
        {
            return string.Format( "Rock:PageLiquid:{0}", CurrentBlock.Id );
        }

        private Template GetTemplate()
        {
            string cacheKey = CacheKey();

            ObjectCache cache = MemoryCache.Default;
            Template template = cache[cacheKey] as Template;

            if ( template != null )
            {
                return template;
            }
            else
            {
                Template.NamingConvention = new DotLiquid.NamingConventions.CSharpNamingConvention();
                //TODO: This should probably use the theme assets folder
                Template.FileSystem = new DotLiquid.FileSystems.LocalFileSystem( System.Web.HttpContext.Current.Server.MapPath( "~/Assets/Liquid" ) );
                template = Template.Parse( GetAttributeValue( "Template" ) );

                var cachePolicy = new CacheItemPolicy();
                cache.Set( cacheKey, template, cachePolicy );

                return template;
            }
        }
    }
}