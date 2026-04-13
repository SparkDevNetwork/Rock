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

import { ComparisonType } from "@Obsidian/Enums/Reporting/comparisonType";
import { ComparisonValue } from "@Obsidian/Types/Reporting/comparisonValue";

/**
 * Returns true if a ComparisonValue carries no meaningful filter.
 * Handles plain empty strings and JSON-serialized picker values
 * like {"value":"","text":""} that look non-empty but are blank.
 */
export function isFilterEntryEmpty(entry: ComparisonValue): boolean {
    // IsBlank / IsNotBlank are valid without a value.
    if (entry.comparisonType === ComparisonType.IsBlank || entry.comparisonType === ComparisonType.IsNotBlank) {
        return false;
    }

    let raw = entry.value?.trim();

    // Some controls emit the literal string "null" when cleared.
    if (!raw || raw === "null") {
        return true;
    }

    // Picker controls emit a JSON bag like {"value":"","text":""}.
    if (raw.startsWith("{")) {
        try {
            raw = JSON.parse(raw)?.value ?? "";
        }
        catch {
            // Not JSON — use the raw string.
        }
    }

    return !raw;
}
