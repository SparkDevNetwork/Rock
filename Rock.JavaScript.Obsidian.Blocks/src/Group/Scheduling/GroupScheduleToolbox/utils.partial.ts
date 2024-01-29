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

import { toOrdinalSuffix } from "@Obsidian/Utility/numberUtils";
import { RockDateTime } from "@Obsidian/Utility/rockDateTime";

/**
 * Formats the date for consistent, friendly display within the toolbox.
 *
 * @param dateString The string that contains the ISO 8601 formatted text.
 * @returns The formatted date.
 */
export function formatToolboxDate(dateString: string): string {
    let formattedDate: string | null | undefined;
    const rockDateTime = RockDateTime.parseISO(dateString);
    if (rockDateTime) {
        formattedDate = rockDateTime.toASPString("ddd, MMM d");

        if (formattedDate) {
            const dateParts = formattedDate.split(" ");
            if (dateParts.length === 3) {
                const ordinalDate = toOrdinalSuffix(+dateParts[2]);
                formattedDate = [
                    dateParts[0],
                    dateParts[1],
                    ordinalDate ?? dateParts[2]
                ].join(" ");
            }
        }
    }

    return formattedDate || "No Date Provided";
}