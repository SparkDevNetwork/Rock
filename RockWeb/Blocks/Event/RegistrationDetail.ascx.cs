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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Attribute = Rock.Model.Attribute;

namespace RockWeb.Blocks.Event
{
    [DisplayName( "Registration Detail" )]
    [Category( "Event" )]
    [Description( "Displays the details of a given registration." )]

    [LinkedPage( "Registrant Page", "The page for viewing details about a registrant", true, "", "", 0 )]
    [LinkedPage( "Transaction Page", "The page for viewing transaction details", true, "", "", 1 )]
    [LinkedPage( "Group Detail Page", "The page for viewing details about a group", true, "", "", 2 )]
    [LinkedPage( "Group Member Page", "The page for viewing details about a group member", true, "", "", 3 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE, "Source", "The Financial Source Type to use when creating transactions", false, false, Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_ONSITE_COLLECTION, "", 4 )]
    [TextField( "Batch Name Prefix", "The batch prefix name to use when creating a new batch", false, "Event Registration", "", 5 )]
    public partial class RegistrationDetail : RockBlock, IDetailBlock
    {

        #region Fields

        private Registration Registration = null;

        #endregion

        #region Properties

        private string Title = null;
        private int? RegistrationInstanceId { get; set; }
        private int? RegistrationId { get; set; }
        private int? RegistrantId { get; set; }
        private bool EditAllowed { get; set; }
        protected bool PercentageDiscountExists { get; set; }

        private RegistrationTemplate RegistrationTemplateState { get; set; }
        private List<RegistrantInfo> RegistrantsState { get; set; }
        #endregion

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            Title = ViewState["Title"] as string ?? string.Empty;
            RegistrationInstanceId = ViewState["RegistrationInstanceId"] as int?;
            RegistrationId = ViewState["RegistrationId"] as int?;
            EditAllowed = ViewState["EditAllowed"] as bool? ?? false;
            PercentageDiscountExists = ViewState["PercentageDiscountExists"] as bool? ?? false;

            string json = ViewState["RegistrationTemplate"] as string;
            if ( !string.IsNullOrWhiteSpace( json ) )
            {
                RegistrationTemplateState = JsonConvert.DeserializeObject<RegistrationTemplate>( json );
            }

            json = ViewState["Registrants"] as string;
            if ( !string.IsNullOrWhiteSpace( json ) )
            {
                RegistrantsState = JsonConvert.DeserializeObject<List<RegistrantInfo>>( json );
            }

            if ( RegistrationTemplateState != null && RegistrantsState != null )
            {
                BuildRegistrationControls( false );
            }

            Registration = GetRegistration( RegistrationId );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RegisterClientScript();

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlRegistrationDetail );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbConfirmationQueued.Visible = false;
            nbPaymentError.Visible = false;

