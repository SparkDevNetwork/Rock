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
    [DisplayName( "Streak Type Exclusion Detail" )]
    [Category( "Streaks" )]
    [Description( "Displays the details of the given Exclusion for editing." )]

    [Rock.SystemGuid.BlockTypeGuid( "21E9D4D3-9111-4E2F-A605-C4556BD62430" )]
    public partial class StreakTypeExclusionDetail : RockBlock
    {
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
            /// The streak type exclusion id page parameter key
            /// </summary>
            public const string StreakTypeExclusionId = "StreakTypeExclusionId";
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
            if ( !Page.IsPostBack )
            {
                var exclusion = GetExclusion();
                pdAuditDetails.SetEntity( exclusion, ResolveRockUrl( "~" ) );

                var streakType = GetStreakType();

                if ( streakType == null )
                {
                    nbEditModeMessage.Text = "A streak type is required.";
                    base.OnLoad( e );
                    return;
                }

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

            var exclusion = GetExclusion();
            var locationName = string.Empty;

            if ( IsAddMode() )
            {
                locationName = "New Exclusion";
            }
            else if ( exclusion.Location != null )
            {
                locationName = exclusion.Location.Name;
            }
            else
            {
                locationName = "Unspecified Location";
            }

            breadCrumbs.Add( new BreadCrumb( locationName, pageReference ) );
            return breadCrumbs;
        }

        /// <summary>
        /// Initialize the action buttons that affect the entire record.
        /// </summary>
        private void InitializeActionButtons()
        {
            btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}');", StreakTypeExclusion.FriendlyTypeName );
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
        /// Delete the current record.
        /// </summary>
        private void DeleteRecord()
        {
            var exclusion = GetExclusion();

            if ( exclusion != null )
            {
                if ( !exclusion.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
                {
                    mdDeleteWarning.Show( "You are not authorized to delete this item.", ModalAlertType.Information );
                    return;
                }

                var service = GetStreakTypeExclusionService();
                var errorMessage = string.Empty;

                if ( !service.CanDelete( exclusion, out errorMessage ) )
                {
                    mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                service.Delete( exclusion );
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
            // Validate the streak type            
            var streakType = GetStreakType();

            if ( streakType == null )
            {
                nbEditModeMessage.Text = "Streak Type is required.";
                return;
            }

            // Get the other non-required values
            var streakTypeService = GetStreakTypeService();
            var exclusionService = GetStreakTypeExclusionService();
            var exclusion = GetExclusion();
            var locationId = rlpLocation.Location != null ? rlpLocation.Location.Id : ( int? ) null;

            // Add the new exclusion if we are adding
            if ( exclusion == null )
            {
                exclusion = new StreakTypeExclusion
                {
                    LocationId = locationId,
                    StreakTypeId = streakType.Id
                };

                exclusionService.Add( exclusion );
            }
            else
            {
                exclusion.LocationId = locationId;
            }

            if ( !exclusion.IsValid )
            {
                // Controls will render the error messages
                return;
            }

            try
            {
                var rockContext = GetRockContext();
                rockContext.SaveChanges();

                if ( !exclusion.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    exclusion.AllowPerson( Authorization.VIEW, CurrentPerson, rockContext );
                }

                if ( !exclusion.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                {
                    exclusion.AllowPerson( Authorization.EDIT, CurrentPerson, rockContext );
                }

                if ( !exclusion.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
                {
                    exclusion.AllowPerson( Authorization.ADMINISTRATE, CurrentPerson, rockContext );
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
                { PageParameterKey.StreakTypeExclusionId, exclusion.Id.ToString() }
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
        /// Shows the mode where the user can edit an existing exclusion
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

            var exclusion = GetExclusion();
            lReadOnlyTitle.Text = ActionTitle.Edit( StreakTypeExclusion.FriendlyTypeName ).FormatAsHtmlTitle();
            rlpLocation.Location = exclusion.Location;
        }

        /// <summary>
        /// Show the mode where a user can add a new exclusion
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

            var exclusion = GetExclusion();
            lReadOnlyTitle.Text = ActionTitle.Add( StreakTypeExclusion.FriendlyTypeName ).FormatAsHtmlTitle();
        }

        /// <summary>
        /// Shows the mode where the user is only viewing an existing exclusion
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

            var exclusion = GetExclusion();
            var streakType = GetStreakType();
            lReadOnlyTitle.Text = ActionTitle.View( StreakTypeExclusion.FriendlyTypeName ).FormatAsHtmlTitle();
            var locationName = exclusion.Location != null ? exclusion.Location.Name : "Unspecified";

            var descriptionList = new DescriptionList();
            descriptionList.Add( "Streak Type", streakType.Name );
            descriptionList.Add( "Location", locationName );

            lExclusionDescription.Text = descriptionList.Html;
        }

        #endregion Internal Methods

        #region State Determining Methods

        /// <summary>
        /// Can the user edit the enrollment
        /// </summary>
        /// <returns></returns>
        private bool CanEdit()
        {
            return UserCanAdministrate && GetExclusion() != null;
        }

        /// <summary>
        /// Can the user add a new enrollment
        /// </summary>
        /// <returns></returns>
        private bool CanAdd()
        {
            return UserCanAdministrate && GetExclusion() == null;
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
            return GetExclusion() != null && hfIsEditMode.Value.Trim().ToLower() != "true";
        }

        #endregion State Determining Methods

        #region Data Interface Methods

        /// <summary>
        /// Get the actual enrollment model for deleting or editing
        /// </summary>
        /// <returns></returns>
        private StreakType GetStreakType()
        {
            if ( _streakType == null )
            {
                var exclusion = GetExclusion();

                if ( exclusion != null && exclusion.StreakType != null )
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
        /// Get the actual streak exclusion model for deleting or editing
        /// </summary>
        /// <returns></returns>
        private StreakTypeExclusion GetExclusion()
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
        /// Get the streak exclusion service
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