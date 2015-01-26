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
using System.ComponentModel;
using System.Collections.Generic;
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

namespace RockWeb.Blocks.WorkFlow
{
    [DisplayName( "Workflow List" )]
    [Category( "WorkFlow" )]
    [Description( "Lists all the workflows." )]

    [LinkedPage( "Entry Page", "Page used to launch a new workflow of the selected type." )]
    [LinkedPage( "Detail Page", "Page used to display details about a workflow." )]
    public partial class WorkflowList : RockBlock
    {
        #region Fields

        private WorkflowType _workflowType = null;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            if ( _workflowType != null && _workflowType.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
            {
                gfWorkflows.ApplyFilterClick += gfWorkflows_ApplyFilterClick;
                gfWorkflows.DisplayFilterValue += gfWorkflows_DisplayFilterValue;

                // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
                this.BlockUpdated += Block_BlockUpdated;
                this.AddConfigurationUpdateTrigger( upnlSettings );

                gWorkflows.DataKeyNames = new string[] { "Id" };
                gWorkflows.Actions.ShowAdd = true;
                gWorkflows.Actions.AddClick += gWorkflows_Add;
                gWorkflows.GridRebind += gWorkflows_GridRebind;
                gWorkflows.Actions.ShowAdd = true;
                gWorkflows.IsDeleteEnabled = true;

                AddAttributeColumns();

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
                statusField.HtmlEncode = false;

                var stateField = new BoundField();
                gWorkflows.Columns.Add( stateField );
                stateField.DataField = "State";
                stateField.SortExpression = "CompletedDateTime";
                stateField.HeaderText = "State";
                stateField.HtmlEncode = false;

                var manageField = new EditField();
                gWorkflows.Columns.Add( manageField );
                manageField.IconCssClass = "fa fa-edit";
                manageField.Click += gWorkflows_Manage;

                var deleteField = new DeleteField();
                gWorkflows.Columns.Add( deleteField );
                deleteField.Click += gWorkflows_Delete;

                if ( !string.IsNullOrWhiteSpace( _workflowType.WorkTerm ) )
                {
                    gWorkflows.RowItemText = _workflowType.WorkTerm;
                    lGridTitle.Text = _workflowType.WorkTerm.Pluralize();
                }

                RockPage.PageTitle = _workflowType.Name;

                if ( !string.IsNullOrWhiteSpace( _workflowType.IconCssClass ) )
                {
                    lHeadingIcon.Text = string.Format("<i class='{0}'></i>", _workflowType.IconCssClass);
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
            if ( !Page.IsPostBack )
            {
                BindFilter();
                BindGrid();
            }
            base.OnLoad( e );
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

            var workflowTypeId = PageParameter( "WorkflowTypeId" ).AsInteger();
            _workflowType = new WorkflowTypeService( new RockContext() ).Get( workflowTypeId );
            if ( _workflowType != null )
            {
                breadCrumbs.Add( new BreadCrumb( _workflowType.Name, pageReference ) );
            }

            return breadCrumbs;
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
            switch ( e.Key )
            {
                case "Activated":
                    {
                        e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
                        break;
                    }
                case "Completed":
                    {
                        e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
                        break;
                    }
                case "Initiator":
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
                        break;
                    }
                case "Name":
                case "Status":
                case "State":
                    {
                        break;
                    }
                default:
                    {
                        e.Value = string.Empty;
                        break;
                    }
            }
        }

        protected void gfWorkflows_ApplyFilterClick( object sender, EventArgs e )
        {
            gfWorkflows.SaveUserPreference( "Activated", drpActivated.DelimitedValues );
            gfWorkflows.SaveUserPreference( "Completed", drpCompleted.DelimitedValues );
            gfWorkflows.SaveUserPreference( "Name", tbName.Text );
            gfWorkflows.SaveUserPreference( "Status", tbStatus.Text );
            gfWorkflows.SaveUserPreference( "State", GetState() );

            int? personId = ppInitiator.SelectedValue;
            gfWorkflows.SaveUserPreference( "Initiator", personId.HasValue ? personId.Value.ToString() : "" );

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
            NavigateToLinkedPage( "DetailPage", "workflowId", e.RowKeyId );
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
                var qryParam = new Dictionary<string, string>();
                qryParam.Add( "WorkflowTypeId", workflow.WorkflowTypeId.ToString() );
                qryParam.Add( "WorkflowId", workflow.Id.ToString() );
                NavigateToLinkedPage( "EntryPage", qryParam );
            }
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
            if (!selectedItems.Any())
            {
                return "None";
            }

            // Only one item was selected, return it's value
            if (selectedItems.Count() == 1)
            {
                return selectedItems[0];
            }

            // All items were selected, which is not technically a 'filter'
            return string.Empty;
        }


