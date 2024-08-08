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
using System.Linq;
using System.Web.UI;

using Newtonsoft.Json;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Tv.Classes;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Tv
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Apple TV Application Detail" )]
    [Category( "TV > TV Apps" )]
    [Description( "Allows a person to edit an Apple TV application." )]

    #region Block Attributes

    #endregion Block Attributes
    [Rock.SystemGuid.BlockTypeGuid( "49F3D87E-BD8D-43D4-8217-340F3DFF4562" )]
    public partial class AppleTvAppDetail : Rock.Web.UI.RockBlock
    {

        #region Attribute Keys

        private static class AttributeKey
        {
            //public const string ShowEmailAddress = "ShowEmailAddress";
        }

        #endregion Attribute Keys

        #region Page Parameter Keys

        private static class PageParameterKey
        {
            public const string SiteId = "SiteId";
        }

        #endregion PageParameterKeys

        #region Base Control Methods

        // Overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // This event gets fired after block settings are updated. It's nice to repaint the screen if these settings would alter it.
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                ShowView();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

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

            int? applicationId = PageParameter( pageReference, PageParameterKey.SiteId ).AsIntegerOrNull();

            if ( applicationId != null )
            {
                var site = SiteCache.Get( applicationId.Value );

                var detailBreadCrumb = pageReference.BreadCrumbs.FirstOrDefault( x => x.Name == "Application Detail" );
                if ( detailBreadCrumb != null )
                {
                    pageReference.BreadCrumbs.Remove( detailBreadCrumb );
                }

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

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowView();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            var applicationId = PageParameter( PageParameterKey.SiteId ).AsInteger();

            var rockContext = new RockContext();
            var siteService = new SiteService( rockContext );
            var site = siteService.Get( applicationId );

            var additionalSettings = new AppleTvApplicationSettings();
            var isNewSite = false;

            // Site is new so create one
            if ( site == null )
            {
                site = new Site();
                siteService.Add( site );
                isNewSite = true;
            }
            else
            {
                additionalSettings = JsonConvert.DeserializeObject<AppleTvApplicationSettings>( site.AdditionalSettings );
            }

            site.Name = tbApplicationName.Text;
            site.Description = tbDescription.Text;
            site.IsActive = cbIsActive.Checked;
            site.SiteType = SiteType.Tv;

            additionalSettings.ApplicationScript = ceApplicationJavaScript.Text;
            additionalSettings.ApplicationStyles = ceApplicationStyles.Text;
            additionalSettings.TvApplicationType = TvApplicationType.AppleTv;

            // Login page
            site.LoginPageId = ppLoginPage.PageId;
            site.LoginPageRouteId = ppLoginPage.PageRouteId;

            avcAttributes.GetEditValues( site );
            rockContext.WrapTransaction( () =>
            {
                rockContext.SaveChanges();
                site.SaveAttributeValues( rockContext );

                // Create/Modify API Key
                additionalSettings.ApiKeyId = SaveApiKey( additionalSettings.ApiKeyId, txtApiKey.Text, string.Format( "tv_application_{0}", site.Id ), rockContext );
                site.AdditionalSettings = additionalSettings.ToJson();
                rockContext.SaveChanges();
            } );

            // Create interaction channel for this site
            var interactionChannelService = new InteractionChannelService( rockContext );
            int channelMediumWebsiteValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_WEBSITE.AsGuid() ).Id;
            var interactionChannelForSite = interactionChannelService.Queryable()
                .Where( a => a.ChannelTypeMediumValueId == channelMediumWebsiteValueId && a.ChannelEntityId == site.Id ).FirstOrDefault();

            if ( interactionChannelForSite == null )
            {
                interactionChannelForSite = new InteractionChannel();
                interactionChannelForSite.ChannelTypeMediumValueId = channelMediumWebsiteValueId;
                interactionChannelForSite.ChannelEntityId = site.Id;
                interactionChannelService.Add( interactionChannelForSite );
            }

            interactionChannelForSite.Name = site.Name;
            interactionChannelForSite.RetentionDuration = nbPageViewRetentionPeriodDays.Text.AsIntegerOrNull();
            interactionChannelForSite.ComponentEntityTypeId = EntityTypeCache.Get<Rock.Model.Page>().Id;

            site.EnablePageViewGeoTracking = cbEnablePageViewGeoTracking.Checked;

            rockContext.SaveChanges();

            // If this is a new site then we also need to add a layout record and a 'default page'
            if ( isNewSite )
            {
                var layoutService = new LayoutService( rockContext );

                var layout = new Layout
                {
                    Name = "Homepage",
                    FileName = "Homepage.xaml",
                    Description = string.Empty,
                    SiteId = site.Id
                };

                layoutService.Add( layout );
                rockContext.SaveChanges();

                var pageService = new PageService( rockContext );
                var page = new Rock.Model.Page
                {
                    InternalName = "Start Screen",
                    BrowserTitle = "Start Screen",
                    PageTitle = "Start Screen",
                    DisplayInNavWhen = DisplayInNavWhen.WhenAllowed,
                    Description = string.Empty,
                    LayoutId = layout.Id,
                    Order = 0
                };

                pageService.Add( page );
                rockContext.SaveChanges();

                site.DefaultPageId = page.Id;
                rockContext.SaveChanges();
            }

            // If the save was successful, reload the page using the new record Id.
            var qryParams = new Dictionary<string, string>();
            qryParams[PageParameterKey.SiteId] = site.Id.ToString();

            NavigateToPage( RockPage.Guid, qryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            var siteId = PageParameter( PageParameterKey.SiteId );

            // If we are in the process of creating a new site, navigate back to the site list page.
            if ( siteId == null || siteId.AsInteger() == 0 )
            {
                NavigateToParentPage();
            }
            // Otherwise, navigate back to the current page with the siteId parameter.
            else
            {
                var qryParams = new Dictionary<string, string>();
                qryParams[PageParameterKey.SiteId] = PageParameter( PageParameterKey.SiteId );
                NavigateToPage( RockPage.Guid, qryParams );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            ShowEdit();
        }

        /// <summary>
        /// Handles the CheckedChanged event of the cbEnablePageViews control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbEnablePageViews_CheckedChanged( object sender, EventArgs e )
        {
            nbPageViewRetentionPeriodDays.Visible = cbEnablePageViews.Checked;
        }

        #endregion

        #region Methods

        private void ShowView()
        {
            pnlEdit.Visible = false;
            pnlView.Visible = true;

            // Show the page list block
            this.HideSecondaryBlocks( false );

            var applicationId = PageParameter( PageParameterKey.SiteId ).AsInteger();

            // We're being instructed to build a new site.
            if ( applicationId == 0 )
            {
                ShowEdit();
                return;
            }

            var rockContext = new RockContext();
            var site = new SiteService( rockContext ).Get( applicationId );

            if ( site == null )
            {
                nbMessages.Text = "Could not find the application.";
            }

            pdAuditDetails.SetEntity( site, ResolveRockUrl( "~" ) );

            hlblInactive.Visible = !site?.IsActive ?? true;

            DescriptionList viewContent = new DescriptionList();

            viewContent.Add( "Description", site.Description );
            viewContent.Add( "Enable Page Views", site.EnablePageViews.ToString() );

            // Display the page view retention duration
            if ( site.EnablePageViews )
            {
                int channelMediumWebsiteValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_WEBSITE.AsGuid() ).Id;
                var retentionDuration = new InteractionChannelService( new RockContext() ).Queryable()
                        .Where( c => c.ChannelTypeMediumValueId == channelMediumWebsiteValueId && c.ChannelEntityId == site.Id )
                        .Select( c => c.RetentionDuration )
                        .FirstOrDefault();

                if ( retentionDuration.HasValue )
                {
                    viewContent.Add( "Page View Retention", retentionDuration.Value.ToString() + " days" );
                }
            }

            // Get API key
            var additionalSettings = JsonConvert.DeserializeObject<AppleTvApplicationSettings>( site.AdditionalSettings );
            var apiKeyLogin = new UserLoginService( rockContext ).Get( additionalSettings.ApiKeyId ?? 0 );

            viewContent.Add( "API Key", apiKeyLogin != null ? apiKeyLogin.ApiKey : string.Empty );


            // Print the content to the screen
            lViewContent.Text = viewContent.Html;

            lBlockTitle.Text = site.Name;
        }

        /// <summary>
        /// Shows the details.
        /// </summary>
        private void ShowEdit()
        {
            pnlView.Visible = false;
            pnlEdit.Visible = true;
            pdAuditDetails.Visible = false;

            // Hide the page list block
            this.HideSecondaryBlocks( true );

            var applicationId = PageParameter( PageParameterKey.SiteId ).AsInteger();

            var rockContext = new RockContext();
            var site = new SiteService( rockContext ).Get( applicationId );

            ceApplicationJavaScript.Visible = PageParameter("ShowApplicationJs").AsBoolean();

            if ( site != null )
            {
                hlblInactive.Visible = !site?.IsActive ?? true;
                lBlockTitle.Text = site.Name;

                tbApplicationName.Text = site.Name;
                tbDescription.Text = site.Description;

                cbIsActive.Checked = site.IsActive;
                cbEnablePageViewGeoTracking.Checked = site.EnablePageViewGeoTracking;

                var additionalSettings = JsonConvert.DeserializeObject<AppleTvApplicationSettings>( site.AdditionalSettings );

                ceApplicationJavaScript.Text = additionalSettings.ApplicationScript;
                ceApplicationStyles.Text = additionalSettings.ApplicationStyles;

                cbEnablePageViews.Checked = site.EnablePageViews;

                site.LoadAttributes( rockContext );
                avcAttributes.AddEditControls( site, Rock.Security.Authorization.EDIT, CurrentPerson );

                // Login Page
                if ( site.LoginPageRoute != null )
                {
                    ppLoginPage.SetValue( site.LoginPageRoute );
                }
                else
                {
                    ppLoginPage.SetValue( site.LoginPage );
                }

                // Set the API key
                var apiKeyLogin = new UserLoginService( rockContext ).Get( additionalSettings.ApiKeyId ?? 0 );
                txtApiKey.Text = apiKeyLogin != null ? apiKeyLogin.ApiKey : GenerateApiKey();

                // Get page view retention
                int channelMediumWebsiteValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_WEBSITE.AsGuid() ).Id;
                var retentionDuration = new InteractionChannelService( new RockContext() ).Queryable()
                        .Where( c => c.ChannelTypeMediumValueId == channelMediumWebsiteValueId && c.ChannelEntityId == site.Id )
                        .Select( c => c.RetentionDuration )
                        .FirstOrDefault();

                if ( retentionDuration.HasValue )
                {
                    nbPageViewRetentionPeriodDays.Text = retentionDuration.Value.ToString();
                }

                nbPageViewRetentionPeriodDays.Visible = site.EnablePageViews;
            }
            else
            {
                var stream = typeof( Rock.Blocks.Tv.AppleTvAppDetail ).Assembly.GetManifestResourceStream( "Rock.Blocks.DefaultTvApplication.js" );
                
                if ( stream != null )
                {
                    using ( var reader = new StreamReader( stream ) )
                    {
                        ceApplicationJavaScript.Text = reader.ReadToEnd();
                    }
                }
            }
        }

        #endregion

        #region Private Methods
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
        /// <param name="rockContext">The rock context.</param>
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
            restPerson.LastName = tbApplicationName.Text;
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
        #endregion

    }
}