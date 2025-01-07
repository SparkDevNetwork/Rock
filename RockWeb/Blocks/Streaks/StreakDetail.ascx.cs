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
using System.Text;
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

namespace RockWeb.Blocks.Streaks
{
    [DisplayName( "Streak Detail" )]
    [Category( "Streaks" )]
    [Description( "Displays the details of the given streak for editing." )]

    [Rock.SystemGuid.BlockTypeGuid( "EA9857FF-6703-4E4E-A6FF-65C23EBD2216" )]
    public partial class StreakDetail : RockBlock
    {
        /// <summary>
        /// The number of chart bits to show
        /// </summary>
        private static int ChartBitsToShow = 350;

        #region Keys

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
            /// The person id page parameter key
            /// </summary>
            public const string PersonId = "PersonId";
        }

        #endregion Keys

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            InitializeActionButtons();
            InitializeSettingsNotification();

            // Add lazyload so that person-link-popover javascript works
            RockPage.AddScriptLink( "~/Scripts/jquery.lazyload.min.js" );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                var enrollment = GetStreak();
                pdAuditDetails.SetEntity( enrollment, ResolveRockUrl( "~" ) );

                var streakType = GetStreakType();

                if ( streakType == null )
                {
                    nbEditModeMessage.Text = "A streak type is required.";
                    base.OnLoad( e );
                    return;
                }

