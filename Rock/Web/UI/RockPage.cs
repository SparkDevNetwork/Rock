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
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Transactions;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

using Page = System.Web.UI.Page;

namespace Rock.Web.UI
{
    /// <summary>
    /// RockPage is the base abstract class that all page templates in Rock should inherit from
    /// </summary>
    public abstract class RockPage : Page
    {
        #region Private Variables

        private PlaceHolder phLoadStats;
        private LinkButton _btnRestoreImpersonatedByUser;
        private ScriptManager _scriptManager;
        private PageCache _pageCache = null;

        private string _clientType = null;
        private BrowserInfo _browserInfo = null;
        private BrowserClient _browserClient = null;

        private PageStatePersister _PageStatePersister = null;
        #endregion

        #region Protected Variables

        /// <summary>
        /// The full name of the currently logged in user
        /// </summary>
        protected string UserName = string.Empty;

        /// <summary>
        /// Gets a dictionary of the current context items (models).
        /// </summary>
        internal Dictionary<string, Rock.Data.KeyEntity> ModelContext = new Dictionary<string, Data.KeyEntity>();

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the current <see cref="Rock.Model.Page">Page's</see> logical Rock Page Id.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the PageId of the current logical <see cref="Rock.Model.Page"/>.
        /// </value>
        public int PageId
        {
            get { return _pageCache.Id; }
        }

        /// <summary>
        /// Gets or sets the browser title.
        /// </summary>
        /// <value>
        /// The browser title.
        /// </value>
        public string BrowserTitle { get; set; }

        /// <summary>
        /// Gets or sets the page title.
        /// </summary>
        /// <value>
        /// The page title.
        /// </value>
        public string PageTitle { get; set; }

        /// <summary>
        /// Gets or sets the page title.
        /// </summary>
        /// <value>
        /// The page icon.
        /// </value>
        public string PageIcon { get; set; }

        /// <summary>
        /// Gets the title for the page and sets both the browser and template title
        /// </summary>
        /// <returns>The title of the page.</returns>
        public new string Title
        {
            get
            {
                return BrowserTitle;
            }
            set
            {
                BrowserTitle = value;
                PageTitle = value;
            }
        }

        /// <summary>
        /// Gets the current <see cref="Rock.Model.Page">Page's</see> logical Rock Page Guid.
        /// </summary>
        /// <value>
        /// A <see cref="System.Guid"/> representing the Guid identifier for the current logical <see cref="Rock.Model.Page"/>
        /// </value>
        public Guid Guid
        {
            get { return _pageCache.Guid; }
        }

        /// <summary>
        /// Gets or sets the body CSS class.
        /// </summary>
        /// <value>
        /// The body CSS class.
        /// </value>
        public string BodyCssClass { get; set; }

        /// <summary>
        /// Gets the current <see cref="Rock.Model.Page">Page's</see> layout.
        /// </summary>
        /// <value>
        /// The <see cref="LayoutCache"/> representing the current <see cref="Rock.Model.Page">Page's</see> layout.
        /// </value>
        public LayoutCache Layout
        {
            get { return _pageCache.Layout; }
        }

