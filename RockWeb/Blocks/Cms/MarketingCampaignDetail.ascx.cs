// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.ComponentModel;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName("Marketing Campaign - Campaign Detail")]
    [Category("CMS")]
    [Description("Displays the details for campaign.")]
    //[LinkedPage( "Event Page" )]
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

            LoadCampusPicker();

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

            // Hide Event until we implement more stuff for Marketing Campaign Events
            ddlEventGroup.Visible = false;
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
                ShowDetail( PageParameter( "marketingCampaignId" ).AsInteger() );
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
                var service = new MarketingCampaignService( new RockContext() );
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
                MarketingCampaignService service = new MarketingCampaignService( new RockContext() );
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
            MarketingCampaignService service = new MarketingCampaignService( new RockContext() );
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

            HideSecondaryBlocks( editable );
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();

            MarketingCampaign marketingCampaign;

            MarketingCampaignService marketingCampaignService = new MarketingCampaignService( rockContext );

            int marketingCampaignId = int.Parse( hfMarketingCampaignId.Value );

            if ( marketingCampaignId == 0 )
            {
                marketingCampaign = new MarketingCampaign();
                marketingCampaignService.Add( marketingCampaign );
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

            rockContext.WrapTransaction( () =>
            {
                /* Save MarketingCampaignAudiences to db */
                if ( marketingCampaign.MarketingCampaignAudiences == null )
                {
                    marketingCampaign.MarketingCampaignAudiences = new List<MarketingCampaignAudience>();
                }

                // delete Audiences that aren't assigned in the UI anymore
                MarketingCampaignAudienceService marketingCampaignAudienceService = new MarketingCampaignAudienceService( rockContext );
                var deletedAudiences = from audienceInDB in marketingCampaign.MarketingCampaignAudiences.AsQueryable()
                                       where !( from audienceStateItem in MarketingCampaignAudiencesState
                                                select audienceStateItem.AudienceTypeValueId ).Contains( audienceInDB.AudienceTypeValueId )
                                       select audienceInDB;
                deletedAudiences.ToList().ForEach( a =>
                {
                    var aud = marketingCampaignAudienceService.Get( a.Guid );
                    marketingCampaignAudienceService.Delete( aud );
                } );

                rockContext.SaveChanges();

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
                MarketingCampaignCampusService marketingCampaignCampusService = new MarketingCampaignCampusService( rockContext );
                var deletedCampuses = from mcc in marketingCampaign.MarketingCampaignCampuses.AsQueryable()
                                      where !cpCampuses.SelectedCampusIds.Contains( mcc.CampusId )
                                      select mcc;

                deletedCampuses.ToList().ForEach( a =>
                {
                    var c = marketingCampaignCampusService.Get( a.Guid );
                    marketingCampaignCampusService.Delete( c );
                } );

                rockContext.SaveChanges();

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

                rockContext.SaveChanges();
            } );


            var qryParams = new Dictionary<string, string>();
            qryParams["marketingCampaignId"] = marketingCampaign.Id.ToString();
            NavigateToPage( RockPage.Guid, qryParams );
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            // Controls on Main Campaign Panel
            GroupService groupService = new GroupService( new RockContext() );
            List<Group> groups = groupService.Queryable().Where( a => a.GroupType.Guid.Equals( new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_EVENTATTENDEES ) ) ).OrderBy( a => a.Name ).ToList();
            groups.Insert( 0, new Group { Id = None.Id, Name = None.Text } );
            ddlEventGroup.DataSource = groups;
            ddlEventGroup.DataBind();
        }

        /// <summary>
        /// Loads the campus picker.
        /// </summary>
        private void LoadCampusPicker()
        {
            CampusService campusService = new CampusService( new RockContext() );

            cpCampuses.Campuses = campusService.Queryable().OrderBy( a => a.Name ).ToList();
            cpCampuses.Visible = cpCampuses.AvailableCampusIds.Count > 0;
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="marketingCampaignId">The marketing campaign identifier.</param>
        public void ShowDetail( int marketingCampaignId )
        {
            MarketingCampaign marketingCampaign = null;

            if ( !marketingCampaignId.Equals( 0 ) )
            {
                MarketingCampaignService marketingCampaignService = new MarketingCampaignService( new RockContext() );
                marketingCampaign = marketingCampaignService.Get( marketingCampaignId );
            }

            if (marketingCampaign == null)
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
            if ( !IsUserAuthorized( Authorization.EDIT ) )
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
                lActionTitle.Text = ActionTitle.Edit( MarketingCampaign.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lActionTitle.Text = ActionTitle.Add( MarketingCampaign.FriendlyTypeName ).FormatAsHtmlTitle();
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
            /* 
             * -- Hide until we implement Marketing Events
            
            string eventPageGuid = this.GetAttributeValue( "EventPage" );
            
            if ( marketingCampaign.EventGroup != null )
            {
                eventGroupHtml = marketingCampaign.EventGroup.Name;

                if ( !string.IsNullOrWhiteSpace( eventPageGuid ) )
                {
                    var page = new PageService( new RockContext() ).Get( new Guid( eventPageGuid ) );

                    Dictionary<string, string> queryString = new Dictionary<string, string>();
                    queryString.Add( "groupId", marketingCampaign.EventGroupId.ToString() );
                    string eventGroupUrl = new PageReference( page.Id, 0, queryString ).BuildUrl();
                    eventGroupHtml = string.Format( "<a href='{0}'>{1}</a>", eventGroupUrl, marketingCampaign.EventGroup.Name );
                }
            }
            */
            

            string primaryAudiences = marketingCampaign.MarketingCampaignAudiences.Where( a => a.IsPrimary ).Select( a => a.Name ).OrderBy( a => a ).ToList().AsDelimited( "<br>" );
            primaryAudiences = marketingCampaign.MarketingCampaignAudiences.Where( a => a.IsPrimary ).Count() == 0 ? None.TextHtml : primaryAudiences;

            string secondaryAudiences = marketingCampaign.MarketingCampaignAudiences.Where( a => !a.IsPrimary ).Select( a => a.Name ).OrderBy( a => a ).ToList().AsDelimited( "<br>" );
            secondaryAudiences = marketingCampaign.MarketingCampaignAudiences.Where( a => !a.IsPrimary ).Count() == 0 ? None.TextHtml : secondaryAudiences;

            lCampaignTitle.Text = marketingCampaign.Title.FormatAsHtmlTitle();

            DescriptionList descriptionListCol1 = new DescriptionList()
                .Add("Contact", contactInfo);

            if (eventGroupHtml != null)
            {
                descriptionListCol1.Add("Linked Event", eventGroupHtml);
            }

            lDetailsCol1.Text = descriptionListCol1.Html;

            DescriptionList descriptionListCol2 = new DescriptionList()
                .Add("Primary Audience", primaryAudiences)
                .Add("Secondary Audience", secondaryAudiences);

            lDetailsCol2.Text = descriptionListCol2.Html;

            lCampusLabels.Text = string.Empty;
            foreach (var campus in marketingCampaign.MarketingCampaignCampuses.Select(a => a.Campus.Name).OrderBy(a => a).ToList())
            {
                lCampusLabels.Text += "<span class='label label-campus'>" + campus + "</span> ";
            }
        }

        /// <summary>
        /// Handles the SelectPerson event of the ppContactPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void ppContactPerson_SelectPerson( object sender, EventArgs e )
        {
            Person contactPerson = new PersonService( new RockContext() ).Get( ppContactPerson.PersonId ?? 0 );
            if ( contactPerson != null )
            {
                tbContactEmail.Text = contactPerson.Email;
                tbContactFullName.Text = contactPerson.FullName;
                PhoneNumber phoneNumber = contactPerson.PhoneNumbers.FirstOrDefault( a => a.NumberTypeValue.Guid == new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ) );
                tbContactPhoneNumber.Text = phoneNumber != null ? phoneNumber.ToString() : string.Empty;
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
            DefinedValueService definedValueService = new DefinedValueService( new RockContext() );

            // populate dropdown with all MarketingCampaignAudiences that aren't already MarketingCampaignAudiences
            var guid = Rock.SystemGuid.DefinedType.MARKETING_CAMPAIGN_AUDIENCE_TYPE.AsGuid();
            var existingAudiences = MarketingCampaignAudiencesState.Select( s => s.AudienceTypeValueId ).ToList();
            var qry = definedValueService.GetByDefinedTypeGuid( guid )
                .Where( v => !existingAudiences.Contains( v.Id ) );

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
            marketingCampaignAudience.IsPrimary = hfMarketingCampaignAudienceIsPrimary.Value.AsBoolean();

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