        /// <summary>
        /// Binds the defined values grid.
        /// </summary>
        protected void AddAttributeColumns()
        {
            // Remove attribute columns
            foreach ( var column in gWorkflows.Columns.OfType<AttributeField>().ToList() )
            {
                gWorkflows.Columns.Remove( column );
            }

            if ( _workflowType != null )
            {
                // Add attribute columns
                int entityTypeId = new Workflow().TypeId;
                string qualifier = _workflowType.Id.ToString();
                foreach ( var attribute in new AttributeService( new RockContext() ).Queryable()
                    .Where( a =>
                        a.EntityTypeId == entityTypeId &&
                        a.IsGridColumn &&
                        a.EntityTypeQualifierColumn.Equals( "WorkflowTypeId", StringComparison.OrdinalIgnoreCase ) &&
                        a.EntityTypeQualifierValue.Equals( qualifier ) )
                    .OrderBy( a => a.Order )
                    .ThenBy( a => a.Name ) )
                {
                    string dataFieldExpression = attribute.Key;
                    bool columnExists = gWorkflows.Columns.OfType<AttributeField>().FirstOrDefault( a => a.DataField.Equals( dataFieldExpression ) ) != null;
                    if ( !columnExists )
                    {
                        AttributeField boundField = new AttributeField();
                        boundField.DataField = dataFieldExpression;
                        boundField.HeaderText = attribute.Name;
                        boundField.SortExpression = string.Empty;

                        var attributeCache = Rock.Web.Cache.AttributeCache.Read( attribute.Id );
                        if ( attributeCache != null )
                        {
                            boundField.ItemStyle.HorizontalAlign = attributeCache.FieldType.Field.AlignValue;
                        }

                        gWorkflows.Columns.Add( boundField );
                    }
                }
            }
        }

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            drpActivated.DelimitedValues = gfWorkflows.GetUserPreference( "Activated" );
            drpCompleted.DelimitedValues = gfWorkflows.GetUserPreference( "Completed" );
            tbName.Text = gfWorkflows.GetUserPreference( "Name" );
            tbStatus.Text = gfWorkflows.GetUserPreference( "Status" );

            int? personId = gfWorkflows.GetUserPreference( "Initiator" ).AsIntegerOrNull();
            if ( personId.HasValue )
            {
                ppInitiator.SetValue( new PersonService( new RockContext() ).Get( personId.Value ) );
            }
            else
            {
                ppInitiator.SetValue( null );
            }

