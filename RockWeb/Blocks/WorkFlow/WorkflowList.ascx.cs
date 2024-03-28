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
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock;
using Attribute = Rock.Model.Attribute;
using Rock.Security;
using Rock.Web.Cache;
using Newtonsoft.Json;
using Rock.Tasks;
using Rock.Constants;

namespace RockWeb.Blocks.WorkFlow
{
    [DisplayName( "Workflow List" )]
    [Category( "WorkFlow" )]
    [Description( "Lists all the workflows." )]

    [LinkedPage( "Entry Page", "Page used to launch a new workflow of the selected type." )]
    [LinkedPage( "Detail Page", "Page used to display details about a workflow." )]
    [WorkflowTypeField( "Default WorkflowType", "The default workflow type to use. If provided the query string will be ignored." )]
    [Rock.SystemGuid.BlockTypeGuid( "C86C80DF-F2FD-47F8-81CF-7C5EA4100C3B" )]
    public partial class WorkflowList : RockBlock, ICustomGridColumns
    {
        #region Fields

        private bool _canView = false;
        private bool _canEdit = false;
        private WorkflowType _workflowType = null;
        BootstrapButton _bbtnDelete = new BootstrapButton();

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the available attributes.
        /// </summary>
        /// <value>
        /// The available attributes.
        /// </value>
        public List<AttributeCache> AvailableAttributes { get; set; }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            AvailableAttributes = ViewState["AvailableAttributes"] as List<AttributeCache>;

