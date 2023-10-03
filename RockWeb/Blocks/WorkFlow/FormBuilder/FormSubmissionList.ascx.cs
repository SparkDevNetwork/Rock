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
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls;

namespace RockWeb.Blocks.WorkFlow.FormBuilder
{
    /// <summary>
    /// List block that shows a list of Data Views that have persistence enabled
    /// </summary>
    [DisplayName( "Form Submission List" )]
    [Category( "WorkFlow > FormBuilder" )]
    [Description( "Shows a list forms submitted for a given FormBuilder form." )]

    #region Rock Attributes

    [LinkedPage(
        "Detail Page",
        Description = "Page to display details about a workflow.",
        Order = 0,
        Key = AttributeKeys.DetailPage,
        DefaultValue = Rock.SystemGuid.Page.WORKFLOW_DETAIL )]

    [LinkedPage(
        "Person Profile Page",
        Description = "Page to display person details.",
        Order = 1,
        Key = AttributeKeys.PersonProfilePage,
        DefaultValue = Rock.SystemGuid.Page.PERSON_PROFILE_PERSON_PAGES )]

    [LinkedPage(
        "Form Builder Page",
        Description = "The page that has the form builder editor.",
        Order = 2,
        Key = AttributeKeys.FormBuilderPage )]

    [LinkedPage(
        "Analytics Page",
        Description = " The page that shows the analytics for this form.",
        Order = 3,
        Key = AttributeKeys.AnalyticsPage )]

    [LinkedPage( "Entry Page",
        Description = "Page used to launch a new workflow of the selected type.",
        Order = 4,
        Key = AttributeKeys.EntryPage,
        DefaultValue = Rock.SystemGuid.Page.WORKFLOW_ENTRY )]

    #endregion Rock Attributes

    [Rock.SystemGuid.BlockTypeGuid( "A23592BB-25F7-4A81-90CD-46700724110A" )]
    public partial class FormSubmissionList : RockBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKeys
        {
            public const string DetailPage = "DetailPage";
            public const string PersonProfilePage = "PersonProfilePage";
            public const string FormBuilderPage = "FormBuilderPage";
            public const string AnalyticsPage = "AnalyticsPage";
            public const string EntryPage = "EntryPage";
        }

        #endregion Attribute Keys

        #region Page Parameter Keys

        /// <summary>
        /// Keys for page parameters extracted from the page route
        /// </summary>
        private static class PageParameterKeys
        {
            public const string WorkflowTypeId = "WorkflowTypeId";
            public const string WorkflowId = "WorkflowId";

            public const string Tab = "Tab";

            public const string SubmissionsTab = "Submissions";
            public const string FormBuilderTab = "FormBuilder";
            public const string CommunicationsTab = "Communications";
            public const string SettingsTab = "Settings";
            public const string AnalyticsTab = "Analytics";
        }

        #endregion Page Parameter Keys

        #region User Preference Keys

        /// <summary>
        /// Keys to use for UserPreferences
        /// </summary>
        protected static class UserPreferenceKeys
        {
            public const string CampusId = "CampusId";
            public const string PersonAliasId = "PersonAliasId";
        }

        #endregion User Preferance Keys

        #region fields

        /// <summary>
        /// Gets or sets the available attributes.
        /// </summary>
        /// <value>
        /// The available attributes.
        /// </value>
        public List<AttributeCache> AvailableAttributes { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the ModalAlert control that shows page-level notifications to the user.
        /// </summary>
        public ModalAlert ModalAlertControl { get; set; }

        #endregion Properties

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gWorkflows.DataKeyNames = new string[] { "Id" };
            gfWorkflows.ApplyFilterClick += gfWorkflows_ApplyFilterClick;

            gWorkflows.GridRebind += gWorkflows_GridRebind;
            gWorkflows.RowDataBound += gWorkflows_RowDataBound;

            gWorkflows.Actions.ShowAdd = WorkflowTypeCache.Get( PageParameter( PageParameterKeys.WorkflowTypeId ).AsInteger() ).Category.IsAuthorized( Authorization.EDIT, CurrentPerson );
            gWorkflows.Actions.AddClick += gWorkflows_Add;

            gWorkflows.Actions.ShowCommunicate = false;
            gWorkflows.Actions.ShowMergePerson = false;
            gWorkflows.Actions.ShowMergeTemplate = true;
            gWorkflows.Actions.ShowBulkUpdate = false;
            gWorkflows.ShowWorkflowOrCustomActionButtons = true;
            gWorkflows.EnableDefaultLaunchWorkflow = true;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            ModalAlertControl = mdAlert;

            BindAttributes();
            AddDynamicControls();
        }

