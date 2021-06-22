System.register([], function (exports_1, context_1) {
    "use strict";
    var __moduleName = context_1 && context_1.id;
    function isEmpty(val) {
        if (typeof val === 'string') {
            return val.length === 0;
        }
        return false;
    }
    exports_1("isEmpty", isEmpty);
    function isWhitespace(val) {
        if (typeof val === 'string') {
            return val.trim().length === 0;
        }
        return false;
    }
    exports_1("isWhitespace", isWhitespace);
    function isNullOrWhitespace(val) {
        return isWhitespace(val) || val === undefined || val === null;
    }
    exports_1("isNullOrWhitespace", isNullOrWhitespace);
    function splitCamelCase(val) {
        if (typeof val === 'string') {
            return val.replace(/([a-z])([A-Z])/g, '$1 $2');
        }
        return val;
    }
    exports_1("splitCamelCase", splitCamelCase);
    function asCommaAnd(strs) {
        if (strs.length === 0) {
            return '';
        }
        if (strs.length === 1) {
            return strs[0];
        }
        if (strs.length === 2) {
            return strs[0] + " and " + strs[1];
        }
        var last = strs.pop();
        return strs.join(', ') + ", and " + last;
    }
    exports_1("asCommaAnd", asCommaAnd);
    function toTitleCase(str) {
        if (!str) {
            return '';
        }
        return str.replace(/\w\S*/g, function (word) {
            return word.charAt(0).toUpperCase() + word.substr(1).toLowerCase();
        });
    }
    exports_1("toTitleCase", toTitleCase);
    function pluralConditional(num, singular, plural) {
        return num === 1 ? singular : plural;
    }
    exports_1("pluralConditional", pluralConditional);
    function formatPhoneNumber(str) {
        str = stripPhoneNumber(str);
        if (str.length === 7) {
            return str.substring(0, 3) + "-" + str.substring(3, 7);
        }
        if (str.length === 10) {
            return "(" + str.substring(0, 3) + ") " + str.substring(3, 6) + "-" + str.substring(6, 10);
        }
        return str;
    }
    exports_1("formatPhoneNumber", formatPhoneNumber);
    function stripPhoneNumber(str) {
        if (!str) {
            return '';
        }
        return str.replace(/\D/g, '');
    }
    exports_1("stripPhoneNumber", stripPhoneNumber);
    return {
        setters: [],
        execute: function () {
            exports_1("default", {
                asCommaAnd: asCommaAnd,
                splitCamelCase: splitCamelCase,
                isNullOrWhitespace: isNullOrWhitespace,
                isWhitespace: isWhitespace,
                isEmpty: isEmpty,
                toTitleCase: toTitleCase,
                pluralConditional: pluralConditional,
                formatPhoneNumber: formatPhoneNumber,
                stripPhoneNumber: stripPhoneNumber
            });
        }
    };
});
//# sourceMappingURL=String.js.map