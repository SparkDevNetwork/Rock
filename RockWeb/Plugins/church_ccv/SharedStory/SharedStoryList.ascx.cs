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

namespace RockWeb.Plugins.church_ccv.SharedStory
{
    [DisplayName( "Shared Story List" )]
    [Category( "CCV > Shared Story" )]
    [Description( "Lists all the shared stories." )]
    [LinkedPage( "Detail Page", "Page used to display details about a story." )]
    public partial class SharedStoryList : RockBlock
    {
        #region Fields
        
        private WorkflowType _workflowType = null;

        const int sYourStory_AttribId = 25997;
        const int sDifference_AttribId = 25998;
        const int sScripture_AttribId = 25999;
        const int sWorkflowTypeId = 163;

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

            if ( _workflowType != null )
            {
                gfWorkflows.ApplyFilterClick += gfWorkflows_ApplyFilterClick;
                gfWorkflows.DisplayFilterValue += gfWorkflows_DisplayFilterValue;

                // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
                this.BlockUpdated += Block_BlockUpdated;
                this.AddConfigurationUpdateTrigger( upnlSettings );

                gWorkflows.DataKeyNames = new string[] { "Id" };
                gWorkflows.Actions.ShowAdd = false;
                gWorkflows.Actions.AddClick += gWorkflows_Add;
                gWorkflows.GridRebind += gWorkflows_GridRebind;
                gWorkflows.IsDeleteEnabled = false;

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
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                SetFilter();
                BindGrid();
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

            /*if ( !string.IsNullOrWhiteSpace(GetAttributeValue("DefaultWorkflowType")) )
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
            }*/
            
            _workflowType = new WorkflowTypeService( new RockContext() ).Get( sWorkflowTypeId );
            if ( _workflowType != null )
            {
                breadCrumbs.Add( new BreadCrumb( _workflowType.Name, pageReference ) );
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
            
            if ( e.Key == MakeKeyUniqueToType( "Story" ) )
            {
                e.Value = tbStory.Text;
            }
            else if ( e.Key == MakeKeyUniqueToType( "Difference" ) )
            {
                e.Value = tbDifference.Text;
            }
            else if ( e.Key == MakeKeyUniqueToType( "Scripture" ) )
            {
                e.Value = tbScripture.Text;
            }
            else
            {
                e.Value = string.Empty;
            }
        }

        protected void gfWorkflows_ApplyFilterClick( object sender, EventArgs e )
        {
            gfWorkflows.SaveUserPreference( MakeKeyUniqueToType( "Story" ), "Story", tbStory.Text );
            gfWorkflows.SaveUserPreference( MakeKeyUniqueToType( "Difference" ), "Difference", tbDifference.Text );
            gfWorkflows.SaveUserPreference( MakeKeyUniqueToType( "Scripture" ), "Scripture", tbScripture.Text );

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
                            gfWorkflows.SaveUserPreference( MakeKeyUniqueToType( attribute.Key ), attribute.Name, attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter ).ToJson() );
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
                    qryParam.Add( "WorkflowId", workflow.Id.ToString() );
                    NavigateToLinkedPage( "EntryPage", qryParam );
                }
                else
                {
                    NavigateToLinkedPage( "DetailPage", "workflowId", e.RowKeyId );
                }
            }
        }

        #endregion

        #region Methods
        
        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void SetFilter()
        {
            BindAttributes();
            AddDynamicControls();
            
            tbStory.Text = gfWorkflows.GetUserPreference( MakeKeyUniqueToType( "Story" ) );
            tbDifference.Text = gfWorkflows.GetUserPreference( MakeKeyUniqueToType( "Difference" ) );
            tbScripture.Text = gfWorkflows.GetUserPreference( MakeKeyUniqueToType( "Scripture" ) );
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
                    AvailableAttributes.Add( AttributeCache.Read( attributeModel ) );
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
                            var rockControl = (IRockControl)control;
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

                    string savedValue = gfWorkflows.GetUserPreference( MakeKeyUniqueToType( attribute.Key ) );
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
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
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
                
                // Filter query by any configured attribute filters
                var attributeValueService = new AttributeValueService( rockContext );
                
                if ( AvailableAttributes != null && AvailableAttributes.Any() )
                {
                    var parameterExpression = attributeValueService.ParameterExpression;

                    foreach ( var attribute in AvailableAttributes )
                    {
                        var filterControl = phAttributeFilters.FindControl( "filter_" + attribute.Id.ToString() );
                        if ( filterControl != null )
                        {
                            var filterValues = attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter );
                            var expression = attribute.FieldType.Field.AttributeFilterExpression( attribute.QualifierValues, filterValues, parameterExpression );
                            if ( expression != null )
                            {
                                var attributeValues = attributeValueService
                                    .Queryable()
                                    .Where( v => v.Attribute.Id == attribute.Id );

                                attributeValues = attributeValues.Where( parameterExpression, expression, null );

                                qry = qry.Where( w => attributeValues.Select( v => v.EntityId ).Contains( w.Id ) );
                            }
                        }
                    }
                }

                // finally, filter by scripture, story and difference. 
                if( !string.IsNullOrEmpty( tbScripture.Text) || !string.IsNullOrEmpty( tbStory.Text) || !string.IsNullOrEmpty( tbDifference.Text) )
                {
                    // allow keyword style searching by splitting according to commas
                    var scriptureKeywords = tbScripture.Text.Split( new char[] {  ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList( );
                    var differenceKeywords = tbDifference.Text.Split( new char[] {  ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList( );
                    var storyKeywords = tbStory.Text.Split( new char[] {  ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList( );

                    // Build an exclusion list of workflowIds.
                    // Example: If the scripture filter is not empty, and a scriptureAttribValue does NOT have the filtered value, take that workflow ID so we can exclude it.
                    // Do the same for the story and difference.
                    var sharedStoryAttribValues = attributeValueService.Queryable().AsNoTracking( )
                                                       .Where( v => 
                                                           // scripture filter
                                                           (!string.IsNullOrEmpty( tbScripture.Text) && v.Attribute.Id == sScripture_AttribId && !scriptureKeywords.AsQueryable( ).Any( s => v.Value.ToLower( ).Contains( s.Trim( ) ) )) ||
                                                           // story filter
                                                           (!string.IsNullOrEmpty( tbStory.Text) && v.Attribute.Id == sYourStory_AttribId && !storyKeywords.AsQueryable( ).Any( s => v.Value.ToLower( ).Contains( s.Trim( ) ) )) ||
                                                           // difference filter
                                                           (!string.IsNullOrEmpty( tbDifference.Text) && v.Attribute.Id == sDifference_AttribId && !differenceKeywords.AsQueryable( ).Any( s => v.Value.ToLower( ).Contains( s.Trim( ) ) )))
                                                       .Select( v => v.EntityId );

                    // now we have a list of workflow Ids that fail one of the filters. Remove them from our list.
                    qry = qry.Where( w => !sharedStoryAttribValues.Contains( w.Id ) );
                }

                
                IQueryable<Workflow> workflows = null;

                var sortProperty = gWorkflows.SortProperty;
                if ( sortProperty != null )
                {
                    workflows = qry.Sort( sortProperty );
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

                gWorkflows.EntityTypeId = EntityTypeCache.Read<Workflow>().Id;
                var qryGrid = workflows.Select( w => new
                {
                    w.Id,
                    w.WorkflowId,
                    SubmittedDate = w.CreatedDateTime
                } );

                gWorkflows.SetLinqDataSource( qryGrid );
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