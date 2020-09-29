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
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;

using Humanizer;

using Rock;
using Rock.Attribute;
using Rock.Common.Mobile.Enums;
using Rock.Data;
using Rock.DownhillCss;
using Rock.Mobile;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

using AdditionalSiteSettings = Rock.Mobile.AdditionalSiteSettings;
using ShellType = Rock.Common.Mobile.Enums.ShellType;
using TabLocation = Rock.Mobile.TabLocation;

namespace RockWeb.Blocks.Mobile
{
    [DisplayName( "Mobile Application Detail" )]
    [Category( "Mobile" )]
    [Description( "Edits and configures the settings of a mobile application." )]
    [LinkedPage( "Layout Detail", "", true ) ]
    [LinkedPage( "Page Detail", "", true )]
    public partial class MobileApplicationDetail : RockBlock
    {
        /// <summary>
        /// Keys to use for block attributes
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// Key for Layout Detail
            /// </summary>
            public const string LayoutDetail = "LayoutDetail";

            /// <summary>
            /// Key for Page Detail
            /// </summary>
            public const string PageDetail = "PageDetail";
        }

        #region Private Fields

        private const string _defaultLayoutXaml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<ContentPage xmlns=""http://xamarin.com/schemas/2014/forms""
             xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
             xmlns:Rock=""clr-namespace:Rock.Mobile.Cms;assembly=Rock.Mobile""
             xmlns:Common=""clr-namespace:Rock.Mobile.Common;assembly=Rock.Mobile.Common"">
    <ScrollView>
        <StackLayout>
            <Rock:Zone ZoneName=""Main"" />
        </StackLayout>
    </ScrollView>
</ContentPage>";

        private enum Tabs
        {
            Application,
            Styles,
            Layouts,
            Pages
        }
        #endregion

        #region Base Method Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gLayouts.Actions.ShowMergeTemplate = false;
            gLayouts.Actions.ShowExcelExport = false;
            gLayouts.Actions.AddClick += gLayouts_AddClick;
            gLayouts.Actions.ShowAdd = true;
            gLayouts.DataKeyNames = new[] { "Id" };

