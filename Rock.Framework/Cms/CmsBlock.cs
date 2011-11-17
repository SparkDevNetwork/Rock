using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock.Cms.Security;
using Rock.Models.Cms;

namespace Rock.Cms
{
    /// <summary>
    /// CmsBlock is the base abstract class that all Blocks should inherit from
    /// </summary>
    public abstract class CmsBlock : System.Web.UI.UserControl
    {
        #region Events

        public event EventHandler<EventArgs> AttributesUpdated;

        #endregion

        #region Public Properties

        /// <summary>
        /// The current page instance.  This value is read and cached by the RockRouteHandler
        /// and set by the layout's base class (CmsPage) when loading the block instance
        /// </summary>
        public Rock.Cms.Cached.Page PageInstance { get; set; }

        /// <summary>
        /// The current block instance.  This value is read and cached by the layout's 
        /// base class (CmsPage) when loading the block instance
        /// </summary>
        public Rock.Cms.Cached.BlockInstance BlockInstance { get; set; }

        /// <summary>
        /// The personID of the currently logged in user.  If user is not logged in, returns null
        /// </summary>
        public int? CurrentPersonId
        {
            get { return ( ( CmsPage )this.Page ).CurrentPersonId; }
        }

        /// <summary>
        /// Returns the currently logged in user.  If user is not logged in, returns null
        /// </summary>
        public MembershipUser CurrentUser
        {
            get { return ( ( CmsPage )this.Page ).CurrentUser; }
        }


        /// <summary>
        /// Returns the currently logged in person. If user is not logged in, returns null
        /// </summary>
        public Rock.Models.Crm.Person CurrentPerson
        {
            get { return ( ( CmsPage )this.Page ).CurrentPerson; }
        }

        /// <summary>
        /// Relative path to the current theme and layout folder.  Useful for setting paths to
        /// theme resource files
        /// <example>
        /// Client Side: <img src='<%= ThemePath %>/Images/avatar.gif' />
        /// Server Side: myImg.ImageUrl = ThemePath + "/Images/avatar.gif";
        /// </example>
        /// </summary>
        public string ThemePath
        {
            get { return ( ( CmsPage )this.Page ).ThemePath; }
        }

        #endregion

        #region Protected Caching Methods

        /// <summary>
        /// Adds an object to the default MemoryCache.
        /// </summary>
        /// <param name="value">Object to cache</param>
        protected virtual void AddCacheItem( object value )
        {
            CacheItemPolicy cacheItemPolicy = null;
            AddCacheItem( string.Empty, value, cacheItemPolicy );
        }

        /// <summary>
        /// Adds an object to the default MemoryCache.
        /// </summary>
        /// <param name="key">Key to differentiate items from same block instance</param>
        /// <param name="value">Object to cache</param>
        protected virtual void AddCacheItem( string key, object value )
        {
            CacheItemPolicy cacheItemPolicy = null;
            AddCacheItem( key, value, cacheItemPolicy );
        }

        /// <summary>
        /// Adds an object to the default MemoryCache.
        /// </summary>
        /// <param name="key">Key to differentiate items from same block instance</param>
        /// <param name="value">Object to cache</param>
        /// <param name="seconds">The Number of seconds to cache object for</param>
        protected virtual void AddCacheItem( string key, object value, int seconds )
        {
            CacheItemPolicy cacheItemPolicy = new CacheItemPolicy();
            cacheItemPolicy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds( seconds );
            AddCacheItem( key, value, cacheItemPolicy );
        }

        /// <summary>
        /// Adds an object to the default MemoryCache.
        /// </summary>
        /// <param name="key">Key to differentiate items from same block instance</param>
        /// <param name="value">Object to cache</param>
        /// <param name="cacheItemPolicy">Optional CacheItemPolicy, defaults to null</param>
        protected virtual void AddCacheItem( string key, object value, CacheItemPolicy cacheItemPolicy )
        {
            ObjectCache cache = MemoryCache.Default;
            cache.Set( ItemCacheKey( key ), value, cacheItemPolicy );
        }

        /// <summary>
        /// Retrieve an object from the cache
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected virtual object GetCacheItem( string key = "" )
        {
            ObjectCache cache = MemoryCache.Default;
            return cache[ItemCacheKey( key )];
        }

        /// <summary>
        /// Flush an object from the cache
        /// </summary>
        /// <param name="key"></param>
        protected virtual void FlushCacheItem( string key = "" )
        {
            ObjectCache cache = MemoryCache.Default;
            cache.Remove( ItemCacheKey( key ) );
        }

