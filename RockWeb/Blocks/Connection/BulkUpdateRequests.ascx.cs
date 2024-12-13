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
//

using Humanizer;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Transactions;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RockWeb.Blocks.Connection
{
    /// <summary>
    /// Block for bulk update of multiple connection requests
    /// </summary>
    /// <seealso cref="Rock.Web.UI.RockBlock" />
    [DisplayName( "Bulk Update Requests" )]
    [Category( "Connection" )]
    [Description( "Used for updating information about several Connection Requests at once. The QueryString must have both the EntitySetId as well as the ConnectionTypeId, and all the connection requests must be for the same opportunity." )]

    #region Block Attributes

    [LinkedPage(
        "Previous Page",
        Key = AttributeKeys.PreviousPage,
        Order = 1,
        DefaultValue = Rock.SystemGuid.Page.CONNECTIONS_BOARD )]

    #endregion

    [Rock.SystemGuid.BlockTypeGuid( "175158F8-F10E-476F-809E-A76825E0AC5D" )]
    public partial class BulkUpdateRequests : RockBlock
    {
        #region AttributeKeys

        private static class AttributeKeys
        {
            public const string PreviousPage = "PreviousPage";
        }

        #endregion AttributeKeys

        #region PageParameterKeys

        /// <summary>
        /// A defined list of page parameter keys used by this block.
        /// </summary>
        private static class PageParameterKey
        {
            public const string ConnectionTypeId = "ConnectionTypeId";
            public const string EntitySetId = "EntitySetId";
        }

        private static class ViewStateKeys
        {
            public const string CONNECTION_REQUEST_IDS_KEY = "ConnectionRequestIdsKey";
            public const string CONNECTION_CAMPUS_COUNT_KEY = "ConnectionCampusCount";
        }

        #endregion PageParameterKeys

        #region Properties

        private ConnectionOpportunity ConnectionOpportunity { get; set; }
        private List<ConnectionActivityType> ConnectionActivityTypes { get; set; } = new List<ConnectionActivityType>();
        private List<ConnectionCampusCountViewModel> ConnectionCampusCountViewModelsState { get; set; } = new List<ConnectionCampusCountViewModel>();
        private List<int> RequestIdsState { get; set; } = new List<int>();
        private List<string> SelectedFields { get; set; } = new List<string>();

        #endregion Properties

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            var script = string.Format( @"

    // Add the 'bulk-item-selected' class to form-group of any item selected after postback
    $( 'label.control-label' ).has( 'span.js-select-item > i.fa-check-circle-o').each( function() {{
        $(this).closest('.form-group').addClass('bulk-item-selected');
    }});

    // Handle the click event for any label that contains a 'js-select-span' span
    $( 'label.control-label' ).has( 'span.js-select-item').on('click', function() {{

        var formGroup = $(this).closest('.form-group');
        var selectIcon = formGroup.find('span.js-select-item').children('i');

        // Toggle the selection of the form group
        formGroup.toggleClass('bulk-item-selected');
        var enabled = formGroup.hasClass('bulk-item-selected');

        // Set the selection icon to show selected
        selectIcon.toggleClass('fa-check-circle-o', enabled);
        selectIcon.toggleClass('fa-circle-o', !enabled);

        // Checkboxes needs special handling
        var checkboxes = formGroup.find(':checkbox');
        if ( checkboxes.length ) {{
            $(checkboxes).each(function() {{
                if (this.nodeName === 'INPUT' ) {{
                    $(this).toggleClass('aspNetDisabled', !enabled);
                    $(this).prop('disabled', !enabled);
                    $(this).closest('label').toggleClass('text-muted', !enabled);
                    $(this).closest('.form-group').toggleClass('bulk-item-selected', enabled);
                }}
            }});
        }}

        // Enable/Disable the controls
        formGroup.find('.form-control').each( function() {{

            $(this).toggleClass('aspNetDisabled', !enabled);
            $(this).prop('disabled', !enabled);

        }});

        // Update the hidden field with the client id of each selected control, (if client id ends with '_hf' as in the case of multi-select attributes, strip the ending '_hf').
        var newValue = '';
        $('div.bulk-item-selected').each(function( index ) {{
            $(this).find('[id]').each(function() {{
                var re = /_hf$/;
                var ctrlId = $(this).prop('id').replace(re, '');
                newValue += ctrlId + '|';
            }});
        }});
        $('#{0}').val(newValue);
        if($(this).closest('.form-group.attribute-matrix-editor').length){{
        __doPostBack('{1}', null);
        }}
    }});
", hfSelectedItems.ClientID, pnlEntry.ClientID );
            ScriptManager.RegisterStartupScript( hfSelectedItems, hfSelectedItems.GetType(), "select-items-" + BlockId.ToString(), script, true );

            this.BlockUpdated += Block_BlockUpdated;
        }

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Block_BlockUpdated( object sender, EventArgs e )
        {
            SetControlSelection();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                GetDetails();
            }
            else
            {
                var causingControlClientId = Request["__EVENTTARGET"].ToStringSafe();
                var causingControl = Page.FindControl( causingControlClientId );

                // If postback is due to a campus change rebind opportunity connector, this will refresh the available connector list based on the campus.
                if ( causingControl.ClientID == cblBulkUpdateCampuses.ClientID )
                {
                    RebindOpportunityConnector();
                }
            }

            SetControlSelection();

            base.OnLoad( e );
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns <see langword="null" />.
        /// </returns>
        protected override object SaveViewState()
        {
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState[ViewStateKeys.CONNECTION_REQUEST_IDS_KEY] = JsonConvert.SerializeObject( RequestIdsState, Formatting.None, jsonSetting );
            ViewState[ViewStateKeys.CONNECTION_CAMPUS_COUNT_KEY] = JsonConvert.SerializeObject( ConnectionCampusCountViewModelsState, Formatting.None, jsonSetting );

            return base.SaveViewState();
        }

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            var rockContext = new RockContext();
            var entitySetId = PageParameter( PageParameterKey.EntitySetId ).AsInteger();
            string json = ViewState[ViewStateKeys.CONNECTION_CAMPUS_COUNT_KEY] as string;

            if ( string.IsNullOrWhiteSpace(json) )
            {
                BindCampusSelector( entitySetId, rockContext );
            }
            else
            {
                ConnectionCampusCountViewModelsState = JsonConvert.DeserializeObject<List<ConnectionCampusCountViewModel>>( json );
                AddCampusSelectorControls( ConnectionCampusCountViewModelsState );
            }

            json = ViewState[ViewStateKeys.CONNECTION_REQUEST_IDS_KEY] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                RequestIdsState = GetConnectionRequestIds( entitySetId, rockContext );
            }
            else
            {
                RequestIdsState = JsonConvert.DeserializeObject<List<int>>( json );
            }

            string selectedItemsValue = Request.Form[hfSelectedItems.UniqueID];
            if ( !string.IsNullOrWhiteSpace( selectedItemsValue ) )
            {
                SelectedFields = selectedItemsValue.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
            }
            else
            {
                SelectedFields = new List<string>();
            }
        }

        #endregion Control Methods

        #region Events

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlOpportunity control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlOpportunity_SelectedIndexChanged( object sender, EventArgs e )
        {
            RebindOpportunityConnector();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlState control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlState_SelectedIndexChanged( object sender, EventArgs e )
        {
            var isFutureFollowUp = !ddlState.SelectedValue.IsNullOrWhiteSpace() && ddlState.SelectedValueAsEnum<ConnectionState>() == ConnectionState.FutureFollowUp;
            dpFollowUpDate.Visible = isFutureFollowUp;
            dpFollowUpDate.Required = isFutureFollowUp;
        }

        /// <summary>
        /// Handles the Click event of the btnBulkRequestUpdateCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnBulkRequestUpdateCancel_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKeys.PreviousPage );
        }

        /// <summary>
        /// Handles the Click event of the btnBulkRequestUpdateSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnBulkRequestUpdateSave_Click( object sender, EventArgs e )
        {
            if ( !Page.IsValid )
            {
                return;
            }

            var currentOpportunityId = ConnectionCampusCountViewModelsState.FirstOrDefault().OpportunityId;
            var selectedOpportunity = ddlOpportunity.SelectedValue.AsInteger();
            var selectedCampusIds = cblBulkUpdateCampuses.SelectedValuesAsInt;
            var includeNoCampus = cblBulkUpdateCampuses.SelectedValues.Exists( v => v.IsNullOrWhiteSpace() );

            bool hasChangeValue = SelectedFields.Count > 0 || currentOpportunityId != selectedOpportunity || !rbBulkUpdateCurrentConnector.Checked || cbAddActivity.Checked || wtpLaunchWorkflow.SelectedValue != "0";

            if ( hasChangeValue )
            {
                var rockContext = new RockContext();
                List<string> changes = GetChangesSummary( rockContext );

                var requestCount = new ConnectionRequestService( rockContext ).Queryable()
                    .AsNoTracking()
                    .Where( cr => RequestIdsState.Contains( cr.Id ) && ( ( includeNoCampus && !cr.CampusId.HasValue ) || selectedCampusIds.Contains( cr.CampusId.Value ) ) )
                    .Count();

                StringBuilder sb = new StringBuilder();
                sb.AppendFormat( "<p>You are about to make the following updates to {0} connection requests:</p>", requestCount.ToString( "N0" ) );
                sb.AppendLine();

                sb.AppendLine( "<ul>" );
                changes.ForEach( c => sb.AppendFormat( "<li>{0}</li>\n", c ) );
                sb.AppendLine( "</ul>" );

                sb.AppendLine( "<p>Please confirm that you want to make these updates.</p>" );

                phConfirmation.Controls.Add( new LiteralControl( sb.ToString() ) );

                nbBulkUpdateNotification.Visible = false;
                nbStatusUpdate.Visible = false;
                mdConfirmUpdateRequests.Show();
            }
            else
            {
                ShowNotification( NotificationBoxType.Info, "You have not selected anything to update." );
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event of the cbAddActivity control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbAddActivity_CheckedChanged( object sender, EventArgs e )
        {
            if ( cbAddActivity.Checked )
            {
                GetActivityDetails();
            }

            dvActivity.Visible = cbAddActivity.Checked;
        }

        /// <summary>
        /// Handles the Click event of the btnConfirm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnConfirm_Click( object sender, EventArgs e )
        {
            if ( mdConfirmUpdateRequests.SaveButtonText == "Ok" )
            {
                mdConfirmUpdateRequests.SaveButtonText = "Confirm";
                mdConfirmUpdateRequests.CancelLinkVisible = true;
                mdConfirmUpdateRequests.Hide();
                return;
            }

            var rockContext = new RockContext();

            var selectedCampusIds = cblBulkUpdateCampuses.SelectedValuesAsInt;
            var includeNoCampus = cblBulkUpdateCampuses.SelectedValues.Exists( v => v.IsNullOrWhiteSpace() );
            var connectionRequestService = new ConnectionRequestService( rockContext );
            var connectionRequestActivityService = new ConnectionRequestActivityService( rockContext );
            var connectionOpportunity = GetConnectionOpportunity(rockContext );

            var guid = Rock.SystemGuid.ConnectionActivityType.BULK_UPDATE.AsGuid();
            var bulkUpdateActivityId = new ConnectionActivityTypeService( rockContext ).Queryable()
                .AsNoTracking()
                .Where( t => t.Guid == guid )
                .Select( t => t.Id )
                .FirstOrDefault();

            var connectionRequests = connectionRequestService.Queryable()
                .Include( cr => cr.Campus )
                .Where( cr => RequestIdsState.Contains( cr.Id ) && ( ( includeNoCampus && !cr.CampusId.HasValue ) || selectedCampusIds.Contains( cr.CampusId.Value ) ) )
                .ToList();

            foreach ( var connectionRequest in connectionRequests )
            {
                connectionRequest.ConnectionOpportunityId = ddlOpportunity.SelectedValue.AsInteger();

                if ( !string.IsNullOrWhiteSpace( ddlStatus.SelectedValue ) )
                {
                    connectionRequest.ConnectionStatusId = ddlStatus.SelectedValue.AsInteger();
                }

                if ( !string.IsNullOrWhiteSpace( ddlState.SelectedValue ) )
                {
                    connectionRequest.ConnectionState = ddlState.SelectedValue.ConvertToEnum<ConnectionState>();
                    if ( connectionRequest.ConnectionState == ConnectionState.FutureFollowUp )
                    {
                        connectionRequest.FollowupDate = dpFollowUpDate.SelectedDate;
                    }
                }

                if ( !rbBulkUpdateCurrentConnector.Checked )
                {
                    if ( rbBulkUpdateDefaultConnector.Checked )
                    {
                        connectionRequest.ConnectorPersonAliasId = GetConnectionOpportunity( rockContext ).GetDefaultConnectorPersonAliasId( connectionRequest.CampusId );
                    }
                    else if ( rbBulkUpdateSelectConnector.Checked )
                    {
                        int? connectorPersonId = ddlBulkUpdateOpportunityConnector.SelectedValue.AsIntegerOrNull();
                        int? connectorPersonAliasId = null;
                        if ( connectorPersonId.HasValue )
                        {
                            var connectorPerson = new PersonService( rockContext ).Get( connectorPersonId.Value );
                            if ( connectorPerson != null )
                            {
                                connectorPersonAliasId = connectorPerson.PrimaryAliasId;
                            }
                        }

                        connectionRequest.ConnectorPersonAliasId = connectorPersonAliasId;
                    }
                    else if ( rbBulkUpdateNoConnector.Checked )
                    {
                        connectionRequest.ConnectorPersonAliasId = null;
                    }
                }

                if ( cbAddActivity.Checked )
                {
                    var connectors = GetConnectors( connectionOpportunity, rockContext, cblBulkUpdateCampuses.SelectedValues );

                    var connectionRequestActivity = new ConnectionRequestActivity();
                    var connector = connectors.ContainsKey( ddlActivityConnector.SelectedValue.AsInteger() ) ? connectors[ddlActivityConnector.SelectedValue.AsInteger()] : null;
                    connectionRequestActivity.ConnectionRequestId = connectionRequest.Id;
                    connectionRequestActivity.ConnectionActivityTypeId = ddlActivityType.SelectedValue.AsInteger();
                    connectionRequestActivity.ConnectorPersonAliasId = connector?.PrimaryAliasId;
                    connectionRequestActivity.ConnectionOpportunityId = connectionOpportunity.Id;
                    connectionRequestActivity.Note = tbActivityNote.Text;

                    connectionRequestActivityService.Add( connectionRequestActivity );
                }

                var bulkUpdateActivity = new ConnectionRequestActivity();

                if (bulkUpdateActivityId != 0)
                {
                    bulkUpdateActivity.ConnectionRequestId = connectionRequest.Id;
                    bulkUpdateActivity.ConnectionActivityTypeId = bulkUpdateActivityId;
                    bulkUpdateActivity.ConnectionOpportunityId = connectionOpportunity.Id;

                    connectionRequestActivityService.Add(bulkUpdateActivity);
                }
            }

            rockContext.SaveChanges();

            int intValue = wtpLaunchWorkflow.SelectedValue.AsInteger();
            if ( intValue != 0 )
            {
                // Queue a transaction to launch workflow
                var workflowDetails = connectionRequests.ConvertAll( p => new LaunchWorkflowDetails( p ) );
                var launchWorkflowsTxn = new LaunchWorkflowsTransaction( intValue, workflowDetails );
                launchWorkflowsTxn.InitiatorPersonAliasId = CurrentPersonAliasId;
                launchWorkflowsTxn.Enqueue();
            }

            ddlState.ClearSelection();
            ddlStatus.ClearSelection();
            wtpLaunchWorkflow.SetValue( 0 );
            ddlActivityType.ClearSelection();
            ddlActivityConnector.ClearSelection();
            tbActivityNote.Text = string.Empty;

            SelectedFields = new List<string>();
            SetControlSelection();

            nbStatusUpdate.Visible = true;
            nbStatusUpdate.Text = string.Format( "{0} {1} successfully updated.", connectionRequests.Count.ToString( "N0" ), connectionRequests.Count > 1 ? "people were" : "person was" );
            mdConfirmUpdateRequests.SaveButtonText = "Ok";
            mdConfirmUpdateRequests.CancelLinkVisible = false;
            btnBulkRequestUpdateCancel.Text = "Back";
        }

        /// <summary>
        /// Handles the ServerValidate event of the cvSelection control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="args">The <see cref="ServerValidateEventArgs"/> instance containing the event data.</param>
        protected void cvSelection_ServerValidate( object source, ServerValidateEventArgs args )
        {
            nbBulkUpdateNotification.Visible = false;
            args.IsValid = cbAddActivity.Checked && !string.IsNullOrWhiteSpace( ddlActivityType.SelectedValue );
        }

        #endregion Events

        #region Methods

        /// <summary>
        /// Gets the details.
        /// </summary>
        private void GetDetails()
        {
            var connectionTypeId = PageParameter( PageParameterKey.ConnectionTypeId ).AsInteger();
            var entitySetId = PageParameter( PageParameterKey.EntitySetId ).AsInteger();

            var rockContext = new RockContext();

            var connectionType = new ConnectionTypeService( rockContext ).Queryable().AsNoTracking()
                    .Include( ct => ct.ConnectionOpportunities )
                    .FirstOrDefault( ct => ct.Id == connectionTypeId );

            if (connectionType is null)
            {
                pnlEntry.Visible = false;
                ShowNotification( NotificationBoxType.Warning,  "There is no reference to a valid Connection.  Please set this page up under the 'Bulk Update Requests' block settings of the Connection Request Board block, and select the connection requests you would like to edit." );
            }
            else
            {
                pnlEntry.Visible = true;
                BindDropdownLists( connectionType );
                BindCampusSelector( entitySetId, rockContext );
            }
        }

        /// <summary>
        /// Binds the campus selector.
        /// </summary>
        /// <param name="entitySetId">The entity set identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        private void BindCampusSelector( int entitySetId, RockContext rockContext )
        {
            RequestIdsState = GetConnectionRequestIds( entitySetId, rockContext );

            ConnectionCampusCountViewModelsState = new ConnectionRequestService( rockContext ).Queryable().AsNoTracking()
                .Include( cr => cr.Campus )
                .Where( cr => RequestIdsState.Contains( cr.Id ) )
                .GroupBy( cr => cr.CampusId )
                .Select( cr => new ConnectionCampusCountViewModel
                {
                    CampusId = cr.Key,
                    Campus = cr.FirstOrDefault().CampusId == null ? "No Campus" : cr.FirstOrDefault().Campus.Name,
                    Count = cr.Count(),
                    OpportunityId = cr.FirstOrDefault().ConnectionOpportunityId,
                } ).ToList();

            AddCampusSelectorControls( ConnectionCampusCountViewModelsState );

            ddlOpportunity.SetValue( ConnectionCampusCountViewModelsState.FirstOrDefault().OpportunityId );
            ddlOpportunity_SelectedIndexChanged( null, null );
        }

        private List<int> GetConnectionRequestIds( int entitySetId, RockContext rockContext )
        {
            var entitySet = new EntitySetService( rockContext ).Queryable().AsNoTracking()
                .Include( m => m.Items )
                .FirstOrDefault( e => e.Id == entitySetId );

            return RequestIdsState = entitySet.Items.Select( m => m.EntityId ).ToList();
        }

        /// <summary>
        /// Adds the campus selector controls.
        /// </summary>
        /// <param name="connectionCampusCountViewModels">The connection campus count view models.</param>
        private void AddCampusSelectorControls( List<ConnectionCampusCountViewModel> connectionCampusCountViewModels )
        {
            if ( connectionCampusCountViewModels.Count > 0 )
            {
                cblBulkUpdateCampuses.Items.Clear();

                for ( int i = 0; i < connectionCampusCountViewModels.Count; i++ )
                {
                    var campusCountItem = connectionCampusCountViewModels[i];
                    var listItem = new ListItem( $"{campusCountItem.Campus} ({campusCountItem.Count})", campusCountItem.CampusId.ToString() ) { Selected = true };
                    cblBulkUpdateCampuses.Items.Add( listItem );
                }
            }

            if ( connectionCampusCountViewModels.Count > 1 )
            {
                rcwBulkUpdateCampuses.Visible = true;
            }
        }

        /// <summary>
        /// Binds the dropdown lists.
        /// </summary>
        /// <param name="connectionType">Type of the connection.</param>
        private void BindDropdownLists( ConnectionType connectionType )
        {
            // Add Opportunites to dropdown list
            var activeOpportunities = connectionType.ConnectionOpportunities
                .Where( co => co.IsActive == true )
                .OrderBy( co => co.Order )
                .ThenBy( co => co.Name )
                .ToList();

            foreach ( var opportunity in activeOpportunities )
            {
                ddlOpportunity.Items.Add( new ListItem( opportunity.Name, opportunity.Id.ToString().ToUpper() ) );
            }

            // Add Statuses to dropdown list
            ddlStatus.Items.Add( new ListItem( string.Empty, string.Empty ) );
            foreach ( var status in connectionType.ConnectionStatuses )
            {
                ddlStatus.Items.Add( new ListItem( status.Name, status.Id.ToString() ) );
            }

            // Add States to dropdown list
            ddlState.Items.Add( new ListItem( string.Empty, string.Empty ) );
            foreach ( var state in Enum.GetValues( typeof( ConnectionState ) ).Cast<ConnectionState>().ToList() )
            {
                ddlState.Items.Add( new ListItem( state.Humanize(), state.ToString() ) );
            }
        }

        /// <summary>
        /// Rebinds the opportunity connector.
        /// </summary>
        private void RebindOpportunityConnector()
        {
            var rockContext = new RockContext();
            var connectionOpportunity = GetConnectionOpportunity( rockContext );

            if ( connectionOpportunity != null )
            {
                rbBulkUpdateDefaultConnector.Text = $"Keep Default Connector for {connectionOpportunity.Name}";
            }

            RebindOpportunityConnector( connectionOpportunity, rockContext );
        }

        /// <summary>
        /// Rebinds the opportunity connector.
        /// </summary>
        /// <param name="connectionOpportunity">The connection opportunity.</param>
        /// <param name="rockContext">The rock context.</param>
        private void RebindOpportunityConnector( ConnectionOpportunity connectionOpportunity, RockContext rockContext )
        {
            var selectedCampusIds = cblBulkUpdateCampuses.SelectedValuesAsInt;

            Dictionary<int, Person> connectors = GetConnectors( connectionOpportunity, rockContext, cblBulkUpdateCampuses.SelectedValues );

            ddlBulkUpdateOpportunityConnector.Items.Clear();
            ddlBulkUpdateOpportunityConnector.Items.Add( new ListItem( string.Empty, string.Empty ) );

            // Add connectors to dropdown list
            connectors.ToList()
                .ForEach( c => ddlBulkUpdateOpportunityConnector.Items.Add( new ListItem( c.Value.FullName, c.Key.ToString() ) ) );

            // If default connector is checked and a single campus is selected set default connector person as selected value in Connector dropdown
            if ( rbBulkUpdateDefaultConnector.Checked && connectionOpportunity != null && selectedCampusIds.Count == 1 )
            {
                var selectedCampusId = selectedCampusIds[0];
                var defaultConnectorPersonId = connectionOpportunity.GetDefaultConnectorPersonId( selectedCampusId );
                if ( defaultConnectorPersonId.HasValue )
                {
                    ddlBulkUpdateOpportunityConnector.SetValue( defaultConnectorPersonId.Value );
                }
            }
        }

        /// <summary>
        /// Gets the connectors.
        /// </summary>
        /// <param name="connectionOpportunity">The connection opportunity.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="selectedCampuses">The selected campus identifier.</param>
        /// <returns></returns>
        private Dictionary<int, Person> GetConnectors( ConnectionOpportunity connectionOpportunity, RockContext rockContext, List<string> selectedCampuses )
        {
            var selectedCampusIds = selectedCampuses.AsIntegerList();

            var connectors = new Dictionary<int, Person>();
            var connectionOpportunityConnectorPersonList = new ConnectionOpportunityConnectorGroupService( rockContext ).Queryable()
                        .Where( a => a.ConnectionOpportunityId == connectionOpportunity.Id
                            && ( !a.CampusId.HasValue || selectedCampusIds.Contains( a.CampusId.Value ) ) )
                        .SelectMany( a => a.ConnectorGroup.Members )
                        .Where( a => a.GroupMemberStatus == GroupMemberStatus.Active )
                        .Select( a => a.Person )
                        .OrderBy( p => p.LastName )
                        .ThenBy( p => p.NickName )
                        .AsNoTracking()
                        .ToList();

            connectionOpportunityConnectorPersonList.ForEach( p => connectors.TryAdd( p.Id, p ) );

            // Add the current person as possible connector
            if ( CurrentPerson != null )
            {
                connectors.TryAdd( CurrentPerson.Id, CurrentPerson );
            }

            return connectors;
        }

        /// <summary>
        /// Gets the activity details.
        /// </summary>
        private void GetActivityDetails()
        {
            var rockContext = new RockContext();
            var connectionOpportunity = GetConnectionOpportunity( rockContext );

            // Get connectors and add to dropdown list
            var connectors = GetConnectors( connectionOpportunity, rockContext, cblBulkUpdateCampuses.SelectedValues );
            ddlActivityConnector.Items.Clear();
            ddlActivityConnector.Items.Add( new ListItem( string.Empty, string.Empty ) );
            connectors.ToList()
                .ForEach( c => ddlActivityConnector.Items.Add( new ListItem( c.Value.FullName, c.Key.ToString() ) ) );

            // Get ActivityTypes and add to dropdown
            if ( ddlActivityType.Items.Count == 0 )
            {
                ddlActivityType.Items.Add( new ListItem( string.Empty, string.Empty ) );
                GetActivityTypes( connectionOpportunity, rockContext )
                    .ForEach( at => ddlActivityType.Items.Add( new ListItem( at.Name, at.Id.ToString() ) ) );
            }
        }

        /// <summary>
        /// Gets the activity types.
        /// </summary>
        /// <param name="connectionOpportunity">The connection opportunity.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private List<ConnectionActivityType> GetActivityTypes( ConnectionOpportunity connectionOpportunity, RockContext rockContext )
        {
            if ( ConnectionActivityTypes.Count > 0 && ConnectionActivityTypes.FirstOrDefault().ConnectionTypeId == connectionOpportunity.ConnectionTypeId )
            {
                return ConnectionActivityTypes;
            }

            var connectionActivityTypeService = new ConnectionActivityTypeService( rockContext );
            ConnectionActivityTypes = connectionActivityTypeService.Queryable()
                .AsNoTracking()
                .Where( cat =>
                    cat.ConnectionTypeId == connectionOpportunity.ConnectionTypeId &&
                    cat.IsActive )
                .OrderBy( cat => cat.Name )
                .ThenBy( cat => cat.Id )
                .ToList();

            return ConnectionActivityTypes;
        }

        /// <summary>
        /// Sets the control selection.
        /// </summary>
        private void SetControlSelection()
        {
            SetControlSelection( ddlStatus, "Status" );
            SetControlSelection( ddlState, "State" );

            SetControlSelection( ddlActivityType, "Activity Type" );
            SetControlSelection( ddlActivityConnector, "Connector" );
            SetControlSelection( tbActivityNote, "Note" );

            rbBulkUpdateSelectConnector.Visible = cblBulkUpdateCampuses.SelectedValues.Count == 1;
        }

        /// <summary>
        /// Sets the control selection for a control
        /// </summary>
        /// <param name="control"></param>
        /// <param name="label"></param>
        private void SetControlSelection( IRockControl control, string label )
        {
            bool controlEnabled = SelectedFields.Contains( control.ClientID, StringComparer.OrdinalIgnoreCase );
            string iconCss = controlEnabled ? "fa-check-circle-o" : "fa-circle-o";
            control.Label = $"<span class='js-select-item'><i class='fa {iconCss}'></i></span> {label}";
            var webControl = control as WebControl;
            if ( webControl != null )
            {
                webControl.Enabled = controlEnabled;
            }
        }

        /// <summary>
        /// Gets the changes summary.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private List<string> GetChangesSummary( RockContext rockContext )
        {
            var changes = new List<string>();

            if ( SelectedFields.Any( m => m.Contains( ddlStatus.ID ) ) )
            {
                EvaluateChange( changes, "Status", ddlStatus.SelectedItem.Text );
            }

            if ( SelectedFields.Any( m => m.Contains( ddlState.ID ) ) )
            {
                EvaluateChange( changes, "State", ddlState.SelectedItem.Text );
            }

            if ( cbAddActivity.Checked && !string.IsNullOrWhiteSpace( ddlActivityType.SelectedValue ) )
            {
                changes.Add( $"Add new Connection Activity of type <span class='field-name'>{ddlActivityType.SelectedItem.Text}</span>" );
            }

            if ( rbBulkUpdateNoConnector.Checked )
            {
                EvaluateChange( changes, "Connector", string.Empty );
            }

            if ( rbBulkUpdateSelectConnector.Checked )
            {
                EvaluateChange( changes, "Connector", ddlBulkUpdateOpportunityConnector.SelectedItem.Text );
            }

            if ( rbBulkUpdateDefaultConnector.Checked )
            {
                var conectionOpportunityName = ConnectionOpportunity != null ? ConnectionOpportunity.Name : GetConnectionOpportunity( rockContext ).Name;
                EvaluateChange( changes, "Connector", $"Default Connector for {conectionOpportunityName}" );
            }

            if ( wtpLaunchWorkflow.SelectedValue != "0" )
            {
                changes.Add( $"Launch Workflow <span class='field-name'>{wtpLaunchWorkflow.ItemName }</span>" );
            }

            var currentOpportunityId = ConnectionCampusCountViewModelsState.FirstOrDefault().OpportunityId;
            var selectedOpportunity = ddlOpportunity.SelectedValue.AsInteger();

            if ( currentOpportunityId != selectedOpportunity )
            {
                EvaluateChange( changes, "Opportunity", ddlOpportunity.SelectedItem.Text );
            }

            if ( dpFollowUpDate.Visible )
            {
                changes.Add( $"Set Follow-up Date to <span class='field-name'>{dpFollowUpDate.SelectedDate}</span>" );
            }

            return changes;
        }

        /// <summary>
        /// Evaluates the changes about to be persisted
        /// </summary>
        /// <param name="historyMessages">The history messages.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="newValue">The new value.</param>
        private void EvaluateChange( List<string> historyMessages, string propertyName, string newValue )
        {
            if ( !string.IsNullOrWhiteSpace( newValue ) )
            {
                historyMessages.Add( $"Update <span class='field-name'>{propertyName}</span> to value of <span class='field-value'>{newValue}</span>." );
            }
            else
            {
                historyMessages.Add( $"Clear <span class='field-name'>{propertyName}</span> value." );
            }
        }

        /// <summary>
        /// Gets the connection opportunity.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public ConnectionOpportunity GetConnectionOpportunity( RockContext rockContext )
        {
            var connectionOpportunityId = ddlOpportunity.SelectedValue.AsIntegerOrNull();
            return ConnectionOpportunity ?? new ConnectionOpportunityService( rockContext )
                .Queryable()
                .AsNoTracking()
                .FirstOrDefault(c => c.Id == connectionOpportunityId.Value );
        }

        /// <summary>
        /// Shows the notification.
        /// </summary>
        /// <param name="notificationType">Type of the notification.</param>
        /// <param name="message">The message.</param>
        private void ShowNotification( NotificationBoxType notificationType, string message )
        {
            nbBulkUpdateNotification.Visible = true;
            nbBulkUpdateNotification.NotificationBoxType = notificationType;
            nbBulkUpdateNotification.Text = message;
        }

        #endregion Methods

        #region Helper Class

        /// <summary>
        /// View Model for the campus selectot filter
        /// </summary>
        private sealed class ConnectionCampusCountViewModel
        {
            public int? CampusId { get; set; }
            public string Campus { get; set; }
            public int Count { get; set; }
            public int OpportunityId { get; set; }
        }

        #endregion Helper Class
    }
}