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

namespace RockWeb.Blocks.Streaks
{
    [DisplayName( "Streak List" )]
    [Category( "Streaks" )]
    [Description( "Lists all the people enrolled in a streak type." )]

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

    [Rock.SystemGuid.BlockTypeGuid( "46A5143E-8DE7-4E3D-96B3-674E8FD12949" )]
    public partial class StreakList : RockBlock, ISecondaryBlock, ICustomGridColumns
    {
        #region Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
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
        private static class PageParameterKey
        {
            /// <summary>
            /// The streak type id page parameter key
            /// </summary>
            public const string StreakTypeId = "StreakTypeId";

            /// <summary>
            /// The streak id page parameter key
            /// </summary>
            public const string StreakId = "StreakId";
        }

        /// <summary>
        /// Keys to use for filters
        /// </summary>
        private static class FilterKey
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

        private bool _canView = false;

        // Cache these fields since they could get called many times in GridRowDataBound
        private RockLiteralField _fullNameField = null;
        private RockLiteralField _nameWithHtmlField = null;
        private RockLiteralField _lBiStateGraph = null;

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
                var streakTypeService = GetStreakTypeService();
                var streakType = GetStreakType();

                if ( streakType != null )
                {
                    var errorMessage = string.Empty;
                    var occurrenceEngagements = streakTypeService.GetRecentEngagementBits( streakType.Id, person.Id, 24, out errorMessage ) ?? new OccurrenceEngagement[0];
                    var stringBuilder = new StringBuilder();
                    foreach ( var occurrence in occurrenceEngagements )
                    {
                        var hasEngagement = occurrence != null && occurrence.HasEngagement;
                        var hasExclusion = occurrence != null && occurrence.HasExclusion;
                        var title = occurrence != null ? occurrence.DateTime.ToShortDateString() : string.Empty;
                        stringBuilder.Insert( 0, string.Format( @"<li class=""binary-state-graph-bit {2} {3}"" title=""{0}""><span style=""height: {1}%""></span></li>",
                            title, // 0
                            hasEngagement ? "100" : "5", // 1
                            hasEngagement ? "has-engagement" : string.Empty, // 2
                            hasExclusion ? "has-exclusion" : string.Empty ) ); // 3
                    }

                    lBiStateGraph.Text = string.Format( @"
                        <div class=""chart-container"">
                            <ul class=""trend-chart trend-chart-sm text-info"">{0}</ul>
                        </div>", stringBuilder );
                }
            }

            if ( person.IsDeceased )
            {
                e.Row.AddCssClass( "is-deceased" );
            }
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SetFilterPreference( FilterKey.FirstName, "First Name", tbFirstName.Text );
            rFilter.SetFilterPreference( FilterKey.LastName, "Last Name", tbLastName.Text );
            rFilter.SetFilterPreference( FilterKey.EnrollmentDate, "Enrollment Date", drpEnrollmentDate.DelimitedValues );

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
            rFilter.DeleteFilterPreferences();
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
            var streakService = GetStreakService();
            var enrollment = streakService.Get( e.RowKeyId );

            if ( enrollment != null )
            {
                streakService.Delete( enrollment );
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
            var streakType = GetStreakType();
            NavigateToLinkedPage( "DetailPage", PageParameterKey.StreakId, 0, PageParameterKey.StreakTypeId, streakType.Id );
        }

        /// <summary>
        /// Handles the Edit event of the gEnrollments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gEnrollments_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", PageParameterKey.StreakId, e.RowKeyId );
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
        private StreakService GetStreakService()
        {
            if ( _streakService == null )
            {
                var rockContext = GetRockContext();
                _streakService = new StreakService( rockContext );
            }

            return _streakService;
        }
        private StreakService _streakService = null;

        /// <summary>
        /// Retrieve a singleton streak type service for data operations in this block.
        /// </summary>
        /// <returns></returns>
        private StreakTypeService GetStreakTypeService()
        {
            if ( _streakTypeService == null )
            {
                var rockContext = GetRockContext();
                _streakTypeService = new StreakTypeService( rockContext );
            }

            return _streakTypeService;
        }
        private StreakTypeService _streakTypeService = null;