            if ( !Page.IsPostBack )
            {
                LoadState();
                if ( RegistrationInstanceId.HasValue )
                {
                    ShowDetail( RegistrationId.Value, RegistrationInstanceId );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
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
            ViewState["Title"] = Title;
            ViewState["RegistrationId"] = RegistrationId;
            ViewState["RegistrationInstanceId"] = RegistrationInstanceId;
            ViewState["EditAllowed"] = EditAllowed;
            ViewState["PercentageDiscountExists"] = PercentageDiscountExists;

            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState["RegistrationTemplate"] = JsonConvert.SerializeObject( RegistrationTemplateState, Formatting.None, jsonSetting );
            ViewState["Registrants"] = JsonConvert.SerializeObject( RegistrantsState, Formatting.None, jsonSetting );

            return base.SaveViewState();
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            if ( RegistrationId.HasValue )
            {
                ShowEditDetails( GetRegistration( RegistrationId.Value ) );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            RockContext rockContext = new RockContext();

            if ( RegistrationId.HasValue )
            {
                var registrationService = new RegistrationService( rockContext );
                Registration registration = registrationService.Get( RegistrationId.Value );

                if ( registration != null )
                {
                    if ( !registration.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) )
                    {
                        mdDeleteWarning.Show( "You are not authorized to delete this registration.", ModalAlertType.Information );
                        return;
                    }

                    string errorMessage;
                    if ( !registrationService.CanDelete( registration, out errorMessage ) )
                    {
                        mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    registrationService.Delete( registration );

                    rockContext.SaveChanges();
                }

                var pageParams = new Dictionary<string, string>();
                pageParams.Add( "RegistrationInstanceId", RegistrationInstanceId.ToString() );
                NavigateToParentPage( pageParams );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            if ( RegistrationId.HasValue )
            {
                Registration registration = null;
                RockContext rockContext = new RockContext();

                var registrationService = new RegistrationService( rockContext );

                bool newRegistration = false;

                if ( RegistrationId.Value != 0 )
                {
                    registration = registrationService.Queryable().Where( g => g.Id == RegistrationId.Value ).FirstOrDefault();
                }

                if ( registration == null )
                {
                    registration = new Registration { RegistrationInstanceId = RegistrationInstanceId ?? 0 };
                    registrationService.Add( registration );
                    newRegistration = true;
                }

                if ( registration != null && RegistrationInstanceId > 0 )
                {
                    registration.PersonAliasId = ppPerson.PersonAliasId;
                    registration.FirstName = tbFirstName.Text;
                    registration.LastName = tbLastName.Text;
                    registration.ConfirmationEmail = ebConfirmationEmail.Text;
                    registration.DiscountCode = ddlDiscountCode.SelectedValue;
                    registration.DiscountPercentage = nbDiscountPercentage.Text.AsDecimal() * 0.01m;
                    registration.DiscountAmount = cbDiscountAmount.Text.AsDecimal();

                    if ( !Page.IsValid )
                    {
                        return;
                    }

                    if ( !registration.IsValid )
                    {
                        // Controls will render the error messages                    
                        return;
                    }

                    // use WrapTransaction since SaveAttributeValues does it's own RockContext.SaveChanges()
                    rockContext.WrapTransaction( () =>
                    {
                        rockContext.SaveChanges();
                    } );

                    if ( newRegistration )
                    {
                        var pageRef = CurrentPageReference;
                        pageRef.Parameters.AddOrReplace("RegistrationId", registration.Id.ToString());
                        NavigateToPage( pageRef );
                    }
                    else
                    {
                        // Reload registration
                        var reloadedRegistration = GetRegistration( registration.Id );
                        lWizardRegistrationName.Text = reloadedRegistration.ToString();
                        ShowReadonlyDetails( reloadedRegistration );
                    }
                }
            }

        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( !RegistrationId.HasValue || RegistrationId.Value == 0 )
            {
                var pageParams = new Dictionary<string, string>();
                pageParams.Add( "RegistrationInstanceId", RegistrationInstanceId.ToString() );
                NavigateToParentPage( pageParams );
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                ShowReadonlyDetails( GetRegistration( RegistrationId.Value ) );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbWizardTemplate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbWizardTemplate_Click( object sender, EventArgs e )
        {
            var qryParams = new Dictionary<string, string>();
            var pageCache = PageCache.Read( RockPage.PageId );
            int templateId = 0;
            if ( Registration != null && Registration.RegistrationInstance != null )
            {
                templateId = Registration.RegistrationInstance.RegistrationTemplateId;
            }
            else
            {
                using ( var rockContext = new RockContext() )
                {
                    int instanceId = PageParameter( "RegistrationInstanceId" ).AsInteger();
                    templateId = new RegistrationInstanceService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( i => i.Id == instanceId )
                        .Select( i => i.RegistrationTemplateId )
                        .FirstOrDefault();
                }
            }
            
            if ( pageCache != null && pageCache.ParentPage != null && pageCache.ParentPage.ParentPage != null  )
            {
                qryParams.Add( "RegistrationTemplateId", templateId.ToString() );
                NavigateToPage( pageCache.ParentPage.ParentPage.Guid, qryParams );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbWizardInstance control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbWizardInstance_Click( object sender, EventArgs e )
        {
            var qryParams = new Dictionary<string, string>();
            var pageCache = PageCache.Read( RockPage.PageId );
            var instanceId = Registration != null ? Registration.RegistrationInstanceId.ToString() : PageParameter("RegistrationInstanceId");
            if ( pageCache != null && pageCache.ParentPage != null )
            {
                qryParams.Add( "RegistrationInstanceId", instanceId );
                NavigateToPage( pageCache.ParentPage.Guid, qryParams );
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            if ( Registration != null )
            {
                ShowReadonlyDetails( Registration );
            }
            else
            {
                string registrationId = PageParameter( "RegistrationId" );
                if ( !string.IsNullOrWhiteSpace( registrationId ) )
                {
                    ShowDetail( registrationId.AsInteger(), PageParameter( "registrationInstanceId" ).AsIntegerOrNull() );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
        }

        #endregion

        #region Registration Detail Events

        protected void ppPerson_SelectPerson( object sender, EventArgs e )
        {
            if ( ppPerson.PersonId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var person = new PersonService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( p => p.Id == ppPerson.PersonId.Value )
                        .Select( p => new
                        {
                            Email = p.Email,
                            NickName = p.NickName,
                            LastName = p.LastName
                        } )
                        .FirstOrDefault();

                    if ( person != null )
                    {
                        if ( string.IsNullOrWhiteSpace( ebConfirmationEmail.Text ) )
                        {
                            ebConfirmationEmail.Text = person.Email;
                        }
                        if ( string.IsNullOrWhiteSpace( tbFirstName.Text ) )
                        {
                            tbFirstName.Text = person.NickName;
                        }
                        if ( string.IsNullOrWhiteSpace( tbLastName.Text ) )
                        {
                            tbLastName.Text = person.LastName;
                        }
                    }
                }
            }
        }

        protected void ddlDiscountCode_SelectedIndexChanged( object sender, EventArgs e )
        {
            RegistrationTemplateDiscount discount = null;

            string code = ddlDiscountCode.SelectedValue;
            if ( !string.IsNullOrWhiteSpace( code ) )
            {
                if ( RegistrationTemplateState != null )
                {
                    discount = RegistrationTemplateState.Discounts
                        .Where( d => d.Code.Equals( code, StringComparison.OrdinalIgnoreCase ) )
                        .FirstOrDefault();
                }
            }

            nbDiscountPercentage.Text = discount != null && discount.DiscountPercentage != 0.0m ? (discount.DiscountPercentage * 100.0m).ToString("N0") : "";
            cbDiscountAmount.Text = discount != null && discount.DiscountAmount != 0.0m ? discount.DiscountAmount.ToString( "N2" ) : "";
        }
        
        protected void lbResendConfirmation_Click( object sender, EventArgs e )
        {
            if ( RegistrationId.HasValue )
            {
                string appRoot = ResolveRockUrl( "~/" );
                string themeRoot = ResolveRockUrl( "~~/" );

                var confirmation = new Rock.Transactions.SendRegistrationConfirmationTransaction();
                confirmation.RegistrationId = RegistrationId.Value;
                confirmation.AppRoot = appRoot;
                confirmation.ThemeRoot = themeRoot;
                Rock.Transactions.RockQueue.TransactionQueue.Enqueue( confirmation );

                nbConfirmationQueued.Visible = true;
            }
        }

        protected void lbAddPayment_Click( object sender, EventArgs e )
        {

            if ( Registration != null && Registration.PersonAliasId.HasValue && RegistrationTemplateState != null && RegistrationTemplateState.FinancialGateway != null )
            {
                var component = RegistrationTemplateState.FinancialGateway.GetGatewayComponent();
                if ( component != null )
                {
                    txtCardFirstName.Visible = component.SplitNameOnCard;
                    txtCardLastName.Visible = component.SplitNameOnCard;
                    txtCardName.Visible = !component.SplitNameOnCard;
                    mypExpiration.MinimumYear = RockDateTime.Now.Year;

                    cbPaymentAmount.Text = string.Empty;
                    cbPaymentAmount.Text = Registration.BalanceDue.ToString( "N2" );

                    txtCreditCard.Text = string.Empty;
                    mypExpiration.SelectedDate = null;
                    txtCVV.Text = string.Empty;

                    if ( Registration.PersonAlias != null && Registration.PersonAlias.Person != null )
                    {
                        var person = Registration.PersonAlias.Person;
                        txtCardFirstName.Text = person.FirstName;
                        txtCardLastName.Text = person.LastName;
                        txtCardName.Text = person.FullName;

                        var location = person.GetHomeLocation();
                        acBillingAddress.SetValues( location );
                    }

                    pnlCosts.Visible = false;
                    pnlPaymentInfo.Visible = true;
                    return;
                }
            }

        }

        protected void lbCancelPayment_Click( object sender, EventArgs e )
        {
            pnlPaymentInfo.Visible = false;
            pnlCosts.Visible = true;
        }

        protected void lbSubmitPayment_Click( object sender, EventArgs e )
        {
            decimal pmtAmount = cbPaymentAmount.Text.AsDecimal();
            if ( Registration != null && Registration.BalanceDue >= pmtAmount && pmtAmount > 0 )
            {
                try
                {
                    var rockContext = new RockContext();
                    rockContext.WrapTransaction( () =>
                    {
                        string errorMessage = string.Empty;
                        if ( !ProcessPayment( rockContext, Registration, pmtAmount, out errorMessage ) )
                        {
                            throw new Exception( errorMessage );
                        }
                    } );

                    // reload registration
                    Registration = GetRegistration( Registration.Id, rockContext );

                    RockPage.UpdateBlocks( "~/Blocks/Finance/TransactionList.ascx" );

                    ShowReadonlyDetails( Registration );

                    pnlPaymentInfo.Visible = false;
                    pnlCosts.Visible = true;

                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex, Context, this.RockPage.PageId, this.RockPage.Site.Id, CurrentPersonAlias );

                    nbPaymentError.Heading = "Error Processing Payment";
                    nbPaymentError.Text = ex.Message;
                    nbPaymentError.Visible = true;

                }
            }
            else
            {
                nbPaymentError.Heading = "Invalid Payment Amount";
                nbPaymentError.Text = "Payment amount must be greater than zero and less than or equal to the balance due.";
                nbPaymentError.Visible = true;
            }

        }

        #endregion

        #region Registrant Events

        void lbEditRegistrant_Click( object sender, EventArgs e )
        {
            var lb = sender as LinkButton;
            if ( lb != null )
            {
                int? registrantId = lb.ID.Substring( 17 ).AsIntegerOrNull();
                if ( registrantId.HasValue )
                {
                    NavigateToLinkedPage( "RegistrantPage", "RegistrantId", registrantId.Value );
                }
            }
        }

        void lbDeleteRegistrant_Click( object sender, EventArgs e )
        {
            var lb = sender as LinkButton;
            if ( lb != null )
            {
                int? registrantId = lb.ID.Substring( 19 ).AsIntegerOrNull();
                if ( registrantId.HasValue )
                {
                    var rockContext = new RockContext();

                    var registrantService = new RegistrationRegistrantService( rockContext );
                    RegistrationRegistrant registrant = registrantService.Get( registrantId.Value );

                    if ( registrant != null )
                    {
                        if ( !registrant.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) )
                        {
                            mdDeleteWarning.Show( "You are not authorized to delete this registrant.", ModalAlertType.Information );
                            return;
                        }

                        string errorMessage;
                        if ( !registrantService.CanDelete( registrant, out errorMessage ) )
                        {
                            mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                            return;
                        }

                        registrantService.Delete( registrant );

                        rockContext.SaveChanges();
                    }

                    // Reload registration
                    ShowReadonlyDetails( GetRegistration( RegistrationId ) );
                }
            }
        }

        protected void lbAddRegistrant_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "RegistrantPage", "RegistrantId", 0, "RegistrationId", RegistrationId );
        }

        #endregion

        #region Methods

        #region Load/Save Methods

        private void LoadState()
        {
            if ( !RegistrationInstanceId.HasValue )
            {
                Title = "New Registration";
                RegistrationInstanceId = PageParameter( "RegistrationInstanceId" ).AsIntegerOrNull();
                RegistrationId = PageParameter( "RegistrationId" ).AsIntegerOrNull();

                var rockContext = new RockContext();

                if ( RegistrationId.HasValue )
                {
                    Registration = GetRegistration( RegistrationId.Value, rockContext );
                    if ( Registration != null )
                    {
                        Title = Registration.ToString();
                        RegistrationInstanceId = Registration.RegistrationInstanceId;
                        RegistrationTemplateState = Registration.RegistrationInstance.RegistrationTemplate;
                        lWizardTemplateName.Text = Registration.RegistrationInstance.RegistrationTemplate.Name;
                        lWizardInstanceName.Text = Registration.RegistrationInstance.Name;
                        lWizardRegistrationName.Text = Registration.ToString();
                    }
                }

                EditAllowed = IsUserAuthorized( Authorization.EDIT ) || ( Registration != null && Registration.IsAuthorized( Authorization.EDIT, CurrentPerson ) );

                if ( RegistrationTemplateState == null && RegistrationInstanceId.HasValue )
                {
                    var registrationInstance = new RegistrationInstanceService( rockContext )
                        .Queryable( "RegistrationTemplate" ).AsNoTracking()
                        .Where( i => i.Id == RegistrationInstanceId.Value )
                        .FirstOrDefault();
                    if ( registrationInstance != null )
                    {
                        lWizardTemplateName.Text = registrationInstance.RegistrationTemplate.Name;
                        lWizardInstanceName.Text = registrationInstance.Name;
                        lWizardRegistrationName.Text = "New Registration";
                        RegistrationTemplateState = registrationInstance.RegistrationTemplate;
                        EditAllowed = EditAllowed || registrationInstance.RegistrationTemplate.IsAuthorized( Authorization.EDIT, CurrentPerson );
                    }
                }
            }
        }

        /// <summary>
        /// Gets the group.
        /// </summary>
        /// <param name="registrationId">The group identifier.</param>
        /// <returns></returns>
        private Registration GetRegistration( int? registrationId, RockContext rockContext = null )
        {
            if ( registrationId.HasValue && registrationId.Value != 0 )
            {
                rockContext = rockContext ?? new RockContext();

                var registration = new RegistrationService( rockContext )
                    .Queryable( "RegistrationInstance.RegistrationTemplate.Forms.Fields,PersonAlias.Person,Group,Registrants.Fees" ).AsNoTracking()
                    .Where( r => r.Id == registrationId.Value )
                    .FirstOrDefault();

                return registration;
            }

            return null;
        }

        #endregion

        #region Display Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="registrationId">The registration identifier.</param>
        public void ShowDetail( int registrationId )
        {
            ShowDetail( registrationId, null );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="registrationId">The registration identifier.</param>
        /// <param name="registrationInstanceId">The registration instance identifier.</param>
        public void ShowDetail( int registrationId, int? registrationInstanceId )
        {
            RockContext rockContext = new RockContext();

            if ( Registration == null && !registrationId.Equals( 0 ) )
            {
                Registration = GetRegistration( registrationId, rockContext );
            }

            if ( Registration == null && registrationInstanceId.HasValue )
            {
                Registration = new Registration { Id = 0, RegistrationInstanceId = registrationInstanceId ?? 0 };
            }

            if ( Registration != null )
            {
                RegistrationInstanceId = Registration.RegistrationInstanceId;

                // render UI based on Authorized and IsSystem
                bool readOnly = false;

                nbEditModeMessage.Text = string.Empty;
                if ( !EditAllowed )
                {
                    readOnly = true;
                    nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( Group.FriendlyTypeName );
                }

                if ( readOnly )
                {
                    btnEdit.Visible = false;
                    btnDelete.Visible = false;
                    ShowReadonlyDetails( Registration );
                }
                else
                {
                    btnEdit.Visible = true;
                    btnDelete.Visible = true;
                    if ( Registration.Id > 0 )
                    {
                        ShowReadonlyDetails( Registration );
                    }
                    else
                    {
                        ShowEditDetails( Registration );
                    }
                }
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="registration">The group.</param>
        private void ShowEditDetails( Registration registration )
        {
            SetCostLabels( registration );
            SetEditMode( true );

            if ( registration.PersonAlias != null )
            {
                ppPerson.SetValue( registration.PersonAlias.Person );
            }
            else
            {
                ppPerson.SetValue( null );
            }

            tbFirstName.Text = registration.FirstName;
            tbLastName.Text = registration.LastName;
            ebConfirmationEmail.Text = registration.ConfirmationEmail;

            var discountCodes = new Dictionary<string, string>();
            if ( RegistrationTemplateState != null )
            {
                foreach( var discount in RegistrationTemplateState.Discounts.OrderBy( d => d.Code ) )
                {
                    discountCodes.AddOrIgnore( discount.Code, discount.Code + ( string.IsNullOrWhiteSpace( discount.DiscountString ) ? "" :
                        string.Format( " ({0})", discount.DiscountString ) ) );
                }
            }

            if ( !string.IsNullOrWhiteSpace( registration.DiscountCode ))
            {
                discountCodes.AddOrIgnore( registration.DiscountCode, registration.DiscountCode );
            }

            ddlDiscountCode.DataSource = discountCodes;
            ddlDiscountCode.DataBind();
            ddlDiscountCode.Items.Insert( 0, new ListItem( "", "" ) );
            ddlDiscountCode.SetValue( registration.DiscountCode );

            nbDiscountPercentage.Text = registration.DiscountPercentage != 0.0m ? ( registration.DiscountPercentage * 100.0m ).ToString( "N0" ) : "";
            cbDiscountAmount.Text = registration.DiscountAmount != 0.0m ? registration.DiscountAmount.ToString( "N2" ) : "";

            RegistrantsState = null;
            BuildRegistrationControls( true );

            lbAddRegistrant.Visible = false;
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="registration">The group.</param>
        private void ShowReadonlyDetails( Registration registration )
        {
            SetCostLabels( registration );
            SetEditMode( false );

            var rockContext = new RockContext();

            if ( registration.PersonAlias != null && registration.PersonAlias.Person != null )
            {
                lName.Text = registration.PersonAlias.Person.GetAnchorTag( ResolveRockUrl( "/" ) );
            }
            else
            {
                lName.Text = string.Format( "{0} {1}", registration.FirstName, registration.LastName );
            }

            lConfirmationEmail.Text = registration.ConfirmationEmail;
            lConfirmationEmail.Visible = !string.IsNullOrWhiteSpace( registration.ConfirmationEmail );
            lbResendConfirmation.Visible = lConfirmationEmail.Visible;

            if ( registration.Group != null )
            {
                var qryParams = new Dictionary<string, string>();
                qryParams.Add( "GroupId", registration.Group.Id.ToString() );
                string groupUrl = LinkedPageUrl( "GroupDetailPage", qryParams );

                lGroup.Text = string.Format( "<a href='{0}'>{1}</a>", groupUrl, registration.Group.Name );
                lGroup.Visible = true;
            }
            else
            {
                lGroup.Visible = false;
            }

            lDiscountCode.Visible = !string.IsNullOrWhiteSpace( registration.DiscountCode );
            lDiscountCode.Text = registration.DiscountCode;

            lDiscountPercent.Visible = registration.DiscountPercentage > 0.0m;
            lDiscountPercent.Text = registration.DiscountPercentage.ToString("P0");

            lDiscountAmount.Visible = registration.DiscountAmount > 0.0m;
            lDiscountAmount.Text = registration.DiscountAmount.ToString( "C2" );

            RegistrantsState = new List<RegistrantInfo>();
            registration.Registrants.ToList().ForEach( r => RegistrantsState.Add( new RegistrantInfo( r, rockContext ) ) );

            PercentageDiscountExists = registration.DiscountPercentage > 0.0m;
            BuildFeeTable( registration );
            BuildRegistrationControls( true );

            bool anyPayments = registration.Payments.Any();
            hfHasPayments.Value = anyPayments.ToString();
            foreach ( RockWeb.Blocks.Finance.TransactionList block in RockPage.RockBlocks.Where( a => a is RockWeb.Blocks.Finance.TransactionList ) )
            {
                block.SetVisible( anyPayments );
            }

            lbAddRegistrant.Visible = EditAllowed;
        }

        /// <summary>
        /// Sets the cost labels.
        /// </summary>
        /// <param name="registration">The registration.</param>
        private void SetCostLabels( Registration registration )
        {
            if ( registration != null && registration.TotalCost > 0.0M )
            {
                hlCost.Visible = true;
                hlCost.Text = registration.DiscountedCost.ToString( "C2" );

                decimal balanceDue = registration.BalanceDue;
                hlBalance.Visible = true;
                hlBalance.Text = balanceDue.ToString( "C2" );
                hlBalance.LabelType = balanceDue > 0 ? LabelType.Danger : 
                    balanceDue < 0 ? LabelType.Warning : LabelType.Success;
            }
            else
            {
                hlCost.Visible = false;
                hlBalance.Visible = false;
            }
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            pnlViewDetails.Visible = !editable;

            this.HideSecondaryBlocks( editable );
        }

        /// <summary>
        /// Registers the client script.
        /// </summary>
        private void RegisterClientScript()
        {
            RockPage.AddScriptLink( ResolveUrl( "~/Scripts/jquery.creditCardTypeDetector.js" ) );

            string script = @"
    // Detect credit card type
    $('.credit-card').creditCardTypeDetector({ 'credit_card_logos': '.card-logos' });
";
            ScriptManager.RegisterStartupScript( Page, Page.GetType(), "registration-detail-card-info", script, true );

            string deleteScript = @"

    $('a.js-delete-registration').click(function( e ){
        e.preventDefault();
        Rock.dialogs.confirm('Are you sure you want to delete this Registration? All of the registrants will also be deleted!', function (result) {
            if (result) {
                if ( $('input.js-has-payments').val() == 'True' ) {
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
            ScriptManager.RegisterStartupScript( btnDelete, btnDelete.GetType(), "deleteRegistrationScript", deleteScript, true );
        }

        #endregion

        #region Payment

        /// <summary>
        /// Processes the payment.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="registration">The registration.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        private bool ProcessPayment( RockContext rockContext, Registration registration, decimal amount, out string errorMessage )
        {
            GatewayComponent gateway = null;
            if ( RegistrationTemplateState != null && RegistrationTemplateState.FinancialGateway != null )
            {
                gateway = RegistrationTemplateState.FinancialGateway.GetGatewayComponent();
            }

            if ( gateway == null )
            {
                errorMessage = "There was a problem creating the payment gateway information";
                return false;
            }

            if ( registration == null || registration.RegistrationInstance == null || registration.RegistrationInstance.Account == null )
            {
                errorMessage = "There was a problem with the account configuration for this registration.";
                return false;
            }

            var paymentInfo = new CreditCardPaymentInfo( txtCreditCard.Text, txtCVV.Text, mypExpiration.SelectedDate.Value );
            paymentInfo.NameOnCard = gateway != null && gateway.SplitNameOnCard ? txtCardFirstName.Text : txtCardName.Text;
            paymentInfo.LastNameOnCard = txtCardLastName.Text;

            paymentInfo.BillingStreet1 = acBillingAddress.Street1;
            paymentInfo.BillingStreet2 = acBillingAddress.Street2;
            paymentInfo.BillingCity = acBillingAddress.City;
            paymentInfo.BillingState = acBillingAddress.State;
            paymentInfo.BillingPostalCode = acBillingAddress.PostalCode;
            paymentInfo.BillingCountry = acBillingAddress.Country;

            paymentInfo.Amount = amount;
            paymentInfo.Email = registration.ConfirmationEmail;

            paymentInfo.FirstName = registration.FirstName;
            paymentInfo.LastName = registration.LastName;

            var transaction = gateway.Charge( RegistrationTemplateState.FinancialGateway, paymentInfo, out errorMessage );
            if ( transaction != null )
            {
                var txnChanges = new List<string>();
                txnChanges.Add( "Created Transaction" );

                History.EvaluateChange( txnChanges, "Transaction Code", string.Empty, transaction.TransactionCode );

                transaction.AuthorizedPersonAliasId = registration.PersonAliasId;

                transaction.TransactionDateTime = RockDateTime.Now;
                History.EvaluateChange( txnChanges, "Date/Time", null, transaction.TransactionDateTime );

                transaction.FinancialGatewayId = RegistrationTemplateState.FinancialGatewayId;
                History.EvaluateChange( txnChanges, "Gateway", string.Empty, RegistrationTemplateState.FinancialGateway.Name );

                var txnType = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_EVENT_REGISTRATION ) );
                transaction.TransactionTypeValueId = txnType.Id;
                History.EvaluateChange( txnChanges, "Type", string.Empty, txnType.Value );

                transaction.CurrencyTypeValueId = paymentInfo.CurrencyTypeValue.Id;
                History.EvaluateChange( txnChanges, "Currency Type", string.Empty, paymentInfo.CurrencyTypeValue.Value );

                transaction.CreditCardTypeValueId = paymentInfo.CreditCardTypeValue != null ? paymentInfo.CreditCardTypeValue.Id : (int?)null;
                if ( transaction.CreditCardTypeValueId.HasValue )
                {
                    var ccType = DefinedValueCache.Read( transaction.CreditCardTypeValueId.Value );
                    History.EvaluateChange( txnChanges, "Credit Card Type", string.Empty, ccType.Value );
                }

                Guid sourceGuid = Guid.Empty;
                if ( Guid.TryParse( GetAttributeValue( "Source" ), out sourceGuid ) )
                {
                    var source = DefinedValueCache.Read( sourceGuid );
                    if ( source != null )
                    {
                        transaction.SourceTypeValueId = source.Id;
                        History.EvaluateChange( txnChanges, "Source", string.Empty, source.Value );
                    }
                }

                transaction.Summary = Registration.GetSummary();

                var transactionDetail = new FinancialTransactionDetail();
                transactionDetail.Amount = amount;
                transactionDetail.AccountId = registration.RegistrationInstance.AccountId;
                transactionDetail.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Registration ) ).Id;
                transactionDetail.EntityId = registration.Id;
                transaction.TransactionDetails.Add( transactionDetail );

                History.EvaluateChange( txnChanges, registration.RegistrationInstance.Account.Name, 0.0M.ToString( "C2" ), transactionDetail.Amount.ToString( "C2" ) );

                var batchService = new FinancialBatchService( rockContext );

                // Get the batch
                var batch = batchService.Get(
                    GetAttributeValue( "BatchNamePrefix" ),
                    paymentInfo.CurrencyTypeValue,
                    paymentInfo.CreditCardTypeValue,
                    transaction.TransactionDateTime.Value,
                    RegistrationTemplateState.FinancialGateway.GetBatchTimeOffset() );

                var batchChanges = new List<string>();

                if ( batch.Id == 0 )
                {
                    batchChanges.Add( "Generated the batch" );
                    History.EvaluateChange( batchChanges, "Batch Name", string.Empty, batch.Name );
                    History.EvaluateChange( batchChanges, "Status", null, batch.Status );
                    History.EvaluateChange( batchChanges, "Start Date/Time", null, batch.BatchStartDateTime );
                    History.EvaluateChange( batchChanges, "End Date/Time", null, batch.BatchEndDateTime );
                }

                decimal newControlAmount = batch.ControlAmount + transaction.TotalAmount;
                History.EvaluateChange( batchChanges, "Control Amount", batch.ControlAmount.ToString( "C2" ), newControlAmount.ToString( "C2" ) );
                batch.ControlAmount = newControlAmount;

                transaction.BatchId = batch.Id;
                batch.Transactions.Add( transaction );

                rockContext.SaveChanges();

                HistoryService.SaveChanges(
                    rockContext,
                    typeof( FinancialBatch ),
                    Rock.SystemGuid.Category.HISTORY_FINANCIAL_BATCH.AsGuid(),
                    batch.Id,
                    batchChanges
                );

                HistoryService.SaveChanges(
                    rockContext,
                    typeof( FinancialBatch ),
                    Rock.SystemGuid.Category.HISTORY_FINANCIAL_TRANSACTION.AsGuid(),
                    batch.Id,
                    txnChanges,
                    CurrentPerson != null ? CurrentPerson.FullName : string.Empty,
                    typeof( FinancialTransaction ),
                    transaction.Id
                );

                return true;
            }
            else
            {
                return false;
            }

        }

        #endregion

        #region Dynamic Controls

        private void BuildFeeTable( Registration registration )
        {
            // Get the cost/fee summary
            var costs = new List<RegistrationCostSummaryInfo>();
            foreach ( var registrant in RegistrantsState )
            {
                if ( registrant.Cost > 0 )
                {
                    var costSummary = new RegistrationCostSummaryInfo();
                    costSummary.Type = RegistrationCostSummaryType.Cost;
                    costSummary.Description = registrant.PersonName;
                    costSummary.Cost = registrant.Cost;
                    if ( registration.DiscountPercentage > 0.0m )
                    {
                        costSummary.DiscountedCost = costSummary.Cost - ( costSummary.Cost * registration.DiscountPercentage );
                    }
                    else
                    {
                        costSummary.DiscountedCost = costSummary.Cost;
                    }

                    costs.Add( costSummary );
                }

                foreach ( var fee in registrant.FeeValues )
                {
                    var templateFee = RegistrationTemplateState.Fees.Where( f => f.Id == fee.Key ).FirstOrDefault();
                    if ( fee.Value != null )
                    {
                        foreach ( var feeInfo in fee.Value )
                        {
                            decimal cost = feeInfo.PreviousCost > 0.0m ? feeInfo.PreviousCost : feeInfo.Cost;
                            string desc = string.Format( "{0}{1} ({2:N0} @ {3:C2})",
                                templateFee != null ? templateFee.Name : "(Previous Cost)",
                                string.IsNullOrWhiteSpace( feeInfo.Option ) ? "" : "-" + feeInfo.Option,
                                feeInfo.Quantity,
                                cost );

                            var costSummary = new RegistrationCostSummaryInfo();
                            costSummary.Type = RegistrationCostSummaryType.Fee;
                            costSummary.Description = desc;
                            costSummary.Cost = feeInfo.Quantity * cost;

                            if ( registration.DiscountPercentage > 0.0m && templateFee != null && templateFee.DiscountApplies )
                            {
                                costSummary.DiscountedCost = costSummary.Cost - ( costSummary.Cost * registration.DiscountPercentage );
                            }
                            else
                            {
                                costSummary.DiscountedCost = costSummary.Cost;
                            }

                            costs.Add( costSummary );
                        }
                    }
                }
            }

            // If there were any costs
            if ( costs.Any() )
            {
                pnlCosts.Visible = true;

                // Get the total min payment for all costs and fees
                decimal minPayment = costs.Sum( c => c.MinPayment );

                // Add row for amount discount
                if ( registration.DiscountAmount > 0.0m )
                {
                    decimal totalDiscount = 0.0m - ( RegistrantsState.Count * registration.DiscountAmount );
                    costs.Add( new RegistrationCostSummaryInfo
                    {
                        Type = RegistrationCostSummaryType.Discount,
                        Description = "Discount",
                        Cost = totalDiscount,
                        DiscountedCost = totalDiscount
                    } );
                }

                // Get the totals

                // Add row for totals
                costs.Add( new RegistrationCostSummaryInfo
                {
                    Type = RegistrationCostSummaryType.Total,
                    Description = "Total",
                    Cost = costs.Sum( c => c.Cost ),
                    DiscountedCost = registration.DiscountedCost,
                } );

                rptFeeSummary.DataSource = costs;
                rptFeeSummary.DataBind();

                // Set the totals
                decimal balanceDue = registration.BalanceDue;
                lTotalCost.Text = registration.DiscountedCost.ToString( "C2" );
                lPreviouslyPaid.Text = registration.TotalPaid.ToString( "C2" );
                lRemainingDue.Text = balanceDue.ToString( "C2" );

                lbAddPayment.Visible = ( balanceDue > 0.0m &&
                    Registration != null &&
                    Registration.PersonAliasId.HasValue &&
                    RegistrationTemplateState != null &&
                    RegistrationTemplateState.FinancialGateway != null );
            }
            else
            {
                pnlCosts.Visible = false;
            }
        }

        private void BuildRegistrationControls( bool setValues )
        {
            phDynamicControls.Controls.Clear();
            if ( RegistrantsState != null )
            {
                foreach ( var registrant in RegistrantsState )
                {
                    BuildRegistrantControls( registrant, setValues );
                }
            }
        }

        private void BuildRegistrantControls( RegistrantInfo registrant, bool setValues )
        {
            var anchor = new HtmlAnchor();
            anchor.Name = registrant.Id.ToString();
            phDynamicControls.Controls.Add( anchor );

            var divPanel = new HtmlGenericControl( "div" );
            divPanel.AddCssClass( "panel" );
            divPanel.AddCssClass( "panel-block" );
            phDynamicControls.Controls.Add( divPanel );

            var divHeading = new HtmlGenericControl( "div" );
            divHeading.AddCssClass( "panel-heading" );
            divHeading.AddCssClass( "clearfix" );
            divPanel.Controls.Add( divHeading );

            var h1Heading = new HtmlGenericControl( "h1" );
            h1Heading.AddCssClass( "panel-title" );
            h1Heading.AddCssClass( "pull-left" );
            h1Heading.InnerText = registrant.PersonName;
            divHeading.Controls.Add( h1Heading );

            var divLabels = new HtmlGenericControl( "div" );
            divLabels.AddCssClass( "panel-labels" );
            divHeading.Controls.Add( divLabels );

            decimal registrantCost = registrant.TotalCost;
            if ( registrantCost != 0.0m )
            {
                var hlCost = new HighlightLabel();
                hlCost.ID = string.Format( "hlCost_{0}", registrant.Id );
                hlCost.LabelType = LabelType.Info;
                hlCost.ToolTip = "Cost";
                hlCost.Text = registrantCost.ToString( "C2" );
                divLabels.Controls.Add( hlCost );
            }

            var divBody = new HtmlGenericControl( "div" );
            divBody.AddCssClass( "panel-body" );
            divPanel.Controls.Add( divBody );

            var divRow = new HtmlGenericControl( "div" );
            divRow.AddCssClass( "row" );
            divBody.Controls.Add( divRow );

            var divFields = new HtmlGenericControl( "div" );
            divFields.AddCssClass( "col-md-6");
            divRow.Controls.Add( divFields );

            var divFees = new HtmlGenericControl( "div" );
            divFees.AddCssClass( "col-md-6");
            divRow.Controls.Add( divFees );

            foreach( var form in RegistrationTemplateState.Forms )
            {
                foreach( var field in form.Fields )
                {
                    var fieldControl = BuildRegistrantFieldControl( field, registrant, setValues );
                    if ( fieldControl != null )
                    {
                        divFields.Controls.Add( fieldControl );
                    }
                }
            }

            if ( registrant.Cost > 0.0m)
            {
                var rlCost = new RockLiteral();
                rlCost.ID = string.Format( "rlCost_{0}", registrant.Id );
                rlCost.Label = "Cost";
                rlCost.Text = registrant.Cost.ToString( "C2" );
                divFees.Controls.Add( rlCost );
            }

            foreach ( var fee in registrant.FeeValues )
            {
                var templateFee = RegistrationTemplateState.Fees.Where( f => f.Id == fee.Key ).FirstOrDefault();
                if ( templateFee != null && fee.Value != null )
                {
                    foreach ( var feeInfo in fee.Value )
                    {
                        var feeControl = BuildRegistrantFeeControl( templateFee, feeInfo, registrant, setValues );
                        if ( feeControl != null )
                        {
                            divFees.Controls.Add( feeControl );
                        }
                    }
                }
            }


            var divActions = new HtmlGenericControl( "Div" );
            divActions.AddCssClass( "actions" );
            divBody.Controls.Add( divActions );

            var lbEditRegistrant = new LinkButton();
            lbEditRegistrant.Visible = EditAllowed;
            lbEditRegistrant.CausesValidation = false;
            lbEditRegistrant.ID = string.Format( "lbEditRegistrant_{0}", registrant.Id );
            lbEditRegistrant.Text = "Edit";
            lbEditRegistrant.CssClass = "btn btn-primary";
            lbEditRegistrant.Click += lbEditRegistrant_Click;
            divActions.Controls.Add( lbEditRegistrant );

            var lbDeleteRegistrant = new LinkButton();
            lbDeleteRegistrant.Visible = EditAllowed;
            lbDeleteRegistrant.CausesValidation = false;
            lbDeleteRegistrant.ID = string.Format( "lbDeleteRegistrant_{0}", registrant.Id );
            lbDeleteRegistrant.Text = "Delete";
            lbDeleteRegistrant.CssClass = "btn btn-link";
            lbDeleteRegistrant.Attributes["onclick"] = "javascript: return Rock.dialogs.confirmDelete(event, 'Registrant');";
            lbDeleteRegistrant.Click += lbDeleteRegistrant_Click;
            divActions.Controls.Add( lbDeleteRegistrant );
        }

        private Control BuildRegistrantFeeControl( RegistrationTemplateFee fee, FeeInfo feeInfo, RegistrantInfo registrant, bool setValues )
        {
            if ( feeInfo.Quantity > 0 )
            {
                var rlField = new RockLiteral();
                rlField.ID = string.Format( "rlFee_{0}_{1}_{2}", registrant.Id, fee.Id, feeInfo.Option );
                rlField.Label = fee.Name;

                if ( !string.IsNullOrWhiteSpace( feeInfo.Option ) )
                {
                    rlField.Label += " - " + feeInfo.Option;
                }

                if ( feeInfo.Quantity > 1 )
                {
                    rlField.Text = string.Format( "({0:N0} @ {1:C2}) {2:C2}",
                    feeInfo.Quantity, feeInfo.Cost, feeInfo.TotalCost );
                }
                else
                {
                    rlField.Text = feeInfo.TotalCost.ToString( "C2" );
                }

                return rlField;
            }

            return null;
        }


        private Control BuildRegistrantFieldControl( RegistrationTemplateFormField field, RegistrantInfo registrant, bool setValues )
        {
            // Ignore the first/last name fields since they are displayed in the panel's heading
            if ( field.FieldSource == RegistrationFieldSource.PersonField &&
                ( field.PersonFieldType == RegistrationPersonFieldType.FirstName || field.PersonFieldType == RegistrationPersonFieldType.LastName ) )
            {
                return null;
            }

            object fieldValue = null;
            if ( registrant != null && registrant.FieldValues != null && registrant.FieldValues.ContainsKey( field.Id ) )
            {
                fieldValue = registrant.FieldValues[field.Id];
            }

            var rlField = new RockLiteral();
            rlField.ID = string.Format( "rlField_{0}_{1}", registrant.Id, field.Id );

            if ( fieldValue != null )
            {
                if ( field.FieldSource == RegistrationFieldSource.PersonField )
                {
                    rlField.Label = field.PersonFieldType.ConvertToString( true );

                    switch ( field.PersonFieldType )
                    {
                        case RegistrationPersonFieldType.Campus:
                            {
                                var campus = CampusCache.Read( fieldValue.ToString().AsInteger() );
                                rlField.Text = campus != null ? campus.Name : string.Empty;
                                break;
                            }

                        case RegistrationPersonFieldType.Address:
                            {
                                var location = fieldValue.ToString().FromJsonOrNull<Location>();
                                rlField.Text = location != null ? location.ToString() : string.Empty;
                                break;
                            }

                        case RegistrationPersonFieldType.Email:
                            {
                                rlField.Text = fieldValue.ToString();
                                break;
                            }

                        case RegistrationPersonFieldType.Birthdate:
                            {
                                var birthDate = fieldValue as DateTime?;
                                rlField.Text = birthDate != null ? birthDate.Value.ToShortDateString() : string.Empty;
                                break;
                            }

                        case RegistrationPersonFieldType.Gender:
                            {
                                var gender = fieldValue.ToString().ConvertToEnumOrNull<Gender>() ?? Gender.Unknown;
                                rlField.Text = gender.ConvertToString();
                                break;
                            }

                        case RegistrationPersonFieldType.MaritalStatus:
                            {
                                var maritalStatusDv = DefinedValueCache.Read( fieldValue.ToString().AsInteger() );
                                rlField.Text = maritalStatusDv != null ? maritalStatusDv.Value : string.Empty;
                                break;
                            }

                        case RegistrationPersonFieldType.MobilePhone:
                        case RegistrationPersonFieldType.HomePhone:
                        case RegistrationPersonFieldType.WorkPhone:
                            {
                                var pn = fieldValue as PhoneNumber;
                                rlField.Text = pn != null ? pn.NumberFormatted : string.Empty;
                                break;
                            }
                    }

                }
                else
                {
                    if ( field.AttributeId.HasValue )
                    {
                        var attribute = AttributeCache.Read( field.AttributeId.Value );
                        if ( attribute == null )
                        {
                            return null;
                        }

                        rlField.Label = attribute.Name;
                        rlField.Text = attribute.FieldType.Field.FormatValueAsHtml( null, fieldValue.ToString(), attribute.QualifierValues );
                    }
                }
            }

            return rlField;
        }

        #endregion

        #endregion

}
}