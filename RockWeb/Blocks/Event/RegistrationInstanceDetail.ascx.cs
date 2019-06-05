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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Newtonsoft.Json;

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

namespace RockWeb.Blocks.Event
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Registration Instance Detail" )]
    [Category( "Event" )]
    [Description( "Template block for editing an event registration instance." )]
    [AccountField( "Default Account", "The default account to use for new registration instances", false, "2A6F9E5F-6859-44F1-AB0E-CE9CF6B08EE5", "", 0 )]
    [LinkedPage( "Registration Page", "The page for editing registration and registrant information", true, "", "", 1 )]
    [LinkedPage( "Linkage Page", "The page for editing registration linkages", true, "", "", 2 )]
    [LinkedPage( "Calendar Item Page", "The page to view calendar item details", true, "", "", 3 )]
    [LinkedPage( "Group Detail Page", "The page for viewing details about a group", true, "", "", 4 )]
    [LinkedPage( "Content Item Page", "The page for viewing details about a content channel item", true, "", "", 5 )]
    [LinkedPage( "Transaction Detail Page", "The page for viewing details about a payment", true, "", "", 6 )]
    [LinkedPage( "Payment Reminder Page", "The page for manually sending payment reminders.", false, "", "", 7 )]
    [LinkedPage( "Wait List Process Page", "The page for moving a person from the wait list to a full registrant.", true, "", "", 8 )]
    [BooleanField( "Display Discount Codes", "Display the discount code used with a payment", false, "", 9 )]
    public partial class RegistrationInstanceDetail : Rock.Web.UI.RockBlock, IDetailBlock
    {
        #region Fields

        private List<FinancialTransactionDetail> registrationPayments;
        private List<Registration> paymentRegistrations;
        private bool _instanceHasCost = false;
        private Dictionary<int, Location> _homeAddresses = new Dictionary<int, Location>();
        private Dictionary<int, PhoneNumber> _mobilePhoneNumbers = new Dictionary<int, PhoneNumber>();
        private Dictionary<int, PhoneNumber> _homePhoneNumbers = new Dictionary<int, PhoneNumber>();
        private List<int> _waitListOrder = null;
        private bool _isExporting = false;

        /// <summary>
        /// Gets or sets the available registration attributes where IsGridColumn = true
        /// </summary>
        /// <value>
        /// The available attributes.
        /// </value>
        public List<AttributeCache> AvailableRegistrationAttributesForGrid { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the registrant form fields that were configured as 'Show on Grid' for the registration template
        /// </summary>
        /// <value>
        /// The registrant fields.
        /// </value>
        public List<RegistrantFormField> RegistrantFields { get; set; }

        /// <summary>
        /// Gets or sets the person campus ids.
        /// </summary>
        /// <value>
        /// The person campus ids.
        /// </value>
        private Dictionary<int, List<int>> PersonCampusIds { get; set; }

        /// <summary>
        /// Gets or sets the signed person ids.
        /// </summary>
        /// <value>
        /// The signed person ids.
        /// </value>
        private List<int> Signers { get; set; }

        /// <summary>
        /// Gets or sets the group links.
        /// </summary>
        /// <value>
        /// The group links.
        /// </value>
        private Dictionary<int, string> GroupLinks { get; set; }

        /// <summary>
        /// Gets or sets the active tab.
        /// </summary>
        /// <value>
        /// The active tab.
        /// </value>
        protected string ActiveTab { get; set; }

        /// <summary>
        /// Gets or sets the registration template identifier.
        /// </summary>
        /// <value>
        /// The registration template identifier.
        /// </value>
        protected int? RegistrationTemplateId { get; set; }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            AvailableRegistrationAttributesForGrid = ViewState["AvailableRegistrationAttributesForGrid"] as List<AttributeCache>;

            ActiveTab = ( ViewState["ActiveTab"] as string ) ?? string.Empty;
            RegistrantFields = ViewState["RegistrantFields"] as List<RegistrantFormField>;
            RegistrationTemplateId = ViewState["RegistrationTemplateId"] as int? ?? 0;

            // don't set the values if this is a postback from a grid 'ClearFilter'
            bool setValues = this.Request.Params["__EVENTTARGET"] == null || !this.Request.Params["__EVENTTARGET"].EndsWith( "_lbClearFilter" );
            SetUserPreferencePrefix( RegistrationTemplateId.Value );
            AddDynamicControls( setValues );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // NOTE: The 3 Grid Filters had a bug where all were sing the same "Date Range" key in a prior version and they didn't really work quite right, so wipe them out
            fRegistrations.SaveUserPreference( "Date Range", null );
            fRegistrants.SaveUserPreference( "Date Range", null );
            fPayments.SaveUserPreference( "Date Range", null );

            fRegistrations.ApplyFilterClick += fRegistrations_ApplyFilterClick;
            gRegistrations.DataKeyNames = new string[] { "Id" };
            gRegistrations.Actions.ShowAdd = true;
            gRegistrations.Actions.AddClick += gRegistrations_AddClick;
            gRegistrations.RowDataBound += gRegistrations_RowDataBound;
            gRegistrations.GridRebind += gRegistrations_GridRebind;
            gRegistrations.ShowConfirmDeleteDialog = false;

            ddlRegistrantsInGroup.Items.Clear();
            ddlRegistrantsInGroup.Items.Add( new ListItem());
            ddlRegistrantsInGroup.Items.Add( new ListItem( "Yes", "Yes" ) );
            ddlRegistrantsInGroup.Items.Add( new ListItem( "No", "No" ) );

            ddlGroupPlacementsInGroup.Items.Clear();
            ddlGroupPlacementsInGroup.Items.Add( new ListItem() );
            ddlGroupPlacementsInGroup.Items.Add( new ListItem( "Yes", "Yes" ) );
            ddlGroupPlacementsInGroup.Items.Add( new ListItem( "No", "No" ) );

            ddlRegistrantsSignedDocument.Items.Clear();
            ddlRegistrantsSignedDocument.Items.Add( new ListItem() );
            ddlRegistrantsSignedDocument.Items.Add( new ListItem( "Yes", "Yes" ) );
            ddlRegistrantsSignedDocument.Items.Add( new ListItem( "No", "No" ) );

            ddlGroupPlacementsSignedDocument.Items.Clear();
            ddlGroupPlacementsSignedDocument.Items.Add( new ListItem() );
            ddlGroupPlacementsSignedDocument.Items.Add( new ListItem( "Yes", "Yes" ) );
            ddlGroupPlacementsSignedDocument.Items.Add( new ListItem( "No", "No" ) );

            fRegistrants.ApplyFilterClick += fRegistrants_ApplyFilterClick;
            gRegistrants.DataKeyNames = new string[] { "Id" };
            gRegistrants.Actions.ShowAdd = true;
            gRegistrants.Actions.AddClick += gRegistrants_AddClick;
            gRegistrants.RowDataBound += gRegistrants_RowDataBound;
            gRegistrants.GridRebind += gRegistrants_GridRebind;

            fWaitList.ApplyFilterClick += fWaitList_ApplyFilterClick;
            gWaitList.DataKeyNames = new string[] { "Id" };
            gWaitList.Actions.ShowAdd = true;
            gWaitList.Actions.AddClick += gWaitList_AddClick;
            gWaitList.RowDataBound += gWaitList_RowDataBound;
            gWaitList.GridRebind += gWaitList_GridRebind;

            // add button to the wait list action grid
            Button btnProcessWaitlist = new Button();
            btnProcessWaitlist.CssClass = "pull-left margin-l-none btn btn-sm btn-default";
            btnProcessWaitlist.Text = "Move From Wait List";
            btnProcessWaitlist.Click += btnProcessWaitlist_Click;
            gWaitList.Actions.AddCustomActionControl( btnProcessWaitlist );

            fPayments.ApplyFilterClick += fPayments_ApplyFilterClick;
            gPayments.DataKeyNames = new string[] { "Id" };
            gPayments.Actions.ShowAdd = false;
            gPayments.RowDataBound += gPayments_RowDataBound;
            gPayments.GridRebind += gPayments_GridRebind;

            fLinkages.ApplyFilterClick += fLinkages_ApplyFilterClick;
            gLinkages.DataKeyNames = new string[] { "Id" };
            gLinkages.Actions.ShowAdd = true;
            gLinkages.Actions.AddClick += gLinkages_AddClick;
            gLinkages.RowDataBound += gLinkages_RowDataBound;
            gLinkages.GridRebind += gLinkages_GridRebind;

            fGroupPlacements.ApplyFilterClick += fGroupPlacements_ApplyFilterClick;
            gGroupPlacements.DataKeyNames = new string[] { "Id" };
            gGroupPlacements.Actions.ShowAdd = false;
            gGroupPlacements.RowDataBound += gRegistrants_RowDataBound; // Intentionally using same row data bound event as the gRegistrants grid
            gGroupPlacements.GridRebind += gGroupPlacements_GridRebind;

            fFees.ApplyFilterClick += fFees_ApplyFilterClick;
            gFees.GridRebind += gFees_GridRebind;

            fDiscounts.ApplyFilterClick += fDiscounts_ApplyFilterClick;
            gDiscounts.GridRebind += gDiscounts_GridRebind;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            string deleteScript = @"
    $('a.js-delete-instance').click(function( e ){
        e.preventDefault();
        Rock.dialogs.confirm('Are you sure you want to delete this registration instance? All of the registrations and registrants will also be deleted!', function (result) {
            if (result) {
                if ( $('input.js-instance-has-payments').val() == 'True' ) {
                    Rock.dialogs.confirm('This registration instance also has registrations with payments. Are you sure that you want to delete the instance?<br/><small>(Payments will not be deleted, but they will no longer be associated with a registration.)</small>', function (result) {
                        if (result) {
                            window.location = e.target.href ? e.target.href : e.target.parentElement.href;
                        }
                    });
                } else {
                    window.location = e.target.href ? e.target.href : e.target.parentElement.href;
                }
            }
        });
    });

    $('table.js-grid-registration a.grid-delete-button').click(function( e ){
        e.preventDefault();
        var $hfHasPayments = $(this).closest('tr').find('input.js-has-payments').first();
        Rock.dialogs.confirm('Are you sure you want to delete this registration? All of the registrants will also be deleted!', function (result) {
            if (result) {
                if ( $hfHasPayments.val() == 'True' ) {
                    Rock.dialogs.confirm('This registration also has payments. Are you sure that you want to delete the registration?<br/><small>(Payments will not be deleted, but they will no longer be associated with a registration.)</small>', function (result) {
                        if (result) {
                            window.location = e.target.href ? e.target.href : e.target.parentElement.href;
                        }
                    });
                } else {
                    window.location = e.target.href ? e.target.href : e.target.parentElement.href;
                }
            }
        });
    });
";
            ScriptManager.RegisterStartupScript( btnDelete, btnDelete.GetType(), "deleteInstanceScript", deleteScript, true );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbPlacementNotifiction.Visible = false;

            if ( !Page.IsPostBack )
            {
                int? tab = PageParameter( "Tab" ).AsIntegerOrNull();
                if ( tab.HasValue )
                {
                    switch ( tab.Value )
                    {
                        case 1:
                            ActiveTab = "lbRegistrations";
                            break;

                        case 2:
                            ActiveTab = "lbRegistrants";
                            break;

                        case 3:
                            ActiveTab = "lbPayments";
                            break;

                        case 4:
                            ActiveTab = "lbLinkage";
                            break;

                        case 5:
                            ActiveTab = "lbGroupPlacement";
                            break;

                        case 6:
                            ActiveTab = "lbFees";
                            break;

                        case 7:
                            ActiveTab = "lbDiscounts";
                            break;
                    }
                }

                ShowDetail();
            }
            else
            {
                SetFollowingOnPostback();
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["RegistrantFields"] = RegistrantFields;
            ViewState["ActiveTab"] = ActiveTab;
			ViewState["RegistrationTemplateId"] = RegistrationTemplateId;
            ViewState["AvailableRegistrationAttributesForGrid"] = AvailableRegistrationAttributesForGrid;

            return base.SaveViewState();
        }

        /// <summary>
        /// Gets the bread crumbs.
        /// </summary>
        /// <param name="pageReference">The page reference.</param>
        /// <returns></returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? registrationInstanceId = PageParameter( pageReference, "RegistrationInstanceId" ).AsIntegerOrNull();
            if ( registrationInstanceId.HasValue )
            {
                RegistrationInstance registrationInstance = GetRegistrationInstance( registrationInstanceId.Value );
                if ( registrationInstance != null )
                {
                    breadCrumbs.Add( new BreadCrumb( registrationInstance.ToString(), pageReference ) );
                    return breadCrumbs;
                }
            }

            breadCrumbs.Add( new BreadCrumb( "New Registration Instance", pageReference ) );
            return breadCrumbs;
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
        }

        #endregion

        #region Events

        #region Main Form Events

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var registrationInstance = new RegistrationInstanceService( rockContext ).Get( hfRegistrationInstanceId.Value.AsInteger() );

                ShowEditDetails( registrationInstance, rockContext );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var service = new RegistrationInstanceService( rockContext );
                var registrationInstance = service.Get( hfRegistrationInstanceId.Value.AsInteger() );

                if ( registrationInstance != null )
                {
                    int registrationTemplateId = registrationInstance.RegistrationTemplateId;

                    if ( UserCanEdit ||
                         registrationInstance.IsAuthorized( Authorization.EDIT, CurrentPerson ) ||
                         registrationInstance.IsAuthorized( Authorization.ADMINISTRATE, this.CurrentPerson ) )
                    {
                        rockContext.WrapTransaction( () =>
                        {
                            new RegistrationService( rockContext ).DeleteRange( registrationInstance.Registrations );
                            service.Delete( registrationInstance );
                            rockContext.SaveChanges();
                        } );

                        var qryParams = new Dictionary<string, string> { { "RegistrationTemplateId", registrationTemplateId.ToString() } };
                        NavigateToParentPage( qryParams );
                    }
                    else
                    {
                        mdDeleteWarning.Show( "You are not authorized to delete this registration instance.", ModalAlertType.Information );
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnPreview control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnPreview_Click( object sender, EventArgs e )
        {
        }

        /// <summary>
        /// Handles the Click event of the btnSendPaymentReminder control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSendPaymentReminder_Click( object sender, EventArgs e )
        {
            Dictionary<string, string> queryParms = new Dictionary<string, string>();
            queryParms.Add( "RegistrationInstanceId", PageParameter( "RegistrationInstanceId" ) );
            NavigateToLinkedPage( "PaymentReminderPage", queryParms );
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            RegistrationInstance instance = null;

            bool newInstance = false;

            using ( var rockContext = new RockContext() )
            {
                var service = new RegistrationInstanceService( rockContext );

                int? registrationInstanceId = hfRegistrationInstanceId.Value.AsIntegerOrNull();
                if ( registrationInstanceId.HasValue )
                {
                    instance = service.Get( registrationInstanceId.Value );
                }

                if ( instance == null )
                {
                    instance = new RegistrationInstance();
                    instance.RegistrationTemplateId = PageParameter( "RegistrationTemplateId" ).AsInteger();
                    service.Add( instance );
                    newInstance = true;
                }

                rieDetails.GetValue( instance );

                if ( !Page.IsValid )
                {
                    return;
                }

                rockContext.SaveChanges();
            }

            if ( newInstance )
            {
                var qryParams = new Dictionary<string, string>();
                qryParams.Add( "RegistrationTemplateId", PageParameter( "RegistrationTemplateId" ) );
                qryParams.Add( "RegistrationInstanceId", instance.Id.ToString() );
                NavigateToCurrentPage( qryParams );
            }
            else
            {
                // Reload instance and show readonly view
                using ( var rockContext = new RockContext() )
                {
                    instance = new RegistrationInstanceService( rockContext ).Get( instance.Id );
                    ShowReadonlyDetails( instance );
                }

                // show send payment reminder link
                if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "PaymentReminderPage" ) ) && ( ( instance.RegistrationTemplate.SetCostOnInstance.HasValue && instance.RegistrationTemplate.SetCostOnInstance == true && instance.Cost.HasValue && instance.Cost.Value > 0 ) || instance.RegistrationTemplate.Cost > 0 ) )
                {
                    btnSendPaymentReminder.Visible = true;
                }
                else
                {
                    btnSendPaymentReminder.Visible = false;
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( hfRegistrationInstanceId.Value.Equals( "0" ) )
            {
                var qryParams = new Dictionary<string, string>();

                int? parentTemplateId = PageParameter( "RegistrationTemplateId" ).AsIntegerOrNull();
                if ( parentTemplateId.HasValue )
                {
                    qryParams["RegistrationTemplateId"] = parentTemplateId.ToString();
                }

                // Cancelling on Add.  Return to Grid
                NavigateToParentPage( qryParams );
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                using ( var rockContext = new RockContext() )
                {
                    RegistrationInstanceService service = new RegistrationInstanceService( rockContext );
                    RegistrationInstance item = service.Get( int.Parse( hfRegistrationInstanceId.Value ) );
                    ShowReadonlyDetails( item );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbTab control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbTab_Click( object sender, EventArgs e )
        {
            LinkButton lb = sender as LinkButton;
            if ( lb != null )
            {
                ActiveTab = lb.ID;
                ShowTab();
            }
        }

        protected void lbTemplate_Click( object sender, EventArgs e )
        {
            var qryParams = new Dictionary<string, string>();
            using ( var rockContext = new RockContext() )
            {
                var service = new RegistrationInstanceService( rockContext );
                var registrationInstance = service.Get( hfRegistrationInstanceId.Value.AsInteger() );
                if ( registrationInstance != null )
                {
                    qryParams.Add( "RegistrationTemplateId", registrationInstance.RegistrationTemplateId.ToString() );
                }
            }

            NavigateToParentPage( qryParams );
        }

        #endregion

        #region Registration Tab Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the fRegistrations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void fRegistrations_ApplyFilterClick( object sender, EventArgs e )
        {
            fRegistrations.SaveUserPreference( "Registrations Date Range", "Registration Date Range", sdrpRegistrationDateRange.DelimitedValues );
            fRegistrations.SaveUserPreference( "Payment Status", ddlRegistrationPaymentStatus.SelectedValue );
            fRegistrations.SaveUserPreference( "RegisteredBy First Name", tbRegistrationRegisteredByFirstName.Text );
            fRegistrations.SaveUserPreference( "RegisteredBy Last Name", tbRegistrationRegisteredByLastName.Text );
            fRegistrations.SaveUserPreference( "Registrant First Name", tbRegistrationRegistrantFirstName.Text );
            fRegistrations.SaveUserPreference( "Registrant Last Name", tbRegistrationRegistrantLastName.Text );

            BindRegistrationsGrid();
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the fRegistrations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void fRegistrations_ClearFilterClick( object sender, EventArgs e )
        {
            fRegistrants.DeleteUserPreferences();
            BindRegistrationsFilter();
        }

        /// <summary>
        /// Fs the registrations_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void fRegistrations_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Registrations Date Range":
                    e.Value = SlidingDateRangePicker.FormatDelimitedValues( e.Value );
                    break;

                case "Payment Status":
                case "RegisteredBy First Name":
                case "RegisteredBy Last Name":
                case "Registrant First Name":
                case "Registrant Last Name":
                    break;
                    
                default:
                   e.Value = string.Empty;
                    break;
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gRegistrations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gRegistrations_GridRebind( object sender, EventArgs e )
        {
            gRegistrations.ExportTitleName = lReadOnlyTitle.Text + " - Registrations";
            gRegistrations.ExportFilename = gRegistrations.ExportFilename ?? lReadOnlyTitle.Text + "Registrations";
            BindRegistrationsGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gRegistrations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gRegistrations_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            var registration = e.Row.DataItem as Registration;
            if ( registration != null )
            {
                // Set the processor value
                var lRegisteredBy = e.Row.FindControl( "lRegisteredBy" ) as Literal;
                if ( lRegisteredBy != null )
                {
                    if ( registration.PersonAlias != null && registration.PersonAlias.Person != null )
                    {
                        lRegisteredBy.Text = registration.PersonAlias.Person.FullNameReversed;
                    }
                    else
                    {
                        lRegisteredBy.Text = string.Format( "{0}, {1}", registration.LastName, registration.FirstName );
                    }
                }

                string registrantNames = string.Empty;
                if ( registration.Registrants != null && registration.Registrants.Any() )
                {
                    var registrants = registration.Registrants
                        .Where( r =>
                            r.PersonAlias != null &&
                            r.PersonAlias.Person != null )
                        .OrderBy( r => r.PersonAlias.Person.NickName )
                        .ThenBy( r => r.PersonAlias.Person.LastName )
                        .ToList();

                    registrantNames = registrants
                        .Select( r => r.OnWaitList ? r.PersonAlias.Person.NickName + " " + r.PersonAlias.Person.LastName + " <span class='label label-warning'>WL</span>" : r.PersonAlias.Person.NickName + " " + r.PersonAlias.Person.LastName )
                        .ToList()
                        .AsDelimited( "<br/>" );
                }

                // Set the Registrants
                var lRegistrants = e.Row.FindControl( "lRegistrants" ) as Literal;
                if ( lRegistrants != null )
                {
                    lRegistrants.Text = registrantNames;
                }

                var payments = registrationPayments.Where( p => p.EntityId == registration.Id );
                bool hasPayments = payments.Any();
                decimal totalPaid = hasPayments ? payments.Select( p => p.Amount ).DefaultIfEmpty().Sum() : 0.0m;

                // Set the Cost
                decimal discountedCost = registration.DiscountedCost;
                var lRegistrationCost = e.Row.FindControl( "lRegistrationCost" ) as Literal;
                if ( lRegistrationCost != null )
                {
                    lRegistrationCost.Visible = _instanceHasCost || discountedCost > 0.0M;
                    lRegistrationCost.Text = string.Format( "<span class='label label-info'>{0}</span>", discountedCost.FormatAsCurrency() );
                }

                var discountCode = registration.DiscountCode;
                var lDiscount = e.Row.FindControl( "lDiscount" ) as Literal;
                if ( lDiscount != null )
                {
                    lDiscount.Visible = _instanceHasCost && !string.IsNullOrEmpty( discountCode );
                    lDiscount.Text = string.Format( "<span class='label label-default'>{0}</span>", discountCode );
                }

                var lBalance = e.Row.FindControl( "lBalance" ) as Literal;
                if ( lBalance != null )
                {
                    decimal balanceDue = registration.DiscountedCost - totalPaid;
                    lBalance.Visible = _instanceHasCost || discountedCost > 0.0M;
                    string balanceCssClass;
                    if ( balanceDue > 0.0m )
                    {
                        balanceCssClass = "label-danger";
                    }
                    else if ( balanceDue < 0.0m )
                    {
                        balanceCssClass = "label-warning";
                    }
                    else
                    {
                        balanceCssClass = "label-success";
                    }

                    lBalance.Text = string.Format(
    @"<span class='label {0}'>{1}</span>
    <input type='hidden' class='js-has-payments' value='{2}' />", balanceCssClass, balanceDue.FormatAsCurrency(), hasPayments.ToTrueFalse() );
                }
            }
        }

        /// <summary>
        /// Handles the AddClick event of the gRegistrations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gRegistrations_AddClick( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "RegistrationPage", "RegistrationId", 0, "RegistrationInstanceId", hfRegistrationInstanceId.ValueAsInt() );
        }

        /// <summary>
        /// Handles the Delete event of the gRegistrations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gRegistrations_Delete( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var registrationService = new RegistrationService( rockContext );
                var registration = registrationService.Get( e.RowKeyId );
                if ( registration != null )
                {
                    int registrationInstanceId = registration.RegistrationInstanceId;

                    if ( !UserCanEdit &&
                        !registration.IsAuthorized( "Register", CurrentPerson ) &&
                        !registration.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) &&
                        !registration.IsAuthorized( Authorization.ADMINISTRATE, this.CurrentPerson ) )
                    {
                        mdDeleteWarning.Show( "You are not authorized to delete this registration.", ModalAlertType.Information );
                        return;
                    }

                    string errorMessage;
                    if ( !registrationService.CanDelete( registration, out errorMessage ) )
                    {
                        mdRegistrationsGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    var changes = new History.HistoryChangeList();
                    changes.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, "Registration" );

                    rockContext.WrapTransaction( () =>
                    {
                        HistoryService.SaveChanges(
                            rockContext,
                            typeof( Registration ),
                            Rock.SystemGuid.Category.HISTORY_EVENT_REGISTRATION.AsGuid(),
                            registration.Id,
                            changes );

                        registrationService.Delete( registration );
                        rockContext.SaveChanges();
                    } );

                    SetHasPayments( registrationInstanceId, rockContext );
                }
            }

            BindRegistrationsGrid();
        }

        /// <summary>
        /// Handles the RowSelected event of the gRegistrations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gRegistrations_RowSelected( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "RegistrationPage", "RegistrationId", e.RowKeyId );
        }

        #endregion

        #region Registrant Tab Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the fRegistrants control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void fRegistrants_ApplyFilterClick( object sender, EventArgs e )
        {
            fRegistrants.SaveUserPreference( "Registrants Date Range", "Registration Date Range",  sdrpRegistrantsRegistrantDateRange.DelimitedValues );
            fRegistrants.SaveUserPreference( "First Name", tbRegistrantsRegistrantFirstName.Text );
            fRegistrants.SaveUserPreference( "Last Name", tbRegistrantsRegistrantLastName.Text );
            fRegistrants.SaveUserPreference( "In Group", ddlRegistrantsInGroup.SelectedValue );
            fRegistrants.SaveUserPreference( "Signed Document", ddlRegistrantsSignedDocument.SelectedValue );

            if ( RegistrantFields != null )
            {
                foreach ( var field in RegistrantFields )
                {
                    if ( field.FieldSource == RegistrationFieldSource.PersonField && field.PersonFieldType.HasValue )
                    {
                        switch ( field.PersonFieldType.Value )
                        {
                            case RegistrationPersonFieldType.Campus:
                                var ddlCampus = phRegistrantsRegistrantFormFieldFilters.FindControl( "ddlRegistrantsCampus" ) as RockDropDownList;
                                if ( ddlCampus != null )
                                {
                                    fRegistrants.SaveUserPreference( "Home Campus", ddlCampus.SelectedValue );
                                }

                                break;

                            case RegistrationPersonFieldType.Email:
                                var tbEmailFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( "tbRegistrantsEmailFilter" ) as RockTextBox;
                                if ( tbEmailFilter != null )
                                {
                                    fRegistrants.SaveUserPreference( "Email", tbEmailFilter.Text );
                                }

                                break;

                            case RegistrationPersonFieldType.Birthdate:
                                var drpBirthdateFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( "drpRegistrantsBirthdateFilter" ) as DateRangePicker;
                                if ( drpBirthdateFilter != null )
                                {
                                    fRegistrants.SaveUserPreference( "Birthdate Range", drpBirthdateFilter.DelimitedValues );
                                }

                                break;
                            case RegistrationPersonFieldType.MiddleName:
                                var tbMiddleNameFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( "tbRegistrantsMiddleNameFilter" ) as RockTextBox;
                                if ( tbMiddleNameFilter != null )
                                {
                                    fRegistrants.SaveUserPreference( "MiddleName", tbMiddleNameFilter.Text );
                                }
                                break;
                            case RegistrationPersonFieldType.AnniversaryDate:
                                var drAnniversaryDateFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( "drpRegistrantsAnniversaryDateFilter" ) as DateRangePicker;
                                if ( drAnniversaryDateFilter != null )
                                {
                                    fRegistrants.SaveUserPreference( "AnniversaryDate Range", drAnniversaryDateFilter.DelimitedValues );
                                }
                                break;
                            case RegistrationPersonFieldType.Grade:
                                var gpGradeFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( "gpRegistrantsGradeFilter" ) as GradePicker;
                                if ( gpGradeFilter != null )
                                {
                                    int? gradeOffset = gpGradeFilter.SelectedValueAsInt( false );
                                    fRegistrants.SaveUserPreference( "Grade", gradeOffset.HasValue ? gradeOffset.Value.ToString() : string.Empty );
                                }

                                break;
                                
                            case RegistrationPersonFieldType.Gender:
                                var ddlGenderFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( "ddlRegistrantsGenderFilter" ) as RockDropDownList;
                                if ( ddlGenderFilter != null )
                                {
                                    fRegistrants.SaveUserPreference( "Gender", ddlGenderFilter.SelectedValue );
                                }

                                break;

                            case RegistrationPersonFieldType.MaritalStatus:
                                var dvpMaritalStatusFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( "dvpRegistrantsMaritalStatusFilter" ) as DefinedValuePicker;
                                if ( dvpMaritalStatusFilter != null )
                                {
                                    fRegistrants.SaveUserPreference( "Marital Status", dvpMaritalStatusFilter.SelectedValue );
                                }

                                break;
                                
                            case RegistrationPersonFieldType.MobilePhone:
                                var tbMobilePhoneFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( "tbRegistrantsMobilePhoneFilter" ) as RockTextBox;
                                if ( tbMobilePhoneFilter != null )
                                {
                                    fRegistrants.SaveUserPreference( "Cell Phone", "Cell Phone", tbMobilePhoneFilter.Text );
                                }

                                break;

                            case RegistrationPersonFieldType.HomePhone:
                                var tbRegistrantsHomePhoneFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( "tbRegistrantsHomePhoneFilter" ) as RockTextBox;
                                if ( tbRegistrantsHomePhoneFilter != null )
                                {
                                    fRegistrants.SaveUserPreference( "Home Phone", tbRegistrantsHomePhoneFilter.Text );
                                }

                                break;
                        }
                    }

                    if ( field.Attribute != null )
                    {
                        var attribute = field.Attribute;
                        var filterControl = phRegistrantsRegistrantFormFieldFilters.FindControl( "filterRegistrants_" + attribute.Id.ToString() );
                        if ( filterControl != null )
                        {
                            try
                            {
                                var values = attribute.FieldType.Field.GetFilterValues( filterControl, field.Attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter );
                                fRegistrants.SaveUserPreference( attribute.Key, attribute.Name, attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter ).ToJson() );
                            }
                            catch { }
                        }
                    }
                }
            }

            BindRegistrantsGrid();
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the fRegistrants control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void fRegistrants_ClearFilterClick( object sender, EventArgs e )
        {
            fRegistrants.DeleteUserPreferences();

            foreach ( var control in phRegistrantsRegistrantFormFieldFilters.ControlsOfTypeRecursive<Control>().Where( a => a.ID != null && a.ID.StartsWith( "filter" ) && a.ID.Contains( "_" ) ) )
            {
                var attributeId = control.ID.Split( '_' )[1].AsInteger();
                var attribute = AttributeCache.Get( attributeId );
                if ( attribute != null )
                {
                    attribute.FieldType.Field.SetFilterValues( control, attribute.QualifierValues, new List<string>() );
                }
            }

            if ( RegistrantFields != null )
            {
                foreach ( var field in RegistrantFields )
                {
                    if ( field.FieldSource == RegistrationFieldSource.PersonField && field.PersonFieldType.HasValue )
                    {
                        switch ( field.PersonFieldType.Value )
                        {
                            case RegistrationPersonFieldType.Campus:
                                var ddlCampus = phRegistrantsRegistrantFormFieldFilters.FindControl( "ddlRegistrantsCampus" ) as RockDropDownList;
                                if ( ddlCampus != null )
                                {
                                    ddlCampus.SetValue( ( Guid? ) null );
                                }

                                break;

                            case RegistrationPersonFieldType.Email:
                                var tbEmailFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( "tbRegistrantsEmailFilter" ) as RockTextBox;
                                if ( tbEmailFilter != null )
                                {
                                    tbEmailFilter.Text = string.Empty;
                                }

                                break;

                            case RegistrationPersonFieldType.Birthdate:
                                var drpBirthdateFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( "drpRegistrantsBirthdateFilter" ) as DateRangePicker;
                                if ( drpBirthdateFilter != null )
                                {
                                    drpBirthdateFilter.LowerValue = null;
                                    drpBirthdateFilter.UpperValue = null;
                                }

                                break;
                            case RegistrationPersonFieldType.MiddleName:
                                var tbMiddleNameFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( "tbRegistrantsMiddleNameFilter" ) as RockTextBox;
                                if ( tbMiddleNameFilter != null )
                                {
                                    tbMiddleNameFilter.Text = string.Empty;
                                }
                                break;
                            case RegistrationPersonFieldType.AnniversaryDate:
                                var drAnniversaryDateFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( "drpRegistrantsAnniversaryDateFilter" ) as DateRangePicker;
                                if ( drAnniversaryDateFilter != null )
                                {
                                    drAnniversaryDateFilter.LowerValue = null;
                                    drAnniversaryDateFilter.UpperValue = null;
                                }
                                break;
                            case RegistrationPersonFieldType.Grade:
                                var gpGradeFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( "gpRegistrantsGradeFilter" ) as GradePicker;
                                if ( gpGradeFilter != null )
                                {
                                    gpGradeFilter.SetValue( ( Guid? ) null );
                                }

                                break;
                                
                            case RegistrationPersonFieldType.Gender:
                                var ddlGenderFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( "ddlRegistrantsGenderFilter" ) as RockDropDownList;
                                if ( ddlGenderFilter != null )
                                {
                                    ddlGenderFilter.SetValue( ( Guid? ) null );
                                }

                                break;

                            case RegistrationPersonFieldType.MaritalStatus:
                                var dvpMaritalStatusFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( "dvpRegistrantsMaritalStatusFilter" ) as DefinedValuePicker;
                                if ( dvpMaritalStatusFilter != null )
                                {
                                    dvpMaritalStatusFilter.SetValue( ( Guid? ) null );
                                }

                                break;
                                
                            case RegistrationPersonFieldType.MobilePhone:
                                var tbMobilePhoneFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( "tbRegistrantsMobilePhoneFilter" ) as RockTextBox;
                                if ( tbMobilePhoneFilter != null )
                                {
                                    tbMobilePhoneFilter.Text = string.Empty;
                                }

                                break;

                            case RegistrationPersonFieldType.HomePhone:
                                var tbRegistrantsHomePhoneFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( "tbRegistrantsHomePhoneFilter" ) as RockTextBox;
                                if ( tbRegistrantsHomePhoneFilter != null )
                                {
                                    tbRegistrantsHomePhoneFilter.Text = string.Empty;
                                }

                                break;
                        }
                    }
                }
            }

            BindRegistrantsFilter( null );
        }

        /// <summary>
        /// Fs the registrants_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void fRegistrants_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            if ( RegistrantFields != null )
            {
                var attribute = RegistrantFields
                    .Where( a =>
                        a.Attribute != null &&
                        a.Attribute.Key == e.Key )
                    .Select( a => a.Attribute )
                    .FirstOrDefault();

                if ( attribute != null )
                {
                    try
                    {
                        var values = JsonConvert.DeserializeObject<List<string>>( e.Value );
                        e.Value = attribute.FieldType.Field.FormatFilterValues( attribute.QualifierValues, values );
                        return;
                    }
                    catch { }
                }
            }

            switch ( e.Key )
            {
                case "Registrants Date Range":
                    e.Value = SlidingDateRangePicker.FormatDelimitedValues( e.Value );
                    break;
                    
                case "Birthdate Range":
                    // The value might either be from a SlidingDateRangePicker or a DateRangePicker, so try both
                    var storedValue = e.Value;
                    e.Value = SlidingDateRangePicker.FormatDelimitedValues( storedValue );
                    if ( e.Value.IsNullOrWhiteSpace() )
                    {
                        e.Value = DateRangePicker.FormatDelimitedValues( storedValue );
                    }

                    break;
                    
                case "Grade":
                    e.Value = Person.GradeFormattedFromGradeOffset( e.Value.AsIntegerOrNull() );
                    break;
                    
                case "First Name":
                case "Last Name":
                case "Email":
                case "Signed Document":
                case "Home Phone":
                case "Cell Phone":
                    break;
                    
                case "Gender":
                    var gender = e.Value.ConvertToEnumOrNull<Gender>();
                    e.Value = gender.HasValue ? gender.ConvertToString() : string.Empty;
                    break;
                    
                case "Campus":
                    int? campusId = e.Value.AsIntegerOrNull();
                    if ( campusId.HasValue )
                    {
                        var campus = CampusCache.Get( campusId.Value );
                        e.Value = campus != null ? campus.Name : string.Empty;
                    }
                    else
                    {
                        e.Value = string.Empty;
                    }

                    break;
                    
                case "Marital Status":
                    int? dvId = e.Value.AsIntegerOrNull();
                    if ( dvId.HasValue )
                    {
                        var maritalStatus = DefinedValueCache.Get( dvId.Value );
                        e.Value = maritalStatus != null ? maritalStatus.Value : string.Empty;
                    }
                    else
                    {
                        e.Value = string.Empty;
                    }

                    break;
                    
                case "In Group":
                        e.Value = e.Value;
                        break;

                default:
                        e.Value = string.Empty;
                        break;
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gRegistrants control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gRegistrants_GridRebind( object sender, GridRebindEventArgs e )
        {
            gRegistrants.ExportTitleName = lReadOnlyTitle.Text + " - Registrants";
            gRegistrants.ExportFilename = gRegistrants.ExportFilename ?? lReadOnlyTitle.Text + "Registrants";
            BindRegistrantsGrid( e.IsExporting );
        }

        /// <summary>
        /// Handles the RowDataBound event of the gRegistrants control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        private void gRegistrants_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            var registrant = e.Row.DataItem as RegistrationRegistrant;
            if ( registrant != null )
            {
                // Set the registrant name value
                var lRegistrant = e.Row.FindControl( "lRegistrant" ) as Literal;
                if ( lRegistrant != null )
                {
                    if ( registrant.PersonAlias != null && registrant.PersonAlias.Person != null )
                    {
                        lRegistrant.Text = registrant.PersonAlias.Person.FullNameReversed +
                            ( Signers != null && !Signers.Contains( registrant.PersonAlias.PersonId ) ? " <i class='fa fa-pencil-square-o text-danger'></i>" : string.Empty );
                    }
                    else
                    {
                        lRegistrant.Text = string.Empty;
                    }
                }

                // Set the Group Name
                if ( registrant.GroupMember != null && GroupLinks.ContainsKey( registrant.GroupMember.GroupId ) )
                {
                    var lGroup = e.Row.FindControl( "lGroup" ) as Literal;
                    if ( lGroup != null )
                    {
                        lGroup.Text = GroupLinks[registrant.GroupMember.GroupId];
                    }
                }

                // Set the campus
                var lCampus = e.Row.FindControl( "lRegistrantsCampus" ) as Literal;
                
                // if it's null, try looking for the "lGroupPlacementsCampus" control since this RowDataBound event is shared between
                // two different grids.
                if ( lCampus == null )
                {
                    lCampus = e.Row.FindControl( "lGroupPlacementsCampus" ) as Literal;
                }
                
                if ( lCampus != null && PersonCampusIds != null )
                {
                    if ( registrant.PersonAlias != null )
                    {
                        if ( PersonCampusIds.ContainsKey( registrant.PersonAlias.PersonId ) )
                        {
                            var campusIds = PersonCampusIds[registrant.PersonAlias.PersonId];
                            if ( campusIds.Any() )
                            {
                                var campusNames = new List<string>();
                                foreach ( int campusId in campusIds )
                                {
                                    var campus = CampusCache.Get( campusId );
                                    if ( campus != null )
                                    {
                                        campusNames.Add( campus.Name );
                                    }
                                }

                                lCampus.Text = campusNames.AsDelimited( "<br/>" );
                            }
                        }
                    }
                }

                // Set the Fees
                var lFees = e.Row.FindControl( "lFees" ) as Literal;
                if ( lFees != null )
                {
                    if ( registrant.Fees != null && registrant.Fees.Any() )
                    {
                        var feeDesc = new List<string>();
                        foreach ( var fee in registrant.Fees )
                        {
                            feeDesc.Add( string.Format(
                                "{0}{1} ({2})",
                                fee.Quantity > 1 ? fee.Quantity.ToString( "N0" ) + " " : string.Empty,
                                fee.Quantity > 1 ? fee.RegistrationTemplateFee.Name.Pluralize() : fee.RegistrationTemplateFee.Name,
                                fee.Cost.FormatAsCurrency() ) );
                        }

                        lFees.Text = feeDesc.AsDelimited( "<br/>" );
                    }
                }

                if (_homeAddresses.Any() )
                {
                    var location = _homeAddresses[registrant.PersonId.Value];
                    // break up addresses if exporting
                    if ( _isExporting )
                    {
                        var lStreet1 = e.Row.FindControl( "lStreet1" ) as Literal;
                        var lStreet2 = e.Row.FindControl( "lStreet2" ) as Literal;
                        var lCity = e.Row.FindControl( "lCity" ) as Literal;
                        var lState = e.Row.FindControl( "lState" ) as Literal;
                        var lPostalCode = e.Row.FindControl( "lPostalCode" ) as Literal;
                        var lCountry = e.Row.FindControl( "lCountry" ) as Literal;

                        if ( location != null )
                        {
                            lStreet1.Text = location.Street1;
                            lStreet2.Text = location.Street2;
                            lCity.Text = location.City;
                            lState.Text = location.State;
                            lPostalCode.Text = location.PostalCode;
                            lCountry.Text = location.Country;
                        }
                    }
                    else
                    {
                        var addressField = e.Row.FindControl( "lRegistrantsAddress" ) as Literal ?? e.Row.FindControl( "lGroupPlacementsAddress" ) as Literal;
                        if ( addressField != null )
                        {
                            addressField.Text = location != null && location.FormattedAddress.IsNotNullOrWhiteSpace() ? location.FormattedAddress : string.Empty;
                        }
                    }
                }

                if (_mobilePhoneNumbers.Any())
                {
                    var mobileNumber = _mobilePhoneNumbers[registrant.PersonId.Value];
                    var mobileField = e.Row.FindControl( "lRegistrantsMobile" ) as Literal ?? e.Row.FindControl( "lGroupPlacementsMobile" ) as Literal;
                    if ( mobileField != null)
                    {
                        if (mobileNumber == null || mobileNumber.NumberFormatted.IsNullOrWhiteSpace())
                        {
                            mobileField.Text = string.Empty;
                        }
                        else
                        {
                            mobileField.Text = mobileNumber.IsUnlisted ? "Unlisted" : mobileNumber.NumberFormatted;
                        }
                    }
                    
                }

                if ( _homePhoneNumbers.Any() )
                {
                    var homePhoneNumber = _homePhoneNumbers[registrant.PersonId.Value];
                    var homePhoneField = e.Row.FindControl( "lRegistrantsHomePhone" ) as Literal ?? e.Row.FindControl( "lGroupPlacementsHomePhone" ) as Literal;
                    if ( homePhoneField != null )
                    {
                        if ( homePhoneNumber == null || homePhoneNumber.NumberFormatted.IsNullOrWhiteSpace() )
                        {
                            homePhoneField.Text = string.Empty;
                        }
                        else
                        {
                            homePhoneField.Text = homePhoneNumber.IsUnlisted ? "Unlisted" : homePhoneNumber.NumberFormatted;
                        }
                    }

                }
            }
        }

        /// <summary>
        /// Handles the AddClick event of the gRegistrants control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gRegistrants_AddClick( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "RegistrationPage", "RegistrationId", 0, "RegistrationInstanceId", hfRegistrationInstanceId.ValueAsInt() );
        }

        /// <summary>
        /// Handles the RowSelected event of the gRegistrants control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gRegistrants_RowSelected( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var registrantService = new RegistrationRegistrantService( rockContext );
                var registrant = registrantService.Get( e.RowKeyId );
                if ( registrant != null )
                {
                    var qryParams = new Dictionary<string, string>();
                    qryParams.Add( "RegistrationId", registrant.RegistrationId.ToString() );
                    string url = LinkedPageUrl( "RegistrationPage", qryParams );
                    url += "#" + e.RowKeyValue;
                    Response.Redirect( url, false );
                }
            }
        }

        /// <summary>
        /// Handles the Delete event of the gRegistrants control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gRegistrants_Delete( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var registrantService = new RegistrationRegistrantService( rockContext );
                var registrant = registrantService.Get( e.RowKeyId );
                if ( registrant != null )
                {
                    string errorMessage;
                    if ( !registrantService.CanDelete( registrant, out errorMessage ) )
                    {
                        mdRegistrantsGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    registrantService.Delete( registrant );
                    rockContext.SaveChanges();
                }
            }

            BindRegistrantsGrid();
        }

        #endregion

        #region Payment Tab Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the fPayments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void fPayments_ApplyFilterClick( object sender, EventArgs e )
        {
            fPayments.SaveUserPreference( "Payments Date Range", "Transaction Date Range", sdrpPaymentDateRange.DelimitedValues );

            BindPaymentsGrid();
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the fPayments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void fPayments_ClearFilterClick( object sender, EventArgs e )
        {
            fPayments.DeleteUserPreferences();
            BindPaymentsFilter();
        }

        /// <summary>
        /// Fs the payments_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void fPayments_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Payments Date Range":
                    {
                        e.Value = SlidingDateRangePicker.FormatDelimitedValues( e.Value );
                        break;
                    }

                default:
                    {
                        e.Value = string.Empty;
                        break;
                    }
            }
        }

        /// <summary>
        /// Handles the RowSelected event of the gPayments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gPayments_RowSelected( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "TransactionDetailPage", "transactionId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the GridRebind event of the gPayments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gPayments_GridRebind( object sender, EventArgs e )
        {
            gPayments.ExportTitleName = lReadOnlyTitle.Text + " - Registration Payments";
            gPayments.ExportFilename = gPayments.ExportFilename ?? lReadOnlyTitle.Text + "RegistrationPayments";
            BindPaymentsGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gPayments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        private void gPayments_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            var transaction = e.Row.DataItem as FinancialTransaction;
            var lRegistrar = e.Row.FindControl( "lRegistrar" ) as Literal;
            var lRegistrants = e.Row.FindControl( "lRegistrants" ) as Literal;

            if ( transaction != null && lRegistrar != null && lRegistrants != null )
            {
                var registrars = new List<string>();
                var registrants = new List<string>();

                var registrationIds = transaction.TransactionDetails.Select( d => d.EntityId ).ToList();
                foreach ( var registration in paymentRegistrations
                    .Where( r => registrationIds.Contains( r.Id ) ) )
                {
                    if ( registration.PersonAlias != null && registration.PersonAlias.Person != null )
                    {
                        var qryParams = new Dictionary<string, string>();
                        qryParams.Add( "RegistrationId", registration.Id.ToString() );
                        string url = LinkedPageUrl( "RegistrationPage", qryParams );
                        registrars.Add( string.Format( "<a href='{0}'>{1}</a>", url, registration.PersonAlias.Person.FullName ) );

                        foreach ( var registrant in registration.Registrants )
                        {
                            if ( registrant.PersonAlias != null && registrant.PersonAlias.Person != null )
                            {
                                registrants.Add( registrant.PersonAlias.Person.FullName );
                            }
                        }
                    }
                }

                lRegistrar.Text = registrars.AsDelimited( "<br/>" );
                lRegistrants.Text = registrants.AsDelimited( "<br/>" );
            }
        }

        #endregion

        #region Fee Tab Events

        protected void fFees_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "FeeDateRange":
                    e.Value = SlidingDateRangePicker.FormatDelimitedValues( e.Value );
                    break;

                case "FeeName":
                    break;

                case "FeeOptions":
                    var values = new List<string>();
                    foreach ( string value in e.Value.Split( ';' ) )
                    {
                        var item = cblFeeOptions.Items.FindByValue( value );
                        if ( item != null )
                        {
                            values.Add( item.Text );
                        }
                    }

                    e.Value = values.AsDelimited( ", " );
                    break;

                default:
                    e.Value = string.Empty;
                    break;
            }
        }

        /// <summary>
        /// Handles the ClearFilterCick event of the fFees control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void fFees_ClearFilterCick( object sender, EventArgs e )
        {
            fFees.DeleteUserPreferences();
            BindFeesFilter();
        }

        protected void fFees_ApplyFilterClick( object sender, EventArgs e )
        {
            fFees.SaveUserPreference( "FeeDateRange", "Fee Date Range", sdrpFeeDateRange.DelimitedValues );
            fFees.SaveUserPreference( "FeeName", "Fee Name", ddlFeeName.SelectedItem.Text );
            fFees.SaveUserPreference( "FeeOptions", "Fee Options", cblFeeOptions.SelectedValues.AsDelimited( ";" ) );

            BindFeesGrid();
        }

        protected void ddlFeeName_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( ddlFeeName.SelectedIndex > 0 )
            {
                Populate_cblFeeOptions();
                cblFeeOptions.Visible = true;
            }
        }

        #endregion

        #region Discount Tab Events

        protected void fDiscounts_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e)
        {
            switch ( e.Key )
            {
                case "DiscountDateRange":
                    e.Value = SlidingDateRangePicker.FormatDelimitedValues( e.Value );
                    break;

                case "DiscountCode":
                    // If that discount code is not in the list, don't show it anymore.
                    if ( ddlDiscountCode.Items.FindByText( e.Value ) == null )
                    {
                        e.Value = string.Empty;
                    }
                    break;

                case "DiscountCodeSearch":
                    break;

                default:
                    e.Value = string.Empty;
                    break;
            }
        }

        protected void fDiscounts_ClearFilterClick( object sender, EventArgs e )
        {
            fDiscounts.DeleteUserPreferences();
            tbDiscountCodeSearch.Enabled = true;
            BindDiscountsFilter();
        }

        protected void fDiscounts_ApplyFilterClick( object sender, EventArgs e )
        {
            fDiscounts.SaveUserPreference( "DiscountDateRange", "Discount Date Range", sdrpDiscountDateRange.DelimitedValues );
            fDiscounts.SaveUserPreference( "DiscountCode", "Discount Code", ddlDiscountCode.SelectedItem.Text );
            fDiscounts.SaveUserPreference( "DiscountCodeSearch", "Discount Code Search", tbDiscountCodeSearch.Text );

            BindDiscountsGrid();
        }

        protected void ddlDiscountCode_SelectedIndexChanged( object sender, EventArgs e )
        {
            tbDiscountCodeSearch.Enabled = ddlDiscountCode.SelectedIndex == 0 ? true : false;
        }

        #endregion

        #region Linkage Tab Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the fLinkages control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void fLinkages_ApplyFilterClick( object sender, EventArgs e )
        {
            fLinkages.SaveUserPreference( "Campus", cblCampus.SelectedValues.AsDelimited( ";" ) );

            BindLinkagesGrid();
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the fLinkages control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void fLinkages_ClearFilterClick( object sender, EventArgs e )
        {
            fLinkages.DeleteUserPreferences();
            BindLinkagesFilter();
        }

        /// <summary>
        /// Fs the campusEventItems_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void fLinkages_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Campus":
                    var values = new List<string>();
                    foreach ( string value in e.Value.Split( ';' ) )
                    {
                        var item = cblCampus.Items.FindByValue( value );
                        if ( item != null )
                        {
                            values.Add( item.Text );
                        }
                    }

                    e.Value = values.AsDelimited( ", " );
                    break;

                default:
                    e.Value = string.Empty;
                    break;
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gLinkages control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gLinkages_GridRebind( object sender, EventArgs e )
        {
            gLinkages.ExportTitleName = lReadOnlyTitle.Text + " - Registration Linkages";
            gLinkages.ExportTitleName = gLinkages.ExportTitleName ?? lReadOnlyTitle.Text + "RegistrationLinkages";
            BindLinkagesGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gLinkages control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gLinkages_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var eventItemOccurrenceGroupMap = e.Row.DataItem as EventItemOccurrenceGroupMap;
                if ( eventItemOccurrenceGroupMap != null && eventItemOccurrenceGroupMap.EventItemOccurrence != null )
                {
                    if ( eventItemOccurrenceGroupMap.EventItemOccurrence.EventItem != null )
                    {
                        var lCalendarItem = e.Row.FindControl( "lCalendarItem" ) as Literal;
                        if ( lCalendarItem != null )
                        {
                            var calendarItems = new List<string>();
                            foreach ( var calendarItem in eventItemOccurrenceGroupMap.EventItemOccurrence.EventItem.EventCalendarItems )
                            {
                                if ( calendarItem.EventItem != null && calendarItem.EventCalendar != null )
                                {
                                    var qryParams = new Dictionary<string, string>();
                                    qryParams.Add( "EventCalendarId", calendarItem.EventCalendarId.ToString() );
                                    qryParams.Add( "EventItemId", calendarItem.EventItem.Id.ToString() );
                                    calendarItems.Add( string.Format( 
                                        "<a href='{0}'>{1}</a> ({2})",
                                        LinkedPageUrl( "CalendarItemPage", qryParams ),
                                        calendarItem.EventItem.Name,
                                        calendarItem.EventCalendar.Name ) );
                                }
                            }

                            lCalendarItem.Text = calendarItems.AsDelimited( "<br/>" );
                        }

                        if ( eventItemOccurrenceGroupMap.EventItemOccurrence.ContentChannelItems.Any() )
                        {
                            var lContentItem = e.Row.FindControl( "lContentItem" ) as Literal;
                            if ( lContentItem != null )
                            {
                                var contentItems = new List<string>();
                                foreach ( var contentItem in eventItemOccurrenceGroupMap.EventItemOccurrence.ContentChannelItems
                                    .Where( c => c.ContentChannelItem != null )
                                    .Select( c => c.ContentChannelItem ) )
                                {
                                    var qryParams = new Dictionary<string, string>();
                                    qryParams.Add( "ContentItemId", contentItem.Id.ToString() );
                                    contentItems.Add( string.Format( "<a href='{0}'>{1}</a>", LinkedPageUrl( "ContentItemPage", qryParams ), contentItem.Title ) );
                                }

                                lContentItem.Text = contentItems.AsDelimited( "<br/>" );
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the AddClick event of the gLinkages control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gLinkages_AddClick( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "LinkagePage", "LinkageId", 0, "RegistrationInstanceId", hfRegistrationInstanceId.ValueAsInt() );
        }

        /// <summary>
        /// Handles the Edit event of the gLinkages control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gLinkages_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "LinkagePage", "LinkageId", e.RowKeyId, "RegistrationInstanceId", hfRegistrationInstanceId.ValueAsInt() );
        }

        /// <summary>
        /// Handles the Delete event of the gLinkages control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gLinkages_Delete( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var campusEventItemService = new EventItemOccurrenceGroupMapService( rockContext );
                var campusEventItem = campusEventItemService.Get( e.RowKeyId );
                if ( campusEventItem != null )
                {
                    string errorMessage;
                    if ( !campusEventItemService.CanDelete( campusEventItem, out errorMessage ) )
                    {
                        mdLinkagesGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    campusEventItemService.Delete( campusEventItem );
                    rockContext.SaveChanges();
                }
            }

            BindLinkagesGrid();
        }

        #endregion

        #region WaitList Tab Events

        /// <summary>
        /// Handles the RowSelected event of the gWaitList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gWaitList_RowSelected( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var registrantService = new RegistrationRegistrantService( rockContext );
                var registrant = registrantService.Get( e.RowKeyId );
                if ( registrant != null )
                {
                    var qryParams = new Dictionary<string, string>();
                    qryParams.Add( "RegistrationId", registrant.RegistrationId.ToString() );
                    string url = LinkedPageUrl( "RegistrationPage", qryParams );
                    url += "#" + e.RowKeyValue;
                    Response.Redirect( url, false );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnProcessWaitlist control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btnProcessWaitlist_Click( object sender, EventArgs e )
        {
            // create entity set with selected individuals
            var keys = gWaitList.SelectedKeys.ToList();
            if ( keys.Any() )
            {
                var entitySet = new Rock.Model.EntitySet();
                entitySet.EntityTypeId = Rock.Web.Cache.EntityTypeCache.Get<Rock.Model.RegistrationRegistrant>().Id;
                entitySet.ExpireDateTime = RockDateTime.Now.AddMinutes( 20 );

                foreach ( var key in keys )
                {
                    try
                    {
                        var item = new Rock.Model.EntitySetItem();
                        item.EntityId = (int)key;
                        entitySet.Items.Add( item );
                    }
                    catch { }
                }

                if ( entitySet.Items.Any() )
                {
                    var rockContext = new RockContext();
                    var service = new Rock.Model.EntitySetService( rockContext );
                    service.Add( entitySet );
                    rockContext.SaveChanges();

                    // redirect to the waitlist page
                    Dictionary<string, string> queryParms = new Dictionary<string, string>();
                    queryParms.Add( "WaitListSetId", entitySet.Id.ToString() );
                    NavigateToLinkedPage( "WaitListProcessPage", queryParms );
                }
            }
        }

        /// <summary>
        /// Handles the AddClick event of the gWaitList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gWaitList_AddClick( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "RegistrationPage", "RegistrationId", 0, "RegistrationInstanceId", hfRegistrationInstanceId.ValueAsInt() );
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the fWaitList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void fWaitList_ApplyFilterClick( object sender, EventArgs e )
        {
            fWaitList.SaveUserPreference( "WL-Date Range", "Date Range", drpWaitListDateRange.DelimitedValues );
            fWaitList.SaveUserPreference( "WL-First Name", "First Name", tbWaitListFirstName.Text );
            fWaitList.SaveUserPreference( "WL-Last Name", "Last Name", tbWaitListLastName.Text );

            if ( RegistrantFields != null )
            {
                foreach ( var field in RegistrantFields )
                {
                    if ( field.FieldSource == RegistrationFieldSource.PersonField && field.PersonFieldType.HasValue )
                    {
                        switch ( field.PersonFieldType.Value )
                        {
                            case RegistrationPersonFieldType.Campus:
                                var ddlCampus = phWaitListFormFieldFilters.FindControl( "ddlWaitlistCampus" ) as RockDropDownList;
                                if ( ddlCampus != null )
                                {
                                    fWaitList.SaveUserPreference( "WL-Home Campus", "Home Campus", ddlCampus.SelectedValue );
                                }

                                break;

                            case RegistrationPersonFieldType.Email:
                                var tbEmailFilter = phWaitListFormFieldFilters.FindControl( "tbWaitlistEmailFilter" ) as RockTextBox;
                                if ( tbEmailFilter != null )
                                {
                                    fWaitList.SaveUserPreference( "WL-Email", "Email", tbEmailFilter.Text );
                                }

                                break;

                            case RegistrationPersonFieldType.Birthdate:
                                var drpBirthdateFilter = phWaitListFormFieldFilters.FindControl( "drpWaitlistBirthdateFilter" ) as DateRangePicker;
                                if ( drpBirthdateFilter != null )
                                {
                                    fWaitList.SaveUserPreference( "WL-Birthdate Range", "Birthdate Range", drpBirthdateFilter.DelimitedValues );
                                }

                                break;
                            case RegistrationPersonFieldType.MiddleName:
                                var tbMiddleNameFilter = phWaitListFormFieldFilters.FindControl( "tbWaitlistMiddleNameFilter" ) as RockTextBox;
                                if ( tbMiddleNameFilter != null )
                                {
                                    fWaitList.SaveUserPreference( "WL-MiddleName", "MiddleName", tbMiddleNameFilter.Text );
                                }
                                break;
                            case RegistrationPersonFieldType.AnniversaryDate:
                                var drpAnniversaryDateFilter = phWaitListFormFieldFilters.FindControl( "drpWaitlistAnniversaryDateFilter" ) as DateRangePicker;
                                if ( drpAnniversaryDateFilter != null )
                                {
                                    fWaitList.SaveUserPreference( "WL-AnniversaryDate Range", "AnniversaryDate Range", drpAnniversaryDateFilter.DelimitedValues );
                                }
                                break;
                            case RegistrationPersonFieldType.Grade:
                                var gpGradeFilter = phWaitListFormFieldFilters.FindControl( "gpWaitlistGradeFilter" ) as GradePicker;
                                if ( gpGradeFilter != null )
                                {
                                    int? gradeOffset = gpGradeFilter.SelectedValueAsInt( false );
                                    fWaitList.SaveUserPreference( "WL-Grade", "Grade", gradeOffset.HasValue ? gradeOffset.Value.ToString() : string.Empty );
                                }

                                break;
                                
                            case RegistrationPersonFieldType.Gender:
                                var ddlGenderFilter = phWaitListFormFieldFilters.FindControl( "ddlWaitlistGenderFilter" ) as RockDropDownList;
                                if ( ddlGenderFilter != null )
                                {
                                    fWaitList.SaveUserPreference( "WL-Gender", "Gender", ddlGenderFilter.SelectedValue );
                                }

                                break;

                            case RegistrationPersonFieldType.MaritalStatus:
                                var dvpMaritalStatusFilter = phWaitListFormFieldFilters.FindControl( "dvpWaitlistMaritalStatusFilter" ) as DefinedValuePicker;
                                if ( dvpMaritalStatusFilter != null )
                                {
                                    fWaitList.SaveUserPreference( "WL-Marital Status", "Marital Status", dvpMaritalStatusFilter.SelectedValue );
                                }

                                break;
                                
                            case RegistrationPersonFieldType.MobilePhone:
                                var tbMobilePhoneFilter = phWaitListFormFieldFilters.FindControl( "tbWaitlistMobilePhoneFilter" ) as RockTextBox;
                                if ( tbMobilePhoneFilter != null )
                                {
                                    fWaitList.SaveUserPreference( "WL-Phone", "Cell Phone", tbMobilePhoneFilter.Text );
                                }

                                break;

                            case RegistrationPersonFieldType.HomePhone:
                                var tbWaitlistHomePhoneFilter = phWaitListFormFieldFilters.FindControl( "tbWaitlistHomePhoneFilter" ) as RockTextBox;
                                if ( tbWaitlistHomePhoneFilter != null )
                                {
                                    fWaitList.SaveUserPreference( "WL-HomePhone", "Home Phone", tbWaitlistHomePhoneFilter.Text );
                                }

                                break;
                        }
                    }

                    if ( field.Attribute != null )
                    {
                        var attribute = field.Attribute;
                        var filterControl = phWaitListFormFieldFilters.FindControl( "filterWaitlist_" + attribute.Id.ToString() );
                        if ( filterControl != null )
                        {
                            try
                            {
                                var values = attribute.FieldType.Field.GetFilterValues( filterControl, field.Attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter );
                                fWaitList.SaveUserPreference( "WL-" + attribute.Key, attribute.Name, attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter ).ToJson() );
                            }
                            catch { }
                        }
                    }
                }
            }

            BindWaitListGrid();
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the fWaitList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void fWaitList_ClearFilterClick( object sender, EventArgs e )
        {
            fWaitList.DeleteUserPreferences();

            foreach ( var control in phWaitListFormFieldFilters.ControlsOfTypeRecursive<Control>().Where( a => a.ID != null && a.ID.StartsWith( "filter" ) && a.ID.Contains( "_" ) ) )
            {
                var attributeId = control.ID.Split( '_' )[1].AsInteger();
                var attribute = AttributeCache.Get( attributeId );
                if ( attribute != null )
                {
                    attribute.FieldType.Field.SetFilterValues( control, attribute.QualifierValues, new List<string>() );
                }
            }

            if ( RegistrantFields != null )
            {
                foreach ( var field in RegistrantFields )
                {
                    if ( field.FieldSource == RegistrationFieldSource.PersonField && field.PersonFieldType.HasValue )
                    {
                        switch ( field.PersonFieldType.Value )
                        {
                            case RegistrationPersonFieldType.Campus:
                                var ddlCampus = phWaitListFormFieldFilters.FindControl( "ddlWaitlistCampus" ) as RockDropDownList;
                                if ( ddlCampus != null )
                                {
                                    ddlCampus.SetValue( ( Guid? ) null );
                                }

                                break;

                            case RegistrationPersonFieldType.Email:
                                var tbEmailFilter = phWaitListFormFieldFilters.FindControl( "tbWaitlistEmailFilter" ) as RockTextBox;
                                if ( tbEmailFilter != null )
                                {
                                    tbEmailFilter.Text = string.Empty;
                                }

                                break;

                            case RegistrationPersonFieldType.Birthdate:
                                var drpBirthdateFilter = phWaitListFormFieldFilters.FindControl( "drpWaitlistBirthdateFilter" ) as DateRangePicker;
                                if ( drpBirthdateFilter != null )
                                {
                                    drpBirthdateFilter.UpperValue = null;
                                    drpBirthdateFilter.LowerValue = null;
                                }

                                break;
                            case RegistrationPersonFieldType.MiddleName:
                                var tbMiddleNameFilter = phWaitListFormFieldFilters.FindControl( "tbWaitlistMiddleNameFilter" ) as RockTextBox;
                                if ( tbMiddleNameFilter != null )
                                {
                                    tbMiddleNameFilter.Text = string.Empty;
                                }
                                break;
                            case RegistrationPersonFieldType.AnniversaryDate:
                                var drpAnniversaryDateFilter = phWaitListFormFieldFilters.FindControl( "drpWaitlistAnniversaryDateFilter" ) as DateRangePicker;
                                if ( drpAnniversaryDateFilter != null )
                                {
                                    drpAnniversaryDateFilter.UpperValue = null;
                                    drpAnniversaryDateFilter.LowerValue = null;
                                }
                                break;
                            case RegistrationPersonFieldType.Grade:
                                var gpGradeFilter = phWaitListFormFieldFilters.FindControl( "gpWaitlistGradeFilter" ) as GradePicker;
                                if ( gpGradeFilter != null )
                                {
                                    gpGradeFilter.SetValue( ( Guid? ) null );
                                }

                                break;
                                
                            case RegistrationPersonFieldType.Gender:
                                var ddlGenderFilter = phWaitListFormFieldFilters.FindControl( "ddlWaitlistGenderFilter" ) as RockDropDownList;
                                if ( ddlGenderFilter != null )
                                {
                                    ddlGenderFilter.SetValue( ( Guid? ) null );
                                }

                                break;

                            case RegistrationPersonFieldType.MaritalStatus:
                                var dvpMaritalStatusFilter = phWaitListFormFieldFilters.FindControl( "dvpWaitlistMaritalStatusFilter" ) as DefinedValuePicker;
                                if ( dvpMaritalStatusFilter != null )
                                {
                                    dvpMaritalStatusFilter.SetValue( ( Guid? ) null );
                                }

                                break;
                                
                            case RegistrationPersonFieldType.MobilePhone:
                                var tbMobilePhoneFilter = phWaitListFormFieldFilters.FindControl( "tbWaitlistMobilePhoneFilter" ) as RockTextBox;
                                if ( tbMobilePhoneFilter != null )
                                {
                                    tbMobilePhoneFilter.Text = string.Empty;
                                }

                                break;

                            case RegistrationPersonFieldType.HomePhone:
                                var tbWaitlistHomePhoneFilter = phWaitListFormFieldFilters.FindControl( "tbWaitlistHomePhoneFilter" ) as RockTextBox;
                                if ( tbWaitlistHomePhoneFilter != null )
                                {
                                    tbWaitlistHomePhoneFilter.Text = string.Empty;
                                }

                                break;
                        }
                    }
                }
            }

            BindWaitListFilter( null );
        }

        /// <summary>
        /// fs the wait list_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void fWaitList_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            if ( e.Key.StartsWith( "WL-" ) )
            {
                var key = e.Key.Remove( 0, 3 );

                if ( RegistrantFields != null )
                {
                    var attribute = RegistrantFields
                        .Where( a =>
                            a.Attribute != null &&
                            a.Attribute.Key == key )
                        .Select( a => a.Attribute )
                        .FirstOrDefault();

                    if ( attribute != null )
                    {
                        try
                        {
                            var values = JsonConvert.DeserializeObject<List<string>>( e.Value );
                            e.Value = attribute.FieldType.Field.FormatFilterValues( attribute.QualifierValues, values );
                            return;
                        }
                        catch { }
                    }
                }

                switch ( key )
                {
                    case "Date Range":
                    case "Birthdate Range":
                        // The value might either be from a SlidingDateRangePicker or a DateRangePicker, so try both
                        var storedValue = e.Value;
                        e.Value = SlidingDateRangePicker.FormatDelimitedValues( storedValue );
                        if ( e.Value.IsNullOrWhiteSpace() )
                        {
                            e.Value = DateRangePicker.FormatDelimitedValues( storedValue );
                        }

                        break;
                        
                    case "Grade":
                        e.Value = Person.GradeFormattedFromGradeOffset( e.Value.AsIntegerOrNull() );
                        break;
                        
                    case "First Name":
                    case "Last Name":
                    case "Email":
                    case "Cell Phone":
                    case "Home Phone":
                    case "Signed Document":
                        break;
                        
                    case "Gender":
                        var gender = e.Value.ConvertToEnumOrNull<Gender>();
                        e.Value = gender.HasValue ? gender.ConvertToString() : string.Empty;
                        break;
                        
                    case "Campus":
                        int? campusId = e.Value.AsIntegerOrNull();
                        if ( campusId.HasValue )
                        {
                            var campus = CampusCache.Get( campusId.Value );
                            e.Value = campus != null ? campus.Name : string.Empty;
                        }
                        else
                        {
                            e.Value = string.Empty;
                        }

                        break;
                        
                    case "Marital Status":
                        int? dvId = e.Value.AsIntegerOrNull();
                        if ( dvId.HasValue )
                        {
                            var maritalStatus = DefinedValueCache.Get( dvId.Value );
                            e.Value = maritalStatus != null ? maritalStatus.Value : string.Empty;
                        }
                        else
                        {
                            e.Value = string.Empty;
                        }

                        break;
                        
                    case "In Group":
                        e.Value = e.Value;
                        break;

                    default:
                        e.Value = string.Empty;
                        break;
                }
            }
            else
            {
                e.Value = string.Empty;
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gWaitList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        private void gWaitList_GridRebind( object sender, GridRebindEventArgs e )
        {
            gWaitList.ExportTitleName = lReadOnlyTitle.Text + " - Registration Wait List";
            gWaitList.ExportFilename = gWaitList.ExportFilename ?? lReadOnlyTitle.Text + " - Registration Wait List";
            BindWaitListGrid( e.IsExporting );
        }

        /// <summary>
        /// Handles the RowDataBound event of the gWaitList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        private void gWaitList_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            var registrant = e.Row.DataItem as RegistrationRegistrant;
            if ( registrant != null )
            {
                // Set the wait list individual name value
                var lWaitListIndividual = e.Row.FindControl( "lWaitListIndividual" ) as Literal;
                if ( lWaitListIndividual != null )
                {
                    if ( registrant.PersonAlias != null && registrant.PersonAlias.Person != null )
                    {
                        lWaitListIndividual.Text = registrant.PersonAlias.Person.FullNameReversed;
                    }
                    else
                    {
                        lWaitListIndividual.Text = string.Empty;
                    }
                }

                var lWaitListOrder = e.Row.FindControl( "lWaitListOrder" ) as Literal;
                if ( lWaitListOrder != null )
                {
                    lWaitListOrder.Text = ( _waitListOrder.IndexOf( registrant.Id ) + 1 ).ToString();
                }

                // Set the campus
                var lCampus = e.Row.FindControl( "lWaitlistCampus" ) as Literal;
                if ( lCampus != null && PersonCampusIds != null )
                {
                    if ( registrant.PersonAlias != null )
                    {
                        if ( PersonCampusIds.ContainsKey( registrant.PersonAlias.PersonId ) )
                        {
                            var campusIds = PersonCampusIds[registrant.PersonAlias.PersonId];
                            if ( campusIds.Any() )
                            {
                                var campusNames = new List<string>();
                                foreach ( int campusId in campusIds )
                                {
                                    var campus = CampusCache.Get( campusId );
                                    if ( campus != null )
                                    {
                                        campusNames.Add( campus.Name );
                                    }
                                }

                                lCampus.Text = campusNames.AsDelimited( "<br/>" );
                            }
                        }
                    }
                }

                var lAddress = e.Row.FindControl( "lWaitlistAddress" ) as Literal;
                if ( lAddress != null && _homeAddresses.Count() > 0 )
                {
                    var location = _homeAddresses[registrant.PersonId.Value];
                    lAddress.Text = location != null && location.FormattedAddress.IsNotNullOrWhiteSpace() ? location.FormattedAddress : string.Empty;
                }


                var mobileField = e.Row.FindControl( "lWaitlistMobile" ) as Literal;
                if ( mobileField != null )
                {

                    var mobilePhoneNumber = _mobilePhoneNumbers[registrant.PersonId.Value];
                    if ( mobilePhoneNumber == null || mobilePhoneNumber.NumberFormatted.IsNullOrWhiteSpace() )
                    {
                        mobileField.Text = string.Empty;
                    }
                    else
                    {
                        mobileField.Text = mobilePhoneNumber.IsUnlisted ? "Unlisted" : mobilePhoneNumber.NumberFormatted;
                    }
                }

                var homePhoneField = e.Row.FindControl( "lWaitlistHomePhone" ) as Literal;
                if ( homePhoneField != null )
                {

                    var homePhoneNumber = _homePhoneNumbers[registrant.PersonId.Value];
                    if ( homePhoneNumber == null || homePhoneNumber.NumberFormatted.IsNullOrWhiteSpace() )
                    {
                        homePhoneField.Text = string.Empty;
                    }
                    else
                    {
                        homePhoneField.Text = homePhoneNumber.IsUnlisted ? "Unlisted" : homePhoneNumber.NumberFormatted;
                    }
                }
            }
        }

        #endregion

        #region Group Placement Tab Events

        /// <summary>
        /// Handles the GridRebind event of the gGroupPlacements control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gGroupPlacements_GridRebind( object sender, GridRebindEventArgs e )
        {
            gGroupPlacements.ExportTitleName = lReadOnlyTitle.Text + " - Registration Group Placements";
            gGroupPlacements.ExportFilename = gGroupPlacements.ExportFilename ?? lReadOnlyTitle.Text + "RegistrationGroupPlacements";
            BindGroupPlacementGrid( e.IsExporting );
        }

        /// <summary>
        /// Handles the SelectItem event of the gpGroupPlacementParentGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gpGroupPlacementParentGroup_SelectItem( object sender, EventArgs e )
        {
            int? parentGroupId = gpGroupPlacementParentGroup.SelectedValueAsInt();

            SetUserPreference(
                string.Format( "ParentGroup_{0}_{1}", BlockId, hfRegistrationInstanceId.Value ),
                parentGroupId.HasValue ? parentGroupId.Value.ToString() : string.Empty,
                true );

            var groupPickerField = gGroupPlacements.Columns.OfType<GroupPickerField>().FirstOrDefault();
            if ( groupPickerField != null )
            {
                groupPickerField.RootGroupId = parentGroupId;
            }

            BindGroupPlacementGrid();
        }

        /// <summary>
        /// Handles the Click event of the lbPlaceInGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbPlaceInGroup_Click( object sender, EventArgs e )
        {
            var col = gGroupPlacements.Columns.OfType<GroupPickerField>().FirstOrDefault();
            if ( col != null )
            {
                var placements = new Dictionary<int, List<int>>();

                var colIndex = gGroupPlacements.GetColumnIndex( col ).ToString();
                foreach ( GridViewRow row in gGroupPlacements.Rows )
                {
                    GroupPicker gp = row.FindControl( "groupPicker_" + colIndex.ToString() ) as GroupPicker;
                    if ( gp != null )
                    {
                        int? groupId = gp.SelectedValueAsInt();
                        if ( groupId.HasValue )
                        {
                            int registrantId = ( int ) gGroupPlacements.DataKeys[row.RowIndex].Value;
                            placements.AddOrIgnore( groupId.Value, new List<int>() );
                            placements[groupId.Value].Add( registrantId );
                        }
                    }
                }

                using ( var rockContext = new RockContext() )
                {
                    try
                    {
                        rockContext.WrapTransaction( () =>
                        {
                            var groupMemberService = new GroupMemberService( rockContext );

                            // Get all the registrants that were selected
                            var registrantIds = placements.SelectMany( p => p.Value ).ToList();
                            var registrants = new RegistrationRegistrantService( rockContext )
                                .Queryable( "PersonAlias" ).AsNoTracking()
                                .Where( r => registrantIds.Contains( r.Id ) )
                                .ToList();

                            // Get any groups that were selected
                            var groupIds = placements.Keys.ToList();
                            foreach ( var group in new GroupService( rockContext )
                                .Queryable( "GroupType" ).AsNoTracking()
                                .Where( g => groupIds.Contains( g.Id ) ) )
                            {
                                foreach ( int registrantId in placements[group.Id] )
                                {
                                    int? roleId = group.GroupType.DefaultGroupRoleId;
                                    if ( !roleId.HasValue )
                                    {
                                        roleId = group.GroupType.Roles
                                            .OrderBy( r => r.Order )
                                            .Select( r => r.Id )
                                            .FirstOrDefault();
                                    }

                                    var registrant = registrants.FirstOrDefault( r => r.Id == registrantId );
                                    if ( registrant != null && roleId.HasValue && roleId.Value > 0 )
                                    {
                                        var groupMember = groupMemberService.Queryable().AsNoTracking()
                                            .FirstOrDefault( m =>
                                                m.PersonId == registrant.PersonAlias.PersonId &&
                                                m.GroupId == group.Id &&
                                                m.GroupRoleId == roleId.Value );
                                        if ( groupMember == null )
                                        {
                                            groupMember = new GroupMember();
                                            groupMember.PersonId = registrant.PersonAlias.PersonId;
                                            groupMember.GroupId = group.Id;
                                            groupMember.GroupRoleId = roleId.Value;
                                            groupMember.GroupMemberStatus = GroupMemberStatus.Active;

                                            if ( !groupMember.IsValidGroupMember( rockContext ) )
                                            {
                                                throw new Exception( string.Format( 
                                                    "Placing '{0}' in the '{1}' group is not valid for the following reason: {2}",
                                                    registrant.Person.FullName,
                                                    group.Name,
                                                    groupMember.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" ) ) );
                                            }

                                            groupMemberService.Add( groupMember );

                                            if ( cbSetGroupAttributes.Checked )
                                            {
                                                registrant.LoadAttributes( rockContext );
                                                groupMember.LoadAttributes( rockContext );
                                                foreach ( var attr in groupMember.Attributes.Where( m => registrant.Attributes.Keys.Contains( m.Key ) ) )
                                                {
                                                    groupMember.SetAttributeValue( attr.Key, registrant.GetAttributeValue( attr.Key ) );
                                                }
                                            }
                                            
                                            rockContext.SaveChanges();
                                            groupMember.SaveAttributeValues( rockContext );
                                        }
                                    }
                                }
                            }
                        } );

                        nbPlacementNotifiction.NotificationBoxType = NotificationBoxType.Success;
                        nbPlacementNotifiction.Text = "Registrants were successfully placed in the selected groups.";
                        nbPlacementNotifiction.Visible = true;
                    }
                    catch ( Exception ex )
                    {
                        nbPlacementNotifiction.NotificationBoxType = NotificationBoxType.Danger;
                        nbPlacementNotifiction.Text = ex.Message;
                        nbPlacementNotifiction.Visible = true;
                    }
                }
            }

            BindGroupPlacementGrid();
        }

        #endregion

        #endregion

        #region Methods

        #region Main Form Methods

        /// <summary>
        /// Gets the registration instance.
        /// </summary>
        /// <param name="registrationInstanceId">The registration instance identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private RegistrationInstance GetRegistrationInstance( int registrationInstanceId, RockContext rockContext = null )
        {
            string key = string.Format( "RegistrationInstance:{0}", registrationInstanceId );
            RegistrationInstance registrationInstance = RockPage.GetSharedItem( key ) as RegistrationInstance;
            if ( registrationInstance == null )
            {
                rockContext = rockContext ?? new RockContext();
                registrationInstance = new RegistrationInstanceService( rockContext )
                    .Queryable( "RegistrationTemplate,Account,RegistrationTemplate.Forms.Fields" )
                    .AsNoTracking()
                    .FirstOrDefault( i => i.Id == registrationInstanceId );
                RockPage.SaveSharedItem( key, registrationInstance );
            }

            return registrationInstance;
        }

        public void ShowDetail( int itemId )
        {
            ShowDetail();
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        private void ShowDetail()
        {
            int? registrationInstanceId = PageParameter( "RegistrationInstanceId" ).AsIntegerOrNull();
            int? parentTemplateId = PageParameter( "RegistrationTemplateId" ).AsIntegerOrNull();

            if ( !registrationInstanceId.HasValue )
            {
                pnlDetails.Visible = false;
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                RegistrationInstance registrationInstance = null;
                if ( registrationInstanceId.HasValue )
                {
                    registrationInstance = GetRegistrationInstance( registrationInstanceId.Value, rockContext );
                }

                if ( registrationInstance == null )
                {
                    registrationInstance = new RegistrationInstance();
                    registrationInstance.Id = 0;
                    registrationInstance.IsActive = true;
                    registrationInstance.RegistrationTemplateId = parentTemplateId ?? 0;

                    Guid? accountGuid = GetAttributeValue( "DefaultAccount" ).AsGuidOrNull();
                    if ( accountGuid.HasValue )
                    {
                        var account = new FinancialAccountService( rockContext ).Get( accountGuid.Value );
                        registrationInstance.AccountId = account != null ? account.Id : 0;
                    }
                }

                if ( registrationInstance.RegistrationTemplate == null && registrationInstance.RegistrationTemplateId > 0 )
                {
                    registrationInstance.RegistrationTemplate = new RegistrationTemplateService( rockContext )
                        .Get( registrationInstance.RegistrationTemplateId );
                }

                AvailableRegistrationAttributesForGrid = new List<AttributeCache>();

                int entityTypeId = new Registration().TypeId;
                foreach ( var attributeCache in new AttributeService( new RockContext() ).GetByEntityTypeQualifier( entityTypeId, "RegistrationTemplateId", registrationInstance.RegistrationTemplateId.ToString(), false )
                    .Where( a => a.IsGridColumn )
                    .OrderBy( a => a.Order )
                    .ThenBy( a => a.Name )
                    .ToAttributeCacheList() )
                {
                    AvailableRegistrationAttributesForGrid.Add( attributeCache );
                }


                hlType.Visible = registrationInstance.RegistrationTemplate != null;
                hlType.Text = registrationInstance.RegistrationTemplate != null ? registrationInstance.RegistrationTemplate.Name : string.Empty;

                lWizardTemplateName.Text = hlType.Text;

                pnlDetails.Visible = true;
                hfRegistrationInstanceId.Value = registrationInstance.Id.ToString();
                hfRegistrationTemplateId.Value = registrationInstance.RegistrationTemplateId.ToString();
                RegistrationTemplateId = registrationInstance.RegistrationTemplateId;
                SetHasPayments( registrationInstance.Id, rockContext );

                FollowingsHelper.SetFollowing( registrationInstance, pnlFollowing, this.CurrentPerson );

                // render UI based on Authorized
                bool readOnly = false;

                bool canEdit = UserCanEdit ||
                    registrationInstance.IsAuthorized( Authorization.EDIT, CurrentPerson ) ||
                    registrationInstance.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );

                nbEditModeMessage.Text = string.Empty;

                // User must have 'Edit' rights to block, or 'Edit' or 'Administrate' rights to instance
                if ( !canEdit )
                {
                    readOnly = true;
                    nbEditModeMessage.Heading = "Information";
                    nbEditModeMessage.Text = EditModeMessage.NotAuthorizedToEdit( RegistrationInstance.FriendlyTypeName );
                }

                if ( readOnly )
                {
                    btnEdit.Visible = false;
                    btnDelete.Visible = false;

                    bool allowRegistrationEdit = registrationInstance.IsAuthorized( "Register", CurrentPerson );
                    gRegistrations.Actions.ShowAdd = allowRegistrationEdit;
                    gRegistrations.IsDeleteEnabled = allowRegistrationEdit;

                    ShowReadonlyDetails( registrationInstance, false );
                }
                else
                {
                    btnEdit.Visible = true;
                    btnDelete.Visible = true;

                    if ( registrationInstance.Id > 0 )
                    {
                        ShowReadonlyDetails( registrationInstance, false );
                    }
                    else
                    {
                        ShowEditDetails( registrationInstance, rockContext );
                    }
                }

                // show send payment reminder link
                if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "PaymentReminderPage" ) ) && ( ( registrationInstance.RegistrationTemplate.SetCostOnInstance.HasValue && registrationInstance.RegistrationTemplate.SetCostOnInstance == true && registrationInstance.Cost.HasValue && registrationInstance.Cost.Value > 0 ) || registrationInstance.RegistrationTemplate.Cost > 0 ) )
                {
                    btnSendPaymentReminder.Visible = true;
                }
                else
                {
                    btnSendPaymentReminder.Visible = false;
                }

                LoadRegistrantFormFields( registrationInstance );
                SetUserPreferencePrefix( hfRegistrationTemplateId.ValueAsInt() );
                BindRegistrationsFilter();
                BindRegistrantsFilter( registrationInstance );
                BindWaitListFilter( registrationInstance );
                BindGroupPlacementsFilter( registrationInstance );
                BindLinkagesFilter();
                BindFeesFilter();
                BindDiscountsFilter();
                AddDynamicControls( true );

                // do the ShowTab now since it may depend on DynamicControls and Filter Bindings
                ShowTab();
            }
        }

        /// <summary>
        /// Sets the user preference prefix.
        /// </summary>
        private void SetUserPreferencePrefix(int registrationTemplateId )
        {
            fRegistrations.UserPreferenceKeyPrefix = string.Format( "{0}-", registrationTemplateId );
            fRegistrants.UserPreferenceKeyPrefix = string.Format( "{0}-", registrationTemplateId );
            fPayments.UserPreferenceKeyPrefix = string.Format( "{0}-", registrationTemplateId );
            fLinkages.UserPreferenceKeyPrefix = string.Format( "{0}-", registrationTemplateId );
            fGroupPlacements.UserPreferenceKeyPrefix = string.Format( "{0}-", registrationTemplateId );
            fWaitList.UserPreferenceKeyPrefix = string.Format( "{0}-", registrationTemplateId );
        }

        /// <summary>
        /// Sets the following on postback.
        /// </summary>
        private void SetFollowingOnPostback()
        {
            int? registrationInstanceId = PageParameter( "RegistrationInstanceId" ).AsIntegerOrNull();
            if ( registrationInstanceId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    RegistrationInstance registrationInstance = GetRegistrationInstance( registrationInstanceId.Value, rockContext );
                    if ( registrationInstance != null )
                    {
                        FollowingsHelper.SetFollowing( registrationInstance, pnlFollowing, this.CurrentPerson );
                    }
                }
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="RegistrationTemplate">The registration template.</param>
        /// <param name="rockContext">The rock context.</param>
        private void ShowEditDetails( RegistrationInstance instance, RockContext rockContext )
        {
            if ( instance.Id == 0 )
            {
                lReadOnlyTitle.Text = ActionTitle.Add( RegistrationInstance.FriendlyTypeName ).FormatAsHtmlTitle();
                hlInactive.Visible = false;
                lWizardInstanceName.Text = "New Instance";
            }
            else
            {
                lWizardInstanceName.Text = instance.Name;
            }

            pdAuditDetails.Visible = false;
            SetEditMode( true );

            rieDetails.SetValue( instance );
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="registrationInstance">The registration template.</param>
        /// <param name="setTab">if set to <c>true</c> [set tab].</param>
        private void ShowReadonlyDetails( RegistrationInstance registrationInstance, bool setTab = true )
        {
            SetEditMode( false );

            pdAuditDetails.SetEntity( registrationInstance, ResolveRockUrl( "~" ) );

            hfRegistrationInstanceId.SetValue( registrationInstance.Id );

            lReadOnlyTitle.Text = registrationInstance.Name.FormatAsHtmlTitle();
            hlInactive.Visible = registrationInstance.IsActive == false;

            lWizardInstanceName.Text = registrationInstance.Name;
            lName.Text = registrationInstance.Name;

            if ( registrationInstance.RegistrationTemplate.SetCostOnInstance ?? false )
            {
                lCost.Text = registrationInstance.Cost.FormatAsCurrency();
                lMinimumInitialPayment.Visible = registrationInstance.MinimumInitialPayment.HasValue;
                lMinimumInitialPayment.Text = registrationInstance.MinimumInitialPayment.HasValue ? registrationInstance.MinimumInitialPayment.Value.FormatAsCurrency() : string.Empty;
                lDefaultPaymentAmount.Visible = registrationInstance.DefaultPayment.HasValue;
                lDefaultPaymentAmount.Text = registrationInstance.DefaultPayment.HasValue ? registrationInstance.DefaultPayment.Value.FormatAsCurrency() : string.Empty;
            }
            else
            {
                lCost.Visible = false;
                lMinimumInitialPayment.Visible = false;
            }

            lAccount.Visible = registrationInstance.Account != null;
            lAccount.Text = registrationInstance.Account != null ? registrationInstance.Account.Name : string.Empty;

            lMaxAttendees.Visible = registrationInstance.MaxAttendees >= 0;
            lMaxAttendees.Text = registrationInstance.MaxAttendees >= 0 ?
                    registrationInstance.MaxAttendees.Value.ToString( "N0" ) :
                    string.Empty;
            lWorkflowType.Text = registrationInstance.RegistrationWorkflowType != null ?
                registrationInstance.RegistrationWorkflowType.Name : string.Empty;
            lWorkflowType.Visible = !string.IsNullOrWhiteSpace( lWorkflowType.Text );

            lStartDate.Text = registrationInstance.StartDateTime.HasValue ?
                registrationInstance.StartDateTime.Value.ToShortDateString() : string.Empty;
            lStartDate.Visible = registrationInstance.StartDateTime.HasValue;
            lEndDate.Text = registrationInstance.EndDateTime.HasValue ?
            registrationInstance.EndDateTime.Value.ToShortDateString() : string.Empty;
            lEndDate.Visible = registrationInstance.EndDateTime.HasValue;

            lDetails.Visible = !string.IsNullOrWhiteSpace( registrationInstance.Details );
            lDetails.Text = registrationInstance.Details;

            liGroupPlacement.Visible = registrationInstance.RegistrationTemplate.AllowGroupPlacement;

            liWaitList.Visible = registrationInstance.RegistrationTemplate.WaitListEnabled;

            int? groupId = GetUserPreference( string.Format( "ParentGroup_{0}_{1}", BlockId, registrationInstance.Id ) ).AsIntegerOrNull();
            if ( groupId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var group = new GroupService( rockContext ).Get( groupId.Value );
                    if ( group != null )
                    {
                        gpGroupPlacementParentGroup.SetValue( group );
                    }
                }
            }

            if ( setTab )
            {
                ShowTab();
            }
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            fieldsetViewDetails.Visible = !editable;
            pnlTabs.Visible = !editable;
        }

        /// <summary>
        /// Shows the tab.
        /// </summary>
        private void ShowTab()
        {
            liRegistrations.RemoveCssClass( "active" );
            pnlRegistrations.Visible = false;

            liRegistrants.RemoveCssClass( "active" );
            pnlRegistrants.Visible = false;

            liPayments.RemoveCssClass( "active" );
            pnlPayments.Visible = false;

            liFees.RemoveCssClass( "active" );
            pnlFees.Visible = false;

            liDiscounts.RemoveCssClass( "active" );
            pnlDiscounts.Visible = false;

            liLinkage.RemoveCssClass( "active" );
            pnlLinkages.Visible = false;

            liWaitList.RemoveCssClass( "active" );
            pnlWaitList.Visible = false;

            liGroupPlacement.RemoveCssClass( "active" );
            pnlGroupPlacement.Visible = false;

            switch ( ActiveTab ?? string.Empty )
            {
                case "lbRegistrants":
                    liRegistrants.AddCssClass( "active" );
                    pnlRegistrants.Visible = true;
                    BindRegistrantsGrid();
                    break;

                case "lbPayments":
                    liPayments.AddCssClass( "active" );
                    pnlPayments.Visible = true;
                    BindPaymentsGrid();
                    break;

                case "lbFees":
                    liFees.AddCssClass( "active" );
                    pnlFees.Visible = true;
                    BindFeesGrid();
                    break;

                case "lbDiscounts":
                    liDiscounts.AddCssClass( "active" );
                    pnlDiscounts.Visible = true;
                    BindDiscountsGrid();
                    break;

                case "lbLinkage":
                    liLinkage.AddCssClass( "active" );
                    pnlLinkages.Visible = true;
                    BindLinkagesGrid();
                    break;

                case "lbGroupPlacement":
                    liGroupPlacement.AddCssClass( "active" );
                    pnlGroupPlacement.Visible = true;
                    cbSetGroupAttributes.Checked = true;
                    BindGroupPlacementGrid();
                    break;

                case "lbWaitList":
                    liWaitList.AddCssClass( "active" );
                    pnlWaitList.Visible = true;
                    BindWaitListGrid();
                    break;

                default:
                    liRegistrations.AddCssClass( "active" );
                    pnlRegistrations.Visible = true;
                    BindRegistrationsGrid();
                    break;
            }
        }

        /// <summary>
        /// Sets whether the registration has payments.
        /// </summary>
        /// <param name="registrationInstanceId">The registration instance identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetHasPayments( int registrationInstanceId, RockContext rockContext )
        {
            var registrationIdQry = new RegistrationService( rockContext )
                .Queryable().AsNoTracking()
                .Where( r =>
                    r.RegistrationInstanceId == registrationInstanceId &&
                    !r.IsTemporary )
                .Select( r => r.Id );

            var registrationEntityType = EntityTypeCache.Get( typeof( Rock.Model.Registration ) );
            hfHasPayments.Value = new FinancialTransactionDetailService( rockContext )
                .Queryable().AsNoTracking()
                .Where( d =>
                    d.EntityTypeId.HasValue &&
                    d.EntityId.HasValue &&
                    d.EntityTypeId.Value == registrationEntityType.Id &&
                    registrationIdQry.Contains( d.EntityId.Value ) )
                .Any().ToString();
        }

        #endregion

        #region Registration Tab

        /// <summary>
        /// Binds the registrations filter.
        /// </summary>
        private void BindRegistrationsFilter()
        {
            sdrpRegistrationDateRange.DelimitedValues = fRegistrations.GetUserPreference( "Registrations Date Range" );
            ddlRegistrationPaymentStatus.SetValue( fRegistrations.GetUserPreference( "Payment Status" ) );
            tbRegistrationRegisteredByFirstName.Text = fRegistrations.GetUserPreference( "Registered By First Name" );
            tbRegistrationRegisteredByLastName.Text = fRegistrations.GetUserPreference( "Registered By Last Name" );
            tbRegistrationRegistrantFirstName.Text = fRegistrations.GetUserPreference( "Registrant First Name" );
            tbRegistrationRegistrantLastName.Text = fRegistrations.GetUserPreference( "Registrant Last Name" );
        }

        /// <summary>
        /// Binds the registrations grid.
        /// </summary>
        private void BindRegistrationsGrid()
        {
            int? instanceId = hfRegistrationInstanceId.Value.AsIntegerOrNull();
            if ( instanceId.HasValue && instanceId > 0)
            {
                using ( var rockContext = new RockContext() )
                {
                    var registrationEntityType = EntityTypeCache.Get( typeof( Rock.Model.Registration ) );

                    var instance = new RegistrationInstanceService( rockContext ).Get( instanceId.Value );
                    if ( instance != null )
                    {
                        decimal cost = instance.RegistrationTemplate.Cost;
                        if ( instance.RegistrationTemplate.SetCostOnInstance ?? false )
                        {
                            cost = instance.Cost ?? 0.0m;
                        }

                        _instanceHasCost = cost > 0.0m;
                    }

                    var qry = new RegistrationService( rockContext )
                        .Queryable( "PersonAlias.Person,Registrants.PersonAlias.Person,Registrants.Fees.RegistrationTemplateFee" )
                        .AsNoTracking()
                        .Where( r =>
                            r.RegistrationInstanceId == instanceId.Value &&
                            !r.IsTemporary );

                    var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( sdrpRegistrationDateRange.DelimitedValues );

                    if ( dateRange.Start.HasValue )
                    {
                        qry = qry.Where( r =>
                            r.CreatedDateTime.HasValue &&
                            r.CreatedDateTime.Value >= dateRange.Start.Value );
                    }

                    if ( dateRange.End.HasValue )
                    {
                        qry = qry.Where( r =>
                            r.CreatedDateTime.HasValue &&
                            r.CreatedDateTime.Value < dateRange.End.Value );
                    }

                    if ( !string.IsNullOrWhiteSpace( tbRegistrationRegisteredByFirstName.Text ) )
                    {
                        string pfname = tbRegistrationRegisteredByFirstName.Text;
                        qry = qry.Where( r =>
                            r.FirstName.StartsWith( pfname ) ||
                            r.PersonAlias.Person.NickName.StartsWith( pfname ) ||
                            r.PersonAlias.Person.FirstName.StartsWith( pfname ) );
                    }

                    if ( !string.IsNullOrWhiteSpace( tbRegistrationRegisteredByLastName.Text ) )
                    {
                        string plname = tbRegistrationRegisteredByLastName.Text;
                        qry = qry.Where( r =>
                            r.LastName.StartsWith( plname ) ||
                            r.PersonAlias.Person.LastName.StartsWith( plname ) );
                    }

                    if ( !string.IsNullOrWhiteSpace( tbRegistrationRegistrantFirstName.Text ) )
                    {
                        string rfname = tbRegistrationRegistrantFirstName.Text;
                        qry = qry.Where( r =>
                            r.Registrants.Any( p =>
                                p.PersonAlias.Person.NickName.StartsWith( rfname ) ||
                                p.PersonAlias.Person.FirstName.StartsWith( rfname ) ) );
                    }

                    if ( !string.IsNullOrWhiteSpace( tbRegistrationRegistrantLastName.Text ) )
                    {
                        string rlname = tbRegistrationRegistrantLastName.Text;
                        qry = qry.Where( r =>
                            r.Registrants.Any( p =>
                                p.PersonAlias.Person.LastName.StartsWith( rlname ) ) );
                    }

                    // If filtering on payment status, need to do some sub-querying...
                    if ( ddlRegistrationPaymentStatus.SelectedValue != string.Empty && registrationEntityType != null )
                    {
                        // Get all the registrant costs
                        var rCosts = new Dictionary<int, decimal>();
                        qry.ToList()
                            .Select( r => new
                            {
                                RegistrationId = r.Id,
                                DiscountCosts = r.Registrants.Sum( p => (decimal?) p.DiscountedCost( r.DiscountPercentage, r.DiscountAmount) ) ?? 0.0m,
                            } ).ToList()
                            .ForEach( c =>
                                rCosts.AddOrReplace( c.RegistrationId, c.DiscountCosts ) );

                        var rPayments = new Dictionary<int, decimal>();
                        new FinancialTransactionDetailService( rockContext )
                            .Queryable().AsNoTracking()
                            .Where( d =>
                                d.EntityTypeId.HasValue &&
                                d.EntityId.HasValue &&
                                d.EntityTypeId.Value == registrationEntityType.Id &&
                                rCosts.Keys.Contains( d.EntityId.Value ) )
                            .Select( d => new
                            {
                                RegistrationId = d.EntityId.Value,
                                Payment = d.Amount
                            } )
                            .ToList()
                            .GroupBy( d => d.RegistrationId )
                            .Select( d => new
                            {
                                RegistrationId = d.Key,
                                Payments = d.Sum( p => p.Payment )
                            } )
                            .ToList()
                            .ForEach( p => rPayments.AddOrReplace( p.RegistrationId, p.Payments ) );

                        var rPmtSummary = rCosts
                            .Join(
                                rPayments,
                                c => c.Key,
                                p => p.Key, 
                                ( c, p ) => new
                                {
                                    RegistrationId = c.Key,
                                    Costs = c.Value,
                                    Payments = p.Value
                                } )
                            .ToList();

                        var ids = new List<int>();

                        if ( ddlRegistrationPaymentStatus.SelectedValue == "Paid in Full" )
                        {
                            ids = rPmtSummary
                                .Where( r => r.Costs <= r.Payments )
                                .Select( r => r.RegistrationId )
                                .ToList();
                        }
                        else
                        {
                            ids = rPmtSummary
                                .Where( r => r.Costs > r.Payments )
                                .Select( r => r.RegistrationId )
                                .ToList();
                        }

                        qry = qry.Where( r => ids.Contains( r.Id ) );
                    }

                    SortProperty sortProperty = gRegistrations.SortProperty;
                    if ( sortProperty != null )
                    {
                        // If sorting by Total Cost or Balance Due, the database query needs to be run first without ordering,
                        // and then ordering needs to be done in memory since TotalCost and BalanceDue are not database fields.
                        if ( sortProperty.Property == "TotalCost" )
                        {
                            if ( sortProperty.Direction == SortDirection.Ascending )
                            {
                                gRegistrations.SetLinqDataSource( qry.ToList().OrderBy( r => r.TotalCost ).AsQueryable() );
                            }
                            else
                            {
                                gRegistrations.SetLinqDataSource( qry.ToList().OrderByDescending( r => r.TotalCost ).AsQueryable() );
                            }
                        }
                        else if ( sortProperty.Property == "BalanceDue" )
                        {
                            if ( sortProperty.Direction == SortDirection.Ascending )
                            {
                                gRegistrations.SetLinqDataSource( qry.ToList().OrderBy( r => r.BalanceDue ).AsQueryable() );
                            }
                            else
                            {
                                gRegistrations.SetLinqDataSource( qry.ToList().OrderByDescending( r => r.BalanceDue ).AsQueryable() );
                            }
                        }
                        else if ( sortProperty.Property == "RegisteredBy" )
                        {
                            // Sort by the Person name if we have it, otherwise the provided first and last name.
                            Func<Registration, string> sortBy = ( r ) =>
                            {
                                return r.PersonAlias != null && r.PersonAlias.Person != null ? r.PersonAlias.Person.FullNameReversed : string.Format( "{0}, {1}", r.LastName, r.FirstName );
                            };

                            if ( sortProperty.Direction == SortDirection.Ascending )
                            {
                                gRegistrations.SetLinqDataSource( qry.ToList().OrderBy( sortBy ).AsQueryable() );
                            }
                            else
                            {
                                gRegistrations.SetLinqDataSource( qry.ToList().OrderByDescending( sortBy ).AsQueryable() );
                            }
                        }
                        else
                        {
                            gRegistrations.SetLinqDataSource( qry.Sort( sortProperty ) );
                        }
                    }
                    else
                    {
                        gRegistrations.SetLinqDataSource( qry.OrderByDescending( r => r.CreatedDateTime ) );
                    }

                    // Get all the payments for any registrations being displayed on the current page.
                    // This is used in the RowDataBound event but queried now so that each row does
                    // not have to query for the data.
                    var currentPageRegistrations = gRegistrations.DataSource as List<Registration>;
                    if ( currentPageRegistrations != null && registrationEntityType != null )
                    {
                        var registrationIds = currentPageRegistrations
                            .Select( r => r.Id )
                            .ToList();

                        registrationPayments = new FinancialTransactionDetailService( rockContext )
                            .Queryable().AsNoTracking()
                            .Where( d =>
                                d.EntityTypeId.HasValue &&
                                d.EntityId.HasValue &&
                                d.EntityTypeId.Value == registrationEntityType.Id &&
                                registrationIds.Contains( d.EntityId.Value ) )
                            .ToList();
                    }

                    var discountCodeHeader = gRegistrations.GetColumnByHeaderText( "Discount Code" );
                    if ( discountCodeHeader != null )
                    {
                        discountCodeHeader.Visible = GetAttributeValue( "DisplayDiscountCodes" ).AsBoolean();
                    }

                    gRegistrations.DataBind();
                }
            }
        }

        #endregion

        #region Registrants Tab

        /// <summary>
        /// Binds the registrants filter.
        /// </summary>
        private void BindRegistrantsFilter( RegistrationInstance instance )
        {
            sdrpRegistrantsRegistrantDateRange.DelimitedValues = fRegistrants.GetUserPreference( "Registrants Date Range" );
            tbRegistrantsRegistrantFirstName.Text = fRegistrants.GetUserPreference( "First Name" );
            tbRegistrantsRegistrantLastName.Text = fRegistrants.GetUserPreference( "Last Name" );
            ddlRegistrantsInGroup.SetValue( fRegistrants.GetUserPreference( "In Group" ) );

            ddlRegistrantsSignedDocument.SetValue( fRegistrants.GetUserPreference( "Signed Document" ) );
            ddlRegistrantsSignedDocument.Visible = instance != null && instance.RegistrationTemplate != null && instance.RegistrationTemplate.RequiredSignatureDocumentTemplateId.HasValue;
        }

        /// <summary>
        /// Binds the registrants grid.
        /// </summary>
        private void BindRegistrantsGrid( bool isExporting = false )
        {
            _isExporting = isExporting;
            int? instanceId = hfRegistrationInstanceId.Value.AsIntegerOrNull();
            if ( instanceId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var registrationInstance = new RegistrationInstanceService( rockContext ).Get( instanceId.Value );

                    if ( registrationInstance != null &&
                        registrationInstance.RegistrationTemplate != null &&
                        registrationInstance.RegistrationTemplate.RequiredSignatureDocumentTemplateId.HasValue )
                    {
                        Signers = new SignatureDocumentService( rockContext )
                            .Queryable().AsNoTracking()
                            .Where( d =>
                                d.SignatureDocumentTemplateId == registrationInstance.RegistrationTemplate.RequiredSignatureDocumentTemplateId.Value &&
                                d.Status == SignatureDocumentStatus.Signed &&
                                d.BinaryFileId.HasValue &&
                                d.AppliesToPersonAlias != null )
                            .OrderByDescending( d => d.LastStatusDate )
                            .Select( d => d.AppliesToPersonAlias.PersonId )
                            .ToList();
                    }

                    // Start query for registrants
                    var registrationRegistrantService =  new RegistrationRegistrantService( rockContext );
                    var qry = registrationRegistrantService
                    .Queryable( "PersonAlias.Person.PhoneNumbers.NumberTypeValue,Fees.RegistrationTemplateFee,GroupMember.Group" ).AsNoTracking()
                    .Where( r =>
                        r.Registration.RegistrationInstanceId == instanceId.Value &&
                        r.PersonAlias != null &&
                        r.PersonAlias.Person != null &&
                        r.OnWaitList == false );

                    // Filter by daterange
                    var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( sdrpRegistrantsRegistrantDateRange.DelimitedValues );
                    if ( dateRange.Start.HasValue )
                    {
                        qry = qry.Where( r =>
                            r.CreatedDateTime.HasValue &&
                            r.CreatedDateTime.Value >= dateRange.Start.Value );
                    }

                    if ( dateRange.End.HasValue )
                    {
                        qry = qry.Where( r =>
                            r.CreatedDateTime.HasValue &&
                            r.CreatedDateTime.Value < dateRange.End.Value );
                    }

                    // Filter by first name
                    if ( !string.IsNullOrWhiteSpace( tbRegistrantsRegistrantFirstName.Text ) )
                    {
                        string rfname = tbRegistrantsRegistrantFirstName.Text;
                        qry = qry.Where( r =>
                            r.PersonAlias.Person.NickName.StartsWith( rfname ) ||
                            r.PersonAlias.Person.FirstName.StartsWith( rfname ) );
                    }

                    // Filter by last name
                    if ( !string.IsNullOrWhiteSpace( tbRegistrantsRegistrantLastName.Text ) )
                    {
                        string rlname = tbRegistrantsRegistrantLastName.Text;
                        qry = qry.Where( r =>
                            r.PersonAlias.Person.LastName.StartsWith( rlname ) );
                    }

                    // Filter by signed documents
                    if ( Signers != null )
                    {
                        if ( ddlRegistrantsSignedDocument.SelectedValue.AsBooleanOrNull() == true )
                        {
                            qry = qry.Where( r => Signers.Contains( r.PersonAlias.PersonId ) );
                        }
                        else if ( ddlRegistrantsSignedDocument.SelectedValue.AsBooleanOrNull() == false )
                        {
                            qry = qry.Where( r => !Signers.Contains( r.PersonAlias.PersonId ) );
                        }
                    }

                    if ( ddlRegistrantsInGroup.SelectedValue.AsBooleanOrNull() == true )
                    {
                        qry = qry.Where( r => r.GroupMemberId.HasValue );
                    }
                    else if ( ddlRegistrantsInGroup.SelectedValue.AsBooleanOrNull() == false )
                    {
                        qry = qry.Where( r => !r.GroupMemberId.HasValue );
                    }

                    bool preloadCampusValues = false;
                    var registrantAttributes = new List<AttributeCache>();
                    var personAttributes = new List<AttributeCache>();
                    var groupMemberAttributes = new List<AttributeCache>();
                    var registrantAttributeIds = new List<int>();
                    var personAttributesIds = new List<int>();
                    var groupMemberAttributesIds = new List<int>();


                    var personIds = qry.Select( r => r.PersonAlias.PersonId ).Distinct().ToList();

                    if ( isExporting || RegistrantFields != null && RegistrantFields.Any(f => f.PersonFieldType != null && f.PersonFieldType == RegistrationPersonFieldType.Address) )
                    {
                        _homeAddresses = Person.GetHomeLocations( personIds );
                    }

                    if ( RegistrantFields != null )
                    {
                        SetPhoneDictionary( rockContext, personIds );

                        // Filter by any selected
                        foreach ( var personFieldType in RegistrantFields
                            .Where( f =>
                                f.FieldSource == RegistrationFieldSource.PersonField &&
                                f.PersonFieldType.HasValue )
                            .Select( f => f.PersonFieldType.Value ) )
                        {
                            switch ( personFieldType )
                            {
                                case RegistrationPersonFieldType.Campus:
                                    preloadCampusValues = true;

                                    var ddlCampus = phRegistrantsRegistrantFormFieldFilters.FindControl( "ddlRegistrantsCampus" ) as RockDropDownList;
                                    if ( ddlCampus != null )
                                    {
                                        var campusId = ddlCampus.SelectedValue.AsIntegerOrNull();
                                        if ( campusId.HasValue )
                                        {
                                            var familyGroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();
                                            qry = qry.Where( r =>
                                                r.PersonAlias.Person.Members.Any( m =>
                                                    m.Group.GroupType.Guid == familyGroupTypeGuid &&
                                                    m.Group.CampusId.HasValue &&
                                                    m.Group.CampusId.Value == campusId ) );
                                        }
                                    }

                                    break;

                                case RegistrationPersonFieldType.Email:
                                    var tbEmailFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( "tbRegistrantsEmailFilter" ) as RockTextBox;
                                    if ( tbEmailFilter != null && !string.IsNullOrWhiteSpace( tbEmailFilter.Text ) )
                                    {
                                        qry = qry.Where( r =>
                                            r.PersonAlias.Person.Email != null &&
                                            r.PersonAlias.Person.Email.Contains( tbEmailFilter.Text ) );
                                    }

                                    break;

                                case RegistrationPersonFieldType.Birthdate:
                                    var drpBirthdateFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( "drpRegistrantsBirthdateFilter" ) as DateRangePicker;
                                    if ( drpBirthdateFilter != null )
                                    {
                                        if ( drpBirthdateFilter.LowerValue.HasValue )
                                        {
                                            qry = qry.Where( r =>
                                                r.PersonAlias.Person.BirthDate.HasValue &&
                                                r.PersonAlias.Person.BirthDate.Value >= drpBirthdateFilter.LowerValue.Value );
                                        }

                                        if ( drpBirthdateFilter.UpperValue.HasValue )
                                        {
                                            qry = qry.Where( r =>
                                                r.PersonAlias.Person.BirthDate.HasValue &&
                                                r.PersonAlias.Person.BirthDate.Value <= drpBirthdateFilter.UpperValue.Value );
                                        }
                                    }

                                    break;
                                case RegistrationPersonFieldType.MiddleName:
                                    var tbMiddleNameFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( "tbRegistrantsMiddleNameFilter" ) as RockTextBox;
                                    if ( tbMiddleNameFilter != null && !string.IsNullOrWhiteSpace( tbMiddleNameFilter.Text ) )
                                    {
                                        qry = qry.Where( r =>
                                            r.PersonAlias.Person.MiddleName != null &&
                                            r.PersonAlias.Person.MiddleName.Contains( tbMiddleNameFilter.Text ) );
                                    }
                                    break;
                                case RegistrationPersonFieldType.AnniversaryDate:
                                    var drpAnniversaryDateFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( "drpRegistrantsAnniversaryDateFilter" ) as DateRangePicker;
                                    if ( drpAnniversaryDateFilter != null )
                                    {
                                        if ( drpAnniversaryDateFilter.LowerValue.HasValue )
                                        {
                                            qry = qry.Where( r =>
                                                r.PersonAlias.Person.AnniversaryDate.HasValue &&
                                                r.PersonAlias.Person.AnniversaryDate.Value >= drpAnniversaryDateFilter.LowerValue.Value );
                                        }

                                        if ( drpAnniversaryDateFilter.UpperValue.HasValue )
                                        {
                                            qry = qry.Where( r =>
                                                r.PersonAlias.Person.AnniversaryDate.HasValue &&
                                                r.PersonAlias.Person.AnniversaryDate.Value <= drpAnniversaryDateFilter.UpperValue.Value );
                                        }
                                    }
                                    break;
                                case RegistrationPersonFieldType.Grade:
                                    var gpGradeFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( "gpRegistrantsGradeFilter" ) as GradePicker;
                                    if ( gpGradeFilter != null )
                                    {
                                        int? graduationYear = Person.GraduationYearFromGradeOffset( gpGradeFilter.SelectedValueAsInt( false ) );
                                        if ( graduationYear.HasValue )
                                        {
                                            qry = qry.Where( r =>
                                                r.PersonAlias.Person.GraduationYear.HasValue &&
                                                r.PersonAlias.Person.GraduationYear == graduationYear.Value );
                                        }
                                    }

                                    break;

                                case RegistrationPersonFieldType.Gender:
                                    var ddlGenderFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( "ddlRegistrantsGenderFilter" ) as RockDropDownList;
                                    if ( ddlGenderFilter != null )
                                    {
                                        var gender = ddlGenderFilter.SelectedValue.ConvertToEnumOrNull<Gender>();
                                        if ( gender.HasValue )
                                        {
                                            qry = qry.Where( r =>
                                                r.PersonAlias.Person.Gender == gender );
                                        }
                                    }

                                    break;

                                case RegistrationPersonFieldType.MaritalStatus:
                                    var dvpMaritalStatusFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( "dvpRegistrantsMaritalStatusFilter" ) as DefinedValuePicker;
                                    if ( dvpMaritalStatusFilter != null )
                                    {
                                        var maritalStatusId = dvpMaritalStatusFilter.SelectedValue.AsIntegerOrNull();
                                        if ( maritalStatusId.HasValue )
                                        {
                                            qry = qry.Where( r =>
                                                r.PersonAlias.Person.MaritalStatusValueId.HasValue &&
                                                r.PersonAlias.Person.MaritalStatusValueId.Value == maritalStatusId.Value );
                                        }
                                    }

                                    break;

                                case RegistrationPersonFieldType.MobilePhone:
                                    var tbMobilePhoneFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( "tbRegistrantsMobilePhoneFilter" ) as RockTextBox;
                                    if ( tbMobilePhoneFilter != null && !string.IsNullOrWhiteSpace( tbMobilePhoneFilter.Text ) )
                                    {
                                        string numericPhone = tbMobilePhoneFilter.Text.AsNumeric();
                                        if ( !string.IsNullOrEmpty( numericPhone ) )
                                        {
                                            var phoneNumberPersonIdQry = new PhoneNumberService( rockContext )
                                                .Queryable()
                                                .Where( a => a.Number.Contains( numericPhone ) )
                                                .Select( a => a.PersonId );

                                            qry = qry.Where( r => phoneNumberPersonIdQry.Contains( r.PersonAlias.PersonId ) );
                                        }
                                    }

                                    break;

                                case RegistrationPersonFieldType.HomePhone:
                                    var tbRegistrantsHomePhoneFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( "tbRegistrantsHomePhoneFilter" ) as RockTextBox;
                                    if ( tbRegistrantsHomePhoneFilter != null && !string.IsNullOrWhiteSpace( tbRegistrantsHomePhoneFilter.Text ) )
                                    {
                                        string numericPhone = tbRegistrantsHomePhoneFilter.Text.AsNumeric();
                                        if ( !string.IsNullOrEmpty( numericPhone ) )
                                        {
                                            var phoneNumberPersonIdQry = new PhoneNumberService( rockContext )
                                                .Queryable()
                                                .Where( a => a.Number.Contains( numericPhone ) )
                                                .Select( a => a.PersonId );

                                            qry = qry.Where( r => phoneNumberPersonIdQry.Contains( r.PersonAlias.PersonId ) );
                                        }
                                    }

                                    break;
                            }
                        }

                        // Get all the registrant attributes selected to be on grid
                        registrantAttributes = RegistrantFields
                            .Where( f =>
                                f.Attribute != null &&
                                f.FieldSource == RegistrationFieldSource.RegistrantAttribute )
                            .Select( f => f.Attribute )
                            .ToList();
                        registrantAttributeIds = registrantAttributes.Select( a => a.Id ).Distinct().ToList();

                        // Filter query by any configured registrant attribute filters
                        if ( registrantAttributes != null && registrantAttributes.Any() )
                        {
                            foreach ( var attribute in registrantAttributes )
                            {
                                var filterControl = phRegistrantsRegistrantFormFieldFilters.FindControl( "filterRegistrants_" + attribute.Id.ToString() );
                                qry = attribute.FieldType.Field.ApplyAttributeQueryFilter( qry, filterControl, attribute, registrationRegistrantService, Rock.Reporting.FilterMode.SimpleFilter );
                            }
                        }

                        // Get all the person attributes selected to be on grid
                        personAttributes = RegistrantFields
                            .Where( f =>
                                f.Attribute != null &&
                                f.FieldSource == RegistrationFieldSource.PersonAttribute )
                            .Select( f => f.Attribute )
                            .ToList();
                        personAttributesIds = personAttributes.Select( a => a.Id ).Distinct().ToList();

                        // Filter query by any configured person attribute filters
                        if ( personAttributes != null && personAttributes.Any() )
                        {
                            PersonService personService = new PersonService( rockContext );
                            var personQry = personService.Queryable().AsNoTracking();
                            foreach ( var attribute in personAttributes )
                            {
                                var filterControl = phRegistrantsRegistrantFormFieldFilters.FindControl( "filterRegistrants_" + attribute.Id.ToString() );
                                personQry = attribute.FieldType.Field.ApplyAttributeQueryFilter( personQry, filterControl, attribute, personService, Rock.Reporting.FilterMode.SimpleFilter );
                            }

                            qry = qry.Where( r => personQry.Any( p => p.Id == r.PersonAlias.PersonId ) );
                        }



                        // Get all the group member attributes selected to be on grid
                        groupMemberAttributes = RegistrantFields
                            .Where( f =>
                                f.Attribute != null &&
                                f.FieldSource == RegistrationFieldSource.GroupMemberAttribute )
                            .Select( f => f.Attribute )
                            .ToList();
                        groupMemberAttributesIds = groupMemberAttributes.Select( a => a.Id ).Distinct().ToList();

                        // Filter query by any configured person attribute filters
                        if ( groupMemberAttributes != null && groupMemberAttributes.Any() )
                        {
                            var groupMemberService = new GroupMemberService( rockContext );
                            var groupMemberQry = groupMemberService.Queryable().AsNoTracking();
                            foreach ( var attribute in groupMemberAttributes )
                            {
                                var filterControl = phRegistrantsRegistrantFormFieldFilters.FindControl( "filterRegistrants_" + attribute.Id.ToString() );
                                groupMemberQry = attribute.FieldType.Field.ApplyAttributeQueryFilter( groupMemberQry, filterControl, attribute, groupMemberService, Rock.Reporting.FilterMode.SimpleFilter );
                            }

                            qry = qry.Where( r => groupMemberQry.Any( g => g.Id == r.GroupMemberId ) );
                        }
                    }

                    // Sort the query
                    IOrderedQueryable<RegistrationRegistrant> orderedQry = null;
                    SortProperty sortProperty = gRegistrants.SortProperty;
                    if ( sortProperty != null )
                    {
                        orderedQry = qry.Sort( sortProperty );
                    }
                    else
                    {
                        orderedQry = qry
                            .OrderBy( r => r.PersonAlias.Person.LastName )
                            .ThenBy( r => r.PersonAlias.Person.NickName );
                    }

                    // increase the timeout just in case. A complex filter on the grid might slow things down
                    rockContext.Database.CommandTimeout = 180;

                    // Set the grids LinqDataSource which will run query and set results for current page
                    gRegistrants.SetLinqDataSource<RegistrationRegistrant>( orderedQry );

                    if ( RegistrantFields != null )
                    {
                        // Get the query results for the current page
                        var currentPageRegistrants = gRegistrants.DataSource as List<RegistrationRegistrant>;
                        if ( currentPageRegistrants != null )
                        {
                            // Get all the registrant ids in current page of query results
                            var registrantIds = currentPageRegistrants
                                .Select( r => r.Id )
                                .Distinct()
                                .ToList();

                            // Get all the person ids in current page of query results
                            var currentPagePersonIds = currentPageRegistrants
                                .Select( r => r.PersonAlias.PersonId )
                                .Distinct()
                                .ToList();

                            // Get all the group member ids and the group id in current page of query results
                            var groupMemberIds = new List<int>();
                            GroupLinks = new Dictionary<int, string>();
                            foreach ( var groupMember in currentPageRegistrants
                                .Where( m =>
                                    m.GroupMember != null &&
                                    m.GroupMember.Group != null )
                                .Select( m => m.GroupMember ) )
                            {
                                groupMemberIds.Add( groupMember.Id );

                                string linkedPageUrl = LinkedPageUrl( "GroupDetailPage", new Dictionary<string, string> { { "GroupId", groupMember.GroupId.ToString() } } );
                                GroupLinks.AddOrIgnore(
                                    groupMember.GroupId,
                                    isExporting ? groupMember.Group.Name : string.Format( "<a href='{0}'>{1}</a>", linkedPageUrl, groupMember.Group.Name ) );
                            }

                            // If the campus column was selected to be displayed on grid, preload all the people's
                            // campuses so that the databind does not need to query each row
                            if ( preloadCampusValues )
                            {
                                PersonCampusIds = new Dictionary<int, List<int>>();

                                Guid familyGroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();
                                foreach ( var personCampusList in new GroupMemberService( rockContext )
                                    .Queryable().AsNoTracking()
                                    .Where( m =>
                                        m.Group.GroupType.Guid == familyGroupTypeGuid &&
                                        currentPagePersonIds.Contains( m.PersonId ) )
                                    .GroupBy( m => m.PersonId )
                                    .Select( m => new
                                    {
                                        PersonId = m.Key,
                                        CampusIds = m
                                            .Where( g => g.Group.CampusId.HasValue )
                                            .Select( g => g.Group.CampusId.Value )
                                            .ToList()
                                    } ) )
                                {
                                    PersonCampusIds.Add( personCampusList.PersonId, personCampusList.CampusIds );
                                }
                            }

                            // If there are any attributes that were selected to be displayed, we're going
                            // to try and read all attribute values in one query and then put them into a
                            // custom grid ObjectList property so that the AttributeField columns don't need
                            // to do the LoadAttributes and querying of values for each row/column
                            if ( personAttributesIds.Any() || groupMemberAttributesIds.Any() || registrantAttributeIds.Any() )
                            {
                                // Query the attribute values for all rows and attributes
                                var attributeValues = new AttributeValueService( rockContext )
                                    .Queryable( "Attribute" ).AsNoTracking()
                                    .Where( v =>
                                        v.EntityId.HasValue &&
                                        (
                                            (
                                                personAttributesIds.Contains( v.AttributeId ) &&
                                                currentPagePersonIds.Contains( v.EntityId.Value )
                                            ) ||
                                            (
                                                groupMemberAttributesIds.Contains( v.AttributeId ) &&
                                                groupMemberIds.Contains( v.EntityId.Value )
                                            ) ||
                                            (
                                                registrantAttributeIds.Contains( v.AttributeId ) &&
                                                registrantIds.Contains( v.EntityId.Value )
                                            )
                                        ) ).ToList();

                                // Get the attributes to add to each row's object
                                var attributes = new Dictionary<string, AttributeCache>();
                                RegistrantFields
                                        .Where( f => f.Attribute != null )
                                        .Select( f => f.Attribute )
                                        .ToList()
                                    .ForEach( a => attributes
                                        .Add( a.Id.ToString() + a.Key, a ) );

                                // Initialize the grid's object list
                                gRegistrants.ObjectList = new Dictionary<string, object>();

                                // Loop through each of the current page's registrants and build an attribute
                                // field object for storing attributes and the values for each of the registrants
                                foreach ( var registrant in currentPageRegistrants )
                                {
                                    // Create a row attribute object
                                    var attributeFieldObject = new AttributeFieldObject();

                                    // Add the attributes to the attribute object
                                    attributeFieldObject.Attributes = attributes;

                                    // Add any person attribute values to object
                                    attributeValues
                                        .Where( v =>
                                            personAttributesIds.Contains( v.AttributeId ) &&
                                            v.EntityId.Value == registrant.PersonAlias.PersonId )
                                        .ToList()
                                        .ForEach( v => attributeFieldObject.AttributeValues
                                            .Add( v.AttributeId.ToString() + v.Attribute.Key, new AttributeValueCache( v ) ) );

                                    // Add any group member attribute values to object
                                    if ( registrant.GroupMemberId.HasValue )
                                    {
                                        attributeValues
                                            .Where( v =>
                                                groupMemberAttributesIds.Contains( v.AttributeId ) &&
                                                v.EntityId.Value == registrant.GroupMemberId.Value )
                                            .ToList()
                                            .ForEach( v => attributeFieldObject.AttributeValues
                                                .Add( v.AttributeId.ToString() + v.Attribute.Key, new AttributeValueCache( v ) ) );
                                    }

                                    // Add any registrant attribute values to object
                                    attributeValues
                                        .Where( v =>
                                            registrantAttributeIds.Contains( v.AttributeId ) &&
                                            v.EntityId.Value == registrant.Id )
                                        .ToList()
                                        .ForEach( v => attributeFieldObject.AttributeValues
                                            .Add( v.AttributeId.ToString() + v.Attribute.Key, new AttributeValueCache( v ) ) );

                                    // Add row attribute object to grid's object list
                                    gRegistrants.ObjectList.Add( registrant.Id.ToString(), attributeFieldObject );
                                }
                            }
                        }
                    }

                    gRegistrants.DataBind();
                }
            }
        }

        private void SetPhoneDictionary( RockContext rockContext, List<int> personIds )
        {
            if ( RegistrantFields.Any( f => f.PersonFieldType != null && f.PersonFieldType == RegistrationPersonFieldType.MobilePhone ) )
            {
                var phoneNumberService = new PhoneNumberService( rockContext );
                foreach ( var personId in personIds )
                {
                    _mobilePhoneNumbers[personId] = phoneNumberService.GetNumberByPersonIdAndType( personId, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE );
                }
            }

            if ( RegistrantFields.Any( f => f.PersonFieldType != null && f.PersonFieldType == RegistrationPersonFieldType.HomePhone ) )
            {
                var phoneNumberService = new PhoneNumberService( rockContext );
                foreach ( var personId in personIds )
                {
                    _homePhoneNumbers[personId] = phoneNumberService.GetNumberByPersonIdAndType( personId, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME );
                }
            }
        }

        /// <summary>
        /// Gets all of the form fields that were configured as 'Show on Grid' for the registration template
        /// </summary>
        /// <param name="registrationInstance">The registration instance.</param>
        private void LoadRegistrantFormFields( RegistrationInstance registrationInstance )
        {
            RegistrantFields = new List<RegistrantFormField>();

            if ( registrationInstance != null )
            {
                foreach ( var form in registrationInstance.RegistrationTemplate.Forms )
                {
                    foreach ( var formField in form.Fields
                        .Where( f => f.IsGridField )
                        .OrderBy( f => f.Order ) )
                    {
                        if ( formField.FieldSource == RegistrationFieldSource.PersonField )
                        {
                            if ( formField.PersonFieldType != RegistrationPersonFieldType.FirstName &&
                                formField.PersonFieldType != RegistrationPersonFieldType.LastName )
                            {
                                RegistrantFields.Add(
                                    new RegistrantFormField
                                    {
                                        FieldSource = formField.FieldSource,
                                        PersonFieldType = formField.PersonFieldType
                                    } );
                            }
                        }
                        else
                        {
                            RegistrantFields.Add(
                                new RegistrantFormField
                                {
                                    FieldSource = formField.FieldSource,
                                    Attribute = AttributeCache.Get( formField.AttributeId.Value )
                                } );
                        }
                    }
                }
            }
        }

        private void ClearGrid(Grid grid )
        {
            // Remove any of the dynamic person fields
            var dynamicColumns = new List<string> {
                "PersonAlias.Person.BirthDate",
            };
            foreach ( var column in grid.Columns
                .OfType<BoundField>()
                .Where( c => dynamicColumns.Contains( c.DataField ) )
                .ToList() )
            {
                grid.Columns.Remove( column );
            }

            // Remove any of the dynamic attribute fields
            foreach ( var column in grid.Columns
                .OfType<AttributeField>()
                .ToList() )
            {
                grid.Columns.Remove( column );
            }

            // Remove the fees field
            foreach ( var column in grid.Columns
                .OfType<TemplateField>()
                .Where( c => c.HeaderText == "Fees" )
                .ToList() )
            {
                grid.Columns.Remove( column );
            }

            // Remove the delete field
            foreach ( var column in grid.Columns
                .OfType<DeleteField>()
                .ToList() )
            {
                grid.Columns.Remove( column );
            }

            // Remove the delete field
            foreach ( var column in grid.Columns
                .OfType<GroupPickerField>()
                .ToList() )
            {
                grid.Columns.Remove( column );
            }
        }

        /// <summary>
        /// Adds the filter controls and grid columns for all of the registration template's form fields
        /// that were configured to 'Show on Grid'
        /// </summary>
        private void AddDynamicControls( bool setValues )
        {
            phRegistrantsRegistrantFormFieldFilters.Controls.Clear();
            phGroupPlacementsFormFieldFilters.Controls.Clear();
            phWaitListFormFieldFilters.Controls.Clear();

            ClearGrid( gGroupPlacements );
            ClearGrid( gRegistrants );
            ClearGrid( gWaitList );

            string dataFieldExpression = string.Empty;

            if ( RegistrantFields != null )
            {
                foreach ( var field in RegistrantFields )
                {
                    if ( field.FieldSource == RegistrationFieldSource.PersonField && field.PersonFieldType.HasValue )
                    {
                        switch ( field.PersonFieldType.Value )
                        {
                            case RegistrationPersonFieldType.Campus:
                                var ddlRegistrantsCampus = new RockDropDownList();
                                ddlRegistrantsCampus.ID = "ddlRegistrantsCampus";
                                ddlRegistrantsCampus.Label = "Home Campus";
                                ddlRegistrantsCampus.DataValueField = "Id";
                                ddlRegistrantsCampus.DataTextField = "Name";
                                ddlRegistrantsCampus.DataSource = CampusCache.All();
                                ddlRegistrantsCampus.DataBind();
                                ddlRegistrantsCampus.Items.Insert( 0, new ListItem( string.Empty, string.Empty ) );

                                if ( setValues )
                                {
                                    ddlRegistrantsCampus.SetValue( fRegistrants.GetUserPreference( "Home Campus" ) );
                                }

                                phRegistrantsRegistrantFormFieldFilters.Controls.Add( ddlRegistrantsCampus );

                                var ddlGroupPlacementsCampus = new RockDropDownList();
                                ddlGroupPlacementsCampus.ID = "ddlGroupPlacementsCampus";
                                ddlGroupPlacementsCampus.Label = "Home Campus";
                                ddlGroupPlacementsCampus.DataValueField = "Id";
                                ddlGroupPlacementsCampus.DataTextField = "Name";
                                ddlGroupPlacementsCampus.DataSource = CampusCache.All();
                                ddlGroupPlacementsCampus.DataBind();
                                ddlGroupPlacementsCampus.Items.Insert( 0, new ListItem( string.Empty, string.Empty ) );

                                if ( setValues )
                                {
                                    ddlGroupPlacementsCampus.SetValue( fGroupPlacements.GetUserPreference( "GroupPlacements-Home Campus" ) );
                                }

                                phGroupPlacementsFormFieldFilters.Controls.Add( ddlGroupPlacementsCampus );

                                var ddlWaitListCampus = new RockDropDownList();
                                ddlWaitListCampus.ID = "ddlWaitlistCampus";
                                ddlWaitListCampus.Label = "Home Campus";
                                ddlWaitListCampus.DataValueField = "Id";
                                ddlWaitListCampus.DataTextField = "Name";
                                ddlWaitListCampus.DataSource = CampusCache.All();
                                ddlWaitListCampus.DataBind();
                                ddlWaitListCampus.Items.Insert( 0, new ListItem( string.Empty, string.Empty ) );
                                ddlWaitListCampus.SetValue( fRegistrants.GetUserPreference( "WL-Home Campus" ) );
                                phWaitListFormFieldFilters.Controls.Add( ddlWaitListCampus );

                                var templateField = new RockLiteralField();
                                templateField.ID = "lRegistrantsCampus";
                                templateField.HeaderText = "Campus";
                                gRegistrants.Columns.Add( templateField );

                                var templateField2 = new RockLiteralField();
                                templateField2.ID = "lGroupPlacementsCampus";
                                templateField2.HeaderText = "Campus";
                                gGroupPlacements.Columns.Add( templateField2 );

                                var templateField3 = new RockLiteralField();
                                templateField3.ID = "lWaitlistCampus";
                                templateField3.HeaderText = "Campus";
                                gWaitList.Columns.Add( templateField3 );

                                break;

                            case RegistrationPersonFieldType.Email:
                                var tbRegistrantsEmailFilter = new RockTextBox();
                                tbRegistrantsEmailFilter.ID = "tbRegistrantsEmailFilter";
                                tbRegistrantsEmailFilter.Label = "Email";

                                if ( setValues )
                                {
                                    tbRegistrantsEmailFilter.Text = fRegistrants.GetUserPreference( "Email" );
                                }

                                phRegistrantsRegistrantFormFieldFilters.Controls.Add( tbRegistrantsEmailFilter );

                                var tbGroupPlacementsEmailFilter = new RockTextBox();
                                tbGroupPlacementsEmailFilter.ID = "tbGroupPlacementsEmailFilter";
                                tbGroupPlacementsEmailFilter.Label = "Email";

                                if ( setValues )
                                {
                                    tbGroupPlacementsEmailFilter.Text = fGroupPlacements.GetUserPreference( "Email" );
                                }

                                phGroupPlacementsFormFieldFilters.Controls.Add( tbGroupPlacementsEmailFilter );

                                var tbWaitlistEmailFilter = new RockTextBox();
                                tbWaitlistEmailFilter.ID = "tbWaitlistEmailFilter";
                                tbWaitlistEmailFilter.Label = "Email";
                                tbWaitlistEmailFilter.Text = fRegistrants.GetUserPreference( "WL-Email" );
                                phWaitListFormFieldFilters.Controls.Add( tbWaitlistEmailFilter );

                                dataFieldExpression = "PersonAlias.Person.Email";
                                var emailField = new RockBoundField();
                                emailField.DataField = dataFieldExpression;
                                emailField.HeaderText = "Email";
                                emailField.SortExpression = dataFieldExpression;
                                gRegistrants.Columns.Add( emailField );

                                var emailField2 = new RockBoundField();
                                emailField2.DataField = dataFieldExpression;
                                emailField2.HeaderText = "Email";
                                emailField2.SortExpression = dataFieldExpression;
                                gGroupPlacements.Columns.Add( emailField2 );

                                var emailField3 = new RockBoundField();
                                emailField3.DataField = dataFieldExpression;
                                emailField3.HeaderText = "Email";
                                emailField3.SortExpression = dataFieldExpression;
                                gWaitList.Columns.Add( emailField3 );

                                break;

                            case RegistrationPersonFieldType.Birthdate:
                                var drpRegistrantsBirthdateFilter = new DateRangePicker();
                                drpRegistrantsBirthdateFilter.ID = "drpRegistrantsBirthdateFilter";
                                drpRegistrantsBirthdateFilter.Label = "Birthdate Range";

                                if ( setValues )
                                {
                                    drpRegistrantsBirthdateFilter.DelimitedValues = fRegistrants.GetUserPreference( "Birthdate Range" );
                                }

                                phRegistrantsRegistrantFormFieldFilters.Controls.Add( drpRegistrantsBirthdateFilter );

                                var drpGroupPlacementsBirthdateFilter = new DateRangePicker();
                                drpGroupPlacementsBirthdateFilter.ID = "drpGroupPlacementsBirthdateFilter";
                                drpGroupPlacementsBirthdateFilter.Label = "Birthdate Range";

                                if ( setValues )
                                {
                                    drpGroupPlacementsBirthdateFilter.DelimitedValues = fGroupPlacements.GetUserPreference( "GroupPlacements-Birthdate Range" );
                                }

                                phGroupPlacementsFormFieldFilters.Controls.Add( drpGroupPlacementsBirthdateFilter );

                                var drpWaitlistBirthdateFilter = new DateRangePicker();
                                drpWaitlistBirthdateFilter.ID = "drpWaitlistBirthdateFilter";
                                drpWaitlistBirthdateFilter.Label = "Birthdate Range";
                                drpWaitlistBirthdateFilter.DelimitedValues = fRegistrants.GetUserPreference( "WL-Birthdate Range" );
                                phWaitListFormFieldFilters.Controls.Add( drpWaitlistBirthdateFilter );

                                dataFieldExpression = "PersonAlias.Person.BirthDate";
                                var birthdateField = new DateField();
                                birthdateField.DataField = dataFieldExpression;
                                birthdateField.HeaderText = "Birthdate";
                                birthdateField.IncludeAge = true;
                                birthdateField.SortExpression = dataFieldExpression;
                                gRegistrants.Columns.Add( birthdateField );

                                var birthdateField2 = new DateField();
                                birthdateField2.DataField = dataFieldExpression;
                                birthdateField2.HeaderText = "Birthdate";
                                birthdateField2.IncludeAge = true;
                                birthdateField2.SortExpression = dataFieldExpression;
                                gGroupPlacements.Columns.Add( birthdateField2 );

                                var birthdateField3 = new DateField();
                                birthdateField3.DataField = dataFieldExpression;
                                birthdateField3.HeaderText = "Birthdate";
                                birthdateField3.IncludeAge = true;
                                birthdateField3.SortExpression = dataFieldExpression;
                                gWaitList.Columns.Add( birthdateField3 );

                                break;
                            case RegistrationPersonFieldType.MiddleName:
                                var tbRegistrantsMiddleNameFilter = new RockTextBox();
                                tbRegistrantsMiddleNameFilter.ID = "tbRegistrantsMiddleNameFilter";
                                tbRegistrantsMiddleNameFilter.Label = "MiddleName";

                                if ( setValues )
                                {
                                    tbRegistrantsMiddleNameFilter.Text = fRegistrants.GetUserPreference( "MiddleName" );
                                }

                                phRegistrantsRegistrantFormFieldFilters.Controls.Add( tbRegistrantsMiddleNameFilter );

                                var tbGroupPlacementsMiddleNameFilter = new RockTextBox();
                                tbGroupPlacementsMiddleNameFilter.ID = "tbGroupPlacementsMiddleNameFilter";
                                tbGroupPlacementsMiddleNameFilter.Label = "MiddleName";

                                if ( setValues )
                                {
                                    tbGroupPlacementsMiddleNameFilter.Text = fGroupPlacements.GetUserPreference( "MiddleName" );
                                }

                                phGroupPlacementsFormFieldFilters.Controls.Add( tbGroupPlacementsMiddleNameFilter );

                                var tbWaitlistMiddleNameFilter = new RockTextBox();
                                tbWaitlistMiddleNameFilter.ID = "tbWaitlistMiddleNameFilter";
                                tbWaitlistMiddleNameFilter.Label = "Email";
                                tbWaitlistMiddleNameFilter.Text = fRegistrants.GetUserPreference( "WL-MiddleName" );
                                phWaitListFormFieldFilters.Controls.Add( tbWaitlistMiddleNameFilter );

                                dataFieldExpression = "PersonAlias.Person.MiddleName";
                                var middleNameField = new RockBoundField();
                                middleNameField.DataField = dataFieldExpression;
                                middleNameField.HeaderText = "MiddleName";
                                middleNameField.SortExpression = dataFieldExpression;
                                gRegistrants.Columns.Add( middleNameField );

                                var middleNameField2 = new RockBoundField();
                                middleNameField2.DataField = dataFieldExpression;
                                middleNameField2.HeaderText = "MiddleName";
                                middleNameField2.SortExpression = dataFieldExpression;
                                gGroupPlacements.Columns.Add( middleNameField2 );

                                var middleNameField3 = new RockBoundField();
                                middleNameField3.DataField = dataFieldExpression;
                                middleNameField3.HeaderText = "MiddleName";
                                middleNameField3.SortExpression = dataFieldExpression;
                                gWaitList.Columns.Add( middleNameField3 );

                                break;

                            case RegistrationPersonFieldType.AnniversaryDate:
                                var drpRegistrantsAnniversaryDateFilter = new DateRangePicker();
                                drpRegistrantsAnniversaryDateFilter.ID = "drpRegistrantsAnniversaryDateFilter";
                                drpRegistrantsAnniversaryDateFilter.Label = "AnniversaryDate Range";

                                if ( setValues )
                                {
                                    drpRegistrantsAnniversaryDateFilter.DelimitedValues = fRegistrants.GetUserPreference( "AnniversaryDate Range" );
                                }

                                phRegistrantsRegistrantFormFieldFilters.Controls.Add( drpRegistrantsAnniversaryDateFilter );

                                var drpGroupPlacementsAnniversaryDateFilter = new DateRangePicker();
                                drpGroupPlacementsAnniversaryDateFilter.ID = "drpGroupPlacementsAnniversaryDateFilter";
                                drpGroupPlacementsAnniversaryDateFilter.Label = "AnniversaryDate Range";

                                if ( setValues )
                                {
                                    drpGroupPlacementsAnniversaryDateFilter.DelimitedValues = fGroupPlacements.GetUserPreference( "GroupPlacements-AnniversaryDate Range" );
                                }

                                phGroupPlacementsFormFieldFilters.Controls.Add( drpGroupPlacementsAnniversaryDateFilter );

                                var drpWaitlistAnniversaryDateFilter = new DateRangePicker();
                                drpWaitlistAnniversaryDateFilter.ID = "drpWaitlistAnniversaryDateFilter";
                                drpWaitlistAnniversaryDateFilter.Label = "AnniversaryDate Range";
                                drpWaitlistAnniversaryDateFilter.DelimitedValues = fRegistrants.GetUserPreference( "WL-AnniversaryDate Range" );
                                phWaitListFormFieldFilters.Controls.Add( drpWaitlistAnniversaryDateFilter );

                                dataFieldExpression = "PersonAlias.Person.AnniversaryDate";
                                var anniversaryDateField = new DateField();
                                anniversaryDateField.DataField = dataFieldExpression;
                                anniversaryDateField.HeaderText = "Anniversary Date";
                                anniversaryDateField.IncludeAge = true;
                                anniversaryDateField.SortExpression = dataFieldExpression;
                                gRegistrants.Columns.Add( anniversaryDateField );

                                var anniversaryDateField2 = new DateField();
                                anniversaryDateField2.DataField = dataFieldExpression;
                                anniversaryDateField2.HeaderText = "Anniversary Date";
                                anniversaryDateField2.IncludeAge = true;
                                anniversaryDateField2.SortExpression = dataFieldExpression;
                                gGroupPlacements.Columns.Add( anniversaryDateField2 );

                                var anniversaryDateField3 = new DateField();
                                anniversaryDateField3.DataField = dataFieldExpression;
                                anniversaryDateField3.HeaderText = "Anniversary Date";
                                anniversaryDateField3.IncludeAge = true;
                                anniversaryDateField3.SortExpression = dataFieldExpression;
                                gWaitList.Columns.Add( anniversaryDateField3 );

                                break;
                            case RegistrationPersonFieldType.Grade:
                                var gpRegistrantsGradeFilter = new GradePicker();
                                gpRegistrantsGradeFilter.ID = "gpRegistrantsGradeFilter";
                                gpRegistrantsGradeFilter.Label = "Grade";
                                gpRegistrantsGradeFilter.UseAbbreviation = true;
                                gpRegistrantsGradeFilter.UseGradeOffsetAsValue = true;
                                gpRegistrantsGradeFilter.CssClass = "input-width-md";
                                    
                                // Since 12th grade is the 0 Value, we need to handle the "no user preference" differently
                                // by not calling SetValue otherwise it will select 12th grade.
                                if ( setValues )
                                {
                                    var registrantsGradeUserPreference = fRegistrants.GetUserPreference( "Grade" ).AsIntegerOrNull();
                                    if ( registrantsGradeUserPreference != null )
                                    {
                                        gpRegistrantsGradeFilter.SetValue( registrantsGradeUserPreference );
                                    }
                                }

                                phRegistrantsRegistrantFormFieldFilters.Controls.Add( gpRegistrantsGradeFilter );

                                var gpGroupPlacementsGradeFilter = new GradePicker();
                                gpGroupPlacementsGradeFilter.ID = "gpGroupPlacementsGradeFilter";
                                gpGroupPlacementsGradeFilter.Label = "Grade";
                                gpGroupPlacementsGradeFilter.UseAbbreviation = true;
                                gpGroupPlacementsGradeFilter.UseGradeOffsetAsValue = true;
                                gpGroupPlacementsGradeFilter.CssClass = "input-width-md";

                                // Since 12th grade is the 0 Value, we need to handle the "no user preference" differently
                                // by not calling SetValue otherwise it will select 12th grade.
                                if ( setValues )
                                {
                                    var groupPlacementsGradeUserPreference = fGroupPlacements.GetUserPreference( "GroupPlacements-Grade" ).AsIntegerOrNull();
                                    if ( groupPlacementsGradeUserPreference != null )
                                    {
                                        gpGroupPlacementsGradeFilter.SetValue( groupPlacementsGradeUserPreference );
                                    }
                                }

                                phGroupPlacementsFormFieldFilters.Controls.Add( gpGroupPlacementsGradeFilter );

                                var gpWaitlistGradeFilter = new GradePicker();
                                gpWaitlistGradeFilter.ID = "gpWaitlistGradeFilter";
                                gpWaitlistGradeFilter.Label = "Grade";
                                gpWaitlistGradeFilter.UseAbbreviation = true;
                                gpWaitlistGradeFilter.UseGradeOffsetAsValue = true;
                                gpWaitlistGradeFilter.CssClass = "input-width-md";
                                var wlGradeUserPreference = fRegistrants.GetUserPreference( "WL-Grade" ).AsIntegerOrNull();
                                if ( wlGradeUserPreference != null )
                                {
                                    gpWaitlistGradeFilter.SetValue( wlGradeUserPreference );
                                }

                                phWaitListFormFieldFilters.Controls.Add( gpWaitlistGradeFilter );

                                // 2017-01-13 as discussed, changing this to Grade but keeping the sort based on grad year
                                dataFieldExpression = "PersonAlias.Person.GradeFormatted";
                                var gradeField = new RockBoundField();
                                gradeField.DataField = dataFieldExpression;
                                gradeField.HeaderText = "Grade";
                                gradeField.SortExpression = "PersonAlias.Person.GraduationYear";
                                gRegistrants.Columns.Add( gradeField );

                                var gradeField2 = new RockBoundField();
                                gradeField2.DataField = dataFieldExpression;
                                gradeField2.HeaderText = "Grade";
                                gGroupPlacements.Columns.Add( gradeField2 );

                                var gradeField3 = new RockBoundField();
                                gradeField3.DataField = dataFieldExpression;
                                gradeField3.HeaderText = "Grade";
                                gWaitList.Columns.Add( gradeField3 );

                                break;

                            case RegistrationPersonFieldType.Gender:
                                var ddlRegistrantsGenderFilter = new RockDropDownList();
                                ddlRegistrantsGenderFilter.BindToEnum<Gender>( true );
                                ddlRegistrantsGenderFilter.ID = "ddlRegistrantsGenderFilter";
                                ddlRegistrantsGenderFilter.Label = "Gender";

                                if ( setValues )
                                {
                                    ddlRegistrantsGenderFilter.SetValue( fRegistrants.GetUserPreference( "Gender" ) );
                                }

                                phRegistrantsRegistrantFormFieldFilters.Controls.Add( ddlRegistrantsGenderFilter );

                                var ddlGroupPlacementsGenderFilter = new RockDropDownList();
                                ddlGroupPlacementsGenderFilter.BindToEnum<Gender>( true );
                                ddlGroupPlacementsGenderFilter.ID = "ddlGroupPlacementsGenderFilter";
                                ddlGroupPlacementsGenderFilter.Label = "Gender";

                                if ( setValues )
                                {
                                    ddlGroupPlacementsGenderFilter.SetValue( fGroupPlacements.GetUserPreference( "GroupPlacements-Gender" ) );
                                }

                                phGroupPlacementsFormFieldFilters.Controls.Add( ddlGroupPlacementsGenderFilter );

                                var ddlWaitlistGenderFilter = new RockDropDownList();
                                ddlWaitlistGenderFilter.BindToEnum<Gender>( true );
                                ddlWaitlistGenderFilter.ID = "ddlWaitlistGenderFilter";
                                ddlWaitlistGenderFilter.Label = "Gender";
                                ddlWaitlistGenderFilter.SetValue( fWaitList.GetUserPreference( "WL-Gender" ) );
                                phWaitListFormFieldFilters.Controls.Add( ddlWaitlistGenderFilter );

                                dataFieldExpression = "PersonAlias.Person.Gender";
                                var genderField = new EnumField();
                                genderField.DataField = dataFieldExpression;
                                genderField.HeaderText = "Gender";
                                genderField.SortExpression = dataFieldExpression;
                                gRegistrants.Columns.Add( genderField );

                                var genderField2 = new EnumField();
                                genderField2.DataField = dataFieldExpression;
                                genderField2.HeaderText = "Gender";
                                genderField2.SortExpression = dataFieldExpression;
                                gGroupPlacements.Columns.Add( genderField2 );

                                var genderField3 = new EnumField();
                                genderField3.DataField = dataFieldExpression;
                                genderField3.HeaderText = "Gender";
                                genderField3.SortExpression = dataFieldExpression;
                                gWaitList.Columns.Add( genderField3 );

                                break;

                            case RegistrationPersonFieldType.MaritalStatus:
                                var dvpRegistrantsMaritalStatusFilter = new DefinedValuePicker();
                                dvpRegistrantsMaritalStatusFilter.ID = "dvpRegistrantsMaritalStatusFilter";
                                dvpRegistrantsMaritalStatusFilter.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS.AsGuid() ).Id;
                                dvpRegistrantsMaritalStatusFilter.Label = "Marital Status";

                                if ( setValues )
                                {
                                    dvpRegistrantsMaritalStatusFilter.SetValue( fRegistrants.GetUserPreference( "Marital Status" ) );
                                }

                                phRegistrantsRegistrantFormFieldFilters.Controls.Add( dvpRegistrantsMaritalStatusFilter );

                                var dvpGroupPlacementsMaritalStatusFilter = new DefinedValuePicker();
                                dvpGroupPlacementsMaritalStatusFilter.ID = "dvpGroupPlacementsMaritalStatusFilter";
                                dvpGroupPlacementsMaritalStatusFilter.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS.AsGuid() ).Id;
                                dvpGroupPlacementsMaritalStatusFilter.Label = "Marital Status";

                                if ( setValues )
                                {
                                    dvpGroupPlacementsMaritalStatusFilter.SetValue( fGroupPlacements.GetUserPreference( "GroupPlacements-Marital Status" ) );
                                }

                                phGroupPlacementsFormFieldFilters.Controls.Add( dvpGroupPlacementsMaritalStatusFilter );

                                var dvpWaitlistMaritalStatusFilter = new DefinedValuePicker();
                                dvpWaitlistMaritalStatusFilter.ID = "dvpWaitlistMaritalStatusFilter";
                                dvpWaitlistMaritalStatusFilter.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS.AsGuid() ).Id;
                                dvpWaitlistMaritalStatusFilter.Label = "Marital Status";
                                dvpWaitlistMaritalStatusFilter.SetValue( fRegistrants.GetUserPreference( "WL-Marital Status" ) );
                                phWaitListFormFieldFilters.Controls.Add( dvpWaitlistMaritalStatusFilter );

                                dataFieldExpression = "PersonAlias.Person.MaritalStatusValue.Value";
                                var maritalStatusField = new RockBoundField();
                                maritalStatusField.DataField = dataFieldExpression;
                                maritalStatusField.HeaderText = "MaritalStatus";
                                maritalStatusField.SortExpression = dataFieldExpression;
                                gRegistrants.Columns.Add( maritalStatusField );

                                var maritalStatusField2 = new RockBoundField();
                                maritalStatusField2.DataField = dataFieldExpression;
                                maritalStatusField2.HeaderText = "MaritalStatus";
                                maritalStatusField2.SortExpression = dataFieldExpression;
                                gGroupPlacements.Columns.Add( maritalStatusField2 );

                                var maritalStatusField3 = new RockBoundField();
                                maritalStatusField3.DataField = dataFieldExpression;
                                maritalStatusField3.HeaderText = "MaritalStatus";
                                maritalStatusField3.SortExpression = dataFieldExpression;
                                gWaitList.Columns.Add( maritalStatusField3 );

                                break;

                            case RegistrationPersonFieldType.MobilePhone:
                                // Per discussion this should not have "Phone" appended to the end if it's missing.
                                var mobileLabel = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ).Value;

                                var tbRegistrantsMobilePhoneFilter = new RockTextBox();
                                tbRegistrantsMobilePhoneFilter.ID = "tbRegistrantsMobilePhoneFilter";
                                tbRegistrantsMobilePhoneFilter.Label = mobileLabel;

                                if ( setValues )
                                {
                                    tbRegistrantsMobilePhoneFilter.Text = fRegistrants.GetUserPreference( "Cell Phone" );
                                }

                                phRegistrantsRegistrantFormFieldFilters.Controls.Add( tbRegistrantsMobilePhoneFilter );

                                var tbGroupPlacementsMobilePhoneFilter = new RockTextBox();
                                tbGroupPlacementsMobilePhoneFilter.ID = "tbGroupPlacementsMobilePhoneFilter";
                                tbGroupPlacementsMobilePhoneFilter.Label = mobileLabel;

                                if ( setValues )
                                {
                                    tbGroupPlacementsMobilePhoneFilter.Text = fGroupPlacements.GetUserPreference( "GroupPlacements-Phone" );
                                }

                                phGroupPlacementsFormFieldFilters.Controls.Add( tbGroupPlacementsMobilePhoneFilter );

                                var tbWaitlistMobilePhoneFilter = new RockTextBox();
                                tbWaitlistMobilePhoneFilter.ID = "tbWaitlistMobilePhoneFilter";
                                tbWaitlistMobilePhoneFilter.Label = mobileLabel;
                                tbWaitlistMobilePhoneFilter.Text = fRegistrants.GetUserPreference( "WL-Phone" );
                                phWaitListFormFieldFilters.Controls.Add( tbWaitlistMobilePhoneFilter );

                                var phoneNumbersField = new RockLiteralField();
                                phoneNumbersField.ID = "lRegistrantsMobile";
                                phoneNumbersField.HeaderText = mobileLabel;
                                gRegistrants.Columns.Add( phoneNumbersField );

                                var phoneNumbersField2 = new RockLiteralField();
                                phoneNumbersField2.ID = "lGroupPlacementsMobile";
                                phoneNumbersField2.HeaderText = mobileLabel;
                                gGroupPlacements.Columns.Add( phoneNumbersField2 );

                                var phoneNumbersField3 = new RockLiteralField();
                                phoneNumbersField3.ID = "lWaitlistMobile";
                                phoneNumbersField3.HeaderText = mobileLabel;
                                gWaitList.Columns.Add( phoneNumbersField3 );

                                break;

                            case RegistrationPersonFieldType.HomePhone:
                                // Per discussion this should not have "Phone" appended to the end if it's missing.
                                var homePhoneLabel = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME ).Value;

                                var tbRegistrantsHomePhoneFilter = new RockTextBox();
                                tbRegistrantsHomePhoneFilter.ID = "tbRegistrantsHomePhoneFilter";
                                tbRegistrantsHomePhoneFilter.Label = homePhoneLabel;

                                if ( setValues )
                                {
                                    tbRegistrantsHomePhoneFilter.Text = fRegistrants.GetUserPreference( "Home Phone" );
                                }

                                phRegistrantsRegistrantFormFieldFilters.Controls.Add( tbRegistrantsHomePhoneFilter );

                                var tbGroupPlacementsHomePhoneFilter = new RockTextBox();
                                tbGroupPlacementsHomePhoneFilter.ID = "tbGroupPlacementsHomePhoneFilter";
                                tbGroupPlacementsHomePhoneFilter.Label = homePhoneLabel;

                                if ( setValues )
                                {
                                    tbGroupPlacementsHomePhoneFilter.Text = fGroupPlacements.GetUserPreference( "GroupPlacements-HomePhone" );
                                }

                                phGroupPlacementsFormFieldFilters.Controls.Add( tbGroupPlacementsHomePhoneFilter );

                                var tbWaitlistHomePhoneFilter = new RockTextBox();
                                tbWaitlistHomePhoneFilter.ID = "tbWaitlistHomePhoneFilter";
                                tbWaitlistHomePhoneFilter.Label = homePhoneLabel;
                                tbWaitlistHomePhoneFilter.Text = fRegistrants.GetUserPreference( "WL-HomePhone" );
                                phWaitListFormFieldFilters.Controls.Add( tbWaitlistHomePhoneFilter );

                                var homePhoneNumbersField = new RockLiteralField();
                                homePhoneNumbersField.ID = "lRegistrantsHomePhone";
                                homePhoneNumbersField.HeaderText = homePhoneLabel;
                                gRegistrants.Columns.Add( homePhoneNumbersField );

                                var homePhoneNumbersField2 = new RockLiteralField();
                                homePhoneNumbersField2.ID = "lGroupPlacementsHomePhone";
                                homePhoneNumbersField2.HeaderText = homePhoneLabel;
                                gGroupPlacements.Columns.Add( homePhoneNumbersField2 );

                                var homePhoneNumbersField3 = new RockLiteralField();
                                homePhoneNumbersField3.ID = "lWaitlistHomePhone";
                                homePhoneNumbersField3.HeaderText = homePhoneLabel;
                                gWaitList.Columns.Add( homePhoneNumbersField3 );

                                break;

                            case RegistrationPersonFieldType.Address:
                                var addressField = new RockLiteralField();
                                addressField.ID = "lRegistrantsAddress";
                                addressField.HeaderText = "Address";
                                // There are specific Street1, Street2, City, etc. fields included instead
                                addressField.ExcelExportBehavior = ExcelExportBehavior.NeverInclude;
                                gRegistrants.Columns.Add( addressField );

                                var addressField2 = new RockLiteralField();
                                addressField2.ID = "lGroupPlacementsAddress";
                                addressField2.HeaderText = "Address";
                                gGroupPlacements.Columns.Add( addressField2 );

                                var addressField3 = new RockLiteralField();
                                addressField3.ID = "lWaitlistAddress";
                                addressField3.HeaderText = "Address";
                                gWaitList.Columns.Add( addressField3 );
                                break;
                        }
                    }
                    else if ( field.Attribute != null )
                    {
                        var attribute = field.Attribute;

                        // add dynamic filter to registrant grid
                        var registrantsControl = attribute.FieldType.Field.FilterControl( attribute.QualifierValues, "filterRegistrants_" + attribute.Id.ToString(), false, Rock.Reporting.FilterMode.SimpleFilter );
                        if ( registrantsControl != null )
                        {
                            if ( registrantsControl is IRockControl )
                            {
                                var rockControl = (IRockControl) registrantsControl;
                                rockControl.Label = attribute.Name;
                                rockControl.Help = attribute.Description;
                                phRegistrantsRegistrantFormFieldFilters.Controls.Add( registrantsControl );
                            }
                            else
                            {
                                var wrapper = new RockControlWrapper();
                                wrapper.ID = registrantsControl.ID + "_wrapper";
                                wrapper.Label = attribute.Name;
                                wrapper.Controls.Add( registrantsControl );
                                phRegistrantsRegistrantFormFieldFilters.Controls.Add( wrapper );
                            }

                            if ( setValues )
                            {
                                string savedValue = fRegistrants.GetUserPreference( attribute.Key );
                                if ( !string.IsNullOrWhiteSpace( savedValue ) )
                                {
                                    try
                                    {
                                        var values = JsonConvert.DeserializeObject<List<string>>( savedValue );
                                        attribute.FieldType.Field.SetFilterValues( registrantsControl, attribute.QualifierValues, values );
                                    }
                                    catch
                                    {
                                    }
                                }
                            }
                        }

                        // add dynamic filter to registrant grid
                        var groupPlacementsControl = attribute.FieldType.Field.FilterControl( attribute.QualifierValues, "filterGroupPlacements_" + attribute.Id.ToString(), false, Rock.Reporting.FilterMode.SimpleFilter );
                        if ( groupPlacementsControl != null )
                        {
                            if ( groupPlacementsControl is IRockControl )
                            {
                                var rockControl = ( IRockControl ) groupPlacementsControl;
                                rockControl.Label = attribute.Name;
                                rockControl.Help = attribute.Description;
                                phGroupPlacementsFormFieldFilters.Controls.Add( groupPlacementsControl );
                            }
                            else
                            {
                                var wrapper = new RockControlWrapper();
                                wrapper.ID = groupPlacementsControl.ID + "_wrapper";
                                wrapper.Label = attribute.Name;
                                wrapper.Controls.Add( groupPlacementsControl );
                                phGroupPlacementsFormFieldFilters.Controls.Add( wrapper );
                            }

                            if ( setValues )
                            {
                                string savedValue = fRegistrants.GetUserPreference( "GroupPlacements-" + attribute.Key );
                                if ( !string.IsNullOrWhiteSpace( savedValue ) )
                                {
                                    try
                                    {
                                        var values = JsonConvert.DeserializeObject<List<string>>( savedValue );
                                        attribute.FieldType.Field.SetFilterValues( groupPlacementsControl, attribute.QualifierValues, values );
                                    }
                                    catch
                                    {
                                    }
                                }
                            }
                        }

                        // add dynamic filter to wait list grid
                        var waitListControl = attribute.FieldType.Field.FilterControl( attribute.QualifierValues, "filterWaitList_" + attribute.Id.ToString(), false, Rock.Reporting.FilterMode.SimpleFilter );
                        if ( waitListControl != null )
                        {
                            if ( waitListControl is IRockControl )
                            {
                                var rockControl2 = (IRockControl)waitListControl;
                                rockControl2.Label = attribute.Name;
                                rockControl2.Help = attribute.Description;
                                phWaitListFormFieldFilters.Controls.Add( waitListControl );
                            }
                            else
                            {
                                var wrapper2 = new RockControlWrapper();
                                wrapper2.ID = waitListControl.ID + "_wrapper";
                                wrapper2.Label = attribute.Name;
                                wrapper2.Controls.Add( waitListControl );
                                phWaitListFormFieldFilters.Controls.Add( wrapper2 );
                            }

                            string savedValue = fWaitList.GetUserPreference( "WL-" + attribute.Key );
                            if ( !string.IsNullOrWhiteSpace( savedValue ) )
                            {
                                try
                                {
                                    var values = JsonConvert.DeserializeObject<List<string>>( savedValue );
                                    attribute.FieldType.Field.SetFilterValues( waitListControl, attribute.QualifierValues, values );
                                }
                                catch
                                {
                                }
                            }
                        }

                        dataFieldExpression = attribute.Id.ToString() + attribute.Key;
                        bool columnExists = gRegistrants.Columns.OfType<AttributeField>().FirstOrDefault( a => a.DataField.Equals( dataFieldExpression ) ) != null;
                        if ( !columnExists )
                        {
                            AttributeField boundField = new AttributeField();
                            boundField.DataField = dataFieldExpression;
                            boundField.AttributeId = attribute.Id;
                            boundField.HeaderText = attribute.Name;

                            AttributeField boundField2 = new AttributeField();
                            boundField2.DataField = dataFieldExpression;
                            boundField2.AttributeId = attribute.Id;
                            boundField2.HeaderText = attribute.Name;

                            AttributeField boundField3 = new AttributeField();
                            boundField3.DataField = dataFieldExpression;
                            boundField3.AttributeId = attribute.Id;
                            boundField3.HeaderText = attribute.Name;

                            var attributeCache = Rock.Web.Cache.AttributeCache.Get( attribute.Id );
                            if ( attributeCache != null )
                            {
                                boundField.ItemStyle.HorizontalAlign = attributeCache.FieldType.Field.AlignValue;
                                boundField2.ItemStyle.HorizontalAlign = attributeCache.FieldType.Field.AlignValue;
                                boundField3.ItemStyle.HorizontalAlign = attributeCache.FieldType.Field.AlignValue;
                            }

                            gRegistrants.Columns.Add( boundField );

                            gGroupPlacements.Columns.Add( boundField2 );

                            gWaitList.Columns.Add( boundField3 );
                        }
                    }
                }
            }

            // Add fee column
            var feeField = new RockLiteralField();
            feeField.ID = "lFees";
            feeField.HeaderText = "Fees";
            gRegistrants.Columns.Add( feeField );

            var deleteField = new DeleteField();
            gRegistrants.Columns.Add( deleteField );
            deleteField.Click += gRegistrants_Delete;

            var groupPickerField = new GroupPickerField();
            groupPickerField.HeaderText = "Group";
            groupPickerField.RootGroupId = gpGroupPlacementParentGroup.SelectedValueAsInt();
            gGroupPlacements.Columns.Add( groupPickerField );

            // Remove attribute columns
            foreach ( var column in gRegistrations.Columns.OfType<AttributeField>().ToList() )
            {
                gRegistrations.Columns.Remove( column );
            }

            if ( AvailableRegistrationAttributesForGrid != null )
            {
                var registrantsField = gRegistrations.ColumnsOfType<RockTemplateField>().FirstOrDefault( a => a.HeaderText == "Registrants" );
                int columnInsertIndex = gRegistrations.Columns.IndexOf( registrantsField ) + 1;
                foreach ( var attributeCache in AvailableRegistrationAttributesForGrid )
                {
                    bool columnExists = gRegistrations.Columns.OfType<AttributeField>().FirstOrDefault( a => a.AttributeId == attributeCache.Id ) != null;
                    if ( !columnExists )
                    {
                        AttributeField boundField = new AttributeField();
                        boundField.DataField = attributeCache.Key;
                        boundField.AttributeId = attributeCache.Id;
                        boundField.HeaderText = attributeCache.Name;
                        boundField.ItemStyle.HorizontalAlign = attributeCache.FieldType.Field.AlignValue;
                        gRegistrations.Columns.Insert( columnInsertIndex++, boundField );
                    }
                }
            }
        }

        #endregion

        #region Payments Tab

        /// <summary>
        /// Binds the payments filter.
        /// </summary>
        private void BindPaymentsFilter()
        {
            sdrpPaymentDateRange.DelimitedValues = fPayments.GetUserPreference( "Payments Date Range" );
        }

        /// <summary>
        /// Binds the payments grid.
        /// </summary>
        private void BindPaymentsGrid()
        {
            int? instanceId = hfRegistrationInstanceId.Value.AsIntegerOrNull();
            if ( instanceId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var currencyTypes = new Dictionary<int, string>();
                    var creditCardTypes = new Dictionary<int, string>();

                    // If configured for a registration and registration is null, return
                    int registrationEntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Registration ) ).Id;

                    // Get all the registrations for this instance
                    paymentRegistrations = new RegistrationService( rockContext )
                        .Queryable( "PersonAlias.Person,Registrants.PersonAlias.Person" ).AsNoTracking()
                        .Where( r =>
                            r.RegistrationInstanceId == instanceId.Value &&
                            !r.IsTemporary )
                        .ToList();

                    // Get the Registration Ids
                    var registrationIds = paymentRegistrations
                        .Select( r => r.Id )
                        .ToList();

                    // Get all the transactions relate to these registrations
                    var qry = new FinancialTransactionService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( t => t.TransactionDetails
                            .Any( d =>
                                d.EntityTypeId.HasValue &&
                                d.EntityTypeId.Value == registrationEntityTypeId &&
                                d.EntityId.HasValue &&
                                registrationIds.Contains( d.EntityId.Value ) ) );

                    // Date Range
                    var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( sdrpPaymentDateRange.DelimitedValues );

                    if ( dateRange.Start.HasValue )
                    {
                        qry = qry.Where( r =>
                            r.TransactionDateTime >= dateRange.Start.Value );
                    }

                    if ( dateRange.End.HasValue )
                    {
                        qry = qry.Where( r =>
                            r.TransactionDateTime < dateRange.End.Value );
                    }

                    SortProperty sortProperty = gPayments.SortProperty;
                    if ( sortProperty != null )
                    {
                        if ( sortProperty.Property == "TotalAmount" )
                        {
                            if ( sortProperty.Direction == SortDirection.Ascending )
                            {
                                qry = qry.OrderBy( t => t.TransactionDetails.Sum( d => (decimal?)d.Amount ) ?? 0.00M );
                            }
                            else
                            {
                                qry = qry.OrderByDescending( t => t.TransactionDetails.Sum( d => (decimal?)d.Amount ) ?? 0.0M );
                            }
                        }
                        else
                        {
                            qry = qry.Sort( sortProperty );
                        }
                    }
                    else
                    {
                        qry = qry.OrderByDescending( t => t.TransactionDateTime ).ThenByDescending( t => t.Id );
                    }

                    gPayments.SetLinqDataSource( qry.AsNoTracking() );
                    gPayments.DataBind();
                }
            }
        }

        #endregion

        #region Fees Tab

        /// <summary>
        /// Binds the fees filter.
        /// </summary>
        private void BindFeesFilter()
        {
            sdrpFeeDateRange.DelimitedValues = fFees.GetUserPreference( "FeeDateRange");
            Populate_ddlFeeName();
            ddlFeeName.SelectedIndex = ddlFeeName.Items.IndexOf(ddlFeeName.Items.FindByText(fFees.GetUserPreference( "FeeName" ) ) );
            Populate_cblFeeOptions();
        }

        /// <summary>
        /// Binds the fees grid.
        /// </summary>
        private void BindFeesGrid()
        {
            int? instanceId = hfRegistrationInstanceId.Value.AsIntegerOrNull();
            if ( instanceId == null || instanceId == 0 )
            {
                return;
            }
            
            RegistrationTemplateFeeService registrationTemplateFeeService = new RegistrationTemplateFeeService( new RockContext() );
            var data = registrationTemplateFeeService.GetRegistrationTemplateFeeReport( (int)instanceId );

            // Add Date Range
            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( sdrpFeeDateRange.DelimitedValues );
            if ( dateRange.Start.HasValue )
            {
                data = data.Where( r => r.RegistrationDate >= dateRange.Start.Value );
            }

            if ( dateRange.End.HasValue )
            {
                data = data.Where( r => r.RegistrationDate < dateRange.End.Value );
            }

            // Fee Name
            if ( ddlFeeName.SelectedIndex > 0 )
            {
                data = data.Where( r => r.FeeName == ddlFeeName.SelectedItem.Text );
            }

            // Fee Options
            if ( cblFeeOptions.SelectedValues.Count > 0 )
            {
                data = data.Where( r => cblFeeOptions.SelectedValues.Any( v => v.Equals( r.FeeItem.Guid.ToString(), StringComparison.OrdinalIgnoreCase) ) );
            }

            SortProperty sortProperty = gFees.SortProperty;
            if ( sortProperty != null )
            {
                data = data.AsQueryable().Sort( sortProperty ).ToList();
            }
            else
            {
                data = data.OrderByDescending( f => f.RegistrationDate ).ToList();
            }

            gFees.DataSource = data;
            gFees.DataBind();
        }

        /// <summary>
        /// Handles the GridRebind event of the gFees control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        protected void gFees_GridRebind( object sender, GridRebindEventArgs e )
        {
            gFees.ExportTitleName = lReadOnlyTitle.Text + " - Registration Fees";
            gFees.ExportFilename = gFees.ExportFilename ?? lReadOnlyTitle.Text + "RegistrationFees";
            BindFeesGrid();
        }

        /// <summary>
        /// Populates ddlFeeName with the name of the DDL fee.
        /// </summary>
        private void Populate_ddlFeeName()
        {
            int? instanceId = hfRegistrationInstanceId.Value.AsIntegerOrNull();
            if ( instanceId == null || instanceId == 0 )
            {
                return;
            }

            var rockContext = new RockContext();
            var registrationInstanceService = new RegistrationInstanceService( rockContext );
            var templateId = registrationInstanceService.Get( ( int ) instanceId ).RegistrationTemplateId;

            var registrationTemplateFeeService = new RegistrationTemplateFeeService( new RockContext() );
            var templateFees = registrationTemplateFeeService.Queryable().Where( f => f.RegistrationTemplateId == templateId ).ToList();

            ddlFeeName.Items.Add( new ListItem() );
            foreach ( var templateFee in templateFees )
            {
                ddlFeeName.Items.Add( new ListItem( templateFee.Name, templateFee.Id.ToString() ) );
            }
        }

        /// <summary>
        /// Populates cblFeeOptions with fee options.
        /// </summary>
        private void Populate_cblFeeOptions()
        {
            cblFeeOptions.Items.Clear();

            int? feeId = ddlFeeName.SelectedValue.AsIntegerOrNull();
            if ( feeId.HasValue )
            {
                var feeItems = new RegistrationTemplateFeeItemService( new RockContext() ).Queryable().Where( a => a.RegistrationTemplateFeeId == feeId );

                foreach ( var feeItem in feeItems )
                {
                    cblFeeOptions.Items.Add( new ListItem( feeItem.Name, feeItem.Guid.ToString() ) );
                }

                string feeOptionValues = fFees.GetUserPreference( "FeeOptions" );
                if ( !string.IsNullOrWhiteSpace( feeOptionValues ) )
                {
                    cblFeeOptions.SetValues( feeOptionValues.Split( ';' ).ToList() );
                }
                
                cblFeeOptions.Visible = true;
            }
        }

        #endregion

        #region Discounts Tab
        
        private void BindDiscountsFilter()
        {
            sdrpDiscountDateRange.DelimitedValues = fDiscounts.GetUserPreference( "DiscountDateRange" );
            Populate_ddlDiscountCode();
            ddlDiscountCode.SelectedIndex = ddlDiscountCode.Items.IndexOf( ddlDiscountCode.Items.FindByText( fDiscounts.GetUserPreference( "DiscountCode" ) ) );
            tbDiscountCodeSearch.Text = fDiscounts.GetUserPreference( "DiscountCodeSearch" );
        }

        private void BindDiscountsGrid()
        {
            int? instanceId = hfRegistrationInstanceId.Value.AsIntegerOrNull();
            if ( instanceId == null || instanceId == 0 )
            {
                return;
            }

            RegistrationTemplateDiscountService registrationTemplateDiscountService = new RegistrationTemplateDiscountService( new RockContext() );
            var data = registrationTemplateDiscountService.GetRegistrationInstanceDiscountCodeReport( ( int ) instanceId );

            // Add Date Range
            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( sdrpDiscountDateRange.DelimitedValues );
            if ( dateRange.Start.HasValue )
            {
                data = data.Where( r => r.RegistrationDate >= dateRange.Start.Value );
            }

            if ( dateRange.End.HasValue )
            {
                data = data.Where( r => r.RegistrationDate < dateRange.End.Value );
            }

            // Discount code, use ddl if one is selected, otherwise try the search box.
            if ( ddlDiscountCode.SelectedIndex > 0 )
            {
                data = data.Where( r => r.DiscountCode == ddlDiscountCode.SelectedItem.Text );
            }
            else if ( tbDiscountCodeSearch.Text.IsNotNullOrWhiteSpace() )
            {
                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex( tbDiscountCodeSearch.Text.ToLower() );
                data = data.Where( r => regex.IsMatch( r.DiscountCode.ToLower()) );
            }

            var results = data.ToList();

            SortProperty sortProperty = gDiscounts.SortProperty;
            if ( sortProperty != null )
            {
                results = results.AsQueryable().Sort( sortProperty ).ToList();
            }
            else
            {
                results = results.OrderByDescending( d => d.RegistrationDate ).ToList();
            }

            
            gDiscounts.DataSource = results;
            gDiscounts.DataBind();

            PopulateTotals( results );
        }

        protected void gDiscounts_GridRebind( object sender, GridRebindEventArgs e)
        {
            gDiscounts.ExportTitleName = lReadOnlyTitle.Text + " - Discount Codes";
            gDiscounts.ExportFilename = gDiscounts.ExportFilename ?? lReadOnlyTitle.Text + "DiscountCodes";
            BindDiscountsGrid();
        }

        private void PopulateTotals( List<TemplateDiscountReport> report )
        {
            lTotalTotalCost.Text = string.Format( GlobalAttributesCache.Value( "CurrencySymbol" ) + "{0:#,##0.00}", report.Sum( r => r.TotalCost ) );
            lTotalDiscountQualifiedCost.Text = string.Format( GlobalAttributesCache.Value( "CurrencySymbol" ) + "{0:#,##0.00}", report.Sum( r => r.DiscountQualifiedCost ) );
            lTotalDiscounts.Text = string.Format( GlobalAttributesCache.Value( "CurrencySymbol" ) + "{0:#,##0.00}", report.Sum( r => r.TotalDiscount ) );
            lTotalRegistrationCost.Text = string.Format( GlobalAttributesCache.Value( "CurrencySymbol" ) + "{0:#,##0.00}", report.Sum( r => r.RegistrationCost ) );
            lTotalRegistrations.Text = report.Count().ToString();
            lTotalRegistrants.Text = report.Sum( r => r.RegistrantCount ).ToString();
        }

        protected void Populate_ddlDiscountCode()
        {
            int? instanceId = hfRegistrationInstanceId.Value.AsIntegerOrNull();
            if ( instanceId == null || instanceId == 0 )
            {
                return;
            }
            
            var discountService = new RegistrationTemplateDiscountService( new RockContext() );
            var discountCodes = discountService.GetDiscountsForRegistrationInstance( instanceId ).AsNoTracking().OrderBy( d => d.Code ).ToList();

            ddlDiscountCode.Items.Clear();
            ddlDiscountCode.Items.Add( new ListItem() );
            foreach ( var discountCode in discountCodes )
            {
                ddlDiscountCode.Items.Add( new ListItem( discountCode.Code, discountCode.Id.ToString() ) );
            }
        }

        #endregion

        #region Wait List Tab

        /// <summary>
        /// Binds the wait list filter.
        /// </summary>
        /// <param name="instance">The instance.</param>
        private void BindWaitListFilter( RegistrationInstance instance )
        {
            drpWaitListDateRange.DelimitedValues = fWaitList.GetUserPreference( "WL-Date Range" );
            tbWaitListFirstName.Text = fWaitList.GetUserPreference( "WL-First Name" );
            tbWaitListLastName.Text = fWaitList.GetUserPreference( "WL-Last Name" );
        }

        /// <summary>
        /// Binds the wait list grid.
        /// </summary>
        /// <param name="isExporting">if set to <c>true</c> [is exporting].</param>
        private void BindWaitListGrid( bool isExporting = false )
        {
            _isExporting = isExporting;
            int? instanceId = hfRegistrationInstanceId.Value.AsIntegerOrNull();
            if ( instanceId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var registrationInstance = new RegistrationInstanceService( rockContext ).Get( instanceId.Value );

                    _waitListOrder = new RegistrationRegistrantService( rockContext ).Queryable().Where( r =>
                                            r.Registration.RegistrationInstanceId == instanceId.Value &&
                                            r.PersonAlias != null &&
                                            r.PersonAlias.Person != null &&
                                            r.OnWaitList )
                                        .OrderBy( r => r.CreatedDateTime )
                                        .Select( r => r.Id ).ToList();

                    // Start query for registrants
                    var registrationRegistrantService = new RegistrationRegistrantService( rockContext );
                    var qry = registrationRegistrantService
                    .Queryable( "PersonAlias.Person.PhoneNumbers.NumberTypeValue,Fees.RegistrationTemplateFee" ).AsNoTracking()
                    .Where( r =>
                        r.Registration.RegistrationInstanceId == instanceId.Value &&
                        r.PersonAlias != null &&
                        r.PersonAlias.Person != null &&
                        r.OnWaitList );

                    // Filter by daterange
                    if ( drpWaitListDateRange.LowerValue.HasValue )
                    {
                        qry = qry.Where( r =>
                            r.CreatedDateTime.HasValue &&
                            r.CreatedDateTime.Value >= drpWaitListDateRange.LowerValue.Value );
                    }

                    if ( drpWaitListDateRange.UpperValue.HasValue )
                    {
                        qry = qry.Where( r =>
                            r.CreatedDateTime.HasValue &&
                            r.CreatedDateTime.Value <= drpWaitListDateRange.UpperValue.Value );
                    }

                    // Filter by first name
                    if ( !string.IsNullOrWhiteSpace( tbWaitListFirstName.Text ) )
                    {
                        string rfname = tbWaitListFirstName.Text;
                        qry = qry.Where( r =>
                            r.PersonAlias.Person.NickName.StartsWith( rfname ) ||
                            r.PersonAlias.Person.FirstName.StartsWith( rfname ) );
                    }

                    // Filter by last name
                    if ( !string.IsNullOrWhiteSpace( tbWaitListLastName.Text ) )
                    {
                        string rlname = tbWaitListLastName.Text;
                        qry = qry.Where( r =>
                            r.PersonAlias.Person.LastName.StartsWith( rlname ) );
                    }

                    var personIds = qry.Select( r => r.PersonAlias.PersonId ).Distinct().ToList();
                    if ( isExporting || RegistrantFields != null && RegistrantFields.Any( f => f.PersonFieldType != null && f.PersonFieldType == RegistrationPersonFieldType.Address ))
                    {
                        _homeAddresses = Person.GetHomeLocations( personIds );
                    }

                    SetPhoneDictionary( rockContext, personIds );

                    bool preloadCampusValues = false;
                    var registrantAttributes = new List<AttributeCache>();
                    var personAttributes = new List<AttributeCache>();
                    var groupMemberAttributes = new List<AttributeCache>();
                    var registrantAttributeIds = new List<int>();
                    var personAttributesIds = new List<int>();
                    var groupMemberAttributesIds = new List<int>();

                    if ( RegistrantFields != null )
                    {
                        // Filter by any selected
                        foreach ( var personFieldType in RegistrantFields
                            .Where( f =>
                                f.FieldSource == RegistrationFieldSource.PersonField &&
                                f.PersonFieldType.HasValue )
                            .Select( f => f.PersonFieldType.Value ) )
                        {
                            switch ( personFieldType )
                            {
                                case RegistrationPersonFieldType.Campus:
                                    preloadCampusValues = true;

                                    var ddlCampus = phWaitListFormFieldFilters.FindControl( "ddlWaitlistCampus" ) as RockDropDownList;
                                    if ( ddlCampus != null )
                                    {
                                        var campusId = ddlCampus.SelectedValue.AsIntegerOrNull();
                                        if ( campusId.HasValue )
                                        {
                                            var familyGroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();
                                            qry = qry.Where( r =>
                                                r.PersonAlias.Person.Members.Any( m =>
                                                    m.Group.GroupType.Guid == familyGroupTypeGuid &&
                                                    m.Group.CampusId.HasValue &&
                                                    m.Group.CampusId.Value == campusId ) );
                                        }
                                    }

                                    break;

                                case RegistrationPersonFieldType.Email:
                                    var tbEmailFilter = phWaitListFormFieldFilters.FindControl( "tbWaitlistEmailFilter" ) as RockTextBox;
                                    if ( tbEmailFilter != null && !string.IsNullOrWhiteSpace( tbEmailFilter.Text ) )
                                    {
                                        qry = qry.Where( r =>
                                            r.PersonAlias.Person.Email != null &&
                                            r.PersonAlias.Person.Email.Contains( tbEmailFilter.Text ) );
                                    }

                                    break;

                                case RegistrationPersonFieldType.Birthdate:
                                    var drpBirthdateFilter = phWaitListFormFieldFilters.FindControl( "drpWaitlistBirthdateFilter" ) as DateRangePicker;
                                    if ( drpBirthdateFilter != null )
                                    {
                                        if ( drpBirthdateFilter.LowerValue.HasValue )
                                        {
                                            qry = qry.Where( r =>
                                                r.PersonAlias.Person.BirthDate.HasValue &&
                                                r.PersonAlias.Person.BirthDate.Value >= drpBirthdateFilter.LowerValue.Value );
                                        }

                                        if ( drpBirthdateFilter.UpperValue.HasValue )
                                        {
                                            qry = qry.Where( r =>
                                                r.PersonAlias.Person.BirthDate.HasValue &&
                                                r.PersonAlias.Person.BirthDate.Value <= drpBirthdateFilter.UpperValue.Value );
                                        }
                                    }

                                    break;
                                case RegistrationPersonFieldType.MiddleName:
                                    var tbWaitlistMiddleNameFilter = phWaitListFormFieldFilters.FindControl( "tbWaitlistMiddleNameFilter" ) as RockTextBox;
                                    if ( tbWaitlistMiddleNameFilter != null && !string.IsNullOrWhiteSpace( tbWaitlistMiddleNameFilter.Text ) )
                                    {
                                        qry = qry.Where( r =>
                                            r.PersonAlias.Person.MiddleName != null &&
                                            r.PersonAlias.Person.MiddleName.Contains( tbWaitlistMiddleNameFilter.Text ) );
                                    }

                                    break;

                                case RegistrationPersonFieldType.AnniversaryDate:
                                    var drpWaitlistAnniversaryDateFilter = phWaitListFormFieldFilters.FindControl( "drpWaitlistAnniversaryDateFilter" ) as DateRangePicker;
                                    if ( drpWaitlistAnniversaryDateFilter != null )
                                    {
                                        if ( drpWaitlistAnniversaryDateFilter.LowerValue.HasValue )
                                        {
                                            qry = qry.Where( r =>
                                                r.PersonAlias.Person.AnniversaryDate.HasValue &&
                                                r.PersonAlias.Person.AnniversaryDate.Value >= drpWaitlistAnniversaryDateFilter.LowerValue.Value );
                                        }

                                        if ( drpWaitlistAnniversaryDateFilter.UpperValue.HasValue )
                                        {
                                            qry = qry.Where( r =>
                                                r.PersonAlias.Person.AnniversaryDate.HasValue &&
                                                r.PersonAlias.Person.AnniversaryDate.Value <= drpWaitlistAnniversaryDateFilter.UpperValue.Value );
                                        }
                                    }

                                    break;
                                case RegistrationPersonFieldType.Grade:
                                    var gpGradeFilter = phWaitListFormFieldFilters.FindControl( "gpWaitlistGradeFilter" ) as GradePicker;
                                    if ( gpGradeFilter != null )
                                    {
                                        int? graduationYear = Person.GraduationYearFromGradeOffset( gpGradeFilter.SelectedValueAsInt( false ) );
                                        if ( graduationYear.HasValue )
                                        {
                                            qry = qry.Where( r =>
                                                r.PersonAlias.Person.GraduationYear.HasValue &&
                                                r.PersonAlias.Person.GraduationYear == graduationYear.Value );
                                        }
                                    }

                                    break;

                                case RegistrationPersonFieldType.Gender:
                                    var ddlGenderFilter = phWaitListFormFieldFilters.FindControl( "ddlWaitlistGenderFilter" ) as RockDropDownList;
                                    if ( ddlGenderFilter != null )
                                    {
                                        var gender = ddlGenderFilter.SelectedValue.ConvertToEnumOrNull<Gender>();
                                        if ( gender.HasValue )
                                        {
                                            qry = qry.Where( r =>
                                                r.PersonAlias.Person.Gender == gender );
                                        }
                                    }

                                    break;

                                case RegistrationPersonFieldType.MaritalStatus:
                                    var dvpMaritalStatusFilter = phWaitListFormFieldFilters.FindControl( "dvpWaitlistMaritalStatusFilter" ) as DefinedValuePicker;
                                    if ( dvpMaritalStatusFilter != null )
                                    {
                                        var maritalStatusId = dvpMaritalStatusFilter.SelectedValue.AsIntegerOrNull();
                                        if ( maritalStatusId.HasValue )
                                        {
                                            qry = qry.Where( r =>
                                                r.PersonAlias.Person.MaritalStatusValueId.HasValue &&
                                                r.PersonAlias.Person.MaritalStatusValueId.Value == maritalStatusId.Value );
                                        }
                                    }

                                    break;
                                   
                                case RegistrationPersonFieldType.MobilePhone:
                                    var tbMobilePhoneFilter = phWaitListFormFieldFilters.FindControl( "tbWaitlistMobilePhoneFilter" ) as RockTextBox;
                                    if ( tbMobilePhoneFilter != null && !string.IsNullOrWhiteSpace( tbMobilePhoneFilter.Text ) )
                                    {
                                        string numericPhone = tbMobilePhoneFilter.Text.AsNumeric();

                                        if ( !string.IsNullOrEmpty( numericPhone ) )
                                        {
                                            var phoneNumberPersonIdQry = new PhoneNumberService( rockContext )
                                                .Queryable()
                                                .Where( a => a.Number.Contains( numericPhone ) )
                                                .Select( a => a.PersonId );

                                            qry = qry.Where( r => phoneNumberPersonIdQry.Contains( r.PersonAlias.PersonId ) );
                                        }
                                    }

                                    break;

                                case RegistrationPersonFieldType.HomePhone:
                                    var tbWaitlistHomePhoneFilter = phWaitListFormFieldFilters.FindControl( "tbWaitlistHomePhoneFilter" ) as RockTextBox;
                                    if ( tbWaitlistHomePhoneFilter != null && !string.IsNullOrWhiteSpace( tbWaitlistHomePhoneFilter.Text ) )
                                    {
                                        string numericPhone = tbWaitlistHomePhoneFilter.Text.AsNumeric();

                                        if ( !string.IsNullOrEmpty( numericPhone ) )
                                        {
                                            var phoneNumberPersonIdQry = new PhoneNumberService( rockContext )
                                                .Queryable()
                                                .Where( a => a.Number.Contains( numericPhone ) )
                                                .Select( a => a.PersonId );

                                            qry = qry.Where( r => phoneNumberPersonIdQry.Contains( r.PersonAlias.PersonId ) );
                                        }
                                    }

                                    break;
                            }
                        }

                        // Get all the registrant attributes selected to be on grid
                        registrantAttributes = RegistrantFields
                            .Where( f =>
                                f.Attribute != null &&
                                f.FieldSource == RegistrationFieldSource.RegistrantAttribute )
                            .Select( f => f.Attribute )
                            .ToList();
                        registrantAttributeIds = registrantAttributes.Select( a => a.Id ).Distinct().ToList();

                        // Filter query by any configured registrant attribute filters
                        if ( registrantAttributes != null && registrantAttributes.Any() )
                        {
                            foreach ( var attribute in registrantAttributes )
                            {
                                var filterControl = phWaitListFormFieldFilters.FindControl( "filterWaitlist_" + attribute.Id.ToString() );
                                qry = attribute.FieldType.Field.ApplyAttributeQueryFilter( qry, filterControl, attribute, registrationRegistrantService, Rock.Reporting.FilterMode.SimpleFilter );
                            }
                        }

                        // Get all the person attributes selected to be on grid
                        personAttributes = RegistrantFields
                            .Where( f =>
                                f.Attribute != null &&
                                f.FieldSource == RegistrationFieldSource.PersonAttribute )
                            .Select( f => f.Attribute )
                            .ToList();
                        personAttributesIds = personAttributes.Select( a => a.Id ).Distinct().ToList();

                        // Filter query by any configured person attribute filters
                        if ( personAttributes != null && personAttributes.Any() )
                        {
                            PersonService personService = new PersonService( rockContext );
                            var personQry = personService.Queryable().AsNoTracking();

                            foreach ( var attribute in personAttributes )
                            {
                                var filterControl = phWaitListFormFieldFilters.FindControl( "filterWaitlist_" + attribute.Id.ToString() );
                                personQry = attribute.FieldType.Field.ApplyAttributeQueryFilter( personQry, filterControl, attribute, personService, Rock.Reporting.FilterMode.SimpleFilter );
                            }

                            qry = qry.Where( r => personQry.Any( p => p.Id == r.PersonAlias.PersonId ) );
                        }

                        // Get all the group member attributes selected to be on grid
                        groupMemberAttributes = RegistrantFields
                            .Where( f =>
                                f.Attribute != null &&
                                f.FieldSource == RegistrationFieldSource.GroupMemberAttribute )
                            .Select( f => f.Attribute )
                            .ToList();
                        groupMemberAttributesIds = groupMemberAttributes.Select( a => a.Id ).Distinct().ToList();

                        // Filter query by any configured person attribute filters
                        if ( groupMemberAttributes != null && groupMemberAttributes.Any() )
                        {
                            var groupMemberService = new GroupMemberService( rockContext );
                            var groupMemberQry = groupMemberService.Queryable().AsNoTracking();

                            foreach ( var attribute in groupMemberAttributes )
                            {
                                var filterControl = phWaitListFormFieldFilters.FindControl( "filterWaitlist_" + attribute.Id.ToString() );
                                groupMemberQry = attribute.FieldType.Field.ApplyAttributeQueryFilter( groupMemberQry, filterControl, attribute, groupMemberService, Rock.Reporting.FilterMode.SimpleFilter );
                            }

                            qry = qry.Where( r => groupMemberQry.Any( g => g.Id == r.GroupMemberId ) );
                        }
                    }

                    // Sort the query
                    IOrderedQueryable<RegistrationRegistrant> orderedQry = null;
                    SortProperty sortProperty = gWaitList.SortProperty;
                    if ( sortProperty != null )
                    {
                        orderedQry = qry.Sort( sortProperty );
                    }
                    else
                    {
                        orderedQry = qry
                            .OrderBy( r => r.Id );
                    }

                    // increase the timeout just in case. A complex filter on the grid might slow things down
                    rockContext.Database.CommandTimeout = 180;

                    // Set the grids LinqDataSource which will run query and set results for current page
                    gWaitList.SetLinqDataSource<RegistrationRegistrant>( orderedQry );

                    if ( RegistrantFields != null )
                    {
                        // Get the query results for the current page
                        var currentPageRegistrants = gWaitList.DataSource as List<RegistrationRegistrant>;
                        if ( currentPageRegistrants != null )
                        {
                            // Get all the registrant ids in current page of query results
                            var registrantIds = currentPageRegistrants
                                .Select( r => r.Id )
                                .Distinct()
                                .ToList();

                            // Get all the person ids in current page of query results
                            var currentPagePersonIds = currentPageRegistrants
                                .Select( r => r.PersonAlias.PersonId )
                                .Distinct()
                                .ToList();

                            // Get all the group member ids and the group id in current page of query results
                            var groupMemberIds = new List<int>();
                            GroupLinks = new Dictionary<int, string>();
                            foreach ( var groupMember in currentPageRegistrants
                                .Where( m =>
                                    m.GroupMember != null &&
                                    m.GroupMember.Group != null )
                                .Select( m => m.GroupMember ) )
                            {
                                groupMemberIds.Add( groupMember.Id );
                                string linkedPageUrl = LinkedPageUrl( "GroupDetailPage", new Dictionary<string, string> { { "GroupId", groupMember.GroupId.ToString() } } );
                                GroupLinks.AddOrIgnore( groupMember.GroupId, isExporting ? groupMember.Group.Name : string.Format( "<a href='{0}'>{1}</a>", linkedPageUrl, groupMember.Group.Name ) );
                            }

                            // If the campus column was selected to be displayed on grid, preload all the people's
                            // campuses so that the databind does not need to query each row
                            if ( preloadCampusValues )
                            {
                                PersonCampusIds = new Dictionary<int, List<int>>();

                                Guid familyGroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();
                                foreach ( var personCampusList in new GroupMemberService( rockContext )
                                    .Queryable().AsNoTracking()
                                    .Where( m =>
                                        m.Group.GroupType.Guid == familyGroupTypeGuid &&
                                        currentPagePersonIds.Contains( m.PersonId ) )
                                    .GroupBy( m => m.PersonId )
                                    .Select( m => new
                                    {
                                        PersonId = m.Key,
                                        CampusIds = m
                                            .Where( g => g.Group.CampusId.HasValue )
                                            .Select( g => g.Group.CampusId.Value )
                                            .ToList()
                                    } ) )
                                {
                                    PersonCampusIds.Add( personCampusList.PersonId, personCampusList.CampusIds );
                                }
                            }

                            // If there are any attributes that were selected to be displayed, we're going
                            // to try and read all attribute values in one query and then put them into a
                            // custom grid ObjectList property so that the AttributeField columns don't need
                            // to do the LoadAttributes and querying of values for each row/column
                            if ( personAttributesIds.Any() || groupMemberAttributesIds.Any() || registrantAttributeIds.Any() )
                            {
                                // Query the attribute values for all rows and attributes
                                var attributeValues = new AttributeValueService( rockContext )
                                    .Queryable( "Attribute" ).AsNoTracking()
                                    .Where( v =>
                                        v.EntityId.HasValue &&
                                        (
                                            (
                                                personAttributesIds.Contains( v.AttributeId ) &&
                                                currentPagePersonIds.Contains( v.EntityId.Value )
                                            ) ||
                                            (
                                                groupMemberAttributesIds.Contains( v.AttributeId ) &&
                                                groupMemberIds.Contains( v.EntityId.Value )
                                            ) ||
                                            (
                                                registrantAttributeIds.Contains( v.AttributeId ) &&
                                                registrantIds.Contains( v.EntityId.Value )
                                            )
                                        ) ).ToList();

                                // Get the attributes to add to each row's object
                                var attributes = new Dictionary<string, AttributeCache>();
                                RegistrantFields
                                        .Where( f => f.Attribute != null )
                                        .Select( f => f.Attribute )
                                        .ToList()
                                    .ForEach( a => attributes
                                        .Add( a.Id.ToString() + a.Key, a ) );

                                // Initialize the grid's object list
                                gWaitList.ObjectList = new Dictionary<string, object>();

                                // Loop through each of the current page's registrants and build an attribute
                                // field object for storing attributes and the values for each of the registrants
                                foreach ( var registrant in currentPageRegistrants )
                                {
                                    // Create a row attribute object
                                    var attributeFieldObject = new AttributeFieldObject();

                                    // Add the attributes to the attribute object
                                    attributeFieldObject.Attributes = attributes;

                                    // Add any person attribute values to object
                                    attributeValues
                                        .Where( v =>
                                            personAttributesIds.Contains( v.AttributeId ) &&
                                            v.EntityId.Value == registrant.PersonAlias.PersonId )
                                        .ToList()
                                        .ForEach( v => attributeFieldObject.AttributeValues
                                            .Add( v.AttributeId.ToString() + v.Attribute.Key, new AttributeValueCache( v ) ) );

                                    // Add any group member attribute values to object
                                    if ( registrant.GroupMemberId.HasValue )
                                    {
                                        attributeValues
                                            .Where( v =>
                                                groupMemberAttributesIds.Contains( v.AttributeId ) &&
                                                v.EntityId.Value == registrant.GroupMemberId.Value )
                                            .ToList()
                                            .ForEach( v => attributeFieldObject.AttributeValues
                                                .Add( v.AttributeId.ToString() + v.Attribute.Key, new AttributeValueCache( v ) ) );
                                    }

                                    // Add any registrant attribute values to object
                                    attributeValues
                                        .Where( v =>
                                            registrantAttributeIds.Contains( v.AttributeId ) &&
                                            v.EntityId.Value == registrant.Id )
                                        .ToList()
                                        .ForEach( v => attributeFieldObject.AttributeValues
                                            .Add( v.AttributeId.ToString() + v.Attribute.Key, new AttributeValueCache( v ) ) );

                                    // Add row attribute object to grid's object list
                                    gWaitList.ObjectList.Add( registrant.Id.ToString(), attributeFieldObject );
                                }
                            }
                        }
                    }

                    gWaitList.DataBind();
                }
            }
        }

        #endregion

        #region Linkages Tab

            /// <summary>
            /// Binds the registrations filter.
            /// </summary>
        private void BindLinkagesFilter()
        {
            cblCampus.DataSource = CampusCache.All();
            cblCampus.DataBind();
            string campusValue = fLinkages.GetUserPreference( "Campus" );
            if ( !string.IsNullOrWhiteSpace( campusValue ) )
            {
                cblCampus.SetValues( campusValue.Split( ';' ).ToList() );
            }
        }

        /// <summary>
        /// Binds the registrations grid.
        /// </summary>
        private void BindLinkagesGrid()
        {
            int? instanceId = hfRegistrationInstanceId.Value.AsIntegerOrNull();
            if ( instanceId.HasValue )
            {
                var groupCol = gLinkages.Columns[2] as HyperLinkField;
                groupCol.DataNavigateUrlFormatString = LinkedPageUrl( "GroupDetailPage" ) + "?GroupID={0}";

                using ( var rockContext = new RockContext() )
                {
                    var qry = new EventItemOccurrenceGroupMapService( rockContext )
                        .Queryable( "EventItemOccurrence.EventItem.EventCalendarItems.EventCalendar,EventItemOccurrence.ContentChannelItems.ContentChannelItem,Group" )
                        .AsNoTracking()
                        .Where( r => r.RegistrationInstanceId == instanceId.Value );

                    List<int> campusIds = cblCampus.SelectedValuesAsInt;
                    if ( campusIds.Any() )
                    {
                        qry = qry
                            .Where( l =>
                                l.EventItemOccurrence != null &&
                                (
                                    !l.EventItemOccurrence.CampusId.HasValue ||
                                    campusIds.Contains( l.EventItemOccurrence.CampusId.Value )
                                ) );
                    }

                    IOrderedQueryable<EventItemOccurrenceGroupMap> orderedQry = null;
                    SortProperty sortProperty = gLinkages.SortProperty;
                    if ( sortProperty != null )
                    {
                        orderedQry = qry.Sort( sortProperty );
                    }
                    else
                    {
                        orderedQry = qry.OrderByDescending( r => r.CreatedDateTime );
                    }

                    gLinkages.SetLinqDataSource( orderedQry );
                    gLinkages.DataBind();
                }
            }
        }

        #endregion

        #region Group Placement Tab

        /// <summary>
        /// Binds the group placement grid.
        /// </summary>
        /// <param name="isExporting">if set to <c>true</c> [is exporting].</param>
        private void BindGroupPlacementGrid( bool isExporting = false )
        {
            _isExporting = isExporting;
            int? parentGroupId = gpGroupPlacementParentGroup.SelectedValueAsInt();
            int? instanceId = hfRegistrationInstanceId.Value.AsIntegerOrNull();
            if ( instanceId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    // Start query for registrants
                    var registrationRegistrantService = new RegistrationRegistrantService( rockContext );
                    var qry = registrationRegistrantService
                        .Queryable( "PersonAlias.Person.PhoneNumbers.NumberTypeValue,Fees.RegistrationTemplateFee,GroupMember.Group" ).AsNoTracking()
                        .Where( r =>
                            r.Registration.RegistrationInstanceId == instanceId.Value &&
                            r.PersonAlias != null &&
                            r.OnWaitList == false &&
                            r.PersonAlias.Person != null );

                    if ( parentGroupId.HasValue )
                    {
                        var validGroupIds = new GroupService( rockContext ).GetAllDescendents( parentGroupId.Value )
                            .Select( g => g.Id )
                            .ToList();

                        var existingPeopleInGroups = new GroupMemberService( rockContext )
                            .Queryable().AsNoTracking()
                            .Where( m => validGroupIds.Contains( m.GroupId ) && m.Group.IsActive && m.GroupMemberStatus == GroupMemberStatus.Active )
                            .Select( m => m.PersonId )
                            .ToList();

                        qry = qry.Where( r => !existingPeopleInGroups.Contains( r.PersonAlias.PersonId ) );
                    }

                    // Filter by daterange
                    var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( sdrpRegistrationDateRange.DelimitedValues );

                    if ( dateRange.Start.HasValue )
                    {
                        qry = qry.Where( r =>
                            r.CreatedDateTime.HasValue &&
                            r.CreatedDateTime.Value >= dateRange.Start.Value );
                    }

                    if ( dateRange.End.HasValue )
                    {
                        qry = qry.Where( r =>
                            r.CreatedDateTime.HasValue &&
                            r.CreatedDateTime.Value < dateRange.End.Value );
                    }

                    // Filter by first name
                    if ( !string.IsNullOrWhiteSpace( tbGroupPlacementsFirstName.Text ) )
                    {
                        string rfname = tbGroupPlacementsFirstName.Text;
                        qry = qry.Where( r =>
                            r.PersonAlias.Person.NickName.StartsWith( rfname ) ||
                            r.PersonAlias.Person.FirstName.StartsWith( rfname ) );
                    }

                    // Filter by last name
                    if ( !string.IsNullOrWhiteSpace( tbGroupPlacementsLastName.Text ) )
                    {
                        string rlname = tbGroupPlacementsLastName.Text;
                        qry = qry.Where( r =>
                            r.PersonAlias.Person.LastName.StartsWith( rlname ) );
                    }

                    var personIds = qry.Select( r => r.PersonAlias.PersonId ).Distinct().ToList();
                    if ( isExporting || RegistrantFields != null && RegistrantFields.Any( f => f.PersonFieldType == RegistrationPersonFieldType.Address ) )
                    {
                        _homeAddresses = Person.GetHomeLocations( personIds );
                    }
                    SetPhoneDictionary( rockContext, personIds );

                    bool preloadCampusValues = false;
                    var registrantAttributes = new List<AttributeCache>();
                    var personAttributes = new List<AttributeCache>();
                    var groupMemberAttributes = new List<AttributeCache>();
                    var registrantAttributeIds = new List<int>();
                    var personAttributesIds = new List<int>();
                    var groupMemberAttributesIds = new List<int>();

                    if ( RegistrantFields != null )
                    {
                        // Filter by any selected
                        foreach ( var personFieldType in RegistrantFields
                            .Where( f =>
                                f.FieldSource == RegistrationFieldSource.PersonField &&
                                f.PersonFieldType.HasValue )
                            .Select( f => f.PersonFieldType.Value ) )
                        {
                            switch ( personFieldType )
                            {
                                case RegistrationPersonFieldType.Campus:
                                    preloadCampusValues = true;

                                    var ddlCampus = phGroupPlacementsFormFieldFilters.FindControl( "ddlGroupPlacementsCampus" ) as RockDropDownList;
                                    if ( ddlCampus != null )
                                    {
                                        var campusId = ddlCampus.SelectedValue.AsIntegerOrNull();
                                        if ( campusId.HasValue )
                                        {
                                            var familyGroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();
                                            qry = qry.Where( r =>
                                                r.PersonAlias.Person.Members.Any( m =>
                                                    m.Group.GroupType.Guid == familyGroupTypeGuid &&
                                                    m.Group.CampusId.HasValue &&
                                                    m.Group.CampusId.Value == campusId ) );
                                        }
                                    }

                                    break;

                                case RegistrationPersonFieldType.Email:
                                    var tbEmailFilter = phGroupPlacementsFormFieldFilters.FindControl( "tbGroupPlacementsEmailFilter" ) as RockTextBox;
                                    if ( tbEmailFilter != null && !string.IsNullOrWhiteSpace( tbEmailFilter.Text ) )
                                    {
                                        qry = qry.Where( r =>
                                            r.PersonAlias.Person.Email != null &&
                                            r.PersonAlias.Person.Email.Contains( tbEmailFilter.Text ) );
                                    }

                                    break;

                                case RegistrationPersonFieldType.Birthdate:
                                    var drpBirthdateFilter = phGroupPlacementsFormFieldFilters.FindControl( "drpGroupPlacementsBirthdateFilter" ) as DateRangePicker;
                                    if ( drpBirthdateFilter != null )
                                    {
                                        if ( drpBirthdateFilter.LowerValue.HasValue )
                                        {
                                            qry = qry.Where( r =>
                                                r.PersonAlias.Person.BirthDate.HasValue &&
                                                r.PersonAlias.Person.BirthDate.Value >= drpBirthdateFilter.LowerValue.Value );
                                        }

                                        if ( drpBirthdateFilter.UpperValue.HasValue )
                                        {
                                            qry = qry.Where( r =>
                                                r.PersonAlias.Person.BirthDate.HasValue &&
                                                r.PersonAlias.Person.BirthDate.Value <= drpBirthdateFilter.UpperValue.Value );
                                        }
                                    }

                                    break;
                                case RegistrationPersonFieldType.MiddleName:
                                    var tbGroupPlacementsMiddleNameFilter = phGroupPlacementsFormFieldFilters.FindControl( "tbGroupPlacementsMiddleNameFilter" ) as RockTextBox;
                                    if ( tbGroupPlacementsMiddleNameFilter != null && !string.IsNullOrWhiteSpace( tbGroupPlacementsMiddleNameFilter.Text ) )
                                    {
                                        qry = qry.Where( r =>
                                            r.PersonAlias.Person.MiddleName != null &&
                                            r.PersonAlias.Person.MiddleName.Contains( tbGroupPlacementsMiddleNameFilter.Text ) );
                                    }

                                    break;

                                case RegistrationPersonFieldType.AnniversaryDate:
                                    var drpGroupPlacementsAnniversaryDateFilter = phGroupPlacementsFormFieldFilters.FindControl( "drpGroupPlacementsAnniversaryDateFilter" ) as DateRangePicker;
                                    if ( drpGroupPlacementsAnniversaryDateFilter != null )
                                    {
                                        if ( drpGroupPlacementsAnniversaryDateFilter.LowerValue.HasValue )
                                        {
                                            qry = qry.Where( r =>
                                                r.PersonAlias.Person.AnniversaryDate.HasValue &&
                                                r.PersonAlias.Person.AnniversaryDate.Value >= drpGroupPlacementsAnniversaryDateFilter.LowerValue.Value );
                                        }

                                        if ( drpGroupPlacementsAnniversaryDateFilter.UpperValue.HasValue )
                                        {
                                            qry = qry.Where( r =>
                                                r.PersonAlias.Person.AnniversaryDate.HasValue &&
                                                r.PersonAlias.Person.AnniversaryDate.Value <= drpGroupPlacementsAnniversaryDateFilter.UpperValue.Value );
                                        }
                                    }

                                    break;
                                case RegistrationPersonFieldType.Grade:
                                    var gpGradeFilter = phGroupPlacementsFormFieldFilters.FindControl( "gpGroupPlacementsGradeFilter" ) as GradePicker;
                                    if ( gpGradeFilter != null )
                                    {
                                        int? graduationYear = Person.GraduationYearFromGradeOffset( gpGradeFilter.SelectedValueAsInt( false ) );
                                        if ( graduationYear.HasValue )
                                        {
                                            qry = qry.Where( r =>
                                                r.PersonAlias.Person.GraduationYear.HasValue &&
                                                r.PersonAlias.Person.GraduationYear == graduationYear.Value );
                                        }
                                    }

                                    break;

                                case RegistrationPersonFieldType.Gender:
                                    var ddlGenderFilter = phGroupPlacementsFormFieldFilters.FindControl( "ddlGroupPlacementsGenderFilter" ) as RockDropDownList;
                                    if ( ddlGenderFilter != null )
                                    {
                                        var gender = ddlGenderFilter.SelectedValue.ConvertToEnumOrNull<Gender>();
                                        if ( gender.HasValue )
                                        {
                                            qry = qry.Where( r =>
                                                r.PersonAlias.Person.Gender == gender );
                                        }
                                    }

                                    break;

                                case RegistrationPersonFieldType.MaritalStatus:
                                    var dvpMaritalStatusFilter = phGroupPlacementsFormFieldFilters.FindControl( "dvpGroupPlacementsMaritalStatusFilter" ) as DefinedValuePicker;
                                    if ( dvpMaritalStatusFilter != null )
                                    {
                                        var maritalStatusId = dvpMaritalStatusFilter.SelectedValueAsId();
                                        if ( maritalStatusId.HasValue )
                                        {
                                            qry = qry.Where( r =>
                                                r.PersonAlias.Person.MaritalStatusValueId.HasValue &&
                                                r.PersonAlias.Person.MaritalStatusValueId.Value == maritalStatusId.Value );
                                        }
                                    }

                                    break;
                                    
                                case RegistrationPersonFieldType.MobilePhone:
                                    var tbMobilePhoneFilter = phGroupPlacementsFormFieldFilters.FindControl( "tbGroupPlacementsMobilePhoneFilter" ) as RockTextBox;
                                    if ( tbMobilePhoneFilter != null && !string.IsNullOrWhiteSpace( tbMobilePhoneFilter.Text ) )
                                    {
                                        string numericPhone = tbMobilePhoneFilter.Text.AsNumeric();

                                        if ( !string.IsNullOrEmpty( numericPhone ) )
                                        {
                                            var phoneNumberPersonIdQry = new PhoneNumberService( rockContext )
                                                .Queryable()
                                                .Where( a => a.Number.Contains( numericPhone ) )
                                                .Select( a => a.PersonId );

                                            qry = qry.Where( r => phoneNumberPersonIdQry.Contains( r.PersonAlias.PersonId ) );
                                        }
                                    }

                                    break;

                                case RegistrationPersonFieldType.HomePhone:
                                    var tbGroupPlacementsHomePhoneFilter = phGroupPlacementsFormFieldFilters.FindControl( "tbGroupPlacementsHomePhoneFilter" ) as RockTextBox;
                                    if ( tbGroupPlacementsHomePhoneFilter != null && !string.IsNullOrWhiteSpace( tbGroupPlacementsHomePhoneFilter.Text ) )
                                    {
                                        string numericPhone = tbGroupPlacementsHomePhoneFilter.Text.AsNumeric();

                                        if ( !string.IsNullOrEmpty( numericPhone ) )
                                        {
                                            var phoneNumberPersonIdQry = new PhoneNumberService( rockContext )
                                                .Queryable()
                                                .Where( a => a.Number.Contains( numericPhone ) )
                                                .Select( a => a.PersonId );

                                            qry = qry.Where( r => phoneNumberPersonIdQry.Contains( r.PersonAlias.PersonId ) );
                                        }
                                    }

                                    break;
                            }
                        }

                        // Get all the registrant attributes selected to be on grid
                        registrantAttributes = RegistrantFields
                            .Where( f =>
                                f.Attribute != null &&
                                f.FieldSource == RegistrationFieldSource.RegistrantAttribute )
                            .Select( f => f.Attribute )
                            .ToList();
                        registrantAttributeIds = registrantAttributes.Select( a => a.Id ).Distinct().ToList();

                        // Filter query by any configured registrant attribute filters
                        if ( registrantAttributes != null && registrantAttributes.Any() )
                        {
                            foreach ( var attribute in registrantAttributes )
                            {
                                var filterControl = phGroupPlacementsFormFieldFilters.FindControl( "filterGroupPlacements_" + attribute.Id.ToString() );
                                qry = attribute.FieldType.Field.ApplyAttributeQueryFilter( qry, filterControl, attribute, registrationRegistrantService, Rock.Reporting.FilterMode.SimpleFilter );
                            }
                        }

                        // Get all the person attributes selected to be on grid
                        personAttributes = RegistrantFields
                            .Where( f =>
                                f.Attribute != null &&
                                f.FieldSource == RegistrationFieldSource.PersonAttribute )
                            .Select( f => f.Attribute )
                            .ToList();
                        personAttributesIds = personAttributes.Select( a => a.Id ).Distinct().ToList();

                        // Filter query by any configured person attribute filters
                        if ( personAttributes != null && personAttributes.Any() )
                        {
                            PersonService personService = new PersonService( rockContext );
                            var personQry = personService.Queryable().AsNoTracking();

                            foreach ( var attribute in personAttributes )
                            {
                                var filterControl = phGroupPlacementsFormFieldFilters.FindControl( "filterGroupPlacements_" + attribute.Id.ToString() );
                                personQry = attribute.FieldType.Field.ApplyAttributeQueryFilter( personQry, filterControl, attribute, personService, Rock.Reporting.FilterMode.SimpleFilter );
                            }

                            qry = qry.Where( r => personQry.Any( p => p.Id == r.PersonAlias.PersonId ) );
                        }

                        // Get all the group member attributes selected to be on grid
                        groupMemberAttributes = RegistrantFields
                            .Where( f =>
                                f.Attribute != null &&
                                f.FieldSource == RegistrationFieldSource.GroupMemberAttribute )
                            .Select( f => f.Attribute )
                            .ToList();
                        groupMemberAttributesIds = groupMemberAttributes.Select( a => a.Id ).Distinct().ToList();

                        // Filter query by any configured person attribute filters
                        if ( groupMemberAttributes != null && groupMemberAttributes.Any() )
                        {
                            var groupMemberService = new GroupMemberService( rockContext );
                            var groupMemberQry = groupMemberService.Queryable().AsNoTracking();

                            foreach ( var attribute in groupMemberAttributes )
                            {
                                var filterControl = phGroupPlacementsFormFieldFilters.FindControl( "filterGroupPlacements_" + attribute.Id.ToString() );
                                groupMemberQry = attribute.FieldType.Field.ApplyAttributeQueryFilter( groupMemberQry, filterControl, attribute, groupMemberService, Rock.Reporting.FilterMode.SimpleFilter );
                            }

                            qry = qry.Where( r => groupMemberQry.Any( g => g.Id == r.GroupMemberId ) );
                        }
                    }

                    // Sort the query
                    IOrderedQueryable<RegistrationRegistrant> orderedQry = null;
                    SortProperty sortProperty = gGroupPlacements.SortProperty;
                    if ( sortProperty != null )
                    {
                        orderedQry = qry.Sort( sortProperty );
                    }
                    else
                    {
                        orderedQry = qry
                            .OrderBy( r => r.PersonAlias.Person.LastName )
                            .ThenBy( r => r.PersonAlias.Person.NickName );
                    }

                    // Set the grids LinqDataSource which will run query and set results for current page
                    gGroupPlacements.SetLinqDataSource<RegistrationRegistrant>( orderedQry );

                    if ( RegistrantFields != null )
                    {
                        // Get the query results for the current page
                        var currentPageRegistrants = gGroupPlacements.DataSource as List<RegistrationRegistrant>;
                        if ( currentPageRegistrants != null )
                        {
                            // Get all the registrant ids in current page of query results
                            var registrantIds = currentPageRegistrants
                                .Select( r => r.Id )
                                .Distinct()
                                .ToList();

                            // Get all the person ids in current page of query results
                            var currentPagePersonIds = currentPageRegistrants
                                .Select( r => r.PersonAlias.PersonId )
                                .Distinct()
                                .ToList();

                            // Get all the group member ids and the group id in current page of query results
                            var groupMemberIds = new List<int>();
                            GroupLinks = new Dictionary<int, string>();
                            foreach ( var groupMember in currentPageRegistrants
                                .Where( m =>
                                    m.GroupMember != null &&
                                    m.GroupMember.Group != null )
                                .Select( m => m.GroupMember ) )
                            {
                                groupMemberIds.Add( groupMember.Id );
                                string linkedPageUrl = LinkedPageUrl( "GroupDetailPage", new Dictionary<string, string> { { "GroupId", groupMember.GroupId.ToString() } } );
                                GroupLinks.AddOrIgnore( groupMember.GroupId, isExporting ? groupMember.Group.Name : string.Format( "<a href='{0}'>{1}</a>", linkedPageUrl, groupMember.Group.Name ) );
                            }

                            // If the campus column was selected to be displayed on grid, preload all the people's
                            // campuses so that the databind does not need to query each row
                            if ( preloadCampusValues )
                            {
                                PersonCampusIds = new Dictionary<int, List<int>>();

                                Guid familyGroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();
                                foreach ( var personCampusList in new GroupMemberService( rockContext )
                                    .Queryable().AsNoTracking()
                                    .Where( m =>
                                        m.Group.GroupType.Guid == familyGroupTypeGuid &&
                                        currentPagePersonIds.Contains( m.PersonId ) )
                                    .GroupBy( m => m.PersonId )
                                    .Select( m => new
                                    {
                                        PersonId = m.Key,
                                        CampusIds = m
                                            .Where( g => g.Group.CampusId.HasValue )
                                            .Select( g => g.Group.CampusId.Value )
                                            .ToList()
                                    } ) )
                                {
                                    PersonCampusIds.Add( personCampusList.PersonId, personCampusList.CampusIds );
                                }
                            }

                            // If there are any attributes that were selected to be displayed, we're going
                            // to try and read all attribute values in one query and then put them into a
                            // custom grid ObjectList property so that the AttributeField columns don't need
                            // to do the LoadAttributes and querying of values for each row/column
                            if ( personAttributesIds.Any() || groupMemberAttributesIds.Any() || registrantAttributeIds.Any() )
                            {
                                // Query the attribute values for all rows and attributes
                                var attributeValues = new AttributeValueService( rockContext )
                                    .Queryable( "Attribute" ).AsNoTracking()
                                    .Where( v =>
                                        v.EntityId.HasValue &&
                                        (
                                            (
                                                personAttributesIds.Contains( v.AttributeId ) &&
                                                currentPagePersonIds.Contains( v.EntityId.Value )
                                            ) ||
                                            (
                                                groupMemberAttributesIds.Contains( v.AttributeId ) &&
                                                groupMemberIds.Contains( v.EntityId.Value )
                                            ) ||
                                            (
                                                registrantAttributeIds.Contains( v.AttributeId ) &&
                                                registrantIds.Contains( v.EntityId.Value )
                                            )
                                        ) ).ToList();

                                // Get the attributes to add to each row's object
                                var attributes = new Dictionary<string, AttributeCache>();
                                RegistrantFields
                                        .Where( f => f.Attribute != null )
                                        .Select( f => f.Attribute )
                                        .ToList()
                                    .ForEach( a => attributes
                                        .Add( a.Id.ToString() + a.Key, a ) );

                                // Initialize the grid's object list
                                gGroupPlacements.ObjectList = new Dictionary<string, object>();

                                // Loop through each of the current page's registrants and build an attribute
                                // field object for storing attributes and the values for each of the registrants
                                foreach ( var registrant in currentPageRegistrants )
                                {
                                    // Create a row attribute object
                                    var attributeFieldObject = new AttributeFieldObject();

                                    // Add the attributes to the attribute object
                                    attributeFieldObject.Attributes = attributes;

                                    // Add any person attribute values to object
                                    attributeValues
                                        .Where( v =>
                                            personAttributesIds.Contains( v.AttributeId ) &&
                                            v.EntityId.Value == registrant.PersonAlias.PersonId )
                                        .ToList()
                                        .ForEach( v => attributeFieldObject.AttributeValues
                                            .Add( v.AttributeId.ToString() + v.Attribute.Key, new AttributeValueCache( v ) ) );

                                    // Add any group member attribute values to object
                                    if ( registrant.GroupMemberId.HasValue )
                                    {
                                        attributeValues
                                            .Where( v =>
                                                groupMemberAttributesIds.Contains( v.AttributeId ) &&
                                                v.EntityId.Value == registrant.GroupMemberId.Value )
                                            .ToList()
                                            .ForEach( v => attributeFieldObject.AttributeValues
                                                .Add( v.AttributeId.ToString() + v.Attribute.Key, new AttributeValueCache( v ) ) );
                                    }

                                    // Add any registrant attribute values to object
                                    attributeValues
                                        .Where( v =>
                                            registrantAttributeIds.Contains( v.AttributeId ) &&
                                            v.EntityId.Value == registrant.Id )
                                        .ToList()
                                        .ForEach( v => attributeFieldObject.AttributeValues
                                            .Add( v.AttributeId.ToString() + v.Attribute.Key, new AttributeValueCache( v ) ) );

                                    // Add row attribute object to grid's object list
                                    gGroupPlacements.ObjectList.Add( registrant.Id.ToString(), attributeFieldObject );
                                }
                            }
                        }
                    }

                    gGroupPlacements.DataBind();
                }
            }
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the fGroupPlacements control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void fGroupPlacements_ApplyFilterClick( object sender, EventArgs e )
        {
            fGroupPlacements.SaveUserPreference( "GroupPlacements-Date Range", "Date Range", sdrpGroupPlacementsDateRange.DelimitedValues );
            fGroupPlacements.SaveUserPreference( "GroupPlacements-First Name", "First Name",  tbGroupPlacementsFirstName.Text );
            fGroupPlacements.SaveUserPreference( "GroupPlacements-Last Name", "Last Name", tbGroupPlacementsLastName.Text );
            fGroupPlacements.SaveUserPreference( "GroupPlacements-In Group", "In Group", ddlGroupPlacementsInGroup.SelectedValue );
            fGroupPlacements.SaveUserPreference( "GroupPlacements-Signed Document", "Signed Document", ddlGroupPlacementsSignedDocument.SelectedValue );

            if ( RegistrantFields != null )
            {
                foreach ( var field in RegistrantFields )
                {
                    if ( field.FieldSource == RegistrationFieldSource.PersonField && field.PersonFieldType.HasValue )
                    {
                        switch ( field.PersonFieldType.Value )
                        {
                            case RegistrationPersonFieldType.Campus:
                                var ddlCampus = phGroupPlacementsFormFieldFilters.FindControl( "ddlGroupPlacementsCampus" ) as RockDropDownList;
                                if ( ddlCampus != null )
                                {
                                    fGroupPlacements.SaveUserPreference( "GroupPlacements-Home Campus", "Home Campus", ddlCampus.SelectedValue );
                                }

                                break;

                            case RegistrationPersonFieldType.Email:
                                var tbEmailFilter = phGroupPlacementsFormFieldFilters.FindControl( "tbGroupPlacementsEmailFilter" ) as RockTextBox;
                                if ( tbEmailFilter != null )
                                {
                                    fGroupPlacements.SaveUserPreference( "GroupPlacements-Email", "Email", tbEmailFilter.Text );
                                }

                                break;

                            case RegistrationPersonFieldType.Birthdate:
                                var drpBirthdateFilter = phGroupPlacementsFormFieldFilters.FindControl( "drpGroupPlacementsBirthdateFilter" ) as DateRangePicker;
                                if ( drpBirthdateFilter != null )
                                {
                                    fGroupPlacements.SaveUserPreference( "GroupPlacements-Birthdate Range", "Birthdate Range", drpBirthdateFilter.DelimitedValues );
                                }

                                break;
                            case RegistrationPersonFieldType.MiddleName:
                                var tbGroupPlacementsMiddleNameFilter = phGroupPlacementsFormFieldFilters.FindControl( "tbGroupPlacementsMiddleNameFilter" ) as RockTextBox;
                                if ( tbGroupPlacementsMiddleNameFilter != null )
                                {
                                    fGroupPlacements.SaveUserPreference( "GroupPlacements-MiddleName", "MiddleName", tbGroupPlacementsMiddleNameFilter.Text );
                                }

                                break;

                            case RegistrationPersonFieldType.AnniversaryDate:
                                var drpGroupPlacementsAnniversaryDateFilter = phGroupPlacementsFormFieldFilters.FindControl( "drpGroupPlacementsAnniversaryDateFilter" ) as DateRangePicker;
                                if ( drpGroupPlacementsAnniversaryDateFilter != null )
                                {
                                    fGroupPlacements.SaveUserPreference( "GroupPlacements-AnniversaryDate Range", "AnniversaryDate Range", drpGroupPlacementsAnniversaryDateFilter.DelimitedValues );
                                }

                                break;
                            case RegistrationPersonFieldType.Grade:
                                var gpGradeFilter = phGroupPlacementsFormFieldFilters.FindControl( "gpGroupPlacementsGradeFilter" ) as GradePicker;
                                if ( gpGradeFilter != null )
                                {
                                    int? gradeOffset = gpGradeFilter.SelectedValueAsInt( false );
                                    fGroupPlacements.SaveUserPreference( "GroupPlacements-Grade", "Grade", gradeOffset.HasValue ? gradeOffset.Value.ToString() : string.Empty );
                                }

                                break;
                                
                            case RegistrationPersonFieldType.Gender:
                                var ddlGenderFilter = phGroupPlacementsFormFieldFilters.FindControl( "ddlGroupPlacementsGenderFilter" ) as RockDropDownList;
                                if ( ddlGenderFilter != null )
                                {
                                    fGroupPlacements.SaveUserPreference( "GroupPlacements-Gender", "Gender", ddlGenderFilter.SelectedValue );
                                }

                                break;

                            case RegistrationPersonFieldType.MaritalStatus:
                                var dvpMaritalStatusFilter = phGroupPlacementsFormFieldFilters.FindControl( "dvpGroupPlacementsMaritalStatusFilter" ) as DefinedValuePicker;
                                if ( dvpMaritalStatusFilter != null )
                                {
                                    fGroupPlacements.SaveUserPreference( "GroupPlacements-Marital Status", "Marital Status", dvpMaritalStatusFilter.SelectedValue );
                                }

                                break;
                                
                            case RegistrationPersonFieldType.MobilePhone:
                                var tbMobilePhoneFilter = phGroupPlacementsFormFieldFilters.FindControl( "tbGroupPlacementsMobilePhoneFilter" ) as RockTextBox;
                                if ( tbMobilePhoneFilter != null )
                                {
                                    fGroupPlacements.SaveUserPreference( "GroupPlacements-Phone", "Cell Phone", tbMobilePhoneFilter.Text );
                                }

                                break;

                            case RegistrationPersonFieldType.HomePhone:
                                var tbGroupPlacementsHomePhoneFilter = phGroupPlacementsFormFieldFilters.FindControl( "tbGroupPlacementsHomePhoneFilter" ) as RockTextBox;
                                if ( tbGroupPlacementsHomePhoneFilter != null )
                                {
                                    fGroupPlacements.SaveUserPreference( "GroupPlacements-HomePhone", "Home Phone", tbGroupPlacementsHomePhoneFilter.Text );
                                }

                                break;
                        }
                    }

                    if ( field.Attribute != null )
                    {
                        var attribute = field.Attribute;
                        var filterControl = phGroupPlacementsFormFieldFilters.FindControl( "filterGroupPlacements_" + attribute.Id.ToString() );
                        if ( filterControl != null )
                        {
                            try
                            {
                                var values = attribute.FieldType.Field.GetFilterValues( filterControl, field.Attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter );
                                fGroupPlacements.SaveUserPreference( "GroupPlacements-" + attribute.Key, attribute.Name, attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter ).ToJson() );
                            }
                            catch { }
                        }
                    }
                }
            }

            BindGroupPlacementGrid();
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the fGroupPlacements control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void fGroupPlacements_ClearFilterClick( object sender, EventArgs e )
        {
            fGroupPlacements.DeleteUserPreferences();

            foreach ( var control in phGroupPlacementsFormFieldFilters.ControlsOfTypeRecursive<Control>().Where(a => a.ID != null && a.ID.StartsWith( "filter" ) && a.ID.Contains("_") ))
            {
                var attributeId = control.ID.Split('_')[1].AsInteger();
                var attribute = AttributeCache.Get( attributeId );
                if ( attribute != null )
                {
                    attribute.FieldType.Field.SetFilterValues( control, attribute.QualifierValues, new List<string>() );
                }
            }

            if ( RegistrantFields != null )
            {
                foreach ( var field in RegistrantFields )
                {
                    if ( field.FieldSource == RegistrationFieldSource.PersonField && field.PersonFieldType.HasValue )
                    {
                        switch ( field.PersonFieldType.Value )
                        {
                            case RegistrationPersonFieldType.Campus:
                                var ddlCampus = phGroupPlacementsFormFieldFilters.FindControl( "ddlGroupPlacementsCampus" ) as RockDropDownList;
                                if ( ddlCampus != null )
                                {
                                    ddlCampus.SetValue( ( Guid? ) null );
                                }

                                break;

                            case RegistrationPersonFieldType.Email:
                                var tbEmailFilter = phGroupPlacementsFormFieldFilters.FindControl( "tbGroupPlacementsEmailFilter" ) as RockTextBox;
                                if ( tbEmailFilter != null )
                                {
                                    tbEmailFilter.Text = string.Empty;
                                }

                                break;

                            case RegistrationPersonFieldType.Birthdate:
                                var drpBirthdateFilter = phGroupPlacementsFormFieldFilters.FindControl( "drpGroupPlacementsBirthdateFilter" ) as DateRangePicker;
                                if ( drpBirthdateFilter != null )
                                {
                                    drpBirthdateFilter.LowerValue = null;
                                    drpBirthdateFilter.UpperValue = null;
                                }

                                break;
                            case RegistrationPersonFieldType.MiddleName:
                                var tbGroupPlacementsMiddleNameFilter = phGroupPlacementsFormFieldFilters.FindControl( "tbGroupPlacementsMiddleNameFilter" ) as RockTextBox;
                                if ( tbGroupPlacementsMiddleNameFilter != null )
                                {
                                    tbGroupPlacementsMiddleNameFilter.Text = string.Empty;
                                }

                                break;

                            case RegistrationPersonFieldType.AnniversaryDate:
                                var drpGroupPlacementsAnniversaryDateFilter = phGroupPlacementsFormFieldFilters.FindControl( "drpGroupPlacementsAnniversaryDateFilter" ) as DateRangePicker;
                                if ( drpGroupPlacementsAnniversaryDateFilter != null )
                                {
                                    drpGroupPlacementsAnniversaryDateFilter.LowerValue = null;
                                    drpGroupPlacementsAnniversaryDateFilter.UpperValue = null;
                                }

                                break;
                            case RegistrationPersonFieldType.Grade:
                                var gpGradeFilter = phGroupPlacementsFormFieldFilters.FindControl( "gpGroupPlacementsGradeFilter" ) as GradePicker;
                                if ( gpGradeFilter != null )
                                {
                                    gpGradeFilter.SetValue( ( Guid? ) null );
                                }

                                break;
                                
                            case RegistrationPersonFieldType.Gender:
                                var ddlGenderFilter = phGroupPlacementsFormFieldFilters.FindControl( "ddlGroupPlacementsGenderFilter" ) as RockDropDownList;
                                if ( ddlGenderFilter != null )
                                {
                                    ddlGenderFilter.SetValue( ( Guid? ) null );
                                }

                                break;

                            case RegistrationPersonFieldType.MaritalStatus:
                                var dvpMaritalStatusFilter = phGroupPlacementsFormFieldFilters.FindControl( "dvpGroupPlacementsMaritalStatusFilter" ) as DefinedValuePicker;
                                if ( dvpMaritalStatusFilter != null )
                                {
                                    dvpMaritalStatusFilter.SetValue( ( Guid? ) null );
                                }

                                break;
                                
                            case RegistrationPersonFieldType.MobilePhone:
                                var tbMobilePhoneFilter = phGroupPlacementsFormFieldFilters.FindControl( "tbGroupPlacementsMobilePhoneFilter" ) as RockTextBox;
                                if ( tbMobilePhoneFilter != null )
                                {
                                    tbMobilePhoneFilter.Text = string.Empty;
                                }

                                break;

                            case RegistrationPersonFieldType.HomePhone:
                                var tbGroupPlacementsHomePhoneFilter = phGroupPlacementsFormFieldFilters.FindControl( "tbGroupPlacementsHomePhoneFilter" ) as RockTextBox;
                                if ( tbGroupPlacementsHomePhoneFilter != null )
                                {
                                    tbGroupPlacementsHomePhoneFilter.Text = string.Empty;
                                }

                                break;
                        }
                    }
                }
            }

            BindGroupPlacementsFilter( null );
        }

        /// <summary>
        /// fs the group placements display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void fGroupPlacements_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            if ( e.Key.StartsWith( "GroupPlacements-" ) )
            {
                var key = e.Key.Remove( 0, "GroupPlacements-".Length );

                if ( RegistrantFields != null )
                {
                    var attribute = RegistrantFields
                        .Where( a =>
                            a.Attribute != null &&
                            a.Attribute.Key == key )
                        .Select( a => a.Attribute )
                        .FirstOrDefault();

                    if ( attribute != null )
                    {
                        try
                        {
                            var values = JsonConvert.DeserializeObject<List<string>>( e.Value );
                            e.Value = attribute.FieldType.Field.FormatFilterValues( attribute.QualifierValues, values );
                            return;
                        }
                        catch { }
                    }
                }

                switch ( key )
                {
                    case "Date Range":
                    case "Birthdate Range":
                        // The value might either be from a SlidingDateRangePicker or a DateRangePicker, so try both
                        var storedValue = e.Value;
                        e.Value = SlidingDateRangePicker.FormatDelimitedValues( storedValue );
                        if ( e.Value.IsNullOrWhiteSpace() )
                        {
                            e.Value = DateRangePicker.FormatDelimitedValues( storedValue );
                        }

                        break;

                    case "Grade":
                        e.Value = Person.GradeFormattedFromGradeOffset( e.Value.AsIntegerOrNull() );
                        break;
                        
                    case "First Name":
                    case "Last Name":
                    case "Email":
                    case "HomePhone":
                    case "Phone":
                    case "Signed Document":
                        break;

                    case "Gender":
                        var gender = e.Value.ConvertToEnumOrNull<Gender>();
                        e.Value = gender.HasValue ? gender.ConvertToString() : string.Empty;
                        break;
                        
                    case "Campus":
                        int? campusId = e.Value.AsIntegerOrNull();
                        if ( campusId.HasValue )
                        {
                            var campus = CampusCache.Get( campusId.Value );
                            e.Value = campus != null ? campus.Name : string.Empty;
                        }
                        else
                        {
                            e.Value = string.Empty;
                        }

                        break;
                        
                    case "Marital Status":
                        int? dvId = e.Value.AsIntegerOrNull();
                        if ( dvId.HasValue )
                        {
                            var maritalStatus = DefinedValueCache.Get( dvId.Value );
                            e.Value = maritalStatus != null ? maritalStatus.Value : string.Empty;
                        }
                        else
                        {
                            e.Value = string.Empty;
                        }

                        break;
                        
                    case "In Group":
                        e.Value = e.Value;
                        break;
                        
                    default:
                        e.Value = string.Empty;
                        break;
                }
            }
            else
            {
                e.Value = string.Empty;
            }
        }

        /// <summary>
        /// Binds the group placements filter.
        /// </summary>
        /// <param name="instance">The instance.</param>
        private void BindGroupPlacementsFilter( RegistrationInstance instance )
        {
            sdrpGroupPlacementsDateRange.DelimitedValues = fGroupPlacements.GetUserPreference( "GroupPlacements-Date Range" );
            tbGroupPlacementsFirstName.Text = fGroupPlacements.GetUserPreference( "GroupPlacements-First Name" );
            tbGroupPlacementsLastName.Text = fGroupPlacements.GetUserPreference( "GroupPlacements-Last Name" );
            ddlGroupPlacementsInGroup.SetValue( fGroupPlacements.GetUserPreference( "GroupPlacements-In Group" ) );

            ddlGroupPlacementsSignedDocument.SetValue( fGroupPlacements.GetUserPreference( "GroupPlacements-Signed Document" ) );
            ddlGroupPlacementsSignedDocument.Visible = instance != null && instance.RegistrationTemplate != null && instance.RegistrationTemplate.RequiredSignatureDocumentTemplateId.HasValue;
        }

        #endregion

        #endregion

        #region Helper Classes

        /// <summary>
        /// Helper class for tracking registration form fields
        /// </summary>
        [Serializable]
        public class RegistrantFormField
        {
            /// <summary>
            /// Gets or sets the field source.
            /// </summary>
            /// <value>
            /// The field source.
            /// </value>
            public RegistrationFieldSource FieldSource { get; set; }

            /// <summary>
            /// Gets or sets the type of the person field.
            /// </summary>
            /// <value>
            /// The type of the person field.
            /// </value>
            public RegistrationPersonFieldType? PersonFieldType { get; set; }

            /// <summary>
            /// Gets or sets the attribute.
            /// </summary>
            /// <value>
            /// The attribute.
            /// </value>
            public AttributeCache Attribute { get; set; }
        }

        #endregion
    }

}