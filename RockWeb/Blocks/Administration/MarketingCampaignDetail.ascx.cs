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
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// 
    /// </summary>
    [LinkedPage( "Event Page" )]
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

            gMarketingCampaignAudiencesPrimary.DataKeyNames = new string[] { "id" };
            gMarketingCampaignAudiencesPrimary.Actions.ShowAdd = true;
            gMarketingCampaignAudiencesPrimary.Actions.AddClick += gMarketingCampaignAudiencesPrimary_Add;
            gMarketingCampaignAudiencesPrimary.GridRebind += gMarketingCampaignAudiences_GridRebind;
            gMarketingCampaignAudiencesPrimary.EmptyDataText = Server.HtmlEncode( None.Text );

            gMarketingCampaignAudiencesSecondary.DataKeyNames = new string[] { "id" };
            gMarketingCampaignAudiencesSecondary.Actions.ShowAdd = true;
            gMarketingCampaignAudiencesSecondary.Actions.AddClick += gMarketingCampaignAudiencesSecondary_Add;
            gMarketingCampaignAudiencesSecondary.GridRebind += gMarketingCampaignAudiences_GridRebind;
            gMarketingCampaignAudiencesSecondary.EmptyDataText = Server.HtmlEncode( None.Text );

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
        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// </summary>
        /// <param name="pageReference">The page reference.</param>
        /// <returns></returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int id = int.MinValue;
            if ( int.TryParse( PageParameter( pageReference, "marketingCampaignId" ), out id ) )
            {
                var service = new MarketingCampaignService();
                var item = service.Get( id );
                if ( item != null )
                {
                    breadCrumbs.Add( new BreadCrumb( item.Title, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Marketing Campaign", pageReference ) );
                }
            }

            return breadCrumbs;
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

            DimOtherBlocks( editable );
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
                marketingCampaign.ContactPersonId = ppContactPerson.PersonId;
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


            var qryParams = new Dictionary<string, string>();
            qryParams["marketingCampaignId"] = marketingCampaign.Id.ToString();
            NavigateToPage( this.CurrentPage.Guid, qryParams );
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
            List<Group> groups = groupService.Queryable().Where( a => a.GroupType.Guid.Equals( new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_EVENTATTENDEES ) ) ).OrderBy( a => a.Name ).ToList();
            groups.Insert( 0, new Group { Id = None.Id, Name = None.Text } );
            ddlEventGroup.DataSource = groups;
            ddlEventGroup.DataBind();

            CampusService campusService = new CampusService();

            cpCampuses.Campuses = campusService.Queryable().OrderBy( a => a.Name ).ToList();
            cpCampuses.Visible = cpCampuses.AvailableCampusIds.Count > 0;
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
            foreach ( var item in marketingCampaign.MarketingCampaignAudiences.ToList() )
            {
                MarketingCampaignAudiencesState.Add( new MarketingCampaignAudience { Id = item.Id, IsPrimary = item.IsPrimary, AudienceTypeValueId = item.AudienceTypeValueId } );
            }

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

            string contactInfo = string.Format( "{0}<br>{1}<br>{2}", marketingCampaign.ContactFullName, marketingCampaign.ContactEmail, marketingCampaign.ContactPhoneNumber );
            contactInfo = string.IsNullOrWhiteSpace( contactInfo.Replace( "<br>", string.Empty ) ) ? None.TextHtml : contactInfo;

            string eventGroupHtml = null;
            string eventPageGuid = this.GetAttributeValue( "EventPage" );
            
            if ( marketingCampaign.EventGroup != null )
            {
                eventGroupHtml = marketingCampaign.EventGroup.Name;

                if ( !string.IsNullOrWhiteSpace( eventPageGuid ) )
                {
                    var page = new PageService().Get( new Guid( eventPageGuid ) );

                    Dictionary<string, string> queryString = new Dictionary<string, string>();
                    queryString.Add( "groupId", marketingCampaign.EventGroupId.ToString() );
                    string eventGroupUrl = new PageReference( page.Id, 0, queryString ).BuildUrl();
                    eventGroupHtml = string.Format( "<a href='{0}'>{1}</a>", eventGroupUrl, marketingCampaign.EventGroup.Name );
                }
            }

            string campusList = marketingCampaign.MarketingCampaignCampuses.Select( a => a.Campus.Name ).OrderBy( a => a ).ToList().AsDelimited( "<br>" );

            string primaryAudiences = marketingCampaign.MarketingCampaignAudiences.Where( a => a.IsPrimary ).Select( a => a.Name ).OrderBy( a => a ).ToList().AsDelimited( "<br>" );
            primaryAudiences = marketingCampaign.MarketingCampaignAudiences.Where( a => a.IsPrimary ).Count() == 0 ? None.TextHtml : primaryAudiences;

            string secondaryAudiences = marketingCampaign.MarketingCampaignAudiences.Where( a => !a.IsPrimary ).Select( a => a.Name ).OrderBy( a => a ).ToList().AsDelimited( "<br>" );
            secondaryAudiences = marketingCampaign.MarketingCampaignAudiences.Where( a => !a.IsPrimary ).Count() == 0 ? None.TextHtml : secondaryAudiences;

            DescriptionList descriptionList = new DescriptionList()
                .Add("Title", marketingCampaign.Title)
                .Add("Contact", contactInfo);

            if (eventGroupHtml != null)
            {
                descriptionList.Add("Linked Event", eventGroupHtml);
            }

            if ( marketingCampaign.MarketingCampaignCampuses.Count > 0 )
            {
                descriptionList.Add("Campuses", campusList);
            }

            descriptionList
                .Add( "Primary Audience", primaryAudiences )
                .Add( "Secondary Audience", secondaryAudiences );

            lblMainDetails.Text = descriptionList.Html;
        }

        /// <summary>
        /// Handles the SelectPerson event of the ppContactPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void ppContactPerson_SelectPerson( object sender, EventArgs e )
        {
            Person contactPerson = new PersonService().Get( ppContactPerson.PersonId ?? 0 );
            if ( contactPerson != null )
            {
                tbContactEmail.Text = contactPerson.Email;
                tbContactFullName.Text = contactPerson.FullName;
                PhoneNumber phoneNumber = contactPerson.PhoneNumbers.FirstOrDefault( a => a.NumberTypeValue.Guid == new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ) );
                tbContactPhoneNumber.Text = phoneNumber == null ? string.Empty : phoneNumber.Number;
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
            var qry = from audienceTypeValue in definedValueService.GetByDefinedTypeGuid( new Guid( Rock.SystemGuid.DefinedType.MARKETING_CAMPAIGN_AUDIENCE_TYPE ) ).AsQueryable()
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
            gMarketingCampaignAudiencesPrimary.DataSource = MarketingCampaignAudiencesState.Where( a => a.IsPrimary ).OrderBy( a => DefinedValueCache.Read(a.AudienceTypeValueId).Name ).ToList();
            gMarketingCampaignAudiencesPrimary.DataBind();

            gMarketingCampaignAudiencesSecondary.DataSource = MarketingCampaignAudiencesState.Where( a => !a.IsPrimary ).OrderBy( a => DefinedValueCache.Read( a.AudienceTypeValueId ).Name ).ToList();
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