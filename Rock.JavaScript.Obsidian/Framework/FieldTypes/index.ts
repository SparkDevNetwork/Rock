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
import { FieldType as FieldTypeGuids } from "@Obsidian/SystemGuids/fieldType";
import { registerFieldType } from "@Obsidian/Utility/fieldTypes";

export { ConfigurationValues, getFieldEditorProps } from "./utils";

/*
 * Define the standard field types in Rock.
 */

import { AchievementTypeFieldType } from "./achievementTypeField.partial";
registerFieldType(FieldTypeGuids.AchievementType, new AchievementTypeFieldType());

import { AddressFieldType } from "./addressField.partial";
registerFieldType(FieldTypeGuids.Address, new AddressFieldType());

import { AssessmentTypesFieldType } from "./assessmentTypesField.partial";
registerFieldType(FieldTypeGuids.AssessmentType, new AssessmentTypesFieldType());

import { AssetStorageProviderFieldType } from "./assetStorageProviderField.partial";
registerFieldType(FieldTypeGuids.AssetStorageProvider, new AssetStorageProviderFieldType());

import { AttributeFieldType } from "./attributeField.partial";
registerFieldType(FieldTypeGuids.Attribute, new AttributeFieldType());

import { AudioFileFieldType } from "./audioFileField.partial";
registerFieldType(FieldTypeGuids.AudioFile, new AudioFileFieldType());

import { AudioUrlFieldType } from "./audioUrlField.partial";
registerFieldType(FieldTypeGuids.AudioUrl, new AudioUrlFieldType());

import { BadgesFieldType } from "./badgesField.partial";
registerFieldType(FieldTypeGuids.Badges, new BadgesFieldType());

import { BackgroundCheckFieldType } from "./backgroundCheckField.partial";
registerFieldType(FieldTypeGuids.Backgroundcheck, new BackgroundCheckFieldType());

import { BenevolenceRequestFieldType } from "./benevolenceRequestField.partial";
registerFieldType(FieldTypeGuids.BenevolenceRequest, new BenevolenceRequestFieldType());

import { BinaryFileFieldType } from "./binaryFileField.partial";
registerFieldType(FieldTypeGuids.BinaryFile, new BinaryFileFieldType());

import { BinaryFileTypeFieldType } from "./binaryFileTypeField.partial";
registerFieldType(FieldTypeGuids.BinaryFileType, new BinaryFileTypeFieldType());

import { BinaryFileTypesFieldType } from "./binaryFileTypesField.partial";
registerFieldType(FieldTypeGuids.BinaryFileTypes, new BinaryFileTypesFieldType());

import { BlockTemplateFieldType } from "./blockTemplateField.partial";
registerFieldType(FieldTypeGuids.BlockTemplate, new BlockTemplateFieldType());

import { BooleanFieldType } from "./booleanField.partial";
registerFieldType(FieldTypeGuids.Boolean, new BooleanFieldType());

import { CampusFieldType } from "./campusField.partial";
registerFieldType(FieldTypeGuids.Campus, new CampusFieldType());

import { CampusesFieldType } from "./campusesField.partial";
registerFieldType(FieldTypeGuids.Campuses, new CampusesFieldType());

import { CategoriesFieldType } from "./categoriesField.partial";
registerFieldType(FieldTypeGuids.Categories, new CategoriesFieldType());

import { CategoryFieldType } from "./categoryField.partial";
registerFieldType(FieldTypeGuids.Category, new CategoryFieldType());

import { CategorizedDefinedValueField } from "./categorizedDefinedValueField.partial";
registerFieldType(FieldTypeGuids.DefinedValueCategorized, new CategorizedDefinedValueField());

import { CheckListFieldType } from "./checkListField.partial";
registerFieldType(FieldTypeGuids.CheckList, new CheckListFieldType());

import { CodeEditorFieldType } from "./codeEditorField.partial";
registerFieldType(FieldTypeGuids.CodeEditor, new CodeEditorFieldType());