        /// <summary>
        /// Gets the <see cref="Rock.Model.Site"/> that the current <see cref="Rock.Model.Page"/> is on.
        /// </summary>
        /// <value>
        /// A <see cref="SiteCache"/> representing the <see cref="Rock.Model.Site"/> that the current <see cref="Rock.Model.Page"/>
        /// is on.
        /// </value>
        public new SiteCache Site
        {
            get { return _pageCache.Layout.Site; }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Rock.Web.PageReference"/>
        /// </summary>
        /// <value>
        /// The current <see cref="Rock.Web.PageReference"/>.
        /// </value>
        public PageReference PageReference
        {
            get
            {
                if ( _PageReference == null && Context.Items.Contains( "Rock:PageReference" ) )
                {
                    _PageReference = Context.Items["Rock:PageReference"] as PageReference;
                }

                return _PageReference;
            }

            set
            {
                _PageReference = value;
                SaveContextItem( "Rock:PageReference", _PageReference );
            }
        }
        private PageReference _PageReference = null;

        /// <summary>
        /// Public gets and privately sets the content areas on a layout page that blocks can be added to.
        /// </summary>
        /// <value>
        /// A <see cref="System.Collections.Generic.Dictionary{String, KeyValuePair}"/> representing the content
        /// areas on the page that content can be added to. Each <see cref="System.Collections.Generic.KeyValuePair{String, KeyValuePair}"/>
        /// has a Key representing the <see cref="Rock.Web.UI.Controls.Zone">Zone's</see> ZoneKey and a value containing
        /// a <see cref="System.Collections.Generic.Dictionary{String, Zone}"/> with the key referencing the <see cref="Rock.Web.UI.Controls.Zone">Zone's</see>
        /// friendly name and the <see cref="Rock.Web.UI.Controls.Zone"/>.
        /// </value>
        /// <remarks>
        /// The Dictionary's key is the zonekey and the KeyValuePair is a combination
        /// of the friendly zone name and the zone control
        /// </remarks>
        public Dictionary<string, KeyValuePair<string, Zone>> Zones { get; private set; }

        /// <summary>
        /// Publicly Gets and privately sets a list containing the Page's <see cref="Rock.Web.UI.BreadCrumb">BreadCrumbs</see>
        /// </summary>
        /// <value>
        /// A <see cref="System.Collections.Generic.List{BreakdCrumb}"/> containing the <see cref="Rock.Web.UI.BreadCrumb">BreadCrumbs</see>
        /// for this Page.
        /// </value>
        public List<BreadCrumb> BreadCrumbs { get; private set; }

        /// <summary>
        /// Publicly gets and privately sets the currently logged in user.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.UserLogin"/> of the currently logged in user.
        /// </value>
        public Rock.Model.UserLogin CurrentUser
        {
            get
            {
                if ( _CurrentUser != null )
                {
                    return _CurrentUser;
                }

                if ( Context.Items.Contains( "CurrentUser" ) )
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

                if ( _CurrentUser != null && _CurrentUser.Person != null && _currentPerson == null )
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
        /// Publicly gets the current <see cref="Rock.Model.Person"/>.  This is either the currently logged in user, or if the user
        /// has not logged in, it may also be an impersonated person determined from using the encrypted
        /// person key.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.Person"/> representing the currently logged in person or impersonated person.
        /// </value>
        public Person CurrentPerson
        {
            get
            {
                if ( _currentPerson != null )
                {
                    return _currentPerson;
                }

                if ( _currentPerson == null && Context.Items.Contains( "CurrentPerson" ) )
                {
                    _currentPerson = Context.Items["CurrentPerson"] as Person;
                    return _currentPerson;
                }

                return null;
            }

            private set
            {
                Context.Items.Remove( "CurrentPerson" );

                _currentPerson = value;
                if ( _currentPerson != null )
                {
                    Context.Items.Add( "CurrentPerson", value );
                }

                _currentPersonAlias = null;
            }
        }
        private Person _currentPerson;

        /// <summary>
        /// The Person ID of the currently logged in user.  Returns null if there is not a user logged in
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32" /> representing the PersonId of the <see cref="Rock.Model.Person"/>
        /// who is logged in as the current user. If a user is not logged in.
        /// </value>
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
        /// Gets the current person alias.
        /// </summary>
        /// <value>
        /// The current person alias.
        /// </value>
        public PersonAlias CurrentPersonAlias
        {
            get
            {
                if ( _currentPersonAlias != null )
                {
                    return _currentPersonAlias;
                }

                if ( _currentPerson != null )
                {
                    _currentPersonAlias = _currentPerson.PrimaryAlias;
                    return _currentPersonAlias;
                }

                return null;
            }
        }
        private PersonAlias _currentPersonAlias = null;

        /// <summary>
        /// Gets the current person alias identifier.
        /// </summary>
        /// <value>
        /// The current person alias identifier.
        /// </value>
        public int? CurrentPersonAliasId
        {
            get
            {
                if ( CurrentPersonAlias != null )
                {
                    return CurrentPersonAlias.Id;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the all the <see cref="Rock.Web.UI.RockBlock">RockBlocks</see> on the Page.
        /// </summary>
        /// <value>
        /// A <see cref="System.Collections.Generic.List{RockBlock}"/> containing all the <see cref="Rock.Web.UI.RockBlock">RockBlocks</see> on the page.
        /// </value>
        public List<RockBlock> RockBlocks
        {
            get
            {
                return this.ControlsOfTypeRecursive<RockBlock>();
            }
        }

        /// <summary>
        /// Gets the type of the client.
        /// </summary>
        /// <value>
        /// The type of the client.
        /// </value>
        public string ClientType
        {
            get
            {
                if ( _clientType == null )
                {
                    _clientType = InteractionDeviceType.GetClientType( Request.UserAgent ?? "" );
                }
                return _clientType;
            }

        }

        /// <summary>
        /// Gets the client information.
        /// </summary>
        /// <value>
        /// The client information.
        /// </value>
        public BrowserInfo BrowserInfo
        {
            get
            {
                if ( _browserInfo == null )
                {
                    _browserInfo = new BrowserInfo( Request.UserAgent ?? "" );
                }
                return _browserInfo;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is mobile request.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is mobile request; otherwise, <c>false</c>.
        /// </value>
        public bool IsMobileRequest
        {
            get
            {
                return this.ClientType == "Mobile";
            }
        }

        /// <summary>
        /// Gets a single object that contains all of the information about the web browser.
        /// </summary>
        /// <value>
        /// The browser client.
        /// </value>
        public BrowserClient BrowserClient
        {
            get
            {
                if ( _browserClient == null )
                {
                    _browserClient = new BrowserClient();
                    _browserClient.BrowserInfo = BrowserInfo;
                    _browserClient.IsMobile = IsMobileRequest;
                    _browserClient.ClientType = ClientType;
                }
                return _browserClient;
            }
        }

        /// <summary>
        /// Gets the size of the view state.
        /// </summary>
        /// <value>
        /// The size of the view state.
        /// </value>
        public int ViewStateSize { get; private set; }


        /// <summary>
        /// Gets the view state size compressed.
        /// </summary>
        /// <value>
        /// The view state size compressed.
        /// </value>
        public int ViewStateSizeCompressed { get; private set; }

        /// <summary>
        /// Gets the view state value.
        /// </summary>
        /// <value>
        /// The view state value.
        /// </value>
        public string ViewStateValue { get; private set; }

        /// <summary>
        /// Gets a value indicating whether [view state is compressed].
        /// </summary>
        /// <value>
        /// <c>true</c> if [view state is compressed]; otherwise, <c>false</c>.
        /// </value>
        public bool ViewStateIsCompressed { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable view state inspection].
        /// </summary>
        /// <value>
        /// <c>true</c> if [enable view state inspection]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableViewStateInspection { get; set; }

    #endregion

        #region Overridden Properties

        /// <summary>
        /// Gets the PageStatePersister object associated with the page.
        /// </summary>
        protected override PageStatePersister PageStatePersister
        {
            get {
                if ( _PageStatePersister == null )
                {
                    _PageStatePersister = new RockHiddenFieldPageStatePersister( this, RockHiddenFieldPageStatePersister.ViewStateCompressionThreshold );
                }
                return _PageStatePersister;
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Recurses the page's <see cref="System.Web.UI.ControlCollection"/> looking for any <see cref="Rock.Web.UI.Controls.Zone"/> controls
        /// </summary>
        /// <param name="controls">A <see cref="System.Web.UI.ControlCollection"/> containing the page's controls.</param>
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
                            Zones.Add( zone.Name.Replace( " ", "" ), new KeyValuePair<string, Zone>( zone.Name, zone ) );
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
        /// <param name="zoneName">A <see cref="System.String"/> representing the name of the zone.</param>
        /// <returns>The <see cref="System.Web.UI.Control"/> for the zone, if the zone is not found, the form control is returned.</returns>
        [RockObsolete( "1.7" )]
        [Obsolete("Use the other FindZone()", true )]
        protected virtual Control FindZone( string zoneName )
        {
            // Find the zone, or use the Form if not found
            return FindZone( zoneName, this.Form );
        }

        /// <summary>
        /// Find the <see cref="Rock.Web.UI.Controls.Zone" /> for the specified zone name.  Looks in the
        /// <see cref="Zones" /> property to see if it has been defined.  If an existing zone
        /// <see cref="Rock.Web.UI.Controls.Zone" /> cannot be found, the defaultZone will be returned
        /// </summary>
        /// <param name="zoneName">A <see cref="System.String" /> representing the name of the zone.</param>
        /// <param name="defaultZone">The default zone.</param>
        /// <returns>
        /// The <see cref="System.Web.UI.Control" /> for the zone, if the zone is not found, the defaultZone is returned.
        /// </returns>
        protected virtual Control FindZone( string zoneName, Control defaultZone )
        {
            // First look in the Zones dictionary
            if ( Zones.ContainsKey( zoneName ) )
                return Zones[zoneName].Value;

            // If no match, return the defaultZone
            return defaultZone;
        }

        #endregion

        #region Custom Events
        /// <summary>
        /// Occurs when view state persisted.
        /// </summary>
        public event EventHandler ViewStatePersisted;

        /// <summary>
        /// Called when [view state persisted].
        /// </summary>
        protected void OnViewStatePersisted()
        {
            if ( this.ViewStatePersisted != null )
            {
                ViewStatePersisted( this, EventArgs.Empty );
            }
        }

        #endregion

        #region Overridden Methods

        /// <summary>
        /// Saves any view-state and control-state information for the page.
        /// </summary>
        /// <param name="state">An <see cref="T:System.Object" /> in which to store the view-state information.</param>
        protected override void SavePageStateToPersistenceMedium( object state )
        {
            base.SavePageStateToPersistenceMedium( state );

            var customPersister = this.PageStatePersister as RockHiddenFieldPageStatePersister;

            if (customPersister != null )
            {
                this.ViewStateValue = customPersister.ViewStateValue;
            }

            OnViewStatePersisted();
        }

        /// <summary>
        /// Initializes the page's culture to use the culture specified by the browser ("auto")
        /// </summary>
        protected override void InitializeCulture()
        {
            base.UICulture = "Auto";
            base.Culture = "Auto";

            base.InitializeCulture();
        }

        /// <summary>
        /// Loads all of the configured blocks for the current page into the control tree
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit( EventArgs e )
        {
            var slDebugTimings = new StringBuilder();
            var stopwatchInitEvents = Stopwatch.StartNew();
            bool showDebugTimings = this.PageParameter( "ShowDebugTimings" ).AsBoolean();
            bool canAdministratePage = false;
            bool canEditPage = false;

            if ( showDebugTimings )
            {
                TimeSpan tsDuration = RockDateTime.Now.Subtract( (DateTime)Context.Items["Request_Start_Time"] );
                slDebugTimings.AppendFormat( "OnInit [{0}ms] @ {1} \n", stopwatchInitEvents.Elapsed.TotalMilliseconds, tsDuration.TotalMilliseconds );
                stopwatchInitEvents.Restart();
            }

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

            // increase the postback timeout
            _scriptManager.AsyncPostBackTimeout = 180;

            // wire up navigation event
            _scriptManager.Navigate += new EventHandler<HistoryEventArgs>( scriptManager_Navigate );

            // Add library and UI bundles during init, that way theme developers will only
            // need to worry about registering any custom scripts or script bundles they need
            _scriptManager.Scripts.Add( new ScriptReference( "~/Bundles/WebFormsJs" ) );
            _scriptManager.Scripts.Add( new ScriptReference( "~/Scripts/Bundles/RockLibs" ) );
            _scriptManager.Scripts.Add( new ScriptReference( "~/Scripts/Bundles/RockUi" ) );
            _scriptManager.Scripts.Add( new ScriptReference( "~/Scripts/Bundles/RockValidation" ) );

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

            if ( showDebugTimings )
            {
                slDebugTimings.AppendFormat( "CheckingForLogout [{0}ms]\n", stopwatchInitEvents.Elapsed.TotalMilliseconds );
                stopwatchInitEvents.Restart();
            }

            // If the logout parameter was entered, delete the user's forms authentication cookie and redirect them
            // back to the same page.
            Page.Trace.Warn( "Checking for logout request" );
            if ( PageParameter( "logout" ) != string.Empty )
            {
                if ( CurrentUser != null )
                {
                    var transaction = new Rock.Transactions.UserLastActivityTransaction();
                    transaction.UserId = CurrentUser.Id;
                    transaction.LastActivityDate = RockDateTime.Now;
                    transaction.IsOnLine = false;
                    Rock.Transactions.RockQueue.TransactionQueue.Enqueue( transaction );
                }

                Authorization.SignOut();

                // After logging out check to see if an anonymous user is allowed to view the current page.  If so
                // redirect back to the current page, otherwise redirect to the site's default page
                if ( _pageCache != null )
                {
                    if ( _pageCache.IsAuthorized( Authorization.VIEW, null ) )
                    {
                        // Remove the 'logout' queryparam before redirecting
                        var pageReference = new PageReference( PageReference.PageId, PageReference.RouteId, PageReference.Parameters );
                        foreach ( string key in PageReference.QueryString )
                        {
                            if ( key != null && !key.Equals( "logout", StringComparison.OrdinalIgnoreCase ) )
                            {
                                pageReference.Parameters.Add( key, PageReference.QueryString[key] );
                            }
                        }
                        Response.Redirect( pageReference.BuildUrl(), false );
                        Context.ApplicationInstance.CompleteRequest();
                    }
                    else
                    {
                        _pageCache.Layout.Site.RedirectToDefaultPage();
                    }
                    return;
                }
                else
                {
                    CurrentPerson = null;
                    CurrentUser = null;
                }
            }

            var rockContext = new RockContext();

            if ( showDebugTimings )
            {
                slDebugTimings.AppendFormat( "CreateRockContext [{0}ms]\n", stopwatchInitEvents.Elapsed.TotalMilliseconds );
                stopwatchInitEvents.Restart();
            }

            // If the impersonated query key was included or is in session then set the current person
            Page.Trace.Warn( "Checking for person impersonation" );
            if (!ProcessImpersonation( rockContext ) )
            {
                return;
            }

            // Get current user/person info
            Page.Trace.Warn( "Getting CurrentUser" );
            Rock.Model.UserLogin user = CurrentUser;

            if ( showDebugTimings )
            {
                slDebugTimings.AppendFormat( "GetCurrentUser [{0}ms]\n", stopwatchInitEvents.Elapsed.TotalMilliseconds );
                stopwatchInitEvents.Restart();
            }

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
                        Rock.Model.PersonService personService = new Model.PersonService( rockContext );
                        Rock.Model.Person person = personService.Get( personId.Value );
                        if ( person != null )
                        {
                            UserName = person.FullName;
                            CurrentPerson = person;
                        }

                        Session[personNameKey] = UserName;
                    }
                }

                if ( showDebugTimings )
                {
                    slDebugTimings.AppendFormat( "GetCurrentPerson [{0}ms]\n", stopwatchInitEvents.Elapsed.TotalMilliseconds );
                    stopwatchInitEvents.Restart();
                }

                // check that they aren't required to change their password
                if ( user.IsPasswordChangeRequired == true && Site.ChangePasswordPageReference != null )
                {
                    // don't redirect if this is the change password page
                    if ( Site.ChangePasswordPageReference.PageId != this.PageId )
                    {
                        Site.RedirectToChangePasswordPage( true, true );
                    }
                }

                // Check if there is a ROCK_PERSONALDEVICE_ADDRESS cookie, link person to device
                HandleRockWiFiCookie( CurrentPersonAliasId );
            }

            // If a PageInstance exists
            if ( _pageCache != null )
            {
                BrowserTitle = _pageCache.BrowserTitle;
                PageTitle = _pageCache.PageTitle;
                PageIcon = _pageCache.IconCssClass;
                BodyCssClass = _pageCache.BodyCssClass;

                // If there's a master page, update its reference to Current Page
                if ( this.Master is RockMasterPage )
                {
                    ( (RockMasterPage)this.Master ).SetPage( _pageCache );
                }

                // Add CSS class to body
                if ( !string.IsNullOrWhiteSpace( this.BodyCssClass ) )
                {
                    // attempt to find the body tag
                    var body = (HtmlGenericControl)this.Master.FindControl( "body" );
                    if ( body != null )
                    {
                        // determine if we need to append or add the class
                        if ( body.Attributes["class"] != null )
                        {
                            body.Attributes["class"] += " " + this.BodyCssClass;
                        }
                        else
                        {
                            body.Attributes.Add( "class", this.BodyCssClass );
                        }
                    }
                }

                // Add Favicon
                if ( Site.FavIconBinaryFileId.HasValue )
                {
                    AddIconLink( Site.FavIconBinaryFileId.Value, 192, "shortcut icon" );
                    AddIconLink( Site.FavIconBinaryFileId.Value, 16 );
                    AddIconLink( Site.FavIconBinaryFileId.Value, 32 );
                    AddIconLink( Site.FavIconBinaryFileId.Value, 144 );
                    AddIconLink( Site.FavIconBinaryFileId.Value, 180 );
                    AddIconLink( Site.FavIconBinaryFileId.Value, 192 );
                }

                // check if page should have been loaded via ssl
                Page.Trace.Warn( "Checking for SSL request" );
                if ( !WebRequestHelper.IsSecureConnection(HttpContext.Current)  && ( _pageCache.RequiresEncryption || Site.RequiresEncryption ) )
                {
                    string redirectUrl = Request.Url.ToString().Replace( "http:", "https:" );

                    // Clear the session state cookie so it can be recreated as secured (see engineering note in Global.asax EndRequest)
                    SessionStateSection sessionState = ( SessionStateSection ) ConfigurationManager.GetSection( "system.web/sessionState" );
                    string sidCookieName = sessionState.CookieName; // ASP.NET_SessionId
                    Response.Cookies[sidCookieName].Expires = DateTime.Now.AddDays( -1 );

                    Response.Redirect( redirectUrl, false );
                    Context.ApplicationInstance.CompleteRequest();
                    return;
                }

                // Verify that the current user is allowed to view the page.
                Page.Trace.Warn( "Checking if user is authorized" );

                var isCurrentPersonAuthorized = _pageCache.IsAuthorized( Authorization.VIEW, CurrentPerson );

                if ( showDebugTimings )
                {
                    slDebugTimings.AppendFormat( "isCurrentPersonAuthorized [{0}ms]\n", stopwatchInitEvents.Elapsed.TotalMilliseconds );
                    stopwatchInitEvents.Restart();
                }

                if ( !isCurrentPersonAuthorized )
                {
                    if ( user == null )
                    {
                        // If not authorized, and the user hasn't logged in yet, redirect to the login page
                        Page.Trace.Warn( "Redirecting to login page" );

                        var site = _pageCache.Layout.Site;
                        if ( site.LoginPageId.HasValue )
                        {
                            site.RedirectToLoginPage( true );
                        }
                        else
                        {
                            FormsAuthentication.RedirectToLoginPage();
                        }
                    }
                    else
                    {
                        // If not authorized, and the user has logged in, redirect to error page
                        Page.Trace.Warn( "Redirecting to error page" );

                        if ( Site != null && !string.IsNullOrWhiteSpace( Site.ErrorPage ) )
                        {
                            Context.Response.Redirect( string.Format( "{0}?type=security", Site.ErrorPage.TrimEnd( new char[] { '/' } ) ), false );
                            Context.ApplicationInstance.CompleteRequest();
                            return;
                        }
                        else
                        {
                            Response.Redirect( "~/Error.aspx?type=security", false );
                            Context.ApplicationInstance.CompleteRequest();
                        }
                    }
                }
                else
                {
                    // Set current models (context)
                    Page.Trace.Warn( "Checking for Context" );
                    try
                    {
                        char[] delim = new char[1] { ',' };

                        // Check to see if a context from query string should be saved to a cookie first
                        foreach ( string param in PageParameter( "SetContext", true ).Split( delim, StringSplitOptions.RemoveEmptyEntries ) )
                        {
                            string[] parts = param.Split( '|' );
                            if ( parts.Length == 2 )
                            {
                                var contextModelEntityType = EntityTypeCache.Get( parts[0], false, rockContext );
                                int? contextId = parts[1].AsIntegerOrNull();

                                if ( contextModelEntityType != null && contextId.HasValue )
                                {
                                    var contextModelType = contextModelEntityType.GetEntityType();
                                    var contextDbContext = Reflection.GetDbContextForEntityType( contextModelType );
                                    if ( contextDbContext != null )
                                    {
                                        var contextService = Reflection.GetServiceForEntityType( contextModelType, contextDbContext );
                                        if ( contextService != null )
                                        {
                                            MethodInfo getMethod = contextService.GetType().GetMethod( "Get", new Type[] { typeof( int ) } );
                                            if ( getMethod != null )
                                            {
                                                var getResult = getMethod.Invoke( contextService, new object[] { contextId.Value } );
                                                var contextEntity = getResult as IEntity;
                                                if ( contextEntity != null )
                                                {
                                                    SetContextCookie( contextEntity, false, false );
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if ( showDebugTimings )
                        {
                            slDebugTimings.AppendFormat( "Set Page Context(s) [{0}ms]\n", stopwatchInitEvents.Elapsed.TotalMilliseconds );
                            stopwatchInitEvents.Restart();
                        }

                        // first search the cookies for any saved context, but pageContext can replace it
                        GetCookieContext( GetContextCookieName( false ) );      // Site
                        GetCookieContext( GetContextCookieName( true ) );       // Page (will replace any site values)

                        // check to see if any of the ModelContext.Keys that got set from Cookies are on the URL. If so, the URL value overrides the Cookie value
                        foreach ( var modelContextName in ModelContext.Keys.ToList() )
                        {
                            var type = Type.GetType( modelContextName, false, false );
                            if ( type != null )
                            {
                                int? contextId = PageParameter( type.Name + "Id" ).AsIntegerOrNull();
                                if ( contextId.HasValue )
                                {
                                    ModelContext.AddOrReplace( modelContextName, new Data.KeyEntity( contextId.Value ) );
                                }
                            }
                        }

                        // check for page context (that were explicitly set in Page Properties)
                        foreach ( var pageContext in _pageCache.PageContexts )
                        {
                            int? contextId = PageParameter( pageContext.Value ).AsIntegerOrNull();
                            if ( contextId.HasValue )
                            {
                                ModelContext.AddOrReplace( pageContext.Key, new Data.KeyEntity( contextId.Value ) );
                            }
                        }

                        // check for any encrypted contextkeys specified in query string
                        foreach ( string param in PageParameter( "context", true ).Split( delim, StringSplitOptions.RemoveEmptyEntries ) )
                        {
                            string contextItem = Rock.Security.Encryption.DecryptString( param );
                            string[] parts = contextItem.Split( '|' );
                            if ( parts.Length == 2 )
                            {
                                ModelContext.AddOrReplace( parts[0], new Data.KeyEntity( parts[1] ) );
                            }
                        }

                        if ( showDebugTimings )
                        {
                            slDebugTimings.AppendFormat( "Check Page Context(s) [{0}ms]\n", stopwatchInitEvents.Elapsed.TotalMilliseconds );
                            stopwatchInitEvents.Restart();
                        }

                    }
                    catch
                    {
                        // intentionally ignore exception
                    }

                    // set viewstate on/off
                    this.EnableViewState = _pageCache.EnableViewState;

                    Page.Trace.Warn( "Checking if user can administer" );
                    canAdministratePage = _pageCache.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );
                    canEditPage = _pageCache.IsAuthorized( Authorization.EDIT, CurrentPerson );

                    // If the current person isn't allowed to edit or administrate the page, check to see if they are being impersonated by someone who
                    // may have edit and/or administrate access to the page.
                    if ( !canAdministratePage || !canEditPage )
                    {
                        // if the current user is being impersonated by another user (typically an admin), then check their security
                        var impersonatedByUser = Session["ImpersonatedByUser"] as UserLogin;
                        var currentUserIsImpersonated = ( HttpContext.Current?.User?.Identity?.Name ?? string.Empty ).StartsWith( "rckipid=" );
                        if ( impersonatedByUser != null && currentUserIsImpersonated )
                        {
                            canAdministratePage = canAdministratePage || _pageCache.IsAuthorized( Authorization.ADMINISTRATE, impersonatedByUser.Person );
                            canEditPage = canEditPage || _pageCache.IsAuthorized( Authorization.EDIT, impersonatedByUser.Person );
                        }
                    }

                    if ( showDebugTimings )
                    {
                        slDebugTimings.AppendFormat( "canAdministratePage [{0}ms]\n", stopwatchInitEvents.Elapsed.TotalMilliseconds );
                        stopwatchInitEvents.Restart();
                    }

                    // Create a javascript object to store information about the current page for client side scripts to use
                    Page.Trace.Warn( "Creating JS objects" );
                    if ( !ClientScript.IsStartupScriptRegistered( "rock-js-object" ) )
                    {
                        string script = string.Format( @"
    Rock.settings.initialize({{
        siteId: {0},
        layoutId: {1},
        pageId: {2},
        layout: '{3}',
        baseUrl: '{4}'
    }});",
                            _pageCache.Layout.SiteId, _pageCache.LayoutId, _pageCache.Id, _pageCache.Layout.FileName, ResolveUrl( "~" ) );

                        ClientScript.RegisterStartupScript( this.Page.GetType(), "rock-js-object", script, true );
                    }

                    AddTriggerPanel();

                    // Add config elements
                    if ( _pageCache.IncludeAdminFooter )
                    {
                        Page.Trace.Warn( "Adding popup controls (footer elements)" );
                        AddPopupControls();

                        Page.Trace.Warn( "Adding zone elements" );
                        AddZoneElements( canAdministratePage );
                    }

                    // Initialize the list of breadcrumbs for the current page (and blocks on the page)
                    Page.Trace.Warn( "Setting breadcrumbs" );
                    PageReference.BreadCrumbs = new List<BreadCrumb>();

                    // If the page is configured to display in the breadcrumbs...
                    string bcName = _pageCache.BreadCrumbText;
                    if ( bcName != string.Empty )
                    {
                        PageReference.BreadCrumbs.Add( new BreadCrumb( bcName, PageReference.BuildUrl() ) );
                    }

                    // Add the Google Analytics Code script if a code was specified for the site
                    if ( !string.IsNullOrWhiteSpace( _pageCache.Layout.Site.GoogleAnalyticsCode ) )
                    {
                        AddGoogleAnalytics( _pageCache.Layout.Site.GoogleAnalyticsCode );
                    }

                    // Flag indicating if user has rights to administer one or more of the blocks on page
                    bool canAdministrateBlockOnPage = false;

                    if ( showDebugTimings )
                    {
                        slDebugTimings.AppendFormat( "start loading blocks [{0}ms]\n", stopwatchInitEvents.Elapsed.TotalMilliseconds );
                        stopwatchInitEvents.Restart();
                    }

                    // If the block's AttributeProperty values have not yet been verified verify them.
                    // (This provides a mechanism for block developers to define the needed block
                    //  attributes in code and have them automatically added to the database)
                    Page.Trace.Warn( "\tChecking if block attributes need refresh" );
                    VerifyBlockTypeInstanceProperties();

                    // Load the blocks and insert them into page zones
                    Page.Trace.Warn( "Loading Blocks" );
                    var pageBlocks = _pageCache.Blocks;

                    foreach ( BlockCache block in pageBlocks )
                    {
                        var stopwatchBlockInit= Stopwatch.StartNew();
                        Page.Trace.Warn( string.Format( "\tLoading '{0}' block", block.Name ) );

                        // Get current user's permissions for the block instance
                        Page.Trace.Warn( "\tChecking permission" );
                        bool canAdministrate = block.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );
                        bool canEdit = block.IsAuthorized( Authorization.EDIT, CurrentPerson );
                        bool canView = block.IsAuthorized( Authorization.VIEW, CurrentPerson );

                        // if this is a Site-wide block, only render it if its Zone exists on this page
                        // In other cases, Rock will add the block to the Form (at the very bottom of the page)
                        Control zone = FindZone( block.Zone, block.BlockLocation == BlockLocation.Site ? null : this.Form );

                        // Make sure there is a Zone for the block, and make sure user has access to view block instance
                        if ( zone != null && (canAdministrate || canEdit || canView) )
                        {
                            // Load the control and add to the control tree
                            Page.Trace.Warn( "\tLoading control" );
                            Control control = null;

                            // Check to see if block is configured to use a "Cache Duration'
                            if ( block.OutputCacheDuration > 0 )
                            {
                                // Cache object used for block output caching
                                Page.Trace.Warn( "Getting memory cache" );
                                string blockCacheKey = string.Format( "Rock:BlockOutput:{0}", block.Id );
                                var blockCacheString = RockCache.Get( blockCacheKey ) as string;
                                if ( blockCacheString.IsNotNullOrWhiteSpace() )
                                {
                                    // If the current block exists in our custom output cache, add the cached output instead of adding the control
                                    control = new LiteralControl( blockCacheString );
                                }
                            }

                            if ( control == null )
                            {
                                try
                                {
                                    control = TemplateControl.LoadControl( block.BlockType.Path );
                                    control.ClientIDMode = ClientIDMode.AutoID;
                                }
                                catch ( Exception ex )
                                {
                                    try
                                    {
                                        LogException( ex );
                                    }
                                    catch
                                    {
                                        //
                                    }

                                    NotificationBox nbBlockLoad = new NotificationBox();
                                    nbBlockLoad.ID = string.Format( "nbBlockLoad_{0}", block.Id );
                                    nbBlockLoad.CssClass = "system-error";
                                    nbBlockLoad.NotificationBoxType = NotificationBoxType.Danger;
                                    nbBlockLoad.Text = string.Format( "Error Loading Block: {0}", block.Name );
                                    nbBlockLoad.Details = string.Format( "{0}<pre>{1}</pre>", HttpUtility.HtmlEncode( ex.Message ), HttpUtility.HtmlEncode( ex.StackTrace ) );
                                    nbBlockLoad.Dismissable = true;
                                    control = nbBlockLoad;

                                    if ( this.IsPostBack )
                                    {
                                        // throw an error on PostBack so that the ErrorPage gets shown (vs nothing happening)
                                        throw;
                                    }
                                }
                            }

                            if ( control != null )
                            {
                                if ( canAdministrate || ( canEdit && control is RockBlockCustomSettings ) )
                                {
                                    canAdministrateBlockOnPage = true;
                                }

                                // If the current control is a block, set its properties
                                var blockControl = control as RockBlock;
                                if ( blockControl != null )
                                {
                                    Page.Trace.Warn( "\tSetting block properties" );
                                    blockControl.SetBlock( _pageCache, block, canEdit, canAdministrate );
                                    control = new RockBlockWrapper( blockControl );

                                    // Add any breadcrumbs to current page reference that the block creates
                                    Page.Trace.Warn( "\tAdding any breadcrumbs from block" );
                                    if ( block.BlockLocation == BlockLocation.Page )
                                    {
                                        blockControl.GetBreadCrumbs( PageReference ).ForEach( c => PageReference.BreadCrumbs.Add( c ) );
                                    }

                                    // If the blocktype's security actions have not yet been loaded, load them now
                                    block.BlockType.SetSecurityActions( blockControl );
                                }
                            }

                            zone.Controls.Add( control );
                            if ( control is RockBlockWrapper )
                            {
                                ( (RockBlockWrapper)control ).EnsureBlockControls();
                            }

                            if ( showDebugTimings )
                            {
                                stopwatchBlockInit.Stop();
                                slDebugTimings.AppendFormat(
                                    "create/init block {0} <span class='label label-{2}'>[{1}ms]</span>\n",
                                    block.Name,
                                    stopwatchBlockInit.Elapsed.TotalMilliseconds,
                                    stopwatchBlockInit.Elapsed.TotalMilliseconds > 500 ? "danger" : "info");
                            }
                        }
                    }

                    // Make the last crumb for this page the active one
                    Page.Trace.Warn( "Setting active breadcrumb" );
                    if ( PageReference.BreadCrumbs.Any() )
                    {
                        PageReference.BreadCrumbs.Last().Active = true;
                    }

                    Page.Trace.Warn( "Getting parent page references" );
                    var pageReferences = PageReference.GetParentPageReferences( this, _pageCache, PageReference );
                    pageReferences.Add( PageReference );
                    PageReference.SavePageReferences( pageReferences );

                    // Update breadcrumbs
                    Page.Trace.Warn( "Updating breadcrumbs" );
                    BreadCrumbs = new List<BreadCrumb>();
                    foreach ( var pageReference in pageReferences )
                    {
                        pageReference.BreadCrumbs.ForEach( c => BreadCrumbs.Add( c ) );
                    }

                    // Add the page admin footer if the user is authorized to edit the page
                    if ( _pageCache.IncludeAdminFooter && ( canAdministratePage || canAdministrateBlockOnPage || canEditPage ) )
                    {
                        // Add the page admin script
                        AddScriptLink( Page, "~/Scripts/Bundles/RockAdmin", false );

                        Page.Trace.Warn( "Adding admin footer to page" );
                        HtmlGenericControl adminFooter = new HtmlGenericControl( "div" );
                        adminFooter.ID = "cms-admin-footer";
                        adminFooter.ClientIDMode = System.Web.UI.ClientIDMode.Static;
                        this.Form.Controls.Add( adminFooter );

                        phLoadStats = new PlaceHolder();
                        adminFooter.Controls.Add( phLoadStats );

                        // If the current user is Impersonated by another user, show a link on the admin bar to login back in as the original user
                        var impersonatedByUser = Session["ImpersonatedByUser"] as UserLogin;
                        var currentUserIsImpersonated = ( HttpContext.Current?.User?.Identity?.Name ?? string.Empty ).StartsWith( "rckipid=" );
                        if ( canAdministratePage && currentUserIsImpersonated && impersonatedByUser != null)
                        {
                            HtmlGenericControl impersonatedByUserDiv = new HtmlGenericControl( "span" );
                            impersonatedByUserDiv.AddCssClass( "label label-default margin-l-md" );
                            _btnRestoreImpersonatedByUser = new LinkButton();
                            _btnRestoreImpersonatedByUser.ID = "_btnRestoreImpersonatedByUser";
                            //_btnRestoreImpersonatedByUser.CssClass = "btn";
                            _btnRestoreImpersonatedByUser.Visible = impersonatedByUser != null;
                            _btnRestoreImpersonatedByUser.Click += _btnRestoreImpersonatedByUser_Click;
                            _btnRestoreImpersonatedByUser.Text = $"<i class='fa-fw fa fa-unlock'></i> "+ $"Restore { impersonatedByUser?.Person?.ToString()}";
                            impersonatedByUserDiv.Controls.Add( _btnRestoreImpersonatedByUser );
                            adminFooter.Controls.Add( impersonatedByUserDiv );
                        }

                        HtmlGenericControl buttonBar = new HtmlGenericControl( "div" );
                        adminFooter.Controls.Add( buttonBar );
                        buttonBar.Attributes.Add( "class", "button-bar" );

                        // RockBlock Config
                        if ( canAdministratePage || canAdministrateBlockOnPage )
                        {
                            HtmlGenericControl aBlockConfig = new HtmlGenericControl( "a" );
                            buttonBar.Controls.Add( aBlockConfig );
                            aBlockConfig.Attributes.Add( "class", "btn block-config" );
                            aBlockConfig.Attributes.Add( "href", "javascript: Rock.admin.pageAdmin.showBlockConfig();" );
                            aBlockConfig.Attributes.Add( "Title", "Block Configuration" );
                            HtmlGenericControl iBlockConfig = new HtmlGenericControl( "i" );
                            aBlockConfig.Controls.Add( iBlockConfig );
                            iBlockConfig.Attributes.Add( "class", "fa fa-th-large" );
                        }

                        if ( canEditPage || canAdministratePage)
                        {
                            // RockPage Properties
                            HtmlGenericControl aPageProperties = new HtmlGenericControl( "a" );
                            buttonBar.Controls.Add( aPageProperties );
                            aPageProperties.ID = "aPageProperties";
                            aPageProperties.ClientIDMode = System.Web.UI.ClientIDMode.Static;
                            aPageProperties.Attributes.Add( "class", "btn properties" );
                            aPageProperties.Attributes.Add( "href", "javascript: Rock.controls.modal.show($(this), '" + ResolveUrl( string.Format( "~/PageProperties/{0}?t=Page Properties", _pageCache.Id ) ) + "')" );
                            aPageProperties.Attributes.Add( "Title", "Page Properties" );
                            HtmlGenericControl iPageProperties = new HtmlGenericControl( "i" );
                            aPageProperties.Controls.Add( iPageProperties );
                            iPageProperties.Attributes.Add( "class", "fa fa-cog" );
                        }

                        if ( canAdministratePage )
                        {
                            // Child Pages
                            HtmlGenericControl aChildPages = new HtmlGenericControl( "a" );
                            buttonBar.Controls.Add( aChildPages );
                            aChildPages.ID = "aChildPages";
                            aChildPages.ClientIDMode = System.Web.UI.ClientIDMode.Static;
                            aChildPages.Attributes.Add( "class", "btn page-child-pages" );
                            aChildPages.Attributes.Add( "href", "javascript: Rock.controls.modal.show($(this), '" + ResolveUrl( string.Format( "~/pages/{0}?t=Child Pages&pb=&sb=Done", _pageCache.Id ) ) + "')" );
                            aChildPages.Attributes.Add( "Title", "Child Pages" );
                            HtmlGenericControl iChildPages = new HtmlGenericControl( "i" );
                            aChildPages.Controls.Add( iChildPages );
                            iChildPages.Attributes.Add( "class", "fa fa-sitemap" );

                            // RockPage Zones
                            HtmlGenericControl aPageZones = new HtmlGenericControl( "a" );
                            buttonBar.Controls.Add( aPageZones );
                            aPageZones.Attributes.Add( "class", "btn page-zones" );
                            aPageZones.Attributes.Add( "href", "javascript: Rock.admin.pageAdmin.showPageZones();" );
                            aPageZones.Attributes.Add( "Title", "Page Zones" );
                            HtmlGenericControl iPageZones = new HtmlGenericControl( "i" );
                            aPageZones.Controls.Add( iPageZones );
                            iPageZones.Attributes.Add( "class", "fa fa-columns" );

                            // RockPage Security
                            HtmlGenericControl aPageSecurity = new HtmlGenericControl( "a" );
                            buttonBar.Controls.Add( aPageSecurity );
                            aPageSecurity.ID = "aPageSecurity";
                            aPageSecurity.ClientIDMode = System.Web.UI.ClientIDMode.Static;
                            aPageSecurity.Attributes.Add( "class", "btn page-security" );
                            aPageSecurity.Attributes.Add( "href", "javascript: Rock.controls.modal.show($(this), '" + ResolveUrl( string.Format( "~/Secure/{0}/{1}?t=Page Security&pb=&sb=Done",
                                EntityTypeCache.Get( typeof( Rock.Model.Page ) ).Id, _pageCache.Id ) ) + "')" );
                            aPageSecurity.Attributes.Add( "Title", "Page Security" );
                            HtmlGenericControl iPageSecurity = new HtmlGenericControl( "i" );
                            aPageSecurity.Controls.Add( iPageSecurity );
                            iPageSecurity.Attributes.Add( "class", "fa fa-lock" );

                            // ShorLink Properties
                            HtmlGenericControl aShortLink = new HtmlGenericControl( "a" );
                            buttonBar.Controls.Add( aShortLink );
                            aShortLink.ID = "aShortLink";
                            aShortLink.ClientIDMode = System.Web.UI.ClientIDMode.Static;
                            aShortLink.Attributes.Add( "class", "btn properties" );
                            aShortLink.Attributes.Add( "href", "javascript: Rock.controls.modal.show($(this), '" +
                                ResolveUrl( string.Format( "~/ShortLink/{0}?t=Shortened Link&url={1}", _pageCache.Id, Server.UrlEncode( HttpContext.Current.Request.Url.AbsoluteUri.ToString() ) ) )
                                + "')" );
                            aShortLink.Attributes.Add( "Title", "Add Short Link" );
                            HtmlGenericControl iShortLink = new HtmlGenericControl( "i" );
                            aShortLink.Controls.Add( iShortLink );
                            iShortLink.Attributes.Add( "class", "fa fa-link" );

                            // System Info
                            HtmlGenericControl aSystemInfo = new HtmlGenericControl( "a" );
                            buttonBar.Controls.Add( aSystemInfo );
                            aSystemInfo.ID = "aSystemInfo";
                            aSystemInfo.ClientIDMode = System.Web.UI.ClientIDMode.Static;
                            aSystemInfo.Attributes.Add( "class", "btn system-info" );
                            aSystemInfo.Attributes.Add( "href", "javascript: Rock.controls.modal.show($(this), '" + ResolveUrl( "~/SystemInfo?t=System Information&pb=&sb=Done" ) + "')" );
                            aSystemInfo.Attributes.Add( "Title", "Rock Information" );
                            HtmlGenericControl iSystemInfo = new HtmlGenericControl( "i" );
                            aSystemInfo.Controls.Add( iSystemInfo );
                            iSystemInfo.Attributes.Add( "class", "fa fa-info-circle" );
                        }
                    }

                    // Check to see if page output should be cached.  The RockRouteHandler
                    // saves the PageCacheData information for the current page to memorycache
                    // so it should always exist
                    if ( _pageCache.OutputCacheDuration > 0 )
                    {
                        Response.Cache.SetCacheability( System.Web.HttpCacheability.Public );
                        Response.Cache.SetExpires( RockDateTime.Now.AddSeconds( _pageCache.OutputCacheDuration ) );
                        Response.Cache.SetValidUntilExpires( true );
                    }
                }

                stopwatchInitEvents.Restart();

                string pageTitle = BrowserTitle ?? string.Empty;
                string siteTitle = _pageCache.Layout.Site.Name;
                string seperator = pageTitle.Trim() != string.Empty && siteTitle.Trim() != string.Empty ? " | " : "";

                base.Title = pageTitle + seperator + siteTitle;

                if ( !string.IsNullOrWhiteSpace( _pageCache.Description ) )
                {
                    HtmlMeta metaTag = new HtmlMeta();
                    metaTag.Attributes.Add( "name", "description" );
                    metaTag.Attributes.Add( "content", _pageCache.Description.Trim() );
                    AddMetaTag( this.Page, metaTag );
                }

                if ( !string.IsNullOrWhiteSpace( _pageCache.KeyWords ) )
                {
                    HtmlMeta metaTag = new HtmlMeta();
                    metaTag.Attributes.Add( "name", "keywords" );
                    metaTag.Attributes.Add( "content", _pageCache.KeyWords.Trim() );
                    AddMetaTag( this.Page, metaTag );
                }

                if (!string.IsNullOrWhiteSpace( _pageCache.Layout.Site.PageHeaderContent ))
                {
                    Page.Header.Controls.Add( new LiteralControl( _pageCache.Layout.Site.PageHeaderContent ) );
                }

                if ( !string.IsNullOrWhiteSpace( _pageCache.HeaderContent ) )
                {
                    Page.Header.Controls.Add( new LiteralControl( _pageCache.HeaderContent ) );
                }

                if ( !_pageCache.AllowIndexing || !_pageCache.Layout.Site.AllowIndexing )
                {
                    Page.Header.Controls.Add( new LiteralControl( "<meta name=\"robots\" content=\"noindex, nofollow\"/>" ) );
                }

                if ( showDebugTimings )
                {
                    TimeSpan tsDuration = RockDateTime.Now.Subtract( (DateTime)Context.Items["Request_Start_Time"] );
                    slDebugTimings.AppendFormat( "done oninit [{0}ms] @ {1} \n", stopwatchInitEvents.Elapsed.TotalMilliseconds, tsDuration.TotalMilliseconds );
                    stopwatchInitEvents.Restart();
                }

                if ( showDebugTimings && canAdministratePage )
                {
                    Page.Form.Controls.Add( new Label
                    {
                        ID="lblShowDebugTimings",
                        Text = string.Format( "<pre>{0}</pre>", slDebugTimings.ToString() )
                    } );
                }
            }
        }


        /// <summary>
        /// The verify block type instance properties lock object
        /// </summary>
        private static readonly object _verifyBlockTypeInstancePropertiesLockObj = new object();

        /// <summary>
        /// Verifies the block type instance properties.
        /// </summary>
        private void VerifyBlockTypeInstanceProperties()
        {
            var blockTypesIdToVerify = _pageCache.Blocks.Select( a => a.BlockType ).Distinct().Where( a => a.IsInstancePropertiesVerified == false ).Select( a => a.Id ).ToList();
            foreach ( int blockTypeId in blockTypesIdToVerify )
            {
                Page.Trace.Warn( "\tCreating block attributes" );

                try
                {
                    if ( BlockTypeCache.Get( blockTypeId )?.IsInstancePropertiesVerified == false )
                    {
                        // make sure that only one thread is trying to compile block properties so that we don't get collisions and unneeded compiler overhead
                        lock ( _verifyBlockTypeInstancePropertiesLockObj )
                        {
                            if ( BlockTypeCache.Get( blockTypeId )?.IsInstancePropertiesVerified == false )
                            {
                                using ( var rockContext = new RockContext() )
                                {
                                    string blockTypePath = BlockTypeCache.Get( blockTypeId ).Path;
                                    var blockCompiledType = System.Web.Compilation.BuildManager.GetCompiledType( blockTypePath );
                                    bool attributesUpdated = RockBlock.CreateAttributes( rockContext, blockCompiledType, blockTypeId );
                                    BlockTypeCache.Get( blockTypeId )?.MarkInstancePropertiesVerified( true );
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // ignore if the block couldn't be compiled, it'll get logged and shown when the page tries to load the block into the page
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Page.LoadComplete" /> event at the end of the page load stage.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnLoadComplete( EventArgs e )
        {
            base.OnLoadComplete( e );

            // create a page view transaction if enabled
            // moved this from OnLoad so we could get the updated title (if Lava or the block changed it)
            if ( !Page.IsPostBack && _pageCache != null )
            {
                if ( _pageCache.Layout.Site.EnablePageViews )
                {
                    var pageViewTransaction = new InteractionTransaction( DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_WEBSITE ), this.Site, this._pageCache );
                    pageViewTransaction.Enqueue();
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the _btnRestoreImpersonatedByUser control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void _btnRestoreImpersonatedByUser_Click( object sender, EventArgs e )
        {
            var impersonatedByUser = Session["ImpersonatedByUser"] as UserLogin;
            if ( impersonatedByUser != null )
            {
                Authorization.SignOut();
                UserLoginService.UpdateLastLogin( impersonatedByUser.UserName );
                Rock.Security.Authorization.SetAuthCookie( impersonatedByUser.UserName, false, false );
                Response.Redirect( PageReference.BuildUrl( true ), false );
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        /// <summary>
        /// Checks for and processes any impersonation parameters
        /// Returns False if an invalid token was specified
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private bool ProcessImpersonation( RockContext rockContext )
        {
            string impersonatedPersonKeyParam = PageParameter( "rckipid" );
            string impersonatedPersonKeyIdentity = string.Empty;
            if ( HttpContext.Current?.User?.Identity?.Name != null )
            {
                if ( HttpContext.Current.User.Identity.Name.StartsWith( "rckipid=" ) )
                {
                    // get the impersonatedPersonKey from the Auth ticket
                    impersonatedPersonKeyIdentity = HttpContext.Current.User.Identity.Name.Substring( 8 );
                }
            }

            // if there is a impersonatedPersonKeyParam specified, and it isn't already associated with the current HttpContext.Current.User.Identity,
            // then set the currentuser and ticket using the impersonatedPersonKeyParam
            if ( !string.IsNullOrEmpty( impersonatedPersonKeyParam ) && impersonatedPersonKeyParam != impersonatedPersonKeyIdentity )
            {
                Rock.Model.PersonService personService = new Model.PersonService( rockContext );

                Rock.Model.Person impersonatedPerson = personService.GetByImpersonationToken( impersonatedPersonKeyParam, true, this.PageId );
                if ( impersonatedPerson != null )
                {
                    Authorization.SignOut();
                    Rock.Security.Authorization.SetAuthCookie( "rckipid=" + impersonatedPersonKeyParam, false, true );
                    CurrentUser = impersonatedPerson.GetImpersonatedUser();
                    UserLoginService.UpdateLastLogin( "rckipid=" + impersonatedPersonKeyParam );

                    // reload page as the impersonated user (we probably could remove the token from the URL, but some blocks might be looking for rckipid in the PageParameters, so just leave it)
                    Response.Redirect( Request.RawUrl, false );
                    Context.ApplicationInstance.CompleteRequest();
                }
                else
                {
                    // Attempting to use an impersonation token that doesn't exist or is no longer valid, so log them out
                    Authorization.SignOut();
                    Session["InvalidPersonToken"] = true;
                    Response.Redirect( PageReference.BuildUrl( true ), false );
                    Context.ApplicationInstance.CompleteRequest();
                    return false;
                }
            }
            else if ( !string.IsNullOrEmpty( impersonatedPersonKeyIdentity ) )
            {
                if ( !this.IsPostBack )
                {
                    var impersonationToken = impersonatedPersonKeyIdentity;
                    var personToken = new PersonTokenService( rockContext ).GetByImpersonationToken( impersonationToken );
                    if ( personToken != null )
                    {
                        // attempting to use a page specific impersonation token for a different page, so log them out
                        if ( personToken.PageId.HasValue && personToken.PageId != this.PageId )
                        {
                            Authorization.SignOut();
                            Session["InvalidPersonToken"] = true;
                            Response.Redirect( Request.RawUrl, false );
                            Context.ApplicationInstance.CompleteRequest();
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Adds the google maps javascript API to the page
        /// </summary>
        public void LoadGoogleMapsApi()
        {
            var googleAPIKey = GlobalAttributesCache.Get().GetValue( "GoogleAPIKey" );
            string keyParameter = string.IsNullOrWhiteSpace( googleAPIKey ) ? "" : string.Format( "key={0}&", googleAPIKey );
            string scriptUrl = string.Format( "https://maps.googleapis.com/maps/api/js?{0}libraries=drawing,visualization,geometry", keyParameter );

            // first, add it to the page to handle cases where the api is needed on first page load
            if ( this.Page != null && this.Page.Header != null )
            {
                var control = new LiteralControl();
                control.ClientIDMode = System.Web.UI.ClientIDMode.Static;

                // note: ID must match the what it is called in \RockWeb\Scripts\Rock\Controls\util.js
                control.ID = "googleMapsApi";
                control.Text = string.Format( "<script id=\"googleMapsApi\" src=\"{0}\" ></script>", scriptUrl );
                if ( !this.Page.Header.Controls.OfType<LiteralControl>().Any( a => a.ID == control.ID ) )
                {
                    this.Page.Header.Controls.Add( control );
                }
            }

            // also, do this in cases where the api is added on a postback, and the above didn't end up getting rendered
            if ( !ClientScript.IsStartupScriptRegistered( "googleMapsApiScript" ) )
            {
                string script = string.Format( @"Rock.controls.util.loadGoogleMapsApi('{0}');", scriptUrl );
                ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "googleMapsApiScript", script, true );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load"/> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            Stopwatch onLoadStopwatch = Stopwatch.StartNew();

            base.OnLoad( e );

            Page.Header.DataBind();

            try
            {
                bool showDebugTimings = this.PageParameter( "ShowDebugTimings" ).AsBoolean();
                if ( showDebugTimings && onLoadStopwatch.Elapsed.TotalMilliseconds > 500 )
                {
                    if ( _pageCache.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
                    {
                        Page.Form.Controls.Add( new Literal
                        {

                            Text = string.Format( "OnLoad [{0}ms]", onLoadStopwatch.Elapsed.TotalMilliseconds )
                        } );
                    }
                }
            }
            catch
            {
                // ignore
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Page.SaveStateComplete" /> event after the page state has been saved to the persistence medium.
        /// </summary>
        /// <param name="e">A <see cref="T:System.EventArgs" /> object containing the event data.</param>
        protected override void OnSaveStateComplete( EventArgs e )
        {
            base.OnSaveStateComplete( e );

            if ( phLoadStats != null )
            {
                TimeSpan tsDuration = RockDateTime.Now.Subtract( (DateTime)Context.Items["Request_Start_Time"] );
                double hitPercent = 0D;

                if ( Context.Items.Contains( "Cache_Hits" ) )
                {
                    var cacheHits = Context.Items["Cache_Hits"] as System.Collections.Generic.Dictionary<string, bool>;
                    if ( cacheHits != null )
                    {
                        int hits = cacheHits.Where( c => c.Value ).Count();
                        int total = cacheHits.Count();
                        hitPercent = total > 0 ? ( (double)hits / (double)total ) : 0D;
                    }
                }

                var customPersister = this.PageStatePersister as RockHiddenFieldPageStatePersister;

                if ( customPersister != null )
                {
                    this.ViewStateSize = customPersister.ViewStateSize;
                    this.ViewStateSizeCompressed = customPersister.ViewStateSizeCompressed;
                    this.ViewStateIsCompressed = customPersister.ViewStateIsCompressed;
                }

                phLoadStats.Controls.Add( new LiteralControl( string.Format(
                    "<span>Page Load Time: {0:N2}s </span><span class='margin-l-md'>Cache Hit Rate: {1:P2} </span> <span class='margin-l-md js-view-state-stats'></span> <span class='margin-l-md js-html-size-stats'></span>", tsDuration.TotalSeconds, hitPercent ) ) );

                if ( !ClientScript.IsStartupScriptRegistered( "rock-js-view-state-size" ) )
                {
                    string script = @"
Sys.Application.add_load(function () {
    if ($('#__CVIEWSTATESIZE').length > 0 && $('#__CVIEWSTATESIZE').val() != '0') {
        $('.js-view-state-stats').html('ViewState Size: ' + ($('#__CVIEWSTATESIZE').val() / 1024).toFixed(0) + ' KB (' + ($('#__CVIEWSTATE').val().length / 1024).toFixed(0) + ' KB Compressed)');
    } else {
        $('.js-view-state-stats').html('ViewState Size: ' + ($('#__CVIEWSTATE').val().length / 1024).toFixed(0) + ' KB');
    }
    $('.js-html-size-stats').html('HTML Size: ' + ($('html').html().length / 1024).toFixed(0) + ' KB');
});
";
                    ClientScript.RegisterStartupScript( this.Page.GetType(), "rock-js-view-state-size", script, true );
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the page.
        /// </summary>
        /// <param name="pageCache">The <see cref="PageCache"/>.</param>
        internal void SetPage( PageCache pageCache )
        {
            _pageCache = pageCache;

            SaveContextItem( "Rock:PageId", _pageCache.Id );
            SaveContextItem( "Rock:LayoutId", _pageCache.LayoutId );
            SaveContextItem( "Rock:SiteId", _pageCache.Layout.SiteId );

            if ( this.Master is RockMasterPage )
            {
                var masterPage = (RockMasterPage)this.Master;
                masterPage.SetPage( pageCache );
            }
        }

        /// <summary>
        /// Returns the current page's first value for the selected attribute
        /// If the attribute doesn't exist, null is returned
        /// </summary>
        /// <param name="key">A <see cref="System.String"/> representing the argument key.</param>
        /// <returns>A <see cref="System.String" /> representing the first attribute value, if the attribute doesn't exist, null is returned.</returns>
        public string GetAttributeValue( string key )
        {
            if ( _pageCache != null )
            {
                return _pageCache.GetAttributeValue( key );
            }
            return null;
        }

        /// <summary>
        /// Returns the current page's values for the selected attribute.
        /// If the attribute doesn't exist an empty list is returned.
        /// </summary>
        /// <param name="key"> A <see cref="System.String"/> representing the key of the selected attribute
        /// </param>
        /// <returns>A <see cref="System.Collections.Generic.List{String}"/> containing the attribute values for specified key or an empty list if none exists</returns>
        public List<string> GetAttributeValues( string key )
        {
            if ( _pageCache != null )
            {
                return _pageCache.GetAttributeValues( key );
            }

            return new List<string>();
        }

        #region HtmlLinks

        /// <summary>
        /// Adds a new CSS link that will be added to the page header prior to the page being rendered
        /// </summary>
        /// <param name="href">A <see cref="System.String" /> representing the path to css file.  Should be relative to layout template.  Will be resolved at runtime.</param>
        /// <param name="fingerprint">if set to <c>true</c> [fingerprint].</param>
        public void AddCSSLink( string href, bool fingerprint = true )
        {
            RockPage.AddCSSLink( this, href, fingerprint );
        }

        /// <summary>
        /// Adds the CSS link to the page
        /// </summary>
        /// <param name="href">A <see cref="System.String" /> representing the path to css file.  Should be relative to layout template.  Will be resolved at runtime</param>
        /// <param name="mediaType">A <see cref="System.String" /> representing the type of the media to use for the css link.</param>
        /// <param name="fingerprint">if set to <c>true</c> [fingerprint].</param>
        public void AddCSSLink( string href, string mediaType, bool fingerprint = true )
        {
            RockPage.AddCSSLink( this, href, mediaType, fingerprint );
        }

        /// <summary>
        /// Adds a meta tag to the page header prior to the page being rendered
        /// </summary>
        /// <param name="htmlMeta">The <see cref="System.Web.UI.HtmlControls.HtmlMeta"/> tag.</param>
        public void AddMetaTag( HtmlMeta htmlMeta )
        {
            RockPage.AddMetaTag( this, htmlMeta );
        }

        /// <summary>
        /// Adds a new Html link that will be added to the page header prior to the page being rendered.
        /// </summary>
        /// <param name="htmlLink">The <see cref="System.Web.UI.HtmlControls.HtmlLink"/>.</param>
        public void AddHtmlLink( HtmlLink htmlLink )
        {
            RockPage.AddHtmlLink( this, htmlLink );
        }

        /// <summary>
        /// Adds a new script tag to the page body prior to the page being rendered.
        /// </summary>
        /// <param name="path">A <see cref="System.String" /> representing the path to the script link.</param>
        /// <param name="fingerprint">if set to <c>true</c> [fingerprint].</param>
        public void AddScriptLink( string path, bool fingerprint = true )
        {
            RockPage.AddScriptLink( this, path, fingerprint );
        }

        /// <summary>
        /// Adds the google analytics script
        /// </summary>
        /// <param name="code">The GoogleAnalyticsCode.</param>
        private void AddGoogleAnalytics( string code )
        {
            try
            {
                string scriptTemplate = Application["GoogleAnalyticsScript"] as string;
                if ( scriptTemplate == null )
                {
                    string scriptFile = MapPath( "~/Assets/Misc/GoogleAnalytics.txt" );
                    if ( File.Exists( scriptFile ) )
                    {
                        scriptTemplate = File.ReadAllText( scriptFile );
                        Application["GoogleAnalyticsScript"] = scriptTemplate;
                    }
                }

                if ( scriptTemplate != null )
                {
                    string script = scriptTemplate.Contains( "{0}" ) ? string.Format( scriptTemplate, code ) : scriptTemplate;
                    AddScriptToHead( this.Page, script, true );
                }
            }
            catch ( Exception ex )
            {
                LogException( ex );
            }
        }

        /// <summary>
        /// Adds an icon icon (favicon) link using a binary file id.
        /// </summary>
        /// <param name="binaryFileId">The binary file identifier.</param>
        /// <param name="size">The size.</param>
        /// <param name="rel">The relative.</param>
        /// <returns></returns>
        public void AddIconLink( int binaryFileId, int size, string rel = "apple-touch-icon-precomposed" )
        {
            HtmlLink favIcon = new HtmlLink();
            favIcon.Attributes.Add( "rel", rel );
            favIcon.Attributes.Add( "sizes", $"{size}x{size}" );
            favIcon.Attributes.Add( "href", ResolveRockUrl( $"~/GetImage.ashx?id={binaryFileId}&width={size}&height={size}&mode=crop&format=png" ) );
            AddHtmlLink( favIcon );
        }


        #endregion

        /// <summary>
        /// Hides any secondary blocks.
        /// </summary>
        /// <param name="caller">The <see cref="Rock.Web.UI.RockBlock"/> that is the caller</param>
        /// <param name="hidden">A <see cref="System.Boolean"/> value that signifies if secondary blocks should be hidden.</param>
        public void HideSecondaryBlocks( RockBlock caller, bool hidden )
        {
            foreach ( ISecondaryBlock secondaryBlock in this.RockBlocks.Where( a => a is ISecondaryBlock ) )
            {
                if ( secondaryBlock != caller )
                {
                    secondaryBlock.SetVisible( !hidden );
                }
            }
        }

        /// <summary>
        /// Disables the idle redirect blocks if disable = true, or re-enables them if disable = false
        /// </summary>
        /// <param name="caller">The caller.</param>
        /// <param name="disable">if set to <c>true</c> [disable].</param>
        public void DisableIdleRedirectBlocks( RockBlock caller, bool disable )
        {
            foreach (  IIdleRedirectBlock idleRedirectBlock in this.RockBlocks.Where( a => a is IIdleRedirectBlock ) )
            {
                if ( idleRedirectBlock != caller )
                {
                    idleRedirectBlock.Disable( disable );
                }
            }
        }

        /// <summary>
        /// Logs the exception.
        /// </summary>
        /// <param name="ex">The <see cref="System.Exception"/> to log.</param>
        public void LogException( Exception ex )
        {
            ExceptionLogService.LogException( ex, Context, _pageCache.Id, _pageCache.Layout.SiteId, CurrentPersonAlias );
        }

        /// <summary>
        /// Adds a history point to the ScriptManager.
        /// Note: ScriptManager's EnableHistory property must be set to True
        /// </summary>
        /// <param name="key">A <see cref="System.String"/> representing the key to use for the history point.</param>
        /// <param name="state">A <see cref="System.String"/> representing any state information to store for the history point.</param>
        /// <param name="title">A <see cref="System.String"/> representing the title to be used by the browser, will use an empty string by default.</param>
        public void AddHistory( string key, string state, string title = "" )
        {
            if ( ScriptManager.GetCurrent( Page ) != null )
            {
                ScriptManager sManager = ScriptManager.GetCurrent( Page );
                if ( string.IsNullOrWhiteSpace( title ) )
                {
                    sManager.AddHistoryPoint( key, state );
                }
                else
                {
                    sManager.AddHistoryPoint( key, state, title );
                }
            }
        }

        /// <summary>
        /// Returns a resolved Rock URL.  Similar to
        /// <see cref="System.Web.UI.Control">System.Web.UI.Control's</see>
        /// <c>ResolveUrl</c> method except that you can prefix
        /// a url with '~~' to indicate a virtual path to Rock's current theme root folder.
        /// </summary>
        /// <param name="url">A <see cref="System.String" /> representing the URL to resolve.</param>
        /// <returns>
        /// A <see cref="System.String" /> with the resolved URL.
        /// </returns>
        public string ResolveRockUrl( string url )
        {
            string themeUrl = url;
            if ( url.StartsWith( "~~" ) )
            {
                themeUrl = "~/Themes/" + _pageCache.Layout.Site.Theme + ( url.Length > 2 ? url.Substring( 2 ) : string.Empty );
            }

            return ResolveUrl( themeUrl );
        }

        /// <summary>
        /// Resolves the rock URL and includes root.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        public string ResolveRockUrlIncludeRoot( string url )
        {
            string virtualPath = this.ResolveRockUrl( url );
            if ( Context.Request != null && Context.Request.Url != null )
            {
                return string.Format( "{0}://{1}{2}", Context.Request.Url.Scheme, Context.Request.Url.Authority, virtualPath );
            }

            return GlobalAttributesCache.Get().GetValue("PublicApplicationRoot").EnsureTrailingForwardslash() + virtualPath.RemoveLeadingForwardslash();
        }

        /// <summary>
        /// Resolves the rock URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="fingerprint">if set to <c>true</c> [fingerprint].</param>
        /// <returns></returns>
        public string ResolveRockUrl( string url, bool fingerprint )
        {
            var resolvedUrl = this.ResolveRockUrl( url );

            if ( fingerprint )
            {
                resolvedUrl = Fingerprint.Tag( resolvedUrl );
            }

            return resolvedUrl;
        }

        /// <summary>
        /// Gets the context entity types.
        /// </summary>
        /// <returns></returns>
        public List<EntityTypeCache> GetContextEntityTypes()
        {
            var result = new List<EntityTypeCache>();

            if ( this.ModelContext != null )
            {
                foreach ( var item in this.ModelContext.Keys )
                {
                    var entityType = EntityTypeCache.Get( item );
                    if ( entityType != null )
                    {
                        result.Add( entityType );
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Gets the current context object for a given entity type.
        /// </summary>
        /// <param name="entity">The <see cref="EntityTypeCache"/> containing a reference to the entity.</param>
        /// <returns>An object that implements the <see cref="Rock.Data.IEntity"/> interface referencing the context object. </returns>
        public Rock.Data.IEntity GetCurrentContext( EntityTypeCache entity )
        {
            if ( this.ModelContext.ContainsKey( entity.Name ) )
            {
                var keyModel = this.ModelContext[entity.Name];

                if ( keyModel.Entity == null )
                {
                    if ( entity.Name.Equals( "Rock.Model.Person", StringComparison.OrdinalIgnoreCase ) )
                    {
                        if ( string.IsNullOrWhiteSpace( keyModel.Key ) )
                        {
                            keyModel.Entity = new PersonService( new RockContext() )
                                .Queryable( "MaritalStatusValue,ConnectionStatusValue,RecordStatusValue,RecordStatusReasonValue,RecordTypevalue,SuffixValue,TitleValue,GivingGroup,Photo,Aliases", true, true )
                                .Where( p => p.Id == keyModel.Id ).FirstOrDefault();
                        }
                        else
                        {
                            keyModel.Entity = new PersonService( new RockContext() ).GetByPublicKey( keyModel.Key );
                        }
                    }
                    else
                    {

                        Type modelType = entity.GetEntityType();

                        if ( modelType == null )
                        {
                            // if the Type isn't found in the Rock.dll (it might be from a Plugin), lookup which assembly it is in and look in there
                            string[] assemblyNameParts = entity.AssemblyName.Split( new char[] { ',' } );
                            if ( assemblyNameParts.Length > 1 )
                            {
                                modelType = Type.GetType( string.Format( "{0}, {1}", entity.Name, assemblyNameParts[1] ) );
                            }
                        }

                        if ( modelType != null )
                        {
                            // In the case of core Rock.dll Types, we'll just use Rock.Data.Service<> and Rock.Data.RockContext<>
                            // otherwise find the first (and hopefully only) Service<> and dbContext we can find in the Assembly.
                            System.Data.Entity.DbContext dbContext = Reflection.GetDbContextForEntityType( modelType );
                            IService serviceInstance = Reflection.GetServiceForEntityType( modelType, dbContext );

                            if ( string.IsNullOrWhiteSpace( keyModel.Key ) )
                            {
                                MethodInfo getMethod = serviceInstance.GetType().GetMethod( "Get", new Type[] { typeof( int ) } );
                                keyModel.Entity = getMethod.Invoke( serviceInstance, new object[] { keyModel.Id } ) as Rock.Data.IEntity;
                            }
                            else
                            {
                                MethodInfo getMethod = serviceInstance.GetType().GetMethod( "GetByPublicKey" );
                                keyModel.Entity = getMethod.Invoke( serviceInstance, new object[] { keyModel.Key } ) as Rock.Data.IEntity;
                            }
                        }

                    }

                    if ( keyModel.Entity != null && keyModel.Entity is IHasAttributes )
                    {
                        Attribute.Helper.LoadAttributes( keyModel.Entity as IHasAttributes );
                    }

                }

                return keyModel.Entity;
            }

            return null;
        }

        /// <summary>
        /// Sets the context cookie.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="pageSpecific">if set to <c>true</c> [page specific].</param>
        /// <param name="refreshPage">if set to <c>true</c> [refresh page].</param>
        public void SetContextCookie( IEntity entity, bool pageSpecific = false, bool refreshPage = true )
        {
            string cookieName = GetContextCookieName( pageSpecific );

            var contextCookie = Request.Cookies[cookieName];
            if ( contextCookie == null )
            {
                contextCookie = new HttpCookie( cookieName );
            }

            Type entityType = entity.GetType();
            if ( entityType.IsDynamicProxyType() )
            {
                entityType = entityType.BaseType;
            }

            contextCookie.Values[entityType.FullName] = HttpUtility.UrlDecode( entity.ContextKey );
            contextCookie.Expires = RockDateTime.Now.AddYears( 1 );

            Response.Cookies.Add( contextCookie );

            if ( refreshPage )
            {
                Response.Redirect( Request.RawUrl, false );
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        /// <summary>
        /// Clears the context cookie.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="pageSpecific">if set to <c>true</c> [page specific].</param>
        /// <param name="refreshPage">if set to <c>true</c> [refresh page].</param>
        public void ClearContextCookie( Type entityType, bool pageSpecific = false, bool refreshPage = true )
        {
            string cookieName = GetContextCookieName( pageSpecific );

            var contextCookie = Request.Cookies[cookieName];
            if ( contextCookie == null )
            {
                contextCookie = new HttpCookie( cookieName );
            }

            if ( entityType.IsDynamicProxyType() )
            {
                entityType = entityType.BaseType;
            }

            contextCookie.Values[entityType.FullName] = null;
            contextCookie.Expires = RockDateTime.Now.AddYears( 1 );

            Response.Cookies.Add( contextCookie );

            if ( refreshPage )
            {
                Response.Redirect( Request.RawUrl, false );
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        private void GetCookieContext( string cookieName )
        {
            HttpCookie cookie = null;
            if ( Response.Cookies.AllKeys.Contains(cookieName))
            {
                cookie = Response.Cookies[cookieName];
            }
            else if ( Request.Cookies.AllKeys.Contains(cookieName))
            {
                cookie = Request.Cookies[cookieName];
            }

            if ( cookie != null )
            {
                for ( int valueIndex = 0; valueIndex < cookie.Values.Count; valueIndex++ )
                {
                    string cookieValue = cookie.Values[valueIndex];
                    if ( !string.IsNullOrWhiteSpace( cookieValue ) )
                    {
                        try
                        {
                            string contextItem = Rock.Security.Encryption.DecryptString( cookieValue );
                            string[] parts = contextItem.Split( '|' );
                            if ( parts.Length == 2 )
                            {
                                ModelContext.AddOrReplace( parts[0], new Data.KeyEntity( parts[1] ) );
                            }
                        }
                        catch
                        {
                            // intentionally ignore exception in case cookie is corrupt
                        }
                    }
                }
            }
        }

        private void HandleRockWiFiCookie( int? personAliasId )
        {
            if ( personAliasId == null)
            {
                return;
            }

            if ( Request.Cookies["rock_wifi"] != null )
            {
                HttpCookie httpCookie = Request.Cookies["rock_wifi"];
                if ( LinkPersonAliasToDevice( ( int ) personAliasId, httpCookie.Values["ROCK_PERSONALDEVICE_ADDRESS"] ) )
                {
                    Response.Cookies["rock_wifi"].Expires = DateTime.Now.AddDays( -1 );
                }
            }
        }

        /// <summary>
        /// Gets the name of the context cookie.
        /// </summary>
        /// <param name="pageSpecific">if set to <c>true</c> [page specific].</param>
        /// <returns></returns>
        public string GetContextCookieName( bool pageSpecific )
        {
            return "Rock_Context" + ( pageSpecific ? ( ":" + PageId.ToString() ) : "" );
        }

        /// <summary>
        /// Adds an update trigger for when the block instance properties are updated.
        /// </summary>
        /// <param name="updatePanel">The <see cref="System.Web.UI.UpdatePanel"/> to add the <see cref="System.Web.UI.AsyncPostBackTrigger"/> to.</param>
        public void AddConfigurationUpdateTrigger( UpdatePanel updatePanel )
        {
            AsyncPostBackTrigger trigger = new AsyncPostBackTrigger();
            trigger.ControlID = "rock-config-trigger";
            trigger.EventName = "Click";
            updatePanel.Triggers.Add( trigger );
        }

        /// <summary>
        /// Links the person alias to device.
        /// </summary>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <param name="macAddress">The mac address.</param>
        public bool LinkPersonAliasToDevice(int personAliasId, string macAddress)
        {
            using ( var rockContext = new RockContext() )
            {
                PersonalDeviceService personalDeviceService = new PersonalDeviceService( rockContext );
                PersonalDevice personalDevice = personalDeviceService.GetByMACAddress( macAddress );

                // It's possible that the device was deleted from the DB but a cookie still exists
                if ( personalDevice == null || personAliasId == 0 )
                {
                    return false;
                }

                // Assign the current Person.Alias to the device and save
                if ( personalDevice.PersonAliasId == null || personalDevice.PersonAliasId != personAliasId )
                {
                    personalDevice.PersonAliasId = personAliasId;
                    rockContext.SaveChanges();
                }

                // Update interactions for this device with this person.alias if they don't already have one.
                InteractionService interactionService = new InteractionService( rockContext );
                interactionService.UpdateInteractionsWithPersonAliasIdForDeviceId( personAliasId, personalDevice.Id );

                return true;
            }
        }

        #endregion

        #region Cms Admin Content

        /// <summary>
        /// Adds the popup controls.
        /// </summary>
        private void AddPopupControls()
        {
            ModalIFrameDialog modalPopup = new ModalIFrameDialog();
            modalPopup.ID = "modal-popup";
            this.Form.Controls.Add( modalPopup );
        }

        // Adds the necessary script elements for managing the page/zone/blocks
        /// <summary>
        /// Adds the config elements.
        /// </summary>
        private void AddTriggerPanel()
        {
            CompiledTemplateBuilder upContent = new CompiledTemplateBuilder(
                delegate( Control content )
                {
                    Button trigger = new Button();
                    trigger.ClientIDMode = System.Web.UI.ClientIDMode.Static;
                    trigger.ID = "rock-config-trigger";
                    trigger.Click += trigger_Click;
                    content.Controls.Add( trigger );

                    HiddenField triggerData = new HiddenField();
                    triggerData.ClientIDMode = System.Web.UI.ClientIDMode.Static;
                    triggerData.ID = "rock-config-trigger-data";
                    content.Controls.Add( triggerData );
                }
            );

            UpdatePanel upTrigger = new UpdatePanel();
            upTrigger.ContentTemplate = upContent;
            this.Form.Controls.Add( upTrigger );
            upTrigger.Attributes.Add( "style", "display:none" );
        }

        // Adds the necessary script elements for managing the page/zone/blocks
        /// <summary>
        /// Adds the config elements.
        /// </summary>
        private void AddZoneElements( bool canConfigPage )
        {
            if ( canConfigPage )
            {
                AddBlockMove();
            }

            // Add Zone Wrappers
            foreach ( KeyValuePair<string, KeyValuePair<string, Zone>> zoneControl in this.Zones )
            {
                var control = zoneControl.Value.Value;
                Control parent = zoneControl.Value.Value.Parent;

                HtmlGenericControl zoneWrapper = new HtmlGenericControl( "div" );
                parent.Controls.AddAt( parent.Controls.IndexOf( control ), zoneWrapper );
                zoneWrapper.ID = string.Format( "zone-{0}", zoneControl.Key.ToLower() );
                zoneWrapper.ClientIDMode = System.Web.UI.ClientIDMode.Static;
                zoneWrapper.Attributes.Add( "class", ("zone-instance" + ( canConfigPage ? " can-configure " : " " ) + control.CssClass).Trim() );

                if ( canConfigPage )
                {
                    // Zone content configuration widget
                    HtmlGenericControl zoneConfig = new HtmlGenericControl( "div" );
                    zoneWrapper.Controls.Add( zoneConfig );
                    zoneConfig.Attributes.Add( "class", "zone-configuration config-bar" );

                    HtmlGenericControl zoneConfigLink = new HtmlGenericControl( "a" );
                    zoneConfigLink.Attributes.Add( "class", "zoneinstance-config" );
                    zoneConfigLink.Attributes.Add( "href", "#" );
                    zoneConfig.Controls.Add( zoneConfigLink );
                    HtmlGenericControl iZoneConfig = new HtmlGenericControl( "i" );
                    iZoneConfig.Attributes.Add( "class", "fa fa-arrow-circle-right" );
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
                    aBlockConfig.ID = string.Format( "aBlockConfig-{0}", zoneControl.Key );
                    aBlockConfig.ClientIDMode = System.Web.UI.ClientIDMode.Static;
                    aBlockConfig.Attributes.Add( "class", "zone-blocks" );
                    aBlockConfig.Attributes.Add( "href", "javascript: Rock.controls.modal.show($(this), '" + ResolveUrl( string.Format( "~/ZoneBlocks/{0}/{1}?t=Zone Blocks&pb=&sb=Done", _pageCache.Id, zoneControl.Key ) ) + "')" );
                    aBlockConfig.Attributes.Add( "Title", "Zone Blocks" );
                    aBlockConfig.Attributes.Add( "zone", zoneControl.Key );
                    //aBlockConfig.InnerText = "Blocks";
                    HtmlGenericControl iZoneBlocks = new HtmlGenericControl( "i" );
                    iZoneBlocks.Attributes.Add( "class", "fa fa-th-large" );
                    aBlockConfig.Controls.Add( iZoneBlocks );
                }

                HtmlGenericContainer zoneContent = new HtmlGenericContainer( "div" );
                zoneContent.Attributes.Add( "class", "zone-content" );
                zoneWrapper.Controls.Add( zoneContent );

                parent.Controls.Remove( control );
                zoneContent.Controls.Add( control );
            }
        }

        /// <summary>
        /// Adds a control to move the block to another zone on the page.
        /// </summary>
        private void AddBlockMove()
        {
            // Add Zone Selection Popup (for moving blocks to another zone)
            ModalDialog modalBlockMove = new ModalDialog();
            modalBlockMove.CssClass = "js-modal-block-move";
            modalBlockMove.Title = "Move Block";
            modalBlockMove.OnOkScript = "Rock.admin.pageAdmin.saveBlockMove();";
            this.Form.Controls.Add( modalBlockMove );
            modalBlockMove.Visible = true;

            HtmlGenericControl fsZoneSelect = new HtmlGenericControl( "fieldset" );
            fsZoneSelect.ClientIDMode = ClientIDMode.Static;
            fsZoneSelect.Attributes.Add( "id", "fsZoneSelect" );
            modalBlockMove.Content.Controls.Add( fsZoneSelect );

            HtmlGenericControl legend = new HtmlGenericControl( "legend" );
            legend.InnerText = "New Location";
            fsZoneSelect.Controls.Add( legend );

            RockDropDownList ddlZones = new RockDropDownList();
            ddlZones.ClientIDMode = ClientIDMode.Static;
            ddlZones.ID = "block-move-zone";
            ddlZones.Label = "Zone";
            foreach ( var zone in Zones )
                ddlZones.Items.Add( new ListItem( zone.Value.Key, zone.Key ) );
            fsZoneSelect.Controls.Add( ddlZones );

            RockRadioButtonList rblLocation = new RockRadioButtonList();
            rblLocation.RepeatDirection = RepeatDirection.Horizontal;
            rblLocation.ClientIDMode = ClientIDMode.Static;
            rblLocation.ID = "block-move-Location";
            rblLocation.CssClass = "inputs-list";
            rblLocation.Items.Add( new ListItem( string.Format( "Page ({0})", _pageCache.InternalName), "Page" ) );
            rblLocation.Items.Add( new ListItem( string.Format( "Layout ({0})", _pageCache.Layout.Name ), "Layout" ) );
            rblLocation.Items.Add( new ListItem( string.Format( "Site ({0})", _pageCache.Layout.Site.Name ), "Site" ) );
            rblLocation.Label = "Parent";
            fsZoneSelect.Controls.Add( rblLocation );
        }

        #endregion

        #region SharedItemCaching

        /// <summary>
        /// Used to save an item to the current HTTPRequests items collection.  This is useful if multiple blocks
        /// on the same page will need access to the same object.  The first block can read the object and save
        /// it using this method for the other blocks to reference
        /// </summary>
        /// <param name="key">A <see cref="System.String"/> representing the item's key</param>
        /// <param name="item">The <see cref="System.Object"/> to save.</param>
        public void SaveSharedItem( string key, object item )
        {
            string itemKey = $"SharedItem:Page:{PageId}:Item:{key}";
            SaveContextItem( itemKey, item );
        }

        private void SaveContextItem( string key, object item )
        {
            System.Collections.IDictionary items = HttpContext.Current.Items;
            if ( items.Contains( key ) )
            {
                items[key] = item;
            }
            else
            {
                items.Add( key, item );
            }
        }

        /// <summary>
        /// Retrieves an item from the current HTTPRequest items collection.  This is useful to retrieve an object
        /// that was saved by a previous block on the same page.
        /// </summary>
        /// <param name="key">A <see cref="System.String"/> representing the object's key value.</param>
        /// <returns>The shared <see cref="System.Object"/>, if a match for the key is not found, a null value will be returned.</returns>
        public object GetSharedItem( string key )
        {
            string itemKey = $"SharedItem:Page:{PageId}:Item:{key}";

            System.Collections.IDictionary items = HttpContext.Current.Items;
            if ( items.Contains( itemKey ) )
            {
                return items[itemKey];
            }

            return null;
        }

        #endregion

        #region Static Helper Methods

        /// <summary>
        /// Checks the page's RouteData values and then the query string for a
        /// parameter matching the specified name, and if found returns the string
        /// value
        /// </summary>
        /// <param name="name">A <see cref="System.String" /> representing the name of the page parameter.</param>
        /// <param name="searchFormParams">if set to <c>true</c> [search form parameters].</param>
        /// <returns>
        /// A <see cref="System.String" /> containing the parameter value; otherwise an empty string.
        /// </returns>
        public string PageParameter( string name, bool searchFormParams = false )
        {
            if ( !string.IsNullOrWhiteSpace( name ) )
            {
                if ( Page.RouteData.Values.ContainsKey( name ) )
                {
                    return (string)Page.RouteData.Values[name];
                }

                if ( !string.IsNullOrEmpty( Request.QueryString[name] ) )
                {
                    return Request.QueryString[name];
                }

                if ( PageReference.Parameters.ContainsKey( name ) )
                {
                    return PageReference.Parameters[name];
                }

                if ( searchFormParams )
                {
                    if ( !string.IsNullOrEmpty( this.Page.Request.Params[name] ) )
                    {
                        return this.Page.Request.Params[name];
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Checks the page reference's parms and querystring for a
        /// parameter matching the specified name, and if found returns the string
        /// </summary>
        /// <param name="pageReference">The <see cref="Rock.Web.PageReference"/>.</param>
        /// <param name="name">A <see cref="System.String"/> containing the name of the parameter.</param>
        /// <returns>A <see cref="System.String"/> containing the value.</returns>
        public string PageParameter( PageReference pageReference, string name )
        {
            if ( String.IsNullOrEmpty( name ) )
            {
                return string.Empty;
            }

            if ( pageReference.Parameters.ContainsKey( name ) )
            {
                return (string)pageReference.Parameters[name];
            }

            if ( String.IsNullOrEmpty( pageReference.QueryString[name] ) )
            {
                return string.Empty;
            }
            else
            {
                return pageReference.QueryString[name];
            }
        }

        /// <summary>
        /// Gets the page route and query string parameters
        /// </summary>
        /// <returns>A <see cref="System.Collections.Generic.Dictionary{String, Object}"/> containing the page route and query string value, the Key is the is the parameter name/key and the object is the value.</returns>
        public Dictionary<string, object> PageParameters()
        {
            var parameters = new Dictionary<string, object>();

            foreach ( var key in Page.RouteData.Values.Keys )
            {
                parameters.Add( key, Page.RouteData.Values[key] );
            }

            foreach ( string param in Request.QueryString.Keys )
            {
                if ( param != null )
                {
                    parameters.Add( param, Request.QueryString[param] );
                }
            }

            return parameters;
        }

        /// <summary>
        /// Adds a new CSS link that will be added to the page header prior to the page being rendered
        /// </summary>
        /// <param name="page">The <see cref="System.Web.UI.Page" />.</param>
        /// <param name="href">A <see cref="System.String" /> representing the path to css file.  Should be relative to layout template.  Will be resolved at runtime</param>
        /// <param name="fingerprint">if set to <c>true</c> [fingerprint].</param>
        public static void AddCSSLink( Page page, string href, bool fingerprint = true )
        {
            AddCSSLink( page, href, string.Empty, fingerprint );
        }

        /// <summary>
        /// Adds the CSS link to the page
        /// </summary>
        /// <param name="page">The <see cref="System.Web.UI.Page" />.</param>
        /// <param name="href">A <see cref="System.String" /> representing the path to css file.  Should be relative to layout template.  Will be resolved at runtime</param>
        /// <param name="mediaType">A <see cref="System.String" /> representing the type of the media to use for the css link.</param>
        /// <param name="fingerprint">if set to <c>true</c> [fingerprint].</param>
        public static void AddCSSLink( Page page, string href, string mediaType, bool fingerprint = true )
        {
            HtmlLink htmlLink = new HtmlLink();

            if ( fingerprint )
            {
                htmlLink.Attributes.Add( "href", Fingerprint.Tag( page.ResolveUrl( href ) ) );
            }
            else
            {
                htmlLink.Attributes.Add( "href", page.ResolveUrl( href ) );
            }

            htmlLink.Attributes.Add( "type", "text/css" );
            htmlLink.Attributes.Add( "rel", "stylesheet" );

            if ( mediaType != string.Empty )
            {
                htmlLink.Attributes.Add( "media", mediaType );
            }

            AddHtmlLink( page, htmlLink, "css" );
        }

        /// <summary>
        /// Adds a meta tag to the page
        /// </summary>
        /// <param name="page">The <see cref="System.Web.UI.Page"/>.</param>
        /// <param name="htmlMeta">A <see cref="System.String"/>representing the HTML meta tag.</param>
        public static void AddMetaTag( Page page, HtmlMeta htmlMeta )
        {
            if ( page != null && page.Header != null )
            {
                if ( !HtmlMetaExists( page, htmlMeta ) )
                {
                    // Find last meta element
                    int index = 0;
                    for ( int i = page.Header.Controls.Count - 1; i >= 0; i-- )
                    {
                        if ( page.Header.Controls[i] is HtmlMeta )
                        {
                            index = i;
                            break;
                        }
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
        }

        /// <summary>
        /// Returns a flag indicating if a meta tag exists on the page.
        /// </summary>
        /// <param name="page">The <see cref="System.Web.UI.Page"/>.</param>
        /// <param name="newMeta">The <see cref="System.Web.UI.HtmlControls.HtmlMeta"/> tag to check for..</param>
        /// <returns>A <see cref="System.Boolean"/> that is <c>true</c> if the meta tag already exists; otherwise <c>false</c>.</returns>
        private static bool HtmlMetaExists( Page page, HtmlMeta newMeta )
        {
            bool existsAlready = false;

            if ( page != null && page.Header != null )
            {
                foreach ( Control control in page.Header.Controls )
                {
                    if ( control is HtmlMeta )
                    {
                        HtmlMeta existingMeta = (HtmlMeta)control;

                        bool sameAttributes = true;

                        foreach ( string attributeKey in newMeta.Attributes.Keys )
                        {
                            if ( existingMeta.Attributes[attributeKey] == null ||
                                existingMeta.Attributes[attributeKey].ToLower() != newMeta.Attributes[attributeKey].ToLower() )
                            {
                                sameAttributes = false;
                                break;
                            }
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
        /// Adds a new Html link that will be added to the page header prior to the page being rendered
        /// </summary>
        /// <param name="page">The <see cref="System.Web.UI.Page"/>.</param>
        /// <param name="htmlLink">The <see cref="System.Web.UI.HtmlControls.HtmlLink"/> to add to the page.</param>
        /// <param name="contentPlaceHolderId">A <see cref="System.String"/> representing the Id of the content placeholder to add the link to.</param>
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
        /// Returns a <see cref="System.Boolean"/> flag indicating if a specified parent control contains the specified HtmlLink.
        /// </summary>
        /// <param name="parentControl">The <see cref="System.Web.UI.Control"/> to search for the HtmlLink.</param>
        /// <param name="newLink">The <see cref="System.Web.UI.HtmlControls.HtmlLink"/> to search for.</param>
        /// <returns>A <see cref="System.Boolean"/> value that is <c>true</c> if if the HtmlLink exists in the parent control; otherwise <c>false</c>.</returns>
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
        /// Adds a new script tag to the page body prior to the page being rendered
        /// </summary>
        /// <param name="page">The <see cref="System.Web.UI.Page" />.</param>
        /// <param name="path">A <see cref="System.String" /> representing the path to script file.  Should be relative to layout template.  Will be resolved at runtime.</param>
        /// <param name="fingerprint">if set to <c>true</c> [fingerprint].</param>
        public static void AddScriptLink( Page page, string path, bool fingerprint = true )
        {
            var scriptManager = ScriptManager.GetCurrent( page );
             
            if ( fingerprint )
            {
                path = Fingerprint.Tag( page.ResolveUrl( path ) );
            }

            if ( scriptManager != null && !scriptManager.Scripts.Any( s => s.Path == path ) )
            {
                scriptManager.Scripts.Add( new ScriptReference( path ) );
            }
        }

        /// <summary>
        /// Adds the script to head.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="script">The script.</param>
        /// <param name="addScriptTags">if set to <c>true</c> [add script tags].</param>
        public static void AddScriptToHead( Page page, string script, bool addScriptTags )
        {
            if ( page != null && page.Header != null )
            {
                var header = page.Header;

                Literal l = new Literal();

                if ( addScriptTags )
                {
                    l.Text = string.Format( @"
    <script type=""text/javascript"">
{0}
    </script>

", script );
                }
                else
                {
                    l.Text = script;
                }

                header.Controls.Add( l );
            }
        }

        /// <summary>
        /// Adds a script tag with the specified id and source to head (if it doesn't already exist)
        /// </summary>
        /// <param name="scriptId">The script identifier.</param>
        /// <param name="src">The source.</param>
        public void AddScriptSrcToHead( string scriptId, string src )
        {
            RockPage.AddScriptSrcToHead( this.Page, scriptId, src );
        }

        /// <summary>
        /// Adds a script tag with the specified id and source to head (if it doesn't already exist)
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="scriptId">The script identifier.</param>
        /// <param name="src">The source.</param>
        public static void AddScriptSrcToHead( Page page, string scriptId, string src )
        {
            if ( page != null && page.Header != null )
            {
                var header = page.Header;

                if ( !header.Controls.OfType<Literal>().Any( a => a.ID == scriptId ) )
                {
                    Literal l = new Literal {
                        ID = scriptId,
                        Text = $"<script id='{scriptId}' src='{src}'></script>"
                    };

                    header.Controls.Add( l );
                }
            }
        }

        /// <summary>
        /// Gets the client's ip address.
        /// </summary>
        /// <returns></returns>
        public static string GetClientIpAddress()
        {
            return WebRequestHelper.GetClientIpAddress( new HttpRequestWrapper( HttpContext.Current.Request ) );
        }

        #region User Preferences

        /// <summary>
        /// Returns a user preference for the current user and given key.
        /// </summary>
        /// <param name="key">A <see cref="System.String" /> representing the key to the user preference.</param>
        /// <returns>A <see cref="System.String" /> representing the specified user preference value, if a match is not found an empty string will be returned.</returns>
        public string GetUserPreference( string key )
        {
            var values = SessionUserPreferences();
            if ( values.ContainsKey( key ) )
            {
                return values[key];
            }

            return string.Empty;
        }

        /// <summary>
        /// Returns the preference values for the current user that start with a given key.
        /// </summary>
        /// <param name="keyPrefix">A <see cref="System.String"/> representing the key prefix. Preference values, for the current user, with a key that begins with this value will be included.</param>
        /// <returns>A <see cref="System.Collections.Generic.Dictionary{String,String}"/> containing  the current user's preference values containing a key that begins with the specified value.
        /// Each <see cref="System.Collections.Generic.KeyValuePair{String,String}"/> contains a key that represents the user preference key and a value that contains the user preference value associated
        /// with that key.
        /// </returns>
        public Dictionary<string, string> GetUserPreferences( string keyPrefix )
        {
            var selectedValues = new Dictionary<string, string>();

            var values = SessionUserPreferences();
            foreach ( var key in values.Where( v => v.Key.StartsWith( keyPrefix ) ) )
            {
                selectedValues.Add( key.Key, key.Value );
            }

            return selectedValues;
        }

        /// <summary>
        /// Sets a user preference value for the specified key. If the key already exists, the value will be updated,
        /// if it is a new key it will be added. Value is then optionally saved to database.
        /// </summary>
        /// <param name="key">A <see cref="System.String" /> representing the name of the key.</param>
        /// <param name="value">A <see cref="System.String" /> representing the preference value.</param>
        /// <param name="saveValue">if set to <c>true</c> [save value].</param>
        public void SetUserPreference( string key, string value, bool saveValue = true )
        {
            var sessionValues = SessionUserPreferences();
            if ( sessionValues.ContainsKey( key ) )
            {
                sessionValues[key] = value;
            }
            else
            {
                sessionValues.Add( key, value );
            }

            if ( saveValue && CurrentPerson != null )
            {
                PersonService.SaveUserPreference( CurrentPerson, key, value );
            }
        }

        /// <summary>
        /// Saves the user preferences.
        /// </summary>
        /// <param name="keyPrefix">The key prefix.</param>
        public void SaveUserPreferences( string keyPrefix )
        {
            if ( CurrentPerson != null )
            {
                var values = new Dictionary<string, string>();
                SessionUserPreferences()
                    .Where( p => p.Key.StartsWith( keyPrefix ) )
                    .ToList()
                    .ForEach( kv => values.Add( kv.Key, kv.Value ) );

                PersonService.SaveUserPreferences( CurrentPerson, values );
            }
        }

        /// <summary>
        /// Deletes a user preference value for the specified key
        /// </summary>
        /// <param name="key">A <see cref="System.String"/> representing the name of the key.</param>
        public void DeleteUserPreference( string key )
        {
            var sessionValues = SessionUserPreferences();
            if ( sessionValues.ContainsKey( key ) )
            {
                sessionValues.Remove( key );
            }

            if ( CurrentPerson != null )
            {
                PersonService.DeleteUserPreference( CurrentPerson, key );
            }
        }

        /// <summary>
        /// Returns the current user's preferences, if they have previously been loaded into the session, they
        /// will be retrieved from there, otherwise they will be retrieved from the database, added to session and
        /// then returned
        /// </summary>
        /// <returns>A <see cref="System.Collections.Generic.Dictionary{String, List}"/> containing the user preferences
        /// for the current user. If the current user is anonymous or unknown an empty dictionary will be returned.</returns>
        public Dictionary<string, string> SessionUserPreferences()
        {
            string sessionKey = string.Format( "{0}_{1}",
                Person.USER_VALUE_ENTITY, CurrentPerson != null ? CurrentPerson.Id : 0 );

            var userPreferences = Session[sessionKey] as Dictionary<string, string>;
            if ( userPreferences == null )
            {
                if ( CurrentPerson != null )
                {
                    userPreferences = PersonService.GetUserPreferences( CurrentPerson );
                }
                else
                {
                    userPreferences = new Dictionary<string, string>();
                }
                Session[sessionKey] = userPreferences;
            }

            return userPreferences;
        }

        #endregion

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the Click event of the trigger control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void trigger_Click( object sender, EventArgs e )
        {
            var dataControl = this.Form.FindControl( "rock-config-trigger-data" );
            if ( dataControl != null && dataControl is HiddenField )
            {
                string triggerData = ( (HiddenField)dataControl ).Value;

                if ( triggerData.StartsWith( "BLOCK_UPDATED:" ) )
                {
                    int blockId = int.MinValue;
                    if ( int.TryParse( triggerData.Replace( "BLOCK_UPDATED:", "" ), out blockId ) )
                    {
                        OnBlockUpdated( blockId );
                    }
                }
            }
        }

        /// <summary>
        /// Updates the blocks.
        /// </summary>
        /// <param name="blockTypePath">The block type path.</param>
        public void UpdateBlocks( string blockTypePath )
        {
            foreach ( var rockBlock in RockBlocks )
            {
                if ( rockBlock.BlockCache.BlockType.Path.Equals( blockTypePath, StringComparison.OrdinalIgnoreCase ) )
                {
                    OnBlockUpdated( rockBlock.BlockId );
                }
            }
        }

        /// <summary>
        /// Occurs when a block's properties are updated.
        /// </summary>
        internal event EventHandler<BlockUpdatedEventArgs> BlockUpdated;

        /// <summary>
        /// Called when a block's properties are updated.
        /// </summary>
        /// <param name="blockId">The block identifier.</param>
        private void OnBlockUpdated( int blockId )
        {
            if ( BlockUpdated != null )
            {
                BlockUpdated( this, new BlockUpdatedEventArgs( blockId ) );
            }
        }
        /// <summary>
        /// Handles the Navigate event of the scriptManager control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="HistoryEventArgs"/> instance containing the event data.</param>
        protected void scriptManager_Navigate( object sender, HistoryEventArgs e )
        {
            if ( PageNavigate != null )
            {
                PageNavigate( this, e );
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
    public delegate void PageNavigateEventHandler( object sender, HistoryEventArgs e );

    /// <summary>
    /// Event Argument used when block properties are updated
    /// </summary>
    internal class BlockUpdatedEventArgs : EventArgs
    {
        public int BlockID { get; private set; }

        public BlockUpdatedEventArgs( int blockId )
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
        /// A <see cref="System.String"/> representing the Action.
        /// </value>
        public string Action { get; set; }

        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        /// <value>
        /// The return <see cref="System.Object"/>
        /// </value>
        public object Result { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonResult"/> class.
        /// </summary>
        /// <param name="action">A <see cref="System.String"/>representing the action.</param>
        /// <param name="result">A <see cref="System.Object"/> representing the result.</param>
        public JsonResult( string action, object result )
        {
            Action = action;
            Result = result;
        }

        /// <summary>
        /// Serializes this instance.
        /// </summary>
        /// <returns>A <see cref="System.String"/> representing a serialized version of this instance.</returns>
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
