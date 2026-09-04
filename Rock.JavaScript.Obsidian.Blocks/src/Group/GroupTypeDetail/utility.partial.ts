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
 * Converts a decimal multiplier (0.00 - 1.00) to a whole-number percent (0 - 100) for display.
 */
export function decimalToPercentage(value: number | null | undefined): number | null {
    if (value === null || value === undefined) {
        return null;
    }

    return Math.round(value * 100);
}

/**
 * Converts a percent (0 - 100) value from the UI into the decimal multiplier (0.00 - 1.00) stored in the model.
 */
export function percentageToDecimal(value: number | null | undefined): number {
    if (value === null || value === undefined) {
        return 0;
    }

    return value / 100;
}