import { ColorFieldType } from "./colorField.partial";
registerFieldType(FieldTypeGuids.Color, new ColorFieldType());

import { ColorSelectorFieldType } from "./colorSelectorField.partial";
registerFieldType(FieldTypeGuids.ColorSelector, new ColorSelectorFieldType());

import { ConnectionOpportunityFieldType } from "./connectionOpportunityField.partial";
registerFieldType(FieldTypeGuids.ConnectionOpportunity, new ConnectionOpportunityFieldType());

import { ConnectionTypeField } from "./connectionTypeField.partial";
registerFieldType(FieldTypeGuids.ConnectionType, new ConnectionTypeField());

import { ConnectionTypesField } from "./connectionTypesField.partial";
registerFieldType(FieldTypeGuids.ConnectionTypes, new ConnectionTypesField());

import { ConnectionStateFieldType } from "./connectionStateField.partial";
registerFieldType(FieldTypeGuids.ConnectionState, new ConnectionStateFieldType());

import { ConnectionStatusFieldType } from "./connectionStatusField.partial";
registerFieldType(FieldTypeGuids.ConnectionStatus, new ConnectionStatusFieldType());

import { ContentChannelFieldType } from "./contentChannelField.partial";
registerFieldType(FieldTypeGuids.ContentChannel, new ContentChannelFieldType());

import { ContentChannelItemFieldType } from "./contentChannelItemField.partial";
registerFieldType(FieldTypeGuids.ContentChannelItem, new ContentChannelItemFieldType());

import { ContentChannelTypeFieldType } from "./contentChannelTypeField.partial";
registerFieldType(FieldTypeGuids.ContentChannelType, new ContentChannelTypeFieldType());

import { ContentChannelTypesFieldType } from "./contentChannelTypesField.partial";
registerFieldType(FieldTypeGuids.ContentChannelTypes, new ContentChannelTypesFieldType());

import { ContentChannelsFieldType } from "./contentChannelsField.partial";
registerFieldType(FieldTypeGuids.ContentChannels, new ContentChannelsFieldType());

import { ConnectionActivityTypeField } from "./connectionActivityTypeField.partial";
registerFieldType(FieldTypeGuids.ConnectionActivityType, new ConnectionActivityTypeField());

import { ConnectionRequestFieldType } from "./connectionRequestField.partial";
registerFieldType(FieldTypeGuids.ConnectionRequest, new ConnectionRequestFieldType());

import { ConnectionRequestActivityFieldType } from "./connectionRequestActivityField.partial";
registerFieldType(FieldTypeGuids.ConnectionRequestActivity, new ConnectionRequestActivityFieldType());

import { CommunicationPreferenceField } from "./communicationPreferenceField.partial";
registerFieldType(FieldTypeGuids.CommunicationPreferenceType, new CommunicationPreferenceField());

import { CommunicationTemplateFieldType } from "./communicationTemplateField.partial";
registerFieldType(FieldTypeGuids.CommunicationTemplate, new CommunicationTemplateFieldType());

import { ComparisonFieldType } from "./comparisonField.partial";
registerFieldType(FieldTypeGuids.Comparison, new ComparisonFieldType());

import { ComponentsFieldType } from "./componentsField.partial";
registerFieldType(FieldTypeGuids.Components, new ComponentsFieldType());

import { ComponentFieldType } from "./componentField.partial";
registerFieldType(FieldTypeGuids.Component, new ComponentFieldType());

import { CurrencyFieldType } from "./currencyField.partial";
registerFieldType(FieldTypeGuids.Currency, new CurrencyFieldType());

import { DataEntryRequirementLevelFieldType } from "./dataEntryRequirementLevelField.partial";
registerFieldType(FieldTypeGuids.DataEntryRequirementLevel, new DataEntryRequirementLevelFieldType());

import { DataViewFieldType } from "./dataViewField.partial";
registerFieldType(FieldTypeGuids.DataView, new DataViewFieldType());

