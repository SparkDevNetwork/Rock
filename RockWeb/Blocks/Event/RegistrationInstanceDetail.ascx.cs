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
using Rock.Blocks.Types.Web.Events;

namespace RockWeb.Blocks.Event
{
    /// <summary>
    /// A Block that allows viewing and editing an event registration instance.
    /// </summary>
    [DisplayName( "Registration Instance - Instance Detail" )]
    [Category( "Event" )]
    [Description( "Displays the details of a Registration Instance for viewing and editing." )]

    #region Block Attributes

    [AccountField( "Default Account",
        "The default account to use for new registration instances",
        Key = AttributeKey.DefaultAccount,
        IsRequired = false,
        DefaultValue = Rock.SystemGuid.FinancialAccount.EVENT_REGISTRATION,
        Order = 0 )]

    #endregion Block Attributes

    public partial class RegistrationInstanceDetail : RegistrationInstanceBlock, IDetailBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes.
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The default account for a new Registration Instance.
            /// </summary>
            public const string DefaultAccount = "DefaultAccount";
        }

        #endregion Attribute Keys

        #region Page Parameter Keys

        /// <summary>
        /// Keys to use for Page Parameters.
        /// </summary>
        private static class PageParameterKey
        {
            /// <summary>
            /// The Registration Instance identifier
            /// </summary>
            public const string RegistrationInstanceId = "RegistrationInstanceId";

            /// <summary>
            /// The Registration Template identifier.
            /// </summary>
            public const string RegistrationTemplateId = "RegistrationTemplateId";
        }

        #endregion Page Parameter Keys

        #region Properties and Fields

        /// <summary>
        /// Gets or sets the available registration attributes where IsGridColumn = true
        /// </summary>
        /// <value>
        /// The available attributes.
        /// </value>
        public List<AttributeCache> AvailableRegistrationAttributesForGrid { get; set; }

        /// <summary>
        /// Gets or sets the registrant form fields that were configured as 'Show on Grid' for the registration template
        /// </summary>
        /// <value>
        /// The registrant fields.
        /// </value>
        public List<RegistrantFormField> RegistrantFields { get; set; }

        /// <summary>
        /// Gets or sets the registration template identifier.
        /// </summary>
        /// <value>
        /// The registration template identifier.
        /// </value>
        //protected int? RegistrationTemplateId { get; set; }

        public Guid? PaymentReminderPageGuid { get; set; }

        //public List<AttributeCache> AvailableRegistrationAttributesForGrid { get; set; }

        public string EditModeMessageHeading { get; set; }
        public string EditModeMessageText { get; set; }

        public bool SendPaymentReminderActionIsAvailable { get; set; }
        public bool EditActionIsAvailable { get; set; }
        public bool DeleteActionIsAvailable { get; set; }

        public bool InactiveFlagIsVisible { get; set; }
        public bool AuditDetailIsVisible
        {
            get
            {
                // Audit Detail is only visible in read-only mode.
                return this.IsReadOnly;
            }
        }

        /// <summary>
        /// Is the entity flagged as Inactive?
        /// </summary>
        public bool IsInactive { get; set; }

        /// <summary>
        /// Is this a new record?
        /// </summary>
        public bool IsNew { get; set; }
        //public Person CurrentPerson { get; set; }
        public string RegistrationTitle { get; set; }
        public string TotalCostDescription { get; set; }
        public bool TotalCostIsVisible { get; set; }
        public string MinimumInitialPaymentDescription { get; set; }
        public string DefaultPaymentAmountDescription { get; set; }

        public string AccountName { get; set; }
        public string MaxAttendeesDescription { get; set; }

        public string WorkflowTypeName { get; set; }
        public string StartDateDescription { get; set; }
        public string EndDateDescription { get; set; }
        public string RegistrationDetails { get; set; }

        private RockContext _DataContext = new RockContext();

        /// <summary>
        /// The data context used to load the data for the view model.
        /// </summary>
        public RockContext DataContext
        {
            get
            {
                return _DataContext;
            }
        }

        /// <summary>
        /// The registration template description suitable for display in the Event Registration Wizard navigation bar.
        /// </summary>
        public string WizardTemplateLabel { get; set; }

        /// <summary>
        /// The registration instance description suitable for display in the Event Registration Wizard navigation bar.
        /// </summary>
        public string WizardInstanceLabel { get; set; }

        /// <summary>
        /// Does the block have data to display?
        /// </summary>
        public bool HasData { get; set; }

        /// <summary>
        /// The text displayed in the title bar of the block.
        /// </summary>
        public string BlockTitle { get; set; }

        /// <summary>
        /// Does the current user have Edit permission for the View/Block?
        /// </summary>
        public bool UserHasEditPermissionForView { get; set; }

        /// <summary>
        /// Is the view in read-only mode?
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// The active Registration Instance.
        /// </summary>
        //public RegistrationInstance RegistrationInstance { get; set; }

        /// <summary>
        /// The unique identifier of the active Registration Instance.
        /// </summary>
        //public int? RegistrationInstanceId { get; set; }

        /// <summary>
        /// The unique identifier of the default account for payments associated with this Registration Instance.
        /// </summary>
        public Guid? DefaultAccountGuid { get; set; }

        /// <summary>
        /// Does this registration instance have associated payments?
        /// </summary>
        //public bool HasPayments { get; set; }
        /// <summary>
        /// Does this registration instance have associated fees or costs?
        /// </summary>
        //public bool HasCosts { get; set; }

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

            RegistrantFields = ViewState["RegistrantFields"] as List<RegistrantFormField>;
            //RegistrationTemplateId = ViewState["RegistrationTemplateId"] as int? ?? 0;

            // don't set the values if this is a postback from a grid 'ClearFilter'
            //bool setValues = this.Request.Params["__EVENTTARGET"] == null || !this.Request.Params["__EVENTTARGET"].EndsWith( "_lbClearFilter" );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            //this.ContextManager = new BlockContextManager( string.Format( "SharedItem:Page:{0}:Item:", this.RockPage.PageId ), System.Web.HttpContext.Current.Items );
            this.UserHasEditPermissionForView = this.UserCanEdit;
            this.DefaultAccountGuid = GetAttributeValue( AttributeKey.DefaultAccount ).AsGuidOrNull();

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

            if ( !Page.IsPostBack )
            {
                var registrationInstanceId = PageParameter( PageParameterKey.RegistrationInstanceId ).AsIntegerOrNull();

                this.LoadDetail( registrationInstanceId, null, false );

                BindControlsToViewModel();
            }
            else
            {
                //this.LoadRegistrationInstance( PageParameter( PageParameterKey.RegistrationInstanceId ).AsIntegerOrNull() );

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
            //ViewState["RegistrationTemplateId"] = RegistrationTemplateId;
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
            //
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
            //using ( var rockContext = new RockContext() )
            //{
            //var registrationInstance = new RegistrationInstanceService( rockContext ).Get( hfRegistrationInstanceId.Value.AsInteger() );

            this.LoadDetail( hfRegistrationInstanceId.Value.AsInteger(), null, true );

            BindControlsToViewModel();
            //BindControlsToViewModel();
            //ShowEditDetails( registrationInstance, rockContext );
            //}
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

                        var qryParams = new Dictionary<string, string> { { PageParameterKey.RegistrationTemplateId, registrationTemplateId.ToString() } };
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
        /// Handles the Click event of the btnSendPaymentReminder control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSendPaymentReminder_Click( object sender, EventArgs e )
        {
            Dictionary<string, string> queryParms = new Dictionary<string, string>();
            queryParms.Add( PageParameterKey.RegistrationInstanceId, PageParameter( PageParameterKey.RegistrationInstanceId ) );
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
                    instance.RegistrationTemplateId = PageParameter( PageParameterKey.RegistrationTemplateId ).AsInteger();
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
                qryParams.Add( PageParameterKey.RegistrationTemplateId, PageParameter( PageParameterKey.RegistrationTemplateId ) );
                qryParams.Add( PageParameterKey.RegistrationInstanceId, instance.Id.ToString() );
                NavigateToCurrentPage( qryParams );
            }
            else
            {
                // Reload instance and show readonly view
                this.LoadDetail( instance.Id, null, false );

                BindControlsToViewModel();
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

                int? parentTemplateId = PageParameter( PageParameterKey.RegistrationTemplateId ).AsIntegerOrNull();
                if ( parentTemplateId.HasValue )
                {
                    qryParams[PageParameterKey.RegistrationTemplateId] = parentTemplateId.ToString();
                }

                // Cancelling on Add.  Return to Grid
                NavigateToParentPage( qryParams );
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                this.LoadDetail( hfRegistrationInstanceId.ValueAsInt(), null, false );

                BindControlsToViewModel();
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
                    qryParams.Add( PageParameterKey.RegistrationTemplateId, registrationInstance.RegistrationTemplateId.ToString() );
                }
            }

            NavigateToParentPage( qryParams );
        }

        #endregion

        #endregion

        #region Main Form Methods

        public void ShowDetail( int itemId )
        {
            var registrationInstanceId = PageParameter( PageParameterKey.RegistrationInstanceId ).AsIntegerOrNull();

            this.LoadDetail( itemId, null, false );

            BindControlsToViewModel();
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        private void BindControlsToViewModel()
        {
            pnlDetails.Visible = this.HasData;
            pnlNoData.Visible = !this.HasData;

            lBlockTitle.Text = this.BlockTitle;

            lWizardTemplateName.Text = this.WizardTemplateLabel;
            lWizardInstanceName.Text = this.WizardInstanceLabel;

            if ( !this.HasData )
            {
                return;
            }

            hlType.Visible = this.RegistrationTemplateId.HasValue;
            hlType.Text = this.WizardTemplateLabel;

            hfRegistrationInstanceId.Value = this.RegistrationInstanceId.ToString();
            hfRegistrationTemplateId.Value = this.RegistrationTemplateId.ToString();

            //RegistrationTemplateId = this.RegistrationTemplateId;

            // Set payment flag in hidden field to be used by client-side script.
            hfHasPayments.Value = this.RegistrationInstanceHasPayments.ToString();

            FollowingsHelper.SetFollowing( this.RegistrationInstance, pnlFollowing, this.CurrentPerson );

            nbEditModeMessage.Heading = this.EditModeMessageHeading;
            nbEditModeMessage.Text = this.EditModeMessageText;

            btnEdit.Visible = this.EditActionIsAvailable;
            btnDelete.Visible = this.DeleteActionIsAvailable;

            // Read-only
            hlInactive.Visible = this.InactiveFlagIsVisible;
            lWizardInstanceName.Text = this.WizardInstanceLabel;
            lReadOnlyTitle.Text = this.BlockTitle;

            pnlEditDetails.Visible = !this.IsReadOnly;
            fieldsetViewDetails.Visible = this.IsReadOnly;

            if ( !this.IsReadOnly )
            {
                rieDetails.SetValue( this.RegistrationInstance );
            }

            pdAuditDetails.Visible = this.AuditDetailIsVisible;

            if ( this.AuditDetailIsVisible )
            {
                pdAuditDetails.SetEntity( this.RegistrationInstance, ResolveRockUrl( "~" ) );
            }

            hfRegistrationInstanceId.SetValue( this.RegistrationInstanceId.GetValueOrDefault() );

            if ( this.IsReadOnly )
            {
                BindControlsToViewModel2();
            }

            btnSendPaymentReminder.Visible = this.SendPaymentReminderActionIsAvailable;
        }

        /// <summary>
        /// Sets the following on postback.
        /// </summary>
        private void SetFollowingOnPostback()
        {
            if ( this.RegistrationInstance != null )
            {
                FollowingsHelper.SetFollowing( this.RegistrationInstance, pnlFollowing, this.CurrentPerson );
            }
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="registrationInstance">The registration template.</param>
        /// <param name="setTab">if set to <c>true</c> [set tab].</param>
        private void BindControlsToViewModel2()
        {
            var registrationInstance = this.RegistrationInstance;

            if ( registrationInstance == null )
            {
                return;
            }

            lName.Text = registrationInstance.Name;

            if ( registrationInstance.RegistrationTemplate != null
                 && registrationInstance.RegistrationTemplate.SetCostOnInstance.GetValueOrDefault() )
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
        }

        /// <summary>
        /// Load the view detail properties using the supplied parameters.
        /// </summary>
        public void LoadDetail( int? registrationInstanceId, int? parentTemplateId, bool isEditMode )
        {
            // Set default values.
            this.HasData = false;
            this.WizardInstanceLabel = "Instance";
            this.WizardTemplateLabel = "Template";

            this.BlockTitle = string.Format( "{0} Detail", RegistrationInstance.FriendlyTypeName ).FormatAsHtmlTitle();

            // If no parameter values are specified, exit with error.
            if ( !registrationInstanceId.HasValue
                 && !parentTemplateId.HasValue )
            {
                return;
            }

            //this.LoadRegistrationInstance( registrationInstanceId, parentTemplateId );

            var registrationInstance = this.RegistrationInstance;

            // If the specified registration instance cannot be loaded, exit with error.
            if ( registrationInstance == null )
            {
                return;
            }

            this.IsNew = ( registrationInstance.Id != 0 );

            this.AvailableRegistrationAttributesForGrid = new List<AttributeCache>();

            int entityTypeId = new Registration().TypeId;

            foreach ( var attributeCache in new AttributeService( this.DataContext ).GetByEntityTypeQualifier( entityTypeId, "RegistrationTemplateId", registrationInstance.RegistrationTemplateId.ToString(), false )
                .Where( a => a.IsGridColumn )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToAttributeCacheList() )
            {
                AvailableRegistrationAttributesForGrid.Add( attributeCache );
            }

            this.WizardInstanceLabel = registrationInstance.Name;
            this.WizardTemplateLabel = registrationInstance.RegistrationTemplate != null ? registrationInstance.RegistrationTemplate.Name : string.Empty;

            this.HasData = true;

            //this.RegistrationInstanceId = registrationInstance.Id;
            //this.RegistrationTemplateId = registrationInstance.RegistrationTemplateId;

            bool canEdit = this.UserHasEditPermissionForView ||
                registrationInstance.IsAuthorized( Authorization.EDIT, CurrentPerson ) ||
                registrationInstance.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );

            this.EditModeMessageText = string.Empty;

            // User must have 'Edit' rights to block, or 'Edit' or 'Administrate' rights to instance
            if ( !canEdit )
            {
                this.EditModeMessageHeading = "Information";
                this.EditModeMessageText = EditModeMessage.NotAuthorizedToEdit( RegistrationInstance.FriendlyTypeName );
            }

            // Render as read-only if requested to do so or if the user does not have edit permission for the block or current item.
            var readOnly = !canEdit || !isEditMode;

            this.EditActionIsAvailable = readOnly && canEdit;
            this.DeleteActionIsAvailable = readOnly && canEdit;

            this.IsReadOnly = readOnly;

            if ( !readOnly && ( isEditMode || registrationInstance.Id == 0 ) )
            {
                ShowEditDetails( registrationInstance );
            }
            else
            {
                PopulateReadonlyDetails( registrationInstance );
            }
        }

        /// <summary>
        /// Populates the edit fields for the block.
        /// </summary>
        /// <param name="RegistrationTemplate">The registration template.</param>
        /// <param name="rockContext">The rock context.</param>
        private void ShowEditDetails( RegistrationInstance instance )
        {
            this.IsReadOnly = false;

            if ( instance.Id == 0 )
            {
                this.BlockTitle = ActionTitle.Add( RegistrationInstance.FriendlyTypeName ).FormatAsHtmlTitle();
                this.WizardInstanceLabel = "New Instance";
            }
            else
            {
                this.BlockTitle = instance.Name;
                this.WizardInstanceLabel = instance.Name;
            }
        }

        /// <summary>
        /// Populates the readonly details for the block.
        /// </summary>
        /// <param name="registrationInstance">The registration template.</param>
        /// <param name="setTab">if set to <c>true</c> [set tab].</param>
        private void PopulateReadonlyDetails( RegistrationInstance registrationInstance )
        {
            this.BlockTitle = registrationInstance.Name.FormatAsHtmlTitle();

            this.InactiveFlagIsVisible = !registrationInstance.IsActive;

            if ( registrationInstance.RegistrationTemplate != null
                 && registrationInstance.RegistrationTemplate.SetCostOnInstance.GetValueOrDefault() )
            {
                this.TotalCostDescription = registrationInstance.Cost.FormatAsCurrency();
                this.TotalCostIsVisible = true;

                this.MinimumInitialPaymentDescription = registrationInstance.MinimumInitialPayment.HasValue ? registrationInstance.MinimumInitialPayment.Value.FormatAsCurrency() : string.Empty;
                this.DefaultPaymentAmountDescription = registrationInstance.DefaultPayment.HasValue ? registrationInstance.DefaultPayment.Value.FormatAsCurrency() : string.Empty;
            }

            this.AccountName = registrationInstance.Account != null ? registrationInstance.Account.Name : string.Empty;

            this.MaxAttendeesDescription = registrationInstance.MaxAttendees >= 0 ?
                    registrationInstance.MaxAttendees.Value.ToString( "N0" ) :
                    string.Empty;
            this.WorkflowTypeName = registrationInstance.RegistrationWorkflowType != null ?
                registrationInstance.RegistrationWorkflowType.Name : string.Empty;


            this.StartDateDescription = registrationInstance.StartDateTime.HasValue ?
                registrationInstance.StartDateTime.Value.ToShortDateString() : string.Empty;

            this.EndDateDescription = registrationInstance.EndDateTime.HasValue ?
                registrationInstance.EndDateTime.Value.ToShortDateString() : string.Empty;

            this.RegistrationDetails = registrationInstance.Details;
        }

        /// <summary>
        /// Gets the registration instance.
        /// </summary>
        /// <param name="registrationInstanceId">The registration instance identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        /// <summary>
        //private RegistrationInstance GetRegistrationInstance( int registrationInstanceId, RockContext rockContext = null )
        //{
        //    string key = string.Format( "RegistrationInstance:{0}", registrationInstanceId );
        //    RegistrationInstance registrationInstance = RockPage.GetSharedItem( key ) as RegistrationInstance;
        //    if ( registrationInstance == null )
        //    {
        //        rockContext = rockContext ?? new RockContext();
        //        registrationInstance = new RegistrationInstanceService( rockContext )
        //            .Queryable( "RegistrationTemplate,Account,RegistrationTemplate.Forms.Fields" )
        //            .AsNoTracking()
        //            .FirstOrDefault( i => i.Id == registrationInstanceId );
        //        RockPage.SaveSharedItem( key, registrationInstance );
        //    }

        //    return registrationInstance;
        //}

        /// <summary>
        /// Load the registration instance data, but do not populate the display properties.
        /// Use this method to load data for postback processing.
        /// </summary>
        /// <param name="registrationInstanceId"></param>
        /// <param name="parentTemplateId"></param>
        //public void LoadRegistrationInstance( int? registrationInstanceId, int? parentTemplateId = null )
        //{
        //    RegistrationInstance registrationInstance = null;

        //    if ( registrationInstanceId.HasValue )
        //    {
        //        registrationInstance = GetRegistrationInstance( registrationInstanceId.Value );
        //    }

        //    if ( registrationInstance == null )
        //    {
        //        registrationInstance = new RegistrationInstance();
        //        registrationInstance.Id = 0;
        //        registrationInstance.IsActive = true;
        //        registrationInstance.RegistrationTemplateId = parentTemplateId ?? 0;

        //        if ( this.DefaultAccountGuid.HasValue )
        //        {
        //            var account = new FinancialAccountService( _DataContext ).Get( this.DefaultAccountGuid.Value );
        //            registrationInstance.AccountId = account != null ? account.Id : 0;
        //        }
        //    }

        //    if ( registrationInstance.RegistrationTemplate == null
        //         && registrationInstance.RegistrationTemplateId > 0 )
        //    {
        //        registrationInstance.RegistrationTemplate = new RegistrationTemplateService( _DataContext )
        //            .Get( registrationInstance.RegistrationTemplateId );
        //    }

        //    // Check if this Registration Instance has associated costs or fees.
        //    this.HasCosts = ( registrationInstance.RegistrationTemplate != null
        //        && ( ( registrationInstance.RegistrationTemplate.SetCostOnInstance.HasValue && registrationInstance.RegistrationTemplate.SetCostOnInstance == true && registrationInstance.Cost.HasValue && registrationInstance.Cost.Value > 0 )
        //        || registrationInstance.RegistrationTemplate.Cost > 0
        //        || registrationInstance.RegistrationTemplate.Fees.Count > 0 ) );

        //    // Check if this Registration Instance has associated payments.
        //    var registrationIdQry = new RegistrationService( _DataContext )
        //        .Queryable().AsNoTracking()
        //        .Where( r =>
        //            r.RegistrationInstanceId == registrationInstanceId &&
        //            !r.IsTemporary )
        //        .Select( r => r.Id );

        //    var registrationEntityType = EntityTypeCache.Get( typeof( Rock.Model.Registration ) );

        //    this.HasPayments = new FinancialTransactionDetailService( _DataContext )
        //        .Queryable().AsNoTracking()
        //        .Where( d =>
        //            d.EntityTypeId.HasValue &&
        //            d.EntityId.HasValue &&
        //            d.EntityTypeId.Value == registrationEntityType.Id &&
        //            registrationIdQry.Contains( d.EntityId.Value ) )
        //        .Any();

        //    this.RegistrationInstance = registrationInstance;
        //}

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
