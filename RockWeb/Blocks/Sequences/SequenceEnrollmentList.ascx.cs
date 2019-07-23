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

namespace RockWeb.Blocks.Sequences
{
    [DisplayName( "Sequence Enrollment List" )]
    [Category( "Sequences" )]
    [Description( "Lists all the people enrolled in a sequence." )]

    #region Block Attributes

    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage,
        Order = 1 )]

    [LinkedPage(
        "Person Profile Page",
        Description = "Page used for viewing a person's profile. If set, a view profile button will show for each enrollment.",
        Key = AttributeKey.ProfilePage,
        IsRequired = false,
        Order = 2 )]

    #endregion

    public partial class SequenceEnrollmentList : RockBlock, ISecondaryBlock, ICustomGridColumns
    {
        #region Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        protected static class AttributeKey
        {
            /// <summary>
            /// The detail page attribute key
            /// </summary>
            public const string DetailPage = "DetailPage";

            /// <summary>
            /// The person profile page attribute key
            /// </summary>
            public const string ProfilePage = "PersonProfilePage";
        }

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        protected static class PageParameterKey
        {
            /// <summary>
            /// The sequence id page parameter key
            /// </summary>
            public const string SequenceId = "SequenceId";

            /// <summary>
            /// The sequence enrollment id page parameter key
            /// </summary>
            public const string SequenceEnrollmentId = "SequenceEnrollmentId";
        }

        /// <summary>
        /// Keys to use for filters
        /// </summary>
        protected static class FilterKey
        {
            /// <summary>
            /// The first name filter key
            /// </summary>
            public const string FirstName = "FirstName";

            /// <summary>
            /// The last name filter key
            /// </summary>
            public const string LastName = "LastName";

            /// <summary>
            /// The enrollment date filter key
            /// </summary>
            public const string EnrollmentDate = "EnrollmentDate";
        }

        #endregion Keys

        #region Private Variables

        private DefinedValueCache _inactiveStatus = null;
        private bool _canView = false;

        // Cache these fields since they could get called many times in GridRowDataBound
        private DeleteField _deleteField = null;
        private int? _deleteFieldColumnIndex = null;

        private RockLiteralField _fullNameField = null;
        private RockLiteralField _nameWithHtmlField = null;
        private RockLiteralField _lBiStateGraph = null;
        private RockBoundField _enrollmentDateField = null;

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
                    BindEnrollmentGrid();
                }
            }
        }

        #endregion

        #region Grid

        private readonly string _photoFormat = "<div class=\"photo-icon photo-round photo-round-xs pull-left margin-r-sm js-person-popover\" personid=\"{0}\" data-original=\"{1}&w=50\" style=\"background-image: url( '{2}' ); background-size: cover; background-repeat: no-repeat;\"></div>";

        /// <summary>
        /// Handles the RowDataBound event of the gEnrollments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gEnrollments_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType != DataControlRowType.DataRow )
            {
                return;
            }

            var enrollmentViewModel = e.Row.DataItem as EnrollmentViewModel;

            if ( enrollmentViewModel == null )
            {
                return;
            }

            var enrollmentId = enrollmentViewModel.Id;
            var person = enrollmentViewModel.Person;

            var lFullName = e.Row.FindControl( _fullNameField.ID ) as Literal;
            if ( lFullName != null )
            {
                lFullName.Text = person.FullNameReversed;
            }

            var lNameWithHtml = e.Row.FindControl( _nameWithHtmlField.ID ) as Literal;
            if ( lNameWithHtml != null )
            {
                var sbNameHtml = new StringBuilder();

                sbNameHtml.AppendFormat( _photoFormat, person.Id, person.PhotoUrl, ResolveUrl( "~/Assets/Images/person-no-photo-unknown.svg" ) );
                sbNameHtml.Append( person.FullName );

                if ( person.TopSignalColor.IsNotNullOrWhiteSpace() )
                {
                    sbNameHtml.Append( person.GetSignalMarkup() );
                }

                lNameWithHtml.Text = sbNameHtml.ToString();
            }

            var lBiStateGraph = e.Row.FindControl( _lBiStateGraph.ID ) as Literal;

            if ( lBiStateGraph != null )
            {
                var sequence = GetSequence();

                if ( sequence != null )
                {
                    var mostRecentBits = SequenceService.GetMostRecentBits( enrollmentViewModel.SequenceEnrollmentMap, sequence.StartDate, sequence.OccurrenceFrequency );
                    var stringBuilder = new StringBuilder();

                    foreach ( var bit in mostRecentBits )
                    {
                        stringBuilder.Insert( 0, string.Format( @"<li><span style=""height: {0}%""></span></li>", ( bit ? "100" : "5" ) ) );
                    }

                    lBiStateGraph.Text = string.Format( @"
                        <div class=""chart-container"">
                            <ul class=""attendance-chart attendance-chart-sm"">{0}</ul>
                        </div>", stringBuilder );
                }
            }

            if ( person.IsDeceased )
            {
                e.Row.AddCssClass( "is-deceased" );
            }

            if ( _deleteField != null && _deleteField.Visible )
            {
                LinkButton deleteButton = null;

                if ( !_deleteFieldColumnIndex.HasValue )
                {
                    _deleteFieldColumnIndex = gEnrollments.GetColumnIndex( gEnrollments.Columns.OfType<DeleteField>().First() );
                }

                if ( _deleteFieldColumnIndex.HasValue && _deleteFieldColumnIndex > -1 )
                {
                    deleteButton = e.Row.Cells[_deleteFieldColumnIndex.Value].ControlsOfTypeRecursive<LinkButton>().FirstOrDefault();
                }
            }

            if ( _inactiveStatus != null && person.RecordStatusValueId == _inactiveStatus.Id )
            {
                e.Row.AddCssClass( "is-inactive-person" );
            }
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( FilterKey.FirstName, "First Name", tbFirstName.Text );
            rFilter.SaveUserPreference( FilterKey.LastName, "Last Name", tbLastName.Text );
            rFilter.SaveUserPreference( FilterKey.EnrollmentDate, "Enrollment Date", drpEnrollmentDate.DelimitedValues );

            BindEnrollmentGrid();
        }

        /// <summary>
        /// Rs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case FilterKey.FirstName:
                case FilterKey.LastName:
                    return;
                case FilterKey.EnrollmentDate:
                    e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
                    break;
                default:
                    e.Value = string.Empty;
                    break;
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

        /// <summary>
        /// Handles the Click event of the delete button in the grid
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs" /> instance containing the event data.</param>
        protected void DeleteEnrollment_Click( object sender, RowEventArgs e )
        {
            var rockContext = GetRockContext();
            var service = GetSequenceEnrollmentService();
            var enrollment = service.Get( e.RowKeyId );

            if ( enrollment != null )
            {
                var errorMessage = string.Empty;
                if ( !service.CanDelete( enrollment, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                service.Delete( enrollment );
                rockContext.SaveChanges();
            }

            BindEnrollmentGrid();
        }

        /// <summary>
        /// Handles the AddClick event of the gEnrollments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gEnrollments_AddClick( object sender, EventArgs e )
        {
            var sequence = GetSequence();
            NavigateToLinkedPage( "DetailPage", PageParameterKey.SequenceEnrollmentId, 0, PageParameterKey.SequenceId, sequence.Id );
        }

        /// <summary>
        /// Handles the Edit event of the gEnrollments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gEnrollments_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", PageParameterKey.SequenceEnrollmentId, e.RowKeyId );
        }

        /// <summary>
        /// Handles the GridRebind event of the gEnrollments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs" /> instance containing the event data.</param>
        protected void gEnrollments_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindEnrollmentGrid( e.IsExporting, e.IsCommunication );
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Retrieve a singleton data context for data operations in this block.
        /// </summary>
        /// <returns></returns>
        private RockContext GetRockContext()
        {
            if ( _rockContext == null )
            {
                _rockContext = new RockContext();
            }

            return _rockContext;
        }
        private RockContext _rockContext = null;

        /// <summary>
        /// Retrieve a singleton enrollment service for data operations in this block.
        /// </summary>
        /// <returns></returns>
        private SequenceEnrollmentService GetSequenceEnrollmentService()
        {
            if ( _sequenceEnrollmentService == null )
            {
                var rockContext = GetRockContext();
                _sequenceEnrollmentService = new SequenceEnrollmentService( rockContext );
            }

            return _sequenceEnrollmentService;
        }
        private SequenceEnrollmentService _sequenceEnrollmentService = null;

        /// <summary>
        /// Retrieve a singleton sequence service for data operations in this block.
        /// </summary>
        /// <returns></returns>
        private SequenceService GetSequenceService()
        {
            if ( _sequenceService == null )
            {
                var rockContext = GetRockContext();
                _sequenceService = new SequenceService( rockContext );
            }

            return _sequenceService;
        }
        private SequenceService _sequenceService = null;

        /// <summary>
        /// Get the sequence
        /// </summary>
        /// <returns></returns>
        private Sequence GetSequence()
        {
            if ( _sequence == null )
            {
                var sequenceId = PageParameter( PageParameterKey.SequenceId ).AsIntegerOrNull();

                if ( sequenceId.HasValue )
                {
                    _sequence = GetSequenceService().Get( sequenceId.Value );
                }
            }

            return _sequence;
        }
        private Sequence _sequence = null;

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
            var sequence = GetSequence();

            if ( sequence != null && sequence.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
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
                var sequence = GetSequence();

                if ( sequence != null )
                {
                    rFilter.UserPreferenceKeyPrefix = string.Format( "{0}-", sequence.Id );
                }

                BindFilter();
            }

            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
            rFilter.DisplayFilterValue += rFilter_DisplayFilterValue;
        }

        private void InitializeGrid()
        {
            gEnrollments.DataKeyNames = new string[] { "Id" };
            gEnrollments.PersonIdField = "Person Id";
            gEnrollments.Actions.AddClick += gEnrollments_AddClick;
            gEnrollments.GridRebind += gEnrollments_GridRebind;
            gEnrollments.RowItemText = "Sequence Enrollment";
            gEnrollments.ExportSource = ExcelExportSource.DataSource;
            gEnrollments.ShowConfirmDeleteDialog = true;

            var sequence = GetSequence();
            var canEditBlock =
                IsUserAuthorized( Authorization.EDIT ) ||
                sequence.IsAuthorized( Authorization.EDIT, CurrentPerson ) ||
                sequence.IsAuthorized( Authorization.MANAGE_MEMBERS, CurrentPerson );

            gEnrollments.Actions.ShowAdd = canEditBlock;
            gEnrollments.IsDeleteEnabled = canEditBlock;

            if ( sequence != null )
            {
                gEnrollments.ExportFilename = sequence.Name;
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindFilter();
            IntializeRowButtons();
            BindEnrollmentGrid();
        }

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            tbFirstName.Text = rFilter.GetUserPreference( FilterKey.FirstName );
            tbLastName.Text = rFilter.GetUserPreference( FilterKey.LastName );
            drpEnrollmentDate.DelimitedValues = rFilter.GetUserPreference( FilterKey.EnrollmentDate );
        }

        /// <summary>
        /// Initialize the row buttons
        /// </summary>
        private void IntializeRowButtons()
        {
            RemoveRowButtons();
            AddRowButtons();
        }

        /// <summary>
        /// Remove the row buttons
        /// </summary>
        private void RemoveRowButtons()
        {
            // Remove added button columns
            DataControlField buttonColumn = gEnrollments.Columns.OfType<DeleteField>().FirstOrDefault( c => c.ItemStyle.CssClass == "grid-columncommand" );
            if ( buttonColumn != null )
            {
                gEnrollments.Columns.Remove( buttonColumn );
            }

            buttonColumn = gEnrollments.Columns.OfType<HyperLinkField>().FirstOrDefault( c => c.ItemStyle.CssClass == "grid-columncommand" );
            if ( buttonColumn != null )
            {
                gEnrollments.Columns.Remove( buttonColumn );
            }

            buttonColumn = gEnrollments.Columns.OfType<LinkButtonField>().FirstOrDefault( c => c.ItemStyle.CssClass == "grid-columncommand" );
            if ( buttonColumn != null )
            {
                gEnrollments.Columns.Remove( buttonColumn );
            }
        }

        /// <summary>
        /// Add the row buttons
        /// </summary>
        private void AddRowButtons()
        {
            // Add Link to Profile Page Column
            if ( !string.IsNullOrEmpty( GetAttributeValue( "PersonProfilePage" ) ) )
            {
                var column = CreatePersonProfileLinkColumn( "PersonId" );

                gEnrollments.Columns.Add( column );
            }

            // Add delete column
            _deleteField = new DeleteField();
            _deleteField.Click += DeleteEnrollment_Click;

            gEnrollments.Columns.Add( _deleteField );
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
        /// Binds the enrollment grid.
        /// </summary>
        protected void BindEnrollmentGrid( bool isExporting = false, bool isCommunication = false )
        {
            var sequence = GetSequence();

            if ( sequence == null )
            {
                pnlEnrollments.Visible = false;
                return;
            }

            pnlEnrollments.Visible = true;
            rFilter.Visible = true;
            gEnrollments.Visible = true;

            lHeading.Text = string.Format( "{0} Enrollments", sequence.Name );

            _fullNameField = gEnrollments.ColumnsOfType<RockLiteralField>().Where( a => a.ID == "lExportFullName" ).FirstOrDefault();
            _nameWithHtmlField = gEnrollments.ColumnsOfType<RockLiteralField>().Where( a => a.ID == "lNameWithHtml" ).FirstOrDefault();
            _lBiStateGraph = gEnrollments.ColumnsOfType<RockLiteralField>().Where( a => a.ID == "lBiStateGraph" ).FirstOrDefault();

            var enrollmentService = GetSequenceEnrollmentService();

            var query = enrollmentService.Queryable()
                .Include( se => se.PersonAlias.Person )
                .AsNoTracking()
                .Where( se => se.SequenceId == sequence.Id );

            // Filter by First Name
            var firstName = tbFirstName.Text;
            if ( !string.IsNullOrWhiteSpace( firstName ) )
            {
                query = query.Where( se =>
                    se.PersonAlias.Person.FirstName.StartsWith( firstName ) ||
                    se.PersonAlias.Person.NickName.StartsWith( firstName ) );
            }

            // Filter by Last Name
            var lastName = tbLastName.Text;
            if ( !string.IsNullOrWhiteSpace( lastName ) )
            {
                query = query.Where( se => se.PersonAlias.Person.LastName.StartsWith( lastName ) );
            }

            // Filter by Enrollment Date
            var enrollmentDateRange = DateRangePicker.CalculateDateRangeFromDelimitedValues( drpEnrollmentDate.DelimitedValues );

            if ( enrollmentDateRange.Start.HasValue )
            {
                query = query.Where( se => se.EnrollmentDate >= enrollmentDateRange.Start.Value );
            }

            if ( enrollmentDateRange.End.HasValue )
            {
                query = query.Where( se => se.EnrollmentDate <= enrollmentDateRange.End.Value );
            }

            // Sort the grid
            gEnrollments.EntityTypeId = new SequenceEnrollment().TypeId;
            var sortProperty = gEnrollments.SortProperty;

            if ( sortProperty != null )
            {
                query = query.Sort( sortProperty );
            }
            else
            {
                query = query.OrderBy( a => a.PersonAlias.Person.LastName ).ThenBy( a => a.PersonAlias.Person.FirstName );
            }

            var viewModelQuery = query.Select( se => new EnrollmentViewModel
            {
                Id = se.Id,
                PersonId = se.PersonAlias.PersonId,
                LastName = se.PersonAlias.Person.LastName,
                NickName = se.PersonAlias.Person.NickName,
                EnrollmentDate = se.EnrollmentDate,
                Person = se.PersonAlias.Person,
                EngagementCount = se.EngagementCount,
                CurrentStreakCount = se.CurrentStreakCount,
                LongestStreakCount = se.LongestStreakCount,
                SequenceEnrollmentMap = se.EngagementMap
            } ).DistinctBy( vm => vm.PersonId ).AsQueryable();

            gEnrollments.SetLinqDataSource( viewModelQuery );
            gEnrollments.DataBind();
        }

        /// <summary>
        /// Add JavaScript to the page
        /// </summary>
        private void InitializeScripts()
        {
            /// add lazyload so that person-link-popover javascript works
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

        #region View Models

        /// <summary>
        /// Represents an enrollment for a row in the grid
        /// </summary>
        public class EnrollmentViewModel
        {
            public int Id { get; set; }
            public int PersonId { get; set; }
            public string LastName { get; set; }
            public string NickName { get; set; }
            public string FullName { get; set; }
            public DateTime EnrollmentDate { get; set; }
            public Person Person { get; set; }
            public int EngagementCount { get; set; }
            public int CurrentStreakCount { get; set; }
            public int LongestStreakCount { get; set; }
            public byte[] SequenceEnrollmentMap { get; set; }
        }

        #endregion View Models
    }
}