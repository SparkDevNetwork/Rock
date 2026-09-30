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
namespace Rock.SystemGuid
{
    /*
        9/29/2026 - CLAUDE

        Every record the platform mobile app builder owns, in one place, so the set can be
        audited at a glance. Core normally keeps Guids in per-entity classes (SystemGuid.Page,
        SystemGuid.Block); this registry is deliberately one class instead, because a shipped
        Guid is frozen forever and every church database holds these exact values. The Site
        Guid itself stays in SystemGuid.Site.PLATFORM_MOBILE_APPLICATION and is not repeated.

        Rules: never change a value once shipped (rename the constant if needed); removing a
        record is a new builder step that deletes by Guid, and its constant stays, marked
        [Obsolete], so that step can still reference it.

        Reason: One auditable registry for the platform mobile app's frozen Guids.
    */

    /// <summary>
    /// The fixed Guids of the records created and maintained by
    /// <see cref="Rock.Mobile.PlatformMobileAppBuilder"/> for the platform mobile application.
    /// They are identical in every Rock database.
    /// </summary>
    public static class PlatformMobileApp
    {
        /// <summary>
        /// Layout Guids for the platform mobile application.
        /// </summary>
        public static class Layout
        {
            /// <summary>
            /// The "Homepage" layout: the Main zone inside a vertical scroll view.
            /// </summary>
            public const string HOMEPAGE = "A5E93A56-075C-46AF-A09D-19B87F00B90E";

            /// <summary>
            /// The "Full" layout: the Main zone with no scroll wrapper, for pages whose
            /// blocks manage their own scrolling.
            /// </summary>
            public const string FULL = "9C2EBBA0-B03E-41CF-8D8B-8E9588776D0B";
        }

        /// <summary>
        /// Page Guids for the platform mobile application.
        /// </summary>
        public static class Page
        {
            /// <summary>
            /// The home page, used as the site's default page.
            /// </summary>
            public const string HOME = "037A6892-30C9-4E69-B53D-FA9C5610FA66";

            /// <summary>
            /// The login page, used as the site's login page.
            /// </summary>
            public const string LOGIN = "80454812-23E1-44C8-B05E-E6A5DA1ED481";

            /// <summary>
            /// The Outreach Toolbox dashboard page.
            /// </summary>
            public const string OUTREACH = "F4E78323-4664-4A35-AA63-502CF07F19CC";

            /// <summary>
            /// The Outreach Toolbox contact list page.
            /// </summary>
            public const string MY_CONTACTS = "4838DB03-BA80-42D8-83CB-9485D423CB9D";

            /// <summary>
            /// The Outreach Toolbox contact profile page.
            /// </summary>
            public const string CONTACT_PROFILE = "6C8ADD29-C4F3-45FD-B32C-9A22DA701C68";

            /// <summary>
            /// The Outreach Toolbox add contact page.
            /// </summary>
            public const string ADD_CONTACT = "7E838C35-DF8C-4661-9A39-1FEE4062B776";

            /// <summary>
            /// The Outreach Toolbox touchpoint detail page, also the site's touchpoint push page.
            /// </summary>
            public const string TOUCHPOINT_DETAIL = "5F76B89E-8C0A-4E73-AFDA-B4215C985A11";

            /// <summary>
            /// The connection type list page.
            /// </summary>
            public const string CONNECTIONS = "DE03E6D2-D18B-474B-941E-360C641AA6AA";

            /// <summary>
            /// The connection opportunity list page.
            /// </summary>
            public const string CONNECTION_OPPORTUNITIES = "D371F9C5-3F46-4D14-AD5C-A29395D68836";

            /// <summary>
            /// The connection request list page.
            /// </summary>
            public const string CONNECTION_REQUESTS = "272E637B-B56D-460B-B4ED-2202415C6655";

            /// <summary>
            /// The connection request detail page.
            /// </summary>
            public const string CONNECTION_REQUEST_DETAIL = "24E24E03-E1A9-47A6-9157-FF607A0EB261";

            /// <summary>
            /// The add connection request page.
            /// </summary>
            public const string ADD_CONNECTION_REQUEST = "0B1A941E-ED0B-491E-B8F7-1A5903DCECEE";
        }

        /// <summary>
        /// Block Guids for the platform mobile application.
        /// </summary>
        public static class Block
        {
            /// <summary>
            /// The content block on the home page with the menu buttons.
            /// </summary>
            public const string HOME_MENU = "C45EE8EC-2A99-46A2-A46D-5E611BA0D00D";

            /// <summary>
            /// The login block on the login page.
            /// </summary>
            public const string LOGIN = "7D1CECA1-0C02-4CD6-BDE3-CE570411D116";

            /// <summary>
            /// The Outreach Dashboard block on the Outreach page.
            /// </summary>
            public const string OUTREACH_DASHBOARD = "4AFB195D-B81A-43BB-97F0-3E09F47668EA";

            /// <summary>
            /// The My Contacts block on the contact list page.
            /// </summary>
            public const string MY_CONTACTS = "B6D76C33-BB40-4AAF-B01D-830D62088C2D";

            /// <summary>
            /// The Contact Profile block on the contact profile page.
            /// </summary>
            public const string CONTACT_PROFILE = "39BCABAB-AC31-4E60-969E-A8E0499D13BE";

            /// <summary>
            /// The Add Contacts block on the add contact page.
            /// </summary>
            public const string ADD_CONTACT = "8EE81EBE-27E1-412F-8596-E9DBDD46C733";

            /// <summary>
            /// The Touchpoint Detail block on the touchpoint detail page.
            /// </summary>
            public const string TOUCHPOINT_DETAIL = "2CFE6C8B-DB3E-41AE-8A9C-E40EE7B315FF";

            /// <summary>
            /// The Connection Type List block on the connections page.
            /// </summary>
            public const string CONNECTION_TYPE_LIST = "6B21EDA5-B093-489B-A1D9-30219F3BB0E5";

            /// <summary>
            /// The Connection Opportunity List block on the connection opportunities page.
            /// </summary>
            public const string CONNECTION_OPPORTUNITY_LIST = "2322FC5D-06CB-4952-BDFF-7A576AECC1FB";

            /// <summary>
            /// The Connection Request List block on the connection requests page.
            /// </summary>
            public const string CONNECTION_REQUEST_LIST = "45CA6D17-0784-4447-A267-79A385B4CD42";

            /// <summary>
            /// The Connection Request Detail block on the connection request detail page.
            /// </summary>
            public const string CONNECTION_REQUEST_DETAIL = "2E26CE9E-E050-4A7C-B776-077F738B4D0D";

            /// <summary>
            /// The Add Connection Request block on the add connection request page.
            /// </summary>
            public const string ADD_CONNECTION_REQUEST = "78CC9E45-9606-4EAF-BCF6-AC1FE8FD29E1";
        }

        /// <summary>
        /// Guids for the platform mobile application's service account, the low-privilege
        /// person behind the bootstrap API key.
        /// </summary>
        public static class Security
        {
            /// <summary>
            /// The service account's REST person.
            /// </summary>
            public const string SERVICE_ACCOUNT_PERSON = "2DD76A8A-FF79-476D-BDA9-A920F12DC9F5";

            /// <summary>
            /// The service account's user login, which holds the bootstrap API key.
            /// </summary>
            public const string SERVICE_ACCOUNT_LOGIN = "113D13D6-E4DB-4C44-AA56-3BE565E856E4";
        }
    }
}
