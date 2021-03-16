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
import DateKey from '../Services/DateKey';
import { isEmail } from '../Services/Email';
import { isNullOrWhitespace } from '../Services/String';
import { defineRule } from 'vee-validate';

export type ValidationRuleFunction = (value: unknown) => boolean | string | Promise<boolean | string>;

export function ruleStringToArray(rulesString: string) {
    return rulesString.split('|');
}

export function ruleArrayToString(rulesArray: string[]) {
    return rulesArray.join('|');
}

defineRule('required', ((value, [optionsJson]) => {
    const options = optionsJson ? JSON.parse(optionsJson) : {};

    if (typeof value === 'string') {
        const allowEmptyString = !!(options.allowEmptyString);

        if (!allowEmptyString && isNullOrWhitespace(value)) {
            return 'is required';
        }

        return true;
    }

    if (typeof value === 'number' && value === 0) {
        return 'is required';
    }

    if (!value) {
        return 'is required';
    }

    return true;
}) as ValidationRuleFunction);

defineRule('email', (value => {
    // Field is empty, should pass
    if (isNullOrWhitespace(value)) {
        return true;
    }

    // Check if email
    if (!isEmail(value)) {
        return 'must be a valid email';
    }

    return true;
}) as ValidationRuleFunction);

defineRule('notequal', ((value, [compare]) => {
    return value !== compare;
}) as ValidationRuleFunction);

defineRule('greaterthan', ((value, [compare]) => {
    return value > compare;
}) as ValidationRuleFunction);

defineRule('datekey', (value => {
    const asString = value as string;

    if (!DateKey.getYear(asString)) {
        return 'must have a year';
    }

    if (!DateKey.getMonth(asString)) {
        return 'must have a month';
    }

    if (!DateKey.getDay(asString)) {
        return 'must have a day';
    }

    return true;
}) as ValidationRuleFunction);