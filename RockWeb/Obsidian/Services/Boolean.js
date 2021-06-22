System.register([], function (exports_1, context_1) {
    "use strict";
    var __moduleName = context_1 && context_1.id;
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
    exports_1("asBooleanOrNull", asBooleanOrNull);
    function asBoolean(val) {
        return !!asBooleanOrNull(val);
    }
    exports_1("asBoolean", asBoolean);
    function asYesNoOrNull(val) {
        var boolOrNull = asBooleanOrNull(val);
        if (boolOrNull === null) {
            return null;
        }
        return boolOrNull ? 'Yes' : 'No';
    }
    exports_1("asYesNoOrNull", asYesNoOrNull);
    function asTrueFalseOrNull(val) {
        var boolOrNull = asBooleanOrNull(val);
        if (boolOrNull === null) {
            return null;
        }
        return boolOrNull ? 'True' : 'False';
    }
    exports_1("asTrueFalseOrNull", asTrueFalseOrNull);
    return {
        setters: [],
        execute: function () {
        }
    };
});
//# sourceMappingURL=Boolean.js.map