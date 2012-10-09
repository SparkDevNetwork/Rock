//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Caching;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock.Cms;
using Rock.Crm;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Web.UI
{
    /// <summary>
    /// RockBlock is the base abstract class that all Blocks should inherit from
    /// </summary>
    public abstract class RockBlock : UserControl
    {
        #region Events

        /// <summary>
        /// Occurs when the block instance properties are updated.
        /// </summary>
        public event EventHandler<EventArgs> AttributesUpdated;

        #endregion

        #region Public Properties

        /// <summary>
        /// The current page.  This value is read and cached by the RockRouteHandler
        /// and set by the layout's base class (Rock.Web.UI.RockPage) when loading the block instance
        /// </summary>
        public PageCache CurrentPage { get; set; }

        /// <summary>
        /// The current block.  This value is read and cached by the layout's 
        /// base class (Rock.Web.UI.RockPage) when loading the block instance
        /// </summary>
        public BlockCache CurrentBlock { get; set; }

        /// <summary>
        /// The personID of the currently logged in user.  If user is not logged in, returns null
        /// </summary>
        public int? CurrentPersonId
        {
            get { return ( (RockPage)this.Page ).CurrentPersonId; }
        }

        /// <summary>
        /// Returns the currently logged in user.  If user is not logged in, returns null
        /// </summary>
        public User CurrentUser
        {
            get { return ( (RockPage)this.Page ).CurrentUser; }
        }

        /// <summary>
        /// Returns the currently logged in person. If user is not logged in, returns null
        /// </summary>
        public Person CurrentPerson
        {
            get { return ( (RockPage)this.Page ).CurrentPerson; }
        }

        /// <summary>
        /// Relative path to the current theme and layout folder.  Useful for setting paths to
        /// theme resource files
        /// <example>
        /// Client Side: <c><![CDATA[<img src='<%= ThemePath %>/Images/avatar.gif' />]]> </c>
        /// Server Side: <c>myImg.ImageUrl = ThemePath + "/Images/avatar.gif";</c>
        /// </example>
        /// </summary>
        public string ThemePath
        {
            get { return ( (RockPage)this.Page ).ThemePath; }
        }

        /// <summary>
        /// Gets the root URL Path.
        /// </summary>
        public string RootPath
        {
            get
            {
                Uri uri = new Uri( HttpContext.Current.Request.Url.ToString() );
                return uri.Scheme + "://" + uri.GetComponents( UriComponents.HostAndPort, UriFormat.UriEscaped ) + Page.ResolveUrl( "~" );
            }
        }

        /// <summary>
        /// Gets a list of any context entities that the block requires.
        /// </summary>
        public virtual List<string> RequiredContext
        {
            get
            {
                return new List<string>();
            }
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

        /// <summary>
        /// Items the cache key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private string ItemCacheKey( string key )
        {
            return string.Format( "Rock:Page:{0}:RockBlock:{1}:ItemCache:{2}",
                this.CurrentPage.Id, CurrentBlock.Id, key );
        }

        /// <summary>
        /// Adds an update trigger for when the block instance properties are updated.
        /// </summary>
        /// <param name="updatePanel">The update panel.</param>
        public void AddAttributeUpdateTrigger( UpdatePanel updatePanel )
        {
            if ( CurrentBlock.IsAuthorized( "Configure", CurrentPerson ) )
            {
                AsyncPostBackTrigger trigger = new AsyncPostBackTrigger();
                trigger.ControlID = string.Format( "blck-cnfg-trggr-{0}", CurrentBlock.Id );
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
        /// default MemoryCache for use next time by the Rock.Web.UI.RockPage.OnInit() method when rendering the 
        /// control content.
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render( HtmlTextWriter writer )
        {
            if ( CurrentBlock.OutputCacheDuration > 0 )
            {
                string blockCacheKey = string.Format( "Rock:BlockOutput:{0}", CurrentBlock.Id );
                StringBuilder sbOutput = new StringBuilder();
                StringWriter swOutput = new StringWriter( sbOutput );
                HtmlTextWriter twOutput = new HtmlTextWriter( swOutput );

                base.Render( twOutput );

                CacheItemPolicy cacheDuration = new CacheItemPolicy();
                cacheDuration.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds( CurrentBlock.OutputCacheDuration );

                ObjectCache cache = MemoryCache.Default;
                cache.Set( blockCacheKey, sbOutput.ToString(), cacheDuration );
            }

            base.Render( writer );
        }

        ///// <summary>
        ///// When an unhandled error occurs in a module, a notification box will be displayed.
        ///// </summary>
        ///// <param name="e"></param>
        //protected override void OnError( EventArgs e )
        //{
        //    DisplayNotification( "Exception", NotificationBoxType.Error,
        //        HttpContext.Current.Server.GetLastError().Message );

        //    base.OnError( e );
        //}
        #endregion

        #region Public Methods

        ///// <summary>
        ///// Clear all child controls and add a notification box with error or warning message
        ///// </summary>
        ///// <param name="title"></param>
        ///// <param name="type"></param>
        ///// <param name="message"></param>
        //public void DisplayNotification( string title, NotificationBoxType type, string message )
        //{
        //    NotificationBox notification = new NotificationBox();
        //    notification.Title = title;
        //    notification.NotificationBoxType = type;
        //    notification.Text = message;
        //    this.Controls.Add( notification );
        //}

        ///// <summary>
        ///// Clear all child controls and add a notification box with an error message
        ///// </summary>
        ///// <param name="message">The message.</param>
        //public void DisplayError( string message )
        //{
        //    DisplayNotification( "Error", NotificationBoxType.Error, message );
        //}

        ///// <summary>
        ///// Clear all child controls and add a notification box with a warning message
        ///// </summary>
        ///// <param name="message">The message.</param>
        //public void DisplayWarning( string message )
        //{
        //    DisplayNotification( "Warning", NotificationBoxType.Warning, message );
        //}

        /// <summary>
        /// Returns the current block value for the selected attribute
        /// If the attribute doesn't exist a null value is returned  
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string AttributeValue( string key )
        {
            if ( CurrentBlock != null &&
                CurrentBlock.AttributeValues != null &&
                CurrentBlock.AttributeValues.ContainsKey( key ) )
                return CurrentBlock.AttributeValues[key].Value[0].Value;

            return null;
        }

        /// <summary>
        /// Evaluates if the user is authorized to perform the requested action 
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool IsUserAuthorized( string action )
        {
            return CurrentBlock.IsAuthorized( action, CurrentPerson );
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
            return ( (RockPage)this.Page ).PageParameter( name );
        }

        /// <summary>
        /// Gets the value for the current user for a given key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetUserValue( string key )
        {
            return ( (RockPage)this.Page ).GetUserValue( key );
        }

        /// <summary>
        /// Sets a value for the current user for a given key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetUserValue( string key, string value )
        {
            ( (RockPage)this.Page ).SetUserValue( key, value );
        }

        /// <summary>
        /// Adds icons to the configuration area of a block instance.  Can be overridden to
        /// add additionsl icons
        /// </summary>
        /// <param name="canConfig"></param>
        /// <param name="canEdit"></param>
        /// <returns></returns>
        public virtual List<Control> GetConfigurationControls( bool canConfig, bool canEdit )
        {
            List<Control> configControls = new List<Control>();

            if ( canConfig || canEdit )
            {
                // Attributes
                CompiledTemplateBuilder upContent = new CompiledTemplateBuilder(
                    delegate( Control content )
                    {
                        Button trigger = new Button();
                        trigger.ClientIDMode = System.Web.UI.ClientIDMode.Static;
                        trigger.ID = string.Format( "blck-cnfg-trggr-{0}", CurrentBlock.Id.ToString() );
                        trigger.Click += trigger_Click;
                        content.Controls.Add( trigger );

                        HiddenField triggerData = new HiddenField();
                        triggerData.ClientIDMode = System.Web.UI.ClientIDMode.Static;
                        triggerData.ID = string.Format( "blck-cnfg-trggr-data-{0}", CurrentBlock.Id.ToString() );
                        content.Controls.Add( triggerData );
                    }
                );

                UpdatePanel upTrigger = new UpdatePanel();
                upTrigger.ContentTemplate = upContent;
                configControls.Add( upTrigger );
                upTrigger.Attributes.Add( "style", "display:none" );

                // Icon to display block properties
                HtmlGenericControl aAttributes = new HtmlGenericControl( "a" );
                aAttributes.Attributes.Add( "class", "properties show-modal-iframe" );
                aAttributes.Attributes.Add( "height", "500px" );
                aAttributes.Attributes.Add( "href", ResolveUrl( string.Format( "~/BlockProperties/{0}?t=Block Properties", CurrentBlock.Id ) ) );
                aAttributes.Attributes.Add( "title", "Block Properties" );
                //aAttributes.Attributes.Add( "instance-id", BlockInstance.Id.ToString() );
                configControls.Add( aAttributes );
                HtmlGenericControl iAttributes = new HtmlGenericControl( "i" );
                aAttributes.Controls.Add( iAttributes );
                iAttributes.Attributes.Add( "class", "icon-cog" );
            }

            if ( canConfig )
            {
                // Security
                HtmlGenericControl aSecureBlock = new HtmlGenericControl( "a" );
                aSecureBlock.Attributes.Add( "class", "security show-modal-iframe" );
                aSecureBlock.Attributes.Add( "height", "500px" );
                aSecureBlock.Attributes.Add( "href", ResolveUrl( string.Format( "~/Secure/{0}/{1}?t=Block Security",
                    Security.Authorization.EncodeEntityTypeName( CurrentBlock.GetType() ), CurrentBlock.Id ) ) );
                aSecureBlock.Attributes.Add( "title", "Block Security" );
                configControls.Add( aSecureBlock );
                HtmlGenericControl iSecureBlock = new HtmlGenericControl( "i" );
                aSecureBlock.Controls.Add( iSecureBlock );
                iSecureBlock.Attributes.Add( "class", "icon-lock" );

                // Move
                HtmlGenericControl aMoveBlock = new HtmlGenericControl( "a" );
                aMoveBlock.Attributes.Add( "class", "block-move block-move" );
                aMoveBlock.Attributes.Add( "href", CurrentBlock.Id.ToString() );
                aMoveBlock.Attributes.Add( "zone", CurrentBlock.Zone );
                aMoveBlock.Attributes.Add( "zoneloc", CurrentBlock.BlockLocation.ToString() );
                aMoveBlock.Attributes.Add( "title", "Move Block" );
                configControls.Add( aMoveBlock );
                HtmlGenericControl iMoveBlock = new HtmlGenericControl( "i" );
                aMoveBlock.Controls.Add( iMoveBlock );
                iMoveBlock.Attributes.Add( "class", "icon-external-link" );

                // Delete
                HtmlGenericControl aDeleteBlock = new HtmlGenericControl( "a" );
                aDeleteBlock.Attributes.Add( "class", "delete block-delete" );
                aDeleteBlock.Attributes.Add( "href", CurrentBlock.Id.ToString() );
                aDeleteBlock.Attributes.Add( "title", "Delete Block" );
                configControls.Add( aDeleteBlock );
                HtmlGenericControl iDeleteBlock = new HtmlGenericControl( "i" );
                aDeleteBlock.Controls.Add( iDeleteBlock );
                iDeleteBlock.Attributes.Add( "class", "icon-remove-circle" );
            }

            return configControls;
        }

        /// <summary>
        /// Handles the Click event of the trigger control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void trigger_Click( object sender, EventArgs e )
        {
            if ( AttributesUpdated != null )
            {
                AttributesUpdated( sender, e );
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Creates and or updates any attributes defined for the block
        /// </summary>
        internal void CreateAttributes()
        {
            if ( Rock.Attribute.Helper.UpdateAttributes( this.GetType(),
                "Rock.Cms.Block", "BlockTypeId", this.CurrentBlock.BlockTypeId.ToString(), CurrentPersonId ) )
            {
                this.CurrentBlock.ReloadAttributeValues();
            }
        }

        /// <summary>
        /// Reads the additional actions.
        /// </summary>
        internal void ReadAdditionalActions()
        {
            object[] customAttributes = this.GetType().GetCustomAttributes( typeof( AdditionalActionsAttribute ), true );
            if ( customAttributes.Length > 0 )
                this.CurrentBlock.BlockTypeActions = ( (AdditionalActionsAttribute)customAttributes[0] ).AdditionalActions;
            else
                this.CurrentBlock.BlockTypeActions = new List<string>();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the BlockAttributesUpdated event of the CmsBlock control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.BlockAttributesUpdatedEventArgs"/> instance containing the event data.</param>
        void CmsBlock_BlockAttributesUpdated( object sender, BlockAttributesUpdatedEventArgs e )
        {
            if ( e.BlockID == CurrentBlock.Id && AttributesUpdated != null )
                AttributesUpdated( sender, e );
        }

        #endregion

    }
}
