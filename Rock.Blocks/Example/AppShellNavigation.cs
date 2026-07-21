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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;

using Rock.Attribute;
using Rock.Enums.Cms;
using Rock.Model;
using Rock.Utility.ExtensionMethods;
using Rock.ViewModels.Blocks.Example.AppShellNavigation;
using Rock.Web.Cache;

namespace Rock.Blocks.Example
{
    /// <summary>
    /// An experimental app-shell "app-shell" navigation sidebar. Unlike the
    /// look-and-feel-only <see cref="AppShellSidebar"/> proof of concept, this
    /// block renders Rock's REAL, security-trimmed page navigation (via
    /// <c>PageCache.GetMenuProperties</c>). The account menu at the bottom of the
    /// sidebar is the separate Login Status block, hosted in the Login zone of the
    /// layout footer. The remaining visual widgets (notifications, projects,
    /// spending, organizations) are static demo content rendered client-side.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "App Shell Navigation" )]
    [Category( "Obsidian > Example" )]
    [Description( "An experimental app-shell-style navigation sidebar wired to Rock's real page navigation, breadcrumbs source, and universal search." )]
    [IconCssClass( "ti ti-layout-sidebar" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [LinkedPage(
        "Root Page",
        Description = "The root page to use for the navigation tree. Defaults to the current page instance if not set.",
        IsRequired = false,
        Order = 0,
        Key = AttributeKey.RootPage )]

    [TextField(
        "Number of Levels",
        Description = "Number of parent-child page levels to display. Default 3.",
        IsRequired = false,
        DefaultValue = "3",
        Order = 1,
        Key = AttributeKey.NumberOfLevels )]

    [BooleanField(
        "Include Current Parameters",
        Description = "Flag indicating if the current page's route parameters should be used when building URLs for child pages.",
        DefaultBooleanValue = false,
        Order = 2,
        Key = AttributeKey.IncludeCurrentParameters )]

    [BooleanField(
        "Include Current QueryString",
        Description = "Flag indicating if the current page's query string should be used when building URLs for child pages.",
        DefaultBooleanValue = false,
        Order = 3,
        Key = AttributeKey.IncludeCurrentQueryString )]

    [BooleanField(
        "Enable Search",
        Description = "Flag indicating whether the universal search trigger is shown in the sidebar.",
        DefaultBooleanValue = true,
        Order = 4,
        Key = AttributeKey.EnableSearch )]

    #endregion

    [ConfigurationChangedReload( BlockReloadMode.Block )]
    [Rock.SystemGuid.EntityTypeGuid( "6C0F1437-7E19-409B-9CE2-C8EC5D9A8CD5" )]
    [Rock.SystemGuid.BlockTypeGuid( "DE118B96-19C4-4992-A3BC-5F777B3D1C68" )]
    public class AppShellNavigation : RockBlockType
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string RootPage = "RootPage";
            public const string NumberOfLevels = "NumberOfLevels";
            public const string IncludeCurrentParameters = "IncludeCurrentParameters";
            public const string IncludeCurrentQueryString = "IncludeCurrentQueryString";
            public const string EnableSearch = "EnableSearch";
        }

        #endregion Attribute Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new AppShellNavigationInitializationBox
            {
                NavItems = GetNavItems(),
                IsSearchEnabled = GetAttributeValue( AttributeKey.EnableSearch ).AsBoolean(),
                HomePageUrl = RequestContext.ResolveRockUrl( "~" )
            };

            return box;
        }

