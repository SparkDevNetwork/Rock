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
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Field.Types;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Steps
{
    [DisplayName( "Step Bulk Entry" )]
    [Category( "Steps" )]
    [Description( "Displays a form to add multiple steps at a time." )]

    #region Block Attributes

    [StepProgramStepTypeField(
        name: "Step Program and Type",
        description: "The step program and step type to use to add a new step. Leave this empty to allow the user to choose.",
        required: false,
        order: 1,
        key: AttributeKey.StepProgramStepType )]

    [StepProgramStepStatusField(
        name: "Step Program and Status",
        description: "The step program and step status to use to add a new step. Leave this empty to allow the user to choose.",
        required: false,
        order: 2,
        key: AttributeKey.StepProgramStepStatus )]

    #endregion Block Attributes

    public partial class StepBulkEntry : RockBlock
    {
        #region Keys

        /// <summary>
        /// Keys for block attributes
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The step program
            /// </summary>
            public const string StepProgramStepType = "StepProgramStepType";

            /// <summary>
            /// The step program step status
            /// </summary>
            public const string StepProgramStepStatus = "StepProgramStepStatus";
        }

        /// <summary>
        /// Keys for page params
        /// </summary>
        private static class PageParameterKey
        {
            /// <summary>
            /// The step program identifier
            /// </summary>
            public const string StepProgramId = "StepProgramId";

            /// <summary>
            /// The step type identifier
            /// </summary>
            public const string StepTypeId = "StepTypeId";

            /// <summary>
            /// The people set
            /// </summary>
            public const string PeopleSet = "Set";
        }

        /// <summary>
        /// Keys for the View State
        /// </summary>
        private static class ViewStateKey
        {
            /// <summary>
            /// The set index
            /// </summary>
            public const string SetIndex = "SetIndex";

            /// <summary>
            /// The set step guids
            /// </summary>
            public const string SetStepGuids = "SetStepGuids";
        }

        #endregion Keys

        #region Life Cycle Events

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            if ( !IsPostBack )
            {
                InitializeNavigationActions();
                InitializeStepProgramPicker();
                InitializeStepTypePicker();
            }

            BlockUpdated += Block_BlockUpdated;
        }

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="NotImplementedException"></exception>
        private void Block_BlockUpdated( object sender, EventArgs e )
        {
            InitializeNavigationActions();
            InitializeStepProgramPicker();
            InitializeStepTypePicker();
        }

        #endregion Life Cycle Events

        #region Methods

        /// <summary>
        /// Gets the step type unique identifier from block attribute.
        /// </summary>
        /// <returns></returns>
        private Guid? GetStepTypeGuidFromBlockAttribute()
        {
            var attributeValue = GetAttributeValue( AttributeKey.StepProgramStepType );

            Guid? stepProgramGuid;
            Guid? stepTypeGuid;
            StepProgramStepTypeFieldType.ParseDelimitedGuids( attributeValue, out stepProgramGuid, out stepTypeGuid );

            return stepTypeGuid;
        }

        /// <summary>
        /// Gets the step program unique identifier from block attribute.
        /// </summary>
        /// <returns></returns>
        private Guid? GetStepProgramGuidFromBlockAttribute()
        {
            var attributeValue = GetAttributeValue( AttributeKey.StepProgramStepType );

            Guid? stepProgramGuid;
            Guid? stepTypeGuid;
            StepProgramStepTypeFieldType.ParseDelimitedGuids( attributeValue, out stepProgramGuid, out stepTypeGuid );

            return stepProgramGuid;
        }

        /// <summary>
        /// Saves the record.
        /// </summary>
        private bool SaveRecord()
        {
            var isSetMode = IsSetMode();
            var isSkipMode = isSetMode && IsSkipMode();

            if ( isSkipMode )
            {
                // Don't save because this person has too many records or unmet prerequisites
                return false;
            }

            var stepType = GetStepType();

            // Step Type is required for the step
            if ( stepType == null )
            {
                ShowBulkLevelError( "The step type is required to save a step record." );
                return false;
            }

            // Verify current user has permission.
            var userCanEdit = stepType.IsAuthorized( Authorization.EDIT, CurrentPerson );

            if ( !userCanEdit )
            {
                ShowNonBulkLevelError( "Sorry, you don't have sufficient permission to add Steps for this Step Type." );
                return false;
            }

            var person = GetPerson();

            // Person is required for the step
            if ( person == null )
            {
                ShowNonBulkLevelError( "The person is required to save a step record." );
                return false;
            }

            // Status is required
            var status = GetStepStatus();

            if ( status == null )
            {
                ShowBulkLevelError( "The step status is required to save a step record." );
                return false;
            }

            // Date is required
            var startDate = dpStartDate.SelectedDate;

            if ( !startDate.HasValue )
            {
                ShowBulkLevelError( "The date is required to save a step record." );
                return false;
            }

            Step step = null;
            Guid? exisitingStepGuid = null;
            var stepService = GetStepService();
            var isAddMode = true;

            // Use the predetermined guid to get an existing step. This would happen if the user clicked previous
            if ( IsSetMode() )
            {
                exisitingStepGuid = GetCurrentSetPersonStepGuid();

                if ( exisitingStepGuid.HasValue )
                {
                    step = GetCurrentSetPersonStep();
                    isAddMode = step == null;
                }
            }

            // If we are adding a new step then add it to the service, using the predetermined guid if that is set
            if ( isAddMode )
            {
                step = new Step { Guid = exisitingStepGuid ?? Guid.NewGuid() };
            }

            // Update the step properties
            step.StepTypeId = stepType.Id;
            step.PersonAliasId = person.PrimaryAliasId.Value;
            step.StartDateTime = startDate.Value;
            step.EndDateTime = stepType.HasEndDate ? dpEndDate.SelectedDate : null;
            step.StepStatusId = status.Id;
            step.CampusId = cpCampus.SelectedCampusId;

            // Mark the completed date if the status is a completed status
            if ( !status.IsCompleteStatus )
            {
                step.CompletedDateTime = null;
            }
            else
            {
                step.CompletedDateTime = step.EndDateTime ?? step.StartDateTime;
            }

            // Validate the step's properties
            if ( !step.IsValid )
            {
                ShowNonBulkLevelError( step.ValidationResults.Select( vr => vr.ErrorMessage ).ToList().AsDelimited( "<br />" ) );
                return false;
            }

            // Validate the step against the step type's multiple settings and prerequisites
            if ( isAddMode )
            {
                var errorMessage = string.Empty;
                var canAdd = stepService.CanAdd( step, out errorMessage );

                if ( !errorMessage.IsNullOrWhiteSpace() )
                {
                    ShowNonBulkLevelError( errorMessage );
                    return false;
                }

                if ( !canAdd )
                {
                    ShowNonBulkLevelError( "The step cannot be added for an unspecified reason" );
                    return false;
                }

                // Save the step record
                stepService.Add( step );
            }

            var rockContext = GetRockContext();
            rockContext.SaveChanges();

            // Save the step attributes from the attribute controls
            step.LoadAttributes( rockContext );
            avcBulkAttributes.GetEditValues( step );
            avcNonBulkAttributes.GetEditValues( step );
            step.SaveAttributeValues( rockContext );

            // Prepare the non bulk form for another step entry
            ClearBlockData();
            ResetNonBulkControls();

            return true;
        }

        /// <summary>
        /// Selects the step program.
        /// </summary>
        private void SelectStepProgram()
        {
            ClearErrors();
            var stepProgram = GetStepProgram();
            var stepProgramId = stepProgram == null ? ( int? ) null : stepProgram.Id;
            stpStepTypePicker.StepProgramId = stepProgramId;
            sspStatusPicker.StepProgramId = stepProgramId;
        }

        /// <summary>
        /// Selects the type of the step.
        /// </summary>
        private void SelectStepType()
        {
            ClearErrors();
            var stepType = GetStepType();

            if ( stepType == null || !stepType.HasEndDate )
            {
                dpStartDate.Label = "Date";
                dpEndDate.Visible = false;
            }
            else
            {
                dpStartDate.Label = "Start Date";
                dpEndDate.Visible = true;
            }

            BuildBulkDynamicControls();
            BuildNonBulkDynamicControls();
        }

        /// <summary>
        /// Initializes the set navigation.
        /// </summary>
        private void InitializeNavigationActions()
        {
            if ( IsSetMode() )
            {
                // Operating on a predetermined set of people
                ppPersonPicker.Enabled = false;
                var set = GetPersonIdSet();

                if ( set == null || !set.Any() )
                {
                    ShowBlockLevelError( "The specified person set is not valid" );
                    return;
                }

                UpdateSetStatusIndicators();

                btnSave.Visible = false;
                btnPrevious.Visible = true;
                lSetStatus.Visible = true;
                btnNext.Visible = true;
                divPersonName.Visible = true;
                ppPersonPicker.Visible = false;
            }
            else
            {
                // Operation without a set of people
                ppPersonPicker.Enabled = true;
                avcNonBulkAttributes.Visible = true;

                btnSave.Visible = true;
                btnPrevious.Visible = false;
                lSetStatus.Visible = false;
                btnNext.Visible = false;
                divPersonName.Visible = false;
                ppPersonPicker.Visible = true;
            }
        }

        /// <summary>
        /// Initializes the step program picker.
        /// </summary>
        private void InitializeStepProgramPicker()
        {
            var isSelectedable =
                !PageParameter( PageParameterKey.StepTypeId ).AsIntegerOrNull().HasValue &&
                !GetStepTypeGuidFromBlockAttribute().HasValue &&
                !GetStepProgramGuidFromBlockAttribute().HasValue &&
                !PageParameter( PageParameterKey.StepProgramId ).AsIntegerOrNull().HasValue;

            var stepProgram = GetStepProgram();

            if ( !isSelectedable )
            {
                sppProgramPicker.Visible = false;

                if ( stepProgram == null )
                {
                    ShowBlockLevelError( "The specified step program could not be found" );
                }
                else
                {
                    SelectStepProgram();
                }
            }
        }

        /// <summary>
        /// Initializes the step type picker.
        /// </summary>
        private void InitializeStepTypePicker()
        {
            var isSelectedable =
                !PageParameter( PageParameterKey.StepTypeId ).AsIntegerOrNull().HasValue &&
                !GetStepTypeGuidFromBlockAttribute().HasValue;

            var stepType = GetStepType();
            stpStepTypePicker.Enabled = isSelectedable;

            if ( !isSelectedable )
            {
                if ( stepType == null )
                {
                    ShowBlockLevelError( "The specified step type could not be found" );
                }
                else
                {
                    stpStepTypePicker.SelectedValue = stepType.Id.ToString();
                    SelectStepType();
                }
            }
        }

        /// <summary>
        /// Builds the bulk dynamic controls.
        /// </summary>
        private void BuildBulkDynamicControls()
        {
            var stepEntityTypeId = EntityTypeCache.GetId( typeof( Step ) );
            var stepAttributes = AttributeCache.All().Where( a => a.EntityTypeId == stepEntityTypeId );

            avcBulkAttributes.ExcludedAttributes = stepAttributes
                .Where( a =>
                    a.Key == "Order" ||
                    a.Key == "Active" ||
                    !a.ShowOnBulk )
                .ToArray();

            var stepType = GetStepType();

            if ( stepType != null )
            {
                var step = new Step { StepTypeId = stepType.Id };
                step.LoadAttributes();
                avcBulkAttributes.AddEditControls( step );
            }
            else
            {
                avcBulkAttributes.AddEditControls( null );
            }
        }

        /// <summary>
        /// Builds the non bulk dynamic controls.
        /// </summary>
        private void BuildNonBulkDynamicControls()
        {
            var stepEntityTypeId = EntityTypeCache.GetId( typeof( Step ) );
            var stepAttributes = AttributeCache.All().Where( a => a.EntityTypeId == stepEntityTypeId );

            avcNonBulkAttributes.ExcludedAttributes = stepAttributes
                .Where( a =>
                    a.Key == "Order" ||
                    a.Key == "Active" ||
                    a.ShowOnBulk )
                .ToArray();

            var stepType = GetStepType();

            if ( stepType != null )
            {
                var step = GetCurrentSetPersonStep() ?? new Step { StepTypeId = stepType.Id };
                step.LoadAttributes();
                avcNonBulkAttributes.AddEditControls( step );
            }
            else
            {
                avcNonBulkAttributes.AddEditControls( null );
            }

            UpdateNonBulkDynamicControlsForPersonChange();
        }

        /// <summary>
        /// Updates the non bulk dynamic controls for person change.
        /// </summary>
        private void UpdateNonBulkDynamicControlsForPersonChange()
        {
            // Determine if this person is able to add a step of this type because of prerequisites and allow multiple
            var personIsValid = PreValidatePerson();

            if ( IsSetMode() )
            {
                var skipMode = IsSkipMode();

                var index = GetCurrentSetIndex();
                var isFirst = index == 0;

                btnNext.Enabled = personIsValid || skipMode;
                btnPrevious.Enabled = !isFirst && ( personIsValid || skipMode );
                btnSave.Enabled = personIsValid || skipMode;

                // If the person is not valid, but this is set mode, then the next button just skips to the next person
                avcNonBulkAttributes.Visible = !skipMode;
                btnNext.ValidationGroup = skipMode ? string.Empty : "BulkEntry";
            }
            else
            {
                btnSave.Enabled = personIsValid;
                btnNext.ValidationGroup = "BulkEntry";

                // Show the non-bulk controls if the person is valid or is not yet selected to avoid confusion
                avcNonBulkAttributes.Visible = personIsValid || GetPerson() == null;
            }
        }

        /// <summary>
        /// Resets the non bulk controls.
        /// </summary>
        private void ResetNonBulkControls()
        {
            ppPersonPicker.SetValue( null );
            lPersonName.Text = string.Empty;
            BuildNonBulkDynamicControls();
        }

        /// <summary>
        /// Pres the validate person.
        /// </summary>
        /// <returns></returns>
        private bool PreValidatePerson()
        {
            var person = GetPerson();

            if ( person == null )
            {
                return false;
            }

            var stepType = GetStepType();

            if ( stepType == null )
            {
                return false;
            }

            if ( !CanAddBecauseMeetsAllowMultipleRule() )
            {
                ShowNonBulkLevelError( string.Format( "{0} is not able to complete {1} again because of the 'Allow Multiple' setting.", person.NickName, stepType.Name ) );
                return false;
            }

            var unmetPrereqs = GetUnmetPrereqs();

            if ( unmetPrereqs != null && unmetPrereqs.Any() )
            {
                var listItems = unmetPrereqs.Select( st => string.Format( "<li>{0}</li>", st.Name ) ).JoinStrings( string.Empty );
                ShowNonBulkLevelError( string.Format( "{0} is not able to complete this step as the following prerequisites are not completed:<ul>{1}</ul>", person.NickName, listItems ) );
                return false;
            }

            return true;
        }

        #endregion Methods

        #region Control Events

        /// <summary>
        /// Handles the SelectedIndexChanged event of the sppProgramPicker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void sppProgramPicker_SelectedIndexChanged( object sender, System.EventArgs e )
        {
            SelectStepProgram();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the stpStepTypePicker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void stpStepTypePicker_SelectedIndexChanged( object sender, System.EventArgs e )
        {
            SelectStepType();
        }

        /// <summary>
        /// Handles the Click event of the btnPrevious control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void btnPrevious_Click( object sender, System.EventArgs e )
        {
            GoToPreviousInSet();
        }

        /// <summary>
        /// Handles the Click event of the btnNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void btnNext_Click( object sender, System.EventArgs e )
        {
            var goToNext = true;
            var index = GetCurrentSetIndex();
            var lastIndex = GetLastSetIndex();
            var isLast = index == lastIndex;

            if ( !IsSkipMode() )
            {
                goToNext = SaveRecord();
            }

            if ( goToNext && isLast )
            {
                var stepType = GetStepType();
                var queryParams = new Dictionary<string, string>();

                if ( stepType != null )
                {
                    queryParams[PageParameterKey.StepTypeId] = stepType.Id.ToString();
                }

                NavigateToParentPage( queryParams );
            }
            else if ( goToNext )
            {
                GoToNextInSet();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, System.EventArgs e )
        {
            SaveRecord();
        }

        /// <summary>
        /// Handles the SelectPerson event of the ppPersonPicker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void ppPersonPicker_SelectPerson( object sender, System.EventArgs e )
        {
            ClearErrors();
            UpdateNonBulkDynamicControlsForPersonChange();
        }

        #endregion Control Events

        #region Messaging

        /// <summary>
        /// Clears the errors.
        /// </summary>
        private void ClearErrors()
        {
            nbNotificationBox.Visible = false;
            nbNonBulkNotificationBox.Visible = false;
        }

        /// <summary>
        /// Shows a block level error. This cannot be cleared and hides the block
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        private void ShowBlockLevelError( string errorMessage )
        {
            nbBlockError.Text = errorMessage;
            nbBlockError.Visible = true;
            pnlStepBulkEntry.Visible = false;
        }

        /// <summary>
        /// Shows the bulk level error.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        private void ShowBulkLevelError( string errorMessage )
        {
            nbNotificationBox.Text = errorMessage;
            nbNotificationBox.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Warning;
            nbNotificationBox.Visible = true;
        }

        /// <summary>
        /// Shows the non bulk level error.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        private void ShowNonBulkLevelError( string errorMessage )
        {
            nbNonBulkNotificationBox.Text = errorMessage;
            nbNonBulkNotificationBox.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Warning;
            nbNonBulkNotificationBox.Visible = true;
        }

        #endregion Messaging

        #region Data Interface

        /// <summary>
        /// Gets the rock context.
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
        /// Gets the step program service.
        /// </summary>
        /// <returns></returns>
        private StepProgramService GetStepProgramService()
        {
            if ( _stepProgramService == null )
            {
                var rockContext = GetRockContext();
                _stepProgramService = new StepProgramService( rockContext );
            }

            return _stepProgramService;
        }
        private StepProgramService _stepProgramService = null;

        /// <summary>
        /// Gets the step service.
        /// </summary>
        /// <returns></returns>
        private StepService GetStepService()
        {
            if ( _stepService == null )
            {
                var rockContext = GetRockContext();
                _stepService = new StepService( rockContext );
            }

            return _stepService;
        }
        private StepService _stepService = null;

        /// <summary>
        /// Gets the step program.
        /// </summary>
        /// <returns></returns>
        private StepProgram GetStepProgram()
        {
            var blockSettingStepTypeGuid = GetStepTypeGuidFromBlockAttribute();
            var blockSettingStepProgramGuid = GetStepProgramGuidFromBlockAttribute();
            var pageParamStepProgramId = PageParameter( PageParameterKey.StepProgramId ).AsIntegerOrNull();
            var pageParamStepTypeId = PageParameter( PageParameterKey.StepTypeId ).AsIntegerOrNull();
            const string includes = "StepStatuses, StepTypes.StepTypePrerequisites.PrerequisiteStepType";

            // #1 priority is the block setting for the step type
            if ( blockSettingStepTypeGuid.HasValue )
            {
                if ( _stepProgram == null || _stepProgram.StepTypes.All( st => st.Guid != blockSettingStepTypeGuid.Value ) )
                {
                    var stepProgramService = GetStepProgramService();
                    _stepProgram = stepProgramService.Queryable( includes ).AsNoTracking()
                        .FirstOrDefault( sp => sp.StepTypes.Any( st => st.Guid == blockSettingStepTypeGuid.Value ) );
                }
            }
            // #2 priority is the block setting for the step program
            else if ( blockSettingStepProgramGuid.HasValue )
            {
                if ( _stepProgram == null || _stepProgram.Guid != blockSettingStepProgramGuid.Value )
                {
                    var stepProgramService = GetStepProgramService();
                    _stepProgram = stepProgramService.Queryable( includes ).AsNoTracking()
                        .FirstOrDefault( sp => sp.Guid == blockSettingStepProgramGuid.Value );
                }
            }
            // #3 priority is the page parameter for the step type
            else if ( pageParamStepTypeId.HasValue )
            {
                if ( _stepProgram == null || _stepProgram.StepTypes.All( st => st.Id != pageParamStepTypeId.Value ) )
                {
                    var stepProgramService = GetStepProgramService();
                    _stepProgram = stepProgramService.Queryable( includes ).AsNoTracking()
                        .FirstOrDefault( sp => sp.StepTypes.Any( st => st.Id == pageParamStepTypeId.Value ) );
                }
            }
            // #4 priority is the page parameter for the step program
            else if ( pageParamStepProgramId.HasValue )
            {
                if ( _stepProgram == null || _stepProgram.Id != pageParamStepProgramId.Value )
                {
                    var stepProgramService = GetStepProgramService();
                    _stepProgram = stepProgramService.Queryable( includes ).AsNoTracking()
                        .FirstOrDefault( sp => sp.Id == pageParamStepProgramId.Value );
                }
            }
            // #4 priority is the user's selection
            else
            {
                var selectedStepProgramId = sppProgramPicker.SelectedValue.AsIntegerOrNull();

                if ( !selectedStepProgramId.HasValue && _stepProgram != null )
                {
                    _stepProgram = null;
                }
                else if ( selectedStepProgramId.HasValue && ( _stepProgram == null || _stepProgram.Id != selectedStepProgramId.Value ) )
                {
                    var stepProgramService = GetStepProgramService();
                    _stepProgram = stepProgramService.Queryable( includes ).AsNoTracking()
                        .FirstOrDefault( sp => sp.Id == selectedStepProgramId.Value );
                }
            }

            return _stepProgram;
        }
        private StepProgram _stepProgram = null;

        /// <summary>
        /// Gets the type of the step.
        /// </summary>
        /// <returns></returns>
        private StepType GetStepType()
        {
            var pageParamStepTypeId = PageParameter( PageParameterKey.StepTypeId ).AsIntegerOrNull();
            var blockSettingStepTypeGuid = GetStepTypeGuidFromBlockAttribute();

            // #1 priority is the block setting for the step type
            if ( blockSettingStepTypeGuid.HasValue )
            {
                if ( _stepType == null || _stepType.Guid != blockSettingStepTypeGuid.Value )
                {
                    var stepProgram = GetStepProgram();

                    if ( stepProgram == null )
                    {
                        _stepType = null;
                    }
                    else
                    {
                        _stepType = stepProgram.StepTypes.FirstOrDefault( st => st.Guid == blockSettingStepTypeGuid.Value );
                    }
                }
            }
            // #2 priority is the page param for the step type
            else if ( pageParamStepTypeId.HasValue )
            {
                if ( _stepType == null || _stepType.Id != pageParamStepTypeId.Value )
                {
                    var stepProgram = GetStepProgram();

                    if ( stepProgram == null )
                    {
                        _stepType = null;
                    }
                    else
                    {
                        _stepType = stepProgram.StepTypes.FirstOrDefault( st => st.Id == pageParamStepTypeId.Value );
                    }
                }
            }
            // #3 priority is the user's selection
            else
            {
                var stepTypeId = stpStepTypePicker.SelectedValue.AsIntegerOrNull();

                if ( !stepTypeId.HasValue && _stepType != null )
                {
                    _stepType = null;
                }
                else if ( stepTypeId.HasValue && ( _stepType == null || _stepType.Id != stepTypeId.Value ) )
                {
                    var stepProgram = GetStepProgram();

                    if ( stepProgram == null )
                    {
                        _stepType = null;
                    }
                    else
                    {
                        _stepType = stepProgram.StepTypes.FirstOrDefault( st => st.Id == stepTypeId.Value );
                    }
                }
            }

            return _stepType;
        }
        private StepType _stepType = null;

        /// <summary>
        /// Gets the step status.
        /// </summary>
        /// <returns></returns>
        private StepStatus GetStepStatus()
        {
            var statusId = sspStatusPicker.SelectedValueAsId();

            if ( !statusId.HasValue )
            {
                return null;
            }

            var program = GetStepProgram();

            if ( program == null )
            {
                return null;
            }

            return program.StepStatuses.FirstOrDefault( ss => ss.Id == statusId.Value );
        }

        /// <summary>
        /// Determines whether the person has met prerequisites to add a step of this type.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if has met prerequisites; otherwise, <c>false</c>.
        /// </returns>
        private List<StepType> GetUnmetPrereqs()
        {
            var person = GetPerson();

            if ( person == null )
            {
                return null;
            }

            var stepType = GetStepType();

            if ( stepType == null )
            {
                return null;
            }

            var prerequisiteStepTypes = stepType.StepTypePrerequisites.Select( stp => stp.PrerequisiteStepType );

            var completedStepTypeIds = GetStepService().Queryable().AsNoTracking()
                .Where( s =>
                    s.PersonAlias.PersonId == person.Id &&
                    s.StepStatus != null &&
                    s.StepStatus.IsCompleteStatus )
                .Select( s => s.StepTypeId );

            var unmetPrereqs = prerequisiteStepTypes.Where( st => !completedStepTypeIds.Contains( st.Id ) ).ToList();
            return unmetPrereqs;
        }

        /// <summary>
        /// Determines whether a step can be added because of the step types allow multiple setting.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance [can add because meets allow multiple rule]; otherwise, <c>false</c>.
        /// </returns>
        private bool CanAddBecauseMeetsAllowMultipleRule()
        {
            var person = GetPerson();

            if ( person == null || !person.PrimaryAliasId.HasValue )
            {
                return false;
            }

            var stepType = GetStepType();

            if ( stepType == null )
            {
                return false;
            }

            var stepService = GetStepService();
            return stepService.CanAddBecauseMeetsAllowMultipleRule( person.PrimaryAliasId.Value, stepType );
        }

        /// <summary>
        /// Gets the person service.
        /// </summary>
        /// <returns></returns>
        private PersonService GetPersonService()
        {
            if ( _personService == null )
            {
                var rockContext = GetRockContext();
                _personService = new PersonService( rockContext );
            }

            return _personService;
        }
        private PersonService _personService = null;

        /// <summary>
        /// Gets the person.
        /// </summary>
        /// <returns></returns>
        private Person GetPerson()
        {
            var isSetMode = IsSetMode();

            // #1 priority is the current person in the set if this is set mode
            if ( isSetMode )
            {
                var setPersonId = GetCurrentSetPersonId();

                if ( !setPersonId.HasValue )
                {
                    _person = null;
                }
                else if ( _person == null || ( _person != null && _person.Id != setPersonId.Value ) )
                {
                    var personService = GetPersonService();
                    _person = personService.Queryable().AsNoTracking().FirstOrDefault( p => p.Id == setPersonId.Value );
                }
            }
            // #2 priority is the user selected person
            else
            {
                if ( !ppPersonPicker.PersonId.HasValue )
                {
                    _person = null;
                }
                else if ( _person == null || ( _person != null && _person.Id != ppPersonPicker.PersonId.Value ) )
                {
                    var personService = GetPersonService();
                    _person = personService.Queryable().AsNoTracking().FirstOrDefault( p => p.Id == ppPersonPicker.PersonId.Value );
                }
            }

            return _person;
        }
        private Person _person = null;

        /// <summary>
        /// Clears the block data.
        /// </summary>
        private void ClearBlockData()
        {
            _rockContext = null;
            _stepProgramService = null;
            _stepProgram = null;
            _stepType = null;
            _person = null;
            _personService = null;
        }

        #endregion Data Interface

        #region Set Helpers

        /// <summary>
        /// Determines whether the block is operating on a set of people.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is set mode]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsSetMode()
        {
            return PageParameter( PageParameterKey.PeopleSet ).AsIntegerOrNull().HasValue;
        }

        /// <summary>
        /// Determines whether the current person should just be skipped instead of saving the record.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is skip mode]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsSkipMode()
        {
            if ( !IsSetMode() )
            {
                return false;
            }

            if ( GetStepType() == null )
            {
                return false;
            }

            if ( !CanAddBecauseMeetsAllowMultipleRule() )
            {
                return true;
            }

            var unmetPrereqs = GetUnmetPrereqs();
            return unmetPrereqs != null && unmetPrereqs.Any();
        }

        /// <summary>
        /// Gets the people set.
        /// </summary>
        /// <returns></returns>
        private List<int> GetPersonIdSet()
        {
            if ( _peopleIds == null )
            {
                var setId = PageParameter( PageParameterKey.PeopleSet ).AsIntegerOrNull();

                if ( setId.HasValue )
                {
                    var rockContext = GetRockContext();
                    var entitySetService = new EntitySetService( rockContext );
                    var peopleQuery = entitySetService.GetEntityQuery<Person>( setId.Value ).AsNoTracking();
                    _peopleIds = peopleQuery.Select( p => p.Id ).Distinct().ToList();
                }
            }

            return _peopleIds;
        }
        private List<int> _peopleIds = null;

        /// <summary>
        /// Gets the index of the current person set.
        /// </summary>
        /// <returns></returns>
        private int GetCurrentSetIndex()
        {
            return ViewState[ViewStateKey.SetIndex].ToStringSafe().AsInteger();
        }

        /// <summary>
        /// Sets the index of the current person set.
        /// </summary>
        /// <param name="newIndex">The new index.</param>
        private void SetCurrentSetIndex( int newIndex )
        {
            ViewState[ViewStateKey.SetIndex] = newIndex;
            _person = null;
        }

        /// <summary>
        /// Gets the last index of the set.
        /// </summary>
        /// <returns></returns>
        private int GetLastSetIndex()
        {
            var set = GetPersonIdSet();
            return ( set != null && set.Any() ) ? ( set.Count - 1 ) : 0;
        }

        /// <summary>
        /// Go to the previous person in the set
        /// </summary>
        private void GoToPreviousInSet()
        {
            var index = GetCurrentSetIndex();
            index--;

            if ( index < 0 )
            {
                index = GetLastSetIndex();
            }

            SetCurrentSetIndex( index );
            UpdateSetStatusIndicators();
            BuildNonBulkDynamicControls();
        }

        /// <summary>
        /// Go to the next person in the set
        /// </summary>
        private void GoToNextInSet()
        {
            var index = GetCurrentSetIndex();
            index++;
            var lastIndex = GetLastSetIndex();

            if ( index > lastIndex )
            {
                index = 0;
            }

            SetCurrentSetIndex( index );
            UpdateSetStatusIndicators();
            BuildNonBulkDynamicControls();
        }

        /// <summary>
        /// Updates the set status indicators.
        /// </summary>
        private void UpdateSetStatusIndicators()
        {
            var set = GetPersonIdSet();
            var setCount = set != null ? set.Count : 0;
            var zeroBasedIndex = GetCurrentSetIndex();
            var oneBasedIndex = zeroBasedIndex + 1;
            var isLast = oneBasedIndex == setCount;

            if ( isLast )
            {
                btnNext.Text = "Finish";
            }

            lSetStatus.Text = string.Format( "{0} of {1}", oneBasedIndex, setCount );

            var person = GetPerson();
            lPersonName.Text = person == null ? string.Empty : person.FullName;
        }

        /// <summary>
        /// Gets the current set person identifier based on the index.
        /// </summary>
        /// <returns></returns>
        private int? GetCurrentSetPersonId()
        {
            var index = GetCurrentSetIndex();

            if ( index < 0 )
            {
                return null;
            }

            var set = GetPersonIdSet();

            if ( set == null || index >= set.Count )
            {
                return null;
            }

            return set[index];
        }

        /// <summary>
        /// Gets the current set person's step unique identifier.
        /// </summary>
        /// <returns></returns>
        private Guid? GetCurrentSetPersonStepGuid()
        {
            var personId = GetCurrentSetPersonId();

            if ( !personId.HasValue )
            {
                return null;
            }

            return GetSetStepGuid( personId.Value );
        }

        /// <summary>
        /// Gets the current set person's existing step.
        /// </summary>
        /// <returns></returns>
        private Step GetCurrentSetPersonStep()
        {
            if ( !IsSetMode() )
            {
                return null;
            }

            var exisitingStepGuid = GetCurrentSetPersonStepGuid();

            if ( !exisitingStepGuid.HasValue )
            {
                return null;
            }

            var stepService = GetStepService();
            return stepService.Get( exisitingStepGuid.Value );
        }

        /// <summary>
        /// Gets the step unique identifier for the person in the step. This is to recall steps that have been entered already if the user
        /// presses the previous button.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        private Guid GetSetStepGuid( int personId )
        {
            var personIdStepGuidMap = ViewState[ViewStateKey.SetStepGuids] as Dictionary<int, Guid>;

            if ( personIdStepGuidMap == null )
            {
                personIdStepGuidMap = new Dictionary<int, Guid>();
            }

            if ( personIdStepGuidMap.ContainsKey( personId ) )
            {
                return personIdStepGuidMap[personId];
            }

            var guid = Guid.NewGuid();
            personIdStepGuidMap[personId] = guid;
            ViewState[ViewStateKey.SetStepGuids] = personIdStepGuidMap;
            return guid;
        }

        #endregion Set Helpers
    }
}

