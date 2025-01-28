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
using System.Data.Entity;
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

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

using Rock.Attribute;
using Rock.Blocks;
using Rock.Cms.Utm;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Net;
using Rock.Observability;
using Rock.Security;
using Rock.Tasks;
using Rock.Transactions;
using Rock.Utility;
using Rock.ViewModels.Crm;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

using Page = System.Web.UI.Page;

namespace Rock.Web.UI
{
    /// <summary>
    /// RockPage is the base abstract class that all page templates in Rock should inherit from
    /// </summary>
    /// <seealso cref="System.Web.UI.Page" />
    public abstract class RockPage : Page, IHttpAsyncHandler
    {
        #region Private Variables

        private PlaceHolder phLoadStats;
        private LinkButton _btnRestoreImpersonatedByUser;
        private ScriptManager _scriptManager;
        private HiddenField _hfInteractionGuid;
        private PageCache _pageCache = null;

        private string _clientType = null;
        private BrowserInfo _browserInfo = null;
        private BrowserClient _browserClient = null;

        private bool _showDebugTimings = false;
        private double _previousTiming = 0;
        private TimeSpan _tsDuration;
        private double _duration = 0;

        private PageStatePersister _PageStatePersister = null;

        /// <summary>
        /// Will be <c>true</c> if the page has anything on it that requires
        /// Obsidian libraries to be loaded.
        /// </summary>
        private bool _pageNeedsObsidian = false;

        private readonly string _obsidianPageTimingControlId = "lObsidianPageTimings";
        private readonly List<DebugTimingViewModel> _debugTimingViewModels = new List<DebugTimingViewModel>();
        private Stopwatch _onLoadStopwatch = null;

        /// <summary>
        /// The fingerprint to use with obsidian files.
        /// </summary>
        private static long _obsidianFingerprint = 0;

        /// <summary>
        /// The obsidian file watchers.
        /// </summary>
        private static readonly List<FileSystemWatcher> _obsidianFileWatchers = new List<FileSystemWatcher>();

        /// <summary>
        /// The service provider to use during requests.
        /// </summary>
        private static readonly Lazy<IServiceProvider> _lazyServiceProvider = new Lazy<IServiceProvider>( CreateServiceProvider );

        /// <summary>
        /// The service scopes that should be disposed.
        /// </summary>
        private readonly List<IServiceScope> _pageServiceScopes = new List<IServiceScope>();

        /// <summary>
        /// A list of blocks (their paths) that will force the obsidian libraries to be loaded.
        /// This is particularly useful when a block has a settings dialog that is dependent on
        /// obsidian, but the block itself is not.
        /// </summary>
        private static readonly List<string> _blocksToForceObsidianLoad = new List<string>
        {
            "~/Blocks/Cms/PageZoneBlocksEditor.ascx",
            "~/Blocks/Mobile/MobilePageDetail.ascx"
        };

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
        /// Gets the current visitor if <see cref="Site.EnableVisitorTracking"/> is enabled.
        /// </summary>
        /// <value>The current visitor.</value>
        public Rock.Model.PersonAlias CurrentVisitor { get; private set; }

        /// <summary>
        /// Gets the Ids of <see cref="PersonalizationSegmentCache">Personalization Segments</see> for the <see cref="CurrentVisitor"/>
        /// or <see cref="CurrentPerson"/> if <see cref="Site.EnablePersonalization">personalization is enabled for the site</see>.
        /// </summary>
        /// <value>The personalization segment ids.</value>
        public int[] PersonalizationSegmentIds { get; private set; }

        /// <summary>
        /// Gets the Ids of <see cref="RequestFilterCache">Personalization Request Filters</see> for the current <see cref="Page.Request"/>
        /// if <see cref="Site.EnablePersonalization">personalization is enabled for the site</see>.
        /// </summary>
        /// <value>The personalization segment ids.</value>
        public int[] PersonalizationRequestFilterIds { get; private set; }