        /// <summary>
        /// Builds the real, security-trimmed navigation tree by mirroring the data
        /// preparation used by the Page Menu block and projecting the resulting
        /// menu dictionary into strongly-typed bags.
        /// </summary>
        /// <returns>The top-level navigation items.</returns>
        private List<AppShellNavItemBag> GetNavItems()
        {
            var currentPage = PageCache;
            PageCache rootPage = null;

            var pageRouteValuePair = GetAttributeValue( AttributeKey.RootPage ).SplitDelimitedValues( false ).AsGuidOrNullList();
            if ( pageRouteValuePair.Any() && pageRouteValuePair[0].HasValue && !pageRouteValuePair[0].Value.IsEmpty() )
            {
                rootPage = PageCache.Get( pageRouteValuePair[0].Value );
            }

            /*
                7/20/26 - CLAUDE

                When no Root Page is configured, fall back to the site's default
                (home) page rather than the current page. Rooting the tree at the
                current page rebuilds the entire menu on every navigation, so the
                sidebar appears to "switch" whenever you open a page. A global app
                shell needs a static navigation tree; the current page still drives
                the active-item highlight and auto-expand via the page hierarchy
                passed to GetMenuProperties below.

                Reason: Keep shell navigation static at the site root instead of
                following the selected page.
            */
            if ( rootPage == null )
            {
                rootPage = currentPage?.Layout.Site.DefaultPage;
            }

            // Last resort: if the site has no default page, use the current page
            // so the sidebar is at least populated.
            if ( rootPage == null )
            {
                rootPage = currentPage;
            }

            if ( rootPage == null )
            {
                return new List<AppShellNavItemBag>();
            }

            var levelsDeep = GetAttributeValue( AttributeKey.NumberOfLevels ).AsInteger();

            Dictionary<string, string> pageParameters = null;
            if ( GetAttributeValue( AttributeKey.IncludeCurrentParameters ).AsBoolean() )
            {
                pageParameters = new Dictionary<string, string>( RequestContext.PageParameters );
            }

            NameValueCollection queryString = null;
            if ( GetAttributeValue( AttributeKey.IncludeCurrentQueryString ).AsBoolean() )
            {
                queryString = RequestContext.QueryString;
            }

            // Get the list of pages in the current page's hierarchy so the active
            // item and its ancestors can be flagged.
            var pageHierarchy = new List<int>();
            if ( currentPage != null )
            {
                pageHierarchy = currentPage.GetPageHierarchy().Select( p => p.Id ).ToList();
            }

            // GetMenuProperties returns the root node; its "Pages" entry is the
            // top-level menu the sidebar renders.
            var menuProperties = rootPage.GetMenuProperties( levelsDeep, RequestContext.CurrentPerson, RockContext, pageHierarchy, pageParameters, queryString );

            if ( menuProperties != null && menuProperties.TryGetValue( "Pages", out var pages ) )
            {
                return MapNavItems( pages );
            }

            return new List<AppShellNavItemBag>();
        }

        /// <summary>
        /// Recursively projects the untyped menu dictionary produced by
        /// <c>PageCache.GetMenuProperties</c> into typed navigation bags.
        /// </summary>
        /// <param name="pagesObject">The "Pages" value from a menu node.</param>
        /// <returns>The mapped navigation items.</returns>
        private List<AppShellNavItemBag> MapNavItems( object pagesObject )
        {
            var items = new List<AppShellNavItemBag>();

            if ( !( pagesObject is List<Dictionary<string, object>> pages ) )
            {
                return items;
            }

            foreach ( var page in pages )
            {
                var children = page.TryGetValue( "Pages", out var childPages )
                    ? MapNavItems( childPages )
                    : new List<AppShellNavItemBag>();

                items.Add( new AppShellNavItemBag
                {
                    Id = GetMenuString( page, "Id" ),
                    Title = GetMenuString( page, "Title" ),
                    Url = GetMenuString( page, "Url" ),
                    IconCssClass = GetMenuString( page, "IconCssClass" ),
                    IsCurrent = GetMenuBool( page, "Current" ),
                    IsParentOfCurrent = GetMenuBool( page, "IsParentOfCurrent" ),
                    DisplayChildPages = GetMenuBool( page, "DisplayChildPages" ),
                    Children = children
                } );
            }

            return items;
        }

        /// <summary>
        /// Reads a string value from a menu-node dictionary, or an empty string if absent.
        /// </summary>
        private static string GetMenuString( Dictionary<string, object> node, string key )
        {
            return node.TryGetValue( key, out var value ) ? value.ToStringSafe() : string.Empty;
        }

        /// <summary>
        /// Reads a boolean value from a menu-node dictionary, or false if absent.
        /// </summary>
        private static bool GetMenuBool( Dictionary<string, object> node, string key )
        {
            return node.TryGetValue( key, out var value ) && value.ToStringSafe().AsBoolean();
        }

        #endregion Methods

        #region Static HTML Rendering

