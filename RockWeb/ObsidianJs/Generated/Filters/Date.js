define(["require", "exports"], function (require, exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.asDateString = exports.asDateOrNull = void 0;
    /**
     * Adjust for the timezone offset so early morning times don't appear as the previous local day.
     * @param val
     */
    function stripTimezone(val) {
        var asUtc = new Date(val.getTime() + val.getTimezoneOffset() * 60000);
        return asUtc;
    }
    /**
     * Transform the value into a date or null
     * @param val
     */
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
            return stripTimezone(new Date(ms));
        }
        return null;
    }
    exports.asDateOrNull = asDateOrNull;
    /**
     * Transforms the value into a string like '9/13/2001'
     * @param val
     */
    function asDateString(val) {
        var dateOrNull = asDateOrNull(val);
        if (!dateOrNull) {
            return '';
        }
        return dateOrNull.toLocaleDateString();
    }
    exports.asDateString = asDateString;
});
//# sourceMappingURL=Date.js.map