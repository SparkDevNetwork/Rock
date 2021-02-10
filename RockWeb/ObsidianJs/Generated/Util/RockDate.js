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
System.register([], function (exports_1, context_1) {
    "use strict";
    var __moduleName = context_1 && context_1.id;
    /**
     * Adjust for the timezone offset so early morning times don't appear as the previous local day.
     * @param val
     */
    function stripTimezone(val) {
        var asUtc = new Date(val.getTime() + val.getTimezoneOffset() * 60000);
        return asUtc;
    }
    exports_1("stripTimezone", stripTimezone);
    /**
     * Convert a date to a RockDate
     * @param d
     */
    function toRockDate(d) {
        if (d instanceof Date && !isNaN(d)) {
            return stripTimezone(d).toISOString().split('T')[0];
        }
        return '';
    }
    exports_1("toRockDate", toRockDate);
    /**
    * Generates a new Rock Date
    */
    function newDate() {
        return toRockDate(new Date());
    }
    exports_1("newDate", newDate);
    /**
     * Returns the day.
     * Ex: 1/2/2000 => 2
     * @param d
     */
    function getDay(d) {
        if (!d) {
            return null;
        }
        var asDate = stripTimezone(new Date(d));
        return asDate.getDate();
    }
    exports_1("getDay", getDay);
    /**
     * Returns the month.
     * Ex: 1/2/2000 => 1
     * @param d
     */
    function getMonth(d) {
        if (!d) {
            return null;
        }
        var asDate = stripTimezone(new Date(d));
        return asDate.getMonth() + 1;
    }
    exports_1("getMonth", getMonth);
    /**
     * Returns the year.
     * Ex: 1/2/2000 => 2000
     * @param d
     */
    function getYear(d) {
        if (!d) {
            return null;
        }
        var asDate = stripTimezone(new Date(d));
        return asDate.getFullYear();
    }
    exports_1("getYear", getYear);
    return {
        setters: [],
        execute: function () {
            exports_1("default", {
                newDate: newDate,
                toRockDate: toRockDate,
                getDay: getDay,
                getMonth: getMonth,
                getYear: getYear,
                stripTimezone: stripTimezone
            });
        }
    };
});
//# sourceMappingURL=RockDate.js.map