        /// <summary>
        /// Renders the resting-state sidebar (header + scrollable content) as static
        /// HTML so the navigation is present and styled at first paint, before the
        /// Obsidian/Vue block mounts. This is the "pseudo-static block" pattern
        /// documented on <see cref="RockBlockType.GetInitialHtmlContent"/>: returning
        /// non-empty content suppresses the default loading skeleton. The Vue island
        /// still mounts afterward and replaces this markup; because it renders the
        /// same class names (styled globally in the theme's _app-shell.scss), the
        /// swap is seamless with no skeleton flash on each page load. The account
        /// menu is not rendered here: it is the separate Login Status block hosted in
        /// the layout footer's Login zone.
        /// </summary>
        /// <returns>The resting-state sidebar HTML.</returns>
        protected override string GetInitialHtmlContent()
        {
            var navItems = GetNavItems();
            var isSearchEnabled = GetAttributeValue( AttributeKey.EnableSearch ).AsBoolean();
            var homePageUrl = RequestContext.ResolveRockUrl( "~" );

            var sb = new StringBuilder();

            sb.Append( "<div class=\"asn-root\">" );

            // Header (brand + collapse toggle).
            sb.Append( "<div class=\"asn-header\">" );
            sb.Append( $"<a class=\"asn-brand\" href=\"{Encode( homePageUrl )}\" title=\"Home\">" );
            sb.Append( "<span class=\"asn-logo\"></span>" );
            sb.Append( "<span class=\"asn-label asn-wordmark\">Rock RMS</span>" );
            sb.Append( "</a>" );
            sb.Append( "<div class=\"asn-header-actions\">" );
            sb.Append( "<button type=\"button\" class=\"asn-collapse-trigger\" aria-label=\"Toggle sidebar\"><i class=\"ti ti-layout-sidebar\"></i></button>" );
            sb.Append( "</div>" );
            sb.Append( "</div>" );

            // Scrollable content region.
            sb.Append( "<div class=\"asn-content\">" );

            if ( isSearchEnabled )
            {
                sb.Append( "<div class=\"asn-group asn-search-group\">" );
                sb.Append( "<button type=\"button\" class=\"asn-search-btn\" aria-label=\"Search\">" );
                sb.Append( "<i class=\"ti ti-search asn-search-icon\"></i>" );
                sb.Append( "<span class=\"asn-label asn-search-text\">Search...</span>" );
                sb.Append( "<kbd class=\"asn-kbd asn-label\">&#8984;K</kbd>" );
                sb.Append( "</button>" );
                sb.Append( "</div>" );
            }

            // The real, security-trimmed navigation tree.
            sb.Append( "<div class=\"asn-group\">" );

            // Group label with the search toggle that reveals the filter field.
            sb.Append( "<div class=\"asn-group-label asn-label asn-nav-label\">" );
            sb.Append( "<span>Navigation</span>" );
            sb.Append( "<button type=\"button\" class=\"asn-nav-search-toggle\" aria-label=\"Search pages\"><i class=\"ti ti-search\"></i></button>" );
            sb.Append( "</div>" );

            // Resting-state nav filter, approximating the DOM the Rock TextBox in
            // navMain.partial.obs renders (field wrapper + overlaid search glyph +
            // form-group > control-wrapper > input-container > input.form-control)
            // so the static markup matches the mounted output. It is hidden by
            // default (the field is revealed only when the toggle is clicked, which
            // the Vue island wires up on mount) and inert (readonly) until then.
            sb.Append( "<div class=\"asn-navsearch-field asn-label\" style=\"display:none\">" );
            sb.Append( "<i class=\"ti ti-filter asn-navsearch-icon\"></i>" );
            sb.Append( "<div class=\"form-group rock-text-box asn-navsearch\">" );
            sb.Append( "<div class=\"control-wrapper\">" );
            sb.Append( "<div class=\"input-container\">" );
            sb.Append( "<input type=\"text\" class=\"form-control input-sm\" placeholder=\"Filter pages...\" autocomplete=\"off\" spellcheck=\"false\" aria-label=\"Filter navigation pages\" readonly>" );
            sb.Append( "</div>" );
            sb.Append( "</div>" );
            sb.Append( "</div>" );
            sb.Append( "</div>" );

            sb.Append( "<ul class=\"asn-menu\">" );
            foreach ( var item in navItems )
            {
                RenderNavItem( sb, item, 0 );
            }
            sb.Append( "</ul>" );
            sb.Append( "</div>" );

            RenderProjects( sb );

            sb.Append( "<div class=\"asn-spacer\">" );
            RenderSpending( sb );
            sb.Append( "</div>" );

            sb.Append( "</div>" );

            sb.Append( "</div>" );

            return sb.ToString();
        }

