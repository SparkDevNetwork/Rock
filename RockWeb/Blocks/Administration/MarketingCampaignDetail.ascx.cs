//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// 
    /// </summary>
    [AdditionalActions( new string[] { "Approve" } )]
    [LinkedPage("Event Page", "EventPageGuid")]
    public partial class MarketingCampaignDetail : RockBlock, IDetailBlock
    {
        #region Child Grid States

        /// <summary>
        /// Gets or sets the state of the marketing campaign audiences.
        /// </summary>
        /// <value>
        /// The state of the marketing campaign audiences.
        /// </value>
        private ViewStateList<MarketingCampaignAudience> MarketingCampaignAudiencesState
        {
            get
            {
                return ViewState["MarketingCampaignAudiencesState"] as ViewStateList<MarketingCampaignAudience>;
            }

            set
            {
                ViewState["MarketingCampaignAudiencesState"] = value;
            }
        }

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gMarketingCampaignAds.DataKeyNames = new string[] { "Id" };
            gMarketingCampaignAds.Actions.AddClick += gMarketingCampaignAds_Add;
            gMarketingCampaignAds.GridRebind += gMarketingCampaignAds_GridRebind;
            gMarketingCampaignAds.EmptyDataText = Server.HtmlEncode( None.Text );

            gMarketingCampaignAudiencesPrimary.DataKeyNames = new string[] { "id" };
            gMarketingCampaignAudiencesPrimary.Actions.IsAddEnabled = true;
            gMarketingCampaignAudiencesPrimary.Actions.AddClick += gMarketingCampaignAudiencesPrimary_Add;
            gMarketingCampaignAudiencesPrimary.GridRebind += gMarketingCampaignAudiences_GridRebind;
            gMarketingCampaignAudiencesPrimary.EmptyDataText = Server.HtmlEncode( None.Text );

            gMarketingCampaignAudiencesSecondary.DataKeyNames = new string[] { "id" };
            gMarketingCampaignAudiencesSecondary.Actions.IsAddEnabled = true;
            gMarketingCampaignAudiencesSecondary.Actions.AddClick += gMarketingCampaignAudiencesSecondary_Add;
            gMarketingCampaignAudiencesSecondary.GridRebind += gMarketingCampaignAudiences_GridRebind;
            gMarketingCampaignAudiencesSecondary.EmptyDataText = Server.HtmlEncode( None.Text );

            // Block Security on Ads grid (RockPage takes care of "View")
            bool canAddEditDelete = IsUserAuthorized( "Edit" );
            gMarketingCampaignAds.Actions.IsAddEnabled = canAddEditDelete;
            gMarketingCampaignAds.IsDeleteEnabled = canAddEditDelete;
            gMarketingCampaignAds.Actions.IsAddEnabled = canAddEditDelete;
            gMarketingCampaignAds.IsDeleteEnabled = canAddEditDelete;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                string itemId = PageParameter( "marketingCampaignId" );
                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    ShowDetail( "marketingCampaignId", int.Parse( itemId ) );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }

            if ( pnlMarketingCampaignAdEditor.Visible )
            {
                LoadAdAttributes( new MarketingCampaignAd(), true, false );
            }
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            SetEditMode( false );

            if ( hfMarketingCampaignId.Value.Equals( "0" ) )
            {
                // Cancelling on Add.  Return to Grid
                NavigateToParentPage();
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                MarketingCampaignService service = new MarketingCampaignService();
                MarketingCampaign item = service.Get( hfMarketingCampaignId.ValueAsInt() );
                ShowReadonlyDetails( item );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            MarketingCampaignService service = new MarketingCampaignService();
            MarketingCampaign item = service.Get( hfMarketingCampaignId.ValueAsInt() );
            ShowEditDetails( item );
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            fieldsetViewDetails.Visible = !editable;

            pnlMarketingCampaignAds.Disabled = editable;
            gMarketingCampaignAds.Enabled = !editable;
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            MarketingCampaign marketingCampaign;
            MarketingCampaignService marketingCampaignService = new MarketingCampaignService();

            int marketingCampaignId = int.Parse( hfMarketingCampaignId.Value );

            if ( marketingCampaignId == 0 )
            {
                marketingCampaign = new MarketingCampaign();
                marketingCampaignService.Add( marketingCampaign, CurrentPersonId );
            }
            else
            {
                marketingCampaign = marketingCampaignService.Get( marketingCampaignId );
            }

            marketingCampaign.Title = tbTitle.Text;
            if ( ppContactPerson.SelectedValue.Equals( None.Id.ToString() ) )
            {
                marketingCampaign.ContactPersonId = null;
            }
            else
            {
                marketingCampaign.ContactPersonId = int.Parse( ppContactPerson.SelectedValue );
            }

            marketingCampaign.ContactEmail = tbContactEmail.Text;
            marketingCampaign.ContactPhoneNumber = tbContactPhoneNumber.Text;
            marketingCampaign.ContactFullName = tbContactFullName.Text;

            if ( ddlEventGroup.SelectedValue.Equals( None.Id.ToString() ) )
            {
                marketingCampaign.EventGroupId = null;
            }
            else
            {
                marketingCampaign.EventGroupId = int.Parse( ddlEventGroup.SelectedValue );
            }

            // check for duplicates
            if ( marketingCampaignService.Queryable().Count( a => a.Title.Equals( marketingCampaign.Title, StringComparison.OrdinalIgnoreCase ) && !a.Id.Equals( marketingCampaign.Id ) ) > 0 )
            {
                tbTitle.ShowErrorMessage( WarningMessage.DuplicateFoundMessage( "title", MarketingCampaign.FriendlyTypeName ) );
                return;
            }

            if ( !marketingCampaign.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }
            
            RockTransactionScope.WrapTransaction( () =>
            {
                /* Save MarketingCampaignAudiences to db */
                if ( marketingCampaign.MarketingCampaignAudiences == null )
                {
                    marketingCampaign.MarketingCampaignAudiences = new List<MarketingCampaignAudience>();
                }

                // delete Audiences that aren't assigned in the UI anymore
                MarketingCampaignAudienceService marketingCampaignAudienceService = new MarketingCampaignAudienceService();
                var deletedAudiences = from audienceInDB in marketingCampaign.MarketingCampaignAudiences.AsQueryable()
                                       where !( from audienceStateItem in MarketingCampaignAudiencesState
                                                select audienceStateItem.AudienceTypeValueId ).Contains( audienceInDB.AudienceTypeValueId )
                                       select audienceInDB;
                deletedAudiences.ToList().ForEach( a =>
                {
                    var aud = marketingCampaignAudienceService.Get( a.Guid );
                    marketingCampaignAudienceService.Delete( aud, CurrentPersonId );
                    marketingCampaignAudienceService.Save( aud, CurrentPersonId );
                } );

                // add or update the Audiences that are assigned in the UI
                foreach ( var item in MarketingCampaignAudiencesState )
                {
                    MarketingCampaignAudience marketingCampaignAudience = marketingCampaign.MarketingCampaignAudiences.FirstOrDefault( a => a.AudienceTypeValueId.Equals( item.AudienceTypeValueId ) );
                    if ( marketingCampaignAudience == null )
                    {
                        marketingCampaignAudience = new MarketingCampaignAudience();
                        marketingCampaign.MarketingCampaignAudiences.Add( marketingCampaignAudience );
                    }

                    marketingCampaignAudience.AudienceTypeValueId = item.AudienceTypeValueId;
                    marketingCampaignAudience.IsPrimary = item.IsPrimary;
                }

                /* Save MarketingCampaignCampuses to db */

                // Update MarketingCampaignCampuses with UI values
                if ( marketingCampaign.MarketingCampaignCampuses == null )
                {
                    marketingCampaign.MarketingCampaignCampuses = new List<MarketingCampaignCampus>();
                }

                // take care of deleted Campuses
                MarketingCampaignCampusService marketingCampaignCampusService = new MarketingCampaignCampusService();
                var deletedCampuses = from mcc in marketingCampaign.MarketingCampaignCampuses.AsQueryable()
                                      where !cpCampuses.SelectedCampusIds.Contains( mcc.CampusId )
                                      select mcc;

                deletedCampuses.ToList().ForEach( a =>
                {
                    var c = marketingCampaignCampusService.Get( a.Guid );
                    marketingCampaignCampusService.Delete( c, CurrentPersonId );
                    marketingCampaignCampusService.Save( c, CurrentPersonId );
                } );

                // add or update the Campuses that are assigned in the UI
                foreach ( int campusId in cpCampuses.SelectedCampusIds )
                {
                    MarketingCampaignCampus marketingCampaignCampus = marketingCampaign.MarketingCampaignCampuses.FirstOrDefault( a => a.CampusId.Equals( campusId ) );
                    if ( marketingCampaignCampus == null )
                    {
                        marketingCampaignCampus = new MarketingCampaignCampus();
                        marketingCampaign.MarketingCampaignCampuses.Add( marketingCampaignCampus );
                    }

                    marketingCampaignCampus.CampusId = campusId;
                }

                marketingCampaignService.Save( marketingCampaign, CurrentPersonId );
            } );

            hfMarketingCampaignId.Value = marketingCampaign.Id.ToString();

            // refresh from db using new service context since the above child items were saved outside of the marketingCampaign object
            marketingCampaign = new MarketingCampaignService().Get( marketingCampaign.Id );
            ShowReadonlyDetails( marketingCampaign );
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            // Controls on Main Campaign Panel
            GroupService groupService = new GroupService();
            List<Group> groups = groupService.Queryable().Where( a => a.GroupType.Guid.Equals( Rock.SystemGuid.GroupType.GROUPTYPE_EVENTATTENDEES ) ).OrderBy( a => a.Name ).ToList();
            groups.Insert( 0, new Group { Id = None.Id, Name = None.Text } );
            ddlEventGroup.DataSource = groups;
            ddlEventGroup.DataBind();

            CampusService campusService = new CampusService();

            cpCampuses.Campuses = campusService.Queryable().OrderBy( a => a.Name ).ToList();
            cpCampuses.Visible = cpCampuses.AvailableCampusIds.Count > 0;
        }

        /// <summary>
        /// Loads the drop downs ads.
        /// </summary>
        private void LoadDropDownsAds()
        {
            // Controls on Ad Child Panel
            MarketingCampaignAdTypeService marketingCampaignAdTypeService = new MarketingCampaignAdTypeService();
            var adtypes = marketingCampaignAdTypeService.Queryable().OrderBy( a => a.Name ).ToList();
            ddlMarketingCampaignAdType.DataSource = adtypes;
            ddlMarketingCampaignAdType.DataBind();
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            if ( !itemKey.Equals( "marketingCampaignId" ) )
            {
                return;
            }

            MarketingCampaign marketingCampaign = null;

            if ( !itemKeyValue.Equals( 0 ) )
            {
                MarketingCampaignService marketingCampaignService = new MarketingCampaignService();
                marketingCampaign = marketingCampaignService.Get( itemKeyValue );
            }
            else
            {
                marketingCampaign = new MarketingCampaign { Id = 0 };
                marketingCampaign.MarketingCampaignAds = new List<MarketingCampaignAd>();
                marketingCampaign.MarketingCampaignAudiences = new List<MarketingCampaignAudience>();
                marketingCampaign.MarketingCampaignCampuses = new List<MarketingCampaignCampus>();
            }

            hfMarketingCampaignId.Value = marketingCampaign.Id.ToString();

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( "Edit" ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( MarketingCampaign.FriendlyTypeName );
            }

            if ( readOnly )
            {
                btnEdit.Visible = false;
                ShowReadonlyDetails( marketingCampaign );
            }
            else
            {
                btnEdit.Visible = true;
                if ( marketingCampaign.Id > 0 )
                {
                    ShowReadonlyDetails( marketingCampaign );
                }
                else
                {
                    ShowEditDetails( marketingCampaign );
                }
            }

            BindMarketingCampaignAdsGrid();
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="marketingCampaign">The marketing campaign.</param>
        private void ShowEditDetails( MarketingCampaign marketingCampaign )
        {
            if ( marketingCampaign.Id > 0 )
            {
                lActionTitle.Text = ActionTitle.Edit( MarketingCampaign.FriendlyTypeName );
            }
            else
            {
                lActionTitle.Text = ActionTitle.Add( MarketingCampaign.FriendlyTypeName );
            }

            SetEditMode( true );

            tbTitle.Text = marketingCampaign.Title;
            tbContactEmail.Text = marketingCampaign.ContactEmail;
            tbContactFullName.Text = marketingCampaign.ContactFullName;
            tbContactPhoneNumber.Text = marketingCampaign.ContactPhoneNumber;

            LoadDropDowns();
            ppContactPerson.SetValue( marketingCampaign.ContactPerson );
            ddlEventGroup.SetValue( marketingCampaign.EventGroupId );

            cpCampuses.SelectedCampusIds = marketingCampaign.MarketingCampaignCampuses.Select( a => a.CampusId ).ToList();

            MarketingCampaignAudiencesState = new ViewStateList<MarketingCampaignAudience>();
            MarketingCampaignAudiencesState.AddAll( marketingCampaign.MarketingCampaignAudiences.ToList() );

            BindMarketingCampaignAudiencesGrid();
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="marketingCampaign">The marketing campaign.</param>
        private void ShowReadonlyDetails( MarketingCampaign marketingCampaign )
        {
            SetEditMode( false );
            
            // set title.text value even though it is hidden so that Ad Edit can get the title of the campaign
            tbTitle.Text = marketingCampaign.Title;
            
            // make a Description section for nonEdit mode
            string descriptionFormat = "<dt>{0}</dt><dd>{1}</dd>";
            lblMainDetails.Text = @"
<div class='span6'>
    <dl>";

            lblMainDetails.Text += string.Format( descriptionFormat, "Title", marketingCampaign.Title );

            string contactInfo = string.Format( "{0}<br>{1}<br>{2}", marketingCampaign.ContactFullName, marketingCampaign.ContactEmail, marketingCampaign.ContactPhoneNumber );
            contactInfo = string.IsNullOrWhiteSpace( contactInfo.Replace( "<br>", string.Empty ) ) ? None.TextHtml : contactInfo;
            lblMainDetails.Text += string.Format( descriptionFormat, "Contact", contactInfo );

            if ( marketingCampaign.EventGroup != null )
            {
                string eventGroupHtml = marketingCampaign.EventGroup.Name;
                string eventPageGuid = this.GetAttributeValue( "EventPageGuid" );

                if ( !string.IsNullOrWhiteSpace( eventPageGuid ) )
                {
                    var page = new PageService().Get(new Guid(eventPageGuid));

                    Dictionary<string, string> queryString = new Dictionary<string, string>();
                    queryString.Add( "groupId", marketingCampaign.EventGroupId.ToString() );
                    string eventGroupUrl = CurrentPage.BuildUrl( page.Id, queryString );
                    eventGroupHtml = string.Format( "<a href='{0}'>{1}</a>", eventGroupUrl, marketingCampaign.EventGroup.Name );
                }

                lblMainDetails.Text += string.Format( descriptionFormat, "Linked Event", eventGroupHtml );
            }

            lblMainDetails.Text += @"
    </dl>
</div>
<div class='span6'>
    <dl>";

            string campusList = marketingCampaign.MarketingCampaignCampuses.Select( a => a.Campus.Name ).OrderBy( a => a ).ToList().AsDelimited( "<br>" );
            if ( marketingCampaign.MarketingCampaignCampuses.Count > 0 )
            {
                lblMainDetails.Text += string.Format( descriptionFormat, "Campuses", campusList );
            }

            string primaryAudiences = marketingCampaign.MarketingCampaignAudiences.Where( a => a.IsPrimary ).Select( a => a.Name ).OrderBy( a => a ).ToList().AsDelimited( "<br>" );
            primaryAudiences = marketingCampaign.MarketingCampaignAudiences.Where( a => a.IsPrimary ).Count() == 0 ? None.TextHtml : primaryAudiences;
            lblMainDetails.Text += string.Format( descriptionFormat, "Primary Audience", primaryAudiences );

            string secondaryAudiences = marketingCampaign.MarketingCampaignAudiences.Where( a => !a.IsPrimary ).Select( a => a.Name ).OrderBy( a => a ).ToList().AsDelimited( "<br>" );
            secondaryAudiences = marketingCampaign.MarketingCampaignAudiences.Where( a => !a.IsPrimary ).Count() == 0 ? None.TextHtml : secondaryAudiences;
            lblMainDetails.Text += string.Format( descriptionFormat, "Secondary Audience", secondaryAudiences );

            lblMainDetails.Text += @"
    </dl>
</div>";
        }

        /// <summary>
        /// Handles the SelectPerson event of the ppContactPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void ppContactPerson_SelectPerson( object sender, EventArgs e )
        {
            int personId = int.Parse( ppContactPerson.SelectedValue );
            Person contactPerson = new PersonService().Get( personId );
            if ( contactPerson != null )
            {
                tbContactEmail.Text = contactPerson.Email;
                tbContactFullName.Text = contactPerson.FullName;
                PhoneNumber phoneNumber = contactPerson.PhoneNumbers.FirstOrDefault( a => a.NumberTypeValue.Guid == Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_PRIMARY );
                tbContactPhoneNumber.Text = phoneNumber == null ? string.Empty : phoneNumber.Number;
            }
        }

        #endregion

        #region MarketingCampaignAds Grid and Editor

        /// <summary>
        /// Handles the Add event of the gMarketingCampaignAds control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gMarketingCampaignAds_Add( object sender, EventArgs e )
        {
            gMarketingCampaignAds_ShowEdit( 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gMarketingCampaignAds control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gMarketingCampaignAds_Edit( object sender, RowEventArgs e )
        {
            gMarketingCampaignAds_ShowEdit( (int)e.RowKeyValue );
        }

        /// <summary>
        /// Gs the marketing campaign ads_ show edit.
        /// </summary>
        /// <param name="marketingCampaignAdId">The marketing campaign ad id.</param>
        protected void gMarketingCampaignAds_ShowEdit( int marketingCampaignAdId )
        {
            hfMarketingCampaignAdId.Value = marketingCampaignAdId.ToString();

            btnApproveAd.Visible = IsUserAuthorized( "Approve" );
            btnDenyAd.Visible = IsUserAuthorized( "Approve" );

            MarketingCampaignAd marketingCampaignAd = null;

            if ( !marketingCampaignAdId.Equals( 0 ) )
            {
                marketingCampaignAd = new MarketingCampaignAdService().Get( marketingCampaignAdId );
                marketingCampaignAd.LoadAttributes();
            }
            else
            {
                marketingCampaignAd = new MarketingCampaignAd { Id = 0, MarketingCampaignAdStatus = MarketingCampaignAdStatus.PendingApproval };
            }

            if ( marketingCampaignAd.Id.Equals( 0 ) )
            {
                lActionTitleAd.Text = ActionTitle.Add( "Marketing Ad for " + tbTitle.Text);
            }
            else
            {
                lActionTitleAd.Text = ActionTitle.Edit( "Marketing Ad for " + tbTitle.Text );
            }

            LoadDropDownsAds();
            ddlMarketingCampaignAdType.SetValue( marketingCampaignAd.MarketingCampaignAdTypeId );
            tbPriority.Text = marketingCampaignAd.Priority.ToString();

            SetApprovalValues( marketingCampaignAd.MarketingCampaignAdStatus, new PersonService().Get( marketingCampaignAd.MarketingCampaignStatusPersonId ?? 0 ) );

            if ( marketingCampaignAdId.Equals( 0 ) )
            {
                tbAdDateRangeStartDate.SelectedDate = null;
                tbAdDateRangeEndDate.SelectedDate = null;
            }
            else
            {
                tbAdDateRangeStartDate.SelectedDate = marketingCampaignAd.StartDate;
                tbAdDateRangeEndDate.SelectedDate = marketingCampaignAd.EndDate;
            }

            tbUrl.Text = marketingCampaignAd.Url;

            LoadAdAttributes( marketingCampaignAd, true, true );

            pnlMarketingCampaignAdEditor.Visible = true;
            pnlDetails.Visible = false;
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlMarketingCampaignAdType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void ddlMarketingCampaignAdType_SelectedIndexChanged( object sender, EventArgs e )
        {
            MarketingCampaignAd marketingCampaignAd = new MarketingCampaignAd();

            LoadAdAttributes( marketingCampaignAd, false, false );
            Rock.Attribute.Helper.GetEditValues( phAttributes, marketingCampaignAd );

            SetApprovalValues( MarketingCampaignAdStatus.PendingApproval, null );

            LoadAdAttributes( marketingCampaignAd, true, true );
        }

        /// <summary>
        /// Loads the attribute controls.
        /// </summary>
        /// <param name="marketingCampaignAd">The marketing campaign ad.</param>
        /// <param name="createControls">if set to <c>true</c> [create controls].</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void LoadAdAttributes( Rock.Attribute.IHasAttributes marketingCampaignAd, bool createControls, bool setValues )
        {
            if ( string.IsNullOrWhiteSpace( ddlMarketingCampaignAdType.SelectedValue ) )
            {
                return;
            }

            int marketingAdTypeId = int.Parse( ddlMarketingCampaignAdType.SelectedValue );

            MarketingCampaignAdType marketingCampaignAdType = new MarketingCampaignAdTypeService().Get( marketingAdTypeId );
            tbAdDateRangeEndDate.Visible = marketingCampaignAdType.DateRangeType.Equals( DateRangeTypeEnum.DateRange );

            List<Rock.Web.Cache.AttributeCache> attributesForAdType = GetAttributesForAdType( marketingAdTypeId );

            marketingCampaignAd.Attributes = marketingCampaignAd.Attributes ?? new Dictionary<string, Rock.Web.Cache.AttributeCache>();
            marketingCampaignAd.AttributeCategories = marketingCampaignAd.AttributeCategories ?? new SortedDictionary<string, List<string>>();
            marketingCampaignAd.AttributeValues = marketingCampaignAd.AttributeValues ?? new Dictionary<string, List<AttributeValue>>();
            foreach ( var attribute in attributesForAdType )
            {
                marketingCampaignAd.Attributes[attribute.Key] = attribute;
                if ( marketingCampaignAd.AttributeValues.Count( v => v.Key.Equals( attribute.Key ) ) == 0 )
                {
                    List<AttributeValue> attributeValues = new List<AttributeValue>();
                    attributeValues.Add( new AttributeValue { Value = attribute.DefaultValue } );
                    marketingCampaignAd.AttributeValues.Add( attribute.Key, attributeValues );
                }
            }

            foreach ( var category in attributesForAdType.Select( a => a.Category ).Distinct() )
            {
                marketingCampaignAd.AttributeCategories[category] = attributesForAdType.Where( a => a.Category.Equals( category ) ).Select( a => a.Key ).ToList();
            }

            if ( createControls )
            {
                phAttributes.Controls.Clear();
                Rock.Attribute.Helper.AddEditControls( marketingCampaignAd, phAttributes, setValues );
            }
        }

        /// <summary>
        /// Gets the type of the attributes for ad.
        /// </summary>
        /// <param name="marketingAdTypeId">The marketing ad type id.</param>
        /// <returns></returns>
        private static List<Rock.Web.Cache.AttributeCache> GetAttributesForAdType( int marketingAdTypeId )
        {
            MarketingCampaignAd temp = new MarketingCampaignAd();
            temp.MarketingCampaignAdTypeId = marketingAdTypeId;
            temp.LoadAttributes();
            return temp.Attributes.Values.ToList();
        }

        /// <summary>
        /// Handles the Delete event of the gMarketingCampaignAds control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gMarketingCampaignAds_Delete( object sender, RowEventArgs e )
        {
            MarketingCampaignAdService marketingCampaignAdService = new MarketingCampaignAdService();
            MarketingCampaignAd marketingCampaignAd = marketingCampaignAdService.Get( (int)e.RowKeyValue );

            marketingCampaignAdService.Delete( marketingCampaignAd, CurrentPersonId );
            marketingCampaignAdService.Save( marketingCampaignAd, CurrentPersonId );
            BindMarketingCampaignAdsGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gMarketingCampaignAds control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gMarketingCampaignAds_GridRebind( object sender, EventArgs e )
        {
            BindMarketingCampaignAdsGrid();
        }

        /// <summary>
        /// Binds the marketing campaign ads grid.
        /// </summary>
        private void BindMarketingCampaignAdsGrid()
        {
            MarketingCampaignAdService marketingCampaignAdService = new MarketingCampaignAdService();
            int marketingCampaignId = int.Parse( hfMarketingCampaignId.Value );
            var qry = marketingCampaignAdService.Queryable().Where( a => a.MarketingCampaignId.Equals( marketingCampaignId ) );

            gMarketingCampaignAds.DataSource = qry.OrderBy( a => a.StartDate ).ThenBy( a => a.Priority ).ThenBy( a => a.MarketingCampaignAdType.Name ).ToList();
            gMarketingCampaignAds.DataBind();
        }

        /// <summary>
        /// Handles the Click event of the btnSaveAd control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSaveAd_Click( object sender, EventArgs e )
        {
            int marketingCampaignAdId = int.Parse( hfMarketingCampaignAdId.Value );
            MarketingCampaignAdService marketingCampaignAdService = new MarketingCampaignAdService();

            MarketingCampaignAd marketingCampaignAd;
            if ( marketingCampaignAdId.Equals( 0 ) )
            {
                marketingCampaignAd = new MarketingCampaignAd { Id = 0 };
            }
            else
            {
                marketingCampaignAd = marketingCampaignAdService.Get( marketingCampaignAdId );
            }

            if ( string.IsNullOrWhiteSpace( ddlMarketingCampaignAdType.SelectedValue ) )
            {
                ddlMarketingCampaignAdType.ShowErrorMessage( WarningMessage.CannotBeBlank( ddlMarketingCampaignAdType.LabelText ) );
                return;
            }

            marketingCampaignAd.MarketingCampaignId = int.Parse( hfMarketingCampaignId.Value );
            marketingCampaignAd.MarketingCampaignAdTypeId = int.Parse( ddlMarketingCampaignAdType.SelectedValue );
            marketingCampaignAd.Priority = tbPriority.Text.AsInteger() ?? 0;
            marketingCampaignAd.MarketingCampaignAdStatus = (MarketingCampaignAdStatus)int.Parse( hfMarketingCampaignAdStatus.Value );
            if ( !string.IsNullOrWhiteSpace( hfMarketingCampaignAdStatusPersonId.Value ) )
            {
                marketingCampaignAd.MarketingCampaignStatusPersonId = int.Parse( hfMarketingCampaignAdStatusPersonId.Value );
            }
            else
            {
                marketingCampaignAd.MarketingCampaignStatusPersonId = null;
            }

            if ( tbAdDateRangeStartDate.SelectedDate == null )
            {
                tbAdDateRangeStartDate.ShowErrorMessage( Rock.Constants.WarningMessage.CannotBeBlank( "StartDate" ) );
                return;
            }

            marketingCampaignAd.StartDate = tbAdDateRangeStartDate.SelectedDate ?? DateTime.MinValue;

            if ( tbAdDateRangeEndDate.Visible )
            {
                if ( tbAdDateRangeEndDate.SelectedDate == null )
                {
                    tbAdDateRangeEndDate.ShowErrorMessage( Rock.Constants.WarningMessage.CannotBeBlank( "EndDate" ) );
                    return;
                }

                marketingCampaignAd.EndDate = tbAdDateRangeEndDate.SelectedDate ?? DateTime.MaxValue;
            }
            else
            {
                marketingCampaignAd.EndDate = marketingCampaignAd.StartDate;
            }

            if ( marketingCampaignAd.EndDate < marketingCampaignAd.StartDate )
            {
                tbAdDateRangeStartDate.ShowErrorMessage( WarningMessage.DateRangeEndDateBeforeStartDate() );
            }

            marketingCampaignAd.Url = tbUrl.Text;

            LoadAdAttributes( marketingCampaignAd, false, false );
            Rock.Attribute.Helper.GetEditValues( phAttributes, marketingCampaignAd );
            Rock.Attribute.Helper.SetErrorIndicators( phAttributes, marketingCampaignAd );

            if ( !Page.IsValid )
            {
                return;
            }

            if ( !marketingCampaignAd.IsValid )
            {
                return;
            }

            RockTransactionScope.WrapTransaction( () =>
                {
                    if ( marketingCampaignAd.Id.Equals( 0 ) )
                    {
                        marketingCampaignAdService.Add( marketingCampaignAd, CurrentPersonId );
                    }

                    marketingCampaignAdService.Save( marketingCampaignAd, CurrentPersonId );
                    Rock.Attribute.Helper.SaveAttributeValues( marketingCampaignAd, CurrentPersonId );
                } );

            pnlMarketingCampaignAdEditor.Visible = false;
            pnlDetails.Visible = true;

            BindMarketingCampaignAdsGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnCancelAd control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancelAd_Click( object sender, EventArgs e )
        {
            pnlMarketingCampaignAdEditor.Visible = false;
            pnlDetails.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the btnApproveAd control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnApproveAd_Click( object sender, EventArgs e )
        {
            SetApprovalValues( MarketingCampaignAdStatus.Approved, CurrentPerson );
        }

        /// <summary>
        /// Handles the Click event of the btnDenyAd control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnDenyAd_Click( object sender, EventArgs e )
        {
            SetApprovalValues( MarketingCampaignAdStatus.Denied, CurrentPerson );
        }

        /// <summary>
        /// Sets the approval values.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <param name="person">The person.</param>
        private void SetApprovalValues( MarketingCampaignAdStatus status, Person person )
        {
            ltMarketingCampaignAdStatus.Text = status.ConvertToString();
            switch ( status )
            {
                case MarketingCampaignAdStatus.Approved: ltMarketingCampaignAdStatus.TextCssClass = "alert MarketingCampaignAdStatus alert-success";
                    break;
                case MarketingCampaignAdStatus.Denied: ltMarketingCampaignAdStatus.TextCssClass = "alert MarketingCampaignAdStatus alert-error";
                    break;
                default: ltMarketingCampaignAdStatus.TextCssClass = "alert MarketingCampaignAdStatus alert-info";
                    break;
            }

            hfMarketingCampaignAdStatus.Value = status.ConvertToInt().ToString();
            lblMarketingCampaignAdStatusPerson.Visible = person != null;
            if ( person != null )
            {
                lblMarketingCampaignAdStatusPerson.Text = "by " + person.FullName;
                hfMarketingCampaignAdStatusPersonId.Value = person.Id.ToString();
            }
        }

        #endregion

        #region MarketingCampaignAudience Grid and Picker

        /// <summary>
        /// Handles the Add event of the gMarketingCampaignAudiencesPrimary control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gMarketingCampaignAudiencesPrimary_Add( object sender, EventArgs e )
        {
            gMarketingCampaignAudiencesAdd( true );
        }

        /// <summary>
        /// Handles the Add event of the gMarketingCampaignAudiencesSecondary control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gMarketingCampaignAudiencesSecondary_Add( object sender, EventArgs e )
        {
            gMarketingCampaignAudiencesAdd( false );
        }

        /// <summary>
        /// Gs the marketing campaign audiences add.
        /// </summary>
        /// <param name="primaryAudience">if set to <c>true</c> [is primary].</param>
        private void gMarketingCampaignAudiencesAdd( bool primaryAudience )
        {
            DefinedValueService definedValueService = new DefinedValueService();

            // populate dropdown with all MarketingCampaignAudiences that aren't already MarketingCampaignAudiences
            var qry = from audienceTypeValue in definedValueService.GetByDefinedTypeGuid( Rock.SystemGuid.DefinedType.MARKETING_CAMPAIGN_AUDIENCE_TYPE ).AsQueryable()
                      where !( from mcaudience in MarketingCampaignAudiencesState
                               select mcaudience.AudienceTypeValueId ).Contains( audienceTypeValue.Id )
                      select audienceTypeValue;

            List<DefinedValue> list = qry.ToList();
            if ( list.Count == 0 )
            {
                list.Add( new DefinedValue { Id = None.Id, Name = None.Text } );
                btnAddMarketingCampaignAudience.Enabled = false;
                btnAddMarketingCampaignAudience.CssClass = "btn btn-primary disabled";
            }
            else
            {
                btnAddMarketingCampaignAudience.Enabled = true;
                btnAddMarketingCampaignAudience.CssClass = "btn btn-primary";
            }

            ddlMarketingCampaignAudiences.DataSource = list;
            ddlMarketingCampaignAudiences.DataBind();
            hfMarketingCampaignAudienceIsPrimary.Value = primaryAudience.ToTrueFalse();
            pnlMarketingCampaignAudiencePicker.Visible = true;
            pnlDetails.Visible = false;
        }

        /// <summary>
        /// Handles the Delete event of the gMarketingCampaignAudiences control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gMarketingCampaignAudiences_Delete( object sender, RowEventArgs e )
        {
            int marketingCampaignAudienceId = (int)e.RowKeyValue;
            MarketingCampaignAudiencesState.RemoveEntity( marketingCampaignAudienceId );
            BindMarketingCampaignAudiencesGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gMarketingCampaignAudiences control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gMarketingCampaignAudiences_GridRebind( object sender, EventArgs e )
        {
            BindMarketingCampaignAudiencesGrid();
        }

        /// <summary>
        /// Binds the marketing campaign audiences grid.
        /// </summary>
        private void BindMarketingCampaignAudiencesGrid()
        {
            gMarketingCampaignAudiencesPrimary.DataSource = MarketingCampaignAudiencesState.Where( a => a.IsPrimary ).OrderBy( a => a.AudienceTypeValue.Name ).ToList();
            gMarketingCampaignAudiencesPrimary.DataBind();

            gMarketingCampaignAudiencesSecondary.DataSource = MarketingCampaignAudiencesState.Where( a => !a.IsPrimary ).OrderBy( a => a.AudienceTypeValue.Name ).ToList();
            gMarketingCampaignAudiencesSecondary.DataBind();
        }

        /// <summary>
        /// Handles the Click event of the btnAddMarketingCampaignAudience control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnAddMarketingCampaignAudience_Click( object sender, EventArgs e )
        {
            int audienceTypeValueId = int.Parse( ddlMarketingCampaignAudiences.SelectedValue );
            MarketingCampaignAudience marketingCampaignAudience = new MarketingCampaignAudience();
            marketingCampaignAudience.AudienceTypeValueId = audienceTypeValueId;
            marketingCampaignAudience.AudienceTypeValue = new DefinedValueService().Get( audienceTypeValueId );
            marketingCampaignAudience.IsPrimary = hfMarketingCampaignAudienceIsPrimary.Value.FromTrueFalse();

            MarketingCampaignAudiencesState.Add( marketingCampaignAudience.Clone() as MarketingCampaignAudience );

            pnlMarketingCampaignAudiencePicker.Visible = false;
            pnlDetails.Visible = true;

            BindMarketingCampaignAudiencesGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnCancelAddMarketingCampaignAudience control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancelAddMarketingCampaignAudience_Click( object sender, EventArgs e )
        {
            pnlMarketingCampaignAudiencePicker.Visible = false;
            pnlDetails.Visible = true;
        }

        #endregion
    }
}