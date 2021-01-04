define(["require", "exports"], function (require, exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.asTrueFalseOrNull = exports.asYesNoOrNull = exports.asBoolean = exports.asBooleanOrNull = void 0;
    /**
     * Transform the value into true, false, or null
     * @param val
     */
    function asBooleanOrNull(val) {
        if (val === undefined || val === null) {
            return null;
        }
        if (typeof val === 'boolean') {
            return val;
        }
        if (typeof val === 'string') {
            var asString = (val || '').trim().toLowerCase();
            if (!asString) {
                return null;
            }
            return ['true', 'yes', 't', 'y', '1'].indexOf(asString) !== -1;
        }
        if (typeof val === 'number') {
            return !!val;
        }
        return null;
    }
    exports.asBooleanOrNull = asBooleanOrNull;
    /**
     * Transform the value into true or false
     * @param val
     */
    function asBoolean(val) {
        return !!asBooleanOrNull(val);
    }
    exports.asBoolean = asBoolean;
    /** Transform the value into the strings "Yes", "No", or null */
    function asYesNoOrNull(val) {
        var boolOrNull = asBooleanOrNull(val);
        if (boolOrNull === null) {
            return null;
        }
        return boolOrNull ? 'Yes' : 'No';
    }
    exports.asYesNoOrNull = asYesNoOrNull;
    /** Transform the value into the strings "True", "False", or null */
    function asTrueFalseOrNull(val) {
        var boolOrNull = asBooleanOrNull(val);
        if (boolOrNull === null) {
            return null;
        }
        return boolOrNull ? 'True' : 'False';
    }
    exports.asTrueFalseOrNull = asTrueFalseOrNull;
});
//# sourceMappingURL=Boolean.js.map