        /// <summary>
        /// Recursively renders a navigation item, mirroring navItem.partial.obs so
        /// the static markup matches the mounted Vue output: top-level items with
        /// children are collapsible buttons (expanded when they contain the current
        /// page), deeper items with children are static grouping labels, and leaves
        /// are links.
        /// </summary>
        /// <param name="sb">The builder to append to.</param>
        /// <param name="item">The navigation item to render.</param>
        /// <param name="depth">The zero-based depth of the item in the tree.</param>
        private void RenderNavItem( StringBuilder sb, AppShellNavItemBag item, int depth )
        {
            var hasChildren = item.DisplayChildPages && item.Children != null && item.Children.Any();

            sb.Append( "<li class=\"asn-menu-item\">" );

            if ( depth == 0 && hasChildren )
            {
                var isOpen = item.IsParentOfCurrent || item.IsCurrent;
                var activeClass = item.IsCurrent ? " is-active" : string.Empty;
                var openClass = isOpen ? " is-open" : string.Empty;

                sb.Append( $"<button type=\"button\" class=\"asn-menu-button{activeClass}\" aria-expanded=\"{( isOpen ? "true" : "false" )}\">" );
                if ( item.IconCssClass.IsNotNullOrWhiteSpace() )
                {
                    sb.Append( $"<i class=\"{Encode( item.IconCssClass )}\"></i>" );
                }
                sb.Append( $"<span class=\"asn-label\">{Encode( item.Title )}</span>" );
                sb.Append( $"<i class=\"ti ti-chevron-right asn-chevron asn-label{openClass}\"></i>" );
                sb.Append( "</button>" );

                if ( isOpen )
                {
                    sb.Append( "<ul class=\"asn-menu asn-submenu\">" );
                    foreach ( var child in item.Children )
                    {
                        RenderNavItem( sb, child, depth + 1 );
                    }
                    sb.Append( "</ul>" );
                }
            }
            else if ( hasChildren )
            {
                sb.Append( $"<div class=\"asn-subgroup-label asn-label\">{Encode( item.Title )}</div>" );
                sb.Append( "<ul class=\"asn-menu\">" );
                foreach ( var child in item.Children )
                {
                    RenderNavItem( sb, child, depth + 1 );
                }
                sb.Append( "</ul>" );
            }
            else
            {
                var activeClass = item.IsCurrent ? " is-active" : string.Empty;
                var url = item.Url.IsNotNullOrWhiteSpace() ? item.Url : "#";

                sb.Append( $"<a href=\"{Encode( url )}\" class=\"asn-menu-button{activeClass}\">" );
                if ( depth == 0 && item.IconCssClass.IsNotNullOrWhiteSpace() )
                {
                    sb.Append( $"<i class=\"{Encode( item.IconCssClass )}\"></i>" );
                }
                sb.Append( $"<span class=\"asn-label\">{Encode( item.Title )}</span>" );
                sb.Append( "</a>" );
            }

            sb.Append( "</li>" );
        }

