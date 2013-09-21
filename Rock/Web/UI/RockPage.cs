//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock.Model;
using Rock.Transactions;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Page = System.Web.UI.Page;

namespace Rock.Web.UI
{
    /// <summary>
    /// RockPage is the base abstract class that all page templates should inherit from
    /// </summary>
    public abstract class RockPage : Page
    {
        #region Private Variables

        private PlaceHolder phLoadTime;
        private ScriptManager _scriptManager;

        #endregion

        #region Protected Variables

        /// <summary>
        /// The full name of the currently logged in user
        /// </summary>
        protected string UserName = string.Empty;

        #endregion

        #region Public Properties

        /// <summary>
        /// The current Rock page instance being requested.  This value is set 
        /// by the RockRouteHandler immediately after instantiating the page
        /// </summary>
        public PageCache CurrentPage
        {
            get
            {
                return _currentPage;
            }
            set
            {
                _currentPage = value;
                HttpContext.Current.Items.Add( "Rock:SiteId", _currentPage.Site.Id );
                HttpContext.Current.Items.Add( "Rock:PageId", _currentPage.Id );
            }
        }
        private PageCache _currentPage = null;

        /// <summary>
        /// Gets the current page reference.
        /// </summary>
        /// <value>
        /// The current page reference.
        /// </value>
        public PageReference CurrentPageReference
        {
            get
            {
                if ( _currentPageReference == null && Context.Items.Contains( "CurrentPageReference" ) )
                {
                    _currentPageReference = Context.Items["CurrentPageReference"] as PageReference;
                }

                return _currentPageReference;
            }

            set
            {
                Context.Items.Remove( "CurrentPageReference" );
                _currentPageReference = value;

                if ( _currentPageReference != null )
                {
                    Context.Items.Add( "CurrentPageReference", _CurrentUser );
                }
            }
        }
        public PageReference _currentPageReference = null;

        /// <summary>
        /// The content areas on a layout page that blocks can be added to 
        /// </summary>
        /// <remarks>
        /// The Dictionary's key is the zonekey and the KeyValuePair is a combination 
        /// of the friendly zone name and the zone control
        /// </remarks>
        public Dictionary<string, KeyValuePair<string, Zone>> Zones { get; private set; }

        /// <summary>
        /// Gets the bread crumbs.
        /// </summary>
        /// <value>
        /// The bread crumbs.
        /// </value>
        public List<BreadCrumb> BreadCrumbs { get; private set; }

        /// <summary>
        /// The currently logged in user
        /// </summary>
        public Rock.Model.UserLogin CurrentUser
        {
            get
            {
                if ( _CurrentUser != null )
                {
                    return _CurrentUser;
                }

                if (Context.Items.Contains( "CurrentUser" ) )
                {
                    _CurrentUser = Context.Items["CurrentUser"] as Rock.Model.UserLogin;
                }

                if ( _CurrentUser == null )
                {
                    _CurrentUser = Rock.Model.UserLoginService.GetCurrentUser();
                    if ( _CurrentUser != null )
                    {
                        Context.Items.Add( "CurrentUser", _CurrentUser );
                    }
                }

                if ( _CurrentUser != null && _CurrentUser.Person != null && _CurrentPerson == null )
                {
                    CurrentPerson = _CurrentUser.Person;
                }

                return _CurrentUser;
            }

            private set
            {
                Context.Items.Remove( "CurrentUser" );
                _CurrentUser = value;

                if ( _CurrentUser != null )
                {
                    Context.Items.Add( "CurrentUser", _CurrentUser );
                    CurrentPerson = _CurrentUser.Person;
                }
                else
                {
                    CurrentPerson = null;
                }
            }
        }
        private Rock.Model.UserLogin _CurrentUser;

        /// <summary>
        /// Returns the current person.  This is either the currently logged in user, or if the user
        /// has not logged in, it may also be an impersonated person determined from using the encrypted
        /// person key
        /// </summary>
        public Person CurrentPerson
        {
            get
            {
                if ( _CurrentPerson != null )
                {
                    return _CurrentPerson;
                }

                if ( _CurrentPerson == null && Context.Items.Contains( "CurrentPerson" ) )
                {
                    _CurrentPerson = Context.Items["CurrentPerson"] as Person;
                }
                
                return null;
            }

            private set
            {
                Context.Items.Remove( "CurrentPerson" );
                _CurrentPerson = value;

                if ( _CurrentPerson != null )
                {
                    Context.Items.Add( "CurrentPerson", value );
                }
            }
        }
        private Person _CurrentPerson;

