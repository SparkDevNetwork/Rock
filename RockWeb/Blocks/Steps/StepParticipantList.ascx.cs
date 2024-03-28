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
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Steps
{
    [DisplayName( "Step Participant List" )]
    [Category( "Steps" )]
    [Description( "Lists all the participants in a Step." )]
    [ContextAware( typeof( Campus ) )]

    #region Block Attributes

    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage,
        Order = 1 )]
    [LinkedPage(
        "Person Profile Page",
        Description = "Page used for viewing a person's profile. If set a view profile button will show for each participant.",
        Key = AttributeKey.ProfilePage,
        IsRequired = false,
        Order = 2 )]
    [BooleanField(
        "Show Note Column",
        Key = AttributeKey.ShowNoteColumn,
        Description = "Should the note be displayed as a separate grid column (instead of displaying a note icon under person's name)?",
        IsRequired = false,
        Order = 3 )]

    #endregion

    [Rock.SystemGuid.BlockTypeGuid( "2E4A1578-145E-4052-9B56-1739F7366827" )]
    public partial class StepParticipantList : ContextEntityBlock, ISecondaryBlock, ICustomGridColumns
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The detail page
            /// </summary>
            public const string DetailPage = "DetailPage";

            /// <summary>
            /// The profile page
            /// </summary>
            public const string ProfilePage = "PersonProfilePage";

            /// <summary>
            /// The show note column
            /// </summary>
            public const string ShowNoteColumn = "ShowNoteColumn";

            /// <summary>
            /// The step type
            /// </summary>
            public const string StepType = "StepType";
        }

        #endregion Attribute Keys

        #region Page Parameter Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            /// <summary>
            /// The step type identifier
            /// </summary>
            public const string StepTypeId = "StepTypeId";

            /// <summary>
            /// The step identifier
            /// </summary>
            public const string StepId = "StepId";
        }

        #endregion Page Parameter Keys

        #region Filter Keys

        /// <summary>
        /// Keys to use for Filters
        /// </summary>
        private static class FilterKey
        {
            public const string FirstName = "FirstName";
            public const string LastName = "LastName";
            public const string StepStatus = "StepStatus";
            public const string DateStarted = "DateStarted";
            public const string DateCompleted = "DateCompleted";
            public const string Note = "Note";
            public const string Campus = "Campus";
        }

        #endregion Filter Keys

        #region Private Variables

        private RockContext _dataContext;
        private DefinedValueCache _inactiveStatus = null;
        private StepType _stepType = null;
        private bool _canView = false;

        private RockLiteralField _fullNameField = null;
        private RockLiteralField _nameWithHtmlField = null;
        private RockLiteralField _stepStatusField = null;
        private RockBoundField _stepDateStartedField = null;

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
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            InitializeBlockContext();

            InitializeScripts();

            InitializeFilter();

            InitializeGrid();

            InitializeSettingsNotification( upMain );
        }

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            if ( ViewState["AvailableAttributeIds"] != null )
            {
                AvailableAttributes = ( ViewState["AvailableAttributeIds"] as int[] ).Select( a => AttributeCache.Get( a ) ).ToList();
            }

            AddDynamicControls();
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
                GetAvailableAttributes();
                AddDynamicControls();

                pnlContent.Visible = _canView;

                if ( _canView )
                {
                    BindFilter();
                    BindParticipantsGrid();
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
            ViewState["AvailableAttributeIds"] = AvailableAttributes == null ? null : AvailableAttributes.Select( a => a.Id ).ToArray();

            return base.SaveViewState();
        }

        #endregion

        #region Steps Grid

        private readonly string _photoFormat = "<div class=\"photo-icon photo-round photo-round-xs pull-left margin-r-sm js-person-popover\" personid=\"{0}\" data-original=\"{1}&w=50\" style=\"background-image: url( '{2}' ); background-size: cover; background-repeat: no-repeat;\"></div>";

        private bool _showNoteColumn = false;

        /// <summary>
        /// Handles the RowDataBound event of the gSteps control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gSteps_RowDataBound( object sender, System.Web.UI.WebControls.GridViewRowEventArgs e )
        {
            if ( e.Row.RowType != DataControlRowType.DataRow )
            {
                return;
            }

            var step = e.Row.DataItem as StepParticipantListViewModel;

            if ( step == null )
            {
                return;
            }

            int stepId = step.Id;

            var stepPerson = step.Person;

            var lFullName = e.Row.FindControl( _fullNameField.ID ) as Literal;
            if ( lFullName != null )
            {
                lFullName.Text = stepPerson.FullNameReversed;
            }

            // Set the status HTML with the correct status color
            var lStepStatusHtml = e.Row.FindControl( _stepStatusField.ID ) as Literal;
            var statusesHtml = GetStepStatusesHtml();

            if ( lStepStatusHtml != null && statusesHtml != null && step.StepStatusId.HasValue && statusesHtml.ContainsKey( step.StepStatusId.Value ) )
            {
                var statusHtml = statusesHtml[step.StepStatusId.Value];
                lStepStatusHtml.Text = statusHtml;
            }

            var lNameWithHtml = e.Row.FindControl( _nameWithHtmlField.ID ) as Literal;
            if ( lNameWithHtml != null )
            {
                var sbNameHtml = new StringBuilder();

                sbNameHtml.AppendFormat( _photoFormat, stepPerson.Id, stepPerson.PhotoUrl, ResolveUrl( "~/Assets/Images/person-no-photo-unknown.svg" ) );
                sbNameHtml.Append( stepPerson.FullName );

                if ( stepPerson.TopSignalColor.IsNotNullOrWhiteSpace() )
                {
                    sbNameHtml.Append( stepPerson.GetSignalMarkup() );
                }

                if ( _showNoteColumn && step.Note.IsNotNullOrWhiteSpace() )
                {
                    sbNameHtml.Append( " <span class='js-member-note' data-toggle='tooltip' data-placement='top' title='" + step.Note.EncodeHtml() + "'><i class='fa fa-file-text-o text-info'></i></span>" );
                }

                lNameWithHtml.Text = sbNameHtml.ToString();
            }

            if ( stepPerson.IsDeceased )
            {
                e.Row.AddCssClass( "is-deceased" );
            }

            if ( _inactiveStatus != null && stepPerson.RecordStatusValueId == _inactiveStatus.Id )
            {
                e.Row.AddCssClass( "is-inactive-person" );
            }
        }

        /// <summary>
        /// Handles the GetRecipientMergeFields event of the gSteps control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GetRecipientMergeFieldsEventArgs"/> instance containing the event data.</param>
        protected void gSteps_GetRecipientMergeFields( object sender, GetRecipientMergeFieldsEventArgs e )
        {
            Step stepRow = e.DataItem as Step;

            if ( stepRow == null )
            {
                return;
            }

            var dataContext = GetDataContext();

            var step = new StepService( dataContext ).Get( stepRow.Id );

            step.LoadAttributes();

            var mergefields = e.MergeValues;

            e.MergeValues.Add( "StepStatus", step.StepStatus.Name );
            e.MergeValues.Add( "StepName", step.StepType.Name );

            dynamic dynamicAttributeCarrier = new RockDynamic();

            foreach ( var attributeKeyValue in step.AttributeValues )
            {
                dynamicAttributeCarrier[attributeKeyValue.Key] = attributeKeyValue.Value.Value;
            }

            e.MergeValues.Add( "StepAttributes", dynamicAttributeCarrier );
        }

        #region Filter Settings

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SetFilterPreference( FilterKey.FirstName, "First Name", tbFirstName.Text );
            rFilter.SetFilterPreference( FilterKey.LastName, "Last Name", tbLastName.Text );
            rFilter.SetFilterPreference( FilterKey.StepStatus, "Status", cblStepStatus.SelectedValues.AsDelimited( ";" ) );
            rFilter.SetFilterPreference( FilterKey.DateStarted, "Date Started", drpDateStarted.DelimitedValues );
            rFilter.SetFilterPreference( FilterKey.DateCompleted, "Date Completed", drpDateCompleted.DelimitedValues );
            rFilter.SetFilterPreference( FilterKey.Note, "Note", tbNote.Text );
            rFilter.SetFilterPreference( FilterKey.Campus, "Campus", cpCampusFilter.SelectedCampusId.ToString() );

            // Save filter settings for custom attributes.
            if ( this.AvailableAttributes != null )
            {
                foreach ( var attribute in this.AvailableAttributes )
                {
                    var filterControl = phAttributeFilters.FindControl( "filter_" + attribute.Id.ToString() );

                    if ( filterControl != null )
                    {
                        try
                        {
                            var values = attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter );

                            rFilter.SetFilterPreference( attribute.Key, attribute.Name, attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter ).ToJson() );
                        }
                        catch
                        {
                            // Ignore
                        }
                    }
                    else
                    {
                        // If this Attribute column is no longer available in the grid, remove the associated user preference.
                        rFilter.SetFilterPreference( attribute.Key, attribute.Name, null );
                    }
                }
            }

            BindParticipantsGrid();
        }

        /// <summary>
        /// Format a filter value to friendly text for display.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            if ( e.Key == FilterKey.StepStatus )
            {
                e.Value = GetStepStatusNames( e.Value, cblStepStatus );
            }
            else if ( e.Key == FilterKey.DateStarted || e.Key == FilterKey.DateCompleted )
            {
                e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
            }
            else if ( e.Key == FilterKey.FirstName || e.Key == FilterKey.LastName || e.Key == FilterKey.Note )
            {
                // No change
            }
            else if ( e.Key == FilterKey.Campus )
            {
                var campus = CampusCache.Get( e.Value.ToIntSafe() );
                var campusContext = GetCampusContextOrNull();

                if ( campus != null && campusContext == null )
                {
                    e.Value = campus.Name;
                }
                else
                {
                    e.Value = string.Empty;
                }
            }
            else
            {
                // Find a matching Attribute.
                if ( this.AvailableAttributes != null )
                {
                    var attribute = AvailableAttributes.FirstOrDefault( a => a.Key == e.Key );

                    if ( attribute != null )
                    {
                        try
                        {
                            var values = JsonConvert.DeserializeObject<List<string>>( e.Value );

                            e.Value = attribute.FieldType.Field.FormatFilterValues( attribute.QualifierValues, values );

                            return;
                        }
                        catch
                        {
                            // Ignore
                        }
                    }
                }

                // Invalid filter field, so ignore the filter value.
                e.Value = string.Empty;
            }
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rFilter_ClearFilterClick( object sender, EventArgs e )
        {
            rFilter.DeleteFilterPreferences();

            // Recreate the Attribute Filter fields to clear the filter values.
            AddAttributeFilterFields();

            BindFilter();
        }

        #endregion

        /// <summary>
        /// Handles the Click event of the delete/archive button in the grid
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs" /> instance containing the event data.</param>
        protected void DeleteStep_Click( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var dataContext = GetDataContext();

            var stepService = new StepService( dataContext );

            var step = stepService.Get( e.RowKeyId );

            if ( step != null )
            {
                string errorMessage;

                if ( !stepService.CanDelete( step, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                stepService.Delete( step );

                dataContext.SaveChanges();
            }

            BindParticipantsGrid();
        }

        /// <summary>
        /// Handles the AddClick event of the gSteps control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gSteps_AddClick( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.StepId, 0, PageParameterKey.StepTypeId, _stepType.Id );
        }

        /// <summary>
        /// Handles the Edit event of the gSteps control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gSteps_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.StepId, e.RowKeyId, PageParameterKey.StepTypeId, _stepType.Id );
        }

        /// <summary>
        /// Handles the GridRebind event of the gSteps control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs" /> instance containing the event data.</param>
        protected void gSteps_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindParticipantsGrid( e.IsExporting, e.IsCommunication );
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Retrieve a singleton data context for data operations in this block.
        /// </summary>
        /// <returns></returns>
        private RockContext GetDataContext()
        {
            if ( _dataContext == null )
            {
                _dataContext = new RockContext();
            }

            return _dataContext;
        }

        /// <summary>
        /// Initialize handlers for block configuration changes.
        /// </summary>
        /// <param name="triggerPanel"></param>
        private void InitializeSettingsNotification( UpdatePanel triggerPanel )
        {
            // Set up Block Settings change notification.
            BlockUpdated += Block_BlockUpdated;

            AddConfigurationUpdateTrigger( triggerPanel );
        }

        /// <summary>
        /// Initialize the essential context in which this block is operating.
        /// </summary>
        private void InitializeBlockContext()
        {
            // if this block has a specific StepTypeId set, use that, otherwise, determine it from the PageParameters
            var stepTypeGuid = GetAttributeValue( AttributeKey.StepType ).AsGuid();

            int stepTypeId = 0;

            if ( stepTypeGuid == Guid.Empty )
            {
                stepTypeId = PageParameter( PageParameterKey.StepTypeId ).AsInteger();
            }

            if ( !( stepTypeId == 0 && stepTypeGuid == Guid.Empty ) )
            {
                string key = string.Format( "StepType:{0}", stepTypeId );

                _stepType = RockPage.GetSharedItem( key ) as StepType;

                if ( _stepType == null )
                {
                    var dataContext = GetDataContext();

                    _stepType = new StepTypeService( dataContext ).Queryable()
                                        .Where( g => g.Id == stepTypeId || g.Guid == stepTypeGuid )
                                        .FirstOrDefault();

                    RockPage.SaveSharedItem( key, _stepType );
                }
            }

            if ( _stepType != null
                && _stepType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
            {
                _canView = true;
            }
        }

        /// <summary>
        /// Initialize the list filter.
        /// </summary>
        private void InitializeFilter()
        {
            if ( !Page.IsPostBack )
            {
                if ( _stepType != null )
                {
                    rFilter.PreferenceKeyPrefix = string.Format( "{0}-", _stepType.Id );
                }
            }

            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
            rFilter.DisplayFilterValue += rFilter_DisplayFilterValue;
            rFilter.ClearFilterClick += rFilter_ClearFilterClick;
        }

        /// <summary>
        /// Initializes the grid.
        /// </summary>
        private void InitializeGrid()
        {
            gSteps.DataKeyNames = new string[] { "Id" };
            gSteps.PersonIdField = "PersonId";
            gSteps.GetRecipientMergeFields += gSteps_GetRecipientMergeFields;
            gSteps.Actions.AddClick += gSteps_AddClick;
            gSteps.GridRebind += gSteps_GridRebind;
            gSteps.RowItemText = "Step Participant";
            gSteps.ExportSource = ExcelExportSource.DataSource;
            gSteps.ShowConfirmDeleteDialog = true;

            var canEdit = false;
            /*
             Block Authorization is removed once the parent authority for Step is Set as Step Type.
            */
            if ( _stepType != null )
            {
                canEdit = _stepType.IsAuthorized( Authorization.EDIT, CurrentPerson ) || _stepType.IsAuthorized( Authorization.MANAGE_STEPS, CurrentPerson );
            }

            gSteps.Actions.ShowAdd = canEdit;
            gSteps.IsDeleteEnabled = canEdit;

            if ( _stepType != null )
            {
                gSteps.ExportFilename = _stepType.Name;
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the StepList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindFilter();

            AddGridRowButtons();

            BindParticipantsGrid();
        }

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            if ( _stepType != null
                 && _stepType.StepProgram != null )
            {
                cblStepStatus.DataSource = _stepType.StepProgram.StepStatuses.OrderBy( x => x.Order ).Select( x => new { Value = x.Id.ToString(), Text = x.Name } ).ToList();
                cblStepStatus.DataValueField = "Value";
                cblStepStatus.DataTextField = "Text";

                cblStepStatus.DataBind();
            }

            tbFirstName.Text = rFilter.GetFilterPreference( FilterKey.FirstName );
            tbLastName.Text = rFilter.GetFilterPreference( FilterKey.LastName );
            tbNote.Text = rFilter.GetFilterPreference( FilterKey.Note );
            cpCampusFilter.SelectedCampusId = rFilter.GetFilterPreference( FilterKey.Campus ).AsIntegerOrNull();

            string statusValue = rFilter.GetFilterPreference( FilterKey.StepStatus );
            if ( !string.IsNullOrWhiteSpace( statusValue ) )
            {
                cblStepStatus.SetValues( statusValue.Split( ';' ).ToList() );
            }

            drpDateStarted.DelimitedValues = rFilter.GetFilterPreference( FilterKey.DateStarted );
            drpDateCompleted.DelimitedValues = rFilter.GetFilterPreference( FilterKey.DateCompleted );
        }

        /// <summary>
        /// Gets the collection of Step Attributes available for display to the current user.
        /// </summary>
        private void GetAvailableAttributes()
        {
            // Parse the attribute filters
            this.AvailableAttributes = new List<AttributeCache>();

            if ( _stepType != null )
            {
                var dataContext = this.GetDataContext();

                int entityTypeId = new Step().TypeId;

                string entityTypeQualifier = _stepType.Id.ToString();

                foreach ( var attribute in new AttributeService( dataContext ).GetByEntityTypeQualifier( entityTypeId, "StepTypeId", entityTypeQualifier, true )
                    .Where( a => a.IsGridColumn )
                    .OrderByDescending( a => a.EntityTypeQualifierColumn )
                    .ThenBy( a => a.Order )
                    .ThenBy( a => a.Name ).ToAttributeCacheList() )
                {
                    if ( attribute.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                    {
                        AvailableAttributes.Add( attribute );
                    }
                }
            }
        }

        /// <summary>
        /// Adds the dynamically-created controls to the page.
        /// </summary>
        private void AddDynamicControls()
        {
            AddAttributeColumns();
            AddAttributeFilterFields();
            AddGridRowButtons();
            ConditionallyHideCampusFilter();
        }

        /// <summary>
        /// Adds the Attribute columns to the grid.
        /// </summary>
        private void AddAttributeColumns()
        {
            // Clear dynamic controls so we can re-add them
            RemoveAttributeColumns();

            if ( AvailableAttributes != null )
            {
                foreach ( var attribute in AvailableAttributes )
                {
                    bool columnExists = gSteps.Columns.OfType<AttributeField>().FirstOrDefault( a => a.AttributeId == attribute.Id ) != null;
                    if ( !columnExists )
                    {
                        AttributeField boundField = new AttributeField();
                        boundField.DataField = attribute.Key;
                        boundField.AttributeId = attribute.Id;
                        boundField.HeaderText = attribute.Name;
                        boundField.ItemStyle.HorizontalAlign = HorizontalAlign.Left;

                        gSteps.Columns.Add( boundField );
                    }
                }
            }
        }

        /// <summary>
        /// Add the available Attribute fields to the grid filter.
        /// </summary>
        private void AddAttributeFilterFields()
        {
            // Clear the filter controls
            phAttributeFilters.Controls.Clear();

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

                        string savedValue = rFilter.GetFilterPreference( attribute.Key );
                        if ( !string.IsNullOrWhiteSpace( savedValue ) )
                        {
                            try
                            {
                                var values = JsonConvert.DeserializeObject<List<string>>( savedValue );
                                attribute.FieldType.Field.SetFilterValues( control, attribute.QualifierValues, values );
                            }
                            catch
                            {
                                // intentionally ignore
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Remove the Attribute columns from the grid.
        /// </summary>
        private void RemoveAttributeColumns()
        {
            // Remove attribute columns
            foreach ( var column in gSteps.Columns.OfType<AttributeField>().ToList() )
            {
                gSteps.Columns.Remove( column );
            }
        }

        /// <summary>
        /// Hides the campus filter, if a context campus has been selected
        /// </summary>
        private void ConditionallyHideCampusFilter()
        {
            var campusContext = GetCampusContextOrNull();
            if ( campusContext != null )
            {
                cpCampusFilter.Visible = false;
            }
        }

        /// <summary>
        /// Gets the campus context, returns null if there is only no more than one active campus.
        /// This is to prevent to filtering out of Steps that are associated with currently inactive
        /// campuses or no campus at all.
        /// </summary>
        /// <returns></returns>
        private Campus GetCampusContextOrNull()
        {
            return ( CampusCache.All( false ).Count > 1 ) ? ContextEntity<Campus>() : null;
        }

        /// <summary>
        /// Removes the "delete" row button columns and any HyperLinkField/LinkButtonField columns.
        /// </summary>
        private void RemoveRowButtons()
        {
            // Remove added button columns
            DataControlField buttonColumn = gSteps.Columns.OfType<DeleteField>().FirstOrDefault( c => c.ItemStyle.CssClass == "grid-columncommand" );
            if ( buttonColumn != null )
            {
                gSteps.Columns.Remove( buttonColumn );
            }

            buttonColumn = gSteps.Columns.OfType<HyperLinkField>().FirstOrDefault( c => c.ItemStyle.CssClass == "grid-columncommand" );
            if ( buttonColumn != null )
            {
                gSteps.Columns.Remove( buttonColumn );
            }

            buttonColumn = gSteps.Columns.OfType<LinkButtonField>().FirstOrDefault( c => c.ItemStyle.CssClass == "grid-columncommand" );
            if ( buttonColumn != null )
            {
                gSteps.Columns.Remove( buttonColumn );
            }
        }

        /// <summary>
        /// Adds the PersonProfilePage button link and the "delete" row button column.
        /// </summary>
        private void AddGridRowButtons()
        {
            // Add Link to Profile Page Column
            if ( !string.IsNullOrEmpty( GetAttributeValue( "PersonProfilePage" ) ) )
            {
                var personProfileLinkField = new PersonProfileLinkField
                {
                    LinkedPageAttributeKey = "PersonProfilePage"
                };

                gSteps.Columns.Add( personProfileLinkField );
            }

            // Add delete column
            var deleteField = new DeleteField();

            gSteps.Columns.Add( deleteField );

            deleteField.Click += DeleteStep_Click;
        }

        /// <summary>
        /// Gets the step statuses.
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, string> GetStepStatusesHtml()
        {
            if ( _stepStatusesHtml != null )
            {
                return _stepStatusesHtml;
            }

            if ( _stepType == null )
            {
                return new Dictionary<int, string>();
            }

            var rockContext = new RockContext();
            var stepStatusService = new StepStatusService( rockContext );

            _stepStatusesHtml = stepStatusService.Queryable()
                .AsNoTracking()
                .Where( ss => ss.StepProgram.StepTypes.Any( st => st.Id == _stepType.Id ) )
                .ToDictionary(
                    ss => ss.Id,
                    ss =>
                        "<span class='label label-default' style='background-color: " +
                        ss.StatusColorOrDefault +
                        ";color:#fff;'>" +
                        ss.Name +
                        "</span>" );

            return _stepStatusesHtml;
        }
        private Dictionary<int, string> _stepStatusesHtml = null;

        /// <summary>
        /// Binds the group members grid.
        /// </summary>
        protected void BindParticipantsGrid( bool isExporting = false, bool isCommunication = false )
        {
            if ( _stepType == null )
            {
                pnlSteps.Visible = false;
                return;
            }

            pnlSteps.Visible = true;
            rFilter.Visible = true;
            gSteps.Visible = true;

            var campusCount = CampusCache.All( false ).Count;
            if ( campusCount <= 1 )
            {
                var campusColumn = gSteps.ColumnsOfType<RockBoundField>().Where( a => a.DataField == "CampusName" ).FirstOrDefault();
                if ( campusColumn != null )
                {
                    campusColumn.Visible = false;
                }
            }

            lHeading.Text = string.Format( "{0} Steps", _stepType.Name );

            _fullNameField = gSteps.ColumnsOfType<RockLiteralField>().Where( a => a.ID == "lExportFullName" ).FirstOrDefault();
            _nameWithHtmlField = gSteps.ColumnsOfType<RockLiteralField>().Where( a => a.ID == "lNameWithHtml" ).FirstOrDefault();

            var dataContext = GetDataContext();

            var stepService = new StepService( dataContext );

            var qry = stepService.Queryable()
                .Include( x => x.StepStatus )
                .Include( x => x.PersonAlias.Person )
                .AsNoTracking()
                .Where( x => x.StepTypeId == _stepType.Id );

            // Filter by First Name
            string firstName = tbFirstName.Text;
            if ( !string.IsNullOrWhiteSpace( firstName ) )
            {
                qry = qry.Where( m =>
                    m.PersonAlias.Person.FirstName.StartsWith( firstName ) ||
                    m.PersonAlias.Person.NickName.StartsWith( firstName ) );
            }

            // Filter by Last Name
            string lastName = tbLastName.Text;
            if ( !string.IsNullOrWhiteSpace( lastName ) )
            {
                qry = qry.Where( m => m.PersonAlias.Person.LastName.StartsWith( lastName ) );
            }

            // Filter by Step Status
            var validStatusIds = _stepType.StepProgram.StepStatuses.Select( r => r.Id ).ToList();

            var statusIds = new List<int>();

            foreach ( var statusId in cblStepStatus.SelectedValues.AsIntegerList() )
            {
                if ( validStatusIds.Contains( statusId ) )
                {
                    statusIds.Add( statusId );
                }
            }

            if ( statusIds.Any() )
            {
                qry = qry.Where( m => statusIds.Contains( m.StepStatusId ?? 0 ) );
            }

            // Filter By Start Date
            var startDateRange = DateRangePicker.CalculateDateRangeFromDelimitedValues( drpDateStarted.DelimitedValues );

            if ( startDateRange.Start.HasValue )
            {
                qry = qry.Where( m =>
                    m.StartDateTime.HasValue &&
                    m.StartDateTime.Value >= startDateRange.Start.Value );
            }

            if ( startDateRange.End.HasValue )
            {
                var exclusiveEndDate = startDateRange.End.Value.Date.AddDays( 1 );
                qry = qry.Where( m =>
                    m.StartDateTime.HasValue &&
                    m.StartDateTime.Value < exclusiveEndDate );
            }

            // Filter by Date Completed
            var completedDateRange = DateRangePicker.CalculateDateRangeFromDelimitedValues( drpDateCompleted.DelimitedValues );

            if ( completedDateRange.Start.HasValue )
            {
                qry = qry.Where( m =>
                    m.CompletedDateTime.HasValue &&
                    m.CompletedDateTime.Value >= completedDateRange.Start.Value );
            }

            if ( completedDateRange.End.HasValue )
            {
                var exclusiveEndDate = completedDateRange.End.Value.Date.AddDays( 1 );
                qry = qry.Where( m =>
                    m.CompletedDateTime.HasValue &&
                    m.CompletedDateTime.Value < exclusiveEndDate );
            }

            // Filter by Note
            string note = tbNote.Text;
            if ( !string.IsNullOrWhiteSpace( note ) )
            {
                qry = qry.Where( m => m.Note.Contains( note ) );
            }

            var campusContext = GetCampusContextOrNull();
            var campusId = campusContext == null ? cpCampusFilter.SelectedCampusId : campusContext.Id;
            if ( campusId != null )
            {
                qry = qry.Where( m => m.CampusId == campusId );
            }

            // Filter query by any configured attribute filters
            if ( AvailableAttributes != null && AvailableAttributes.Any() )
            {
                foreach ( var attribute in AvailableAttributes )
                {
                    var filterControl = phAttributeFilters.FindControl( "filter_" + attribute.Id.ToString() );
                    qry = attribute.FieldType.Field.ApplyAttributeQueryFilter( qry, filterControl, attribute, stepService, Rock.Reporting.FilterMode.SimpleFilter );
                }
            }

            // Apply Grid Sort
            var sortProperty = gSteps.SortProperty;

            if ( sortProperty != null )
            {
                qry = qry.Sort( sortProperty );
            }
            else
            {
                qry = qry.OrderBy( a => a.PersonAlias.Person.LastName ).ThenBy( a => a.PersonAlias.Person.FirstName );
            }

            var stepIdQuery = qry.Select( m => m.Id );

            _inactiveStatus = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE );

            gSteps.EntityTypeId = new Step().TypeId;

            _stepStatusField = gSteps.ColumnsOfType<RockLiteralField>().FirstOrDefault( a => a.ID == lStepStatusHtml.ID );

            _stepDateStartedField = gSteps.ColumnsOfType<RockBoundField>().FirstOrDefault( a => a.HeaderText == "Date Started" );

            if ( _stepDateStartedField != null )
            {
                _stepDateStartedField.Visible = _stepType.HasEndDate;
            }

            _showNoteColumn = GetAttributeValue( AttributeKey.ShowNoteColumn ).AsBoolean();

            gSteps.ColumnsOfType<RockBoundField>().First( a => a.DataField == "Note" ).Visible = _showNoteColumn;

            var hasDuration = _stepType.HasEndDate;

            var qrySteps = qry.Select( x => new StepParticipantListViewModel
            {
                Id = x.Id,
                PersonId = x.PersonAlias.PersonId,
                LastName = x.PersonAlias.Person.LastName,
                NickName = x.PersonAlias.Person.NickName,
                StartedDateTime = x.StartDateTime,
                CompletedDateTime = x.CompletedDateTime,
                StepStatusId = x.StepStatusId,
                IsCompleted = ( x.StepStatus == null ? false : x.StepStatus.IsCompleteStatus ),
                Note = x.Note,
                CampusName = x.Campus == null ? string.Empty : x.Campus.Name,
                Person = x.PersonAlias.Person
            } );

            // Add the Step data models to the grid object list to allow custom attribute values to be read.
            if ( this.AvailableAttributes != null
                 && this.AvailableAttributes.Any() )
            {
                gSteps.ObjectList = qry.ToDictionary( k => k.Id.ToString(), v => ( object ) v );
            }

            gSteps.SetLinqDataSource( qrySteps );

            gSteps.DataBind();
        }

        /// <summary>
        /// Get the step status names.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="listControl">The list control.</param>
        /// <returns></returns>
        private string GetStepStatusNames( string values, System.Web.UI.WebControls.CheckBoxList checkBoxList )
        {
            var resolvedValues = new List<string>();

            foreach ( string value in values.Split( ';' ) )
            {
                var item = checkBoxList.Items.FindByValue( value );
                if ( item != null )
                {
                    resolvedValues.Add( item.Text );
                }
            }

            return resolvedValues.AsDelimited( ", " );
        }

        /// <summary>
        /// Initializes any scripts to the page.
        /// </summary>
        private void InitializeScripts()
        {
            /// add lazyload js so that person-link-popover javascript works (see StepList.ascx)
            RockPage.AddScriptLink( "~/Scripts/jquery.lazyload.min.js" );
        }

        #endregion

        #region ISecondaryBlock

        /// <summary>
        /// Sets the visible.
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlContent.Visible = visible;
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// A view-model that represents a single row on the Steps Participant grid.
        /// </summary>
        public class StepParticipantListViewModel : RockDynamic
        {
            public int Id { get; set; }
            public int PersonId { get; set; }
            public string LastName { get; set; }
            public string NickName { get; set; }
            public string FullName { get; set; }
            public DateTime? StartedDateTime { get; set; }
            public DateTime? CompletedDateTime { get; set; }
            public int? StepStatusId { get; set; }
            public bool IsCompleted { get; set; }
            public string Note { get; set; }

            public Person Person { get; set; }
            public string CampusName { get; set; }
        }

        #endregion
    }
}