import { DataViewsFieldType } from "./dataViewsField.partial";
registerFieldType(FieldTypeGuids.Dataviews, new DataViewsFieldType());

import { DateFieldType } from "./dateField.partial";
registerFieldType(FieldTypeGuids.Date, new DateFieldType());

import { DateRangeFieldType } from "./dateRangeField.partial";
registerFieldType(FieldTypeGuids.DateRange, new DateRangeFieldType());

import { DateTimeFieldType } from "./dateTimeField.partial";
registerFieldType(FieldTypeGuids.DateTime, new DateTimeFieldType());

import { DayOfWeekFieldType } from "./dayOfWeekField.partial";
registerFieldType(FieldTypeGuids.DayOfWeek, new DayOfWeekFieldType());

import { DaysOfWeekFieldType } from "./daysOfWeekField.partial";
registerFieldType(FieldTypeGuids.DaysOfWeek, new DaysOfWeekFieldType());

import { DecimalFieldType } from "./decimalField.partial";
registerFieldType(FieldTypeGuids.Decimal, new DecimalFieldType());

import { DecimalRangeFieldType } from "./decimalRangeField.partial";
registerFieldType(FieldTypeGuids.DecimalRange, new DecimalRangeFieldType());

import { DefinedTypeFieldType } from "./definedTypeField.partial";
registerFieldType(FieldTypeGuids.DefinedType, new DefinedTypeFieldType());

import { DefinedValueFieldType } from "./definedValueField.partial";
registerFieldType(FieldTypeGuids.DefinedValue, new DefinedValueFieldType());

import { DefinedValueRangeFieldType } from "./definedValueRangeField.partial";
registerFieldType(FieldTypeGuids.DefinedValueRange, new DefinedValueRangeFieldType());

import { DocumentTypeFieldType } from "./documentTypeField.partial";
registerFieldType(FieldTypeGuids.DocumentType, new DocumentTypeFieldType());

import { EmailFieldType } from "./emailField.partial";
registerFieldType(FieldTypeGuids.Email, new EmailFieldType());

import { EncryptedTextFieldType } from "./encryptedTextField.partial";
registerFieldType(FieldTypeGuids.EncryptedText, new EncryptedTextFieldType());

import { EntityFieldType } from "./entityField.partial";
registerFieldType(FieldTypeGuids.Entity, new EntityFieldType());

import { EntityTypeFieldType } from "./entityTypeField.partial";
registerFieldType(FieldTypeGuids.Entitytype, new EntityTypeFieldType());

import { EventCalendarFieldType } from "./eventCalendarField.partial";
registerFieldType(FieldTypeGuids.EventCalendar, new EventCalendarFieldType());

import { EventItemFieldType } from "./eventItemField.partial";
registerFieldType(FieldTypeGuids.EventItem, new EventItemFieldType());

import { FinancialAccountFieldType  } from "./financialAccountField.partial";
registerFieldType(FieldTypeGuids.FinancialAccount, new FinancialAccountFieldType());

import { FinancialAccountsFieldType  } from "./financialAccountsField.partial";
registerFieldType(FieldTypeGuids.FinancialAccounts, new FinancialAccountsFieldType());

import { FinancialGatewayFieldType  } from "./financialGatewayField.partial";
registerFieldType(FieldTypeGuids.FinancialGateway, new FinancialGatewayFieldType());

import { FileFieldType } from "./fileField.partial";
registerFieldType(FieldTypeGuids.File, new FileFieldType());

import { GenderFieldType } from "./genderField.partial";
registerFieldType(FieldTypeGuids.Gender, new GenderFieldType());

import { GroupAndRoleFieldType } from "./groupAndRoleField.partial";
registerFieldType(FieldTypeGuids.GroupAndRole, new GroupAndRoleFieldType());

import { GroupFieldType } from "./groupField.partial";
registerFieldType(FieldTypeGuids.Group, new GroupFieldType());

import { GroupLocationTypeFieldType } from "./groupLocationTypeField.partial";
registerFieldType(FieldTypeGuids.GroupLocationType, new GroupLocationTypeFieldType());

