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

import type { PropType } from "vue";

/** The custom validation function signature. */
export type ValidationRuleFunction = (value: unknown, params?: unknown[]) => ValidationResult;

/** The return type allowed in a validation function. */
export type ValidationResult = string | boolean;

/**
 * A reference to a defined validation rule with parameters. This is primarily
 * an internal use type but is perfectly valid for public use as well.
 */
export type ValidationRuleReference = {
    /** The name of the rule. */
    name: string;

    /** Any parameters that were found. */
    params: unknown[];
};

/** The rule types that are valid to be processed during validation checks. */
export type ValidationRule = string | ValidationRuleReference | ValidationRuleFunction;

export type RulesPropType = {
    type: PropType<ValidationRule | ValidationRule[]>,
    default: ""
};
