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

namespace RockWeb.Blocks.Event
{
    /// <summary>
    /// A Block that allows viewing and editing an event registration instance.
    /// </summary>
    [DisplayName( "Registration Instance - Instance Detail" )]
    [Category( "Event" )]
    [Description( "Displays the details of a Registration Instance for viewing and editing." )]

    #region Block Attributes

    [AccountField(
        "Default Account",
        Description = "The default account to use for new registration instances",
        Key = AttributeKey.DefaultAccount,
        IsRequired = false,
        DefaultValue = Rock.SystemGuid.FinancialAccount.EVENT_REGISTRATION,
        Order = 0 )]

    [LinkedPage(
        "Payment Reminder Page",
        Key = AttributeKey.PaymentReminderPage,
        Description = "The page for manually sending payment reminders.",
        IsRequired = false,
        Order = 1 )]

    #endregion Block Attributes
    [Rock.SystemGuid.BlockTypeGuid( "22B67EDB-6D13-4D29-B722-DF45367AA3CB" )]
    public partial class RegistrationInstanceDetail : RegistrationInstanceBlock
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

            public const string PaymentReminderPage = "PaymentReminderPage";
        }

        #endregion Attribute Keys

        #region Page Parameter Keys

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

        #region Fields

        #endregion Fields

        #region Properties

        #endregion Properties

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            string deleteScript = @"
    $('a.js-delete-instance').on('click', function( e ){
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
                ShowDetail();
            }
            else
            {
                SetFollowingOnPostback();
            }
        }

        /// <summary>
        /// Gets the breadcrumbs for the page on which this block resides.
        /// </summary>
        /// <param name="pageReference">The page reference.</param>
        /// <returns></returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? registrationInstanceId = this.PageParameter( PageParameterKey.RegistrationInstanceId ).AsIntegerOrNull();

            if ( registrationInstanceId.HasValue )
            {
                var registrationInstance = GetSharedRegistrationInstance( registrationInstanceId.Value );

                if ( registrationInstance != null )
                {
                    var breadCrumbReference = new PageReference( pageReference );
                    breadCrumbReference.Parameters.AddOrReplace( PageParameterKey.RegistrationInstanceId, registrationInstanceId.ToString() );
                    breadCrumbs.Add( new BreadCrumb( registrationInstance.ToString(), breadCrumbReference ) );

                    return breadCrumbs;
                }
            }

            if ( registrationInstanceId == 0 )
            {
                breadCrumbs.Add( new BreadCrumb( "New Registration Instance", pageReference ) );
                return breadCrumbs;
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
        }

        #endregion Base Control Methods

        #region Events

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
            NavigateToLinkedPage( AttributeKey.PaymentReminderPage, queryParms );
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            RegistrationInstance registrationInstance = null;

            bool newInstance = false;

            int? existingRegistrationTemplateId = PageParameter( PageParameterKey.RegistrationTemplateId ).AsIntegerOrNull() ?? hfRegistrationTemplateId.Value.AsIntegerOrNull();

            using ( var rockContext = new RockContext() )
            {
                var registrationInstanceService = new RegistrationInstanceService( rockContext );

                int? registrationInstanceId = hfRegistrationInstanceId.Value.AsIntegerOrNull();
                if ( registrationInstanceId.HasValue )
                {
                    registrationInstance = registrationInstanceService.Get( registrationInstanceId.Value );
                }

                if ( registrationInstance == null )
                {
                    registrationInstance = new RegistrationInstance();
                    if ( existingRegistrationTemplateId.HasValue )
                    {
                        registrationInstance.RegistrationTemplateId = existingRegistrationTemplateId.Value;
                    }

                    registrationInstanceService.Add( registrationInstance );
                    newInstance = true;
                }

                rieDetails.GetValue( registrationInstance );

                if ( !Page.IsValid )
                {
                    return;
                }

                rockContext.SaveChanges();
            }

            if ( newInstance )
            {
                var qryParams = new Dictionary<string, string>();
                if ( existingRegistrationTemplateId.HasValue )
                {
                    qryParams.Add( PageParameterKey.RegistrationTemplateId, existingRegistrationTemplateId.ToString() );
                }

                qryParams.Add( PageParameterKey.RegistrationInstanceId, registrationInstance.Id.ToString() );
                NavigateToCurrentPage( qryParams );
            }
            else
            {
                // Reload instance and show readonly view
                using ( var rockContext = new RockContext() )
                {
                    registrationInstance = new RegistrationInstanceService( rockContext ).Get( registrationInstance.Id );
                    ShowReadonlyDetails( registrationInstance );

                    // show send payment reminder link
                    if ( !string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.PaymentReminderPage ) ) &&
                        ( ( registrationInstance.RegistrationTemplate.SetCostOnInstance.HasValue && registrationInstance.RegistrationTemplate.SetCostOnInstance == true && registrationInstance.Cost.HasValue && registrationInstance.Cost.Value > 0 ) ||
                            registrationInstance.RegistrationTemplate.Cost > 0 ||
                            registrationInstance.RegistrationTemplate.Fees.Count > 0 ) )
                    {
                        btnSendPaymentReminder.Visible = true;
                    }
                    else
                    {
                        btnSendPaymentReminder.Visible = false;
                    }
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
                using ( var rockContext = new RockContext() )
                {
                    RegistrationInstanceService service = new RegistrationInstanceService( rockContext );
                    RegistrationInstance item = service.Get( int.Parse( hfRegistrationInstanceId.Value ) );
                    ShowReadonlyDetails( item );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbTemplate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
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

        protected void btnCopy_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var registrationInstance = new RegistrationInstanceService( rockContext ).Get( hfRegistrationInstanceId.Value.AsInteger() );
            if ( registrationInstance != null )
            {
                // Clone the Registration Instance without the old Id.
                var newRegistrationInstance = registrationInstance.CloneWithoutIdentity();

                // delete account from registration if account is inactive
                if( !registrationInstance.Account.IsActive )
                {
                    newRegistrationInstance.Account = null;
                    newRegistrationInstance.AccountId = 0;
                }

                hfRegistrationInstanceId.Value = newRegistrationInstance.Id.ToString();
                hfRegistrationTemplateId.Value = newRegistrationInstance.RegistrationTemplateId.ToString();
                newRegistrationInstance.Name = registrationInstance.Name + " - Copy";
                newRegistrationInstance.IsActive = true;

                if ( newRegistrationInstance.RegistrationTemplateId > 0 )
                {
                    newRegistrationInstance.RegistrationTemplate = new RegistrationTemplateService( rockContext ).Get( newRegistrationInstance.RegistrationTemplateId );
                }

                registrationInstance.LoadAttributes();
                newRegistrationInstance.CopyAttributesFrom( registrationInstance );

                ShowEditDetails( newRegistrationInstance, rockContext );
            }
        }

        #endregion Events

        #region Methods

        /// <summary>
        /// Gets the registration instance.
        /// </summary>
        /// <param name="registrationInstanceId">The registration instance identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private RegistrationInstance GetRegistrationInstance( int registrationInstanceId, RockContext rockContext = null )
        {
            return base.GetRegistrationInstance( registrationInstanceId, rockContext );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemId">The item id value.</param>
        public void ShowDetail( int itemId )
        {
            ShowDetail();
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        private void ShowDetail()
        {
            int? registrationInstanceId = PageParameter( PageParameterKey.RegistrationInstanceId ).AsIntegerOrNull();
            int? parentTemplateId = PageParameter( PageParameterKey.RegistrationTemplateId ).AsIntegerOrNull();

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

                    Guid? accountGuid = GetAttributeValue( AttributeKey.DefaultAccount ).AsGuidOrNull();
                    if ( accountGuid.HasValue )
                    {
                        var account = FinancialAccountCache.Get( accountGuid.Value );
                        registrationInstance.AccountId = account != null && account.IsActive ? account.Id : 0;
                    }

                    // Do not allow copying an empty Registration Instance.
                    btnCopy.Visible = false;
                }

                if ( registrationInstance.RegistrationTemplate == null && registrationInstance.RegistrationTemplateId > 0 )
                {
                    registrationInstance.RegistrationTemplate = new RegistrationTemplateService( rockContext )
                        .Get( registrationInstance.RegistrationTemplateId );
                }

                hlType.Visible = registrationInstance.RegistrationTemplate != null;
                hlType.Text = registrationInstance.RegistrationTemplate != null ? registrationInstance.RegistrationTemplate.Name : string.Empty;

                // Display the status of whether the registration is open or not
                var registrationIsOpen = ( registrationInstance.StartDateTime == null || registrationInstance.StartDateTime.Value <= RockDateTime.Now )
                                            && ( registrationInstance.EndDateTime == null || registrationInstance.EndDateTime.Value >= RockDateTime.Now );

                if ( registrationIsOpen )
                {
                    hlStatus.Text = "Open";
                    hlStatus.LabelType = LabelType.Success;
                }
                else
                {
                    hlStatus.Text = "Closed";
                    hlStatus.LabelType = LabelType.Type;
                }

                lWizardTemplateName.Text = hlType.Text;

                pnlDetails.Visible = true;
                hfRegistrationInstanceId.Value = registrationInstance.Id.ToString();
                hfRegistrationTemplateId.Value = registrationInstance.RegistrationTemplateId.ToString();

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
                if ( !string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.PaymentReminderPage ) ) &&
                    ( ( registrationInstance.RegistrationTemplate.SetCostOnInstance.HasValue && registrationInstance.RegistrationTemplate.SetCostOnInstance == true && registrationInstance.Cost.HasValue && registrationInstance.Cost.Value > 0 ) ||
                    registrationInstance.RegistrationTemplate.Cost > 0 ||
                    registrationInstance.RegistrationTemplate.Fees.Count > 0 ) )
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
        /// Sets the following on postback.
        /// </summary>
        private void SetFollowingOnPostback()
        {
            int? registrationInstanceId = PageParameter( PageParameterKey.RegistrationInstanceId ).AsIntegerOrNull();
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
                lDefaultPaymentAmount.Visible = false;
            }

            // Display the status of whether the registration is open or not
            var registrationIsOpen = ( registrationInstance.StartDateTime == null || registrationInstance.StartDateTime.Value <= RockDateTime.Now )
                                        && ( registrationInstance.EndDateTime == null || registrationInstance.EndDateTime.Value >= RockDateTime.Now );

            if ( registrationIsOpen )
            {
                hlStatus.Text = "Open";
                hlStatus.LabelType = LabelType.Success;
            }
            else
            {
                hlStatus.Text = "Closed";
                hlStatus.LabelType = LabelType.Type;
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
            btnCopy.ToolTip = $"Copy { registrationInstance.Name }";
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

        #endregion Methods
    }
}