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
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Streaks
{
    [DisplayName( "Achievement Attempt List" )]
    [Category( "Streaks" )]
    [Description( "Lists all the people that have made an attempt at earning an achievement." )]

    [LinkedPage(
        "Detail Page",
        Description = "Page navigated to when a grid item is clicked.",
        Key = AttributeKey.DetailPage,
        IsRequired = false,
        Order = 1 )]

    public partial class AchievementAttemptList : RockBlock, ISecondaryBlock, ICustomGridColumns
    {
        #region Keys

        /// <summary>
        /// Keys to use for Attributes
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The achievements page
            /// </summary>
            public const string DetailPage = "DetailPage";
        }

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            /// <summary>
            /// The achievement type id page parameter key
            /// </summary>
            public const string StreakTypeAchievementTypeId = "StreakTypeAchievementTypeId";

            /// <summary>
            /// The streak identifier
            /// </summary>
            public const string StreakId = "StreakId";

            /// <summary>
            /// The streak achievement attempt identifier
            /// </summary>
            public const string StreakAchievementAttemptId = "StreakAchievementAttemptId";
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
            /// The attempt start date filter key
            /// </summary>
            public const string AttemptStartDateRange = "AttemptStartDateRange";

            /// <summary>
            /// The status
            /// </summary>
            public const string Status = "Status";

            /// <summary>
            /// The achievement type
            /// </summary>
            public const string AchievementType = "AchievementType";
        }

        #endregion Keys

        #region Private Variables

        // Cache these fields since they could get called many times in GridRowDataBound
        private readonly string _fullNameFieldId = "lExportFullName";
        private readonly string _nameWithHtmlFieldId = "lNameWithHtml";
        private readonly string _progressFieldId = "lProgress";

        #endregion Private Variables

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
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
                var canView = CanView();
                pnlContent.Visible = canView;

                if ( canView )
                {
                    BindFilter();
                    BindGrid();
                    BindStatusDropDown();
                }
            }
        }

        #endregion Base Control Methods

        #region Grid

        private readonly string _photoFormat = "<div class=\"photo-icon photo-round photo-round-xs pull-left margin-r-sm js-person-popover\" personid=\"{0}\" data-original=\"{1}&w=50\" style=\"background-image: url( '{2}' ); background-size: cover; background-repeat: no-repeat;\"></div>";

        /// <summary>
        /// Handles the Delete event of the gAttempts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gAttempts_Delete( object sender, RowEventArgs e )
        {
            var rockContext = GetRockContext();
            var attemptService = GetAttemptService();
            var attempt = attemptService.Get( e.RowKeyId );

            if ( attempt != null )
            {
                string errorMessage;

                if ( !attemptService.CanDelete( attempt, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                attemptService.Delete( attempt );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gAttempts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gAttempts_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType != DataControlRowType.DataRow )
            {
                return;
            }

            var achievementViewModel = e.Row.DataItem as AttemptViewModel;

            if ( achievementViewModel == null )
            {
                return;
            }

            var enrollmentId = achievementViewModel.Id;
            var person = achievementViewModel.Person;

            var lFullName = e.Row.FindControl( _fullNameFieldId ) as Literal;
            if ( lFullName != null )
            {
                lFullName.Text = person.FullNameReversed;
            }

            var lProgress = e.Row.FindControl( _progressFieldId ) as Literal;
            if ( lProgress != null )
            {
                lProgress.Text = GetProgressBarHtml( achievementViewModel.Progress );
            }

            var lNameWithHtml = e.Row.FindControl( _nameWithHtmlFieldId ) as Literal;
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

            if ( person.IsDeceased )
            {
                e.Row.AddCssClass( "is-deceased" );
            }
        }

        /// <summary>
        /// Handles the RowEditing event of the gAttempts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewEditEventArgs"/> instance containing the event data.</param>
        protected void gAttempts_RowSelected( object sender, RowEventArgs e )
        {
            var detailPage = GetAttributeValue( AttributeKey.DetailPage );

            if ( detailPage.IsNullOrWhiteSpace() )
            {
                return;
            }

            var attemptId = e.RowKeyId;
            NavigateToLinkedPage( AttributeKey.DetailPage, new Dictionary<string, string> {
                { PageParameterKey.StreakAchievementAttemptId, attemptId.ToString() }
            } );
        }

        /// <summary>
        /// Gets the progress bar HTML.
        /// </summary>
        /// <param name="progress">The progress.</param>
        /// <returns></returns>
        private string GetProgressBarHtml( decimal progress )
        {
            var progressInt = Convert.ToInt32( decimal.Round( progress * 100 ) );
            var progressBarWidth = progressInt < 0 ? 0 : ( progressInt > 100 ? 100 : progressInt );
            var insideProgress = progressInt >= 50 ? progressInt : ( int? ) null;
            var outsideProgress = progressInt < 50 ? progressInt : ( int? ) null;
            var progressBarClass = progressInt >= 100 ? "progress-bar-success" : string.Empty;

            return string.Format(
@"<div class=""progress"" style=""margin-bottom: 0;"">
    <div class=""progress-bar {5}"" role=""progressbar"" style=""width: {0}%;"">
        {1}{2}
    </div>
    <span style=""padding-left: 5px;"">{3}{4}</span>
</div>", progressBarWidth, insideProgress, insideProgress.HasValue ? "%" : string.Empty, outsideProgress, outsideProgress.HasValue ? "%" : string.Empty, progressBarClass );
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
            rFilter.SaveUserPreference( FilterKey.AttemptStartDateRange, "Start Date", drpStartDate.DelimitedValues );
            rFilter.SaveUserPreference( FilterKey.Status, "Status", ddlStatus.SelectedValue );
            rFilter.SaveUserPreference( FilterKey.AchievementType, "Achievement Type", statPicker.SelectedValue );

            BindGrid();
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
                case FilterKey.AchievementType:
                    var achievementTypeCache = StreakTypeAchievementTypeCache.Get( e.Value.AsInteger() );
                    e.Value = achievementTypeCache != null ? achievementTypeCache.Name : string.Empty;
                    break;
                case FilterKey.Status:
                case FilterKey.FirstName:
                case FilterKey.LastName:
                    break;
                case FilterKey.AttemptStartDateRange:
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
        /// Handles the GridRebind event of the gAttempts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs" /> instance containing the event data.</param>
        protected void gAttempts_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the Add event of the gAchievements control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gAttempts_Add( object sender, EventArgs e )
        {
            var parameters = new Dictionary<string, string>();
            var achievementType = GetAchievementType();
            var streak = GetStreak();

            if ( streak != null )
            {
                parameters[PageParameterKey.StreakId] = streak.Id.ToString();
            }

            if ( achievementType != null )
            {
                parameters[PageParameterKey.StreakTypeAchievementTypeId] = achievementType.Id.ToString();
            }

            NavigateToLinkedPage( AttributeKey.DetailPage, parameters );
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the status drop down.
        /// </summary>
        private void BindStatusDropDown()
        {
            ddlStatus.DataSource = new List<string> {
                string.Empty,
                "Open",
                "Successful",
                "Unsuccessful"
            };
            ddlStatus.DataBind();
        }

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
        /// Gets the attempt service.
        /// </summary>
        /// <returns></returns>
        private StreakAchievementAttemptService GetAttemptService()
        {
            if ( _attemptService == null )
            {
                var rockContext = GetRockContext();
                _attemptService = new StreakAchievementAttemptService( rockContext );
            }

            return _attemptService;
        }
        private StreakAchievementAttemptService _attemptService = null;

        /// <summary>
        /// Gets the achievement type service.
        /// </summary>
        /// <returns></returns>
        private StreakTypeAchievementTypeService GetAchievementTypeService()
        {
            if ( _achievementTypeService == null )
            {
                var rockContext = GetRockContext();
                _achievementTypeService = new StreakTypeAchievementTypeService( rockContext );
            }

            return _achievementTypeService;
        }
        private StreakTypeAchievementTypeService _achievementTypeService = null;

        /// <summary>
        /// Gets the streak service.
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
        /// Gets the type of the achievement.
        /// </summary>
        /// <returns></returns>
        private StreakTypeAchievementType GetAchievementType()
        {
            if ( _achievementType == null )
            {
                var achievementTypeId = PageParameter( PageParameterKey.StreakTypeAchievementTypeId ).AsIntegerOrNull();

                if ( achievementTypeId.HasValue )
                {
                    _achievementType = GetAchievementTypeService().Get( achievementTypeId.Value );
                }
            }

            return _achievementType;
        }
        private StreakTypeAchievementType _achievementType = null;

        /// <summary>
        /// Gets the streak.
        /// </summary>
        /// <returns></returns>
        private Streak GetStreak()
        {
            if ( _streak == null )
            {
                var streakId = PageParameter( PageParameterKey.StreakId ).AsIntegerOrNull();

                if ( streakId.HasValue )
                {
                    _streak = GetStreakService().Get( streakId.Value );
                }
            }

            return _streak;
        }
        private Streak _streak = null;

        /// <summary>
        /// Gets the attempts query.
        /// </summary>
        /// <returns></returns>
        private IQueryable<StreakAchievementAttempt> GetAttemptsQuery()
        {
            var achievementType = GetAchievementType();
            var streak = GetStreak();
            var attemptService = GetAttemptService();

            var query = attemptService.Queryable()
                .Include( saa => saa.Streak.PersonAlias.Person )
                .AsNoTracking();

            if ( achievementType != null )
            {
                query = query.Where( saa => saa.StreakTypeAchievementTypeId == achievementType.Id );
            }

            if ( streak != null )
            {
                query = query.Where( saa => saa.StreakId == streak.Id );
            }

            return query;
        }

        /// <summary>
        /// Determines whether the person is authorized to view attempts of this achievement type.
        /// </summary>
        private bool CanView()
        {
            if ( !_canView.HasValue )
            {
                var achievementType = GetAchievementType();

                if ( achievementType != null )
                {
                    _canView = achievementType.IsAuthorized( Authorization.VIEW, CurrentPerson );
                }
                else
                {
                    var streak = GetStreak();

                    if ( streak != null )
                    {
                        _canView = streak.IsAuthorized( Authorization.VIEW, CurrentPerson );
                    }
                }
            }

            return _canView ?? false;
        }
        private bool? _canView = null;

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
        /// Initialize the list filter.
        /// </summary>
        private void InitializeFilter()
        {
            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;

            if ( !Page.IsPostBack )
            {
                var achievementType = GetAchievementType();

                if ( achievementType != null )
                {
                    rFilter.UserPreferenceKeyPrefix = string.Format( "{0}-", achievementType.Guid );
                }
                else
                {
                    var streak = GetStreak();

                    if ( streak != null )
                    {
                        rFilter.UserPreferenceKeyPrefix = string.Format( "{0}-", streak.Guid );
                    }
                }

                BindFilter();
            }
        }

        /// <summary>
        /// Initializes the grid.
        /// </summary>
        private void InitializeGrid()
        {
            gAttempts.DataKeyNames = new string[] { "Id" };
            gAttempts.PersonIdField = "Person Id";
            gAttempts.GridRebind += gAttempts_GridRebind;
            gAttempts.Actions.AddClick += gAttempts_Add;
            gAttempts.Actions.ShowAdd = !GetAttributeValue( AttributeKey.DetailPage ).IsNullOrWhiteSpace();
            gAttempts.RowItemText = "Attempt";
            gAttempts.ExportSource = ExcelExportSource.DataSource;

            var achievementType = GetAchievementType();

            if ( achievementType != null )
            {
                gAttempts.ExportFilename = achievementType.Name;
                var achievementNameCol = gAttempts.ColumnsOfType<RockBoundField>().FirstOrDefault( c => c.DataField == "AchievementName" );

                if ( achievementNameCol != null )
                {
                    achievementNameCol.Visible = false;
                }
            }
            else if( GetStreak() != null )
            {
                var nameCol = gAttempts.ColumnsOfType<RockLiteralField>().FirstOrDefault( c => c.ID == _nameWithHtmlFieldId );

                if ( nameCol != null )
                {
                    nameCol.Visible = false;
                }
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
            BindGrid();
        }

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            if ( GetAchievementType() == null )
            {
                statPicker.SelectedValue = rFilter.GetUserPreference( FilterKey.AchievementType );
            }
            else
            {
                statPicker.SelectedValue = null;
                statPicker.Visible = false;
            }

            if ( GetStreak() == null )
            {
                tbFirstName.Text = rFilter.GetUserPreference( FilterKey.FirstName );
                tbLastName.Text = rFilter.GetUserPreference( FilterKey.LastName );
            }
            else
            {
                tbFirstName.Text = string.Empty;
                tbLastName.Text = string.Empty;
                tbFirstName.Visible = false;
                tbLastName.Visible = false;
            }

            drpStartDate.DelimitedValues = rFilter.GetUserPreference( FilterKey.AttemptStartDateRange );
            ddlStatus.SelectedValue = rFilter.GetUserPreference( FilterKey.Status );
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
            var hyperLink = gAttempts.Columns.OfType<HyperLinkField>().FirstOrDefault( c => c.ItemStyle.CssClass == "grid-columncommand" );
            if ( hyperLink != null )
            {
                gAttempts.Columns.Remove( hyperLink );
            }

            var buttonColumn = gAttempts.Columns.OfType<LinkButtonField>().FirstOrDefault( c => c.ItemStyle.CssClass == "grid-columncommand" );
            if ( buttonColumn != null )
            {
                gAttempts.Columns.Remove( buttonColumn );
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
                gAttempts.Columns.Add( column );
            }
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
        /// Binds the attempts grid.
        /// </summary>
        protected void BindGrid()
        {
            var achievementType = GetAchievementType();

            if ( achievementType != null )
            {
                lHeading.Text = string.Format( "{0} Attempts", achievementType.Name );
            }
            else
            {
                lHeading.Text = "Achievement Attempts";
            }

            var query = GetAttemptsQuery();

            // Filter by First Name
            var firstName = tbFirstName.Text;
            if ( !firstName.IsNullOrWhiteSpace() )
            {
                query = query.Where( saa =>
                    saa.Streak.PersonAlias.Person.FirstName.StartsWith( firstName ) ||
                    saa.Streak.PersonAlias.Person.NickName.StartsWith( firstName ) );
            }

            // Filter by Last Name
            var lastName = tbLastName.Text;
            if ( !lastName.IsNullOrWhiteSpace() )
            {
                query = query.Where( saa => saa.Streak.PersonAlias.Person.LastName.StartsWith( lastName ) );
            }

            // Filter by start Date
            var startDateRange = DateRangePicker.CalculateDateRangeFromDelimitedValues( drpStartDate.DelimitedValues );

            if ( startDateRange.Start.HasValue )
            {
                query = query.Where( saa => saa.AchievementAttemptStartDateTime >= startDateRange.Start.Value );
            }

            if ( startDateRange.End.HasValue )
            {
                query = query.Where( saa => saa.AchievementAttemptStartDateTime <= startDateRange.End.Value );
            }

            // Filter by achievement type
            var achievementTypeId = statPicker.SelectedValue.AsIntegerOrNull();
            if ( achievementTypeId.HasValue )
            {
                query = query.Where( saa => saa.StreakTypeAchievementTypeId == achievementTypeId.Value );
            }

            // Filter by status
            var status = ddlStatus.SelectedValue;
            if ( !status.IsNullOrWhiteSpace() )
            {
                status = status.ToLower().Trim();

                if ( status == "successful" )
                {
                    query = query.Where( saa => saa.IsSuccessful );
                }
                else if ( status == "unsuccessful" )
                {
                    query = query.Where( saa => !saa.IsSuccessful && saa.IsClosed );
                }
                else if ( status == "open" )
                {
                    query = query.Where( saa => !saa.IsClosed );
                }
            }

            var viewModelQuery = query.Select( saa => new AttemptViewModel
            {
                Id = saa.Id,
                PersonId = saa.Streak.PersonAlias.PersonId,
                LastName = saa.Streak.PersonAlias.Person.LastName,
                NickName = saa.Streak.PersonAlias.Person.NickName,
                StartDate = saa.AchievementAttemptStartDateTime,
                Person = saa.Streak.PersonAlias.Person,
                EndDate = saa.AchievementAttemptEndDateTime,
                IsSuccessful = saa.IsSuccessful,
                IsClosed = saa.IsClosed,
                Progress = saa.Progress,
                AchievementName = saa.StreakTypeAchievementType.Name
            } );

            // Sort the grid
            var sortProperty = gAttempts.SortProperty;

            if ( sortProperty != null )
            {
                viewModelQuery = viewModelQuery.Sort( sortProperty );
            }
            else
            {
                viewModelQuery = viewModelQuery
                    .OrderBy( avm => avm.IsClosed )
                    .OrderByDescending( avm => avm.StartDate )
                    .ThenBy( avm => avm.LastName );
            }

            gAttempts.SetLinqDataSource( viewModelQuery );
            gAttempts.DataBind();
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
        public class AttemptViewModel
        {
            public int Id { get; set; }
            public int PersonId { get; set; }
            public string LastName { get; set; }
            public string NickName { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public bool IsSuccessful { get; set; }
            public bool IsClosed { get; set; }
            public Person Person { get; set; }
            public decimal Progress { get; set; }
            public string AchievementName { get; set; }
        }

        #endregion View Models
    }
}