        /// <summary>
        /// The Person ID of the currently logged in user.  Returns null if there is not a user logged in
        /// </summary>
        public int? CurrentPersonId
        {
            get
            {
                if ( CurrentPerson != null )
                {
                    return CurrentPerson.Id;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the full url path to the current theme folder
        /// </summary>
        public string CurrentTheme
        {
            get
            {
                return ResolveUrl( string.Format( "~/Themes/{0}", CurrentPage.Site.Theme ) );
            }
        }

        /// <summary>
        /// Gets the root url path
        /// </summary>
        public string AppPath
        {
            get
            {
                return ResolveUrl( "~" );
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Recurses a control collection looking for any zone controls
        /// </summary>
        /// <param name="controls">The controls.</param>
        protected virtual void FindRockControls( ControlCollection controls )
        {
            if ( controls != null )
            {
                foreach ( Control control in controls )
                {
                    if ( control is Zone )
                    {
                        Zone zone = control as Zone;
                        if ( zone != null )
                            Zones.Add( zone.ID, new KeyValuePair<string, Zone>( zone.Name, zone ) );
                    }

                    FindRockControls( control.Controls );
                }
            }
        }

        /// <summary>
        /// Find the <see cref="Rock.Web.UI.Controls.Zone"/> for the specified zone name.  Looks in the
        /// <see cref="Zones"/> property to see if it has been defined.  If an existing zone 
        /// <see cref="Rock.Web.UI.Controls.Zone"/> cannot be found, the <see cref="HtmlForm"/> control
        /// is returned
        /// </summary>
        /// <param name="zoneName">Name of the zone.</param>
        /// <returns></returns>
        protected virtual Control FindZone( string zoneName )
        {
            // First look in the Zones dictionary
            if ( Zones.ContainsKey( zoneName ) )
                return Zones[zoneName].Value;

            // If no match, just add module to the form
            return this.Form;
        }

        #endregion

        #region Overridden Methods

        /// <summary>
        /// Initializes the page's culture to use the culture specified by the browser ("auto")
        /// </summary>
        protected override void InitializeCulture()
        {
            base.UICulture = "auto";
            base.Culture = "auto";

            base.InitializeCulture();
        }

        /// <summary>
        /// Loads all of the configured blocks for the current page into the control tree
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit( EventArgs e )
        {
            // Add the ScriptManager to each page
            _scriptManager = ScriptManager.GetCurrent( this.Page );
            
            if ( _scriptManager == null )
            {
                _scriptManager = new ScriptManager { ID = "sManager" };
                Page.Trace.Warn( "Adding script manager" );
                Page.Form.Controls.AddAt( 0, _scriptManager );
            }

            // enable history on the ScriptManager
            _scriptManager.EnableHistory = true;

            // TODO: Delete this line, only used for testing
            _scriptManager.AsyncPostBackTimeout = 180;

            // wire up navigation event
            _scriptManager.Navigate += new EventHandler<HistoryEventArgs>(scriptManager_Navigate);

            // Add library and UI bundles during init, that way theme developers will only
            // need to worry about registering any custom scripts or script bundles they need
            _scriptManager.Scripts.Add( new ScriptReference { Name = "WebFormsBundle" } );
            _scriptManager.Scripts.Add( new ScriptReference( "~/bundles/RockLibs" ) );
            _scriptManager.Scripts.Add( new ScriptReference( "~/bundles/RockUi" ) );
            _scriptManager.Scripts.Add( new ScriptReference( "~/bundles/RockValidation" ) );

            // Recurse the page controls to find the rock page title and zone controls
            Page.Trace.Warn( "Recursing layout to find zones" );
            Zones = new Dictionary<string, KeyValuePair<string, Zone>>();
            FindRockControls( this.Controls );

            // Add a Rock version meta tag
            Page.Trace.Warn( "Adding Rock metatag" );
            string version = typeof( Rock.Web.UI.RockPage ).Assembly.GetName().Version.ToString();
            HtmlMeta rockVersion = new HtmlMeta();
            rockVersion.Attributes.Add( "name", "generator" );
            rockVersion.Attributes.Add( "content", string.Format( "Rock v{0}", version ) );
            AddMetaTag( this.Page, rockVersion );

            // If the logout parameter was entered, delete the user's forms authentication cookie and redirect them
            // back to the same page.
            Page.Trace.Warn( "Checking for logout request" );
            if ( PageParameter( "logout" ) != string.Empty )
            {
                FormsAuthentication.SignOut();
                CurrentPerson = null;
                CurrentUser = null;
                Response.Redirect( CurrentPageReference.BuildUrl() );
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            // If the impersonated query key was included then set the current person
            Page.Trace.Warn( "Checking for person impersanation" );
            string impersonatedPersonKey = PageParameter( "rckipid" );
            if ( !String.IsNullOrEmpty( impersonatedPersonKey ) )
            {
                Rock.Model.PersonService personService = new Model.PersonService();
                Rock.Model.Person impersonatedPerson = personService.GetByEncryptedKey( impersonatedPersonKey );
                if ( impersonatedPerson != null )
                {
                    Rock.Security.Authorization.SetAuthCookie( "rckipid=" + impersonatedPerson.EncryptedKey, false, true );
                    CurrentUser = impersonatedPerson.ImpersonatedUser;
                }
            }

            // Get current user/person info
            Page.Trace.Warn( "Getting CurrentUser" );
            Rock.Model.UserLogin user = CurrentUser;

            // If there is a logged in user, see if it has an associated Person Record.  If so, set the UserName to 
            // the person's full name (which is then cached in the Session state for future page requests)
            if ( user != null )
            {
                Page.Trace.Warn( "Setting CurrentPerson" );
                UserName = user.UserName;
                int? personId = user.PersonId;

                if ( personId.HasValue )
                {
                    string personNameKey = "PersonName_" + personId.Value.ToString();
                    if ( Session[personNameKey] != null )
                    {
                        UserName = Session[personNameKey].ToString();
                    }
                    else
                    {
                        Rock.Model.PersonService personService = new Model.PersonService();
                        Rock.Model.Person person = personService.Get( personId.Value );
                        if ( person != null )
                        {
                            UserName = person.FullName;
                            CurrentPerson = person;
                        }

                        Session[personNameKey] = UserName;
                    }
                }
            }

            // If a PageInstance exists
            if ( CurrentPage != null )
            {
                // check if page should have been loaded via ssl
                Page.Trace.Warn( "Checking for SSL request" );
                if ( !Request.IsSecureConnection && CurrentPage.RequiresEncryption )
                {
                    string redirectUrl = Request.Url.ToString().Replace( "http:", "https:" );
                    Response.Redirect( redirectUrl, false );
                    Context.ApplicationInstance.CompleteRequest();
                    return;
                }

                // Verify that the current user is allowed to view the page.  If not, and 
                // the user hasn't logged in yet, redirect to the login page
                Page.Trace.Warn( "Checking if user is authorized" );
                if ( !CurrentPage.IsAuthorized( "View", CurrentPerson ) )
                {
                    if ( user == null )
                    {
                        Page.Trace.Warn( "Redirecting to login page" );
                        if (!string.IsNullOrWhiteSpace(CurrentPage.Site.LoginPageReference))
                        {
                            // if the QueryString already has a returnUrl, use that, otherwise redirect to RawUrl
                            string returnUrl = Request.QueryString["returnUrl"] ?? Server.UrlEncode(Request.RawUrl);
                            
                            string loginPageRequestPath = ResolveUrl( CurrentPage.Site.LoginPageReference );

                            if ( loginPageRequestPath.Equals( Request.Path ) )
                            {
                                // The LoginPage security isn't set to Allow All, so throw exception to prevent recursive loop
                                throw new Exception( string.Format("Page security for Site.LoginPageReference {0} is invalid", CurrentPage.Site.LoginPageReference));
                            }
                            else
                            {
                                Response.Redirect( loginPageRequestPath + "?returnurl=" + returnUrl );
                            }
                        }
                        else
                        {
                            FormsAuthentication.RedirectToLoginPage();
                        }
                    }
                }
                else
                {
                    // Set current models (context)
                    Page.Trace.Warn( "Checking for Context" );
                    CurrentPage.Context = new Dictionary<string, Data.KeyEntity>();
                    try 
                    {
                        foreach ( var pageContext in CurrentPage.PageContexts )
                        {
                            int contextId = 0;
                            if ( Int32.TryParse( PageParameter( pageContext.Value ), out contextId ) )
                                CurrentPage.Context.Add( pageContext.Key, new Data.KeyEntity( contextId ) );
                        }

                        char[] delim = new char[1] { ',' };
                        foreach ( string param in PageParameter( "context" ).Split( delim, StringSplitOptions.RemoveEmptyEntries ) )
                        {
                            string contextItem = Rock.Security.Encryption.DecryptString( param );
                            string[] parts = contextItem.Split('|');
                            if (parts.Length == 2)
                                CurrentPage.Context.Add(parts[0], new Data.KeyEntity(parts[1]));
                        }

                    }
                    catch { }

                    // set page title
                    Page.Trace.Warn( "Setting page title" );
                    if ( CurrentPage.Title != null && CurrentPage.Title != "" )
                    {
                        this.Title = CurrentPage.Title;
                    }
                    else
                    {
                        this.Title = CurrentPage.Name;
                    }

                    // set viewstate on/off
                    this.EnableViewState = CurrentPage.EnableViewState;

                    // Cache object used for block output caching
                    Page.Trace.Warn( "Getting memory cache" );
                    ObjectCache cache = MemoryCache.Default;

                    Page.Trace.Warn( "Checking if user can administer" );
                    bool canAdministratePage = CurrentPage.IsAuthorized( "Administrate", CurrentPerson );

                    // Create a javascript object to store information about the current page for client side scripts to use
                    Page.Trace.Warn( "Creating JS objects" );
                    string script = string.Format( @"
    Rock.settings.initialize({{ 
        siteId: {0},
        pageId: {1}, 
        layout: '{2}',
        baseUrl: '{3}' 
    }});",
                        CurrentPage.SiteId.Value, CurrentPage.Id, CurrentPage.Layout, AppPath );
                    ScriptManager.RegisterStartupScript( this.Page, this.GetType(), "rock-js-object", script, true );

                    // Add config elements
                    if ( CurrentPage.IncludeAdminFooter )
                    {
                        Page.Trace.Warn( "Adding popup controls (footer elements)" );
                        AddPopupControls();
                        if ( canAdministratePage )
                        {
                            Page.Trace.Warn( "Adding adminstration options" );
                            AddConfigElements();
                        }
                    }

                    // Initialize the list of breadcrumbs for the current page (and blocks on the page)
                    Page.Trace.Warn( "Setting breadcrumbs" );
                    CurrentPageReference.BreadCrumbs = new List<BreadCrumb>();

                    // If the page is configured to display in the breadcrumbs...
                    string bcName = CurrentPage.BreadCrumbText;
                    if (bcName != string.Empty)
                    {
                        CurrentPageReference.BreadCrumbs.Add( new BreadCrumb( bcName, CurrentPageReference.BuildUrl() ) );
                    }

                    // Load the blocks and insert them into page zones
                    Page.Trace.Warn( "Loading Blocks" );
                    foreach ( Rock.Web.Cache.BlockCache block in CurrentPage.Blocks )
                    {
                        Page.Trace.Warn( string.Format( "\tLoading '{0}' block", block.Name ) );

                        // Get current user's permissions for the block instance
                        Page.Trace.Warn( "\tChecking permission" );
                        bool canAdministrate = block.IsAuthorized( "Administrate", CurrentPerson );
                        bool canEdit = block.IsAuthorized( "Edit", CurrentPerson );
                        bool canView = block.IsAuthorized( "View", CurrentPerson );

                        // Make sure user has access to view block instance
                        if ( canAdministrate || canEdit || canView )
                        {
                            // Create block wrapper control (implements INamingContainer so child control IDs are unique for
                            // each block instance
                            Page.Trace.Warn( "\tAdding block wrapper html" );

                            HtmlGenericContainer blockWrapper = new HtmlGenericContainer( "div" );
                            blockWrapper.ID = string.Format( "bid_{0}", block.Id );
                            blockWrapper.Attributes.Add( "zoneloc", block.BlockLocation.ToString() );
                            blockWrapper.ClientIDMode = ClientIDMode.Static;
                            FindZone( block.Zone ).Controls.Add( blockWrapper );
                            blockWrapper.Attributes.Add( "class", "block-instance " +
                                ( canAdministrate || canEdit ? "can-configure " : "" ) +
                                block.BlockType.Name.ToLower().Replace( ' ', '-' ) );

                            // Check to see if block is configured to use a "Cache Duration'
                            string blockCacheKey = string.Format( "Rock:BlockInstanceOutput:{0}", block.Id );
                            if ( block.OutputCacheDuration > 0 && cache.Contains( blockCacheKey ) )
                            {
                                // If the current block exists in our custom output cache, add the cached output instead of adding the control
                                blockWrapper.Controls.Add( new LiteralControl( cache[blockCacheKey] as string ) );
                            }
                            else
                            {
                                // Load the control and add to the control tree
                                Page.Trace.Warn( "\tLoading control" );
                                Control control;

                                try
                                {
                                    control = TemplateControl.LoadControl( block.BlockType.Path );
                                    control.ClientIDMode = ClientIDMode.AutoID;
                                }
                                catch ( Exception ex )
                                {
                                    HtmlGenericControl div = new HtmlGenericControl( "div" );
                                    div.Attributes.Add( "class", "alert-message block-message error" );
                                    div.InnerHtml = string.Format( "Error Loading Block:<br/><br/><strong>{0}</strong>", ex.Message );
                                    control = div;

                                    if ( this.IsPostBack )
                                    {
                                        // throw an error on PostBack so that the ErrorPage gets shown (vs nothing happening)
                                        throw ex;
                                    }
                                }

                                RockBlock blockControl = null;

                                // Check to see if the control was a PartialCachingControl or not
                                Page.Trace.Warn( "\tChecking block for partial caching" );
                                if ( control is RockBlock )
                                    blockControl = control as RockBlock;
                                else
                                {
                                    if ( control is PartialCachingControl && ( (PartialCachingControl)control ).CachedControl != null )
                                    {
                                        blockControl = (RockBlock)( (PartialCachingControl)control ).CachedControl;
                                    }
                                }
                                
                                // If the current control is a block, set it's properties
                                if ( blockControl != null )
                                {
                                    Page.Trace.Warn( "\tSetting block properties" );

                                    blockControl.CurrentPage = CurrentPage;
                                    blockControl.CurrentPageReference = CurrentPageReference;
                                    blockControl.CurrentBlock = block;

                                    // Add any breadcrumbs to current page reference that the block creates
                                    Page.Trace.Warn( "\tAdding any breadcrumbs from block" );
                                    if ( block.BlockLocation == BlockLocation.Page )
                                    {
                                        blockControl.GetBreadCrumbs( CurrentPageReference ).ForEach( c => CurrentPageReference.BreadCrumbs.Add( c ) );
                                    }

                                    // If the blocktype's additional actions have not yet been loaded, load them now
                                    if ( !block.BlockType.CheckedAdditionalSecurityActions )
                                    {
                                        Page.Trace.Warn( "\tAdding additional security actions for blcok" );
                                        foreach ( string action in blockControl.GetAdditionalActions() )
                                        {
                                            if ( !block.BlockType.SupportedActions.Contains( action ) )
                                            {
                                                block.BlockType.SupportedActions.Add( action );
                                            }
                                        }
                                        block.BlockType.CheckedAdditionalSecurityActions = true;
                                    }

                                    // If the block's AttributeProperty values have not yet been verified verify them.
                                    // (This provides a mechanism for block developers to define the needed block
                                    //  attributes in code and have them automatically added to the database)
                                    Page.Trace.Warn( "\tChecking if block attributes need refresh" );
                                    if ( !block.BlockType.IsInstancePropertiesVerified )
                                    {
                                        Page.Trace.Warn( "\tCreating block attributes" );
                                        blockControl.CreateAttributes();
                                        block.BlockType.IsInstancePropertiesVerified = true;
                                    }

                                    // Add the block configuration scripts and icons if user is authorized
                                    if ( CurrentPage.IncludeAdminFooter )
                                    {
                                        Page.Trace.Warn( "\tAdding block configuration tools" );
                                        AddBlockConfig( blockWrapper, blockControl, block, canAdministrate, canEdit );
                                    }
                                }

                                Page.Trace.Warn( "\tAdding block to control tree" );
                                HtmlGenericContainer blockContent = new HtmlGenericContainer( "div" );
                                blockContent.Attributes.Add( "class", "block-content" );
                                blockWrapper.Controls.Add( blockContent );

                                // Add the block
                                blockContent.Controls.Add( control );
                            }
                        }
                    }

                    // Make the last crumb for this page the active one
                    Page.Trace.Warn( "Setting active breadcrumb" );
                    if ( CurrentPageReference.BreadCrumbs.Any() )
                    {
                        CurrentPageReference.BreadCrumbs.Last().Active = true;
                    }

                    Page.Trace.Warn( "Getting parent page references" );
                    var pageReferences = PageReference.GetParentPageReferences( this, CurrentPage, CurrentPageReference );
                    pageReferences.Add( CurrentPageReference );
                    PageReference.SavePageReferences( pageReferences );

                    // Update breadcrumbs
                    Page.Trace.Warn( "Updating breadcrumbs" );
                    BreadCrumbs = new List<BreadCrumb>();
                    foreach ( var pageReference in pageReferences )
                    {
                        pageReference.BreadCrumbs.ForEach( c => BreadCrumbs.Add( c ) );
                    }

                    // Add favicon and apple touch icons to page
                    Page.Trace.Warn( "Adding favicons and appletouch links" );
                    if ( CurrentPage.Site.FaviconUrl != null )
                    {
                        System.Web.UI.HtmlControls.HtmlLink faviconLink = new System.Web.UI.HtmlControls.HtmlLink();

                        faviconLink.Attributes.Add( "rel", "shortcut icon" );
                        faviconLink.Attributes.Add( "href", ResolveUrl( "~/" + CurrentPage.Site.FaviconUrl ) );

                        CurrentPage.AddHtmlLink( this.Page, faviconLink );
                    }

                    if ( CurrentPage.Site.AppleTouchIconUrl != null )
                    {
                        System.Web.UI.HtmlControls.HtmlLink touchLink = new System.Web.UI.HtmlControls.HtmlLink();

                        touchLink.Attributes.Add( "rel", "apple-touch-icon" );
                        touchLink.Attributes.Add( "href", ResolveUrl( "~/" + CurrentPage.Site.AppleTouchIconUrl ) );

                        CurrentPage.AddHtmlLink( this.Page, touchLink );
                    }

                    // Add the page admin footer if the user is authorized to edit the page
                    if ( CurrentPage.IncludeAdminFooter && canAdministratePage )
                    {
                        Page.Trace.Warn( "Adding admin footer to page" );
                        HtmlGenericControl adminFooter = new HtmlGenericControl( "div" );
                        adminFooter.ID = "cms-admin-footer";
                        adminFooter.ClientIDMode = System.Web.UI.ClientIDMode.Static;
                        this.Form.Controls.Add( adminFooter );

                        phLoadTime = new PlaceHolder();
                        adminFooter.Controls.Add( phLoadTime );
                        
                        HtmlGenericControl buttonBar = new HtmlGenericControl( "div" );
                        adminFooter.Controls.Add( buttonBar );
                        buttonBar.Attributes.Add( "class", "button-bar" );

                        // RockBlock Config
                        HtmlGenericControl aBlockConfig = new HtmlGenericControl( "a" );
                        buttonBar.Controls.Add( aBlockConfig );
                        aBlockConfig.Attributes.Add( "class", "btn block-config" );
                        aBlockConfig.Attributes.Add( "href", "javascript: Rock.admin.pageAdmin.showBlockConfig();" );
                        aBlockConfig.Attributes.Add( "Title", "Block Configuration" );
                        HtmlGenericControl iBlockConfig = new HtmlGenericControl( "i" );
                        aBlockConfig.Controls.Add( iBlockConfig );
                        iBlockConfig.Attributes.Add( "class", "icon-th-large" );

                        // RockPage Properties
                        HtmlGenericControl aAttributes = new HtmlGenericControl( "a" );
                        buttonBar.Controls.Add( aAttributes );
                        aAttributes.ID = "aPageProperties";
                        aAttributes.ClientIDMode = System.Web.UI.ClientIDMode.Static;
                        aAttributes.Attributes.Add( "class", "btn properties" );
                        aAttributes.Attributes.Add( "height", "500px" );
                        aAttributes.Attributes.Add( "href", "javascript: Rock.controls.modal.show($(this), '" + ResolveUrl( string.Format( "~/PageProperties/{0}?t=Page Properties", CurrentPage.Id ) ) + "')" );
                        aAttributes.Attributes.Add( "Title", "Page Properties" );
                        HtmlGenericControl iAttributes = new HtmlGenericControl( "i" );
                        aAttributes.Controls.Add( iAttributes );
                        iAttributes.Attributes.Add( "class", "icon-cog" );

                        // Child Pages
                        HtmlGenericControl aChildPages = new HtmlGenericControl( "a" );
                        buttonBar.Controls.Add( aChildPages );
                        aChildPages.ID = "aChildPages";
                        aChildPages.ClientIDMode = System.Web.UI.ClientIDMode.Static;
                        aChildPages.Attributes.Add( "class", "btn page-child-pages" );
                        aChildPages.Attributes.Add( "height", "500px" );
                        aChildPages.Attributes.Add( "href", "javascript: Rock.controls.modal.show($(this), '" + ResolveUrl( string.Format( "~/pages/{0}?t=Child Pages&pb=&sb=Done", CurrentPage.Id ) ) + "')" );
                        aChildPages.Attributes.Add( "Title", "Child Pages" );
                        HtmlGenericControl iChildPages = new HtmlGenericControl( "i" );
                        aChildPages.Controls.Add( iChildPages );
                        iChildPages.Attributes.Add( "class", "icon-sitemap" );

                        // RockPage Zones
                        HtmlGenericControl aPageZones = new HtmlGenericControl( "a" );
                        buttonBar.Controls.Add( aPageZones );
                        aPageZones.Attributes.Add( "class", "btn page-zones" );
                        aPageZones.Attributes.Add( "href", "javascript: Rock.admin.pageAdmin.showPageZones();" );
                        aPageZones.Attributes.Add( "Title", "Page Zones" );
                        HtmlGenericControl iPageZones = new HtmlGenericControl( "i" );
                        aPageZones.Controls.Add( iPageZones );
                        iPageZones.Attributes.Add( "class", "icon-columns" );

                        // RockPage Security
                        HtmlGenericControl aPageSecurity = new HtmlGenericControl( "a" );
                        buttonBar.Controls.Add( aPageSecurity );
                        aPageSecurity.ID = "aPageSecurity";
                        aPageSecurity.ClientIDMode = System.Web.UI.ClientIDMode.Static;
                        aPageSecurity.Attributes.Add( "class", "btn page-security" );
                        aPageSecurity.Attributes.Add( "height", "500px" );
                        aPageSecurity.Attributes.Add( "href", "javascript: Rock.controls.modal.show($(this), '" + ResolveUrl( string.Format( "~/Secure/{0}/{1}?t=Page Security&pb=&sb=Done",
                            EntityTypeCache.Read( typeof( Rock.Model.Page ) ).Id, CurrentPage.Id ) ) + "')" );
                        aPageSecurity.Attributes.Add( "Title", "Page Security" );
                        HtmlGenericControl iPageSecurity = new HtmlGenericControl( "i" );
                        aPageSecurity.Controls.Add( iPageSecurity );
                        iPageSecurity.Attributes.Add( "class", "icon-lock" );

                        // System Info
                        HtmlGenericControl aSystemInfo = new HtmlGenericControl( "a" );
                        buttonBar.Controls.Add( aSystemInfo );
                        aSystemInfo.ID = "aSystemInfo";
                        aSystemInfo.ClientIDMode = System.Web.UI.ClientIDMode.Static;
                        aSystemInfo.Attributes.Add( "class", "btn system-info" );
                        aSystemInfo.Attributes.Add( "height", "500px" );
                        aSystemInfo.Attributes.Add( "href", "javascript: Rock.controls.modal.show($(this), '" + ResolveUrl( "~/SystemInfo?t=System Information&pb=&sb=Done" ) + "')" );
                        aSystemInfo.Attributes.Add( "Title", "Rock Information" );
                        HtmlGenericControl iSystemInfo = new HtmlGenericControl( "i" );
                        aSystemInfo.Controls.Add( iSystemInfo );
                        iSystemInfo.Attributes.Add( "class", "icon-info-sign" );

                    }

                    // Check to see if page output should be cached.  The RockRouteHandler
                    // saves the PageCacheData information for the current page to memorycache 
                    // so it should always exist
                    if ( CurrentPage.OutputCacheDuration > 0 )
                    {
                        Response.Cache.SetCacheability( System.Web.HttpCacheability.Public );
                        Response.Cache.SetExpires( DateTime.Now.AddSeconds( CurrentPage.OutputCacheDuration ) );
                        Response.Cache.SetValidUntilExpires( true );
                    }
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load"/> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            Page.Header.DataBind();

            // create a page view transaction if enabled
            if (CurrentPage != null && Convert.ToBoolean( ConfigurationManager.AppSettings["EnablePageViewTracking"] ) )
            {
                PageViewTransaction transaction = new PageViewTransaction();
                transaction.DateViewed = DateTime.Now;
                transaction.PageId = CurrentPage.Id;
                transaction.SiteId = CurrentPage.Site.Id;
                if ( CurrentPersonId != null )
                    transaction.PersonId = (int)CurrentPersonId;
                transaction.IPAddress = Request.UserHostAddress;
                transaction.UserAgent = Request.UserAgent;

                RockQueue.TransactionQueue.Enqueue( transaction );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Page.SaveStateComplete" /> event after the page state has been saved to the persistence medium.
        /// </summary>
        /// <param name="e">A <see cref="T:System.EventArgs" /> object containing the event data.</param>
        protected override void OnSaveStateComplete( EventArgs e )
        {
            base.OnSaveStateComplete( e );

            if ( phLoadTime != null )
            {
                TimeSpan tsDuration = DateTime.Now.Subtract( (DateTime)Context.Items["Request_Start_Time"] );
                phLoadTime.Controls.Add( new LiteralControl( string.Format( "{0}: {1:N2}s", "Page Load Time", tsDuration.TotalSeconds ) ) );
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the current page's first value for the selected attribute
        /// If the attribute doesn't exist, null is returned
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public string GetAttributeValue( string key )
        {
            if ( CurrentPage != null )
            {
                return CurrentPage.GetAttributeValue( key );
            }
            return null;
        }

        /// <summary>
        /// Returns the current page's values for the selected attribute.
        /// If the attribute doesn't exist an empty list is returned.
        /// </summary>
        /// <param name="key">the block attribute key</param>
        /// <returns>a list of strings or an empty list if none exists</string></returns>
        public List<string> GetAttributeValues( string key )
        {
            if ( CurrentPage != null )
            {
                return CurrentPage.GetAttributeValues( key );
            }

            return new List<string>();
        }

        /// <summary>
        /// 
        /// </summary>
        public List<RockBlock> RockBlocks
        {
            get
            {
                return this.ControlsOfTypeRecursive<RockBlock>();
            }
        }

        /// <summary>
        /// Dims the other blocks.
        /// </summary>
        /// <param name="caller">The caller.</param>
        /// <param name="dimmed">if set to <c>true</c> [dimmed].</param>
        public void DimOtherBlocks( RockBlock caller, bool dimmed )
        {
            foreach ( IDimmableBlock dimmableBlock in this.RockBlocks.Where( a => a is IDimmableBlock ) )
            {
                if ( dimmableBlock != caller )
                {
                    dimmableBlock.SetDimmed( dimmed );
                }
            }
        }

        /// <summary>
        /// Logs the exception.
        /// </summary>
        /// <param name="ex">The System.Exception to log.</param>
        public void LogException( Exception ex )
        {
            ExceptionLogService.LogException( ex, Context, CurrentPage.Id, CurrentPage.SiteId, CurrentPersonId );
        }

        /// <summary>
        /// Adds a history point to the ScriptManager.
        /// Note: ScriptManager's EnableHistory property must be set to True
        /// </summary>
        /// <param name="key">The key to use for the history point</param>
        /// <param name="state">any state information to store for the history point</param>
        /// <param name="title">The title to be used by the browser</param>
        public void AddHistory(string key, string state, string title)
        {
            if (ScriptManager.GetCurrent(Page) != null)
            {
                ScriptManager sManager = ScriptManager.GetCurrent(Page);
                sManager.AddHistoryPoint(key, state, title);
            }
        }

        #endregion

        #region Cms Admin Content

        /// <summary>
        /// Adds the popup controls.
        /// </summary>
        private void AddPopupControls()
        {
            // Add the page admin script
            //AddScriptLink( Page, "~/Scripts/Rock/popup.js" );

            ModalIFrameDialog modalPopup = new ModalIFrameDialog();
            modalPopup.ID = "modal-popup";
            modalPopup.OnCancelScript = "window.parent.Rock.controls.modal.close();";
            this.Form.Controls.Add( modalPopup );
        }

        // Adds the neccessary script elements for managing the page/zone/blocks
        /// <summary>
        /// Adds the config elements.
        /// </summary>
        private void AddConfigElements()
        {
            // Add the page admin script
            AddScriptLink( Page, "~/bundles/RockAdmin" );

            AddBlockMove();
            // Add Zone Wrappers
            foreach ( KeyValuePair<string, KeyValuePair<string, Zone>> zoneControl in this.Zones )
            {
                Control control = zoneControl.Value.Value;
                Control parent = zoneControl.Value.Value.Parent;

                HtmlGenericControl zoneWrapper = new HtmlGenericControl( "div" );
                parent.Controls.AddAt( parent.Controls.IndexOf( control ), zoneWrapper );
                zoneWrapper.ID = string.Format( "zone-{0}", control.ID );
                zoneWrapper.ClientIDMode = System.Web.UI.ClientIDMode.Static;
                zoneWrapper.Attributes.Add( "class", "zone-instance can-configure" );

                // Zone content configuration widget
                HtmlGenericControl zoneConfig = new HtmlGenericControl( "div" );
                zoneWrapper.Controls.Add( zoneConfig );
                zoneConfig.Attributes.Add( "class", "zone-configuration config-bar" );

                HtmlGenericControl zoneConfigLink = new HtmlGenericControl( "a" );
                zoneConfigLink.Attributes.Add( "class", "zoneinstance-config" );
                zoneConfigLink.Attributes.Add( "href", "#" );
                zoneConfig.Controls.Add( zoneConfigLink );
                HtmlGenericControl iZoneConfig = new HtmlGenericControl( "i" );
                iZoneConfig.Attributes.Add( "class", "icon-circle-arrow-right" );
                zoneConfigLink.Controls.Add( iZoneConfig );

                HtmlGenericControl zoneConfigBar = new HtmlGenericControl( "div" );
                zoneConfigBar.Attributes.Add( "class", "zone-configuration-bar" );
                zoneConfig.Controls.Add( zoneConfigBar );

                HtmlGenericControl zoneConfigTitle = new HtmlGenericControl( "span" );
                zoneConfigTitle.InnerText = zoneControl.Value.Key;
                zoneConfigBar.Controls.Add( zoneConfigTitle );

                // Configure Blocks icon
                HtmlGenericControl aBlockConfig = new HtmlGenericControl( "a" );
                zoneConfigBar.Controls.Add( aBlockConfig );
                aBlockConfig.ID = string.Format( "aBlockConfig-{0}", control.ID );
                aBlockConfig.ClientIDMode = System.Web.UI.ClientIDMode.Static;
                aBlockConfig.Attributes.Add( "class", "zone-blocks" );
                aBlockConfig.Attributes.Add( "height", "500px" );
                aBlockConfig.Attributes.Add( "href", "javascript: Rock.controls.modal.show($(this), '" + ResolveUrl( string.Format( "~/ZoneBlocks/{0}/{1}?t=Zone Blocks&pb=&sb=Done", CurrentPage.Id, control.ID ) ) + "')" );
                aBlockConfig.Attributes.Add( "Title", "Zone Blocks" );
                aBlockConfig.Attributes.Add( "zone", zoneControl.Key );
                //aBlockConfig.InnerText = "Blocks";
                HtmlGenericControl iZoneBlocks = new HtmlGenericControl( "i" );
                iZoneBlocks.Attributes.Add( "class", "icon-th-large" );
                aBlockConfig.Controls.Add( iZoneBlocks );

                HtmlGenericContainer zoneContent = new HtmlGenericContainer( "div" );
                zoneContent.Attributes.Add( "class", "zone-content" );
                zoneWrapper.Controls.Add( zoneContent );

                parent.Controls.Remove( control );
                zoneContent.Controls.Add( control );
            }
        }

        // Adds the configuration html elements for editing a block
        /// <summary>
        /// Adds the block config.
        /// </summary>
        /// <param name="blockWrapper">The block wrapper.</param>
        /// <param name="blockControl">The block control.</param>
        /// <param name="block">The block.</param>
        /// <param name="canAdministrate">if set to <c>true</c> [can config].</param>
        /// <param name="canEdit">if set to <c>true</c> [can edit].</param>
        private void AddBlockConfig( HtmlGenericContainer blockWrapper, RockBlock blockControl,
            Rock.Web.Cache.BlockCache block, bool canAdministrate, bool canEdit )
        {
            if ( canAdministrate || canEdit )
            {
                // Add the config buttons
                HtmlGenericControl blockConfig = new HtmlGenericControl( "div" );
                blockConfig.ClientIDMode = ClientIDMode.AutoID;
                blockConfig.Attributes.Add( "class", "block-configuration config-bar" );
                blockWrapper.Controls.Add( blockConfig );

                HtmlGenericControl blockConfigLink = new HtmlGenericControl( "a" );
                //blockConfigLink.Attributes.Add( "class", "blockinstance-config" );
                blockConfigLink.Attributes.Add( "href", "#" );
                HtmlGenericControl iBlockConfig = new HtmlGenericControl( "i" );
                iBlockConfig.Attributes.Add( "class", "icon-circle-arrow-right" );
                blockConfigLink.Controls.Add( iBlockConfig );

                blockConfig.Controls.Add( blockConfigLink );

                HtmlGenericControl blockConfigBar = new HtmlGenericControl( "div" );
                blockConfigBar.Attributes.Add( "class", "block-configuration-bar config-bar" );
                blockConfig.Controls.Add( blockConfigBar );

                HtmlGenericControl blockConfigTitle = new HtmlGenericControl( "span" );
                if ( string.IsNullOrWhiteSpace( block.Name ) )
                    blockConfigTitle.InnerText = block.BlockType.Name;
                else
                    blockConfigTitle.InnerText = block.Name;
                blockConfigBar.Controls.Add( blockConfigTitle );

                foreach ( Control configControl in blockControl.GetAdministrateControls( canAdministrate, canEdit ) )
                {
                    configControl.ClientIDMode = ClientIDMode.AutoID;
                    blockConfigBar.Controls.Add( configControl );
                }
            }
        }

        /// <summary>
        /// Adds the block move.
        /// </summary>
        private void AddBlockMove()
        {
            // Add Zone Selection Popup (for moving blocks to another zone)
            ModalDialog modalBlockMove = new ModalDialog();
            modalBlockMove.ID = "modal-block-move";
            modalBlockMove.Title = "Move Block";
            modalBlockMove.OnOkScript = "Rock.admin.pageAdmin.saveBlockMove();";
            this.Form.Controls.Add( modalBlockMove );

            HtmlGenericControl fsZoneSelect = new HtmlGenericControl( "fieldset" );
            fsZoneSelect.ClientIDMode = ClientIDMode.Static;
            fsZoneSelect.Attributes.Add( "id", "fsZoneSelect" );
            modalBlockMove.Content.Controls.Add( fsZoneSelect );

            HtmlGenericControl legend = new HtmlGenericControl( "legend" );
            legend.InnerText = "New Location";
            fsZoneSelect.Controls.Add( legend );

            LabeledDropDownList ddlZones = new LabeledDropDownList();
            ddlZones.ClientIDMode = ClientIDMode.Static;
            ddlZones.ID = "block-move-zone";
            ddlZones.LabelText = "Zone";
            foreach ( var zone in Zones )
                ddlZones.Items.Add( new ListItem( zone.Value.Key, zone.Value.Value.ID ) );
            fsZoneSelect.Controls.Add( ddlZones );

            LabeledRadioButtonList rblLocation = new LabeledRadioButtonList();
            rblLocation.RepeatDirection = RepeatDirection.Horizontal;
            rblLocation.ClientIDMode = ClientIDMode.Static;
            rblLocation.ID = "block-move-Location";
            rblLocation.CssClass = "inputs-list";
            rblLocation.Items.Add( new ListItem( "Current Page" ) );
            rblLocation.Items.Add( new ListItem( string.Format( "All Pages Using the '{0}' Layout", CurrentPage.Layout ) ) );
            rblLocation.LabelText = "Parent";
            fsZoneSelect.Controls.Add( rblLocation );
        }

        #endregion

        #region Static Helper Methods

        /// <summary>
        /// Checks the page's RouteData values and then the query string for a
        /// parameter matching the specified name, and if found returns the string
        /// value
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public string PageParameter( string name )
        {
            if ( String.IsNullOrEmpty( name ) )
                return string.Empty;

            if ( Page.RouteData.Values.ContainsKey( name ) )
                return (string)Page.RouteData.Values[name];

            if ( String.IsNullOrEmpty( Request.QueryString[name] ) )
                return string.Empty;
            else
                return Request.QueryString[name];
        }

        /// <summary>
        /// Checks the page reference's parms and querystring for a
        /// parameter matching the specified name, and if found returns the string
        /// </summary>
        /// <param name="pageReference">The page reference.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public string PageParameter( PageReference pageReference, string name )
        {
            if ( String.IsNullOrEmpty( name ) )
                return string.Empty;

            if ( pageReference.Parameters.ContainsKey( name ) )
                return (string)pageReference.Parameters[name];

            if ( String.IsNullOrEmpty( pageReference.QueryString[name] ) )
                return string.Empty;
            else
                return pageReference.QueryString[name];
        }

        /// <summary>
        /// Gets the page route and query string parameters
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> PageParameters()
        {
            var parameters = new Dictionary<string, object>();

            foreach ( var key in Page.RouteData.Values.Keys )
            {
                parameters.Add( key, Page.RouteData.Values[key] );
            }

            foreach( string param in Request.QueryString.Keys)
            {
                parameters.Add( param, Request.QueryString[param]);
            }

            return parameters;
        }

        /// <summary>
        /// Adds a new CSS link that will be added to the page header prior to the page being rendered
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="href">Path to css file.  Should be relative to layout template.  Will be resolved at runtime</param>
        public static void AddCSSLink( Page page, string href )
        {
            AddCSSLink( page, href, string.Empty );
        }

        /// <summary>
        /// Adds the CSS link.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="href">Path to css file.  Should be relative to layout template.  Will be resolved at runtime</param>
        /// <param name="mediaType">Type of the media to use for the css link.</param>
        public static void AddCSSLink( Page page, string href, string mediaType )
        {
            HtmlLink htmlLink = new HtmlLink();

            htmlLink.Attributes.Add( "type", "text/css" );
            htmlLink.Attributes.Add( "rel", "stylesheet" );
            htmlLink.Attributes.Add( "href", page.ResolveUrl( href ) );
            if ( mediaType != string.Empty )
                htmlLink.Attributes.Add( "media", mediaType );

            AddHtmlLink( page, htmlLink, "css" );
        }

        /// <summary>
        /// Adds a meta tag.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="htmlMeta">The HTML meta tag.</param>
        public static void AddMetaTag( Page page, HtmlMeta htmlMeta )
        {
            if ( page != null && page.Header != null )
                if ( !HtmlMetaExists( page, htmlMeta ) )
                {
                    // Find last meta element
                    int index = 0;
                    for ( int i = page.Header.Controls.Count - 1; i >= 0; i-- )
                        if ( page.Header.Controls[i] is HtmlMeta )
                        {
                            index = i;
                            break;
                        }

                    if ( index == page.Header.Controls.Count )
                    {
                        page.Header.Controls.Add( new LiteralControl( "\n\t" ) );
                        page.Header.Controls.Add( htmlMeta );
                    }
                    else
                    {
                        page.Header.Controls.AddAt( ++index, new LiteralControl( "\n\t" ) );
                        page.Header.Controls.AddAt( ++index, htmlMeta );
                    }
                }
        }

        /// <summary>
        /// HTMLs the meta exists.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="newMeta">The new meta.</param>
        /// <returns></returns>
        private static bool HtmlMetaExists( Page page, HtmlMeta newMeta )
        {
            bool existsAlready = false;

            if ( page != null && page.Header != null )
                foreach ( Control control in page.Header.Controls )
                    if ( control is HtmlMeta )
                    {
                        HtmlMeta existingMeta = (HtmlMeta)control;

                        bool sameAttributes = true;

                        foreach ( string attributeKey in newMeta.Attributes.Keys )
                            if ( existingMeta.Attributes[attributeKey] != null &&
                                existingMeta.Attributes[attributeKey].ToLower() != newMeta.Attributes[attributeKey].ToLower() )
                            {
                                sameAttributes = false;
                                break;
                            }

                        if ( sameAttributes )
                        {
                            existsAlready = true;
                            break;
                        }
                    }
            return existsAlready;
        }

        /// <summary>
        /// Adds a new Html link that will be added to the page header prior to the page being rendered
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="htmlLink">The HTML link.</param>
        public static void AddHtmlLink( Page page, HtmlLink htmlLink, string contentPlaceHolderId = "" )
        {
            if ( page != null && page.Header != null )
            {
                var header = page.Header;
                if ( !HtmlLinkExists( header, htmlLink ) )
                {
                    bool inserted = false;

                    if ( !string.IsNullOrWhiteSpace( contentPlaceHolderId ) )
                    {
                        for ( int i = 0; i < header.Controls.Count; i++ )
                        {
                            if ( header.Controls[i] is ContentPlaceHolder )
                            {
                                var ph = (ContentPlaceHolder)header.Controls[i];
                                if ( ph.ID == contentPlaceHolderId )
                                {
                                    ph.Controls.Add( new LiteralControl( "\n\t" ) );
                                    ph.Controls.Add( htmlLink );

                                    inserted = true;
                                    break;
                                }
                            }
                        }
                    }

                    if ( !inserted )
                    {
                        header.Controls.Add( new LiteralControl( "\n\t" ) );
                        header.Controls.Add( htmlLink );
                    }

                }
            }
        }

        /// <summary>
        /// HTMLs the link exists.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <param name="newLink">The new link.</param>
        /// <returns></returns>
        private static bool HtmlLinkExists( Control parentControl, HtmlLink newLink )
        {
            bool existsAlready = false;

            if ( parentControl != null )
            {
                foreach ( Control control in parentControl.Controls )
                {
                    if ( control is ContentPlaceHolder )
                    {
                        if ( HtmlLinkExists( control, newLink ) )
                        {
                            existsAlready = true;
                            break;
                        }
                    }
                    else if ( control is HtmlLink )
                    {
                        HtmlLink existingLink = (HtmlLink)control;

                        bool sameAttributes = true;

                        foreach ( string attributeKey in newLink.Attributes.Keys )
                            if ( existingLink.Attributes[attributeKey] != null &&
                                existingLink.Attributes[attributeKey].ToLower() != newLink.Attributes[attributeKey].ToLower() )
                            {
                                sameAttributes = false;
                                break;
                            }

                        if ( sameAttributes )
                        {
                            existsAlready = true;
                            break;
                        }
                    }
                }
            }

            return existsAlready;
        }

        /// <summary>
        /// Adds a new script tag to the page header prior to the page being rendered
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="path">Path to script file.  Should be relative to layout template.  Will be resolved at runtime</param>
        public static void AddScriptLink( Page page, string path )
        {
            var scriptManager = ScriptManager.GetCurrent( page );

            if ( scriptManager != null )
            {
                scriptManager.Scripts.Add( new ScriptReference( path ) );
            }
        }

        #region User Preferences

        /// <summary>
        /// Gets the value for the current user for a given key
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public string GetUserPreference( string key )
        {
            var values = SessionUserPreferences();
            if ( values.ContainsKey( key ) )
                foreach ( string value in SessionUserPreferences()[key] )
                    return value;
            return string.Empty;
        }

        /// <summary>
        /// Gets the values for the current user that start with a given key.
        /// </summary>
        /// <param name="keyPrefix">The key prefix.</param>
        /// <returns></returns>
        public Dictionary<string, string> GetUserPreferences( string keyPrefix )
        {
            var selectedValues = new Dictionary<string,string>();

            var values = SessionUserPreferences();
            foreach(var key in values.Where ( v => v.Key.StartsWith(keyPrefix) ) )
            {
                string firstValue = string.Empty;
                foreach( string value in key.Value)
                {
                    firstValue = value;
                    break;
                }

                selectedValues.Add(key.Key, firstValue);
            }

            return selectedValues;
        }

        /// <summary>
        /// Sets a value for the current user for a given key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetUserPreference( string key, string value )
        {
            var newValues = new List<string>();
            newValues.Add( value );

            var sessionValues = SessionUserPreferences();
            if ( sessionValues.ContainsKey( key ) )
                sessionValues[key] = newValues;
            else
                sessionValues.Add( key, newValues );

            if ( CurrentPerson != null )
                new PersonService().SaveUserPreference( CurrentPerson, key, newValues, CurrentPersonId );
        }

        /// <summary>
        /// Sessions the user values.
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, List<string>> SessionUserPreferences()
        {
            string sessionKey = string.Format( "{0}_{1}",
                Person.USER_VALUE_ENTITY, CurrentPersonId.HasValue ? CurrentPersonId.Value : 0 );

            var userPreferences = Session[sessionKey] as Dictionary<string, List<string>>;
            if ( userPreferences == null )
            {
                if ( CurrentPerson != null )
                    userPreferences = new PersonService().GetUserPreferences( CurrentPerson );
                else
                    userPreferences = new Dictionary<string, List<string>>();

                Session[sessionKey] = userPreferences;
            }

            return userPreferences;
        }

        #endregion

        #endregion

        #region Event Handlers

        //void btnSaveAttributes_Click( object sender, EventArgs e )
        //{
        //    Button btnSave = ( Button )sender;
        //    int blockInstanceId = Convert.ToInt32( btnSave.ID.Replace( "attributes-", "" ).Replace( "-hide", "" ) );

        //    Cached.BlockInstance blockInstance = PageInstance.BlockInstances.Where( b => b.Id == blockInstanceId ).FirstOrDefault();
        //    if ( blockInstance != null )
        //    {
        //        // Find the container control
        //        Control blockWrapper = RecurseControls(this, string.Format("bid_{0}", blockInstance.Id));
        //        if ( blockWrapper != null )
        //        {
        //            foreach ( Rock.Web.Cache.Attribute attribute in blockInstance.Attributes )
        //            {
        //                //HtmlGenericControl editCell = ( HtmlGenericControl )blockWrapper.FindControl( string.Format( "attribute-{0}", attribute.Id.ToString() ) );
        //                Control control = blockWrapper.FindControl( string.Format( "attribute-field-{0}", attribute.Id.ToString() ) );
        //                if ( control != null )
        //                    blockInstance.AttributeValues[attribute.Key] = new KeyValuePair<string, string>( attribute.Name, attribute.FieldType.Field.ReadValue( control ) );
        //            }

        //            blockInstance.SaveAttributeValues( CurrentPersonId );

        //            if ( BlockInstanceAttributesUpdated != null )
        //                BlockInstanceAttributesUpdated( sender, new BlockInstanceAttributesUpdatedEventArgs( blockInstanceId ) );
        //        }
        //    }
        //}

        protected void scriptManager_Navigate(object sender, HistoryEventArgs e)
        {
            if (PageNavigate != null)
            {
                PageNavigate(this, e);
            }
        }

        /// <summary>
        /// Occurs when the ScriptManager detects a history change. This allows UpdatePanels to work when the
        /// browser's back button is pressed.
        /// </summary>
        public event PageNavigateEventHandler PageNavigate;

        #endregion
    }

    #region Event Argument Classes

    /// <summary>
    /// Delegate used for the ScriptManager's Navigate Event
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Web.UI.HistoryEventArgs"/> instance containing the history data.</param>
    public delegate void PageNavigateEventHandler(object sender, HistoryEventArgs e);

    /// <summary>
    /// Event Argument used when block instance properties are updated
    /// </summary>
    internal class BlockAttributesUpdatedEventArgs : EventArgs
    {
        public int BlockID { get; private set; }

        public BlockAttributesUpdatedEventArgs( int blockId )
        {
            BlockID = blockId;
        }
    }

    /// <summary>
    /// JSON Object used for client/server communication
    /// </summary>
    internal class JsonResult
    {
        /// <summary>
        /// Gets or sets the action.
        /// </summary>
        /// <value>
        /// The action.
        /// </value>
        public string Action { get; set; }

        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        /// <value>
        /// The result.
        /// </value>
        public object Result { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonResult"/> class.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="result">The result.</param>
        public JsonResult( string action, object result )
        {
            Action = action;
            Result = result;
        }

        /// <summary>
        /// Serializes this instance.
        /// </summary>
        /// <returns></returns>
        public string Serialize()
        {
            System.Web.Script.Serialization.JavaScriptSerializer serializer =
                new System.Web.Script.Serialization.JavaScriptSerializer();

            StringBuilder sb = new StringBuilder();

            serializer.Serialize( this, sb );

            return sb.ToString();
        }
    }


    #endregion

}

