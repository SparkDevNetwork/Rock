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

            if ( _workflowType != null )
            {
                gfWorkflows.ApplyFilterClick += gfWorkflows_ApplyFilterClick;
                gfWorkflows.DisplayFilterValue += gfWorkflows_DisplayFilterValue;

                // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
                this.BlockUpdated += Block_BlockUpdated;
                this.AddConfigurationUpdateTrigger( upnlSettings );

                gWorkflows.DataKeyNames = new string[] { "id" };
                gWorkflows.Actions.ShowAdd = true;
                gWorkflows.Actions.AddClick += gWorkflows_Add;
                gWorkflows.GridRebind += gWorkflows_GridRebind;
                gWorkflows.RowDataBound += gWorkflows_RowDataBound;

                // Block Security and special attributes (RockPage takes care of View)
                bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
                gWorkflows.Actions.ShowAdd = canAddEditDelete;
                gWorkflows.IsDeleteEnabled = canAddEditDelete;

                AddAttributeColumns();

                var dateField = new DateTimeField();
                gWorkflows.Columns.Add( dateField );
                dateField.DataField = "CreatedDateTime";
                dateField.SortExpression = "CreatedDateTime";
                dateField.HeaderText = "Created";
                dateField.FormatAsElapsedTime = true;

                var formField = new EditField();
                gWorkflows.Columns.Add( formField );
                formField.IconCssClass = "fa fa-edit";
                formField.Click += formField_Click;

                var deleteField = new DeleteField();
                gWorkflows.Columns.Add( deleteField );
                deleteField.Click += gWorkflows_Delete;

                if ( !string.IsNullOrWhiteSpace( _workflowType.WorkTerm ) )
                {
                    gWorkflows.RowItemText = _workflowType.WorkTerm;
                    lGridTitle.Text = _workflowType.WorkTerm.Pluralize();
                }

                RockPage.PageTitle = _workflowType.Name;
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
                case "State":
                    {
                        e.Value =  e.Value == "0" ? "Active" : "Completed";
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
            }
        }

        protected void gfWorkflows_ApplyFilterClick( object sender, EventArgs e )
        {
            gfWorkflows.SaveUserPreference( "Activated", drpActivated.DelimitedValues );
            gfWorkflows.SaveUserPreference( "State", ddlState.SelectedValue );
            gfWorkflows.SaveUserPreference( "Completed", drpCompleted.DelimitedValues );
            gfWorkflows.SaveUserPreference( "Name", tbName.Text );
            gfWorkflows.SaveUserPreference( "Status", tbStatus.Text );

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
        protected void gWorkflows_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "workflowId", (int)e.RowKeyValue );
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
            Workflow workflow = workflowService.Get( (int)e.RowKeyValue );
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
        /// Handles the RowDataBound event of the gWorkflows control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        void gWorkflows_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                Literal lActivities = e.Row.FindControl( "lActivities" ) as Literal;
                if ( lActivities != null )
                {
                    var workflow = e.Row.DataItem as Workflow;
                    if ( workflow != null )
                    {
                        string activities = string.Empty;
                        workflow.ActiveActivities.ToList().ForEach( a => activities += a.ActivityType.Name + "<br/>" );
                        lActivities.Text = activities;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the formField control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        void formField_Click( object sender, RowEventArgs e )
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
            ddlState.SetValue( gfWorkflows.GetUserPreference( "State" ) );
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
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var rockContext = new RockContext();
            var workflowService = new WorkflowService( rockContext );

            var qry = workflowService.Queryable( "Activities" )
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
            int? state = gfWorkflows.GetUserPreference( "State" ).AsIntegerOrNull();
            if ( state.HasValue )
            {
                qry = qry.Where( w => w.CompletedDateTime.HasValue == ( state == 1 ) );
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

            string name = gfWorkflows.GetUserPreference("Name");
            if (!string.IsNullOrWhiteSpace(name))
            {
                qry = qry.Where( w => w.Name.StartsWith(name));
            }

            int? personId = gfWorkflows.GetUserPreference( "Initiator" ).AsIntegerOrNull();
            if (personId.HasValue)
            {
                qry = qry.Where( w => w.InitiatorPersonAlias.PersonId == personId.Value );
            }

            string status = gfWorkflows.GetUserPreference("Status");
            if (!string.IsNullOrWhiteSpace(status))
            {
                qry = qry.Where( w => w.Status.StartsWith(status));
            }

            var sortProperty = gWorkflows.SortProperty;
            if ( sortProperty != null )
            {
                gWorkflows.DataSource = qry.Sort( sortProperty ).ToList();
            }
            else
            {
                gWorkflows.DataSource = qry.OrderByDescending( s => s.CreatedDateTime ).ToList();
            }

            gWorkflows.DataBind();
        }

        #endregion
    }
}