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
    [Category( "Achievements" )]
    [Description( "Displays the details of the given attempt for editing." )]

    [LinkedPage(
        "Achievement Type Page",
        Description = "Page used for viewing the achievement type that this attempt is toward.",
        Key = AttributeKey.AchievementPage,
        IsRequired = false,
        Order = 2 )]

    [Rock.SystemGuid.BlockTypeGuid( "7E4663CD-2176-48D6-9CC2-2DBC9B880C23" )]
    public partial class AchievementAttemptDetail : RockBlock
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
            /// The achievement type identifier
            /// </summary>
            public const string AchievementTypeId = "AchievementTypeId";

            /// <summary>
            /// The streak id page parameter key
            /// </summary>
            public const string StreakId = "StreakId";

            /// <summary>
            /// The streak achievement attempt identifier
            /// </summary>
            public const string AchievementAttemptId = "AchievementAttemptId";
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
                RenderState();
            }

            base.OnLoad( e );
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
                text = attempt.AchievementAttemptStartDateTime.ToShortDateString();
            }

            breadCrumbs.Add( new BreadCrumb( text, pageReference ) );
            return breadCrumbs;
        }

        /// <summary>
        /// Initialize the action buttons that affect the entire record.
        /// </summary>
        private void InitializeActionButtons()
        {
            btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}');", AchievementAttempt.FriendlyTypeName );
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
                { PageParameterKey.AchievementTypeId, attempt.AchievementTypeId.ToString() }
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

                parameters[PageParameterKey.AchievementTypeId] = attempt.AchievementTypeId.ToString();

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
                var achievementTypeId = achievementType == null ? atpAchievementType.SelectedValue.AsInteger() : achievementType.Id;

                attempt = new AchievementAttempt
                {
                    AchievementTypeId = achievementTypeId,
                    AchieverEntityId = nbAchieverEntityId.IntegerValue ?? 0
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
                { PageParameterKey.AchievementAttemptId, attempt.Id.ToString() }
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
        /// Shows the mode where the user can edit an existing item
        /// </summary>
        private void ShowEditMode()
        {
            if ( !IsEditMode() )
            {
                return;
            }

            divAchiever.Visible = false;
            divAchievement.Visible = false;
            pnlEditDetails.Visible = true;
            pnlViewDetails.Visible = false;
            HideSecondaryBlocks( true );

            var attempt = GetAttempt();

            dpStart.SelectedDate = attempt.AchievementAttemptStartDateTime;
            dpEnd.SelectedDate = attempt.AchievementAttemptEndDateTime;
            tbProgress.Text = attempt.Progress.ToStringSafe();

            SetLinkVisibility( btnAchievement, AttributeKey.AchievementPage );
        }

        /// <summary>
        /// Show the mode where a user can add a new item
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

            dpStart.SelectedDate = RockDateTime.Today;
            tbProgress.Text = 0m.ToString();

            btnAchievement.Visible = false;
        }

        /// <summary>
        /// Shows the mode where the user is only viewing an existing item
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

            var achiever = GetAchiever();
            lAchiever.Text = GetPersonHtml( achiever );
            lProgress.Text = GetProgressHtml();

            var descriptionList = new DescriptionList();
            descriptionList.Add( "Date", GetAttemptDateRangeString() );
            lAttemptDescription.Text = descriptionList.Html;

            var achievementTypeCache = GetAchievementTypeCache();

            if ( achievementTypeCache != null )
            {
                SetLinkVisibility( btnAchievement, AttributeKey.AchievementPage );
            }
            else
            {
                btnAchievement.Visible = false;
            }
        }

        /// <summary>
        /// Gets the person HTML.
        /// </summary>
        /// <returns></returns>
        private string GetPersonHtml( IEntity achiever )
        {
            var personImageStringBuilder = new StringBuilder();
            const string photoFormat = "<div class=\"photo-icon photo-round photo-round-sm pull-left margin-r-sm js-person-popover\" personid=\"{0}\" data-original=\"{1}&w=50\" style=\"background-image: url( '{2}' ); background-size: cover; background-repeat: no-repeat;\"></div>";
            const string nameLinkFormat = @"
    {0}
    <p><small><a href='/Person/{1}'>View Profile</a></small></p>
";

            if ( achiever is PersonAlias personAlias )
            {
                personImageStringBuilder.AppendFormat( photoFormat, personAlias.PersonId, personAlias.Person.PhotoUrl, ResolveUrl( "~/Assets/Images/person-no-photo-unknown.svg" ) );
                personImageStringBuilder.AppendFormat( nameLinkFormat, personAlias.Person.FullName, personAlias.PersonId );
            }
            else
            {
                personImageStringBuilder.AppendFormat( photoFormat, null, null, ResolveUrl( "~/Assets/Images/person-no-photo-unknown.svg" ) );
                personImageStringBuilder.Append( "Unknown" );
            }

            return personImageStringBuilder.ToString();
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

            var progressLong = Convert.ToInt64( decimal.Round( attempt.Progress * 100 ) );
            var progressBarWidth = progressLong < 0 ? 0 : ( progressLong > 100 ? 100 : progressLong );
            var insideProgress = progressLong >= 50 ? progressLong : ( long? ) null;
            var outsideProgress = progressLong < 50 ? progressLong : ( long? ) null;
            var progressBarClass = progressLong >= 100 ? "progress-bar-success" : string.Empty;

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
        private AchievementTypeCache GetAchievementTypeCache()
        {
            if ( _achievementTypeCache != null )
            {
                return _achievementTypeCache;
            }

            var attempt = GetAttempt();
            var achievementTypeId = PageParameter( PageParameterKey.AchievementTypeId ).AsIntegerOrNull();

            if ( attempt != null )
            {
                achievementTypeId = attempt.AchievementTypeId;
            }

            if ( achievementTypeId.HasValue && achievementTypeId.Value > 0 )
            {
                _achievementTypeCache = AchievementTypeCache.Get( achievementTypeId.Value );
            }

            return _achievementTypeCache;
        }
        private AchievementTypeCache _achievementTypeCache = null;

        /// <summary>
        /// Gets the attempt.
        /// </summary>
        /// <returns></returns>
        private AchievementAttempt GetAttempt()
        {
            if ( _attempt == null )
            {
                var attemptId = PageParameter( PageParameterKey.AchievementAttemptId ).AsIntegerOrNull();

                if ( attemptId.HasValue && attemptId.Value > 0 )
                {
                    var service = GetAttemptService();
                    _attempt = service.Queryable().FirstOrDefault( saa => saa.Id == attemptId.Value );
                }
            }

            return _attempt;
        }
        private AchievementAttempt _attempt = null;

        /// <summary>
        /// Gets the achiever.
        /// </summary>
        /// <returns></returns>
        private IEntity GetAchiever()
        {
            if ( _achiever != null )
            {
                return _achiever;
            }

            var attempt = GetAttempt();
            var achievementTypeCache = GetAchievementTypeCache();

            if ( attempt != null && achievementTypeCache != null )
            {
                var service = GetEntityTypeService();
                _achiever = service.GetEntity( achievementTypeCache.AchieverEntityTypeId, attempt.AchieverEntityId );
            }

            return _achiever;
        }
        private IEntity _achiever = null;

        /// <summary>
        /// Get the entity type service
        /// </summary>
        /// <returns></returns>
        private EntityTypeService GetEntityTypeService()
        {
            if ( _entityTypeService == null )
            {
                var rockContext = GetRockContext();
                _entityTypeService = new EntityTypeService( rockContext );
            }

            return _entityTypeService;
        }
        private EntityTypeService _entityTypeService = null;

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