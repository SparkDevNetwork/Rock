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
 * A helper object that provides equivalent format strings for a given locale,
 * for the various date libraries used throughout Rock.
 *
 * This API is internal to Rock, and is not subject to the same compatibility
 * standards as public APIs. It may be changed or removed without notice in any
 * release. You should not use this API directly in any plug-ins. Doing so can
 * result in application failures when updating to a new Rock release.
 */
export class LocaleDateFormatter {
    /**
     * The internal JavaScript date format string for the locale represented
     * by this formatter instance.
     */
    private jsDateFormatString: string;

    /**
     * The internal ASP C# date format string for the locale represented by this
     * formatter instance.
     */
    private aspDateFormatString: string | undefined;

    /**
     * The internal date picker format string for the locale represented by this
     * formatter instance.
     */
    private datePickerFormatString: string | undefined;

    /**
     * Creates a new instance of LocaleDateFormatter.
     *
     * @param jsDateFormatString The JavaScript date format string for the
     * locale represented by this formatter instance.
     * https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Date#date_time_string_format
     */
    private constructor(jsDateFormatString: string) {
        this.jsDateFormatString = jsDateFormatString;
    }

    /**
     * Creates a new instance of LocaleDateFormatter from the current locale. If
     * the current locale cannot be determined, a default "en-US" locale
     * formatter instance will be returned.
     *
     * @returns A LocaleDateFormatter instance representing the current locale.
     */
    public static fromCurrent(): LocaleDateFormatter {
        // Create an arbitrary date with recognizable numeric parts; format the
        // date using the current locale settings and then replace the numeric
        // parts with date format placeholders to get the locale date format
        // string. Note that month is specified as an index in the Date
        // constructor, so "2" represents month "3".
        const date = new Date(2222, 2, 4);
        const localeDateString = date.toLocaleDateString(undefined, {
            year: "numeric",
            month: "numeric",
            day: "numeric"
        });

        // Fall back to a default, en-US format string if any step of the
        // parsing fails.
        const defaultFormatString = "MM/DD/YYYY";

        let localeFormatString = localeDateString;

        // Replace the known year date part with a 2 or 4 digit format string.
        if (localeDateString.includes("2222")) {
            localeFormatString = localeDateString
                .replace("2222", "YYYY");
        }
        else if (localeDateString.includes("22")) {
            localeFormatString = localeDateString
                .replace("22", "YY");
        }
        else {
            return new LocaleDateFormatter(defaultFormatString);
        }

        // Replace the known month date part with a 1 or 2 digit format string.
        if (localeFormatString.includes("03")) {
            localeFormatString = localeFormatString.replace("03", "MM");
        }
        else if (localeFormatString.includes("3")) {
            localeFormatString = localeFormatString.replace("3", "M");
        }
        else {
            return new LocaleDateFormatter(defaultFormatString);
        }

        // Replace the known day date part with a 1 or 2 digit format string.
        if (localeFormatString.includes("04")) {
            localeFormatString = localeFormatString.replace("04", "DD");
        }
        else if (localeFormatString.includes("4")) {
            localeFormatString = localeFormatString.replace("4", "D");
        }
        else {
            return new LocaleDateFormatter(defaultFormatString);
        }

        return new LocaleDateFormatter(localeFormatString);
    }

    /**
     * The ASP C# date format string for the locale represented by this
     * formatter instance.
     */
    public get aspDateFormat(): string {
        if (!this.aspDateFormatString) {
            // Transform the standard JavaScript format string to follow C# date
            // formatting rules.
            // https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings
            this.aspDateFormatString = this.jsDateFormatString
                .replace(/D/g, "d")
                .replace(/Y/g, "y");
        }

        return this.aspDateFormatString;
    }

    /**
     * The date picker format string for the locale represented by this
     * formatter instance.
     */
    public get datePickerFormat(): string {
        if (!this.datePickerFormatString) {
            // Transform the standard JavaScript format string to follow the
            // bootstrap-datepicker library's formatting rules.
            // https://bootstrap-datepicker.readthedocs.io/en/stable/options.html#format
            this.datePickerFormatString = this.jsDateFormatString
                .replace(/D/g, "d")
                .replace(/M/g, "m")
                .replace(/Y/g, "y");
        }

        return this.datePickerFormatString;
    }
}
