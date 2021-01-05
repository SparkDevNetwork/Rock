define(["require", "exports"], function (require, exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.splitCamelCase = exports.isNullOrWhitespace = exports.isWhitespace = exports.isEmpty = void 0;
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
    exports.isEmpty = isEmpty;
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
    exports.isWhitespace = isWhitespace;
    /**
     * Is the value null, undefined or whitespace?
     * @param val
     */
    function isNullOrWhitespace(val) {
        return isWhitespace(val) || val === undefined || val === null;
    }
    exports.isNullOrWhitespace = isNullOrWhitespace;
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
    exports.splitCamelCase = splitCamelCase;
});
//# sourceMappingURL=String.js.map