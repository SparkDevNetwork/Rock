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
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Streaks
{
    [DisplayName( "Streak Map Editor" )]
    [Category( "Streaks" )]
    [Description( "Allows editing a streak occurrence, engagement, or exclusion map." )]

    #region Block Attributes

    [BooleanField(
        name: "Show Streak Enrollment Exclusion Map",
        description: "If this map editor is placed in the context of a streak enrollment, should it show the person exclusion map for that streak enrollment?",
        defaultValue: false,
        Key = AttributeKey.IsEngagementExclusion,
        Order = 0 )]

    #endregion

    [Rock.SystemGuid.BlockTypeGuid( "4DB69FBA-32C7-448A-B322-EDFBCEF2D124" )]
    public partial class StreakMapEditor : RockBlock, ISecondaryBlock
    {
        #region Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The is engagement exclusion map key
            /// </summary>
            public const string IsEngagementExclusion = "IsEngagementExclusion";
        }

        #endregion Keys

        #region Constants

        private const int DaysPerWeek = 7;
        private const int DefaultCheckboxCount = 7;

        #endregion

        #region Keys

        /// <summary>
        /// Keys for user preferences
        /// </summary>
        private static class UserPreferenceKey
        {
            /// <summary>
            /// The date range user preference key
            /// </summary>
            public const string DateRange = "date-range";
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

            /// <summary>
            /// The streak type exclusion id page parameter key
            /// </summary>
            public const string StreakTypeExclusionId = "StreakTypeExclusionId";
        }

        /// <summary>
        /// Keys to use for View State
        /// </summary>
        private static class ViewStateKey
        {
            /// <summary>
            /// The map view state key
            /// </summary>
            public const string Map = "Map";
        }

        #endregion Keys

        #region ISecondaryBlock

        /// <summary>
        /// Hook so that other blocks can set the visibility of all ISecondaryBlocks on its page
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlDetails.Visible = visible;
        }

        #endregion ISecondaryBlock

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            BlockUpdated += Block_BlockUpdated;
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
                var streakType = GetStreakType();

                if ( streakType == null )
                {
                    nbMessage.Text = "A streak type is required.";
                    return;
                }

                InitializeDatePicker();
                RenderCheckboxes();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// When the date range is changed and the user clicks refresh to see those changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnRefresh_Click( object sender, EventArgs e )
        {
            SaveMapState();
            RenderCheckboxes();

            if ( sdrpDateRange.SlidingDateRangeMode != SlidingDateRangePicker.SlidingDateRangeType.All )
            {
                var preferences = GetBlockPersonPreferences();

                preferences.SetValue( UserPreferenceKey.DateRange, sdrpDateRange.DelimitedValues );
                preferences.Save();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            SaveMapState();
            SaveRecord();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            RenderCheckboxes();
        }

        #endregion Events

        #region Block Notification Messages

        /// <summary>
        /// Show a notification message for the block.
        /// </summary>
        /// <param name="notificationControl"></param>
        /// <param name="message"></param>
        /// <param name="notificationType"></param>
        private void ShowBlockNotification( NotificationBox notificationControl, string message, NotificationBoxType notificationType = NotificationBoxType.Info )
        {
            notificationControl.Text = message;
            notificationControl.NotificationBoxType = notificationType;
        }

        private void ShowBlockError( NotificationBox notificationControl, string message )
        {
            ShowBlockNotification( notificationControl, message, NotificationBoxType.Danger );
        }

        private void ShowBlockException( NotificationBox notificationControl, Exception ex, bool writeToLog = true )
        {
            ShowBlockNotification( notificationControl, ex.Message, NotificationBoxType.Danger );

            if ( writeToLog )
            {
                LogException( ex );
            }
        }

        private void ShowBlockSuccess( NotificationBox notificationControl, string message )
        {
            ShowBlockNotification( notificationControl, message, NotificationBoxType.Success );
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Save the current record.
        /// </summary>
        /// <returns></returns>
        private void SaveRecord()
        {
            if ( !CanEdit() )
            {
                ShowBlockError( nbMessage, "You are not authorized" );
                return;
            }

            // Validate the streak type            
            var streakType = GetStreakType();

            if ( streakType == null )
            {
                ShowBlockError( nbMessage, "A streak type is required" );
                return;
            }

            var rockContext = GetRockContext();
            var map = GetTargetMap();

            if ( IsTargetingEngagementMap() )
            {
                var isEngagementExclusionMap = GetAttributeValue( AttributeKey.IsEngagementExclusion ).AsBoolean();
                var enrollment = GetStreak();

                if ( isEngagementExclusionMap )
                {
                    enrollment.ExclusionMap = map;
                }
                else
                {
                    enrollment.EngagementMap = map;
                }
            }
            else if ( IsTargetingExclusionMap() )
            {
                var exclusion = GetStreakTypeExclusion();
                exclusion.ExclusionMap = map;
            }
            else
            {
                streakType.OccurrenceMap = map;
            }

            rockContext.SaveChanges();

            if ( IsTargetingOccurrenceMap() )
            {
                var occurrenceTerm = IsStreakTypeDaily() ? "today" : "this week";
                ShowBlockSuccess( nbMessage, string.Format(
                    "Saved successfully. Please note that streak counts will not reflect these changes until the end of {0}.",
                    occurrenceTerm ) );
            }
            else
            {
                ShowBlockSuccess( nbMessage, "Saved successfully." );
            }
        }

        /// <summary>
        /// Initialize the date range picker
        /// </summary>
        private void InitializeDatePicker()
        {
            // Try to set to the user's saved preference
            var preferences = GetBlockPersonPreferences();
            var userPreference = preferences.GetValue( UserPreferenceKey.DateRange );

            if ( !userPreference.IsNullOrWhiteSpace() && !userPreference.StartsWith( SlidingDateRangePicker.SlidingDateRangeType.All.ToString() ) )
            {
                sdrpDateRange.DelimitedValues = userPreference;
            }

            // Default to last 7 frequency units
            if ( sdrpDateRange.SlidingDateRangeMode == SlidingDateRangePicker.SlidingDateRangeType.All )
            {
                sdrpDateRange.SlidingDateRangeMode = SlidingDateRangePicker.SlidingDateRangeType.Last;
                sdrpDateRange.NumberOfTimeUnits = 7;
                sdrpDateRange.TimeUnit = IsStreakTypeDaily() ?
                    SlidingDateRangePicker.TimeUnitType.Day :
                    SlidingDateRangePicker.TimeUnitType.Week;
            }
        }

        /// <summary>
        /// Shows the controls needed
        /// </summary>
        private void RenderCheckboxes()
        {
            nbMessage.Text = string.Empty;
            var streakType = GetStreakType();
            var streakTypeCache = GetStreakTypeCache();

            if ( streakType == null )
            {
                ShowBlockError( nbMessage, "A streak type is required." );
                return;
            }

            if ( !CanEdit() )
            {
                SetVisible( false );
                return;
            }

            lTitle.Text = GetTargetMapTitle();
            var map = GetTargetMap();

            var dateRange = GetDateRange();
            var startDate = StreakTypeService.AlignDate( dateRange.Start.Value, streakTypeCache );
            var endDate = StreakTypeService.AlignDate( dateRange.End.Value, streakTypeCache );

            // Change values based on streakTypeCache.OccurrenceFrequency (Days/Weeks/Months/Years).
            string dateTimeFormat;
            Func<DateTime, int, DateTime> dateTimeGenerator;

            switch ( streakTypeCache.OccurrenceFrequency )
            {
                case StreakOccurrenceFrequency.Daily:
                    cblCheckboxes.Label = "Days";
                    // Format as "ddd, MMM dd, yyyy" (ex. Wed, Jan 02, 2019).
                    dateTimeFormat = "ddd, MMM dd, yyyy";
                    dateTimeGenerator = ( d, i ) => d.AddDays( i );
                    break;
                case StreakOccurrenceFrequency.Weekly:
                    cblCheckboxes.Label = "Weeks";
                    // Format as "MMM dd, yyyy" (ex. Jan 02, 2019).
                    dateTimeFormat = "MMM dd, yyyy";
                    dateTimeGenerator = ( d, i ) => d.AddDays( i * DaysPerWeek );
                    break;
                case StreakOccurrenceFrequency.Monthly:
                    cblCheckboxes.Label = "Months";
                    // Format as "MMM yyyy" (ex. Jan 2019).
                    dateTimeFormat = "MMM yyyy";
                    dateTimeGenerator = ( d, i ) => d.AddMonths( i );
                    break;
                case StreakOccurrenceFrequency.Yearly:
                    cblCheckboxes.Label = "Years";
                    // Format as "yyyy" (ex. 2019).
                    dateTimeFormat = "yyyy";
                    dateTimeGenerator = ( d, i ) => d.AddYears( i );
                    break;
                default:
                    throw new NotImplementedException( $"StreakOccurrenceFrequency '{streakTypeCache.OccurrenceFrequency}' is not implemented" );
            }

            cblCheckboxes.Items.Clear();

            var minDate = GetMinDate();
            var maxDate = StreakTypeService.AlignDate( RockDateTime.Today, streakTypeCache );
            var checkboxCount = StreakTypeService.GetFrequencyUnitDifference( startDate, endDate, streakTypeCache, true );

            for ( var i = 0; i < checkboxCount; i++ )
            {
                var representedDate = dateTimeGenerator( startDate, i );
                cblCheckboxes.Items.Add( new ListItem
                {
                    Enabled = representedDate >= minDate && representedDate <= maxDate,
                    Selected = StreakTypeService.IsBitSet( streakTypeCache, map, representedDate, out _ ),
                    Text = representedDate.ToString( dateTimeFormat ),
                    Value = representedDate.ToISO8601DateString()
                } );
            }

            cblCheckboxes.DataBind();
        }

        /// <summary>
        /// Can the user edit the enrollment
        /// </summary>
        /// <returns></returns>
        private bool CanEdit()
        {
            return UserCanAdministrate && GetStreakType() != null;
        }

        #endregion Internal Methods

        #region Data Interface Methods

        /// <summary>
        /// Get the actual enrollment model for deleting or editing
        /// </summary>
        /// <returns></returns>
        private StreakType GetStreakType()
        {
            if ( _streakType == null )
            {
                var streak = GetStreak();
                var exclusion = GetStreakTypeExclusion();

                if ( streak != null && streak.StreakType != null )
                {
                    _streakType = streak.StreakType;
                }
                else if ( streak != null )
                {
                    var streakTypeService = GetStreakTypeService();
                    _streakType = streakTypeService.Get( streak.StreakTypeId );
                }
                else if ( exclusion != null && exclusion.StreakType != null )
                {
                    _streakType = exclusion.StreakType;
                }
                else if ( exclusion != null )
                {
                    var streakTypeService = GetStreakTypeService();
                    _streakType = streakTypeService.Get( exclusion.StreakTypeId );
                }
                else
                {
                    var streakTypeId = PageParameter( PageParameterKey.StreakTypeId ).AsIntegerOrNull();

                    if ( streakTypeId.HasValue && streakTypeId.Value > 0 )
                    {
                        var streakTypeService = GetStreakTypeService();
                        _streakType = streakTypeService.Get( streakTypeId.Value );
                    }
                }
            }

            return _streakType;
        }
        private StreakType _streakType = null;

        /// <summary>
        /// Gets the streak type cache.
        /// </summary>
        /// <returns></returns>
        private StreakTypeCache GetStreakTypeCache()
        {
            var streakType = GetStreakType();
            return streakType == null ? null : StreakTypeCache.Get( streakType.Id );
        }

        /// <summary>
        /// Returns true if the streak type is daily (vs weekly)
        /// </summary>
        /// <returns></returns>
        private bool IsStreakTypeDaily()
        {
            var streakType = GetStreakType();
            return streakType != null && streakType.OccurrenceFrequency == StreakOccurrenceFrequency.Daily;
        }

        /// <summary>
        /// Get the default end date that should be used for the date picker
        /// </summary>
        /// <returns></returns>
        private DateTime GetDefaultEndDate()
        {
            var streakTypeCache = GetStreakTypeCache();
            return StreakTypeService.AlignDate( RockDateTime.Now, streakTypeCache );
        }

        /// <summary>
        /// Get the default start date that should be used for the date picker
        /// </summary>
        /// <returns></returns>
        private DateTime GetDefaultStartDate()
        {
            // Since the date picked is the earliest bit shown, we want today to be the last bit shown by default
            var isDaily = IsStreakTypeDaily();
            const int defaultCheckboxCount = 7;

            if ( isDaily )
            {
                return RockDateTime.Today.AddDays( 0 - ( defaultCheckboxCount - 1 ) );
            }

            return RockDateTime.Today.AddDays( 0 - ( DaysPerWeek * ( defaultCheckboxCount - 1 ) ) );
        }

        /// <summary>
        /// Get the selected date range or the default range
        /// </summary>
        /// <returns></returns>
        private DateRange GetDateRange()
        {
            var isDaily = IsStreakTypeDaily();
            var range = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( sdrpDateRange.DelimitedValues );

            if ( !range.Start.HasValue && !range.End.HasValue )
            {
                range.Start = GetDefaultStartDate();
                range.End = GetDefaultEndDate();
            }
            else if ( !range.Start.HasValue )
            {
                range.Start = range.End.Value.AddDays( ( 0 - DefaultCheckboxCount ) * ( isDaily ? 1 : DaysPerWeek ) );
            }
            else if ( !range.End.HasValue )
            {
                range.End = range.Start.Value.AddDays( ( DefaultCheckboxCount ) * ( isDaily ? 1 : DaysPerWeek ) );
            }

            return range;
        }

        /// <summary>
        /// Get the map being edited
        /// </summary>
        /// <returns></returns>
        private byte[] GetTargetMap()
        {
            var savedMapState = ViewState[ViewStateKey.Map].ToStringSafe();

            if ( !savedMapState.IsNullOrWhiteSpace() )
            {
                return StreakTypeService.GetMapFromHexDigitString( savedMapState );
            }

            if ( IsTargetingEngagementMap() )
            {
                var isEngagementExclusionMap = GetAttributeValue( AttributeKey.IsEngagementExclusion ).AsBoolean();
                return isEngagementExclusionMap ? GetStreak().ExclusionMap : GetStreak().EngagementMap;
            }

            if ( IsTargetingExclusionMap() )
            {
                return GetStreakTypeExclusion().ExclusionMap;
            }

            if ( IsTargetingOccurrenceMap() )
            {
                return GetStreakType().OccurrenceMap;
            }

            return null;
        }

        /// <summary>
        /// Get the map being edited
        /// </summary>
        /// <returns></returns>
        private DateTime GetMinDate()
        {
            var streakType = GetStreakType();
            var streakTypeCache = GetStreakTypeCache();
            var minDate = StreakTypeService.AlignDate( streakType.StartDate, streakTypeCache );

            if ( IsTargetingEngagementMap() )
            {
                var enrollment = GetStreak();
                var enrollmentDate = StreakTypeService.AlignDate( enrollment.EnrollmentDate, streakTypeCache );

                if ( enrollmentDate > minDate )
                {
                    minDate = enrollmentDate;
                }
            }

            return minDate;
        }

        /// <summary>
        /// Write the map to the view state but not to the database
        /// </summary>
        private void SaveMapState()
        {
            var streakType = GetStreakType();
            var streakTypeCache = GetStreakTypeCache();
            var map = GetTargetMap();
            var errorMessage = string.Empty;

            foreach ( ListItem checkbox in cblCheckboxes.Items )
            {
                var representedDate = checkbox.Value.AsDateTime();

                if ( representedDate.HasValue )
                {
                    map = StreakTypeService.SetBit( streakTypeCache, map, representedDate.Value, checkbox.Selected, out errorMessage );
                }
            }

            ViewState[ViewStateKey.Map] = StreakTypeService.GetHexDigitStringFromMap( map );
        }

        /// <summary>
        /// Get the title of the map being edited
        /// </summary>
        /// <returns></returns>
        private string GetTargetMapTitle()
        {
            if ( IsTargetingEngagementMap() )
            {
                var isEngagementExclusionMap = GetAttributeValue( AttributeKey.IsEngagementExclusion ).AsBoolean();
                return isEngagementExclusionMap ? "Engagement Exclusion Map" : "Engagement Map";
            }

            if ( IsTargetingExclusionMap() )
            {
                return "Exclusion Map";
            }

            return "Occurrence Map";
        }

        /// <summary>
        /// Is the editor editing the enrollment Engagement map
        /// </summary>
        /// <returns></returns>
        private bool IsTargetingEngagementMap()
        {
            return GetStreak() != null;
        }

        /// <summary>
        /// Is the editor editing the exclusion map
        /// </summary>
        /// <returns></returns>
        private bool IsTargetingExclusionMap()
        {
            return !IsTargetingEngagementMap() && GetStreakTypeExclusion() != null;
        }

        /// <summary>
        /// Is the editor editing the occurrence map
        /// </summary>
        /// <returns></returns>
        private bool IsTargetingOccurrenceMap()
        {
            return !IsTargetingEngagementMap() && !IsTargetingExclusionMap() && GetStreakType() != null;
        }

        /// <summary>
        /// Get the actual streak model for deleting or editing
        /// </summary>
        /// <returns></returns>
        private Streak GetStreak()
        {
            if ( _streak == null )
            {
                var streakId = PageParameter( PageParameterKey.StreakId ).AsIntegerOrNull();

                if ( streakId.HasValue && streakId.Value > 0 )
                {
                    var streakService = GetStreakService();
                    _streak = streakService.Get( streakId.Value );
                }
            }

            return _streak;
        }
        private Streak _streak = null;

        /// <summary>
        /// Get the exclusion model
        /// </summary>
        /// <returns></returns>
        private StreakTypeExclusion GetStreakTypeExclusion()
        {
            if ( _streakTypeExclusion == null )
            {
                var id = PageParameter( PageParameterKey.StreakTypeExclusionId ).AsIntegerOrNull();

                if ( id.HasValue && id.Value > 0 )
                {
                    var service = GetStreakTypeExclusionService();
                    _streakTypeExclusion = service.Get( id.Value );
                }
            }

            return _streakTypeExclusion;
        }
        private StreakTypeExclusion _streakTypeExclusion = null;

        /// <summary>
        /// Get the rock context
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
        /// Get the streak type service
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
        /// Get the streak service
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
        /// Get the streak type exclusion service
        /// </summary>
        /// <returns></returns>
        private StreakTypeExclusionService GetStreakTypeExclusionService()
        {
            if ( _streakTypeExclusionService == null )
            {
                var rockContext = GetRockContext();
                _streakTypeExclusionService = new StreakTypeExclusionService( rockContext );
            }

            return _streakTypeExclusionService;
        }
        private StreakTypeExclusionService _streakTypeExclusionService = null;

        #endregion Data Interface Methods
    }
}