        /// <summary>
        /// Raises the <see cref="System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                var workflowType = new WorkflowTypeService( new RockContext() ).Get( PageParameter( PageParameterKeys.WorkflowTypeId ).AsInteger() );
                if ( workflowType != null )
                {
                    lTitle.Text = $"{workflowType.Name} Form";
                    BindGrid();
                    BindFilters();
                }
                else
                {
                    pnlView.Visible = false;
                }
            }

            base.OnLoad( e );
        }

        #endregion Base Control Methods

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gWorkflows control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.GridRebindEventArgs"/> instance containing the event data.</param>
        private void gWorkflows_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the RowSelected event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gWorkflows_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKeys.DetailPage, PageParameterKeys.WorkflowId, e.RowKeyId );
        }

        private void gfWorkflows_ApplyFilterClick( object sender, EventArgs e )
        {
            gfWorkflows.SetFilterPreference( UserPreferenceKeys.PersonAliasId, ppPerson.PersonAliasId.ToString() );
            gfWorkflows.SetFilterPreference( UserPreferenceKeys.CampusId, cpCampus.SelectedCampusId.ToString() );

            BindGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        private void gWorkflows_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var rowViewModel = e.Row.DataItem as FormSubmissionListViewModel;
                var attributeFields = gWorkflows.Columns.OfType<AttributeField>();
                var personField = attributeFields.FirstOrDefault( a => a.DataField == "Person" );
                int? personLinkColumnIndex = gWorkflows.GetColumnIndex( gWorkflows.Columns.OfType<RockLiteralField>().First() );

                if ( personField == null )
                {
                    if ( personLinkColumnIndex.HasValue && personLinkColumnIndex > -1 )
                    {
                        HidePersonLinkButton();
                    }
                }
                else
                {
                    string key = gWorkflows.DataKeys[e.Row.RowIndex].Value.ToString();
                    if ( !string.IsNullOrWhiteSpace( key ) && gWorkflows.ObjectList.ContainsKey( key ) )
                    {
                        var dataItem = gWorkflows.ObjectList[key] as IHasAttributes;
                        var value = dataItem?.GetAttributeValue( personField.DataField );

                        if ( string.IsNullOrWhiteSpace( value ) )
                        {
                            HidePersonLinkButton();
                        }
                        else
                        {
                            var personService = new PersonAliasService( new RockContext() );
                            var personAlias = personService.Get( value.AsGuid() );

                            if ( personAlias == null )
                            {
                                HidePersonLinkButton();
                            }
                            else
                            {
                                e.Row.Cells[personLinkColumnIndex.Value].Text = $"<a class='btn btn-default btn-sm' href='/person/{personAlias.PersonId}'><i class='fa fa-user'></i></a>";

                                rowViewModel.Person = personAlias.Person;
                                rowViewModel.PersonId = personAlias.PersonId;
                            }
                        }
                    }
                }

                void HidePersonLinkButton()
                {
                    var personLinkButton = e.Row.Cells[personLinkColumnIndex.Value].ControlsOfTypeRecursive<HyperLink>().FirstOrDefault();
                    if ( personLinkButton != null )
                    {
                        personLinkButton.Visible = false;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Delete event of the gWorkflows control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        /// <exception cref="NotImplementedException"></exception>
        private void gWorkflows_Delete( object sender, RowEventArgs e )
        {
            var id = ( int ) e.RowKeyValue;

            if ( CanDelete( id ) )
            {
                BindGrid();
            }
        }

        protected void lnkSubmissions_Click( object sender, EventArgs e )
        {
            NavigateToCurrentPage( GetQueryString( PageParameterKeys.SubmissionsTab ) );
        }

        protected void lnkFormBuilder_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKeys.FormBuilderPage, GetQueryString( PageParameterKeys.FormBuilderTab ) );
        }

        protected void lnkComminucations_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKeys.FormBuilderPage, GetQueryString( PageParameterKeys.CommunicationsTab ) );
        }

        protected void lnkSettings_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKeys.FormBuilderPage, GetQueryString( PageParameterKeys.SettingsTab ) );
        }

        protected void lnkAnalytics_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKeys.AnalyticsPage, GetQueryString( PageParameterKeys.AnalyticsTab ) );
        }

        private void gWorkflows_Add( object sender, EventArgs e )
        {
            var workflowType = new WorkflowTypeService( new RockContext() ).Get( PageParameter( PageParameterKeys.WorkflowTypeId ).AsInteger() );
            NavigateToLinkedPage( AttributeKeys.EntryPage, "WorkflowTypeId", workflowType.Id );
        }

        #endregion Events

        #region Methods

        /// <summary>
        /// Saves available attributes as <see cref="AvailableAttributes"/> so dynamic rendering of attribute columns on the <see cref="gWorkflows"/> grid
        /// </summary>
        private void BindAttributes()
        {
            var rockContext = new RockContext();
            AvailableAttributes = new List<AttributeCache>();
            var workflowType = new WorkflowTypeService( rockContext ).Get( PageParameter( PageParameterKeys.WorkflowTypeId ).AsInteger() );

            if ( workflowType != null )
            {
                int entityTypeId = new Workflow().TypeId;
                foreach ( var attributeModel in new AttributeService( rockContext ).Queryable()
                    .Where( a =>
                        a.EntityTypeId == entityTypeId &&
                        a.IsGridColumn &&
                        a.EntityTypeQualifierColumn.Equals( "WorkflowTypeId", StringComparison.OrdinalIgnoreCase ) &&
                        a.EntityTypeQualifierValue.Equals( workflowType.Id.ToString() ) )
                    .OrderBy( a => a.Order )
                    .ThenBy( a => a.Name ) )
                {
                    AvailableAttributes.Add( AttributeCache.Get( attributeModel ) );
                }
            }
        }

        /// <summary>
        /// Dynamically display attributes added to the rendered workflow as columns in the <see cref="gWorkflows"/> grid
        /// </summary>
        private void AddDynamicControls()
        {
            // Remove attribute columns
            foreach ( var column in gWorkflows.Columns.OfType<AttributeField>().ToList() )
            {
                gWorkflows.Columns.Remove( column );
            }

            if ( AvailableAttributes != null )
            {
                foreach ( var attribute in AvailableAttributes )
                {
                    bool columnExists = gWorkflows.Columns.OfType<AttributeField>().Any( a => a.AttributeId == attribute.Id );
                    if ( !columnExists )
                    {
                        AttributeField boundField = new AttributeField();
                        boundField.DataField = attribute.Key;
                        boundField.AttributeId = attribute.Id;
                        boundField.HeaderText = attribute.Name;

                        var attributeCache = AttributeCache.Get( attribute.Id );
                        if ( attributeCache != null )
                        {
                            boundField.ItemStyle.HorizontalAlign = attributeCache.FieldType.Field.AlignValue;
                        }

                        gWorkflows.Columns.Add( boundField );
                    }
                }
            }

            // Add PersonLinkField column
            var personLinkField = new RockLiteralField();
            personLinkField.ItemStyle.HorizontalAlign = HorizontalAlign.Right;
            gWorkflows.Columns.Add( personLinkField );

            // Add delete column
            var deleteField = new DeleteField();
            deleteField.Click += gWorkflows_Delete;
            gWorkflows.Columns.Add( deleteField );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var workflowTypeId = PageParameter( PageParameterKeys.WorkflowTypeId ).AsInteger();

            RockContext rockContext = new RockContext();
            var workflowService = new WorkflowService( rockContext );

            var workflows = workflowService.Queryable( "Campus,InitiatorPersonAlias.Person" ).AsNoTracking().Where( w => w.WorkflowTypeId == workflowTypeId );
            workflows = ApplyFiltersAndSorting( workflows );

            gWorkflows.ObjectList = workflows.AsEnumerable().ToDictionary( k => k.Id.ToString(), v => v as object );
            gWorkflows.EntityTypeId = EntityTypeCache.Get<Workflow>().Id;

            var submissionsList = workflows.Select( w => new FormSubmissionListViewModel
            {
                Id = w.Id,
                ActivatedDateTime = w.ActivatedDateTime,
                Campus = w.Campus
            } );

            gWorkflows.SetLinqDataSource( submissionsList );
            gWorkflows.DataBind();
        }

        /// <summary>
        /// Get and display previously selected filter values
        /// </summary>
        private void BindFilters()
        {
            var context = new RockContext();
            var personAliasId = gfWorkflows.GetFilterPreference( UserPreferenceKeys.PersonAliasId ).AsIntegerOrNull();
            if ( personAliasId.HasValue )
            {
                var person = new PersonAliasService( context ).Get( personAliasId.Value );
                ppPerson.SetValue( person?.Person );
            }

            var campusId = gfWorkflows.GetFilterPreference( UserPreferenceKeys.CampusId ).AsIntegerOrNull();
            if ( campusId.HasValue )
            {
                var campus = new CampusService( context ).Get( campusId.Value );
                cpCampus.SetValue( campus );
            }
        }

        private IQueryable<Workflow> ApplyFiltersAndSorting( IQueryable<Workflow> query )
        {
            var personAliasId = gfWorkflows.GetFilterPreference( UserPreferenceKeys.PersonAliasId ).AsIntegerOrNull();
            if ( personAliasId.HasValue )
            {
                query = query.Where( w => w.InitiatorPersonAliasId == personAliasId );
            }

            var campusId = gfWorkflows.GetFilterPreference( UserPreferenceKeys.CampusId ).AsIntegerOrNull();
            if ( campusId.HasValue )
            {
                query = query.Where( w => w.CampusId == campusId );
            }

            var sortProperty = gWorkflows.SortProperty;
            if ( gWorkflows.AllowSorting && sortProperty != null )
            {
                query = query.Sort( sortProperty );
            }
            else
            {
                query = query.OrderBy( w => w.ActivatedDateTime );
            }

            return query;
        }

        private Dictionary<string, string> GetQueryString( string tab )
        {
            return new Dictionary<string, string>
            {
                { PageParameterKeys.Tab, tab  },
                { PageParameterKeys.WorkflowTypeId, PageParameter( PageParameterKeys.WorkflowTypeId ) }
            };
        }

        /// <summary>
        /// Show an alert message that requires user acknowledgement to continue.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="alertType"></param>
        private void ShowAlert( string message, ModalAlertType alertType )
        {
            ModalAlertControl.Show( message, alertType );
        }

        private bool CanDelete( int id )
        {
            RockContext rockContext = new RockContext();
            var workflowService = new WorkflowService( rockContext );
            var workflow = workflowService.Get( id );
            string errorMessage;

            if ( workflow == null )
            {
                ShowAlert( "This item could not be found", ModalAlertType.Information );
                return false;
            }

            if ( !workflowService.CanDelete( workflow, out errorMessage ) )
            {
                ShowAlert( errorMessage, ModalAlertType.Warning );
                return false;
            }

            workflowService.Delete( workflow );
            rockContext.SaveChanges();
            return true;
        }

        #endregion Methods

        #region Support Classes

        /// <summary>
        /// View model for <see cref="gWorkflows"/> grid data
        /// </summary>
        private class FormSubmissionListViewModel
        {
            public int Id { get; set; }
            public DateTime? ActivatedDateTime { get; set; }
            public Campus Campus { get; set; }
            public Person Person { get; set; }
            public int? PersonId { get; set; }
        }

        #endregion
    }
}