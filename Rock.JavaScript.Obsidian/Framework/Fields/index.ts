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
import { FieldType as FieldTypeGuids } from "../SystemGuids";
import { registerFieldType } from "./utils";

export { ConfigurationValues, getFieldEditorProps, registerFieldType, getFieldType } from "./utils";

/*
 * Define the standard field types in Rock.
 */

import { AddressFieldType } from "./addressField";
registerFieldType(FieldTypeGuids.Address, new AddressFieldType());

import { BooleanFieldType } from "./booleanField";
registerFieldType(FieldTypeGuids.Boolean, new BooleanFieldType());

import { CampusFieldType } from "./campusField";
registerFieldType(FieldTypeGuids.Campus, new CampusFieldType());

import { CampusesFieldType } from "./campusesField";
registerFieldType(FieldTypeGuids.Campuses, new CampusesFieldType());

import { ColorFieldType } from "./colorField";
registerFieldType(FieldTypeGuids.Color, new ColorFieldType());

import { CurrencyFieldType } from "./currencyField";
registerFieldType(FieldTypeGuids.Currency, new CurrencyFieldType());

import { DateFieldType } from "./dateField";
registerFieldType(FieldTypeGuids.Date, new DateFieldType());

import { DateRangeFieldType } from "./dateRangeField";
registerFieldType(FieldTypeGuids.DateRange, new DateRangeFieldType());

import { DateTimeFieldType } from "./dateTimeField";
registerFieldType(FieldTypeGuids.DateTime, new DateTimeFieldType());

import { DayOfWeekFieldType } from "./dayOfWeekField";
registerFieldType(FieldTypeGuids.DayOfWeek, new DayOfWeekFieldType());

import { DaysOfWeekFieldType } from "./daysOfWeekField";
registerFieldType(FieldTypeGuids.DaysOfWeek, new DaysOfWeekFieldType());

import { DecimalFieldType } from "./decimalField";
registerFieldType(FieldTypeGuids.Decimal, new DecimalFieldType());

import { DecimalRangeFieldType } from "./decimalRangeField";
registerFieldType(FieldTypeGuids.DecimalRange, new DecimalRangeFieldType());

import { DefinedValueFieldType } from "./definedValueField";
registerFieldType(FieldTypeGuids.DefinedValue, new DefinedValueFieldType());

import { DefinedValueRangeFieldType } from "./definedValueRangeField";
registerFieldType(FieldTypeGuids.DefinedValueRange, new DefinedValueRangeFieldType());

import { EmailFieldType } from "./emailField";
registerFieldType(FieldTypeGuids.Email, new EmailFieldType());

import { GenderFieldType } from "./genderField";
registerFieldType(FieldTypeGuids.Gender, new GenderFieldType());

import { IntegerFieldType } from "./integerField";
registerFieldType(FieldTypeGuids.Integer, new IntegerFieldType());

import { IntegerRangeFieldType } from "./integerRangeField";
registerFieldType(FieldTypeGuids.IntegerRange, new IntegerRangeFieldType());

import { KeyValueListFieldType } from "./keyValueListField";
registerFieldType(FieldTypeGuids.KeyValueList, new KeyValueListFieldType());

import { MemoFieldType } from "./memoField";
registerFieldType(FieldTypeGuids.Memo, new MemoFieldType());

import { MonthDayFieldType } from "./monthDayField";
registerFieldType(FieldTypeGuids.MonthDay, new MonthDayFieldType());

import { MultiSelectFieldType } from "./multiSelectField";
registerFieldType(FieldTypeGuids.MultiSelect, new MultiSelectFieldType());

import { PhoneNumberFieldType } from "./phoneNumberField";
registerFieldType(FieldTypeGuids.PhoneNumber, new PhoneNumberFieldType());

import { RatingFieldType } from "./ratingField";
registerFieldType(FieldTypeGuids.Rating, new RatingFieldType());

import { SingleSelectFieldType } from "./singleSelectField";
registerFieldType(FieldTypeGuids.SingleSelect, new SingleSelectFieldType());

import { SSNFieldType } from "./ssnField";
registerFieldType(FieldTypeGuids.Ssn, new SSNFieldType());

import { TextFieldType } from "./textField";
registerFieldType(FieldTypeGuids.Text, new TextFieldType());

import { TimeFieldType } from "./timeField";
registerFieldType(FieldTypeGuids.Time, new TimeFieldType());

import { UrlLinkFieldType } from "./urlLinkField";
registerFieldType(FieldTypeGuids.UrlLink, new UrlLinkFieldType());
