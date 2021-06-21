System.register(["./Number"], function (exports_1, context_1) {
    "use strict";
    var Number_1, dateKeyLength, dateKeyNoYearLength;
    var __moduleName = context_1 && context_1.id;
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