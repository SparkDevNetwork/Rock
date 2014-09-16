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
    public partial class ContentChannelDetail : RockBlock, IDetailBlock
    {
        #region Child Grid States

        /// <summary>
        /// Gets or sets the state of the marketing campaign audiences.
        /// </summary>
        /// <value>
        /// The state of the marketing campaign audiences.
        /// </value>
        private ViewStateList<ContentChannelAudience> ContentChannelAudiencesState
        {
            get
            {
                return ViewState["ContentChannelAudiencesState"] as ViewStateList<ContentChannelAudience>;
            }

            set
            {
                ViewState["ContentChannelAudiencesState"] = value;
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

            gContentChannelAudiencesPrimary.DataKeyNames = new string[] { "id" };
            gContentChannelAudiencesPrimary.Actions.ShowAdd = true;
            gContentChannelAudiencesPrimary.Actions.AddClick += gContentChannelAudiencesPrimary_Add;
            gContentChannelAudiencesPrimary.GridRebind += gContentChannelAudiences_GridRebind;
            gContentChannelAudiencesPrimary.EmptyDataText = Server.HtmlEncode( None.Text );

            gContentChannelAudiencesSecondary.DataKeyNames = new string[] { "id" };
            gContentChannelAudiencesSecondary.Actions.ShowAdd = true;
            gContentChannelAudiencesSecondary.Actions.AddClick += gContentChannelAudiencesSecondary_Add;
            gContentChannelAudiencesSecondary.GridRebind += gContentChannelAudiences_GridRebind;
            gContentChannelAudiencesSecondary.EmptyDataText = Server.HtmlEncode( None.Text );

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
                ShowDetail( PageParameter( "contentChannelId" ).AsInteger() );
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
            if ( int.TryParse( PageParameter( pageReference, "contentChannelId" ), out id ) )
            {
                var service = new ContentChannelService( new RockContext() );
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

            if ( hfContentChannelId.Value.Equals( "0" ) )
            {
                // Cancelling on Add.  Return to Grid
                NavigateToParentPage();
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                ContentChannelService service = new ContentChannelService( new RockContext() );
                ContentChannel item = service.Get( hfContentChannelId.ValueAsInt() );
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
            ContentChannelService service = new ContentChannelService( new RockContext() );
            int contentChannelId = hfContentChannelId.ValueAsInt();
            ContentChannel item = service.Queryable( "ContactPersonAlias.Person" )
                .FirstOrDefault( c => c.Id == contentChannelId );
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

            ContentChannel contentChannel;

            ContentChannelService contentChannelService = new ContentChannelService( rockContext );

            int contentChannelId = int.Parse( hfContentChannelId.Value );

            if ( contentChannelId == 0 )
            {
                contentChannel = new ContentChannel();
                contentChannelService.Add( contentChannel );
            }
            else
            {
                contentChannel = contentChannelService.Get( contentChannelId );
            }

            contentChannel.Title = tbTitle.Text;
            if ( ppContactPerson.PersonId.HasValue )
            {
                contentChannel.ContactPersonAliasId = new PersonAliasService( rockContext ).GetPrimaryAliasId( ppContactPerson.PersonId.Value );
            }
            else
            {
                contentChannel.ContactPersonAliasId = null;
            }

            contentChannel.ContactEmail = tbContactEmail.Text;
            contentChannel.ContactPhoneNumber = tbContactPhoneNumber.Text;
            contentChannel.ContactFullName = tbContactFullName.Text;

            if ( ddlEventGroup.SelectedValue.Equals( None.Id.ToString() ) )
            {
                contentChannel.EventGroupId = null;
            }
            else
            {
                contentChannel.EventGroupId = int.Parse( ddlEventGroup.SelectedValue );
            }

            if ( !contentChannel.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            rockContext.WrapTransaction( () =>
            {
                /* Save ContentChannelAudiences to db */
                if ( contentChannel.ContentChannelAudiences == null )
                {
                    contentChannel.ContentChannelAudiences = new List<ContentChannelAudience>();
                }

                // delete Audiences that aren't assigned in the UI anymore
                ContentChannelAudienceService contentChannelAudienceService = new ContentChannelAudienceService( rockContext );
                var deletedAudiences = from audienceInDB in contentChannel.ContentChannelAudiences.AsQueryable()
                                       where !( from audienceStateItem in ContentChannelAudiencesState
                                                select audienceStateItem.AudienceTypeValueId ).Contains( audienceInDB.AudienceTypeValueId )
                                       select audienceInDB;
                deletedAudiences.ToList().ForEach( a =>
                {
                    var aud = contentChannelAudienceService.Get( a.Guid );
                    contentChannelAudienceService.Delete( aud );
                } );

                rockContext.SaveChanges();

                // add or update the Audiences that are assigned in the UI
                foreach ( var item in ContentChannelAudiencesState )
                {
                    ContentChannelAudience contentChannelAudience = contentChannel.ContentChannelAudiences.FirstOrDefault( a => a.AudienceTypeValueId.Equals( item.AudienceTypeValueId ) );
                    if ( contentChannelAudience == null )
                    {
                        contentChannelAudience = new ContentChannelAudience();
                        contentChannel.ContentChannelAudiences.Add( contentChannelAudience );
                    }

                    contentChannelAudience.AudienceTypeValueId = item.AudienceTypeValueId;
                    contentChannelAudience.IsPrimary = item.IsPrimary;
                }

                /* Save ContentChannelCampuses to db */

                // Update ContentChannelCampuses with UI values
                if ( contentChannel.ContentChannelCampuses == null )
                {
                    contentChannel.ContentChannelCampuses = new List<ContentChannelCampus>();
                }

                // take care of deleted Campuses
                ContentChannelCampusService contentChannelCampusService = new ContentChannelCampusService( rockContext );
                var deletedCampuses = from mcc in contentChannel.ContentChannelCampuses.AsQueryable()
                                      where !cpCampuses.SelectedCampusIds.Contains( mcc.CampusId )
                                      select mcc;

                deletedCampuses.ToList().ForEach( a =>
                {
                    var c = contentChannelCampusService.Get( a.Guid );
                    contentChannelCampusService.Delete( c );
                } );

                rockContext.SaveChanges();

                // add or update the Campuses that are assigned in the UI
                foreach ( int campusId in cpCampuses.SelectedCampusIds )
                {
                    ContentChannelCampus contentChannelCampus = contentChannel.ContentChannelCampuses.FirstOrDefault( a => a.CampusId.Equals( campusId ) );
                    if ( contentChannelCampus == null )
                    {
                        contentChannelCampus = new ContentChannelCampus();
                        contentChannel.ContentChannelCampuses.Add( contentChannelCampus );
                    }

                    contentChannelCampus.CampusId = campusId;
                }

                rockContext.SaveChanges();
            } );


            var qryParams = new Dictionary<string, string>();
            qryParams["contentChannelId"] = contentChannel.Id.ToString();
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
        /// <param name="contentChannelId">The marketing campaign identifier.</param>
        public void ShowDetail( int contentChannelId )
        {
            ContentChannel contentChannel = null;

            if ( !contentChannelId.Equals( 0 ) )
            {
                ContentChannelService contentChannelService = new ContentChannelService( new RockContext() );
                contentChannel = contentChannelService.Queryable( "ContactPersonAlias.Person" )
                    .FirstOrDefault( c => c.Id == contentChannelId );
            }

            if (contentChannel == null)
            {
                contentChannel = new ContentChannel { Id = 0 };
                contentChannel.ContentItems = new List<ContentItem>();
                contentChannel.ContentChannelAudiences = new List<ContentChannelAudience>();
                contentChannel.ContentChannelCampuses = new List<ContentChannelCampus>();
            }

            hfContentChannelId.Value = contentChannel.Id.ToString();

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( ContentChannel.FriendlyTypeName );
            }

            if ( readOnly )
            {
                btnEdit.Visible = false;
                ShowReadonlyDetails( contentChannel );
            }
            else
            {
                btnEdit.Visible = true;
                if ( contentChannel.Id > 0 )
                {
                    ShowReadonlyDetails( contentChannel );
                }
                else
                {
                    ShowEditDetails( contentChannel );
                }
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="contentChannel">The marketing campaign.</param>
        private void ShowEditDetails( ContentChannel contentChannel )
        {
            if ( contentChannel.Id > 0 )
            {
                lActionTitle.Text = ActionTitle.Edit( ContentChannel.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lActionTitle.Text = ActionTitle.Add( ContentChannel.FriendlyTypeName ).FormatAsHtmlTitle();
            }

            SetEditMode( true );

            tbTitle.Text = contentChannel.Title;
            tbContactEmail.Text = contentChannel.ContactEmail;
            tbContactFullName.Text = contentChannel.ContactFullName;
            tbContactPhoneNumber.Text = contentChannel.ContactPhoneNumber;

            LoadDropDowns();
            if ( contentChannel.ContactPersonAlias != null )
            {
                ppContactPerson.SetValue( contentChannel.ContactPersonAlias.Person );
            }
            else
            {
                ppContactPerson.SetValue( null );
            }

            ddlEventGroup.SetValue( contentChannel.EventGroupId );

            cpCampuses.SelectedCampusIds = contentChannel.ContentChannelCampuses.Select( a => a.CampusId ).ToList();

            ContentChannelAudiencesState = new ViewStateList<ContentChannelAudience>();
            foreach ( var item in contentChannel.ContentChannelAudiences.ToList() )
            {
                ContentChannelAudiencesState.Add( new ContentChannelAudience { Id = item.Id, IsPrimary = item.IsPrimary, AudienceTypeValueId = item.AudienceTypeValueId } );
            }

            BindContentChannelAudiencesGrid();
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="contentChannel">The marketing campaign.</param>
        private void ShowReadonlyDetails( ContentChannel contentChannel )
        {
            SetEditMode( false );

            // set title.text value even though it is hidden so that Ad Edit can get the title of the campaign
            tbTitle.Text = contentChannel.Title;

            string contactInfo = string.Format( "{0}<br>{1}<br>{2}", contentChannel.ContactFullName, contentChannel.ContactEmail, contentChannel.ContactPhoneNumber );
            contactInfo = string.IsNullOrWhiteSpace( contactInfo.Replace( "<br>", string.Empty ) ) ? None.TextHtml : contactInfo;


            string eventGroupHtml = null;
            /* 
             * -- Hide until we implement Marketing Events
            
            string eventPageGuid = this.GetAttributeValue( "EventPage" );
            
            if ( contentChannel.EventGroup != null )
            {
                eventGroupHtml = contentChannel.EventGroup.Name;

                if ( !string.IsNullOrWhiteSpace( eventPageGuid ) )
                {
                    var page = new PageService( new RockContext() ).Get( new Guid( eventPageGuid ) );

                    Dictionary<string, string> queryString = new Dictionary<string, string>();
                    queryString.Add( "groupId", contentChannel.EventGroupId.ToString() );
                    string eventGroupUrl = new PageReference( page.Id, 0, queryString ).BuildUrl();
                    eventGroupHtml = string.Format( "<a href='{0}'>{1}</a>", eventGroupUrl, contentChannel.EventGroup.Name );
                }
            }
            */
            

            string primaryAudiences = contentChannel.ContentChannelAudiences.Where( a => a.IsPrimary ).Select( a => a.Name ).OrderBy( a => a ).ToList().AsDelimited( "<br>" );
            primaryAudiences = contentChannel.ContentChannelAudiences.Where( a => a.IsPrimary ).Count() == 0 ? None.TextHtml : primaryAudiences;

            string secondaryAudiences = contentChannel.ContentChannelAudiences.Where( a => !a.IsPrimary ).Select( a => a.Name ).OrderBy( a => a ).ToList().AsDelimited( "<br>" );
            secondaryAudiences = contentChannel.ContentChannelAudiences.Where( a => !a.IsPrimary ).Count() == 0 ? None.TextHtml : secondaryAudiences;

            lCampaignTitle.Text = contentChannel.Title.FormatAsHtmlTitle();

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
            foreach (var campus in contentChannel.ContentChannelCampuses.Select(a => a.Campus.Name).OrderBy(a => a).ToList())
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

        #region ContentChannelAudience Grid and Picker

        /// <summary>
        /// Handles the Add event of the gContentChannelAudiencesPrimary control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gContentChannelAudiencesPrimary_Add( object sender, EventArgs e )
        {
            gContentChannelAudiencesAdd( true );
        }

        /// <summary>
        /// Handles the Add event of the gContentChannelAudiencesSecondary control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gContentChannelAudiencesSecondary_Add( object sender, EventArgs e )
        {
            gContentChannelAudiencesAdd( false );
        }

        /// <summary>
        /// Gs the marketing campaign audiences add.
        /// </summary>
        /// <param name="primaryAudience">if set to <c>true</c> [is primary].</param>
        private void gContentChannelAudiencesAdd( bool primaryAudience )
        {
            DefinedValueService definedValueService = new DefinedValueService( new RockContext() );

            // populate dropdown with all ContentChannelAudiences that aren't already ContentChannelAudiences
            var guid = Rock.SystemGuid.DefinedType.MARKETING_CAMPAIGN_AUDIENCE_TYPE.AsGuid();
            var existingAudiences = ContentChannelAudiencesState.Select( s => s.AudienceTypeValueId ).ToList();
            var qry = definedValueService.GetByDefinedTypeGuid( guid )
                .Where( v => !existingAudiences.Contains( v.Id ) );

            List<DefinedValue> list = qry.ToList();
            if ( list.Count == 0 )
            {
                list.Add( new DefinedValue { Id = None.Id, Value = None.Text } );
                btnAddContentChannelAudience.Enabled = false;
                btnAddContentChannelAudience.CssClass = "btn btn-primary disabled";
            }
            else
            {
                btnAddContentChannelAudience.Enabled = true;
                btnAddContentChannelAudience.CssClass = "btn btn-primary";
            }

            ddlContentChannelAudiences.DataSource = list;
            ddlContentChannelAudiences.DataBind();
            hfContentChannelAudienceIsPrimary.Value = primaryAudience.ToTrueFalse();
            pnlContentChannelAudiencePicker.Visible = true;
            pnlDetails.Visible = false;
        }

        /// <summary>
        /// Handles the Delete event of the gContentChannelAudiences control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gContentChannelAudiences_Delete( object sender, RowEventArgs e )
        {
            int contentChannelAudienceId = (int)e.RowKeyValue;
            ContentChannelAudiencesState.RemoveEntity( contentChannelAudienceId );
            BindContentChannelAudiencesGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gContentChannelAudiences control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gContentChannelAudiences_GridRebind( object sender, EventArgs e )
        {
            BindContentChannelAudiencesGrid();
        }

        /// <summary>
        /// Binds the marketing campaign audiences grid.
        /// </summary>
        private void BindContentChannelAudiencesGrid()
        {
            gContentChannelAudiencesPrimary.DataSource = ContentChannelAudiencesState.Where( a => a.IsPrimary ).OrderBy( a => DefinedValueCache.Read(a.AudienceTypeValueId).Value ).ToList();
            gContentChannelAudiencesPrimary.DataBind();

            gContentChannelAudiencesSecondary.DataSource = ContentChannelAudiencesState.Where( a => !a.IsPrimary ).OrderBy( a => DefinedValueCache.Read( a.AudienceTypeValueId ).Value ).ToList();
            gContentChannelAudiencesSecondary.DataBind();
        }

        /// <summary>
        /// Handles the Click event of the btnAddContentChannelAudience control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnAddContentChannelAudience_Click( object sender, EventArgs e )
        {
            int audienceTypeValueId = int.Parse( ddlContentChannelAudiences.SelectedValue );
            ContentChannelAudience contentChannelAudience = new ContentChannelAudience();
            contentChannelAudience.AudienceTypeValueId = audienceTypeValueId;
            contentChannelAudience.IsPrimary = hfContentChannelAudienceIsPrimary.Value.AsBoolean();

            ContentChannelAudiencesState.Add( contentChannelAudience.Clone() as ContentChannelAudience );

            pnlContentChannelAudiencePicker.Visible = false;
            pnlDetails.Visible = true;

            BindContentChannelAudiencesGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnCancelAddContentChannelAudience control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancelAddContentChannelAudience_Click( object sender, EventArgs e )
        {
            pnlContentChannelAudiencePicker.Visible = false;
            pnlDetails.Visible = true;
        }

        #endregion
    }
}