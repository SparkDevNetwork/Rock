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

import { AudioUrlFieldType } from "./audioUrlField.partial";
registerFieldType(FieldTypeGuids.AudioUrl, new AudioUrlFieldType());

import { BadgesFieldType } from "./badgesField.partial";
registerFieldType(FieldTypeGuids.Badges, new BadgesFieldType());

import { BackgroundCheckFieldType } from "./backgroundCheckField.partial";
registerFieldType(FieldTypeGuids.Backgroundcheck, new BackgroundCheckFieldType());

import { BinaryFileTypeFieldType } from "./binaryFileTypeField.partial";
registerFieldType(FieldTypeGuids.BinaryFileType, new BinaryFileTypeFieldType());

import { BinaryFileTypesFieldType } from "./binaryFileTypesField.partial";
registerFieldType(FieldTypeGuids.BinaryFileTypes, new BinaryFileTypesFieldType());

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

import { ColorFieldType } from "./colorField.partial";
registerFieldType(FieldTypeGuids.Color, new ColorFieldType());

import { ColorSelectorFieldType } from "./colorSelectorField.partial";
registerFieldType(FieldTypeGuids.ColorSelector, new ColorSelectorFieldType());

import { ConnectionTypeField } from "./connectionTypeField.partial";
registerFieldType(FieldTypeGuids.ConnectionType, new ConnectionTypeField());

import { ConnectionTypesField } from "./connectionTypesField.partial";
registerFieldType(FieldTypeGuids.ConnectionTypes, new ConnectionTypesField());

import { CommunicationPreferenceField } from "./communicationPreferenceField.partial";
registerFieldType(FieldTypeGuids.CommunicationPreferenceType, new CommunicationPreferenceField());

import { ComponentsFieldType } from "./componentsField.partial";
registerFieldType(FieldTypeGuids.Components, new ComponentsFieldType());

import { ComponentFieldType } from "./componentField.partial";
registerFieldType(FieldTypeGuids.Component, new ComponentFieldType());

import { CurrencyFieldType } from "./currencyField.partial";
registerFieldType(FieldTypeGuids.Currency, new CurrencyFieldType());

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

import { DefinedValueFieldType } from "./definedValueField.partial";
registerFieldType(FieldTypeGuids.DefinedValue, new DefinedValueFieldType());

import { DefinedValueRangeFieldType } from "./definedValueRangeField.partial";
registerFieldType(FieldTypeGuids.DefinedValueRange, new DefinedValueRangeFieldType());

import { EmailFieldType } from "./emailField.partial";
registerFieldType(FieldTypeGuids.Email, new EmailFieldType());

import { FinancialAccountFieldType  } from "./financialAccountField.partial";
registerFieldType(FieldTypeGuids.FinancialAccount, new FinancialAccountFieldType());

import { FinancialAccountsFieldType  } from "./financialAccountsField.partial";
registerFieldType(FieldTypeGuids.FinancialAccounts, new FinancialAccountsFieldType());

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

import { ImageFieldType } from "./imageField.partial";
registerFieldType(FieldTypeGuids.Image, new ImageFieldType());

import { IntegerFieldType } from "./integerField.partial";
registerFieldType(FieldTypeGuids.Integer, new IntegerFieldType());

import { IntegerRangeFieldType } from "./integerRangeField.partial";
registerFieldType(FieldTypeGuids.IntegerRange, new IntegerRangeFieldType());

import { KeyValueListFieldType } from "./keyValueListField.partial";
registerFieldType(FieldTypeGuids.KeyValueList, new KeyValueListFieldType());

import { LocationListFieldType } from "./locationListField.partial";
registerFieldType(FieldTypeGuids.LocationList, new LocationListFieldType());

import { MemoFieldType } from "./memoField.partial";
registerFieldType(FieldTypeGuids.Memo, new MemoFieldType());

import { MonthDayFieldType } from "./monthDayField.partial";
registerFieldType(FieldTypeGuids.MonthDay, new MonthDayFieldType());

import { MediaSelectorFieldType } from "./mediaSelectorField.partial";
registerFieldType(FieldTypeGuids.MediaSelector, new MediaSelectorFieldType());

import { MultiSelectFieldType } from "./multiSelectField.partial";
registerFieldType(FieldTypeGuids.MultiSelect, new MultiSelectFieldType());

import { NoteTypeField } from "./noteTypeField.partial";
registerFieldType(FieldTypeGuids.NoteType, new NoteTypeField());

import { NoteTypesField } from "./noteTypesField.partial";
registerFieldType(FieldTypeGuids.NoteTypes, new NoteTypesField());

import { PhoneNumberFieldType } from "./phoneNumberField.partial";
registerFieldType(FieldTypeGuids.PhoneNumber, new PhoneNumberFieldType());

import { PersonFieldType } from "./personField.partial";
registerFieldType(FieldTypeGuids.Person, new PersonFieldType());

import { RangeSliderFieldType } from "./rangeSliderField.partial";
registerFieldType(FieldTypeGuids.RangeSlider, new RangeSliderFieldType());

import { RatingFieldType } from "./ratingField.partial";
registerFieldType(FieldTypeGuids.Rating, new RatingFieldType());

import { RegistryEntryFieldType } from "./registryEntryField.partial";
registerFieldType(FieldTypeGuids.RegistryEntry, new RegistryEntryFieldType());

import { ReminderTypeFieldType } from "./reminderTypeField.partial";
registerFieldType(FieldTypeGuids.ReminderType, new ReminderTypeFieldType());

import { ReminderTypesFieldType } from "./reminderTypesField.partial";
registerFieldType(FieldTypeGuids.ReminderTypes, new ReminderTypesFieldType());

import { ScheduleFieldType } from "./scheduleField.partial";
registerFieldType(FieldTypeGuids.Schedule, new ScheduleFieldType());

import { SchedulesFieldType } from "./schedulesField.partial";
registerFieldType(FieldTypeGuids.Schedules, new SchedulesFieldType());

import { SingleSelectFieldType } from "./singleSelectField.partial";
registerFieldType(FieldTypeGuids.SingleSelect, new SingleSelectFieldType());

import { SlidingDateRangeFieldType } from "./slidingDateRangeField.partial";
registerFieldType(FieldTypeGuids.SlidingDateRange, new SlidingDateRangeFieldType());

import { SocialMediaAccountFieldType } from "./socialMediaAccountField.partial";
registerFieldType(FieldTypeGuids.SocialMediaAccount, new SocialMediaAccountFieldType());

import { SSNFieldType } from "./ssnField.partial";
registerFieldType(FieldTypeGuids.Ssn, new SSNFieldType());

import { TextFieldType } from "./textField.partial";
registerFieldType(FieldTypeGuids.Text, new TextFieldType());

import { TimeFieldType } from "./timeField.partial";
registerFieldType(FieldTypeGuids.Time, new TimeFieldType());

import { TimeZoneFieldType } from "./timeZoneField.partial";
registerFieldType(FieldTypeGuids.TimeZone, new TimeZoneFieldType());

import { UrlLinkFieldType } from "./urlLinkField.partial";
registerFieldType(FieldTypeGuids.UrlLink, new UrlLinkFieldType());

import { ValueListFieldType } from "./valueListField.partial";
registerFieldType(FieldTypeGuids.ValueList, new ValueListFieldType());