        /// <summary>
        /// Gets or sets the request context. This contains all the details
        /// about the network request.
        /// </summary>
        /// <value>
        /// The request context.
        /// </value>
        public RockRequestContext RequestContext { get; private set; }

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
                        Context.AddOrReplaceItem( "CurrentUser", _CurrentUser );
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
                    Context.AddOrReplaceItem( "CurrentUser", _CurrentUser );
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
                    Context.AddOrReplaceItem( "CurrentPerson", value );
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
        /// Gets a value indicating whether this instance is being served in a <see cref="ModalIFrameDialog"/>.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is being served in a <see cref="ModalIFrameDialog"/>; otherwise, <c>false</c>.
        /// </value>
        public bool IsIFrameModal
        {
            get
            {
                return this.PageParameter( "IsIFrameModal" ).AsBoolean();
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
            get
            {
                if ( _PageStatePersister == null )
                {
                    _PageStatePersister = new RockHiddenFieldPageStatePersister( this, RockHiddenFieldPageStatePersister.ViewStateCompressionThreshold );
                }
                return _PageStatePersister;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes the <see cref="RockPage"/> class.
        /// </summary>
        static RockPage()
        {
            InitializeObsidianFingerprint();
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

        /// <summary>
        /// Occurs when [page initialized]. This event is for registering any custom event shortkeys for the page.
        /// </summary>
        protected virtual void RegisterShortcutKeys()
        {
            // Register the shortcut keys with debouncing
            string script = @"
                (function() {
                    var lastDispatchTime = 0;
                    var lastDispatchedElement = null;
                    var debounceDelay = 500;

                    document.addEventListener('keydown', function (event) {
                        if (event.altKey) {
                            var shortcutKey = event.key.toLowerCase();

                            // Check if a shortcut key is registered for the pressed key
                            var element = document.querySelector('[data-shortcut-key=""' + shortcutKey + '""]');

                    
                            if (element) {
                                var currentTime = performance.now();

                                if (lastDispatchedElement === element && (currentTime - lastDispatchTime) < debounceDelay) {
                                    return;
                                }

                                lastDispatchTime = currentTime;
                                lastDispatchedElement = element;

                                if (shortcutKey === 'arrowright' || shortcutKey === 'arrowleft') {
                                    event.preventDefault();
                                }

                                event.preventDefault();
                                element.click();
                            }
                        }
                    });
                })();
            ";

            ScriptManager.RegisterStartupScript( this, typeof( RockPage ), "ShortcutKeys", script, true );
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

            if ( customPersister != null )
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
            // Add configuration specific to Rock Page to the observability activity
            if ( Activity.Current != null )
            {
                Activity.Current.DisplayName = $"PAGE: {Context.Request.HttpMethod} {PageReference.Route}";

                // If the route has parameters show the route slug, otherwise use the request path
                if ( PageReference.Parameters.Count > 0 )
                {
                    Activity.Current.DisplayName = $"PAGE: {Context.Request.HttpMethod} {PageReference.Route}";
                }
                else
                {
                    Activity.Current.DisplayName = $"PAGE: {Context.Request.HttpMethod} {Context.Request.Path}";
                }

                // Highlight postbacks
                if ( this.IsPostBack )
                {
                    Activity.Current.DisplayName = Activity.Current.DisplayName + " [Postback]";
                }
                else
                {
                    // Only add a metric if for non-postback requests
                    var pageTags = RockMetricSource.CommonTags;
                    pageTags.Add( "rock-page", this.PageId );
                    pageTags.Add( "rock-site", this.Site.Name );
                    RockMetricSource.PageRequestCounter.Add( 1, pageTags );
                }

                // Add attributes
                Activity.Current.AddTag( "rock.otel_type", "rock-page" );
                Activity.Current.AddTag( "rock.current_user", this.CurrentUser?.UserName );
                Activity.Current.AddTag( "rock.current_person", this.CurrentPerson?.FullName );
                Activity.Current.AddTag( "rock.current_visitor", this.CurrentVisitor?.AliasPersonGuid );
                Activity.Current.AddTag( "rock.site.id", this.Site.Id );
                Activity.Current.AddTag( "rock.page.id", this.PageId );
                Activity.Current.AddTag( "rock.page.ispostback", this.IsPostBack );
            }

            var stopwatchInitEvents = Stopwatch.StartNew();

            // Register shortcut keys
            RegisterShortcutKeys();

#pragma warning disable 618
            ConvertLegacyContextCookiesToJSON();
#pragma warning restore 618

            if ( _pageCache != null )
            {
                RequestContext.PrepareRequestForPage( _pageCache );
            }

            _showDebugTimings = this.PageParameter( "ShowDebugTimings" ).AsBoolean();

            if ( _showDebugTimings )
            {
                _tsDuration = RockDateTime.Now.Subtract( ( DateTime ) Context.Items["Request_Start_Time"] );
                _previousTiming = _tsDuration.TotalMilliseconds;
                _pageNeedsObsidian = true;
            }

            bool canAdministratePage = false;
            bool canEditPage = false;

            if ( _showDebugTimings )
            {
                stopwatchInitEvents.Stop();
                _debugTimingViewModels.Add( GetDebugTimingOutput( "Server Start Initialization", stopwatchInitEvents.Elapsed.TotalMilliseconds, 0, true ) );
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

            _scriptManager.Scripts.Add( new ScriptReference( "~/Scripts/Bundles/RockLibs" ) );
            _scriptManager.Scripts.Add( new ScriptReference( "~/Scripts/Bundles/RockUi" ) );
            _scriptManager.Scripts.Add( new ScriptReference( "~/Scripts/Bundles/RockValidation" ) );

            /*
                2/16/2021 - JME
                The code below provides the opportunity for an external system to disable
                partial postbacks. This was put in place to allow dynamic language translation
                tools to be able to proxy Rock requests and translate the output. Partial postbacks
                were not able to be translated.
            */
            if ( Request.Headers["Disable_Postbacks"].AsBoolean() )
            {
                _scriptManager.EnablePartialRendering = false;
            }

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

            if ( _showDebugTimings )
            {
                stopwatchInitEvents.Stop();
                _debugTimingViewModels.Add( GetDebugTimingOutput( "Check For Logout", stopwatchInitEvents.Elapsed.TotalMilliseconds, 1 ) );
                stopwatchInitEvents.Restart();
            }

            // If the logout parameter was entered, delete the user's forms authentication cookie and redirect them
            // back to the same page.
            Page.Trace.Warn( "Checking for logout request" );
            if ( PageParameter( "Logout" ) != string.Empty )
            {
                if ( CurrentUser != null )
                {
                    var message = new UpdateUserLastActivity.Message
                    {
                        UserId = CurrentUser.Id,
                        LastActivityDate = RockDateTime.Now,
                        IsOnline = false
                    };
                    message.Send();
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
                            if ( key != null && !key.Equals( "Logout", StringComparison.OrdinalIgnoreCase ) )
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

            if ( _showDebugTimings )
            {
                stopwatchInitEvents.Stop();
                _debugTimingViewModels.Add( GetDebugTimingOutput( "Create Rock Context", stopwatchInitEvents.Elapsed.TotalMilliseconds, 1 ) );
                stopwatchInitEvents.Restart();
            }

            // If the impersonated query key was included or is in session then set the current person
            Page.Trace.Warn( "Checking for person impersonation" );
            if ( !ProcessImpersonation( rockContext ) )
            {
                return;
            }

            // Get current user/person info
            Page.Trace.Warn( "Getting CurrentUser" );
            Rock.Model.UserLogin user = CurrentUser;

            if ( _showDebugTimings )
            {
                stopwatchInitEvents.Stop();
                _debugTimingViewModels.Add( GetDebugTimingOutput( "Get Current User", stopwatchInitEvents.Elapsed.TotalMilliseconds, 1 ) );
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

                if ( _showDebugTimings )
                {
                    stopwatchInitEvents.Stop();
                    _debugTimingViewModels.Add( GetDebugTimingOutput( "Get Current Person", stopwatchInitEvents.Elapsed.TotalMilliseconds, 1 ) );
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

                // Check that they are two-factor authenticated if two-factor authentication is required for their protection profile.
                var securitySettings = new SecuritySettingsService().SecuritySettings;

                if ( securitySettings.RequireTwoFactorAuthenticationForAccountProtectionProfiles?.Contains( user.Person.AccountProtectionProfile ) == true
                     && !user.IsTwoFactorAuthenticated )
                {
                    // Sign out and redirect to the login page to force two-factor authentication.
                    Authorization.SignOut();

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
                    ( ( RockMasterPage ) this.Master ).SetPage( _pageCache );
                }

                // Add CSS class to body
                if ( !string.IsNullOrWhiteSpace( this.BodyCssClass ) && this.Master != null )
                {
                    // attempt to find the body tag
                    var body = ( HtmlGenericControl ) this.Master.FindControl( "body" );
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
                if ( !WebRequestHelper.IsSecureConnection( HttpContext.Current ) && ( _pageCache.RequiresEncryption || Site.RequiresEncryption ) )
                {
                    string redirectUrl = Request.UrlProxySafe().ToString().Replace( "http:", "https:" );

                    // Clear the session state cookie so it can be recreated as secured (see engineering note in Global.asax EndRequest)
                    SessionStateSection sessionState = ( SessionStateSection ) ConfigurationManager.GetSection( "system.web/sessionState" );
                    string sidCookieName = sessionState.CookieName; // ASP.NET_SessionId
                    var cookie = Response.Cookies[sidCookieName];
                    cookie.Expires = RockDateTime.SystemDateTime.AddDays( -1 );
                    AddOrUpdateCookie( cookie );

                    Response.Redirect( redirectUrl, false );
                    Context.ApplicationInstance.CompleteRequest();
                    return;
                }

                // Verify that the current user is allowed to view the page.
                Page.Trace.Warn( "Checking if user is authorized" );

                var isCurrentPersonAuthorized = _pageCache.IsAuthorized( Authorization.VIEW, CurrentPerson );

                if ( _showDebugTimings )
                {
                    stopwatchInitEvents.Stop();
                    _debugTimingViewModels.Add( GetDebugTimingOutput( "Is Current Person Authorized", stopwatchInitEvents.Elapsed.TotalMilliseconds, 1 ) );
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
                    /* At this point, we know the Person (or NULL person) is authorized to View the page */

                    if ( Site.EnableVisitorTracking )
                    {
                        bool isLoggingIn = this.PageId == Site.LoginPageId;

                        // Check if this is the Login page. If so, we don't need do Visitor logic,
                        // and we can avoid a situation where an un-needed Ghost alias could get created.
                        if ( !isLoggingIn )
                        {
                            Page.Trace.Warn( "Processing Current Visitor" );

                            // Visitor Tracking is enabled, and we aren't logging in so do the visitor logic.
                            ProcessCurrentVisitor();
                        }
                    }

                    if ( Site.EnablePersonalization )
                    {
                        Page.Trace.Warn( "Loading Personalization Data" );
                        LoadPersonalizationSegments();
                        LoadPersonalizationRequestFilters();
                    }

                    // Set current models (context)
                    Page.Trace.Warn( "Checking for Context" );
                    try
                    {
                        // Check to see if a context from the query string should be saved to a cookie before
                        // building the model context for the page.
                        SetCookieContextFromQueryString( rockContext );

                        if ( _showDebugTimings )
                        {
                            stopwatchInitEvents.Stop();
                            _debugTimingViewModels.Add( GetDebugTimingOutput( "Set Page Context(s)", stopwatchInitEvents.Elapsed.TotalMilliseconds, 1 ) );
                            stopwatchInitEvents.Restart();
                        }

                        // Build the model context, including all context objects (site-wide and page-specific).
                        this.ModelContext = BuildPageContextData( ContextEntityScope.All );

                        if ( _showDebugTimings )
                        {
                            stopwatchInitEvents.Stop();
                            _debugTimingViewModels.Add( GetDebugTimingOutput( "Check Page Contexts", stopwatchInitEvents.Elapsed.TotalMilliseconds, 1 ) );
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

                    // The Short Link Modal in the System Dialogs need the page to have obsidian.
                    if ( canAdministratePage )
                    {
                        _pageNeedsObsidian = true;
                    }
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

                    if ( _showDebugTimings )
                    {
                        stopwatchInitEvents.Stop();
                        _debugTimingViewModels.Add( GetDebugTimingOutput( "Can Administrate Page", stopwatchInitEvents.Elapsed.TotalMilliseconds, 1 ) );
                        stopwatchInitEvents.Restart();
                    }

                    // Create a javascript object to store information about the current page for client side scripts to use
                    Page.Trace.Warn( "Creating JS objects" );
                    if ( !ClientScript.IsStartupScriptRegistered( "rock-js-object" ) )
                    {
                        var script = $@"
Rock.settings.initialize({{
    siteId: {_pageCache.Layout.SiteId},
    layoutId: {_pageCache.LayoutId},
    pageId: {_pageCache.Id},
    layout: '{_pageCache.Layout.FileName}',
    baseUrl: '{ResolveUrl( "~" )}'
}});";

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

                    if ( _showDebugTimings )
                    {
                        stopwatchInitEvents.Stop();
                        _debugTimingViewModels.Add( GetDebugTimingOutput( "Server Block OnInit", stopwatchInitEvents.Elapsed.TotalMilliseconds, 1, true ) );
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
                        var stopwatchBlockInit = Stopwatch.StartNew();
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
                        if ( zone != null && ( canAdministrate || canEdit || canView ) )
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
                                    if ( !string.IsNullOrWhiteSpace( block.BlockType.Path ) )
                                    {
                                        control = TemplateControl.LoadControl( block.BlockType.Path );
                                        control.ClientIDMode = ClientIDMode.AutoID;

                                        // These blocks needs Obsidian for their settings dialog to display properly.
                                        if ( _blocksToForceObsidianLoad.Any( blockTypePath => blockTypePath.Equals( block.BlockType.Path ) ) )
                                        {
                                            _pageNeedsObsidian = true;
                                        }
                                    }
                                    else if ( block.BlockType.EntityTypeId.HasValue )
                                    {
                                        var scope = CreateServiceScope();
                                        var blockEntity = ActivatorUtilities.CreateInstance( scope.ServiceProvider, block.BlockType.EntityType.GetEntityType() );

                                        if ( blockEntity is RockBlockType rockBlockType )
                                        {
                                            rockBlockType.RockContext = scope.ServiceProvider.GetRequiredService<RockContext>();
                                        }

                                        if ( blockEntity is IRockBlockType rockBlockEntity )
                                        {
                                            rockBlockEntity.RequestContext = RequestContext;

                                            var wrapper = new RockBlockTypeWrapper
                                            {
                                                Page = this,
                                                Block = rockBlockEntity
                                            };

                                            wrapper.InitializeAsUserControl( this );
                                            wrapper.AppRelativeTemplateSourceDirectory = "~";

                                            control = wrapper;
                                            control.ClientIDMode = ClientIDMode.AutoID;
                                        }

                                        if ( blockEntity is IRockObsidianBlockType )
                                        {
                                            _pageNeedsObsidian = true;
                                        }
                                    }

                                    if ( control == null )
                                    {
                                        throw new Exception( "Cannot instantiate unknown block type" );
                                    }
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
                                    blockControl.RequestContext = RequestContext;
                                    control = new RockBlockWrapper( blockControl );
                                }
                            }

                            zone.Controls.Add( control );
                            if ( control is RockBlockWrapper )
                            {
                                ( ( RockBlockWrapper ) control ).EnsureBlockControls();
                            }

                            if ( _showDebugTimings )
                            {

                                stopwatchBlockInit.Stop();
                                _debugTimingViewModels.Add( GetDebugTimingOutput( block.Name, stopwatchBlockInit.Elapsed.TotalMilliseconds, 2, false, $"({block.BlockType})" ) );
                            }
                        }
                    }

                    if ( _pageNeedsObsidian )
                    {
                        AddScriptLink( "~/Obsidian/obsidian-core.js", true );
                        AddCSSLink( "~/Obsidian/obsidian-vendor.min.css", true );

                        Page.Trace.Warn( "Initializing Obsidian" );

                        var body = ( HtmlGenericControl ) this.Master?.FindControl( "body" );
                        if ( body != null )
                        {
                            body.AddCssClass( "obsidian-loading" );
                        }

                        if ( !ClientScript.IsStartupScriptRegistered( "rock-obsidian-init" ) )
                        {
                            var currentPersonJson = "null";
                            var isAnonymousVisitor = false;

                            if ( CurrentPerson != null && CurrentPerson.Guid != new Guid( SystemGuid.Person.GIVER_ANONYMOUS ) )
                            {
                                currentPersonJson = new CurrentPersonBag
                                {
                                    IdKey = CurrentPerson.IdKey,
                                    Guid = CurrentPerson.Guid,
                                    PrimaryAliasIdKey = CurrentPerson.PrimaryAlias.IdKey,
                                    PrimaryAliasGuid = CurrentPerson.PrimaryAlias.Guid,
                                    FirstName = CurrentPerson.FirstName,
                                    NickName = CurrentPerson.NickName,
                                    LastName = CurrentPerson.LastName,
                                    FullName = CurrentPerson.FullName,
                                    Email = CurrentPerson.Email,
                                }.ToCamelCaseJson( false, false );
                            }
                            else if ( CurrentPerson != null )
                            {
                                isAnonymousVisitor = true;
                            }

                            // Prevent XSS attacks in page parameters.
                            var sanitizedPageParameters = new Dictionary<string, string>();
                            foreach ( var pageParam in PageParameters() )
                            {
                                var sanitizedKey = pageParam.Key.Replace( "</", "<\\/" );
                                var sanitizedValue = pageParam.Value.ToStringSafe().Replace( "</", "<\\/" );

                                sanitizedPageParameters.AddOrReplace( sanitizedKey, sanitizedValue );
                            }

                            var script = $@"
Obsidian.onReady(() => {{
    System.import('@Obsidian/Templates/rockPage.js').then(module => {{
        module.initializePage({{
            executionStartTime: new Date().getTime(),
            pageId: {_pageCache.Id},
            pageGuid: '{_pageCache.Guid}',
            pageParameters: {sanitizedPageParameters.ToJson()},
            interactionGuid: '{RequestContext.RelatedInteractionGuid}',
            currentPerson: {currentPersonJson},
            isAnonymousVisitor: {( isAnonymousVisitor ? "true" : "false" )},
            loginUrlWithReturnUrl: '{GetLoginUrlWithReturnUrl()}'
        }});
    }});
}});

Obsidian.init({{ debug: true, fingerprint: ""v={_obsidianFingerprint}"" }});
";

                            ClientScript.RegisterStartupScript( this.Page.GetType(), "rock-obsidian-init", script, true );
                        }
                    }

                    /*
                     * 2020-06-17 - JH
                     *
                     * When Rock is loaded via an iFrame modal (i.e. Block Properties), the PageReferences for the "main" instance of Rock
                     * are overwritten, causing some Blocks to no longer render their BreadCrumbs once the modal is closed, and subsequent
                     * Page loads/postbacks occur. By providing a suffix for the storage key when in an iFrame modal, we can preserve the
                     * main Rock instance's PageReference history.
                     */
                    Page.Trace.Warn( "Getting breadcrumbs" );

                    var pageReferencesKeySuffix = IsIFrameModal ? "_iFrameModal" : null;
                    var pageReferences = PageReference.GetBreadCrumbPageReferences( this, _pageCache, PageReference, pageReferencesKeySuffix );

                    BreadCrumbs = pageReferences.SelectMany( pr => pr.BreadCrumbs ).ToList();

                    // Update the current page reference to have the correct breadcrumbs.
                    var currentPageReference = pageReferences.FirstOrDefault( pr => pr.PageId == PageReference.PageId );
                    if ( currentPageReference != null )
                    {
                        PageReference.BreadCrumbs = currentPageReference.BreadCrumbs;

                        if ( PageReference.BreadCrumbs.Any() )
                        {
                            PageReference.BreadCrumbs.Last().Active = true;
                        }
                    }

                    // Add the page admin footer if the user is authorized to edit the page
                    if ( _pageCache.IncludeAdminFooter && ( canAdministratePage || canAdministrateBlockOnPage || canEditPage ) )
                    {
                        // Add the page admin script
                        AddScriptLink( Page, "~/Scripts/Bundles/RockAdmin", false );

                        Page.Trace.Warn( "Adding admin footer to page" );
                        HtmlGenericControl adminFooter = new HtmlGenericControl( "div" );
                        adminFooter.ID = "cms-admin-footer";
                        adminFooter.AddCssClass( "js-cms-admin-footer" );
                        adminFooter.ClientIDMode = System.Web.UI.ClientIDMode.Static;
                        this.Form.Controls.Add( adminFooter );

                        phLoadStats = new PlaceHolder();
                        adminFooter.Controls.Add( phLoadStats );

                        var cacheControlCookie = Request.Cookies[RockCache.CACHE_CONTROL_COOKIE];
                        var isCacheEnabled = cacheControlCookie == null || cacheControlCookie.Value.AsBoolean();

                        var cacheIndicator = isCacheEnabled ? "text-success" : "text-danger";
                        var cacheEnabled = isCacheEnabled ? "enabled" : "disabled";

                        var lbCacheControl = new LinkButton();
                        lbCacheControl.Click += lbCacheControl_Click;
                        lbCacheControl.CssClass = $"pull-left margin-l-md {cacheIndicator}";
                        lbCacheControl.ToolTip = $"Web cache {cacheEnabled}";
                        lbCacheControl.Text = "<i class='fa fa-running'></i>";
                        adminFooter.Controls.Add( lbCacheControl );

                        // If the current user is Impersonated by another user, show a link on the admin bar to log back in as the original user
                        var impersonatedByUser = Session["ImpersonatedByUser"] as UserLogin;
                        var currentUserIsImpersonated = ( HttpContext.Current?.User?.Identity?.Name ?? string.Empty ).StartsWith( "rckipid=" );
                        if ( canAdministratePage && currentUserIsImpersonated && impersonatedByUser != null )
                        {
                            HtmlGenericControl impersonatedByUserDiv = new HtmlGenericControl( "span" );
                            impersonatedByUserDiv.AddCssClass( "label label-default margin-l-md" );
                            _btnRestoreImpersonatedByUser = new LinkButton();
                            _btnRestoreImpersonatedByUser.ID = "_btnRestoreImpersonatedByUser";
                            //_btnRestoreImpersonatedByUser.CssClass = "btn";
                            _btnRestoreImpersonatedByUser.Visible = impersonatedByUser != null;
                            _btnRestoreImpersonatedByUser.Click += _btnRestoreImpersonatedByUser_Click;
                            _btnRestoreImpersonatedByUser.Text = $"<i class='fa-fw fa fa-unlock'></i> " + $"Restore {impersonatedByUser?.Person?.ToString()}";
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
                            aBlockConfig.Attributes.Add( "class", "btn block-config js-block-config" );
                            aBlockConfig.Attributes.Add( "href", "javascript: Rock.admin.pageAdmin.showBlockConfig();" );
                            aBlockConfig.Attributes.Add( "Title", "Block Configuration (Alt-B)" );
                            HtmlGenericControl iBlockConfig = new HtmlGenericControl( "i" );
                            aBlockConfig.Controls.Add( iBlockConfig );
                            iBlockConfig.Attributes.Add( "class", "fa fa-th-large" );
                        }

                        if ( canEditPage || canAdministratePage )
                        {
                            // RockPage Properties
                            HtmlGenericControl aPageProperties = new HtmlGenericControl( "a" );
                            buttonBar.Controls.Add( aPageProperties );
                            aPageProperties.ID = "aPageProperties";
                            aPageProperties.ClientIDMode = System.Web.UI.ClientIDMode.Static;
                            aPageProperties.Attributes.Add( "class", "btn properties js-page-properties" );
                            aPageProperties.Attributes.Add( "href", "javascript: Rock.controls.modal.show($(this), '" + ResolveUrl( string.Format( "~/PageProperties/{0}?t=Page Properties", _pageCache.Id ) ) + "')" );
                            aPageProperties.Attributes.Add( "Title", "Page Properties (Alt+P)" );
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
                            aChildPages.Attributes.Add( "class", "btn page-child-pages js-page-child-pages" );
                            aChildPages.Attributes.Add( "href", "javascript: Rock.controls.modal.show($(this), '" + ResolveUrl( string.Format( "~/pages/{0}?t=Child Pages&pb=&sb=Done", _pageCache.Id ) ) + "')" );
                            aChildPages.Attributes.Add( "Title", "Child Pages (Alt+L)" );
                            HtmlGenericControl iChildPages = new HtmlGenericControl( "i" );
                            aChildPages.Controls.Add( iChildPages );
                            iChildPages.Attributes.Add( "class", "fa fa-sitemap" );

                            // RockPage Zones
                            HtmlGenericControl aPageZones = new HtmlGenericControl( "a" );
                            buttonBar.Controls.Add( aPageZones );
                            aPageZones.Attributes.Add( "class", "btn page-zones js-page-zones" );
                            aPageZones.Attributes.Add( "href", "javascript: Rock.admin.pageAdmin.showPageZones();" );
                            aPageZones.Attributes.Add( "Title", "Page Zones (Alt+Z)" );
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

                            // ShortLink Properties
                            HtmlGenericControl aShortLink = new HtmlGenericControl( "a" );
                            buttonBar.Controls.Add( aShortLink );
                            aShortLink.ID = "aShortLink";
                            aShortLink.ClientIDMode = System.Web.UI.ClientIDMode.Static;
                            aShortLink.Attributes.Add( "class", "btn properties" );
                            aShortLink.Attributes.Add( "href", "javascript: Rock.controls.modal.show($(this), '" +
                                ResolveUrl( string.Format( "~/ShortLink/{0}?t=Shortened Link&Url={1}", _pageCache.Id, Server.UrlEncode( HttpContext.Current.Request.UrlProxySafe().AbsoluteUri.ToString() ) ) )
                                + "')" );
                            aShortLink.Attributes.Add( "Title", "Add Short Link" );
                            HtmlGenericControl iShortLink = new HtmlGenericControl( "i" );
                            aShortLink.Controls.Add( iShortLink );
                            iShortLink.Attributes.Add( "class", "fa fa-link" );

                            // Obsidian ShortLink Properties - TO BE UNCOMMENTED IN v17
/*
                            var administratorShortlinkScript = $@"Obsidian.onReady(() => {{
    System.import('@Obsidian/Templates/rockPage.js').then(module => {{
        module.showShortLink('{ResolveUrl( string.Format( "~/ShortLink/{0}?t=Shortened Link&Url={1}", _pageCache.Id, Server.UrlEncode( HttpContext.Current.Request.UrlProxySafe().AbsoluteUri.ToString() ) ) )}');
    }});
}});";
                            HtmlGenericControl aObsidianShortLink = new HtmlGenericControl( "a" );
                            buttonBar.Controls.Add( aObsidianShortLink );
                            aObsidianShortLink.ID = "aObsidianShortLink";
                            aObsidianShortLink.ClientIDMode = System.Web.UI.ClientIDMode.Static;
                            aObsidianShortLink.Attributes.Add( "class", "btn properties" );
                            aObsidianShortLink.Attributes.Add( "href", "#" );
                            aObsidianShortLink.Attributes.Add( "onclick", $"event.preventDefault(); {administratorShortlinkScript}" );
                            aObsidianShortLink.Attributes.Add( "Title", "Add Obsidian Short Link" );
                            HtmlGenericControl iObsidianShortLink = new HtmlGenericControl( "i" );
                            aObsidianShortLink.Controls.Add( iObsidianShortLink );
                            iObsidianShortLink.Attributes.Add( "class", "fa fa-link" );
*/

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
                    if ( _pageCache.CacheControlHeader != null )
                    {
                        _pageCache.CacheControlHeader.SetupHttpCachePolicy( Response.Cache );
                    }
                }

                // Put a hidden field on the form that contains the interaction
                // unique identifier of the original page load. When the view
                // state is loaded the value will be replaced with the original.
                _hfInteractionGuid = new HiddenField
                {
                    ID = "hfInteractionGuid",
                    Value = RequestContext.RelatedInteractionGuid.ToString()
                };

                Form.Controls.Add( _hfInteractionGuid );

                Page.Trace.Warn( "Setting meta tags" );

                stopwatchInitEvents.Restart();

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

                if ( !string.IsNullOrWhiteSpace( _pageCache.Layout.Site.PageHeaderContent ) )
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

                // Add reponse headers to request that the client tell us if they prefer dark mode
                Response.Headers.Add( "Accept-CH", "Sec-CH-Prefers-Color-Scheme" );
                Response.Headers.Add( "Vary", "Sec-CH-Prefers-Color-Scheme" );
                Response.Headers.Add( "Critical-CH", "Sec-CH-Prefers-Color-Scheme" );

                if ( _showDebugTimings )
                {
                    stopwatchInitEvents.Stop();
                    _debugTimingViewModels.Add( GetDebugTimingOutput( "Server Complete Initialization", stopwatchInitEvents.Elapsed.TotalMilliseconds, 0, true ) );
                    _debugTimingViewModels.Add( GetDebugTimingOutput( "Server Block OnLoad", stopwatchInitEvents.Elapsed.TotalMilliseconds, 0, true ) );
                    stopwatchInitEvents.Restart();
                }

                if ( _showDebugTimings && canAdministratePage )
                {
                    Page.Trace.Warn( "Initializing Obsidian Page Timings" );
                    Page.Form.Controls.Add( new Literal
                    {
                        ID = _obsidianPageTimingControlId,
                        Text = $@"
<span>
    <style>
        .debug-timestamp {{
            text-align: right;
        }}

        .debug-waterfall {{
            width: 40%;
            position: relative;
            vertical-align: middle !important;
            padding: 0 !important;
        }}

        .debug-chart-bar {{
            position: absolute;
            display: block;
            min-width: 1px;
            height: 1.125em;
            background: #009ce3;
            margin-top: -0.5625em;
        }}
    </style>
    <div id=""{_obsidianPageTimingControlId}""></div>
</span>"
                    } );
                }
            }
        }

        /// <summary>
        /// Checks to see if a context from query string should be saved to a cookie.  This is used during
        /// OnInit to ensure the context is set in the page data.
        /// </summary>
        /// <param name="rockContext">The <see cref="RockContext"/>.</param>
        private void SetCookieContextFromQueryString( RockContext rockContext )
        {
            char[] delim = new char[1] { ',' };

            foreach ( string param in PageParameter( "SetContext", true ).Split( delim, StringSplitOptions.RemoveEmptyEntries ) )
            {
                string[] parts = param.Split( '|' );
                if ( parts.Length != 2 )
                {
                    continue; // Cookie value is invalid (not delimited).
                }

                var contextModelEntityType = EntityTypeCache.Get( parts[0], false, rockContext );
                if ( contextModelEntityType == null )
                {
                    continue; // Couldn't load EntityType.
                }

                int? contextId = parts[1].AsIntegerOrNull();
                if ( contextId == null )
                {
                    continue;  // Invalid Entity Id.
                }

                var contextModelType = contextModelEntityType.GetEntityType();
                var contextDbContext = Reflection.GetDbContextForEntityType( contextModelType );
                if ( contextDbContext == null )
                {
                    continue;  // Failed to load DbContext.
                }

                var contextService = Reflection.GetServiceForEntityType( contextModelType, contextDbContext );
                if ( contextService == null )
                {
                    continue; // Couldn't load Entity service.
                }

                MethodInfo getMethod = contextService.GetType().GetMethod( "Get", new Type[] { typeof( int ) } );
                if ( getMethod == null )
                {
                    continue;  // Couldn't find method to fetch Entity.
                }

                var getResult = getMethod.Invoke( contextService, new object[] { contextId.Value } );
                var contextEntity = getResult as IEntity;
                if ( contextEntity == null )
                {
                    continue;  // Entity doesn't seem to exist.
                }

                // If all the checks up to this point have succeeded, we have a real entity and we can add it to the
                // Cookie Context.  Note that this is being added as a site-wide context object.
                SetContextCookie( contextEntity, false, false );
            }
        }

        /// <summary>
        /// Builds the <see cref="KeyEntity"/> dictionary for the page (this is used to set the
        /// ModelContext property as well as by the GetScopedContextEntities() method).
        /// </summary>
        private Dictionary<string, Data.KeyEntity> BuildPageContextData( ContextEntityScope scope )
        {
            var keyEntityDictionary = new Dictionary<string, Data.KeyEntity>();

            // The order things are added to the ModelContext collection is important.  Since we're using
            // AddOrReplace, the last object of any given type will be the context object that ends up in
            // the collection.

            // Cookie context objects are checked first, according to scope.  If objects exist in
            // pageContext, they will replace any objects added here (e.g., a context object set by the
            // Group Detail block will take precedence over a Group that was set in a Cookie Context.

            if ( scope == ContextEntityScope.All || scope == ContextEntityScope.Site )
            {
                // Load site-wide context objects from the cookie.
                var siteCookieName = GetContextCookieName( false );
                var siteCookie = FindCookie( siteCookieName );
                keyEntityDictionary = AddCookieContextEntities( siteCookie, keyEntityDictionary );
            }

            // If we're only looking for the "Site" scope, then we're done, now, and we can skip the rest
            // of this method.
            if ( scope == ContextEntityScope.Site )
            {
                return keyEntityDictionary;
            }

            // Load any page-specific context objects from the cookie.  These will replace any
            // site-wide values (if the scope is "All").
            var cookieName = GetContextCookieName( true );
            var cookie = FindCookie( cookieName );
            keyEntityDictionary = AddCookieContextEntities( cookie, keyEntityDictionary );


            // Check to see if any of the ModelContext.Keys that got set from Cookies included in the
            // query string.  If so, the URL value overrides the Cookie value.
            foreach ( var modelContextName in keyEntityDictionary.Keys.ToList() )
            {
                var type = Type.GetType( modelContextName, false, false );
                if ( type == null )
                {
                    continue;
                }

                // Look for Id first, this can be either integer, guid or IdKey.
                var contextId = PageParameter( type.Name + "Id" );
                if ( contextId.IsNotNullOrWhiteSpace() )
                {
                    if ( !Site.DisablePredictableIds && int.TryParse( contextId, out var id ) )
                    {
                        keyEntityDictionary.AddOrReplace( modelContextName, new KeyEntity( id ) );
                    }
                    else if ( Guid.TryParse( contextId, out var guid ) )
                    {
                        keyEntityDictionary.AddOrReplace( modelContextName, new KeyEntity( guid ) );
                    }
                    else if ( IdHasher.Instance.TryGetId( contextId, out id ) )
                    {
                        keyEntityDictionary.AddOrReplace( modelContextName, new KeyEntity( id ) );
                    }
                }

                // If Guid is present, it will override Id.
                Guid? contextGuid = PageParameter( type.Name + "Guid" ).AsGuidOrNull();
                if ( contextGuid.HasValue )
                {
                    keyEntityDictionary.AddOrReplace( modelContextName, new Data.KeyEntity( contextGuid.Value ) );
                }
            }

            // Check for page context that were explicitly set in Page Properties.  These will
            // override any values that were already set, either by cookies or the query string
            // (meaning an explicitly set Page Context overrides the generic Id/Guid from the
            // code block immediately preceding this one).
            foreach ( var pageContext in _pageCache.PageContexts )
            {
                var contextId = PageParameter( pageContext.Value );
                if ( contextId.IsNotNullOrWhiteSpace() )
                {
                    if ( !Site.DisablePredictableIds && int.TryParse( contextId, out var id ) )
                    {
                        keyEntityDictionary.AddOrReplace( pageContext.Key, new KeyEntity( id ) );
                    }
                    else if ( Guid.TryParse( contextId, out var guid ) )
                    {
                        keyEntityDictionary.AddOrReplace( pageContext.Key, new KeyEntity( guid ) );
                    }
                    else if ( IdHasher.Instance.TryGetId( contextId, out id ) )
                    {
                        keyEntityDictionary.AddOrReplace( pageContext.Key, new KeyEntity( id ) );
                    }
                }
            }

            // Check for any encrypted context keys specified in query string.  These take precedence
            // over any previously set context values.
            char[] delim = new char[1] { ',' };
            foreach ( string param in PageParameter( "context", true ).Split( delim, StringSplitOptions.RemoveEmptyEntries ) )
            {
                string contextItem = Rock.Security.Encryption.DecryptString( param );
                string[] parts = contextItem.Split( '|' );
                if ( parts.Length == 2 )
                {
                    keyEntityDictionary.AddOrReplace( parts[0], new Data.KeyEntity( parts[1] ) );
                }
            }

            return keyEntityDictionary;
        }

        /// <summary>
        /// If <see cref="SiteCache.EnableVisitorTracking" />, this will determine the <see cref="CurrentVisitor" />
        /// and do any additional processing needed to verify and validate the CurrentVisitor.
        /// </summary>
        private void ProcessCurrentVisitor()
        {
            if ( !Site.EnableVisitorTracking )
            {
                // Visitor Tracking isn't enabled, so we can just return.
                return;
            }

            var currentPersonAlias = this.CurrentPersonAlias;

            var currentPerson = currentPersonAlias?.Person;
            var currentPersonId = currentPersonAlias?.PersonId;

            var rockContext = new RockContext();

            var visitorKeyCookie = GetCookie( Rock.Personalization.RequestCookieKey.ROCK_VISITOR_KEY );
            PersonAlias currentVisitorCookiePersonAlias = null;
            if ( visitorKeyCookie != null )
            {
                var visitorKeyPersonAliasIdKey = visitorKeyCookie.Value;
                if ( visitorKeyPersonAliasIdKey.IsNullOrWhiteSpace() )
                {
                    // There is a ROCK_VISITOR_KEY key, but it doesn't have a value, so invalid visitor key. 
                    visitorKeyCookie = null;
                }
                else
                {
                    currentVisitorCookiePersonAlias = new PersonAliasService( rockContext ).Get( visitorKeyPersonAliasIdKey );
                    if ( currentVisitorCookiePersonAlias == null )
                    {
                        // There is a ROCK_VISITOR_KEY key with an IdKey, but that PersonAlias record
                        // isn't in the database, so it isn't a valid ROCK_VISITOR_KEY.
                        visitorKeyCookie = null;
                    }
                }
            }

            var currentUTCDateTime = RockDateTime.Now.ToUniversalTime();

            var persistedCookieExpirationDays = SystemSettings.GetValue( Rock.SystemKey.SystemSetting.VISITOR_COOKIE_PERSISTENCE_DAYS ).AsIntegerOrNull() ?? 365;
            var persistedCookieExpiration = currentUTCDateTime.AddDays( persistedCookieExpirationDays );

            // Set the Session Start DateTime cookie if it hasn't been set yet
            var rockSessionStartDatetimeCookie = GetCookie( Rock.Personalization.RequestCookieKey.ROCK_SESSION_START_DATETIME );
            if ( rockSessionStartDatetimeCookie == null || rockSessionStartDatetimeCookie.Value.IsNullOrWhiteSpace() )
            {
                rockSessionStartDatetimeCookie = new HttpCookie( Rock.Personalization.RequestCookieKey.ROCK_SESSION_START_DATETIME, currentUTCDateTime.ToISO8601DateString() );
                RockPage.AddOrUpdateCookie( rockSessionStartDatetimeCookie );
            }

            PersonAlias calculatedCurrentVisitor = null;

            if ( visitorKeyCookie == null )
            {
                if ( currentPersonAlias == null )
                {
                    // ROCK_VISITOR_KEY does not exist and there is no current login, so set the ROCK_FIRSTTIME_VISITOR cookie.
                    // This cookie does not specify an expiry, so it is automatically expired when the browser session ends.
                    var firstTimeCookie = new HttpCookie( Rock.Personalization.RequestCookieKey.ROCK_FIRSTTIME_VISITOR, true.ToString() );

                    RockPage.AddOrUpdateCookie( firstTimeCookie );
                }
                else
                {
                    // If ROCK_VISITOR_KEY does not exist and person *is* logged in, create a new ROCK_VISITOR_KEY cookie using the CurrentPersonAlias's IdKey
                    var visitorPersonAliasIdKey = currentPersonAlias.IdKey;
                    visitorKeyCookie = new System.Web.HttpCookie( Rock.Personalization.RequestCookieKey.ROCK_VISITOR_KEY, visitorPersonAliasIdKey )
                    {
                        Expires = persistedCookieExpiration
                    };

                    RockPage.AddOrUpdateCookie( visitorKeyCookie );

                    calculatedCurrentVisitor = currentPersonAlias;
                }
            }
            else
            {
                // ROCK_VISITOR_KEY exists
                if ( currentPersonAlias == null )
                {
                    // ROCK_VISITOR_KEY exists, but nobody is logged in
                    calculatedCurrentVisitor = currentVisitorCookiePersonAlias;

                    // renew, extend cookie
                    visitorKeyCookie.Expires = persistedCookieExpiration;
                    RockPage.AddOrUpdateCookie( visitorKeyCookie );
                }
                else
                {
                    // ROCK_VISITOR_KEY exists, and somebody is logged in
                    if ( currentVisitorCookiePersonAlias.PersonId == currentPersonId )
                    {
                        // Our visitor person alias is already associated with the current person,
                        // so we are good. Extend expiration.
                        visitorKeyCookie.Expires = persistedCookieExpiration;
                        RockPage.AddOrUpdateCookie( visitorKeyCookie );
                    }
                    else
                    {
                        // Visitor Person Alias is either for the core Anonymous Person ( GhostPerson ) or
                        // for some other person that has previously logged into rock with this browser
                        var ghostPersonId = new PersonService( rockContext ).GetOrCreateAnonymousVisitorPersonId();

                        // ROCK_VISITOR_KEY exists, and somebody is logged in
                        if ( currentVisitorCookiePersonAlias.PersonId == ghostPersonId )
                        {
                            // Our current visitor cookie was associated with GhostPerson, but now we have a current person,
                            // so convert the GhostVisitor PersonAlias to a PersonAlias of the CurrentPerson.
                            // NOTE: This needs to be done synchronously because we'll need to know which real person this
                            // PersonAlias is for on subsequent requests.
                            if ( new PersonAliasService( rockContext ).MigrateAnonymousVisitorAliasToRealPerson( currentVisitorCookiePersonAlias, currentPerson ) )
                            {
                                rockContext.SaveChanges();

                                /*  MP 06/16/2022

                                At this point, we might have set FirstTime visitor as true in this session, but then merged with a real person that has been here before.
                                This could mean a false-positive 'First Time Visitor' for the duration of the session, but that is OK.
                                 
                                */
                            }
                        }
                        else
                        {
                            // Our visitor person alias is for some other person that has previously logged into Rock with this browser
                            // So update the cookie to the current person's PersonAlias
                            visitorKeyCookie.Value = currentPersonAlias.IdKey;
                            visitorKeyCookie.Expires = persistedCookieExpiration;

                            RockPage.AddOrUpdateCookie( visitorKeyCookie );
                        }
                    }

                    calculatedCurrentVisitor = currentPersonAlias;
                }
            }

            CurrentVisitor = calculatedCurrentVisitor;

            RockPage.AddOrUpdateCookie( Rock.Personalization.RequestCookieKey.ROCK_VISITOR_LASTSEEN, currentUTCDateTime.ToISO8601DateString(), persistedCookieExpiration );

            if ( CurrentVisitor != null )
            {
                var message = new UpdatePersonAliasLastVisitDateTime.Message
                {
                    PersonAliasId = CurrentVisitor.Id,
                    LastVisitDateTime = RockDateTime.Now,
                };

                message.SendIfNeeded();
            }
        }

        /// <summary>
        /// Loads the matching <see cref="PersonalizationSegmentIds"/> for the <see cref="CurrentPerson"/> or <see cref="CurrentVisitor"/>.
        /// Only call this if the Site.EnablePersonalization is true. 
        /// </summary>
        private void LoadPersonalizationSegments()
        {
            var rockSegmentFiltersCookie = GetCookie( Rock.Personalization.RequestCookieKey.ROCK_SEGMENT_FILTERS );
            var personalizationPersonAliasId = CurrentVisitor?.Id ?? CurrentPersonAliasId;
            if ( !personalizationPersonAliasId.HasValue )
            {
                // no visitor or person logged in
                return;
            }

            var cookieValueJson = rockSegmentFiltersCookie?.Value;
            Personalization.SegmentFilterCookieData segmentFilterCookieData = null;
            if ( cookieValueJson != null )
            {
                segmentFilterCookieData = cookieValueJson.FromJsonOrNull<Personalization.SegmentFilterCookieData>();
                bool isCookieDataValid = false;
                if ( segmentFilterCookieData != null )
                {
                    if ( segmentFilterCookieData.IsSamePersonAlias( personalizationPersonAliasId.Value ) && segmentFilterCookieData.SegmentIdKeys != null )
                    {
                        isCookieDataValid = true;
                    }

                    if ( segmentFilterCookieData.IsStale( RockDateTime.Now ) )
                    {
                        isCookieDataValid = false;
                    }
                }

                if ( !isCookieDataValid )
                {
                    segmentFilterCookieData = null;
                }
            }

            if ( segmentFilterCookieData == null )
            {
                segmentFilterCookieData = new Personalization.SegmentFilterCookieData();
                segmentFilterCookieData.PersonAliasIdKey = IdHasher.Instance.GetHash( personalizationPersonAliasId.Value );
                segmentFilterCookieData.LastUpdateDateTime = RockDateTime.Now;
                var segmentIdKeys = new PersonalizationSegmentService( new RockContext() ).GetPersonalizationSegmentIdKeysForPersonAliasId( personalizationPersonAliasId.Value );
                segmentFilterCookieData.SegmentIdKeys = segmentIdKeys;
            }

            AddOrUpdateCookie( new HttpCookie( Rock.Personalization.RequestCookieKey.ROCK_SEGMENT_FILTERS, segmentFilterCookieData.ToJson() ) );

            this.PersonalizationSegmentIds = segmentFilterCookieData.GetSegmentIds();
        }

        /// <summary>
        /// Loads the matching <see cref="PersonalizationRequestFilterIds"/> for the current <see cref="Page.Request"/>.
        /// </summary>
        private void LoadPersonalizationRequestFilters()
        {
            var requestFilters = RequestFilterCache.All().Where( a => a.IsActive );
            var requestFilterIds = new List<int>();
            foreach ( var requestFilter in requestFilters )
            {
                if ( requestFilter.RequestMeetsCriteria( this.Request, this.Site ) )
                {
                    requestFilterIds.Add( requestFilter.Id );
                }
            }

            this.PersonalizationRequestFilterIds = requestFilterIds.ToArray();
        }

        /// <summary>
        /// Verifies the block type instance properties to make sure they are compiled and have the attributes updated.
        /// </summary>
        private void VerifyBlockTypeInstanceProperties()
        {
            var blockTypesIdToVerify = _pageCache.Blocks.Select( a => a.BlockType ).Distinct().Where( a => a.IsInstancePropertiesVerified == false ).Select( a => a.Id );

            if ( !blockTypesIdToVerify.Any() )
            {
                return;
            }

            Page.Trace.Warn( "\tCreating block attributes" );
            BlockTypeService.VerifyBlockTypeInstanceProperties( blockTypesIdToVerify.ToArray() );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Page.LoadComplete" /> event at the end of the page load stage.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnLoadComplete( EventArgs e )
        {
            base.OnLoadComplete( e );

            // Set the title displayed in the browser on the base page.
            string pageTitle = BrowserTitle ?? string.Empty;
            string siteTitle = _pageCache.Layout.Site.Name;
            string seperator = pageTitle.Trim() != string.Empty && siteTitle.Trim() != string.Empty ? " | " : "";

            base.Title = pageTitle + seperator + siteTitle;

            // Make the last breadcrumb on this page the only one active. This
            // takes care of any late additions to the breadcrumbs by Lava or
            // Obsidian blocks.
            if ( BreadCrumbs != null && BreadCrumbs.Any() )
            {
                BreadCrumbs.ForEach( bc => bc.Active = false );
                BreadCrumbs.Last().Active = true;
            }

            // Finalize the debug settings
            if ( _showDebugTimings )
            {
                _tsDuration = RockDateTime.Now.Subtract( ( DateTime ) Context.Items["Request_Start_Time"] );

                if ( _pageNeedsObsidian )
                {
                    Page.Trace.Warn( "Finalizing Obsidian Page Timings" );
                    if ( !ClientScript.IsStartupScriptRegistered( "rock-obsidian-page-timings" ) )
                    {
                        var script = $@"
Obsidian.onReady(() => {{
    System.import('@Obsidian/Templates/rockPage.js').then(module => {{
        module.initializePageTimings({{
            elementId: '{_obsidianPageTimingControlId}',
            debugTimingViewModels: {_debugTimingViewModels.ToCamelCaseJson( false, true )}
        }});
    }});
}});";

                        ClientScript.RegisterStartupScript( this.Page.GetType(), "rock-obsidian-page-timings", script, true );
                    }
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

                /*
                    10/23/2023 - JMH

                    Bypass two-factor authentication when restoring the "impersonated by" user's session;
                    otherwise, they would have to use two-factor authentication again.

                    Reason: Two-Factor Authentication
                 */
                Rock.Security.Authorization.SetAuthCookie(
                    impersonatedByUser.UserName,
                    isPersisted: false,
                    isImpersonated: false,
                    isTwoFactorAuthenticated: true );
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
                    // Is the impersonated person the same as the person who's already logged in?
                    // If so, don't ruin their existing session... just return true.
                    if ( CurrentUser != null && impersonatedPerson.Id == CurrentUser.PersonId )
                    {
                        return true;
                    }

                    Authorization.SignOut();

                    /*
                        10/19/2023 - JMH

                        Bypass the two-factor authentication requirement when impersonating;
                        otherwise, an administrator would be forced to provide username and password,
                        as well as complete a passwordless login.

                        Reason: Two-Factor Authentication
                     */
                    Rock.Security.Authorization.SetAuthCookie(
                        "rckipid=" + impersonatedPersonKeyParam,
                        isPersisted: false,
                        isImpersonated: true,
                        isTwoFactorAuthenticated: true );
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
            _onLoadStopwatch = Stopwatch.StartNew();

            base.OnLoad( e );

            // Attempt to restore the original interaction unique identifier.
            if ( IsPostBack && Guid.TryParse( _hfInteractionGuid.Value, out var originalInteractionGuid ) )
            {
                RequestContext.RelatedInteractionGuid = originalInteractionGuid;
            }

            Page.Header.DataBind();

            try
            {
                bool showDebugTimings = this.PageParameter( "ShowDebugTimings" ).AsBoolean();
                if ( showDebugTimings && _onLoadStopwatch.Elapsed.TotalMilliseconds > 500 )
                {
                    if ( _pageCache.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
                    {
                        Page.Form.Controls.Add( new Literal
                        {

                            Text = string.Format( "OnLoad [{0} ms]", _onLoadStopwatch.Elapsed.TotalMilliseconds )
                        } );
                    }
                }
            }
            catch
            {
                // ignore
            }
        }

        /// <inheritdoc/>
        protected override void OnUnload( EventArgs e )
        {
            // Dispose of all the service scopes that were created during this
            // page's lifecycle.
            foreach ( var scope in _pageServiceScopes )
            {
                scope.Dispose();
            }

            base.OnUnload( e );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Page.SaveStateComplete" /> event after the page state has been saved to the persistence medium.
        /// </summary>
        /// <param name="e">A <see cref="T:System.EventArgs" /> object containing the event data.</param>
        protected override void OnSaveStateComplete( EventArgs e )
        {
            base.OnSaveStateComplete( e );

            _tsDuration = RockDateTime.Now.Subtract( ( DateTime ) Context.Items["Request_Start_Time"] );

            ProcessPageInteraction();

            if ( phLoadStats != null )
            {
                var customPersister = this.PageStatePersister as RockHiddenFieldPageStatePersister;

                if ( customPersister != null )
                {
                    this.ViewStateSize = customPersister.ViewStateSize;
                    this.ViewStateSizeCompressed = customPersister.ViewStateSizeCompressed;
                    this.ViewStateIsCompressed = customPersister.ViewStateIsCompressed;
                }

                string showTimingsUrl = this.Request.UrlProxySafe().ToString();
                if ( showTimingsUrl.IndexOf( "ShowDebugTimings", StringComparison.OrdinalIgnoreCase ) < 0 )
                {
                    if ( showTimingsUrl.Contains( "?" ) )
                    {
                        showTimingsUrl += "&ShowDebugTimings=true";
                    }
                    else
                    {
                        showTimingsUrl += "?ShowDebugTimings=true";
                    }
                }

                phLoadStats.Controls.Add( new LiteralControl( $"<span class='cms-admin-footer-property'><a href='{showTimingsUrl}'> Page Load Time: {_tsDuration.TotalSeconds:N2}s </a></span><span class='margin-l-md js-view-state-stats cms-admin-footer-property'></span> <span class='margin-l-md js-html-size-stats cms-admin-footer-property'></span>" ) );

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

        /// <summary>
        /// Process page view interactions if they are enabled for this website.
        /// </summary>
        private void ProcessPageInteraction()
        {
            // Do not process page interactions for a postback, or if not enabled for this site.
            if ( Page.IsPostBack )
            {
                return;
            }

            if ( _pageCache == null
                 || !( _pageCache?.Layout?.Site?.EnablePageViews ?? false ) )
            {
                return;
            }

            // If we have identified a logged-in user, record the page interaction immediately and return. (Does not include anonymous visitors)
            if ( CurrentPerson != null )
            {
                var interactionInfo = new InteractionTransactionInfo
                {
                    InteractionGuid = RequestContext.RelatedInteractionGuid,
                    InteractionTimeToServe = _tsDuration.TotalSeconds,
                    InteractionChannelCustomIndexed1 = Request.UrlReferrerNormalize(),
                    InteractionChannelCustom2 = Request.UrlReferrerSearchTerms(),
                    InteractionChannelCustom1 = Activity.Current?.TraceId.ToString()
                };

                // If we have a UTM cookie, add the information to the interaction.
                var utmInfo = UtmHelper.GetUtmCookieDataFromRequest( this.Request );
                UtmHelper.AddUtmInfoToInteractionTransactionInfo( interactionInfo, utmInfo );

                var pageViewTransaction = new InteractionTransaction( DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_WEBSITE ),
                    this.Site,
                    _pageCache,
                    interactionInfo );

                pageViewTransaction.Enqueue();

                InteractionService.RegisterIntentInteractions( _pageCache.InteractionIntentValueIds );

                return;
            }

            // Add a script to register an interaction for this page after it has been loaded by the browser.
            // The intention of using a client callback here is to delay the creation of the Anonymous Visitor
            // database records used to track interactions for visitors until we know that the page has been executed
            // on a valid client with Javascript and cookies enabled.
            if ( ClientScript.IsStartupScriptRegistered( "rock-js-register-interaction" ) )
            {
                return;
            }

            var rockSessionGuid = Session["RockSessionId"]?.ToString().AsGuidOrNull() ?? Guid.Empty;

            // Construct the page interaction data object that will be returned to the client.
            // This object is serialized into the page script, so we must be sure to sanitize values
            // extracted from the request header to prevent cross-site scripting (XSS) issues.
            var pageInteraction = new PageInteractionInfo
            {
                Guid = RequestContext.RelatedInteractionGuid,
                ActionName = "View",
                BrowserSessionGuid = rockSessionGuid,
                PageId = this.PageId,
                PageRequestUrl = Request.UrlProxySafe().ToString(),
                PageRequestDateTime = RockDateTime.Now,
                PageRequestTimeToServe = _tsDuration.TotalSeconds,
                UrlReferrerHostAddress = Request.UrlReferrerNormalize(),
                UrlReferrerSearchTerms = Request.UrlReferrerSearchTerms(),
                UserAgent = Request.UserAgent.SanitizeHtml(),
                UserHostAddress = GetClientIpAddress().SanitizeHtml(),
                UserIdKey = CurrentPersonAlias?.IdKey
            };

            // This script adds a callback to record a View interaction for this page.
            // If the user is logged in, they are identified by the supplied UserIdKey representing their current PersonAlias.
            // If the user is a visitor, the ROCK_VISITOR_KEY cookie is read from the client browser to obtain the
            // UserIdKey supplied to them. For a first visit, the cookie is set in this response.
            // Additionally, this script now stores a list of interaction GUIDs in sessionStorage to prevent duplicate interactions.
            // Each time a new interaction is recorded, the GUID is checked against the stored list in sessionStorage.
            // If the GUID has already been recorded in the current session, the interaction will not be sent again, ensuring
            // that only unique interactions are tracked during the session. This additional change was needed to prevent the
            // scenario where a duplicate interaction would be sent whenever an individual used a browser's back arrow to navigate
            // back to a page that had already sent an interaction. 
            string script = @"
Sys.Application.add_load(function () {
    const getCookieValue = (name) => {
        const match = document.cookie.match('(^|;)\\s*' + name + '\\s*=\\s*([^;]+)');

        return !match ? '' : match.pop();
    };

    var interactionGuid = '<interactionGuid>';
    var interactionGuids = JSON.parse(sessionStorage.getItem('interactionGuids')) || [];

    if (!interactionGuids.includes(interactionGuid)) {
        interactionGuids.push(interactionGuid);
        sessionStorage.setItem('interactionGuids', JSON.stringify(interactionGuids));

        var interactionArgs = <jsonData>;
        if (!interactionArgs.<userIdProperty>) {
            interactionArgs.<userIdProperty> = getCookieValue('<rockVisitorCookieName>');
        }
        $.ajax({
            url: '/api/Interactions/RegisterPageInteraction',
            type: 'POST',
            data: interactionArgs
            });
    }
});
";

            script = script.Replace( "<rockVisitorCookieName>", Rock.Personalization.RequestCookieKey.ROCK_VISITOR_KEY );
            script = script.Replace( "<interactionGuid>", pageInteraction.Guid.ToString() );
            script = script.Replace( "<jsonData>", pageInteraction.ToJson() );
            script = script.Replace( "<userIdProperty>", nameof( pageInteraction.UserIdKey ) );

            ClientScript.RegisterStartupScript( this.Page.GetType(), "rock-js-register-interaction", script, true );
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the debug timing view model.
        /// </summary>
        /// <param name="eventTitle">The event title.</param>
        /// <param name="stepDuration">Duration of the step.</param>
        /// <param name="indentLevel">The indent level.</param>
        /// <param name="boldTitle">if set to <c>true</c> [bold title].</param>
        /// <param name="subtitle">The subtitle.</param>
        /// <returns></returns>
        private DebugTimingViewModel GetDebugTimingOutput( string eventTitle, double stepDuration, int indentLevel = 0, bool boldTitle = false, string subtitle = "" )
        {
            _tsDuration = RockDateTime.Now.Subtract( ( DateTime ) Context.Items["Request_Start_Time"] );
            _duration = Math.Round( stepDuration, 2 );

            var viewModel = new DebugTimingViewModel
            {
                TimestampMs = _previousTiming,
                DurationMs = _duration,
                Title = eventTitle,
                SubTitle = subtitle,
                IsTitleBold = boldTitle,
                IndentLevel = indentLevel
            };

            _previousTiming += _duration;

            return viewModel;
        }

        /// <summary>
        /// Creates the service provider that will provides services for all
        /// requests during the lifetime of this application.
        /// </summary>
        /// <returns>A new service provider.</returns>
        private static IServiceProvider CreateServiceProvider()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IRockRequestContextAccessor, RockRequestContextAccessor>();
            serviceCollection.AddScoped<RockContext>();
            serviceCollection.AddSingleton<IWebHostEnvironment>( provider => new Utility.WebHostEnvironment
            {
                WebRootPath = AppDomain.CurrentDomain.BaseDirectory
            } );

            return serviceCollection.BuildServiceProvider();
        }

        /// <summary>
        /// Creates the service scope and initializes any required values.
        /// </summary>
        /// <returns>An new service scope.</returns>
        private IServiceScope CreateServiceScope()
        {
            var scope = _lazyServiceProvider.Value.CreateScope();

            _pageServiceScopes.Add( scope );

            return scope;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Reports the debug timing.
        /// </summary>
        /// <param name="eventTitle">The event title.</param>
        /// <param name="subtitle">The subtitle.</param>
        /// <returns></returns>
        internal void ReportOnLoadDebugTiming( string eventTitle, string subtitle = "" )
        {
            if ( !_showDebugTimings || _onLoadStopwatch == null )
            {
                return;
            }

            _onLoadStopwatch.Stop();

            if ( !subtitle.IsNullOrWhiteSpace() && !subtitle.StartsWith( ")" ) )
            {
                subtitle = $"({subtitle})";
            }

            var duration = _onLoadStopwatch.Elapsed.TotalMilliseconds;
            _debugTimingViewModels.Add( GetDebugTimingOutput( eventTitle, duration, 1, false, subtitle ) );
            _onLoadStopwatch.Restart();
        }

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
                var masterPage = ( RockMasterPage ) this.Master;
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
        /// Adds a new Html literal link that will be added to the page header prior to the page being rendered.
        /// </summary>
        /// <param name="htmlLink">The <see cref="System.Web.UI.WebControls.Literal"/>.</param>
        private void AddHtmlLink( Literal htmlLink )
        {
            RockPage.AddHtmlLink( this, htmlLink );
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
                // If the script has already been loaded then don't do it again
                if ( Application["GoogleAnalyticsScript"] is string scriptTemplate )
                {
                    return;
                }

                // Parse the list of codes, we want the "G-" codes to be first because the first code is used as the default in the <script> src property.
                var gtagCodes = code.Split( ',' ).Select( a => a.Trim() ).Where( a => a.StartsWith( "G-", StringComparison.OrdinalIgnoreCase ) ).ToList() ?? new List<string>();

                // Add the measurement codes that start with 'UA' to the gtag script. If there are multiple measurement IDs the first one is used as the default.
                gtagCodes.AddRange( code.Split( ',' ).Select( a => a.Trim() ).Where( a => a.StartsWith( "UA-", StringComparison.OrdinalIgnoreCase ) ).ToList() ?? new List<string>() );

                if ( gtagCodes.Any() )
                {
                    var sb = new StringBuilder();
                    sb.Append( $@"
    <!-- BEGIN Global site tag (gtag.js) - Google Analytics -->
    <script async src=""https://www.googletagmanager.com/gtag/js?id={gtagCodes.First()}""></script>
    <script>
      window.dataLayer = window.dataLayer || [];
      function gtag(){{window.dataLayer.push(arguments);}}
      gtag('js', new Date());" );
                    sb.AppendLine( "" );
                    gtagCodes.ForEach( a => sb.AppendLine( $"      gtag('config', '{a}');" ) );
                    sb.AppendLine( "    </script>" );
                    sb.AppendLine( "    <!-- END Global site tag (gtag.js) - Google Analytics -->" );

                    AddScriptToHead( this.Page, sb.ToString(), false );
                }
            }
            catch ( Exception ex )
            {
                // Log any error but still let the page load.
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
            Literal favIcon = new Literal();
            favIcon.Mode = LiteralMode.PassThrough;
            var baseUrl = FileUrlHelper.GetImageUrl( binaryFileId );
            var url = ResolveRockUrl( $"{baseUrl}&width={size}&height={size}&mode=crop&format=png" );
            favIcon.Text = $"<link rel=\"{rel}\" sizes=\"{size}x{size}\" href=\"{url}\" />";
             
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
            foreach ( IIdleRedirectBlock idleRedirectBlock in this.RockBlocks.Where( a => a is IIdleRedirectBlock ) )
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
            if ( Context.Request != null && Context.Request.UrlProxySafe() != null )
            {
                /*
                     4/30/2021 - NA

                     Due to the interaction between Rock 2-3 Step Payment Gateways and a possible CDN,
                     the URL that is returned needs to be the proxy safe one, not the one that the
                     CDN uses (such as Origin)

                     Reason: CDN and Payment Gateways
                */

                string protocol = WebRequestHelper.IsSecureConnection( Context ) ? "https" : Context.Request.UrlProxySafe().Scheme;
                return string.Format( "{0}://{1}{2}", protocol, Context.Request.UrlProxySafe().Authority, virtualPath );
            }

            return GlobalAttributesCache.Get().GetValue( "PublicApplicationRoot" ) + virtualPath.RemoveLeadingForwardslash();
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
        /// Gets the login URL with return URL.
        /// </summary>
        /// <returns></returns>
        public string GetLoginUrlWithReturnUrl()
        {
            return Site.GetLoginUrlWithReturnUrl();
        }

        /// <summary>
        /// Gets the context entities.
        /// </summary>
        /// <returns></returns>
        internal Dictionary<string, IEntity> GetContextEntities()
        {
            var contextEntities = new Dictionary<string, IEntity>();

            foreach ( var contextEntityType in GetContextEntityTypes() )
            {
                var contextEntity = GetCurrentContext( contextEntityType );

                if ( contextEntity != null && LavaHelper.IsLavaDataObject( contextEntity ) )
                {
                    var type = Type.GetType( contextEntityType.AssemblyName ?? contextEntityType.Name );

                    if ( type != null )
                    {
                        contextEntities.Add( type.Name, contextEntity );
                    }
                }
            }

            return contextEntities;
        }

        /// <summary>
        /// Gets the context entities for the specified scope.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <returns></returns>
        public Dictionary<string, IEntity> GetScopedContextEntities( ContextEntityScope scope )
        {
            var contextEntities = new Dictionary<string, IEntity>();

            var keyEntityDictionary = BuildPageContextData( scope );
            foreach ( var contextEntityTypeKey in keyEntityDictionary.Keys )
            {
                var entityType = EntityTypeCache.Get( contextEntityTypeKey );
                if ( entityType == null )
                {
                    continue;
                }

                var contextEntity = GetCurrentContext( entityType, keyEntityDictionary );
                if ( contextEntity == null )
                {
                    continue;
                }

                if ( !LavaHelper.IsLavaDataObject( contextEntity ) )
                {
                    continue;
                }

                var type = Type.GetType( entityType.AssemblyName ?? entityType.Name );
                if ( type == null )
                {
                    continue;
                }

                contextEntities.Add( type.Name, contextEntity );
            }

            return contextEntities;
        }

        /// <summary>
        /// Gets the context entity types for the specified scope.  This is useful for determining what context entity
        /// types a page is dealing with without causing extra database hits.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <returns></returns>
        public Dictionary<string, EntityTypeCache> GetScopedContextEntityTypes( ContextEntityScope scope )
        {
            var contextEntityTypes = new Dictionary<string, EntityTypeCache>();
            var keyEntityDictionary = BuildPageContextData( scope );
            foreach ( var contextEntityTypeKey in keyEntityDictionary.Keys )
            {
                var entityType = EntityTypeCache.Get( contextEntityTypeKey );
                if ( entityType == null )
                {
                    continue;
                }

                var type = Type.GetType( entityType.AssemblyName ?? entityType.Name );
                if ( type == null )
                {
                    continue;
                }

                contextEntityTypes.Add( type.Name, entityType );
            }

            return contextEntityTypes;
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
            return GetCurrentContext( entity, this.ModelContext );
        }

        /// <summary>
        /// Gets the current context object for a given entity type.
        /// </summary>
        /// <param name="entity">The <see cref="EntityTypeCache"/> containing a reference to the entity.</param>
        /// <param name="keyEntityDictionary">The <see cref="KeyEntity"/> dictionary containing a reference to the context object (typically the ModelContext property, unless attempting to access a context entity within a specific scope).</param>
        /// <returns>An object that implements the <see cref="Rock.Data.IEntity"/> interface referencing the context object. </returns>
        internal Rock.Data.IEntity GetCurrentContext( EntityTypeCache entity, Dictionary<string, KeyEntity> keyEntityDictionary )
        {
            if ( entity == null || keyEntityDictionary == null )
            {
                return null;
            }

            if ( keyEntityDictionary.ContainsKey( entity.Name ) )
            {
                var keyModel = keyEntityDictionary[entity.Name];

                if ( keyModel.Entity == null )
                {
                    if ( entity.Name.Equals( "Rock.Model.Person", StringComparison.OrdinalIgnoreCase ) )
                    {
                        if ( keyModel.Id.HasValue || keyModel.Guid.HasValue )
                        {
                            var qry = new PersonService( new RockContext() )
                                .Queryable( true, true )
                                .Include( p => p.MaritalStatusValue )
                                .Include( p => p.ConnectionStatusValue )
                                .Include( p => p.RecordStatusValue )
                                .Include( p => p.RecordStatusReasonValue )
                                .Include( p => p.RecordTypeValue )
                                .Include( p => p.SuffixValue )
                                .Include( p => p.TitleValue )
                                .Include( p => p.GivingGroup )
                                .Include( p => p.Photo )
                                .Include( p => p.Aliases );

                            if ( keyModel.Id.HasValue )
                            {
                                qry = qry.Where( p => p.Id == keyModel.Id.Value );
                            }
                            else
                            {
                                qry = qry.Where( p => p.Guid == keyModel.Guid.Value );
                            }

                            keyModel.Entity = qry.FirstOrDefault();
                        }
                        else if ( keyModel.Key.IsNotNullOrWhiteSpace() )
                        {
                            keyModel.Entity = new PersonService( new RockContext() ).GetByPublicKey( keyModel.Key );
                        }
                    }
                    else
                    {

                        Type modelType = entity.GetEntityType();

                        if ( modelType == null && entity.AssemblyName.IsNotNullOrWhiteSpace() )
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

                            if ( keyModel.Id.HasValue )
                            {
                                MethodInfo getMethod = serviceInstance.GetType().GetMethod( "Get", new Type[] { typeof( int ) } );
                                keyModel.Entity = getMethod.Invoke( serviceInstance, new object[] { keyModel.Id } ) as Rock.Data.IEntity;
                            }
                            else if ( keyModel.Guid.HasValue )
                            {
                                MethodInfo getMethod = serviceInstance.GetType().GetMethod( "Get", new Type[] { typeof( Guid ) } );
                                keyModel.Entity = getMethod.Invoke( serviceInstance, new object[] { keyModel.Guid } ) as Rock.Data.IEntity;
                            }
                            else if ( keyModel.Key.IsNotNullOrWhiteSpace() )
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
        /// Sets the provided entity within the context cookie.
        /// </summary>
        /// <param name="entity">The entity to set within the cookie.</param>
        /// <param name="pageSpecific">Whether to set the entity within a page-specific cookie.</param>
        /// <param name="refreshPage">Whether to refresh the page after adding/updating the cookie.</param>
        public void SetContextCookie( IEntity entity, bool pageSpecific = false, bool refreshPage = true )
        {
            if ( entity == null )
            {
                return;
            }

            var entityType = entity.GetType();
            if ( entityType.IsDynamicProxyType() )
            {
                entityType = entityType.BaseType;
            }

            if ( entity.Guid == Guid.Empty )
            {
                // Clear this entity type from the context cookie instead.
                ClearContextCookie( entityType, pageSpecific, refreshPage );
                return;
            }

            try
            {
                var cookieName = GetContextCookieName( pageSpecific );
                var contextCookie = FindCookie( cookieName ) ?? new HttpCookie( cookieName );
                var contextItems = contextCookie.Value.FromJsonOrNull<Dictionary<string, string>>() ?? new Dictionary<string, string>();

                /*
                    12/1/2023 - JPH

                    Here's how this used to work:
                        1. `entity.ContextKey` returns an encrypted, encoded string;
                        2. We used to immediately decode the string before putting it into the cookie; not sure why;
                        3. The `System.Web` library's handling of cookies successfully wrote AND retrieved this
                           unencoded string; they gave it back to us exactly as we gave it to them, likely
                           auto-encoding and auto-decoding for us behind the scenes.

                    WHAT THE CODE USED TO BE:
                    contextItems.AddOrReplace( entityType.FullName, HttpUtility.UrlDecode( entity.ContextKey ) );

                    But when attempting to retrieve these `System.Web`-written cookies using our new, Obsidian request flow:
                        1. All of the "+" characters were replaced with " " characters, breaking our decryption attempt.
                        2. This is because the newer, `System.Net` library's retrieving of cookies seemingly double-decodes
                           the string values, leading to the plus sign replacement behavior we're seeing.
                           https://stackoverflow.com/a/55077150

                    WHAT THE CODE IS NOW:
                    contextItems.AddOrReplace( entityType.FullName, entity.ContextKey );

                    The "fix" is to leave the string encoded on the way into the cookie, which fixes the `System.Net` lib's
                    retrieval of the cookie. But this now means we need to manually decode the cookie on this (`System.Web`)
                    side, within the `AddCookieContextEntities()` method.

                    Reason: Context cookie compatibility between Web Forms and Obsidian.
                    https://github.com/SparkDevNetwork/Rock/issues/5634
                 */
                contextItems.AddOrReplace( entityType.FullName, entity.ContextKey );

                contextCookie.Value = contextItems.ToJson();
                contextCookie.Expires = RockDateTime.Now.AddYears( 1 );

                AddOrUpdateCookie( contextCookie );

                if ( refreshPage )
                {
                    Response.Redirect( Request.RawUrl, false );
                    Context.ApplicationInstance.CompleteRequest();
                }
            }
            catch
            {
                // Intentionally ignore exception in case JSON [de]serialization fails.
            }
        }

        /// <summary>
        /// Clears the specified entity type from the context cookie and deletes the cookie itself if
        /// no more entity types remain within its value.
        /// </summary>
        /// <param name="entityType">Type of the entity to clear from the cookie.</param>
        /// <param name="pageSpecific">Whether to clear the entity type from a page-specific cookie.</param>
        /// <param name="refreshPage">Whether to refresh the page after clearing the entity type from the cookie.</param>
        public void ClearContextCookie( Type entityType, bool pageSpecific = false, bool refreshPage = true )
        {
            if ( entityType == null )
            {
                return;
            }

            try
            {
                var cookieName = GetContextCookieName( pageSpecific );
                var contextCookie = FindCookie( cookieName ) ?? new HttpCookie( cookieName );
                var contextItems = contextCookie.Value.FromJsonOrNull<Dictionary<string, string>>();

                if ( entityType.IsDynamicProxyType() )
                {
                    entityType = entityType.BaseType;
                }

                contextItems?.Remove( entityType.FullName );

                if ( contextItems?.Any() == true )
                {
                    // Re-serialize the value and bump the expiration date out.
                    contextCookie.Value = contextItems.ToJson();
                    contextCookie.Expires = RockDateTime.Now.AddYears( 1 );
                }
                else
                {
                    // No more entity types remain; delete the cookie.
                    contextCookie.Value = null;
                    contextCookie.Expires = RockDateTime.Now.AddDays( -1 );
                }

                AddOrUpdateCookie( contextCookie );

                if ( refreshPage )
                {
                    Response.Redirect( Request.RawUrl, false );
                    Context.ApplicationInstance.CompleteRequest();
                }
            }
            catch
            {
                // Intentionally ignore exception in case JSON [de]serialization fails.
            }
        }

        /// <summary>
        /// Finds a cookie by name in the request or response collections.
        /// </summary>
        /// <param name="cookieName">The cookie name.</param>
        /// <returns></returns>
        private HttpCookie FindCookie( string cookieName )
        {
            HttpCookie cookie = null;

            if ( Response.Cookies.AllKeys.Contains( cookieName ) )
            {
                cookie = Response.Cookies[cookieName];
            }
            else if ( Request.Cookies.AllKeys.Contains( cookieName ) )
            {
                cookie = Request.Cookies[cookieName];
            }

            return cookie;
        }

        /// <summary>
        /// Converts the legacy, "structured" context cookies to a simpler, JSON format.
        /// </summary>
        [Obsolete( "Remove this method after a few major versions, hopefully allowing enough time to convert all legacy context cookies." )]
        [RockObsolete( "1.17" )]
        private void ConvertLegacyContextCookiesToJSON()
        {
            // Find any cookies whose names start with the legacy cookie name prefix.
            var legacyCookies = new List<HttpCookie>();
            foreach ( var cookieName in Request.Cookies.AllKeys )
            {
                if ( !cookieName.StartsWith( "Rock_Context" ) )
                {
                    continue;
                }

                legacyCookies.Add( Request.Cookies[cookieName] );
            }

            foreach ( var legacyCookie in legacyCookies )
            {
                try
                {
                    // Add each of the "structured" values to a simple dictionary.
                    var contextItems = new Dictionary<string, string>();

                    for ( var i = 0; i < legacyCookie.Values.Count; i++ )
                    {
                        var cookieValue = legacyCookie.Values[i];
                        if ( cookieValue.IsNullOrWhiteSpace() )
                        {
                            continue;
                        }

                        // We need to decrypt the value so we can use the entity type name for the key.
                        var contextItem = Rock.Security.Encryption.DecryptString( cookieValue );
                        var valueParts = contextItem.Split( '|' );
                        if ( valueParts.Length != 2 )
                        {
                            continue;
                        }

                        // Re-add the entire, encoded value, as the object loading process depends on this specific format.
                        var encodedCookieValue = HttpUtility.UrlEncode( cookieValue );
                        contextItems.Add( valueParts[0], encodedCookieValue );
                    }

                    // Add the new, JSON-based cookie.
                    if ( contextItems.Any() )
                    {
                        // We're changing the names of the cookies:
                        //  1. Renaming the site cookie from the old name (Rock_Context) will make it easier
                        //     to know when we've already converted to the new, JSON cookie on a given client,
                        //     without having to dig into the cookie's value, thereby making subsequent
                        //     request/response cycles faster.
                        //  2. Legacy, page-specific cookie names followed this format: "Rock_Context:n",
                        //     where n is the page ID. Since colons may not be used in cookie names, we'll
                        //     convert these names to a valid format.
                        var legacyCookieNameParts = legacyCookie.Name.Split( ':' );
                        int? pageId = null;

                        if ( legacyCookieNameParts.Length == 2 )
                        {
                            pageId = legacyCookieNameParts[1].AsIntegerOrNull();
                            if ( pageId.GetValueOrDefault() <= 0 )
                            {
                                // There was something wrong with this cookie name; skip it.
                                continue;
                            }
                        }

                        var newCookieName = pageId.HasValue
                            ? $"{RockRequestContext.PageContextCookieNamePrefix}{pageId.Value}"
                            : RockRequestContext.SiteContextCookieName;

                        var newCookie = new HttpCookie( newCookieName, contextItems.ToJson() )
                        {
                            Expires = legacyCookie.Expires // Leave the expiration date/time as it was.
                        };

                        AddOrUpdateCookie( newCookie );
                    }
                }
                catch
                {
                    // Intentionally ignore exception in case conversion fails.
                }

                // Always remove the legacy, "structured" cookie, regardless of conversion success.
                legacyCookie.Values.Clear();
                legacyCookie.Expires = RockDateTime.Now.AddDays( -1 );
                AddOrUpdateCookie( legacyCookie );
            }
        }

        /// <summary>
        /// Adds context entities from a cookie to a provided <see cref="KeyEntity"/> dictionary.
        /// </summary>
        /// <param name="cookie">The context cookie that contains the encrypted context entities.</param>
        /// <param name="keyEntityDictionary">The dictionary into which to place the decrypted context entities.</param>
        /// <returns>The dictionary holding the decrypted context entities.</returns>
        private Dictionary<string, Data.KeyEntity> AddCookieContextEntities( HttpCookie cookie, Dictionary<string, Data.KeyEntity> keyEntityDictionary )
        {
            if ( cookie == null )
            {
                return keyEntityDictionary; // nothing to do.
            }

            try
            {
                var contextItems = cookie.Value.FromJsonOrNull<Dictionary<string, string>>();
                if ( contextItems?.Any( c => c.Value.IsNotNullOrWhiteSpace() ) != true )
                {
                    // Delete the cookie since it holds no context items. Should never happen.
                    cookie.Value = null;
                    cookie.Expires = RockDateTime.Now.AddHours( -1 );

                    return keyEntityDictionary;
                }

                foreach ( var encryptedItem in contextItems.Values )
                {
                    if ( encryptedItem.IsNullOrWhiteSpace() )
                    {
                        continue;
                    }

                    var decodedItem = HttpUtility.UrlDecode( encryptedItem );
                    var decryptedItem = Rock.Security.Encryption.DecryptString( decodedItem );
                    var itemParts = decryptedItem.Split( '|' );
                    if ( itemParts.Length != 2 )
                    {
                        continue;
                    }

                    keyEntityDictionary.AddOrReplace( itemParts[0], new Data.KeyEntity( itemParts[1] ) );
                }
            }
            catch
            {
                // Intentionally ignore exception in case any part of this process fails.
            }

            return keyEntityDictionary;
        }

        private void HandleRockWiFiCookie( int? personAliasId )
        {
            if ( personAliasId == null )
            {
                return;
            }

            if ( Request.Cookies["rock_wifi"] != null )
            {
                HttpCookie httpCookie = Request.Cookies["rock_wifi"];
                if ( LinkPersonAliasToDevice( ( int ) personAliasId, httpCookie.Values["ROCK_PERSONALDEVICE_ADDRESS"] ) )
                {
                    var wiFiCookie = Response.Cookies["rock_wifi"];
                    wiFiCookie.Expires = RockDateTime.SystemDateTime.AddDays( -1 );
                    AddOrUpdateCookie( wiFiCookie );
                }
            }
        }

        /// <summary>
        /// Gets the name of the context cookie.
        /// </summary>
        /// <param name="pageSpecific">Whether to get the name for a page-specific context cookie.</param>
        /// <returns>The name of the context cookie or <c>null</c> if <paramref name="pageSpecific"/> == <c>true</c>
        /// and this request has not yet been prepared for a given page.</returns>
        public string GetContextCookieName( bool pageSpecific )
        {
            return RequestContext?.GetContextCookieName( pageSpecific );
        }

        /// <summary>
        /// Creates/Overwrites the specified cookie using the global default for the SameSite setting.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <param name="expirationDate">The expiration date.</param>
        public static void AddOrUpdateCookie( string name, string value, DateTime? expirationDate )
        {
            WebRequestHelper.AddOrUpdateCookie( HttpContext.Current, name, value, expirationDate );
        }

        /// <summary>
        /// Creates/Overwrites the specified cookie using the global default for the SameSite setting.
        /// This method creates a new cookie using a deep clone of the provided cookie to ensure a cookie written to the response
        /// does not contain properties that are not compatible with .Net 4.5.2 (e.g. SameSite).
        /// Removes the cookie from the Request and Response using the cookie name, then adds the cloned clean cookie to the Response.
        /// </summary>
        /// <param name="cookie">The cookie.</param>
        public static void AddOrUpdateCookie( HttpCookie cookie )
        {
            WebRequestHelper.AddOrUpdateCookie( HttpContext.Current, cookie );
        }

        /// <summary>
        /// Gets the specified cookie. If the cookie is not found in the Request then it checks the Response, otherwise it will return null.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public HttpCookie GetCookie( string name )
        {
            return WebRequestHelper.GetCookieFromContext( this.Context, name );
        }

        /// <summary>
        /// Gets the cookie value from request.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>System.String.</returns>
        private string GetCookieValueFromRequest( string name )
        {
            if ( Request.Cookies.AllKeys.Contains( name ) )
            {
                return Request.Cookies[name]?.Value;
            }

            return null;
        }

        /// <summary>
        /// Gets the cookie value from response.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>System.String.</returns>
        private string GetCookieValueFromResponse( string name )
        {
            if ( Response.Cookies.AllKeys.Contains( name ) )
            {
                return Request.Cookies[name]?.Value;
            }

            return null;
        }

        /// <summary>
        /// Gets the cookie value.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="preferResponseCookie">The prefer response cookie.</param>
        /// <returns>string.</returns>
        private string GetCookieValue( string name, bool preferResponseCookie )
        {
            string requestValue = GetCookieValueFromRequest( name );
            string responseValue = GetCookieValueFromResponse( name );

            if ( preferResponseCookie )
            {
                return responseValue ?? requestValue;
            }
            else
            {
                return requestValue ?? responseValue;
            }
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
        public bool LinkPersonAliasToDevice( int personAliasId, string macAddress )
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
                delegate ( Control content )
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
                zoneWrapper.Attributes.Add( "class", ( "zone-instance" + ( canConfigPage ? " can-configure " : " " ) + control.CssClass ).Trim() );

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
            rblLocation.Items.Add( new ListItem( string.Format( "Page ({0})", _pageCache.InternalName ), "Page" ) );
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

        #region Page Parameters

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
                    return ( string ) Page.RouteData.Values[name];
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
                return ( string ) pageReference.Parameters[name];
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
        /// Gets the page route and query string parameters.
        /// </summary>
        /// <returns>A case-insensitive <see cref="System.Collections.Generic.Dictionary{String, Object}"/> containing the page route and query string values, where the Key is the parameter name and the object is the value.</returns>
        public Dictionary<string, object> PageParameters()
        {
            var parameters = new Dictionary<string, object>( StringComparer.OrdinalIgnoreCase );

            foreach ( var key in Page.RouteData.Values.Keys )
            {
                parameters.Add( key, Page.RouteData.Values[key] );
            }

            foreach ( string param in Request.QueryString.Keys )
            {
                if ( param != null )
                {
                    /*
                        2021-01-07 ETD
                        It is possible to get a route included in the list of QueryString.Keys when using a Page Route and the PageParameterFilter block.
                        When this occurs then the Dictionary.Add() will get a duplicate key exception. Since this is a route we should keep it as such
                        and ignore the value stored in the QueryString list (the value is the same). In any case if there is contention between a
                        Route Key and QueryString Key the Route will take precedence.
                    */
                    parameters.TryAdd( param, Request.QueryString[param] );
                }
            }

            return parameters;
        }

        /// <summary>
        /// Gets the page route and query string parameters.
        /// </summary>
        /// <returns>A case-insensitive <see cref="System.Collections.Generic.Dictionary{String, Object}"/> containing the page route and query string values, where the Key is the parameter name and the object is the value.</returns>
        public Dictionary<string, object> QueryParameters()
        {
            var parameters = new Dictionary<string, object>( StringComparer.OrdinalIgnoreCase );

            foreach ( string param in Request.QueryString.Keys )
            {
                if ( param != null )
                {
                    parameters.Add( param, Request.QueryString[param] );
                }
            }

            return parameters;
        }

        #endregion

        #region Static Helper Methods

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
                /*
                     6/26/2021 - SK

                     The AddMetaTagToHead in the lava filter removes some of the existing Meta tag
                     from the Head section at the later stage in page cycle. So at the time of
                     postback The control tree into which viewstate is being loaded doesn't match
                     the control tree that was used to save viewstate during the previous request.

                     So instead of removing it and adding some of the existing meta tag again at the
                     end, if we replace it with the new value at the same position, it will help
                     maintain the viewstate.

                     Reason: To fix issue #4560 (a viewstate error on any postback)
                */
                var isExisting = ReplaceHtmlMetaIfExists( page, htmlMeta );

                if ( isExisting )
                {
                    return;
                }

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

        /// <summary>
        /// Replaces an existing HtmlMeta control if all attributes match except for Content.
        /// Returns <c>true</c> if the meta tag already exists and was removed.
        /// </summary>
        /// <param name="page">The <see cref="System.Web.UI.Page"/>.</param>
        /// <param name="newMeta">The <see cref="System.Web.UI.HtmlControls.HtmlMeta"/> tag to check for.</param>
        /// <returns>A <see cref="System.Boolean"/> that is <c>true</c> if the meta tag already exists; otherwise <c>false</c>.</returns>
        private static bool ReplaceHtmlMetaIfExists( Page page, HtmlMeta newMeta )
        {
            bool existsAlready = false;

            if ( page != null && page.Header != null )
            {
                var index = 0;
                foreach ( Control control in page.Header.Controls )
                {
                    if ( control is HtmlMeta )
                    {
                        HtmlMeta existingMeta = ( HtmlMeta ) control;

                        bool sameAttributes = true;
                        bool hasContentAttribute_ExistingMeta = ( existingMeta.Attributes["Content"] != null );

                        foreach ( string attributeKey in newMeta.Attributes.Keys )
                        {
                            if ( attributeKey.ToLower() != "content" ) // ignore content attribute.
                            {
                                if ( existingMeta.Attributes[attributeKey] == null ||
                                    existingMeta.Attributes[attributeKey].ToLower() != newMeta.Attributes[attributeKey].ToLower() )
                                {
                                    sameAttributes = false;
                                    break;
                                }
                            }
                        }

                        if ( sameAttributes )
                        {
                            index = page.Header.Controls.IndexOf( control );
                            page.Header.Controls.Remove( control );
                            existsAlready = true;
                            break;
                        }
                    }
                }

                if ( existsAlready )
                {
                    page.Header.Controls.AddAt( index, newMeta );
                }
            }

            return existsAlready;
        }

        /// <summary>
        /// Adds a new Html link Literal that will be added to the page header prior to the page being rendered.
        /// NOTE: This method differs from the other AddHtmlLink because a literal whose Mode
        /// is set to PassThrough will not have its parameters/attributes encoded (the ampersande char changed to &amp;).
        /// </summary>
        /// <param name="page">The <see cref="System.Web.UI.Page"/>.</param>
        /// <param name="htmlLink">The <see cref="System.Web.UI.WebControls.Literal"/> to add to the page.</param>
        /// <param name="contentPlaceHolderId">A <see cref="System.String"/> representing the Id of the content placeholder to add the link to.</param>
        private static void AddHtmlLink( Page page, Literal htmlLink, string contentPlaceHolderId = "" )
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
                                var ph = ( ContentPlaceHolder ) header.Controls[i];
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
        /// Adds a new Html link that will be added to the page header prior to the page being rendered.
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
                                var ph = ( ContentPlaceHolder ) header.Controls[i];
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
        /// Returns a <see cref="System.Boolean"/> flag indicating if a specified parent control contains the specified HtmlLink literal.
        /// </summary>
        /// <param name="parentControl">The <see cref="System.Web.UI.Control"/> to search for the HtmlLink.</param>
        /// <param name="newLink">The <see cref="System.Web.UI.WebControls.Literal"/> to search for.</param>
        /// <returns>A <see cref="System.Boolean"/> value that is <c>true</c> if the HtmlLink exists in the parent control; otherwise <c>false</c>.</returns>
        private static bool HtmlLinkExists( Control parentControl, Literal newLink )
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
                    else if ( control is Literal )
                    {
                        Literal existingLink = ( Literal ) control;

                        if ( newLink.Text == existingLink.Text )
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
        /// Returns a <see cref="System.Boolean"/> flag indicating if a specified parent control contains the specified HtmlLink.
        /// </summary>
        /// <param name="parentControl">The <see cref="System.Web.UI.Control"/> to search for the HtmlLink.</param>
        /// <param name="newLink">The <see cref="System.Web.UI.HtmlControls.HtmlLink"/> to search for.</param>
        /// <returns>A <see cref="System.Boolean"/> value that is <c>true</c> if the HtmlLink exists in the parent control; otherwise <c>false</c>.</returns>
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
                        HtmlLink existingLink = ( HtmlLink ) control;

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
            AddScriptSrcToHead( page, scriptId, src, null );
        }

        /// <summary>
        /// Adds a script tag with the specified id, source, and attributes to head (if it doesn't already exist)
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="scriptId">The script identifier.</param>
        /// <param name="src">The source.</param>
        /// <param name="additionalAttributes">The additional attributes.</param>
        public static void AddScriptSrcToHead( Page page, string scriptId, string src, Dictionary<string, string> additionalAttributes )
        {
            if ( page != null && page.Header != null )
            {
                var header = page.Header;

                if ( !header.Controls.OfType<Literal>().Any( a => a.ID == scriptId ) )
                {
                    Literal l = new Literal
                    {
                        ID = scriptId
                    };

                    StringBuilder sbScriptTagHTML = new StringBuilder();
                    sbScriptTagHTML.Append( $"<script id='{scriptId}' src='{src}'" );
                    string additionalAttributesHtml = additionalAttributes?
                        .Select( a =>
                            a.Value == null
                                ? $"{a.Key}"
                                : $"{a.Key}='{a.Value}'"
                        )
                        .ToList()
                        .AsDelimited( " " );

                    if ( additionalAttributesHtml.IsNotNullOrWhiteSpace() )
                    {
                        sbScriptTagHTML.Append( $" {additionalAttributesHtml}" );
                    }

                    sbScriptTagHTML.Append( "></script>" );
                    l.Text = sbScriptTagHTML.ToString();
                    header.Controls.Add( l );
                }
            }
        }

        /// <summary>
        /// Adds a style tag with the specified styleTagId and css
        /// </summary>
        /// <param name="styleTagId">The script identifier.</param>
        /// <param name="src">The source.</param>
        public void AddStyleToHead( string styleTagId, string src )
        {
            RockPage.AddStyleToHead( this.Page, styleTagId, src );
        }

        /// <summary>
        /// Adds a style tag with the specified styleTagId and css
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="styleTagId">The style tag identifier.</param>
        /// <param name="css">The CSS.</param>
        public static void AddStyleToHead( Page page, string styleTagId, string css )
        {
            AddStyleToHead( page, styleTagId, css, null );
        }

        /// <summary>
        /// Adds a style tag with the specified styleTagId, css, and attributes to head (if it doesn't already exist)
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="styleTagId">The style tag identifier.</param>
        /// <param name="css">The CSS.</param>
        /// <param name="additionalAttributes">The additional attributes.</param>
        public static void AddStyleToHead( Page page, string styleTagId, string css, Dictionary<string, string> additionalAttributes )
        {
            if ( page != null && page.Header != null )
            {
                var header = page.Header;
                if ( !header.Controls.OfType<Literal>().Any( a => a.ID == styleTagId ) )
                {
                    Literal l = new Literal
                    {
                        ID = styleTagId
                    };
                    StringBuilder sbStyleTagHTML = new StringBuilder();
                    sbStyleTagHTML.Append( $"<style id='{styleTagId}'" );
                    string additionalAttributesHtml = additionalAttributes?.Select( a => $"{a.Key}='{a.Value}'" ).ToList().AsDelimited( " " );
                    if ( additionalAttributesHtml.IsNotNullOrWhiteSpace() )
                    {
                        sbStyleTagHTML.Append( $" {additionalAttributesHtml}" );
                    }
                    sbStyleTagHTML.Append( $">\n{css}\n</style>" );
                    l.Text = sbStyleTagHTML.ToString();
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
            return WebRequestHelper.GetClientIpAddress( new HttpRequestWrapper( HttpContext.Current?.Request ) );
        }

        #endregion

        #region Obsidian Fingerprinting

        /// <summary>
        /// Initializes the obsidian file fingerprint. This sets the initial
        /// fingerprint value and then if we are in Debug mode it monitors for
        /// any file system changes related to Obsidian and updates the
        /// fingerprint used when loading files to bust cache.
        /// </summary>
        private static void InitializeObsidianFingerprint()
        {
            // Do everything in a try/catch because this is called from the
            // static initializer, meaning if something goes wrong Rock will
            // fail to start.
            try
            {
                var obsidianPath = System.Web.Hosting.HostingEnvironment.MapPath( "~/Obsidian" );
                var pluginsPath = System.Web.Hosting.HostingEnvironment.MapPath( "~/Plugins" );
                var now = RockDateTime.Now;

                // Find the last date any obsidian file was modified.
                var lastWriteTime = Directory.EnumerateFiles( obsidianPath, "*.js", SearchOption.AllDirectories )
                    .Union( Directory.EnumerateFiles( pluginsPath, "*.js", SearchOption.AllDirectories ) )
                    .Select( f =>
                    {
                        try
                        {
                            return ( DateTime? ) new FileInfo( f ).LastWriteTime;
                        }
                        catch
                        {
                            return null;
                        }
                    } )
                    .Where( d => d.HasValue )
                    .Select( d => ( DateTime? ) RockDateTime.ConvertLocalDateTimeToRockDateTime( d.Value ) )
                    // This is an attempt to fix random issues where people have the
                    // JS file cached in the browser. A theory is that some JS file
                    // has a future date time, so even after an upgrade the same
                    // fingerprint value is used. Ignore any dates in the future.
                    .Where( d => d < now )
                    .OrderByDescending( d => d )
                    .FirstOrDefault();

                _obsidianFingerprint = ( lastWriteTime ?? now ).Ticks;

                // Check if we are in debug mode and if so enable the watchers.
                var cfg = ( CompilationSection ) ConfigurationManager.GetSection( "system.web/compilation" );
                if ( cfg != null && cfg.Debug )
                {
                    AddObsidianFileSystemWatcher( obsidianPath, "*.js" );
                    AddObsidianFileSystemWatcher( pluginsPath, "*.js" );
                }
            }
            catch ( Exception ex )
            {
                _obsidianFingerprint = RockDateTime.Now.Ticks;
                Debug.WriteLine( ex.Message );
            }
        }

        /// <summary>
        /// Add a new file system watcher for the specified <paramref name="directory"/>.
        /// It will update the fingerprint whenever a file matching the
        /// <paramref name="filter"/> changes.
        /// </summary>
        /// <param name="directory">The directory, and any sub-directories, to watch.</param>
        /// <param name="filter">The filename filter to use when watching for changes.</param>
        private static void AddObsidianFileSystemWatcher( string directory, string filter )
        {
            // Setup a watcher to notify us of any changes to the directory.
            var watcher = new FileSystemWatcher
            {
                Path = directory,
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                Filter = filter
            };

            // Add event handlers.
            watcher.Changed += ObsidianFileSystemWatcher_OnChanged;
            watcher.Created += ObsidianFileSystemWatcher_OnChanged;
            watcher.Renamed += ObsidianFileSystemWatcher_OnRenamed;

            _obsidianFileWatchers.Add( watcher );

            // Begin watching.
            watcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// Handles the OnRenamed event of the Obsidian FileSystemWatcher.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="renamedEventArgs">The <see cref="RenamedEventArgs"/> instance containing the event data.</param>
        private static void ObsidianFileSystemWatcher_OnRenamed( object sender, RenamedEventArgs renamedEventArgs )
        {
            try
            {
                var dateTime = new FileInfo( renamedEventArgs.FullPath ).LastWriteTime;

                dateTime = RockDateTime.ConvertLocalDateTimeToRockDateTime( dateTime );

                _obsidianFingerprint = Math.Max( _obsidianFingerprint, dateTime.Ticks );
            }
            catch
            {
                _obsidianFingerprint = RockDateTime.Now.Ticks;
            }
        }

        /// <summary>
        /// Handles the OnChanged event of the Obsidian FileSystemWatcher.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="fileSystemEventArgs">The <see cref="FileSystemEventArgs"/> instance containing the event data.</param>
        private static void ObsidianFileSystemWatcher_OnChanged( object sender, FileSystemEventArgs fileSystemEventArgs )
        {
            try
            {
                var dateTime = new FileInfo( fileSystemEventArgs.FullPath ).LastWriteTime;

                dateTime = RockDateTime.ConvertLocalDateTimeToRockDateTime( dateTime );

                _obsidianFingerprint = Math.Max( _obsidianFingerprint, dateTime.Ticks );
            }
            catch
            {
                _obsidianFingerprint = RockDateTime.Now.Ticks;
            }
        }

        #endregion

        #region Person Preferences

        /// <summary>
        /// Gets the global person preferences. These are unique to the person
        /// but global across the entire system. Global preferences should be
        /// used with extreme caution and care.
        /// </summary>
        /// <returns>An instance of <see cref="PersonPreferenceCollection"/> that provides access to the preferences. This will never return <c>null</c>.</returns>
        public PersonPreferenceCollection GetGlobalPersonPreferences()
        {
            return RequestContext.GetGlobalPersonPreferences();
        }

        /// <summary>
        /// Gets the person preferences scoped to the specified entity.
        /// </summary>
        /// <param name="scopedEntity">The entity to use when scoping the preferences for a particular use.</param>
        /// <returns>An instance of <see cref="PersonPreferenceCollection"/> that provides access to the preferences. This will never return <c>null</c>.</returns>
        public PersonPreferenceCollection GetScopedPersonPreferences( IEntity scopedEntity )
        {
            return RequestContext.GetScopedPersonPreferences( scopedEntity );
        }

        /// <summary>
        /// Gets the person preferences scoped to the specified entity.
        /// </summary>
        /// <param name="scopedEntity">The entity to use when scoping the preferences for a particular use.</param>
        /// <returns>An instance of <see cref="PersonPreferenceCollection"/> that provides access to the preferences. This will never return <c>null</c>.</returns>
        public PersonPreferenceCollection GetScopedPersonPreferences( IEntityCache scopedEntity )
        {
            return RequestContext.GetScopedPersonPreferences( scopedEntity );
        }

        #endregion

        #region User Preferences (Obsolete)

        /// <summary>
        /// Returns a user preference for the current user and given key.
        /// </summary>
        /// <param name="key">A <see cref="System.String" /> representing the key to the user preference.</param>
        /// <returns>A <see cref="System.String" /> representing the specified user preference value, if a match is not found an empty string will be returned.</returns>
        [Obsolete( "Use the new PersonPreference methods instead." )]
        [RockObsolete( "1.16" )]
        public string GetUserPreference( string key )
        {
            return GetGlobalPersonPreferences().GetValue( key );
        }

        /// <summary>
        /// Returns the preference values for the current user that start with a given key.
        /// </summary>
        /// <param name="keyPrefix">A <see cref="System.String"/> representing the key prefix. Preference values, for the current user, with a key that begins with this value will be included.</param>
        /// <returns>A <see cref="System.Collections.Generic.Dictionary{String,String}"/> containing  the current user's preference values containing a key that begins with the specified value.
        /// Each <see cref="System.Collections.Generic.KeyValuePair{String,String}"/> contains a key that represents the user preference key and a value that contains the user preference value associated
        /// with that key.
        /// </returns>
        [Obsolete( "Use the new PersonPreference methods instead." )]
        [RockObsolete( "1.16" )]
        public Dictionary<string, string> GetUserPreferences( string keyPrefix )
        {
            var selectedValues = new Dictionary<string, string>();
            var preferences = GetGlobalPersonPreferences();

            foreach ( var key in preferences.GetKeys().Where( k => k.StartsWith( keyPrefix ) ) )
            {
                selectedValues.TryAdd( key, preferences.GetValue( key ) );
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
        [Obsolete( "Use the new PersonPreference methods instead." )]
        [RockObsolete( "1.16" )]
        public void SetUserPreference( string key, string value, bool saveValue = true )
        {
            var preferences = GetGlobalPersonPreferences();

            preferences.SetValue( key, value );

            if ( saveValue )
            {
                preferences.Save();
            }
        }

        /// <summary>
        /// Saves the user preferences.
        /// </summary>
        /// <param name="keyPrefix">The key prefix.</param>
        [Obsolete( "Use the new PersonPreference methods instead." )]
        [RockObsolete( "1.16" )]
        public void SaveUserPreferences( string keyPrefix )
        {
            GetGlobalPersonPreferences().Save();
        }

        /// <summary>
        /// Deletes a user preference value for the specified key
        /// </summary>
        /// <param name="key">A <see cref="System.String"/> representing the name of the key.</param>
        [Obsolete( "Use the new PersonPreference methods instead." )]
        [RockObsolete( "1.16" )]
        public void DeleteUserPreference( string key )
        {
            GetGlobalPersonPreferences().SetValue( key, string.Empty );
        }

        /// <summary>
        /// Returns the current user's preferences, if they have previously been loaded into the session, they
        /// will be retrieved from there, otherwise they will be retrieved from the database, added to session and
        /// then returned
        /// </summary>
        /// <returns>A <see cref="System.Collections.Generic.Dictionary{String, List}"/> containing the user preferences
        /// for the current user. If the current user is anonymous or unknown an empty dictionary will be returned.</returns>
        [Obsolete( "Use the new PersonPreference methods instead." )]
        [RockObsolete( "1.16" )]
        public Dictionary<string, string> SessionUserPreferences()
        {
            var preferences = GetGlobalPersonPreferences();
            var userPreferences = new Dictionary<string, string>();

            foreach ( var key in preferences.GetKeys() )
            {
                userPreferences.TryAdd( key, preferences.GetValue( key ) );
            }

            return userPreferences;
        }

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
                string triggerData = ( ( HiddenField ) dataControl ).Value;

                if ( triggerData.StartsWith( "BLOCK_UPDATED:" ) )
                {
                    var dataSegments = triggerData.Split( ':' );

                    if ( int.TryParse( dataSegments[1], out var blockId ) )
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
                if ( rockBlock.BlockCache.BlockType.Path?.Equals( blockTypePath, StringComparison.OrdinalIgnoreCase ) ?? false )
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

        /// <summary>
        /// Handles the Click event of the lbCacheControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void lbCacheControl_Click( object sender, EventArgs e )
        {
            var cacheControlCookie = Request.Cookies[RockCache.CACHE_CONTROL_COOKIE];
            var isCacheEnabled = cacheControlCookie == null || cacheControlCookie.Value.AsBoolean();

            if ( cacheControlCookie == null )
            {
                cacheControlCookie = new HttpCookie( RockCache.CACHE_CONTROL_COOKIE );
            }

            cacheControlCookie.Value = ( !isCacheEnabled ).ToString();

            AddOrUpdateCookie( cacheControlCookie );

            if ( PageReference != null )
            {
                string pageUrl = PageReference.BuildUrl();
                if ( !string.IsNullOrWhiteSpace( pageUrl ) )
                {
                    Response.Redirect( pageUrl, false );
                    Context.ApplicationInstance.CompleteRequest();
                }
            }
        }
        #endregion

        #region IHttpAsyncHandler Implementation

        /// <inheritdoc/>
        public IAsyncResult BeginProcessRequest( HttpContext context, AsyncCallback cb, object extraData )
        {
            RequestContext = new RockRequestContext( context.Request, new RockResponseContext( this ), CurrentUser );

            if ( _lazyServiceProvider.Value.GetRequiredService<IRockRequestContextAccessor>() is RockRequestContextAccessor internalAccessor )
            {
                internalAccessor.RockRequestContext = RequestContext;
            }

            return AsyncPageBeginProcessRequest( context, cb, extraData );
        }

        /// <inheritdoc/>
        public void EndProcessRequest( IAsyncResult result )
        {
            AsyncPageEndProcessRequest( result );

            if ( _lazyServiceProvider.Value.GetRequiredService<IRockRequestContextAccessor>() is RockRequestContextAccessor internalAccessor )
            {
                if ( ReferenceEquals( internalAccessor.RockRequestContext, RequestContext ) )
                {
                    internalAccessor.RockRequestContext = null;
                }
            }
        }

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

    /// <summary>
    /// The Context Entity Scope 
    /// </summary>
    public enum ContextEntityScope
    {
        /// <summary>
        /// Context Entities scoped to the Page.
        /// </summary>
        Page,

        /// <summary>
        /// Context Entities scoped to the Site.
        /// </summary>
        Site,

        /// <summary>
        /// All Context Entities, in any scope.
        /// </summary>
        All
    }

    #endregion

    /// <summary>
    /// Debug Timing
    /// </summary>
    public sealed class DebugTimingViewModel
    {
        /// <summary>
        /// Gets or sets the timestamp milliseconds.
        /// </summary>
        /// <value>
        /// The timestamp.
        /// </value>
        public double TimestampMs { get; set; }

        /// <summary>
        /// Gets or sets the event title.
        /// </summary>
        /// <value>
        /// The event HTML.
        /// </value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the sub title.
        /// </summary>
        /// <value>
        /// The sub title.
        /// </value>
        public string SubTitle { get; set; }

        /// <summary>
        /// Gets or sets the indent level.
        /// </summary>
        /// <value>
        /// The indent level.
        /// </value>
        public int IndentLevel { get; set; }

        /// <summary>
        /// Gets or sets the duration ms.
        /// </summary>
        /// <value>
        /// The duration ms.
        /// </value>
        public double DurationMs { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is title bold.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is title bold; otherwise, <c>false</c>.
        /// </value>
        public bool IsTitleBold { get; set; }
    }
}

