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

/**
 * Transform the value into true, false, or null
 * @param val
 */
export function asBooleanOrNull(val: unknown): boolean | null {
    if (val === undefined || val === null) {
        return null;
    }

    if (typeof val === "boolean") {
        return val;
    }

    if (typeof val === "string") {
        const asString = (val || "").trim().toLowerCase();

        if (!asString) {
            return null;
        }

        return ["true", "yes", "t", "y", "1"].indexOf(asString) !== -1;
    }

    if (typeof val === "number") {
        return !!val;
    }

    return null;
}

/**
 * Transform the value into true or false
 * @param val
 */
export function asBoolean(val: unknown): boolean {
    return !!asBooleanOrNull(val);
}

/** Transform the value into the strings "Yes", "No", or null */
export function asYesNoOrNull(val: unknown): "Yes" | "No" | null {
    const boolOrNull = asBooleanOrNull(val);

    if (boolOrNull === null) {
        return null;
    }

    return boolOrNull ? "Yes" : "No";
}

/** Transform the value into the strings "True", "False", or null */
export function asTrueFalseOrNull(val: unknown): "True" | "False" | null {
    const boolOrNull = asBooleanOrNull(val);

    if (boolOrNull === null) {
        return null;
    }

    return boolOrNull ? "True" : "False";
}

/** Transform the value into the strings "True" if truthy or "False" if falsey */
export function asTrueOrFalseString(val: unknown): "True" | "False" {
    const boolOrNull = asBooleanOrNull(val);

    return boolOrNull ? "True" : "False";
}
