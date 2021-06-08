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
System.register(["./Number"], function (exports_1, context_1) {
    "use strict";
    var Number_1, dateKeyLength, dateKeyNoYearLength;
    var __moduleName = context_1 && context_1.id;
    /**
     * Gets the year value from the date key.
     * Ex: 20210228 => 2021
     * @param dateKey
     */
    function getYear(dateKey) {
        var defaultValue = 0;
        if (!dateKey || dateKey.length !== dateKeyLength) {
            return defaultValue;
        }
        var asString = dateKey.substring(0, 4);
        var year = Number_1.toNumberOrNull(asString) || defaultValue;
        return year;
    }
    exports_1("getYear", getYear);
    /**
     * Gets the month value from the date key.
     * Ex: 20210228 => 2
     * @param dateKey
     */
    function getMonth(dateKey) {
        var defaultValue = 0;
        if (!dateKey) {
            return defaultValue;
        }
        if (dateKey.length === dateKeyLength) {
            var asString = dateKey.substring(4, 6);
            return Number_1.toNumberOrNull(asString) || defaultValue;
        }
        if (dateKey.length === dateKeyNoYearLength) {
            var asString = dateKey.substring(0, 2);
            return Number_1.toNumberOrNull(asString) || defaultValue;
        }
        return defaultValue;
    }
    exports_1("getMonth", getMonth);
    /**
     * Gets the day value from the date key.
     * Ex: 20210228 => 28
     * @param dateKey
     */
    function getDay(dateKey) {
        var defaultValue = 0;
        if (!dateKey) {
            return defaultValue;
        }
        if (dateKey.length === dateKeyLength) {
            var asString = dateKey.substring(6, 8);
            return Number_1.toNumberOrNull(asString) || defaultValue;
        }
        if (dateKey.length === dateKeyNoYearLength) {
            var asString = dateKey.substring(2, 4);
            return Number_1.toNumberOrNull(asString) || defaultValue;
        }
        return defaultValue;
    }
    exports_1("getDay", getDay);
    /**
     * Gets the datekey constructed from the parts.
     * Ex: (2021, 2, 28) => '20210228'
     * @param year
     * @param month
     * @param day
     */
    function toDateKey(year, month, day) {
        if (!year || year > 9999 || year < 0) {
            year = 0;
        }
        if (!month || month > 12 || month < 0) {
            month = 0;
        }
        if (!day || day > 31 || day < 0) {
            day = 0;
        }
        var yearStr = Number_1.zeroPad(year, 4);
        var monthStr = Number_1.zeroPad(month, 2);
        var dayStr = Number_1.zeroPad(day, 2);
        return "" + yearStr + monthStr + dayStr;
    }
    exports_1("toDateKey", toDateKey);
    /**
     * Gets the datekey constructed from the parts.
     * Ex: (2, 28) => '0228'
     * @param month
     * @param day
     */
    function toNoYearDateKey(month, day) {
        if (!month || month > 12 || month < 0) {
            month = 0;
        }
        if (!day || day > 31 || day < 0) {
            day = 0;
        }
        var monthStr = Number_1.zeroPad(month, 2);
        var dayStr = Number_1.zeroPad(day, 2);
        return "" + monthStr + dayStr;
    }
    exports_1("toNoYearDateKey", toNoYearDateKey);
    return {
        setters: [
            function (Number_1_1) {
                Number_1 = Number_1_1;
            }
        ],
        execute: function () {
            dateKeyLength = 'YYYYMMDD'.length;
            dateKeyNoYearLength = 'MMDD'.length;
            exports_1("default", {
                getYear: getYear,
                getMonth: getMonth,
                getDay: getDay,
                toDateKey: toDateKey,
                toNoYearDateKey: toNoYearDateKey
            });
        }
    };
});
//# sourceMappingURL=DateKey.js.map