            gPages.Actions.ShowMergeTemplate = false;
            gPages.Actions.ShowExcelExport = false;
            gPages.Actions.AddClick += gPages_AddClick;
            gPages.Actions.ShowAdd = true;
            gPages.DataKeyNames = new[] { "Id" };
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !IsPostBack )
            {
                ConfigureControls();

                var siteId = PageParameter( "SiteId" ).AsInteger();
                hfCurrentTab.Value = PageParameter( "Tab" ) ?? Tabs.Application.ConvertToString();

                if ( siteId != 0 )
                {
                    ShowDetail( siteId );
                }
                else
                {
                    ltAppName.Text = "Add Application";
                    ShowEdit( siteId );
                }
            }
        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs.
        /// </summary>
        /// <param name="pageReference">The <see cref="Rock.Web.PageReference" />.</param>
        /// <returns>
        /// A <see cref="System.Collections.Generic.List{BreadCrumb}" /> of block related <see cref="Rock.Web.UI.BreadCrumb">BreadCrumbs</see>.
        /// </returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? siteId = PageParameter( pageReference, "SiteId" ).AsIntegerOrNull();
            if ( siteId != null )
            {
                var site = new SiteService( new RockContext() ).Get( siteId.Value );

                if ( site != null )
                {
                    breadCrumbs.Add( new BreadCrumb( site.Name, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Application", pageReference ) );
                }
            }
            else
            {
                // don't show a breadcrumb if we don't have a pageparam to work with
            }

            return breadCrumbs;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the provided tab and hides the others.
        /// </summary>
        /// <param name="showTab">The show tab.</param>
        private void ShowTab( Tabs showTab )
        {
            liTabApplication.RemoveCssClass( "active" );
            liTabStyles.RemoveCssClass( "active" );
            liTabLayouts.RemoveCssClass( "active" );
            liTabPages.RemoveCssClass( "active" );

            string tabName = showTab.ConvertToString();
            hfCurrentTab.Value = tabName;

            pnlApplication.Visible = tabName == Tabs.Application.ConvertToString();
            pnlStyles.Visible = tabName == Tabs.Styles.ConvertToString();
            pnlLayouts.Visible = tabName == Tabs.Layouts.ConvertToString();
            pnlPages.Visible = tabName == Tabs.Pages.ConvertToString();
            
            switch ( showTab )
            {
                case Tabs.Application:
                    liTabApplication.AddCssClass( "active" );
                    break;

                case Tabs.Styles:
                    liTabStyles.AddCssClass( "active" );
                    break;

                case Tabs.Layouts:
                    liTabLayouts.AddCssClass( "active" );
                    break;

                case Tabs.Pages:
                    liTabPages.AddCssClass( "active" );
                    break;

                default:
                    liTabApplication.AddCssClass( "active" );
                    pnlApplication.Visible = true;
                    hfCurrentTab.Value = Tabs.Application.ConvertToString();
                    break;
            }
        }

        /// <summary>
        /// Configures the controls.
        /// </summary>
        private void ConfigureControls()
        {
            ddlEditLockPhoneOrientation.Items.Clear();
            ddlEditLockPhoneOrientation.Items.Add( Rock.Constants.None.ListItem );
            ddlEditLockPhoneOrientation.Items.Add( new ListItem( "Portrait", ( ( int ) DeviceOrientation.Portrait ).ToString() ) );
            ddlEditLockPhoneOrientation.Items.Add( new ListItem( "Landscape", ( ( int ) DeviceOrientation.Landscape ).ToString() ) );

            ddlEditLockTabletOrientation.Items.Clear();
            ddlEditLockTabletOrientation.Items.Add( Rock.Constants.None.ListItem );
            ddlEditLockTabletOrientation.Items.Add( new ListItem( "Portrait", ( ( int ) DeviceOrientation.Portrait ).ToString() ) );
            ddlEditLockTabletOrientation.Items.Add( new ListItem( "Landscape", ( ( int ) DeviceOrientation.Landscape ).ToString() ) );

            imgEditHeaderImage.BinaryFileTypeGuid = Rock.SystemGuid.BinaryFiletype.DEFAULT.AsGuid();
            imgEditPreviewThumbnail.BinaryFileTypeGuid = Rock.SystemGuid.BinaryFiletype.DEFAULT.AsGuid();

            rblEditApplicationType.BindToEnum<ShellType>();
            rblEditAndroidTabLocation.BindToEnum<TabLocation>();

            cpEditPersonAttributeCategories.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Attribute ) ).Id;
            cpEditPersonAttributeCategories.EntityTypeQualifierColumn = "EntityTypeId";
            cpEditPersonAttributeCategories.EntityTypeQualifierValue = EntityTypeCache.Get( typeof( Person ) ).Id.ToString();

            dvpCampusFilter.EntityTypeId = EntityTypeCache.GetId( Rock.SystemGuid.EntityType.CAMPUS ) ?? 0;
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        private void ShowDetail( int siteId )
        {
            var rockContext = new RockContext();
            var site = new SiteService( rockContext ).Get( siteId );

            //
            // Make sure the site exists.
            //
            if ( site == null )
            {
                nbError.Text = "That mobile application does not exist.";
                pnlOverview.Visible = false;

                return;
            }

            //
            // Ensure user is authorized to view mobile sites.
            //
            if ( !IsUserAuthorized( Authorization.VIEW ) )
            {
                nbError.Text = Rock.Constants.EditModeMessage.NotAuthorizedToView( "mobile application" );
                pnlOverview.Visible = false;

                return;
            }

            //
            // Ensure this is a mobile site.
            //
            if ( site.SiteType != SiteType.Mobile )
            {
                nbError.Text = "This block only supports mobile sites.";
                pnlOverview.Visible = false;

                return;
            }

            //
            // Set the UI fields for the standard values.
            //
            hfSiteId.Value = site.Id.ToString();
            ltAppName.Text = site.Name.EncodeHtml();
            ltDescription.Text = site.Description.EncodeHtml();
            lSiteId.Text = site.Id.ToString();
            if ( site.LatestVersionDateTime.HasValue )
            {
                var updateTimeSpan = RockDateTime.Now - site.LatestVersionDateTime.Value;
                lLastDeployDate.Text = string.Format( "<span class='label label-success' data-toggle='tooltip' title='{0}'>Last Deploy: {1} ago</span>",
                    site.LatestVersionDateTime.Value.ToString( "dddd MMMM, d M yyyy h:mm tt" ),
                    updateTimeSpan.Humanize() );
            }
            else
            {
                lLastDeployDate.Text = "<span class='label label-warning'>Not Deployed</span>";
            }
            

            // Set the UI fields for the preview thumbnail.
            imgAppPreview.ImageUrl = string.Format( "~/GetImage.ashx?Id={0}", site.ThumbnailBinaryFileId );
            pnlPreviewImage.Visible = site.ThumbnailBinaryFileId.HasValue;

            //
            // Set the UI fields for the additional details.
            //
            var additionalSettings = site.AdditionalSettings.FromJsonOrNull<AdditionalSiteSettings>() ?? new AdditionalSiteSettings();
            var fields = new List<KeyValuePair<string, string>>();

            if ( additionalSettings.ShellType.HasValue )
            {
                fields.Add( new KeyValuePair<string, string>( "Application Type", additionalSettings.ShellType.ToString() ) );
            }

            var apiKeyLogin = new UserLoginService( rockContext ).Get( additionalSettings.ApiKeyId ?? 0 );
            fields.Add( new KeyValuePair<string, string>( "API Key", apiKeyLogin != null ? apiKeyLogin.ApiKey : string.Empty ) );

            if ( additionalSettings.LastDeploymentDate.HasValue )
            {
                fields.Add( new KeyValuePair<string, string>( "Last Deployed", additionalSettings.LastDeploymentDate.Value.ToShortDateTimeString() ) );
            }

            var selectedCategories = CategoryCache.All( rockContext )
                .Where( c => additionalSettings.PersonAttributeCategories.Contains( c.Id ) )
                .Select( c => c.Name )
                .ToList();
            if ( selectedCategories.Any() )
            {
                fields.Add( new KeyValuePair<string, string>( "Person Attribute Categories", string.Join( ", ", selectedCategories ) ) );
            }

            // TODO: I'm pretty sure something like this already exists in Rock, but I can never find it. - dh
            //ltAppDetails.Text = string.Join( "", fields.Select( f => string.Format( "<dl><dt>{0}</dt><dd>{1}</dd></dl>", f.Key, f.Value ) ) );
            ltAppDetails.Text = fields.Select( f => string.Format( "<dl><dt>{0}</dt><dd>{1}</dd></dl>", f.Key, f.Value ) ).JoinStrings( string.Empty );

            //
            // Bind the grids.
            //
            BindLayouts( siteId );
            BindPages( siteId );

            ShowStylesTabDetails( site );

            pnlContent.Visible = true;
            pnlOverview.Visible = true;
            pnlEdit.Visible = false;

            //
            // If returning from a child page make sure the correct tab is selected.
            //
            switch ( hfCurrentTab.Value )
            {
                case "Styles":
                    ShowTab( Tabs.Styles );
                    break;

                case "Layouts":
                    ShowTab( Tabs.Layouts );
                    break;

                case "Pages":
                    ShowTab( Tabs.Pages );
                    break;

                default:
                    ShowTab( Tabs.Application );
                    break;
            }
        }

        /// <summary>
        /// Shows the edit for the application tab.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        private void ShowEdit( int siteId )
        {
            var rockContext = new RockContext();
            var site = new SiteService( rockContext ).Get( siteId );
            AdditionalSiteSettings additionalSettings;

            //
            // Ensure user can edit the mobile site.
            //
            if ( !IsUserAuthorized( Authorization.EDIT ) )
            {
                nbError.Text = Rock.Constants.EditModeMessage.NotAuthorizedToEdit( "mobile application" );
                pnlOverview.Visible = false;

                return;
            }

            //
            // If we are generating a new site, set the initial values.
            //
            if ( site == null )
            {
                site = new Site
                {
                    IsActive = true,
                    AdditionalSettings = new AdditionalSiteSettings().ToJson()
                };
            }

            //
            // Decode our additional site settings.
            //
            if ( site.AdditionalSettings != null )
            {
                additionalSettings = site.AdditionalSettings.FromJsonOrNull<AdditionalSiteSettings>() ?? new AdditionalSiteSettings();
            }
            else
            {
                additionalSettings = new AdditionalSiteSettings();
            }

            //
            // Set basic UI fields.
            //
            tbEditName.Text = site.Name;
            cbEditActive.Checked = site.IsActive;
            tbEditDescription.Text = site.Description;

            rblEditApplicationType.SetValue( ( int? ) additionalSettings.ShellType ?? ( int ) ShellType.Flyout );
            rblEditAndroidTabLocation.SetValue( ( int? ) additionalSettings.TabLocation ?? ( int ) TabLocation.Bottom );
            ddlEditLockPhoneOrientation.SetValue( ( int ) additionalSettings.LockedPhoneOrientation );
            ddlEditLockTabletOrientation.SetValue( ( int ) additionalSettings.LockedTabletOrientation );
            
            ceEditFlyoutXaml.Text = additionalSettings.FlyoutXaml;
            ceEditNavBarActionXaml.Text = additionalSettings.NavigationBarActionXaml;

            cpEditPersonAttributeCategories.SetValues( CategoryCache.All( rockContext ).Where( c => additionalSettings.PersonAttributeCategories.Contains( c.Id ) ).Select( c => c.Id ) );

            dvpCampusFilter.SetValue( additionalSettings.CampusFilterDataViewId );

            rblEditAndroidTabLocation.Visible = rblEditApplicationType.SelectedValueAsInt() == ( int ) ShellType.Tabbed;

            ppEditLoginPage.SetValue( site.LoginPageId );
            ppEditProfilePage.SetValue( additionalSettings.ProfilePageId );
            ppCommunicationViewPage.SetValue( additionalSettings.CommunicationViewPageId );

            //
            // Set the API Key.
            //
            var apiKeyLogin = new UserLoginService( rockContext ).Get( additionalSettings.ApiKeyId ?? 0 );
            tbEditApiKey.Text = apiKeyLogin != null ? apiKeyLogin.ApiKey : GenerateApiKey();

            //
            // Set image UI field.
            //
            imgEditPreviewThumbnail.BinaryFileId = site.ThumbnailBinaryFileId;

            pnlContent.Visible = false;
            pnlEdit.Visible = true;
        }


        private void ShowStylesTabDetails( Site site )
        {
            using ( var rockContext = new RockContext() )
            {
                AdditionalSiteSettings additionalSettings = site.AdditionalSettings.FromJsonOrNull<AdditionalSiteSettings>() ?? new AdditionalSiteSettings();

                // Ensure user can edit the mobile site.
                if ( !IsUserAuthorized( Authorization.EDIT ) )
                {
                    nbError.Text = Rock.Constants.EditModeMessage.NotAuthorizedToEdit( "mobile application" );
                    pnlOverview.Visible = false;

                    return;
                }

                cpEditBarBackgroundColor.Value = additionalSettings.BarBackgroundColor;
                cpEditMenuButtonColor.Value = additionalSettings.MenuButtonColor;
                cpEditActivityIndicatorColor.Value = additionalSettings.ActivityIndicatorColor;
                cpTextColor.Value = additionalSettings.DownhillSettings.TextColor;
                cpHeadingColor.Value = additionalSettings.DownhillSettings.HeadingColor;
                cpBackgroundColor.Value = additionalSettings.DownhillSettings.BackgroundColor;

                ceEditCssStyles.Text = additionalSettings.CssStyle ?? string.Empty;

                cpPrimary.Value = additionalSettings.DownhillSettings.ApplicationColors.Primary;
                cpSecondary.Value = additionalSettings.DownhillSettings.ApplicationColors.Secondary;
                cpSuccess.Value = additionalSettings.DownhillSettings.ApplicationColors.Success;
                cpInfo.Value = additionalSettings.DownhillSettings.ApplicationColors.Info;
                cpDanger.Value = additionalSettings.DownhillSettings.ApplicationColors.Danger;
                cpWarning.Value = additionalSettings.DownhillSettings.ApplicationColors.Warning;
                cpLight.Value = additionalSettings.DownhillSettings.ApplicationColors.Light;
                cpDark.Value = additionalSettings.DownhillSettings.ApplicationColors.Dark;
                cpBrand.Value = additionalSettings.DownhillSettings.ApplicationColors.Brand;
                cpInfo.Value = additionalSettings.DownhillSettings.ApplicationColors.Info;

                nbRadiusBase.Text = decimal.ToInt32( additionalSettings.DownhillSettings.RadiusBase ).ToStringSafe();

                nbFontSizeDefault.Text = decimal.ToInt32( additionalSettings.DownhillSettings.FontSizeDefault ).ToStringSafe();

                imgEditHeaderImage.BinaryFileId = site.FavIconBinaryFileId;
            }
        }

        /// <summary>
        /// Generates the API key.
        /// </summary>
        /// <returns></returns>
        private string GenerateApiKey()
        {
            // Generate a unique random 12 digit api key
            return Rock.Utility.KeyHelper.GenerateKey( ( RockContext rockContext, string key ) => new UserLoginService( rockContext ).Queryable().Any( a => a.ApiKey == key ) );
        }

        /// <summary>
        /// Saves the API key.
        /// </summary>
        /// <param name="restLoginId">The rest login identifier.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        private int SaveApiKey( int? restLoginId, string apiKey, string userName, RockContext rockContext )
        {
            var userLoginService = new UserLoginService( rockContext );
            var personService = new PersonService( rockContext );
            UserLogin userLogin = null;
            Person restPerson = null;

            // the key gets saved in the api key field of a user login (which you have to create if needed)
            var entityType = new EntityTypeService( rockContext )
                .Get( "Rock.Security.Authentication.Database" );

            if ( restLoginId.HasValue )
            {
                userLogin = userLoginService.Get( restLoginId.Value );
                restPerson = userLogin.Person;
            }
            else
            {
                restPerson = new Person();
                personService.Add( restPerson );
            }

            // the rest user name gets saved as the last name on a person
            restPerson.LastName = tbEditName.Text;
            restPerson.RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_RESTUSER.AsGuid() ).Id;
            restPerson.RecordStatusValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;

            rockContext.SaveChanges();

            if ( userLogin == null )
            {
                userLogin = new UserLogin();
                userLoginService.Add( userLogin );
            }

            userLogin.UserName = userName;
            userLogin.IsConfirmed = true;
            userLogin.ApiKey = apiKey;
            userLogin.EntityTypeId = entityType.Id;
            userLogin.PersonId = restPerson.Id;

            rockContext.SaveChanges();

            return userLogin.Id;
        }

        /// <summary>
        /// Parses the color and returns a hex string.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns></returns>
        private string ParseColor( string color )
        {
            //
            // Match on rgb(r,g,b) format.
            //
            var match = Regex.Match( color, "rgb *\\( *([0-9]+) *, *([0-9]+) *, *([0-9]+) *\\)" );
            if ( match.Success )
            {
                int red = match.Groups[1].Value.AsInteger();
                int green = match.Groups[2].Value.AsInteger();
                int blue = match.Groups[3].Value.AsInteger();
                return string.Format( "#{0:x2}{1:x2}{2:x2}", red, green, blue );
            }

            //
            // Match on rgba(r,g,b,a) format.
            //
            match = Regex.Match( color, "rgba *\\( *([0-9]+) *, *([0-9]+) *, *([0-9]+) *, *([\\.0-9]+) *\\)" );
            if ( match.Success )
            {
                int red = match.Groups[1].Value.AsInteger();
                int green = match.Groups[2].Value.AsInteger();
                int blue = match.Groups[3].Value.AsInteger();
                return string.Format( "#{0:x2}{1:x2}{2:x2}", red, green, blue );
            }

            //
            // Match on #rrggbb format.
            //
            match = Regex.Match( color, "#([0-9a-fA-F]{2})([0-9a-fA-F]{2})([0-9a-fA-F]{2})" );
            if ( match.Success )
            {
                return match.Value;
            }

            return null;
        }

        /// <summary>
        /// Binds the layouts.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        private void BindLayouts( int siteId )
        {
            var layouts = LayoutCache.All()
                .Where( l => l.SiteId == siteId )
                .OrderBy( l => l.Name )
                .ToList();

            gLayouts.DataSource = layouts;
            gLayouts.DataBind();
        }

        /// <summary>
        /// Binds the pages.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        private void BindPages( int siteId )
        {
            var pages = PageCache.All()
                .Where( p => p.SiteId == siteId )
                .OrderBy( p => p.Order )
                .ThenBy( p => p.InternalName )
                .Select( p => new
                {
                    p.Id,
                    p.InternalName,
                    LayoutName = p.Layout.Name,
                    DisplayInNav = p.DisplayInNavWhen != DisplayInNavWhen.Never
                } )
                .ToList();

            gPages.DataSource = pages;
            gPages.DataBind();
        }

        /// <summary>
        /// Gets the file path.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        private string GetFilePath( BinaryFile file )
        {
            string url = file.Url;

            //
            // FileSystem provider currently returns a bad URL.
            //
            if ( file.BinaryFileType.StorageEntityType.Name == "Rock.Storage.Provider.FileSystem" )
            {
                url = System.Web.VirtualPathUtility.ToAbsolute( string.Format( "~/GetFile.ashx?Id={0}", file.Id ) );
                var uri = new Uri( GlobalAttributesCache.Get().GetValue( "PublicApplicationRoot" ) );

                url = uri.Scheme + "://" + uri.GetComponents( UriComponents.HostAndPort, UriFormat.UriEscaped ) + url;
            }

            return url;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEdit_Click( object sender, EventArgs e )
        {
            ShowEdit( hfSiteId.Value.AsInteger() );
        }

        /// <summary>
        /// Handles the Click event of the lbEditCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEditCancel_Click( object sender, EventArgs e )
        {
            var siteId = PageParameter( "SiteId" ).AsInteger();

            if ( siteId == 0 )
            {
                NavigateToParentPage();
            }
            else
            {
                ShowDetail( siteId );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the lbEditSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEditSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var siteService = new SiteService( rockContext );
            var binaryFileService = new BinaryFileService( rockContext );
            var userLoginService = new UserLoginService( rockContext );

            //
            // Find the site or if we are creating a new one, bootstrap it.
            //
            var site = siteService.Get( PageParameter( "SiteId" ).AsInteger() );
            if ( site == null )
            {
                site = new Site
                {
                    SiteType = SiteType.Mobile
                };
                siteService.Add( site );
            }

            //
            // Save the basic settings.
            //
            site.Name = tbEditName.Text;
            site.IsActive = cbEditActive.Checked;
            site.Description = tbEditDescription.Text;
            site.LoginPageId = ppEditLoginPage.PageId;

            var additionalSettings = site.AdditionalSettings.FromJsonOrNull<AdditionalSiteSettings>() ?? new AdditionalSiteSettings();

            // Ensure that the Downhill CSS platform is mobile
            if ( additionalSettings.DownhillSettings == null )
            {
                additionalSettings.DownhillSettings = new DownhillSettings();
            }
            additionalSettings.DownhillSettings.Platform = DownhillPlatform.Mobile;

            //
            // Save the additional settings.
            //
            additionalSettings.ShellType = rblEditApplicationType.SelectedValueAsEnum<ShellType>();
            additionalSettings.TabLocation = rblEditAndroidTabLocation.SelectedValueAsEnum<TabLocation>();
            
            additionalSettings.PersonAttributeCategories = cpEditPersonAttributeCategories.SelectedValues.AsIntegerList();
            additionalSettings.ProfilePageId = ppEditProfilePage.PageId;
            additionalSettings.CommunicationViewPageId = ppCommunicationViewPage.PageId;
            additionalSettings.FlyoutXaml = ceEditFlyoutXaml.Text;
            additionalSettings.LockedPhoneOrientation = ddlEditLockPhoneOrientation.SelectedValueAsEnumOrNull<DeviceOrientation>() ?? DeviceOrientation.Unknown;
            additionalSettings.LockedTabletOrientation = ddlEditLockTabletOrientation.SelectedValueAsEnumOrNull<DeviceOrientation>() ?? DeviceOrientation.Unknown;
            additionalSettings.CampusFilterDataViewId = dvpCampusFilter.SelectedValueAsId();
            additionalSettings.NavigationBarActionXaml = ceEditNavBarActionXaml.Text;

            //
            // Save the image.
            //
            site.ThumbnailBinaryFileId = imgEditPreviewThumbnail.BinaryFileId;

            //
            // Ensure the images are persisted.
            //
            if ( site.SiteLogoBinaryFileId.HasValue )
            {
                binaryFileService.Get( site.SiteLogoBinaryFileId.Value ).IsTemporary = false;
            }
            if ( site.ThumbnailBinaryFileId.HasValue )
            {
                binaryFileService.Get( site.ThumbnailBinaryFileId.Value ).IsTemporary = false;
            }

            if ( site.Id == 0 )
            {
                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();

                    //
                    // Save the API Key.
                    //
                    additionalSettings.ApiKeyId = SaveApiKey( additionalSettings.ApiKeyId, tbEditApiKey.Text, string.Format( "mobile_application_{0}", site.Id ), rockContext );
                    site.AdditionalSettings = additionalSettings.ToJson();

                    var pageService = new PageService( rockContext );
                    var layoutService = new LayoutService( rockContext );
                    var pageName = string.Format( "{0} Homepage", site.Name );

                    var layout = new Layout
                    {
                        Name = "Homepage",
                        FileName = "Homepage.xaml",
                        Description = string.Empty,
                        LayoutMobilePhone = _defaultLayoutXaml,
                        LayoutMobileTablet = _defaultLayoutXaml,
                        SiteId = site.Id
                    };

                    layoutService.Add( layout );
                    rockContext.SaveChanges();

                    var page = new Page
                    {
                        InternalName = pageName,
                        BrowserTitle = pageName,
                        PageTitle = pageName,
                        Description = string.Empty,
                        LayoutId = layout.Id,
                        DisplayInNavWhen = DisplayInNavWhen.WhenAllowed
                    };

                    pageService.Add( page );
                    rockContext.SaveChanges();

                    site.DefaultPageId = page.Id;
                    rockContext.SaveChanges();
                } );
            }
            else
            {
                //
                // Save the API Key.
                //
                additionalSettings.ApiKeyId = SaveApiKey( additionalSettings.ApiKeyId, tbEditApiKey.Text, string.Format( "mobile_application_{0}", site.Id ), rockContext );
                additionalSettings.DownhillSettings.Platform = Rock.DownhillCss.DownhillPlatform.Mobile;
                site.AdditionalSettings = additionalSettings.ToJson();

                rockContext.SaveChanges();
            }

            NavigateToCurrentPage( new Dictionary<string, string>
            {
                { "SiteId", site.Id.ToString() }
            } );
        }

        /// <summary>
        /// Handles the Click event of the lbStylesEditSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbStylesEditSave_Click( object sender, EventArgs e )
        {
            // Perform the edit save here
            using ( var rockContext = new RockContext() )
            {
                var siteService = new SiteService( rockContext );
                var binaryFileService = new BinaryFileService( rockContext );

                var site = siteService.Get( PageParameter( "SiteId" ).AsInteger() );
                var additionalSettings = site.AdditionalSettings.FromJsonOrNull<AdditionalSiteSettings>() ?? new AdditionalSiteSettings();

                site.FavIconBinaryFileId = imgEditHeaderImage.BinaryFileId;

                additionalSettings.BarBackgroundColor = ParseColor( cpEditBarBackgroundColor.Value );
                additionalSettings.MenuButtonColor = ParseColor( cpEditMenuButtonColor.Value );
                additionalSettings.ActivityIndicatorColor = ParseColor( cpEditActivityIndicatorColor.Value );
                additionalSettings.DownhillSettings.TextColor = ParseColor( cpTextColor.Value );
                additionalSettings.DownhillSettings.HeadingColor = ParseColor( cpHeadingColor.Value );
                additionalSettings.DownhillSettings.BackgroundColor = ParseColor( cpBackgroundColor.Value );

                additionalSettings.DownhillSettings.ApplicationColors.Primary = ParseColor( cpPrimary.Value );
                additionalSettings.DownhillSettings.ApplicationColors.Secondary = ParseColor( cpSecondary.Value );
                additionalSettings.DownhillSettings.ApplicationColors.Success = ParseColor( cpSuccess.Value );
                additionalSettings.DownhillSettings.ApplicationColors.Info = ParseColor( cpInfo.Value );
                additionalSettings.DownhillSettings.ApplicationColors.Danger = ParseColor( cpDanger.Value );
                additionalSettings.DownhillSettings.ApplicationColors.Warning = ParseColor( cpWarning.Value );
                additionalSettings.DownhillSettings.ApplicationColors.Light = ParseColor( cpLight.Value );
                additionalSettings.DownhillSettings.ApplicationColors.Dark = ParseColor( cpDark.Value );
                additionalSettings.DownhillSettings.ApplicationColors.Brand = ParseColor( cpBrand.Value );
                additionalSettings.DownhillSettings.ApplicationColors.Info = ParseColor( cpInfo.Value );

                additionalSettings.DownhillSettings.RadiusBase = nbRadiusBase.Text.AsDecimal();

                additionalSettings.DownhillSettings.FontSizeDefault = nbFontSizeDefault.Text.AsDecimal();
                additionalSettings.DownhillSettings.Platform = Rock.DownhillCss.DownhillPlatform.Mobile;

                additionalSettings.CssStyle = ceEditCssStyles.Text;

                site.AdditionalSettings = additionalSettings.ToJson();

                // Ensure the images are persisted.
                if ( site.FavIconBinaryFileId.HasValue )
                {
                    binaryFileService.Get( site.FavIconBinaryFileId.Value ).IsTemporary = false;
                }

                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the rblEditApplicationType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rblEditApplicationType_SelectedIndexChanged( object sender, EventArgs e )
        {
            rblEditAndroidTabLocation.Visible = rblEditApplicationType.SelectedValueAsInt() == ( int ) ShellType.Tabbed;
        }

        /// <summary>
        /// Handles the Click event of the lbTabApplication control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbTabApplication_Click( object sender, EventArgs e )
        {
            ShowTab( Tabs.Application );
        }

        /// <summary>
        /// Handles the Click event of the lbTabStyles control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbTabStyles_Click( object sender, EventArgs e )
        {
            ShowTab( Tabs.Styles );
        }

        /// <summary>
        /// Handles the Click event of the lbTabLayouts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbTabLayouts_Click( object sender, EventArgs e )
        {
            ShowTab( Tabs.Layouts );
        }

        /// <summary>
        /// Handles the Click event of the lbTabPages control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbTabPages_Click( object sender, EventArgs e )
        {
            ShowTab( Tabs.Pages );
        }

        /// <summary>
        /// Handles the Click event of the lbDeploy control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDeploy_Click( object sender, EventArgs e )
        {
            var applicationId = PageParameter( "SiteId" ).AsInteger();
            var deploymentDateTime = RockDateTime.Now;
            var versionId = ( int ) ( deploymentDateTime.ToJavascriptMilliseconds() / 1000 );

            //
            // Generate the packages and then encode to JSON.
            //
            var phonePackage = MobileHelper.BuildMobilePackage( applicationId, DeviceType.Phone, versionId );
            var tabletPackage = MobileHelper.BuildMobilePackage( applicationId, DeviceType.Tablet, versionId );
            var phoneJson = phonePackage.ToJson();
            var tabletJson = tabletPackage.ToJson();

            using ( var rockContext = new RockContext() )
            {
                var binaryFileService = new BinaryFileService( rockContext );
                var site = new SiteService( rockContext ).Get( applicationId );
                var binaryFileType = new BinaryFileTypeService( rockContext ).Get( Rock.SystemGuid.BinaryFiletype.MOBILE_APP_BUNDLE.AsGuid() );

                // Enable this once the shell updates have been installed.
                var enableCompression = false;
                var mimeType = enableCompression ? "application/gzip" : "application/json";
                var filenameExtension = enableCompression ? "json.gz" : "json";

                //
                // Prepare the phone configuration file.
                //
                Stream phoneJsonStream;
                if ( enableCompression )
                {
                    phoneJsonStream = new MemoryStream();
                    using ( var gzipStream = new GZipStream( phoneJsonStream, CompressionMode.Compress, true ) )
                    {
                        var bytes = Encoding.UTF8.GetBytes( phoneJson );
                        gzipStream.Write( bytes, 0, bytes.Length );
                    }
                    phoneJsonStream.Position = 0;
                }
                else
                {
                    phoneJsonStream = new MemoryStream( Encoding.UTF8.GetBytes( phoneJson ) );
                }

                var phoneFile = new BinaryFile
                {
                    IsTemporary = false,
                    BinaryFileTypeId = binaryFileType.Id,
                    MimeType = mimeType,
                    FileSize = phoneJson.Length,
                    FileName = "phone." + filenameExtension,
                    ContentStream = phoneJsonStream
                };
                binaryFileService.Add( phoneFile );

                //
                // Prepare the tablet configuration file.
                //
                Stream tabletJsonStream;
                if ( enableCompression )
                {
                    tabletJsonStream = new MemoryStream();
                    using ( var gzipStream = new GZipStream( tabletJsonStream, CompressionMode.Compress, true ) )
                    {
                        var bytes = Encoding.UTF8.GetBytes( tabletJson );
                        gzipStream.Write( bytes, 0, bytes.Length );
                    }
                    tabletJsonStream.Position = 0;
                }
                else
                {
                    tabletJsonStream = new MemoryStream( Encoding.UTF8.GetBytes( tabletJson ) );
                }

                var tabletFile = new BinaryFile
                {
                    IsTemporary = false,
                    BinaryFileTypeId = binaryFileType.Id,
                    MimeType = mimeType,
                    FileSize = tabletJson.Length,
                    FileName = "tablet." + filenameExtension,
                    ContentStream = tabletJsonStream
                };
                binaryFileService.Add( tabletFile );

                rockContext.SaveChanges();

                //
                // Remove old configuration files.
                //
                if ( site.ConfigurationMobilePhoneBinaryFile != null )
                {
                    site.ConfigurationMobilePhoneBinaryFile.IsTemporary = true;
                }

                if ( site.ConfigurationMobileTabletBinaryFile != null )
                {
                    site.ConfigurationMobileTabletBinaryFile.IsTemporary = true;
                }

                //
                // Set new configuration file references.
                //
                site.ConfigurationMobilePhoneBinaryFileId = phoneFile.Id;
                site.ConfigurationMobileTabletBinaryFileId = tabletFile.Id;

                //
                // Update the last deployment date.
                //
                var additionalSettings = site.AdditionalSettings.FromJsonOrNull<AdditionalSiteSettings>() ?? new AdditionalSiteSettings();
                additionalSettings.LastDeploymentDate = deploymentDateTime;
                additionalSettings.LastDeploymentVersionId = versionId;
                additionalSettings.PhoneUpdatePackageUrl = GetFilePath( phoneFile );
                additionalSettings.TabletUpdatePackageUrl = GetFilePath( tabletFile );
                site.AdditionalSettings = additionalSettings.ToJson();
                site.LatestVersionDateTime = RockDateTime.Now;

                rockContext.SaveChanges();

                ShowDetail( applicationId );
            }
        }

        #endregion

        #region Layouts Grid Event Handlers

        /// <summary>
        /// Handles the AddClick event of the gLayouts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gLayouts_AddClick( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.LayoutDetail, new Dictionary<string, string>
            {
                { "SiteId", hfSiteId.Value },
                { "LayoutId", "0" }
            } );
        }

        /// <summary>
        /// Handles the RowSelected event of the gLayouts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gLayouts_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.LayoutDetail, new Dictionary<string, string>
            {
                { "SiteId", hfSiteId.Value },
                { "LayoutId", e.RowKeyId.ToString() }
            } );
        }

        /// <summary>
        /// Handles the GridRebind event of the gLayouts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.GridRebindEventArgs"/> instance containing the event data.</param>
        protected void gLayouts_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            BindLayouts( hfSiteId.Value.AsInteger() );
        }

        /// <summary>
        /// Handles the DeleteClick event of the gLayouts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gLayouts_DeleteClick( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var rockContext = new RockContext();
            var layoutService = new LayoutService( rockContext );
            var layout = layoutService.Get( e.RowKeyId );

            layoutService.Delete( layout );

            rockContext.SaveChanges();

            BindLayouts( hfSiteId.ValueAsInt() );
        }

        #endregion

        #region Pages Grid Event Handlers

        /// <summary>
        /// Handles the AddClick event of the gPages control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gPages_AddClick( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.PageDetail, new Dictionary<string, string>
            {
                { "SiteId", hfSiteId.Value },
                { "Page", "0" }
            } );
        }

        /// <summary>
        /// Handles the RowSelected event of the gPages control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gPages_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.PageDetail, new Dictionary<string, string>
            {
                { "SiteId", hfSiteId.Value },
                { "Page", e.RowKeyId.ToString() }
            } );
        }

        /// <summary>
        /// Handles the GridRebind event of the gPages control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.GridRebindEventArgs"/> instance containing the event data.</param>
        protected void gPages_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            BindPages( hfSiteId.Value.AsInteger() );
        }

        /// <summary>
        /// Handles the DeleteClick event of the gPages control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gPages_DeleteClick( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var rockContext = new RockContext();
            var pageService = new PageService( rockContext );
            var page = pageService.Get( e.RowKeyId );

            if ( page != null )
            {
                string errorMessage;
                if ( !pageService.CanDelete( page, out errorMessage ) )
                {
                    mdWarning.Show( errorMessage, ModalAlertType.Warning );
                    return;
                }

                pageService.Delete( page );

                rockContext.SaveChanges();
            }

            BindPages( hfSiteId.ValueAsInt() );
        }

        /// <summary>
        /// Handles the GridReorder event of the gPages control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gPages_GridReorder( object sender, Rock.Web.UI.Controls.GridReorderEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                PageService pageService = new PageService( rockContext );
                var pages = pageService.GetBySiteId( hfSiteId.Value.AsInteger() )
                    .OrderBy( p => p.Order )
                    .ThenBy( p => p.InternalName )
                    .ToList();
                pageService.Reorder( pages, e.OldIndex, e.NewIndex );
                rockContext.SaveChanges();
            }

            BindPages( hfSiteId.Value.AsInteger() );
        }


        /// <summary>
        /// Handles the RowDataBound event of the gPages control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gPages_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType != DataControlRowType.DataRow )
            {
                return;
            }

            if ( hfSiteId.Value.IsNullOrWhiteSpace() || hfSiteId.Value.AsInteger() == 0 )
            {
                return;
            }

            int? defaultPageId = SiteCache.Get( hfSiteId.Value.AsInteger() ).DefaultPageId;
            if ( defaultPageId == null )
            {
                return;
            }
            
            var deleteField = gPages.ColumnsOfType<DeleteField>().FirstOrDefault();
            if ( deleteField == null || !deleteField.Visible )
            {
                return;
            }

            int? pageId = gPages.DataKeys[e.Row.RowIndex].Values[0].ToString().AsIntegerOrNull();
            if ( pageId == defaultPageId )
            {
                var deleteFieldColumnIndex = gPages.GetColumnIndex( deleteField );
                var deleteButton = e.Row.Cells[deleteFieldColumnIndex].ControlsOfTypeRecursive<LinkButton>().FirstOrDefault();
                if ( deleteButton != null )
                {
                    deleteButton.Visible = false;
                }
            }
        }

        #endregion
    }
}
