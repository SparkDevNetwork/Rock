using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.ServiceModel.Syndication;
using System.Runtime.Caching;
using System.Web.UI;

using DotLiquid;

using Rock;
using Rock.Attribute;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Web;

namespace RockWeb.Blocks.Cms
{

    [DisplayName("RSS Feed")]
    [Category("CMS")]
    [Description("Gets and consumes and RSS Feed. The feed is rendered based on a provided liquid template. ")]
    [TextField("RSS Feed Url", "The Url of the RSS Feed to retrieve and consume", true, "", "Feed")]
    [IntegerField("Results per page", "How many results/articles to display on the page at a time. Default is 10.", false, 10, "Feed")]
    [IntegerField("Cache Duration", "The length of time (in minutes) that the RSS Feed data is stored in cache. If this value is 0, the feed will not be cached. Default is 20 minutes", false, 20, "Feed")]
    [TextField("CSS File", "An optional CSS file to add to the page for styling. Example \"Styles/rss.css\" would point to the stylesheet in the current theme's styles folder.", false, "", "Layout")]
    [CodeEditorField("Template", "The liquid template to use for rendering. This template should be in the theme's \"Assets/Liquid\" folder and should have an underscore prepended to the filename.", 
        CodeEditorMode.Liquid, CodeEditorTheme.Rock, 200, true, @"{% include 'RSSFeed' %}", "Layout")]
    [LinkedPage("Detail Page")]
    public partial class RSSFeed : RockBlock
    {
        #region Private Properties

        private string TemplateCacheKey
        {
            get
            {
                return string.Format( "Rock:Template:{0}", BlockId );
            }
        }
        #endregion

        #region Control Methods
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            BlockUpdated += RSSFeed_BlockUpdated;
            AddConfigurationUpdateTrigger( upContent );    
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            SetNotificationBox( String.Empty, String.Empty );

            if(!Page.IsPostBack)
            {
                LoadFeed();
            }
        }
        #endregion

        #region Page Events 
        protected void RSSFeed_BlockUpdated( object sender, EventArgs e )
        {
            ClearCache();
            pnlContent.Visible = false;
            LoadFeed();
        }
        #endregion

        #region Internal Methods
        private void ClearCache()
        {
            ObjectCache cache = MemoryCache.Default;
            SyndicationFeedHelper.ClearCachedFeed( GetAttributeValue( "RSSFeedUrl" ) );
            cache.Remove( TemplateCacheKey );
        }

        private Template GetTemplate()
        {
            string liquidFolder = System.Web.HttpContext.Current.Server.MapPath( ResolveRockUrl( "~~/Assets/Liquid" ) );
            Template.NamingConvention = new DotLiquid.NamingConventions.CSharpNamingConvention();
            Template.FileSystem = new DotLiquid.FileSystems.LocalFileSystem( liquidFolder );

            ObjectCache cache = MemoryCache.Default;
            Template template = null;

            if ( cache[TemplateCacheKey] != null )
            {
                template = (Template)cache[TemplateCacheKey];
            }
            else
            {
                template = Template.Parse( GetAttributeValue( "Template" ) );
                cache.Set( TemplateCacheKey, template, new CacheItemPolicy() );
            }

            return template;
        }

        private void LoadFeed()
        {

            string feedUrl = GetAttributeValue( "RSSFeedUrl" );

            Dictionary<string, string> messages = new Dictionary<string, string>();
            bool isError = false;

            try
            {

                Dictionary<string, object> feedDictionary = SyndicationFeedHelper.GetFeed( feedUrl, GetAttributeValue( "DetailPage" ), (int)GetAttributeValue( "CacheDuration" ).AsInteger(true), ref messages, ref isError );

                if ( feedDictionary != null )
                {

                    string content = GetTemplate().Render( Hash.FromDictionary( feedDictionary ) );

                    if ( content.Contains( "No such template" ) )
                    {
                        System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match( GetAttributeValue( "Template" ), @"'([^']*)" );
                        if ( match.Success )
                        {
                            messages.Add( "Warning", string.Format( "Could not find the template _{0}.liquid in {1}.", match.Groups[1].Value, ResolveRockUrl( "~~/Assets/Liquid" ) ) );
                            isError = true;
                        }
                        else
                        {
                            messages.Add( "Warning", "Unable to parse the template name from settings." );
                            isError = true;

                        }
                    }
                    else
                    {
                        phRSSFeed.Controls.Clear();
                        phRSSFeed.Controls.Add( new LiteralControl( content ) );
                        
                    }

                    pnlContent.Visible = true;
                }


            }
            catch ( Exception ex )
            {
                if ( IsUserAuthorized( "Administrate" ) )
                {
                    throw ex;
                }
            }

            if ( messages.Count > 0 )
            {
                if ( IsUserAuthorized( "Administrate" ) )
                {
                    SetNotificationBox( messages.FirstOrDefault().Key, messages.FirstOrDefault().Value, isError ? NotificationBoxType.Warning : NotificationBoxType.Info );
                }
                else
                {
                    SetNotificationBox( "Content not available", "Oops. The requested content is not currently available. Please try again later." );
                }
            }

        }

        private void SetNotificationBox( string heading, string bodyText )
        {
            SetNotificationBox( heading, bodyText, NotificationBoxType.Info );
        }

        private void SetNotificationBox( string heading, string bodyText, NotificationBoxType boxType )
        {
            nbRSSFeed.Heading = heading;
            nbRSSFeed.Text = bodyText;
            nbRSSFeed.NotificationBoxType = boxType;

            nbRSSFeed.Visible = !( String.IsNullOrWhiteSpace( heading ) || String.IsNullOrWhiteSpace( bodyText ) );
        }

        #endregion
    }

  
}