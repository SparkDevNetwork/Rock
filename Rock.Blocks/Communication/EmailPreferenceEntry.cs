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

using Rock.Attribute;
using Rock.Data;
using Rock.Enums.Blocks.Communication.EmailPreferenceEntry;
using Rock.Enums.Communication;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks.Communication.EmailPreferenceEntry;
using Rock.Web.Cache;
using Rock.Web.UI;

using CommunicationType = Rock.Enums.Communication.CommunicationType;

namespace Rock.Blocks.Communication
{
    /// <summary>
    /// Allows a person to set their email preferences and manage their subscriptions to communication channels.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Email Preference Entry" )]
    [Category( "Communication" )]
    [Description( "Allows a person to set their email preferences and manage their subscriptions to communication channels." )]
    [IconCssClass( "ti ti-question-mark" )]
    [SupportedSiteTypes( Model.SiteType.Web )]
    [ContextAware( typeof( Campus ) )]

    #region Block Attributes

    #region Basic Settings > General Settings

    [CustomCheckboxListField( "Communication Configuration Options",
        Key = AttributeKey.CommunicationConfigurationOptions,
        Description = "Select the options that should be available to a person when they are updating their communication preferences.",
        ListSource = "Update Email Address^Update Email Address,Unsubscribe^Manage Channel Subscriptions,Not Involved^Remove Involvement (Unsubscribe Mode Only)",
        DefaultValue = "Update Email Address,Unsubscribe,Not Involved",
        Category = AttributeCategory.BasicSettings_GeneralSettings,
        Order = 0,
        IsRequired = true )]

    [BooleanField( "Show Header Icon",
        Key = AttributeKey.ShowHeaderIcon,
        Description = "When enabled, a default icon will be shown beside the header title.",
        DefaultBooleanValue = true,
        Category = AttributeCategory.BasicSettings_GeneralSettings,
        Order = 1,
        IsRequired = false )]

    [BooleanField( "Exclude Communication Flows",
        Key = AttributeKey.ExcludeCommunicationFlows,
        Description = "When enabled, communication flows will not be displayed in the Current or Available Communication Channels sections.",
        DefaultBooleanValue = false,
        Category = AttributeCategory.BasicSettings_GeneralSettings,
        Order = 2,
        IsRequired = false )]

    [BooleanField( "Filter Communication Lists by Campus Context",
        Key = AttributeKey.FilterCommunicationListsByCampusContext,
        Description = "When enabled, all communication lists will be filtered by the campus context of the page. Lists with no campus will always be shown.",
        DefaultBooleanValue = false,
        Category = AttributeCategory.BasicSettings_GeneralSettings,
        Order = 3,
        IsRequired = false )]

    [BooleanField( "Always Include Subscribed Communication Lists",
        Key = AttributeKey.AlwaysIncludeSubscribedCommunicationLists,
        Description = "If campus context filtering is enabled, always include lists the person is subscribed to, even if they don't match the current campus context. Note: Category filtering still applies — lists outside the selected categories will not be shown.",
        DefaultBooleanValue = true,
        Category = AttributeCategory.BasicSettings_GeneralSettings,
        Order = 4,
        IsRequired = false )]

    [IntegerField( "Message Count Window (In Days)",
        Key = AttributeKey.MessageCountWindowDays,
        Description = "Set the number of days to look back when calculating how many messages a person has received for the Current Communication Channels section. Set to 0 to disable feature.",
        DefaultIntegerValue = 30,
        Category = AttributeCategory.BasicSettings_GeneralSettings,
        Order = 5,
        IsRequired = false )]

    [LinkedPage( "Manage My Account Page",
        Key = AttributeKey.ManageMyAccountPage,
        Description = "Select the page that the Manage My Account button should open for a person's account. This button will be shown if a person's account has been deactivated and also for Unsubscribe Mode. Leave blank to hide the button.",
        Category = AttributeCategory.BasicSettings_GeneralSettings,
        Order = 6,
        IsRequired = false )]

    [WorkflowTypeField( "Unsubscribe from Communication List Workflow",
        Key = AttributeKey.UnsubscribeFromCommunicationListWorkflow,
        Description = "Select the workflow type to launch when a person unsubscribes from a communication list. The person will be passed in as the Entity and the communication list ID will be passed to the workflow 'CommunicationListIds' attribute if it exists.",
        Category = AttributeCategory.BasicSettings_GeneralSettings,
        Order = 7,
        IsRequired = false )]

    #endregion Basic Settings > General Settings

    #region Basic Settings > Preference Mode Settings

    [CustomRadioListField( "Available Communication Channels",
        Key = AttributeKey.AvailableCommunicationChannels,
        Description = "Choose which communication channels to display in the Available Communication Channels section. Previously Subscribed Channels: Shows all lists where the person was once subscribed, even if currently inactive. All Channels Available: Shows all lists the person has permission to view.",
        ListSource = "Subscribed^Show Only Previously Subscribed Channels,All^Show All Channels Available",
        DefaultValue = "All",
        Category = AttributeCategory.BasicSettings_PreferenceModeSettings,
        Order = 0,
        IsRequired = true )]

    [GroupCategoryField( "Communication List Categories",
        Key = AttributeKey.CommunicationListCategories_PreferenceMode,
        Description = "Select which categories of communication lists to display for Preference Mode. If no categories are selected, all lists the person is authorized to view will be shown. Note: category permissions override communication list permissions.",
        GroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_COMMUNICATIONLIST,
        AllowMultiple = true,
        Category = AttributeCategory.BasicSettings_PreferenceModeSettings,
        Order = 1,
        IsRequired = false )]

    [CategoryField( "Communication Flow Categories",
        Key = AttributeKey.CommunicationFlowCategories_PreferenceMode,
        Description = "Select which categories of communication flows to display in the Current Communication Channels section for Preference Mode. If no categories are selected, all flows the person is in will be shown.",
        EntityType = typeof( CommunicationFlow ),
        AllowMultiple = true,
        Category = AttributeCategory.BasicSettings_PreferenceModeSettings,
        Order = 2,
        IsRequired = false )]

    [BooleanField( "Show Medium Preference On Current Channels",
        Key = AttributeKey.ShowMediumPreferenceOnCurrentChannels,
        Description = "When enabled, will display the person's current communication medium preference for each list and allow changes.",
        DefaultBooleanValue = true,
        Category = AttributeCategory.BasicSettings_PreferenceModeSettings,
        Order = 3,
        IsRequired = false )]

    #endregion Basic Settings > Preference Mode Settings

    #region Basic Settings > Unsubscribe Mode Settings

    [GroupCategoryField( "Communication List Categories",
        Key = AttributeKey.CommunicationListCategories_UnsubscribeMode,
        Description = "Select which categories of communication lists to display for Unsubscribe Mode. If no categories are selected, all lists the person is authorized to view will be shown. Note: category permissions override communication list permissions.",
        GroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_COMMUNICATIONLIST,
        AllowMultiple = true,
        Category = AttributeCategory.BasicSettings_UnsubscribeModeSettings,
        Order = 0,
        IsRequired = false )]

    [CategoryField( "Communication Flow Categories",
        Key = AttributeKey.CommunicationFlowCategories_UnsubscribeMode,
        Description = "Select which categories of communication flows to display for Unsubscribe Mode. If no categories are selected, all flows the person is in will be shown.",
        EntityType = typeof( CommunicationFlow ),
        AllowMultiple = true,
        Category = AttributeCategory.BasicSettings_UnsubscribeModeSettings,
        Order = 1,
        IsRequired = false )]

    [BooleanField( "Allow Deactivating Family",
        Key = AttributeKey.AllowDeactivatingFamily,
        Description = "When enabled, if the person chooses to remove their involvement, show the option to deactivate records for the whole family. This will not show if the person is a member of more than one family or is not an adult.",
        DefaultBooleanValue = true,
        Category = AttributeCategory.BasicSettings_UnsubscribeModeSettings,
        Order = 2,
        IsRequired = false )]

    [DefinedValueField( "Inactive Reasons to Exclude",
        Key = AttributeKey.InactiveReasonsToExclude,
        Description = "The inactive reasons to exclude from the list when a person is removing themselves from church involvement.",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS_REASON,
        // PERSON_RECORD_STATUS_REASON_NO_ACTIVITY,PERSON_RECORD_STATUS_REASON_DECEASED
        DefaultValue = "64014FE6-943D-4ACF-8014-FED9F9169AE8,05D35BC4-5816-4210-965F-1BF44F35A16A",
        AllowMultiple = true,
        Category = AttributeCategory.BasicSettings_UnsubscribeModeSettings,
        Order = 3,
        IsRequired = false )]

    #endregion Basic Settings > Unsubscribe Mode Settings

    #region Customize Text > Section Descriptions

    [MemoField( "Opt-Out Options Description",
        Key = AttributeKey.OptOutOptionsDescription,
        Description = "The text to display for the Opt-Out Options section description. <span class='tip tip-lava'></span>",
        DefaultValue = AttributeDefault.OptOutOptionsDescription,
        NumberOfRows = 2,
        Category = AttributeCategory.CustomizeText_SectionDescriptions,
        Order = 0,
        IsRequired = false )]

    [MemoField( "Current Communication Channels Description",
        Key = AttributeKey.CurrentCommunicationChannelsDescription,
        Description = "The text to display for the Current Communication Channels section description. <span class='tip tip-lava'></span>",
        DefaultValue = AttributeDefault.CurrentCommunicationChannelsDescription,
        NumberOfRows = 2,
        Category = AttributeCategory.CustomizeText_SectionDescriptions,
        Order = 1,
        IsRequired = false )]

    [MemoField( "Available Communication Channels Description",
        Key = AttributeKey.AvailableCommunicationChannelsDescription,
        Description = "The text to display for the Available Communication Channels section description. <span class='tip tip-lava'></span>",
        DefaultValue = AttributeDefault.AvailableCommunicationChannelsDescription,
        NumberOfRows = 2,
        Category = AttributeCategory.CustomizeText_SectionDescriptions,
        Order = 2,
        IsRequired = false )]

    #endregion Customize Text > Section Descriptions

    #region Customize Text > Opt-Out Options Section

    [TextField( "Email Preference Title",
        Key = AttributeKey.EmailPreferenceTitle,
        Description = "The title text to display for the Email Preference option.",
        DefaultValue = "Email Preference",
        Category = AttributeCategory.CustomizeText_OptOutOptionsSection,
        Order = 0,
        IsRequired = false )]

    [TextField( "Email Preference Description",
        Key = AttributeKey.EmailPreferenceDescription,
        Description = "The description text to display for the Email Preference option.",
        DefaultValue = "Choose how you'd like to receive emails from us. You can update this anytime.",
        Category = AttributeCategory.CustomizeText_OptOutOptionsSection,
        Order = 1,
        IsRequired = false )]

    [TextField( "Remove Involvement Title",
        Key = AttributeKey.RemoveInvolvementTitle,
        Description = "The title text to display for the Remove Involvement option.",
        DefaultValue = "Remove Me From All Church Involvement",
        Category = AttributeCategory.CustomizeText_OptOutOptionsSection,
        Order = 2,
        IsRequired = false )]

