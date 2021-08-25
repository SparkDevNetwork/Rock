System.register([], function (exports_1, context_1) {
    "use strict";
    var __moduleName = context_1 && context_1.id;
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
    function toNumber(str) {
        return toNumberOrNull(str) || 0;
    }
    exports_1("toNumber", toNumber);
    function toNumberOrNull(str) {
        if (str === null) {
            return null;
        }
        var replaced = str.replace(/[$,]/g, '');
        return Number(replaced) || 0;
    }
    exports_1("toNumberOrNull", toNumberOrNull);
    function toOrdinalSuffix(num) {
        if (!num) {
            return '';
        }
        var j = num % 10;
        var k = num % 100;
        if (j == 1 && k != 11) {
            return num + 'st';
        }
        if (j == 2 && k != 12) {
            return num + 'nd';
        }
        if (j == 3 && k != 13) {
            return num + 'rd';
        }
        return num + 'th';
    }
    exports_1("toOrdinalSuffix", toOrdinalSuffix);
    function toOrdinal(num) {
        if (!num) {
            return '';
        }
        switch (num) {
            case 1: return 'first';
            case 2: return 'second';
            case 3: return 'third';
            case 4: return 'fourth';
            case 5: return 'fifth';
            case 6: return 'sixth';
            case 7: return 'seventh';
            case 8: return 'eighth';
            case 9: return 'ninth';
            case 10: return 'tenth';
            default: return toOrdinalSuffix(num);
        }
    }
    exports_1("toOrdinal", toOrdinal);
    function toWord(num) {
        switch (num) {
            case 1: return 'one';
            case 2: return 'two';
            case 3: return 'three';
            case 4: return 'four';
            case 5: return 'five';
            case 6: return 'six';
            case 7: return 'seven';
            case 8: return 'eight';
            case 9: return 'nine';
            case 10: return 'ten';
            default: return "" + num;
        }
    }
    exports_1("toWord", toWord);
    function zeroPad(num, length) {
        var str = num.toString();
        while (str.length < length) {
            str = '0' + str;
        }
        return str;
    }
    exports_1("zeroPad", zeroPad);
    return {
        setters: [],
        execute: function () {
            exports_1("default", {
                toOrdinal: toOrdinal,
                toOrdinalSuffix: toOrdinalSuffix,
                toNumberOrNull: toNumberOrNull,
                asFormattedString: asFormattedString
            });
        }
    };
});
//# sourceMappingURL=Number.js.map