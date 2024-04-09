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
using System.Text;
using System.Web;
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
using Rock.Tasks;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Event
{
    [DisplayName( "Registration Detail" )]
    [Category( "Event" )]
    [Description( "Displays the details of a given registration." )]

    [LinkedPage( "Registrant Page", "The page for viewing details about a registrant", true, "", "", 0 )]
    [LinkedPage( "Transaction Page", "The page for viewing transaction details", true, "", "", 1 )]
    [LinkedPage( "Group Detail Page", "The page for viewing details about a group", true, "", "", 2 )]
    [LinkedPage( "Group Member Page", "The page for viewing details about a group member", true, "", "", 3 )]
    [LinkedPage( "Transaction Detail Page", "The page for viewing details about a payment", true, "", "", 4 )]
    [LinkedPage( "Audit Page", "Page used to display the history of changes to a registration.", true, "", "", 5 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE, "Source", "The Financial Source Type to use when creating transactions", false, false, Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_ONSITE_COLLECTION, "", 6 )]
    [TextField( "Batch Name Prefix", "The batch prefix name to use when creating a new batch", false, "Event Registration", "", 7 )]
    [Rock.SystemGuid.BlockTypeGuid( "A1C967B2-EEDA-416F-A53C-7BE46D6DA4E1" )]
    public partial class RegistrationDetail : RockBlock
    {
        #region ViewState Keys

        private static class ViewStateKey
        {
            public const string HostPaymentInfoSubmitScript = "HostPaymentInfoSubmitScript";
        }

        #endregion ViewState Keys

        #region Fields

        private Registration Registration = null;

        private Control _hostedPaymentInfoControl;

        #endregion Fields

        #region Properties

        private string Title = null;

        /// <summary>
        /// Gets or sets the registration instance identifier.
        /// </summary>
        /// <value>
        /// The registration instance identifier.
        /// </value>
        private int? RegistrationInstanceId
        {
            get
            {
                return ViewState["RegistrationInstanceId"] as int?;
            }

            set
            {
                ViewState["RegistrationInstanceId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the registration template identifier.
        /// </summary>
        /// <value>
        /// The registration template identifier.
        /// </value>
        private int RegistrationTemplateId
        {
            get
            {
                return ViewState["RegistrationTemplateId"] as int? ?? 0;
            }

            set
            {
                ViewState["RegistrationTemplateId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the registration identifier.
        /// </summary>
        /// <value>
        /// The registration identifier.
        /// </value>
        private int? RegistrationId
        {
            get
            {
                return ViewState["RegistrationId"] as int?;
            }

            set
            {
                ViewState["RegistrationId"] = value;
            }
        }

        private bool EditAllowed { get; set; }

        protected bool PercentageDiscountExists { get; set; }

        /// <summary>
        /// Gets the financial gateway.
        /// </summary>
        /// <value>
        /// The financial gateway.
        /// </value>
        protected FinancialGateway FinancialGateway
        {
            get
            {
                FinancialGateway financialGateway = null;
                if ( RegistrationTemplate != null )
                {
                    financialGateway = RegistrationTemplate.FinancialGateway;
                }
                else
                {
                    var rockContext = new RockContext();
                    var registrationInstanceId = this.PageParameter( "RegistrationInstanceId" ).AsIntegerOrNull();
                    if ( !registrationInstanceId.HasValue )
                    {
                        var registrationId = this.PageParameter( "RegistrationId" ).AsIntegerOrNull();
                        if ( registrationId.HasValue )
                        {
                            registrationInstanceId = new RegistrationService( rockContext ).GetSelect( registrationId.Value, s => s.RegistrationInstanceId );
                        }
                    }

                    if ( registrationInstanceId.HasValue )
                    {
                        financialGateway = new RegistrationInstanceService( rockContext ).GetSelect( registrationInstanceId.Value, s => s.RegistrationTemplate.FinancialGateway );
                    }
                }

                return financialGateway;
            }
        }

        private IHostedGatewayComponent _financialGatewayComponent = null;

        /// <summary>
        /// Gets the financial gateway component that is configured for this block
        /// </summary>
        private IHostedGatewayComponent FinancialGatewayComponent
        {
            get
            {
                if ( _financialGatewayComponent == null )
                {
                    var financialGateway = FinancialGateway;
                    if ( financialGateway != null )
                    {
                        _financialGatewayComponent = financialGateway.GetGatewayComponent() as IHostedGatewayComponent;
                    }
                }

                return _financialGatewayComponent;
            }
        }

        /// <summary>
        /// Gets or sets the host payment information submit JavaScript.
        /// </summary>
        /// <value>
        /// The host payment information submit script.
        /// </value>
        protected string HostPaymentInfoSubmitScript
        {
            get
            {
                return ViewState[ViewStateKey.HostPaymentInfoSubmitScript] as string;
            }

            set
            {
                ViewState[ViewStateKey.HostPaymentInfoSubmitScript] = value;
            }
        }

        private RegistrationTemplate RegistrationTemplate
        {
            get
            {
                if ( _registrationTemplate == null )
                {
                    _registrationTemplate = new RegistrationTemplateService( new RockContext() )
                        .Queryable().Where( a => a.Id == this.RegistrationTemplateId )
                        .Include( a => a.FinancialGateway )
                        .Include( a => a.Discounts )
                        .Include( a => a.Fees )
                        .Include( a => a.Forms )
                        .FirstOrDefault();
                }

                return _registrationTemplate;
            }
        }

        private RegistrationTemplate _registrationTemplate = null;

        private List<RegistrantInfo> RegistrantsState { get; set; }

        #endregion Properties

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            Title = ViewState["Title"] as string ?? string.Empty;
            EditAllowed = ViewState["EditAllowed"] as bool? ?? false;
            PercentageDiscountExists = ViewState["PercentageDiscountExists"] as bool? ?? false;

            var json = ViewState["Registrants"] as string;
            if ( !string.IsNullOrWhiteSpace( json ) )
            {
                RegistrantsState = JsonConvert.DeserializeObject<List<RegistrantInfo>>( json );
            }

            Registration = GetRegistration( RegistrationId );

            if ( RegistrationTemplate != null && RegistrantsState != null )
            {
                BuildRegistrationControls( false );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RegisterClientScript();

            gPayments.DataKeyNames = new string[] { "Id" };
            gPayments.Actions.ShowAdd = false;
            gPayments.GridRebind += gPayments_GridRebind;

            var qryParam = new Dictionary<string, string>();
            qryParam.Add( "TransactionId", "PLACEHOLDER" );
            var hlCol = gPayments.Columns[0] as HyperLinkField;
            if ( hlCol != null )
            {
                hlCol.DataNavigateUrlFormatString = LinkedPageUrl( "TransactionDetailPage", qryParam ).Replace( "PLACEHOLDER", "{0}" );
            }

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlRegistrationDetail );

            InitializeFinancialGatewayControls();
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
                if ( RegistrationInstanceId.HasValue && RegistrationId.HasValue )
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
            ViewState["EditAllowed"] = EditAllowed;
            ViewState["PercentageDiscountExists"] = PercentageDiscountExists;

            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState["Registrants"] = JsonConvert.SerializeObject( RegistrantsState, Formatting.None, jsonSetting );

            return base.SaveViewState();
        }

        #endregion Control Methods

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
                        mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
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
                }

                var pageParams = new Dictionary<string, string>();
                pageParams.Add( "RegistrationInstanceId", RegistrationInstanceId.ToString() );
                NavigateToParentPage( pageParams );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbHistory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbHistory_Click( object sender, EventArgs e )
        {
            var qryParam = new Dictionary<string, string>();
            qryParam.Add( "RegistrationId", Registration.Id.ToString() );
            NavigateToLinkedPage( "AuditPage", qryParam );
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
                var changes = new History.HistoryChangeList();

                if ( RegistrationId.Value != 0 )
                {
                    registration = registrationService.Queryable().Where( g => g.Id == RegistrationId.Value ).FirstOrDefault();
                }

                if ( registration == null )
                {
                    registration = new Registration { RegistrationInstanceId = RegistrationInstanceId ?? 0 };
                    registrationService.Add( registration );
                    newRegistration = true;
                    changes.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Registration" );
                }

                if ( registration != null && RegistrationInstanceId > 0 )
                {
                    if ( !registration.PersonAliasId.Equals( ppPerson.PersonAliasId ) )
                    {
                        string prevPerson = ( registration.PersonAlias != null && registration.PersonAlias.Person != null ) ?
                            registration.PersonAlias.Person.FullName : string.Empty;
                        string newPerson = ppPerson.PersonName;
                        History.EvaluateChange( changes, "Registrar", prevPerson, newPerson );
                    }

                    registration.PersonAliasId = ppPerson.PersonAliasId;

                    History.EvaluateChange( changes, "First Name", registration.FirstName, tbFirstName.Text );
                    registration.FirstName = tbFirstName.Text;

                    History.EvaluateChange( changes, "Last Name", registration.LastName, tbLastName.Text );
                    registration.LastName = tbLastName.Text;

                    History.EvaluateChange( changes, "Confirmation Email", registration.ConfirmationEmail, ebConfirmationEmail.Text );
                    registration.ConfirmationEmail = ebConfirmationEmail.Text;

                    bool groupChanged = !registration.GroupId.Equals( ddlGroup.SelectedValueAsInt() );
                    if ( groupChanged )
                    {
                        History.EvaluateChange( changes, "Group", registration.GroupId, ddlGroup.SelectedValueAsInt() );
                        registration.GroupId = ddlGroup.SelectedValueAsInt();
                    }

                    avcEditAttributes.GetEditValues( registration );

                    History.EvaluateChange( changes, "Discount Code", registration.DiscountCode, ddlDiscountCode.SelectedValue );
                    registration.DiscountCode = ddlDiscountCode.SelectedValue;

                    History.EvaluateChange( changes, "Discount Percentage", registration.DiscountPercentage, nbDiscountPercentage.Text.AsDecimal() * 0.01m );
                    registration.DiscountPercentage = nbDiscountPercentage.Text.AsDecimal() * 0.01m;

                    History.EvaluateChange( changes, "Discount Amount", registration.DiscountAmount, cbDiscountAmount.Value );
                    registration.DiscountAmount = cbDiscountAmount.Value == null ? 0 : cbDiscountAmount.Value.Value;

                    bool campusChanged = !registration.CampusId.Equals( cpRegistrationCampus.SelectedValueAsInt() );
                    if ( campusChanged )
                    {
                        History.EvaluateChange( changes, "Campus", registration.CampusId, cpRegistrationCampus.SelectedValueAsInt() );
                        registration.CampusId = cpRegistrationCampus.SelectedValueAsInt();
                    }

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
                        HistoryService.SaveChanges(
                            rockContext,
                            typeof( Registration ),
                            Rock.SystemGuid.Category.HISTORY_EVENT_REGISTRATION.AsGuid(),
                            registration.Id,
                            changes );

                        registration.SaveAttributeValues( rockContext );
                    } );

                    if ( newRegistration )
                    {
                        var pageRef = CurrentPageReference;
                        pageRef.Parameters.AddOrReplace( "RegistrationId", registration.Id.ToString() );
                        NavigateToPage( pageRef );
                    }
                    else
                    {
                        // Reload registration
                        Registration = GetRegistration( Registration.Id );

                        if ( groupChanged && Registration.GroupId.HasValue )
                        {
                            foreach ( var registrant in Registration.Registrants.Where( r => !r.GroupMemberId.HasValue ) )
                            {
                                AddRegistrantToGroup( registrant.Id );
                            }

                            // ...Add, reload again
                            Registration = GetRegistration( Registration.Id );
                        }

                        lWizardRegistrationName.Text = Registration.ToString();
                        ShowReadonlyDetails( Registration );
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
            var pageCache = PageCache.Get( RockPage.PageId );
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

            if ( pageCache != null && pageCache.ParentPage != null && pageCache.ParentPage.ParentPage != null )
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
            var pageCache = PageCache.Get( RockPage.PageId );
            var instanceId = Registration != null ? Registration.RegistrationInstanceId.ToString() : PageParameter( "RegistrationInstanceId" );
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
                    ShowDetail( registrationId.AsInteger(), PageParameter( "RegistrationInstanceId" ).AsIntegerOrNull() );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
        }

        #endregion Edit Events

        #region Registration Detail Events

        protected void lbShowMoveRegistrationDialog_Click( object sender, EventArgs e )
        {
            if ( RegistrationId.HasValue )
            {
                // add current registration instance name
                lCurrentRegistrationInstance.Text = Registration.RegistrationInstance.Name;

                BindOtherInstances();

                mdMoveRegistration.Show();
            }
        }

        protected void cbShowAll_CheckedChanged( object sender, EventArgs e )
        {
            BindOtherInstances();
        }

        protected void ddlNewRegistrationInstance_SelectedIndexChanged( object sender, EventArgs e )
        {
            ddlMoveGroup.Items.Clear();

            int? instanceId = ddlNewRegistrationInstance.SelectedValueAsInt();
            if ( instanceId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var instance = new RegistrationInstanceService( rockContext ).Get( instanceId.Value );
                    if ( instance != null )
                    {
                        var groups = instance.Linkages
                            .Where( l => l.Group != null )
                            .Select( l => new
                            {
                                Value = l.Group.Id,
                                Text = l.Group.Name
                            } )
                            .ToList();

                        ddlMoveGroup.DataSource = groups;
                        ddlMoveGroup.DataBind();
                        ddlMoveGroup.Items.Insert( 0, new ListItem( string.Empty, string.Empty ) );
                    }
                }
            }

            ddlMoveGroup.Visible = ddlMoveGroup.Items.Count > 0;
        }

        protected void btnMoveRegistration_Click( object sender, EventArgs e )
        {
            // set the new registration id
            using ( var rockContext = new RockContext() )
            {
                var registrationService = new RegistrationService( rockContext );
                var groupMemberService = new GroupMemberService( rockContext );

                var registration = registrationService.Get( Registration.Id );

                var oldRegistrationInstanceId = registration.RegistrationInstanceId;
                var oldRegistrationInstanceName = registration.RegistrationInstance.Name;
                var newRegistrationInstanceId = ddlNewRegistrationInstance.SelectedValue.AsInteger();

                registration.RegistrationInstanceId = newRegistrationInstanceId;

                // Get new registration instance so we have it's properties for history
                var newRegistrationInstance = new RegistrationInstanceService( rockContext ).Get( newRegistrationInstanceId );

                //
                // Add History record
                var historyService = new HistoryService( rockContext );
                var historyRecord = new History();
                historyService.Add( historyRecord );

                historyRecord.EntityTypeId = EntityTypeCache.Get<Registration>().Id;
                historyRecord.EntityId = registration.Id;

                historyRecord.Verb = "MOVED";
                historyRecord.ValueName = "Registration Instance";
                historyRecord.ChangeType = "Moved";
                historyRecord.OldValue = oldRegistrationInstanceName;
                historyRecord.OldRawValue = oldRegistrationInstanceId.ToStringSafe();
                historyRecord.NewValue = newRegistrationInstance.Name;
                historyRecord.NewRawValue = newRegistrationInstance.Id.ToString();
                historyRecord.Caption = GetInternalComment( registration, 200 );
                historyRecord.CategoryId = CategoryCache.Get( Rock.SystemGuid.Category.HISTORY_EVENT_REGISTRATION ).Id;

                //
                // Move registrants to new group
                int? groupId = ddlMoveGroup.SelectedValueAsInt();
                if ( groupId.HasValue )
                {
                    registration.GroupId = groupId;
                    rockContext.SaveChanges();

                    var group = new GroupService( rockContext ).Get( groupId.Value );
                    if ( group != null )
                    {
                        int? groupRoleId = null;
                        var template = registration.RegistrationInstance.RegistrationTemplate;
                        if ( group.GroupTypeId == template.GroupTypeId && template.GroupMemberRoleId.HasValue )
                        {
                            groupRoleId = template.GroupMemberRoleId.Value;
                        }

                        if ( !groupRoleId.HasValue )
                        {
                            groupRoleId = group.GroupType.DefaultGroupRoleId;
                        }

                        if ( !groupRoleId.HasValue )
                        {
                            groupRoleId = group.GroupType.Roles.OrderBy( r => r.Order ).Select( r => r.Id ).FirstOrDefault();
                        }

                        if ( groupRoleId.HasValue )
                        {
                            foreach ( var registrant in registration.Registrants.Where( r => r.PersonAlias != null ) )
                            {
                                var newGroupMembers = groupMemberService.GetByGroupIdAndPersonId( groupId.Value, registrant.PersonAlias.PersonId );
                                if ( !newGroupMembers.Any() )
                                {
                                    // Get any existing group member attribute values
                                    var existingAttributeValues = new Dictionary<string, string>();
                                    if ( registrant.GroupMemberId.HasValue )
                                    {
                                        var existingGroupMember = groupMemberService.Get( registrant.GroupMemberId.Value );
                                        if ( existingGroupMember != null )
                                        {
                                            existingGroupMember.LoadAttributes( rockContext );
                                            foreach ( var attributeValue in existingGroupMember.AttributeValues )
                                            {
                                                existingAttributeValues.Add( attributeValue.Key, attributeValue.Value.Value );
                                            }
                                        }

                                        registrant.GroupMember = null;
                                        groupMemberService.Delete( existingGroupMember );
                                    }

                                    var newGroupMember = new GroupMember();
                                    newGroupMember.Group = group;
                                    newGroupMember.PersonId = registrant.PersonAlias.PersonId;
                                    newGroupMember.GroupRoleId = groupRoleId.Value;
                                    groupMemberService.Add( newGroupMember );
                                    rockContext.SaveChanges();

                                    newGroupMember = groupMemberService.Get( newGroupMember.Id );
                                    newGroupMember.LoadAttributes();

                                    foreach ( var attr in newGroupMember.Attributes )
                                    {
                                        if ( existingAttributeValues.ContainsKey( attr.Key ) )
                                        {
                                            newGroupMember.SetAttributeValue( attr.Key, existingAttributeValues[attr.Key] );
                                        }
                                    }

                                    newGroupMember.SaveAttributeValues( rockContext );

                                    registrant.GroupMember = newGroupMember;
                                    rockContext.SaveChanges();
                                }
                            }
                        }
                    }
                }
                else
                {
                    rockContext.SaveChanges();
                }

                // Reload registration
                Registration = GetRegistration( Registration.Id );

                lWizardInstanceName.Text = Registration.RegistrationInstance.Name;
                ShowReadonlyDetails( Registration );
            }

            mdMoveRegistration.Hide();
        }

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
                        if ( person.Email.IsNotNullOrWhiteSpace() )
                        {
                            ebConfirmationEmail.Text = person.Email;
                        }

                        if ( person.NickName.IsNotNullOrWhiteSpace() )
                        {
                            tbFirstName.Text = person.NickName;
                        }

                        if ( person.LastName.IsNotNullOrWhiteSpace() )
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
                if ( this.RegistrationTemplate != null )
                {
                    discount = this.RegistrationTemplate.Discounts
                        .Where( d => d.Code.Equals( code, StringComparison.OrdinalIgnoreCase ) )
                        .FirstOrDefault();
                }
            }

            nbDiscountPercentage.Text = discount != null && discount.DiscountPercentage != 0.0m ? ( discount.DiscountPercentage * 100.0m ).ToString( "N0" ) : string.Empty;
            cbDiscountAmount.Value = discount != null && discount.DiscountAmount != 0.0m ? discount.DiscountAmount : ( decimal? ) null;
        }

        protected void cbDiscountAmount_TextChanged( object sender, EventArgs e )
        {
            // Clear out the other discount controls since only one discount control can be used.
            ddlDiscountCode.SelectedValue = string.Empty;
            nbDiscountPercentage.Text = string.Empty;
        }

        protected void nbDiscountPercentage_TextChanged( object sender, EventArgs e )
        {
            // Clear out the other discount controls since only one discount control can be used.
            ddlDiscountCode.SelectedValue = string.Empty;
            cbDiscountAmount.Value = null;
        }

        protected void lbResendConfirmation_Click( object sender, EventArgs e )
        {
            if ( RegistrationId.HasValue )
            {
                string appRoot = ResolveRockUrlIncludeRoot( "~/" );
                string themeRoot = ResolveRockUrlIncludeRoot( "~~/" );

                var processSendRegistrationConfirmationMsg = new ProcessSendRegistrationConfirmation.Message
                {
                    RegistrationId = RegistrationId.Value,
                    AppRoot = appRoot,
                    ThemeRoot = themeRoot
                };
                processSendRegistrationConfirmationMsg.Send();

                var changes = new History.HistoryChangeList();
                changes.AddChange( History.HistoryVerb.Sent, History.HistoryChangeType.Record, "Confirmation" ).SetRelatedData( "Resent", null, null );
                using ( var rockContext = new RockContext() )
                {
                    HistoryService.SaveChanges(
                        rockContext,
                        typeof( Registration ),
                        Rock.SystemGuid.Category.HISTORY_EVENT_REGISTRATION.AsGuid(),
                        RegistrationId.Value,
                        changes );
                }

                nbConfirmationQueued.Visible = true;
            }
        }

        #endregion Registration Detail Events

        #region Payment Buttons Events

        protected void lbAddPayment_Click( object sender, EventArgs e )
        {
            if ( Registration != null )
            {
                if ( Registration.PersonAlias != null && Registration.PersonAlias.Person != null )
                {
                    ppPayee.SetValue( Registration.PersonAlias.Person );
                }
                else
                {
                    ppPayee.SetValue( null );
                }

                using ( var rockContext = new RockContext() )
                {
                    dvpCurrencyType.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE.AsGuid(), rockContext ).Id;
                    dvpCreditCardType.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.FINANCIAL_CREDIT_CARD_TYPE.AsGuid(), rockContext ).Id;
                    dvpCreditCardType.Visible = false;
                }

                cbPaymentAmount.Text = null;
                tbTransactionCode.Text = string.Empty;

                this.SetActiveAccountPanel( RegistrationDetailAccountPanelSpecifier.PaymentManualDetails );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbProcessPayment control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbProcessPayment_Click( object sender, EventArgs e )
        {
            if ( Registration == null || Registration.PersonAliasId == null || this.RegistrationTemplate == null || this.RegistrationTemplate.FinancialGateway == null )
            {
                return;
            }

            var iHostedGatewayComponent = this.RegistrationTemplate.FinancialGateway.GetGatewayComponent() as IHostedGatewayComponent;
            if ( iHostedGatewayComponent == null )
            {
                return;
            }

            cbPaymentAmount.Value = Registration.BalanceDue;

            if ( Registration.PersonAlias != null && Registration.PersonAlias.Person != null )
            {
                var person = Registration.PersonAlias.Person;

                ppPayee.SetValue( person );

                var location = person.GetHomeLocation();
                acBillingAddress.SetValues( location );
            }
            else
            {
                ppPayee.SetValue( null );
                acBillingAddress.SetValues( null );
            }

            SetActiveAccountPanel( RegistrationDetailAccountPanelSpecifier.PaymentProcess );
        }

        protected void lbSubmitPayment_Click( object sender, EventArgs e )
        {
            int? personAliasId = ppPayee.PersonAliasId;

            decimal pmtAmount = cbPaymentAmount.Value == null ? 0 : cbPaymentAmount.Value.Value;
            if ( Registration != null && Registration.BalanceDue >= pmtAmount && pmtAmount > 0 )
            {
                if ( !personAliasId.HasValue && Registration.PersonAliasId.HasValue )
                {
                    personAliasId = Registration.PersonAliasId;
                }

                if ( this.RegistrationTemplate.FinancialGateway == null )
                {
                    nbPaymentError.Heading = "Error Processing Payment";
                    nbPaymentError.Text = "A Financial Gateway must be set on the Registration Template.";
                    nbPaymentError.Visible = true;
                    return;
                }

                try
                {
                    var rockContext = new RockContext();
                    rockContext.WrapTransaction( () =>
                    {
                        string errorMessage = string.Empty;

                        if ( !ProcessPayment( !phManualDetails.Visible, rockContext, Registration, personAliasId, pmtAmount, out errorMessage ) )
                        {
                            throw new Exception( errorMessage );
                        }
                    } );

                    // reload registration
                    Registration = GetRegistration( Registration.Id, rockContext );

                    RockPage.UpdateBlocks( "~/Blocks/Finance/TransactionList.ascx" );

                    ShowReadonlyDetails( Registration );

                    SetActiveAccountPanel( RegistrationDetailAccountPanelSpecifier.PaymentList );

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

        protected void lbCancelPayment_Click( object sender, EventArgs e )
        {
            BindPaymentsGrid();

            SetActiveAccountPanel( RegistrationDetailAccountPanelSpecifier.PaymentList );
        }

        #endregion Payment Buttons Events

        #region Registrant Events

        protected void lbGroupMember_Click( object sender, EventArgs e )
        {
            var lb = sender as LinkButton;
            if ( lb != null )
            {
                int? registrantId = lb.ID.Substring( 14 ).AsIntegerOrNull();
                if ( registrantId.HasValue )
                {
                    var previousRegistrantPersonIds = Registration.Registrants
                                .Where( r => r.PersonAlias != null )
                                .Select( r => r.PersonAlias.PersonId )
                                .ToList();

                    if ( Registration.PersonId.HasValue )
                    {
                        Registration.SavePersonNotesAndHistory( new PersonService( new RockContext() ).Get( Registration.PersonId.Value ), CurrentPersonAliasId, previousRegistrantPersonIds );
                    }

                    AddRegistrantToGroup( registrantId.Value );
                }
            }

            // Reload registration
            Registration = GetRegistration( Registration.Id );
            lWizardRegistrationName.Text = Registration.ToString();
            ShowReadonlyDetails( Registration );
        }

        private void AddRegistrantToGroup( int registrantId )
        {
            if ( this.RegistrationTemplate != null &&
                this.RegistrationTemplate.GroupTypeId.HasValue &&
                Registration.GroupId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var registrant = new RegistrationRegistrantService( rockContext ).Get( registrantId );
                    if ( registrant != null && registrant.PersonId.HasValue && !registrant.GroupMemberId.HasValue )
                    {
                        var groupService = new GroupService( rockContext );
                        var group = groupService.Get( Registration.GroupId.Value );
                        if ( group != null && group.GroupTypeId == this.RegistrationTemplate.GroupTypeId.Value )
                        {
                            int? groupRoleId = this.RegistrationTemplate.GroupMemberRoleId.HasValue ?
                                this.RegistrationTemplate.GroupMemberRoleId.Value :
                                group.GroupType.DefaultGroupRoleId;
                            if ( groupRoleId.HasValue )
                            {
                                var registrantChanges = new History.HistoryChangeList();

                                var groupMemberService = new GroupMemberService( rockContext );
                                var groupMember = groupMemberService
                                    .Queryable().AsNoTracking()
                                    .Where( m =>
                                        m.GroupId == Registration.Group.Id &&
                                        m.PersonId == registrant.PersonId &&
                                        m.GroupRoleId == groupRoleId.Value )
                                    .FirstOrDefault();
                                if ( groupMember == null )
                                {
                                    groupMember = new GroupMember();
                                    groupMember.GroupId = group.Id;
                                    groupMember.PersonId = registrant.PersonId.Value;
                                    groupMember.GroupRoleId = groupRoleId.Value;
                                    groupMember.GroupMemberStatus = this.RegistrationTemplate.GroupMemberStatus;
                                    groupMemberService.Add( groupMember );

                                    rockContext.SaveChanges();

                                    registrantChanges.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, string.Format( "Registrant to {0} group", group.Name ) );
                                }
                                else
                                {
                                    registrantChanges.AddChange( History.HistoryVerb.Modify, History.HistoryChangeType.Record, string.Format( "Registrant to existing person in {0} group", group.Name ) );
                                }

                                groupMember.GroupMemberStatus = this.RegistrationTemplate.GroupMemberStatus;
                                registrant.GroupMemberId = groupMember.Id;
                                rockContext.SaveChanges();

                                HistoryService.SaveChanges(
                                    rockContext,
                                    typeof( Registration ),
                                    Rock.SystemGuid.Category.HISTORY_EVENT_REGISTRATION.AsGuid(),
                                    registrant.RegistrationId,
                                    registrantChanges,
                                    "Registrant: " + CurrentPerson.FullName,
                                    null,
                                    null );
                            }
                        }
                    }
                }
            }
        }

        private void lbResendDocumentRequest_Click( object sender, EventArgs e )
        {
            var lb = sender as LinkButton;
            if ( lb != null )
            {
                int? registrantId = lb.ID.Substring( 24 ).AsIntegerOrNull();
                if ( registrantId.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var personService = new PersonService( rockContext );
                        var signatureDocumentTemplateService = new SignatureDocumentTemplateService( rockContext );
                        var registrant = new RegistrationRegistrantService( rockContext ).Get( registrantId.Value );
                        if ( registrant != null &&
                            registrant.PersonAlias != null &&
                            registrant.PersonAlias.Person != null &&
                            Registration != null &&
                            Registration.RegistrationInstance != null &&
                            Registration.RegistrationInstance.RegistrationTemplate != null )
                        {
                            // Make sure to load these person records using the current rockContext
                            var assignedTo = personService.Get( registrant.PersonAlias.PersonId );
                            var appliesTo = personService.Get( registrant.PersonAlias.PersonId );
                            string email = Registration.ConfirmationEmail;
                            if ( string.IsNullOrWhiteSpace( email ) && Registration.PersonAlias != null && Registration.PersonAlias.Person != null )
                            {
                                email = Registration.PersonAlias.Person.Email;
                            }

                            Guid? adultRole = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid();
                            var registrantIsAdult = adultRole.HasValue && new GroupMemberService( rockContext )
                                .Queryable().AsNoTracking()
                                .Any( m =>
                                    m.PersonId == registrant.PersonId &&
                                    m.GroupRole.Guid.Equals( adultRole.Value ) );
                            if ( !registrantIsAdult && Registration.PersonAlias != null && Registration.PersonAlias.Person != null )
                            {
                                assignedTo = personService.Get( Registration.PersonAlias.PersonId );
                            }
                            else
                            {
                                if ( !string.IsNullOrWhiteSpace( registrant.PersonAlias.Person.Email ) )
                                {
                                    email = registrant.PersonAlias.Person.Email;
                                }
                            }

                            var sendErrorMessages = new List<string>();
                            if ( new SignatureDocumentTemplateService( rockContext ).SendLegacyProviderDocument(
                                signatureDocumentTemplateService.Get( Registration.RegistrationInstance.RegistrationTemplate.RequiredSignatureDocumentTemplateId.Value ),
                                appliesTo,
                                assignedTo,
                                Registration.RegistrationInstance.Name,
                                email,
                                out sendErrorMessages ) )
                            {
                                rockContext.SaveChanges();
                                maSignatureRequestSent.Show( "A Signature Request Has Been Sent.", Rock.Web.UI.Controls.ModalAlertType.Information );
                            }
                            else
                            {
                                string errorMessage = string.Format( "Unable to send a signature request: <ul><li>{0}</li></ul>", sendErrorMessages.AsDelimited( "</li><li>" ) );
                                maSignatureRequestSent.Show( errorMessage, Rock.Web.UI.Controls.ModalAlertType.Alert );
                            }
                        }
                    }
                }
            }

            ShowReadonlyDetails( GetRegistration( RegistrationId ) );
        }

        protected void lbEditRegistrant_Click( object sender, EventArgs e )
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

        protected void lbDeleteRegistrant_Click( object sender, EventArgs e )
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
                        if ( !UserCanEdit &&
                            !registrant.IsAuthorized( "Register", CurrentPerson ) &&
                            !registrant.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) &&
                            !registrant.IsAuthorized( Authorization.ADMINISTRATE, this.CurrentPerson ) )
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

                        var changes = new History.HistoryChangeList();
                        changes.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, "Registrant" ).SetOldValue( registrant.PersonAlias.Person.FullName );

                        rockContext.WrapTransaction( () =>
                        {
                            HistoryService.SaveChanges(
                                rockContext,
                                typeof( Registration ),
                                Rock.SystemGuid.Category.HISTORY_EVENT_REGISTRATION.AsGuid(),
                                registrant.RegistrationId,
                                changes );

                            registrantService.Delete( registrant );

                            rockContext.SaveChanges();
                        } );
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

        #endregion Registrant Events

        #region Payment Details Events

        /// <summary>
        /// Handles the GridRebind event of the gPayments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gPayments_GridRebind( object sender, EventArgs e )
        {
            BindPaymentsGrid();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the dvpCurrencyType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dvpCurrencyType_SelectedIndexChanged( object sender, EventArgs e )
        {
            int? currencyType = dvpCurrencyType.SelectedValueAsInt();
            var creditCardCurrencyType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD );
            dvpCreditCardType.Visible = currencyType.HasValue && currencyType.Value == creditCardCurrencyType.Id;
        }

        #endregion Payment Details Events

        #region Load/Save Methods

        private void LoadState()
        {
            EditAllowed = UserCanEdit;

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
                        RegistrationTemplateId = Registration.RegistrationInstance.RegistrationTemplateId;
                        lWizardTemplateName.Text = Registration.RegistrationInstance.RegistrationTemplate.Name;
                        lWizardInstanceName.Text = Registration.RegistrationInstance.Name;
                        lWizardRegistrationName.Text = Registration.ToString();

                        EditAllowed = EditAllowed ||
                            Registration.RegistrationInstance.IsAuthorized( "Register", CurrentPerson ) ||
                            Registration.RegistrationInstance.IsAuthorized( Authorization.EDIT, CurrentPerson ) ||
                            Registration.RegistrationInstance.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );
                    }
                }

                if ( this.RegistrationTemplate == null && RegistrationInstanceId.HasValue )
                {
                    var registrationInstance = new RegistrationInstanceService( rockContext )
                        .Queryable()
                        .Include( a => a.RegistrationTemplate ).AsNoTracking()
                        .Where( i => i.Id == RegistrationInstanceId.Value )
                        .FirstOrDefault();
                    if ( registrationInstance != null )
                    {
                        lWizardTemplateName.Text = registrationInstance.RegistrationTemplate.Name;
                        lWizardInstanceName.Text = registrationInstance.Name;
                        lWizardRegistrationName.Text = "New Registration";
                        this.RegistrationTemplateId = registrationInstance.RegistrationTemplateId;

                        EditAllowed = EditAllowed ||
                            registrationInstance.IsAuthorized( "Register", CurrentPerson ) ||
                            registrationInstance.IsAuthorized( Authorization.EDIT, CurrentPerson ) ||
                            registrationInstance.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );
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
                    .Queryable()
                    .Include( a => a.RegistrationInstance.RegistrationTemplate.Forms )
                    .Include( a => a.RegistrationInstance.RegistrationTemplate.Forms.Select( s => s.Fields ) )
                    .Include( a => a.PersonAlias.Person )
                    .Include( a => a.Group )
                    .Include( a => a.Registrants )
                    .Include( a => a.Registrants.Select( s => s.Fees ) )
                    .Where( r => r.Id == registrationId.Value )
                    .FirstOrDefault();

                return registration;
            }

            return null;
        }

        #endregion Load/Save Methods

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
                if ( Registration.RegistrationInstanceId > 0 )
                {
                    Registration.RegistrationInstance = new RegistrationInstanceService( rockContext ).Get( Registration.RegistrationInstanceId );
                }
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
            if ( this.RegistrationTemplate != null )
            {
                foreach ( var discount in this.RegistrationTemplate.Discounts.OrderBy( d => d.Code ) )
                {
                    discountCodes.AddOrIgnore(
                        discount.Code,
                        discount.Code + ( string.IsNullOrWhiteSpace( discount.DiscountString ) ? string.Empty : string.Format( " ({0})", HttpUtility.HtmlDecode( discount.DiscountString ) ) ) );
                }
            }

            if ( !string.IsNullOrWhiteSpace( registration.DiscountCode ) )
            {
                discountCodes.AddOrIgnore( registration.DiscountCode, registration.DiscountCode );
            }

            ddlGroup.Items.Clear();
            ddlGroup.Items.Add( new ListItem( string.Empty, string.Empty ) );
            if ( registration.RegistrationInstance != null &&
                registration.RegistrationInstance.Linkages != null &&
                registration.RegistrationInstance.Linkages.Any() )
            {
                var linkageGroups = registration.RegistrationInstance.Linkages
                    .Where( l => l.Group != null )
                    .OrderBy( l => l.Group.Name )
                    .Select( l => l.Group );
                if ( linkageGroups.Any() )
                {
                    foreach ( var group in linkageGroups )
                    {
                        ddlGroup.Items.Add( new ListItem( group.Name, group.Id.ToString() ) );
                    }

                    ddlGroup.Visible = true;
                }
            }

            ddlGroup.SetValue( registration.Group );

            if ( registration.CampusId.HasValue )
            {
                cpRegistrationCampus.SelectedCampusId = registration.CampusId;
            }

            registration.LoadAttributes();

            // Don't show the Categories, since they will probably be 'Start of Registration' or 'End of Registration';
            avcEditAttributes.ShowCategoryLabel = false;
            avcEditAttributes.ExcludedAttributes = registration.Attributes.Where( a => !a.Value.IsAuthorized( Rock.Security.Authorization.EDIT, this.CurrentPerson ) ).Select( a => a.Value ).ToArray();
            avcEditAttributes.AddEditControls( registration );

            ddlDiscountCode.DataSource = discountCodes;
            ddlDiscountCode.DataBind();
            ddlDiscountCode.Items.Insert( 0, new ListItem( string.Empty, string.Empty ) );
            ddlDiscountCode.SetValue( registration.DiscountCode );

            nbDiscountPercentage.Text = registration.DiscountPercentage != 0.0m ? ( registration.DiscountPercentage * 100.0m ).ToString( "N0" ) : string.Empty;
            cbDiscountAmount.Value = registration.DiscountAmount != 0.0m ? registration.DiscountAmount : ( decimal? ) null;

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

            registration.LoadAttributes();
            avcDisplayAttributes.ExcludedAttributes = registration.Attributes.Where( a => !a.Value.IsAuthorized( Rock.Security.Authorization.VIEW, this.CurrentPerson ) ).Select( a => a.Value ).ToArray();

            // Don't show the Categories, since they will probably be 'Start of Registration' or 'End of Registration';
            avcDisplayAttributes.ShowCategoryLabel = false;
            avcDisplayAttributes.AddDisplayControls( registration );

            lDiscountCode.Visible = !string.IsNullOrWhiteSpace( registration.DiscountCode );
            lDiscountCode.Text = registration.DiscountCode;

            lDiscountPercent.Visible = registration.DiscountPercentage > 0.0m;
            lDiscountPercent.Text = registration.DiscountPercentage.ToString( "P0" );

            lDiscountAmount.Visible = registration.DiscountAmount > 0.0m;
            lDiscountAmount.Text = registration.DiscountAmount.FormatAsCurrency();

            RegistrantsState = new List<RegistrantInfo>();
            registration.Registrants.ToList().ForEach( r => RegistrantsState.Add( new RegistrantInfo( r, rockContext ) ) );

            if ( registration.RegistrationInstance != null &&
                registration.RegistrationInstance.RegistrationTemplate != null &&
                registration.RegistrationInstance.RegistrationTemplate.RequiredSignatureDocumentTemplateId.HasValue )
            {
                var personIds = RegistrantsState.Select( r => r.PersonId ).ToList();
                var documents = new SignatureDocumentService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( d =>
                        d.SignatureDocumentTemplateId == registration.RegistrationInstance.RegistrationTemplate.RequiredSignatureDocumentTemplateId.Value &&
                        d.Status == SignatureDocumentStatus.Signed &&
                        d.BinaryFileId.HasValue &&
                        d.AppliesToPersonAlias != null && personIds.Contains( d.AppliesToPersonAlias.PersonId ) )
                    .OrderByDescending( d => d.LastStatusDate )
                    .ToList();

                foreach ( var registrantInfo in RegistrantsState )
                {
                    var document = documents.Where( d => d.AppliesToPersonAlias.PersonId == registrantInfo.PersonId ).FirstOrDefault();
                    registrantInfo.SignatureDocumentId = document != null ? document.BinaryFileId : ( int? ) null;
                    registrantInfo.SignatureDocumentLastSent = document != null ? document.LastInviteDate : ( DateTime? ) null;
                    registrantInfo.SignatureDocumentSignedDateTime = document != null ? document.SignedDateTime : ( DateTime? ) null;
                    registrantInfo.SignatureDocumentSignedName = document != null ? document.SignedName : null;
                }
            }

            PercentageDiscountExists = registration.DiscountPercentage > 0.0m;
            BuildFeeTable( registration );

            SetActiveAccountPanel( RegistrationDetailAccountPanelSpecifier.AccountSummary );

            BuildRegistrationControls( true );

            bool anyPayments = registration.Payments.Any();
            hfHasPayments.Value = anyPayments.ToString();
            foreach ( RockWeb.Blocks.Finance.TransactionList block in RockPage.RockBlocks.Where( a => a is RockWeb.Blocks.Finance.TransactionList ) )
            {
                block.SetVisible( anyPayments );
            }

            lbAddRegistrant.Visible = EditAllowed;

            BindPaymentsGrid();
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
                hlCost.Text = registration.DiscountedCost.FormatAsCurrency();

                decimal balanceDue = registration.BalanceDue;
                hlBalance.Visible = true;
                hlBalance.Text = balanceDue.FormatAsCurrency();
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
            string deleteScript = @"

    $('a.js-delete-registration').on('click', function( e ){
        e.preventDefault();
        Rock.dialogs.confirm('Are you sure you want to delete this Registration? All of the registrants will also be deleted!', function (result) {
            if (result) {
                if ( $('input.js-has-payments').val() == 'True' ) {
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
            ScriptManager.RegisterStartupScript( btnDelete, btnDelete.GetType(), "deleteRegistrationScript", deleteScript, true );
        }

        #endregion Display Methods

        #region Payment / Hosted Gateway

        private void InitializeFinancialGatewayControls()
        {
            bool enableACH = false;
            bool enableCreditCard = true;
            if ( this.FinancialGatewayComponent != null && this.FinancialGateway != null )
            {
                _hostedPaymentInfoControl = this.FinancialGatewayComponent.GetHostedPaymentInfoControl( this.FinancialGateway, $"_hostedPaymentInfoControl_{this.FinancialGateway.Id}", new HostedPaymentInfoControlOptions { EnableACH = enableACH, EnableCreditCard = enableCreditCard, EnableBillingAddressCollection = false } );
                phHostedPaymentControl.Controls.Add( _hostedPaymentInfoControl );
                this.HostPaymentInfoSubmitScript = this.FinancialGatewayComponent.GetHostPaymentInfoSubmitScript( this.FinancialGateway, _hostedPaymentInfoControl );
            }

            if ( _hostedPaymentInfoControl is IHostedGatewayPaymentControlTokenEvent )
            {
                ( _hostedPaymentInfoControl as IHostedGatewayPaymentControlTokenEvent ).TokenReceived += _hostedPaymentInfoControl_TokenReceived;
            }
        }

        /// <summary>
        /// Handles the TokenReceived event of the _hostedPaymentInfoControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void _hostedPaymentInfoControl_TokenReceived( object sender, HostedGatewayPaymentControlTokenEventArgs e )
        {
            if ( !e.IsValid )
            {
                nbPaymentTokenError.Text = e.ErrorMessage;
                nbPaymentTokenError.Visible = true;
            }
            else
            {
                nbPaymentTokenError.Visible = false;
                lbSubmitPayment_Click( sender, e );
            }
        }

        /// <summary>
        /// Gets the formatted payment comment for a registration.
        /// </summary>
        /// <param name="registration">The <see cref="Registration"/>.</param>
        /// <returns></returns>
        private string GetPaymentComment( Registration registration )
        {
            return $"{registration.RegistrationInstance.Name} ({registration.RegistrationInstance.Account.GlCode})";
        }

        /// <summary>
        /// Gets the formatted internal comment for a registration, which includes the payment comment
        /// (sent to the processing gateway) and the comments entered by a user.
        /// </summary>
        /// <param name="registration">The <see cref="Registration"/>.</param>
        /// <param name="maxLength">The maximum length of the comment, if it should be truncated.</param>
        /// <returns></returns>
        private string GetInternalComment( Registration registration, int maxLength = -1 )
        {
            var internalComment = $"{GetPaymentComment( registration )}: {tbComments.Text}";

            if ( maxLength > -1 && internalComment.Length > maxLength )
            {
                internalComment = internalComment.Substring( 0, 200 );
            }

            return internalComment;
        }

        /// <summary>
        /// Processes the payment.
        /// </summary>
        /// <param name="submitToGateway">if set to <c>true</c> [submit to gateway].</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="registration">The registration.</param>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        private bool ProcessPayment( bool submitToGateway, RockContext rockContext, Registration registration, int? personAliasId, decimal amount, out string errorMessage )
        {
            var registrationChanges = new History.HistoryChangeList();

            if ( submitToGateway )
            {
                IHostedGatewayComponent gateway = null;
                if ( this.RegistrationTemplate != null && this.RegistrationTemplate.FinancialGateway != null )
                {
                    gateway = this.RegistrationTemplate.FinancialGateway.GetGatewayComponent() as IHostedGatewayComponent;
                }

                if ( gateway == null )
                {
                    errorMessage = "There was a problem creating the payment gateway information";
                    return false;
                }

                if ( registration == null || registration.RegistrationInstance == null || !registration.RegistrationInstance.AccountId.HasValue || registration.RegistrationInstance.Account == null )
                {
                    errorMessage = "There was a problem with the account configuration for this registration.";
                    return false;
                }

                var paymentInfo = new ReferencePaymentInfo();
                paymentInfo.UpdateAddressFieldsFromAddressControl( acBillingAddress );

                paymentInfo.Amount = amount;
                paymentInfo.Email = registration.ConfirmationEmail;

                paymentInfo.FirstName = registration.FirstName;
                paymentInfo.LastName = registration.LastName;

                paymentInfo.Comment1 = GetPaymentComment( registration );

                var txnType = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_EVENT_REGISTRATION ) );
                paymentInfo.TransactionTypeValueId = txnType.Id;

                gateway.UpdatePaymentInfoFromPaymentControl( this.FinancialGateway, _hostedPaymentInfoControl, paymentInfo, out errorMessage );
                if ( errorMessage.IsNotNullOrWhiteSpace() )
                {
                    nbPaymentError.Text = errorMessage ?? "Unknown Error";
                    nbPaymentError.Visible = true;
                    return false;
                }

                var customerToken = gateway.CreateCustomerAccount( this.FinancialGateway, paymentInfo, out errorMessage );
                if ( errorMessage.IsNotNullOrWhiteSpace() || customerToken.IsNullOrWhiteSpace() )
                {
                    nbPaymentError.Text = errorMessage ?? "Unknown Error";
                    nbPaymentError.Visible = true;
                    return false;
                }

                paymentInfo.GatewayPersonIdentifier = customerToken;

                var gatewayTransaction = ProcessTransaction( gateway, rockContext, paymentInfo, amount, registrationChanges, out errorMessage );
                if ( gatewayTransaction == null )
                {
                    return false;
                }

                SaveTransaction( rockContext, registration, gatewayTransaction, personAliasId, amount );
            }
            else
            {
                var manualTransaction = ProcessManualTransaction( amount, registrationChanges );
                SaveTransaction( rockContext, registration, manualTransaction, personAliasId, amount );
            }

            if ( registrationChanges.Any() )
            {
                HistoryService.SaveChanges(
                    rockContext,
                    typeof( Registration ),
                    Rock.SystemGuid.Category.HISTORY_EVENT_REGISTRATION.AsGuid(),
                    registration.Id,
                    registrationChanges );
            }

            errorMessage = string.Empty;
            return true;
        }

        private FinancialTransaction ProcessTransaction( IHostedGatewayComponent gateway, RockContext rockContext, PaymentInfo paymentInfo, decimal amount, History.HistoryChangeList registrationChanges, out string errorMessage )
        {
            var transaction = gateway.Charge( this.RegistrationTemplate.FinancialGateway, paymentInfo, out errorMessage );
            if ( transaction != null )
            {
                transaction.FinancialGatewayId = this.RegistrationTemplate.FinancialGatewayId;
                if ( transaction.FinancialPaymentDetail == null )
                {
                    transaction.FinancialPaymentDetail = new FinancialPaymentDetail();
                }

                transaction.FinancialPaymentDetail.SetFromPaymentInfo( paymentInfo, gateway as GatewayComponent, rockContext );

                registrationChanges.AddChange( History.HistoryVerb.Process, History.HistoryChangeType.Record, string.Format( "Payment of {0}.", amount.FormatAsCurrency() ) );
            }

            return transaction;
        }

        private FinancialTransaction ProcessManualTransaction( decimal amount, History.HistoryChangeList registrationChanges )
        {
            var transaction = new FinancialTransaction();
            transaction.FinancialPaymentDetail = new FinancialPaymentDetail();
            transaction.FinancialPaymentDetail.CurrencyTypeValueId = dvpCurrencyType.SelectedValueAsInt();
            transaction.FinancialPaymentDetail.CreditCardTypeValueId = dvpCreditCardType.SelectedValueAsInt();
            transaction.TransactionCode = tbTransactionCode.Text;

            registrationChanges.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, string.Format( "Manual payment of {0}.", amount.FormatAsCurrency() ) );

            return transaction;
        }

        private bool SaveTransaction( RockContext rockContext, Registration registration, FinancialTransaction transaction, int? personAliasId, decimal amount )
        {
            if ( transaction == null )
            {
                return false;
            }

            transaction.Summary = GetInternalComment( registration );
            transaction.AuthorizedPersonAliasId = personAliasId;
            transaction.TransactionDateTime = RockDateTime.Now;

            var txnType = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_EVENT_REGISTRATION ) );
            transaction.TransactionTypeValueId = txnType.Id;

            Guid sourceGuid = Guid.Empty;
            if ( Guid.TryParse( GetAttributeValue( "Source" ), out sourceGuid ) )
            {
                var source = DefinedValueCache.Get( sourceGuid );
                if ( source != null )
                {
                    transaction.SourceTypeValueId = source.Id;
                }
            }

            var transactionDetail = new FinancialTransactionDetail();
            transactionDetail.Amount = amount;
            transactionDetail.AccountId = registration.RegistrationInstance.AccountId.Value;
            transactionDetail.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Registration ) ).Id;
            transactionDetail.EntityId = registration.Id;
            transaction.TransactionDetails.Add( transactionDetail );

            var batchService = new FinancialBatchService( rockContext );

            // determine batch prefix
            string batchPrefix = string.Empty;
            if ( !string.IsNullOrWhiteSpace( this.RegistrationTemplate.BatchNamePrefix ) )
            {
                batchPrefix = this.RegistrationTemplate.BatchNamePrefix;
            }
            else
            {
                batchPrefix = GetAttributeValue( "BatchNamePrefix" );
            }

            DefinedValueCache dvCurrencyType = ( transaction.FinancialPaymentDetail != null && transaction.FinancialPaymentDetail.CurrencyTypeValueId.HasValue ) ?
                DefinedValueCache.Get( transaction.FinancialPaymentDetail.CurrencyTypeValueId.Value ) : null;
            DefinedValueCache dvCredCardType = ( transaction.FinancialPaymentDetail != null && transaction.FinancialPaymentDetail.CreditCardTypeValueId.HasValue ) ?
                DefinedValueCache.Get( transaction.FinancialPaymentDetail.CreditCardTypeValueId.Value ) : null;

            // Get the batch
            var batch = batchService.GetForNewTransaction( transaction, batchPrefix );

            var batchChanges = new History.HistoryChangeList();
            FinancialBatchService.EvaluateNewBatchHistory( batch, batchChanges );

            decimal newControlAmount = batch.ControlAmount + transaction.TotalAmount;
            History.EvaluateChange( batchChanges, "Control Amount", batch.ControlAmount.FormatAsCurrency(), newControlAmount.FormatAsCurrency() );
            batch.ControlAmount = newControlAmount;

            var financialTransactionService = new FinancialTransactionService( rockContext );

            // If this is a new Batch, SaveChanges so that we can get the Batch.Id
            if ( batch.Id == 0 )
            {
                rockContext.SaveChanges();
            }

            transaction.BatchId = batch.Id;

            // use the financialTransactionService to add the transaction instead of batch.Transactions to avoid lazy-loading the transactions already associated with the batch
            financialTransactionService.Add( transaction );

            rockContext.SaveChanges();

            HistoryService.SaveChanges(
                rockContext,
                typeof( FinancialBatch ),
                Rock.SystemGuid.Category.HISTORY_FINANCIAL_BATCH.AsGuid(),
                batch.Id,
                batchChanges );

            return true;
        }

        #endregion Payment

        #region Payment Details

        /// <summary>
        /// Binds the payments grid.
        /// </summary>
        private void BindPaymentsGrid()
        {
            if ( Registration != null )
            {
                using ( var rockContext = new RockContext() )
                {
                    var currencyTypes = new Dictionary<int, string>();
                    var creditCardTypes = new Dictionary<int, string>();

                    int registrationEntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Registration ) ).Id;

                    var transactionDetailQuery = new FinancialTransactionDetailService( rockContext )
                        .Queryable()
                        .Where( d => d.EntityTypeId.HasValue &&
                                    d.EntityTypeId.Value == registrationEntityTypeId &&
                                    d.EntityId.HasValue &&
                                    d.EntityId.Value == Registration.Id );

                    var transactionIds = transactionDetailQuery.Select( a => a.TransactionId ).ToList();

                    // Get all the transactions related to this registration
                    var qry = new FinancialTransactionService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( t => transactionIds.Contains( t.Id ) );

                    SortProperty sortProperty = gPayments.SortProperty;
                    if ( sortProperty != null )
                    {
                        if ( sortProperty.Property == "TotalAmount" )
                        {
                            if ( sortProperty.Direction == SortDirection.Ascending )
                            {
                                qry = qry.OrderBy( t => t.TransactionDetails.Sum( d => ( decimal? ) d.Amount ) ?? 0.00M );
                            }
                            else
                            {
                                qry = qry.OrderByDescending( t => t.TransactionDetails.Sum( d => ( decimal? ) d.Amount ) ?? 0.0M );
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

                    var transactionList = qry.AsNoTracking().ToList();

                    gPayments.DataSource = transactionList.Select( p => new
                    {
                        p.Id,
                        TransactionDateTime = p.TransactionDateTime.Value.ToShortDateString() + "<br/>" +
                            p.TransactionDateTime.Value.ToShortTimeString(),
                        Details = FormatDetails( p ),
                        p.TotalAmount
                    } );

                    gPayments.DataBind();
                }
            }
        }

        private string FormatDetails( FinancialTransaction txn )
        {
            var details = new List<string>();

            if ( txn.AuthorizedPersonAlias != null && txn.AuthorizedPersonAlias.Person != null )
            {
                details.Add( txn.AuthorizedPersonAlias.Person.FullNameFormalReversed );
            }

            if ( txn.FinancialPaymentDetail != null )
            {
                details.Add( txn.FinancialPaymentDetail.CurrencyAndCreditCardType );
                details.Add( txn.FinancialPaymentDetail.AccountNumberMasked );
            }

            details.Add( txn.TransactionCode );

            string formattedDetails = details.Where( d => d != null && d != string.Empty ).ToList().AsDelimited( "<br/>" );
            if ( txn.RefundDetails != null )
            {
                return "<span class='label label-danger'>Refund</span> " + formattedDetails;
            }
            else
            {
                return formattedDetails;
            }
        }

        #endregion Payment Details

        #region Registration Detail Methods

        private void BindOtherInstances()
        {
            int? currentValue = ddlNewRegistrationInstance.SelectedValueAsInt();

            // list other registration instances
            using ( var rockContext = new RockContext() )
            {
                var otherRegistrationInstances = new RegistrationInstanceService( rockContext ).Queryable()
                        .Where( i =>
                            i.RegistrationTemplateId == Registration.RegistrationInstance.RegistrationTemplateId
                            && i.Id != Registration.RegistrationInstanceId );

                if ( !cbShowAll.Checked )
                {
                    otherRegistrationInstances = otherRegistrationInstances
                        .Where( i =>
                            i.IsActive == true
                            && ( i.EndDateTime >= RockDateTime.Now || i.EndDateTime == null ) );
                }

                var instances = otherRegistrationInstances
                        .Select( i => new
                        {
                            Value = i.Id,
                            Text = i.Name
                        } )
                        .ToList();

                ddlNewRegistrationInstance.DataSource = instances;
                ddlNewRegistrationInstance.DataBind();

                ddlNewRegistrationInstance.Items.Insert( 0, new ListItem( string.Empty, string.Empty ) );
                ddlNewRegistrationInstance.SetValue( currentValue );
            }
        }

        #endregion Registration Detail Methods

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
                    if ( registrant.OnWaitList )
                    {
                        costSummary.Description += " (Waiting List)";
                        costSummary.Cost = 0.0M;
                        costSummary.DiscountedCost = 0.0M;
                    }
                    else
                    {
                        costSummary.Cost = registrant.Cost;
                        if ( registration.DiscountPercentage > 0.0m && registrant.DiscountApplies )
                        {
                            costSummary.DiscountedCost = costSummary.Cost - ( costSummary.Cost * registration.DiscountPercentage );
                        }
                        else
                        {
                            costSummary.DiscountedCost = costSummary.Cost;
                        }
                    }

                    costs.Add( costSummary );
                }

                foreach ( var fee in registrant.FeeValues )
                {
                    var templateFee = this.RegistrationTemplate.Fees.Where( f => f.Id == fee.Key ).FirstOrDefault();
                    if ( fee.Value != null )
                    {
                        foreach ( var feeInfo in fee.Value )
                        {
                            decimal cost = feeInfo.PreviousCost > 0.0m ? feeInfo.PreviousCost : feeInfo.Cost;
                            string feeName;
                            if ( templateFee != null )
                            {
                                feeName = templateFee.Name;

                                if ( templateFee.FeeType == RegistrationFeeType.Multiple && feeInfo.FeeLabel.IsNotNullOrWhiteSpace() )
                                {
                                    feeName = string.Format( "{0}-{1}", templateFee.Name, feeInfo.FeeLabel );
                                }
                            }
                            else
                            {
                                feeName = "(Previous Cost)";
                                if ( feeInfo.FeeLabel.IsNotNullOrWhiteSpace() )
                                {
                                    feeName += "-" + feeInfo.FeeLabel;
                                }
                            }

                            string desc = string.Format( "{0} ({1:N0} @ {2})", feeName, feeInfo.Quantity, cost.FormatAsCurrency() );

                            var costSummary = new RegistrationCostSummaryInfo();
                            costSummary.Type = RegistrationCostSummaryType.Fee;
                            costSummary.Description = desc;
                            costSummary.Cost = feeInfo.Quantity * cost;

                            if ( registration.DiscountPercentage > 0.0m && templateFee != null && templateFee.DiscountApplies && registrant.DiscountApplies )
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

            // Show Fees Summary
            var hasFees = costs.Any();

            nbNoCost.Visible = !hasFees;
            pnlCosts.Visible = hasFees;

            // Get the total min payment for all costs and fees
            decimal minPayment = costs.Sum( c => c.MinPayment );

            // Add row for amount discount
            if ( registration.DiscountAmount > 0.0m )
            {
                decimal totalDiscount = 0.0m - ( RegistrantsState.Where( r => r.DiscountApplies ).Count() * registration.DiscountAmount );
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
            lTotalCost.Text = registration.DiscountedCost.FormatAsCurrency();
            lPreviouslyPaid.Text = registration.TotalPaid.FormatAsCurrency();
            lRemainingDue.Text = balanceDue.FormatAsCurrency();

            if ( registration.BalanceDue > 0.0m &&
                registration != null &&
                registration.RegistrationInstance != null &&
                registration.RegistrationInstance.AccountId.HasValue &&
                registration.PersonAliasId.HasValue &&
                this.RegistrationTemplate != null &&
                EditAllowed )
            {
                lbAddPayment.Visible = true;

                lbProcessPayment.Visible = this.RegistrationTemplate.FinancialGateway != null;
                var isHostedGateway = this.FinancialGatewayComponent?.GetSupportedHostedGatewayModes( this.RegistrationTemplate.FinancialGateway ).Contains( HostedGatewayMode.Hosted ) ?? false;
                if ( !isHostedGateway )
                {
                    lbProcessPayment.Enabled = false;
                    lbProcessPaymentButtonTooltipWrapper.Attributes["title"] = "The payment gateway used by this event's registration template does not support making payments here.";
                }

                nbNoAssociatedPerson.Visible = false;
            }
            else
            {
                lbAddPayment.Visible = false;
                lbProcessPayment.Visible = false;
                nbNoAssociatedPerson.Visible = registration.BalanceDue > 0.0m ? true : false;
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
            h1Heading.InnerHtml = "<i class='fa fa-user'></i> " + registrant.PersonName;
            divHeading.Controls.Add( h1Heading );

            var divLabels = new HtmlGenericControl( "div" );
            divLabels.AddCssClass( "panel-labels" );
            divHeading.Controls.Add( divLabels );

            if ( registrant.OnWaitList )
            {
                var hlOnWaitList = new HighlightLabel();
                hlOnWaitList.ID = string.Format( "hlWaitList_{0}", registrant.Id );
                hlOnWaitList.LabelType = LabelType.Warning;
                hlOnWaitList.Text = "Wait List";
                hlOnWaitList.CssClass = "margin-r-sm";
                divLabels.Controls.Add( hlOnWaitList );
            }

            decimal discountedTotalCost = registrant.DiscountedTotalCost( Registration.DiscountPercentage, Registration.DiscountAmount );
            if ( discountedTotalCost != 0.0m )
            {
                var hlCost = new HighlightLabel();
                hlCost.ID = string.Format( "hlCost_{0}", registrant.Id );
                hlCost.LabelType = LabelType.Info;
                hlCost.ToolTip = "Cost";
                hlCost.Text = discountedTotalCost.FormatAsCurrency();
                divLabels.Controls.Add( hlCost );
            }

            if ( registrant.PersonId.HasValue )
            {
                var aProfileLink = new HtmlAnchor();
                aProfileLink.HRef = ResolveRockUrl( string.Format( "~/Person/{0}", registrant.PersonId.Value ) );
                divLabels.Controls.Add( aProfileLink );
                aProfileLink.AddCssClass( "btn btn-default btn-xs btn-square margin-l-sm" );
                var iProfileLink = new HtmlGenericControl( "i" );
                iProfileLink.AddCssClass( "fa fa-user" );
                aProfileLink.Controls.Add( iProfileLink );
            }

            var divBody = new HtmlGenericControl( "div" );
            divBody.AddCssClass( "panel-body" );
            divPanel.Controls.Add( divBody );

            SignatureDocumentTemplate documentTemplate = null;

            if ( Registration != null &&
                Registration.RegistrationInstance != null &&
                Registration.RegistrationInstance.RegistrationTemplate != null &&
                Registration.RegistrationInstance.RegistrationTemplate.RequiredSignatureDocumentTemplate != null )
            {
                documentTemplate = Registration.RegistrationInstance.RegistrationTemplate.RequiredSignatureDocumentTemplate;
            }

            if ( documentTemplate != null && !registrant.SignatureDocumentId.HasValue && documentTemplate.IsLegacy )
            {
                var template = Registration.RegistrationInstance.RegistrationTemplate;
                var divSigAlert = new HtmlGenericControl( "div" );
                divSigAlert.AddCssClass( "alert alert-warning" );
                divBody.Controls.Add( divSigAlert );

                StringBuilder sb = new StringBuilder();
                sb.Append( "<div class='row'><div class='col-md-9'>" );

                sb.AppendFormat(
                    "There is not a signed {0} for {1}",
                    template.RequiredSignatureDocumentTemplate.Name,
                    registrant.GetFirstName( template ) );

                if ( registrant.SignatureDocumentLastSent.HasValue )
                {
                    sb.AppendFormat(
                        " (a request was sent {0})",
                        registrant.SignatureDocumentLastSent.Value.ToElapsedString() );
                }

                sb.Append( ".</div>" );

                divSigAlert.Controls.Add( new LiteralControl( sb.ToString() ) );

                var divSigAction = new HtmlGenericControl( "div" );
                divSigAction.AddCssClass( "col-md-3 text-right" );
                divSigAlert.Controls.Add( divSigAction );

                var lbResendDocumentRequest = new LinkButton();
                lbResendDocumentRequest.CausesValidation = false;
                lbResendDocumentRequest.ID = string.Format( "lbResendDocumentRequest_{0}", registrant.Id );
                lbResendDocumentRequest.Text = registrant.SignatureDocumentLastSent.HasValue ? "Resend Signature Request" : "Send Signature Request";
                lbResendDocumentRequest.CssClass = "btn btn-warning btn-sm";
                lbResendDocumentRequest.Click += lbResendDocumentRequest_Click;
                divSigAction.Controls.Add( lbResendDocumentRequest );

                divSigAlert.Controls.Add( new LiteralControl( "</div>" ) );
            }

            var divRow = new HtmlGenericControl( "div" );
            divRow.AddCssClass( "row" );
            divBody.Controls.Add( divRow );

            var divLeftColumn = new HtmlGenericControl( "div" );
            divLeftColumn.AddCssClass( "col-md-6" );
            divRow.Controls.Add( divLeftColumn );

            var divRightColumn = new HtmlGenericControl( "div" );
            divRightColumn.AddCssClass( "col-md-6" );
            divRow.Controls.Add( divRightColumn );

            if ( this.RegistrationTemplate != null &&
                this.RegistrationTemplate.GroupTypeId.HasValue &&
                Registration != null &&
                Registration.Group != null &&
                Registration.Group.GroupTypeId == this.RegistrationTemplate.GroupTypeId.Value )
            {
                if ( Registration != null && Registration.Group != null )
                {
                    var rcwGroupMember = new RockControlWrapper();
                    rcwGroupMember.ID = string.Format( "rcwGroupMember_{0}", registrant.Id );
                    divLeftColumn.Controls.Add( rcwGroupMember );
                    rcwGroupMember.Label = "Group";

                    var pGroupMember = new HtmlGenericControl( "p" );
                    pGroupMember.ID = string.Format( "pGroupMember_{0}", registrant.Id );
                    divRow.AddCssClass( "form-control-static" );
                    rcwGroupMember.Controls.Add( pGroupMember );

                    if ( registrant.GroupMemberId.HasValue )
                    {
                        var qryParams = new Dictionary<string, string>();
                        qryParams.Add( "GroupMemberId", registrant.GroupMemberId.Value.ToString() );

                        var aProfileLink = new HtmlAnchor();
                        aProfileLink.HRef = LinkedPageUrl( "GroupMemberPage", qryParams );
                        pGroupMember.Controls.Add( aProfileLink );
                        aProfileLink.Controls.Add( new LiteralControl( string.IsNullOrWhiteSpace( registrant.GroupName ) ? "Group" : registrant.GroupName ) );
                    }
                    else
                    {
                        pGroupMember.Controls.Add( new LiteralControl( "None (" ) );

                        var lbGroupMember = new LinkButton();
                        lbGroupMember.CausesValidation = false;
                        lbGroupMember.ID = string.Format( "lbGroupMember_{0}", registrant.Id );
                        lbGroupMember.Text = string.Format( "Add {0} to Target Group", registrant.GetFirstName( this.RegistrationTemplate ) );
                        lbGroupMember.Click += lbGroupMember_Click;
                        pGroupMember.Controls.Add( lbGroupMember );

                        pGroupMember.Controls.Add( new LiteralControl( ")" ) );
                    }
                }
            }

            foreach ( var form in this.RegistrationTemplate.Forms.OrderBy( f => f.Order ) )
            {
                foreach ( var field in form.Fields.OrderBy( f => f.Order ) )
                {
                    var fieldControl = BuildRegistrantFieldControl( field, registrant, setValues );
                    if ( fieldControl != null )
                    {
                        divLeftColumn.Controls.Add( fieldControl );
                    }
                }
            }

            if ( registrant.Cost > 0.0m )
            {
                var rlCost = new RockLiteral();
                rlCost.ID = string.Format( "rlCost_{0}", registrant.Id );
                rlCost.Label = "Cost";
                rlCost.Text = registrant.Cost.FormatAsCurrency();

                decimal discountedCost = registrant.DiscountedCost( Registration.DiscountPercentage, Registration.DiscountAmount );
                if ( registrant.Cost == discountedCost )
                {
                    var divCost = new HtmlGenericControl( "div" );
                    divCost.AddCssClass( "col-xs-12" );
                    divCost.Controls.Add( rlCost );
                    divRightColumn.Controls.Add( divCost );
                }
                else
                {
                    var rlDiscountedCost = new RockLiteral();
                    rlDiscountedCost.ID = string.Format( "rlDiscountedCost_{0}", registrant.Id );
                    rlDiscountedCost.Label = "Discounted Cost";
                    rlDiscountedCost.Text = discountedCost.FormatAsCurrency();

                    var divCost = new HtmlGenericControl( "div" );
                    divCost.AddCssClass( "col-xs-6" );
                    divCost.Controls.Add( rlCost );
                    divRightColumn.Controls.Add( divCost );

                    var divDiscountedCost = new HtmlGenericControl( "div" );
                    divDiscountedCost.AddCssClass( "col-xs-6" );
                    divDiscountedCost.Controls.Add( rlDiscountedCost );
                    divRightColumn.Controls.Add( divDiscountedCost );
                }
            }

            foreach ( var fee in registrant.FeeValues )
            {
                var templateFee = this.RegistrationTemplate.Fees.Where( f => f.Id == fee.Key ).FirstOrDefault();
                if ( templateFee != null && fee.Value != null )
                {
                    foreach ( var feeInfo in fee.Value )
                    {
                        var discountedCost = registrant.DiscountApplies ? feeInfo.DiscountedCost( Registration.DiscountPercentage ) : feeInfo.TotalCost;
                        var feeControl = BuildRegistrantFeeControl( templateFee, feeInfo, registrant, setValues );
                        if ( feeControl != null )
                        {
                            if ( feeInfo.TotalCost == discountedCost )
                            {
                                var divFee = new HtmlGenericControl( "div" );
                                divFee.AddCssClass( "col-xs-12" );
                                divFee.Controls.Add( feeControl );
                                divRightColumn.Controls.Add( divFee );
                            }
                            else
                            {
                                var rlDiscountedFee = new RockLiteral();
                                rlDiscountedFee.ID = string.Format( "rlDiscountedFee_{0}_{1}_{2}", registrant.Id, templateFee.Id, feeInfo.FeeLabel );
                                rlDiscountedFee.Label = "Discounted Amount";
                                rlDiscountedFee.Text = discountedCost.FormatAsCurrency();

                                var divFee = new HtmlGenericControl( "div" );
                                divFee.AddCssClass( "col-xs-6" );
                                divFee.Controls.Add( feeControl );
                                divRightColumn.Controls.Add( divFee );

                                var divDiscountedFee = new HtmlGenericControl( "div" );
                                divDiscountedFee.AddCssClass( "col-xs-6" );
                                divDiscountedFee.Controls.Add( rlDiscountedFee );
                                divRightColumn.Controls.Add( divDiscountedFee );
                            }
                        }
                    }
                }
            }

            if ( documentTemplate != null )
            {
                var rlDocumentLink = new RockLiteral();
                rlDocumentLink.ID = string.Format( "rlDocumentLink_{0}", registrant.Id );
                rlDocumentLink.Label = documentTemplate.Name;

                const string htmlFormat = @"
    <div class='icon-property'>
        <div class='icon' style='background: {1}; color: {0};'>
            <i class='fa fa-signature'></i>
        </div>
        <div class='property'>
            {2}
        </div>
    </div>";
                string borderColor = string.Empty;
                string backgroundColor = string.Empty;
                string links = string.Empty;

                if ( registrant.SignatureDocumentId.HasValue )
                {
                    links = string.Format(
                        @"<a href='{0}' target='_blank' rel='noopener noreferrer'>Signed on {1}</a>
                        <small>Signed by {2}</small>",
                        ResolveRockUrl( string.Format( "~/GetFile.ashx?id={0}", registrant.SignatureDocumentId ?? 0 ) ),
                        registrant.SignatureDocumentSignedDateTime?.ToString( "dddd, MMMM dd, yyyy" ),
                        registrant.SignatureDocumentSignedName );

                    borderColor = "#16C98D";
                    backgroundColor = "#D6FFF1";
                }
                else
                {
                    links = "<span>Not yet Signed</span>";
                    borderColor = "#737475";
                    backgroundColor = "#DFE0E1";
                }

                rlDocumentLink.Text = string.Format(
                        htmlFormat,
                        borderColor,
                        backgroundColor,
                        links );
                divRightColumn.Controls.Add( rlDocumentLink );
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
                rlField.ID = string.Format( "rlFee_{0}_{1}_{2}", registrant.Id, fee.Id, feeInfo.FeeLabel );

                rlField.Label = fee.Name;

                if ( fee.FeeType == RegistrationFeeType.Multiple && feeInfo.FeeLabel.IsNotNullOrWhiteSpace() )
                {
                    rlField.Label = string.Format( "{0}-{1}", fee.Name, feeInfo.FeeLabel );
                }

                if ( feeInfo.Quantity > 1 )
                {
                    rlField.Text = string.Format(
                    "({0:N0} @ {1}) {2}",
                    feeInfo.Quantity,
                    feeInfo.Cost.FormatAsCurrency(),
                    feeInfo.TotalCost.FormatAsCurrency() );
                }
                else
                {
                    rlField.Text = feeInfo.TotalCost.FormatAsCurrency();
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
                fieldValue = registrant.FieldValues[field.Id].FieldValue;
            }

            if ( fieldValue != null )
            {
                var rlField = new RockLiteral();
                rlField.ID = string.Format( "rlField_{0}_{1}", registrant.Id, field.Id );

                if ( field.FieldSource == RegistrationFieldSource.PersonField )
                {
                    rlField.Label = field.PersonFieldType.ConvertToString( true );

                    switch ( field.PersonFieldType )
                    {
                        case RegistrationPersonFieldType.MiddleName:
                            rlField.Text = fieldValue.ToString() ?? string.Empty;
                            break;

                        case RegistrationPersonFieldType.Campus:
                            var campus = CampusCache.Get( fieldValue.ToString().AsInteger() );
                            rlField.Text = campus != null ? campus.Name : string.Empty;
                            break;

                        case RegistrationPersonFieldType.Address:
                            var location = fieldValue.ToString();
                            rlField.Text = location != null ? location.ToString() : string.Empty;
                            break;

                        case RegistrationPersonFieldType.Email:
                            rlField.Text = fieldValue.ToString();
                            break;

                        case RegistrationPersonFieldType.Birthdate:
                            var birthDate = fieldValue as DateTime?;
                            rlField.Text = birthDate != null ? birthDate.Value.ToShortDateString() : string.Empty;
                            break;

                        case RegistrationPersonFieldType.Grade:
                            int? graduationYear = fieldValue.ToString().AsIntegerOrNull();
                            rlField.Text = Person.GradeFormattedFromGraduationYear( graduationYear );
                            break;

                        case RegistrationPersonFieldType.Gender:
                            var gender = fieldValue.ToString().ConvertToEnumOrNull<Gender>() ?? Gender.Unknown;
                            rlField.Text = gender.ConvertToString();
                            break;

                        case RegistrationPersonFieldType.MaritalStatus:
                            var maritalStatusDv = DefinedValueCache.Get( fieldValue.ToString().AsInteger() );
                            rlField.Text = maritalStatusDv != null ? maritalStatusDv.Value : string.Empty;
                            break;

                        case RegistrationPersonFieldType.AnniversaryDate:
                            var anniversaryDate = fieldValue as DateTime?;
                            rlField.Text = anniversaryDate != null ? anniversaryDate.Value.ToShortDateString() : string.Empty;
                            break;

                        case RegistrationPersonFieldType.MobilePhone:
                        case RegistrationPersonFieldType.HomePhone:
                        case RegistrationPersonFieldType.WorkPhone:
                            var pn = fieldValue as PhoneNumber;
                            rlField.Text = pn != null ? pn.NumberFormatted : string.Empty;
                            break;

                        case RegistrationPersonFieldType.ConnectionStatus:
                            var connectionStatus = DefinedValueCache.Get( fieldValue.ToString().AsInteger() );
                            rlField.Text = connectionStatus != null ? connectionStatus.Value : string.Empty;
                            break;
                    }
                }
                else
                {
                    if ( field.AttributeId.HasValue )
                    {
                        var attribute = AttributeCache.Get( field.AttributeId.Value );
                        if ( attribute == null )
                        {
                            return null;
                        }

                        rlField.Label = attribute.Name;
                        rlField.Text = attribute.FieldType.Field.FormatValueAsHtml( null, attribute.EntityTypeId, registrant.Id, fieldValue.ToString(), attribute.QualifierValues );
                    }
                }

                if ( !string.IsNullOrWhiteSpace( rlField.Text ) )
                {
                    return rlField;
                }
            }

            return null;
        }

        /// <summary>
        /// Set the active state of the account tabs and controls.
        /// </summary>
        /// <param name="tab"></param>
        private void SetActiveAccountPanel( RegistrationDetailAccountPanelSpecifier tab )
        {
            ShowTabAccont( false );
            ShowTabFees( false );
            ShowTabPayments( false );

            pnlPaymentInfo.Visible = false;
            phPaymentAmount.Visible = false;
            phManualDetails.Visible = false;
            pnlCCDetails.Visible = false;

            switch ( tab )
            {
                case RegistrationDetailAccountPanelSpecifier.AccountSummary:
                    ShowTabAccont( true );
                    break;

                case RegistrationDetailAccountPanelSpecifier.FeeList:
                    ShowTabFees( true );
                    break;

                case RegistrationDetailAccountPanelSpecifier.PaymentList:
                    ShowTabPayments( true );
                    break;

                case RegistrationDetailAccountPanelSpecifier.PaymentManualDetails:
                    ShowTabPayments( true );
                    pnlPaymentInfo.Visible = true;
                    phPaymentAmount.Visible = true;
                    phManualDetails.Visible = true;
                    break;

                case RegistrationDetailAccountPanelSpecifier.PaymentProcess:
                    ShowTabPayments( true );
                    pnlPaymentInfo.Visible = true;
                    phPaymentAmount.Visible = true;
                    pnlCCDetails.Visible = true;
                    break;

                default:
                    break;
            }

            pnlPaymentDetails.Visible = !pnlPaymentInfo.Visible;

            lnkTabAccount.Attributes["href"] = "#" + tabPaneAccount.ClientID;
            lnkTabFees.Attributes["href"] = "#" + tabPaneFees.ClientID;
            lnkTabPayments.Attributes["href"] = "#" + tabPanePayments.ClientID;
        }

        private void ShowTabAccont( bool showTabAccount )
        {
            if ( showTabAccount )
            {
                tabAccount.AddCssClass( "active" );
                tabPaneAccount.AddCssClass( "active" );
            }
            else
            {
                tabAccount.RemoveCssClass( "active" );
                tabPaneAccount.RemoveCssClass( "active" );
            }
        }

        private void ShowTabFees( bool showTabFees )
        {
            if ( showTabFees )
            {
                tabFees.AddCssClass( "active" );
                tabPaneFees.AddCssClass( "active" );
            }
            else
            {
                tabFees.RemoveCssClass( "active" );
                tabPaneFees.RemoveCssClass( "active" );
            }
        }

        private void ShowTabPayments( bool showTabPayments )
        {
            if ( showTabPayments )
            {
                tabPayments.AddCssClass( "active" );
                tabPanePayments.AddCssClass( "active" );
            }
            else
            {
                tabPayments.RemoveCssClass( "active" );
                tabPanePayments.RemoveCssClass( "active" );
            }
        }

        #endregion Dynamic Controls

        #region Support Classes and Enumerations

        public enum RegistrationDetailAccountPanelSpecifier
        {
            AccountSummary,
            FeeList,
            PaymentList,
            PaymentManualDetails,
            PaymentProcess
        }

        #endregion Support Classes and Enumerations
    }
}