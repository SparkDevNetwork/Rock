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
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Streaks
{
    [DisplayName( "Attempt Detail" )]
    [Category( "Streaks" )]
    [Description( "Displays the details of the given attempt for editing." )]

    [LinkedPage(
        "Streak Page",
        Description = "Page used for viewing the streak that these attempts are derived from.",
        Key = AttributeKey.StreakPage,
        IsRequired = false,
        Order = 1 )]

    [LinkedPage(
        "Achievement Type Page",
        Description = "Page used for viewing the achievement type that this attempt is toward.",
        Key = AttributeKey.AchievementPage,
        IsRequired = false,
        Order = 2 )]

    public partial class AchievementAttemptDetail : RockBlock, IDetailBlock
    {
        #region Keys

        /// <summary>
        /// Keys to use for Attributes
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The streak page
            /// </summary>
            public const string StreakPage = "StreakPage";

            /// <summary>
            /// The achievement page
            /// </summary>
            public const string AchievementPage = "AchievementPage";
        }

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            /// <summary>
            /// The streak type achievement type identifier
            /// </summary>
            public const string StreakTypeAchievementTypeId = "StreakTypeAchievementTypeId";

            /// <summary>
            /// The streak id page parameter key
            /// </summary>
            public const string StreakId = "StreakId";

            /// <summary>
            /// The streak achievement attempt identifier
            /// </summary>
            public const string StreakAchievementAttemptId = "StreakAchievementAttemptId";
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
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
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
            var attempt = GetAttempt();
            var text = "New Attempt";

            if ( attempt != null )
            {
                var achievementType = GetAchievementTypeCache();
                var person = GetPerson();
                var streakPageIsLinked = !GetAttributeValue( AttributeKey.StreakPage ).IsNullOrWhiteSpace();

                if ( streakPageIsLinked && person != null )
                {
                    text = person.FullName;
                }
                else if ( achievementType != null )
                {
                    text = achievementType.Name;
                }
                else
                {
                    text = attempt.AchievementAttemptStartDateTime.ToShortDateString();
                }
            }

            breadCrumbs.Add( new BreadCrumb( text, pageReference ) );
            return breadCrumbs;
        }

        /// <summary>
        /// Initialize the action buttons that affect the entire record.
        /// </summary>
        private void InitializeActionButtons()
        {
            btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}', 'Are you sure?');", StreakAchievementAttempt.FriendlyTypeName );
        }

        /// <summary>
        /// Initialize handlers for block configuration changes.
        /// </summary>
        /// <param name="triggerPanel"></param>
        private void InitializeSettingsNotification()
        {
            BlockUpdated += Block_BlockUpdated;
            AddConfigurationUpdateTrigger( upAttemptDetail );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnStreak control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnStreak_Click( object sender, EventArgs e )
        {
            var attempt = GetAttempt();

            if ( attempt == null )
            {
                ShowBlockError( nbEditModeMessage, "An attempt is required" );
                return;
            }

            NavigateToLinkedPage( AttributeKey.StreakPage, new Dictionary<string, string> {
                { PageParameterKey.StreakId, attempt.StreakId.ToString() }
            } );
        }

        /// <summary>
        /// Handles the Click event of the btnAchievement control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnAchievement_Click( object sender, EventArgs e )
        {
            var attempt = GetAttempt();

            if ( attempt == null )
            {
                ShowBlockError( nbEditModeMessage, "An attempt is required" );
                return;
            }

            NavigateToLinkedPage( AttributeKey.AchievementPage, new Dictionary<string, string> {
                { PageParameterKey.StreakTypeAchievementTypeId, attempt.StreakTypeAchievementTypeId.ToString() }
            } );
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

        /// <summary>
        /// Shows the block success.
        /// </summary>
        /// <param name="notificationControl">The notification control.</param>
        /// <param name="message">The message.</param>
        private void ShowBlockSuccess( NotificationBox notificationControl, string message )
        {
            ShowBlockNotification( notificationControl, message, NotificationBoxType.Success );
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Delete the current record.
        /// </summary>
        private void DeleteRecord()
        {
            var rockContext = GetRockContext();
            var attempt = GetAttempt();
            var parameters = new Dictionary<string, string>();

            if ( attempt != null )
            {
                if ( !attempt.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
                {
                    mdDeleteWarning.Show( "You are not authorized to delete this item.", ModalAlertType.Information );
                    return;
                }

                parameters[PageParameterKey.StreakTypeAchievementTypeId] = attempt.StreakTypeAchievementTypeId.ToString();
                parameters[PageParameterKey.StreakId] = attempt.StreakId.ToString();

                var service = GetAttemptService();
                service.Delete( attempt );
                rockContext.SaveChanges();
            }

            NavigateToParentPage( parameters );
        }

        /// <summary>
        /// Save the current record.
        /// </summary>
        /// <returns></returns>
        private void SaveRecord()
        {
            var attempt = GetAttempt();
            var achievementType = GetAchievementTypeCache();

            // Add the new attempt if we are adding
            if ( attempt == null )
            {
                var attemptService = GetAttemptService();
                var streak = GetOrAddStreak();
                var achievementTypeId = achievementType == null ? atpAchievementType.SelectedValue.AsInteger() : achievementType.Id;

                attempt = new StreakAchievementAttempt
                {
                    StreakTypeAchievementTypeId = achievementTypeId,
                    StreakId = streak.Id
                };

                attemptService.Add( attempt );
            }

            var progress = tbProgress.Text.AsDecimal();

            if ( attempt.Progress < 0m )
            {
                attempt.Progress = 0m;
            }

            if ( attempt.Progress > 1m && !achievementType.AllowOverAchievement )
            {
                attempt.Progress = 1m;
            }

            var isSuccess = progress >= 1m;

            var startDate = dpStart.SelectedDate ?? RockDateTime.Today;
            var endDate = dpEnd.SelectedDate;

            if ( !endDate.HasValue && isSuccess && !achievementType.AllowOverAchievement )
            {
                endDate = RockDateTime.Today;
            }

            if ( endDate.HasValue && endDate < startDate )
            {
                endDate = startDate;
            }

            attempt.IsClosed = ( endDate.HasValue && endDate.Value < RockDateTime.Today ) || ( isSuccess && !achievementType.AllowOverAchievement );
            attempt.AchievementAttemptStartDateTime = startDate;
            attempt.AchievementAttemptEndDateTime = endDate;
            attempt.Progress = progress;
            attempt.IsSuccessful = isSuccess;

            if ( !attempt.IsValid )
            {
                // Controls will render the error messages
                return;
            }

            try
            {
                var rockContext = GetRockContext();
                rockContext.SaveChanges();

                if ( !attempt.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    attempt.AllowPerson( Authorization.VIEW, CurrentPerson, rockContext );
                }

                if ( !attempt.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                {
                    attempt.AllowPerson( Authorization.EDIT, CurrentPerson, rockContext );
                }

                if ( !attempt.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
                {
                    attempt.AllowPerson( Authorization.ADMINISTRATE, CurrentPerson, rockContext );
                }
            }
            catch ( Exception ex )
            {
                ShowBlockException( nbEditModeMessage, ex );
                return;
            }

            // If the save was successful, reload the page using the new record Id.
            NavigateToPage( RockPage.Guid, new Dictionary<string, string> {
                { PageParameterKey.StreakAchievementAttemptId, attempt.Id.ToString() }
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
            SetIcon();
            SetTitle();

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

            divPerson.Visible = false;
            divAchievement.Visible = false;
            pnlEditDetails.Visible = true;
            pnlViewDetails.Visible = false;
            HideSecondaryBlocks( true );

            var attempt = GetAttempt();

            dpStart.SelectedDate = attempt.AchievementAttemptStartDateTime;
            dpEnd.SelectedDate = attempt.AchievementAttemptEndDateTime;
            tbProgress.Text = attempt.Progress.ToStringSafe();

            SetLinkVisibility( btnAchievement, AttributeKey.AchievementPage );
            SetLinkVisibility( btnStreak, AttributeKey.StreakPage );
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

            divPerson.Visible = GetStreak() == null;
            divAchievement.Visible = GetAchievementTypeCache() == null;
            pnlEditDetails.Visible = true;
            pnlViewDetails.Visible = false;
            HideSecondaryBlocks( true );

            dpStart.SelectedDate = RockDateTime.Today;
            tbProgress.Text = 0m.ToString();

            btnAchievement.Visible = false;
            btnStreak.Visible = false;
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

            btnEdit.Visible = canEdit;
            btnDelete.Visible = canEdit;

            lPersonHtml.Text = GetPersonHtml();
            lProgress.Text = GetProgressHtml();

            var descriptionList = new DescriptionList();
            descriptionList.Add( "Date", GetAttemptDateRangeString() );
            lAttemptDescription.Text = descriptionList.Html;

            SetLinkVisibility( btnAchievement, AttributeKey.AchievementPage );
            SetLinkVisibility( btnStreak, AttributeKey.StreakPage );
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

            personImageStringBuilder.AppendFormat( photoFormat, person.Id, person.PhotoUrl, ResolveUrl( "~/Assets/Images/person-no-photo-unknown.svg" ) );
            personImageStringBuilder.Append( person.FullName );

            if ( person.TopSignalColor.IsNotNullOrWhiteSpace() )
            {
                personImageStringBuilder.Append( person.GetSignalMarkup() );
            }

            return personImageStringBuilder.ToString();
        }

        /// <summary>
        /// Gets the attempt date range string.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns></returns>
        private string GetAttemptDateRangeString()
        {
            var attempt = GetAttempt();

            if ( attempt == null )
            {
                return string.Empty;
            }

            var start = attempt.AchievementAttemptStartDateTime;
            var end = attempt.AchievementAttemptEndDateTime;

            if ( !end.HasValue )
            {
                return string.Format( "Started on {0}", start.ToShortDateString() );
            }

            return string.Format( "Ranging from {0} - {1}", start.ToShortDateString(), end.ToShortDateString() );
        }

        private string GetProgressHtml()
        {
            var attempt = GetAttempt();

            if ( attempt == null )
            {
                return string.Empty;
            }

            var progressInt = Convert.ToInt32( decimal.Round( attempt.Progress * 100 ) );
            var progressBarWidth = progressInt < 0 ? 0 : ( progressInt > 100 ? 100 : progressInt );
            var insideProgress = progressInt >= 50 ? progressInt : ( int? ) null;
            var outsideProgress = progressInt < 50 ? progressInt : ( int? ) null;
            var progressBarClass = progressInt >= 100 ? "progress-bar-success" : string.Empty;

            return string.Format(
@"<div class=""progress"">
    <div class=""progress-bar {5}"" role=""progressbar"" style=""width: {0}%;"">
        {1}{2}
    </div>
    <span style=""padding-left: 5px;"">{3}{4}</span>
</div>", progressBarWidth, insideProgress, insideProgress.HasValue ? "%" : string.Empty, outsideProgress, outsideProgress.HasValue ? "%" : string.Empty, progressBarClass );
        }

        /// <summary>
        /// Sets the title.
        /// </summary>
        private void SetTitle()
        {
            var achievementType = GetAchievementTypeCache();
            var title = "Achievement";

            if ( achievementType != null && !achievementType.Name.IsNullOrWhiteSpace() )
            {
                title = achievementType.Name;
            }

            lTitle.Text = string.Format( "{0} Attempt", title );
        }

        /// <summary>
        /// Sets the icon.
        /// </summary>
        private void SetIcon()
        {
            var achievementType = GetAchievementTypeCache();
            var iconClass = "fa fa-medal";

            if ( achievementType != null && !achievementType.AchievementIconCssClass.IsNullOrWhiteSpace() )
            {
                iconClass = achievementType.AchievementIconCssClass;
            }

            lIcon.Text = string.Format( @"<i class=""{0}""></i>", iconClass );
        }

        #endregion Internal Methods

        #region State Determining Methods

        /// <summary>
        /// Can the user edit the enrollment
        /// </summary>
        /// <returns></returns>
        private bool CanEdit()
        {
            return UserCanAdministrate && GetAttempt() != null;
        }

        /// <summary>
        /// Can the user add a new enrollment
        /// </summary>
        /// <returns></returns>
        private bool CanAdd()
        {
            return UserCanAdministrate && GetAttempt() == null;
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
            return GetAttempt() != null && hfIsEditMode.Value.Trim().ToLower() != "true";
        }

        #endregion State Determining Methods

        #region Data Interface Methods

        /// <summary>
        /// Gets the type of the achievement.
        /// </summary>
        /// <returns></returns>
        private StreakTypeAchievementTypeCache GetAchievementTypeCache()
        {
            if ( _streakTypeAchievementTypeCache != null )
            {
                return _streakTypeAchievementTypeCache;
            }

            var attempt = GetAttempt();
            var achievementTypeId = PageParameter( PageParameterKey.StreakTypeAchievementTypeId ).AsIntegerOrNull();

            if ( attempt != null )
            {
                achievementTypeId = attempt.StreakTypeAchievementTypeId;
            }

            if ( achievementTypeId.HasValue && achievementTypeId.Value > 0 )
            {
                _streakTypeAchievementTypeCache = StreakTypeAchievementTypeCache.Get( achievementTypeId.Value );
            }

            return _streakTypeAchievementTypeCache;
        }
        private StreakTypeAchievementTypeCache _streakTypeAchievementTypeCache = null;

        /// <summary>
        /// Gets the attempt.
        /// </summary>
        /// <returns></returns>
        private StreakAchievementAttempt GetAttempt()
        {
            if ( _attempt == null )
            {
                var attemptId = PageParameter( PageParameterKey.StreakAchievementAttemptId ).AsIntegerOrNull();

                if ( attemptId.HasValue && attemptId.Value > 0 )
                {
                    var service = GetAttemptService();
                    _attempt = service.Queryable( "Streak.PersonAlias.Person" ).FirstOrDefault( saa => saa.Id == attemptId.Value );
                }
            }

            return _attempt;
        }
        private StreakAchievementAttempt _attempt = null;

        /// <summary>
        /// Get the actual streak model for deleting or editing
        /// </summary>
        /// <returns></returns>
        private Streak GetStreak()
        {
            if ( _streak != null )
            {
                return _streak;
            }

            var attempt = GetAttempt();

            if ( attempt != null && attempt.Streak != null )
            {
                _streak = attempt.Streak;
                return _streak;
            }

            var streakId = PageParameter( PageParameterKey.StreakId ).AsIntegerOrNull();

            if ( attempt != null )
            {
                streakId = attempt.StreakId;
            }

            if ( streakId.HasValue && streakId.Value > 0 )
            {
                var service = GetStreakService();
                _streak = service.Queryable( "PersonAlias.Person" ).FirstOrDefault( s => s.Id == streakId.Value );
            }

            return _streak;
        }
        private Streak _streak = null;

        /// <summary>
        /// Gets the or add streak.
        /// </summary>
        /// <returns></returns>
        private Streak GetOrAddStreak()
        {
            var streak = GetStreak();

            if ( streak != null )
            {
                return streak;
            }

            var achievementType = GetAchievementTypeCache();
            var streakTypeId = achievementType.StreakTypeId;
            var personId = ppPerson.PersonId ?? 0;

            var streakService = GetStreakService();
            streak = streakService.GetByStreakTypeAndPerson( streakTypeId, personId ).FirstOrDefault();

            if ( streak == null )
            {
                var streakTypeService = GetStreakTypeService();
                var errorMessage = string.Empty;
                streak = streakTypeService.Enroll( achievementType.StreakTypeCache, personId, out errorMessage );
            }

            return streak;
        }

        /// <summary>
        /// Gets the person.
        /// </summary>
        /// <returns></returns>
        private Person GetPerson()
        {
            if ( _person != null )
            {
                return _person;
            }

            var streak = GetStreak();

            if ( streak != null && streak.PersonAlias != null && streak.PersonAlias.Person != null )
            {
                _person = streak.PersonAlias.Person;
                return _person;
            }

            if ( streak != null )
            {
                var service = GetPersonAliasService();
                _person = service.GetPerson( streak.PersonAliasId );
            }

            return _person;
        }
        private Person _person = null;

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

        #endregion Data Interface Methods
    }
}