System.register([], function (exports_1, context_1) {
    "use strict";
    var __moduleName = context_1 && context_1.id;
    /**
     * Is the value an empty string?
     * @param val
     */
    function isEmpty(val) {
        if (typeof val === 'string') {
            return val.length === 0;
        }
        return false;
    }
    exports_1("isEmpty", isEmpty);
    /**
     * Is the value an empty string?
     * @param val
     */
    function isWhitespace(val) {
        if (typeof val === 'string') {
            return val.trim().length === 0;
        }
        return false;
    }
    exports_1("isWhitespace", isWhitespace);
    /**
     * Is the value null, undefined or whitespace?
     * @param val
     */
    function isNullOrWhitespace(val) {
        return isWhitespace(val) || val === undefined || val === null;
    }
    exports_1("isNullOrWhitespace", isNullOrWhitespace);
    /**
     * Turns "MyCamelCaseString" into "My Camel Case String"
     * @param val
     */
    function splitCamelCase(val) {
        if (typeof val === 'string') {
            return val.replace(/([a-z])([A-Z])/g, '$1 $2');
        }
        return val;
    }
    exports_1("splitCamelCase", splitCamelCase);
    return {
        setters: [],
        execute: function () {
        }
    };
});
//# sourceMappingURL=String.js.map