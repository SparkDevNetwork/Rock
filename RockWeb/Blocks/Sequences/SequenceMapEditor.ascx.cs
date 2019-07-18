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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Sequences
{
    [DisplayName( "Sequence Map Editor" )]
    [Category( "Sequences" )]
    [Description( "Allows editing a sequence occurrence, engagement, or exclusion map." )]

    public partial class SequenceMapEditor : RockBlock, ISecondaryBlock
    {
        #region Constants

        private const int DaysPerWeek = 7;
        private const int DefaultCheckboxCount = 7;

        #endregion

        #region Keys

        /// <summary>
        /// Keys for user preferences
        /// </summary>
        private static class UserPreferenceKeys
        {
            public const string DateRange = "SequenceMapEditorDateRange";
        }

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKeys
        {
            public const string SequenceId = "SequenceId";
            public const string SequenceEnrollmentId = "SequenceEnrollmentId";
            public const string SequenceOccurrenceExclusionId = "SequenceOccurrenceExclusionId";
        }

        /// <summary>
        /// Keys to use for View State
        /// </summary>
        private static class ViewStateKeys
        {
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
                var sequence = GetSequence();

                if ( sequence == null )
                {
                    nbMessage.Text = "A sequence is required.";
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
                SetUserPreference( UserPreferenceKeys.DateRange, sdrpDateRange.DelimitedValues );
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

            // Validate the sequence            
            var sequence = GetSequence();

            if ( sequence == null )
            {
                ShowBlockError( nbMessage, "A sequence is required" );
                return;
            }

            var rockContext = GetRockContext();
            var map = GetTargetMap();

            if ( IsTargetingEngagementMap() )
            {
                var enrollment = GetSequenceEnrollment();
                enrollment.EngagementMap = map;
            }
            else if ( IsTargetingExclusionMap() )
            {
                var exclusion = GetSequenceOccurrenceExclusion();
                exclusion.ExclusionMap = map;
            }
            else
            {
                sequence.OccurrenceMap = map;
            }

            rockContext.SaveChanges();
            ShowBlockSuccess( nbMessage, "Saved successfully!" );
        }

        /// <summary>
        /// Initialize the date range picker
        /// </summary>
        private void InitializeDatePicker()
        {
            // Try to set to the user's saved preference
            var userPreference = GetUserPreference( UserPreferenceKeys.DateRange );

            if ( !userPreference.IsNullOrWhiteSpace() && !userPreference.StartsWith( SlidingDateRangePicker.SlidingDateRangeType.All.ToString() ) )
            {
                sdrpDateRange.DelimitedValues = userPreference;
            }

            // Default to last 7 frequency units
            if ( sdrpDateRange.SlidingDateRangeMode == SlidingDateRangePicker.SlidingDateRangeType.All )
            {
                sdrpDateRange.SlidingDateRangeMode = SlidingDateRangePicker.SlidingDateRangeType.Last;
                sdrpDateRange.NumberOfTimeUnits = 7;
                sdrpDateRange.TimeUnit = IsSequenceDaily() ?
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
            var sequence = GetSequence();

            if ( sequence == null )
            {
                ShowBlockError( nbMessage, "A sequence is required." );
                return;
            }

            if ( !CanEdit() )
            {
                SetVisible( false );
                return;
            }

            lTitle.Text = GetTargetMapTitle();
            var map = GetTargetMap();
            var errorMessage = string.Empty;

            var isDaily = sequence.OccurrenceFrequency == SequenceOccurrenceFrequency.Daily;
            var dateRange = GetDateRange();
            var startDate = dateRange.Start.Value;
            var endDate = dateRange.End.Value;

            if ( !isDaily )
            {
                startDate = startDate.SundayDate();
                endDate = endDate.SundayDate();
            }

            cblCheckboxes.Label = isDaily ? "Days" : "Weeks (Monday to Sunday)";
            cblCheckboxes.Items.Clear();

            var minDate = GetMinDate();
            var maxDate = isDaily ? RockDateTime.Today : RockDateTime.Today.SundayDate();
            var checkboxCount = SequenceService.GetFrequencyUnitDifference( startDate, endDate, sequence.OccurrenceFrequency, true );

            for ( var i = 0; i < checkboxCount; i++ )
            {
                var representedDate = startDate.AddDays( isDaily ? i : ( i * DaysPerWeek ) );

                cblCheckboxes.Items.Add( new ListItem
                {
                    Enabled = representedDate >= minDate && representedDate <= maxDate,
                    Selected = SequenceService.IsBitSet( map, sequence.StartDate, representedDate, sequence.OccurrenceFrequency, out errorMessage ),
                    Text = GetLabel( isDaily, representedDate ),
                    Value = representedDate.ToISO8601DateString()
                } );
            }

            cblCheckboxes.DataBind();
        }

        /// <summary>
        /// Get the label for the checkbox control
        /// </summary>
        /// <param name="isDaily"></param>
        /// <param name="representedDate"></param>
        /// <returns></returns>
        private string GetLabel( bool isDaily, DateTime representedDate )
        {
            if ( isDaily )
            {
                const string dateFormat = "ddd, MMM dd";
                return representedDate.ToString( dateFormat );
            }
            else
            {
                const string dateFormat = "MMM dd";
                return string.Format( "{0} - {1}",
                    representedDate.AddDays( 0 - ( DaysPerWeek - 1 ) ).ToString( dateFormat ),
                    representedDate.ToString( dateFormat ) );
            }
        }

        /// <summary>
        /// Can the user edit the enrollment
        /// </summary>
        /// <returns></returns>
        private bool CanEdit()
        {
            return UserCanAdministrate && GetSequence() != null;
        }

        #endregion Internal Methods

        #region Data Interface Methods

        /// <summary>
        /// Get the actual enrollment model for deleting or editing
        /// </summary>
        /// <returns></returns>
        private Sequence GetSequence()
        {
            if ( _sequence == null )
            {
                var enrollment = GetSequenceEnrollment();
                var exclusion = GetSequenceOccurrenceExclusion();

                if ( enrollment != null && enrollment.Sequence != null )
                {
                    _sequence = enrollment.Sequence;
                }
                else if ( enrollment != null )
                {
                    var sequenceService = GetSequenceService();
                    _sequence = sequenceService.Get( enrollment.SequenceId );
                }
                else if ( exclusion != null && exclusion.Sequence != null )
                {
                    _sequence = exclusion.Sequence;
                }
                else if ( exclusion != null )
                {
                    var sequenceService = GetSequenceService();
                    _sequence = sequenceService.Get( exclusion.SequenceId );
                }
                else
                {
                    var sequenceId = PageParameter( PageParameterKeys.SequenceId ).AsIntegerOrNull();

                    if ( sequenceId.HasValue && sequenceId.Value > 0 )
                    {
                        var sequenceService = GetSequenceService();
                        _sequence = sequenceService.Get( sequenceId.Value );
                    }
                }
            }

            return _sequence;
        }
        private Sequence _sequence = null;

        /// <summary>
        /// Returns true if the sequence is daily (vs weekly)
        /// </summary>
        /// <returns></returns>
        private bool IsSequenceDaily()
        {
            var sequence = GetSequence();
            return sequence != null && sequence.OccurrenceFrequency == SequenceOccurrenceFrequency.Daily;
        }

        /// <summary>
        /// Get the default end date that should be used for the date picker
        /// </summary>
        /// <returns></returns>
        private DateTime GetDefaultEndDate()
        {
            var isDaily = IsSequenceDaily();

            if ( isDaily )
            {
                return RockDateTime.Today;
            }

            return RockDateTime.Today.SundayDate();
        }

        /// <summary>
        /// Get the default start date that should be used for the date picker
        /// </summary>
        /// <returns></returns>
        private DateTime GetDefaultStartDate()
        {
            // Since the date picked is the earliest bit shown, we want today to be the last bit shown by default
            var isDaily = IsSequenceDaily();
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
            var isDaily = IsSequenceDaily();
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
            var savedMapState = ViewState[ViewStateKeys.Map].ToStringSafe();

            if ( !savedMapState.IsNullOrWhiteSpace() )
            {
                return SequenceService.GetMapFromHexDigitString( savedMapState );
            }

            if ( IsTargetingEngagementMap() )
            {
                return GetSequenceEnrollment().EngagementMap;
            }

            if ( IsTargetingExclusionMap() )
            {
                return GetSequenceOccurrenceExclusion().ExclusionMap;
            }

            if ( IsTargetingOccurrenceMap() )
            {
                return GetSequence().OccurrenceMap;
            }

            return null;
        }

        /// <summary>
        /// Get the map being edited
        /// </summary>
        /// <returns></returns>
        private DateTime GetMinDate()
        {
            var sequence = GetSequence();
            var isDaily = sequence.OccurrenceFrequency == SequenceOccurrenceFrequency.Daily;
            var minDate = isDaily ? sequence.StartDate.Date : sequence.StartDate.SundayDate();

            if ( IsTargetingEngagementMap() )
            {
                var enrollment = GetSequenceEnrollment();
                var enrollmentDate = isDaily ? enrollment.EnrollmentDate.Date : enrollment.EnrollmentDate.SundayDate();

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
            var sequence = GetSequence();
            var map = GetTargetMap();
            var errorMessage = string.Empty;

            foreach ( ListItem checkbox in cblCheckboxes.Items )
            {
                var representedDate = checkbox.Value.AsDateTime();

                if ( representedDate.HasValue )
                {
                    map = SequenceService.SetBit( map, sequence.StartDate, representedDate.Value,
                        sequence.OccurrenceFrequency, checkbox.Selected, out errorMessage );
                }
            }

            ViewState[ViewStateKeys.Map] = SequenceService.GetHexDigitStringFromMap( map );
        }

        /// <summary>
        /// Get the title of the map being edited
        /// </summary>
        /// <returns></returns>
        private string GetTargetMapTitle()
        {
            if ( IsTargetingEngagementMap() )
            {
                return "Engagement Map Editor";
            }

            if ( IsTargetingExclusionMap() )
            {
                return "Exclusion Map Editor";
            }

            return "Occurrence Map Editor";
        }

        /// <summary>
        /// Is the editor editing the enrollment Engagement map
        /// </summary>
        /// <returns></returns>
        private bool IsTargetingEngagementMap()
        {
            return GetSequenceEnrollment() != null;
        }

        /// <summary>
        /// Is the editor editing the exclusion map
        /// </summary>
        /// <returns></returns>
        private bool IsTargetingExclusionMap()
        {
            return !IsTargetingEngagementMap() && GetSequenceOccurrenceExclusion() != null;
        }

        /// <summary>
        /// Is the editor editing the occurrence map
        /// </summary>
        /// <returns></returns>
        private bool IsTargetingOccurrenceMap()
        {
            return !IsTargetingEngagementMap() && !IsTargetingExclusionMap() && GetSequence() != null;
        }

        /// <summary>
        /// Get the actual sequence enrollment model for deleting or editing
        /// </summary>
        /// <returns></returns>
        private SequenceEnrollment GetSequenceEnrollment()
        {
            if ( _sequenceEnrollment == null )
            {
                var sequenceEnrollmentId = PageParameter( PageParameterKeys.SequenceEnrollmentId ).AsIntegerOrNull();

                if ( sequenceEnrollmentId.HasValue && sequenceEnrollmentId.Value > 0 )
                {
                    var sequenceEnrollmentService = GetSequenceEnrollmentService();
                    _sequenceEnrollment = sequenceEnrollmentService.Get( sequenceEnrollmentId.Value );
                }
            }

            return _sequenceEnrollment;
        }
        private SequenceEnrollment _sequenceEnrollment = null;

        /// <summary>
        /// Get the exclusion model
        /// </summary>
        /// <returns></returns>
        private SequenceOccurrenceExclusion GetSequenceOccurrenceExclusion()
        {
            if ( _sequenceOccurrenceExclusion == null )
            {
                var id = PageParameter( PageParameterKeys.SequenceOccurrenceExclusionId ).AsIntegerOrNull();

                if ( id.HasValue && id.Value > 0 )
                {
                    var service = GetSequenceOccurrenceExclusionService();
                    _sequenceOccurrenceExclusion = service.Get( id.Value );
                }
            }

            return _sequenceOccurrenceExclusion;
        }
        private SequenceOccurrenceExclusion _sequenceOccurrenceExclusion = null;

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
        /// Get the sequence service
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
        /// Get the sequence enrollment service
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
        /// Get the sequence exclusion service
        /// </summary>
        /// <returns></returns>
        private SequenceOccurrenceExclusionService GetSequenceOccurrenceExclusionService()
        {
            if ( _sequenceOccurrenceExclusionService == null )
            {
                var rockContext = GetRockContext();
                _sequenceOccurrenceExclusionService = new SequenceOccurrenceExclusionService( rockContext );
            }

            return _sequenceOccurrenceExclusionService;
        }
        private SequenceOccurrenceExclusionService _sequenceOccurrenceExclusionService = null;

        #endregion Data Interface Methods        
    }
}