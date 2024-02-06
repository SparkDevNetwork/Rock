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

using Rock.Attribute;
using Rock.Data;
using Rock.Enums.Blocks.Group.Scheduling;
using Rock.Model;
using Rock.ViewModels.Blocks.Group.Scheduling.GroupScheduleToolbox;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

namespace Rock.Blocks.Group.Scheduling
{
    /// <summary>
    /// Allows management of group scheduling for a specific person (worker).
    /// </summary>

    [DisplayName( "Group Schedule Toolbox" )]
    [Category( "Group Scheduling" )]
    [Description( "Allows management of group scheduling for a specific person (worker)." )]
    [IconCssClass( "fa fa-calendar-alt" )]
    [SupportedSiteTypes( Model.SiteType.Web )]
    [ContextAware( typeof( Person ) )]

    #region Block Attributes

    #region Additional Time Sign-Up

    [BooleanField( "Enable Additional Time Sign-Up",
        Key = AttributeKey.EnableAdditionalTimeSignUp,
        Description = "When enabled, a button will allow the individual to sign up for upcoming schedules for their group.",
        DefaultBooleanValue = true,
        Category = AttributeCategory.AdditionalTimeSignUp,
        Order = 0,
        IsRequired = false )]

    [TextField( "Additional Time Sign-Up Button Text",
        Key = AttributeKey.AdditionalTimeSignUpButtonText,
        Description = "The text to display for the Additional Time Sign-Up button.",
        DefaultValue = "Sign Up for Additional Times",
        Category = AttributeCategory.AdditionalTimeSignUp,
        Order = 1,
        IsRequired = true )]

    [CodeEditorField( "Additional Time Sign-Up Header",
        Key = AttributeKey.AdditionalTimeSignUpHeader,
        Description = "Header content to show above the Additional Time Sign-Up panel. <span class='tip tip-lava'></span>",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        Category = AttributeCategory.AdditionalTimeSignUp,
        Order = 2,
        IsRequired = false )]

    [SlidingDateRangeField( "Date Range",
        Key = AttributeKey.AdditionalTimeSignUpDateRange,
        Description = "The date range to allow individuals to sign up for a schedule. Please note that only current and future dates will be accepted. Schedules that have already started will never be displayed.",
        EnabledSlidingDateRangeTypes = "Current, DateRange, Next, Upcoming",
        EnabledSlidingDateRangeUnits = "Day, Week, Month, Year",
        DefaultValue = "Next|6|Week||",
        Category = AttributeCategory.AdditionalTimeSignUp,
        Order = 3,
        IsRequired = true )]

    [IntegerField( "Cutoff Time (Hours)",
        Key = AttributeKey.AdditionalTimeSignUpCutoffTime,
        Description = "Set the cutoff time in hours for hiding schedules that are too close to their start time. Schedules within this cutoff window will not be displayed for sign-up. Schedules that have already started will never be displayed.",
        DefaultIntegerValue = 12,
        Category = AttributeCategory.AdditionalTimeSignUp,
        Order = 4,
        IsRequired = false )]

    [BooleanField( "Require Location for Additional Time Sign-Up",
        Key = AttributeKey.AdditionalTimeSignUpRequireLocation,
        Description = "When enabled, a location will be required when signing up for additional times.",
        DefaultBooleanValue = false,
        Category = AttributeCategory.AdditionalTimeSignUp,
        Order = 5,
        IsRequired = false )]

    [SchedulesField( "Additional Time Sign-Up Schedule Exclusions",
        Key = AttributeKey.AdditionalTimeSignUpScheduleExclusions,
        Description = "Select named schedules that you would like to exclude from all groups on the Additional Time Sign-Up panel.",
        Category = AttributeCategory.AdditionalTimeSignUp,
        Order = 6,
        IsRequired = false )]

    [BooleanField( "Enable Immediate Needs",
        Key = AttributeKey.EnableImmediateNeeds,
        Description = "When enabled, upcoming opportunities that still need individuals will be highlighted.",
        DefaultBooleanValue = false,
        Category = AttributeCategory.AdditionalTimeSignUp,
        Order = 7,
        IsRequired = false )]

    [TextField( "Immediate Need Title",
        Key = AttributeKey.ImmediateNeedTitle,
        Description = "The title to use for the Immediate Need panel.",
        DefaultValue = "Immediate Needs",
        Category = AttributeCategory.AdditionalTimeSignUp,
        Order = 8,
        IsRequired = false )]

    [MemoField( "Immediate Need Introduction",
        Key = AttributeKey.ImmediateNeedIntroduction,
        Description = "The introductory text to show above the Immediate Need panel.",
        DefaultValue = "This group has an immediate need for volunteers. If you're able to assist we would greatly appreciate your help.",
        Category = AttributeCategory.AdditionalTimeSignUp,
        Order = 9,
        IsRequired = false )]

    [IntegerField( "Immediate Need Window (Hours)",
        Key = AttributeKey.ImmediateNeedWindow,
        Description = "The hour range to determine which schedules are in the immediate window. This works with the cutoff setting so ensure that you reduce the cutoff setting to include schedules you will want shown in the Immediate Need panel.",
        DefaultIntegerValue = 0,
        Category = AttributeCategory.AdditionalTimeSignUp,
        Order = 10,
        IsRequired = false )]

    #endregion Additional Time Sign-Up

    #region Current Schedule

    [TextField( "Current Schedule Button Text",
        Key = AttributeKey.CurrentScheduleButtonText,
        Description = "The text to display for the Current Schedule button.",
        DefaultValue = "Current Schedule",
        Category = AttributeCategory.CurrentSchedule,
        Order = 0,
        IsRequired = true )]

    [CodeEditorField( "Current Schedule Header",
        Key = AttributeKey.CurrentScheduleHeader,
        Description = "Header content to show above the Current Schedule panel. <span class='tip tip-lava'></span>",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        Category = AttributeCategory.CurrentSchedule,
        Order = 1,
        IsRequired = false )]

    [BooleanField( "Scheduler Receive Confirmation Emails",
        Key = AttributeKey.SchedulerReceiveConfirmationEmails,
        Description = @"When enabled, the scheduler will receive an email for each confirmation or decline. Note that if a Group's ""Schedule Cancellation Person to Notify"" is defined, that person will automatically receive an email for schedules that are declined or cancelled, regardless of this setting.",
        DefaultBooleanValue = false,
        Category = AttributeCategory.CurrentSchedule,
        Order = 2,
        IsRequired = false )]

    [SystemCommunicationField( "Scheduling Response Email",
        Key = AttributeKey.SchedulingResponseEmail,
        Description = @"The system communication that will be used for sending emails to the scheduler for each confirmation or decline. If a Group's ""Schedule Cancellation Person to Notify"" is defined, this system communication will also be used to send those emails for schedules that are declined or cancelled.",
        DefaultSystemCommunicationGuid = Rock.SystemGuid.SystemCommunication.SCHEDULING_RESPONSE,
        Category = AttributeCategory.CurrentSchedule,
        Order = 3,
        IsRequired = false )]

    [CustomDropdownListField( "Decline Reason Note",
        Key = AttributeKey.DeclineReasonNote,
        Description = "Controls whether a note will be shown for the person to elaborate on why they cannot attend. A schedule's Group Type must also require a decline reason for this setting to have any effect.",
        ListSource = "hide^Hide,optional^Optional,required^Required",
        DefaultValue = "hide",
        Category = AttributeCategory.CurrentSchedule,
        Order = 4,
        IsRequired = false )]

    #endregion Current Schedule

    #region Schedule Preferences

    [BooleanField( "Enable Update Schedule Preferences",
        Key = AttributeKey.EnableUpdateSchedulePreferences,
        Description = "When enabled, a button will allow the individual to set their group reminder preferences and preferred schedule.",
        DefaultBooleanValue = true,
        Category = AttributeCategory.SchedulePreferences,
        Order = 0,
        IsRequired = false )]

    [TextField( "Update Schedule Preferences Button Text",
        Key = AttributeKey.UpdateSchedulePreferencesButtonText,
        Description = "The text to display for the Update Schedule Preferences button.",
        DefaultValue = "Update Schedule Preferences",
        Category = AttributeCategory.SchedulePreferences,
        Order = 1,
        IsRequired = true )]

    [CodeEditorField( "Update Schedule Preferences Header",
        Key = AttributeKey.UpdateSchedulePreferencesHeader,
        Description = "Header content to show above the Update Schedule Preferences panel. <span class='tip tip-lava'></span>",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        Category = AttributeCategory.SchedulePreferences,
        Order = 2,
        IsRequired = false )]

    #endregion Schedule Preferences

    #region Schedule Unavailability

    [BooleanField( "Enable Schedule Unavailability",
        Key = AttributeKey.EnableScheduleUnavailability,
        Description = "When enabled, a button will allow the individual to specify dates or date ranges when they will be unavailable to serve.",
        DefaultBooleanValue = true,
        Category = AttributeCategory.ScheduleUnavailability,
        Order = 0,
        IsRequired = false )]

    [TextField( "Schedule Unavailability Button Text",
        Key = AttributeKey.ScheduleUnavailabilityButtonText,
        Description = "The text to display for the Schedule Unavailability button.",
        DefaultValue = "Schedule Unavailability",
        Category = AttributeCategory.ScheduleUnavailability,
        Order = 1,
        IsRequired = true )]

    [CodeEditorField( "Schedule Unavailability Header",
        Key = AttributeKey.ScheduleUnavailabilityHeader,
        Description = "Header content to show above the Schedule Unavailability panel. <span class='tip tip-lava'></span>",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        Category = AttributeCategory.ScheduleUnavailability,
        Order = 2,
        IsRequired = false )]

    #endregion Schedule Unavailability

    #region Shared Settings (Applies to Multiple Panels)

    [CodeEditorField( "Action Header Lava Template",
        Key = AttributeKey.ActionHeaderLavaTemplate,
        Description = "Header content to show above the action buttons. <span class='tip tip-lava'></span>",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        DefaultValue = AttributeDefault.ActionHeaderLavaTemplate,
        Category = AttributeCategory.SharedSettings,
        Order = 0,
        IsRequired = true )]

    [BooleanField( "Override Hide from Toolbox",
        Key = AttributeKey.OverrideHideFromToolbox,
        Description = @"When enabled this setting will show all schedule enabled groups no matter what their ""Disable Schedule Toolbox Access"" setting is set to.",
        DefaultBooleanValue = false,
        Category = AttributeCategory.SharedSettings,
        Order = 1,
        IsRequired = false )]

    [GroupTypesField( "Include Group Types",
        Key = AttributeKey.IncludeGroupTypes,
        Description = "The group types to display in the list. If none are selected, all group types will be included.",
        Category = AttributeCategory.SharedSettings,
        Order = 2,
        IsRequired = false )]

    [GroupTypesField( "Exclude Group Types",
        Key = AttributeKey.ExcludeGroupTypes,
        Description = "The group types to exclude from the list (only valid if including all groups).",
        Category = AttributeCategory.SharedSettings,
        Order = 3,
        IsRequired = false )]

    [CustomDropdownListField( "Show Campus on Tabs",
        Key = AttributeKey.ShowCampusOnTabs,
        Description = "Optionally shows the group's campus on the tabs.",
        ListSource = "always^Always,never^Never,multiple^When Multiple",
        DefaultValue = "never",
        Category = AttributeCategory.SharedSettings,
        Order = 4,
        IsRequired = false )]

    [CustomDropdownListField( "Schedule List Format",
        Key = AttributeKey.ScheduleListFormat,
        Description = "The format to be used when displaying schedules for schedule preferences and additional time sign-ups.",
        ListSource = "1^Schedule Time,2^Schedule Name,3^Schedule Time and Name",
        DefaultValue = "1",
        Category = AttributeCategory.SharedSettings,
        Order = 5,
        IsRequired = false )]

