﻿// <copyright>
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

using Rock.Common.Mobile;
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

        private const string _defaultFlyoutXaml = @"<CollectionView ItemsSource=""{Binding MenuItems}"" StyleClass=""bg-interface-softest""
	    SelectionMode=""Single"">
	    <CollectionView.Header>
	        <StackLayout VerticalOptions=""FillAndExpand""
	            Orientation=""Vertical"">
	
	            <Rock:LoginStatus Padding=""64 0 24 0""
	                TitleTextColor=""{AppThemeBinding Light={Rock:PaletteColor Interface-Stronger}, Dark={Rock:PaletteColor Interface-Softest}}""
	                NotLoggedInColor=""{AppThemeBinding Light={Rock:PaletteColor App-Primary-Strong}, Dark={Rock:PaletteColor App-Primary-Strong}}""
	                NoProfileIconColor=""{AppThemeBinding Light={Rock:PaletteColor App-Primary-Strong}, Dark={Rock:PaletteColor App-Primary-Strong}}""
	                SubTitleTextColor=""{AppThemeBinding Light={Rock:PaletteColor Interface-strong}, Dark={Rock:PaletteColor Interface-Soft}}""
	                ImageSize=""75""
	                ImageBorderColor=""{AppThemeBinding Light={Rock:PaletteColor Interface-Stronger}, Dark={Rock:PaletteColor Interface-Softest}}"" 
	                ImageBorderSize=""5"" />
	            
	            <ContentView Padding=""0 0 24 0"">
	                <BoxView HeightRequest=""1""
	                    WidthRequest=""250""
	                    BackgroundColor=""{AppThemeBinding Light={Rock:PaletteColor Interface-Soft}, Dark={Rock:PaletteColor Interface-Strong}}"" />
	            </ContentView>
	                
	        </StackLayout>
	    </CollectionView.Header>
	
	    <CollectionView.ItemTemplate>
	        <DataTemplate>
				<Grid StyleClass=""py-12, px-24"" 
				    VerticalOptions =""Center"">

					<VisualStateManager.VisualStateGroups>
						<VisualStateGroup Name=""CommonStates"">
							<VisualState Name=""Normal"" />
							<VisualState Name=""Selected"">
								<VisualState.Setters>
									<Setter Property=""BackgroundColor"" Value=""{AppThemeBinding Light={Rock:PaletteColor App-Primary-Strong}, Dark={Rock:PaletteColor App-Primary-Strong}}"" />
									<Setter TargetName=""TitleLabel"" Property=""Label.TextColor"" Value=""{AppThemeBinding Light={Rock:PaletteColor Interface-Softest}, Dark={Rock:PaletteColor Interface-Stronger}}"" />
									<Setter TargetName=""fillTransformation"" Property=""Rock:FillColorTransformation.Color"" Value=""{AppThemeBinding Light={Rock:PaletteColor Interface-Softest}, Dark={Rock:PaletteColor Interface-Stronger}}"" />
								</VisualState.Setters>
							</VisualState>
						</VisualStateGroup>
					</VisualStateManager.VisualStateGroups>

					<Grid.ColumnDefinitions>
						<ColumnDefinition Width=""24""/>
						<ColumnDefinition Width=""*""/>
					</Grid.ColumnDefinitions>
						
				
					<Rock:Image Source=""{Binding IconUrl}""
						Grid.Column=""0""
						HeightRequest=""24""
						WidthRequest=""24"">
						<Rock:FillColorTransformation x:Name=""fillTransformation"" Color=""{AppThemeBinding Light={Rock:PaletteColor App-Primary-Strong}, Dark={Rock:PaletteColor App-Primary-Strong}}"" />
					</Rock:Image>
							
					<Label x:Name=""TitleLabel"" 
						StyleClass=""text-interface-stronger, title3, ml-16, bold, flyout-menu-item""
						Grid.Column=""1""
						Text=""{Binding Title}"" 
						VerticalOptions=""Center"" 
						HorizontalOptions=""FillAndExpand"" />
				</Grid>
	        </DataTemplate>
	    </CollectionView.ItemTemplate>
	</CollectionView>
