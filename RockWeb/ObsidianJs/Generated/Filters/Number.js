System.register([], function (exports_1, context_1) {
    "use strict";
    var __moduleName = context_1 && context_1.id;
    /**
     * Get a formatted string.
     * Ex: 10001.2 => 10,001.2
     * @param num
     */
    function asFormattedString(num, digits) {
        if (digits === void 0) { digits = 2; }
        if (num === null) {
            return '';
        }
        return num.toLocaleString('en-US', {
            minimumFractionDigits: digits,
            maximumFractionDigits: digits
        });
    }
    exports_1("asFormattedString", asFormattedString);
    /**
     * Get a number value from a formatted string.
     * Ex: $1,000.20 => 1000.2
     * @param str
     */
    function toNumberOrNull(str) {
        if (str === null) {
            return null;
        }
        var replaced = str.replace(/[$,]/g, '');
        return Number(replaced) || 0;
    }
    exports_1("toNumberOrNull", toNumberOrNull);
    return {
        setters: [],
        execute: function () {
        }
    };
});
//# sourceMappingURL=Number.js.map