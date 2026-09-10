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

namespace Rock.ViewModels.Blocks.CheckIn.MobileCheckInLauncher
{
    /// <summary>
    /// The box that contains all the initialization information for the Mobile Check-in Launcher block.
    /// </summary>
    public class MobileCheckInLauncherInitializationBox : BlockBox
    {
        /// <summary>
        /// Gets or sets the rendered header content shown above every state of the check-in flow.
        /// </summary>
        public string HeaderHtml { get; set; }

        /// <summary>
        /// Gets or sets the rendered prompt asking the individual to identify themselves before check-in.
        /// </summary>
        public string IdentifyYouPromptHtml { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the individual checking in has been identified. When
        /// <c>false</c> the identification prompt is shown instead of the check-in flow.
        /// </summary>
        public bool IsIndividualIdentified { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the kiosk is resolved from a campus the individual picks
        /// instead of from their device's location.
        /// </summary>
        public bool IsLocationServicesDisabled { get; set; }

        /// <summary>
        /// Gets or sets the url that reloads this page in the configured check-in theme. Empty when the page is
        /// already in it, which is every load once the site's theme cookie has been set. The theme is resolved
        /// before any block runs, so changing it means loading the page again rather than restyling in place.
        /// </summary>
        public string ThemeRedirectUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the individual has already granted location permission on a
        /// previous visit. When <c>true</c> their location is requested without prompting first.
        /// </summary>
        public bool IsLocationApprovalRemembered { get; set; }

        /// <summary>
        /// Gets or sets the rendered prompt explaining that the browser is about to ask for location permission.
        /// </summary>
        public string AllowLocationPromptHtml { get; set; }

        /// <summary>
        /// Gets or sets the rendered message shown while the individual's location is being determined.
        /// </summary>
        public string LocationProgressHtml { get; set; }

        /// <summary>
        /// Gets or sets the rendered message shown when the browser will not report the individual's location.
        /// </summary>
        public string UnableToDetermineLocationHtml { get; set; }

        /// <summary>
        /// Gets or sets the campuses the individual can pick from, whose values are the hashed identifier of the
        /// kiosk device serving that campus. Only populated when location services are disabled.
        /// </summary>
        public List<ListItemBag> CampusDeviceItems { get; set; }

        /// <summary>
        /// Gets or sets the rendered message that replaces the check-in flow when it cannot start, such as when no
        /// campuses are available to pick from.
        /// </summary>
        public string MessageHtml { get; set; }

        /// <summary>
        /// Gets or sets the rendered message shown when the kiosk is open but nobody in the individual's family is
        /// eligible to check in there.
        /// </summary>
        public string NoPeopleMessageHtml { get; set; }

        /// <summary>
        /// Gets or sets the hashed identifier of the family being checked in. The server resolves the family from
        /// the identified individual on every request regardless of what the client sends back.
        /// </summary>
        public string FamilyIdKey { get; set; }

        /// <summary>
        /// Gets or sets the name of the family being checked in. The check-in flow will not leave its family select
        /// step without a family it can resolve by identifier.
        /// </summary>
        public string FamilyName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether every available schedule is selected without asking. This also
        /// skips anyone with nothing to check into rather than telling them so.
        /// </summary>
        public bool AreAllSchedulesSelectedAutomatically { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this browser has completed a check-in recently, which is what
        /// makes the check-in action read as adding to one.
        /// </summary>
        public bool IsCheckInCompleted { get; set; }

        /// <summary>
        /// Gets or sets the url of the QR code image for the check-ins this browser has already completed, which a
        /// kiosk scans to print their labels. Empty when no recent check-in is still printable, or when the block
        /// is configured to show no code.
        /// </summary>
        public string QrCodeImageUrl { get; set; }
    }
}
