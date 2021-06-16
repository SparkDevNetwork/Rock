System.register(["../Util/RockDate"], function (exports_1, context_1) {
    "use strict";
    var RockDate_1;
    var __moduleName = context_1 && context_1.id;
    function asDateOrNull(val) {
        if (val === undefined || val === null) {
            return null;
        }
        if (val instanceof Date) {
            return val;
        }
        if (typeof val === 'string') {
            var ms = Date.parse(val);
            if (isNaN(ms)) {
                return null;
            }
            return RockDate_1.default.stripTimezone(new Date(ms));
        }
        return null;
    }
    exports_1("asDateOrNull", asDateOrNull);
    function toRockDateOrNull(val) {
        var date = asDateOrNull(val);
        if (date === null) {
            return null;
        }
        return RockDate_1.default.toRockDate(date);
    }
    exports_1("toRockDateOrNull", toRockDateOrNull);
    function asDateString(val) {
        var dateOrNull = asDateOrNull(val);
        if (!dateOrNull) {
            return '';
        }
        return dateOrNull.toLocaleDateString();
    }
    exports_1("asDateString", asDateString);
    function asElapsedString(dateTime) {
        var now = new Date();
        var msPerHour = 1000 * 60 * 60;
        var hoursPerDay = 24;
        var daysPerMonth = 30.4167;
        var daysPerYear = 365.25;
        var totalMs = Math.abs(now.getTime() - dateTime.getTime());
        var totalHours = totalMs / msPerHour;
        var totalDays = totalHours / hoursPerDay;
        if (totalDays < 2) {
            return '1day';
        }
        if (totalDays < 31) {
            return Math.floor(totalDays) + "days";
        }
        var totalMonths = totalDays / daysPerMonth;
        if (totalMonths <= 1) {
            return '1mon';
        }
        if (totalMonths <= 18) {
            return Math.round(totalMonths) + "mon";
        }
        var totalYears = totalDays / daysPerYear;
        if (totalYears <= 1) {
            return '1yr';
        }
        return Math.round(totalYears) + "yrs";
    }
    exports_1("asElapsedString", asElapsedString);
    return {
        setters: [
            function (RockDate_1_1) {
                RockDate_1 = RockDate_1_1;
            }
        ],
        execute: function () {
        }
    };
});
//# sourceMappingURL=Date.js.map