    #endregion Shared Settings (Applies to Multiple Panels)

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "FDADA51C-C7E6-4ECA-A984-646B42FBFC40" )]
    [Rock.SystemGuid.BlockTypeGuid( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32" )]
    public class GroupScheduleToolbox : RockBlockType
    {
        #region Keys & Constants

        private static class AttributeKey
        {
            // Additional Time Sign-Ups
            public const string EnableAdditionalTimeSignUp = "EnableAdditionalTimeSignUp";
            public const string AdditionalTimeSignUpButtonText = "AdditionalTimeSignUpButtonText";
            public const string AdditionalTimeSignUpHeader = "SignupforAdditionalTimesHeader";
            public const string AdditionalTimeSignUpDateRange = "FutureWeekDateRange";
            public const string AdditionalTimeSignUpRequireLocation = "RequireLocationForAdditionalSignups";
            public const string AdditionalTimeSignUpScheduleExclusions = "AdditionalTimeSignUpScheduleExclusions";
            public const string AdditionalTimeSignUpCutoffTime = "AdditionalTimeSignUpCutoffTime";

            public const string EnableImmediateNeeds = "EnableImmediateNeeds";
            public const string ImmediateNeedTitle = "ImmediateNeedTitle";
            public const string ImmediateNeedIntroduction = "ImmediateNeedIntroduction";
            public const string ImmediateNeedWindow = "ImmediateNeedWindow";

            // Current Schedule
            public const string CurrentScheduleButtonText = "CurrentScheduleButtonText";
            public const string CurrentScheduleHeader = "CurrentScheduleHeader";
            public const string SchedulerReceiveConfirmationEmails = "SchedulerReceiveConfirmationEmails";
            public const string SchedulingResponseEmail = "SchedulingResponseEmail";
            public const string DeclineReasonNote = "DeclineReasonNote";

            // Schedule Preferences
            public const string EnableUpdateSchedulePreferences = "EnableUpdateSchedulePreferences";
            public const string UpdateSchedulePreferencesButtonText = "UpdateSchedulePreferencesButtonText";
            public const string UpdateSchedulePreferencesHeader = "UpdateSchedulePreferencesHeader";

            // Schedule Unavailability
            public const string EnableScheduleUnavailability = "EnableScheduleUnavailability";
            public const string ScheduleUnavailabilityButtonText = "ScheduleUnavailabilityButtonText";
            public const string ScheduleUnavailabilityHeader = "ScheduleUnavailabilityHeader";

            // Shared Settings (Applies to Multiple Panels)
            public const string ActionHeaderLavaTemplate = "ActionHeaderLavaTemplate";
            public const string OverrideHideFromToolbox = "OverrideHideFromToolbox";
            public const string ScheduleListFormat = "ScheduleListFormat";
            public const string IncludeGroupTypes = "IncludeGroupTypes";
            public const string ExcludeGroupTypes = "ExcludeGroupTypes";
            public const string ShowCampusOnTabs = "ShowCampusOnTabs";
        }

        private static class AttributeCategory
        {
            public const string AdditionalTimeSignUp = "Additional Time Sign-Up";
            public const string CurrentSchedule = "Current Schedule";
            public const string SchedulePreferences = "Schedule Preferences";
            public const string ScheduleUnavailability = "Schedule Unavailability";
            public const string SharedSettings = "Shared Settings (Applies to Multiple Panels)";
        }

        private static class AttributeDefault
        {
            public const string ActionHeaderLavaTemplate = "<h4>Actions</h4>";
        }

        private static class NavigationUrlKey
        {
            public const string DeclineReasonPage = "DeclineReasonPage";
        }

        private static class PageParameterKey
        {
            public const string AttendanceId = "AttendanceId";
            public const string IsConfirmed = "IsConfirmed";
            public const string ReturnUrl = "ReturnUrl";
            public const string ToolboxActionType = "ToolboxActionType";
            public const string ToolboxGroupId = "ToolboxGroupId";
        }

        protected const string ALL_GROUPS_STRING = "All Groups";
        protected const string NO_LOCATION_PREFERENCE = "No Location Preference";

        #endregion Keys & Constants

        #region Fields

        private IDictionary<string, string> _pageParameters;

        private int? _scheduleListFormat;

        #endregion Fields

        #region Properties

        public IDictionary<string, string> PageParameters
        {
            get
            {
                if ( _pageParameters == null )
                {
                    _pageParameters = this.RequestContext?.GetPageParameters() ?? new Dictionary<string, string>();
                }

                return _pageParameters;
            }
        }

        /// <summary>
        /// Gets the current person.
        /// </summary>
        public Person CurrentPerson => this.RequestContext.CurrentPerson;

        /// <summary>
        /// Gets the schedule list format.
        /// </summary>
        public int ScheduleListFormat
        {
            get
            {
                if ( !_scheduleListFormat.HasValue )
                {
                    _scheduleListFormat = GetAttributeValue( AttributeKey.ScheduleListFormat ).AsInteger();
                }

                return _scheduleListFormat.Value;
            }
        }

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = new InitializationBox();

                SetBoxInitialState( rockContext, box );

                return box;
            }
        }

        /// <summary>
        /// Sets the initial state of the box.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="box">The box.</param>
        private void SetBoxInitialState( RockContext rockContext, InitializationBox box )
        {
            var toolboxData = GetCommonToolboxData( rockContext );
            if ( toolboxData.ErrorMessage.IsNotNullOrWhiteSpace() )
            {
                box.ErrorMessage = toolboxData.ErrorMessage;
                return;
            }

            box.IsSchedulePreferencesEnabled = toolboxData.IsSchedulePreferencesEnabled;
            box.IsScheduleUnavailabilityEnabled = toolboxData.IsScheduleUnavailabilityEnabled;
            box.IsAdditionalTimeSignUpsEnabled = toolboxData.IsAdditionalTimeSignUpEnabled;

            SetButtonText( box );
            SetDynamicContentText( box );

            box.ToolboxActionType = toolboxData.ToolboxActionType;
            box.SchedulableFamilyMembers = toolboxData.SchedulableFamilyMemberPeople.Select( fmp => fmp.Person ).ToListItemBagList();

            box.SecurityGrantToken = GetSecurityGrantToken();
        }

        /// <summary>
        /// Gets the common toolbox data needed for the block to operate.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="config">A configuration object to dictate how toolbox data should be loaded.</param>
        /// <returns>The common toolbox data needed for the board to operate.</returns>
        private ToolboxData GetCommonToolboxData( RockContext rockContext, GetToolboxDataConfig config = null )
        {
            var toolboxData = new ToolboxData();

            config = config ?? new GetToolboxDataConfig();

            var block = new BlockService( rockContext ).Get( this.BlockId );
            block.LoadAttributes( rockContext );

            SetSubFeatureConfiguration( toolboxData );

            toolboxData.ToolboxActionType = GetToolboxActionType( config, toolboxData );
            if ( config.ActionTypeOverride.HasValue && toolboxData.ToolboxActionType != config.ActionTypeOverride.Value )
            {
                toolboxData.ErrorMessage = "The specified action is not currently supported.";
                return toolboxData;
            }

            SetToolboxPeople( rockContext, toolboxData, config.SelectedPersonGuidOverride );
            if ( toolboxData.ToolboxPerson == null
                || toolboxData.SelectedPerson == null
                || toolboxData.SelectedPrimaryAlias == null )
            {
                toolboxData.ErrorMessage = "Unable to load Group Schedule Toolbox data for selected individual.";
                return toolboxData;
            }

            if ( config.SchedulableGroupLoadingMode != SchedulableGroupLoadingMode.DoNotLoad )
            {
                toolboxData.SchedulableGroups = GetSchedulableGroups( rockContext, toolboxData.SelectedPerson, config.SchedulableGroupLoadingMode );

                var selectedGroupId = GetEntityIdFromPageParameterOrOverride<Rock.Model.Group>( PageParameterKey.ToolboxGroupId, rockContext, config.EntityKeyOverrides );
                if ( selectedGroupId.HasValue )
                {
                    toolboxData.SelectedGroup = toolboxData.SchedulableGroups?.FirstOrDefault( g => g.Group.Id == selectedGroupId.Value );
                }

                if ( toolboxData.SelectedGroup == null )
                {
                    toolboxData.SelectedGroup = toolboxData.SchedulableGroups?.FirstOrDefault();
                }
            }

            return toolboxData;
        }

        /// <summary>
        /// Sets which sub-features of the group schedule toolbox are enabled, as well as any supporting configuration values.
        /// </summary>
        /// <param name="toolboxData">The toolbox data onto which to set the sub-feature configuration.</param>
        private void SetSubFeatureConfiguration( ToolboxData toolboxData )
        {
            toolboxData.IsSchedulePreferencesEnabled = GetAttributeValue( AttributeKey.EnableUpdateSchedulePreferences ).AsBoolean();
            toolboxData.IsScheduleUnavailabilityEnabled = GetAttributeValue( AttributeKey.EnableScheduleUnavailability ).AsBoolean();
            toolboxData.IsAdditionalTimeSignUpEnabled = GetAttributeValue( AttributeKey.EnableAdditionalTimeSignUp ).AsBoolean();
            toolboxData.IsLocationRequiredForAdditionalTimeSignUp = GetAttributeValue( AttributeKey.AdditionalTimeSignUpRequireLocation ).AsBoolean();
            toolboxData.IsImmediateNeedsEnabled = GetAttributeValue( AttributeKey.EnableImmediateNeeds ).AsBoolean();
        }

        /// <summary>
        /// Gets the toolbox action type from the config override or URL query string, ensuring the selected type is actually
        /// enabled in block settings, and falling back to a default value if not provided or not enabled.
        /// </summary>
        /// <param name="config">A configuration object to dictate how toolbox data should be loaded.</param>
        /// <param name="toolboxData">The toolbox data indicating which sub-features are enabled.</param>
        /// <returns>The toolbox action type.</returns>
        private ToolboxActionType GetToolboxActionType( GetToolboxDataConfig config, ToolboxData toolboxData )
        {
            ToolboxActionType? selectedActionType = null;

            if ( config.ActionTypeOverride.HasValue )
            {
                selectedActionType = config.ActionTypeOverride.Value;
            }
            else if ( HasPageParameter( PageParameterKey.ToolboxActionType )
                && Enum.TryParse( this.PageParameter( PageParameterKey.ToolboxActionType ), out ToolboxActionType pageParamActionType ) )
            {
                selectedActionType = pageParamActionType;
            }

            var authorizedActionType = ToolboxActionType.ViewCurrent; // Start with the always-authorized default.
            if ( ( selectedActionType == ToolboxActionType.UpdatePreferences && toolboxData.IsSchedulePreferencesEnabled )
                || ( selectedActionType == ToolboxActionType.SignUp && toolboxData.IsAdditionalTimeSignUpEnabled ) )
            {
                authorizedActionType = selectedActionType.Value;
            }

            return authorizedActionType;
        }

        /// <summary>
        /// Gets whether the current page has the specified parameter.
        /// </summary>
        /// <param name="pageParameterKey">The page parameter key to check.</param>
        /// <returns>Whether the current page has the specified parameter.</returns>
        private bool HasPageParameter( string pageParameterKey )
        {
            return this.PageParameters.ContainsKey( pageParameterKey );
        }

        /// <summary>
        /// Sets the toolbox person and their family members from the request context or the current person.
        /// The selected person and primary person alias will then be set; these could represent the toolbox
        /// person themself or one of their family members.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="toolboxData">The toolbox data onto which to set the toolbox people.</param>
        /// <param name="selectedPersonGuidOverride">An optional person unique identifier to override that of the toolbox person.</param>
        private void SetToolboxPeople( RockContext rockContext, ToolboxData toolboxData, Guid? selectedPersonGuidOverride )
        {
            toolboxData.ToolboxPerson = this.RequestContext.GetContextEntity<Person>();
            if ( toolboxData.ToolboxPerson == null )
            {
                toolboxData.ToolboxPerson = this.CurrentPerson;
            }

            if ( toolboxData.ToolboxPerson == null )
            {
                // We don't know who this toolbox should represent.
                return;
            }

            // Default the selected person to the toolbox person if another family member wasn't specified.
            if ( !selectedPersonGuidOverride.HasValue )
            {
                selectedPersonGuidOverride = toolboxData.ToolboxPerson.Guid;
            }

            // Get the toolbox person's family member people, noting which of them are schedulable.
            var overrideHideFromToolbox = GetAttributeValue( AttributeKey.OverrideHideFromToolbox ).AsBoolean();
            var schedulableGroupMembersQry = new GroupMemberService( rockContext )
                .Queryable()
                .Where( gm =>
                    gm.Group.IsActive
                    && !gm.Group.IsArchived
                    && !gm.Group.DisableScheduling
                    && ( overrideHideFromToolbox || !gm.Group.DisableScheduleToolboxAccess )
                    && gm.Group.GroupType.IsSchedulingEnabled
                    && gm.GroupMemberStatus == GroupMemberStatus.Active
                );

            var primaryAliasQry = new PersonAliasService( rockContext ).GetPrimaryAliasQuery();

            var familyMemberPeople = new PersonService( rockContext )
                .GetFamilyMembers( toolboxData.ToolboxPerson.Id, true )
                .OrderByDescending( fm => fm.PersonId == toolboxData.ToolboxPerson.Id ) // Always place the toolbox person first in this list.
                .ThenBy( fm => fm.Person.AgeClassification )
                .ThenBy( fm => fm.Person.Gender )
                .Select( fm => new FamilyMemberPerson
                {
                    Person = fm.Person,
                    PrimaryAlias = primaryAliasQry.Where( pa => pa.PersonId == fm.PersonId ).FirstOrDefault(),
                    IsSchedulable = (
                        fm.Person.Id == toolboxData.ToolboxPerson.Id // The toolbox person themself should always be considered "schedulable."
                        || schedulableGroupMembersQry.Any( gm => gm.PersonId == fm.PersonId )
                    )
                } )
                // Only include members who have a primary alias, as we require this value for many toolbox operations.
                .Where( fmp => fmp.PrimaryAlias != null )
                .ToList();

            // Set the schedulable family member people.
            toolboxData.SchedulableFamilyMemberPeople = familyMemberPeople
                .Where( fmp => fmp.IsSchedulable )
                .ToList();

            // Find the selected person from those who are schedulable.
            var selectedFamilyMemberPerson = toolboxData.SchedulableFamilyMemberPeople
                .FirstOrDefault( fmp => fmp.Person.Guid == selectedPersonGuidOverride.Value );

            if ( selectedFamilyMemberPerson == null )
            {
                // We weren't able to find a schedulable family member based on the selected person unique identifier.
                return;
            }

            toolboxData.SelectedPerson = selectedFamilyMemberPerson.Person;
            toolboxData.SelectedPrimaryAlias = selectedFamilyMemberPerson.PrimaryAlias;

            // Finally, set all family member people, regardless of whether they're schedulable.
            // Place the selected person first in this list, then sort by age classification, gender.
            toolboxData.AllFamilyMemberPeople = familyMemberPeople
                .OrderByDescending( fmp => fmp.Person.Id == toolboxData.SelectedPerson.Id )
                .ThenBy( fmp => fmp.Person.AgeClassification )
                .ThenBy( fmp => fmp.Person.Gender )
                .ToList();
        }

        /// <summary>
        /// Gets the schedulable groups for the selected person.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="selectedPerson">The selected person.</param>
        /// <param name="loadingMode">How to load the selected person's schedulable groups.</param>
        /// <returns>The schedulable groups for the selected person.</returns>
        private List<SchedulableGroup> GetSchedulableGroups( RockContext rockContext, Person selectedPerson, SchedulableGroupLoadingMode loadingMode )
        {
            var overrideHideFromToolbox = GetAttributeValue( AttributeKey.OverrideHideFromToolbox ).AsBoolean();

            var groupMembersQry = new GroupMemberService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( gm =>
                    gm.Group.IsActive
                    && !gm.Group.IsArchived
                    && !gm.Group.DisableScheduling
                    && ( overrideHideFromToolbox || !gm.Group.DisableScheduleToolboxAccess )
                    && gm.Group.GroupType.IsSchedulingEnabled
                    && gm.GroupMemberStatus == GroupMemberStatus.Active
                    && gm.PersonId == selectedPerson.Id
                );

            if ( loadingMode == SchedulableGroupLoadingMode.LoadOnlyGroupsHavingLocationSchedules )
            {
                groupMembersQry = groupMembersQry
                    .Where( gm =>
                        gm.Group.GroupLocations.Any( gl => gl.Schedules.Any() )
                    );
            }

            var includeGroupTypeGuids = GetAttributeValue( AttributeKey.IncludeGroupTypes ).SplitDelimitedValues().AsGuidList();
            var excludeGroupTypeGuids = GetAttributeValue( AttributeKey.ExcludeGroupTypes ).SplitDelimitedValues().AsGuidList();

            if ( includeGroupTypeGuids.Any() )
            {
                groupMembersQry = groupMembersQry.Where( gm => includeGroupTypeGuids.Contains( gm.Group.GroupType.Guid ) );
            }
            else if ( excludeGroupTypeGuids.Any() )
            {
                groupMembersQry = groupMembersQry.Where( gm => !excludeGroupTypeGuids.Contains( gm.Group.GroupType.Guid ) );
            }

            return groupMembersQry
                .Include( gm => gm.Group.Campus )
                .GroupBy( gm => gm.GroupId )
                .Select( g => new SchedulableGroup
                {
                    Group = g.FirstOrDefault().Group,
                    GroupMember = g.OrderBy( gm => gm.GroupRole.IsLeader ).FirstOrDefault()
                } )
                .OrderBy( g => g.Group.Order )
                .ThenBy( g => g.Group.Name )
                .ToList();
        }

        /// <summary>
        /// Gets the <see cref="IEntity"/> integer ID value if it exists in the override collection or can be parsed from page parameters,
        /// or <see langword="null"/> if not.
        /// <para>
        /// The page parameter's value may be an integer ID (if predictable IDs are allowed by site settings), a Guid, or an IdKey.
        /// </para>
        /// </summary>
        /// <typeparam name="T">The <see cref="IEntity"/> type whose ID should be parsed.</typeparam>
        /// <param name="pageParameterKey">The key of the page parameter from which to parse the ID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="entityKeyOverrides">Optional entity keys to override page parameters and person preferences.</param>
        /// <returns>The <see cref="IEntity"/> integer ID value if it exists in the override collection or can be parsed from page parameters,
        /// or <see langword="null"/> if not.</returns>
        private int? GetEntityIdFromPageParameterOrOverride<T>( string pageParameterKey, RockContext rockContext, Dictionary<string, string> entityKeyOverrides = null ) where T : IEntity
        {
            string entityKey = null;
            if ( entityKeyOverrides?.TryGetValue( pageParameterKey, out entityKey ) != true )
            {
                // Only look in page parameters if the value wasn't overridden.
                entityKey = PageParameter( pageParameterKey );
            }

            if ( entityKey.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var entityTypeId = EntityTypeCache.GetId( typeof( T ) );
            if ( !entityTypeId.HasValue )
            {
                return null;
            }

            return Reflection.GetEntityIdForEntityType( entityTypeId.Value, entityKey, !PageCache.Layout.Site.DisablePredictableIds, rockContext );
        }

        /// <summary>
        /// Sets button text for the sub-features of the group schedule toolbox.
        /// </summary>
        /// <param name="box">The group schedule toolbox initialization box onto which to set the button text.</param>
        private void SetButtonText( InitializationBox box )
        {
            box.CurrentScheduleButtonText = GetAttributeValue( AttributeKey.CurrentScheduleButtonText );
            box.SchedulePreferencesButtonText = GetAttributeValue( AttributeKey.UpdateSchedulePreferencesButtonText );
            box.ScheduleUnavailabilityButtonText = GetAttributeValue( AttributeKey.ScheduleUnavailabilityButtonText );
            box.AdditionalTimeSignUpsButtonText = GetAttributeValue( AttributeKey.AdditionalTimeSignUpButtonText );
        }

        /// <summary>
        /// Sets dynamic content text using Lava templates, Etc.
        /// </summary>
        /// <param name="box">The group schedule toolbox initialization box onto which to set the dynamic content text.</param>
        private void SetDynamicContentText( InitializationBox box )
        {
            var commonMergeFields = this.RequestContext.GetCommonMergeFields();

            box.ActionHeaderHtml = GetAttributeValue( AttributeKey.ActionHeaderLavaTemplate ).ResolveMergeFields( commonMergeFields );
            box.CurrentScheduleHeaderHtml = GetAttributeValue( AttributeKey.CurrentScheduleHeader ).ResolveMergeFields( commonMergeFields );
            box.SchedulePreferencesHeaderHtml = GetAttributeValue( AttributeKey.UpdateSchedulePreferencesHeader ).ResolveMergeFields( commonMergeFields );
            box.ScheduleUnavailabilityHeaderHtml = GetAttributeValue( AttributeKey.ScheduleUnavailabilityHeader ).ResolveMergeFields( commonMergeFields );
            box.AdditionalTimeSignUpsHeaderHtml = GetAttributeValue( AttributeKey.AdditionalTimeSignUpHeader ).ResolveMergeFields( commonMergeFields );

            // Non-Lava-enabled dynamic content:
            box.ImmediateNeedsTitle = GetAttributeValue( AttributeKey.ImmediateNeedTitle );
            box.ImmediateNeedsIntroduction = GetAttributeValue( AttributeKey.ImmediateNeedIntroduction );
        }

        /// <summary>
        /// Gets the security grant token that will be used by UI controls on this block to ensure they have the proper permissions.
        /// </summary>
        /// <returns>A string that represents the security grant token.</returns>
        private string GetSecurityGrantToken()
        {
            return new Rock.Security.SecurityGrant().ToToken();
        }

        /// <summary>
        /// Gets information about the selected person's current schedule.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="selectedPersonGuid">The selected person's unique identifier.</param>
        /// <param name="errorMessage">A friendly error message to describe any problems encountered.</param>
        /// <returns>An object containing information about the selected person's current schedule.</returns>
        private CurrentScheduleBag GetCurrentSchedule( RockContext rockContext, Guid selectedPersonGuid, out string errorMessage )
        {
            errorMessage = null;

            var config = new GetToolboxDataConfig
            {
                SelectedPersonGuidOverride = selectedPersonGuid,
                ActionTypeOverride = ToolboxActionType.ViewCurrent,
                SchedulableGroupLoadingMode = SchedulableGroupLoadingMode.LoadAll
            };

            var toolboxData = GetCommonToolboxData( rockContext, config );
            if ( toolboxData.ErrorMessage.IsNotNullOrWhiteSpace() )
            {
                errorMessage = toolboxData.ErrorMessage;
                return null;
            }

            var currentScheduleRows = GetCurrentScheduleRows( rockContext, toolboxData.SelectedPerson );

            return new CurrentScheduleBag
            {
                ScheduleRows = currentScheduleRows,
                PersonGroupScheduleFeedUrl = GetPersonGroupScheduleFeedUrl( currentScheduleRows, toolboxData.SelectedPrimaryAlias.Guid ),
                FamilyMembers = toolboxData.AllFamilyMemberPeople.Select( fmp => fmp.Person ).ToListItemBagList(),
                SchedulableGroups = GetGroupBags( toolboxData.SchedulableGroups )
            };
        }

        /// <summary>
        /// Gets the current schedule rows for the selected person.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="selectedPerson">The selected person.</param>
        /// <returns>The current schedule rows for the selected person.</returns>
        private List<ScheduleRowBag> GetCurrentScheduleRows( RockContext rockContext, Person selectedPerson )
        {
            var attendanceService = new AttendanceService( rockContext );

            // Get pending attendance records.
            var rows = attendanceService
                .GetPendingScheduledConfirmations()
                .Where( a => a.PersonAlias.PersonId == selectedPerson.Id )
                .Select( a => new
                {
                    a.Guid,
                    a.Occurrence.Group,
                    a.Occurrence.Location,
                    a.Occurrence.Schedule,
                    PersonAlias = ( PersonAlias ) null,
                    OccurrenceStartDate = a.Occurrence.OccurrenceDate,
                    OccurrenceEndDate = a.Occurrence.OccurrenceDate,
                    ConfirmationStatus = ToolboxScheduleRowConfirmationStatus.Pending
                } )
                .ToList();

            var today = RockDateTime.Now.Date;

            // Get confirmed attendance records.
            var confirmedAttendances = attendanceService
                .GetConfirmedScheduled()
                .Where( a =>
                    a.Occurrence.OccurrenceDate >= today
                    && a.PersonAlias.PersonId == selectedPerson.Id
                )
                .Select( a => new
                {
                    a.Guid,
                    a.Occurrence.Group,
                    a.Occurrence.Location,
                    a.Occurrence.Schedule,
                    PersonAlias = ( PersonAlias ) null,
                    OccurrenceStartDate = a.Occurrence.OccurrenceDate,
                    OccurrenceEndDate = a.Occurrence.OccurrenceDate,
                    ConfirmationStatus = ToolboxScheduleRowConfirmationStatus.Confirmed
                } )
                .ToList();

            rows.AddRange( confirmedAttendances );

            // Get declined attendance records.
            var declinedAttendances = attendanceService
                .GetDeclinedScheduleConfirmations()
                .Where( a =>
                    a.Occurrence.OccurrenceDate >= today
                    && a.PersonAlias.PersonId == selectedPerson.Id
                )
                .Select( a => new
                {
                    a.Guid,
                    a.Occurrence.Group,
                    a.Occurrence.Location,
                    a.Occurrence.Schedule,
                    PersonAlias = ( PersonAlias ) null,
                    OccurrenceStartDate = a.Occurrence.OccurrenceDate,
                    OccurrenceEndDate = a.Occurrence.OccurrenceDate,
                    ConfirmationStatus = ToolboxScheduleRowConfirmationStatus.Declined
                } )
                .ToList();

            rows.AddRange( declinedAttendances );

            // Get person schedule exclusions (including those of family members).
            var familyMemberAliasIds = new PersonService( rockContext )
                .GetFamilyMembers( selectedPerson.Id, true )
                .SelectMany( fm => fm.Person.Aliases )
                .Select( a => a.Id )
                .ToList();

            var personScheduleExclusions = new PersonScheduleExclusionService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Include( e => e.PersonAlias.Person )
                .Where( e =>
                    familyMemberAliasIds.Contains( e.PersonAliasId.Value )
                    && (
                        e.StartDate >= today
                        || e.EndDate >= today
                    )
                )
                .Select( e => new
                {
                    e.Guid,
                    e.Group,
                    Location = ( Location ) null,
                    Schedule = ( Schedule ) null,
                    e.PersonAlias,
                    OccurrenceStartDate = DbFunctions.TruncateTime( e.StartDate ).Value,
                    OccurrenceEndDate = DbFunctions.TruncateTime( e.EndDate ).Value,
                    ConfirmationStatus = ToolboxScheduleRowConfirmationStatus.Unavailable
                } )
                .ToList();

            rows.AddRange( personScheduleExclusions );

            // Sort and project all of the above into a final collection of rows.
            return rows
                .OrderBy( r => r.OccurrenceStartDate )
                .ThenBy( r => r.OccurrenceEndDate )
                .ThenBy( r => r.Schedule?.Order ?? int.MaxValue )
                .ThenBy( r => r.Schedule?.Name.IsNotNullOrWhiteSpace() )
                .ThenBy( r => r.Schedule?.Name )
                .ThenBy( r => r.Schedule?.Id ?? int.MaxValue )
                .ThenBy( r => r.Group?.Name.IsNotNullOrWhiteSpace() )
                .ThenBy( r => r.Group?.Name )
                .Select( r =>
                {
                    var familyMemberName = r.PersonAlias != null && r.PersonAlias.PersonId != selectedPerson.Id
                        ? r.PersonAlias.Person.FullName
                        : null;

                    var groupName = r.Group?.Name;
                    var locationName = r.Location?.ToString( true );
                    var scheduleName = r.Schedule?.ToString( true );

                    DateTimeOffset? occurrenceEndDate = null;

                    if ( r.ConfirmationStatus == ToolboxScheduleRowConfirmationStatus.Unavailable )
                    {
                        if ( groupName == null )
                        {
                            groupName = ALL_GROUPS_STRING;
                        }

                        if ( ( r.OccurrenceEndDate - r.OccurrenceStartDate ).Days > 1 )
                        {
                            occurrenceEndDate = r.OccurrenceEndDate;
                        }
                    }

                    return new ScheduleRowBag
                    {
                        EntityGuid = r.Guid,
                        FamilyMemberName = familyMemberName,
                        GroupName = groupName,
                        LocationName = locationName,
                        OccurrenceStartDate = r.OccurrenceStartDate,
                        OccurrenceEndDate = occurrenceEndDate,
                        ScheduleName = scheduleName,
                        ConfirmationStatus = r.ConfirmationStatus
                    };
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the selected person's group schedule feed if they have any confirmed schedules, or null if not.
        /// </summary>
        /// <param name="currentScheduleRows">The selected person's current schedule rows.</param>
        /// <param name="selectedPrimaryAliasGuid">The selected person's primary alias unique identifier.</param>
        /// <returns>The selected person's group schedule feed if they have any confirmed schedules, or null if not.</returns>
        private string GetPersonGroupScheduleFeedUrl( List<ScheduleRowBag> currentScheduleRows, Guid selectedPrimaryAliasGuid )
        {
            if ( currentScheduleRows?.Any( r => r.ConfirmationStatus == ToolboxScheduleRowConfirmationStatus.Confirmed ) != true )
            {
                return null;
            }

            return $"{GlobalAttributesCache.Get().GetValue( "PublicApplicationRoot" )}GetPersonGroupScheduleFeed.ashx?paguid={selectedPrimaryAliasGuid}";
        }

        /// <summary>
        /// Gets the group bags for the selected person's schedulable groups.
        /// </summary>
        /// <param name="groups">The selected person's schedulable groups.</param>
        /// <returns>The group bags for the selected person's schedulable groups.</returns>
        private List<GroupBag> GetGroupBags( List<SchedulableGroup> groups )
        {
            if ( groups?.Any() != true )
            {
                return null;
            }

            var distinctCampusCount = groups
                .Select( g => g.Group.Campus?.Id )
                //.Where( id => id.HasValue ) // Also count null as a "distinct" value, so exclude this where clause.
                .Distinct()
                .Count();

            var showCampusOnTabs = GetAttributeValue( AttributeKey.ShowCampusOnTabs )?.ToLower();
            var includeCampusName = showCampusOnTabs == "always" || ( showCampusOnTabs == "multiple" && distinctCampusCount > 1 );

            return groups.Select( g =>
                {
                    var campusName = includeCampusName
                        ? g.Group.Campus?.ShortCode // Prefer short code.
                        : null;

                    if ( includeCampusName && campusName.IsNullOrWhiteSpace() )
                    {
                        campusName = g.Group.Campus?.Name; // Fall back to name.
                    }

                    return new GroupBag
                    {
                        Guid = g.Group.Guid,
                        IdKey = g.Group.IdKey,
                        Name = g.Group.Name?.ToString(),
                        CampusName = campusName
                    };
                } )
                .ToList();
        }

        /// <summary>
        /// Performs the specified action on a current schedule row.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="bag">The information needed to perform the specified action.</param>
        /// <param name="errorMessage">A friendly error message to describe any problems encountered.</param>
        /// <returns>An object containing the outcome of the request.</returns>
        private PerformScheduleRowActionResponseBag PerformScheduleRowAction( RockContext rockContext, PerformScheduleRowActionRequestBag bag, out string errorMessage )
        {
            errorMessage = null;

            if ( bag == null )
            {
                // No request data was provided.
                errorMessage = "Unable to perform schedule row action.";
                return null;
            }

            // Get the current schedule rows, so we can ensure this request is valid for the selected person.
            var config = new GetToolboxDataConfig
            {
                SelectedPersonGuidOverride = bag.SelectedPersonGuid,
                ActionTypeOverride = ToolboxActionType.ViewCurrent
            };

            var toolboxData = GetCommonToolboxData( rockContext, config );
            if ( toolboxData.ErrorMessage.IsNotNullOrWhiteSpace() )
            {
                errorMessage = toolboxData.ErrorMessage;
                return null;
            }

            var currentScheduleRows = GetCurrentScheduleRows( rockContext, toolboxData.SelectedPerson );

            // Go ahead and assign the feed URL value based on the current row statuses, just in case we exit this method early.
            // We'll update this value if necessary, below.
            var response = new PerformScheduleRowActionResponseBag
            {
                PersonGroupScheduleFeedUrl = GetPersonGroupScheduleFeedUrl( currentScheduleRows, toolboxData.SelectedPrimaryAlias.Guid )
            };

            var friendlyErrorMessage = $"Unable to {bag.ActionType.ConvertToString().ToLower()} schedule.";

            var scheduleRow = currentScheduleRows.FirstOrDefault( r => r.EntityGuid == bag.EntityGuid );
            if ( scheduleRow == null )
            {
                // The specified row doesn't exist in the current list of rows.

                // If the specified action is to delete this schedule row, fail silently by returning a null "new status",
                // which will instruct the client to remove the row from the UI. Maybe an admin or family member already
                // deleted this row, and the current individual has a stale list of rows in their current UI.
                if ( bag.ActionType == ToolboxScheduleRowActionType.Delete )
                {
                    return response;
                }

                // Otherwise, return an error.
                errorMessage = friendlyErrorMessage;
                return null;
            }

            if ( bag.ActionType == ToolboxScheduleRowActionType.Delete )
            {
                // Delete existing person schedule exclusion record.
                var personScheduleExclusionService = new PersonScheduleExclusionService( rockContext );

                var personScheduleExclusion = personScheduleExclusionService.Get( bag.EntityGuid );
                if ( personScheduleExclusion != null )
                {
                    var scheduleExclusionChildren = personScheduleExclusionService
                        .Queryable()
                        .Where( e => e.ParentPersonScheduleExclusionId == personScheduleExclusion.Id );

                    foreach ( var scheduleExclusionChild in scheduleExclusionChildren )
                    {
                        scheduleExclusionChild.ParentPersonScheduleExclusionId = null;
                    }

                    personScheduleExclusionService.Delete( personScheduleExclusion );
                    rockContext.SaveChanges();
                }

                // Don't set a "new status" on the response, so the client will remove this row from the UI.
            }
            else
            {
                // Modify existing attendance record.
                var attendanceService = new AttendanceService( rockContext );

                // Get all the supporting data we'll need to operate below.
                // Note that some of these entities aren't directly needed here, but will be needed if sending emails.
                var attendance = attendanceService
                    .Queryable()
                    .Include( a => a.Occurrence.Group.GroupType )
                    .Include( a => a.Occurrence.Group.ScheduleCancellationPersonAlias.Person )
                    .Include( a => a.Occurrence.Schedule )
                    .Include( a => a.PersonAlias.Person )
                    .Include( a => a.ScheduledByPersonAlias.Person )
                    .FirstOrDefault( a => a.Guid == bag.EntityGuid );

                if ( attendance == null )
                {
                    ExceptionLogService.LogException( $@"{nameof( GroupScheduleToolbox )}: {friendlyErrorMessage} No Attendance record found with Guid = '{bag.EntityGuid}' (Scheduled Person ID = {toolboxData.SelectedPerson.Id})." );

                    errorMessage = friendlyErrorMessage;
                    return null;
                }

                var shouldTrySendingResponseEmails = true;

                if ( bag.ActionType == ToolboxScheduleRowActionType.Accept )
                {
                    if ( !attendance.IsScheduledPersonConfirmed() )
                    {
                        attendanceService.ScheduledPersonConfirm( attendance.Id );
                        rockContext.SaveChanges();
                    }

                    response.NewStatus = ToolboxScheduleRowConfirmationStatus.Confirmed;
                }
                else // ToolboxScheduleRowActionType.Cancel || ToolboxScheduleRowActionType.Decline
                {
                    if ( !attendance.IsScheduledPersonDeclined() )
                    {
                        attendanceService.ScheduledPersonDecline( attendance.Id, null );
                        rockContext.SaveChanges();
                    }

                    response.NewStatus = ToolboxScheduleRowConfirmationStatus.Declined;
                    response.WasSchedulePreviouslyConfirmed = bag.ActionType == ToolboxScheduleRowActionType.Cancel;
                    response.IsDeclineReasonRequired = attendance.Occurrence?.Group?.GroupType?.RequiresReasonIfDeclineSchedule == true;

                    // If a decline reason is not required, go ahead and try sending "scheduled person response" emails now.
                    // Otherwise, we'll wait to send them after the decline reason is saved.
                    shouldTrySendingResponseEmails = !response.IsDeclineReasonRequired;

                    var declineReasonNote = GetAttributeValue( AttributeKey.DeclineReasonNote );
                    response.IsDeclineNoteRequired = declineReasonNote.ToLower() == "required";
                    response.IsDeclineNoteVisible = response.IsDeclineNoteRequired || declineReasonNote.ToLower() == "optional";
                }

                if ( shouldTrySendingResponseEmails )
                {
                    TrySendScheduledPersonResponseEmails( toolboxData.SelectedPerson.Id, attendance, attendanceService, bag.ActionType );
                }
            }

            if ( response.NewStatus.HasValue )
            {
                // The feed URL might now be enabled/disabled based on the latest row statuses.
                scheduleRow.ConfirmationStatus = response.NewStatus.Value;
                response.PersonGroupScheduleFeedUrl = GetPersonGroupScheduleFeedUrl( currentScheduleRows, toolboxData.SelectedPrimaryAlias.Guid );
            }

            return response;
        }

        /// <summary>
        /// Tries to send response emails to "scheduled by person" and/or "cancellation person".
        /// </summary>
        /// <param name="scheduledPersonId">The scheduled person identifier.</param>
        /// <param name="attendance">The attendance record for which to send response emails.</param>S
        /// <param name="attendanceService">The initialized attendance service to be used to send response emails.</param>
        /// <param name="actionType">The action type being performed against the attendance record.</param>
        private void TrySendScheduledPersonResponseEmails( int scheduledPersonId, Attendance attendance, AttendanceService attendanceService, ToolboxScheduleRowActionType actionType )
        {
            var schedulingResponseEmailGuid = GetAttributeValue( AttributeKey.SchedulingResponseEmail ).AsGuidOrNull();

            // Send "accept" and "decline" emails to scheduled-by person (defined on the attendance record).
            var scheduledByPerson = attendance.ScheduledByPersonAlias?.Person;
            var shouldSendScheduledByPersonEmail = scheduledByPerson != null
                && schedulingResponseEmailGuid.HasValue
                && actionType != ToolboxScheduleRowActionType.Cancel
                && GetAttributeValue( AttributeKey.SchedulerReceiveConfirmationEmails ).AsBoolean();

            // Send "decline" and "cancel" emails to cancellation person (defined on the group record).
            var groupScheduleCancellationPerson = attendance.Occurrence?.Group?.ScheduleCancellationPersonAlias?.Person;
            var shouldSendCancellationPersonEmail = groupScheduleCancellationPerson != null
                && ( !shouldSendScheduledByPersonEmail || scheduledByPerson.Id != groupScheduleCancellationPerson.Id ) // Prevent duplicate email.
                && schedulingResponseEmailGuid.HasValue
                && actionType != ToolboxScheduleRowActionType.Accept;

            void SendEmail( Person recipient )
            {
                try
                {
                    attendanceService.SendScheduledPersonResponseEmail( attendance, schedulingResponseEmailGuid, recipient );
                }
                catch ( Exception ex )
                {
                    var message = $"{nameof( GroupScheduleToolbox )}: Unable to send scheduled person response email to {recipient.FullName} (Attendance ID = {attendance.Id}, Scheduled Person ID = {scheduledPersonId}, Recipient Person ID = {recipient.Id}).";

                    ExceptionLogService.LogException( new Exception( message, ex ) );
                }
            }

            if ( shouldSendScheduledByPersonEmail )
            {
                SendEmail( scheduledByPerson );
            }

            if ( shouldSendCancellationPersonEmail )
            {
                SendEmail( groupScheduleCancellationPerson );
            }
        }

        /// <summary>
        /// Saves decline reason information for a declined schedule row.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="bag">The information about the schedule row being declined.</param>
        /// <param name="errorMessage">A friendly error message to describe any problems encountered.</param>
        private void SaveDeclineReason( RockContext rockContext, SaveDeclineReasonRequestBag bag, out string errorMessage )
        {
            errorMessage = null;

            var friendlyErrorMessage = "Unable to save decline reason.";

            if ( bag == null )
            {
                // No request data was provided.
                errorMessage = friendlyErrorMessage;
                return;
            }

            var config = new GetToolboxDataConfig
            {
                SelectedPersonGuidOverride = bag.SelectedPersonGuid,
                ActionTypeOverride = ToolboxActionType.ViewCurrent
            };

            var toolboxData = GetCommonToolboxData( rockContext, config );
            if ( toolboxData.ErrorMessage.IsNotNullOrWhiteSpace() )
            {
                errorMessage = toolboxData.ErrorMessage;
                return;
            }

            var attendanceService = new AttendanceService( rockContext );

            // Get all the supporting data we'll need to operate below.
            // Note that some of these entities aren't directly needed here, but will be needed if sending emails.
            var attendance = attendanceService
                .Queryable()
                .Include( a => a.Occurrence.Group.ScheduleCancellationPersonAlias.Person )
                .Include( a => a.Occurrence.Schedule )
                .Include( a => a.PersonAlias.Person )
                .Include( a => a.ScheduledByPersonAlias.Person )
                .FirstOrDefault( a => a.Guid == bag.AttendanceGuid );

            if ( attendance == null )
            {
                ExceptionLogService.LogException( $@"{nameof( GroupScheduleToolbox )}: {friendlyErrorMessage} No Attendance record found with Guid = '{bag.AttendanceGuid}' (Scheduled Person ID = {toolboxData.SelectedPerson.Id})." );

                errorMessage = friendlyErrorMessage;
                return;
            }
            else if ( attendance.PersonAliasId != toolboxData.SelectedPrimaryAlias.Id )
            {
                ExceptionLogService.LogException( $@"{nameof( GroupScheduleToolbox )}: {friendlyErrorMessage} The specified Attendance record doesn't belong to this Person (Scheduled Person ID = {toolboxData.SelectedPerson.Id}, Attendance ID = {attendance.Id})." );

                errorMessage = friendlyErrorMessage;
                return;
            }

            var declineReasonValueGuid = bag.DeclineReason?.Value.AsGuidOrNull();
            if ( declineReasonValueGuid.HasValue )
            {
                var declineReasonValueId = DefinedValueCache.GetId( declineReasonValueGuid.Value );
                if ( declineReasonValueId.HasValue )
                {
                    attendance.DeclineReasonValueId = declineReasonValueId;
                }
            }

            if ( bag.DeclineReasonNote.IsNotNullOrWhiteSpace() )
            {
                attendance.Note = bag.DeclineReasonNote;
            }

            rockContext.SaveChanges();

            var actionType = bag.WasSchedulePreviouslyConfirmed
                ? ToolboxScheduleRowActionType.Cancel
                : ToolboxScheduleRowActionType.Decline;

            TrySendScheduledPersonResponseEmails( toolboxData.SelectedPerson.Id, attendance, attendanceService, actionType );
        }

        /// <summary>
        /// Saves unavailability for the selected people.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="bag">The information about the unavailability to be saved.</param>
        /// <param name="errorMessage">A friendly error message to describe any problems encountered.</param>
        private void SaveUnavailability( RockContext rockContext, SaveUnavailabilityRequestBag bag, out string errorMessage )
        {
            errorMessage = null;

            var friendlyErrorMessage = "Unable to save unavailability.";

            if ( bag == null )
            {
                // No request data was provided.
                errorMessage = friendlyErrorMessage;
                return;
            }

            // Ensure we have a valid start and end date.
            if ( !bag.StartDate.HasValue || !bag.EndDate.HasValue || bag.StartDate.Value > bag.EndDate.Value )
            {
                errorMessage = "Please enter a valid date range.";
                return;
            }

            var today = RockDateTime.Today;
            var startDate = bag.StartDate.Value.Date;
            var endDate = bag.EndDate.Value.Date;
            if ( startDate < today && endDate < today )
            {
                errorMessage = "Please enter a current or future date range.";
                return;
            }

            var config = new GetToolboxDataConfig
            {
                SelectedPersonGuidOverride = bag.SelectedPersonGuid,
                ActionTypeOverride = ToolboxActionType.ViewCurrent,
                SchedulableGroupLoadingMode = SchedulableGroupLoadingMode.LoadAll
            };

            var toolboxData = GetCommonToolboxData( rockContext, config );
            if ( toolboxData.ErrorMessage.IsNotNullOrWhiteSpace() )
            {
                errorMessage = toolboxData.ErrorMessage;
                return;
            }

            if ( bag.PersonGuids.Any( g => !toolboxData.AllFamilyMemberPeople.Any( fmp => fmp.Person.Guid == g ) ) )
            {
                // Attempting to save unavailability for someone not in the selected person's family.
                errorMessage = friendlyErrorMessage;
                return;
            }

            var personScheduleExclusionService = new PersonScheduleExclusionService( rockContext );
            int? parentId = null;

            foreach ( var personGuid in bag.PersonGuids )
            {
                // Because of checks performed before we got here, this ID is guaranteed to be present.
                var personAliasId = toolboxData.AllFamilyMemberPeople
                    .First( fmp => fmp.Person.Guid == personGuid )
                    .PrimaryAlias.Id;

                var groupId = bag.GroupGuid.HasValue
                    ? toolboxData.SchedulableGroups.FirstOrDefault( g => g.Group.Guid == bag.GroupGuid )?.Group.Id
                    : null;

                var personScheduleExclusion = new PersonScheduleExclusion
                {
                    PersonAliasId = personAliasId,
                    StartDate = startDate,
                    EndDate = endDate,
                    GroupId = groupId,
                    Title = bag.Notes,
                    ParentPersonScheduleExclusionId = parentId
                };

                personScheduleExclusionService.Add( personScheduleExclusion );
                rockContext.SaveChanges();

                if ( !parentId.HasValue )
                {
                    parentId = personScheduleExclusion.Id;
                }
            }
        }

        /// <summary>
        /// Gets information about the selected person's schedule preferences.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="bag">The information needed to get the selected person's schedule preferences.</param>
        /// <param name="errorMessage">A friendly error message to describe any problems encountered.</param>
        /// <returns>An object containing information about the selected person's schedule preferences.</returns>
        private SchedulePreferencesBag GetSchedulePreferences( RockContext rockContext, GetSchedulePreferencesRequestBag bag, out string errorMessage )
        {
            errorMessage = null;

            if ( bag == null )
            {
                // No request data was provided.
                errorMessage = "Unable to get schedule preferences.";
                return null;
            }

            var config = new GetToolboxDataConfig
            {
                SelectedPersonGuidOverride = bag.SelectedPersonGuid,
                ActionTypeOverride = ToolboxActionType.UpdatePreferences,
                SchedulableGroupLoadingMode = SchedulableGroupLoadingMode.LoadOnlyGroupsHavingLocationSchedules
            };

            if ( bag.SelectedGroupGuid.HasValue )
            {
                // Only set this override if a group guid was actually provided by the client.
                // Otherwise, even a null value will override any value provided by page parameters.
                config.EntityKeyOverrides = new Dictionary<string, string>
                {
                    { PageParameterKey.ToolboxGroupId, bag.SelectedGroupGuid.ToStringSafe() }
                };
            }

            var toolboxData = GetCommonToolboxData( rockContext, config );
            if ( toolboxData.ErrorMessage.IsNotNullOrWhiteSpace() )
            {
                errorMessage = toolboxData.ErrorMessage;
                return null;
            }

            // If a group guid wasn't provided or a guid was provided that doesn't represent one of the
            // schedulable groups, the first group in the list will have been auto-selected instead.
            var group = toolboxData.SelectedGroup?.Group;
            var groupMember = toolboxData.SelectedGroup?.GroupMember;

            var schedulableGroups = GetGroupBags( toolboxData.SchedulableGroups );
            var selectedGroup = schedulableGroups?.FirstOrDefault( g => g.Guid == group?.Guid );

            var response = new SchedulePreferencesBag();

            if ( group == null || groupMember == null || !schedulableGroups.Any() || selectedGroup == null )
            {
                // Return an empty response to indicate that the selected person doesn't have any schedulable
                // groups that also have schedule(s) and location(s).
                return response;
            }

            response.SchedulableGroups = schedulableGroups;
            response.SelectedGroup = selectedGroup;

            var scheduleTemplates = new GroupMemberScheduleTemplateService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( t => t.GroupTypeId == null || t.GroupTypeId == group.GroupTypeId )
                .ToListItemBagList();

            scheduleTemplates.Insert( 0, new ListItemBag { Text = "No Schedule", Value = "" } );

            var scheduleStartDate = groupMember.ScheduleStartDate;
            if ( !scheduleStartDate.HasValue )
            {
                scheduleStartDate = RockDateTime.Today;
            }

            var groupMemberAssignments = GetGroupMemberAssignments( rockContext, group.Id, groupMember.Id );

            response.SchedulePreference = new SchedulePreferenceBag
            {
                ScheduleReminderEmailOffsetDays = groupMember.ScheduleReminderEmailOffsetDays.ToStringSafe(),
                ScheduleTemplates = scheduleTemplates,
                SelectedScheduleTemplateGuid = groupMember.ScheduleTemplate?.Guid,
                ScheduleStartDate = scheduleStartDate,
                Assignments = GetSchedulePreferenceAssignmentBags( groupMemberAssignments )
            };

            return response;
        }

        /// <summary>
        /// Gets the group member assignments for the specified group and group member.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="groupMemberId">The group member identifier.</param>
        /// <returns>The group member assignments for the specified group and group member.</returns>
        private List<GroupMemberAssignment> GetGroupMemberAssignments( RockContext rockContext, int groupId, int groupMemberId )
        {
            var groupLocationsQry = new GroupLocationService( rockContext )
                .Queryable()
                .Where( gl => gl.GroupId == groupId );

            var groupMemberAssignmentQry = new GroupMemberAssignmentService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( gma =>
                    gma.GroupMemberId == groupMemberId
                    && (
                        !gma.LocationId.HasValue
                        || groupLocationsQry.Any( gl => gl.LocationId == gma.LocationId && gl.Schedules.Any( s => s.Id == gma.ScheduleId ) )
                    )
                );

            // Calculate the next start date/time based on the start of the week so that schedule columns are in the correct order.
            var occurrenceDate = RockDateTime.Now.SundayDate().AddDays( 1 );

            return groupMemberAssignmentQry
                .Include( a => a.Schedule )
                .Include( a => a.Location )
                .ToList() // Instantiate the collection so we can perform in-memory sorting against the next start date/time.
                .OrderBy( a => a.Schedule.Order )
                .ThenBy( a => a.Schedule.GetNextStartDateTime( occurrenceDate ) )
                .ThenBy( a => a.Schedule.Name )
                .ThenBy( a => a.Schedule.Id )
                .ThenBy( a => a.LocationId.HasValue ? a.Location.ToString( true ) : string.Empty )
                .ToList();
        }

        /// <summary>
        /// Gets the schedule preference assignment bags for the provided group member assignments.
        /// </summary>
        /// <param name="groupMemberAssignments">The group member assignments.</param>
        /// <returns>The schedule preference assignment bags for the provided group member assignments.</returns>
        private List<SchedulePreferenceAssignmentBag> GetSchedulePreferenceAssignmentBags( List<GroupMemberAssignment> groupMemberAssignments )
        {
            return groupMemberAssignments
                ?.Select( gma => new SchedulePreferenceAssignmentBag
                {
                    GroupMemberAssignmentGuid = gma.Guid,
                    ScheduleName = GetFormattedSchedule( gma.Schedule ),
                    LocationName = GetFormattedLocation( gma.Location )
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the formatted name to be used when displaying schedules for schedule preferences and additional time sign-up.
        /// </summary>
        /// <param name="schedule"></param>
        /// <returns></returns>
        private string GetFormattedSchedule( Schedule schedule )
        {
            if ( schedule == null )
            {
                return string.Empty;
            }

            switch ( ScheduleListFormat )
            {
                case 1: // Schedule Time
                    return schedule.StartTimeOfDay.ToTimeString();
                case 2: // Schedule Name
                    return schedule.ToString( true );
                case 3: // Schedule Time and Name
                    return $"{schedule.StartTimeOfDay.ToTimeString()} {schedule.ToString( true )}";
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Gets the formatted name to be used when displaying locations for schedule preferences and additional time sign-up.
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        private string GetFormattedLocation( Location location )
        {
            return location != null
                ? location.ToString( true )
                : NO_LOCATION_PREFERENCE;
        }

        /// <summary>
        /// Saves schedule preference selections for the selected person and group.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="bag">The information about the schedule preference to be saved.</param>
        /// <param name="errorMessage">A friendly error message to describe any problems encountered.</param>
        private void SaveSchedulePreference( RockContext rockContext, SaveSchedulePreferenceRequestBag bag, out string errorMessage )
        {
            errorMessage = null;

            var friendlyErrorMessage = "Unable to save schedule preferences.";

            if ( bag == null )
            {
                // No request data was provided.
                errorMessage = friendlyErrorMessage;
                return;
            }

            var config = new GetToolboxDataConfig
            {
                SelectedPersonGuidOverride = bag.SelectedPersonGuid,
                ActionTypeOverride = ToolboxActionType.UpdatePreferences,
                EntityKeyOverrides = new Dictionary<string, string>
                {
                    { PageParameterKey.ToolboxGroupId, bag.SelectedGroupGuid.ToString() }
                },
                SchedulableGroupLoadingMode = SchedulableGroupLoadingMode.LoadOnlyGroupsHavingLocationSchedules
            };

            var toolboxData = GetCommonToolboxData( rockContext, config );
            if ( toolboxData.ErrorMessage.IsNotNullOrWhiteSpace() )
            {
                errorMessage = toolboxData.ErrorMessage;
                return;
            }

            if ( toolboxData.SelectedGroup?.Group?.Guid != bag.SelectedGroupGuid || toolboxData.SelectedGroup?.GroupMember == null )
            {
                // The specified group is not in the list of schedulable groups.
                errorMessage = friendlyErrorMessage;
                return;
            }

            // Get a tracked group member instance against which we can save changes.
            var groupMember = new GroupMemberService( rockContext ).Get( toolboxData.SelectedGroup.GroupMember.Id );

            int? scheduleTemplateId = null;
            if ( bag.SelectedScheduleTemplateGuid.HasValue )
            {
                scheduleTemplateId = new GroupMemberScheduleTemplateService( rockContext ).GetId( bag.SelectedScheduleTemplateGuid.Value );
            }

            DateTime? scheduleStartDate = null;
            if ( bag.ScheduleStartDate.HasValue )
            {
                scheduleStartDate = bag.ScheduleStartDate.Value.Date;
            }

            groupMember.ScheduleReminderEmailOffsetDays = bag.ScheduleReminderEmailOffsetDays.AsIntegerOrNull();
            groupMember.ScheduleTemplateId = scheduleTemplateId;
            groupMember.ScheduleStartDate = scheduleStartDate;

            rockContext.SaveChanges();
        }

        /// <summary>
        /// Gets schedule preference assignment options for the selected person, group and optional assignment.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="bag">The information needed to get schedule preference assignment options.</param>
        /// <param name="errorMessage">A friendly error message to describe any problems encountered.</param>
        /// <returns>
        /// An object containing information about the current schedule preference assignment options, as well
        /// as the toolbox data and actual (non-transformed) entities that were used to build the options object.
        /// This data will be helpful when attempting to edit existing / save new assignments, so we don't have
        /// to run additional, duplicate queries to retrieve the needed data at that time. Lastly, a friendly
        /// error message will be returned to describe any problems encountered.
        /// </returns>
        private (
            AssignmentOptionsBag assignmentOptions,
            ToolboxData toolboxData,
            Dictionary<Schedule, List<Location>> allowedLocationsBySchedule,
            string errorMessage
        ) GetAssignmentOptions( RockContext rockContext, GetAssignmentOptionsRequestBag bag )
        {
            var response = (
                assignmentOptions: ( AssignmentOptionsBag ) null,
                toolboxData: ( ToolboxData ) null,
                allowedLocationsBySchedule: ( Dictionary<Schedule, List<Location>> ) null,
                errorMessage: ( string ) null
            );

            var friendlyErrorMessage = "Unable to get assignment options.";

            if ( bag == null )
            {
                // No request data was provided.
                response.errorMessage = friendlyErrorMessage;
                return response;
            }

            var config = new GetToolboxDataConfig
            {
                SelectedPersonGuidOverride = bag.SelectedPersonGuid,
                ActionTypeOverride = ToolboxActionType.UpdatePreferences,
                EntityKeyOverrides = new Dictionary<string, string>
                {
                    { PageParameterKey.ToolboxGroupId, bag.SelectedGroupGuid.ToString() }
                },
                SchedulableGroupLoadingMode = SchedulableGroupLoadingMode.LoadOnlyGroupsHavingLocationSchedules
            };

            var toolboxData = GetCommonToolboxData( rockContext, config );
            response.toolboxData = toolboxData;

            if ( toolboxData.ErrorMessage.IsNotNullOrWhiteSpace() )
            {
                response.errorMessage = toolboxData.ErrorMessage;
                return response;
            }

            if ( toolboxData.SelectedGroup?.Group?.Guid != bag.SelectedGroupGuid || toolboxData.SelectedGroup?.GroupMember == null )
            {
                // The specified group is not in the list of schedulable groups.
                response.errorMessage = friendlyErrorMessage;
                return response;
            }

            // Get all public schedules for the selected group's locations.
            var groupId = toolboxData.SelectedGroup.Group.Id;
            var schedules = new GroupLocationService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( gl => gl.GroupId == groupId )
                .SelectMany( gl => gl.Schedules )
                .Where( s => s.IsPublic == true )
                .Distinct()
                .ToList();

            // Get all existing assignments for the selected person's group member.
            var assignments = GetGroupMemberAssignments( rockContext, groupId, toolboxData.SelectedGroup.GroupMember.Id );

            // If attempting to edit an existing assignment, try to find its currently-specified schedule
            // and location in the available lists so we can pre-select these values on the edit form.
            GroupMemberAssignment assignmentToEdit = null;
            Schedule selectedSchedule = null;
            int? selectedLocationId = null;
            if ( bag.EditAssignmentGuid.HasValue )
            {
                assignmentToEdit = assignments.FirstOrDefault( a => a.Guid == bag.EditAssignmentGuid.Value );
                if ( assignmentToEdit?.ScheduleId.HasValue == true )
                {
                    selectedSchedule = schedules.FirstOrDefault( s => s.Id == assignmentToEdit.ScheduleId.Value );
                }

                selectedLocationId = assignmentToEdit?.LocationId;
            }

            // Get schedule IDs for which the selected person's group member already has a preference set.
            var configuredScheduleIds = assignments
                .Where( a => a.ScheduleId.HasValue )
                .Select( a => a.ScheduleId.Value )
                .Distinct()
                .ToList();

            // Limit the response to schedules that haven't had a preference set yet, as well
            // as the schedule from the existing assignment being edited, if applicable.
            schedules = schedules
                .Where( s =>
                    !configuredScheduleIds.Contains( s.Id )
                    || (
                        selectedSchedule != null
                        && s.Id == selectedSchedule.Id
                    )
                )
                .ToList()
                .OrderByOrderAndNextScheduledDateTime();

            // Begin building the response object. Note that we'll only set the edit assignment's unique
            // identifier on the response if we actually found an existing assignment, regardless of what
            // was specified in the request.
            response.assignmentOptions = new AssignmentOptionsBag
            {
                Schedules = new List<ListItemBag>(),
                LocationsBySchedule = new Dictionary<string, List<ListItemBag>>(),
                EditAssignmentGuid = assignmentToEdit?.Guid,
                SelectedScheduleGuid = selectedSchedule?.Guid,
                Assignments = GetSchedulePreferenceAssignmentBags( assignments )
            };

            // This is the collection of actual (non-transformed) entities we'll pass back to the
            // caller of this method, so they have access to any IDs they need.
            response.allowedLocationsBySchedule = new Dictionary<Schedule, List<Location>>();

            var locationService = new LocationService( rockContext );

            // Build the list of available schedules, formatting their names according to
            // block settings. Also, for each schedule, get its list of locations.
            foreach ( var schedule in schedules )
            {
                response.assignmentOptions.Schedules.Add( new ListItemBag
                {
                    Text = GetFormattedSchedule( schedule ),
                    Value = schedule.Guid.ToString()
                } );

                // Get this schedule's locations.
                var locations = locationService
                    .GetByGroupSchedule( schedule.Id, groupId )
                    .OrderBy( l => l.Name )
                    .ToList();

                response.allowedLocationsBySchedule.AddOrReplace( schedule, locations );

                var locationListItemBags = locations
                    .Select( l => new ListItemBag
                    {
                        Text = l.ToString( true ),
                        Value = l.Guid.ToString()
                    } )
                    .ToList();

                locationListItemBags.Insert( 0, new ListItemBag { Text = NO_LOCATION_PREFERENCE, Value = "" } );

                response.assignmentOptions.LocationsBySchedule.AddOrReplace( schedule.Guid.ToString(), locationListItemBags );

                // If we're editing an existing assignment and this is the assignment's previously-selected schedule,
                // try to set the previously-selected location on the response.
                if ( selectedSchedule?.Guid == schedule.Guid && selectedLocationId.HasValue )
                {
                    var selectedLocation = locations.FirstOrDefault( l => l.Id == selectedLocationId.Value );
                    if ( selectedLocation != null )
                    {
                        response.assignmentOptions.SelectedLocationGuid = selectedLocation.Guid;
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// Saves a schedule preference assignment for the selected person and group.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="bag">The information about the schedule preference assignment to be saved.</param>
        /// <returns>An object containing information about the outcome of the request.</returns>
        private SaveAssignmentResponseBag SaveAssignment( RockContext rockContext, SaveAssignmentRequestBag bag )
        {
            // Get the currently-available assignment options, against which we can validate the provided selections.
            var optionsRequestBag = new GetAssignmentOptionsRequestBag
            {
                SelectedPersonGuid = bag.SelectedPersonGuid,
                SelectedGroupGuid = bag.SelectedGroupGuid,
                EditAssignmentGuid = bag.EditAssignmentGuid
            };

            var (
                assignmentOptions,
                toolboxData,
                allowedLocationsBySchedule,
                errorMessage
            ) = GetAssignmentOptions( rockContext, optionsRequestBag );

            var response = new SaveAssignmentResponseBag { AssignmentOptions = assignmentOptions };

            // Local function to consistently return friendly errors + updated options & assignments.
            SaveAssignmentResponseBag ClearSelectionsAndReturnError( string saveError )
            {
                response.SaveError = saveError;
                response.AssignmentOptions.SelectedScheduleGuid = null;
                response.AssignmentOptions.SelectedLocationGuid = null;

                return response;
            }

            if ( errorMessage.IsNotNullOrWhiteSpace() )
            {
                return ClearSelectionsAndReturnError( errorMessage );
            }

            // Since we got this far, we know:
            //  1. The current person is authorized to manage assignments for the selected individual.
            //  2. The group and group member IDs are guaranteed to be present on the toolboxData object
            //     (meaning.. the selected individual is a group member of the specified schedulable group).
            //  3. If the assignmentOptions.EditAssignmentGuid property has a value, this request represents
            //     the editing of an existing assignment. We'll do one final check below to ensure the
            //     specified assignment still exists, and simply create a new one if not.

            var groupId = toolboxData.SelectedGroup.Group.Id;
            var groupMemberId = toolboxData.SelectedGroup.GroupMember.Id;

            // Ensure the selected schedule is allowed.
            var selectedSchedule = allowedLocationsBySchedule?.Keys.FirstOrDefault( s => s.Guid == bag.SelectedScheduleGuid );
            if ( selectedSchedule == null )
            {
                return ClearSelectionsAndReturnError( "The selected schedule is no longer available. Please make another selection." );
            }

            int? selectedLocationId = null;
            if ( bag.SelectedLocationGuid.HasValue )
            {
                // Ensure the selected location is allowed.
                var selectedLocation = allowedLocationsBySchedule[selectedSchedule].FirstOrDefault( l => l.Guid == bag.SelectedLocationGuid.Value );
                if ( selectedLocation == null )
                {
                    return ClearSelectionsAndReturnError( "The selected schedule and location combination is no longer available. Please make another selection." );
                }

                selectedLocationId = selectedLocation.Id;
            }

            GroupMemberAssignment assignment = null;
            var groupMemberAssignmentService = new GroupMemberAssignmentService( rockContext );

            // Final check to see if any specified assignment (to edit) still exists.
            // If not, we'll just create a new one.
            if ( assignmentOptions.EditAssignmentGuid.HasValue )
            {
                assignment = groupMemberAssignmentService.Get( assignmentOptions.EditAssignmentGuid.Value );
            }

            if ( assignment == null )
            {
                assignment = new GroupMemberAssignment();
                groupMemberAssignmentService.Add( assignment );
            }

            assignment.GroupMemberId = groupMemberId;
            assignment.ScheduleId = selectedSchedule.Id;
            assignment.LocationId = selectedLocationId;

            rockContext.SaveChanges();

            // Finally, return an updated & properly sorted list of assignments.
            var groupMemberAssignments = GetGroupMemberAssignments( rockContext, groupId, groupMemberId );

            response.AssignmentOptions.Assignments = GetSchedulePreferenceAssignmentBags( groupMemberAssignments );

            return response;
        }

        /// <summary>
        /// Deletes the specified schedule preference assignment for the selected person.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="bag">The information about the schedule preference assignment to be deleted.</param>
        /// <param name="errorMessage">A friendly error message to describe any problems encountered.</param>
        private void DeleteAssignment( RockContext rockContext, DeleteAssignmentRequestBag bag, out string errorMessage )
        {
            errorMessage = null;

            if ( bag == null )
            {
                // No request data was provided.
                errorMessage = "Unable to delete assignment";
                return;
            }

            // Perform a basic authorization check for this request (no need to load groups or existing assignments, Etc.).
            var config = new GetToolboxDataConfig
            {
                SelectedPersonGuidOverride = bag.SelectedPersonGuid,
                ActionTypeOverride = ToolboxActionType.UpdatePreferences
            };

            var toolboxData = GetCommonToolboxData( rockContext, config );
            if ( toolboxData.ErrorMessage.IsNotNullOrWhiteSpace() )
            {
                errorMessage = toolboxData.ErrorMessage;
                return;
            }

            var groupMemberAssignmentService = new GroupMemberAssignmentService( rockContext );
            var groupMemberAssignment = groupMemberAssignmentService.Get( bag.DeleteAssignmentGuid );

            if ( groupMemberAssignment != null )
            {
                if ( !groupMemberAssignmentService.CanDelete( groupMemberAssignment, out errorMessage ) )
                {
                    // The error message will describe why this assignment cannot be deleted.
                    return;
                }

                groupMemberAssignmentService.Delete( groupMemberAssignment );

                rockContext.SaveChanges();
            }

            return;
        }

        /// <summary>
        /// Gets information about the selected person's additional time sign-ups.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="bag">The information needed to get the selected person's additional time sign-ups.</param>
        /// <param name="errorMessage">A friendly error message to describe any problems encountered.</param>
        /// <returns>
        /// An object containing information about the selected person's additional time sign-ups, as well
        /// as the toolbox data and Guid-to-ID dictionaries for the schedules and locations that were used to
        /// build the sign-up occurrences. This data will be helpful when attempting to save / delete attendances,
        /// so we don't have to run additional, duplicate queries to retrieve the needed data at that time. Lastly,
        /// a friendly error message will be returned to describe any problems encountered.
        /// </returns>
        private (
            SignUpsBag signUps,
            ToolboxData toolboxData,
            Dictionary<Guid, int> scheduleIdsByGuid,
            Dictionary<Guid, int?> locationIdsByGuid,
            string errorMessage
        ) GetSignUps( RockContext rockContext, GetSignUpsRequestBag bag )
        {
            var response = (
                signUps: ( SignUpsBag ) null,
                toolboxData: ( ToolboxData ) null,
                scheduleIdsByGuid: ( Dictionary<Guid, int> ) null,
                locationIdsByGuid: ( Dictionary<Guid, int?> ) null,
                errorMessage: ( string ) null
            );

            if ( bag == null )
            {
                // No request data was provided.
                response.errorMessage = "Unable to get additional time sign-ups.";
                return response;
            }

            var config = new GetToolboxDataConfig
            {
                SelectedPersonGuidOverride = bag.SelectedPersonGuid,
                ActionTypeOverride = ToolboxActionType.SignUp,
                SchedulableGroupLoadingMode = SchedulableGroupLoadingMode.LoadOnlyGroupsHavingLocationSchedules
            };

            if ( bag.SelectedGroupGuid.HasValue )
            {
                // Only set this override if a group guid was actually provided by the client.
                // Otherwise, even a null value will override any value provided by page parameters.
                config.EntityKeyOverrides = new Dictionary<string, string>
                {
                    { PageParameterKey.ToolboxGroupId, bag.SelectedGroupGuid.ToStringSafe() }
                };
            }

            var toolboxData = GetCommonToolboxData( rockContext, config );
            response.toolboxData = toolboxData;

            if ( toolboxData.ErrorMessage.IsNotNullOrWhiteSpace() )
            {
                response.errorMessage = toolboxData.ErrorMessage;
                return response;
            }

            // If a group guid wasn't provided or a guid was provided that doesn't represent one of the
            // schedulable groups, the first group in the list will have been auto-selected instead.
            var group = toolboxData.SelectedGroup?.Group;

            var schedulableGroups = GetGroupBags( toolboxData.SchedulableGroups );
            var selectedGroup = schedulableGroups?.FirstOrDefault( g => g.Guid == group?.Guid );

            response.signUps = new SignUpsBag();

            if ( group == null || !schedulableGroups.Any() || selectedGroup == null )
            {
                // Return an empty response to indicate that the selected person doesn't have any schedulable
                // groups that also have schedule(s) and location(s).
                return response;
            }

            response.signUps.SchedulableGroups = schedulableGroups;
            response.signUps.SelectedGroup = selectedGroup;

            var (occurrences, scheduleIdsByGuid, locationIdsByGuid) = GetSignUpOccurrences( rockContext, group, toolboxData );
            response.signUps.Occurrences = occurrences;
            response.scheduleIdsByGuid = scheduleIdsByGuid;
            response.locationIdsByGuid = locationIdsByGuid;

            return response;
        }

        /// <summary>
        /// Gets the sign-up occurrences for the specified group and person.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="selectedGroup">The selected group.</param>
        /// <param name="toolboxData">The toolbox data.</param>
        /// <returns>
        /// The sign-up occurrences for the specified group and person, as well as Guid-to-ID dictionaries
        /// for the schedules and locations that were used to build the sign-up occurrences. This data will
        /// be helpful when attempting to save / delete attendances, so we don't have to run additional,
        /// duplicate queries to retrieve the needed data at that time.
        /// </returns>
        private (
            List<SignUpOccurrenceBag> occurrences,
            Dictionary<Guid, int> scheduleIdsByGuid,
            Dictionary<Guid, int?> locationIdsByGuid
        ) GetSignUpOccurrences( RockContext rockContext, Rock.Model.Group selectedGroup, ToolboxData toolboxData )
        {
            var selectedPersonId = toolboxData.SelectedPerson.Id;

            var response = (
                occurrences: ( List<SignUpOccurrenceBag> ) null,
                scheduleIdsByGuid: ( Dictionary<Guid, int> ) null,
                locationIdsByGuid: ( Dictionary<Guid, int?> ) null
            );

            if ( selectedGroup.SchedulingMustMeetRequirements )
            {
                // Ensure the selected person meets group requirements.
                if ( new GroupService( rockContext )
                        .GroupMembersNotMeetingRequirements( selectedGroup, false, false )
                        .Any( gm => gm.Key.PersonId == selectedPersonId ) )
                {
                    return response;
                }
            }

            // Determine the date/time ranges to use when sourcing additional time sign-ups. The minimum begin date/time
            // (dictated by the cutoff hours setting) should be enforced for immediate AND non-immediate need sign-ups.
            var cutOffHours = GetAttributeValue( AttributeKey.AdditionalTimeSignUpCutoffTime ).AsInteger();
            if ( cutOffHours < 0 )
            {
                cutOffHours = 0;
            }

            var now = RockDateTime.Now;
            var minimumBeginDateTime = now.AddHours( cutOffHours );

            var nonImmediateStartDateTimesBySchedule = new Dictionary<Schedule, List<DateTime>>();
            var nonImmediateDateRange = GetAdditionalTimeSignUpDateRange();
            var nonImmediateBeginDateTime = nonImmediateDateRange.Start.Value;
            var nonImmediateEndDateTime = nonImmediateDateRange.End.Value;

            // Ensure the non-immediate begin date/time is not sooner than the minimum
            // begin date/time as dictated by the cutoff hours above.
            if ( nonImmediateBeginDateTime < minimumBeginDateTime )
            {
                nonImmediateBeginDateTime = minimumBeginDateTime;
            }

            // Immediate need sign-ups will only be shown if:
            //  1) The "Enable Immediate Needs" block setting is true;
            //  2) We have occurrences that fall within the specified window;
            //  3) Said occurrences aren't excluded by the cutoff time.
            Dictionary<Schedule, List<DateTime>> immediateNeedsStartDateTimesBySchedule = null;
            DateTime? immediateNeedsEndDateTime = null;

            if ( toolboxData.IsImmediateNeedsEnabled )
            {
                var immediateNeedsWindowHours = GetAttributeValue( AttributeKey.ImmediateNeedWindow ).AsInteger();
                if ( immediateNeedsWindowHours > 0 )
                {
                    immediateNeedsStartDateTimesBySchedule = new Dictionary<Schedule, List<DateTime>>();
                    immediateNeedsEndDateTime = now.AddHours( immediateNeedsWindowHours );

                    // Ensure the immediate needs end date/time is not later than the maximum end date/time
                    // as dictated by the sign-up date range attribute. This will probably never happen, but
                    // let's protect against it just in case.
                    if ( immediateNeedsEndDateTime > nonImmediateEndDateTime )
                    {
                        immediateNeedsEndDateTime = nonImmediateEndDateTime;
                    }
                }
            }

            // Add all sign-up occurrences (immediate AND non-immediate needs) to a single collection.
            // The UI will be responsible for separating and presenting them as needed.
            response.occurrences = new List<SignUpOccurrenceBag>();
            response.scheduleIdsByGuid = new Dictionary<Guid, int>();
            response.locationIdsByGuid = new Dictionary<Guid, int?>();

            var attendanceService = new AttendanceService( rockContext );
            var personScheduleExclusionService = new PersonScheduleExclusionService( rockContext );

            // Get the active, public schedules + configs for this group's locations.
            var groupLocationScheduleQry = new GroupLocationService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( gl => gl.GroupId == selectedGroup.Id )
                .SelectMany( gl => gl.Schedules, ( gl, s ) => new
                {
                    GroupLocationOrder = gl.Order,
                    gl.Location,
                    Schedule = s,
                    Config = gl.GroupLocationScheduleConfigs.FirstOrDefault( glsc => glsc.ScheduleId == s.Id )
                } )
                .Where( gls => ( gls.Schedule.IsPublic ?? true ) && gls.Schedule.IsActive );

            var excludeScheduleGuids = GetAttributeValue( AttributeKey.AdditionalTimeSignUpScheduleExclusions ).SplitDelimitedValues().AsGuidList();
            if ( excludeScheduleGuids.Any() )
            {
                groupLocationScheduleQry = groupLocationScheduleQry
                    .Where( gls => !excludeScheduleGuids.Contains( gls.Schedule.Guid ) );
            }

            foreach ( var gls in groupLocationScheduleQry.ToList() )
            {
                var maximumCapacitySetting = gls.Config?.MaximumCapacity.GetValueOrDefault() ?? 0;
                var desiredCapacitySetting = gls.Config?.DesiredCapacity.GetValueOrDefault() ?? 0;
                var minimumCapacitySetting = gls.Config?.MinimumCapacity.GetValueOrDefault() ?? 0;
                var desiredOrMinimumNeededCount = Math.Max( desiredCapacitySetting, minimumCapacitySetting );

                // A given additional time sign-up should be considered either an immediate OR non-immediate need; not both.
                // Immediate needs take precedence.
                var signUps = new List<AdditionalTimeSignUp>();

                if ( immediateNeedsEndDateTime.HasValue )
                {
                    // Only calculate immediate start date/times once per schedule and store them in memory for this request.
                    if ( !immediateNeedsStartDateTimesBySchedule.TryGetValue( gls.Schedule, out List<DateTime> immediateNeedsStartDateTimes ) )
                    {
                        immediateNeedsStartDateTimes = gls.Schedule.GetScheduledStartTimes( minimumBeginDateTime, immediateNeedsEndDateTime.Value, true );
                        immediateNeedsStartDateTimesBySchedule.AddOrReplace( gls.Schedule, immediateNeedsStartDateTimes );
                    }

                    foreach ( var startDateTime in immediateNeedsStartDateTimes )
                    {
                        signUps.Add( new AdditionalTimeSignUp
                        {
                            StartDateTime = startDateTime,
                            IsImmediateNeed = true
                        } );
                    }
                }

                // Only calculate non-immediate start date/times once per schedule and store them in memory for this request.
                if ( !nonImmediateStartDateTimesBySchedule.TryGetValue( gls.Schedule, out List<DateTime> nonImmediateStartDateTimes ) )
                {
                    nonImmediateStartDateTimes = gls.Schedule.GetScheduledStartTimes( nonImmediateBeginDateTime, nonImmediateEndDateTime, true );
                    nonImmediateStartDateTimesBySchedule.AddOrReplace( gls.Schedule, nonImmediateStartDateTimes );
                }

                foreach ( var startDateTime in nonImmediateStartDateTimes )
                {
                    // Only add this sign-up if not already represented by an immediate need.
                    if ( signUps.Any( t => t.StartDateTime.Equals( startDateTime ) ) )
                    {
                        continue;
                    }

                    signUps.Add( new AdditionalTimeSignUp
                    {
                        StartDateTime = startDateTime,
                        IsImmediateNeed = false
                    } );
                }

                foreach ( var signUp in signUps )
                {
                    var occurrenceDate = signUp.StartDateTime.Date;
                    if ( attendanceService.IsScheduled( occurrenceDate, gls.Schedule.Id, selectedPersonId ) )
                    {
                        continue;
                    }

                    if ( personScheduleExclusionService.IsExclusionDate( selectedPersonId, selectedGroup.Id, occurrenceDate ) )
                    {
                        continue;
                    }

                    // Get the count of RSVP "Yes" attendances for the group/schedule/date combo (regardless of location).
                    var peopleScheduledQry = attendanceService
                        .Queryable()
                        .Where( a =>
                            a.Occurrence.GroupId == selectedGroup.Id
                            && a.Occurrence.ScheduleId == gls.Schedule.Id
                            && a.Occurrence.OccurrenceDate == signUp.StartDateTime.Date
                            && a.RSVP == RSVP.Yes
                        );

                    // Since we're looping over the combination of [group]location + schedule + startDateTime,
                    // we might have already created this schedule + startDateTime occurrence; try to find it.
                    var scheduleOccurrence = response.occurrences
                        .FirstOrDefault( o =>
                            o.ScheduleGuid == gls.Schedule.Guid
                            && o.OccurrenceDateTime == signUp.StartDateTime
                            && o.IsImmediateNeed == signUp.IsImmediateNeed
                        );

                    if ( scheduleOccurrence == null )
                    {
                        // Execute the non-location-based RSVP "Yes" attendance count query.
                        var peopleScheduledWithoutLocationCount = peopleScheduledQry
                            .Where( a => !a.Occurrence.LocationId.HasValue )
                            .Count();

                        scheduleOccurrence = new SignUpOccurrenceBag
                        {
                            ScheduleGuid = gls.Schedule.Guid,
                            ScheduleName = gls.Schedule.ToString( true ),
                            FormattedScheduleName = GetFormattedSchedule( gls.Schedule ),
                            OccurrenceDateTime = signUp.StartDateTime,
                            PeopleScheduledWithoutLocationCount = peopleScheduledWithoutLocationCount,
                            IsImmediateNeed = signUp.IsImmediateNeed,
                            Locations = new List<SignUpOccurrenceLocationBag>()
                        };

                        response.occurrences.Add( scheduleOccurrence );

                        // Add a schedule Guid-to-ID dictionary entry, so the caller of this method will have access to any IDs they need.
                        response.scheduleIdsByGuid.AddOrIgnore( gls.Schedule.Guid, gls.Schedule.Id );
                    }

                    // Narrow the RSVP "Yes" count down to the specific location.
                    var peopleScheduledAtLocationCount = peopleScheduledQry
                        .Where( a => a.Occurrence.LocationId == gls.Location.Id )
                        .Count();

                    // If this location is already at maximum capacity, no need to add it.
                    if ( maximumCapacitySetting > 0 && peopleScheduledAtLocationCount >= maximumCapacitySetting )
                    {
                        continue;
                    }

                    var peopleNeededAtLocationCount = desiredOrMinimumNeededCount > 0
                        ? desiredOrMinimumNeededCount - peopleScheduledAtLocationCount
                        : 0;

                    if ( peopleNeededAtLocationCount < 0 )
                    {
                        peopleNeededAtLocationCount = 0;
                    }

                    // Add the location to this schedule + startDateTime instance.
                    var locationOccurrence = new SignUpOccurrenceLocationBag
                    {
                        LocationGuid = gls.Location.Guid,
                        LocationName = GetFormattedLocation( gls.Location ),
                        LocationOrder = gls.GroupLocationOrder,
                        MaximumCapacity = maximumCapacitySetting,
                        PeopleScheduledCount = peopleScheduledAtLocationCount,
                        PeopleNeededCount = peopleNeededAtLocationCount
                    };

                    scheduleOccurrence.Locations.Add( locationOccurrence );

                    // Add a location Guid-to-ID dictionary entry, so the caller of this method will have access to any IDs they need.
                    response.locationIdsByGuid.AddOrIgnore( gls.Location.Guid, gls.Location.Id );
                }
            }

            // Filter out schedules already at max capacity and sort those that remain.
            response.occurrences = response.occurrences
                .Where( o => !o.AreAllLocationsAtMaximumCapacity )
                .OrderBy( o => o.OccurrenceDateTime )
                .ThenBy( o => o.LocationSortOrder )
                .ThenBy( o => o.LocationSortName )
                .ToList();

            // Sort each schedule's locations.
            foreach ( var occurrence in response.occurrences )
            {
                occurrence.Locations = occurrence.Locations
                    .OrderBy( l => l.LocationOrder )
                    .ThenBy( l => l.LocationName )
                    .ToList();

                // Check if we need to insert the "No Location Preference" option.
                if ( !toolboxData.IsLocationRequiredForAdditionalTimeSignUp )
                {
                    // Change of logic from the Web Forms block:
                    // If there is only one location then don’t show the drop-down and auto-select that location.
                    // This means we'll only insert the "No Location Preference" option if there is more than one
                    // location for the individual to choose from.
                    if ( occurrence.Locations.Count > 1 )
                    {
                        occurrence.Locations.Insert( 0, new SignUpOccurrenceLocationBag
                        {
                            LocationGuid = Guid.Empty,
                            LocationName = NO_LOCATION_PREFERENCE
                        } );

                        response.locationIdsByGuid.AddOrIgnore( Guid.Empty, null );
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// Gets the additional time sign-up date range.
        /// </summary>
        /// <returns>The additional time sign-up date range.</returns>
        private DateRange GetAdditionalTimeSignUpDateRange()
        {
            var currentDay = RockDateTime.Today;
            var dateRangeDelimitedValues = GetAttributeValue( AttributeKey.AdditionalTimeSignUpDateRange );
            var dateRange = RockDateTimeHelper.CalculateDateRangeFromDelimitedValues( dateRangeDelimitedValues );

            // Cannot start before today.
            if ( !dateRange.Start.HasValue || dateRange.Start.Value < currentDay )
            {
                dateRange.Start = currentDay;
            }

            // Cannot end before today.
            if ( !dateRange.End.HasValue || dateRange.End.Value < currentDay )
            {
                dateRange.End = currentDay;
            }

            dateRange.Start = dateRange.Start.Value.StartOfDay();
            dateRange.End = dateRange.End.Value.EndOfDay();

            return dateRange;
        }

        /// <summary>
        /// Saves or deletes an additional time sign-up for the select person, group, schedule and location.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="bag">The information about the additional time sign-up to be saved or deleted.</param>
        /// <returns>An object containing information about the outcome of the request.</returns>
        private SaveSignUpResponseBag SaveSignUp( RockContext rockContext, SaveSignUpRequestBag bag )
        {
            var friendlyErrorMessage = "Unable to save sign-up.";

            var response = new SaveSignUpResponseBag();

            if ( bag == null )
            {
                // No request data was provided.
                response.SaveError = friendlyErrorMessage;
                return response;
            }

            // If there is an existing attendance for the occurrence date + person + group + schedule combo,
            // it's likely one they just signed up for, and have either unselected it or changed the location.
            // Delete it. It's important to remove this record BEFORE trying to get the currently-available
            // sign-ups, so we can validate any attempt to save new / modify existing sign-ups against the
            // latest data.
            var attendanceService = new AttendanceService( rockContext );
            var localOccurrenceDateTime = bag.OccurrenceDateTime.LocalDateTime;

            rockContext.SqlLogging( true );

            var attendance = attendanceService
                .Queryable()
                .FirstOrDefault( a =>
                    a.Occurrence.OccurrenceDate == localOccurrenceDateTime.Date
                    && a.PersonAlias.Person.Guid == bag.SelectedPersonGuid
                    && a.Occurrence.Group.Guid == bag.SelectedGroupGuid
                    && a.Occurrence.Schedule.Guid == bag.SelectedScheduleGuid
                );

            rockContext.SqlLogging( false );

            if ( attendance != null )
            {
                attendanceService.Delete( attendance );
                rockContext.SaveChanges();
            }

            if ( !bag.IsSigningUp )
            {
                // If they were simply deleting an existing attendance, we're done.
                // Send back an empty object indicating success.
                return response;
            }

            // Get the currently-available sign-ups, against which we can validate the remainder of the request.
            var signUpsRequestBag = new GetSignUpsRequestBag
            {
                SelectedPersonGuid = bag.SelectedPersonGuid,
                SelectedGroupGuid = bag.SelectedGroupGuid
            };

            var (
                signUps,
                toolboxData,
                scheduleGuidsById,
                locationGuidsById,
                errorMessage
            ) = GetSignUps( rockContext, signUpsRequestBag );

            response.SignUps = signUps; // Just in case we need to send back an updated list of sign-ups.

            if ( errorMessage.IsNotNullOrWhiteSpace() )
            {
                response.SaveError = errorMessage;
                return response;
            }

            // Ensure the selected person is a group member of the specified schedulable group.
            if ( toolboxData.SelectedGroup?.Group?.Guid != bag.SelectedGroupGuid )
            {
                response.SaveError = friendlyErrorMessage;
                return response;
            }

            var groupId = toolboxData.SelectedGroup.Group.Id;

            // Ensure the request is referencing a currently-available occurrence.
            var matchingOccurrence = signUps.Occurrences
                ?.FirstOrDefault( o =>
                    o.ScheduleGuid == bag.SelectedScheduleGuid
                    && o.OccurrenceDateTime.LocalDateTime == localOccurrenceDateTime
                    && o.Locations.Any( l => l.LocationGuid == bag.SelectedLocationGuid )
                );

            if ( matchingOccurrence == null && bag.SelectedLocationGuid == Guid.Empty )
            {
                // It's possible a previously-available location has been maxed-out, leading
                // to the "No Location Preference" option no longer being available. If the
                // specified occurrence date + schedule combo has ANY locations available,
                // just pick the first one in the list and assign this individual there. This
                // way, we'll avoid showing them a needless error in the UI.
                matchingOccurrence = signUps.Occurrences
                    ?.FirstOrDefault( o =>
                        o.ScheduleGuid == bag.SelectedScheduleGuid
                        && o.OccurrenceDateTime.LocalDateTime == localOccurrenceDateTime
                        && o.Locations.Any()
                    );

                if ( matchingOccurrence != null )
                {
                    bag.SelectedLocationGuid = matchingOccurrence.Locations.First().LocationGuid;
                }
            }

            int scheduleId = 0;
            var wasScheduleFound = true;
            if ( scheduleGuidsById == null || !scheduleGuidsById.TryGetValue( bag.SelectedScheduleGuid, out scheduleId ) )
            {
                wasScheduleFound = false;
            }

            int? locationId = null;
            var wasLocationFound = true;
            if ( locationGuidsById == null || !locationGuidsById.TryGetValue( bag.SelectedLocationGuid, out locationId ) )
            {
                wasLocationFound = false;
            }

            if ( matchingOccurrence == null || !wasScheduleFound || !wasLocationFound )
            {
                response.SaveError = friendlyErrorMessage;
                return response;
            }

            // Since we got this far, we have the group, schedule & location IDs we need to move forward.
            // Add a new attendance record. Start by getting the attendance occurrence.
            var attendanceOccurrence = new AttendanceOccurrenceService( rockContext )
                .GetOrAdd( localOccurrenceDateTime.Date, groupId, locationId, scheduleId );

            if ( attendanceOccurrence == null )
            {
                response.SaveError = friendlyErrorMessage;
                return response;
            }

            // Save the attendance record to get the ID.
            attendance = attendanceService.ScheduledPersonAddPending( toolboxData.SelectedPerson.Id, attendanceOccurrence.Id, this.RequestContext.CurrentPerson.PrimaryAlias );
            rockContext.SaveChanges();

            if ( attendance == null )
            {
                response.SaveError = friendlyErrorMessage;
                return response;
            }

            // Finally, we can confirm their attendance.
            attendanceService.ScheduledPersonConfirm( attendance.Id );
            rockContext.SaveChanges();

            // Since we got this far, the save was successful.
            response.SignUps = null; // No need to send back the complete list of sign-ups in this case.
            response.SignUpOccurrence = matchingOccurrence; // Send back an updated instance in case the list of locations changed.
            response.SelectedLocationGuid = bag.SelectedLocationGuid; // Send back the identifier of the location that was saved.
            return response;
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Gets information about the selected person's current schedule.
        /// </summary>
        /// <param name="selectedPersonGuid">The selected person's unique identifier.</param>
        /// <returns>An object containing information about the selected person's current schedule.</returns>
        [BlockAction]
        public BlockActionResult GetCurrentSchedule( Guid selectedPersonGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var response = GetCurrentSchedule( rockContext, selectedPersonGuid, out string errorMessage );

                if ( errorMessage.IsNotNullOrWhiteSpace() )
                {
                    return ActionBadRequest( errorMessage );
                }

                return ActionOk( response );
            }
        }

        /// <summary>
        /// Performs the specified action on a current schedule row.
        /// </summary>
        /// <param name="bag">The information needed to perform the specified action.</param>
        /// <returns>An object containing information about the outcome of the request.</returns>
        [BlockAction]
        public BlockActionResult PerformScheduleRowAction( PerformScheduleRowActionRequestBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var response = PerformScheduleRowAction( rockContext, bag, out string errorMessage );

                if ( errorMessage.IsNotNullOrWhiteSpace() )
                {
                    return ActionBadRequest( errorMessage );
                }

                return ActionOk( response );
            }
        }

        /// <summary>
        /// Saves decline reason information for a declined schedule row.
        /// </summary>
        /// <param name="bag">The information about the schedule row being declined.</param>
        /// <returns>An object containing information about the outcome of the request.</returns>
        [BlockAction]
        public BlockActionResult SaveDeclineReason( SaveDeclineReasonRequestBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                SaveDeclineReason( rockContext, bag, out string errorMessage );

                if ( errorMessage.IsNotNullOrWhiteSpace() )
                {
                    return ActionBadRequest( errorMessage );
                }

                return ActionOk();
            }
        }

        /// <summary>
        /// Saves unavailability for the selected people.
        /// </summary>
        /// <param name="bag">The information about the unavailability to be saved.</param>
        /// <returns>An object containing information about the outcome of the request.</returns>
        [BlockAction]
        public BlockActionResult SaveUnavailability( SaveUnavailabilityRequestBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                SaveUnavailability( rockContext, bag, out string errorMessage );

                if ( errorMessage.IsNotNullOrWhiteSpace() )
                {
                    return ActionBadRequest( errorMessage );
                }

                return ActionOk();
            }
        }

        /// <summary>
        /// Gets information about the selected person's schedule preferences.
        /// </summary>
        /// <param name="bag">The information needed to get the selected person's schedule preferences.</param>
        /// <returns>An object containing information about the selected person's schedule preferences.</returns>
        [BlockAction]
        public BlockActionResult GetSchedulePreferences( GetSchedulePreferencesRequestBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var response = GetSchedulePreferences( rockContext, bag, out string errorMessage );

                if ( errorMessage.IsNotNullOrWhiteSpace() )
                {
                    return ActionBadRequest( errorMessage );
                }

                return ActionOk( response );
            }
        }

        /// <summary>
        /// Saves schedule preference selections for the selected person and group.
        /// </summary>
        /// <param name="bag">The information about the schedule preference to be saved.</param>
        /// <returns>An object containing information about the outcome of the request.</returns>
        [BlockAction]
        public BlockActionResult SaveSchedulePreference( SaveSchedulePreferenceRequestBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                SaveSchedulePreference( rockContext, bag, out string errorMessage );

                if ( errorMessage.IsNotNullOrWhiteSpace() )
                {
                    return ActionBadRequest( errorMessage );
                }

                return ActionOk();
            }
        }

        /// <summary>
        /// Gets schedule preference assignment options for the selected person, group and optional assignment.
        /// </summary>
        /// <param name="bag">The information needed to get schedule preference assignment options.</param>
        /// <returns>An object containing information about schedule preference assignment options.</returns>
        [BlockAction]
        public BlockActionResult GetAssignmentOptions( GetAssignmentOptionsRequestBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var (assignmentOptions, _, __, errorMessage) = GetAssignmentOptions( rockContext, bag );

                if ( errorMessage.IsNotNullOrWhiteSpace() )
                {
                    return ActionBadRequest( errorMessage );
                }

                return ActionOk( assignmentOptions );
            }
        }

        /// <summary>
        /// Saves a schedule preference assignment for the selected selected person and group.
        /// </summary>
        /// <param name="bag">The information about the schedule preference assignment to be saved.</param>
        /// <returns>An object containing information about the outcome of the request.</returns>
        [BlockAction]
        public BlockActionResult SaveAssignment( SaveAssignmentRequestBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var response = SaveAssignment( rockContext, bag );

                // In this case, any error message will be included in the response,
                // along with an updated options object, to give the individual a
                // chance to try again.
                return ActionOk( response );
            }
        }

        /// <summary>
        /// Deletes the specified schedule preference assignment for the selected person.
        /// </summary>
        /// <param name="bag">The information about the schedule preference assignment to be deleted.</param>
        /// <returns>An object containing information about the outcome of the request.</returns>
        [BlockAction]
        public BlockActionResult DeleteAssignment( DeleteAssignmentRequestBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                DeleteAssignment( rockContext, bag, out string errorMessage );

                if ( errorMessage.IsNotNullOrWhiteSpace() )
                {
                    return ActionBadRequest( errorMessage );
                }

                return ActionOk();
            }
        }

        /// <summary>
        /// Gets information about the selected person's additional time sign-ups.
        /// </summary>
        /// <param name="bag">The information needed to get the selected person's additional time sign-ups.</param>
        /// <returns>An object containing information about the selected person's additional time sign-ups.</returns>
        [BlockAction]
        public BlockActionResult GetSignUps( GetSignUpsRequestBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var (signUps, _, __, ___, errorMessage) = GetSignUps( rockContext, bag );

                if ( errorMessage.IsNotNullOrWhiteSpace() )
                {
                    return ActionBadRequest( errorMessage );
                }

                return ActionOk( signUps );
            }
        }

        /// <summary>
        /// Saves or deletes an additional time sign-up for the select person, group, schedule and location.
        /// </summary>
        /// <param name="bag">The information about the additional time sign-up to be saved or deleted.</param>
        /// <returns>An object containing information about the outcome of the request.</returns>
        [BlockAction]
        public BlockActionResult SaveSignUp( SaveSignUpRequestBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var response = SaveSignUp( rockContext, bag );

                // In this case, any error message will be included in the response,
                // along with an updated sign-ups object, to give the individual a
                // chance to try again.
                return ActionOk( response );
            }
        }

        #endregion Block Actions

        #region Supporting Members

        /// <summary>
        /// A runtime object representing the common data needed for the block to operate.
        /// <para>
        /// This object is intended to be assembled using a combination of page parameter values and existing database records;
        /// to be passed between private helper methods as needed, and ultimately sent back out the door in the form of view models.
        /// </para>
        /// </summary>
        private class ToolboxData
        {
            /// <summary>
            /// Gets or sets the error message.
            /// </summary>
            public string ErrorMessage { get; set; }

            /// <summary>
            /// Gets or sets the toolbox person.
            /// </summary>
            public Person ToolboxPerson { get; set; }

            /// <summary>
            /// Gets or sets the toolbox person's schedulable family member people. Note that the
            /// toolbox person themself will always be the first person in this list, regardless
            /// of whether or not they belong to any schedulable groups.
            /// </summary>
            public List<FamilyMemberPerson> SchedulableFamilyMemberPeople { get; set; }

            /// <summary>
            /// Gets or sets the selected person.
            /// This could be the toolbox person or one of their family members.
            /// </summary>
            public Person SelectedPerson { get; set; }

            /// <summary>
            /// Gets or sets the selected person's primary alias.
            /// </summary>
            public PersonAlias SelectedPrimaryAlias { get; set; }

            /// <summary>
            /// Gets or sets the selected person's family member people, regardless of whether
            /// they're schedulable. Note that the selected person themself will always be the
            /// first person in this list.
            /// </summary>
            public List<FamilyMemberPerson> AllFamilyMemberPeople { get; set; }

            /// <summary>
            /// Gets or sets the selected person's schedulable groups.
            /// </summary>
            public List<SchedulableGroup> SchedulableGroups { get; set; }

            /// <summary>
            /// Gets or sets the selected schedulable group.
            /// </summary>
            public SchedulableGroup SelectedGroup { get; set; }

            /// <summary>
            /// Gets or sets whether the "schedule preferences" feature is enabled.
            /// </summary>
            public bool IsSchedulePreferencesEnabled { get; set; }

            /// <summary>
            /// Gets or sets whether the "schedule unavailability" feature is enabled.
            /// </summary>
            public bool IsScheduleUnavailabilityEnabled { get; set; }

            /// <summary>
            /// Gets or sets whether the "additional time sign-up" feature is enabled.
            /// </summary>
            public bool IsAdditionalTimeSignUpEnabled { get; set; }

            /// <summary>
            /// Gets or sets whether to require a location for additional time sign-up.
            /// </summary>
            public bool IsLocationRequiredForAdditionalTimeSignUp { get; set; }

            /// <summary>
            /// Gets or sets whether the "immediate needs" feature is enabled.
            /// </summary>
            public bool IsImmediateNeedsEnabled { get; set; }

            /// <summary>
            /// Gets or sets the toolbox action type.
            /// </summary>
            public ToolboxActionType ToolboxActionType { get; set; }
        }

        /// <summary>
        /// A runtime object to dictate how toolbox data should be loaded.
        /// </summary>
        private class GetToolboxDataConfig
        {
            /// <summary>
            /// Gets or sets an optional person unique identifier to override that of the toolbox person.
            /// </summary>
            public Guid? SelectedPersonGuidOverride { get; set; }

            /// <summary>
            /// Gets or sets an optional toolbox action type to override the page parameter.
            /// </summary>
            public ToolboxActionType? ActionTypeOverride { get; set; }

            /// <summary>
            /// Gets or sets optional entity keys to override page parameters.
            /// </summary>
            public Dictionary<string, string> EntityKeyOverrides { get; set; }

            /// <summary>
            /// Gets or sets how to load the selected person's schedulable groups.
            /// </summary>
            public SchedulableGroupLoadingMode SchedulableGroupLoadingMode { get; set; } = SchedulableGroupLoadingMode.DoNotLoad;
        }

        /// <summary>
        /// A runtime object to represent a family member person.
        /// </summary>
        private class FamilyMemberPerson
        {
            /// <summary>
            /// Gets or sets the person.
            /// </summary>
            public Person Person { get; set; }

            /// <summary>
            /// Gets or sets the person's primary person alias.
            /// </summary>
            public PersonAlias PrimaryAlias { get; set; }

            /// <summary>
            /// Gets or sets whether this person is schedulable.
            /// </summary>
            public bool IsSchedulable { get; set; }
        }

        /// <summary>
        /// A runtime object to represent a schedulable group and the associated group member.
        /// </summary>
        private class SchedulableGroup
        {
            /// <summary>
            /// The schedulable group.
            /// </summary>
            public Rock.Model.Group Group { get; set; }

            /// <summary>
            /// The selected person's associated group member.
            /// If the person is in there more than once, this will prefer the IsLeader role.
            /// </summary>
            public GroupMember GroupMember { get; set; }
        }

        /// <summary>
        /// An enum to dictate how to load the selected person's schedulable groups.
        /// </summary>
        private enum SchedulableGroupLoadingMode
        {
            /// <summary>
            /// Do not load any of the selected person's schedulable groups.
            /// </summary>
            DoNotLoad = 0,

            /// <summary>
            /// Load all of the selected person's schedulable groups.
            /// </summary>
            LoadAll = 1,

            /// <summary>
            /// Load only the the selected person's schedulable groups that actually have locations and schedules.
            /// </summary>
            LoadOnlyGroupsHavingLocationSchedules = 2
        }

        /// <summary>
        /// A runtime object to represent an additional time sign-up.
        /// </summary>
        private class AdditionalTimeSignUp
        {
            /// <summary>
            /// Gets or sets the start date/time.
            /// </summary>
            public DateTime StartDateTime { get; set; }

            /// <summary>
            /// Gets or sets whether this sign-up is an immediate need.
            /// </summary>
            public bool IsImmediateNeed { get; set; }
        }

        #endregion Supporting Classes
    }
}