        private string ItemCacheKey( string key )
        {
            return string.Format( "Rock:Page:{0}:Block:{1}:ItemCache:{2}",
                this.PageInstance.Id, BlockInstance.Id, key );
        }

        public void AddAttributeUpdateTrigger( UpdatePanel updatePanel )
        {
            if ( BlockInstance.Authorized( "Configure", CurrentUser ) )
            {
                AsyncPostBackTrigger trigger = new AsyncPostBackTrigger();
                trigger.ControlID = string.Format( "blck-cnfg-trggr-{0}", BlockInstance.Id );
                trigger.EventName = "Click";
                updatePanel.Triggers.Add( trigger );
            }
        }

        #endregion

        #region Overridden Methods

        /// <summary>
        /// When a control renders it's content to the page, this method will also check to see if 
        /// the block instance of this control has been configured for output caching, and if so, 
        /// the contents will also be rendered to a string variable that will gets cached in the 
        /// default MemoryCache for use next time by the CmsPage.OnInit() method when rendering the 
        /// control content.
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render( HtmlTextWriter writer )
        {
            if ( BlockInstance.OutputCacheDuration > 0 )
            {
                string blockCacheKey = string.Format( "Rock:BlockInstanceOutput:{0}", BlockInstance.Id );
                StringBuilder sbOutput = new StringBuilder();
                StringWriter swOutput = new StringWriter( sbOutput );
                HtmlTextWriter twOutput = new HtmlTextWriter( swOutput );

                base.Render( twOutput );

                CacheItemPolicy cacheDuration = new CacheItemPolicy();
                cacheDuration.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds( BlockInstance.OutputCacheDuration );

                ObjectCache cache = MemoryCache.Default;
                cache.Set( blockCacheKey, sbOutput.ToString(), cacheDuration );
            }

            base.Render( writer );
        }

