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

import { RockColor } from "@Obsidian/Core/Utilities/rockColor";

/**
 * Gets the inline style for a status label using the raw status color with
 * a readable text color, so the label looks the same in both views.
 *
 * @param color The status color, or empty when the status has no color.
 */
export function getStatusLabelStyle(color: string | null | undefined): Record<string, string> | undefined {
    if (!color) {
        return undefined;
    }

    const rockColor = new RockColor(color);

    return {
        backgroundColor: color,
        color: rockColor.isLight ? "var(--base-interface-strong)" : "var(--base-interface-softest)"
    };
}
