System.register(["./number"], function (exports_1, context_1) {
    "use strict";
    var number_1, dateKeyLength, dateKeyNoYearLength;
    var __moduleName = context_1 && context_1.id;
    function getYear(dateKey) {
        const defaultValue = 0;
        if (!dateKey || dateKey.length !== dateKeyLength) {
            return defaultValue;
        }
        const asString = dateKey.substring(0, 4);
        const year = number_1.toNumberOrNull(asString) || defaultValue;
        return year;
    }
    exports_1("getYear", getYear);
    function getMonth(dateKey) {
        const defaultValue = 0;
        if (!dateKey) {
            return defaultValue;
        }
        if (dateKey.length === dateKeyLength) {
            const asString = dateKey.substring(4, 6);
            return number_1.toNumberOrNull(asString) || defaultValue;
        }
        if (dateKey.length === dateKeyNoYearLength) {
            const asString = dateKey.substring(0, 2);
            return number_1.toNumberOrNull(asString) || defaultValue;
        }
        return defaultValue;
    }
    exports_1("getMonth", getMonth);
    function getDay(dateKey) {
        const defaultValue = 0;
        if (!dateKey) {
            return defaultValue;
        }
        if (dateKey.length === dateKeyLength) {
            const asString = dateKey.substring(6, 8);
            return number_1.toNumberOrNull(asString) || defaultValue;
        }
        if (dateKey.length === dateKeyNoYearLength) {
            const asString = dateKey.substring(2, 4);
            return number_1.toNumberOrNull(asString) || defaultValue;
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
        const yearStr = number_1.zeroPad(year, 4);
        const monthStr = number_1.zeroPad(month, 2);
        const dayStr = number_1.zeroPad(day, 2);
        return `${yearStr}${monthStr}${dayStr}`;
    }
    exports_1("toDateKey", toDateKey);
    function toNoYearDateKey(month, day) {
        if (!month || month > 12 || month < 0) {
            month = 0;
        }
        if (!day || day > 31 || day < 0) {
            day = 0;
        }
        const monthStr = number_1.zeroPad(month, 2);
        const dayStr = number_1.zeroPad(day, 2);
        return `${monthStr}${dayStr}`;
    }
    exports_1("toNoYearDateKey", toNoYearDateKey);
    return {
        setters: [
            function (number_1_1) {
                number_1 = number_1_1;
            }
        ],
        execute: function () {
            dateKeyLength = "YYYYMMDD".length;
            dateKeyNoYearLength = "MMDD".length;
            exports_1("default", {
                getYear,
                getMonth,
                getDay,
                toDateKey,
                toNoYearDateKey
            });
        }
    };
});
//# sourceMappingURL=dateKey.js.map