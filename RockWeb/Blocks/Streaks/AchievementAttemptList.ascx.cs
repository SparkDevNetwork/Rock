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
using Rock.Achievement;
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
    [DisplayName( "Achievement Attempt List" )]
    [Category( "Achievements" )]
    [Description( "Lists all the people that have made an attempt at earning an achievement." )]

    [LinkedPage(
        "Detail Page",
        Description = "Page navigated to when a grid item is clicked.",
        Key = AttributeKey.DetailPage,
        IsRequired = false,
        Order = 1 )]

    [Rock.SystemGuid.BlockTypeGuid( "9C1236AE-4FF0-480C-A7DF-0E5277CA75FB" )]
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
            public const string AchievementTypeId = "AchievementTypeId";

            /// <summary>
            /// The achievement attempt identifier
            /// </summary>
            public const string AchievementAttemptId = "AchievementAttemptId";
        }

        /// <summary>
        /// Keys to use for filters
        /// </summary>
        private static class FilterKey
        {
            /// <summary>
            /// The achiever name filter key
            /// </summary>
            public const string AchieverName = "AchieverName";

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

            base.OnLoad( e );
        }

        #endregion Base Control Methods

        #region Grid

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

            var lProgress = e.Row.FindControl( _progressFieldId ) as Literal;
            if ( lProgress != null )
            {
                lProgress.Text = GetProgressBarHtml( achievementViewModel.Progress );
            }

            var personColumn = e.Row.FindControl( "lPerson" ) as Literal;
            if ( personColumn != null && achievementViewModel.Entity is PersonAlias personAlias )
            {
                personColumn.Text = $"<a class='btn btn-default btn-sm' href='/person/{personAlias.PersonId}'><i class='fa fa-user'></i></a>";
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
                { PageParameterKey.AchievementAttemptId, attemptId.ToString() }
            } );
        }

        /// <summary>
        /// Gets the progress bar HTML.
        /// </summary>
        /// <param name="progress">The progress.</param>
        /// <returns></returns>
        private string GetProgressBarHtml( decimal progress )
        {
            var progressLong = Convert.ToInt64( decimal.Round( progress * 100 ) );
            var progressBarWidth = progressLong < 0 ? 0 : ( progressLong > 100 ? 100 : progressLong );
            var insideProgress = progressLong >= 50 ? progressLong : ( long? ) null;
            var outsideProgress = progressLong < 50 ? progressLong : ( long? ) null;
            var progressBarClass = progressLong >= 100 ? "progress-bar-success" : string.Empty;

            return string.Format(
@"<div class=""progress m-0"">
    <div class=""progress-bar {5}"" role=""progressbar"" style=""width: {0}%;"">
        {1}{2}
    </div>
    <span class=""pl-1"">{3}{4}</span>
</div>", progressBarWidth, insideProgress, insideProgress.HasValue ? "%" : string.Empty, outsideProgress, outsideProgress.HasValue ? "%" : string.Empty, progressBarClass );
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SetFilterPreference( FilterKey.AchieverName, "Achiever Name", tbAchieverName.Text );
            rFilter.SetFilterPreference( FilterKey.AttemptStartDateRange, "Start Date", drpStartDate.DelimitedValues );
            rFilter.SetFilterPreference( FilterKey.Status, "Status", ddlStatus.SelectedValue );
            rFilter.SetFilterPreference( FilterKey.AchievementType, "Achievement Type", statPicker.SelectedValue );

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
                    var achievementTypeCache = AchievementTypeCache.Get( e.Value.AsInteger() );
                    e.Value = achievementTypeCache != null ? achievementTypeCache.Name : string.Empty;
                    break;
                case FilterKey.Status:
                case FilterKey.AchieverName:
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
            rFilter.DeleteFilterPreferences();
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
            var achievementType = GetAchievementTypeCache();

            if ( achievementType != null )
            {
                parameters[PageParameterKey.AchievementTypeId] = achievementType.Id.ToString();
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
        private AchievementAttemptService GetAttemptService()
        {
            if ( _attemptService == null )
            {
                var rockContext = GetRockContext();
                _attemptService = new AchievementAttemptService( rockContext );
            }

            return _attemptService;
        }
        private AchievementAttemptService _attemptService = null;

        /// <summary>
        /// Gets the achievement type service.
        /// </summary>
        /// <returns></returns>
        private AchievementTypeService GetAchievementTypeService()
        {
            if ( _achievementTypeService == null )
            {
                var rockContext = GetRockContext();
                _achievementTypeService = new AchievementTypeService( rockContext );
            }

            return _achievementTypeService;
        }
        private AchievementTypeService _achievementTypeService = null;

        /// <summary>
        /// Gets the type of the achievement.
        /// </summary>
        /// <returns></returns>
        private AchievementTypeCache GetAchievementTypeCache()
        {
            if ( _achievementTypeCache == null )
            {
                var achievementTypeId = PageParameter( PageParameterKey.AchievementTypeId ).AsIntegerOrNull();

                if ( achievementTypeId.HasValue )
                {
                    _achievementTypeCache = AchievementTypeCache.Get( achievementTypeId.Value );
                }
            }

            return _achievementTypeCache;
        }
        private AchievementTypeCache _achievementTypeCache = null;

        /// <summary>
        /// Gets the attempts query.
        /// </summary>
        /// <returns></returns>
        private IQueryable<AchieverAttemptItem> GetAttemptsQuery()
        {
            if ( _attemptsQuery != null )
            {
                return _attemptsQuery;
            }

            var rockContext = GetRockContext();
            var achievementType = GetAchievementTypeCache();
            var achievementTypes = AchievementTypeCache.All();

            if ( achievementType != null )
            {
                achievementTypes = achievementTypes.Where( at => at.Id == achievementType.Id ).ToList();
            }

            var subQueries = new List<IQueryable<AchieverAttemptItem>>();

            foreach ( var at in achievementTypes )
            {
                var component = at.AchievementComponent;

                if (component == null)
                {
                    continue;
                }

                var componentQuery = component.GetAchieverAttemptQuery( at, rockContext ).AsNoTracking();
                subQueries.Add( componentQuery );
            }

            _attemptsQuery = subQueries.Any() ?
                subQueries.Aggregate( ( a, b ) => a.Union( b ) ) :
                new List<AchieverAttemptItem>().AsQueryable();

            return _attemptsQuery;
        }
        private IQueryable<AchieverAttemptItem> _attemptsQuery = null;

        /// <summary>
        /// Determines whether the person is authorized to view attempts of this achievement type.
        /// </summary>
        private bool CanView()
        {
            if ( !_canView.HasValue )
            {
                var achievementType = GetAchievementTypeCache();

                if ( achievementType != null )
                {
                    _canView = achievementType.IsAuthorized( Authorization.VIEW, CurrentPerson );
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
                var achievementType = GetAchievementTypeCache();

                if ( achievementType != null )
                {
                    rFilter.PreferenceKeyPrefix = string.Format( "{0}-", achievementType.Guid );
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
            gAttempts.GridRebind += gAttempts_GridRebind;
            gAttempts.Actions.AddClick += gAttempts_Add;
            gAttempts.Actions.ShowAdd = !GetAttributeValue( AttributeKey.DetailPage ).IsNullOrWhiteSpace();
            gAttempts.RowItemText = "Attempt";
            gAttempts.ExportSource = ExcelExportSource.DataSource;

            var achievementType = GetAchievementTypeCache();

            if ( achievementType != null )
            {
                gAttempts.ExportFilename = achievementType.Name;
                var achievementNameCol = gAttempts.ColumnsOfType<RockBoundField>().FirstOrDefault( c => c.DataField == "AchievementName" );

                if ( achievementNameCol != null )
                {
                    achievementNameCol.Visible = false;
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
            BindGrid();
        }

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            if ( GetAchievementTypeCache() == null )
            {
                statPicker.SelectedValue = rFilter.GetFilterPreference( FilterKey.AchievementType );
            }
            else
            {
                statPicker.SelectedValue = null;
                statPicker.Visible = false;
            }

            tbAchieverName.Text = rFilter.GetFilterPreference( FilterKey.AchieverName );
            drpStartDate.DelimitedValues = rFilter.GetFilterPreference( FilterKey.AttemptStartDateRange );
            ddlStatus.SelectedValue = rFilter.GetFilterPreference( FilterKey.Status );
        }

        /// <summary>
        /// Binds the attempts grid.
        /// </summary>
        protected void BindGrid()
        {
            var achievementType = GetAchievementTypeCache();

            if ( achievementType != null )
            {
                lHeading.Text = string.Format( "{0} Attempts", achievementType.Name );
            }
            else
            {
                lHeading.Text = "Achievement Attempts";
            }

            var query = GetAttemptsQuery();

            // Filter by Achiever Name
            var achieverName = tbAchieverName.Text;
            if ( !achieverName.IsNullOrWhiteSpace() )
            {
                query = query.Where( aa => aa.AchieverName.StartsWith( achieverName ) );
            }

            // Filter by start Date
            var startDateRange = DateRangePicker.CalculateDateRangeFromDelimitedValues( drpStartDate.DelimitedValues );

            if ( startDateRange.Start.HasValue )
            {
                query = query.Where( aa => aa.AchievementAttempt.AchievementAttemptStartDateTime >= startDateRange.Start.Value );
            }

            if ( startDateRange.End.HasValue )
            {
                query = query.Where( aa => aa.AchievementAttempt.AchievementAttemptStartDateTime <= startDateRange.End.Value );
            }

            // Filter by achievement type
            var achievementTypeId = statPicker.SelectedValue.AsIntegerOrNull();
            if ( achievementTypeId.HasValue )
            {
                query = query.Where( aa => aa.AchievementAttempt.AchievementTypeId == achievementTypeId.Value );
            }

            // Filter by status
            var status = ddlStatus.SelectedValue;
            if ( !status.IsNullOrWhiteSpace() )
            {
                status = status.ToLower().Trim();

                if ( status == "successful" )
                {
                    query = query.Where( aa => aa.AchievementAttempt.IsSuccessful );
                }
                else if ( status == "unsuccessful" )
                {
                    query = query.Where( aa => !aa.AchievementAttempt.IsSuccessful && aa.AchievementAttempt.IsClosed );
                }
                else if ( status == "open" )
                {
                    query = query.Where( aa => !aa.AchievementAttempt.IsClosed );
                }
            }

            var viewModelQuery = query.Select( aa => new AttemptViewModel
            {
                Id = aa.AchievementAttempt.Id,
                AchieverName = aa.AchieverName,
                StartDate = aa.AchievementAttempt.AchievementAttemptStartDateTime,
                EndDate = aa.AchievementAttempt.AchievementAttemptEndDateTime,
                IsSuccessful = aa.AchievementAttempt.IsSuccessful,
                IsClosed = aa.AchievementAttempt.IsClosed,
                Progress = aa.AchievementAttempt.Progress,
                AchievementName = aa.AchievementAttempt.AchievementType.Name,
                Entity = aa.Achiever
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
                    .ThenBy( avm => avm.AchieverName );
            }

            gAttempts.SetLinqDataSource( viewModelQuery );
            gAttempts.DataBind();
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
        public class AttemptViewModel : RockDynamic
        {
            public int Id { get; set; }
            public string AchieverName { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public bool IsSuccessful { get; set; }
            public bool IsClosed { get; set; }
            public decimal Progress { get; set; }
            public string AchievementName { get; set; }
            public IEntity Entity { get; set; }
        }

        /// <summary>
        /// View Model for when the attempt qu
        /// </summary>
        public class JoinedQueryViewModel
        {
            /// <summary>
            /// Gets or sets the achievement attempt.
            /// </summary>
            public AchievementAttempt AchievementAttempt { get; set; }

            /// <summary>
            /// Gets or sets the achiever.
            /// </summary>
            public IEntity Achiever { get; set; }
        }

        #endregion View Models
    }
}