        /// <summary>
        /// Renders the static "Active Projects" demo widget. This content has no
        /// Rock data source; it mirrors the activeProjects array in data.partial.ts
        /// so the resting markup matches the Vue partial and the mount swap is
        /// seamless.
        /// </summary>
        /// <param name="sb">The builder to append to.</param>
        private void RenderProjects( StringBuilder sb )
        {
            var projects = new[]
            {
                new { Name = "Design System", Progress = 72, Color = "var(--color-info-strong)" },
                new { Name = "API Integration", Progress = 45, Color = "var(--color-primary)" },
                new { Name = "Mobile App", Progress = 88, Color = "var(--color-success-strong)" },
                new { Name = "Analytics Dashboard", Progress = 30, Color = "var(--color-warning-strong)" },
                new { Name = "Auth Module", Progress = 60, Color = "var(--color-danger-strong)" }
            };

            const double radius = 6;
            var circumference = 2 * Math.PI * radius;
            var circumferenceText = circumference.ToString( "0.###", CultureInfo.InvariantCulture );

            sb.Append( "<div class=\"asn-group asn-projects\">" );
            sb.Append( "<button type=\"button\" class=\"asn-group-label asn-projects-toggle asn-label\" aria-expanded=\"true\">" );
            sb.Append( "Active Projects" );
            sb.Append( "<i class=\"ti ti-chevron-down asn-chevron\"></i>" );
            sb.Append( "</button>" );
            sb.Append( "<ul class=\"asn-menu asn-projects-list\">" );

            foreach ( var project in projects )
            {
                var clamped = Math.Min( 100, Math.Max( 0, project.Progress ) );
                var dashOffset = ( circumference * ( 1 - clamped / 100.0 ) ).ToString( "0.###", CultureInfo.InvariantCulture );
                var title = $"{project.Name} - {project.Progress}% complete";

                sb.Append( "<li class=\"asn-menu-item\">" );
                sb.Append( $"<a href=\"#\" class=\"asn-menu-button\" title=\"{Encode( title )}\">" );
                sb.Append( "<svg width=\"16\" height=\"16\" viewBox=\"0 0 16 16\" class=\"asn-donut\" aria-hidden=\"true\">" );
                sb.Append( "<circle cx=\"8\" cy=\"8\" r=\"6\" fill=\"none\" class=\"asn-donut-track\" stroke-width=\"2.5\"></circle>" );
                sb.Append( $"<circle cx=\"8\" cy=\"8\" r=\"6\" fill=\"none\" style=\"stroke: {project.Color}\" stroke-width=\"2.5\" stroke-linecap=\"round\" stroke-dasharray=\"{circumferenceText}\" stroke-dashoffset=\"{dashOffset}\"></circle>" );
                sb.Append( "</svg>" );
                sb.Append( $"<span class=\"asn-label asn-project-name\">{Encode( project.Name )}</span>" );
                sb.Append( "</a>" );
                sb.Append( $"<button type=\"button\" class=\"asn-action\" aria-label=\"Actions for {Encode( project.Name )}\"><i class=\"ti ti-dots\"></i></button>" );
                sb.Append( "</li>" );
            }

            sb.Append( "</ul>" );
            sb.Append( "</div>" );
        }

        /// <summary>
        /// Renders the static "Spending Limit" demo widget. This content has no Rock
        /// data source; it mirrors the fixed figures in spendingCard.partial.obs.
        /// </summary>
        /// <param name="sb">The builder to append to.</param>
        private void RenderSpending( StringBuilder sb )
        {
            const int used = 8240;
            const int total = 10000;
            var usedPct = ( int ) Math.Round( used / ( double ) total * 100 );
            var freePct = 100 - usedPct;

            sb.Append( "<div class=\"asn-group asn-spending\">" );
            sb.Append( "<div class=\"asn-spending-card\">" );
            sb.Append( "<div class=\"asn-spending-title\">Spending Limit</div>" );
            sb.Append( "<p class=\"asn-spending-copy\">Consumption and balance will be reset at the end of the month</p>" );
            sb.Append( "<div class=\"asn-spending-track\">" );
            sb.Append( "<div class=\"asn-spending-hatch\" aria-hidden=\"true\"></div>" );
            sb.Append( $"<div class=\"asn-spending-fill\" style=\"width: {usedPct}%\"></div>" );
            sb.Append( "</div>" );
            sb.Append( "<div class=\"asn-spending-legend\">" );
            sb.Append( $"<div class=\"asn-spending-stat\"><span class=\"asn-spending-value\">{usedPct}%</span><span class=\"asn-spending-muted\">Used</span></div>" );
            sb.Append( $"<div class=\"asn-spending-stat\"><span class=\"asn-spending-value\">{freePct}%</span><span class=\"asn-spending-muted\">Free</span></div>" );
            sb.Append( "</div>" );
            sb.Append( "</div>" );
            sb.Append( "</div>" );
        }

        /// <summary>
        /// HTML-encodes a value for safe inclusion in the static markup. Uses
        /// <see cref="WebUtility"/> (in System, not System.Web) so this shared block
        /// avoids a System.Web dependency.
        /// </summary>
        /// <param name="value">The value to encode.</param>
        /// <returns>The HTML-encoded value.</returns>
        private static string Encode( string value )
        {
            return WebUtility.HtmlEncode( value ?? string.Empty );
        }

        #endregion Static HTML Rendering
    }
}
