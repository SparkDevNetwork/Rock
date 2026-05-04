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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Mobile.MobileApplicationDetail
{
    /// <summary>
    /// The bag that represents a mobile application's styling configuration
    /// (the "Styles" tab) for the Mobile Application Detail block.
    /// </summary>
    public class MobileApplicationStylesBag
    {
        /// <summary>
        /// Gets or sets the style framework that drives which set of style
        /// fields are persisted. Standard hides the legacy fields, Legacy
        /// hides the MAUI fields, Blended shows both.
        /// </summary>
        public int MobileStyleFramework { get; set; }

        #region MAUI Interface Colors

        /// <summary>
        /// Gets or sets the strongest interface color.
        /// </summary>
        public string InterfaceStrongest { get; set; }

        /// <summary>
        /// Gets or sets the stronger interface color.
        /// </summary>
        public string InterfaceStronger { get; set; }

        /// <summary>
        /// Gets or sets the strong interface color.
        /// </summary>
        public string InterfaceStrong { get; set; }

        /// <summary>
        /// Gets or sets the medium interface color.
        /// </summary>
        public string InterfaceMedium { get; set; }

        /// <summary>
        /// Gets or sets the soft interface color.
        /// </summary>
        public string InterfaceSoft { get; set; }

        /// <summary>
        /// Gets or sets the softer interface color.
        /// </summary>
        public string InterfaceSofter { get; set; }

        /// <summary>
        /// Gets or sets the softest interface color.
        /// </summary>
        public string InterfaceSoftest { get; set; }

        #endregion

        #region MAUI Accent Colors

        /// <summary>
        /// Gets or sets the strong primary accent color.
        /// </summary>
        public string PrimaryStrong { get; set; }

        /// <summary>
        /// Gets or sets the soft primary accent color.
        /// </summary>
        public string PrimarySoft { get; set; }

        /// <summary>
        /// Gets or sets the strong secondary accent color.
        /// </summary>
        public string SecondaryStrong { get; set; }

        /// <summary>
        /// Gets or sets the soft secondary accent color.
        /// </summary>
        public string SecondarySoft { get; set; }

        /// <summary>
        /// Gets or sets the strong brand accent color.
        /// </summary>
        public string BrandStrong { get; set; }

        /// <summary>
        /// Gets or sets the soft brand accent color.
        /// </summary>
        public string BrandSoft { get; set; }

        #endregion

        #region MAUI Functional Colors

        /// <summary>
        /// Gets or sets the strong success status color.
        /// </summary>
        public string SuccessStrong { get; set; }

        /// <summary>
        /// Gets or sets the soft success status color.
        /// </summary>
        public string SuccessSoft { get; set; }

        /// <summary>
        /// Gets or sets the strong info status color.
        /// </summary>
        public string InfoStrong { get; set; }

        /// <summary>
        /// Gets or sets the soft info status color.
        /// </summary>
        public string InfoSoft { get; set; }

        /// <summary>
        /// Gets or sets the strong danger status color.
        /// </summary>
        public string DangerStrong { get; set; }

        /// <summary>
        /// Gets or sets the soft danger status color.
        /// </summary>
        public string DangerSoft { get; set; }

        /// <summary>
        /// Gets or sets the strong warning status color.
        /// </summary>
        public string WarningStrong { get; set; }

        /// <summary>
        /// Gets or sets the soft warning status color.
        /// </summary>
        public string WarningSoft { get; set; }

        #endregion

        #region Navigation Bar Settings

        /// <summary>
        /// Gets or sets the background color of the navigation bar.
        /// </summary>
        public string BarBackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the iOS navigation bar
        /// should be drawn with transparency.
        /// </summary>
        public bool IsNavBarTransparent { get; set; }

        /// <summary>
        /// Gets or sets the iOS navigation bar blur style applied when
        /// transparency is enabled.
        /// </summary>
        public int NavBarBlurStyle { get; set; }

        /// <summary>
        /// Gets or sets the light-mode navigation bar header image.
        /// </summary>
        public ListItemBag LightHeaderImage { get; set; }

        /// <summary>
        /// Gets or sets the dark-mode navigation bar header image.
        /// </summary>
        public ListItemBag DarkHeaderImage { get; set; }

        #endregion

        #region Legacy Xamarin Forms Settings

        /// <summary>
        /// Gets or sets the menu button color used by legacy Xamarin Forms
        /// shells.
        /// </summary>
        public string MenuButtonColor { get; set; }

        /// <summary>
        /// Gets or sets the activity indicator color used by legacy Xamarin
        /// Forms shells.
        /// </summary>
        public string ActivityIndicatorColor { get; set; }

        /// <summary>
        /// Gets or sets the default text color for legacy Xamarin Forms
        /// shells.
        /// </summary>
        public string TextColor { get; set; }

        /// <summary>
        /// Gets or sets the default heading color for legacy Xamarin Forms
        /// shells.
        /// </summary>
        public string HeadingColor { get; set; }

        /// <summary>
        /// Gets or sets the application background color for legacy Xamarin
        /// Forms shells.
        /// </summary>
        public string BackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the legacy primary application color.
        /// </summary>
        public string Primary { get; set; }

        /// <summary>
        /// Gets or sets the legacy secondary application color.
        /// </summary>
        public string Secondary { get; set; }

        /// <summary>
        /// Gets or sets the legacy success application color.
        /// </summary>
        public string Success { get; set; }

        /// <summary>
        /// Gets or sets the legacy info application color.
        /// </summary>
        public string Info { get; set; }

        /// <summary>
        /// Gets or sets the legacy danger application color.
        /// </summary>
        public string Danger { get; set; }

        /// <summary>
        /// Gets or sets the legacy warning application color.
        /// </summary>
        public string Warning { get; set; }

        /// <summary>
        /// Gets or sets the legacy light application color.
        /// </summary>
        public string Light { get; set; }

        /// <summary>
        /// Gets or sets the legacy dark application color.
        /// </summary>
        public string Dark { get; set; }

        /// <summary>
        /// Gets or sets the legacy brand application color.
        /// </summary>
        public string Brand { get; set; }

        #endregion

        #region Advanced

        /// <summary>
        /// Gets or sets the default font size used throughout the application.
        /// </summary>
        public int FontSizeDefault { get; set; }

        /// <summary>
        /// Gets or sets the custom CSS styles applied to UI elements in the
        /// application.
        /// </summary>
        public string CssStyles { get; set; }

        #endregion
    }
}
