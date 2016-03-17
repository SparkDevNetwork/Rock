using System;
using System.Collections.Generic;
using System.Linq;
using Rock;
using Rock.Attribute;
using Rock.Web.Cache;
using UAParser;

namespace church.ccv.Web.Cms
{

    /// <summary>
    /// CCV abstract base Content Block that takes care of loading merge fields, processing lava, and caching
    /// </summary>
    /// <seealso cref="Rock.Web.UI.RockBlock" />
    [BooleanField( "Show Debug", "Show Lava Objects and Help", order: 104 )]
    [IntegerField( "Cache Duration", "Number of seconds to cache the content. Set to 0 to disable caching.", false, 3600, "", order: 105 )]
    [TextField( "Cache Key", "Additional CacheKey to use when caching the content using Context Lava Merge Fields. For example: <pre>Campus={{ Context.Campus.Guid }}</pre>", required: false, order: 106 )]

    [CustomCheckboxListField(
        "Include MergeFields",
        "Select the Merge Fields that this block needs. Selecting only what you need can improve performance. Note that GlobalAttributes is only needed if you use the old v1 syntax.",
        "GlobalAttributes,DeviceFamily,OSFamily,Context,CurrentPerson,PageParameter,Campuses",
        false,
        "DeviceFamily,OSFamily,Context,CurrentPerson,PageParameter,Campuses",
        order: 107 )]
    public abstract class BaseContentBlock : Rock.Web.UI.RockBlock
    {
        private bool _flushCache = false;

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            this.BlockUpdated += BaseContentBlock_BlockUpdated;
        }

        /// <summary>
        /// Handles the BlockUpdated event of the BaseContentBlock control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void BaseContentBlock_BlockUpdated( object sender, EventArgs e )
        {
            _flushCache = true;
            ShowContent();
        }

        /// <summary>
        /// Flushes the cached content for this Content Block
        /// </summary>
        public void FlushCachedContent()
        {
            _flushCache = true;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !this.IsPostBack )
            {
                ShowContent();
            }
        }

        /// <summary>
        /// Implement to show the content. For example, simply have it do "lContent.Text = this.GetContentHtml();"
        /// </summary>
        public abstract void ShowContent();

        /// <summary>
        /// Automatically gets the HTML Content either from the Cache or from processing the ContentTemplate
        /// </summary>
        public virtual string GetContentHtml()
        {
            string result = string.Empty;
            try
            {
                var mergeFields = this.GetContentMergeFields();

                var cacheKey = this.GetAttributeValue( "CacheKey" ) ?? string.Empty;
                cacheKey = cacheKey.ResolveMergeFields( mergeFields );

                if ( _flushCache )
                {
                    this.FlushCacheItem( cacheKey );
                    _flushCache = false;
                }

                var cachedContent = this.GetCacheItem( cacheKey ) as string;
                if ( string.IsNullOrEmpty( cachedContent ) )
                {

                    var template = GetContentTemplate();

                    if ( this.GetAttributeValue( "ShowDebug" ).AsBoolean() && this.IsUserAuthorized( Rock.Security.Authorization.EDIT ) )
                    {
                        result = mergeFields.lavaDebugInfo();
                    }
                    else
                    {
                        result = template.ResolveMergeFields( mergeFields );
                        var cacheDuration = this.GetAttributeValue( "CacheDuration" ).AsInteger();
                        if ( cacheDuration > 0 )
                        {
                            this.AddCacheItem( cacheKey, result, cacheDuration );
                        }
                    }
                }
                else
                {
                    result = cachedContent;
                }
            }
            catch ( Exception ex )
            {
                LogException( ex );
                result = string.Format( "<div class='alert alert-danger'>{0}</div>", ex.Message );
            }

            return result;
        }

        /// <summary>
        /// Gets the content template.
        /// </summary>
        /// <returns></returns>
        public abstract string GetContentTemplate();

        /// <summary>
        /// Gets the common content merge fields (Context, PageParameters, GlobalAttributes (if enabled), etc)
        /// </summary>
        /// <returns></returns>
        public virtual Dictionary<string, object> GetContentMergeFields()
        {
            Dictionary<string, object> mergeFields;
            var includeMergeFields = this.GetAttributeValue( "IncludeMergeFields" ).SplitDelimitedValues().ToList();
            if ( includeMergeFields.Contains( "GlobalAttributes" ) )
            {
                mergeFields = Rock.Web.Cache.GlobalAttributesCache.GetMergeFields( CurrentPerson );
            }
            else
            {
                mergeFields = new Dictionary<string, object>();
            }

            if ( includeMergeFields.Contains( "Context" ) )
            {
                var contextObjects = new Dictionary<string, object>();
                foreach ( var contextEntityType in RockPage.GetContextEntityTypes() )
                {
                    var contextEntity = RockPage.GetCurrentContext( contextEntityType );
                    if ( contextEntity != null && contextEntity is DotLiquid.ILiquidizable )
                    {
                        var type = Type.GetType( contextEntityType.AssemblyName ?? contextEntityType.Name );
                        if ( type != null )
                        {
                            contextObjects.Add( type.Name, contextEntity );
                        }
                    }
                }

                if ( contextObjects.Any() )
                {
                    mergeFields.Add( "Context", contextObjects );
                }
            }

            if ( includeMergeFields.Contains( "PageParameter" ) )
            {
                mergeFields.Add( "PageParameter", PageParameters() );
            }

            if ( includeMergeFields.Contains( "DeviceFamily" ) || includeMergeFields.Contains( "OSFamily" ) )
            {
                if ( !string.IsNullOrEmpty( this.Request.UserAgent ) )
                {
                    Parser uaParser = Parser.GetDefault();
                    ClientInfo client = uaParser.Parse( this.Request.UserAgent );
                    mergeFields.Add( "DeviceFamily", client.Device.Family );
                    mergeFields.Add( "OSFamily", client.OS.Family.ToLower() );
                }
            }

            if ( includeMergeFields.Contains( "CurrentPerson" ) )
            {
                if ( CurrentPerson != null )
                {
                    mergeFields.Add( "CurrentPerson", CurrentPerson );
                }
            }

            if ( includeMergeFields.Contains( "Campuses" ) )
            {
                mergeFields.Add( "Campuses", CampusCache.All() );
            }

            return mergeFields;
        }
    }
}
