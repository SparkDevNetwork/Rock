System.register([], function (exports_1, context_1) {
    "use strict";
    var formatter;
    var __moduleName = context_1 && context_1.id;
    /**
     * Get a formatted currency string.
     * Ex: 1.2 => $1.20
     * @param num
     */
    function asFormattedString(num) {
        if (num === null) {
            return '';
        }
        return formatter.format(num);
    }
    exports_1("asFormattedString", asFormattedString);
    /**
     * Get a number value from a formatted string.
     * Ex: $1.20 => 1.2
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
            formatter = new Intl.NumberFormat('en-US', {
                style: 'currency',
                currency: 'USD'
            });
        }
    };
});
//# sourceMappingURL=Currency.js.map