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
    [DisplayName( "Sequence Enrollment Detail" )]
    [Category( "Sequences" )]
    [Description( "Displays the details of the given Enrollment for editing." )]

    public partial class SequenceEnrollmentDetail : RockBlock, IDetailBlock
    {
        #region Keys

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
                var enrollment = GetSequenceEnrollment();
                pdAuditDetails.SetEntity( enrollment, ResolveRockUrl( "~" ) );

                var sequence = GetSequence();

                if ( sequence == null )
                {
                    nbEditModeMessage.Text = "A sequence is required.";
                    return;
                }

                RenderState();
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

            var enrollment = GetSequenceEnrollment();
            breadCrumbs.Add( new BreadCrumb( IsAddMode() ? "New Enrollment" : enrollment.PersonAlias.Person.FullName, pageReference ) );

            return breadCrumbs;
        }

        /// <summary>
        /// Initialize the action buttons that affect the entire record.
        /// </summary>
        private void InitializeActionButtons()
        {
            btnRebuild.Attributes["onclick"] = "javascript: return Rock.dialogs.confirmDelete(event, 'data', 'Enrollment map data belonging to this person for this sequence will be deleted and rebuilt from attendance records! This process occurs real-time (not in a job).');";
            btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}', 'Are you sure?');", Sequence.FriendlyTypeName );
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
        /// Click event for the enrollment buttons
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void rbgEnrollmentLinks_SelectedIndexChanged( object sender, EventArgs e )
        {
            var enrollmentId = rbgEnrollmentLinks.SelectedValue.AsIntegerOrNull();

            if ( enrollmentId.HasValue )
            {
                NavigateToCurrentPage( new Dictionary<string, string> {
                    { PageParameterKey.SequenceEnrollmentId, enrollmentId.Value.ToString() }
                } );
            }
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
        /// Go to the parent page and use the sequence id in the params
        /// </summary>
        /// <returns></returns>
        private bool NavigateToParentPage()
        {
            return NavigateToParentPage( new Dictionary<string, string> {
                { PageParameterKey.SequenceId, GetSequence().Id.ToString() }
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
        /// Rebuild the enrollment data
        /// </summary>
        private void RebuildData()
        {
            var rockContext = GetRockContext();
            var enrollment = GetSequenceEnrollment();

            if ( !enrollment.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
            {
                mdDeleteWarning.Show( "You are not authorized to rebuild this item.", ModalAlertType.Information );
                return;
            }

            var errorMessage = string.Empty;
            SequenceService.RebuildEnrollmentFromAttendance( enrollment.SequenceId, enrollment.PersonAliasId, out errorMessage );

            if ( !errorMessage.IsNullOrWhiteSpace() )
            {
                ShowBlockError( nbEditModeMessage, errorMessage );
                return;
            }

            ShowBlockSuccess( nbEditModeMessage, "The enrollment rebuild was successful!" );
        }

        /// <summary>
        /// Delete the current record.
        /// </summary>
        private void DeleteRecord()
        {
            var enrollment = GetSequenceEnrollment();

            if ( enrollment != null )
            {
                if ( !enrollment.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
                {
                    mdDeleteWarning.Show( "You are not authorized to delete this item.", ModalAlertType.Information );
                    return;
                }

                var service = GetSequenceEnrollmentService();
                var errorMessage = string.Empty;

                if ( !service.CanDelete( enrollment, out errorMessage ) )
                {
                    mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                service.Delete( enrollment );
                GetRockContext().SaveChanges();
            }

            NavigateToParentPage();
        }

        /// <summary>
        /// Save the current record.
        /// </summary>
        /// <returns></returns>
        private void SaveRecord()
        {
            // Validate the sequence            
            var sequence = GetSequence();

            if ( sequence == null )
            {
                nbEditModeMessage.Text = "Sequence is required.";
                return;
            }

            // Validate the person
            var personId = rppPerson.PersonId;
            var personAliasId = rppPerson.PersonAliasId;

            if ( !personId.HasValue || !personAliasId.HasValue )
            {
                nbEditModeMessage.Text = "Person is required.";
                return;
            }

            // Get the other non-required values
            var sequenceService = GetSequenceService();
            var sequenceCache = SequenceCache.Get( sequence.Id );
            var enrollment = GetSequenceEnrollment();
            var enrollmentDate = rdpEnrollmentDate.SelectedDate;
            var locationId = rlpLocation.Location != null ? rlpLocation.Location.Id : ( int? ) null;

            // Add the new enrollment if we are adding
            if ( enrollment == null )
            {
                var errorMessage = string.Empty;
                enrollment = sequenceService.Enroll( sequenceCache, personId.Value, out errorMessage, enrollmentDate, locationId );

                if ( !errorMessage.IsNullOrWhiteSpace() )
                {
                    nbEditModeMessage.Text = errorMessage;
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
            }

            if ( !enrollment.IsValid )
            {
                // Controls will render the error messages
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
                { PageParameterKey.SequenceId, sequence.Id.ToString() },
                { PageParameterKey.SequenceEnrollmentId, enrollment.Id.ToString() }
            } );
        }

        /// <summary>
        /// This method satisfies the IDetailBlock requirement
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
        /// Shows the mode where the user can edit an existing sequence
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

            var enrollment = GetSequenceEnrollment();
            lReadOnlyTitle.Text = ActionTitle.Edit( SequenceEnrollment.FriendlyTypeName ).FormatAsHtmlTitle();

            rppPerson.SetValue( enrollment.PersonAlias.Person );
            rppPerson.Enabled = false;

            rdpEnrollmentDate.SelectedDate = enrollment.EnrollmentDate;
            rdpEnrollmentDate.Enabled = false;

            rlpLocation.Location = enrollment.Location;
        }

        /// <summary>
        /// Show the mode where a user can add a new sequence
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

            lReadOnlyTitle.Text = ActionTitle.Add( SequenceEnrollment.FriendlyTypeName ).FormatAsHtmlTitle();

            rdpEnrollmentDate.SelectedDate = RockDateTime.Today;

            var presetPersonId = PageParameter( PageParameterKey.PersonId ).AsIntegerOrNull();

            if ( presetPersonId.HasValue )
            {
                var enrollmentService = GetSequenceEnrollmentService();
                var sequence = GetSequence();
                var enrollments = enrollmentService.GetBySequenceAndPerson( sequence.Id, presetPersonId.Value );

                if ( enrollments.Any() )
                {
                    NavigateToCurrentPage( new Dictionary<string, string> {
                        { PageParameterKey.SequenceEnrollmentId, enrollments.First().Id.ToString() }
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
        /// Shows the mode where the user is only viewing an existing sequence
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

            var enrollment = GetSequenceEnrollment();
            var sequence = GetSequence();
            lReadOnlyTitle.Text = ActionTitle.View( SequenceEnrollment.FriendlyTypeName ).FormatAsHtmlTitle();
            btnRebuild.Enabled = sequence.IsActive;

            var descriptionList = new DescriptionList();
            descriptionList.Add( "Sequence", sequence.Name );
            descriptionList.Add( "Person", enrollment.PersonAlias.Person.FullName );
            descriptionList.Add( "Enrollment Date", enrollment.EnrollmentDate.ToShortDateString() );

            if ( enrollment.Location != null )
            {
                descriptionList.Add( "Location", enrollment.Location.Name );
            }

            lEnrollmentDescription.Text = descriptionList.Html;

            var streakData = GetSequenceEnrollmentData();
            var streakDetailsList = new DescriptionList();

            if ( streakData != null )
            {
                if ( streakData.EnrollmentCount > 1 )
                {
                    var enrollments = GetPersonSequenceEnrollments();

                    if ( enrollments != null && enrollments.Count > 1 )
                    {
                        rbgEnrollmentLinks.Visible = true;
                        rbgEnrollmentLinks.DataSource = enrollments.OrderBy( e => e.Id ).Select( e => new ListItem
                        {
                            Text = e.Id.ToString(),
                            Value = e.Id.ToString()
                        } );
                        rbgEnrollmentLinks.DataBind();
                        rbgEnrollmentLinks.SelectedValue = enrollment.Id.ToString();
                    }
                    else
                    {
                        rbgEnrollmentLinks.Visible = false;
                    }

                    streakDetailsList.Add( "First Enrollment Date", streakData.FirstEnrollmentDate.ToShortDateString() );
                }
                else
                {
                    rbgEnrollmentLinks.Visible = false;
                    h5Left.Visible = false;
                    h5Right.Visible = false;
                }

                streakDetailsList.Add( "Current Streak", streakData.CurrentStreakCount.ToString() );
                streakDetailsList.Add( "Current Streak Start", streakData.CurrentStreakStartDate.ToShortDateString() );
                streakDetailsList.Add( "Longest Streak", streakData.LongestStreakCount.ToString() );

                if ( streakData.LongestStreakStartDate.HasValue && streakData.LongestStreakEndDate.HasValue )
                {
                    streakDetailsList.Add( "Longest Streak Range", string.Format( "{0} - {1}",
                        streakData.LongestStreakStartDate.ToShortDateString(),
                        streakData.LongestStreakEndDate.ToShortDateString() ) );
                }
            }

            lStreakData.Text = streakDetailsList.Html;

            RenderStreakChart();
        }

        /// <summary>
        /// Render the streak chart
        /// </summary>
        private void RenderStreakChart()
        {
            var streakData = GetSequenceEnrollmentData();

            if ( streakData == null || streakData.PerFrequencyUnit == null )
            {
                return;
            }

            var stringBuilder = new StringBuilder();
            var bitsToShow = 250;
            var bitItemFormat = @"<li title=""{0}""><span style=""height: {1}%""></span></li>";
            var bitsRendered = 0;

            while ( bitsRendered < bitsToShow )
            {
                var currentBitIndex = streakData.PerFrequencyUnit.Count - 1 - bitsRendered;
                var currentBit = currentBitIndex >= 0 ? streakData.PerFrequencyUnit[currentBitIndex] : null;
                var title = currentBit == null ? string.Empty : currentBit.DateTime.ToShortDateString();
                var bitIsSet = currentBit == null ? false : currentBit.HasEngagement;
                stringBuilder.AppendFormat( bitItemFormat, title, bitIsSet ? 100 : 5 );

                bitsRendered++;
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
            return UserCanAdministrate && GetSequenceEnrollment() != null;
        }

        /// <summary>
        /// Can the user add a new enrollment
        /// </summary>
        /// <returns></returns>
        private bool CanAdd()
        {
            return UserCanAdministrate && GetSequenceEnrollment() == null;
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
            return GetSequenceEnrollment() != null && hfIsEditMode.Value.Trim().ToLower() != "true";
        }

        #endregion State Determining Methods

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

                if ( enrollment != null && enrollment.Sequence != null )
                {
                    _sequence = enrollment.Sequence;
                }
                else if ( enrollment != null )
                {
                    var sequenceService = GetSequenceService();
                    _sequence = sequenceService.Get( enrollment.SequenceId );
                }
                else
                {
                    var sequenceId = PageParameter( PageParameterKey.SequenceId ).AsIntegerOrNull();

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
        /// Get the actual sequence enrollment model for deleting or editing
        /// </summary>
        /// <returns></returns>
        private SequenceEnrollment GetSequenceEnrollment()
        {
            if ( _sequenceEnrollment == null )
            {
                var sequenceEnrollmentId = PageParameter( PageParameterKey.SequenceEnrollmentId ).AsIntegerOrNull();

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
        /// Get the sequence enrollment models for the person
        /// </summary>
        /// <returns></returns>
        private List<SequenceEnrollment> GetPersonSequenceEnrollments()
        {
            if ( _sequenceEnrollments == null )
            {
                var enrollment = GetSequenceEnrollment();

                if ( enrollment != null )
                {
                    var service = GetSequenceEnrollmentService();
                    _sequenceEnrollments = service.GetBySequenceAndPerson( enrollment.SequenceId, enrollment.PersonAlias.PersonId ).ToList();
                }
            }

            return _sequenceEnrollments;
        }
        private List<SequenceEnrollment> _sequenceEnrollments = null;

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
        /// Get the streak data for the enrollment
        /// </summary>
        /// <returns></returns>
        private SequenceStreakData GetSequenceEnrollmentData()
        {
            if ( _sequenceEnrollmentData == null )
            {
                var sequence = GetSequence();
                var person = GetPerson();

                if ( sequence != null && person != null )
                {
                    var service = GetSequenceService();
                    var sequenceCache = SequenceCache.Get( sequence.Id );
                    var errorMessage = string.Empty;
                    _sequenceEnrollmentData = service.GetSequenceStreakData( sequenceCache, person.Id, out errorMessage, createObjectArray: true );
                }
            }

            return _sequenceEnrollmentData;
        }
        private SequenceStreakData _sequenceEnrollmentData = null;

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
                var enrollment = GetSequenceEnrollment();

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

        #endregion Data Interface Methods        
    }
}