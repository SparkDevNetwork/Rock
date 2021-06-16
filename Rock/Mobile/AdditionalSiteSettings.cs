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

using Rock.Common.Mobile.Enums;
using Rock.DownhillCss;

namespace Rock.Mobile
{
    /// <summary>
    /// This class is used to store and retrieve
    /// Additional Setting for Mobile against the Site Entity
    /// </summary>
    public class AdditionalSiteSettings
    {
        #region Private Fields

        private const string _defaultFlyoutXaml = @"<ListView SeparatorVisibility=""None"" 
    HasUnevenRows=""true"" 
    ItemsSource=""{Binding MenuItems}"">

    <ListView.Header>
        <StackLayout VerticalOptions=""FillAndExpand""
            Orientation=""Vertical"">

            <Rock:LoginStatus Padding=""20, 70, 20, 50"" 
                ImageSize=""120"" 
                ImageBorderColor=""rgba(255, 255, 255, 0.4)"" 
                ImageBorderSize=""5"" />

            <BoxView HeightRequest=""1"" BackgroundColor=""rgba(255, 255, 255, 0.2)""
                HorizontalOptions=""FillAndExpand""/>

        </StackLayout>
    </ListView.Header>

    <ListView.ItemTemplate>
        <DataTemplate>
            <Rock:ViewCell SelectedBackgroundColor=""rgba(255, 255, 255, 0.2)"">
            
                <StackLayout VerticalOptions=""FillAndExpand"" 
                    Orientation=""Vertical"">

                    <ContentView StyleClass=""pt-16, pb-12"">
                        <Label StyleClass=""text-white, ml-32, flyout-menu-item""
                            Text=""{Binding Title}"" 
                            VerticalOptions=""Center"" 
                            HorizontalOptions=""FillAndExpand"" />
                    </ContentView>

                    <BoxView HeightRequest=""1""
                        BackgroundColor=""rgba(255, 255, 255, 0.4)""
                        HorizontalOptions=""FillAndExpand"" />

                </StackLayout>

            </Rock:ViewCell>
        </DataTemplate>
    </ListView.ItemTemplate>

</ListView>";

        #endregion

        /// <summary>
        /// Gets or sets the last deployment date.
        /// </summary>
        /// <value>
        /// The last deployment date.
        /// </value>
        public DateTime? LastDeploymentDate { get; set; }

        /// <summary>
        /// Gets or sets the last deployment version identifier.
        /// </summary>
        /// <value>
        /// The last deployment version identifier.
        /// </value>
        /// <remarks>
        /// This must match the value stored in <see cref="Rock.Common.Mobile.UpdatePackage.ApplicationVersionId"/>
        /// otherwise the shell will keep downloading the same version over and over again.
        /// </remarks>
        public int? LastDeploymentVersionId { get; set; }

        /// <summary>
        /// Gets or sets the phone update package URL.
        /// </summary>
        /// <value>
        /// The phone update package URL.
        /// </value>
        public string PhoneUpdatePackageUrl { get; set; }

        /// <summary>
        /// Gets or sets the tablet update package URL.
        /// </summary>
        /// <value>
        /// The tablet update package URL.
        /// </value>
        public string TabletUpdatePackageUrl { get; set; }

        /// <summary>
        /// Gets or sets the type of the shell.
        /// </summary>
        /// <value>
        /// The type of the shell.
        /// </value>
        public ShellType? ShellType { get; set; } = Rock.Common.Mobile.Enums.ShellType.Flyout;

        /// <summary>
        /// Gets or sets the tab location.
        /// </summary>
        /// <value>
        /// The tab location.
        /// </value>
        public TabLocation? TabLocation { get; set; } = Mobile.TabLocation.Bottom;

        /// <summary>
        /// Gets or sets the CSS style.
        /// </summary>
        /// <value>
        /// The CSS style.
        /// </value>
        public string CssStyle { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the API key identifier.
        /// </summary>
        /// <value>
        /// The API key identifier.
        /// </value>
        public int? ApiKeyId { get; set; }

        /// <summary>
        /// Gets or sets the profile page identifier.
        /// </summary>
        /// <value>
        /// The profile page identifier.
        /// </value>
        public int? ProfilePageId { get; set; }

        /// <summary>
        /// Gets or sets the person attribute categories.
        /// </summary>
        /// <value>
        /// The person attribute categories.
        /// </value>
        public List<int> PersonAttributeCategories { get; set; } = new List<int>();

        /// <summary>
        /// Gets or sets the color of the bar background.
        /// </summary>
        /// <value>
        /// The color of the bar background.
        /// </value>
        public string BarBackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the color of the menu button.
        /// </summary>
        /// <value>
        /// The color of the menu button.
        /// </value>
        public string MenuButtonColor { get; set; }

        /// <summary>
        /// Gets or sets the color of the activity indicator.
        /// </summary>
        /// <value>
        /// The color of the activity indicator.
        /// </value>
        public string ActivityIndicatorColor { get; set; }

        /// <summary>
        /// Gets or sets the xaml to use for the flyout shell menu.
        /// </summary>
        /// <value>
        /// The xaml to use for the flyout shell menu.
        /// </value>
        public string FlyoutXaml { get; set; } = _defaultFlyoutXaml;

        /// <summary>
        /// Gets or sets the locked phone orientation.
        /// </summary>
        /// <value>
        /// The locked phone orientation.
        /// </value>
        public DeviceOrientation LockedPhoneOrientation { get; set; } = DeviceOrientation.Portrait;

        /// <summary>
        /// Gets or sets the locked tablet orientation.
        /// </summary>
        /// <value>
        /// The locked tablet orientation.
        /// </value>
        public DeviceOrientation LockedTabletOrientation { get; set; } = DeviceOrientation.Unknown;

        /// <summary>
        /// Gets or sets the downhill settings.
        /// </summary>
        /// <value>
        /// The downhill settings.
        /// </value>
        public DownhillSettings DownhillSettings { get; set; } = new DownhillSettings();

        /// <summary>
        /// Gets or sets the navigation bar action xaml.
        /// </summary>
        /// <value>
        /// The navigation bar action xaml.
        /// </value>
        public string NavigationBarActionXaml { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the lava to use for homepage routing logic.
        /// </summary>
        /// <value>
        /// The lava to use for homepage routing logic.
        /// </value>
        public string HomepageRoutingLogic { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the campus filter data view identifier.
        /// </summary>
        /// <value>
        /// The campus filter data view identifier.
        /// </value>
        public int? CampusFilterDataViewId { get; set; }

        /// <summary>
        /// Gets or sets the communication view page identifier.
        /// </summary>
        /// <value>
        /// The communication view page identifier.
        /// </value>
        public int? CommunicationViewPageId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to enable notifications automatically.
        /// </summary>
        /// <value>
        ///   <c>true</c> if application should enable notifications automatically; otherwise, <c>false</c>.
        /// </value>
        public bool EnableNotificationsAutomatically { get; set; } = true;
    }
}