    [TextField( "Remove Involvement Description",
        Key = AttributeKey.RemoveInvolvementDescription,
        Description = "The description text to display for the Remove Involvement option.",
        DefaultValue = "I'm No Longer Involved With This Church, Please Deactivate My Account",
        Category = AttributeCategory.CustomizeText_OptOutOptionsSection,
        Order = 3,
        IsRequired = false )]

    #endregion Customize Text > Opt-Out Options Section

    #region Customize Text > Preference Mode Header

    [TextField( "Preference Mode Header Title",
        Key = AttributeKey.PreferenceModeHeaderTitle,
        Description = "The text that is displayed for the header title when navigating to this block from the website.",
        DefaultValue = "Communication Preferences",
        Category = AttributeCategory.CustomizeText_PreferenceModeHeader,
        Order = 0,
        IsRequired = false )]

    [MemoField( "Preference Mode Header Description",
        Key = AttributeKey.PreferenceModeHeaderDescription,
        Description = "The description text that is displayed under the header title when navigating to this block from the website. <span class='tip tip-lava'></span>",
        DefaultValue = AttributeDefault.PreferenceModeHeaderDescription,
        NumberOfRows = 2,
        Category = AttributeCategory.CustomizeText_PreferenceModeHeader,
        Order = 1,
        IsRequired = false )]

    #endregion Customize Text > Preference Mode Header

    #region Customize Text > Unsubscribe Mode Header

    [TextField( "Unsubscribed From Channel Confirmation Title",
        Key = AttributeKey.UnsubscribedFromChannelConfirmationTitle,
        Description = "The text that is displayed for the header title when someone successfully unsubscribes from an email link.",
        DefaultValue = "You've Been Unsubscribed",
        Category = AttributeCategory.CustomizeText_UnsubscribeModeHeader,
        Order = 0,
        IsRequired = false )]

    [MemoField( "Unsubscribed From Channel Confirmation Description",
        Key = AttributeKey.UnsubscribedFromChannelConfirmationDescription,
        Description = "The description text that is displayed under the header title when someone successfully unsubscribes from an email link. <span class='tip tip-lava'></span>",
        DefaultValue = AttributeDefault.UnsubscribedFromChannelConfirmationDescription,
        NumberOfRows = 2,
        Category = AttributeCategory.CustomizeText_UnsubscribeModeHeader,
        Order = 1,
        IsRequired = false )]

    [TextField( "Unsubscribed From Bulk Email Confirmation Title",
        Key = AttributeKey.UnsubscribedFromBulkEmailConfirmationTitle,
        Description = "The text that is displayed for the header title when someone successfully unsubscribes from all bulk lists. Note: This is triggered when someone unsubscribes from an email link without being a part of any channels.",
        DefaultValue = "You've Been Unsubscribed From All Bulk Emails",
        Category = AttributeCategory.CustomizeText_UnsubscribeModeHeader,
        Order = 2,
        IsRequired = false )]

    [MemoField( "Unsubscribed From Bulk Email Confirmation Description",
        Key = AttributeKey.UnsubscribedFromBulkEmailConfirmationDescription,
        Description = "The description text that is displayed under the header title when someone successfully unsubscribes from all bulk lists. Note: This is triggered when someone unsubscribes from an email link without being a part of any channels. <span class='tip tip-lava'></span>",
        DefaultValue = AttributeDefault.UnsubscribedFromBulkEmailConfirmationDescription,
        NumberOfRows = 2,
        Category = AttributeCategory.CustomizeText_UnsubscribeModeHeader,
        Order = 3,
        IsRequired = false )]

    [TextField( "Unsubscribed From All Emails Confirmation Title",
        Key = AttributeKey.UnsubscribedFromAllEmailsConfirmationTitle,
        Description = "The text that is displayed for the header title when someone successfully unsubscribes from all emails. Note: This is triggered when someone unsubscribes from an email link who has already unsubscribed from all bulk email lists.",
        DefaultValue = "You've Been Unsubscribed From All Emails",
        Category = AttributeCategory.CustomizeText_UnsubscribeModeHeader,
        Order = 4,
        IsRequired = false )]

    [MemoField( "Unsubscribed From All Emails Confirmation Description",
        Key = AttributeKey.UnsubscribedFromAllEmailsConfirmationDescription,
        Description = "The description text that is displayed under the header title when someone successfully unsubscribes from all emails. Note: This is triggered when someone unsubscribes from an email link who has already unsubscribed from all bulk email lists. <span class='tip tip-lava'></span>",
        DefaultValue = AttributeDefault.UnsubscribedFromAllEmailsConfirmationDescription,
        NumberOfRows = 2,
        Category = AttributeCategory.CustomizeText_UnsubscribeModeHeader,
        Order = 5,
        IsRequired = false )]

    #endregion Customize Text > Unsubscribe Mode Header

    #region Customize Text > Account Deactivation Header

    [TextField( "Remove Involvement Confirmation Title",
        Key = AttributeKey.RemoveInvolvementConfirmationTitle,
        Description = "The text that is displayed for the header title when someone requests to be removed from involvement.",
        DefaultValue = "Confirm Your Request",
        Category = AttributeCategory.CustomizeText_AccountDeactivationHeader,
        Order = 0,
        IsRequired = false )]

    [MemoField( "Remove Involvement Confirmation Description",
        Key = AttributeKey.RemoveInvolvementConfirmationDescription,
        Description = "The description text that is displayed under the header title when someone requests to be removed from involvement. <span class='tip tip-lava'></span>",
        DefaultValue = AttributeDefault.RemoveInvolvementConfirmationDescription,
        NumberOfRows = 2,
        Category = AttributeCategory.CustomizeText_AccountDeactivationHeader,
        Order = 1,
        IsRequired = false )]

    [TextField( "Person Removed Involvement Success Title",
        Key = AttributeKey.PersonRemovedInvolvementSuccessTitle,
        Description = "The text that is displayed for the header title when a person has been removed from involvement.",
        DefaultValue = "Your Request Has Been Completed",
        Category = AttributeCategory.CustomizeText_AccountDeactivationHeader,
        Order = 2,
        IsRequired = false )]

    [MemoField( "Person Removed Involvement Success Description",
        Key = AttributeKey.PersonRemovedInvolvementSuccessDescription,
        Description = "The description text that is displayed under the header title when a person has been removed from involvement. <span class='tip tip-lava'></span>",
        DefaultValue = AttributeDefault.PersonRemovedInvolvementSuccessDescription,
        NumberOfRows = 2,
        Category = AttributeCategory.CustomizeText_AccountDeactivationHeader,
        Order = 3,
        IsRequired = false )]

    [TextField( "Family Removed Involvement Success Title",
        Key = AttributeKey.FamilyRemovedInvolvementSuccessTitle,
        Description = "The text that is displayed for the header title when a person and their family have been removed from involvement.",
        DefaultValue = "Your Request Has Been Completed",
        Category = AttributeCategory.CustomizeText_AccountDeactivationHeader,
        Order = 4,
        IsRequired = false )]

    [MemoField( "Family Removed Involvement Success Description",
        Key = AttributeKey.FamilyRemovedInvolvementSuccessDescription,
        Description = "The description text that is displayed under the header title when a person and their family have been removed from involvement. <span class='tip tip-lava'></span>",
        DefaultValue = AttributeDefault.FamilyRemovedInvolvementSuccessDescription,
        NumberOfRows = 2,
        Category = AttributeCategory.CustomizeText_AccountDeactivationHeader,
        Order = 5,
        IsRequired = false )]