import { GroupRoleFieldType } from "./groupRoleField.partial";
registerFieldType(FieldTypeGuids.GroupRole, new GroupRoleFieldType());

import { GroupMemberFieldType } from "./groupMemberField.partial";
registerFieldType(FieldTypeGuids.GroupMember, new GroupMemberFieldType());

import { GroupTypeField } from "./groupTypeField.partial";
registerFieldType(FieldTypeGuids.GroupType, new GroupTypeField());

import { GroupTypesFieldType } from "./groupTypesField.partial";
registerFieldType(FieldTypeGuids.GroupTypes, new GroupTypesFieldType());

import { GroupTypeGroupField } from "./groupTypeGroupField.partial";
registerFieldType(FieldTypeGuids.GroupTypeGroup, new GroupTypeGroupField());

import { ImageFieldType } from "./imageField.partial";
registerFieldType(FieldTypeGuids.Image, new ImageFieldType());

import { IntegerFieldType } from "./integerField.partial";
registerFieldType(FieldTypeGuids.Integer, new IntegerFieldType());

import { IntegerRangeFieldType } from "./integerRangeField.partial";
registerFieldType(FieldTypeGuids.IntegerRange, new IntegerRangeFieldType());

import { InteractionChannelFieldType } from "./interactionChannelField.partial";
registerFieldType(FieldTypeGuids.InteractionChannel, new InteractionChannelFieldType());

import { InteractionChannelsFieldType } from "./interactionChannelsField.partial";
registerFieldType(FieldTypeGuids.InteractionChannels, new InteractionChannelsFieldType());

import { InteractionChannelInteractionComponentFieldType } from "./interactionChannelInteractionComponentField.partial";
registerFieldType(FieldTypeGuids.InteractionChannelInteractionComponent, new InteractionChannelInteractionComponentFieldType());

import { KeyValueListFieldType } from "./keyValueListField.partial";
registerFieldType(FieldTypeGuids.KeyValueList, new KeyValueListFieldType());

import { LabelFieldType } from "./labelField.partial";
registerFieldType(FieldTypeGuids.Label, new LabelFieldType());

import { LavaFieldType } from "./lavaField.partial";
registerFieldType(FieldTypeGuids.Lava, new LavaFieldType());

import { LavaCommandsFieldType } from "./lavaCommandsField.partial";
registerFieldType(FieldTypeGuids.LavaCommands, new LavaCommandsFieldType());

import { LocationListFieldType } from "./locationListField.partial";
registerFieldType(FieldTypeGuids.LocationList, new LocationListFieldType());

import { MatrixFieldType } from "./matrixField.partial";
registerFieldType(FieldTypeGuids.Matrix, new MatrixFieldType());

import { MemoFieldType } from "./memoField.partial";
registerFieldType(FieldTypeGuids.Memo, new MemoFieldType());

import { MonthDayFieldType } from "./monthDayField.partial";
registerFieldType(FieldTypeGuids.MonthDay, new MonthDayFieldType());

import { MediaSelectorFieldType } from "./mediaSelectorField.partial";
registerFieldType(FieldTypeGuids.MediaSelector, new MediaSelectorFieldType());

import { MergeTemplateFieldType } from "./mergeTemplateField.partial";
registerFieldType(FieldTypeGuids.MergeTemplate, new MergeTemplateFieldType());

import { MetricCategoriesFieldType } from "./metricCategoriesField.partial";
registerFieldType(FieldTypeGuids.MetricCategories, new MetricCategoriesFieldType());

import { MetricFieldType } from "./metricField.partial";
registerFieldType(FieldTypeGuids.Metric, new MetricFieldType());

import { MultiSelectFieldType } from "./multiSelectField.partial";
registerFieldType(FieldTypeGuids.MultiSelect, new MultiSelectFieldType());

import { NoteTypeField } from "./noteTypeField.partial";
registerFieldType(FieldTypeGuids.NoteType, new NoteTypeField());

