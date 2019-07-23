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

namespace RockWeb.Blocks.Sequences
{
    [DisplayName( "Sequence Exclusion Detail" )]
    [Category( "Sequences" )]
    [Description( "Displays the details of the given Exclusion for editing." )]

    public partial class SequenceExclusionDetail : RockBlock, IDetailBlock
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
            /// The sequence exclusion id page parameter key
            /// </summary>
            public const string SequenceExclusionId = "SequenceOccurrenceExclusionId";
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
                var exclusion = GetExclusion();
                pdAuditDetails.SetEntity( exclusion, ResolveRockUrl( "~" ) );

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
            btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}');", SequenceOccurrenceExclusion.FriendlyTypeName );
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

                var service = GetExclusionService();
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
            // Validate the sequence            
            var sequence = GetSequence();

            if ( sequence == null )
            {
                nbEditModeMessage.Text = "Sequence is required.";
                return;
            }

            // Get the other non-required values
            var sequenceService = GetSequenceService();
            var exclusionService = GetExclusionService();
            var exclusion = GetExclusion();
            var locationId = rlpLocation.Location != null ? rlpLocation.Location.Id : ( int? ) null;

            // Add the new exclusion if we are adding
            if ( exclusion == null )
            {
                exclusion = new SequenceOccurrenceExclusion
                {
                    LocationId = locationId,
                    SequenceId = sequence.Id
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
                { PageParameterKey.SequenceId, sequence.Id.ToString() },
                { PageParameterKey.SequenceExclusionId, exclusion.Id.ToString() }
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

            var exclusion = GetExclusion();
            lReadOnlyTitle.Text = ActionTitle.Edit( SequenceOccurrenceExclusion.FriendlyTypeName ).FormatAsHtmlTitle();
            rlpLocation.Location = exclusion.Location;
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

            var exclusion = GetExclusion();
            lReadOnlyTitle.Text = ActionTitle.Add( SequenceEnrollment.FriendlyTypeName ).FormatAsHtmlTitle();
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

            var exclusion = GetExclusion();
            var sequence = GetSequence();
            lReadOnlyTitle.Text = ActionTitle.View( SequenceOccurrenceExclusion.FriendlyTypeName ).FormatAsHtmlTitle();
            var locationName = exclusion.Location != null ? exclusion.Location.Name : "Unspecified";

            var descriptionList = new DescriptionList();
            descriptionList.Add( "Sequence", sequence.Name );
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
        private Sequence GetSequence()
        {
            if ( _sequence == null )
            {
                var exclusion = GetExclusion();

                if ( exclusion != null && exclusion.Sequence != null )
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
        /// Get the actual sequence exclusion model for deleting or editing
        /// </summary>
        /// <returns></returns>
        private SequenceOccurrenceExclusion GetExclusion()
        {
            if ( _sequenceOccurrenceExclusion == null )
            {
                var id = PageParameter( PageParameterKey.SequenceExclusionId ).AsIntegerOrNull();

                if ( id.HasValue && id.Value > 0 )
                {
                    var service = GetExclusionService();
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
        /// Get the sequence exclusion service
        /// </summary>
        /// <returns></returns>
        private SequenceOccurrenceExclusionService GetExclusionService()
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