";

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
        /// Gets or sets the chat page identifier.
        /// </summary>
        /// <value>
        /// The chat page identifier.
        /// </value>
        public int? ChatPageId { get; set; }

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
        /// Gets or sets a value indicating whether to enable bar transparency.
        /// </summary>
        /// <value><c>true</c> if enable bar transparency; otherwise, <c>false</c>.</value>
        public bool IOSEnableBarTransparency { get; set; }

        /// <summary>
        /// Gets or sets the IOS blur style.
        /// </summary>
        /// <value>The IOS blur style.</value>
        public IOSBlurStyle IOSBarBlurStyle { get; set; }

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
        /// Gets or sets the identifier of the page that will display interactive
        /// experience occurrences to the individual.
        /// </summary>
        /// <value>
        /// The interactive experience page identifier.
        /// </value>
        public int? InteractiveExperiencePageId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the page that will display a SMS
        /// conversation between Rock and an individual.
        /// </summary>
        /// <value>The SMS conversation page identifier.</value>
        public int? SmsConversationPageId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to enable notifications automatically.
        /// </summary>
        /// <value>
        ///   <c>true</c> if application should enable notifications automatically; otherwise, <c>false</c>.
        /// </value>
        public bool EnableNotificationsAutomatically { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating a push token update has been requested.
        /// </summary>
        /// <value>
        /// A value indicating a push token update has been requested.
        /// </value>
        public string PushTokenUpdateValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is deep linking enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is deep linking enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsDeepLinkingEnabled { get; set; }

        /// <summary>
        /// Gets or sets the iOS bundle identifier.
        /// </summary>
        /// <value>
        /// The iOS bundle identifier.
        /// </value>
        public string BundleIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the team identifier.
        /// </summary>
        /// <value>
        /// The team identifier.
        /// </value>
        public string TeamIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the name of the android package.
        /// </summary>
        /// <value>
        /// The name of the android package.
        /// </value>
        public string PackageName { get; set; }

        /// <summary>
        /// Gets or sets the android certificate fingerprint.
        /// </summary>
        /// <value>
        /// The certificate fingerprint.
        /// </value>
        public string CertificateFingerprint { get; set; }

        /// <summary>
        /// Gets or sets the deep link path prefix.
        /// </summary>
        /// <value>
        /// The deep link path prefix.
        /// </value>
        public string DeepLinkPathPrefix { get; set; }

        /// <summary>
        /// Gets or sets the deep link routes.
        /// </summary>
        /// <value>
        /// The deep link routes.
        /// </value>
        public List<DeepLinkRoute> DeepLinkRoutes { get; set; } = new List<DeepLinkRoute>();

        /// <summary>
        /// Gets or sets the deep link domains.
        /// </summary>
        /// <value>The deep link domains.</value>
        public string DeepLinkDomains { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this application site will
        /// compress the update packages with GZip. This provides a 95% size
        /// reduction but is not supported on shell v1.
        /// </summary>
        /// <value><c>true</c> if this application site will compress update packages, <c>false</c>.</value>
        public bool IsPackageCompressionEnabled { get; set; }

        /// <summary>
        /// Gets or sets the auth0 domain.
        /// </summary>
        /// <value>The auth0 domain.</value>
        public string Auth0Domain { get; set; }

        /// <summary>
        /// Gets or sets the auth0 client identifier.
        /// </summary>
        /// <value>The auth0 client identifier.</value>
        public string Auth0ClientId { get; set; }

        /// <summary>
        /// Gets or sets the connection status to use for new Auth0 logins.
        /// </summary>
        public int? Auth0ConnectionStatusValueId { get; set; }

        /// <summary>
        /// Gets or sets the record status to use for new Auth0 logins.
        /// </summary>
        public int? Auth0RecordStatusValueId { get; set; }

        /// <summary>
        /// The Entra client identifier.
        /// </summary>
        public string EntraClientId { get; set; }

        /// <summary>
        /// The Entra tenant identifier.
        /// </summary>
        public string EntraTenantId { get; set; }

        /// <summary>
        /// The entra authentication component.
        /// </summary>
        public Guid? EntraAuthenticationComponent { get; set; }

        /// <summary>
        /// Gets or sets the dark theme CSS style.
        /// </summary>
        public int? DarkFavIconBinaryFileId { get; set; }
    }
}