import { NoteTypesField } from "./noteTypesField.partial";
registerFieldType(FieldTypeGuids.NoteTypes, new NoteTypesField());

import { PersistedDatasetField } from "./persistedDatasetField.partial";
registerFieldType(FieldTypeGuids.PersistedDataset, new PersistedDatasetField());

import { PhoneNumberFieldType } from "./phoneNumberField.partial";
registerFieldType(FieldTypeGuids.PhoneNumber, new PhoneNumberFieldType());

import { PageReferenceFieldType } from "./pageReferenceField.partial";
registerFieldType(FieldTypeGuids.PageReference, new PageReferenceFieldType());

import { PersonFieldType } from "./personField.partial";
registerFieldType(FieldTypeGuids.Person, new PersonFieldType());

import { RangeSliderFieldType } from "./rangeSliderField.partial";
registerFieldType(FieldTypeGuids.RangeSlider, new RangeSliderFieldType());

import { RatingFieldType } from "./ratingField.partial";
registerFieldType(FieldTypeGuids.Rating, new RatingFieldType());

import { RegistrationTemplateField } from "./registrationTemplateField.partial";
registerFieldType(FieldTypeGuids.RegistrationTemplate, new RegistrationTemplateField());

import { RegistrationTemplatesField } from "./registrationTemplatesField.partial";
registerFieldType(FieldTypeGuids.RegistrationTemplates, new RegistrationTemplatesField());

import { RegistrationInstanceField } from "./registrationInstanceField.partial";
registerFieldType(FieldTypeGuids.RegistrationInstance, new RegistrationInstanceField());

import { RegistryEntryFieldType } from "./registryEntryField.partial";
registerFieldType(FieldTypeGuids.RegistryEntry, new RegistryEntryFieldType());

import { ReminderTypeFieldType } from "./reminderTypeField.partial";
registerFieldType(FieldTypeGuids.ReminderType, new ReminderTypeFieldType());

import { ReminderTypesFieldType } from "./reminderTypesField.partial";
registerFieldType(FieldTypeGuids.ReminderTypes, new ReminderTypesFieldType());

import { RemoteAuthsFieldType } from "./remoteAuthsField.partial";
registerFieldType(FieldTypeGuids.RemoteAuths, new RemoteAuthsFieldType());

import { ReportFieldType } from "./reportField.partial";
registerFieldType(FieldTypeGuids.Report, new ReportFieldType());

import { ScheduleFieldType } from "./scheduleField.partial";
registerFieldType(FieldTypeGuids.Schedule, new ScheduleFieldType());

import { SchedulesFieldType } from "./schedulesField.partial";
registerFieldType(FieldTypeGuids.Schedules, new SchedulesFieldType());

import { SecurityRoleFieldType } from "./securityRoleField.partial";
registerFieldType(FieldTypeGuids.SecurityRole, new SecurityRoleFieldType());

import { SingleSelectFieldType } from "./singleSelectField.partial";
registerFieldType(FieldTypeGuids.SingleSelect, new SingleSelectFieldType());

import { SlidingDateRangeFieldType } from "./slidingDateRangeField.partial";
registerFieldType(FieldTypeGuids.SlidingDateRange, new SlidingDateRangeFieldType());

import { SocialMediaAccountFieldType } from "./socialMediaAccountField.partial";
registerFieldType(FieldTypeGuids.SocialMediaAccount, new SocialMediaAccountFieldType());

import { SiteFieldType } from "./siteField.partial";
registerFieldType(FieldTypeGuids.Site, new SiteFieldType());

import { StepProgramFieldType } from "./stepProgramField.partial";
registerFieldType(FieldTypeGuids.StepProgram, new StepProgramFieldType());

import { SSNFieldType } from "./ssnField.partial";
registerFieldType(FieldTypeGuids.Ssn, new SSNFieldType());

import { StepFieldType } from "./stepField.partial";
registerFieldType(FieldTypeGuids.Step, new StepFieldType());

