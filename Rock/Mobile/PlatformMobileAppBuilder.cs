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
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Rock.Data;
using Rock.Enums.Mobile;
using Rock.Model;
using Rock.SystemKey;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Mobile
{
    /// <summary>
    /// Creates and maintains the platform mobile application: the one mobile Site, with fixed
    /// Guids, that every church's Rock serves to the shared app. Build, Update and Repair are
    /// the same idempotent code: every step finds its record by Guid, creates it if missing,
    /// then asserts the platform owned fields.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The definition is a ladder of versions keyed by the Rock version each ships in. The
    /// ladder only moves forward: once a Rock version ships, no entry may be added at or below
    /// it on any branch, hotfix steps are merged forward into develop, and a shipped step is
    /// frozen. That rule is what lets the stamp be a single high-water value.
    /// </para>
    /// <para>
    /// Church owned values (the palette colors, the in-app logo, a block's church-picked
    /// settings) are never written here. Seed-once values, such as the bootstrap API key, are
    /// created when missing and never overwritten.
    /// </para>
    /// </remarks>
    internal static class PlatformMobileAppBuilder
    {
        #region Fields

        /// <summary>
        /// The Guid of the platform mobile application Site.
        /// </summary>
        private static readonly Guid PlatformSiteGuid = new Guid( SystemGuid.Site.PLATFORM_MOBILE_APPLICATION );

        /// <summary>
        /// The builder ladder, one entry per Rock version that changes the definition, in
        /// strictly increasing order. See the forward-only rule in the class remarks.
        /// </summary>
        private static readonly IReadOnlyList<LadderEntry> Ladder = new List<LadderEntry>
        {
            new LadderEntry( new Version( 21, 0 ), EnsureRock21_0 )
        };

        /// <summary>
        /// The default Phone/Tablet XAML for the "Homepage" layout, matching the layout a new
        /// mobile application gets from MobileApplicationDetail.
        /// </summary>
        private const string HomepageLayoutXaml = @"<ContentPage xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
    xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
    xmlns:Rock=""clr-namespace:Rock.Mobile.Cms;assembly=Rock.Mobile"">
    <ScrollView VerticalScrollBarVisibility=""Never"">
        <Rock:Zone ZoneName=""Main"" />
    </ScrollView>
</ContentPage>";

        /// <summary>
        /// The Phone/Tablet XAML for the "Full" layout: the Main zone with no scroll wrapper.
        /// </summary>
        private const string FullLayoutXaml = @"<ContentPage xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
    xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
    xmlns:Rock=""clr-namespace:Rock.Mobile.Cms;assembly=Rock.Mobile"">
    <Rock:Zone ZoneName=""Main"" />
</ContentPage>";

        /// <summary>
        /// The platform owned name of the Site. Shown in the admin UI's mobile application list.
        /// </summary>
        private const string SiteName = "Platform Mobile Application";

        /// <summary>
        /// The zone every builder block is placed in. Both layouts define it.
        /// </summary>
        private const string MainZone = "Main";

        /// <summary>
        /// The home page menu. Rendered on the server per request (Dynamic Content) so it can
        /// show the log in button or the feature buttons depending on who is logged in.
        /// </summary>
        private const string HomeMenuXaml = @"<StackLayout Spacing=""12"" Padding=""16"">
{% if CurrentPerson %}
    <Label Text=""Hi {{ CurrentPerson.NickName | Escape }}"" StyleClass=""title1"" />
    <Button Text=""Outreach Toolbox"" StyleClass=""btn, btn-primary"" Command=""{Binding PushPage}"" CommandParameter=""" + SystemGuid.PlatformMobileApp.Page.OUTREACH + @""" />
    <Button Text=""Connections"" StyleClass=""btn, btn-primary"" Command=""{Binding PushPage}"" CommandParameter=""" + SystemGuid.PlatformMobileApp.Page.CONNECTIONS + @""" />
    <Button Text=""Log Out"" StyleClass=""btn, btn-link"" Command=""{Binding Logout}"" />
{% else %}
    <Label Text=""Welcome"" StyleClass=""title1"" />
    <Label Text=""Log in to use the Outreach Toolbox and Connections."" />
    <Button Text=""Log In"" StyleClass=""btn, btn-primary"" Command=""{Binding PushPage}"" CommandParameter=""" + SystemGuid.PlatformMobileApp.Page.LOGIN + @""" />
{% endif %}
    <Button Text=""Switch Church"" StyleClass=""btn, btn-link"" Command=""{Binding SwitchChurch}"" />
</StackLayout>";

        /// <summary>
        /// The block types the builder places. Their attributes are created before the ladder
        /// runs, because a block's settings can only be written once its attributes exist.
        /// </summary>
        private static readonly string[] BuilderBlockTypeGuids = new[]
        {
            MobileContentBlockTypeGuid,
            MobileLoginBlockTypeGuid,
            SystemGuid.BlockType.MOBILE_OUTREACH_OUTREACH_BEACON_DASHBOARD,
            SystemGuid.BlockType.MOBILE_OUTREACH_MY_CONTACTS,
            SystemGuid.BlockType.MOBILE_OUTREACH_CONTACT_PROFILE,
            SystemGuid.BlockType.MOBILE_OUTREACH_ADD_CONTACT,
            SystemGuid.BlockType.MOBILE_OUTREACH_TOUCHPOINT_DETAIL,
            MobileConnectionTypeListBlockTypeGuid,
            MobileConnectionOpportunityListBlockTypeGuid,
            MobileConnectionRequestListBlockTypeGuid,
            MobileConnectionRequestDetailBlockTypeGuid,
            MobileAddConnectionRequestBlockTypeGuid
        };

        /*
            9/29/2026 - CLAUDE

            These block types declare their Guids inline on the block class and have no
            SystemGuid.BlockType constant, and Rock does not reference Rock.Blocks, so the
            values are repeated here. Block type Guids never change.

            Reason: No SystemGuid constants exist for these block types.
        */

        /// <summary>
        /// The mobile Content block type (Rock.Blocks.Types.Mobile.Cms.Content).
        /// </summary>
        private const string MobileContentBlockTypeGuid = "7258A210-E936-4260-B573-9FA1193AD9E2";

        /// <summary>
        /// The mobile Login block type (Rock.Blocks.Types.Mobile.Cms.Login).
        /// </summary>
        private const string MobileLoginBlockTypeGuid = "6006FE32-DC01-4B1C-A9B8-EE172451F4C5";

        /// <summary>
        /// The mobile Connection Type List block type (Rock.Blocks.Mobile.Connection).
        /// </summary>
        private const string MobileConnectionTypeListBlockTypeGuid = "A7FF3F7F-AC1D-4C07-A1E1-FBDE8F689F6A";

        /// <summary>
        /// The mobile Connection Opportunity List block type (Rock.Blocks.Mobile.Connection).
        /// </summary>
        private const string MobileConnectionOpportunityListBlockTypeGuid = "039AB104-FDFE-4BB0-944A-2C02F4C1D73A";

        /// <summary>
        /// The mobile Connection Request List block type (Rock.Blocks.Mobile.Connection).
        /// </summary>
        private const string MobileConnectionRequestListBlockTypeGuid = "117ADAF8-8173-4A88-8C88-2C97F88985DC";

        /// <summary>
        /// The mobile Connection Request Detail block type (Rock.Blocks.Mobile.Connection).
        /// </summary>
        private const string MobileConnectionRequestDetailBlockTypeGuid = "74DDC1A2-2025-4072-8F47-DF7A5A76CF83";

        /// <summary>
        /// The mobile Add Connection Request block type (Rock.Blocks.Mobile.Connection).
        /// </summary>
        private const string MobileAddConnectionRequestBlockTypeGuid = "5A198A75-177C-4A2A-8558-BFB5A4EFCB30";

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets the highest ladder version compiled into this Rock build. This is not the
        /// running Rock version: a Rock release with no definition change adds no entry.
        /// </summary>
        public static Version CurrentDefinitionVersion => Ladder.Max( e => e.Version );

        /// <summary>
        /// Gets the ladder versions in order. Exposed for the ladder ordering test.
        /// </summary>
        internal static IReadOnlyList<Version> LadderVersions => Ladder.Select( e => e.Version ).ToList();

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Gets the version stamped on the platform Site, the last ladder version that
        /// completed.
        /// </summary>
        /// <param name="rockContext">The Rock context to use.</param>
        /// <returns>The stamped version, <c>0.0</c> if the Site exists without a stamp, or <c>null</c> if the Site does not exist.</returns>
        public static Version GetStampVersion( RockContext rockContext )
        {
            var site = new SiteService( rockContext ).Get( PlatformSiteGuid );

            if ( site == null )
            {
                return null;
            }

            return ReadStamp( site, rockContext );
        }

        /// <summary>
        /// Gets the state of the platform mobile application compared with this build's ladder,
        /// which decides the one action the control panel offers.
        /// </summary>
        /// <param name="rockContext">The Rock context to use.</param>
        /// <returns>The current build state.</returns>
        public static PlatformMobileAppBuildState GetState( RockContext rockContext )
        {
            var stamp = GetStampVersion( rockContext );

            if ( stamp == null )
            {
                return PlatformMobileAppBuildState.NotBuilt;
            }

            if ( stamp < CurrentDefinitionVersion )
            {
                return PlatformMobileAppBuildState.UpdateAvailable;
            }

            if ( stamp > CurrentDefinitionVersion )
            {
                return PlatformMobileAppBuildState.AheadOfCode;
            }

            return PlatformMobileAppBuildState.Current;
        }

        /// <summary>
        /// Gets the report of the last run, as stored on the platform Site.
        /// </summary>
        /// <param name="rockContext">The Rock context to use.</param>
        /// <returns>The last run report, or <c>null</c> if there is none.</returns>
        public static PlatformMobileAppRunReport GetLastRunReport( RockContext rockContext )
        {
            var site = new SiteService( rockContext ).Get( PlatformSiteGuid );

            return site?.GetMetadataValue( MetadataKey.PlatformMobileAppLastRun, rockContext )
                .FromJsonOrNull<PlatformMobileAppRunReport>();
        }

        /// <summary>
        /// Runs the builder: Build when the platform Site does not exist, Update when the stamp
        /// is behind the ladder, and Repair when it is current. Each ladder version runs in its
        /// own transaction and stamps the Site as it completes, so a failure leaves the earlier
        /// versions committed and the next run resumes at the failed one. After a successful
        /// run the application is deployed.
        /// </summary>
        /// <returns>The report of what the run did.</returns>
        /// <exception cref="InvalidOperationException">The database is ahead of this Rock's ladder.</exception>
        public static async Task<PlatformMobileAppRunReport> RunAsync()
        {
            PlatformMobileAppBuildState state;
            Version stamp;

            using ( var rockContext = new RockContext() )
            {
                state = GetState( rockContext );
                stamp = GetStampVersion( rockContext ) ?? new Version( 0, 0 );
            }

            if ( state == PlatformMobileAppBuildState.AheadOfCode )
            {
                throw new InvalidOperationException( $"The platform mobile application is at version {stamp}, which is newer than this Rock's builder ({CurrentDefinitionVersion}). Run the Rock version the database came from." );
            }

            var report = new PlatformMobileAppRunReport
            {
                Mode = GetRunMode( state ),
                StartedDateTime = RockDateTime.Now
            };

            // Update runs only the versions above the stamp. Build and Repair run them all.
            var entries = report.Mode == PlatformMobileAppRunMode.Update
                ? Ladder.Where( e => e.Version > stamp ).ToList()
                : Ladder.ToList();

            VerifyBuilderBlockTypes();

            foreach ( var entry in entries )
            {
                var versionReport = new PlatformMobileAppVersionReport
                {
                    Version = entry.Version.ToString()
                };

                report.Versions.Add( versionReport );

                if ( !TryRunVersion( entry, versionReport ) )
                {
                    break;
                }
            }

            int? siteId;

            using ( var rockContext = new RockContext() )
            {
                siteId = new SiteService( rockContext ).GetId( PlatformSiteGuid );

                if ( siteId.HasValue )
                {
                    report.UnexpectedRecords = FindUnexpectedRecords( siteId.Value, rockContext );
                    FlushSiteCaches( siteId.Value, rockContext );
                }
            }

            if ( report.IsSuccess && siteId.HasValue )
            {
                await DeployAsync( siteId.Value, report );
            }

            report.CompletedDateTime = RockDateTime.Now;
            SaveLastRunReport( report );

            return report;
        }

        #endregion Public Methods

        #region Run Machinery

        /// <summary>
        /// Maps the build state to the kind of run the builder performs.
        /// </summary>
        /// <param name="state">The current build state.</param>
        /// <returns>The run mode.</returns>
        private static PlatformMobileAppRunMode GetRunMode( PlatformMobileAppBuildState state )
        {
            switch ( state )
            {
                case PlatformMobileAppBuildState.NotBuilt:
                    return PlatformMobileAppRunMode.Build;

                case PlatformMobileAppBuildState.UpdateAvailable:
                    return PlatformMobileAppRunMode.Update;

                default:
                    return PlatformMobileAppRunMode.Repair;
            }
        }

        /// <summary>
        /// Runs one ladder version in its own transaction and stamps the Site inside it.
        /// </summary>
        /// <param name="entry">The ladder entry to run.</param>
        /// <param name="versionReport">The report entry to fill in.</param>
        /// <returns><c>true</c> if the version completed; <c>false</c> if it failed and was rolled back.</returns>
        private static bool TryRunVersion( LadderEntry entry, PlatformMobileAppVersionReport versionReport )
        {
            /*
                9/29/2026 - CLAUDE

                Each version gets a fresh RockContext rather than sharing one across the
                loop. A version that throws is rolled back by WrapTransaction, but the
                entities it touched stay tracked, and the next version must not see them.

                Reason: A failed version must leave no tracked state behind.
            */
            try
            {
                using ( var rockContext = new RockContext() )
                {
                    rockContext.WrapTransaction( () =>
                    {
                        entry.Run( new BuilderContext( rockContext, versionReport ) );

                        var site = new SiteService( rockContext ).Get( PlatformSiteGuid );

                        if ( site == null )
                        {
                            throw new InvalidOperationException( $"Version {entry.Version} completed but the platform Site does not exist." );
                        }

                        site.SaveMetadataValue( MetadataKey.PlatformMobileAppVersion, entry.Version.ToString(), rockContext );
                    } );
                }

                return true;
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );

                // The transaction was rolled back, so nothing it recorded was kept.
                versionReport.Created.Clear();
                versionReport.Updated.Clear();
                versionReport.Error = ex.Message;

                return false;
            }
        }

        /// <summary>
        /// Makes sure every block type the builder places has its attributes created. Runs
        /// before the ladder and outside its transactions, because Rock creates block type
        /// attributes with its own context.
        /// </summary>
        private static void VerifyBuilderBlockTypes()
        {
            var blockTypeIds = BuilderBlockTypeGuids
                .Select( guid => BlockTypeCache.GetId( guid.AsGuid() ) )
                .Where( id => id.HasValue )
                .Select( id => id.Value )
                .ToArray();

            BlockTypeService.VerifyBlockTypeInstanceProperties( blockTypeIds );
        }

        /// <summary>
        /// Flushes the cached pages and blocks of the platform Site after a run, so the deploy
        /// and the next launch see the settings the run wrote. Block settings are saved as
        /// attribute values, which do not flush the block cache on their own.
        /// </summary>
        /// <param name="siteId">The platform Site identifier.</param>
        /// <param name="rockContext">The Rock context to use.</param>
        private static void FlushSiteCaches( int siteId, RockContext rockContext )
        {
            var pageIds = new PageService( rockContext ).Queryable()
                .Where( p => p.Layout.SiteId == siteId )
                .Select( p => p.Id )
                .ToList();

            var blockIds = new BlockService( rockContext ).Queryable()
                .Where( b => b.PageId.HasValue && pageIds.Contains( b.PageId.Value ) )
                .Select( b => b.Id )
                .ToList();

            foreach ( var blockId in blockIds )
            {
                BlockCache.Remove( blockId );
            }

            foreach ( var pageId in pageIds )
            {
                PageCache.FlushPage( pageId );
            }
        }

        /// <summary>
        /// Deploys the application so it is immediately serveable, recording any failure on
        /// the report instead of throwing.
        /// </summary>
        /// <param name="siteId">The platform Site identifier.</param>
        /// <param name="report">The run report.</param>
        private static async Task DeployAsync( int siteId, PlatformMobileAppRunReport report )
        {
            try
            {
                using ( var rockContext = new RockContext() )
                {
                    await new SiteService( rockContext ).BuildMobileApplicationAsync( siteId );
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                report.DeployError = ex.Message;
            }
        }

        /// <summary>
        /// Stores the run report on the platform Site. Skipped when the Site does not exist,
        /// which only happens when the very first version of a Build failed.
        /// </summary>
        /// <param name="report">The run report.</param>
        private static void SaveLastRunReport( PlatformMobileAppRunReport report )
        {
            using ( var rockContext = new RockContext() )
            {
                var site = new SiteService( rockContext ).Get( PlatformSiteGuid );

                site?.SaveMetadataValue( MetadataKey.PlatformMobileAppLastRun, report.ToJson(), rockContext );
            }
        }

        /// <summary>
        /// Reads the stamp from the Site. A missing or unreadable stamp reads as <c>0.0</c>.
        /// </summary>
        /// <param name="site">The platform Site.</param>
        /// <param name="rockContext">The Rock context to use.</param>
        /// <returns>The stamped version.</returns>
        private static Version ReadStamp( Site site, RockContext rockContext )
        {
            var value = site.GetMetadataValue( MetadataKey.PlatformMobileAppVersion, rockContext );

            return Version.TryParse( value, out var version ) ? version : new Version( 0, 0 );
        }

        /// <summary>
        /// Lists the layouts, pages and blocks on the platform Site whose Guids are not in the
        /// registry. They are reported, never deleted.
        /// </summary>
        /// <param name="siteId">The platform Site identifier.</param>
        /// <param name="rockContext">The Rock context to use.</param>
        /// <returns>A description of each unexpected record.</returns>
        private static List<string> FindUnexpectedRecords( int siteId, RockContext rockContext )
        {
            var knownGuids = GetRegistryGuids();
            var unexpected = new List<string>();

            var layouts = new LayoutService( rockContext ).Queryable()
                .Where( l => l.SiteId == siteId )
                .Select( l => new { l.Id, l.Guid, l.Name } )
                .ToList();
            var layoutIds = layouts.Select( l => l.Id ).ToList();

            var pages = new PageService( rockContext ).Queryable()
                .Where( p => p.Layout.SiteId == siteId )
                .Select( p => new { p.Id, p.Guid, p.InternalName } )
                .ToList();
            var pageIds = pages.Select( p => p.Id ).ToList();

            var blocks = new BlockService( rockContext ).Queryable()
                .Where( b => b.SiteId == siteId
                    || ( b.LayoutId.HasValue && layoutIds.Contains( b.LayoutId.Value ) )
                    || ( b.PageId.HasValue && pageIds.Contains( b.PageId.Value ) ) )
                .Select( b => new { b.Guid, b.Name } )
                .ToList();

            unexpected.AddRange( layouts.Where( l => !knownGuids.Contains( l.Guid ) ).Select( l => $"Layout '{l.Name}' ({l.Guid})" ) );
            unexpected.AddRange( pages.Where( p => !knownGuids.Contains( p.Guid ) ).Select( p => $"Page '{p.InternalName}' ({p.Guid})" ) );
            unexpected.AddRange( blocks.Where( b => !knownGuids.Contains( b.Guid ) ).Select( b => $"Block '{b.Name}' ({b.Guid})" ) );

            return unexpected;
        }

        /// <summary>
        /// Gets every Guid declared in <see cref="SystemGuid.PlatformMobileApp"/>.
        /// </summary>
        /// <returns>The set of registry Guids.</returns>
        private static HashSet<Guid> GetRegistryGuids()
        {
            // The registry is plain nested classes of string constants, so reading them by
            // reflection keeps this list from ever drifting from the registry itself.
            var guids = typeof( SystemGuid.PlatformMobileApp )
                .GetNestedTypes()
                .SelectMany( t => t.GetFields( BindingFlags.Public | BindingFlags.Static ) )
                .Where( f => f.IsLiteral && f.FieldType == typeof( string ) )
                .Select( f => ( ( string ) f.GetRawConstantValue() ).AsGuid() );

            return new HashSet<Guid>( guids );
        }

        #endregion Run Machinery

        #region Ladder Versions

        /// <summary>
        /// Rock 21.0: the Site, its two layouts, the home page with its menu, the login page,
        /// the Outreach Toolbox and Connections pages, and the service account with its
        /// seed-once bootstrap API key.
        /// </summary>
        /// <param name="context">The builder context.</param>
        private static void EnsureRock21_0( BuilderContext context )
        {
            var site = EnsureSite( context );

            var homepageLayout = EnsureLayout( context, site, SystemGuid.PlatformMobileApp.Layout.HOMEPAGE, "Homepage", "Homepage.xaml", HomepageLayoutXaml );
            var fullLayout = EnsureLayout( context, site, SystemGuid.PlatformMobileApp.Layout.FULL, "Full", "Full.xaml", FullLayoutXaml );

            var homePage = EnsurePage( context, SystemGuid.PlatformMobileApp.Page.HOME, "Home", homepageLayout, null, 0, DisplayInNavWhen.WhenAllowed );

            var homeMenuBlock = EnsureBlock( context, SystemGuid.PlatformMobileApp.Block.HOME_MENU, "Home Menu", homePage, MobileContentBlockTypeGuid, 0, new Dictionary<string, string>
            {
                ["Content"] = HomeMenuXaml,
                ["DynamicContent"] = "True"
            } );

            // The menu's Lava picks the buttons, so it must render on the server before the XAML reaches the phone.
            EnsureBlockProcessesLavaOnServer( context, homeMenuBlock, "Home Menu" );

            // The login block's header and footer only render outside a ScrollView, so it uses Full.
            var loginPage = EnsurePage( context, SystemGuid.PlatformMobileApp.Page.LOGIN, "Login", fullLayout, homePage, 0, DisplayInNavWhen.Never );
            EnsureBlock( context, SystemGuid.PlatformMobileApp.Block.LOGIN, "Login", loginPage, MobileLoginBlockTypeGuid, 0, null );

            var touchpointPage = EnsureOutreachPages( context, homepageLayout, fullLayout, homePage );
            EnsureConnectionPages( context, homepageLayout, fullLayout, homePage );

            var serviceAccountLogin = EnsureServiceAccount( context, site );

            // Structure: the Site's references to the records it depends on.
            site.DefaultPageId = homePage.Id;
            site.LoginPageId = loginPage.Id;

            var settings = site.AdditionalSettings.FromJsonOrNull<AdditionalSiteSettings>() ?? new AdditionalSiteSettings();
            settings.ShellType = Common.Mobile.Enums.ShellType.Tabbed;
            settings.ApiKeyId = serviceAccountLogin.Id;
            settings.IsPackageCompressionEnabled = true;
            settings.OutreachToolboxTouchpointPageId = touchpointPage.Id;
            EnsureMobileStyleSettings( settings );
            SetAdditionalSettingsIfChanged( site, settings );

            context.Save( site, "Site references" );
        }

        /// <summary>
        /// Ensures the Outreach Toolbox pages: the dashboard, the contact list, the contact
        /// profile, add contact, and the touchpoint detail page. Lists use the Full layout
        /// because they scroll themselves; the rest use the scrolling Homepage layout.
        /// </summary>
        /// <param name="context">The builder context.</param>
        /// <param name="homepageLayout">The scrolling layout.</param>
        /// <param name="fullLayout">The non-scrolling layout.</param>
        /// <param name="homePage">The home page, the parent of the dashboard.</param>
        /// <returns>The touchpoint detail page, which push notifications open.</returns>
        private static Page EnsureOutreachPages( BuilderContext context, Layout homepageLayout, Layout fullLayout, Page homePage )
        {
            var outreachPage = EnsurePage( context, SystemGuid.PlatformMobileApp.Page.OUTREACH, "Outreach Toolbox", homepageLayout, homePage, 1, DisplayInNavWhen.WhenAllowed );
            var myContactsPage = EnsurePage( context, SystemGuid.PlatformMobileApp.Page.MY_CONTACTS, "My Contacts", fullLayout, outreachPage, 0, DisplayInNavWhen.Never );
            var contactProfilePage = EnsurePage( context, SystemGuid.PlatformMobileApp.Page.CONTACT_PROFILE, "Contact Profile", homepageLayout, outreachPage, 1, DisplayInNavWhen.Never );
            var addContactPage = EnsurePage( context, SystemGuid.PlatformMobileApp.Page.ADD_CONTACT, "Add Contact", homepageLayout, outreachPage, 2, DisplayInNavWhen.Never );
            var touchpointPage = EnsurePage( context, SystemGuid.PlatformMobileApp.Page.TOUCHPOINT_DETAIL, "Touchpoint", homepageLayout, outreachPage, 3, DisplayInNavWhen.Never );

            EnsureBlock( context, SystemGuid.PlatformMobileApp.Block.OUTREACH_DASHBOARD, "Outreach Dashboard", outreachPage, SystemGuid.BlockType.MOBILE_OUTREACH_OUTREACH_BEACON_DASHBOARD, 0, new Dictionary<string, string>
            {
                ["DetailPage"] = touchpointPage.Guid.ToString(),
                ["MyContact"] = myContactsPage.Guid.ToString(),
                ["AddContact"] = addContactPage.Guid.ToString()
            } );

            EnsureBlock( context, SystemGuid.PlatformMobileApp.Block.MY_CONTACTS, "My Contacts", myContactsPage, SystemGuid.BlockType.MOBILE_OUTREACH_MY_CONTACTS, 0, new Dictionary<string, string>
            {
                ["AddContact"] = addContactPage.Guid.ToString(),
                ["ContactProfile"] = contactProfilePage.Guid.ToString()
            } );

            EnsureBlock( context, SystemGuid.PlatformMobileApp.Block.CONTACT_PROFILE, "Contact Profile", contactProfilePage, SystemGuid.BlockType.MOBILE_OUTREACH_CONTACT_PROFILE, 0, null );
            EnsureBlock( context, SystemGuid.PlatformMobileApp.Block.ADD_CONTACT, "Add Contact", addContactPage, SystemGuid.BlockType.MOBILE_OUTREACH_ADD_CONTACT, 0, null );
            EnsureBlock( context, SystemGuid.PlatformMobileApp.Block.TOUCHPOINT_DETAIL, "Touchpoint Detail", touchpointPage, SystemGuid.BlockType.MOBILE_OUTREACH_TOUCHPOINT_DETAIL, 0, null );

            return touchpointPage;
        }

        /// <summary>
        /// Ensures the Connections pages: connection types, then opportunities, then
        /// requests, then a request's detail, plus the add request page every list links to.
        /// </summary>
        /// <param name="context">The builder context.</param>
        /// <param name="homepageLayout">The scrolling layout.</param>
        /// <param name="fullLayout">The non-scrolling layout.</param>
        /// <param name="homePage">The home page, the parent of the connection types page.</param>
        private static void EnsureConnectionPages( BuilderContext context, Layout homepageLayout, Layout fullLayout, Page homePage )
        {
            var connectionsPage = EnsurePage( context, SystemGuid.PlatformMobileApp.Page.CONNECTIONS, "Connections", fullLayout, homePage, 2, DisplayInNavWhen.WhenAllowed );
            var opportunitiesPage = EnsurePage( context, SystemGuid.PlatformMobileApp.Page.CONNECTION_OPPORTUNITIES, "Connection Opportunities", fullLayout, connectionsPage, 0, DisplayInNavWhen.Never );
            var requestsPage = EnsurePage( context, SystemGuid.PlatformMobileApp.Page.CONNECTION_REQUESTS, "Connection Requests", fullLayout, connectionsPage, 1, DisplayInNavWhen.Never );
            var requestDetailPage = EnsurePage( context, SystemGuid.PlatformMobileApp.Page.CONNECTION_REQUEST_DETAIL, "Connection Request", homepageLayout, connectionsPage, 2, DisplayInNavWhen.Never );
            var addRequestPage = EnsurePage( context, SystemGuid.PlatformMobileApp.Page.ADD_CONNECTION_REQUEST, "Add Connection Request", homepageLayout, connectionsPage, 3, DisplayInNavWhen.Never );

            EnsureBlock( context, SystemGuid.PlatformMobileApp.Block.CONNECTION_TYPE_LIST, "Connection Type List", connectionsPage, MobileConnectionTypeListBlockTypeGuid, 0, new Dictionary<string, string>
            {
                ["DetailPage"] = opportunitiesPage.Guid.ToString(),
                ["AddPage"] = addRequestPage.Guid.ToString()
            } );

            EnsureBlock( context, SystemGuid.PlatformMobileApp.Block.CONNECTION_OPPORTUNITY_LIST, "Connection Opportunity List", opportunitiesPage, MobileConnectionOpportunityListBlockTypeGuid, 0, new Dictionary<string, string>
            {
                ["DetailPage"] = requestsPage.Guid.ToString(),
                ["AddPage"] = addRequestPage.Guid.ToString()
            } );

            EnsureBlock( context, SystemGuid.PlatformMobileApp.Block.CONNECTION_REQUEST_LIST, "Connection Request List", requestsPage, MobileConnectionRequestListBlockTypeGuid, 0, new Dictionary<string, string>
            {
                ["DetailPage"] = requestDetailPage.Guid.ToString(),
                ["AddPage"] = addRequestPage.Guid.ToString()
            } );

            EnsureBlock( context, SystemGuid.PlatformMobileApp.Block.CONNECTION_REQUEST_DETAIL, "Connection Request Detail", requestDetailPage, MobileConnectionRequestDetailBlockTypeGuid, 0, null );
            EnsureBlock( context, SystemGuid.PlatformMobileApp.Block.ADD_CONNECTION_REQUEST, "Add Connection Request", addRequestPage, MobileAddConnectionRequestBlockTypeGuid, 0, null );
        }

        #endregion Ladder Versions

        #region Ensure Steps

        /// <summary>
        /// Gets or creates the platform Site and asserts its platform owned fields.
        /// </summary>
        /// <param name="context">The builder context.</param>
        /// <returns>The platform Site.</returns>
        private static Site EnsureSite( BuilderContext context )
        {
            var siteService = new SiteService( context.RockContext );
            var site = siteService.Get( PlatformSiteGuid );

            if ( site == null )
            {
                site = new Site
                {
                    Guid = PlatformSiteGuid,
                    AdditionalSettings = new AdditionalSiteSettings
                    {
                        IsPackageCompressionEnabled = true,
                        DownhillSettings = new DownhillCss.DownhillSettings
                        {
                            Platform = DownhillCss.DownhillPlatform.Mobile
                        }
                    }.ToJson()
                };

                siteService.Add( site );
            }

            site.Name = SiteName;
            site.Description = string.Empty;
            site.SiteType = SiteType.Mobile;
            site.IsActive = true;
            site.IsSystem = true;

            context.Save( site, $"Site '{SiteName}'" );

            return site;
        }

        /// <summary>
        /// Gets or creates a layout on the platform Site and asserts its platform owned fields.
        /// </summary>
        /// <param name="context">The builder context.</param>
        /// <param name="site">The platform Site.</param>
        /// <param name="guid">The layout's registry Guid.</param>
        /// <param name="name">The layout name.</param>
        /// <param name="fileName">The layout file name.</param>
        /// <param name="xaml">The XAML used for both phone and tablet.</param>
        /// <returns>The layout.</returns>
        private static Layout EnsureLayout( BuilderContext context, Site site, string guid, string name, string fileName, string xaml )
        {
            var layoutService = new LayoutService( context.RockContext );
            var layout = layoutService.Get( guid.AsGuid() );

            if ( layout == null )
            {
                layout = new Layout
                {
                    Guid = guid.AsGuid()
                };

                layoutService.Add( layout );
            }

            layout.SiteId = site.Id;
            layout.Name = name;
            layout.FileName = fileName;
            layout.Description = string.Empty;
            layout.LayoutMobilePhone = xaml;
            layout.LayoutMobileTablet = xaml;

            context.Save( layout, $"Layout '{name}'" );

            return layout;
        }

        /// <summary>
        /// Gets or creates a page and asserts its platform owned fields and structure.
        /// </summary>
        /// <param name="context">The builder context.</param>
        /// <param name="guid">The page's registry Guid.</param>
        /// <param name="name">The page name, used for the internal name and titles.</param>
        /// <param name="layout">The layout the page uses.</param>
        /// <param name="parentPage">The parent page, or <c>null</c> for a top-level page.</param>
        /// <param name="order">The page's order among its siblings.</param>
        /// <param name="displayInNavWhen">When the page shows in the app's navigation.</param>
        /// <returns>The page.</returns>
        private static Page EnsurePage( BuilderContext context, string guid, string name, Layout layout, Page parentPage, int order, DisplayInNavWhen displayInNavWhen )
        {
            var pageService = new PageService( context.RockContext );
            var page = pageService.Get( guid.AsGuid() );

            if ( page == null )
            {
                page = new Page
                {
                    Guid = guid.AsGuid()
                };

                pageService.Add( page );
            }

            page.InternalName = name;
            page.PageTitle = name;
            page.BrowserTitle = name;
            page.Description = string.Empty;
            page.LayoutId = layout.Id;
            page.ParentPageId = parentPage?.Id;
            page.Order = order;
            page.DisplayInNavWhen = displayInNavWhen;

            context.Save( page, $"Page '{name}'" );

            return page;
        }

        /// <summary>
        /// Gets or creates a block in the Main zone of a page and asserts its platform owned
        /// fields and settings. Only the settings passed in are written; the block's other
        /// settings keep their defaults or whatever the church picked.
        /// </summary>
        /// <param name="context">The builder context.</param>
        /// <param name="guid">The block's registry Guid.</param>
        /// <param name="name">The block name.</param>
        /// <param name="page">The page the block is on.</param>
        /// <param name="blockTypeGuid">The Guid of the block's block type.</param>
        /// <param name="order">The block's order in the zone.</param>
        /// <param name="attributeValues">The platform owned settings by attribute key, or <c>null</c>.</param>
        /// <returns>The block.</returns>
        private static Block EnsureBlock( BuilderContext context, string guid, string name, Page page, string blockTypeGuid, int order, Dictionary<string, string> attributeValues )
        {
            var rockContext = context.RockContext;
            var blockService = new BlockService( rockContext );
            var blockTypeId = BlockTypeCache.GetId( blockTypeGuid.AsGuid() )
                ?? throw new InvalidOperationException( $"The block type for block '{name}' ({blockTypeGuid}) was not found." );

            var block = blockService.Get( guid.AsGuid() );

            if ( block == null )
            {
                block = new Block
                {
                    Guid = guid.AsGuid()
                };

                blockService.Add( block );
            }

            block.PageId = page.Id;
            block.LayoutId = null;
            block.SiteId = null;
            block.BlockTypeId = blockTypeId;
            block.Zone = MainZone;
            block.Order = order;
            block.Name = name;

            context.Save( block, $"Block '{name}'" );

            if ( attributeValues == null || !attributeValues.Any() )
            {
                return block;
            }

            block.LoadAttributes( rockContext );

            var isChanged = false;

            foreach ( var attributeValue in attributeValues )
            {
                // A missing attribute means the block type changed; fail loudly rather than skip the setting.
                if ( !block.Attributes.ContainsKey( attributeValue.Key ) )
                {
                    throw new InvalidOperationException( $"Block '{name}' has no setting '{attributeValue.Key}'." );
                }

                if ( block.GetAttributeValue( attributeValue.Key ) != attributeValue.Value )
                {
                    block.SetAttributeValue( attributeValue.Key, attributeValue.Value );
                    isChanged = true;
                }
            }

            if ( isChanged )
            {
                block.SaveAttributeValues( rockContext );
                context.RecordUpdated( $"Block '{name}' settings" );
            }

            return block;
        }

        /// <summary>
        /// Gets or creates the service account behind the bootstrap API key. The key is
        /// seed-once: it is generated only when the login is created and never overwritten,
        /// because every connected phone holds it.
        /// </summary>
        /// <param name="context">The builder context.</param>
        /// <param name="site">The platform Site.</param>
        /// <returns>The service account's user login.</returns>
        private static UserLogin EnsureServiceAccount( BuilderContext context, Site site )
        {
            var rockContext = context.RockContext;
            var personService = new PersonService( rockContext );
            var userLoginService = new UserLoginService( rockContext );
            var personGuid = SystemGuid.PlatformMobileApp.Security.SERVICE_ACCOUNT_PERSON.AsGuid();
            var loginGuid = SystemGuid.PlatformMobileApp.Security.SERVICE_ACCOUNT_LOGIN.AsGuid();

            var person = personService.Get( personGuid );

            if ( person == null )
            {
                person = new Person
                {
                    Guid = personGuid
                };

                personService.Add( person );
            }

            /*
                9/29/2026 - CLAUDE

                Unlike the service account MobileApplicationDetail creates for an ordinary
                app, this one is deliberately not added to the Mobile Application Users
                group. The bootstrap key is handed to anyone who installs the shared app and
                picks this church, and the [Secured] endpoint fallback grants the key's
                person whatever that person can do, so it must hold no roles at all.

                Reason: The shared app's bootstrap key must grant nothing.
            */
            person.LastName = SiteName;
            person.RecordTypeValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_TYPE_RESTUSER.AsGuid() ).Id;
            person.RecordStatusValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;

            context.Save( person, "Service account person" );

            var userLogin = userLoginService.Get( loginGuid );

            if ( userLogin == null )
            {
                userLogin = new UserLogin
                {
                    Guid = loginGuid,
                    UserName = $"mobile_application_{site.Id}",
                    ApiKey = GenerateUniqueApiKey()
                };

                userLoginService.Add( userLogin );
            }

            // Never write an empty key: a blank key matches a request that sends no key.
            if ( userLogin.ApiKey.IsNullOrWhiteSpace() )
            {
                throw new InvalidOperationException( "The platform mobile application's service account has no API key." );
            }

            userLogin.PersonId = person.Id;
            userLogin.IsConfirmed = true;
            userLogin.EntityTypeId = EntityTypeCache.GetId( "Rock.Security.Authentication.Database" )
                ?? throw new InvalidOperationException( "The database authentication entity type was not found." );

            context.Save( userLogin, "Service account login" );

            return userLogin;
        }

        /// <summary>
        /// Turns on the block's "Process Lava on Server" mobile setting. Mobile content blocks
        /// only render their Lava on the server when this is on; otherwise the raw Lava tags
        /// reach the phone as text, which the XAML parser rejects.
        /// </summary>
        /// <param name="context">The builder context.</param>
        /// <param name="block">The block.</param>
        /// <param name="name">The block name, used in the report.</param>
        private static void EnsureBlockProcessesLavaOnServer( BuilderContext context, Block block, string name )
        {
            var settings = block.AdditionalSettings.FromJsonOrNull<AdditionalBlockSettings>() ?? new AdditionalBlockSettings();

            settings.ProcessLavaOnServer = true;

            var json = settings.ToJson();

            if ( json != block.AdditionalSettings )
            {
                block.AdditionalSettings = json;
                context.Save( block, $"Block '{name}'" );
            }
        }

        /*
            9/29/2026 - CLAUDE

            AdditionalSiteSettings starts with DownhillSettings for the Web platform, and
            FontSizeDefault is lazily computed from the platform and then persisted. Saved
            while still Web, it stores 1 (rem). The package build flips the platform to
            Mobile, but the stored 1 is kept, so every "?font-size-default" in the mobile
            CSS becomes 1. MobileApplicationDetail avoids this by flagging the settings
            Mobile on save; the builder has to do the same.

            Reason: A web flagged style setting gave the platform app a 1pt base font.
        */

        /// <summary>
        /// Flags the style settings for the Mobile platform and replaces a web base font
        /// size with the mobile default. A base font below 2 is only ever the web rem value,
        /// never a real mobile size, so a church chosen size is left alone.
        /// </summary>
        /// <param name="settings">The Site's additional settings.</param>
        private static void EnsureMobileStyleSettings( AdditionalSiteSettings settings )
        {
            if ( settings.DownhillSettings == null )
            {
                settings.DownhillSettings = new DownhillCss.DownhillSettings();
            }

            settings.DownhillSettings.Platform = DownhillCss.DownhillPlatform.Mobile;

            if ( settings.DownhillSettings.FontSizeDefault < 2 )
            {
                settings.DownhillSettings.FontSizeDefault = 16;
            }
        }

        /// <summary>
        /// Writes the settings back to the Site only if their JSON changed, so a healthy
        /// Repair reports nothing.
        /// </summary>
        /// <param name="site">The platform Site.</param>
        /// <param name="settings">The settings, already asserted field by field.</param>
        private static void SetAdditionalSettingsIfChanged( Site site, AdditionalSiteSettings settings )
        {
            var json = settings.ToJson();

            if ( json != site.AdditionalSettings )
            {
                site.AdditionalSettings = json;
            }
        }

        /// <summary>
        /// Generates a fresh API key that no user login already uses.
        /// </summary>
        /// <returns>The new key.</returns>
        private static string GenerateUniqueApiKey()
        {
            return KeyHelper.GenerateKey( ( RockContext rockContext, string key )
                => new UserLoginService( rockContext ).Queryable().Any( a => a.ApiKey == key ) );
        }

        #endregion Ensure Steps

        #region Support Classes

        /// <summary>
        /// One rung of the builder ladder.
        /// </summary>
        private sealed class LadderEntry
        {
            /// <summary>
            /// Gets the Rock version this entry ships in.
            /// </summary>
            public Version Version { get; }

            /// <summary>
            /// Gets the step that brings the definition up to this version.
            /// </summary>
            public Action<BuilderContext> Run { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="LadderEntry"/> class.
            /// </summary>
            /// <param name="version">The Rock version this entry ships in.</param>
            /// <param name="run">The step.</param>
            public LadderEntry( Version version, Action<BuilderContext> run )
            {
                Version = version;
                Run = run;
            }
        }

        /// <summary>
        /// The state one ladder version runs with: its Rock context and the report it fills in.
        /// </summary>
        private sealed class BuilderContext
        {
            /// <summary>
            /// Gets the Rock context, inside the version's transaction.
            /// </summary>
            public RockContext RockContext { get; }

            /// <summary>
            /// Gets the report entry for this version.
            /// </summary>
            public PlatformMobileAppVersionReport Report { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="BuilderContext"/> class.
            /// </summary>
            /// <param name="rockContext">The Rock context.</param>
            /// <param name="report">The report entry.</param>
            public BuilderContext( RockContext rockContext, PlatformMobileAppVersionReport report )
            {
                RockContext = rockContext;
                Report = report;
            }

            /// <summary>
            /// Records whether the entity is being created or changed, then saves. An entity
            /// whose asserted fields already held the same values is unchanged, so EF writes
            /// nothing and nothing is recorded.
            /// </summary>
            /// <param name="entity">The entity being saved.</param>
            /// <param name="description">How the entity is described in the report.</param>
            public void Save( object entity, string description )
            {
                var state = RockContext.Entry( entity ).State;

                if ( state == System.Data.Entity.EntityState.Added && !Report.Created.Contains( description ) )
                {
                    Report.Created.Add( description );
                }
                else if ( state == System.Data.Entity.EntityState.Modified && !Report.Updated.Contains( description ) && !Report.Created.Contains( description ) )
                {
                    Report.Updated.Add( description );
                }

                RockContext.SaveChanges();
            }

            /// <summary>
            /// Records a change that was saved outside <see cref="Save"/>, such as block
            /// settings, which are stored as attribute values.
            /// </summary>
            /// <param name="description">How the change is described in the report.</param>
            public void RecordUpdated( string description )
            {
                if ( !Report.Updated.Contains( description ) && !Report.Created.Contains( description ) )
                {
                    Report.Updated.Add( description );
                }
            }
        }

        #endregion Support Classes
    }
}
