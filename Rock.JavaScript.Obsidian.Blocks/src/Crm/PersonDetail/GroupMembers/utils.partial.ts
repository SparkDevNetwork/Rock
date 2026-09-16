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
 * Builds the profile page URL for a person, preserving any profile subpage
 * route in the current URL so member links stay on the same subpage (for
 * example, "/Person/abc123/extended-attributes" links members to their own
 * extended attributes subpage).
 *
 * @param personIdKey The IdKey of the person to link to.
 *
 * @returns The person profile URL.
 */
export function buildPersonProfileUrl(personIdKey: string | null | undefined): string {
    const key = personIdKey ?? "";
    const path = window.location.pathname;
    const personSegment = "/person/";
    const segmentIndex = path.toLowerCase().indexOf(personSegment);

    if (segmentIndex >= 0) {
        const applicationRoot = path.substring(0, segmentIndex);
        const remainder = path.substring(segmentIndex + personSegment.length);
        const subpageIndex = remainder.indexOf("/");
        const subpageRoute = subpageIndex >= 0 ? remainder.substring(subpageIndex) : "";

        return `${applicationRoot}/Person/${key}${subpageRoute}`;
    }

    return `/Person/${key}`;
}
