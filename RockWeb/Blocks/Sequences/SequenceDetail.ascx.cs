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
    [DisplayName( "Sequence Detail" )]
    [Category( "Sequences" )]
    [Description( "Displays the details of the given Sequence for editing." )]

    [LinkedPage(
        "Map Editor Page",
        Description = "Page used for editing the sequence map.",
        Key = AttributeKey.MapEditorPage,
        IsRequired = false,
        Order = 1 )]

    [LinkedPage(
        "Exclusions Page",
        Description = "Page used for viewing a list of sequence exclusions.",
        Key = AttributeKey.ExclusionsPage,
        IsRequired = false,
        Order = 2 )]

    public partial class SequenceDetail : RockBlock, IDetailBlock
    {
        #region Keys

        /// <summary>
        /// Keys to use for Attributes
        /// </summary>
        protected static class AttributeKey
        {
            /// <summary>
            /// Key for the Map Editor Page
            /// </summary>
            public const string MapEditorPage = "MapEditorPage";

            /// <summary>
            /// Key for the Exclusions Page
            /// </summary>
            public const string ExclusionsPage = "ExclusionsPage";
        }

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        protected static class PageParameterKey
        {
            /// <summary>
            /// Key for the Sequence Id
            /// </summary>
            public const string SequenceId = "SequenceId";

            /// <summary>
            /// Key for the sequence enrollment id
            /// </summary>
            public const string SequenceEnrollmentId = "SequenceEnrollmentId";
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
                var sequenceEntity = GetSequence();
                pdAuditDetails.SetEntity( sequenceEntity, ResolveRockUrl( "~" ) );

                BindDropDownLists();
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
            var sequence = GetSequence();
            breadCrumbs.Add( new BreadCrumb( IsAddMode() ? "New Sequence" : sequence.Name, pageReference ) );
            return breadCrumbs;
        }

        /// <summary>
        /// Initialize the action buttons that affect the entire record.
        /// </summary>
        private void InitializeActionButtons()
        {
            btnRebuild.Attributes["onclick"] = "javascript: return Rock.dialogs.confirmDelete(event, 'data', 'Occurrence and enrollment map data belonging to this sequence will be deleted and rebuilt from attendance records! This process runs in a job and may take several minutes to complete.');";
            btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}', 'All associated Enrollments and Exclusions will also be deleted!');", Sequence.FriendlyTypeName );
            btnSecurity.EntityTypeId = EntityTypeCache.Get( typeof( Sequence ) ).Id;
        }

        /// <summary>
        /// Initialize handlers for block configuration changes.
        /// </summary>
        /// <param name="triggerPanel"></param>
        private void InitializeSettingsNotification()
        {
            BlockUpdated += Block_BlockUpdated;
            AddConfigurationUpdateTrigger( upSequenceDetail );
        }

        #endregion

        #region Events

        /// <summary>
        /// Button to go to the map editor page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnMapEditor_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.MapEditorPage );
        }

        /// <summary>
        /// Button to go to the exclusions page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnExclusions_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.ExclusionsPage );
        }

        /// <summary>
        /// Navigate to a linked page
        /// </summary>
        /// <param name="attributeKey"></param>
        private void NavigateToLinkedPage( string attributeKey )
        {
            var sequence = GetSequence();

            if ( sequence == null )
            {
                ShowBlockError( nbEditModeMessage, "A sequence is required" );
                return;
            }

            NavigateToLinkedPage( attributeKey, new Dictionary<string, string> {
                { PageParameterKey.SequenceId, sequence.Id.ToString() }
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
        /// The change event of the attendance structure selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ddlStructureType_SelectedIndexChanged( object sender, EventArgs e )
        {
            RenderAttendanceStructureControls();
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
        /// Rebuild the sequence and enrollment data
        /// </summary>
        private void RebuildData()
        {
            var rockContext = GetRockContext();
            var sequence = GetSequence();

            if ( !sequence.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
            {
                mdDeleteWarning.Show( "You are not authorized to rebuild this item.", ModalAlertType.Information );
                return;
            }

            var job = new ServiceJobService( rockContext ).Get( Rock.SystemGuid.ServiceJob.REBUILD_SEQUENCE.AsGuid() );

            if ( job == null )
            {
                ShowBlockError( nbEditModeMessage, "The sequence rebuild job could not be found." );
            }

            var jobData = new Dictionary<string, string> {
                { Rock.Jobs.RebuildSequenceMaps.DataMapKeys.SequenceId, sequence.Id.ToString() }
            };

            var transaction = new Rock.Transactions.RunJobNowTransaction( job.Id, jobData );
            System.Threading.Tasks.Task.Run( () => transaction.Execute() );
            ShowBlockSuccess( nbEditModeMessage, "The sequence rebuild has been started. Check the Rock Jobs page for the status." );
        }

        /// <summary>
        /// Delete the current record.
        /// </summary>
        private void DeleteRecord()
        {
            var sequence = GetSequence();

            if ( sequence != null )
            {
                if ( !sequence.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
                {
                    mdDeleteWarning.Show( "You are not authorized to delete this item.", ModalAlertType.Information );
                    return;
                }

                var service = GetSequenceService();
                var errorMessage = string.Empty;

                if ( !service.CanDelete( sequence, out errorMessage ) )
                {
                    mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                service.Delete( sequence );
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
            var sequence = GetSequence();
            var service = GetSequenceService();
            var isNew = sequence == null;

            if ( isNew )
            {
                sequence = new Sequence();
                service.Add( sequence );
            }

            // Can only set the start date if the sequence is new
            if ( isNew )
            {
                var frequencySelected = GetEnumSelected<SequenceOccurrenceFrequency>( ddlFrequencyOccurrence ) ?? SequenceOccurrenceFrequency.Daily;
                var isDaily = frequencySelected == SequenceOccurrenceFrequency.Daily;
                sequence.OccurrenceFrequency = frequencySelected;

                var selectedDate = rdpStartDate.SelectedDate ?? RockDateTime.Today;
                sequence.StartDate = isDaily ? selectedDate : selectedDate.SundayDate();                
            }

            sequence.Name = tbName.Text;
            sequence.IsActive = cbActive.Checked;
            sequence.Description = tbDescription.Text;
            sequence.EnableAttendance = cbEnableAttendance.Checked;
            sequence.RequiresEnrollment = cbRequireEnrollment.Checked;            
            sequence.StructureType = GetEnumSelected<SequenceStructureType>( ddlStructureType );            
            sequence.StructureEntityId = GetStructureEntityIdSelected();

            if ( !sequence.IsValid )
            {
                // Controls will render the error messages
                return;
            }

            try
            {
                var rockContext = GetRockContext();
                rockContext.SaveChanges();

                if ( !sequence.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    sequence.AllowPerson( Authorization.VIEW, CurrentPerson, rockContext );
                }

                if ( !sequence.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                {
                    sequence.AllowPerson( Authorization.EDIT, CurrentPerson, rockContext );
                }

                if ( !sequence.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
                {
                    sequence.AllowPerson( Authorization.ADMINISTRATE, CurrentPerson, rockContext );
                }

            }
            catch ( Exception ex )
            {
                ShowBlockException( nbEditModeMessage, ex );
                return;
            }

            // If the save was successful, reload the page using the new record Id.
            NavigateToPage( RockPage.Guid, new Dictionary<string, string> {
                { PageParameterKey.SequenceId, sequence.Id.ToString() }
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
                nbEditModeMessage.Text = "The sequence was not found";
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

            rdpStartDate.Enabled = false; // Cannot change the start date
            ddlFrequencyOccurrence.Enabled = false; // Cannot change the frequency

            var sequence = GetSequence();
            var structureType = sequence.StructureType;
            var structureEntityId = sequence.StructureEntityId;

            lReadOnlyTitle.Text = sequence.Name.FormatAsHtmlTitle();
            hlInactive.Visible = !sequence.IsActive;

            tbName.Text = sequence.Name;
            cbActive.Checked = sequence.IsActive;
            tbDescription.Text = sequence.Description;
            cbEnableAttendance.Checked = sequence.EnableAttendance;
            cbRequireEnrollment.Checked = sequence.RequiresEnrollment;
            rdpStartDate.SelectedDate = sequence.StartDate;
            ddlFrequencyOccurrence.SelectedValue = sequence.OccurrenceFrequency.ToString();
            ddlStructureType.SelectedValue = structureType.HasValue ? structureType.Value.ToString() : string.Empty;

            RenderAttendanceStructureControls();
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
            rdpStartDate.Enabled = true; // Can only set the start date when adding a new sequence
            ddlFrequencyOccurrence.Enabled = true; // Can only set the frequency when adding a new sequence

            rdpStartDate.SelectedDate = RockDateTime.Today;

            lReadOnlyTitle.Text = ActionTitle.Add( Sequence.FriendlyTypeName ).FormatAsHtmlTitle();
            hlInactive.Visible = false;
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
            btnSecurity.Visible = canEdit;

            var sequence = GetSequence();
            lReadOnlyTitle.Text = sequence.Name.FormatAsHtmlTitle();
            hlInactive.Visible = !sequence.IsActive;
            btnRebuild.Enabled = sequence.IsActive;

            var descriptionList = new DescriptionList();
            descriptionList.Add( "Description", sequence.Description );
            descriptionList.Add( "Frequency", sequence.OccurrenceFrequency.ConvertToString() );
            descriptionList.Add( "Start Date", sequence.StartDate.ToShortDateString() );
            descriptionList.Add( "Requires Enrollment", sequence.RequiresEnrollment.ToYesNo() );
            descriptionList.Add( "Attendance Enabled", sequence.EnableAttendance.ToYesNo() );

            if ( sequence.StructureType.HasValue )
            {
                var structureName = GetSequenceStructureName();
                var structureString = string.Format( "{0}{1}",
                    sequence.StructureType.Value.ConvertToString(),
                    string.Format( "{0}{1}",
                        structureName.IsNullOrWhiteSpace() ? string.Empty : " - ",
                        structureName
                    ) );

                descriptionList.Add( "Attendance Structure", structureString );
            }

            lSequenceDescription.Text = descriptionList.Html;

            if ( canEdit )
            {
                btnSecurity.Title = "Secure " + sequence.Name;
                btnSecurity.EntityId = sequence.Id;
            }

            SetLinkVisibility( btnExclusions, AttributeKey.ExclusionsPage );
            SetLinkVisibility( btnMapEditor, AttributeKey.MapEditorPage );
        }

        /// <summary>
        /// Bind the options to the drop down lists
        /// </summary>
        private void BindDropDownLists()
        {
            BindDropDownListToEnum( typeof( SequenceOccurrenceFrequency ), ddlFrequencyOccurrence, false );
            BindDropDownListToEnum( typeof( SequenceStructureType ), ddlStructureType, true );
        }

        /// <summary>
        /// Take an enum type and bind it's options to the drop down list
        /// </summary>
        /// <param name="enumType"></param>
        /// <param name="ddl"></param>
        private void BindDropDownListToEnum( Type enumType, DataDropDownList ddl, bool includeBlank )
        {
            if ( includeBlank )
            {
                ddl.Items.Add( new ListItem() );
            }

            var itemValues = Enum.GetValues( enumType );
            var itemNames = Enum.GetNames( enumType );

            for ( var i = 0; i < itemNames.Length; i++ )
            {
                ddl.Items.Add( new ListItem( itemNames[i].SplitCase(), itemValues.GetValue( i ).ToString() ) );
            }
        }

        /// <summary>
        /// Get the enum value from the drop down list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ddl"></param>
        /// <returns></returns>
        private T? GetEnumSelected<T>( DataDropDownList ddl ) where T : struct
        {
            var asString = ddl.SelectedValue;
            T asEnum;
            var success = Enum.TryParse( asString, out asEnum );
            return success ? asEnum : ( T? ) null;
        }

        /// <summary>
        /// Render the appropriate controls for the selected attendance structure type
        /// </summary>
        private int? GetStructureEntityIdSelected()
        {
            var selectedStructureType = GetEnumSelected<SequenceStructureType>( ddlStructureType );

            if ( !selectedStructureType.HasValue )
            {
                return null;
            }

            switch ( selectedStructureType.Value )
            {
                case SequenceStructureType.CheckInConfig:
                case SequenceStructureType.GroupType:
                    return gtpStructureGroupTypePicker.SelectedGroupTypeId;
                case SequenceStructureType.Group:
                    return gpStructureGroupPicker.GroupId;
                case SequenceStructureType.GroupTypePurpose:
                    return dvpStructureGroupTypePurposePicker.SelectedDefinedValueId;
                default:
                    throw new NotImplementedException( "The structure type is not implemented" );
            }
        }

        /// <summary>
        /// Render the appropriate controls for the selected attendance structure type
        /// </summary>
        private void RenderAttendanceStructureControls()
        {
            var isAddMode = IsAddMode();

            if ( !IsEditMode() && !isAddMode )
            {
                return;
            }

            gpStructureGroupPicker.Visible = false;
            dvpStructureGroupTypePurposePicker.Visible = false;
            gtpStructureGroupTypePicker.Visible = false;

            var sequence = isAddMode ? null : GetSequence();
            var originalStructureType = isAddMode ? null : sequence.StructureType;
            var selectedStructureType = GetEnumSelected<SequenceStructureType>( ddlStructureType );
            var originalEntityId = isAddMode ? null : sequence.StructureEntityId;
            var structureEntityId = originalStructureType == selectedStructureType ? originalEntityId : null;

            if ( !selectedStructureType.HasValue )
            {
                return;
            }

            switch ( selectedStructureType.Value )
            {
                case SequenceStructureType.CheckInConfig:
                    RenderCheckinConfigControl( structureEntityId );
                    break;
                case SequenceStructureType.Group:
                    RenderGroupControl( structureEntityId );
                    break;
                case SequenceStructureType.GroupType:
                    RenderGroupTypeControl( structureEntityId );
                    break;
                case SequenceStructureType.GroupTypePurpose:
                    RenderGroupTypePurposeControl( structureEntityId );
                    break;
                default:
                    throw new NotImplementedException( "The structure type is not implemented" );
            }
        }

        /// <summary>
        /// Render control for attendance structure of check in config
        /// </summary>
        private void RenderCheckinConfigControl( int? structureEntityId )
        {
            var groupTypeService = GetGroupTypeService();
            var checkInPurposeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE ).Id;

            var groupTypes = groupTypeService.Queryable().AsNoTracking()
                .Where( gt => gt.GroupTypePurposeValueId == checkInPurposeId )
                .OrderBy( gt => gt.Name )
                .ToList();

            gtpStructureGroupTypePicker.Label = "Check-In Configuration";
            gtpStructureGroupTypePicker.GroupTypes = groupTypes;
            gtpStructureGroupTypePicker.SelectedGroupTypeId = structureEntityId;
            gtpStructureGroupTypePicker.Visible = true;
        }

        /// <summary>
        /// Render control for attendance structure of group
        /// </summary>
        private void RenderGroupControl( int? structureEntityId )
        {
            gpStructureGroupPicker.Label = "Group";
            gpStructureGroupPicker.GroupId = structureEntityId;
            gpStructureGroupPicker.Visible = true;
        }

        /// <summary>
        /// Render control for attendance structure of group type
        /// </summary>
        private void RenderGroupTypeControl( int? structureEntityId )
        {
            var groupTypeService = GetGroupTypeService();

            var groupTypes = groupTypeService.Queryable().AsNoTracking()
                .OrderBy( gt => gt.Name )
                .ToList();

            gtpStructureGroupTypePicker.Label = "Group Type";
            gtpStructureGroupTypePicker.GroupTypes = groupTypes;
            gtpStructureGroupTypePicker.SelectedGroupTypeId = structureEntityId;
            gtpStructureGroupTypePicker.Visible = true;
        }

        /// <summary>
        /// Render control for attendance structure of group type purpose
        /// </summary>
        private void RenderGroupTypePurposeControl( int? structureEntityId )
        {
            dvpStructureGroupTypePurposePicker.Label = "Group Type Purpose";
            dvpStructureGroupTypePurposePicker.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.GROUPTYPE_PURPOSE ).Id;
            dvpStructureGroupTypePurposePicker.SelectedDefinedValueId = structureEntityId;
            dvpStructureGroupTypePurposePicker.Visible = true;
        }

        #endregion Internal Methods

        #region State Determining Methods

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
        /// Can the user edit the sequence
        /// </summary>
        /// <returns></returns>
        private bool CanEdit()
        {
            return UserCanAdministrate && GetSequence() != null;
        }

        /// <summary>
        /// Can the user add a new sequence
        /// </summary>
        /// <returns></returns>
        private bool CanAdd()
        {
            return UserCanAdministrate && GetSequence() == null;
        }

        /// <summary>
        /// Is this block currently adding a new sequence
        /// </summary>
        /// <returns></returns>
        private bool IsAddMode()
        {
            return CanAdd();
        }

        /// <summary>
        /// Is this block currently editing an existing sequence
        /// </summary>
        /// <returns></returns>
        private bool IsEditMode()
        {
            return CanEdit() && hfIsEditMode.Value.Trim().ToLower() == "true";
        }

        /// <summary>
        /// Is the block currently showing information about a sequence
        /// </summary>
        /// <returns></returns>
        private bool IsViewMode()
        {
            return GetSequence() != null && hfIsEditMode.Value.Trim().ToLower() != "true";
        }

        #endregion State Determining Methods

        #region Data Interface Methods

        /// <summary>
        /// Get the actual sequence model for deleting or editing
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
        /// Get the name of the sequence attendance structure
        /// </summary>
        /// <returns></returns>
        private string GetSequenceStructureName()
        {
            if ( _sequenceStructureName == null )
            {
                var sequence = GetSequence();

                if ( sequence != null )
                {
                    var service = GetSequenceService();
                    _sequenceStructureName = service.GetStructureName( sequence.StructureType, sequence.StructureEntityId );
                }
            }

            return _sequenceStructureName;
        }
        private string _sequenceStructureName = null;

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
        /// Get the group type service
        /// </summary>
        /// <returns></returns>
        private GroupTypeService GetGroupTypeService()
        {
            if ( _groupTypeService == null )
            {
                var rockContext = GetRockContext();
                _groupTypeService = new GroupTypeService( rockContext );
            }

            return _groupTypeService;
        }
        private GroupTypeService _groupTypeService = null;

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