            string state = gfWorkflows.GetUserPreference( "State" );
            foreach( ListItem li in cblState.Items )
            {
                li.Selected = string.IsNullOrWhiteSpace( state ) || state.Contains( li.Value );
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            if ( _workflowType != null )
            {
                pnlWorkflowList.Visible = true;

                var rockContext = new RockContext();
                var workflowService = new WorkflowService( rockContext );

                var qry = workflowService.Queryable( "Activities.ActivityType,InitiatorPersonAlias.Person" )
                    .Where( w => w.WorkflowTypeId.Equals( _workflowType.Id ) );

                // Activated Date Range Filter
                var drp = new DateRangePicker();
                drp.DelimitedValues = gfWorkflows.GetUserPreference( "Activated" );
                if ( drp.LowerValue.HasValue )
                {
                    qry = qry.Where( w => w.ActivatedDateTime >= drp.LowerValue.Value );
                }
                if ( drp.UpperValue.HasValue )
                {
                    DateTime upperDate = drp.UpperValue.Value.Date.AddDays( 1 );
                    qry = qry.Where( w => w.ActivatedDateTime.Value < upperDate );
                }

                // State Filter
                string state = gfWorkflows.GetUserPreference( "State" );
                if (!string.IsNullOrWhiteSpace(state))
                {
                    // somewhat of a backwards comparison to account for value of "None"
                    if ( !state.Equals( "Active" ) )
                    {
                        qry = qry.Where( w => w.CompletedDateTime.HasValue );
                    }
                    if ( !state.Equals( "Completed" ) )
                    {
                        qry = qry.Where( w => !w.CompletedDateTime.HasValue );
                    }
                }

                // Completed Date Range Filter
                var drp2 = new DateRangePicker();
                drp2.DelimitedValues = gfWorkflows.GetUserPreference( "Completed" );
                if ( drp2.LowerValue.HasValue )
                {
                    qry = qry.Where( w => w.CompletedDateTime.HasValue && w.CompletedDateTime.Value >= drp2.LowerValue.Value );
                }
                if ( drp2.UpperValue.HasValue )
                {
                    DateTime upperDate = drp2.UpperValue.Value.Date.AddDays( 1 );
                    qry = qry.Where( w => w.CompletedDateTime.HasValue && w.CompletedDateTime.Value < upperDate );
                }

                string name = gfWorkflows.GetUserPreference( "Name" );
                if ( !string.IsNullOrWhiteSpace( name ) )
                {
                    qry = qry.Where( w => w.Name.StartsWith( name ) );
                }

                int? personId = gfWorkflows.GetUserPreference( "Initiator" ).AsIntegerOrNull();
                if ( personId.HasValue )
                {
                    qry = qry.Where( w => w.InitiatorPersonAlias.PersonId == personId.Value );
                }

                string status = gfWorkflows.GetUserPreference( "Status" );
                if ( !string.IsNullOrWhiteSpace( status ) )
                {
                    qry = qry.Where( w => w.Status.StartsWith( status ) );
                }

                List<Workflow> workflows = null;

                var sortProperty = gWorkflows.SortProperty;
                if ( sortProperty != null )
                {
                    if ( sortProperty.Property == "Initiator" )
                    {
                        if ( sortProperty.Direction == SortDirection.Ascending )
                        {
                            workflows = qry
                                .OrderBy( w => w.InitiatorPersonAlias.Person.LastName )
                                .ThenBy( w => w.InitiatorPersonAlias.Person.NickName )
                                .ToList();
                        }
                        else
                        {
                            workflows = qry
                                .OrderByDescending( w => w.InitiatorPersonAlias.Person.LastName )
                                .ThenByDescending( w => w.InitiatorPersonAlias.Person.NickName )
                                .ToList();
                        }
                    }
                    else
                    {
                        workflows = qry.Sort( sortProperty ).ToList();
                    }
                }
                else
                {
                    workflows = qry.OrderByDescending( s => s.CreatedDateTime ).ToList();
                }

                // Since we're not binding to actual workflow list, but are using AttributeField columns,
                // we need to save the workflows into the grid's object list
                gWorkflows.ObjectList = new Dictionary<string, object>();
                workflows.ForEach( w => gWorkflows.ObjectList.Add( w.Id.ToString(), w ) );

                gWorkflows.DataSource = workflows.Select( w => new
                {
                    w.Id,
                    w.Name,
                    Initiator = ( w.InitiatorPersonAlias != null ? w.InitiatorPersonAlias.Person.FullName : "" ),
                    Activities = w.ActiveActivities.Select( a => a.ActivityType.Name ).ToList().AsDelimited( "<br/>" ),
                    w.CreatedDateTime,
                    Status = string.Format( "<span class='label label-info'>{0}</span>", w.Status ),
                    State = ( w.CompletedDateTime.HasValue ? "<span class='label label-default'>Completed</span>" : "<span class='label label-success'>Active</span>" )
                } ).ToList();
                gWorkflows.DataBind();
            }
            else
            {
                pnlWorkflowList.Visible = false;
            }
        }

        #endregion
    }
}