                RenderState();
                base.OnLoad( e );
            }
        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs
        /// </summary>
        /// <param name="pageReference">The page reference.</param>
        /// <returns></returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            var enrollment = GetStreak();
            breadCrumbs.Add( new BreadCrumb( IsAddMode() ? "New Enrollment" : enrollment.PersonAlias.Person.FullName, pageReference ) );

            return breadCrumbs;
        }

        /// <summary>
        /// Initialize the action buttons that affect the entire record.
        /// </summary>
        private void InitializeActionButtons()
        {
            btnRebuild.Attributes["onclick"] = "javascript: return Rock.dialogs.confirmDelete(event, 'data', 'Enrollment map data belonging to this person for this streak type will be deleted and rebuilt from attendance records! This process occurs real-time (not in a job).');";
            btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}');", Streak.FriendlyTypeName );
        }

        /// <summary>
        /// Initialize handlers for block configuration changes.
        /// </summary>
        /// <param name="triggerPanel"></param>
        private void InitializeSettingsNotification()
        {
            BlockUpdated += Block_BlockUpdated;
            AddConfigurationUpdateTrigger( upEnrollmentDetail );
        }

        #endregion

        #region Events

        /// <summary>
        /// Navigate to a linked page
        /// </summary>
        /// <param name="attributeKey"></param>
        private void NavigateToLinkedPage( string attributeKey )
        {
            var streak = GetStreak();

            if ( streak == null )
            {
                ShowBlockError( nbEditModeMessage, "A streak is required" );
                return;
            }

            NavigateToLinkedPage( attributeKey, new Dictionary<string, string> {
                { PageParameterKey.StreakId, streak.Id.ToString() }
            } );
        }

        /// <summary>
        /// The click event for the rebuild button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnRebuild_Click( object sender, EventArgs e )
        {
            RebuildData();
        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            hfIsEditMode.Value = CanEdit() ? "true" : string.Empty;
            RenderState();
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            DeleteRecord();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            SaveRecord();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( IsAddMode() )
            {
                NavigateToParentPage();
            }
            else
            {
                hfIsEditMode.Value = string.Empty;
                RenderState();
            }
        }

        /// <summary>
        /// Go to the parent page and use the streak type id in the params
        /// </summary>
        /// <returns></returns>
        private bool NavigateToParentPage()
        {
            return NavigateToParentPage( new Dictionary<string, string> {
                { PageParameterKey.StreakTypeId, GetStreakType().Id.ToString() }
            } );
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            RenderState();
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

        /// <summary>
        /// Show a validation error
        /// </summary>
        /// <param name="message"></param>
        private void ShowValidationError( string message )
        {
            nbEditModeMessage.Text = string.Format( "Please correct the following:<ul><li>{0}</li></ul>", message );
            nbEditModeMessage.NotificationBoxType = NotificationBoxType.Validation;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="notificationControl"></param>
        /// <param name="message"></param>
        private void ShowBlockError( NotificationBox notificationControl, string message )
        {
            ShowBlockNotification( notificationControl, message, NotificationBoxType.Danger );
        }

        /// <summary>
        /// Show a block exception
        /// </summary>
        /// <param name="notificationControl"></param>
        /// <param name="ex"></param>
        /// <param name="writeToLog"></param>
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
        /// Rebuild the enrollment data
        /// </summary>
        private void RebuildData()
        {
            var rockContext = GetRockContext();
            var streak = GetStreak();

            if ( !streak.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
            {
                mdDeleteWarning.Show( "You are not authorized to rebuild this item.", ModalAlertType.Information );
                return;
            }

            var errorMessage = string.Empty;
            StreakTypeService.RebuildStreakFromAttendance( streak.StreakTypeId, streak.PersonAlias.PersonId, out errorMessage );

            if ( !errorMessage.IsNullOrWhiteSpace() )
            {
                ShowBlockError( nbEditModeMessage, errorMessage );
                return;
            }

            ShowBlockSuccess( nbEditModeMessage, "The streak rebuild was successful!" );
        }

        /// <summary>
        /// Delete the current record.
        /// </summary>
        private void DeleteRecord()
        {
            var rockContext = GetRockContext();
            var enrollment = GetStreak();

            if ( enrollment != null )
            {
                if ( !enrollment.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
                {
                    mdDeleteWarning.Show( "You are not authorized to delete this item.", ModalAlertType.Information );
                    return;
                }

                var service = GetStreakService();
                var errorMessage = string.Empty;

                service.Delete( enrollment );
                rockContext.SaveChanges();
            }

            NavigateToParentPage();
        }

        /// <summary>
        /// Save the current record.
        /// </summary>
        /// <returns></returns>
        private void SaveRecord()
        {
            // Validate the streak type            
            var streakType = GetStreakType();

            if ( streakType == null )
            {
                nbEditModeMessage.Text = "Streak Type is required.";
                return;
            }

            // Get the other non-required values
            var streakTypeService = GetStreakTypeService();
            var streakTypeCache = StreakTypeCache.Get( streakType.Id );
            var enrollment = GetStreak();
            var enrollmentDate = rdpEnrollmentDate.SelectedDate;
            var locationId = rlpLocation.Location != null ? rlpLocation.Location.Id : ( int? ) null;

            // Add the new enrollment if we are adding
            if ( enrollment == null )
            {
                // Validate the person
                var personId = rppPerson.PersonId;
                var personAliasId = rppPerson.PersonAliasId;

                if ( !personId.HasValue || !personAliasId.HasValue )
                {
                    nbEditModeMessage.Text = "Person is required.";
                    return;
                }

                var errorMessage = string.Empty;
                enrollment = streakTypeService.Enroll( streakTypeCache, personId.Value, out errorMessage, enrollmentDate, locationId );

                if ( !errorMessage.IsNullOrWhiteSpace() )
                {
                    ShowValidationError( errorMessage );
                    return;
                }

                if ( enrollment == null )
                {
                    nbEditModeMessage.Text = "Enrollment failed but no error was specified.";
                    return;
                }
            }
            else
            {
                enrollment.LocationId = locationId;
                enrollment.EnrollmentDate = enrollmentDate ?? RockDateTime.Today;
            }

            if ( !enrollment.IsValid )
            {
                var validationResult = enrollment.ValidationResults.FirstOrDefault();
                var message = validationResult == null ? "The values entered are not valid." : validationResult.ErrorMessage;
                nbEditModeMessage.Text = message;
                return;
            }

            try
            {
                var rockContext = GetRockContext();
                rockContext.SaveChanges();

                if ( !enrollment.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    enrollment.AllowPerson( Authorization.VIEW, CurrentPerson, rockContext );
                }

                if ( !enrollment.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                {
                    enrollment.AllowPerson( Authorization.EDIT, CurrentPerson, rockContext );
                }

                if ( !enrollment.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
                {
                    enrollment.AllowPerson( Authorization.ADMINISTRATE, CurrentPerson, rockContext );
                }

            }
            catch ( Exception ex )
            {
                ShowBlockException( nbEditModeMessage, ex );
                return;
            }

            // If the save was successful, reload the page using the new record Id.
            NavigateToPage( RockPage.Guid, new Dictionary<string, string> {
                { PageParameterKey.StreakTypeId, streakType.Id.ToString() },
                { PageParameterKey.StreakId, enrollment.Id.ToString() }
            } );
        }

        /// <summary>
        /// Called by a related block to show the detail for a specific entity.
        /// </summary>
        /// <param name="unused"></param>
        public void ShowDetail( int unused )
        {
            RenderState();
        }

        /// <summary>
        /// Shows the controls needed
        /// </summary>
        public void RenderState()
        {
            nbEditModeMessage.Text = string.Empty;

            if ( IsAddMode() )
            {
                ShowAddMode();
            }
            else if ( IsEditMode() )
            {
                ShowEditMode();
            }
            else if ( IsViewMode() )
            {
                ShowViewMode();
            }
            else
            {
                nbEditModeMessage.Text = "The page parameters are not valid";
            }
        }

        /// <summary>
        /// Shows the mode where the user can edit an existing streak type
        /// </summary>
        private void ShowEditMode()
        {
            if ( !IsEditMode() )
            {
                return;
            }

            pnlEditDetails.Visible = true;
            pnlViewDetails.Visible = false;
            HideSecondaryBlocks( true );
            pdAuditDetails.Visible = true;

            var enrollment = GetStreak();
            lReadOnlyTitle.Text = ActionTitle.Edit( Streak.FriendlyTypeName ).FormatAsHtmlTitle();

            rppPerson.Visible = false;
            divPerson.Visible = true;
            lPersonName.Text = enrollment.PersonAlias.Person.FullName;

            rdpEnrollmentDate.SelectedDate = enrollment.EnrollmentDate;
            rlpLocation.Location = enrollment.Location;
        }

        /// <summary>
        /// Show the mode where a user can add a new streak type
        /// </summary>
        private void ShowAddMode()
        {
            if ( !IsAddMode() )
            {
                return;
            }

            pnlEditDetails.Visible = true;
            pnlViewDetails.Visible = false;
            HideSecondaryBlocks( true );
            pdAuditDetails.Visible = false;
            divPerson.Visible = false;

            lReadOnlyTitle.Text = ActionTitle.Add( Streak.FriendlyTypeName ).FormatAsHtmlTitle();

            rdpEnrollmentDate.SelectedDate = RockDateTime.Today;

            var presetPersonId = PageParameter( PageParameterKey.PersonId ).AsIntegerOrNull();

            if ( presetPersonId.HasValue )
            {
                var streakService = GetStreakService();
                var streakType = GetStreakType();
                var enrollments = streakService.GetByStreakTypeAndPerson( streakType.Id, presetPersonId.Value );

                if ( enrollments.Any() )
                {
                    NavigateToCurrentPage( new Dictionary<string, string> {
                        { PageParameterKey.StreakId, enrollments.First().Id.ToString() }
                    } );
                }
                else
                {
                    var personService = new PersonService( GetRockContext() );
                    var presetPerson = personService.Get( presetPersonId.Value );

                    if ( presetPerson != null )
                    {
                        rppPerson.SetValue( presetPerson );
                    }
                }
            }
        }

        /// <summary>
        /// Shows the mode where the user is only viewing an existing streak type
        /// </summary>
        private void ShowViewMode()
        {
            if ( !IsViewMode() )
            {
                return;
            }

            var canEdit = CanEdit();

            pnlEditDetails.Visible = false;
            pnlViewDetails.Visible = true;
            HideSecondaryBlocks( false );
            pdAuditDetails.Visible = canEdit;

            btnEdit.Visible = canEdit;
            btnDelete.Visible = canEdit;

            var enrollment = GetStreak();
            var streakType = GetStreakType();
            lReadOnlyTitle.Text = ActionTitle.View( Streak.FriendlyTypeName ).FormatAsHtmlTitle();
            btnRebuild.Enabled = streakType.IsActive;

            lPersonHtml.Text = GetPersonHtml();

            var descriptionList = new DescriptionList();
            descriptionList.Add( "Streak Type", streakType.Name );
            descriptionList.Add( "Enrollment Date", enrollment.EnrollmentDate.ToShortDateString() );

            if ( enrollment.Location != null )
            {
                descriptionList.Add( "Location", enrollment.Location.Name );
            }

            lEnrollmentDescription.Text = descriptionList.Html;

            var streakDetailsList = new DescriptionList();

            streakDetailsList.Add( "Current Streak", GetStreakStateString( enrollment.CurrentStreakCount, enrollment.CurrentStreakStartDate ) );
            streakDetailsList.Add( "Longest Streak", GetStreakStateString( enrollment.LongestStreakCount, enrollment.LongestStreakStartDate, enrollment.LongestStreakEndDate ) );

            lStreakData.Text = streakDetailsList.Html;

            RenderStreakChart();
        }

        /// <summary>
        /// Set the visibility of a button depending on if an attribute is set
        /// </summary>
        /// <param name="button"></param>
        /// <param name="attributeKey"></param>
        private void SetLinkVisibility( LinkButton button, string attributeKey )
        {
            var hasValue = !GetAttributeValue( attributeKey ).IsNullOrWhiteSpace();
            button.Visible = hasValue;
        }

        /// <summary>
        /// Gets the person HTML.
        /// </summary>
        /// <returns></returns>
        private string GetPersonHtml()
        {
            var personImageStringBuilder = new StringBuilder();
            var person = GetPerson();
            const string photoFormat = "<div class=\"photo-icon photo-round photo-round-sm pull-left margin-r-sm js-person-popover\" personid=\"{0}\" data-original=\"{1}&w=50\" style=\"background-image: url( '{2}' ); background-size: cover; background-repeat: no-repeat;\"></div>";
            const string nameLinkFormat = @"
    {0}
    <p><small><a href='/Person/{1}'>View Profile</a></small></p>
";

            personImageStringBuilder.AppendFormat( photoFormat, person.Id, person.PhotoUrl, ResolveUrl( "~/Assets/Images/person-no-photo-unknown.svg" ) );
            personImageStringBuilder.AppendFormat( nameLinkFormat, person.FullName, person.Id );

            if ( person.TopSignalColor.IsNotNullOrWhiteSpace() )
            {
                personImageStringBuilder.Append( person.GetSignalMarkup() );
            }

            return personImageStringBuilder.ToString();
        }

        /// <summary>
        /// Gets the streak state string.
        /// </summary>
        /// <param name="streakCount">The streak count.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns></returns>
        private string GetStreakStateString( int streakCount, DateTime? start, DateTime? end = null )
        {
            var dateString = GetStreakDateRangeString( start, end );

            if ( dateString.IsNullOrWhiteSpace() )
            {
                return streakCount.ToString();
            }

            return string.Format( "{0} <small>{1}</small>", streakCount, dateString );
        }

        /// <summary>
        /// Gets the streak date range string.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns></returns>
        private string GetStreakDateRangeString( DateTime? start, DateTime? end = null )
        {
            if ( !start.HasValue && !end.HasValue )
            {
                return string.Empty;
            }

            if ( !start.HasValue )
            {
                return string.Format( "Ended on {0}", end.ToShortDateString() );
            }

            if ( !end.HasValue )
            {
                return string.Format( "Started on {0}", start.ToShortDateString() );
            }

            return string.Format( "Ranging from {0} - {1}", start.ToShortDateString(), end.ToShortDateString() );
        }

        /// <summary>
        /// Render the streak chart
        /// </summary>
        private void RenderStreakChart()
        {
            var occurrenceEngagement = GetOccurrenceEngagement();

            if ( occurrenceEngagement == null )
            {
                return;
            }

            var stringBuilder = new StringBuilder();
            var bitItemFormat = @"<li class=""binary-state-graph-bit {2} {3}"" title=""{0}""><span style=""height: {1}%""></span></li>";

            for ( var i = 0; i < occurrenceEngagement.Length; i++ )
            {
                var occurrence = occurrenceEngagement[i];
                var hasEngagement = occurrence != null && occurrence.HasEngagement;
                var hasExclusion = occurrence != null && occurrence.HasExclusion;
                var title = occurrence != null ? occurrence.DateTime.ToShortDateString() : string.Empty;

                stringBuilder.AppendFormat( bitItemFormat,
                    title, // 0
                    hasEngagement ? 100 : 5, // 1
                    hasEngagement ? "has-engagement" : string.Empty, // 2
                    hasExclusion ? "has-exclusion" : string.Empty ); // 3
            }

            lStreakChart.Text = stringBuilder.ToString();
        }

        #endregion Internal Methods

        #region State Determining Methods

        /// <summary>
        /// Can the user edit the enrollment
        /// </summary>
        /// <returns></returns>
        private bool CanEdit()
        {
            return UserCanAdministrate && GetStreak() != null;
        }

        /// <summary>
        /// Can the user add a new enrollment
        /// </summary>
        /// <returns></returns>
        private bool CanAdd()
        {
            return UserCanAdministrate && GetStreak() == null;
        }

        /// <summary>
        /// Is this block currently adding a new enrollment
        /// </summary>
        /// <returns></returns>
        private bool IsAddMode()
        {
            return CanAdd();
        }

        /// <summary>
        /// Is this block currently editing an existing enrollment
        /// </summary>
        /// <returns></returns>
        private bool IsEditMode()
        {
            return CanEdit() && hfIsEditMode.Value.Trim().ToLower() == "true";
        }

        /// <summary>
        /// Is the block currently showing information about an enrollment
        /// </summary>
        /// <returns></returns>
        private bool IsViewMode()
        {
            return GetStreak() != null && hfIsEditMode.Value.Trim().ToLower() != "true";
        }

        #endregion State Determining Methods

        #region Data Interface Methods

        /// <summary>
        /// Get the actual streak type model for deleting or editing
        /// </summary>
        /// <returns></returns>
        private StreakType GetStreakType()
        {
            if ( _streakType == null )
            {
                var streak = GetStreak();

                if ( streak != null && streak.StreakType != null )
                {
                    _streakType = streak.StreakType;
                }
                else if ( streak != null )
                {
                    var streakTypeService = GetStreakTypeService();
                    _streakType = streakTypeService.Get( streak.StreakTypeId );
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
                    var service = GetStreakService();
                    _streak = service.Queryable( "PersonAlias" ).FirstOrDefault( s => s.Id == streakId.Value );
                }
            }

            return _streak;
        }
        private Streak _streak = null;

        /// <summary>
        /// Get the streak models for the person
        /// </summary>
        /// <returns></returns>
        private List<Streak> GetPersonStreaks()
        {
            if ( _streaks == null )
            {
                var streak = GetStreak();

                if ( streak != null )
                {
                    var service = GetStreakService();
                    _streaks = service.GetByStreakTypeAndPerson( streak.StreakTypeId, streak.PersonAlias.PersonId ).ToList();
                }
            }

            return _streaks;
        }
        private List<Streak> _streaks = null;

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
        /// Get the recent bits data for the chart
        /// </summary>
        /// <returns></returns>
        private OccurrenceEngagement[] GetOccurrenceEngagement()
        {
            if ( _occurrenceEngagement == null )
            {
                var streakTypeService = GetStreakTypeService();
                var streakType = GetStreakType();
                var person = GetPerson();

                if ( person != null && streakType != null )
                {
                    var errorMessage = string.Empty;
                    _occurrenceEngagement = streakTypeService.GetRecentEngagementBits( streakType.Id, person.Id, ChartBitsToShow, out errorMessage );
                }
            }

            return _occurrenceEngagement;
        }
        private OccurrenceEngagement[] _occurrenceEngagement = null;

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
        /// Get the person alias service
        /// </summary>
        /// <returns></returns>
        private PersonAliasService GetPersonAliasService()
        {
            if ( _personAliasService == null )
            {
                var rockContext = GetRockContext();
                _personAliasService = new PersonAliasService( rockContext );
            }

            return _personAliasService;
        }
        private PersonAliasService _personAliasService = null;

        /// <summary>
        /// Get the person alias service
        /// </summary>
        /// <returns></returns>
        private Person GetPerson()
        {
            if ( _person == null )
            {
                var enrollment = GetStreak();

                if ( enrollment != null )
                {
                    var service = GetPersonAliasService();
                    _person = service.GetPerson( enrollment.PersonAliasId );
                }
            }

            return _person;
        }
        private Person _person = null;

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

        #endregion Data Interface Methods        
    }
}