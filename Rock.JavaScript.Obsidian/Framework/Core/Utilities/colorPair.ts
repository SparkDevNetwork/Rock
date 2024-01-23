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

import { RockColor } from "./rockColor";

/**
 * Pair of colors representing the foreground and background colors.
 */
export class ColorPair {
    /** The foreground color. */
    public foregroundColor: RockColor;

    /** The background color. */
    public backgroundColor: RockColor;

    /**
     * The contrast ratio between the foreground and background colors.
     */
    public get contrastRatio(): number {
        return RockColor.calculateContrastRatio(this.foregroundColor, this.backgroundColor);
    }

    /**
     * Creates a new pair of colors from the given colors.
     *
     * @param foregroundColor The foreground color.
     * @param backgroundColor The background color.
     */
    constructor(foregroundColor: RockColor, backgroundColor: RockColor) {
        this.foregroundColor = foregroundColor;
        this.backgroundColor = backgroundColor;
    }

    /**
     * Flips the foreground and background colors.
     */
    public flip(): void {
        const tempColor = this.foregroundColor;

        this.foregroundColor = this.backgroundColor;
        this.backgroundColor = tempColor;
    }
}