        /// <summary>
        /// Get the streak type
        /// </summary>
        /// <returns></returns>
        private StreakType GetStreakType()
        {
            if ( _streakType == null )
            {
                var streakTypeId = PageParameter( PageParameterKey.StreakTypeId ).AsIntegerOrNull();

                if ( streakTypeId.HasValue )
                {
                    _streakType = GetStreakTypeService().Get( streakTypeId.Value );
                }
            }

            return _streakType;
        }
        private StreakType _streakType = null;

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
            var streakType = GetStreakType();

            if ( streakType != null && streakType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
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
                var streakType = GetStreakType();

                if ( streakType != null )
                {
                    rFilter.PreferenceKeyPrefix = string.Format( "{0}-", streakType.Id );
                }

                BindFilter();
            }

            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
            rFilter.DisplayFilterValue += rFilter_DisplayFilterValue;
        }

        private void InitializeGrid()
        {
            gEnrollments.DataKeyNames = new string[] { "Id" };
            gEnrollments.PersonIdField = "PersonId";
            gEnrollments.Actions.AddClick += gEnrollments_AddClick;
            gEnrollments.GridRebind += gEnrollments_GridRebind;
            gEnrollments.RowItemText = "Streak";
            gEnrollments.ExportSource = ExcelExportSource.DataSource;
            gEnrollments.ShowConfirmDeleteDialog = true;

            var streakType = GetStreakType();
            var canEditBlock =
                IsUserAuthorized( Authorization.EDIT ) ||
                streakType.IsAuthorized( Authorization.EDIT, CurrentPerson ) ||
                streakType.IsAuthorized( Authorization.MANAGE_MEMBERS, CurrentPerson );

            gEnrollments.Actions.ShowAdd = canEditBlock;
            gEnrollments.IsDeleteEnabled = canEditBlock;

            if ( streakType != null )
            {
                gEnrollments.ExportFilename = streakType.Name;
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
            BindEnrollmentGrid();
        }

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            tbFirstName.Text = rFilter.GetFilterPreference( FilterKey.FirstName );
            tbLastName.Text = rFilter.GetFilterPreference( FilterKey.LastName );
            drpEnrollmentDate.DelimitedValues = rFilter.GetFilterPreference( FilterKey.EnrollmentDate );
        }

        /// <summary>
        /// Binds the enrollment grid.
        /// </summary>
        protected void BindEnrollmentGrid( bool isExporting = false, bool isCommunication = false )
        {
            var streakType = GetStreakType();

            if ( streakType == null )
            {
                pnlEnrollments.Visible = false;
                return;
            }

            pnlEnrollments.Visible = true;
            rFilter.Visible = true;
            gEnrollments.Visible = true;

            lHeading.Text = string.Format( "{0} Enrollments", streakType.Name );

            _fullNameField = gEnrollments.ColumnsOfType<RockLiteralField>().Where( a => a.ID == "lExportFullName" ).FirstOrDefault();
            _nameWithHtmlField = gEnrollments.ColumnsOfType<RockLiteralField>().Where( a => a.ID == "lNameWithHtml" ).FirstOrDefault();
            _lBiStateGraph = gEnrollments.ColumnsOfType<RockLiteralField>().Where( a => a.ID == "lBiStateGraph" ).FirstOrDefault();

            var streakService = GetStreakService();

            var query = streakService.Queryable()
                .Include( se => se.PersonAlias.Person )
                .AsNoTracking()
                .Where( se => se.StreakTypeId == streakType.Id );

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
            gEnrollments.EntityTypeId = new Streak().TypeId;
            var sortProperty = gEnrollments.SortProperty;

            if ( sortProperty != null )
            {
                query = query.Sort( sortProperty );
            }
            else
            {
                query = query.OrderBy( a => a.PersonAlias.Person.LastName ).ThenBy( a => a.PersonAlias.Person.NickName );
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
                EngagementMap = se.EngagementMap
            } ).AsQueryable();

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
        public class EnrollmentViewModel : RockDynamic
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
            public byte[] EngagementMap { get; set; }
        }

        #endregion View Models
    }
}