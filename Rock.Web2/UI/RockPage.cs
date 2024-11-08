using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using AngleSharp.Dom;
using AngleSharp.Html.Dom;

using Microsoft.AspNetCore.Mvc;

using Rock;
using Rock.Blocks;
using Rock.Net;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Web2.UI
{
    public class RockPage
    {
        #region Private Fields

        private readonly List<IElement> _headerElements = new List<IElement>();

        private readonly Dictionary<string, IElement> _startupScripts = new Dictionary<string, IElement>();

        private IHtmlDocument _layoutDocument;

        #endregion

        #region Public Properties

        public PageCache PageCache { get; }

        public int PageId => PageCache.Id;

        public LayoutCache Layout => PageCache.Layout;

        public SiteCache Site => Layout.Site;

        public RockRequestContext Context { get; }

        /// <summary>
        /// Publicly gets the currently logged in user.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.UserLogin"/> of the currently logged in user.
        /// </value>
        public Rock.Model.UserLogin CurrentUser
        {
            get
            {
                return Context.CurrentUser;
            }
        }

        /// <summary>
        /// Publicly gets the current <see cref="Rock.Model.Person"/>.  This is either the currently logged in user, or if the user
        /// has not logged in, it may also be an impersonated person determined from using the encrypted
        /// person key.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.Person"/> representing the currently logged in person or impersonated person.
        /// </value>
        public Rock.Model.Person CurrentPerson
        {
            get
            {
                return Context.CurrentPerson;
            }
        }

        /// <summary>
        /// The Person ID of the currently logged in user.  Returns null if there is not a user logged in
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32" /> representing the PersonId of the <see cref="Rock.Model.Person"/>
        /// who is logged in as the current user. If a user is not logged in.
        /// </value>
        public int? CurrentPersonId => CurrentPerson?.Id;

        /// <summary>
        /// Gets the current person alias.
        /// </summary>
        /// <value>
        /// The current person alias.
        /// </value>
        public Rock.Model.PersonAlias CurrentPersonAlias => CurrentPerson?.PrimaryAlias;

        /// <summary>
        /// Gets the current person alias identifier.
        /// </summary>
        /// <value>
        /// The current person alias identifier.
        /// </value>
        public int? CurrentPersonAliasId => CurrentPersonAlias?.Id;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RockPage"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <exception cref="System.Exception">Page not found</exception>
        public RockPage( PageCache pageCache, RockRequestContext context )
        {
            PageCache = pageCache ?? throw new ArgumentNullException( nameof( pageCache ), "pageCache cannot be null." );
            Context = context;
        }

        #endregion

        #region Support Methods for Blocks

        /// <summary>
        /// Resolves the rock URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        public string ResolveRockUrl( string url )
        {
            string themeUrl = url;
            if ( url.StartsWith( "~~" ) )
            {
                themeUrl = "~/Themes/" + Layout.Site.Theme + ( url.Length > 2 ? url.Substring( 2 ) : string.Empty );
            }

            return themeUrl.StartsWith( "~" ) ? themeUrl.Substring( 1 ) : themeUrl;
        }

        /// <summary>
        /// Adds the script link.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="fingerprint">if set to <c>true</c> [fingerprint].</param>
        public void AddScriptLink( string url, bool fingerprint = true )
        {
            if ( _headerElements.Any( c => c.GetAttribute( "src" ) == url ) )
            {
                return;
            }

            var el = _layoutDocument.CreateElement( "script" );
            el.SetAttribute( "src", url );
            _headerElements.Add( el );
        }

        /// <summary>
        /// Adds the meta tag.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="content">The content.</param>
        public void AddMetaTag( string name, string content )
        {
            var el = new HtmlElement( null, "meta" );
            el.SetAttribute( "name", name );
            el.SetAttribute( "content", content );

            _headerElements.Add( el );
        }

        /// <summary>
        /// Adds the icon link.
        /// </summary>
        /// <param name="binaryFileId">The binary file identifier.</param>
        /// <param name="size">The size.</param>
        /// <param name="rel">The relative.</param>
        public void AddIconLink( int binaryFileId, int size, string rel = "apple-touch-icon-precomposed" )
        {
            var el = new HtmlElement( null, "link" );
            el.SetAttribute( "rel", rel );
            el.SetAttribute( "sizes", $"{size}x{size}" );
            el.SetAttribute( "href", $"/GetImage.ashx?id={binaryFileId}&width={size}&height={size}&mode=crop&format=png" );

            _headerElements.Add( el );
        }

        /// <summary>
        /// Adds the CSS link.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="fingerprint">if set to <c>true</c> [fingerprint].</param>
        public void AddCSSLink( string url, bool fingerprint = true )
        {
            if ( _headerElements.Any( c => c.GetAttribute( "href" ) == url ) )
            {
                return;
            }

            var el = new HtmlElement( null, "link" );
            el.SetAttribute( "rel", "stylesheet" );
            el.SetAttribute( "href", url );

            _headerElements.Add( el );
        }

        /// <summary>
        /// Registers the startup script.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="key">The key.</param>
        /// <param name="script">The script.</param>
        public void RegisterStartupScript( Type type, string key, string script )
        {
            if ( _startupScripts.ContainsKey( $"{type.FullName}-{key}" ) )
            {
                return;
            }

            var el = _layoutDocument.CreateElement( "script" );
            el.TextContent = script;

            _startupScripts.Add( $"{type.FullName}-{key}", el );
        }

        #endregion

        #region Admin Features

        protected void GenerateAdminFooter( bool canAdministrateBlockOnPage = false )
        {
            bool canAdministratePage = PageCache.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, CurrentPerson );
            bool canEditPage = PageCache.IsAuthorized( Rock.Security.Authorization.EDIT, CurrentPerson );

            if ( PageCache.IncludeAdminFooter && ( canAdministratePage || canAdministrateBlockOnPage || canEditPage ) )
            {
                // Add the page admin script
                AddScriptLink( ResolveRockUrl( "~/Scripts/Bundles/RockAdmin.js" ), false );

                var adminFooter = _layoutDocument.CreateElement( "div" );
                adminFooter.SetAttribute( "id", "cms-admin-footer" );
                _layoutDocument.Body.AppendChild( adminFooter );

                //phLoadStats = new PlaceHolder();
                //adminFooter.Controls.Add( phLoadStats );

                //// If the current user is Impersonated by another user, show a link on the admin bar to login back in as the original user
                //var impersonatedByUser = Session["ImpersonatedByUser"] as UserLogin;
                //var currentUserIsImpersonated = ( HttpContext.Current?.User?.Identity?.Name ?? string.Empty ).StartsWith( "rckipid=" );
                //if ( canAdministratePage && currentUserIsImpersonated && impersonatedByUser != null )
                //{
                //    HtmlGenericControl impersonatedByUserDiv = new HtmlGenericControl( "span" );
                //    impersonatedByUserDiv.AddCssClass( "label label-default margin-l-md" );
                //    _btnRestoreImpersonatedByUser = new LinkButton();
                //    _btnRestoreImpersonatedByUser.ID = "_btnRestoreImpersonatedByUser";
                //    //_btnRestoreImpersonatedByUser.CssClass = "btn";
                //    _btnRestoreImpersonatedByUser.Visible = impersonatedByUser != null;
                //    _btnRestoreImpersonatedByUser.Click += _btnRestoreImpersonatedByUser_Click;
                //    _btnRestoreImpersonatedByUser.Text = $"<i class='fa-fw fa fa-unlock'></i> " + $"Restore { impersonatedByUser?.Person?.ToString()}";
                //    impersonatedByUserDiv.Controls.Add( _btnRestoreImpersonatedByUser );
                //    adminFooter.Controls.Add( impersonatedByUserDiv );
                //}

                var buttonBar = _layoutDocument.CreateElement( "div" );
                adminFooter.AppendChild( buttonBar );
                buttonBar.SetAttribute( "class", "button-bar" );

                // RockBlock Config
                if ( canAdministratePage || canAdministrateBlockOnPage )
                {
                    var aBlockConfig = JavascriptIconLinkElement( "Block Configuration", "btn block-config", "fa fa-th-large", "Rock.admin.pageAdmin.showBlockConfig()" );
                    buttonBar.AppendChild( aBlockConfig );
                }

                if ( canEditPage || canAdministratePage )
                {
                    // RockPage Properties
                    var aPageProperties = ModalIconLinkElement( "Page Properties", "btn properties", "fa fa-cog", ResolveRockUrl( $"~/PageProperties/{PageId}?t=Page Properties" ) );
                    aPageProperties.SetAttribute( "id", "aPageProperties" );
                    buttonBar.AppendChild( aPageProperties );
                }

                if ( canAdministratePage )
                {
                    // Child Pages
                    var aChildPages = ModalIconLinkElement( "Child Pages", "btn page-child-pages", "fa fa-sitemap", ResolveRockUrl( $"~/pages/{PageId}?t=Child Pages&pb=&sb=Done" ) );
                    aChildPages.SetAttribute( "id", "aChildPages" );
                    buttonBar.AppendChild( aChildPages );

                    // RockPage Zones
                    var aPageZones = JavascriptIconLinkElement( "Page Zones", "btn page-zones", "fa fa-columns", "Rock.admin.pageAdmin.showPageZones()" );
                    buttonBar.AppendChild( aPageZones );

                    // RockPage Security
                    //var pageEntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Page ) ).Id;
                    var pageEntityTypeId = 42;
                    var aPageSecurity = ModalIconLinkElement( "Page Security", "btn page-security", "fa fa-lock", ResolveRockUrl( $"~/Secure/{pageEntityTypeId}/{PageId}?t=Page Security&pb=&sb=Done" ) );
                    buttonBar.AppendChild( aPageSecurity );

                    // ShorLink Properties
                    //var aShortLink = ModalIconLinkElement( "Add Short Link", "btn properties", "fa fa-link", ResolveRockUrl( $"~/ShortLink/{PageId}?t=Shortened Link&url={Context.RawUrl}" ) );
                    //aShortLink.SetAttribute( "id", "aShortLink" );
                    //buttonBar.AppendChild( aShortLink );

                    // System Info
                    var aSystemInfo = ModalIconLinkElement( "Rock Information", "btn system-info", "fa fa-info-circle", ResolveRockUrl( "~/SystemInfo?t=System Information&pb=&sb=Done" ) );
                    buttonBar.AppendChild( aSystemInfo );
                }
            }
        }

        protected IElement JavascriptIconLinkElement( string title, string linkClass, string iconClass, string script )
        {
            var el = _layoutDocument.CreateElement( "a" );

            el.SetAttribute( "title", title );
            el.SetAttribute( "class", linkClass );
            el.SetAttribute( "href", $"javascript: {script}" );

            var icon = _layoutDocument.CreateElement( "i" );
            icon.SetAttribute( "class", iconClass );

            el.AppendChild( icon );

            return el;
        }

        protected IElement ModalIconLinkElement( string title, string linkClass, string iconClass, string url )
        {
            return JavascriptIconLinkElement( title, linkClass, iconClass, $"Rock.controls.modal.show($(this), '{url}')" );
        }

        #endregion

        #region Render Methods

        /// <summary>
        /// Finalizes the content of the page with any data that is waiting for final insertion.
        /// </summary>
        protected void FinalizePageContent()
        {
            var head = _layoutDocument.GetElementsByTagName( "head" ).Single();
            head.AppendNodes( _headerElements.ToArray() );

            var body = _layoutDocument.GetElementsByTagName( "body" ).Single();
            body.AppendNodes( _startupScripts.Values.ToArray() );
        }

        /// <summary>
        /// Renders the page into the stream.
        /// </summary>
        /// <param name="stream">The stream to write the page contents to.</param>
        /// <returns>A task that can be awaited.</returns>
        public async Task<IActionResult> RenderAsync()
        {
            if ( !PageCache.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
            {
                if ( CurrentPerson != null )
                {
                    return new ForbidResult();
                }
                else
                {
                    return new RedirectResult( $"/page/{Site.LoginPageId}" );
                }
            }

            _layoutDocument = await Layout.GetHtmlDocumentAsync( this );

            AddScriptLink( ResolveRockUrl( "/Scripts/Bundles/RockLibs.js" ) );
            AddScriptLink( ResolveRockUrl( "/Scripts/Bundles/RockUi.js" ) );
            AddScriptLink( "/Obsidian/obsidian-core.js", true );
            //AddScriptLink( ResolveRockUrl( "~/Scripts/Bundles/RockValidation.js" ) );

            var script = $@"
Obsidian.onReady(() => {{
    System.import('/Obsidian/Index.js').then(indexModule => {{
        indexModule.initializePage({{
            executionStartTime: new Date().getTime(),
            pageId: {PageCache.Id},
            pageGuid: '{PageCache.Guid}',
            pageParameters: {{}},
            currentPerson: null,
            contextEntities: {{}},
            loginUrlWithReturnUrl: ''
        }});
    }});
}});

Obsidian.init({{ debug: true, fingerprint: ""v={Guid.NewGuid()}"" }});
";

            RegisterStartupScript( GetType(), "rock-obsidian-init", script );

            GenerateAdminFooter();

            FinalizePageContent();

            var html = _layoutDocument.DocumentElement.OuterHtml;

            //
            // Create the block wrappers for each block and add them to the DOM.
            //
            var authorizedBlocks = PageCache.Blocks
                .Where( b => b.IsAuthorized( Rock.Security.Authorization.VIEW, CurrentPerson ) );

            var zoneData = new Dictionary<string, string>();

            foreach ( var b in authorizedBlocks )
            {
                if ( b.BlockType.EntityType?.AssemblyName.IsNullOrWhiteSpace() ?? true )
                {
                    continue;
                }

                var instanceType = Type.GetType( b.BlockType.EntityType.AssemblyName.Replace( ", Rock.Blocks,", ", Rock.Blocks.NG," ) );

                if ( instanceType == null )
                {
                    continue;
                }

                var block = ( IRockWebBlockType ) Activator.CreateInstance( instanceType );
                block.BlockCache = b;
                block.PageCache = PageCache;
                block.RequestContext = Context;

                var blockInit = ( await block.GetControlMarkupAsync() ) ?? string.Empty;

                if ( zoneData.ContainsKey( b.Zone ) )
                {
                    zoneData[b.Zone] = zoneData[b.Zone] + blockInit;
                }
                else
                {
                    zoneData[b.Zone] = blockInit;
                }
            };

            html = System.Text.RegularExpressions.Regex.Replace( html, "\\<rock:zone name=\\\"([a-zA-Z ]+)\\\">\\s*\\<\\/rock:zone\\>", match =>
            {
                var zoneName = match.Groups[1].Value.Replace( " ", string.Empty );

                if ( zoneData.ContainsKey( zoneName ) )
                {
                    return zoneData[zoneName];
                }
                else
                {
                    return string.Empty;
                }
            }, System.Text.RegularExpressions.RegexOptions.IgnoreCase );

            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = 200,
                Content = html
            };
        }

        #endregion
    }
}