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
using Rock.Attribute;
using Rock.ClientService.Core.Campus;
using Rock.ClientService.Core.Campus.Options;
using Rock.Communication;
using Rock.Data;
using Rock.Enums.Blocks.Crm.FamilyPreRegistration;
using Rock.Logging;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks.Crm.FamilyPreRegistration;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Crm
{
    [DisplayName( "Family Pre Registration" )]
    [Category( "Obsidian > CRM" )]
    [Description( "Provides a way to allow people to pre-register their families for weekend check-in." )]

    #region Block Attributes

    [BooleanField(
        "Show Campus",
        Key = AttributeKey.ShowCampus,
        Description = "Should the campus field be displayed? If there is only one active campus then the campus field will not show.",
        DefaultBooleanValue = true,
        Order = 0 )]

    [CampusField(
        name: "Default Campus",
        description: "An optional campus to use by default when adding a new family.",
        required: false,
        includeInactive: true,
        key: AttributeKey.DefaultCampus,
        order: 1 )]

    [BooleanField(
        "Require Campus",
        Key = AttributeKey.RequireCampus,
        Description = "Require that a campus be selected",
        DefaultBooleanValue = true,
        Order = 2 )]

    [DefinedValueField(
        "Campus Types",
        Key = AttributeKey.CampusTypes,
        Description = "This setting filters the list of campuses by type that are displayed in the campus drop-down.",
        IsRequired = false,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_TYPE,
        AllowMultiple = true,
        Order = 3 )]

    [DefinedValueField(
        "Campus Statuses",
        Key = AttributeKey.CampusStatuses,
        Description = "This setting filters the list of campuses by statuses that are displayed in the campus drop-down.",
        IsRequired = false,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_STATUS,
        AllowMultiple = true,
        Order = 4 )]

    // LPC CODE - Note: also shifted Order Number of subsequent items
    [BooleanField(
        "Prevent Updating Existing Campus to Alternate Language",
        Key = AttributeKey.PreventCampusLanguageUpdate,
        Description = "Should changing a family's existing campus to the same campus in another language be prevented? If someone's family's campus is LPE - Rockwall, and they select Rockwall on this form, should their campus stay as LPE - Rockwall?",
        DefaultBooleanValue = true,
        Order = 5 )]
    // END LPC CODE

    [AttributeField(
        "Campus Schedule Attribute",
        Key = AttributeKey.CampusScheduleAttribute,
        Description = "Allows you to select a campus attribute that contains schedules for determining which dates and times for which pre-registration is available. This requires the creation of an Entity attribute for 'Campus' using a Field Type of 'Schedules'. The schedules can then be selected in the 'Edit Campus' block. The Lava merge field for this in workflows is 'ScheduleId'.",
        EntityTypeGuid = Rock.SystemGuid.EntityType.CAMPUS,
        IsRequired = false,
        Order = 6 )]

    [CustomDropdownListField(
        "Planned Visit Date",
        Key = AttributeKey.PlannedVisitDate,
        Description = "How should the Planned Visit Date field be displayed. The date selected by the user is only used for the workflow. If the 'Campus Schedule Attribute' block setting has a selection this will control if schedule date/time are required or not but not if it shows or not. The Lava merge field for this in workflows is 'PlannedVisitDate'.",
        ListSource = ListSource.HIDE_OPTIONAL_REQUIRED,
        IsRequired = false,
        DefaultValue = "Optional",
        Order = 7 )]

    [IntegerField(
        "Scheduled Days Ahead",
        Key = AttributeKey.ScheduledDaysAhead,
        Description = "When using campus specific scheduling this setting determines how many days ahead a person can select. The default is 28 days.",
        IsRequired = false,
        DefaultIntegerValue = 28,
        Order = 8 )]

    [AttributeField(
        "Family Attributes",
        Key = AttributeKey.FamilyAttributes,
        Description = "The Family attributes that should be displayed",
        EntityTypeGuid = Rock.SystemGuid.EntityType.GROUP,
        EntityTypeQualifierColumn = "GroupTypeId",
        EntityTypeQualifierValue = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY,
        IsRequired = false,
        AllowMultiple = true,
        Order = 9 )]

    [BooleanField(
        "Allow Updates",
        Key = AttributeKey.AllowUpdates,
        Description = "If the person visiting this block is logged in, should the block be used to update their family? If not, a new family will always be created unless 'Auto Match' is enabled and the information entered matches an existing person.",
        DefaultBooleanValue = false,
        Order = 10 )]

    [BooleanField(
        "Auto Match",
        Key = AttributeKey.AutoMatch,
        Description = "Should this block attempt to match people to current records in the database?",
        DefaultBooleanValue = true,
        Order = 11 )]

    [DefinedValueField(
        "Connection Status",
        Key = AttributeKey.ConnectionStatus,
        Description = "The connection status that should be used when adding new people.",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS,
        IsRequired = false,
        AllowMultiple = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_VISITOR,
        Order = 12 )]

    [DefinedValueField(
        "Record Status",
        Key = AttributeKey.RecordStatus,
        Description = "The record status that should be used when adding new people.",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS,
        IsRequired = false,
        AllowMultiple = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE,
        Order = 13 )]

    [WorkflowTypeField(
        "Workflow Types",
        Key = AttributeKey.WorkflowTypes,
        Description = BlockAttributeDescription.WorkflowTypes,
        AllowMultiple = true,
        IsRequired = false,
        Order = 14 )]

    [WorkflowTypeField(
        "Parent Workflow",
        Key = AttributeKey.ParentWorkflow,
        Description = BlockAttributeDescription.ParentWorkflow,
        AllowMultiple = false,
        IsRequired = false,
        Order = 15 )]

    [WorkflowTypeField(
        "Child Workflow",
        Key = AttributeKey.ChildWorkflow,
        Description = BlockAttributeDescription.ChildWorkflow,
        AllowMultiple = false,
        IsRequired = false,
        Order = 16 )]

    [CodeEditorField(
        "Redirect URL",
        Key = AttributeKey.RedirectURL,
        DefaultValue = "/FamilyPreRegistrationSuccess?FamilyId={{ Family.Id }}&Parents={{ ParentIds }}&Children={{ ChildIds }}&When={{ PlannedVisitDate }}",
        Description = BlockAttributeDescription.RedirectURL,
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        Order = 17 )]

    [TextField(
        "Planned Visit Information Panel Title",
        Key = AttributeKey.PlannedVisitInformationPanelTitle,
        Description = "The title for the Planned Visit Information panel",
        DefaultValue = "Visit Information",
        IsRequired = false,
        Order = 18 )]

    [CustomDropdownListField(
        "Show SMS Opt-in",
        Key = AttributeKey.DisplaySmsOptIn,
        Description = "If 'Mobile Phone' is not set to 'Hide' then this option will show the SMS Opt-In option for the selection.",
        ListSource = ListSource.DISPLAY_SMS_OPT_IN,
        IsRequired = true,
        DefaultValue = "Hide",
        Order = 19 )]

    #region Adult Category

    [CustomDropdownListField(
        "Suffix",
        Key = AttributeKey.AdultSuffix,
        Description = "How should Suffix be displayed for adults?",
        ListSource = ListSource.HIDE_OPTIONAL,
        IsRequired = false,
        DefaultValue = "Hide",
        Category = CategoryKey.AdultFields,
        Order = 0 )]

    [CustomDropdownListField(
        "Gender",
        Key = AttributeKey.AdultGender,
        Description = "How should Gender be displayed for adults?",
        ListSource = ListSource.HIDE_OPTIONAL_REQUIRED,
        IsRequired = false,
        DefaultValue = "Optional",
        Category = CategoryKey.AdultFields,
        Order = 1 )]

    [CustomDropdownListField(
        "Birth Date",
        Key = AttributeKey.AdultBirthdate,
        Description = "How should Birth Date be displayed for adults?",
        ListSource = ListSource.HIDE_OPTIONAL_REQUIRED_ADULT_BIRTHDATE,
        IsRequired = false,
        DefaultValue = "Optional",
        Category = CategoryKey.AdultFields,
        Order = 2 )]

    [CustomDropdownListField(
        "Marital Status",
        Key = AttributeKey.AdultMaritalStatus,
        Description = "How should Marital Status be displayed for adults?",
        ListSource = ListSource.HIDE_OPTIONAL_REQUIRED,
        IsRequired = false,
        DefaultValue = "Required",
        Category = CategoryKey.AdultFields,
        Order = 3 )]

    [CustomDropdownListField(
        "Email",
        Key = AttributeKey.AdultEmail,
        Description = "How should Email be displayed for adults?",
        ListSource = ListSource.HIDE_OPTIONAL_REQUIRED,
        IsRequired = false,
        DefaultValue = "Required",
        Category = CategoryKey.AdultFields,
        Order = 4 )]

    [CustomDropdownListField(
        "Mobile Phone",
        Key = AttributeKey.AdultMobilePhone,
        Description = "How should Mobile Phone be displayed for adults?",
        ListSource = ListSource.HIDE_OPTIONAL_REQUIRED,
        IsRequired = false,
        DefaultValue = "Required",
        Category = CategoryKey.AdultFields,
        Order = 5 )]

    [AttributeCategoryField(
        "Attribute Categories",
        Key = AttributeKey.AdultAttributeCategories,
        Description = "The adult Attribute Categories to display attributes from.",
        AllowMultiple = true,
        EntityTypeName = "Rock.Model.Person",
        IsRequired = false,
        Category = CategoryKey.AdultFields,
        Order = 6 )]

    [CustomDropdownListField(
        "Display Communication Preference",
        Key = AttributeKey.AdultDisplayCommunicationPreference,
        Description = "How should Communication Preference be displayed for adults?",
        ListSource = "Hide,Required",
        IsRequired = false,
        DefaultValue = "Hide",
        Category = CategoryKey.AdultFields,
        Order = 7 )]

    [CustomDropdownListField(
        "Address",
        Key = AttributeKey.AdultAddress,
        Description = "How should Address be displayed for adults?",
        ListSource = ListSource.HIDE_OPTIONAL_REQUIRED,
        IsRequired = false,
        DefaultValue = "Optional",
        Category = CategoryKey.AdultFields,
        Order = 8 )]

    [CustomDropdownListField(
        "Profile Photos",
        Key = AttributeKey.AdultProfilePhoto,
        Description = "How should Profile Photo be displayed for adults?",
        ListSource = ListSource.HIDE_SHOW_REQUIRED,
        IsRequired = false,
        DefaultValue = "Hide",
        Category = CategoryKey.AdultFields,
        Order = 9 )]

    [BooleanField(
        "Profile Photo Mode",
        Key = AttributeKey.AdultPhotoMode,
        Description = "How should Profile Photo be displayed for adults?\nUpload Photo (Default): Allows the user to upload a photo from their device.\nTake Photo: Allows the user to take a photo using their device's camera.",
        TrueText = "Take Photo",
        FalseText = "Upload Photo",
        IsRequired = false,
        DefaultBooleanValue = false,
        Category = CategoryKey.AdultFields,
        Order = 10 )]

    [CustomDropdownListField(
        "First Adult Create Account",
        Key = AttributeKey.FirstAdultCreateAccount,
        Description = "Allows the first adult to create an account for themselves.",
        ListSource = ListSource.HIDE_SHOW_REQUIRED,
        IsRequired = false,
        DefaultValue = "Hide",
        Category = CategoryKey.AdultFields,
        Order = 11 )]

    [TextField(
        "Create Account Title",
        Key = AttributeKey.CreateAccountTitle,
        Description = "Configures the description for the create account card.",
        IsRequired = false,
        DefaultValue = "Create Account",
        Category = CategoryKey.AdultFields,
        Order = 12 )]

    [TextField(
        "Create Account Description",
        Key = AttributeKey.CreateAccountDescription,
        Description = "Allows the first adult to create an account for themselves.",
        IsRequired = false,
        DefaultValue = "Create an account to personalize your experience and access additional capabilities on our site.",
        Category = CategoryKey.AdultFields,
        Order = 13 )]

    [CustomDropdownListField(
        "Race",
        Key = AttributeKey.RaceOption,
        Description = "Allow Race to be optionally selected.",
        ListSource = ListSource.HIDE_OPTIONAL_REQUIRED,
        IsRequired = false,
        DefaultValue = "Hide",
        Category = CategoryKey.AdultFields,
        Order = 14 )]

    [CustomDropdownListField(
        "Ethnicity",
        Key = AttributeKey.EthnicityOption,
        Description = "Allow Ethnicity to be optionally selected.",
        ListSource = ListSource.HIDE_OPTIONAL_REQUIRED,
        IsRequired = false,
        DefaultValue = "Hide",
        Category = CategoryKey.AdultFields,
        Order = 15 )]

    #endregion

    #region Child Category

    [CustomDropdownListField(
        "Suffix",
        Key = AttributeKey.ChildSuffix,
        Description = "How should Suffix be displayed for children?",
        ListSource = ListSource.HIDE_OPTIONAL,
        IsRequired = false,
        DefaultValue = "Hide",
        Category = CategoryKey.ChildFields,
        Order = 0 )]

    [CustomDropdownListField(
        "Gender",
        Key = AttributeKey.ChildGender,
        Description = "How should Gender be displayed for children?",
        ListSource = ListSource.HIDE_OPTIONAL_REQUIRED,
        IsRequired = false,
        DefaultValue = "Optional",
        Category = CategoryKey.ChildFields,
        Order = 1 )]

    [CustomDropdownListField(
        "Birth Date",
        Key = AttributeKey.ChildBirthdate,
        Description = "How should Birth Date be displayed for children?",
        ListSource = ListSource.HIDE_OPTIONAL_REQUIRED,
        IsRequired = false,
        DefaultValue = "Required",
        Category = CategoryKey.ChildFields,
        Order = 2 )]

    [CustomDropdownListField(
        "Grade",
        Key = AttributeKey.ChildGrade,
        Description = "How should Grade be displayed for children?",
        ListSource = ListSource.HIDE_OPTIONAL_REQUIRED,
        IsRequired = false,
        DefaultValue = "Optional",
        Category = CategoryKey.ChildFields,
        Order = 3 )]

    // LPC CODE
    [CustomDropdownListField(
        "Self Release",
        Key = AttributeKey.SelfRelease,
        Description = "How should Self Release be displayed for children?",
        ListSource = ListSource.HIDE_OPTIONAL,
        IsRequired = false,
        DefaultValue = "Optional",
        Category = CategoryKey.ChildFields,
        Order = 4 )]
    // END LPC CODE

    [CustomDropdownListField(
        "Email",
        Key = AttributeKey.ChildEmail,
        Description = "How should Email be displayed for children?  Be sure to seek legal guidance when collecting email addresses on minors.",
        ListSource = ListSource.HIDE_OPTIONAL_REQUIRED,
        IsRequired = false,
        DefaultValue = "Hide",
        Category = CategoryKey.ChildFields,
        Order = 5 )]

    [CustomDropdownListField(
        "Mobile Phone",
        Key = AttributeKey.ChildMobilePhone,
        Description = "How should Mobile Phone be displayed for children?",
        ListSource = ListSource.HIDE_OPTIONAL_REQUIRED,
        IsRequired = false,
        DefaultValue = "Hide",
        Category = CategoryKey.ChildFields,
        Order = 6 )]

    [AttributeCategoryField(
        "Attribute Categories",
        Key = AttributeKey.ChildAttributeCategories,
        Description = "The children Attribute Categories to display attributes from.",
        AllowMultiple = true,
        EntityTypeName = "Rock.Model.Person",
        IsRequired = false,
        Category = CategoryKey.ChildFields,
        Order = 7 )]

    [CustomDropdownListField(
        "Display Communication Preference",
        Key = AttributeKey.ChildDisplayCommunicationPreference,
        Description = "How should Communication Preference be displayed for children?",
        ListSource = "Hide,Required",
        IsRequired = false,
        DefaultValue = "Hide",
        Category = CategoryKey.ChildFields,
        Order = 8 )]

    [CustomDropdownListField(
        "Profile Photos",
        Key = AttributeKey.ChildProfilePhoto,
        Description = "How should Profile Photo be displayed for children?",
        ListSource = ListSource.HIDE_SHOW_REQUIRED,
        IsRequired = false,
        DefaultValue = "Hide",
        Category = CategoryKey.ChildFields,
        Order = 9 )]

    [BooleanField(
        "Profile Photo Mode",
        Key = AttributeKey.ChildPhotoMode,
        Description = "How should Profile Photo be displayed for children?\nUpload Photo (Default): Allows the user to upload a photo from their device.\nTake Photo: Allows the user to take a photo using their device's camera.",
        TrueText = "Take Photo",
        FalseText = "Upload Photo",
        IsRequired = false,
        DefaultBooleanValue = false,
        Category = CategoryKey.ChildFields,
        Order = 10 )]

    [CustomDropdownListField(
        "Race",
        Key = AttributeKey.ChildRaceOption,
        Description = "Allow Race to be optionally selected.",
        ListSource = ListSource.HIDE_OPTIONAL_REQUIRED,
        IsRequired = false,
        DefaultValue = "Hide",
        Category = CategoryKey.ChildFields,
        Order = 11 )]

    [CustomDropdownListField(
        "Ethnicity",
        Key = AttributeKey.ChildEthnicityOption,
        Description = "Allow Ethnicity to be optionally selected.",
        ListSource = ListSource.HIDE_OPTIONAL_REQUIRED,
        IsRequired = false,
        DefaultValue = "Hide",
        Category = CategoryKey.ChildFields,
        Order = 12 )]
    #endregion

    #region Child Relationship Category

    [CustomEnhancedListField(
        "Relationship Types",
        Key = AttributeKey.Relationships,
        Description = "The possible child-to-adult relationships. The value 'Child' will always be included.",
        ListSource = ListSource.SQL_RELATIONSHIP_TYPES,
        DefaultValue = "0",
        Category = CategoryKey.ChildRelationship,
        IsRequired = false,
        Order = 0 )]

    [CustomEnhancedListField(
        "Same Immediate Family Relationships",
        Key = AttributeKey.FamilyRelationships,
        Description = "The relationships which indicate the child is in the same immediate family as the adult(s) rather than creating a new family for the child. In most cases, 'Child' will be the only value included in this list. Any values included in this list that are not in the Relationship Types list will be ignored.",
        ListSource = ListSource.SQL_SAME_IMMEDIATE_FAMILY_RELATIONSHIPS,
        IsRequired = false,
        DefaultValue = "0",
        Category = CategoryKey.ChildRelationship,
        Order = 1 )]

    [CustomEnhancedListField(
        "Can Check-in Relationship",
        Key = AttributeKey.CanCheckinRelationships,
        Description = "Any relationships that, if selected, will also create an additional 'Can Check-in' relationship between the adult and the child. This is only necessary if the relationship (selected by the user) does not have the 'Allow Check-in' option.",
        ListSource = ListSource.SQL_CAN_CHECKIN_RELATIONSHIP,
        IsRequired = false,
        Category = CategoryKey.ChildRelationship,
        Order = 2 )]

    // LPC CODE
    [CustomEnhancedListField(
        "Temporary Can Check-in Relationship",
        Key = AttributeKey.TemporaryCanCheckinRelationships,
        Description = "Any relationships that, if selected, will also create an additional 'Temporary Can Check-in' relationship between the adult and the child. This is only necessary if the relationship (selected by the user) does not have the 'Allow Check-in' option.",
        ListSource = ListSource.SQL_TEMPORARY_CAN_CHECKIN_RELATIONSHIP,
        IsRequired = false,
        Category = CategoryKey.ChildRelationship,
        Order = 3 )]
    // END LPC CODE

    #endregion

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074" )]
    [Rock.SystemGuid.EntityTypeGuid( "C03CE9ED-8572-4BE5-AB2A-FF7498494905")]
    public partial class FamilyPreRegistration : RockObsidianBlockType
    {
        #region Attribute Keys, Categories and Values

        private static class AttributeKey
        {
            public const string ShowCampus = "ShowCampus";
            public const string DefaultCampus = "DefaultCampus";
            public const string CampusTypes = "CampusTypes";
            public const string CampusStatuses = "CampusStatuses";
            // LPC CODE
            public const string PreventCampusLanguageUpdate = "PreventCampusLanguageUpdate";
            // END LPC CODE
            public const string PlannedVisitDate = "PlannedVisitDate";
            public const string CampusScheduleAttribute = "CampusScheduleAttribute";
            public const string ScheduledDaysAhead = "ScheduledDaysAhead";
            public const string FamilyAttributes = "FamilyAttributes";
            public const string AllowUpdates = "AllowUpdates";
            public const string AutoMatch = "AutoMatch";
            public const string ConnectionStatus = "ConnectionStatus";
            public const string RecordStatus = "RecordStatus";
            public const string WorkflowTypes = "WorkflowTypes";
            public const string ParentWorkflow = "ParentWorkflow";
            public const string ChildWorkflow = "ChildWorkflow";
            public const string RedirectURL = "RedirectURL";
            public const string RequireCampus = "RequireCampus";
            public const string DisplaySmsOptIn = "DisplaySmsOptIn";
            
            public const string AdultSuffix = "AdultSuffix";
            public const string AdultGender = "AdultGender";
            public const string AdultBirthdate = "AdultBirthdate";
            public const string AdultMaritalStatus = "AdultMaritalStatus";
            public const string AdultEmail = "AdultEmail";
            public const string AdultMobilePhone = "AdultMobilePhone";
            public const string AdultAttributeCategories = "AdultAttributeCategories";
            public const string AdultDisplayCommunicationPreference = "AdultDisplayCommunicationPreference";
            public const string AdultAddress = "AdultAddress";
            public const string AdultProfilePhoto = "AdultProfilePhoto";
            // LPC CODE
            public const string AdultPhotoMode = "AdultPhotoMode";
            // END LPC CODE
            public const string FirstAdultCreateAccount = "FirstAdultCreateAccount";
            public const string CreateAccountTitle = "CreateAccountTitle";
            public const string CreateAccountDescription = "CreateAccountDescription";
            public const string RaceOption = "RaceOption";
            public const string EthnicityOption = "EthnicityOption";

            public const string ChildSuffix = "ChildSuffix";
            public const string ChildGender = "ChildGender";
            public const string ChildBirthdate = "ChildBirthdate";
            public const string ChildGrade = "ChildGrade";
            // LPC CODE
            public const string SelfRelease = "SelfRelease";
            // END LPC CODE
            public const string ChildMobilePhone = "ChildMobilePhone";
            public const string ChildEmail = "ChildEmail";
            public const string ChildAttributeCategories = "ChildAttributeCategories";
            public const string ChildDisplayCommunicationPreference = "ChildDisplayCommunicationPreference";
            public const string ChildProfilePhoto = "ChildProfilePhoto";
            // LPC CODE
            public const string ChildPhotoMode = "ChildPhotoMode";
            // END LPC CODE
            public const string ChildRaceOption = "ChildRaceOption";
            public const string ChildEthnicityOption = "ChildEthnicityOption";

            public const string Relationships = "Relationships";
            public const string FamilyRelationships = "FamilyRelationships";
            public const string CanCheckinRelationships = "CanCheckinRelationships";
            // LPC CODE
            public const string TemporaryCanCheckinRelationships = "TemporaryCanCheckinRelationships";
            // END LPC CODE

            public const string PlannedVisitInformationPanelTitle = "PlannedVisitInformationPanelTitle";
        }

        private static class CategoryKey
        {
            public const string AdultFields = "Adult Fields";
            public const string ChildFields = "Child Fields";
            public const string ChildRelationship = "Child Relationship";
        }

        private static class BlockAttributeDescription
        {
            public const string WorkflowTypes = @"The workflow type(s) to launch when a family is added. The primary family will be passed to each workflow as the entity. Additionally if the workflow type has any of the following attribute keys defined, those attribute values will also be set: ParentIds, ChildIds, PlannedVisitDate.";
            public const string ParentWorkflow = @"If set, this workflow type will launch for each parent provided. The parent will be passed to the workflow as the Entity.";
            public const string ChildWorkflow = @"If set, this workflow type will launch for each child provided. The child will be passed to the workflow as the Entity.";
            public const string RedirectURL = @"The URL to redirect user to when they have completed the registration. The merge fields that are available includes 'Family', which is an object for the primary family that is created/updated; 'RelatedChildren', which is a list of the children who have a relationship with the family, but are not in the family; 'ParentIds' which is a comma-delimited list of the person ids for each adult; 'ChildIds' which is a comma-delimited list of the person ids for each child; and 'PlannedVisitDate' which is the value entered for the Planned Visit Date field if it was displayed.";
        }

        private static class ListSource
        {
            public const string DISPLAY_SMS_OPT_IN = "Hide,First Adult,All Adults,Adults and Children";
            public const string HIDE_OPTIONAL_REQUIRED_ADULT_BIRTHDATE = "Hide^Hide,Optional^Optional,Required_Partial^Required (month and day only),Required^Required (full)";
            public const string HIDE_OPTIONAL_REQUIRED = "Hide,Optional,Required";
            public const string HIDE_SHOW_REQUIRED = "Hide,Show,Required";
            public const string HIDE_OPTIONAL = "Hide,Optional";
            public const string SQL_RELATIONSHIP_TYPES = @"
                SELECT 
	                R.[Id] AS [Value],
	                R.[Name] AS [Text]
                FROM [GroupType] T
                INNER JOIN [GroupTypeRole] R ON R.[GroupTypeId] = T.[Id]
                WHERE T.[Guid] = 'E0C5A0E2-B7B3-4EF4-820D-BBF7F9A374EF'
                AND R.[Name] <> 'Child'
                UNION ALL
                SELECT 0, 'Child'
                ORDER BY [Text]";

            public const string SQL_SAME_IMMEDIATE_FAMILY_RELATIONSHIPS = @"
                SELECT 
	                R.[Id] AS [Value],
	                R.[Name] AS [Text]
                FROM [GroupType] T
                INNER JOIN [GroupTypeRole] R ON R.[GroupTypeId] = T.[Id]
                WHERE T.[Guid] = 'E0C5A0E2-B7B3-4EF4-820D-BBF7F9A374EF'
                AND R.[Name] <> 'Child'
                UNION ALL
                SELECT 0, 'Child'
                ORDER BY [Text]";

            public const string SQL_CAN_CHECKIN_RELATIONSHIP = @"
                SELECT 
	                R.[Id] AS [Value],
	                R.[Name] AS [Text]
                FROM [GroupType] T
                INNER JOIN [GroupTypeRole] R ON R.[GroupTypeId] = T.[Id]
                WHERE T.[Guid] = 'E0C5A0E2-B7B3-4EF4-820D-BBF7F9A374EF'
                AND R.[Name] <> 'Child'
                UNION ALL
                SELECT 0, 'Child'
                ORDER BY [Text]";

            // LPC CODE
            public const string SQL_TEMPORARY_CAN_CHECKIN_RELATIONSHIP = @"
                SELECT 
	                R.[Id] AS [Value],
	                R.[Name] AS [Text]
                FROM [GroupType] T
                INNER JOIN [GroupTypeRole] R ON R.[GroupTypeId] = T.[Id]
                WHERE T.[Guid] = 'E0C5A0E2-B7B3-4EF4-820D-BBF7F9A374EF'
                AND R.[Name] <> 'Child'
                UNION ALL
                SELECT 0, 'Child'
                ORDER BY [Text]";
            // END LPC CODE
        }

        private static class PageParameterKey
        {
            public static string CampusGuid = "CampusGuid";
            public static string CampusId = "CampusId";
        }

        #endregion Attribute Keys, Categories and Values

        #region Properties

        /// <summary>
        /// Gets the adult attribute category guids.
        /// </summary>
        private List<Guid> AdultAttributeCategoryGuids => this.GetAttributeValues( AttributeKey.AdultAttributeCategories ).AsGuidList();

        /// <summary>
        /// Gets the optional unique identifier for the attribute to use for selecting a campus schedule.
        /// </summary>
        private Guid? CampusSchedulesAttributeGuid => this.GetAttributeValue( AttributeKey.CampusScheduleAttribute ).AsGuidOrNull();

        /// <summary>
        /// List of campus status defined value guids used to filter the campus field.
        /// </summary>
        private List<Guid> CampusStatusesFilter => this.GetAttributeValues( AttributeKey.CampusStatuses ).AsGuidList().Where( guid => DefinedValueCache.Get( guid ) != null ).ToList();

        /// <summary>
        /// List of campus type defined value guids used to filter the campus field.
        /// </summary>
        private List<Guid> CampusTypesFilter => this.GetAttributeValues( AttributeKey.CampusTypes ).AsGuidList().Where( guid => DefinedValueCache.Get( guid ) != null ).ToList();

        /// <summary>
        /// Gets the child attribute category guids.
        /// </summary>
        private List<Guid> ChildAttributeCategoryGuids => this.GetAttributeValues( AttributeKey.ChildAttributeCategories ).AsGuidList();

        /// <summary>
        /// Gets the "Create Account" description.
        /// </summary>
        private string CreateAccountDescription => this.GetAttributeValue( AttributeKey.CreateAccountDescription );

        /// <summary>
        /// Gets the "Create Account" title.
        /// </summary>
        private string CreateAccountTitle => this.GetAttributeValue( AttributeKey.CreateAccountTitle );

        /// <summary>
        /// An optional campus to use by default when adding a new family.
        /// </summary>
        private Guid DefaultCampusGuid => this.GetAttributeValue( AttributeKey.DefaultCampus ).AsGuid();

        /// <summary>
        /// Gets the family attribute guids.
        /// </summary>
        private List<Guid> FamilyAttributeGuids => this.GetAttributeValues( AttributeKey.FamilyAttributes ).AsGuidList();

        /// <summary>
        /// Gets whether to match people to current records in the database.
        /// </summary>
        private bool IsPersonMatchEnabled => this.GetAttributeValue( AttributeKey.AutoMatch ).AsBoolean();

        /// <summary>
        /// Indicates whether campus is hidden.
        /// </summary>
        private bool IsCampusHidden => !this.GetAttributeValue( AttributeKey.ShowCampus ).AsBoolean();

        /// <summary>
        /// Indicates whether campus is optional.
        /// </summary>
        private bool IsCampusOptional => !this.GetAttributeValue( AttributeKey.RequireCampus ).AsBoolean();

        /// <summary>
        /// If the person visiting this block is logged in, should the block be used to update their family?
        /// If not, a new family will always be created unless 'Auto Match' is enabled and the information entered matches an existing person.
        /// </summary>
        private bool IsFamilyUpdateAllowedForCurrentPerson => this.GetAttributeValue( AttributeKey.AllowUpdates ).AsBoolean();

        // LPC CODE
        /// <summary>
        /// Indicates changing a family's existing campus to the same campus in another language should be prevented.
        /// </summary>
        private bool PreventCampusLanguageUpdate => this.GetAttributeValue( AttributeKey.PreventCampusLanguageUpdate ).AsBoolean();
        // END LPC CODE

        /// <summary>
        /// Gets the connection status that should be used when adding new people.
        /// </summary>
        private Guid ConnectionStatusDefinedValueGuid => this.GetAttributeValue( AttributeKey.ConnectionStatus ).AsGuid();

        /// <summary>
        /// Gets the record status that should be used when adding new people.
        /// </summary>
        private Guid RecordStatusDefinedValueGuid => this.GetAttributeValue( AttributeKey.RecordStatus ).AsGuid();

        #endregion

        public override string BlockFileUrl => $"{base.BlockFileUrl}.obs";

        public override object GetObsidianBlockInitialization()
        {
            return GetInitializationBox();
        }

        #region Block Actions

        /// <summary>
        /// Gets the planned schedule dates.
        /// </summary>
        [BlockAction( "GetScheduleDates" )]
        public BlockActionResult GetPlannedScheduleDates( FamilyPreRegistrationGetScheduleDatesRequestBag bag )
        {
            if ( bag == null )
            {
                return ActionBadRequest();
            }

            var response = new FamilyPreRegistrationGetScheduleDatesResponseBag
            {
                ScheduleDates = new List<FamilyPreRegistrationScheduleDateBag>(),
                VisitDateField = GetVisitDateFieldBag( out var errorMessage )
            };

            var isCampusShown = !this.IsCampusHidden;

            if ( !response.VisitDateField.IsDateAndTimeShown || ( isCampusShown && ( !bag.CampusGuid.HasValue || bag.CampusGuid.Value == Guid.Empty ) ) )
            {
                // If the schedule panel is hidden or no campus is selected, then return empty schedule options.
                return ActionOk( response );
            }

            var occurrenceSchedules = new List<OccurrenceSchedule>();

            if ( !isCampusShown )
            {
                if ( CampusCache.All( includeInactive: false ).Count == 1 )
                {
                    // If there is just one active campus then use that one.
                    bag.CampusGuid = CampusCache.All( false )[0].Guid;
                }
                else
                {
                    // Rock should show an error before getting here, but just in case... If there are multiple campuses then we will need the campus to get the schedule.
                    // If the user has edit permission display an error message so the configuration can be fixed.
                    if ( this.BlockCache.IsAuthorized( Authorization.EDIT, this.GetCurrentPerson() ) )
                    {
                        response.ErrorTitle = "Must Show Campus";
                        response.ErrorText = "In order to show campus schedules the campus has to be shown so it can be selected. Change this block's 'Show Campus' attribute to 'Yes'. A user without edit permission to this block will just see \"Planned Visit Date\".";
                        return ActionOk( response );
                    }

                    // Since we can't show the schedules and this is not a user authorized to edit the block just show the date.
                    response.VisitDateField.IsDateAndTimeShown = false;
                    response.VisitDateField.IsDateShown = true;

                    return ActionOk( response );
                }
            }

            var campusScheduleAttributeKey = AttributeCache.Get( this.CampusSchedulesAttributeGuid.Value ).Key;
            var campusScheduleAttributeValue = CampusCache.Get( bag.CampusGuid.Value )?.GetAttributeValue( campusScheduleAttributeKey );

            if ( campusScheduleAttributeValue.IsNullOrWhiteSpace() )
            {
                // If the user has edit permission then display an error message so the configuration can be fixed.
                if ( this.BlockCache.IsAuthorized( Authorization.EDIT, this.GetCurrentPerson() ) )
                {
                    response.ErrorTitle = "Missing Campus Schedule attribute.";
                    response.ErrorText = "This requires the creation of an Entity attribute for 'Campus' using a Field Type of 'Schedules'. The schedules can then be selected in the 'Edit Campus' block. A user without edit permission to this block will just see \"Planned Visit Date\".";

                    return ActionOk( response );
                }

                // Since we can't show the schedules and this is not a user authorized to edit the block just show the date.
                response.VisitDateField.IsDateAndTimeShown = false;
                response.VisitDateField.IsDateShown = true;

                return ActionOk( response );
            }

            var scheduleDates = new HashSet<DateTime>();

            using ( var rockContext = new RockContext() )
            {
                var schedules = campusScheduleAttributeValue.Split( ',' ).Select( g => new ScheduleService( rockContext ).Get( g.AsGuid() ) );
                var daysAhead = GetAttributeValue( AttributeKey.ScheduledDaysAhead ).AsIntegerOrNull() ?? 28;

                foreach ( var schedule in schedules )
                {
                    var occurrences = schedule.GetScheduledStartTimes( RockDateTime.Today, RockDateTime.Today.AddDays( daysAhead ) ).ToList();
                    foreach ( var occurrence in occurrences )
                    {
                        occurrenceSchedules.Add( new OccurrenceSchedule { IcalOccurrenceDateTime = occurrence, ScheduleGuid = schedule.Guid } );
                        scheduleDates.Add( occurrence.Date );
                    }
                }
            }

            var sortedScheduleDates = scheduleDates.ToList();
            sortedScheduleDates.Sort();

            foreach ( var sortedScheduleDate in sortedScheduleDates )
            {
                var scheduleDate = new FamilyPreRegistrationScheduleDateBag
                {
                    Text = sortedScheduleDate.ToString( "dddd, MM/dd" ),
                    Value = sortedScheduleDate.ToString( "s" ),
                    ScheduleTimes = new List<ListItemBag>()
                };

                var scheduleOccurrencesForDate = occurrenceSchedules.Where( o => o.IcalOccurrenceDateTime.Date == sortedScheduleDate ).ToList();
                scheduleOccurrencesForDate.Sort( ( a, b ) => a.IcalOccurrenceDateTime.CompareTo( b.IcalOccurrenceDateTime ) );

                foreach ( var scheduleOccurrenceForDate in scheduleOccurrencesForDate )
                {
                    scheduleDate.ScheduleTimes.Add( new ListItemBag
                    {
                        Text = scheduleOccurrenceForDate.IcalOccurrenceDateTime.ToString( "h:mm tt" ),
                        Value = scheduleOccurrenceForDate.ScheduleGuid.ToString()
                    } );
                }

                response.ScheduleDates.Add( scheduleDate );
            }

            return ActionOk( response );
        }

        /// <summary>
        /// Gets a new child bag.
        /// </summary>
        /// <returns>A new child bag.</returns>
        [BlockAction( "GetNewChild" )]
        public BlockActionResult GetNewChild()
        {
            using ( var rockContext = new RockContext() )
            {
                var person = new Person();
                person.LoadAttributes();

                var personAttributes = GetAttributeCategoryAttributes( rockContext, this.ChildAttributeCategoryGuids );
                var bag = GetFamilyPreRegistrationPersonBag( person, Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid(), this.GetCurrentPerson(), personAttributes );

                return ActionOk( bag );
            }
        }

        /// <summary>
        /// Processes the family pre-registration request and saves the family and its members.
        /// </summary>
        /// <returns>The save response with errors or a successful redirect URL.</returns>
        [BlockAction( "Save" )]
        public BlockActionResult Save( FamilyPreRegistrationSaveRequestBag bag )
        {
            if ( !IsValid( bag, out var errorMessages ) )
            {
                return ActionOk( new FamilyPreRegistrationSaveResponseBag
                {
                    Errors = errorMessages
                } );
            }

            // Get some system values
            var familyGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
            var adultRoleId = familyGroupType.Roles.FirstOrDefault( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ).Id;
            var childRoleId = familyGroupType.Roles.FirstOrDefault( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() ).Id;

            var recordTypePersonId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
            var recordStatusValue = DefinedValueCache.Get( GetAttributeValue( AttributeKey.RecordStatus ).AsGuid() ) ?? DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() );
            var connectionStatusValue = DefinedValueCache.Get( GetAttributeValue( AttributeKey.ConnectionStatus ).AsGuid() ) ?? DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_VISITOR.AsGuid() );

            var knownRelationshipGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS.AsGuid() );
            var canCheckInRole = knownRelationshipGroupType.Roles.FirstOrDefault( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_CAN_CHECK_IN.AsGuid() );
            // LPC CODE
            var temporaryCanCheckInRole = knownRelationshipGroupType.Roles.FirstOrDefault( r => r.Guid == "EFAEE6AE-6889-43D8-84F2-25154AACEF69".AsGuid() );
            // END LPC CODE
            var knownRelationshipOwnerRoleGuid = Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER.AsGuid();

            // ...and some block settings
            var familyRelationships = GetAttributeValue( AttributeKey.FamilyRelationships ).SplitDelimitedValues().AsIntegerList();
            var canCheckinRelationships = GetAttributeValue( AttributeKey.CanCheckinRelationships ).SplitDelimitedValues().AsIntegerList();
            // LPC CODE
            var temporaryCanCheckinRelationships = GetAttributeValue( AttributeKey.TemporaryCanCheckinRelationships ).SplitDelimitedValues().AsIntegerList();
            // END LPC CODE
            var showChildMobilePhone = GetFieldBag( AttributeKey.ChildMobilePhone ).IsShown;
            var showChildEmailAddress = GetFieldBag( AttributeKey.ChildEmail ).IsShown;
            var showChildCommunicationPreference = GetFieldBag( AttributeKey.ChildDisplayCommunicationPreference ).IsShown;
            var showAdultAddress = GetFieldBag( AttributeKey.AdultAddress ).IsShown;
            var createFirstAdultAccount = GetFieldBag( AttributeKey.FirstAdultCreateAccount ).IsShown;

            // ...and some service objects
            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );
                var groupService = new GroupService( rockContext );
                var groupMemberService = new GroupMemberService( rockContext );
                var groupLocationService = new GroupLocationService( rockContext );

                // Check to see if we're viewing an existing family
                Rock.Model.Group primaryFamily = null;
                if ( bag.FamilyGuid.HasValue )
                {
                    primaryFamily = groupService.Get( bag.FamilyGuid.Value );
                }

                // If editing an existing family, we'll need to handle any children/relationships that they remove
                var processRemovals = primaryFamily != null && GetAttributeValue( AttributeKey.AllowUpdates ).AsBoolean();

                // If editing an existing family, we should also save any empty family values (campus, address)
                var saveEmptyValues = primaryFamily != null;

                // Save the adults
                var adults = new List<Person>();
                if ( bag.Adult1 != null )
                {
                    SaveAdult( ref primaryFamily, rockContext, adults, bag.Adult1, true );
                }
                if ( bag.Adult2 != null )
                {
                    SaveAdult( ref primaryFamily, rockContext, adults, bag.Adult2, false );
                }

                var isNewFamily = false;

                // If two adults were entered, let's check to see if we should assume they're married.
                if ( adults.Count == 2 )
                {
                    var marriedStatusValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED.AsGuid() );
                    if ( marriedStatusValueId.HasValue )
                    {
                        // As long as neither of the adults has a marital status...
                        if ( !adults.Any( a => a.MaritalStatusValueId.HasValue ) )
                        {
                            // ...set them all to married.
                            foreach ( var adult in adults )
                            {
                                adult.MaritalStatusValueId = marriedStatusValueId;
                            }

                            rockContext.SaveChanges();
                        }
                    }
                }

                // Create UserLogin for first adult.
                if ( GetFieldBag( AttributeKey.FirstAdultCreateAccount ).IsShown
                     && bag.CreateAccount != null
                     && bag.CreateAccount.Username.IsNotNullOrWhiteSpace()
                     && bag.CreateAccount.Password.IsNotNullOrWhiteSpace() )
                {
                    CreateUser( adults[0], bag.CreateAccount.Username, bag.CreateAccount.Password );
                }

                int? familyCampusId = null;
                var isCampusShown = !this.IsCampusHidden;

                if ( isCampusShown )
                {
                    // If the campus selection was visible, set the families campus based on selection.
                    familyCampusId = bag.CampusGuid.HasValue ? CampusCache.GetId( bag.CampusGuid.Value ) : null;
                }

                if ( primaryFamily != null )
                {
                    // If we do have an existing family and if the campus was shown, update the family's campus to the selected campus.
                    if ( isCampusShown )
                    {
                        // LPC CODE
                        if ( PreventCampusLanguageUpdate == true )
                        {
                            if ( primaryFamily.CampusId != null && familyCampusId != null )
                            {
                                var currentCampus = primaryFamily.Campus;
                                var newCampus = CampusCache.Get( ( int )familyCampusId );

                                // If language is different and one of the campus names contains the other campus name
                                if ( currentCampus.GetAttributeValue("Language") != newCampus.GetAttributeValue( "Language" )
                                    && ( newCampus.Name.Contains( currentCampus.Name ) || currentCampus.Name.Contains( newCampus.Name ) ) )
                                {
                                    // Don't update the campus
                                    familyCampusId = primaryFamily.CampusId;
                                }
                                else
                                {
                                    // The campus is at a different physical location - update it
                                    primaryFamily.CampusId = familyCampusId;
                                }
                            }
                            else
                            {
                                // If one of the values is blank, use the original logic
                                primaryFamily.CampusId = familyCampusId;
                            }
                        }
                        else
                        {
                            // Original logic
                            // END LPC CODE
                            primaryFamily.CampusId = familyCampusId;
                            // LPC CODE
                        }
                        // END LPC CODE
                    }
                }
                else
                {
                    // Otherwise, create a new family and save it with the selected campus or,
                    // if the campus was not shown, save the family with the default campus.
                    if ( !isCampusShown )
                    {
                        var defaultCampusGuid = this.DefaultCampusGuid;
                        familyCampusId = defaultCampusGuid != Guid.Empty ? CampusCache.GetId( defaultCampusGuid ) : null;
                    }

                    var familyLastName = bag.Adult1?.LastName.IsNotNullOrWhiteSpace() == true ? bag.Adult1.LastName : bag.Adult2?.LastName;
                    primaryFamily = CreateNewFamily( familyGroupType.Id, familyLastName, familyCampusId );
                    isNewFamily = true;
                    groupService.Add( primaryFamily );
                    saveEmptyValues = true;
                }

                // Save the family.
                rockContext.SaveChanges();

                // Make sure adults are part of the primary family, and if not, add them.
                foreach ( var adult in adults )
                {
                    var currentFamilyMember = primaryFamily.Members.FirstOrDefault( m => m.PersonId == adult.Id );

                    if ( currentFamilyMember == null )
                    {
                        currentFamilyMember = new GroupMember
                        {
                            GroupId = primaryFamily.Id,
                            PersonId = adult.Id,
                            GroupRoleId = adultRoleId,
                            GroupMemberStatus = GroupMemberStatus.Active
                        };

                        if ( isNewFamily )
                        {
                            adult.GivingGroupId = primaryFamily.Id;
                        }

                        groupMemberService.Add( currentFamilyMember );

                        rockContext.SaveChanges();
                    }
                }

                // Save the family address.
                if ( GetFieldBag( AttributeKey.AdultAddress ).IsShown )
                {
                    var homeLocationType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() );
                    if ( homeLocationType != null )
                    {
                        // Find a location record for the address that was entered.
                        Location location = null;
                        if ( bag.Address.Street1.IsNotNullOrWhiteSpace() && bag.Address.City.IsNotNullOrWhiteSpace() )
                        {
                            location = new LocationService( rockContext ).Get(
                                // TODO: The default country should be removed once Obsidian has full country support.
                                bag.Address.Street1,
                                bag.Address.Street2,
                                bag.Address.City,
                                bag.Address.State,
                                bag.Address.PostalCode,
                                bag.Address.Country ?? GlobalAttributesCache.Get().OrganizationCountry,
                                primaryFamily,
                                verifyLocation: true );
                        }
                        else
                        {
                            location = null;
                        }

                        // Check to see if family has an existing home address.
                        var groupLocation = primaryFamily.GroupLocations.FirstOrDefault( l => l.GroupLocationTypeValueId.HasValue && l.GroupLocationTypeValueId.Value == homeLocationType.Id );

                        if ( location != null )
                        {
                            if ( groupLocation == null || groupLocation.LocationId != location.Id )
                            {
                                // If family does not currently have a home address or it is different than the one entered, add a new address (move old address to prev).
                                GroupService.AddNewGroupAddress(
                                    rockContext,
                                    primaryFamily,
                                    homeLocationType.Guid.ToString(),
                                    location,
                                    moveExistingToPrevious: true,
                                    modifiedBy: string.Empty,
                                    isMailingLocation: true,
                                    isMappedLocation: true );
                            }
                        }
                        else
                        {
                            if ( groupLocation != null && saveEmptyValues )
                            {
                                // If an address was not entered, and family has one on record, update it to be a previous address.
                                var prevLocationTypeId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_PREVIOUS.AsGuid() );
                                groupLocation.GroupLocationTypeValueId = prevLocationTypeId.HasValue ? prevLocationTypeId : groupLocation.GroupLocationTypeValueId;
                            }
                        }

                        // Save the family address.
                        rockContext.SaveChanges();
                    }
                }

                // Save any family attribute values.
                primaryFamily.LoadAttributes( rockContext );
                var familyAttributes = GetFamilyAttributes( this.GetCurrentPerson() );
                primaryFamily.SetPublicAttributeValues(
                    bag.FamilyAttributeValues,
                    this.GetCurrentPerson(),
                    // Do not enforce security; otherwise, some attribute values may not be set for unauthenticated users.
                    enforceSecurity: false,
                    attributeFilter: a1 => familyAttributes.Any( a => a.Guid == a1.Guid ) );
                primaryFamily.SaveAttributeValues( rockContext );

                // Get the adult known relationship groups.
                var adultIds = adults.Select( a => a.Id ).ToList();
                var knownRelationshipGroupIds = groupMemberService.Queryable()
                    .Where( m =>
                        m.GroupRole.Guid == knownRelationshipOwnerRoleGuid &&
                        adultIds.Contains( m.PersonId ) )
                    .Select( m => m.GroupId )
                    .ToList();

                // Variables for tracking the new children/relationships that should exist.
                var newChildIds = new List<int>();
                var newRelationships = new Dictionary<int, List<int>>();

                // Reload the primary family.
                primaryFamily = groupService.Get( primaryFamily.Id );

                // Loop through each of the children.
                var newFamilyIds = new Dictionary<string, int>();
                var isChildRaceShown = GetFieldBag( AttributeKey.ChildRaceOption ).IsShown;
                var isChildEthnicityShown = GetFieldBag( AttributeKey.ChildEthnicityOption ).IsShown;
                var isChildEmailShown = GetFieldBag( AttributeKey.ChildEmail ).IsShown;
                var isChildCommunicationPreferenceShown = GetFieldBag( AttributeKey.ChildDisplayCommunicationPreference ).IsShown;
                var isChildSuffixShown = GetFieldBag( AttributeKey.ChildSuffix ).IsShown;
                var isChildGenderShown = GetFieldBag( AttributeKey.ChildGender ).IsShown;
                var isChildGradeShown = GetFieldBag( AttributeKey.ChildGrade ).IsShown;
                // LPC CODE
                var isChildSelfReleaseShown = GetFieldBag( AttributeKey.ChildGrade ).IsShown;
                // END LPC CODE
                var isChildProfileShown = GetFieldBag( AttributeKey.ChildProfilePhoto ).IsShown;

                var childAttributes = GetAttributeCategoryAttributes( rockContext, this.ChildAttributeCategoryGuids );

                var binaryFileService = new BinaryFileService( rockContext );
                var groupTypeRoleService = new GroupTypeRoleService( rockContext );

                foreach ( var child in bag.Children )
                {
                    // Save the child's person information.
                    var person = personService.Get( child.Guid );

                    // If person was not found, Look for existing person in same family with same name and birthdate
                    var birthDate = child.BirthDate.ToDateTime();
                    if ( person == null && birthDate.HasValue )
                    {
                        var possibleMatch = new Person { NickName = child.FirstName, LastName = child.LastName };
                        possibleMatch.SetBirthDate( birthDate );
                        person = primaryFamily.MatchingFamilyMember( possibleMatch );
                    }

                    // Otherwise create a new person.
                    if ( person == null )
                    {
                        person = new Person
                        {
                            Guid = child.Guid,
                            FirstName = child.FirstName.FixCase(),
                            LastName = child.LastName.FixCase(),
                            RecordTypeValueId = recordTypePersonId,
                            RecordStatusValueId = recordStatusValue != null ? recordStatusValue.Id : ( int? ) null,
                            ConnectionStatusValueId = connectionStatusValue != null ? connectionStatusValue.Id : ( int? ) null
                        };

                        personService.Add( person );
                    }
                    else
                    {
                        person.NickName = child.FirstName;
                        person.LastName = child.LastName;
                    }

                    if ( isChildSuffixShown )
                    {
                        person.SuffixValueId = child.SuffixDefinedValueGuid.HasValue ? DefinedValueCache.GetId( child.SuffixDefinedValueGuid.Value ) : null;
                    }

                    if ( isChildGenderShown )
                    {
                        person.Gender = child.Gender;
                    }

                    if ( birthDate.HasValue )
                    {
                        person.SetBirthDate( birthDate );
                    }

                    if ( isChildGradeShown )
                    {
                        person.GradeOffset = child.GradeDefinedValueGuid.HasValue ? DefinedValueCache.Get( child.GradeDefinedValueGuid.Value ).Value.AsIntegerOrNull() : null;
                    }

                    // Save the email address but do not allow deletes.
                    if ( isChildEmailShown && child.Email.IsNotNullOrWhiteSpace() )
                    {
                        person.Email = child.Email;
                    }

                    if ( isChildCommunicationPreferenceShown )
                    {
                        person.CommunicationPreference = ( CommunicationType ) ( int ) child.CommunicationPreference;
                    }

                    if ( isChildProfileShown && child.ProfilePhotoGuid.HasValue )
                    {
                        person.PhotoId = binaryFileService.GetId( child.ProfilePhotoGuid.Value );
                    }

                    if ( isChildRaceShown )
                    {
                        person.RaceValueId = child.RaceDefinedValueGuid.HasValue ? DefinedValueCache.GetId( child.RaceDefinedValueGuid.Value ) : null;
                    }

                    if ( isChildEthnicityShown )
                    {
                        person.EthnicityValueId = child.EthnicityDefinedValueGuid.HasValue ? DefinedValueCache.GetId( child.EthnicityDefinedValueGuid.Value ) : null;
                    }

                    // Save the child.
                    rockContext.SaveChanges();

                    // Save the mobile phone number but do not allow deletes.
                    if ( showChildMobilePhone && child.MobilePhone.IsNotNullOrWhiteSpace() )
                    {
                        var isSmsNumber = false;
                        var displaySmsOptInSetting = GetSmsOptInFieldBag();

                        if ( displaySmsOptInSetting.IsHidden && isChildCommunicationPreferenceShown )
                        {
                            // If the SMS Opt-In is not shown then use the communication preference to indicate if SMS should be enabled for the phone number.
                            // Since person.CommunicationPreference has already been updated use that instead of the child bag and doing the cast again.
                            isSmsNumber = person.CommunicationPreference == CommunicationType.SMS;
                        }
                        else if ( displaySmsOptInSetting.IsShowChildren )
                        {
                            isSmsNumber = child.IsMessagingEnabled;
                        }

                        SavePhoneNumber( rockContext, person.Id, child.MobilePhoneCountryCode, child.MobilePhone, isSmsNumber );
                    }

                    // Save the attributes for the child.
                    person.LoadAttributes();
                    person.SetPublicAttributeValues(
                        child.AttributeValues,
                        this.GetCurrentPerson(),
                        // Do not enforce security; otherwise, some attribute values may not be set for unauthenticated users.
                        enforceSecurity: false,
                        attributeFilter: a1 => childAttributes.Any( a => a.Guid == a1.Guid ) );

                    // LPC CODE
                    if ( child.Allergy != null && child.Allergy != "" )
                    {
                        person.SetAttributeValue( "Allergy", child.Allergy );
                    }
                    if ( isChildGradeShown && isChildSelfReleaseShown )
                    {
                        int grade = 12 - ( child.GradeDefinedValueGuid.HasValue ? DefinedValueCache.Get( child.GradeDefinedValueGuid.Value ).Value.AsInteger() : 0);
                        if ( grade == 4 || grade == 5 )
                        {
                            person.SetAttributeValue( "Arena-16-384", child.IsSelfRelease.ToString() );
                        }
                    }
                    // END LPC CODE

                    person.SaveAttributeValues( rockContext );

                    // Get the child's current family state.
                    var inPrimaryFamily = primaryFamily.Members.Any( m => m.PersonId == person.Id );

                    // Get what the family/relationship state should be for the child
                    var childFamilyRoleId = child.FamilyRoleGuid.HasValue ? groupTypeRoleService.GetId( child.FamilyRoleGuid.Value ) : null;
                    var shouldBeInPrimaryFamily = childRoleId == childFamilyRoleId || familyRelationships.Contains( childFamilyRoleId ?? 0 );
                    var newRelationshipId = shouldBeInPrimaryFamily ? ( int? ) null : childFamilyRoleId;
                    var canCheckin = !shouldBeInPrimaryFamily && canCheckinRelationships.Contains( childFamilyRoleId ?? -1 );

                    // Check to see if child needs to be added to the primary family or not
                    if ( shouldBeInPrimaryFamily )
                    {
                        // If so, add to list of children.
                        newChildIds.Add( person.Id );

                        if ( !inPrimaryFamily )
                        {
                            var familyMember = new GroupMember
                            {
                                GroupId = primaryFamily.Id,
                                PersonId = person.Id,
                                GroupRoleId = childRoleId,
                                GroupMemberStatus = GroupMemberStatus.Active
                            };

                            groupMemberService.Add( familyMember );

                            // Save the child as a family member.
                            rockContext.SaveChanges();
                        }

                        // LPC CODE
                        foreach ( var adultId in adultIds )
                        {
                            groupMemberService.CreateKnownRelationship( adultId, person.Id, canCheckInRole.Id );
                            newRelationships.AddOrIgnore( person.Id, new List<int>() );
                            newRelationships[person.Id].Add( canCheckInRole.Id );
                        }
                        // END LPC CODE
                    }
                    else
                    {
                        // Make sure they have another family.
                        EnsurePersonInOtherFamily( rockContext, familyGroupType.Id, primaryFamily.Id, person.Id, person.LastName, childRoleId, newFamilyIds, familyCampusId );

                        // If the selected relationship for this person should also create the can-check in relationship, make sure to add it
                        if ( canCheckinRelationships.Contains( childFamilyRoleId ?? -1 ) )
                        {
                            foreach ( var adultId in adultIds )
                            {
                                groupMemberService.CreateKnownRelationship( adultId, person.Id, canCheckInRole.Id );
                                newRelationships.AddOrIgnore( person.Id, new List<int>() );
                                newRelationships[person.Id].Add( canCheckInRole.Id );
                            }
                        }

                        // LPC CODE
                        if ( temporaryCanCheckinRelationships.Contains( childFamilyRoleId ?? -1 ) )
                        {
                            foreach ( var adultId in adultIds )
                            {
                                groupMemberService.CreateKnownRelationship( adultId, person.Id, temporaryCanCheckInRole.Id );
                                newRelationships.AddOrIgnore( person.Id, new List<int>() );
                                newRelationships[person.Id].Add( temporaryCanCheckInRole.Id );
                            }
                        }
                        // END LPC CODE
                    }

                    // Check to see if child needs to be removed from the primary family
                    if ( !shouldBeInPrimaryFamily && inPrimaryFamily )
                    {
                        RemovePersonFromFamily( rockContext, familyGroupType.Id, primaryFamily.Id, person.Id );
                    }

                    // If child has a relationship type, make sure they belong to a family, and ensure that they have that relationship with each adult
                    if ( newRelationshipId.HasValue )
                    {
                        foreach ( var adultId in adultIds )
                        {
                            groupMemberService.CreateKnownRelationship( adultId, person.Id, newRelationshipId.Value );
                            newRelationships.AddOrIgnore( person.Id, new List<int>() );
                            newRelationships[person.Id].Add( newRelationshipId.Value );
                        }
                    }
                }

                var childRelationshipTypes = GetChildRelationshipTypes();

                // If editing an existing family, check for any people that need to be removed from the family or relationships
                if ( processRemovals )
                {
                    // Find all the existing children that were removed and make sure they have another family, and then remove them from this family.
                    var na = new Dictionary<string, int>();
                    var childrenToRemove = groupMemberService.Queryable()
                        .Where( m =>
                            m.GroupId == primaryFamily.Id &&
                            m.GroupRoleId == childRoleId &&
                            !newChildIds.Contains( m.PersonId ) )
                        .ToList();
                    foreach ( var removedChild in childrenToRemove )
                    {
                        EnsurePersonInOtherFamily( rockContext, familyGroupType.Id, primaryFamily.Id, removedChild.Person.Id, removedChild.Person.LastName, childRoleId, na, familyCampusId );
                        groupMemberService.Delete( removedChild );
                    }

                    rockContext.SaveChanges();

                    // Find all the existing relationships that were removed and delete them.
                    var roleIds = childRelationshipTypes.Select( r => r.Key ).ToList();
                    foreach ( var groupMember in new PersonService( rockContext ).GetRelatedPeople( adultIds, roleIds ) )
                    {
                        if ( !newRelationships.ContainsKey( groupMember.PersonId ) || !newRelationships[groupMember.PersonId].Contains( groupMember.GroupRoleId ) )
                        {
                            foreach ( var adultId in adultIds )
                            {
                                groupMemberService.DeleteKnownRelationship( adultId, groupMember.PersonId, groupMember.GroupRoleId );
                                if ( canCheckinRelationships.Contains( groupMember.GroupRoleId ) )
                                {
                                    groupMemberService.DeleteKnownRelationship( adultId, groupMember.PersonId, canCheckInRole.Id );
                                }
                                // LPC CODE
                                if ( temporaryCanCheckinRelationships.Contains( groupMember.GroupRoleId ) )
                                {
                                    groupMemberService.DeleteKnownRelationship( adultId, groupMember.PersonId, temporaryCanCheckInRole.Id );
                                }
                                // END LPC CODE
                            }
                        }
                    }
                }

                var parentWorkflowTypeGuid = GetAttributeValue( AttributeKey.ParentWorkflow ).AsGuidOrNull();
                if ( parentWorkflowTypeGuid.HasValue )
                {
                    var workflowType = WorkflowTypeCache.Get( parentWorkflowTypeGuid.Value );
                    if ( workflowType != null && ( workflowType.IsActive ?? true ) )
                    {
                        foreach ( var adult in adults )
                        {
                            try
                            {
                                adult.LaunchWorkflow( parentWorkflowTypeGuid.Value, adult.FullName, workflowAttributeValues: null, initiatorPersonAliasId: null );
                            }
                            catch ( Exception ex )
                            {
                                RockLogger.Log.Error( RockLogDomains.Crm, ex, ex.Message );
                            }
                        }
                    }
                }

                var childIds = new List<int>( newChildIds );
                childIds.AddRange( newRelationships.Select( r => r.Key ).ToList() );
                var childWorkflowTypeGuid = GetAttributeValue( AttributeKey.ChildWorkflow ).AsGuidOrNull();
                if ( childWorkflowTypeGuid.HasValue && childIds.Any() )
                {
                    var workflowType = WorkflowTypeCache.Get( childWorkflowTypeGuid.Value );
                    if ( workflowType != null && ( workflowType.IsActive ?? true ) )
                    {
                        var children = personService.Queryable().Where( p => childIds.Contains( p.Id ) ).ToList();
                        foreach ( var child in children )
                        {
                            try
                            {
                                child.LaunchWorkflow( childWorkflowTypeGuid.Value, child.FullName, workflowAttributeValues: null, initiatorPersonAliasId: null );
                            }
                            catch ( Exception ex )
                            {
                                RockLogger.Log.Error( RockLogDomains.Crm, ex, ex.Message );
                            }
                        }
                    }
                }

                List<Guid> familyWorkflowGuids = GetAttributeValue( AttributeKey.WorkflowTypes ).SplitDelimitedValues().AsGuidList();
                var redirectUrlLavaTemplate = GetAttributeValue( AttributeKey.RedirectURL );
                string redirectUrl = null;
                if ( familyWorkflowGuids.Any() || redirectUrlLavaTemplate.IsNotNullOrWhiteSpace() )
                {
                    var family = groupService.Get( primaryFamily.Id );
                    var schedule = bag.ScheduleGuid.HasValue ? new ScheduleService( rockContext ).Get( bag.ScheduleGuid.Value ) : null;

                    // Create parameters
                    var parameters = new Dictionary<string, string>
                    {
                        { "ParentIds", adultIds.AsDelimited( "," ) },
                        { "ChildIds", childIds.AsDelimited( "," ) }
                    };

                    var visitDateField = GetVisitDateFieldBag( out var errorMessage );
                    var isPlannedVisitDateShown = visitDateField.IsShown && visitDateField.IsDateShown;
                    var isPlannedVisitScheduleShown = visitDateField.IsShown && visitDateField.IsDateAndTimeShown;

                    if ( isPlannedVisitDateShown )
                    {
                        var visitDate = bag.PlannedVisitDate?.Date;
                        if ( visitDate.HasValue )
                        {
                            parameters.Add( AttributeKey.PlannedVisitDate, visitDate.Value.ToString( "o" ) );
                        }
                    }
                    else if ( isPlannedVisitScheduleShown )
                    {
                        var visitDate = bag.PlannedVisitDate?.Date;
                        if ( visitDate.HasValue )
                        {
                            parameters.Add( AttributeKey.PlannedVisitDate, visitDate.Value.ToString( "o" ) );
                        }

                        if ( schedule != null )
                        {
                            // Also add the schedule id
                            parameters.Add( "ScheduleId", schedule.Id.ToString() );
                        }
                    }

                    // Look for any workflows
                    if ( familyWorkflowGuids.Any() )
                    {
                        // Launch all the workflows
                        foreach ( var familyWorkflowGuid in familyWorkflowGuids )
                        {
                            family.LaunchWorkflow( familyWorkflowGuid, family.Name, parameters, null );
                        }
                    }

                    if ( redirectUrlLavaTemplate.IsNotNullOrWhiteSpace() )
                    {
                        var relatedPersonIds = newRelationships.Select( r => r.Key ).ToList();
                        var relatedChildren = personService.Queryable().Where( p => relatedPersonIds.Contains( p.Id ) ).ToList();

                        // Create parameters

                        var mergeFields = this.RequestContext.GetCommonMergeFields();
                        mergeFields.Add( "Family", family );
                        mergeFields.Add( "RelatedChildren", relatedChildren );
                        mergeFields.Add( "Schedule", schedule );
                        foreach ( var keyval in parameters )
                        {
                            mergeFields.Add( keyval.Key, keyval.Value );
                        }

                        // Using ResolveRockUrl to strip any leading "~" character from the user-entered URL lava template.
                        redirectUrl = this.RequestContext.ResolveRockUrl( redirectUrlLavaTemplate.ResolveMergeFields( mergeFields ) );
                    }
                }

                return ActionOk( new FamilyPreRegistrationSaveResponseBag
                {
                    RedirectUrl = redirectUrl
                } );
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates a new family group (without saving).
        /// </summary>
        /// <param name="familyGroupTypeId">The family group type identifier.</param>
        /// <param name="lastName">The family name.</param>
        /// <param name="campusId">The family's campus.</param>
        /// <returns>The new family.</returns>
        private Rock.Model.Group CreateNewFamily( int familyGroupTypeId, string lastName, int? campusId )
        {
            var family = new Rock.Model.Group
            {
                Name = lastName.FixCase() + " Family",
                GroupTypeId = familyGroupTypeId,
                CampusId = campusId
            };

            return family;
        }

        /// <summary>
        /// Creates the user.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="confirmed">if set to <c>true</c> [confirmed].</param>
        /// <returns></returns>
        private Rock.Model.UserLogin CreateUser( Person person, string username, string password )
        {
            var rockContext = new RockContext();
            var user = UserLoginService.Create(
                rockContext,
                person,
                Rock.Model.AuthenticationServiceType.Internal,
                EntityTypeCache.Get( Rock.SystemGuid.EntityType.AUTHENTICATION_DATABASE.AsGuid() ).Id,
                username,
                password,
                false );

            var mergeObjects = this.RequestContext.GetCommonMergeFields();
            mergeObjects.Add( "ConfirmAccountUrl", $"{ this.RequestContext.RootUrlPath }/ConfirmAccount" );
            mergeObjects.Add( "Person", person );
            mergeObjects.Add( "User", user );

            var emailMessage = new RockEmailMessage( Rock.SystemGuid.SystemCommunication.SECURITY_CONFIRM_ACCOUNT.AsGuid() );
            emailMessage.AddRecipient( new RockEmailMessageRecipient( person, mergeObjects ) );
            emailMessage.AppRoot = "/";
            emailMessage.ThemeRoot = this.RequestContext.ResolveRockUrl( "~~/" );
            emailMessage.Send();

            return user;
        }

        /// <summary>
        /// Ensures the person in other family.
        /// </summary>
        /// <param name="familyGroupTypeId">The family group type identifier.</param>
        /// <param name="primaryfamilyId">The primaryfamily identifier.</param>
        /// <param name="personId">The person identifier.</param>
        /// <param name="lastName">The last name.</param>
        /// <param name="childRoleId">The child role identifier.</param>
        /// <param name="newFamilyIds">The new family ids.</param>
        private void EnsurePersonInOtherFamily( RockContext rockContext, int familyGroupTypeId, int primaryfamilyId, int personId, string lastName, int childRoleId, Dictionary<string, int> newFamilyIds, int? campusId )
        {
            var groupMemberService = new GroupMemberService( rockContext );

            // Get any other family memberships.
            if ( groupMemberService.Queryable()
                .Any( m =>
                    m.Group.GroupTypeId == familyGroupTypeId &&
                    m.PersonId == personId &&
                    m.GroupId != primaryfamilyId ) )
            {
                // They have other families, so just return
                return;
            }

            // Check to see if we've already created a family with someone who has same last name
            var key = lastName.ToLower();
            var newFamilyId = newFamilyIds.ContainsKey( key ) ? newFamilyIds[key] : ( int? ) null;

            // If not, create a new family.
            if ( !newFamilyId.HasValue )
            {
                var family = CreateNewFamily( familyGroupTypeId, lastName, campusId );
                new GroupService( rockContext ).Add( family );

                // Save the family.
                rockContext.SaveChanges();

                newFamilyId = family.Id;
                newFamilyIds.Add( key, family.Id );
            }

            // Add the person to the family
            var familyMember = new GroupMember
            {
                GroupId = newFamilyId.Value,
                PersonId = personId,
                GroupRoleId = childRoleId,
                GroupMemberStatus = GroupMemberStatus.Active
            };

            groupMemberService.Add( familyMember );

            // Save the child as a family member.
            rockContext.SaveChanges();
        }

        /// <summary>
        /// Returns true if the save request bag is valid.
        /// </summary>
        /// <param name="bag">The bag.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns>
        ///   <c>true</c> if the specified bag is valid; otherwise, <c>false</c>.
        /// </returns>
        private bool IsValid( FamilyPreRegistrationSaveRequestBag bag, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            if ( bag.FullName.IsNotNullOrWhiteSpace() )
            {
                /* 03/22/2021 MDP

                see https://app.asana.com/0/1121505495628584/1200018171012738/f on why this is done

                */

                errorMessages.Add( "Invalid Form Value" );
                return false;
            }

            // Client-side validation should have caught the following errors, but we should run them on the backend too.

            // Validate visit information.

            var visitDateField = GetVisitDateFieldBag( out var errorMessage );

            if ( errorMessage.IsNotNullOrWhiteSpace() )
            {
                errorMessages.Add( errorMessage );
            }

            if ( visitDateField.IsRequired )
            {
                if ( visitDateField.IsDateShown && bag.PlannedVisitDate.HasValue == false )
                {
                    errorMessages.Add( "Planned Visit Date is required" );
                }
                else if ( visitDateField.IsDateAndTimeShown && ( bag.PlannedVisitDate.HasValue == false || bag.ScheduleGuid.HasValue == false ) )
                {
                    errorMessages.Add( "Schedule Date and Time are required" );
                }
            }

            // Validate adult information.

            if ( GetFieldBag( AttributeKey.AdultAddress ).IsRequired && bag.Address == null )
            {
                errorMessages.Add( "Address is required" );
            }

            var createAccountField = GetFieldBag( AttributeKey.FirstAdultCreateAccount );

            if ( createAccountField.IsShown && bag.CreateAccount != null )
            {
                var isUsernameEntered = bag.CreateAccount.Username.IsNotNullOrWhiteSpace();
                var isPasswordEntered = bag.CreateAccount.Password.IsNotNullOrWhiteSpace();

                var isRequiredAndEitherUsernameOrPasswordMissing = createAccountField.IsRequired && ( !isUsernameEntered || !isPasswordEntered );
                var isOptionalAndOnlyOneValueEntered = createAccountField.IsOptional && ( ( isUsernameEntered && !isPasswordEntered ) || ( !isUsernameEntered && isPasswordEntered ) );

                if ( isRequiredAndEitherUsernameOrPasswordMissing || isOptionalAndOnlyOneValueEntered )
                {
                    errorMessages.Add( "Username and Password are required" );
                }
            }

            if ( GetFieldBag( AttributeKey.AdultBirthdate ).IsRequired && IsPropertyInvalidForEitherAdult( bag, adult => adult.BirthDate.ToDateTime().HasValue == false ) )
            {
                errorMessages.Add( "Birth Date is required for each adult" );
            }

            if ( GetFieldBag( AttributeKey.AdultDisplayCommunicationPreference ).IsRequired && IsPropertyInvalidForEitherAdult( bag, adult => adult.CommunicationPreference == CommunicationPreference.None) )
            {
                errorMessages.Add( "Communication Preference is required for each adult" );
            }

            if ( GetFieldBag( AttributeKey.AdultEmail ).IsRequired && IsPropertyInvalidForEitherAdult( bag, adult => adult.Email.IsNullOrWhiteSpace() ) )
            {
                errorMessages.Add( "Email is required for each adult" );
            }

            if ( GetFieldBag( AttributeKey.EthnicityOption ).IsRequired && IsPropertyInvalidForEitherAdult( bag, adult => adult.EthnicityDefinedValueGuid.HasValue == false ) )
            {
                errorMessages.Add( "Ethnicity is required for each adult" );
            }

            if ( GetFieldBag( AttributeKey.AdultGender ).IsRequired && IsPropertyInvalidForEitherAdult( bag, adult => adult.Gender == Gender.Unknown ) )
            {
                errorMessages.Add( "Gender is required for each adult" );
            }

            if ( GetFieldBag( AttributeKey.AdultMaritalStatus ).IsRequired && IsPropertyInvalidForEitherAdult( bag, adult => adult.MaritalStatusDefinedValueGuid.HasValue == false ) )
            {
                errorMessages.Add( "Marital Status is required for each adult" );
            }

            if ( GetFieldBag( AttributeKey.AdultMobilePhone ).IsRequired && IsPropertyInvalidForEitherAdult( bag, adult => adult.MobilePhone.IsNullOrWhiteSpace() ) )
            {
                errorMessages.Add( "Mobile Phone is required for each adult" );
            }

            if ( GetFieldBag( AttributeKey.AdultProfilePhoto ).IsRequired && IsPropertyInvalidForEitherAdult( bag, adult => adult.ProfilePhotoGuid.HasValue == false ) )
            {
                errorMessages.Add( "Profile Photo is required for each adult" );
            }

            if ( GetFieldBag( AttributeKey.RaceOption).IsRequired && IsPropertyInvalidForEitherAdult( bag, adult => adult.RaceDefinedValueGuid.HasValue == false ) )
            {
                errorMessages.Add( "Race is required for each adult" );
            }

            if ( GetFieldBag( AttributeKey.AdultSuffix ).IsRequired && IsPropertyInvalidForEitherAdult( bag, adult => adult.SuffixDefinedValueGuid.HasValue == false ) )
            {
                errorMessages.Add( "Suffix is required for each adult" );
            }

            // Validate children.

            if ( GetFieldBag( AttributeKey.ChildBirthdate ).IsRequired && IsPropertyInvalidForAnyChildren( bag, child => child.BirthDate.ToDateTime().HasValue == false ) )
            {
                errorMessages.Add( "Birth Date is required for all children" );
            }

            if ( GetFieldBag( AttributeKey.ChildDisplayCommunicationPreference ).IsRequired && IsPropertyInvalidForAnyChildren( bag, child => child.CommunicationPreference == CommunicationPreference.None ) )
            {
                errorMessages.Add( "Communication Preference is required for all children" );
            }

            if ( GetFieldBag( AttributeKey.ChildEmail ).IsRequired && IsPropertyInvalidForAnyChildren( bag, child => child.Email.IsNullOrWhiteSpace() ) )
            {
                errorMessages.Add( "Email is required for all children" );
            }

            if ( GetFieldBag( AttributeKey.ChildEthnicityOption ).IsRequired && IsPropertyInvalidForAnyChildren( bag, child => child.EthnicityDefinedValueGuid.HasValue == false ) )
            {
                errorMessages.Add( "Ethnicity is required for all children" );
            }

            if ( GetFieldBag( AttributeKey.ChildGender ).IsRequired && IsPropertyInvalidForAnyChildren( bag, child => child.Gender == Gender.Unknown ) )
            {
                errorMessages.Add( "Gender is required for all children" );
            }

            if ( GetFieldBag( AttributeKey.ChildGrade ).IsRequired && IsPropertyInvalidForAnyChildren( bag, child => child.GradeDefinedValueGuid.HasValue == false ) )
            {
                errorMessages.Add( "Grade is required for all children" );
            }

            if ( GetFieldBag( AttributeKey.ChildMobilePhone ).IsRequired && IsPropertyInvalidForAnyChildren( bag, child => child.MobilePhone.IsNullOrWhiteSpace() ) )
            {
                errorMessages.Add( "Mobile Phone is required for all children" );
            }

            if ( GetFieldBag( AttributeKey.ChildProfilePhoto ).IsRequired && IsPropertyInvalidForAnyChildren( bag, child => child.ProfilePhotoGuid.HasValue == false ) )
            {
                errorMessages.Add( "Profile Photo is required for all children" );
            }

            if ( GetFieldBag( AttributeKey.ChildRaceOption ).IsRequired && IsPropertyInvalidForAnyChildren( bag, child => child.RaceDefinedValueGuid.HasValue == false ) )
            {
                errorMessages.Add( "Race is rquired for all children" );
            }

            if ( GetFieldBag( AttributeKey.ChildSuffix ).IsRequired && IsPropertyInvalidForAnyChildren( bag, child => child.SuffixDefinedValueGuid.HasValue == false ) )
            {
                errorMessages.Add( "Suffix is required for all children" );
            }

            // Next, perform custom validation.

            if ( IsPropertyInvalidForEitherAdult( bag, adult => adult.CommunicationPreference == CommunicationPreference.SMS && adult.MobilePhone.IsNullOrWhiteSpace() ) )
            {
                errorMessages.Add( "SMS Number is required if SMS communication preference is selected" );
            }
            if ( createAccountField.IsShown && bag.CreateAccount?.Username.IsNotNullOrWhiteSpace() == true && bag.CreateAccount.Password.IsNotNullOrWhiteSpace() )
            {
                var regexString = GlobalAttributesCache.Get().GetValue( "core.ValidUsernameRegularExpression" );
                var match = System.Text.RegularExpressions.Regex.Match( bag.CreateAccount.Username, regexString );

                if ( !match.Success )
                {
                    errorMessages.Add( $"Username is not valid. {GlobalAttributesCache.Get().GetValue( "core.ValidUsernameCaption" )}" );
                }

                if ( UserLoginService.IsPasswordValid( bag.CreateAccount.Password ) )
                {
                    var userLoginService = new UserLoginService( new RockContext() );
                    var userLogin = userLoginService.GetByUserName( bag.CreateAccount.Username );

                    if ( userLogin != null )
                    {
                        errorMessages.Add( "The username you selected is already in use" );
                    }
                }
                else
                {
                    errorMessages.Add( UserLoginService.FriendlyPasswordRules() );
                }
            }

            if ( errorMessages.Any() )
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether a save request is invalid by testing each child.
        /// </summary>
        /// <param name="bag">The save request bag.</param>
        /// <param name="isInvalidFunc">The func that determines if a child request is invalid.</param>
        /// <returns>
        ///   <c>true</c> if any child request is invalid; otherwise, <c>false</c>.
        /// </returns>
        private bool IsPropertyInvalidForAnyChildren( FamilyPreRegistrationSaveRequestBag bag, Func<FamilyPreRegistrationPersonBag, bool> isInvalidFunc )
        {
            if ( bag.Children?.Any() != true )
            {
                return false;
            }

            foreach ( var child in bag.Children.Where( c => c != null ) )
            {
                if ( isInvalidFunc( child ) )
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether a save request is invalid by testing each adult.
        /// </summary>
        /// <param name="bag">The save request bag.</param>
        /// <param name="isInvalidFunc">The funct that determines if an adult request is invalid.</param>
        /// <returns>
        ///   <c>true</c> if any adult request is invalid; otherwise, <c>false</c>.
        /// </returns>
        private bool IsPropertyInvalidForEitherAdult( FamilyPreRegistrationSaveRequestBag bag, Func<FamilyPreRegistrationPersonBag, bool> isInvalidFunc )
        {
            return ( bag.Adult1 != null && isInvalidFunc( bag.Adult1 ) ) || ( bag.Adult2 != null && isInvalidFunc( bag.Adult2 ) );
        }

        /// <summary>
        /// Gets the attributes for the specified attribute categories.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="attributeCategoryGuids">The attribute category guids.</param>
        /// <returns>The attributes for the specified attribute categories.</returns>
        private List<AttributeCache> GetAttributeCategoryAttributes( RockContext rockContext, List<Guid> attributeCategoryGuids )
        {
            var attributeService = new AttributeService( rockContext );
            var attributes = new List<AttributeCache>();

            foreach ( var categoryGuid in attributeCategoryGuids )
            {
                var category = CategoryCache.Get( categoryGuid );

                if ( category != null )
                {
                    foreach ( var attribute in attributeService.GetByCategoryId( category.Id, false ) )
                    {
                        if ( !attributes.Any( a => a.Guid == attribute.Guid ) )
                        {
                            attributes.Add( AttributeCache.Get( attribute ) );
                        }
                    }
                }
            }

            return attributes;
        }

        /// <summary>
        /// Gets the family attributes.
        /// </summary>
        private List<AttributeCache> GetFamilyAttributes( Person currentPerson )
        {
            var attributes = new List<AttributeCache>();

            foreach ( var attributeGuid in this.FamilyAttributeGuids )
            {
                var attribute = AttributeCache.Get( attributeGuid );

                if ( attribute != null && attribute.IsAuthorized( Authorization.EDIT, currentPerson ) )
                {
                    attributes.Add( attribute );
                }
            }

            return attributes;
        }

        /// <summary>
        /// Gets the initialization box.
        /// </summary>
        /// <returns>The initialization box.</returns>
        private FamilyPreRegistrationInitializationBox GetInitializationBox()
        {
            var box = new FamilyPreRegistrationInitializationBox
            {
                CampusSchedulesAttributeGuid = this.CampusSchedulesAttributeGuid,
                CampusStatusesFilter = this.CampusStatusesFilter,
                CampusTypesFilter = this.CampusTypesFilter,
                CreateAccountDescription = this.CreateAccountDescription,
                CreateAccountTitle = this.CreateAccountTitle,
                VisitInfoTitle = this.GetAttributeValue( AttributeKey.PlannedVisitInformationPanelTitle ),
                CampusField = GetFieldBag( this.IsCampusOptional, this.IsCampusHidden ),
                VisitDateField = GetVisitDateFieldBag( out var errorMessage ),
                ErrorMessage = errorMessage,
                DisplaySmsOptIn = GetSmsOptInFieldBag(),
                AdultMobilePhoneField = GetFieldBag( AttributeKey.AdultMobilePhone ),
                AdultProfilePhotoField = GetFieldBag( AttributeKey.AdultProfilePhoto ),
                // LPC CODE
                AdultProfilePhotoFieldMode = GetAttributeValue( AttributeKey.AdultPhotoMode ).AsBoolean(),
                // END LPC CODE
                CreateAccountField = GetFieldBag( AttributeKey.FirstAdultCreateAccount ),
                AddressField = GetFieldBag( AttributeKey.AdultAddress ),
                AdultGenderField = GetFieldBag( AttributeKey.AdultGender ),
                AdultSuffixField = GetFieldBag( AttributeKey.AdultSuffix ),
                AdultBirthdayField = GetDatePickerFieldBag( AttributeKey.AdultBirthdate ),
                AdultEmailField = GetFieldBag( AttributeKey.AdultEmail ),
                AdultMaritalStatusField = GetFieldBag( AttributeKey.AdultMaritalStatus ),
                AdultCommunicationPreferenceField = GetFieldBag( AttributeKey.AdultDisplayCommunicationPreference ),
                AdultRaceField = GetFieldBag( AttributeKey.RaceOption ),
                AdultEthnicityField = GetFieldBag( AttributeKey.EthnicityOption ),
                ChildSuffixField = GetFieldBag( AttributeKey.ChildSuffix ),
                ChildGenderField = GetFieldBag( AttributeKey.ChildGender ),
                ChildBirthDateField = GetFieldBag( AttributeKey.ChildBirthdate ),
                ChildGradeField = GetFieldBag( AttributeKey.ChildGrade ),
                // LPC CODE
                ChildSelfReleaseField = GetFieldBag( AttributeKey.SelfRelease ),
                // END LPC CODE
                ChildMobilePhoneField = GetFieldBag( AttributeKey.ChildMobilePhone ),
                ChildEmailField = GetFieldBag( AttributeKey.ChildEmail ),
                ChildCommunicationPreferenceField = GetFieldBag( AttributeKey.ChildDisplayCommunicationPreference ),
                ChildProfilePhotoField = GetFieldBag( AttributeKey.ChildProfilePhoto ),
                // LPC CODE
                ChildProfilePhotoFieldMode = GetAttributeValue( AttributeKey.ChildPhotoMode ).AsBoolean(),
                // END LPC CODE
                ChildRaceField = GetFieldBag( AttributeKey.ChildRaceOption ),
                ChildEthnicityField = GetFieldBag( AttributeKey.ChildEthnicityOption )
            };

            using ( var rockContext = new RockContext() )
            {
                var currentPerson = this.GetCurrentPerson();

                box.CampusGuid = GetInitialCampusGuid( rockContext, currentPerson, box.CampusTypesFilter, box.CampusStatusesFilter );

                var childRelationshipTypes = GetChildRelationshipTypes();

                box.ChildRelationshipTypes = childRelationshipTypes.Values.ToList();

                var (adult1, adult2, children, family) = GetCurrentOrNewFamily( rockContext, currentPerson, childRelationshipTypes );

                var adultAttributes = GetAttributeCategoryAttributes( rockContext, this.AdultAttributeCategoryGuids );

                box.Adult1 = GetFamilyPreRegistrationPersonBag( adult1, currentPerson, adultAttributes );
                box.Adult2 = GetFamilyPreRegistrationPersonBag( adult2, currentPerson, adultAttributes );

                box.FamilyGuid = family.Guid;
                var familyAttributes = GetFamilyAttributes( currentPerson );
                box.FamilyAttributes = family.GetPublicAttributesForEdit( currentPerson, attributeFilter: f => familyAttributes.Any( a => a.Guid == f.Guid ) );
                box.FamilyAttributeValues = family.GetPublicAttributeValuesForEdit( currentPerson, attributeFilter: f => familyAttributes.Any( a => a.Guid == f.Guid ) );

                var mockChild = new Person
                {
                    Guid = Guid.NewGuid(),
                    Gender = Gender.Unknown,
                    GradeOffset = null
                };
                mockChild.LoadAttributes( rockContext );
                var childAttributes = GetAttributeCategoryAttributes( rockContext, this.ChildAttributeCategoryGuids );
                box.ChildAttributes = mockChild.GetPublicAttributesForEdit( currentPerson, attributeFilter: f => childAttributes.Any( a => a.Guid == f.Guid ) );
                box.Children = children.Select( child => GetFamilyPreRegistrationPersonBag( child.Person, child.FamilyRoleGuid, currentPerson, childAttributes ) ).ToList();

                // Only load the home address if the Address field is shown.
                if ( this.IsFamilyUpdateAllowedForCurrentPerson && currentPerson != null && family != null )
                {
                    // Set the campus from the family.
                    box.CampusGuid = family.CampusId.HasValue ? CampusCache.GetGuid( family.CampusId.Value ) : null;

                    // Set the address from the family.
                    var homeLocationTypeId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() );
                    if ( homeLocationTypeId.HasValue )
                    {
                        var location = family.GroupLocations
                            .Where( l =>
                                l.GroupLocationTypeValueId.HasValue &&
                                l.GroupLocationTypeValueId.Value == homeLocationTypeId )
                            .Select( l => l.Location )
                            .FirstOrDefault();
                        box.Address = new AddressControlBag
                        {
                            City = location?.City,
                            Country = location?.Country,
                            State = location?.State,
                            Locality = location?.County,
                            PostalCode = location?.PostalCode,
                            Street1 = location?.Street1,
                            Street2 = location?.Street2,
                        };
                        location.ToViewModel( currentPerson );
                    }
                }
            }

            return box;
        }

        private Guid? GetInitialCampusGuid( RockContext rockContext, Person currentPerson, List<Guid> campusTypesFilter, List<Guid> campusStatusesFilter )
        {
            if ( this.IsCampusHidden )
            {
                // No initial campus if the block setting hides the field.
                return null;
            }

            // LPC CODE
            // If there is a CampusGuid or CampusId parameter, use that as the default campus
            var campusGuid = PageParameter( PageParameterKey.CampusGuid ).AsGuidOrNull();
            var campusId = PageParameter( PageParameterKey.CampusId ).AsIntegerOrNull();
            CampusCache paramCampus = null;

            // Make sure the campus exists
            if ( campusGuid != null )
            {
                paramCampus = CampusCache.Get( ( Guid )campusGuid );
            }
            if ( campusId != null && paramCampus == null )
            {
                paramCampus = CampusCache.Get( ( int )campusId );
            }

            // If a parameter contains a real campus return it
            if ( paramCampus != null && paramCampus.Id > 0 )
            {
                return paramCampus.Guid;
            }
            // END LPC CODE

            var client = new CampusClientService( rockContext, currentPerson );
            var campuses = client.GetCampusesAsListItems( new CampusOptions
            {
                LimitCampusStatuses = campusStatusesFilter,
                LimitCampusTypes = campusTypesFilter,
                IncludeInactive = false                
            } );

            if ( campuses.Any() )
            {
                Guid? defaultCampusGuid = this.DefaultCampusGuid;

                if ( defaultCampusGuid == Guid.Empty )
                {
                    defaultCampusGuid = null;
                }

                if ( campuses.Count == 1 )
                {
                    // If there is only one filtered campus, then return the default campus OR the single campus if default is missing.
                    return defaultCampusGuid ?? campuses.First().Value.AsGuidOrNull();
                }
                else
                {
                    // If there is more than one filtered campus, then return the default campus.
                    return defaultCampusGuid;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the child relationship types.
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, ListItemBag> GetChildRelationshipTypes()
        {
            var childRelationshipTypeIds = this.GetAttributeValues( AttributeKey.Relationships ).AsIntegerList();

            Dictionary<int, ListItemBag> relationshipTypes = null;

            if ( childRelationshipTypeIds.Any( id => id != 0 ) )
            {
                var knownRelationshipGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS.AsGuid() );
                if ( knownRelationshipGroupType != null )
                {
                    relationshipTypes = knownRelationshipGroupType
                        .Roles
                        .Where( r => childRelationshipTypeIds.Contains( r.Id ) )
                        .Select( r => new
                        {
                            r.Id,
                            r.Name,
                            r.Guid
                        } )
                        .ToDictionary( r => r.Id, r => new ListItemBag
                        {
                            Text = r.Name,
                            Value = r.Guid.ToString()
                        } );
                }
            }
            else
            {
                relationshipTypes = new Dictionary<int, ListItemBag>();
            } 

            if ( childRelationshipTypeIds.Contains( 0 ) )
            {
                relationshipTypes.Add( 0, new ListItemBag
                {
                    Text = "Child",
                    Value = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD
                } );
            }

            return relationshipTypes;
        }

        /// <summary>
        /// Gets the current family or a new family based on the block settings.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <returns>The current family or a new family.</returns>
        private ( Person adult1, Person adult2, List<( Person Person, Guid? FamilyRoleGuid)> children, Rock.Model.Group family ) GetCurrentOrNewFamily( RockContext rockContext, Person currentPerson, Dictionary<int, ListItemBag> childRelationshipTypes )
        {
            Person adult1 = null;
            Person adult2 = null;
            Rock.Model.Group family = null;
            // Guid is the child's role in the family.
            List<(Person Person, Guid? RoleId)> children = new List<( Person, Guid? )>();

            // If there is a logged in person, attempt to find their family and spouse.
            if ( this.IsFamilyUpdateAllowedForCurrentPerson && currentPerson != null )
            {
                Person spouse = null;

                // Get all their families.
                var families = currentPerson.GetFamilies( rockContext );

                if ( families.Any() )
                {
                    // Get their spouse.
                    spouse = currentPerson.GetSpouse( rockContext );

                    if ( spouse != null )
                    {
                        // If spouse was found, find the first family that spouse belongs to also.
                        family = families.Where( f => f.Members.Any( m => m.PersonId == spouse.Id ) ).FirstOrDefault();

                        if ( family == null )
                        {
                            // If there was not family with spouse, something went wrong and assume there is no spouse.
                            spouse = null;
                        }
                    }

                    // If we didn't find a family yet (by checking spouses family), assume the first family.
                    if ( family == null )
                    {
                        family = families.FirstOrDefault();
                    }

                    // Assume Adult1 is the current person...
                    adult1 = currentPerson;

                    if ( spouse != null )
                    {
                        // ...and Adult2 is the spouse.
                        adult2 = spouse;

                        // However, if spouse is actually head of family, make them Adult1 and current person Adult2
                        var headOfFamilyId = family.Members
                            .OrderBy( m => m.GroupRole.Order )
                            .ThenBy( m => m.Person.Gender )
                            .Select( m => m.PersonId )
                            .FirstOrDefault();

                        if ( headOfFamilyId != 0 && headOfFamilyId == spouse.Id )
                        {
                            adult1 = spouse;
                            adult2 = currentPerson;
                        }
                    }

                    if ( family != null )
                    {
                        // Find all the children in the family
                        var childRoleGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid();

                        foreach ( var groupMember in family.Members
                            .Where( m => m.GroupRole.Guid == childRoleGuid )
                            .OrderByDescending( m => m.Person.Age ) )
                        {
                            children.Add( ( groupMember.Person, childRoleGuid ) );
                        }

                        // Find all the related people.
                        var adultIds = new List<int>();

                        if ( adult1 != null )
                        {
                            adultIds.Add( adult1.Id );
                        }

                        if ( adult2 != null )
                        {
                            adultIds.Add( adult2.Id );
                        }

                        var roleIds = childRelationshipTypes.Keys.ToList();

                        foreach ( var groupMember in new PersonService( rockContext )
                            .GetRelatedPeople( adultIds, roleIds )
                            .OrderBy( m => m.GroupRole.Order )
                            .ThenByDescending( m => m.Person.Age ) )
                        {
                            if ( !children.Any( c => c.Person.Id == groupMember.PersonId ) )
                            {
                                children.Add( ( groupMember.Person, groupMember.GroupRole?.Guid ) );
                            }
                        }
                    }
                }

            }
            else
            {
                adult1 = new Person();
                adult2 = new Person();
                family = new Rock.Model.Group
                {
                    // Setting group type id so family-specific attributes are loaded.
                    GroupTypeId = GroupTypeCache.GetId( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() ) ?? 0
                };
            }

            // Load attributes for all.
            adult1.LoadAttributes( rockContext );
            adult2.LoadAttributes( rockContext );
            family.LoadAttributes( rockContext );

            foreach ( var child in children.Select( c => c.Person ) )
            {
                child.LoadAttributes( rockContext );
            }

            return ( adult1, adult2, children, family );
        }

        /// <summary>
        /// Chooses the planned date panel to show. Either pnlPlannedDate which only shows a date, or pnlPlannedSchedule which provides a list of date and times for a campus' schedule.
        /// </summary>
        private FamilyPreRegistrationDateAndTimeFieldBag GetVisitDateFieldBag( out string errorMessage )
        {
            var visitDateField = GetFieldBag( AttributeKey.PlannedVisitDate );

            errorMessage = null;

            var campusSchedulesAttributeGuid = this.CampusSchedulesAttributeGuid;

            if ( !campusSchedulesAttributeGuid.HasValue )
            {
                // There is no campus schedules attribute from which to select schedules, so show the visit date field by itself.
                return CreateVisitDateFieldBagFromField( visitDateField, bag =>
                {
                    bag.IsDateShown = visitDateField.IsShown;
                    bag.IsDateAndTimeShown = false;
                } );
            }

            // Make sure the campus schedules attribute uses the Schedules field type and display the date panel if not.
            var campusSchedulesAttribute = AttributeCache.Get( campusSchedulesAttributeGuid.Value );
            if ( campusSchedulesAttribute?.FieldType == null || campusSchedulesAttribute.FieldType.Guid != Rock.SystemGuid.FieldType.SCHEDULES.AsGuidOrNull() )
            {
                // If the user has edit permission then display an error message so the configuration can be fixed
                if ( this.BlockCache.IsAuthorized( Authorization.EDIT, this.GetCurrentPerson() ) )
                {
                    errorMessage = "The campus attribute for schedules is not using the field type of 'Schedules' or a value was not specified. Please adjust this.";
                }

                // Since the campusScheduleAttribute is not correct just display the date panel.
                return CreateVisitDateFieldBagFromField( visitDateField, bag =>
                {
                    bag.IsDateShown = visitDateField.IsShown;
                    bag.IsDateAndTimeShown = false;
                } );
            }

            // If there are multiple campuses and the campus picker is not visible then just display the date panel.
            if ( this.IsCampusHidden && CampusCache.All( false ).Count > 1 )
            {
                // If the user has edit permission then display an error message so the configuration can be fixed.
                if ( this.BlockCache.IsAuthorized( Authorization.EDIT, this.GetCurrentPerson() ) )
                {
                    errorMessage = "In order to show campus schedules the campus has to be shown so it can be selected. Change this block's 'Show Campus' attribute to 'Yes'.";
                }

                // Since the campus is not available for the campusScheduleAttribute just display the date panel.
                return CreateVisitDateFieldBagFromField( visitDateField, bag =>
                {
                    bag.IsDateShown = visitDateField.IsShown;
                    bag.IsDateAndTimeShown = false;
                } );
            }

            // Display the schedule panel if there are multiple campuses and the campus picker is shown or if there is a single campus.
            return CreateVisitDateFieldBagFromField( visitDateField, bag =>
            {
                bag.IsDateShown = false;
                bag.IsDateAndTimeShown = visitDateField.IsShown;
            } );
        }

        /// <summary>
        /// Creates the visit date field bag from another field bag.
        /// </summary>
        private FamilyPreRegistrationDateAndTimeFieldBag CreateVisitDateFieldBagFromField( FamilyPreRegistrationFieldBag source, Action<FamilyPreRegistrationDateAndTimeFieldBag> updateAction = null )
        {
            // By default the visit date field should only show the date and be optional.
            var bag = new FamilyPreRegistrationDateAndTimeFieldBag
            {
                IsDateShown = true,
                IsDateAndTimeShown = false,
                IsHidden = source?.IsHidden ?? false,
                IsShown = source?.IsShown ?? true,
                IsOptional = source?.IsOptional ?? true,
                IsRequired = source?.IsRequired ?? false,
            };

            updateAction?.Invoke( bag );

            return bag;
        }

        /// <summary>
        /// Get the display settings for the SMS Opt-In checkbox, will show it as hidden if the mobile number is hidden.
        /// </summary>
        private FamilyPreRegistrationSmsOptInFieldBag GetSmsOptInFieldBag()
        {
            // Don't display the SMS Opt-In if the phone number is set to "Hide"
            var displayMobilePhone = this.GetAttributeValue( AttributeKey.AdultMobilePhone );
            if ( string.Equals( displayMobilePhone, "Hide", StringComparison.OrdinalIgnoreCase ) )
            {
                return new FamilyPreRegistrationSmsOptInFieldBag
                {
                    IsHidden = true,
                    IsShowFirstAdult = false,
                    IsShowAllAdults = false,
                    IsShowChildren = false
                };
            }

            var displaySmsAttributeValue = this.GetAttributeValue( AttributeKey.DisplaySmsOptIn );
            var smsOptInDisplayText = Rock.Web.SystemSettings.GetValue( Rock.SystemKey.SystemSetting.SMS_OPT_IN_MESSAGE_LABEL );

            //Options for displaying the SMS Opt-In checkbox: Hide,First Adult,All Adults,Adults and Children
            switch ( displaySmsAttributeValue )
            {
                case "Hide":
                return new FamilyPreRegistrationSmsOptInFieldBag
                {
                    IsHidden = true,
                    IsShowFirstAdult = false,
                    IsShowAllAdults = false,
                    IsShowChildren = false
                };
                case "First Adult":
                return new FamilyPreRegistrationSmsOptInFieldBag
                {
                    SmsOptInDisplayText = smsOptInDisplayText,
                    IsHidden = false,
                    IsShowFirstAdult = true,
                    IsShowAllAdults = false,
                    IsShowChildren = false
                };
                case "All Adults":
                return new FamilyPreRegistrationSmsOptInFieldBag
                {
                    SmsOptInDisplayText = smsOptInDisplayText,
                    IsHidden = false,
                    IsShowFirstAdult = true,
                    IsShowAllAdults = true,
                    IsShowChildren = false
                };
                case "Adults and Children":
                return new FamilyPreRegistrationSmsOptInFieldBag
                {
                    SmsOptInDisplayText = smsOptInDisplayText,
                    IsHidden = false,
                    IsShowFirstAdult = true,
                    IsShowAllAdults = true,
                    IsShowChildren = true
                };
                default:
                return new FamilyPreRegistrationSmsOptInFieldBag
                {
                    SmsOptInDisplayText = smsOptInDisplayText,
                    IsHidden = true,
                    IsShowFirstAdult = false,
                    IsShowAllAdults = false,
                    IsShowChildren = false
                };
            }
        }

        /// <summary>
        /// Gets the isOptional and isHidden field properties associated with a given attribute.
        /// </summary>
        /// <param name="attributeKey">The attribute key.</param>
        /// <param name="getFieldBagForAttributeValue">Used to return a field bag for non-standard attribute values.</param>
        /// <returns>Whether the field isOptional and isHidden.</returns>
        private T GetFieldBag<T>( string attributeKey, Func<string, T> getFieldBagForAttributeValue = null ) where T : FamilyPreRegistrationFieldBag, new()
        {
            var attributeValue = GetAttributeValue( attributeKey );

            if ( string.Equals( attributeValue, "Hide", StringComparison.OrdinalIgnoreCase ) )
            {
                // Hidden and optional.
                return new T
                {
                    IsHidden = true,
                    IsShown = false,
                    IsOptional = true,
                    IsRequired = false
                };
            }
            else if ( string.Equals( attributeValue, "Required", StringComparison.OrdinalIgnoreCase ) )
            {
                // Shown and required.
                return new T
                {
                    IsHidden = false,
                    IsShown = true,
                    IsOptional = false,
                    IsRequired = true
                };
            }
            else if ( string.Equals( attributeValue, "Optional", StringComparison.OrdinalIgnoreCase )
                      || string.Equals( attributeValue, "Show", StringComparison.OrdinalIgnoreCase ) )
            {
                // Shown and optional.
                return new T
                {
                    IsHidden = false,
                    IsShown = true,
                    IsOptional = true,
                    IsRequired = false
                };
            }
            else
            {
                // If none of the above checks pass,
                // then try to get the field bag from the supplied delegate.
                return getFieldBagForAttributeValue?.Invoke( attributeValue )
                    // Shown and optional by default.
                    ?? new T
                    {
                        IsHidden = false,
                        IsShown = true,
                        IsOptional = true,
                        IsRequired = false
                    };
            }
        }

        /// <summary>
        /// Gets the isOptional and isHidden field properties associated with a given attribute.
        /// </summary>
        /// <param name="attributeKey">The attribute key.</param>
        /// <returns>Whether the field isOptional and isHidden.</returns>
        private FamilyPreRegistrationFieldBag GetFieldBag( string attributeKey )
        {
            return GetFieldBag<FamilyPreRegistrationFieldBag>( attributeKey );
        }

        /// <summary>
        /// Gets the isOptional and isHidden field properties associated with a given attribute.
        /// </summary>
        /// <param name="attributeKey">The attribute key.</param>
        /// <returns>Whether the field isOptional and isHidden.</returns>
        private FamilyPreRegistrationDatePickerFieldBag GetDatePickerFieldBag( string attributeKey )
        {
            return GetFieldBag(
                attributeKey,
                attributeValue =>
                {
                    if ( string.Equals( attributeValue, "Required_Partial", StringComparison.OrdinalIgnoreCase ) )
                    {
                        return new FamilyPreRegistrationDatePickerFieldBag
                        {
                            IsMonthAndDayRequired = true,
                            IsRequired = false,
                            IsHidden = false,
                            IsShown = true,
                            IsOptional = false
                        };
                    }

                    // Return `null` here to let the parent method handle the default return value.
                    return null;
                } );

        }

        /// <summary>
        /// Gets the isOptional and isHidden field properties associated with a given attribute.
        /// </summary>
        /// <param name="attributeKey">The attribute key.</param>
        /// <returns>Whether the field isOptional and isHidden.</returns>
        private FamilyPreRegistrationFieldBag GetFieldBag( bool isOptional, bool isHidden )
        {
            return new FamilyPreRegistrationFieldBag
            {
                IsHidden = isHidden,
                IsShown = !isHidden,
                IsOptional = !isHidden && isOptional,
                IsRequired = !isHidden && !isOptional,
            };
        }

        /// <summary>
        /// Gets a family pre-registration person bag.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="personAttributes">The person attributes.</param>
        /// <returns>A family pre-registration person bag.</returns>
        private FamilyPreRegistrationPersonBag GetFamilyPreRegistrationPersonBag( Person person, Person currentPerson, List<AttributeCache> personAttributes )
        {
            return GetFamilyPreRegistrationPersonBag( person, null, currentPerson, personAttributes );
        }

        /// <summary>
        /// Gets a family pre-registration person bag.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="familyRoleGuid">The family role guid.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="personAttributes">The person attributes.</param>
        /// <returns>A family pre-registration person bag.</returns>
        private FamilyPreRegistrationPersonBag GetFamilyPreRegistrationPersonBag( Person person, Guid? familyRoleGuid, Person currentPerson, List<AttributeCache> personAttributes )
        {
            if ( person == null )
            {
                return new FamilyPreRegistrationPersonBag();
            }

            var bag = new FamilyPreRegistrationPersonBag
            {
                Attributes = person.GetPublicAttributesForEdit( currentPerson, attributeFilter: a1 => personAttributes.Any( a => a.Guid == a1.Guid ) ),
                AttributeValues = person.GetPublicAttributeValuesForEdit( currentPerson, attributeFilter: a1 => personAttributes.Any( a => a.Guid == a1.Guid ) ),
                BirthDate = person.BirthDate != null ?
                    new BirthdayPickerBag
                    {
                        Day = person.BirthDate.Value.Day,
                        Month = person.BirthDate.Value.Month,
                        Year = person.BirthDate.Value.Year
                    }
                    : null,
                CommunicationPreference = ( CommunicationPreference ) ( int ) person.CommunicationPreference,
                Email = person.Email,
                EthnicityDefinedValueGuid = person.EthnicityValue?.Guid ?? ( person.EthnicityValueId.HasValue ? DefinedValueCache.GetGuid( person.EthnicityValueId.Value ) : null ),
                FirstName = person.NickName,
                Guid = person.Guid,
                IsFirstNameReadOnly = person.Id != 0,
                IsLastNameReadOnly = person.Id != 0,
                Gender = person.Gender,
                GradeDefinedValueGuid = person.GradeOffset.HasValue ? GetGradeDefinedValueByGradeOffset( person.GradeOffset.Value )?.Guid : null,
                LastName = person.LastName,
                MaritalStatusDefinedValueGuid = person.MaritalStatusValue?.Guid ?? ( person.MaritalStatusValueId.HasValue ? DefinedValueCache.GetGuid( person.MaritalStatusValueId.Value ) : null ),
                ProfilePhotoGuid = person.Photo?.Guid,
                RaceDefinedValueGuid = person.RaceValue?.Guid ?? ( person.RaceValueId.HasValue ? DefinedValueCache.GetGuid( person.RaceValueId.Value ) : null ),
                FamilyRoleGuid = familyRoleGuid,
                SuffixDefinedValueGuid = person.SuffixValue?.Guid ?? ( person.SuffixValueId.HasValue ? DefinedValueCache.GetGuid( person.SuffixValueId.Value ) : null ),
                // LPC CODE
                Allergy = person.GetAttributeValue( "Allergy" ),
                IsSelfRelease = person.GetAttributeValue( "Arena-16-384" ).AsBoolean()
                // END LPC CODE
            };

            var mobilePhone = person.GetPhoneNumber( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );

            if ( mobilePhone != null )
            {
                bag.MobilePhone = mobilePhone.Number;
                bag.MobilePhoneCountryCode = mobilePhone.CountryCode;
                bag.IsMessagingEnabled = mobilePhone.IsMessagingEnabled;
            }

            return bag;
        }

        /// <summary>
        /// Gets the grade defined value by grade offset.
        /// </summary>
        /// <param name="gradeOffset">The grade offset.</param>
        /// <returns>The grade defined value.</returns>
        private DefinedValueCache GetGradeDefinedValueByGradeOffset( int gradeOffset )
        {
            if ( gradeOffset < 0 )
            {
                return null;
            }

            var gradeType = DefinedTypeCache.Get( SystemGuid.DefinedType.SCHOOL_GRADES.AsGuid() );

            if ( gradeType == null )
            {
                return null;
            }

            var gradeValues = gradeType.DefinedValues
                .Select( definedValue => new
                {
                    DefinedValue = definedValue,
                    Grade = definedValue.Value.AsInteger()
                } )
                .OrderBy( g => g.Grade )
                .ToList();

            return gradeValues
                
                .Where( g => g.Grade >= gradeOffset )
                .FirstOrDefault()?.DefinedValue;
        }

        /// <summary>
        /// Removes a person from a family.
        /// </summary>
        /// <param name="familyGroupTypeId">The family group type identifier.</param>
        /// <param name="familyId">The family identifier.</param>
        /// <param name="personId">The person identifier.</param>
        private void RemovePersonFromFamily( RockContext rockContext, int familyGroupTypeId, int familyId, int personId )
        {
            var groupMemberService = new GroupMemberService( rockContext );

            // Get all their current group memberships.
            var groupMembers = groupMemberService.Queryable()
                .Where( m =>
                    m.Group.GroupTypeId == familyGroupTypeId &&
                    m.PersonId == personId )
                .ToList();

            // Find their membership in current family, if not found, skip processing, as something is amiss.
            var currentFamilyMembership = groupMembers.FirstOrDefault( m => m.GroupId == familyId );

            if ( currentFamilyMembership != null )
            {
                // If the person does not currently belong to any other families, we'll have to create a new family for them, and move them to that new group.
                if ( !groupMembers.Where( m => m.GroupId != familyId ).Any() )
                {
                    var newGroup = new Rock.Model.Group
                    {
                        Name = currentFamilyMembership.Person.LastName + " Family",
                        GroupTypeId = familyGroupTypeId,
                        CampusId = currentFamilyMembership.Group.CampusId
                    };

                    new GroupService( rockContext ).Add( newGroup );

                    // Save the new family.
                    rockContext.SaveChanges();

                    // If person's previous giving group was this family, set it to their new family id.
                    if ( currentFamilyMembership.Person.GivingGroupId.HasValue && currentFamilyMembership.Person.GivingGroupId == currentFamilyMembership.GroupId )
                    {
                        currentFamilyMembership.Person.GivingGroupId = newGroup.Id;
                    }

                    currentFamilyMembership.Group = newGroup;

                    rockContext.SaveChanges();
                }
                else
                {
                    // Otherwise, just remove them from the current family.
                    groupMemberService.Delete( currentFamilyMembership );
                    rockContext.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Saves an adult.
        /// </summary>
        /// <param name="primaryFamily">The primary family.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="adults">The saved adults.</param>
        /// <param name="bag">The adult request bag.</param>
        private void SaveAdult( ref Rock.Model.Group primaryFamily, RockContext rockContext, List<Person> adults, FamilyPreRegistrationPersonBag bag, bool isFirstAdult )
        {
            var familyGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
            var personService = new PersonService( rockContext );

            // Get the adult if we're editing an existing person.
            var adult = personService.Get( bag.Guid );

            // Skip if this is a new person, and if a first or last name was not entered for this adult.
            if ( adult == null && ( bag.FirstName.IsNullOrWhiteSpace() || bag.LastName.IsNullOrWhiteSpace() ) )
            {
                return;
            }

            // Flag indicating if empty values should be saved to person record (Should not do this if a matched record was found).
            var saveEmptyValues = true;

            var isEmailShown = GetFieldBag( AttributeKey.AdultEmail ).IsShown;

            // If not editing an existing person, attempt to match them to existing (if configured to do so).
            if ( adult == null && isEmailShown && this.IsPersonMatchEnabled )
            {
                var personQuery = new PersonService.PersonMatchQuery(
                    bag.FirstName.Trim(),
                    bag.LastName.Trim(),
                    bag.Email?.Trim(),
                    bag.MobilePhone?.Trim(),
                    bag.Gender == Gender.Unknown ? (Gender?)null : bag.Gender,
                    bag.BirthDate.ToDateTime(),
                    DefinedValueCache.GetId( bag.SuffixDefinedValueGuid.GetValueOrDefault() ) );

                adult = personService.FindPerson( personQuery, updatePrimaryEmail: true );

                if ( adult != null )
                {
                    // Empty values should not be saved if a match was found.
                    saveEmptyValues = false;

                    if ( primaryFamily == null )
                    {
                        primaryFamily = adult.GetFamily( rockContext );
                    }
                }
            }

            // If this is a new person, add them.
            if ( adult == null )
            {
                var recordTypePersonId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() );
                var recordStatusValueId = DefinedValueCache.GetId( this.RecordStatusDefinedValueGuid ) ?? DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() );
                var connectionStatusValue = DefinedValueCache.Get( this.ConnectionStatusDefinedValueGuid ) ?? DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_VISITOR.AsGuid() );

                adult = new Person
                {
                    FirstName = bag.FirstName.FixCase(),
                    LastName = bag.LastName.FixCase(),
                    RecordTypeValueId = recordTypePersonId,
                    RecordStatusValueId = recordStatusValueId,
                    ConnectionStatusValueId = connectionStatusValue != null ? connectionStatusValue.Id : ( int? ) null
                };

                personService.Add( adult );
            }

            // Set adult values from request.

            if ( GetFieldBag( AttributeKey.AdultSuffix ).IsShown )
            {
                adult.SuffixValueId = bag.SuffixDefinedValueGuid.HasValue ? DefinedValueCache.GetId( bag.SuffixDefinedValueGuid.Value ) : null;
            }

            if ( GetFieldBag( AttributeKey.AdultGender ).IsShown )
            {
                if ( bag.Gender != Gender.Unknown || saveEmptyValues )
                {
                    adult.Gender = bag.Gender;
                }
            }

            var adultBirthdateOptions = GetDatePickerFieldBag( AttributeKey.AdultBirthdate );
            if ( adultBirthdateOptions.IsShown )
            {
                if ( ( adultBirthdateOptions.IsMonthAndDayRequired || adultBirthdateOptions.IsOptional )
                     && bag.BirthDate?.Year == 0 )
                {
                    // If the year is optional and the year is not set,
                    // then default it to the minimum year value.
                    bag.BirthDate.Year = DateTime.MinValue.Year;
                }

                var birthDate = bag.BirthDate.ToDateTime();

                if ( birthDate.HasValue || saveEmptyValues )
                {
                    adult.SetBirthDate( birthDate );
                }
            }

            if ( GetFieldBag( AttributeKey.AdultMaritalStatus ).IsShown )
            {
                if ( bag.MaritalStatusDefinedValueGuid.HasValue )
                {
                    adult.MaritalStatusValueId = DefinedValueCache.GetId( bag.MaritalStatusDefinedValueGuid.Value );
                } 
            }

            if ( isEmailShown )
            {
                if ( bag.Email.IsNotNullOrWhiteSpace() || saveEmptyValues )
                {
                    adult.Email = bag.Email;
                }
            }

            if ( GetFieldBag( AttributeKey.AdultDisplayCommunicationPreference ).IsShown )
            {
                adult.CommunicationPreference = ( CommunicationType ) ( int ) bag.CommunicationPreference;
            }

            if ( GetFieldBag( AttributeKey.AdultProfilePhoto ).IsShown )
            {
                if ( bag.ProfilePhotoGuid.HasValue )
                {
                    adult.PhotoId = new BinaryFileService( rockContext ).GetId( bag.ProfilePhotoGuid.Value );
                }
            }

            if ( GetFieldBag( AttributeKey.RaceOption ).IsShown )
            {
                adult.RaceValueId = bag.RaceDefinedValueGuid.HasValue ? DefinedValueCache.GetId( bag.RaceDefinedValueGuid.Value ) : null;
            }

            if ( GetFieldBag( AttributeKey.EthnicityOption ).IsShown )
            {
                adult.EthnicityValueId = bag.EthnicityDefinedValueGuid.HasValue ? DefinedValueCache.GetId( bag.EthnicityDefinedValueGuid.Value ) : null;
            }

            // Save the person.
            rockContext.SaveChanges();

            // Save the mobile phone number.
            if ( GetFieldBag( AttributeKey.AdultMobilePhone ).IsShown )
            {
                var isSmsNumber = false;
                var displaySmsOptInSetting = GetSmsOptInFieldBag();
                var showCommunicationPreference = !GetFieldBag( AttributeKey.AdultDisplayCommunicationPreference ).IsHidden;

                if ( displaySmsOptInSetting.IsHidden && showCommunicationPreference )
                {
                    // If the SMS Opt-In is not shown then use the communication preference to indicate if SMS should be enabled for the phone number.
                    isSmsNumber = adult.CommunicationPreference == CommunicationType.SMS;
                }
                else if ( isFirstAdult || displaySmsOptInSetting.IsShowAllAdults )
                {
                    // If the control is not hidden and this the first adult or the sms display is set to show for all adulsts then use use it to set the SMS number
                    isSmsNumber = bag.IsMessagingEnabled;
                }

                SavePhoneNumber( rockContext, adult.Id, bag.MobilePhoneCountryCode, bag.MobilePhone, isSmsNumber );
            }

            // Save any attribute values
            adult.LoadAttributes( rockContext );
            var adultAttributes = GetAttributeCategoryAttributes( rockContext, this.AdultAttributeCategoryGuids );
            adult.SetPublicAttributeValues(
                bag.AttributeValues,
                this.GetCurrentPerson(),
                // Do not enforce security; otherwise, some attribute values may not be set for unauthenticated users.
                enforceSecurity: false,
                attributeFilter: a1 => adultAttributes.Any( a => a.Guid == a1.Guid ) );

            // LPC CODE
            if ( bag.Allergy != null && bag.Allergy != "" )
            {
                adult.SetAttributeValue( "Allergy", bag.Allergy );
            }
            if ( bag.IsSelfRelease != null )
            {
                adult.SetAttributeValue( "Arena-16-384", bag.IsSelfRelease.ToString() );
            }
            // END LPC CODE

            adult.SaveAttributeValues( rockContext );

            adults.Add( adult );
        }

        /// <summary>
        /// Saves the phone number.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="number">The number.</param>
        /// <param name="countryCode">The country code.</param>
        private void SavePhoneNumber( RockContext rockContext, int personId, string countryCode, string number, bool isSmsNumber )
        {
            var mobilePhoneDefinedValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );

            if ( !mobilePhoneDefinedValueId.HasValue )
            {
                return;
            }

            var phoneNumberService = new PhoneNumberService( rockContext );
            var mobilePhoneNumber = phoneNumberService.Queryable()
                .Where( n =>
                    n.PersonId == personId &&
                    n.NumberTypeValueId.HasValue &&
                    n.NumberTypeValueId.Value == mobilePhoneDefinedValueId.Value )
                .FirstOrDefault();

            number = PhoneNumber.CleanNumber( number );

            if ( number.IsNotNullOrWhiteSpace() )
            {
                if ( mobilePhoneNumber == null )
                {
                    // Create the person's mobile phone number.

                    mobilePhoneNumber = new PhoneNumber
                    {
                        PersonId = personId,
                        NumberTypeValueId = mobilePhoneDefinedValueId.Value
                    };

                    phoneNumberService.Add( mobilePhoneNumber );
                }

                mobilePhoneNumber.CountryCode = PhoneNumber.CleanNumber( countryCode );
                mobilePhoneNumber.Number = number;
                mobilePhoneNumber.IsMessagingEnabled = isSmsNumber;
            }
            else
            {
                if ( mobilePhoneNumber != null )
                {
                    // Delete the person's mobile phone number.
                    phoneNumberService.Delete( mobilePhoneNumber );
                }
            }

            rockContext.SaveChanges();
        }

        #endregion

        #region Helper Classes and Enums

        private class OccurrenceSchedule
        {
            public DateTime IcalOccurrenceDateTime { get; set; }

            public Guid ScheduleGuid { get; set; }
        }

        #endregion
    }
}