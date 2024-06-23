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

import Cache from "./cache";
import { useHttp } from "./http";
import { PhoneNumberBoxGetConfigurationResultsBag } from "@Obsidian/ViewModels/Rest/Controls/phoneNumberBoxGetConfigurationResultsBag";
import { PhoneNumberCountryCodeRulesConfigurationBag } from "@Obsidian/ViewModels/Rest/Controls/phoneNumberCountryCodeRulesConfigurationBag";
import { PhoneNumberBoxGetConfigurationOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/phoneNumberBoxGetConfigurationOptionsBag";

const http = useHttp();

/**
 * Fetch the configuration for phone numbers and their possible formats for different countries
 */
async function fetchPhoneNumberConfiguration(): Promise<PhoneNumberBoxGetConfigurationResultsBag> {
    const result = await http.post<PhoneNumberBoxGetConfigurationResultsBag>("/api/v2/Controls/PhoneNumberBoxGetConfiguration", undefined, null);

    if (result.isSuccess && result.data) {
        return result.data;
    }

    throw new Error(result.errorMessage ?? "Error fetching phone number configuration");
}

/**
 * Fetch the configuration for phone numbers, SMS option, and possible phone number formats for different countries
 */
async function fetchPhoneNumberAndSmsConfiguration(): Promise<PhoneNumberBoxGetConfigurationResultsBag> {
    const options: PhoneNumberBoxGetConfigurationOptionsBag = {
        showSmsOptIn: true
    };
    const result = await http.post<PhoneNumberBoxGetConfigurationResultsBag>("/api/v2/Controls/PhoneNumberBoxGetConfiguration", undefined, options);

    if (result.isSuccess && result.data) {
        return result.data;
    }

    throw new Error(result.errorMessage ?? "Error fetching phone number configuration");
}

/**
 * Fetch the configuration for phone numbers and their possible formats for different countries.
 * Cacheable version of fetchPhoneNumberConfiguration cacheable
 */
export const getPhoneNumberConfiguration = Cache.cachePromiseFactory("phoneNumberConfiguration", fetchPhoneNumberConfiguration);

export const getPhoneNumberAndSmsConfiguration = Cache.cachePromiseFactory("phoneNumberAndSmsConfiguration", fetchPhoneNumberAndSmsConfiguration);

const defaultRulesConfig = [
    {
        "match": "^(\\d{3})(\\d{4})$",
        "format": "$1-$2"
    },
    {
        "match": "^(\\d{3})(\\d{3})(\\d{4})$",
        "format": "($1) $2-$3"
    },
    {
        "match": "^1(\\d{3})(\\d{3})(\\d{4})$",
        "format": "($1) $2-$3"
    }
];

/**
 * Format a phone number according to a given configuration
 *
 * e.g. from the default configuration:
 * 3214567 => 321-4567
 * 3214567890 => (321) 456-7890
 */
export function formatPhoneNumber(value: string, rules: PhoneNumberCountryCodeRulesConfigurationBag[] = defaultRulesConfig): string {
    value = stripPhoneNumber(value);

    if (!value || rules.length == 0) {
        return value;
    }

    for (const rule of rules) {
        const regex = new RegExp(rule.match ?? "");

        if (regex.test(value)) {
            return value.replace(regex, rule.format ?? "") || value;
        }
    }

    return value;
}

/**
 * Strips special characters from the phone number.
 * (321) 456-7890 => 3214567890
 * @param str
 */
export function stripPhoneNumber(str: string): string {
    if (!str) {
        return "";
    }

    return str.replace(/\D/g, "");
}

export default {
    getPhoneNumberConfiguration,
    formatPhoneNumber,
    stripPhoneNumber
};

/* eslint-disable */
// @ts-ignore
window.formatPhoneNumber = formatPhoneNumber;