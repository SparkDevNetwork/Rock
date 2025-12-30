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

export class RockTimeSpan {
    // eslint-disable-next-line @typescript-eslint/naming-convention
    constructor(private _milliseconds: number) {
        this._milliseconds = Math.abs(_milliseconds);
    }

    static fromMilliseconds(milliseconds: number): RockTimeSpan {
        return new RockTimeSpan(milliseconds);
    }

    static fromSeconds(seconds: number): RockTimeSpan {
        return new RockTimeSpan(seconds * 1000);
    }

    static fromMinutes(minutes: number): RockTimeSpan {
        return new RockTimeSpan(minutes * 60 * 1000);
    }

    static fromHours(hours: number): RockTimeSpan {
        return new RockTimeSpan(hours * 60 * 60 * 1000);
    }

    static fromDays(days: number): RockTimeSpan {
        return new RockTimeSpan(days * 24 * 60 * 60 * 1000);
    }

    get totalMilliseconds(): number {
        return this._milliseconds;
    }

    get totalSeconds(): number {
        return this._milliseconds / 1000;
    }

    get totalMinutes(): number {
        return this._milliseconds / (60 * 1000);
    }

    get totalHours(): number {
        return this._milliseconds / (60 * 60 * 1000);
    }

    get totalDays(): number {
        return this._milliseconds / (24 * 60 * 60 * 1000);
    }

    get approximatedTotalMonths(): number {
        // Treat a month as 31 days
        const msPerMonth = 31 * 24 * 60 * 60 * 1000;
        return this._milliseconds / msPerMonth;
    }

    get milliseconds(): number {
        return this._milliseconds % 1000;
    }

    get seconds(): number {
        return Math.floor((this._milliseconds / 1000) % 60);
    }

    get minutes(): number {
        return Math.floor((this._milliseconds / (60 * 1000)) % 60);
    }

    get hours(): number {
        return Math.floor((this._milliseconds / (60 * 60 * 1000)) % 24);
    }

    get days(): number {
        return Math.floor(this._milliseconds / (24 * 60 * 60 * 1000));
    }

    /**
     * Transforms the date into a human friendly elapsed time string.
     *
     * @example
     * // Returns "25 Years Ago" if the current date is 2025-03-04 and this instance is 2000-03-04.
     * RockDateTime.fromParts(2000, 3, 4).toElapsedString();
     *
     * @returns A string that represents the amount of time that has elapsed.
     */
    public toElapsedString(): string {
        const daysPerYear = 365;

        const totalHours = this.totalHours;
        const totalSeconds = this.totalSeconds;
        const totalMinutes = this.totalMinutes;
        const totalDays = this.totalDays;
        const approximatedTotalMonths = this.approximatedTotalMonths;

        if (totalHours < 24) {
            if (totalSeconds < 1) {
                return `${(0).toLocaleString(undefined)} Seconds`;
            }

            if (totalSeconds < 2) {
                return `${(1).toLocaleString(undefined)} Second`;
            }

            if (totalSeconds < 60) {
                return `${Math.floor(totalSeconds).toLocaleString(undefined)} Seconds`;
            }

            if (totalMinutes < 2) {
                return `${(1).toLocaleString(undefined)} Minute`;
            }

            if (totalMinutes < 60) {
                return `${Math.floor(totalMinutes).toLocaleString(undefined)} Minutes`;
            }

            if (totalHours < 2) {
                return `${(1).toLocaleString(undefined)} Hour`;
            }

            if (totalHours < 60) {
                return `${Math.floor(totalHours).toLocaleString(undefined)} Hours`;
            }
        }

        if (totalDays < 2) {
            return `${(1).toLocaleString(undefined)} Day`;
        }

        if (totalDays < 31) {
            return `${Math.floor(totalDays).toLocaleString(undefined)} Days`;
        }

        if (approximatedTotalMonths <= 1) {
            return `${(1).toLocaleString(undefined)} Month`;
        }

        if (approximatedTotalMonths <= 18) {
            return `${Math.round(approximatedTotalMonths).toLocaleString(undefined)} Months`;
        }

        const totalYears = Math.floor(totalDays / daysPerYear);

        if (totalYears <= 1) {
            return `${(1).toLocaleString(undefined)} Year`;
        }

        return `${Math.round(totalYears).toLocaleString(undefined)} Years`;
    }
}
