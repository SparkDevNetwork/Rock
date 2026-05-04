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

using System.Collections.Generic;

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Mobile.MobileApplicationDetail
{
    /// <summary>
    /// The bag that represents a mobile application's basic settings (the
    /// "Application" tab) for the Mobile Application Detail block.
    /// </summary>
    public class MobileApplicationBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the friendly name of the mobile application.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the mobile application is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the description of the mobile application.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the application shell type (e.g. Flyout, Tabbed, Blank).
        /// </summary>
        public int ApplicationType { get; set; }

        /// <summary>
        /// Gets or sets the location of the tab bar on Android applications when
        /// the application uses the tabbed shell.
        /// </summary>
        public int AndroidTabLocation { get; set; }

        /// <summary>
        /// Gets or sets the orientation that phones running this application
        /// should be locked to.
        /// </summary>
        public int LockPhoneOrientation { get; set; }

        /// <summary>
        /// Gets or sets the orientation that tablets running this application
        /// should be locked to.
        /// </summary>
        public int LockTabletOrientation { get; set; }

        /// <summary>
        /// Gets or sets the page that will be shown when the user is required
        /// to log in.
        /// </summary>
        public ListItemBag LoginPage { get; set; }

        /// <summary>
        /// Gets or sets the page that will be shown when the user views their
        /// profile.
        /// </summary>
        public ListItemBag ProfilePage { get; set; }

        /// <summary>
        /// Gets or sets the page that contains the Live Experience block when
        /// using interactive experiences.
        /// </summary>
        public ListItemBag InteractiveExperiencePage { get; set; }

        /// <summary>
        /// Gets or sets the page that displays an individual communication.
        /// </summary>
        public ListItemBag CommunicationViewPage { get; set; }

        /// <summary>
        /// Gets or sets the page that contains the SMS Conversation block.
        /// </summary>
        public ListItemBag SmsConversationPage { get; set; }

        /// <summary>
        /// Gets or sets the page that displays the chat interface.
        /// </summary>
        public ListItemBag ChatPage { get; set; }

        /// <summary>
        /// Gets or sets the page used by the Outreach Toolbox for Touchpoint
        /// notifications. Tapping a Touchpoint notification opens this page.
        /// </summary>
        public ListItemBag OutreachToolboxTouchpointPage { get; set; }

        /// <summary>
        /// Gets or sets the API key associated with the mobile application's
        /// REST user. New applications generate a new key automatically.
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Gets or sets the data view of campuses to use for the campus lists
        /// within the application.
        /// </summary>
        public ListItemBag CampusFilterDataView { get; set; }

        /// <summary>
        /// Gets or sets the categories of person attributes that will be sent
        /// to the client and made available remotely.
        /// </summary>
        public List<ListItemBag> PersonAttributeCategories { get; set; }

        /// <summary>
        /// Gets or sets the Auth0 client ID for Auth0-based login.
        /// </summary>
        public string Auth0ClientId { get; set; }

        /// <summary>
        /// Gets or sets the Auth0 domain for Auth0-based login.
        /// </summary>
        public string Auth0Domain { get; set; }

        /// <summary>
        /// Gets or sets the connection status used when an Auth0 login creates
        /// a new person.
        /// </summary>
        public ListItemBag Auth0ConnectionStatus { get; set; }

        /// <summary>
        /// Gets or sets the record status used when an Auth0 login creates a
        /// new person.
        /// </summary>
        public ListItemBag Auth0RecordStatus { get; set; }

        /// <summary>
        /// Gets or sets the Microsoft Entra (Azure AD) client ID for
        /// authentication support.
        /// </summary>
        public string EntraClientId { get; set; }

        /// <summary>
        /// Gets or sets the Microsoft Entra (Azure AD) tenant ID for
        /// authentication support.
        /// </summary>
        public string EntraTenantId { get; set; }

        /// <summary>
        /// Gets or sets the authentication component used for web-based Entra
        /// authentication. Typically supplied by a plugin.
        /// </summary>
        public ListItemBag EntraAuthenticationComponent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the application should
        /// automatically request push notification permission at launch.
        /// </summary>
        public bool EnableNotificationsAutomatically { get; set; }

        /// <summary>
        /// Gets or sets the XAML template used for the menu in the Flyout
        /// Shell.
        /// </summary>
        public string FlyoutXaml { get; set; }

        /// <summary>
        /// Gets or sets the XAML template used to place content into the top
        /// navigation bar.
        /// </summary>
        public string NavigationBarActionXaml { get; set; }

        /// <summary>
        /// Gets or sets the Lava that is executed at application start to
        /// determine which page should open initially.
        /// </summary>
        public string HomepageRoutingLogic { get; set; }

        /// <summary>
        /// Gets or sets the preview thumbnail used by Rock to distinguish the
        /// application.
        /// </summary>
        public ListItemBag PreviewThumbnailBinaryFile { get; set; }

        /// <summary>
        /// Gets or sets a value used to force all clients to update their push
        /// token. Setting or changing this value triggers a refresh.
        /// </summary>
        public string PushTokenUpdateValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether update packages should be
        /// compressed to reduce their download size.
        /// </summary>
        public bool CompressUpdatePackages { get; set; }

        /// <summary>
        /// Gets or sets the number of days that page views should be retained
        /// for. Null means retain indefinitely.
        /// </summary>
        public int? PageViewRetentionPeriodDays { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether deep linking is enabled for
        /// this application.
        /// </summary>
        public bool IsDeepLinkingEnabled { get; set; }

        /// <summary>
        /// Gets or sets the URL path prefix that signals a URL should be
        /// opened in the application instead of the browser.
        /// </summary>
        public string DeepLinkPathPrefix { get; set; }

        /// <summary>
        /// Gets or sets the pipe-delimited list of domains accepted for deep
        /// linking. The format matches the persisted shape used by the mobile
        /// shell.
        /// </summary>
        public string DeepLinkDomains { get; set; }

        /// <summary>
        /// Gets or sets the iOS bundle identifier supplied by the shell
        /// hosting service. Required when deep linking is enabled.
        /// </summary>
        public string BundleIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the iOS team identifier. Required when deep linking is
        /// enabled.
        /// </summary>
        public string TeamIdentifier { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the deep link path prefix
        /// is locked from further edits. Locking applies once an application
        /// has been saved with deep linking enabled — changing the prefix at
        /// that point would invalidate every previously-deployed deep link
        /// URL on user devices.
        /// </summary>
        public bool IsDeepLinkPrefixLocked { get; set; }

        /// <summary>
        /// Gets or sets the Android package name supplied by the shell hosting
        /// service. Required when deep linking is enabled.
        /// </summary>
        public string PackageName { get; set; }

        /// <summary>
        /// Gets or sets the Android certificate fingerprint supplied by the
        /// shell hosting service. Required when deep linking is enabled.
        /// </summary>
        public string CertificateFingerprint { get; set; }
    }
}