    #endregion Customize Text > Account Deactivation Header

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "E80E5BA7-40C8-4E99-B2F7-0B668012521F" )]
    [Rock.SystemGuid.BlockTypeGuid( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F" )]
    public class EmailPreferenceEntry : RockBlockType
    {
        #region Keys & Constants

        private static class AttributeKey
        {
            // Basic Settings > General Settings
            public const string CommunicationConfigurationOptions = "AvailableOptions";
            public const string ShowHeaderIcon = "ShowHeaderIcon";
            public const string ExcludeCommunicationFlows = "ExcludeCommunicationFlows";
            public const string FilterCommunicationListsByCampusContext = "FilterGroupsByCampusContext";
            public const string AlwaysIncludeSubscribedCommunicationLists = "AlwaysIncludeSubscribedLists";
            public const string MessageCountWindowDays = "MessageCountWindowDays";
            public const string ManageMyAccountPage = "ManageMyAccountPage";
            public const string UnsubscribeFromCommunicationListWorkflow = "UnsubscribeWorkflow";

            // Basic Settings > Preference Mode Settings
            public const string AvailableCommunicationChannels = "AvailableCommunicationChannels";
            public const string CommunicationListCategories_PreferenceMode = "CommunicationListCategories_PreferenceMode";
            public const string CommunicationFlowCategories_PreferenceMode = "CommunicationFlowCategories_PreferenceMode";
            public const string ShowMediumPreferenceOnCurrentChannels = "ShowMediumPreference";

            // Basic Settings > Unsubscribe Mode Settings
            public const string CommunicationListCategories_UnsubscribeMode = "CommunicationListCategories_UnsubscribeMode";
            public const string CommunicationFlowCategories_UnsubscribeMode = "CommunicationFlowCategories_UnsubscribeMode";
            public const string AllowDeactivatingFamily = "AllowInactivatingFamily";
            public const string InactiveReasonsToExclude = "ReasonstoExclude";

            // Customize Text > Section Descriptions
            public const string OptOutOptionsDescription = "OptOutOptionsDescription";
            public const string CurrentCommunicationChannelsDescription = "CurrentCommunicationChannelsDescription";
            public const string AvailableCommunicationChannelsDescription = "AvailableCommunicationChannelsDescription";

            // Customize Text > Opt-Out Options
            public const string EmailPreferenceTitle = "EmailPreferenceTitle";
            public const string EmailPreferenceDescription = "EmailPreferenceDescription";
            public const string RemoveInvolvementTitle = "RemoveInvolvementTitle";
            public const string RemoveInvolvementDescription = "RemoveInvolvementDescription";

            // Customize Text > Preference Mode Header
            public const string PreferenceModeHeaderTitle = "PreferenceModeHeaderTitle";
            public const string PreferenceModeHeaderDescription = "PreferenceModeHeaderDescription";

            // Customize Text > Unsubscribe Mode Header
            public const string UnsubscribedFromChannelConfirmationTitle = "UnsubscribedFromChannelConfirmationTitle";
            public const string UnsubscribedFromChannelConfirmationDescription = "UnsubscribedFromChannelConfirmationDescription";
            public const string UnsubscribedFromBulkEmailConfirmationTitle = "UnsubscribedFromBulkEmailConfirmationTitle";
            public const string UnsubscribedFromBulkEmailConfirmationDescription = "UnsubscribedFromBulkEmailConfirmationDescription";
            public const string UnsubscribedFromAllEmailsConfirmationTitle = "UnsubscribedFromAllEmailsConfirmationTitle";
            public const string UnsubscribedFromAllEmailsConfirmationDescription = "UnsubscribedFromAllEmailsConfirmationDescription";

            // Customize Text > Account Deactivation Header
            public const string RemoveInvolvementConfirmationTitle = "RemoveInvolvementConfirmationTitle";
            public const string RemoveInvolvementConfirmationDescription = "RemoveInvolvementConfirmationDescription";
            public const string PersonRemovedInvolvementSuccessTitle = "PersonRemovedInvolvementSuccessTitle";
            public const string PersonRemovedInvolvementSuccessDescription = "PersonRemovedInvolvementSuccessDescription";
            public const string FamilyRemovedInvolvementSuccessTitle = "FamilyRemovedInvolvementSuccessTitle";
            public const string FamilyRemovedInvolvementSuccessDescription = "FamilyRemovedInvolvementSuccessDescription";

            // The following keys are not used for block settings, but to load attribute values for entities.
            public const string Category = "Category";
            public const string PublicName = "PublicName";
        }

        private static class AttributeCategory
        {
            public const string BasicSettings_GeneralSettings = "";
            public const string BasicSettings_PreferenceModeSettings = "Preference Mode Settings";
            public const string BasicSettings_UnsubscribeModeSettings = "Unsubscribe Mode Settings";

            public const string CustomizeText_SectionDescriptions = "Customize Text^Section Descriptions";
            public const string CustomizeText_OptOutOptionsSection = "Customize Text^Opt-Out Options";
            public const string CustomizeText_PreferenceModeHeader = "Customize Text^Preference Mode Header";
            public const string CustomizeText_UnsubscribeModeHeader = "Customize Text^Unsubscribe Mode Header";
            public const string CustomizeText_AccountDeactivationHeader = "Customize Text^Account Deactivation Header";
        }

        private static class AttributeDefault
        {
            public const string OptOutOptionsDescription = "";

            public const string CurrentCommunicationChannelsDescription = "These are the communication lists you've subscribed to. You may unsubscribe or update your preferred communication method.";

            public const string AvailableCommunicationChannelsDescription = "These are the communication lists you've subscribed to before, or that you might be interested in subscribing to.";

            public const string PreferenceModeHeaderDescription = "Configure below which communications you want to receive from {{ 'Global' | Attribute:'OrganizationName' }}.";

            public const string UnsubscribedFromChannelConfirmationDescription = "You've successfully unsubscribed from {{ Channel.Name }} emails. Manage your other email preferences below.";

            public const string UnsubscribedFromBulkEmailConfirmationDescription = "You will no longer receive bulk emails from {{ 'Global' | Attribute:'OrganizationName' }}, but you may still receive individual messages when necessary.";

            public const string UnsubscribedFromAllEmailsConfirmationDescription = "You will no longer receive any emails from {{ 'Global' | Attribute:'OrganizationName' }}, including announcements, updates, and personal correspondence.";

            public const string RemoveInvolvementConfirmationDescription = "Once you confirm, we will update our records, and you will no longer receive official church communications. If there's anything we can do to support you, please let us know.";

            public const string PersonRemovedInvolvementSuccessDescription = "Your personal record at {{ 'Global' | Attribute:'OrganizationName' }} has been deactivated, and you will no longer receive official communications from us. If there's anything else we can do for you, please let us know.";

            public const string FamilyRemovedInvolvementSuccessDescription = "Your personal record, along with your family's records, has been deactivated at {{ 'Global' | Attribute:'OrganizationName' }}. You will no longer receive official communications from us. If there's anything else we can do for you, please let us know.";
        }

        private static class PageParameterKey
        {
            // "Communication" allows Communication Id, Guid, or IdKey values,
            // while the older "CommunicationId" only supports Id.
            public const string Communication = "Communication";
            public const string CommunicationId = "CommunicationId";

            public const string Person = "Person";
        }

        private static class NavigationUrlKey
        {
            public const string ManageMyAccountPage = "ManageMyAccountPage";
        }

        private static class MergeFieldKey
        {
            public const string Communication = "Communication";
            public const string Channel = "Channel";
            public const string UnsubscribeLevel = "UnsubscribeLevel";
        }

        private static class AvailableOption
        {
            public const string UpdateContactInfo = "Update Email Address";
            public const string RemoveInvolvement = "Not Involved";
            public const string ManageChannelSubscriptions = "Unsubscribe";
        }

        private static class AvailableCommunicationChannels
        {
            public const string PreviouslySubscribed = "Subscribed";
            public const string All = "All";
        }

        private static class GroupMemberNote
        {
            public const string Unsubscribed = "Unsubscribed";
        }

        private static class WorkflowAttributeKey
        {
            public const string CommunicationListIds = "CommunicationListIds";
        }

        #endregion Keys & Constants

        #region Fields

        /// <summary>
        /// The backing field for the <see cref="CommunicationKeyFromPageParameter"/> property.
        /// </summary>
        private string _communicationKeyFromPageParameter;


        /// <summary>
        /// The backing field for the <see cref="CommunicationFromPageParameter"/> property.
        /// </summary>
        private Model.Communication _communicationFromPageParameter;

        /// <summary>
        /// The backing field for the <see cref="IsUnsubscribeMode"/> property.
        /// </summary>
        private bool? _isUnsubscribeMode;

        /// <summary>
        /// The backing field for the <see cref="MessageCountWindowDays"/> property.
        /// </summary>
        private int? _messageCountWindowDays;

        /// <summary>
        /// The backing field for the <see cref="ContextCampus"/> property.
        /// </summary>
        private Campus _contextCampus;

        /// <summary>
        /// Whether we've already checked for a context <see cref="Campus"/>.
        /// </summary>
        private bool _hasCheckedForContextCampus;

        /// <summary>
        /// The backing field for the <see cref="AllowedCommunicationListCategoryGuids"/> property.
        /// </summary>
        private HashSet<Guid> _allowedCommunicationListCategoryGuids;

        /// <summary>
        /// The backing field for the <see cref="AllowedCommunicationFlowCategoryIds"/> property.
        /// </summary>
        private HashSet<int> _allowedCommunicationFlowCategoryIds;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets the <see cref="Model.Communication"/> entity key passed to the "Communication" or "CommunicationId" page parameter.
        /// </summary>
        private string CommunicationKeyFromPageParameter
        {
            get
            {
                if ( _communicationKeyFromPageParameter.IsNullOrWhiteSpace() )
                {
                    var communicationPageParameter = PageParameter( PageParameterKey.Communication );

                    if ( communicationPageParameter.IsNotNullOrWhiteSpace() )
                    {
                        _communicationKeyFromPageParameter = communicationPageParameter;
                    }
                    else
                    {
                        // Only allow the CommunicationId to contain an ID, but return it as a string so it can be used as an entity key.
                        _communicationKeyFromPageParameter = PageParameter( PageParameterKey.CommunicationId ).AsIntegerOrNull()?.ToString();
                    }
                }

                return _communicationKeyFromPageParameter;
            }
        }

        /// <summary>
        /// Gets the <see cref="Model.Communication"/> specified in the "Communication" or "CommunicationId" page parameter.
        /// </summary>
        private Model.Communication CommunicationFromPageParameter
        {
            get
            {
                if ( _communicationFromPageParameter == null )
                {
                    if ( CommunicationKeyFromPageParameter.IsNullOrWhiteSpace() )
                    {
                        return null;
                    }

                    // Do not disable tracking (`.AsNoTracking()`) since this communication instance will be passed to several
                    // Lava templates to resolve the block's UI content.
                    _communicationFromPageParameter = new CommunicationService( RockContext )
                        .GetQueryableByKey( CommunicationKeyFromPageParameter, !PageCache.Layout.Site.DisablePredictableIds )
                        .Include( c => c.ListGroup )
                        .FirstOrDefault();
                }

                return _communicationFromPageParameter;
            }
        }

        /// <summary>
        /// Gets whether this block is operating in "Unsubscribe" mode.
        /// </summary>
        private bool IsUnsubscribeMode
        {
            get
            {
                if ( !_isUnsubscribeMode.HasValue )
                {
                    _isUnsubscribeMode = CommunicationFromPageParameter != null;
                }

                return _isUnsubscribeMode.Value;
            }
        }

        /// <summary>
        /// Gets the number of days used to calculate how many messages a person has received per current channel.
        /// </summary>
        private int MessageCountWindowDays
        {
            get
            {
                if ( !_messageCountWindowDays.HasValue )
                {
                    _messageCountWindowDays = Math.Abs( GetAttributeValue( AttributeKey.MessageCountWindowDays ).AsIntegerOrNull() ?? 0 );
                }

                return _messageCountWindowDays.Value;
            }
        }

        /// <summary>
        /// Gets the campus from the request context, if defined.
        /// </summary>
        private Campus ContextCampus
        {
            get
            {
                if ( !_hasCheckedForContextCampus )
                {
                    _contextCampus = RequestContext.GetContextEntity<Campus>();
                    _hasCheckedForContextCampus = true;
                }

                return _contextCampus;
            }
        }

        /// <summary>
        /// Gets the allowed <see cref="Model.Communication"/> list <see cref="Category"/> unique identifiers, if any.
        /// </summary>
        private HashSet<Guid> AllowedCommunicationListCategoryGuids
        {
            get
            {
                if ( _allowedCommunicationListCategoryGuids == null )
                {
                    var attributeKey = IsUnsubscribeMode
                        ? AttributeKey.CommunicationListCategories_UnsubscribeMode
                        : AttributeKey.CommunicationListCategories_PreferenceMode;

                    _allowedCommunicationListCategoryGuids = new HashSet<Guid>(
                        GetAttributeValue( attributeKey ).SplitDelimitedValues().AsGuidList()
                    );
                }

                return _allowedCommunicationListCategoryGuids;
            }
        }

        /// <summary>
        /// Gets the allowed <see cref="CommunicationFlow"/> <see cref="Category"/> identifiers, if any.
        /// </summary>
        private HashSet<int> AllowedCommunicationFlowCategoryIds
        {
            get
            {
                if ( _allowedCommunicationFlowCategoryIds == null )
                {
                    var attributeKey = IsUnsubscribeMode
                        ? AttributeKey.CommunicationFlowCategories_UnsubscribeMode
                        : AttributeKey.CommunicationFlowCategories_PreferenceMode;

                    _allowedCommunicationFlowCategoryIds = GetAttributeValue( attributeKey )
                        .SplitDelimitedValues()
                        .AsGuidList()
                        .Select( g => CategoryCache.GetId( g ) )
                        .Where( id => id.HasValue )
                        .Select( id => id.Value )
                        .ToHashSet();
                }

                return _allowedCommunicationFlowCategoryIds;
            }
        }

        /// <summary>
        /// Gets whether to display the person's current communication medium preference for each list and allow changes.
        /// </summary>
        private bool ShowMediumPreference => !IsUnsubscribeMode
            && GetAttributeValue( AttributeKey.ShowMediumPreferenceOnCurrentChannels ).AsBoolean();

        /// <summary>
        /// Gets whether to show all available communication channels.
        /// </summary>
        /// <value>
        /// <c>true</c> if all available communication channels should be shown; <c>false</c> if only
        /// previously-subscribed, available channels should be shown.
        /// </value>
        private bool ShowAllAvailableChannels => GetAttributeValue( AttributeKey.AvailableCommunicationChannels ) == AvailableCommunicationChannels.All;

        /// <summary>
        /// Gets the "Inactive" <see cref="Person"/> record status <see cref="DefinedValue"/> identifier.
        /// </summary>
        private int InactivePersonRecordStatusValueId => DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE )?.Id ?? 0;

        #endregion Properties

        #region RockBlockType Implementation

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new EmailPreferenceEntryInitializationBox();

            var (person, isFromPageParameter) = GetPerson();
            if ( person == null )
            {
                box.ErrorMessage = "Unable to update your email preference, as we're not sure who you are.";
                return box;
            }

            // Take note of their email preference at the time of block initialization so we can know whether they were
            // unsubscribed as a result of this particular block load.
            var originalEmailPreference = person.EmailPreference;

            var mergeFields = RequestContext.GetCommonMergeFields();

            var communication = CommunicationFromPageParameter;
            if ( communication != null )
            {
                mergeFields.Add( MergeFieldKey.Communication, communication );
            }

            UnsubscribeLevel? unsubscribeLevel = null;
            CommunicationChannel communicationChannel = null;

            /*
                5/31/2024 - JMH

                Some individuals were being unsubscribed from email automatically without clicking an unsubscribe button
                in their email.

                Per the spec, https://datatracker.ietf.org/doc/html/rfc8058#section-3.2, an email client must send a
                POST request with the key-value pair, list-unsubscribe=one-click in order to perform a one-click
                un-subscription. It also states that an email client "...MUST NOT perform a POST on the HTTPS URI without
                user consent...".

                If an email client is following the spec, then it isn't likely that POST requests are being sent without
                user consent (clicking an unsubscribe button); however, an email client can still send HEAD, GET,
                OPTIONS or other requests to this URL. Since this block didn't differentiate between requests, it would
                always unsubscribe a person automatically regardless of the request type.

                To prevent unintentional un-subscriptions, only unsubscribe a person from email automatically if
                responding to a one-click unsubscribe request.
            */
            if ( IsUnsubscribeMode && isFromPageParameter && ( IsOneClickUnsubscribeRequest() || IsGetRequest() ) )
            {
                // If we get into this block of code, this means:
                //  1) There's a communication key in the query string and a matching communication was found: hence the
                //     "Unsubscribe Mode".
                //  2) There's a person identifier in the query string - either an [unsubscribe] action identifier or an
                //     impersonation token - and a matching person was found.
                //  3) This request is one of the following:
                //      A) A one-click unsubscribe request triggered from the email client by the person when clicking
                //         on the client's built-in [Unsubscribe] button.
                //      B) The person actually clicked the unsubscribe link in the email, as opposed to an automated
                //         HEAD or OPTIONS request sent by the email client.
                //      C) The person reloaded the page after previously clicking on the email's unsubscribe link (or
                //         clicked on the link a 2nd time).

                var personService = new PersonService( RockContext );

                if ( communication?.ListGroupId.HasValue == true )
                {
                    // Auto-unsubscribe from a specific communication list.
                    var communicationListsQry = new GroupService( RockContext )
                        .GetQueryableByKey( communication.ListGroupId.ToString() )
                        .IsCommunicationList();

                    personService.UnsubscribeFromEmail( person, communicationListsQry );

                    unsubscribeLevel = UnsubscribeLevel.CommunicationList;

                    // Get the list's name so we can provide it to the Lava templates.
                    var communicationList = communication.ListGroup;
                    communicationList.LoadAttributes();

                    var communicationListName = communicationList.GetAttributeValue( AttributeKey.PublicName );
                    if ( communicationListName.IsNullOrWhiteSpace() )
                    {
                        communicationListName = communicationList.Name;
                    }

                    communicationChannel = new CommunicationChannel { Name = communicationListName };

                    var communicationListIdKey = communicationList.IdKey;
                    box.UnsubscribedFromChannel = new CommunicationChannelBag
                    {
                        IdKey = communicationListIdKey,
                        CommunicationChannelType = CommunicationChannelType.List,
                        Name = communicationListName
                    };

                    TryStartUnsubscribeWorkflow(
                        new UnsubscribeWorkflowArgs
                        {
                            ChannelIdKey = communicationListIdKey,
                            CommunicationChannelType = CommunicationChannelType.List,
                            Person = person
                        }
                    );
                }
                else if ( communication != null )
                {
                    // Is this communication from a flow?
                    var communicationFlowInstance = new CommunicationFlowInstanceService( RockContext )
                        .Queryable()
                        .AsNoTracking()
                        .Include( i => i.CommunicationFlow )
                        .Where( i =>
                            i.CommunicationFlowInstanceCommunications.Any( c => c.CommunicationId == communication.Id )
                        )
                        .FirstOrDefault();

                    if ( communicationFlowInstance != null )
                    {
                        new CommunicationFlowService( RockContext )
                            .UnsubscribePersonFromFlow(
                                communicationFlowInstance.CommunicationFlowId,
                                person.Id
                            );

                        unsubscribeLevel = UnsubscribeLevel.Flows;

                        var communicationFlowName = communicationFlowInstance.CommunicationFlow.PublicName;
                        if ( communicationFlowName.IsNullOrWhiteSpace() )
                        {
                            communicationFlowName = communicationFlowInstance.CommunicationFlow.Name;
                        }

                        communicationChannel = new CommunicationChannel { Name = communicationFlowName };

                        box.UnsubscribedFromChannel = new CommunicationChannelBag
                        {
                            IdKey = communicationFlowInstance.CommunicationFlowId.AsIdKey(),
                            CommunicationChannelType = CommunicationChannelType.Flow,
                            Name = communicationFlowName
                        };
                    }
                }

                // If we don't have a communication or the communication wasn't tied to a list or flow,
                // auto-unsubscribe at the person level.
                if ( !unsubscribeLevel.HasValue )
                {
                    personService.UnsubscribeFromEmail( person );

                    switch ( person.EmailPreference )
                    {
                        case EmailPreference.DoNotEmail:
                            unsubscribeLevel = UnsubscribeLevel.All;
                            break;
                        case EmailPreference.NoMassEmails:
                            unsubscribeLevel = UnsubscribeLevel.Bulk;
                            break;
                    }
                }

                RockContext.SaveChanges();
            }

            if ( unsubscribeLevel.HasValue )
            {
                TryUpdateCommunicationRecipientUnsubscribeLevel(
                    person,
                    originalEmailPreference,
                    communication,
                    unsubscribeLevel.Value
                );

                mergeFields.Add( MergeFieldKey.UnsubscribeLevel, unsubscribeLevel.Value.ConvertToString() );
            }

            if ( communicationChannel != null )
            {
                mergeFields.Add( MergeFieldKey.Channel, communicationChannel );
            }

            box.EmailAddress = person.Email;
            box.EmailPreference = person.EmailPreference;
            box.IsRemovedFromChurchInvolvement = person.RecordStatusValueId == InactivePersonRecordStatusValueId;

            if ( !box.IsRemovedFromChurchInvolvement )
            {
                var availableOptions = GetAttributeValue( AttributeKey.CommunicationConfigurationOptions )
                    .SplitDelimitedValues( whitespace: false );

                box.IsUpdateContactInfoEnabled = availableOptions.Contains( AvailableOption.UpdateContactInfo );
                box.IsRemoveInvolvementEnabled = IsUnsubscribeMode && availableOptions.Contains( AvailableOption.RemoveInvolvement );
                box.IsCurrentCommunicationChannelsSectionEnabled = availableOptions.Contains( AvailableOption.ManageChannelSubscriptions );
                box.IsAvailableCommunicationChannelsSectionEnabled = availableOptions.Contains( AvailableOption.ManageChannelSubscriptions );

                if ( box.IsRemoveInvolvementEnabled )
                {
                    box.IsDeactivatingFamilyEnabled = GetAttributeValue( AttributeKey.AllowDeactivatingFamily ).AsBoolean()
                        && person.AgeClassification == AgeClassification.Adult
                        && person.GetFamilies().Count() == 1;

                    var inactiveReasonsToExclude = GetAttributeValues( AttributeKey.InactiveReasonsToExclude ).AsGuidList();

                    box.InactiveReasons = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS_REASON )
                        ?.DefinedValues
                        ?.Where( av => !inactiveReasonsToExclude.Contains( av.Guid ) )
                        ?.ToListItemBagList();

                    if ( box.InactiveReasons?.Any() != true )
                    {
                        // If the admin hasn't allowed any inactive reasons, the person won't be able to deactivate their record.
                        box.IsRemoveInvolvementEnabled = false;
                    }
                }

                // Always load current and available channels (if enabled) in case they toggle their email preference in the UI.
                if ( box.IsCurrentCommunicationChannelsSectionEnabled )
                {
                    box.CurrentCommunicationChannels = GetCurrentCommunicationChannels( person );
                    box.MessageCountWindowDays = MessageCountWindowDays;
                    box.ShowMediumPreference = ShowMediumPreference;
                }

                if ( !IsUnsubscribeMode && box.IsAvailableCommunicationChannelsSectionEnabled )
                {
                    box.AvailableCommunicationChannels = GetAvailableCommunicationChannels( person );
                }
            }

            SetInitializationBoxDisplayValues( box, unsubscribeLevel, mergeFields );

            box.NavigationUrls = GetBoxNavigationUrls();

            return box;
        }

        #endregion RockBlockType Implementation

        #region Block Actions

        /// <summary>
        /// Updates the person's contact info.
        /// </summary>
        /// <param name="bag">The information needed to update the person's contact info.</param>
        /// <returns>A bag containing information about the outcome of the request.</returns>
        [BlockAction]
        public BlockActionResult UpdateContactInfo( UpdateContactInfoRequestBag bag )
        {
            var errorMessagePrefix = "Unable to update your email address";

            if ( ( bag?.EmailAddress ).IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( $"{errorMessagePrefix}." );
            }

            var (person, _) = GetPerson();
            if ( person == null )
            {
                return ActionBadRequest( $"{errorMessagePrefix}, as we're not sure who you are." );
            }

            var trackedPerson = new PersonService( RockContext ).Get( person.Id );
            if ( trackedPerson == null )
            {
                return ActionBadRequest( $"{errorMessagePrefix}, as we're not sure who you are." );
            }

            trackedPerson.Email = bag.EmailAddress;
            if ( trackedPerson.EmailPreference != EmailPreference.DoNotEmail )
            {
                trackedPerson.IsEmailActive = true;
            }

            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Updates the person's email preference.
        /// </summary>
        /// <param name="bag">The information needed to update the person's email preference.</param>
        /// <returns>A bag containing information about the outcome of the request.</returns>
        [BlockAction]
        public BlockActionResult UpdateEmailPreference( UpdateEmailPreferenceRequestBag bag )
        {
            var errorMessagePrefix = "Unable to update your email preference";

            if ( bag == null )
            {
                return ActionBadRequest( $"{errorMessagePrefix}." );
            }

            var (person, _) = GetPerson();
            if ( person == null )
            {
                return ActionBadRequest( $"{errorMessagePrefix}, as we're not sure who you are." );
            }

            var trackedPerson = new PersonService( RockContext ).Get( person.Id );
            if ( trackedPerson == null )
            {
                return ActionBadRequest( $"{errorMessagePrefix}, as we're not sure who you are." );
            }

            trackedPerson.EmailPreference = bag.EmailPreference;
            if ( trackedPerson.EmailPreference != EmailPreference.DoNotEmail )
            {
                trackedPerson.IsEmailActive = true;
            }

            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Subscribes a person to a communication channel.
        /// </summary>
        /// <param name="bag">The information needed to mange the person's communication channel subscription.</param>
        /// <returns>A bag containing information about the outcome of the request.</returns>
        [BlockAction]
        public BlockActionResult Subscribe( ManageChannelSubscriptionRequestBag bag )
        {
            var errorMessagePrefix = "Unable to subscribe to communication channel";

            if ( ( bag?.IdKey ).IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( $"{errorMessagePrefix}." );
            }

            var channelId = IdHasher.Instance.GetId( bag.IdKey );
            if ( !channelId.HasValue )
            {
                return ActionBadRequest( $"{errorMessagePrefix}, as the channel cannot be found." );
            }

            var (person, _) = GetPerson();
            if ( person == null )
            {
                return ActionBadRequest( $"{errorMessagePrefix}, as we're not sure who you are." );
            }

            if ( bag.CommunicationChannelType == CommunicationChannelType.Flow )
            {
                new CommunicationFlowService( RockContext ).ResubscribePersonToFlow( channelId.Value, person.Id );

                RockContext.SaveChanges();

                return ActionOk();
            }

            // Get any existing list group members for this person.
            var groupMemberService = new GroupMemberService( RockContext );
            var listGroupMembers = groupMemberService
                .Queryable()
                .Where( gm =>
                    gm.GroupId == channelId.Value
                    && gm.PersonId == person.Id
                )
                .ToList();

            if ( listGroupMembers.Any() )
            {
                // If this person already has group members, ensure they're active.
                foreach ( var gm in listGroupMembers )
                {
                    gm.GroupMemberStatus = GroupMemberStatus.Active;

                    if ( gm.Note == GroupMemberNote.Unsubscribed )
                    {
                        gm.Note = string.Empty;
                    }
                }
            }
            else
            {
                // Otherwise, add a new member record.
                var groupTypeId = GroupCache.Get( channelId.Value )?.GroupTypeId;
                if ( !groupTypeId.HasValue )
                {
                    return ActionBadRequest( $"{errorMessagePrefix}." );
                }

                var defaultGroupRoleId = GroupTypeCache.Get( groupTypeId.Value ).DefaultGroupRoleId;
                if ( !defaultGroupRoleId.HasValue )
                {
                    return ActionBadRequest( $"{errorMessagePrefix}." );
                }

                var groupMember = new GroupMember
                {
                    GroupId = channelId.Value,
                    PersonId = person.Id,
                    GroupRoleId = defaultGroupRoleId.Value,
                    CommunicationPreference = Model.CommunicationType.Email
                };

                groupMemberService.Add( groupMember );
            }

            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Unsubscribes an person from a communication channel.
        /// </summary>
        /// <param name="bag">The information needed to mange the person's communication channel subscription.</param>
        /// <returns>A bag containing information about the outcome of the request.</returns>
        [BlockAction]
        public BlockActionResult Unsubscribe( ManageChannelSubscriptionRequestBag bag )
        {
            var errorMessagePrefix = "Unable to unsubscribe from communication channel";

            if ( ( bag?.IdKey ).IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( $"{errorMessagePrefix}." );
            }

            var channelId = IdHasher.Instance.GetId( bag.IdKey );
            if ( !channelId.HasValue )
            {
                return ActionBadRequest( $"{errorMessagePrefix}, as the channel cannot be found." );
            }

            var (person, _) = GetPerson();
            if ( person == null )
            {
                return ActionBadRequest( $"{errorMessagePrefix}, as we're not sure who you are." );
            }

            if ( bag.CommunicationChannelType == CommunicationChannelType.Flow )
            {
                new CommunicationFlowService( RockContext ).UnsubscribePersonFromFlow( channelId.Value, person.Id );

                RockContext.SaveChanges();

                return ActionOk();
            }

            // Get any existing list group members for this person.
            var listGroupMembers = new GroupMemberService( RockContext )
                .Queryable()
                .Where( gm =>
                    gm.GroupId == channelId.Value
                    && gm.PersonId == person.Id
                )
                .ToList();

            if ( !listGroupMembers.Any() )
            {
                return ActionOk();
            }

            foreach ( var gm in listGroupMembers )
            {
                gm.GroupMemberStatus = GroupMemberStatus.Inactive;

                if ( gm.Note.IsNullOrWhiteSpace() )
                {
                    gm.Note = GroupMemberNote.Unsubscribed;
                }
            }

            RockContext.SaveChanges();

            TryStartUnsubscribeWorkflow(
                new UnsubscribeWorkflowArgs
                {
                    ChannelIdKey = bag.IdKey,
                    CommunicationChannelType = bag.CommunicationChannelType,
                    Person = person
                }
            );

            return ActionOk();
        }

        /// <summary>
        /// Updates a person's communication preference for a communication channel.
        /// </summary>
        /// <param name="bag">The information needed to update the person's communication preference.</param>
        /// <returns>A bag containing information about the outcome of the request.</returns>
        [BlockAction]
        public BlockActionResult UpdateCommunicationPreference( UpdateCommunicationPreferenceRequestBag bag )
        {
            var errorMessagePrefix = "Unable to update your communication preference";

            if ( ( bag?.IdKey ).IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( $"{errorMessagePrefix}." );
            }

            if ( bag.CommunicationChannelType != CommunicationChannelType.List )
            {
                return ActionBadRequest( $"{errorMessagePrefix}." );
            }

            var channelId = IdHasher.Instance.GetId( bag.IdKey );
            if ( !channelId.HasValue )
            {
                return ActionBadRequest( $"{errorMessagePrefix}, as the channel cannot be found." );
            }

            var (person, _) = GetPerson();
            if ( person == null )
            {
                return ActionBadRequest( $"{errorMessagePrefix}, as we're not sure who you are." );
            }

            // Get any existing list group members for this person.
            var listGroupMembers = new GroupMemberService( RockContext )
                .Queryable()
                .Where( gm =>
                    gm.GroupId == channelId.Value
                    && gm.PersonId == person.Id
                )
                .ToList();

            if ( !listGroupMembers.Any() )
            {
                return ActionOk();
            }

            var communicationPreference = bag.CommunicationPreference == CommunicationType.SMS
                ? Model.CommunicationType.SMS
                : Model.CommunicationType.Email;

            foreach ( var gm in listGroupMembers )
            {
                gm.CommunicationPreference = communicationPreference;
            }

            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Deactivates a person's Rock record, and optionally the records of all family members.
        /// </summary>
        /// <param name="bag">The information needed to deactivate the person's Rock record.</param>
        /// <returns>A bag containing information about the outcome of the request.</returns>
        [BlockAction]
        public BlockActionResult DeactivateRecord( DeactivateRecordRequestBag bag )
        {
            var errorMessagePrefix = "Unable to deactivate your record";

            if ( bag == null )
            {
                return ActionBadRequest( $"{errorMessagePrefix}." );
            }

            var (person, _) = GetPerson();
            if ( person == null )
            {
                return ActionBadRequest( $"{errorMessagePrefix}, as we're not sure who you are." );
            }

            var personService = new PersonService( RockContext );
            var trackedPerson = personService.Get( person.Id );
            if ( trackedPerson == null )
            {
                return ActionBadRequest( $"{errorMessagePrefix}, as we're not sure who you are." );
            }

            var inactiveReasonsToExclude = GetAttributeValues( AttributeKey.InactiveReasonsToExclude ).AsGuidList();
            var allowedInactiveReasonDefinedValues = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS_REASON )
                ?.DefinedValues
                ?.Where( av => !inactiveReasonsToExclude.Contains( av.Guid ) )
                ?.ToList();

            var inactiveReasonDefinedValue = DefinedValueCache.Get( bag.InactiveReasonValueGuid );
            if ( inactiveReasonDefinedValue == null || allowedInactiveReasonDefinedValues?.Any( allowed => allowed.Id == inactiveReasonDefinedValue.Id ) != true )
            {
                return ActionBadRequest( $"{errorMessagePrefix}, as an invalid reason was provided." );
            }

            var selfInactivatedDefinedValue = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_REVIEW_REASON_SELF_INACTIVATED );

            // If the inactive reason note is the same as the current review reason note, update it also.
            string inactiveReasonNote = ( trackedPerson.InactiveReasonNote ?? string.Empty ) == ( trackedPerson.ReviewReasonNote ?? string.Empty )
                ? bag.InactiveReasonNote
                : trackedPerson.InactiveReasonNote;

            var isDeactivatingFamilyAllowed = GetAttributeValue( AttributeKey.AllowDeactivatingFamily ).AsBoolean()
                && trackedPerson.AgeClassification == AgeClassification.Adult
                && trackedPerson.GetFamilies().Count() == 1;

            var response = new DeactivateRecordResponseBag();
            var mergeFields = RequestContext.GetCommonMergeFields();

            // Are we deactivating just one person or the whole family?
            if ( !isDeactivatingFamilyAllowed || !bag.IsFamilyDeactivation )
            {
                personService.InactivatePerson( trackedPerson, inactiveReasonDefinedValue, inactiveReasonNote, selfInactivatedDefinedValue, bag.InactiveReasonNote );

                response.RemovedInvolvementSuccessTitle = GetAttributeValue( AttributeKey.PersonRemovedInvolvementSuccessTitle );
                response.RemovedInvolvementSuccessDescriptionHtml = GetAttributeValue( AttributeKey.PersonRemovedInvolvementSuccessDescription )
                    .ResolveMergeFields( mergeFields );
            }
            else
            {
                // Deactivate each person.
                var inactivatePersonList = personService.GetFamilyMembers( trackedPerson.Id, true ).Select( m => m.Person );
                foreach ( var inactivatePerson in inactivatePersonList )
                {
                    personService.InactivatePerson( inactivatePerson, inactiveReasonDefinedValue, inactiveReasonNote, selfInactivatedDefinedValue, bag.InactiveReasonNote );
                }

                response.RemovedInvolvementSuccessTitle = GetAttributeValue( AttributeKey.FamilyRemovedInvolvementSuccessTitle );
                response.RemovedInvolvementSuccessDescriptionHtml = GetAttributeValue( AttributeKey.FamilyRemovedInvolvementSuccessDescription )
                    .ResolveMergeFields( mergeFields );
            }

            RockContext.SaveChanges();

            return ActionOk( response );
        }

        #endregion Block Actions

        #region Private Methods

        /// <summary>
        /// Gets the <see cref="Person"/> whose preferences should be updated.
        /// </summary>
        /// <returns>
        /// A tuple containing the <see cref="Person"/> whose preferences should be updated as well as a <see langword="bool"/>
        /// indicating whether this person was loaded from a page parameter (either an [unsubscribe] action identifier or
        /// an impersonation token).
        /// </returns>
        private (Person person, bool isFromPageParameter) GetPerson()
        {
            // Return the person associated with the page parameter first, as they likely got to this page from an
            // unsubscribe link. If there is none, then use the authenticated person, as they likely navigated to this
            // page to modify their preferences.
            Person person = null;
            var personPageParameter = PageParameter( PageParameterKey.Person );
            if ( personPageParameter.IsNotNullOrWhiteSpace() )
            {
                var personService = new PersonService( RockContext );
                person = personService.GetByPersonActionIdentifier( personPageParameter, "Unsubscribe" );
                if ( person == null )
                {
                    person = personService.GetByUrlEncodedKey( personPageParameter );
                }
            }

            if ( person != null )
            {
                return (person, true);
            }
            else
            {
                return (GetCurrentPerson(), false);
            }
        }

        /// <summary>
        /// Determines if the current request is a one-click unsubscribe request.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the current request is a one-click unsubscribe request; otherwise, <see langword="false"/>.
        /// </returns>
        private bool IsOneClickUnsubscribeRequest()
        {
            // See https://datatracker.ietf.org/doc/html/rfc8058#section-3.2 for email client spec
            // and https://datatracker.ietf.org/doc/html/rfc8058#section-8 for example POST requests.
            return this.RequestContext.HttpMethod == "POST"
                && this.RequestContext.Form != null
                && this.RequestContext.Form.Get( "List-Unsubscribe" )?.Equals( "One-Click", StringComparison.OrdinalIgnoreCase ) == true;
        }

        /// <summary>
        /// Determines if the current request is a GET request.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the current request is a GET request; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// It's possible for an email client to send HEAD, OPTIONS or other requests to this URL without the person
        /// actually interacting with the unsubscribe link in the email they received. Our goal with this check is to
        /// prevent the initiation of auto-unsubscribe behavior unless they actually click the unsubscribe link.
        /// </remarks>
        private bool IsGetRequest()
        {
            return this.RequestContext.HttpMethod == "GET";
        }

        /// <summary>
        /// Tries to update this person's <see cref="CommunicationRecipient.UnsubscribeLevel"/> if they're unsubscribing
        /// as a result of receiving a particular <see cref="Model.Communication"/>.
        /// </summary>
        /// <param name="person">The <see cref="Person"/> who received the <see cref="Model.Communication"/>.</param>
        /// <param name="originalEmailPreference">The person's <see cref="EmailPreference"/> at the time of block initialization.</param>
        /// <param name="communication">The <see cref="Model.Communication"/> they received.</param>
        /// <param name="unsubscribeLevel">The <see cref="UnsubscribeLevel"/> that should be set.</param>
        private void TryUpdateCommunicationRecipientUnsubscribeLevel(
            Person person,
            EmailPreference originalEmailPreference,
            Model.Communication communication,
            UnsubscribeLevel unsubscribeLevel )
        {
            if ( person == null || communication == null )
            {
                return;
            }

            var wasPersonEmailPreferenceAlreadySet = (
                    unsubscribeLevel == UnsubscribeLevel.All
                    && originalEmailPreference == EmailPreference.DoNotEmail
                )
                || (
                    unsubscribeLevel == UnsubscribeLevel.Bulk
                    && originalEmailPreference == EmailPreference.NoMassEmails
                );

            if ( wasPersonEmailPreferenceAlreadySet )
            {
                // This person's email preference was previously set to match the corresponding unsubscribe level we're
                // attempting to assign to their recipient record here, so we cannot attribute this communication to the
                // reason the person's email preference was set to this value.
                return;
            }

            var personId = person.Id;
            var communicationId = communication.Id;

            var communicationRecipients = new CommunicationRecipientService( RockContext )
                .Queryable()
                .Where( cr =>
                    cr.CommunicationId == communicationId
                    && cr.PersonAliasId.HasValue
                    && cr.PersonAlias.PersonId == personId
                    && (
                        cr.UnsubscribeLevel != unsubscribeLevel
                        || !cr.UnsubscribeDateTime.HasValue
                    )
                )
                .ToList();

            if ( !communicationRecipients.Any() )
            {
                return;
            }

            var now = RockDateTime.Now;
            communicationRecipients.ForEach( cr =>
            {
                cr.UnsubscribeLevel = unsubscribeLevel;
                cr.UnsubscribeDateTime = now;
            } );

            RockContext.SaveChanges();
        }

        /// <summary>
        /// Tries to start a workflow when a person unsubscribes.
        /// </summary>
        /// <param name="args">The arguments to provide the unsubscribe details.</param>
        private void TryStartUnsubscribeWorkflow( UnsubscribeWorkflowArgs args )
        {
            Model.Workflow workflow = null;

            var workflowName = $"UnsubscribeNotice {args.Person.FullName}";

            if ( args.CommunicationChannelType == CommunicationChannelType.List )
            {
                var unsubscribeWorkflowGuid = GetAttributeValue( AttributeKey.UnsubscribeFromCommunicationListWorkflow ).AsGuidOrNull();
                if ( !unsubscribeWorkflowGuid.HasValue )
                {
                    return;
                }

                var workflowType = WorkflowTypeCache.Get( unsubscribeWorkflowGuid.Value );
                if ( workflowType == null || workflowType.IsActive == false )
                {
                    return;
                }

                workflow = Model.Workflow.Activate( workflowType, workflowName );
                workflow.SetAttributeValue(
                    WorkflowAttributeKey.CommunicationListIds,
                    IdHasher.Instance.GetId( args.ChannelIdKey )?.ToString()
                );
            }

            if ( workflow != null )
            {
                new WorkflowService( RockContext ).Process(
                    workflow,
                    args.Person,
                    out List<string> workflowErrors
                );
            }
        }

        /// <summary>
        /// Gets a list of <see cref="CommunicationChannelBag"/>s, one for each active <see cref="Model.Communication"/>
        /// list or <see cref="CommunicationFlow"/> this <paramref name="person"/> is currently subscribed to.
        /// </summary>
        /// <param name="person">The <see cref="Person"/> whose current communication channels to get.</param>
        /// <param name="skipMessageCounts">
        /// Whether to skip the retrieval of message counts the person has received per communication channel.
        /// </param>
        /// <returns>
        /// A list of <see cref="CommunicationChannelBag"/>s, one for each active <see cref="Model.Communication"/>
        /// list or <see cref="CommunicationFlow"/>.
        /// </returns>
        private List<CommunicationChannelBag> GetCurrentCommunicationChannels( Person person, bool skipMessageCounts = false )
        {
            DateTime? countMessagesSentOnOrAfterDateTime = null;

            if ( !skipMessageCounts && MessageCountWindowDays != 0 )
            {
                countMessagesSentOnOrAfterDateTime = RockDateTime.Today.AddDays( -MessageCountWindowDays );
            }

            var communicationChannels = new List<CommunicationChannelBag>();

            communicationChannels.AddRange( GetCurrentCommunicationLists( person, countMessagesSentOnOrAfterDateTime ) );

            if ( !GetAttributeValue( AttributeKey.ExcludeCommunicationFlows ).AsBoolean() )
            {
                communicationChannels.AddRange( GetCurrentCommunicationFlows( person, countMessagesSentOnOrAfterDateTime ) );
            }

            return communicationChannels
                .OrderBy( c => c.Name )
                .ToList();
        }

        /// <summary>
        /// Gets the active <see cref="Model.Communication"/> lists of which this <paramref name="person"/> is an active
        /// member, represented as a list of <see cref="CommunicationChannelBag"/>s.
        /// </summary>
        /// <param name="person">The <see cref="Person"/> whose current communication lists to get.</param>
        /// <param name="countMessagesSentOnOrAfterDateTime">
        /// If provided, get a count of messages this person has received - per list - on or after this <see cref="DateTime"/>.
        /// </param>
        /// <returns>
        /// A list of <see cref="CommunicationChannelBag"/>s, one for each active <see cref="Model.Communication"/> list.
        /// </returns>
        private IEnumerable<CommunicationChannelBag> GetCurrentCommunicationLists( Person person, DateTime? countMessagesSentOnOrAfterDateTime )
        {
            var communicationListGroupTypeId = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_COMMUNICATIONLIST.AsGuid() )?.Id ?? 0;

            // Get a list of all active communication lists where the person is an active member.
            var communicationListsQry = new GroupMemberService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( gm =>
                    gm.Group.IsActive
                    && gm.Group.IsPublic
                    && gm.Group.GroupTypeId == communicationListGroupTypeId
                    && gm.PersonId == person.Id
                    && gm.GroupMemberStatus == GroupMemberStatus.Active
                );

            var shouldFilterByCampus = GetAttributeValue( AttributeKey.FilterCommunicationListsByCampusContext ).AsBoolean()
                && !GetAttributeValue( AttributeKey.AlwaysIncludeSubscribedCommunicationLists ).AsBoolean()
                && ContextCampus != null;

            if ( shouldFilterByCampus )
            {
                communicationListsQry = communicationListsQry
                    .Where( gm =>
                        !gm.Group.CampusId.HasValue
                        || gm.Group.CampusId == ContextCampus.Id
                    );
            }

            // Materialize the list so we can perform additional filtering in memory.
            // If the person is in there more than once, prefer the IsLeader role.
            var communicationListItems = communicationListsQry
                .GroupBy( gm => gm.GroupId )
                .SelectMany( grouping =>
                    grouping
                        .OrderByDescending( gm => gm.GroupRole.IsLeader )
                        .ThenBy( gm => gm.Id )
                        .Take( 1 )
                )
                .Select( gm => new CommunicationListItem
                {
                    ListGroup = gm.Group,
                    GroupMemberCommunicationPreference = gm.CommunicationPreference,
                    PersonCommunicationPreference = gm.Person.CommunicationPreference
                } )
                .ToList();

            if ( !communicationListItems.Any() )
            {
                return Enumerable.Empty<CommunicationChannelBag>();
            }

            // Go ahead and load attributes for all list groups, as we'll need them for each list's public name, and
            // also when comparing against allowed categories.
            var listGroups = communicationListItems
                .Select( i => i.ListGroup )
                .ToList();

            listGroups.LoadAttributes( RockContext );

            List<CommunicationListItem> visibleCommunicationListItems;

            if ( AllowedCommunicationListCategoryGuids.Any() )
            {
                visibleCommunicationListItems = communicationListItems
                    .Where( i =>
                    {
                        var categoryGuid = i.ListGroup.GetAttributeValue( AttributeKey.Category ).AsGuidOrNull();
                        if ( categoryGuid.HasValue && AllowedCommunicationListCategoryGuids.Contains( categoryGuid.Value ) )
                        {
                            return true;
                        }

                        return false;
                    } )
                    .ToList();
            }
            else
            {
                visibleCommunicationListItems = communicationListItems
                    .Where( i => i.ListGroup.IsAuthorized( Authorization.VIEW, person ) )
                    .ToList();
            }

            if ( !visibleCommunicationListItems.Any() )
            {
                return Enumerable.Empty<CommunicationChannelBag>();
            }

            Dictionary<int, int> messageCountsByListId = null;
            if ( countMessagesSentOnOrAfterDateTime.HasValue )
            {
                // Get the count of messages this person was sent - grouped by list - within the specified window.
                // They're highly unlikely to belong to enough lists for a SQL `WHERE...IN` clause to become problematic.
                var visibleListGroupIds = visibleCommunicationListItems.Select( l => l.ListGroup.Id ).ToList();
                messageCountsByListId = new CommunicationRecipientService( RockContext )
                    .Queryable()
                    .Where( cr =>
                        cr.Communication.ListGroupId.HasValue
                        && visibleListGroupIds.Contains( cr.Communication.ListGroupId.Value )
                        && cr.PersonAliasId.HasValue
                        && cr.PersonAlias.PersonId == person.Id
                        && cr.SendDateTime >= countMessagesSentOnOrAfterDateTime
                    )
                    .GroupBy( cr => cr.Communication.ListGroupId.Value )
                    .ToDictionary(
                        grouping => grouping.Key,
                        grouping => grouping.Count()
                    );
            }

            return visibleCommunicationListItems.Select( i =>
            {
                var name = i.ListGroup.GetAttributeValue( AttributeKey.PublicName );
                if ( name.IsNullOrWhiteSpace() )
                {
                    name = i.ListGroup.Name;
                }

                int? messagesReceivedCount = null;
                if ( messageCountsByListId != null )
                {
                    if ( messageCountsByListId.TryGetValue( i.ListGroup.Id, out var messageCount ) )
                    {
                        messagesReceivedCount = messageCount;
                    }
                }

                CommunicationType? communicationPreference = null;
                if ( ShowMediumPreference )
                {
                    communicationPreference = i.PersonCommunicationPreference == Model.CommunicationType.SMS
                        ? CommunicationType.SMS
                        : CommunicationType.Email;

                    if ( i.GroupMemberCommunicationPreference == Model.CommunicationType.SMS
                        || i.GroupMemberCommunicationPreference == Model.CommunicationType.Email )
                    {
                        communicationPreference = ( CommunicationType ) i.GroupMemberCommunicationPreference;
                    }
                }

                return new CommunicationChannelBag
                {
                    IdKey = i.ListGroup.Id.AsIdKey(),
                    CommunicationChannelType = CommunicationChannelType.List,
                    Name = name,
                    MessagesReceivedCount = messagesReceivedCount,
                    CommunicationPreference = communicationPreference
                };
            } );
        }

        /// <summary>
        /// Gets the active <see cref="CommunicationFlow"/>s to which this <paramref name="person"/> is subscribed,
        /// represented as a list of <see cref="CommunicationChannelBag"/>s.
        /// </summary>
        /// <param name="person">The <see cref="Person"/> whose current communication flows to get.</param>
        /// <param name="countMessagesSentOnOrAfterDateTime">
        /// If provided, get a count of messages this person has received - per flow - on or after this <see cref="DateTime"/>.
        /// </param>
        /// <returns>
        /// A list of <see cref="CommunicationChannelBag"/>s, one for each active <see cref="CommunicationFlow"/>.
        /// </returns>
        private IEnumerable<CommunicationChannelBag> GetCurrentCommunicationFlows( Person person, DateTime? countMessagesSentOnOrAfterDateTime )
        {
            // Query all active flows' communications where the person is an active recipient.
            var flowCommunicationsQry = new CommunicationFlowService( RockContext )
                .Queryable()
                .Where( f =>
                    f.IsActive
                    && (
                        !AllowedCommunicationFlowCategoryIds.Any()
                        || (
                            // We're most likely dealing with a small list of categories, so a SQL `WHERE...IN` clause
                            // shouldn't be problematic here.
                            f.CategoryId.HasValue
                            && AllowedCommunicationFlowCategoryIds.Contains( f.CategoryId.Value )
                        )
                    )
                    && f.CommunicationFlowInstances.Any( i =>
                        i.CommunicationFlowInstanceRecipients.Any( r =>
                            r.Status == CommunicationFlowInstanceRecipientStatus.Active
                            && r.RecipientPersonAlias.PersonId == person.Id
                        )
                    )
                )
                .SelectMany( f =>
                    f.CommunicationFlowInstances.SelectMany( i =>
                        i.CommunicationFlowInstanceCommunications.Select( c => new
                        {
                            f.Id,
                            f.Name,
                            f.PublicName,
                            c.CommunicationId
                        } )
                    )
                );

            IEnumerable<CommunicationFlowItem> communicationFlowItems;

            if ( countMessagesSentOnOrAfterDateTime.HasValue )
            {
                // Get the count of messages this person was sent - grouped by flow - within the specified window.
                // The following produces a LEFT OUTER JOIN (to include flows for which they haven't yet received any communications).
                var communicationRecipientQry = new CommunicationRecipientService( RockContext )
                    .Queryable()
                    .Where( cr =>
                        cr.PersonAliasId.HasValue
                        && cr.PersonAlias.PersonId == person.Id
                        && cr.SendDateTime >= countMessagesSentOnOrAfterDateTime
                    );

                communicationFlowItems = flowCommunicationsQry
                    .GroupJoin(
                        communicationRecipientQry,
                        fc => fc.CommunicationId,
                        cr => cr.CommunicationId,
                        ( fc, crs ) => new
                        {
                            FlowCommunication = fc,
                            Recipients = crs
                        }
                    )
                    .SelectMany(
                        flowWithRecipients => flowWithRecipients.Recipients.DefaultIfEmpty(),
                        ( flowWithRecipients, cr ) => new
                        {
                            flowWithRecipients.FlowCommunication,
                            CommunicationRecipientId = cr != null ? cr.Id : ( int? ) null
                        }
                    )
                    .GroupBy( flowCommRecipientRow => new
                    {
                        flowCommRecipientRow.FlowCommunication.Id,
                        flowCommRecipientRow.FlowCommunication.Name,
                        flowCommRecipientRow.FlowCommunication.PublicName
                    } )
                    .Select( grouping => new CommunicationFlowItem
                    {
                        CommunicationFlowId = grouping.Key.Id,
                        Name = grouping.Key.Name,
                        PublicName = grouping.Key.PublicName,
                        MessagesReceivedCount = grouping.Count( g => g.CommunicationRecipientId.HasValue )
                    } );
            }
            else
            {
                // Return the unique list of flows.
                communicationFlowItems = flowCommunicationsQry
                    .GroupBy( fc => new
                    {
                        fc.Id,
                        fc.Name,
                        fc.PublicName
                    } )
                    .Select( grouping => new CommunicationFlowItem
                    {
                        CommunicationFlowId = grouping.Key.Id,
                        Name = grouping.Key.Name,
                        PublicName = grouping.Key.PublicName
                    } );
            }

            return communicationFlowItems.Select( i =>
            {
                var name = i.PublicName.IsNotNullOrWhiteSpace()
                    ? i.PublicName
                    : i.Name;

                return new CommunicationChannelBag
                {
                    IdKey = i.CommunicationFlowId.AsIdKey(),
                    CommunicationChannelType = CommunicationChannelType.Flow,
                    Name = name,
                    MessagesReceivedCount = i.MessagesReceivedCount
                };
            } );
        }

        /// <summary>
        /// Gets a list of <see cref="CommunicationChannelBag"/>s, one for each available <see cref="Model.Communication"/>
        /// list or <see cref="CommunicationFlow"/> this <paramref name="person"/> may subscribe to.
        /// </summary>
        /// <param name="person">The <see cref="Person"/> whose available communication channels to get.</param>
        /// <returns>
        /// A list of <see cref="CommunicationChannelBag"/>s, one for each available <see cref="Model.Communication"/>
        /// list or <see cref="CommunicationFlow"/>.
        /// </returns>
        private List<CommunicationChannelBag> GetAvailableCommunicationChannels( Person person )
        {
            var communicationChannels = new List<CommunicationChannelBag>();

            communicationChannels.AddRange( GetAvailableCommunicationLists( person ) );

            if ( !GetAttributeValue( AttributeKey.ExcludeCommunicationFlows ).AsBoolean() )
            {
                communicationChannels.AddRange( GetAvailableCommunicationFlows( person ) );
            }

            return communicationChannels
                .OrderBy( c => c.Name )
                .ToList();
        }

        /// <summary>
        /// Gets the available <see cref="Model.Communication"/> lists to which this <paramref name="person"/> may
        /// subscribe, represented as a list of <see cref="CommunicationChannelBag"/>s.
        /// </summary>
        /// <param name="person">The <see cref="Person"/></param> whose available communication lists to get.
        /// <returns>
        /// A list of <see cref="CommunicationChannelBag"/>s, one for each available <see cref="Model.Communication"/> list.
        /// </returns>
        private IEnumerable<CommunicationChannelBag> GetAvailableCommunicationLists( Person person )
        {
            var communicationListGroupTypeId = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_COMMUNICATIONLIST.AsGuid() )?.Id ?? 0;

            // Get a list of all active communication lists where the person is not an active member.
            var communicationListsQry = new GroupService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( g =>
                    g.IsActive
                    && g.IsPublic
                    && g.GroupTypeId == communicationListGroupTypeId
                    && !g.Members.Any( gm =>
                        gm.GroupMemberStatus == GroupMemberStatus.Active
                        && gm.PersonId == person.Id
                    )
                );

            var shouldFilterByCampus = GetAttributeValue( AttributeKey.FilterCommunicationListsByCampusContext ).AsBoolean()
                && ContextCampus != null;

            if ( shouldFilterByCampus )
            {
                communicationListsQry = communicationListsQry
                    .Where( g =>
                        !g.CampusId.HasValue
                        || g.CampusId == ContextCampus.Id
                    );
            }

            // Materialize the list so we can perform additional filtering in memory.
            var listGroups = communicationListsQry.ToList();

            // Go ahead and load attributes for all list groups, as we'll need them for each list's public name, and
            // also when comparing against allowed categories.
            listGroups.LoadAttributes( RockContext );

            List<Model.Group> visibleListGroups;

            if ( AllowedCommunicationListCategoryGuids.Any() )
            {
                visibleListGroups = listGroups
                    .Where( g =>
                    {
                        var categoryGuid = g.GetAttributeValue( AttributeKey.Category ).AsGuidOrNull();
                        if ( categoryGuid.HasValue && AllowedCommunicationListCategoryGuids.Contains( categoryGuid.Value ) )
                        {
                            return true;
                        }

                        return false;
                    } )
                    .ToList();
            }
            else
            {
                visibleListGroups = listGroups
                    .Where( g => g.IsAuthorized( Authorization.VIEW, person ) )
                    .ToList();
            }

            if ( !visibleListGroups.Any() )
            {
                return Enumerable.Empty<CommunicationChannelBag>();
            }

            // Get past communication preferences from the active communication list groups this person has previously
            // unsubscribed from, so the UI will accurately represent this preference if they resubscribe. If the person
            // is in there more than once, prefer the IsLeader role.
            var unsubscribedGroupMemberPreferences = new GroupMemberService( RockContext )
                .Queryable()
                .Where( gm =>
                    gm.Group.IsActive
                    && gm.Group.GroupTypeId == communicationListGroupTypeId
                    && gm.PersonId == person.Id
                    && gm.GroupMemberStatus == GroupMemberStatus.Inactive
                )
                .GroupBy( gm => gm.GroupId )
                .SelectMany( grouping =>
                    grouping
                        .OrderByDescending( gm => gm.GroupRole.IsLeader )
                        .ThenBy( gm => gm.Id )
                        .Take( 1 )
                )
                .Select( gm => new
                {
                    gm.GroupId,
                    gm.CommunicationPreference
                } )
                .ToList();

            return visibleListGroups
                .Select( g =>
                {
                    var unsubscribedGroupMemberPreference = unsubscribedGroupMemberPreferences
                        .FirstOrDefault( p => p.GroupId == g.Id );

                    return new
                    {
                        Group = g,
                        IsUnsubscribed = unsubscribedGroupMemberPreference != null,
                        UnsubscribedPreference = unsubscribedGroupMemberPreference?.CommunicationPreference
                    };
                } )
                .Where( listGroupItem => ShowAllAvailableChannels || listGroupItem.IsUnsubscribed )
                .Select( listGroupItem =>
                {
                    var name = listGroupItem.Group.GetAttributeValue( AttributeKey.PublicName );
                    if ( name.IsNullOrWhiteSpace() )
                    {
                        name = listGroupItem.Group.Name;
                    }

                    CommunicationType? communicationPreference = null;
                    if ( ShowMediumPreference )
                    {
                        communicationPreference = person.CommunicationPreference == Model.CommunicationType.SMS
                            ? CommunicationType.SMS
                            : CommunicationType.Email;

                        if ( listGroupItem.UnsubscribedPreference == Model.CommunicationType.SMS
                            || listGroupItem.UnsubscribedPreference == Model.CommunicationType.Email )
                        {
                            communicationPreference = ( CommunicationType ) listGroupItem.UnsubscribedPreference;
                        }
                    }

                    return new CommunicationChannelBag
                    {
                        IdKey = listGroupItem.Group.Id.AsIdKey(),
                        CommunicationChannelType = CommunicationChannelType.List,
                        Name = name,
                        IsUnsubscribed = listGroupItem.IsUnsubscribed,
                        CommunicationPreference = communicationPreference
                    };
                } );
        }

        /// <summary>
        /// Gets the available <see cref="CommunicationFlow"/>s to which this <paramref name="person"/> may subscribe,
        /// represented as a list of <see cref="CommunicationChannelBag"/>s.
        /// </summary>
        /// <param name="person">The <see cref="Person"/> whose available communication flows to get.</param>
        /// <returns>
        /// A list of <see cref="CommunicationChannelBag"/>s, one for each available <see cref="CommunicationFlow"/>.
        /// </returns>
        private IEnumerable<CommunicationChannelBag> GetAvailableCommunicationFlows( Person person )
        {
            // Get all unique, active flows where the person is unsubscribed.
            return new CommunicationFlowService( RockContext )
                .Queryable()
                .Where( f =>
                    f.IsActive
                    && (
                        !AllowedCommunicationFlowCategoryIds.Any()
                        || (
                            // We're most likely dealing with a small list of categories, so a SQL `WHERE...IN` clause
                            // shouldn't be problematic here.
                            f.CategoryId.HasValue
                            && AllowedCommunicationFlowCategoryIds.Contains( f.CategoryId.Value )
                        )
                    )
                    && f.CommunicationFlowInstances.Any( i =>
                        i.CommunicationFlowInstanceRecipients.Any( r =>
                            r.Status == CommunicationFlowInstanceRecipientStatus.Inactive
                            && r.InactiveReason == CommunicationFlowInstanceRecipientInactiveReason.UnsubscribedFromFlow
                            && r.RecipientPersonAlias.PersonId == person.Id
                        )
                    )
                )
                .ToList() // Materialize the list so we determine the final return values in-memory (name + IdKey).
                .Select( f =>
                {
                    var name = f.PublicName.IsNotNullOrWhiteSpace()
                        ? f.PublicName
                        : f.Name;

                    return new CommunicationChannelBag
                    {
                        IdKey = f.Id.AsIdKey(),
                        CommunicationChannelType = CommunicationChannelType.Flow,
                        Name = name,
                        IsUnsubscribed = true
                    };
                } );
        }

        /// <summary>
        /// Sets the UI display values on the initialization box.
        /// </summary>
        /// <param name="box">The initialization box on which to set the text values.</param>
        /// <param name="unsubscribeLevel">The level at which this person just unsubscribed.</param>
        /// <param name="mergeFields">The merge fields available to the Lava templates used to generate text values.</param>
        private void SetInitializationBoxDisplayValues(
            EmailPreferenceEntryInitializationBox box,
            UnsubscribeLevel? unsubscribeLevel,
            Dictionary<string, object> mergeFields )
        {
            box.IsUnsubscribeMode = IsUnsubscribeMode;
            box.ShowHeaderIcon = GetAttributeValue( AttributeKey.ShowHeaderIcon ).AsBoolean();

            if ( box.IsRemovedFromChurchInvolvement )
            {
                box.HeaderTitle = GetAttributeValue( AttributeKey.PersonRemovedInvolvementSuccessTitle );
                box.HeaderDescriptionHtml = GetAttributeValue( AttributeKey.PersonRemovedInvolvementSuccessDescription )
                    .ResolveMergeFields( mergeFields );

                return;
            }

            box.OptOutOptionsDescriptionHtml = GetAttributeValue( AttributeKey.OptOutOptionsDescription )
                .ResolveMergeFields( mergeFields );

            box.EmailPreferenceTitle = GetAttributeValue( AttributeKey.EmailPreferenceTitle );
            box.EmailPreferenceDescription = GetAttributeValue( AttributeKey.EmailPreferenceDescription );

            if ( box.IsRemoveInvolvementEnabled )
            {
                box.RemoveInvolvementTitle = GetAttributeValue( AttributeKey.RemoveInvolvementTitle );
                box.RemoveInvolvementDescription = GetAttributeValue( AttributeKey.RemoveInvolvementDescription );
                box.RemoveInvolvementConfirmationTitle = GetAttributeValue( AttributeKey.RemoveInvolvementConfirmationTitle );
                box.RemoveInvolvementConfirmationDescriptionHtml = GetAttributeValue( AttributeKey.RemoveInvolvementConfirmationDescription )
                    .ResolveMergeFields( mergeFields );
            }

            if ( box.IsCurrentCommunicationChannelsSectionEnabled )
            {
                box.CurrentCommunicationChannelsDescriptionHtml = GetAttributeValue( AttributeKey.CurrentCommunicationChannelsDescription )
                    .ResolveMergeFields( mergeFields );
            }

            if ( box.IsAvailableCommunicationChannelsSectionEnabled )
            {
                box.AvailableCommunicationChannelsDescriptionHtml = GetAttributeValue( AttributeKey.AvailableCommunicationChannelsDescription )
                    .ResolveMergeFields( mergeFields );
            }

            var preferenceModeHeaderTitle = GetAttributeValue( AttributeKey.PreferenceModeHeaderTitle );
            var preferenceModeHeaderDescriptionHtml = GetAttributeValue( AttributeKey.PreferenceModeHeaderDescription )
                    .ResolveMergeFields( mergeFields );

            if ( !IsUnsubscribeMode )
            {
                box.HeaderTitle = preferenceModeHeaderTitle;
                box.HeaderDescriptionHtml = preferenceModeHeaderDescriptionHtml;

                return;
            }

            box.ResubscribedHeaderTitle = preferenceModeHeaderTitle;
            box.ResubscribedHeaderDescriptionHtml = preferenceModeHeaderDescriptionHtml;

            string titleKey = null;
            string descriptionKey = null;

            switch ( unsubscribeLevel )
            {
                case UnsubscribeLevel.All:
                    titleKey = AttributeKey.UnsubscribedFromAllEmailsConfirmationTitle;
                    descriptionKey = AttributeKey.UnsubscribedFromAllEmailsConfirmationDescription;
                    break;
                case UnsubscribeLevel.Bulk:
                    titleKey = AttributeKey.UnsubscribedFromBulkEmailConfirmationTitle;
                    descriptionKey = AttributeKey.UnsubscribedFromBulkEmailConfirmationDescription;
                    break;
                case UnsubscribeLevel.CommunicationList:
                case UnsubscribeLevel.Flows:
                    titleKey = AttributeKey.UnsubscribedFromChannelConfirmationTitle;
                    descriptionKey = AttributeKey.UnsubscribedFromChannelConfirmationDescription;
                    break;
            }

            if ( titleKey.IsNotNullOrWhiteSpace() && descriptionKey.IsNotNullOrWhiteSpace() )
            {
                box.HeaderTitle = GetAttributeValue( titleKey );
                box.HeaderDescriptionHtml = GetAttributeValue( descriptionKey ).ResolveMergeFields( mergeFields );
            }
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var urls = new Dictionary<string, string>();

            if ( GetAttributeValue( AttributeKey.ManageMyAccountPage ).IsNotNullOrWhiteSpace() )
            {
                urls[NavigationUrlKey.ManageMyAccountPage] = this.GetLinkedPageUrl( AttributeKey.ManageMyAccountPage );
            }

            return urls;
        }

        #endregion Private Methods

        #region Supporting Members

        /// <summary>
        /// A POCO to represent the communication channel being unsubscribed from at the time of block initialization
        /// that will be passed to Lava templates that are used when resolving text values displayed to the person.
        /// </summary>
        /// <seealso cref="Rock.Utility.RockDynamic" />
        private class CommunicationChannel : RockDynamic
        {
            /// <summary>
            /// Gets or sets the name of this communication channel.
            /// </summary>
            public string Name { get; set; }
        }

        /// <summary>
        /// A POCO to represent a communication channel with an underlying list <see cref="Model.Group"/>.
        /// </summary>
        private class CommunicationListItem
        {
            /// <summary>
            /// Gets or sets the <see cref="Model.Group"/> that represents this list.
            /// </summary>
            public Model.Group ListGroup { get; set; }

            /// <summary>
            /// Gets or sets the <see cref="Person"/>'s <see cref="GroupMember"/>'s communication preference.
            /// </summary>
            public Model.CommunicationType GroupMemberCommunicationPreference { get; set; }

            /// <summary>
            /// Gets or sets the <see cref="Person"/>'s communication preference.
            /// </summary>
            public Model.CommunicationType PersonCommunicationPreference { get; set; }
        }

        /// <summary>
        /// A POCO to represent a communication channel with an underlying <see cref="CommunicationFlow"/>.
        /// </summary>
        private class CommunicationFlowItem
        {
            /// <summary>
            /// Gets or sets the <see cref="CommunicationFlow"/> identifier.
            /// </summary>
            public int CommunicationFlowId { get; set; }

            /// <summary>
            /// Gets or sets the <see cref="CommunicationFlow.Name"/>.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the <see cref="CommunicationFlow.PublicName"/>.
            /// </summary>
            public string PublicName { get; set; }

            /// <summary>
            /// Gets or sets the count of messages the <see cref="Person"/> has received for this flow.
            /// </summary>
            public int MessagesReceivedCount { get; set; }
        }

        /// <summary>
        /// A POCO to pass arguments to an unsubscribe workflow.
        /// </summary>
        private class UnsubscribeWorkflowArgs
        {
            /// <summary>
            /// Gets or sets the hashed identifier key for the communication channel being unsubscribed from.
            /// </summary>
            /// <remarks>
            /// The type of entity can be inferred from the <see cref="CommunicationChannelType"/> property value.
            /// </remarks>
            public string ChannelIdKey { get; set; }

            /// <summary>
            /// Gets or sets the type of communication channel being unsubscribed from.
            /// </summary>
            public CommunicationChannelType? CommunicationChannelType { get; set; }

            /// <summary>
            /// Gets or sets the <see cref="Model.Person"/> who's unsubscribing.
            /// </summary>
            public Person Person { get; set; }
        }

        #endregion Supporting Members
    }
}