import { StepProgramStepStatusFieldType } from "./stepProgramStepStatusField.partial";
registerFieldType(FieldTypeGuids.StepProgramStepStatus, new StepProgramStepStatusFieldType());

import { StepProgramStepTypeFieldType } from "./stepProgramStepTypeField.partial";
registerFieldType(FieldTypeGuids.StepProgramStepType, new StepProgramStepTypeFieldType());

import { StreakTypeFieldType } from "./streakTypeField.partial";
registerFieldType(FieldTypeGuids.StreakType, new StreakTypeFieldType());

import { StructureContentEditorFieldType } from "./structureContentEditorField.partial";
registerFieldType(FieldTypeGuids.StructureContentEditor, new StructureContentEditorFieldType());

import { SystemCommunicationFieldType } from "./systemCommunicationField.partial";
registerFieldType(FieldTypeGuids.SystemCommunication, new SystemCommunicationFieldType());

import { SystemEmailFieldType } from "./systemEmailField.partial";
registerFieldType(FieldTypeGuids.SystemEmail, new SystemEmailFieldType());

import { SystemPhoneNumberFieldType } from "./systemPhoneNumberField.partial";
registerFieldType(FieldTypeGuids.SystemPhoneNumber, new SystemPhoneNumberFieldType());

import { TextFieldType } from "./textField.partial";
registerFieldType(FieldTypeGuids.Text, new TextFieldType());

import { TimeFieldType } from "./timeField.partial";
registerFieldType(FieldTypeGuids.Time, new TimeFieldType());

import { TimeZoneFieldType } from "./timeZoneField.partial";
registerFieldType(FieldTypeGuids.TimeZone, new TimeZoneFieldType());

import { UniversalItemPickerFieldType } from "./universalItemPickerField.partial";
registerFieldType("b69b5a61-6fcd-4e3b-bb45-5f6802514953", new UniversalItemPickerFieldType());

import { UniversalItemSearchPickerFieldType } from "./universalItemSearchPickerField.partial";
registerFieldType("c5b32713-fb46-41c0-8bbc-9bd4142f841a", new UniversalItemSearchPickerFieldType());

import { UniversalItemTreePickerFieldType } from "./universalItemTreePickerField.partial";
registerFieldType("c7485f3f-0c10-4db6-9574-c10b195617e4", new UniversalItemTreePickerFieldType());

import { UrlLinkFieldType } from "./urlLinkField.partial";
registerFieldType(FieldTypeGuids.UrlLink, new UrlLinkFieldType());

import { ValueFilterFieldType } from "./valueFilterField.partial";
registerFieldType(FieldTypeGuids.ValueFilter, new ValueFilterFieldType());

import { ValueListFieldType } from "./valueListField.partial";
registerFieldType(FieldTypeGuids.ValueList, new ValueListFieldType());

import { VideoUrlFieldType } from "./videoUrlField.partial";
registerFieldType(FieldTypeGuids.VideoUrl, new VideoUrlFieldType());

import { VideoFileFieldType } from "./videoFileField.partial";
registerFieldType(FieldTypeGuids.VideoFile, new VideoFileFieldType());

import { WorkflowFieldType } from "./workflowField.partial";
registerFieldType(FieldTypeGuids.Workflow, new WorkflowFieldType());

import { WorkflowActivityFieldType } from "./workflowActivityField.partial";
registerFieldType(FieldTypeGuids.WorkflowActivity, new WorkflowActivityFieldType());

import { WorkflowAttributeFieldType } from "./workflowAttributeField.partial";
registerFieldType(FieldTypeGuids.WorkflowAttribute, new WorkflowAttributeFieldType());

import { WorkflowTypeFieldType } from "./workflowTypeField.partial";
registerFieldType(FieldTypeGuids.WorkflowType, new WorkflowTypeFieldType());

import { WorkflowTypesFieldType } from "./workflowTypesField.partial";
registerFieldType(FieldTypeGuids.WorkflowTypes, new WorkflowTypesFieldType());
