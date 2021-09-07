System.register([], function (exports_1, context_1) {
    "use strict";
    var __moduleName = context_1 && context_1.id;
    function stripTimezone(val) {
        var asUtc = new Date(val.getTime() + val.getTimezoneOffset() * 60000);
        return asUtc;
    }
    exports_1("stripTimezone", stripTimezone);
    function toRockDate(d) {
        if (d instanceof Date && !isNaN(d)) {
            return stripTimezone(d).toISOString().split('T')[0];
        }
        return '';
    }
    exports_1("toRockDate", toRockDate);
    function newDate() {
        return toRockDate(new Date());
    }
    exports_1("newDate", newDate);
    function getDay(d) {
        if (!d) {
            return null;
        }
        var asDate = stripTimezone(new Date(d));
        return asDate.getDate();
    }
    exports_1("getDay", getDay);
    function getMonth(d) {
        if (!d) {
            return null;
        }
        var asDate = stripTimezone(new Date(d));
        return asDate.getMonth() + 1;
    }
    exports_1("getMonth", getMonth);
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