            AddDynamicControls();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "DefaultWorkflowType" ) ) )
            {
                Guid workflowTypeGuid = Guid.Empty;
                Guid.TryParse( GetAttributeValue( "DefaultWorkflowType" ), out workflowTypeGuid );
                _workflowType = new WorkflowTypeService( new RockContext() ).Get( workflowTypeGuid );
            }
            else
            {
                int workflowTypeId = 0;
                workflowTypeId = PageParameter( "WorkflowTypeId" ).AsInteger();
                _workflowType = new WorkflowTypeService( new RockContext() ).Get( workflowTypeId );
            }

            if ( _workflowType != null )
            {
                _canEdit = UserCanEdit || _workflowType.IsAuthorized( Authorization.EDIT, CurrentPerson );
                _canView = _canEdit || ( _workflowType.IsAuthorized( Authorization.VIEW, CurrentPerson ) && _workflowType.IsAuthorized( "ViewList", CurrentPerson ) );

                gfWorkflows.ApplyFilterClick += gfWorkflows_ApplyFilterClick;
                gfWorkflows.DisplayFilterValue += gfWorkflows_DisplayFilterValue;

                // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
                this.BlockUpdated += Block_BlockUpdated;
                this.AddConfigurationUpdateTrigger( upnlSettings );

                _bbtnDelete.Text = "Delete";
                _bbtnDelete.Click += new EventHandler( bbtnDelete_Click );
                _bbtnDelete.CssClass = "btn btn-xs btn-default btn-grid-custom-action pull-left";
                gWorkflows.Actions.AddCustomActionControl( _bbtnDelete );
                gWorkflows.DataKeyNames = new string[] { "Id" };
                gWorkflows.Actions.ShowAdd = _canEdit;
                gWorkflows.Actions.AddClick += gWorkflows_Add;
                gWorkflows.GridRebind += gWorkflows_GridRebind;
                gWorkflows.Actions.ShowAdd = true;
                gWorkflows.IsDeleteEnabled = _canEdit;

                if ( !string.IsNullOrWhiteSpace( _workflowType.WorkTerm ) )
                {
                    gWorkflows.RowItemText = _workflowType.WorkTerm;
                    lGridTitle.Text = _workflowType.WorkTerm.Pluralize();
                }

                RockPage.PageTitle = _workflowType.Name;

                if ( !string.IsNullOrWhiteSpace( _workflowType.IconCssClass ) )
                {
                    lHeadingIcon.Text = string.Format( "<i class='{0}'></i>", _workflowType.IconCssClass );
                }
            }
            else
            {
                pnlWorkflowList.Visible = false;
            }
        }


        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbResult.Visible = false;
            if ( !Page.IsPostBack )
            {
                if ( _canView )
                {
                    SetFilter();
                    BindGrid();
                }
                else
                {
                    pnlWorkflowList.Visible = false;
                    nbMessage.Visible = true;
                    nbMessage.Text = EditModeMessage.NotAuthorizedToView( WorkflowType.FriendlyTypeName );
                }
            }
        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs.
        /// </summary>
        /// <param name="pageReference">The <see cref="Rock.Web.PageReference" />.</param>
        /// <returns>
        /// A <see cref="System.Collections.Generic.List{BreadCrumb}" /> of block related <see cref="Rock.Web.UI.BreadCrumb">BreadCrumbs</see>.
        /// </returns>
        public override List<BreadCrumb> GetBreadCrumbs( Rock.Web.PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();
            WorkflowType workflowType;

            if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "DefaultWorkflowType" ) ) )
            {
                Guid workflowTypeGuid = Guid.Empty;
                Guid.TryParse( GetAttributeValue( "DefaultWorkflowType" ), out workflowTypeGuid );
                workflowType = new WorkflowTypeService( new RockContext() ).Get( workflowTypeGuid );
            }
            else
            {
                int workflowTypeId = 0;
                workflowTypeId = PageParameter( "WorkflowTypeId" ).AsInteger();
                workflowType = new WorkflowTypeService( new RockContext() ).Get( workflowTypeId );
            }

            if ( workflowType != null )
            {
                breadCrumbs.Add( new BreadCrumb( workflowType.Name, pageReference ) );
            }

            return breadCrumbs;
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["AvailableAttributes"] = AvailableAttributes;

            return base.SaveViewState();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
        }

        protected void gfWorkflows_DisplayFilterValue( object sender, Rock.Web.UI.Controls.GridFilter.DisplayFilterValueArgs e )
        {

            if ( AvailableAttributes != null )
            {
                var attribute = AvailableAttributes.FirstOrDefault( a => MakeKeyUniqueToType( a.Key ) == e.Key );
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

            if ( e.Key == MakeKeyUniqueToType( "Activated" ) )
            {
                e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
            }
            else if ( e.Key == MakeKeyUniqueToType( "Completed" ) )
            {
                e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
            }
            else if ( e.Key == MakeKeyUniqueToType( "Initiator" ) )
            {
                int? personId = e.Value.AsIntegerOrNull();
                if ( personId.HasValue )
                {
                    var person = new PersonService( new RockContext() ).Get( personId.Value );
                    if ( person != null )
                    {
                        e.Value = person.FullName;
                    }
                }
            }
            else if ( e.Key == MakeKeyUniqueToType( "Name" ) )
            {
                return;
            }
            else if ( e.Key == MakeKeyUniqueToType( "Status" ) )
            {
                return;
            }
            else if ( e.Key == MakeKeyUniqueToType( "State" ) )
            {
                return;
            }
            else
            {
                e.Value = string.Empty;
            }
        }

        protected void gfWorkflows_ApplyFilterClick( object sender, EventArgs e )
        {
            gfWorkflows.SetFilterPreference( MakeKeyUniqueToType( "Activated" ), "Activated", drpActivated.DelimitedValues );
            gfWorkflows.SetFilterPreference( MakeKeyUniqueToType( "Completed" ), "Completed", drpCompleted.DelimitedValues );
            gfWorkflows.SetFilterPreference( MakeKeyUniqueToType( "Name" ), "Name", tbName.Text );
            gfWorkflows.SetFilterPreference( MakeKeyUniqueToType( "Status" ), "Status", tbStatus.Text );
            gfWorkflows.SetFilterPreference( MakeKeyUniqueToType( "State" ), "State", GetState() );

            int? personId = ppInitiator.SelectedValue;
            gfWorkflows.SetFilterPreference( MakeKeyUniqueToType( "Initiator" ), "Initiator", personId.HasValue ? personId.Value.ToString() : "" );

            if ( AvailableAttributes != null )
            {
                foreach ( var attribute in AvailableAttributes )
                {
                    var filterControl = phAttributeFilters.FindControl( "filter_" + attribute.Id.ToString() );
                    if ( filterControl != null )
                    {
                        try
                        {
                            var values = attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter );
                            gfWorkflows.SetFilterPreference( MakeKeyUniqueToType( attribute.Key ), attribute.Name, attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter ).ToJson() );
                        }
                        catch { }
                    }
                }
            }

                BindGrid();
        }

        /// <summary>
        /// Handles the Add event of the gWorkflows control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gWorkflows_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "EntryPage", "WorkflowTypeId", _workflowType.Id );
        }

        /// <summary>
        /// Handles the Edit event of the gWorkflows control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gWorkflows_Manage( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "WorkflowId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gWorkflows control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gWorkflows_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            WorkflowService workflowService = new WorkflowService( rockContext );
            Workflow workflow = workflowService.Get( e.RowKeyId );
            if ( workflow != null )
            {
                string errorMessage;
                if ( !workflowService.CanDelete( workflow, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                workflowService.Delete( workflow );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gWorkflows control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gWorkflows_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the Edit event of the gWorkflows control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gWorkflows_Edit( object sender, RowEventArgs e )
        {
            var workflow = new WorkflowService( new RockContext() ).Get( e.RowKeyId );
            if ( workflow != null )
            {
                if ( workflow.HasActiveEntryForm( CurrentPerson ) )
                {
                    var qryParam = new Dictionary<string, string>();
                    qryParam.Add( "WorkflowTypeId", workflow.WorkflowTypeId.ToString() );
                    qryParam.Add( "WorkflowGuid", workflow.Guid.ToString() );
                    NavigateToLinkedPage( "EntryPage", qryParam );
                }
                else
                {
                    NavigateToLinkedPage( "DetailPage", "WorkflowId", e.RowKeyId );
                }
            }
        }

        /// <summary>
        /// Marks the selected workflow to delete.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void bbtnDelete_Click( object sender, EventArgs e )
        {
            var workflowsSelected = new List<int>();
            gWorkflows.SelectedKeys.ToList().ForEach( b => workflowsSelected.Add( b.ToString().AsInteger() ) );

            if ( !workflowsSelected.Any() )
            {
                nbResult.Text = string.Format( "At least one workflow must be selected." );
                nbResult.NotificationBoxType = NotificationBoxType.Warning;
                nbResult.Visible = true;
                return;
            }

            var deleteWorkflowsMsg = new DeleteWorkflows.Message
            {
                WorkflowIds = workflowsSelected
            };

            deleteWorkflowsMsg.Send();
            BindGrid();
            mdAlert.Show();
        }

        /// <summary>
        /// Handles the OkClick event of the mdAlert control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdAlert_OkClick( object sender, EventArgs e )
        {
            Response.Redirect( Request.RawUrl );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the state.
        /// </summary>
        /// <returns></returns>
        private string GetState()
        {
            // Get the check box list values by evaluating the posted form values for each input item in the rendered checkbox list.
            // This is required because of a bug in ASP.NET that results in the Selected property for CheckBoxList items to not be
            // set correctly on a postback.
            var selectedItems = new List<string>();
            for ( int i = 0; i < cblState.Items.Count; i++ )
            {
                string value = Request.Form[cblState.UniqueID + "$" + i.ToString()];
                if ( value != null )
                {
                    cblState.Items[i].Selected = true;
                    selectedItems.Add( value );
                }
                else
                {
                    cblState.Items[i].Selected = false;
                }
            }

            // no items were selected (not good)
            if ( !selectedItems.Any() )
            {
                return "None";
            }

            // Only one item was selected, return it's value
            if ( selectedItems.Count() == 1 )
            {
                return selectedItems[0];
            }

            // All items were selected, which is not technically a 'filter'
            return string.Empty;
        }

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void SetFilter()
        {
            BindAttributes();
            AddDynamicControls();

            drpActivated.DelimitedValues = gfWorkflows.GetFilterPreference( MakeKeyUniqueToType( "Activated" ) );
            drpCompleted.DelimitedValues = gfWorkflows.GetFilterPreference( MakeKeyUniqueToType( "Completed" ) );
            tbName.Text = gfWorkflows.GetFilterPreference( MakeKeyUniqueToType( "Name" ) );
            tbStatus.Text = gfWorkflows.GetFilterPreference( MakeKeyUniqueToType( "Status" ) );

            int? personId = gfWorkflows.GetFilterPreference( MakeKeyUniqueToType( "Initiator" ) ).AsIntegerOrNull();
            if ( personId.HasValue )
            {
                ppInitiator.SetValue( new PersonService( new RockContext() ).Get( personId.Value ) );
            }
            else
            {
                ppInitiator.SetValue( null );
            }

            string state = gfWorkflows.GetFilterPreference( MakeKeyUniqueToType( "State" ) );
            foreach ( ListItem li in cblState.Items )
            {
                li.Selected = string.IsNullOrWhiteSpace( state ) || state.Contains( li.Value );
            }
        }

        private void BindAttributes()
        {
            // Parse the attribute filters
            AvailableAttributes = new List<AttributeCache>();
            if ( _workflowType != null )
            {
                int entityTypeId = new Workflow().TypeId;
                string workflowQualifier = _workflowType.Id.ToString();
                foreach ( var attributeModel in new AttributeService( new RockContext() ).Queryable()
                    .Where( a =>
                        a.EntityTypeId == entityTypeId &&
                        a.IsGridColumn &&
                        a.EntityTypeQualifierColumn.Equals( "WorkflowTypeId", StringComparison.OrdinalIgnoreCase ) &&
                        a.EntityTypeQualifierValue.Equals( workflowQualifier ) )
                    .OrderByDescending( a => a.EntityTypeQualifierColumn )
                    .ThenBy( a => a.Order )
                    .ThenBy( a => a.Name ) )
                {
                    AvailableAttributes.Add( AttributeCache.Get( attributeModel ) );
                }
            }
        }

        /// <summary>
        /// Adds the attribute columns.
        /// </summary>
        private void AddDynamicControls()
        {
            // Clear the filter controls
            phAttributeFilters.Controls.Clear();

            // Remove attribute columns
            foreach ( var column in gWorkflows.Columns.OfType<AttributeField>().ToList() )
            {
                gWorkflows.Columns.Remove( column );
            }

            if ( AvailableAttributes != null )
            {
                foreach ( var attribute in AvailableAttributes )
                {
                    var control = attribute.FieldType.Field.FilterControl( attribute.QualifierValues, "filter_" + attribute.Id.ToString(), false, Rock.Reporting.FilterMode.SimpleFilter );
                    if ( control != null )
                    {
                        if ( control is IRockControl )
                        {
                            var rockControl = ( IRockControl ) control;
                            rockControl.Label = attribute.Name;
                            rockControl.Help = attribute.Description;
                            phAttributeFilters.Controls.Add( control );
                        }
                        else
                        {
                            var wrapper = new RockControlWrapper();
                            wrapper.ID = control.ID + "_wrapper";
                            wrapper.Label = attribute.Name;
                            wrapper.Controls.Add( control );
                            phAttributeFilters.Controls.Add( wrapper );
                        }
                    }

                    string savedValue = gfWorkflows.GetFilterPreference( MakeKeyUniqueToType( attribute.Key ) );
                    if ( !string.IsNullOrWhiteSpace( savedValue ) )
                    {
                        try
                        {
                            var values = JsonConvert.DeserializeObject<List<string>>( savedValue );
                            attribute.FieldType.Field.SetFilterValues( control, attribute.QualifierValues, values );
                        }
                        catch { }
                    }

                    string dataFieldExpression = attribute.Key;
                    bool columnExists = gWorkflows.Columns.OfType<AttributeField>().FirstOrDefault( a => a.DataField.Equals( dataFieldExpression ) ) != null;
                    if ( !columnExists )
                    {
                        AttributeField boundField = new AttributeField();
                        boundField.DataField = dataFieldExpression;
                        boundField.AttributeId = attribute.Id;
                        boundField.HeaderText = attribute.Name;
                        boundField.Condensed = false;

                        var attributeCache = Rock.Web.Cache.AttributeCache.Get( attribute.Id );
                        if ( attributeCache != null )
                        {
                            boundField.ItemStyle.HorizontalAlign = attributeCache.FieldType.Field.AlignValue;
                        }

                        gWorkflows.Columns.Add( boundField );
                    }
                }
            }

            var dateField = new DateTimeField();
            gWorkflows.Columns.Add( dateField );
            dateField.DataField = "CreatedDateTime";
            dateField.SortExpression = "CreatedDateTime";
            dateField.HeaderText = "Created";
            dateField.FormatAsElapsedTime = true;

            var statusField = new BoundField();
            gWorkflows.Columns.Add( statusField );
            statusField.DataField = "Status";
            statusField.SortExpression = "Status";
            statusField.HeaderText = "Status";
            statusField.DataFormatString = "<span class='label label-info'>{0}</span>";
            statusField.HtmlEncode = false;

            var stateField = new CallbackField();
            gWorkflows.Columns.Add( stateField );
            stateField.DataField = "IsCompleted";
            stateField.SortExpression = "CompletedDateTime";
            stateField.HeaderText = "State";
            stateField.HtmlEncode = false;
            stateField.OnFormatDataValue += ( sender, e ) =>
            {
                if ( ( bool ) e.DataValue )
                {
                    e.FormattedValue = "<span class='label label-default'>Completed</span>";
                }
                else
                {
                    e.FormattedValue = "<span class='label label-success'>Active</span>";
                }
            };

            if ( _canView )
            {
                var manageField = new LinkButtonField();
                gWorkflows.Columns.Add( manageField );
                manageField.CssClass = "btn btn-default btn-sm";
                manageField.Text = "<i class='fa fa-file-text-o'></i>";
                manageField.Click += gWorkflows_Manage;
            }

            if ( _canEdit )
            {
                var deleteField = new DeleteField();
                gWorkflows.Columns.Add( deleteField );
                deleteField.Click += gWorkflows_Delete;
            }
        }

        /// <summary>
        /// Binds the grid.
        /// 
        /// </summary>
        private void BindGrid()
        {
            if ( !_canView )
            {
                return;
            }

            if ( _workflowType != null )
            {
                pnlWorkflowList.Visible = true;

                var idCol = gWorkflows.ColumnsOfType<BoundField>().Where( c => c.DataField == "WorkflowId" ).FirstOrDefault();
                if ( idCol != null )
                {
                    idCol.Visible = !string.IsNullOrWhiteSpace( _workflowType.WorkflowIdPrefix );
                }

                var rockContext = new RockContext();
                var workflowService = new WorkflowService( rockContext );

                var qry = workflowService
                    .Queryable( "Activities.ActivityType,InitiatorPersonAlias.Person" ).AsNoTracking()
                    .Where( w => w.WorkflowTypeId.Equals( _workflowType.Id ) );

                // Activated Date Range Filter
                if ( drpActivated.LowerValue.HasValue )
                {
                    qry = qry.Where( w => w.ActivatedDateTime >= drpActivated.LowerValue.Value );
                }
                if ( drpActivated.UpperValue.HasValue )
                {
                    DateTime upperDate = drpActivated.UpperValue.Value.Date.AddDays( 1 );
                    qry = qry.Where( w => w.ActivatedDateTime.Value < upperDate );
                }

                // State Filter
                List<string> states = cblState.SelectedValues;
                if ( states.Count == 1 )    // Don't filter if none or both options are selected
                {
                    if ( states[0] == "Active" )
                    {
                        qry = qry.Where( w => !w.CompletedDateTime.HasValue );
                    }
                    else
                    {
                        qry = qry.Where( w => w.CompletedDateTime.HasValue );
                    }
                }

                // Completed Date Range Filter
                if ( drpCompleted.LowerValue.HasValue )
                {
                    qry = qry.Where( w => w.CompletedDateTime.HasValue && w.CompletedDateTime.Value >= drpCompleted.LowerValue.Value );
                }
                if ( drpCompleted.UpperValue.HasValue )
                {
                    DateTime upperDate = drpCompleted.UpperValue.Value.Date.AddDays( 1 );
                    qry = qry.Where( w => w.CompletedDateTime.HasValue && w.CompletedDateTime.Value < upperDate );
                }

                string name = tbName.Text;
                if ( !string.IsNullOrWhiteSpace( name ) )
                {
                    qry = qry.Where( w => w.Name.StartsWith( name ) );
                }

                int? personId = ppInitiator.SelectedValue;
                if ( personId.HasValue )
                {
                    qry = qry.Where( w => w.InitiatorPersonAlias.PersonId == personId.Value );
                }

                string status = tbStatus.Text;
                if ( !string.IsNullOrWhiteSpace( status ) )
                {
                    qry = qry.Where( w => w.Status.StartsWith( status ) );
                }

                // Filter query by any configured attribute filters
                if ( AvailableAttributes != null && AvailableAttributes.Any() )
                {
                    var attributeValueService = new AttributeValueService( rockContext );
                    var parameterExpression = attributeValueService.ParameterExpression;

                    foreach ( var attribute in AvailableAttributes )
                    {
                        var filterControl = phAttributeFilters.FindControl( "filter_" + attribute.Id.ToString() );
                        qry = attribute.FieldType.Field.ApplyAttributeQueryFilter( qry, filterControl, attribute, workflowService, Rock.Reporting.FilterMode.SimpleFilter );
                    }
                }

                IQueryable<Workflow> workflows = null;

                var sortProperty = gWorkflows.SortProperty;
                if ( sortProperty != null )
                {
                    if ( sortProperty.Property == "Initiator" )
                    {
                        if ( sortProperty.Direction == SortDirection.Ascending )
                        {
                            workflows = qry
                                .OrderBy( w => w.InitiatorPersonAlias.Person.LastName )
                                .ThenBy( w => w.InitiatorPersonAlias.Person.NickName );
                        }
                        else
                        {
                            workflows = qry
                                .OrderByDescending( w => w.InitiatorPersonAlias.Person.LastName )
                                .ThenByDescending( w => w.InitiatorPersonAlias.Person.NickName );
                        }
                    }
                    else
                    {
                        workflows = qry.Sort( sortProperty );
                    }
                }
                else
                {
                    workflows = qry.OrderByDescending( s => s.CreatedDateTime );
                }

                // Since we're not binding to actual workflow list, but are using AttributeField columns,
                // we need to save the workflows into the grid's object list
                var workflowObjectQry = workflows;
                if ( gWorkflows.AllowPaging )
                {
                    workflowObjectQry = workflowObjectQry.Skip( gWorkflows.PageIndex * gWorkflows.PageSize ).Take( gWorkflows.PageSize );
                }

                gWorkflows.ObjectList = workflowObjectQry.ToList().ToDictionary( k => k.Id.ToString(), v => v as object );

                gWorkflows.EntityTypeId = EntityTypeCache.Get<Workflow>().Id;
                var qryGrid = workflows.Select( w => new
                {
                    w.Id,
                    w.WorkflowId,
                    w.Name,
                    Initiator = w.InitiatorPersonAlias != null ? w.InitiatorPersonAlias.Person : null,
                    Activities = w.Activities.Where( a => a.ActivatedDateTime.HasValue && !a.CompletedDateTime.HasValue ).OrderBy( a => a.ActivityType.Order ).Select( a => a.ActivityType.Name ),
                    w.CreatedDateTime,
                    Status = w.Status,
                    IsCompleted = w.CompletedDateTime.HasValue
                } );

                gWorkflows.SetLinqDataSource( qryGrid );
                if ( qryGrid.Count() == 0 )
                {
                    _bbtnDelete.Visible = false;
                }
                else
                {
                    _bbtnDelete.Visible = true;
                }

                gWorkflows.DataBind();
            }
            else
            {
                pnlWorkflowList.Visible = false;
            }
        }

        private string MakeKeyUniqueToType( string key )
        {
            if ( _workflowType != null )
            {
                return string.Format( "{0}-{1}", _workflowType.Id, key );
            }
            return key;
        }

        #endregion
    }
}