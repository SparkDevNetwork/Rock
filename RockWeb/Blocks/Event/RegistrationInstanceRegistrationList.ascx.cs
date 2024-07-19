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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Event
{
    /// <summary>
    /// A Block that displays the list of Registrations related to a Registration Instance.
    /// </summary>
    [DisplayName( "Registration Instance - Registration List" )]
    [Category( "Event" )]
    [Description( "Displays the list of Registrations related to a Registration Instance." )]

    #region Block Attributes

    [LinkedPage(
        "Registration Page",
        "The page for editing registration and registrant information",
        Key = AttributeKey.RegistrationPage,
        DefaultValue = Rock.SystemGuid.Page.REGISTRATION_DETAIL,
        IsRequired = false,
        Order = 1 )]

    [BooleanField(
        "Display Discount Codes",
        "Display the discount code used with a payment",
        Key = AttributeKey.DisplayDiscountCodes,
        DefaultBooleanValue = false,
        Order = 2 )]

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "A8DB2C89-F80A-43A2-AA53-36C78673F504" )]
    public partial class RegistrationInstanceRegistrationList : RegistrationInstanceBlock, ISecondaryBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The linked page used to display registration details.
            /// </summary>
            public const string RegistrationPage = "RegistrationPage";

            /// <summary>
            /// Should discount codes be displayed in the list?
            /// </summary>
            public const string DisplayDiscountCodes = "DisplayDiscountCodes";
        }

        #endregion Attribute Keys

        #region Page Parameter Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            /// <summary>
            /// The Registration Instance identifier
            /// </summary>
            public const string RegistrationInstanceId = "RegistrationInstanceId";
        }

        #endregion Page Parameter Keys

        #region Properties and Fields

        private List<FinancialTransactionDetail> _registrationPayments;

        private bool _instanceHasCost = false;

        /// <summary>
        /// Gets or sets the available registration attributes where IsGridColumn = true
        /// </summary>
        /// <value>
        /// The available attributes.
        /// </value>
        public int[] AvailableRegistrationAttributeIdsForGrid { get; set; }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            AvailableRegistrationAttributeIdsForGrid = ViewState[ViewStateKeyBase.AvailableRegistrationAttributeIdsForGrid] as int[];

            // Don't set the dynamic control values if this is a postback from a grid 'ClearFilter'.
            bool setValues = this.Request.Params["__EVENTTARGET"] == null || !this.Request.Params["__EVENTTARGET"].EndsWith( "_lbClearFilter" );

            RegistrationsTabAddDynamicControls();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            fRegistrations.ApplyFilterClick += fRegistrations_ApplyFilterClick;

            gRegistrations.EmptyDataText = "No Registrations Found";
            gRegistrations.DataKeyNames = new string[] { "Id" };
            gRegistrations.Actions.ShowAdd = true;
            gRegistrations.Actions.AddClick += gRegistrations_AddClick;
            gRegistrations.RowDataBound += gRegistrations_RowDataBound;
            gRegistrations.GridRebind += gRegistrations_GridRebind;
            gRegistrations.ShowConfirmDeleteDialog = false;

            // Add the main content panel as an update trigger for block configuration changes.
            this.AddConfigurationUpdateTrigger( upnlContent );

            string deleteScript = @"
                $('table.js-grid-registration a.grid-delete-button').click(function( e ){
                    e.preventDefault();
                    var $hfHasPayments = $(this).closest('tr').find('input.js-has-payments').first();
                    Rock.dialogs.confirm('Are you sure you want to delete this registration? All of the registrants will also be deleted!', function (result) {
                        if (result) {
                            if ( $hfHasPayments.val() == 'True' ) {
                                Rock.dialogs.confirm('This registration also has payments. Are you really sure that you want to delete the registration?<br/><small>(payments will not be deleted, but they will no longer be associated with a registration)</small>', function (result) {
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
            ScriptManager.RegisterStartupScript( gRegistrations, gRegistrations.GetType(), "deleteInstanceScript", deleteScript, true );
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
                ShowDetail();
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
            ViewState[ViewStateKeyBase.AvailableRegistrationAttributeIdsForGrid] = AvailableRegistrationAttributeIdsForGrid;

            return base.SaveViewState();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the fRegistrations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void fRegistrations_ApplyFilterClick( object sender, EventArgs e )
        {
            fRegistrations.SetFilterPreference( "Registrations Date Range", "Registration Date Range", sdrpRegistrationDateRange.DelimitedValues );
            fRegistrations.SetFilterPreference( "Payment Status", ddlRegistrationPaymentStatus.SelectedValue );
            fRegistrations.SetFilterPreference( "RegisteredBy First Name", tbRegistrationRegisteredByFirstName.Text );
            fRegistrations.SetFilterPreference( "RegisteredBy Last Name", tbRegistrationRegisteredByLastName.Text );
            fRegistrations.SetFilterPreference( "Registrant First Name", tbRegistrationRegistrantFirstName.Text );
            fRegistrations.SetFilterPreference( "Registrant Last Name", tbRegistrationRegistrantLastName.Text );
            fRegistrations.SetFilterPreference( UserPreferenceKeyBase.GridFilter_RegistrationCampus, cblCampus.SelectedValues.AsDelimited( ";" ) );

            // Store the selected date range in the page context so it can be used to synchronise the data displayed by other blocks, such as the RegistrationInstanceGroupPlacement block.
            RockPage.SaveSharedItem( RegistrationInstanceBlock.SharedItemKey.RegistrationDateRange, DateRange.FromDelimitedValues( sdrpRegistrationDateRange.DelimitedValues ) );

            BindRegistrationsGrid();
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the fRegistrations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void fRegistrations_ClearFilterClick( object sender, EventArgs e )
        {
            fRegistrations.SetFilterPreference( "Registrations Date Range", "Registration Date Range", string.Empty );
            fRegistrations.SetFilterPreference( "Payment Status", string.Empty );
            fRegistrations.SetFilterPreference( "RegisteredBy First Name", string.Empty );
            fRegistrations.SetFilterPreference( "RegisteredBy Last Name", string.Empty );
            fRegistrations.SetFilterPreference( "Registrant First Name", string.Empty );
            fRegistrations.SetFilterPreference( "Registrant Last Name", string.Empty );
            fRegistrations.SetFilterPreference( UserPreferenceKeyBase.GridFilter_RegistrationCampus, string.Empty );

            BindRegistrationsFilter();
        }

        /// <summary>
        /// Get the display value for a filter field.
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
                case UserPreferenceKeyBase.GridFilter_RegistrationCampus:
                    e.Value = GetRegistrationsFilterCampuses( e.Value );
                    break;
                default:
                    e.Value = string.Empty;
                    break;
            }
        }

        /// <summary>
        /// Gets the registrations filter campuses.
        /// </summary>
        /// <param name="filterValue">The value.</param>
        private string GetRegistrationsFilterCampuses( string filterValue )
        {
            var values = new List<string>();
            foreach ( string value in filterValue.Split( ';' ) )
            {
                var item = cblCampus.Items.FindByValue( value );
                if ( item != null )
                {
                    values.Add( item.Text );
                }
            }

            return values.AsDelimited( ", " );
        }

        /// <summary>
        /// Handles the GridRebind event of the gRegistrations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gRegistrations_GridRebind( object sender, EventArgs e )
        {
            var registrationInstance = this.RegistrationInstance;

            if ( registrationInstance == null )
            {
                return;
            }

            gRegistrations.ExportTitleName = registrationInstance.Name + " - Registrations";
            gRegistrations.ExportFilename = gRegistrations.ExportFilename ?? registrationInstance.Name + "Registrations";

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

                var payments = _registrationPayments.Where( p => p.EntityId == registration.Id );
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
                    var balanceDue = registration.DiscountedCost - totalPaid;
                    lBalance.Visible = _instanceHasCost || discountedCost > 0.0M;

                    var isPaymentPlanActive = registration.IsPaymentPlanActive;

                    string balanceCssClass;
                    if ( balanceDue > 0.0m )
                    {
                        if ( !isPaymentPlanActive )
                        {
                            balanceCssClass = "label-danger";
                        }
                        else
                        {
                            balanceCssClass = "label-warning";
                        }
                    }
                    else if ( balanceDue < 0.0m )
                    {
                        balanceCssClass = "label-warning";
                    }
                    else
                    {
                        balanceCssClass = "label-success";
                    }

                    var paymentPlanIcon = string.Empty;
                    if ( isPaymentPlanActive )
                    {
                        paymentPlanIcon = "<i class='fa fa-calendar-day'></i>";
                    }

                    lBalance.Text = $"<span class='label {balanceCssClass}'>{balanceDue.FormatAsCurrency()}{paymentPlanIcon}</span><input type='hidden' class='js-has-payments' value='{hasPayments.ToTrueFalse()}' />";
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
            NavigateToLinkedPage( AttributeKey.RegistrationPage, "RegistrationId", 0, PageParameterKey.RegistrationInstanceId, this.RegistrationInstanceId );
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

                    hfHasPayments.Value = this.RegistrationInstanceHasPayments.ToString();
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
            NavigateToLinkedPage( AttributeKey.RegistrationPage, "RegistrationId", e.RowKeyId, PageParameterKey.RegistrationInstanceId, this.RegistrationInstanceId );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        private void ShowDetail()
        {
            var registrationInstance = this.RegistrationInstance;

            if ( registrationInstance == null )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var availableRegistrationAttributesForGrid = new List<AttributeCache>();

                int entityTypeId = new Registration().TypeId;
                foreach ( var attributeCache in new AttributeService( new RockContext() ).GetByEntityTypeQualifier( entityTypeId, "RegistrationTemplateId", registrationInstance.RegistrationTemplateId.ToString(), false )
                    .Where( a => a.IsGridColumn )
                    .OrderBy( a => a.Order )
                    .ThenBy( a => a.Name )
                    .ToAttributeCacheList() )
                {
                    availableRegistrationAttributesForGrid.Add( attributeCache );
                }

                AvailableRegistrationAttributeIdsForGrid = availableRegistrationAttributesForGrid.Select( a => a.Id ).ToArray();

                pnlDetails.Visible = true;

                hfHasPayments.Value = this.RegistrationInstanceHasPayments.ToString();

                if ( !this.UserCanEditBlockContent )
                {
                    bool allowRegistrationEdit = registrationInstance.IsAuthorized( "Register", CurrentPerson );

                    gRegistrations.Actions.ShowAdd = allowRegistrationEdit;
                    gRegistrations.IsDeleteEnabled = allowRegistrationEdit;
                }

                BindRegistrationsFilter();
                RegistrationsTabAddDynamicControls();
                BindRegistrationsGrid();
            }
        }

        /// <summary>
        /// Sets the user preference prefix.
        /// </summary>
        private void SetUserPreferencePrefix( int registrationTemplateId )
        {
            fRegistrations.PreferenceKeyPrefix = string.Format( "{0}-", registrationTemplateId );
        }

        /// <summary>
        /// Binds the registrations filter.
        /// </summary>
        private void BindRegistrationsFilter()
        {
            sdrpRegistrationDateRange.DelimitedValues = fRegistrations.GetFilterPreference( UserPreferenceKeyBase.GridFilter_RegistrationsDateRange );
            ddlRegistrationPaymentStatus.SetValue( fRegistrations.GetFilterPreference( UserPreferenceKeyBase.GridFilter_PaymentStatus ) );
            tbRegistrationRegisteredByFirstName.Text = fRegistrations.GetFilterPreference( UserPreferenceKeyBase.GridFilter_RegisteredByFirstName );
            tbRegistrationRegisteredByLastName.Text = fRegistrations.GetFilterPreference( UserPreferenceKeyBase.GridFilter_RegisteredByLastName );
            tbRegistrationRegistrantFirstName.Text = fRegistrations.GetFilterPreference( UserPreferenceKeyBase.GridFilter_RegistrantFirstName );
            tbRegistrationRegistrantLastName.Text = fRegistrations.GetFilterPreference( UserPreferenceKeyBase.GridFilter_RegistrantLastName );

            cblCampus.DataSource = CampusCache.All();
            cblCampus.DataBind();

            string campusValue = fRegistrations.GetFilterPreference( UserPreferenceKeyBase.GridFilter_RegistrationCampus );

            if ( !string.IsNullOrWhiteSpace( campusValue ) )
            {
                cblCampus.SetValues( campusValue.Split( ';' ).ToList() );
            }
        }

        /// <summary>
        /// Binds the registrations grid.
        /// </summary>
        private void BindRegistrationsGrid()
        {
            int? instanceId = this.RegistrationInstanceId;

            if ( instanceId.HasValue && instanceId > 0 )
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
                        .Queryable( "PersonAlias.Person,Registrants.PersonAlias.Person,Registrants.Fees.RegistrationTemplateFee,Campus,PaymentPlanFinancialScheduledTransaction" )
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

                    List<int> campusIds = cblCampus.SelectedValuesAsInt;
                    if ( campusIds.Count > 0 )
                    {
                        qry = qry
                            .Where( r => r.CampusId.HasValue && campusIds.Contains( r.CampusId.Value ) );
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
                                DiscountCosts = r.Registrants.Sum( p => ( decimal? ) p.DiscountedCost( r.DiscountPercentage, r.DiscountAmount ) ) ?? 0.0m,
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
                            .GroupJoin(
                                rPayments,
                                c => c.Key,
                                p => p.Key,
                                ( c, p ) => new { rCosts = c, rPayments = p } )
                            .SelectMany( c => c.rPayments.DefaultIfEmpty(),
                                ( cp, p ) => new
                                {
                                    RegistrationId = cp.rCosts.Key,
                                    Costs = cp.rCosts.Value,
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

                        _registrationPayments = new FinancialTransactionDetailService( rockContext )
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
                        discountCodeHeader.Visible = GetAttributeValue( AttributeKey.DisplayDiscountCodes ).AsBoolean();
                    }

                    var campusColum = gRegistrations.ColumnsOfType<DataControlField>().FirstOrDefault( m => m.HeaderText == "Campus" );
                    if ( campusColum != null )
                    {
                        campusColum.Visible = qry.Any( m => m.CampusId.HasValue );
                    }

                    gRegistrations.DataBind();
                }
            }
        }

        /// <summary>
        /// Add all of the columns to the Registrations grid after the Registrants column.
        /// The Column.Insert method does not play well with buttons.
        /// </summary>
        private void RegistrationsTabAddDynamicControls()
        {
            var registrantsField = gRegistrations.ColumnsOfType<RockTemplateField>().FirstOrDefault( a => a.HeaderText == "Registrants" );
            int registrantsFieldIndex = gRegistrations.Columns.IndexOf( registrantsField );

            // Remove all columns to the right of Registrants
            for ( int i = registrantsFieldIndex + 2; i < gRegistrations.Columns.Count; i++ )
            {
                gRegistrations.Columns.RemoveAt( i );
            }

            // Add Attribute columns if necessary
            if ( AvailableRegistrationAttributeIdsForGrid != null )
            {
                foreach ( var attributeCache in AvailableRegistrationAttributeIdsForGrid.Select( a => AttributeCache.Get( a ) ) )
                {
                    bool columnExists = gRegistrations.Columns.OfType<AttributeField>().FirstOrDefault( a => a.AttributeId == attributeCache.Id ) != null;
                    if ( !columnExists )
                    {
                        AttributeField boundField = new AttributeField();
                        boundField.DataField = attributeCache.Key;
                        boundField.AttributeId = attributeCache.Id;
                        boundField.HeaderText = attributeCache.Name;
                        boundField.ItemStyle.HorizontalAlign = attributeCache.FieldType.Field.AlignValue;
                        gRegistrations.Columns.Add( boundField );
                    }
                }
            }

            // Add the rest of the columns
            var dtWhen = new DateTimeField { DataField = "CreatedDateTime", HeaderText = "When", SortExpression = "CreatedDateTime" };
            dtWhen.HeaderStyle.HorizontalAlign = HorizontalAlign.Left;
            dtWhen.ItemStyle.HorizontalAlign = HorizontalAlign.Left;
            gRegistrations.Columns.Add( dtWhen );

            var lDiscount = new RockLiteralField { ID = "lDiscount", HeaderText = "Discount Code", SortExpression = "DiscountCode", Visible = false };
            lDiscount.HeaderStyle.HorizontalAlign = HorizontalAlign.Left;
            lDiscount.ItemStyle.HorizontalAlign = HorizontalAlign.Left;
            gRegistrations.Columns.Add( lDiscount );

            var lRegistrationCost = new RockLiteralField { ID = "lRegistrationCost", HeaderText = "Total Cost", SortExpression = "TotalCost" };
            lRegistrationCost.HeaderStyle.HorizontalAlign = HorizontalAlign.Right;
            lRegistrationCost.ItemStyle.HorizontalAlign = HorizontalAlign.Right;
            gRegistrations.Columns.Add( lRegistrationCost );

            var lBalance = new RockLiteralField { ID = "lBalance", HeaderText = "Balance Due", SortExpression = "BalanceDue" };
            lBalance.HeaderStyle.HorizontalAlign = HorizontalAlign.Right;
            lBalance.ItemStyle.HorizontalAlign = HorizontalAlign.Right;
            gRegistrations.Columns.Add( lBalance );

            DeleteField deleteField = new DeleteField();
            deleteField.Click += gRegistrations_Delete;
            gRegistrations.Columns.Add( deleteField );
        }

        #endregion

        #region ISecondaryBlock

        /// <summary>
        /// Sets the visibility of the block.
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlDetails.Visible = visible;
        }

        #endregion
    }
}