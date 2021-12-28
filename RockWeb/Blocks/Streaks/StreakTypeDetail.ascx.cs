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
using Rock.Tasks;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Streaks
{
    [DisplayName( "Streak Type Detail" )]
    [Category( "Streaks" )]
    [Description( "Displays the details of the given Streak Type for editing." )]

    [LinkedPage(
        "Map Editor Page",
        Description = "Page used for editing the streak type map.",
        Key = AttributeKey.MapEditorPage,
        IsRequired = false,
        Order = 1 )]

    [LinkedPage(
        "Exclusions Page",
        Description = "Page used for viewing a list of streak type exclusions.",
        Key = AttributeKey.ExclusionsPage,
        IsRequired = false,
        Order = 2 )]

    [LinkedPage(
        "Achievements Page",
        Description = "Page used for viewing a list of streak type achievement types.",
        Key = AttributeKey.AchievementsPage,
        IsRequired = false,
        Order = 3 )]

    public partial class StreakTypeDetail : RockBlock
    {
        #region Keys

        /// <summary>
        /// Keys to use for Attributes
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// Key for the Map Editor Page
            /// </summary>
            public const string MapEditorPage = "MapEditorPage";

            /// <summary>
            /// Key for the Exclusions Page
            /// </summary>
            public const string ExclusionsPage = "ExclusionsPage";

            /// <summary>
            /// The achievements page
            /// </summary>
            public const string AchievementsPage = "AchievementsPage";
        }

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            /// <summary>
            /// Key for the streak type Id
            /// </summary>
            public const string StreakTypeId = "StreakTypeId";

            /// <summary>
            /// Key for the streak id
            /// </summary>
            public const string StreakId = "StreakId";
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
                var streakType = GetStreakType();
                pdAuditDetails.SetEntity( streakType, ResolveRockUrl( "~" ) );

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
            var streakType = GetStreakType();
            breadCrumbs.Add( new BreadCrumb( IsAddMode() ? "New Streak Type" : streakType.Name, pageReference ) );
            return breadCrumbs;
        }

        /// <summary>
        /// Initialize the action buttons that affect the entire record.
        /// </summary>
        private void InitializeActionButtons()
        {
            btnRebuild.Attributes["onclick"] = "javascript: return Rock.dialogs.confirmDelete(event, 'data', 'Occurrence and enrollment map data belonging to this streak type will be deleted and rebuilt from attendance records! This process runs in separate process and may take several minutes to complete.');";
            btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}', 'All associated Enrollments and Exclusions will also be deleted!');", StreakType.FriendlyTypeName );
            btnSecurity.EntityTypeId = EntityTypeCache.Get( typeof( StreakType ) ).Id;
        }

        /// <summary>
        /// Initialize handlers for block configuration changes.
        /// </summary>
        /// <param name="triggerPanel"></param>
        private void InitializeSettingsNotification()
        {
            BlockUpdated += Block_BlockUpdated;
            AddConfigurationUpdateTrigger( upStreakTypeDetail );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlFrequencyOccurrence control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlFrequencyOccurrence_SelectedIndexChanged( object sender, EventArgs e )
        {
            SyncFrequencyControls();
        }

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
        /// Button to go to the achievements page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnAchievements_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.AchievementsPage );
        }

        /// <summary>
        /// Navigate to a linked page
        /// </summary>
        /// <param name="attributeKey"></param>
        private void NavigateToLinkedPage( string attributeKey )
        {
            var streakType = GetStreakType();

            if ( streakType == null )
            {
                ShowBlockError( nbEditModeMessage, "A streak type is required" );
                return;
            }

            NavigateToLinkedPage( attributeKey, new Dictionary<string, string> {
                { PageParameterKey.StreakTypeId, streakType.Id.ToString() }
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
        /// The change event of the linked activity structure selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ddlStructureType_SelectedIndexChanged( object sender, EventArgs e )
        {
            RenderLinkedActivityStructureControls();
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
        /// Rebuild the streak type and enrollment data
        /// </summary>
        private void RebuildData()
        {
            var rockContext = GetRockContext();
            var streakType = GetStreakType();

            if ( !streakType.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
            {
                mdDeleteWarning.Show( "You are not authorized to rebuild this item.", ModalAlertType.Information );
                return;
            }

            new ProcessRebuildStreakType.Message { StreakTypeId = streakType.Id }.Send();
            ShowBlockSuccess( nbEditModeMessage, "The streak type rebuild has been started." );
            btnRebuild.Enabled = false;
        }

        /// <summary>
        /// Delete the current record.
        /// </summary>
        private void DeleteRecord()
        {
            var streakType = GetStreakType();

            if ( streakType != null )
            {
                if ( !streakType.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
                {
                    mdDeleteWarning.Show( "You are not authorized to delete this item.", ModalAlertType.Information );
                    return;
                }

                var service = GetStreakTypeService();
                var errorMessage = string.Empty;

                if ( !service.CanDelete( streakType, out errorMessage, true ) )
                {
                    mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                service.Delete( streakType );
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
            var streakType = GetStreakType();
            var streakTypeService = GetStreakTypeService();
            var isNew = streakType == null;

            if ( isNew )
            {
                streakType = new StreakType();
                streakTypeService.Add( streakType );
            }

            // Can only set the start date if the streak type is new
            if ( isNew )
            {
                var frequencySelected = GetEnumSelected<StreakOccurrenceFrequency>( ddlFrequencyOccurrence ) ?? StreakOccurrenceFrequency.Daily;
                var isDaily = frequencySelected == StreakOccurrenceFrequency.Daily;
                streakType.OccurrenceFrequency = frequencySelected;

                var selectedDate = rdpStartDate.SelectedDate ?? RockDateTime.Today;
                streakType.StartDate = isDaily ? selectedDate : selectedDate.SundayDate();
            }

            streakType.Name = tbName.Text;
            streakType.IsActive = cbActive.Checked;
            streakType.Description = tbDescription.Text;
            streakType.EnableAttendance = cbEnableAttendance.Checked;
            streakType.RequiresEnrollment = cbRequireEnrollment.Checked;
            streakType.StructureType = GetEnumSelected<StreakStructureType>( ddlStructureType );
            streakType.StructureEntityId = GetStructureEntityIdSelected();

            if ( streakType.OccurrenceFrequency == StreakOccurrenceFrequency.Weekly )
            {
                streakType.FirstDayOfWeek = dowPicker.SelectedDayOfWeek;
            }
            else
            {
                streakType.FirstDayOfWeek = null;
            }

            if ( streakType.StructureType == StreakStructureType.FinancialTransaction )
            {
                streakType.StructureSettings.IncludeChildAccounts = cbIncludeChildAccounts.Checked;
            }

            if ( !streakType.IsValid )
            {
                // Controls will render the error messages
                return;
            }

            try
            {
                var rockContext = GetRockContext();
                rockContext.SaveChanges();

                if ( !streakType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    streakType.AllowPerson( Authorization.VIEW, CurrentPerson, rockContext );
                }

                if ( !streakType.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                {
                    streakType.AllowPerson( Authorization.EDIT, CurrentPerson, rockContext );
                }

                if ( !streakType.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
                {
                    streakType.AllowPerson( Authorization.ADMINISTRATE, CurrentPerson, rockContext );
                }

            }
            catch ( Exception ex )
            {
                ShowBlockException( nbEditModeMessage, ex );
                return;
            }

            // If the save was successful, reload the page using the new record Id.
            NavigateToPage( RockPage.Guid, new Dictionary<string, string> {
                { PageParameterKey.StreakTypeId, streakType.Id.ToString() }
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
                nbEditModeMessage.Text = "The streak type was not found";
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

            rdpStartDate.Enabled = false; // Cannot change the start date
            ddlFrequencyOccurrence.Enabled = false; // Cannot change the frequency

            var streakType = GetStreakType();
            var structureType = streakType.StructureType;
            var structureEntityId = streakType.StructureEntityId;

            lReadOnlyTitle.Text = streakType.Name.FormatAsHtmlTitle();
            hlInactive.Visible = !streakType.IsActive;

            tbName.Text = streakType.Name;
            cbActive.Checked = streakType.IsActive;
            tbDescription.Text = streakType.Description;
            cbEnableAttendance.Checked = streakType.EnableAttendance;
            cbRequireEnrollment.Checked = streakType.RequiresEnrollment;
            rdpStartDate.SelectedDate = streakType.StartDate;
            ddlFrequencyOccurrence.SelectedValue = streakType.OccurrenceFrequency.ToString();
            dowPicker.SelectedDayOfWeek = streakType.FirstDayOfWeek;
            ddlStructureType.SelectedValue = structureType.HasValue ? structureType.Value.ToString() : string.Empty;

            RenderLinkedActivityStructureControls();
            SyncFrequencyControls();
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
            rdpStartDate.Enabled = true; // Can only set the start date when adding a new streak type
            ddlFrequencyOccurrence.Enabled = true; // Can only set the frequency when adding a new streak type

            rdpStartDate.SelectedDate = RockDateTime.Today;

            lReadOnlyTitle.Text = ActionTitle.Add( StreakType.FriendlyTypeName ).FormatAsHtmlTitle();
            hlInactive.Visible = false;

            SyncFrequencyControls();
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
            btnSecurity.Visible = canEdit;

            var streakType = GetStreakType();
            lReadOnlyTitle.Text = streakType.Name.FormatAsHtmlTitle();
            hlInactive.Visible = !streakType.IsActive;
            btnRebuild.Enabled = streakType.IsActive;
            btnRebuild.Visible = streakType.StructureType.HasValue &&
                ( streakType.StructureType == StreakStructureType.AnyAttendance || streakType.StructureEntityId.HasValue );

            var descriptionList = new DescriptionList();
            descriptionList.Add( "Description", streakType.Description );
            descriptionList.Add( "Frequency", streakType.OccurrenceFrequency.ConvertToString() );
            descriptionList.Add( "Start Date", streakType.StartDate.ToShortDateString() );
            descriptionList.Add( "Requires Enrollment", streakType.RequiresEnrollment.ToYesNo() );
            descriptionList.Add( "Sync Linked Activity", streakType.EnableAttendance.ToYesNo() );

            if ( streakType.StructureType.HasValue )
            {
                var structureName = GetStreakStructureName();
                var structureString = string.Format( "{0}{1}",
                    streakType.StructureType.Value.GetDescription(),
                    string.Format( "{0}{1}",
                        structureName.IsNullOrWhiteSpace() ? string.Empty : " - ",
                        structureName
                    ) );

                descriptionList.Add( "Linked Activity", structureString );
            }
            else
            {
                descriptionList.Add( "Linked Activity", "None" );
            }

            lStreakTypeDescription.Text = descriptionList.Html;

            if ( canEdit )
            {
                btnSecurity.Title = "Secure " + streakType.Name;
                btnSecurity.EntityId = streakType.Id;
            }

            SetLinkVisibility( btnAchievements, AttributeKey.AchievementsPage );
            SetLinkVisibility( btnExclusions, AttributeKey.ExclusionsPage );
            SetLinkVisibility( btnMapEditor, AttributeKey.MapEditorPage );
        }

        /// <summary>
        /// Bind the options to the drop down lists
        /// </summary>
        private void BindDropDownLists()
        {
            BindDropDownListToEnum( typeof( StreakOccurrenceFrequency ), ddlFrequencyOccurrence, false );
            BindStreakStructureTypeToDropDownList( ddlStructureType, true, "None" );
        }

        /// <summary>
        /// Take an enum type and bind it's options to the drop down list
        /// </summary>
        /// <param name="enumType">The type of the enum to populate the dropdown with.</param>
        /// <param name="ddl">The dropdown list.</param>
        private void BindDropDownListToEnum( Type enumType, DataDropDownList ddl, bool includeBlank, string blankText = "" )
        {
            if ( includeBlank )
            {
                ddl.Items.Add( new ListItem( blankText, string.Empty ) );
            }

            var itemValues = Enum.GetValues( enumType );
            var itemNames = Enum.GetNames( enumType );


            for ( var i = 0; i < itemNames.Length; i++ )
            {
                ddl.Items.Add( new ListItem( itemNames[i].SplitCase(), itemValues.GetValue( i ).ToString() ) );
            }
        }

        /// <summary>
        /// Binds the streak structure type to drop down list.
        /// </summary>
        /// <param name="ddl">The dropdown list.</param>
        /// <param name="includeBlank">if set to <c>true</c> [include blank].</param>
        /// <param name="blankText">The blank text.</param>
        private void BindStreakStructureTypeToDropDownList( DataDropDownList ddl, bool includeBlank, string blankText = "" )
        {
            if ( includeBlank )
            {
                ddl.Items.Add( new ListItem( blankText, string.Empty ) );
            }

            var itemValues = Enum.GetValues( typeof( StreakStructureType ) );
            var itemNames = GetStreakStructureTypeDisplayValues();

            for ( var i = 0; i < itemNames.Count(); i++ )
            {
                ddl.Items.Add( new ListItem( itemNames.ElementAt( i ), itemValues.GetValue( i ).ToString() ) );
            }

        }

        /// <summary>
        /// Gets the streak structure type display values.
        /// </summary>
        /// <returns>A <see cref="IEnumerable&lt;string&gt;"/> of display attribute values for the enum.</returns>
        private IEnumerable<string> GetStreakStructureTypeDisplayValues()
        {
            foreach ( object item in Enum.GetValues( typeof( StreakStructureType ) ) )
            {
                yield return ( ( StreakStructureType ) item ).GetDescription();
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
        /// Render the appropriate controls for the selected linked activity type
        /// </summary>
        private int? GetStructureEntityIdSelected()
        {
            var selectedStructureType = GetEnumSelected<StreakStructureType>( ddlStructureType );

            if ( !selectedStructureType.HasValue )
            {
                return null;
            }

            switch ( selectedStructureType.Value )
            {
                case StreakStructureType.AnyAttendance:
                    return null;
                case StreakStructureType.CheckInConfig:
                case StreakStructureType.GroupType:
                    return gtpStructureGroupTypePicker.SelectedGroupTypeId;
                case StreakStructureType.Group:
                    return gpStructureGroupPicker.GroupId;
                case StreakStructureType.GroupTypePurpose:
                case StreakStructureType.InteractionMedium:
                    return dvpStructureDefinedValuePicker.SelectedDefinedValueId;
                case StreakStructureType.InteractionChannel:
                    return icChannelPicker.SelectedValueAsInt();
                case StreakStructureType.InteractionComponent:
                    return icicComponentPicker.InteractionComponentId;
                case StreakStructureType.FinancialTransaction:
                    return apStructureAccountPicker.AccountId;
                default:
                    throw new NotImplementedException( "The structure type is not implemented" );
            }
        }

        /// <summary>
        /// Synchronizes the frequency controls.
        /// </summary>
        private void SyncFrequencyControls()
        {
            var frequencySelected = GetEnumSelected<StreakOccurrenceFrequency>( ddlFrequencyOccurrence ) ?? StreakOccurrenceFrequency.Daily;

            dowPicker.Visible = ( frequencySelected == StreakOccurrenceFrequency.Weekly );
        }

        /// <summary>
        /// Render the appropriate controls for the selected linked activity type
        /// </summary>
        private void RenderLinkedActivityStructureControls()
        {
            var isAddMode = IsAddMode();

            if ( !IsEditMode() && !isAddMode )
            {
                return;
            }

            gpStructureGroupPicker.Visible = false;
            dvpStructureDefinedValuePicker.Visible = false;
            gtpStructureGroupTypePicker.Visible = false;
            icicComponentPicker.Visible = false;
            icChannelPicker.Visible = false;
            apStructureAccountPicker.Visible = false;
            cbIncludeChildAccounts.Visible = false;

            var streakType = isAddMode ? null : GetStreakType();
            var originalStructureType = isAddMode ? null : streakType.StructureType;
            var selectedStructureType = GetEnumSelected<StreakStructureType>( ddlStructureType );
            var originalEntityId = isAddMode ? null : streakType.StructureEntityId;
            var structureEntityId = originalStructureType == selectedStructureType ? originalEntityId : null;
            var includeChildAccounts = isAddMode ? false : streakType.StructureSettings.IncludeChildAccounts;

            if ( !selectedStructureType.HasValue )
            {
                return;
            }

            switch ( selectedStructureType.Value )
            {
                case StreakStructureType.AnyAttendance:
                    break;
                case StreakStructureType.CheckInConfig:
                    RenderCheckinConfigControl( structureEntityId );
                    break;
                case StreakStructureType.Group:
                    RenderGroupControl( structureEntityId );
                    break;
                case StreakStructureType.GroupType:
                    RenderGroupTypeControl( structureEntityId );
                    break;
                case StreakStructureType.GroupTypePurpose:
                    RenderStructureDefinedValueControl( structureEntityId, "Group Type Purpose", Rock.SystemGuid.DefinedType.GROUPTYPE_PURPOSE );
                    break;
                case StreakStructureType.InteractionChannel:
                    RenderInteractionChannelControl( structureEntityId );
                    break;
                case StreakStructureType.InteractionComponent:
                    RenderInteractionComponentControl( structureEntityId );
                    break;
                case StreakStructureType.InteractionMedium:
                    RenderStructureDefinedValueControl( structureEntityId, "Interaction Medium", Rock.SystemGuid.DefinedType.INTERACTION_CHANNEL_MEDIUM );
                    break;
                case StreakStructureType.FinancialTransaction:
                    RenderFinancialTransactionControl( structureEntityId, includeChildAccounts );
                    break;
                default:
                    throw new NotImplementedException( "The structure type is not implemented" );
            }
        }

        /// <summary>
        /// Render control for linked activity structure of check in config
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
        /// Render control for linked activity structure of group
        /// </summary>
        private void RenderGroupControl( int? structureEntityId )
        {
            gpStructureGroupPicker.Label = "Group";
            gpStructureGroupPicker.GroupId = structureEntityId;
            gpStructureGroupPicker.Visible = true;
        }

        /// <summary>
        /// Render control for linked activity structure of group type
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
        /// Render control for linked activity structure of group type purpose or interaction medium
        /// </summary>
        private void RenderStructureDefinedValueControl( int? structureEntityId, string label, string definedTypeGuidString )
        {
            dvpStructureDefinedValuePicker.Label = label;
            dvpStructureDefinedValuePicker.DefinedTypeId = DefinedTypeCache.Get( definedTypeGuidString ).Id;
            dvpStructureDefinedValuePicker.SelectedDefinedValueId = structureEntityId;
            dvpStructureDefinedValuePicker.Visible = true;
        }

        /// <summary>
        /// Renders the interaction channel control.
        /// </summary>
        /// <param name="structureEntityId">The structure entity identifier.</param>
        private void RenderInteractionChannelControl( int? structureEntityId )
        {
            icChannelPicker.Label = "Interaction Channel";
            icChannelPicker.SetValue( structureEntityId );
            icChannelPicker.Visible = true;
        }

        /// <summary>
        /// Renders the interaction component control.
        /// </summary>
        /// <param name="structureEntityId">The structure entity identifier.</param>
        private void RenderInteractionComponentControl( int? structureEntityId )
        {
            icicComponentPicker.InteractionComponentId = structureEntityId;
            icicComponentPicker.Visible = true;
        }

        /// <summary>
        /// Render control for linked activity structure of financial transactions.
        /// </summary>
        private void RenderFinancialTransactionControl( int? structureEntityId, bool includeChildAccounts )
        {
            apStructureAccountPicker.Label = "Account";
            apStructureAccountPicker.AccountId = structureEntityId;
            apStructureAccountPicker.Visible = true;

            cbIncludeChildAccounts.Label = "Include Child Accounts";
            cbIncludeChildAccounts.Checked = includeChildAccounts;
            cbIncludeChildAccounts.Visible = true;
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
        /// Can the user edit the streak type
        /// </summary>
        /// <returns></returns>
        private bool CanEdit()
        {
            return UserCanAdministrate && GetStreakType() != null;
        }

        /// <summary>
        /// Can the user add a new streak type
        /// </summary>
        /// <returns></returns>
        private bool CanAdd()
        {
            return UserCanAdministrate && GetStreakType() == null;
        }

        /// <summary>
        /// Is this block currently adding a new streak type
        /// </summary>
        /// <returns></returns>
        private bool IsAddMode()
        {
            return CanAdd();
        }

        /// <summary>
        /// Is this block currently editing an existing streak type
        /// </summary>
        /// <returns></returns>
        private bool IsEditMode()
        {
            return CanEdit() && hfIsEditMode.Value.Trim().ToLower() == "true";
        }

        /// <summary>
        /// Is the block currently showing information about a streak type
        /// </summary>
        /// <returns></returns>
        private bool IsViewMode()
        {
            return GetStreakType() != null && hfIsEditMode.Value.Trim().ToLower() != "true";
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
        /// Get the actual streak enrollment model for deleting or editing
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
        /// Get the name of the streak type linked activity structure
        /// </summary>
        /// <returns></returns>
        private string GetStreakStructureName()
        {
            if ( _streakTypeStructureName == null )
            {
                var streakType = GetStreakType();

                if ( streakType != null )
                {
                    var service = GetStreakTypeService();
                    _streakTypeStructureName = service.GetStructureName( streakType.StructureType, streakType.StructureEntityId );
                }
            }

            return _streakTypeStructureName;
        }
        private string _streakTypeStructureName = null;

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