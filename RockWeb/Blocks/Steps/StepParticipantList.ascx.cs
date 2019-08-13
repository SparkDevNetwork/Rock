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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
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

    public partial class StepParticipantList : RockBlock, ISecondaryBlock, ICustomGridColumns
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        protected static class AttributeKey
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
        protected static class PageParameterKey
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
        protected static class FilterKey
        {
            public const string FirstName = "FirstName";
            public const string LastName = "LastName";
            public const string StepStatus = "StepStatus";
            public const string DateStarted = "DateStarted";
            public const string DateCompleted = "DateCompleted";
            public const string Note = "Note";
        }

        #endregion Filter Keys

        #region Private Variables

        private RockContext _dataContext;
        private DefinedValueCache _inactiveStatus = null;
        private StepType _stepType = null;
        private bool _canView = false;

        // Cache these fields since they could get called many times in GridRowDataBound
        private DeleteField _deleteField = null;
        private int? _deleteFieldColumnIndex = null;

        private RockLiteralField _fullNameField = null;
        private RockLiteralField _nameWithHtmlField = null;
        private RockLiteralField _stepStatusField = null;
        private RockBoundField _stepDateStartedField = null;

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

            IntializeRowButtons();

            InitializeSettingsNotification( upMain );
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
                pnlContent.Visible = _canView;
                if ( _canView )
                {
                    BindFilter();
                    BindParticipantsGrid();
                }
            }
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

            var lStepStatusHtml = e.Row.FindControl( _stepStatusField.ID ) as Literal;

            if ( lStepStatusHtml != null )
            {
                if ( step.IsCompleted )
                {
                    lStepStatusHtml.Text = string.Format( "<div class='badge badge-success'>{0}</div>", step.StepStatusName );
                }
                else
                {
                    lStepStatusHtml.Text = string.Format( "<div class='badge badge-info'>{0}</div>", step.StepStatusName );
                }
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

            if ( _deleteField != null && _deleteField.Visible )
            {
                LinkButton deleteButton = null;

                if ( !_deleteFieldColumnIndex.HasValue )
                {
                    _deleteFieldColumnIndex = gSteps.GetColumnIndex( gSteps.Columns.OfType<DeleteField>().First() );
                }

                if ( _deleteFieldColumnIndex.HasValue && _deleteFieldColumnIndex > -1 )
                {
                    deleteButton = e.Row.Cells[_deleteFieldColumnIndex.Value].ControlsOfTypeRecursive<LinkButton>().FirstOrDefault();
                }
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
            rFilter.SaveUserPreference( FilterKey.FirstName, "First Name", tbFirstName.Text );
            rFilter.SaveUserPreference( FilterKey.LastName, "Last Name", tbLastName.Text );
            rFilter.SaveUserPreference( FilterKey.StepStatus, "Status", cblStepStatus.SelectedValues.AsDelimited( ";" ) );
            rFilter.SaveUserPreference( FilterKey.DateStarted, "Date Started", drpDateStarted.DelimitedValues );
            rFilter.SaveUserPreference( FilterKey.DateCompleted, "Date Completed", drpDateCompleted.DelimitedValues );
            rFilter.SaveUserPreference( FilterKey.Note, "Note", tbNote.Text );

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
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rFilter_ClearFilterClick( object sender, EventArgs e )
        {
            rFilter.DeleteUserPreferences();
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
                    rFilter.UserPreferenceKeyPrefix = string.Format( "{0}-", _stepType.Id );
                }

                BindFilter();
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
            gSteps.PersonIdField = "Person Id";
            gSteps.GetRecipientMergeFields += gSteps_GetRecipientMergeFields;
            gSteps.Actions.AddClick += gSteps_AddClick;
            gSteps.GridRebind += gSteps_GridRebind;
            gSteps.RowItemText = "Step Participant";
            gSteps.ExportSource = ExcelExportSource.DataSource;
            gSteps.ShowConfirmDeleteDialog = true;

            // Allow Edit if authorized to edit the block or the Step Type.
            bool canEditBlock = IsUserAuthorized( Authorization.EDIT ) || _stepType.IsAuthorized( Authorization.EDIT, CurrentPerson ) || _stepType.IsAuthorized( Authorization.MANAGE_MEMBERS, CurrentPerson );

            gSteps.Actions.ShowAdd = canEditBlock;
            gSteps.IsDeleteEnabled = canEditBlock;

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

            IntializeRowButtons();

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

            tbFirstName.Text = rFilter.GetUserPreference( FilterKey.FirstName );
            tbLastName.Text = rFilter.GetUserPreference( FilterKey.LastName );
            tbNote.Text = rFilter.GetUserPreference( FilterKey.Note );

            string statusValue = rFilter.GetUserPreference( FilterKey.StepStatus );
            if ( !string.IsNullOrWhiteSpace( statusValue ) )
            {
                cblStepStatus.SetValues( statusValue.Split( ';' ).ToList() );
            }

            drpDateStarted.DelimitedValues = rFilter.GetUserPreference( FilterKey.DateStarted );
            drpDateCompleted.DelimitedValues = rFilter.GetUserPreference( FilterKey.DateCompleted );
        }

        /// <summary>
        /// Initialize the row buttons.
        /// </summary>
        private void IntializeRowButtons()
        {
            RemoveRowButtons();
            AddRowButtons();
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
        private void AddRowButtons()
        {
            // Add Link to Profile Page Column
            if ( !string.IsNullOrEmpty( GetAttributeValue( "PersonProfilePage" ) ) )
            {
                var column = CreatePersonProfileLinkColumn( "PersonId" );

                gSteps.Columns.Add( column );
            }

            // Add delete column
            _deleteField = new DeleteField();
            _deleteField.Click += DeleteStep_Click;

            gSteps.Columns.Add( _deleteField );
        }

        /// <summary>
        /// Adds the column with a link to profile page.
        /// </summary>
        private HyperLinkField CreatePersonProfileLinkColumn( string fieldName )
        {
            HyperLinkField hlPersonProfileLink = new HyperLinkField();
            hlPersonProfileLink.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
            hlPersonProfileLink.HeaderStyle.CssClass = "grid-columncommand";
            hlPersonProfileLink.ItemStyle.CssClass = "grid-columncommand";
            hlPersonProfileLink.DataNavigateUrlFields = new string[1] { fieldName };
            hlPersonProfileLink.DataNavigateUrlFormatString = LinkedPageUrl( "PersonProfilePage", new Dictionary<string, string> { { "PersonId", "###" } } ).Replace( "###", "{0}" );
            hlPersonProfileLink.DataTextFormatString = "<div class='btn btn-default btn-sm'><i class='fa fa-user'></i></div>";
            hlPersonProfileLink.DataTextField = fieldName;

            return hlPersonProfileLink;
        }

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

            lHeading.Text = string.Format( "{0} Step Participants", _stepType.Name );

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

            if ( _stepType.HasEndDate )
            {
                var startDateRange = DateRangePicker.CalculateDateRangeFromDelimitedValues( drpDateStarted.DelimitedValues );

                if ( startDateRange.Start.HasValue )
                {
                    qry = qry.Where( m =>
                        m.StartDateTime.HasValue &&
                        m.StartDateTime.Value >= startDateRange.Start.Value );
                }

                if ( startDateRange.End.HasValue )
                {
                    var end = startDateRange.End.Value.AddHours( 23 ).AddMinutes( 59 ).AddSeconds( 59 );
                    qry = qry.Where( m =>
                        m.StartDateTime.HasValue &&
                        m.StartDateTime.Value < end );
                }
            }

            // Filter by Date Completed
            var endDateRange = DateRangePicker.CalculateDateRangeFromDelimitedValues( drpDateCompleted.DelimitedValues );

            if ( endDateRange.Start.HasValue )
            {
                qry = qry.Where( m =>
                    m.EndDateTime.HasValue &&
                    m.EndDateTime.Value >= endDateRange.Start.Value );
            }

            if ( endDateRange.End.HasValue )
            {
                var end = endDateRange.End.Value.AddHours( 23 ).AddMinutes( 59 ).AddSeconds( 59 );
                qry = qry.Where( m =>
                    m.EndDateTime.HasValue &&
                    m.EndDateTime.Value < end );
            }

            // Filter by Note
            string note = tbNote.Text;
            if ( !string.IsNullOrWhiteSpace( note ) )
            {
                qry = qry.Where( m => m.Note.Contains( note ) );
            }

            _inactiveStatus = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE );

            gSteps.EntityTypeId = new Step().TypeId;

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
                StartedDateTime = x.StartDateTime ?? DateTime.MinValue,
                CompletedDateTime = x.CompletedDateTime,
                StepStatusName = ( x.StepStatus == null ? "" : x.StepStatus.Name ),
                IsCompleted = ( x.StepStatus == null ? false : x.StepStatus.IsCompleteStatus ),
                Note = x.Note,
                Person = x.PersonAlias.Person
            } );

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
        /// <seealso cref="DotLiquid.Drop" />
        public class StepParticipantListViewModel : DotLiquid.Drop
        {
            public int Id { get; set; }
            public int PersonId { get; set; }
            public string LastName { get; set; }
            public string NickName { get; set; }
            public string FullName { get; set; }
            public DateTime StartedDateTime { get; set; }
            public DateTime? CompletedDateTime { get; set; }
            public string StepStatusName { get; set; }
            public bool IsCompleted { get; set; }
            public string Note { get; set; }

            public Person Person { get; set; }
        }

        #endregion
    }
}