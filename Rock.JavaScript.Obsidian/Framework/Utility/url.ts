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
 * Is the value a valid URL?
 * @param val
 */
export function isUrl(val: unknown): boolean {
    if (typeof val === "string") {
        // https://www.regextester.com/1965
        // Modified from link above to support urls like "http://localhost:6229/Person/1/Edit" (Url does not have a period)
        const re = /^(http[s]?:\/\/)?[^\s(["<,>]*\.?[^\s[",><]*$/;
        return re.test(val);
    }

    return false;
}
