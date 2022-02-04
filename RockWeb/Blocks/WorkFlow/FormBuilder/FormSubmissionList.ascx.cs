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
    /// List block the shows a list of Data Views that have persistence enabled
    /// </summary>
    [DisplayName( "Form Submission List" )]
    [Category( "WorkFlow" )]
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
        "FormBuilder Detail Page",
        Description = "Page to edit using the form builder.",
        Order = 2,
        Key = AttributeKeys.FormBuilderDetailPage )]

    [LinkedPage(
        "Analytics Detail Page",
        Description = "Page used to view the analytics for this form.",
        Order = 3,
        Key = AttributeKeys.AnalyticsDetailPage )]

    #endregion Rock Attributes

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
            public const string FormBuilderDetailPage = "FormBuilderDetailPage";
            public const string AnalyticsDetailPage = "AnalyticsDetailPage";
            public const string CommunicationsDetailPage = "CommunicationsDetailPage";
            public const string SettingsDetailPage = "SettingsDetailPage";
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

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

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
                BindGrid();
                BindFilters();
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
        /// Handles the GridRebind event of the gList control.
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
            gfWorkflows.SaveUserPreference( UserPreferenceKeys.PersonAliasId, ppPerson.PersonAliasId.ToString() );
            gfWorkflows.SaveUserPreference( UserPreferenceKeys.CampusId, cpCampus.SelectedCampusId.ToString() );

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
                if ( rowViewModel.Person == null )
                {
                    int? personLinkColumnIndex = gWorkflows.GetColumnIndex( gWorkflows.Columns.OfType<PersonProfileLinkField>().First() );
                    if( personLinkColumnIndex.HasValue && personLinkColumnIndex > -1 )
                    {
                        var personLinkButton = e.Row.Cells[personLinkColumnIndex.Value].ControlsOfTypeRecursive<HyperLink>().FirstOrDefault();
                        personLinkButton.Visible = false;
                    }
                }
            }
        }

        protected void lnkSubmissions_Click( object sender, EventArgs e )
        {
            NavigateToCurrentPage( GetQueryString( PageParameterKeys.SubmissionsTab ) );
        }

        protected void lnkFormBuilder_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKeys.FormBuilderDetailPage, GetQueryString( PageParameterKeys.FormBuilderTab ) );
        }

        protected void lnkComminucations_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKeys.CommunicationsDetailPage, GetQueryString( PageParameterKeys.CommunicationsTab ) );
        }

        protected void lnkSettings_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKeys.SettingsDetailPage, GetQueryString( PageParameterKeys.SettingsTab ) );
        }

        protected void lnkAnalytics_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKeys.AnalyticsDetailPage, GetQueryString( PageParameterKeys.AnalyticsTab ) );
        }

        #endregion Events

        #region Methods

        /// <summary>
        /// Saves available attributes as <see cref="AvailableAttributes"/> so dynamic rendering of attribute columns on the <see cref="gWorkflows"/> grid
        /// </summary>
        private void BindAttributes()
        {
            AvailableAttributes = new List<AttributeCache>();

            int entityTypeId = new Workflow().TypeId;
            foreach ( var attributeModel in new AttributeService( new RockContext() ).Queryable()
                .Where( a =>
                    a.EntityTypeId == entityTypeId &&
                    a.IsGridColumn )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name ) )
            {
                AvailableAttributes.Add( AttributeCache.Get( attributeModel ) );
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
            var personLinkField = new PersonProfileLinkField();
            personLinkField.LinkedPageAttributeKey = AttributeKeys.PersonProfilePage;
            gWorkflows.Columns.Add(personLinkField);

            // Add delete column
            var deleteField = new DeleteField();
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

            var qryGrid = workflows.Select( w => new FormSubmissionListViewModel
            {
                Id = w.Id,
                ActivatedDateTime = w.ActivatedDateTime,
                Campus = w.Campus,
                Person = w.InitiatorPersonAlias != null ? w.InitiatorPersonAlias.Person : null,
                Personid = w.InitiatorPersonAliasId
            } );

            gWorkflows.SetLinqDataSource( qryGrid );
            gWorkflows.DataBind();
        }

        /// <summary>
        /// Get and display previously selected filter values
        /// </summary>
        private void BindFilters()
        {
            var context = new RockContext();
            var personAliasId = gfWorkflows.GetUserPreference( UserPreferenceKeys.PersonAliasId ).AsIntegerOrNull();
            if ( personAliasId.HasValue )
            {
                var person = new PersonAliasService( context ).Get( personAliasId.Value );
                ppPerson.SetValue( person?.Person );
            }

            var campusId = gfWorkflows.GetUserPreference( UserPreferenceKeys.CampusId ).AsIntegerOrNull();
            if ( campusId.HasValue )
            {
                var campus = new CampusService( context ).Get( campusId.Value );
                cpCampus.SetValue( campus );
            }
        }

        private IQueryable<Workflow> ApplyFiltersAndSorting( IQueryable<Workflow> query )
        {
            var personAliasId = gfWorkflows.GetUserPreference( UserPreferenceKeys.PersonAliasId ).AsIntegerOrNull();
            if ( personAliasId.HasValue )
            {
                query = query.Where( w => w.InitiatorPersonAliasId == personAliasId );
            }

            var campusId = gfWorkflows.GetUserPreference( UserPreferenceKeys.CampusId ).AsIntegerOrNull();
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
                query = query.OrderBy( w => w.Id );
            }

            return query;
        }

        private Dictionary<string, string> GetQueryString(string tab)
        {
            return new Dictionary<string, string>
            {
                { PageParameterKeys.Tab, tab  },
                { PageParameterKeys.WorkflowTypeId, PageParameter( PageParameterKeys.WorkflowTypeId ) }
            };
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
            public int? Personid { get; set; }
        }

        #endregion
    }
}