        /// <summary>
        /// When an unhandled error occurs in a module, a notification box will be displayed.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnError( EventArgs e )
        {
            DisplayNotification( "Exception", Rock.Controls.NotificationBoxType.Error,
                HttpContext.Current.Server.GetLastError().Message );

            base.OnError( e );
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Clear all child controls and add a notification box with error or warning message
        /// </summary>
        /// <param name="title"></param>
        /// <param name="type"></param>
        /// <param name="message"></param>
        public void DisplayNotification( string title, Rock.Controls.NotificationBoxType type, string message )
        {
            this.Controls.Clear();

            Rock.Controls.NotificationBox notification = new Controls.NotificationBox();
            notification.Title = title;
            notification.NotificationBoxType = type;
            notification.Text = message;
            this.Controls.Add( notification );
        }

        public void DisplayError( string message )
        {
            DisplayNotification( "Error", Rock.Controls.NotificationBoxType.Error, message );
        }

        public void DisplayWarning( string message )
        {
            DisplayNotification( "Warning", Rock.Controls.NotificationBoxType.Warning, message );
        }

        /// <summary>
        /// Returns the current blockinstance value for the selected attribute
        /// If the attribute doesn't exist a null value is returned  
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string AttributeValue( string key )
        {
            if ( BlockInstance == null )
                return string.Empty;

            if ( BlockInstance.AttributeValues == null )
                return string.Empty;

            if ( BlockInstance.AttributeValues.ContainsKey( key ) )
                return BlockInstance.AttributeValues[key].Value;

            return null;
        }

        /// <summary>
        /// Evaluates if the user is authorized to perform the requested action 
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool UserAuthorized( string action )
        {
            return BlockInstance.Authorized( action, CurrentUser );
        }

        /// <summary>
        /// Checks the page's RouteData values and then the query string for a
        /// parameter matching the specified name, and if found returns the string
        /// value
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string PageParameter( string name )
        {
            if ( Page.RouteData.Values.ContainsKey( name ) )
                return ( string )Page.RouteData.Values[name];

            if ( String.IsNullOrEmpty( Request.QueryString[name] ) )
                return string.Empty;
            else
                return Request.QueryString[name];
        }

        /// <summary>
        /// Adds iconts to the configuration area of a block instance.  Can be overridden to
        /// add additionsl icons
        /// </summary>
        /// <param name="canConfig"></param>
        /// <param name="canEdit"></param>
        /// <returns></returns>
        public virtual List<Control> GetConfigurationControls( bool canConfig, bool canEdit )
        {
            List<Control> configControls = new List<Control>();

            if ( canConfig || canEdit)
            {
                // Attributes
                CompiledTemplateBuilder upContent = new CompiledTemplateBuilder(
                    delegate( Control content )
                    {
                        Button trigger = new Button();
                        trigger.ClientIDMode = System.Web.UI.ClientIDMode.Static;
                        trigger.ID = string.Format( "blck-cnfg-trggr-{0}", BlockInstance.Id.ToString() );
                        trigger.Click += trigger_Click;
                        content.Controls.Add( trigger );

                        HiddenField triggerData = new HiddenField();
                        triggerData.ClientIDMode = System.Web.UI.ClientIDMode.Static;
                        triggerData.ID = string.Format( "blck-cnfg-trggr-data-{0}", BlockInstance.Id.ToString() );
                        content.Controls.Add( triggerData );
                    }
                );

                UpdatePanel upTrigger = new UpdatePanel();
                upTrigger.ContentTemplate = upContent;
                configControls.Add( upTrigger );
                upTrigger.Attributes.Add( "style", "display:none" );

                HtmlGenericControl aAttributes = new HtmlGenericControl( "a" );
                aAttributes.Attributes.Add( "class", "properties icon-button show-iframe-dialog" );
                aAttributes.Attributes.Add("href", ResolveUrl(string.Format("~/BlockProperties/{0}", BlockInstance.Id)));
                aAttributes.Attributes.Add("title", "Block Properties");
                aAttributes.Attributes.Add("instance-id", BlockInstance.Id.ToString());
                configControls.Add( aAttributes );
            }

            if ( canConfig )
            {
                // Security
                HtmlGenericControl aSecureBlock = new HtmlGenericControl( "a" );
                aSecureBlock.Attributes.Add( "class", "security icon-button show-iframe-dialog" );
                aSecureBlock.Attributes.Add( "href", ResolveUrl( string.Format( "~/Secure/{0}/{1}",
                    Rock.Cms.Security.Authorization.EncodeEntityTypeName( BlockInstance.GetType() ), BlockInstance.Id ) ) );
                aSecureBlock.Attributes.Add( "title", "Block Security" );
                configControls.Add( aSecureBlock );
                
                // Move
                HtmlGenericControl aMoveBlock = new HtmlGenericControl( "a" );
                aMoveBlock.Attributes.Add( "class", "block-move icon-button blockinstance-move" );
                aMoveBlock.Attributes.Add("href", BlockInstance.Id.ToString());
                aMoveBlock.Attributes.Add( "title", "Move" );
                configControls.Add( aMoveBlock );

                // Delete
                HtmlGenericControl aDeleteBlock = new HtmlGenericControl( "a" );
                aDeleteBlock.Attributes.Add( "class", "delete icon-button blockinstance-delete" );
                aDeleteBlock.Attributes.Add("href", BlockInstance.Id.ToString());
                aDeleteBlock.Attributes.Add( "title", "Delete" );
                configControls.Add( aDeleteBlock );
            }

            return configControls;
        }

        protected void trigger_Click( object sender, EventArgs e )
        {
            if ( AttributesUpdated != null )
                AttributesUpdated( sender, e );
        }

        #endregion

        #region Internal Methods

        internal void VerifyInstanceAttributes()
        {
            bool attributesUpdated = false;

            using ( new Rock.Helpers.UnitOfWorkScope() )
            {

                Type type = this.GetType();
                foreach ( object customAttribute in type.GetCustomAttributes( typeof( BlockInstancePropertyAttribute ), true ) )
                {
                    BlockInstancePropertyAttribute blockInstanceProperty = ( BlockInstancePropertyAttribute )customAttribute;
                    attributesUpdated = blockInstanceProperty.UpdateAttribute( this.BlockInstance, CurrentPersonId ) || attributesUpdated;
                }
            }

            if (attributesUpdated)
                this.BlockInstance.ReloadAttributeValues();

            this.BlockInstance.Block.InstancePropertiesVerified = true;
        }

        #endregion

        #region Event Handlers

        void CmsBlock_BlockInstanceAttributesUpdated( object sender, BlockInstanceAttributesUpdatedEventArgs e )
        {
            if ( e.BlockInstanceID == BlockInstance.Id && AttributesUpdated != null )
                AttributesUpdated( sender, e );
        }

        #endregion

    }
}
