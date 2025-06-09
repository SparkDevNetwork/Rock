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
 * Attempts to parse the JSON and returns undefined if it could not be parsed.
 *
 * @param value The JSON value to parse.
 *
 * @returns The object that represents the JSON or undefined.
 */
export function safeParseJson<T>(value: string | null | undefined): T | undefined {
    if (!value) {
        return undefined;
    }

    try {
        return JSON.parse(value);
    }
    catch {
        return undefined;
    }
}