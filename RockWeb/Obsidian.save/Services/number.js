System.register([], function (exports_1, context_1) {
    "use strict";
    var __moduleName = context_1 && context_1.id;
    function asFormattedString(num, digits) {
        if (num === null) {
            return "";
        }
        return num.toLocaleString("en-US", {
            minimumFractionDigits: digits,
            maximumFractionDigits: digits !== null && digits !== void 0 ? digits : 9
        });
    }
    exports_1("asFormattedString", asFormattedString);
    function toNumber(str) {
        return toNumberOrNull(str) || 0;
    }
    exports_1("toNumber", toNumber);
    function toNumberOrNull(str) {
        if (str === null || str === undefined || str == "") {
            return null;
        }
        const replaced = str.replace(/[$,]/g, "");
        const num = Number(replaced);
        return !isNaN(num) ? num : null;
    }
    exports_1("toNumberOrNull", toNumberOrNull);
    function toCurrencyOrNull(value) {
        if (typeof value === "string") {
            value = toNumberOrNull(value);
        }
        if (value === null || value === undefined) {
            return null;
        }
        return "$" + asFormattedString(value, 2);
    }
    exports_1("toCurrencyOrNull", toCurrencyOrNull);
    function toOrdinalSuffix(num) {
        if (!num) {
            return "";
        }
        const j = num % 10;
        const k = num % 100;
        if (j == 1 && k != 11) {
            return num + "st";
        }
        if (j == 2 && k != 12) {
            return num + "nd";
        }
        if (j == 3 && k != 13) {
            return num + "rd";
        }
        return num + "th";
    }
    exports_1("toOrdinalSuffix", toOrdinalSuffix);
    function toOrdinal(num) {
        if (!num) {
            return "";
        }
        switch (num) {
            case 1: return "first";
            case 2: return "second";
            case 3: return "third";
            case 4: return "fourth";
            case 5: return "fifth";
            case 6: return "sixth";
            case 7: return "seventh";
            case 8: return "eighth";
            case 9: return "ninth";
            case 10: return "tenth";
            default: return toOrdinalSuffix(num);
        }
    }
    exports_1("toOrdinal", toOrdinal);
    function toWord(num) {
        if (num === null || num === undefined) {
            return "";
        }
        switch (num) {
            case 1: return "one";
            case 2: return "two";
            case 3: return "three";
            case 4: return "four";
            case 5: return "five";
            case 6: return "six";
            case 7: return "seven";
            case 8: return "eight";
            case 9: return "nine";
            case 10: return "ten";
            default: return `${num}`;
        }
    }
    exports_1("toWord", toWord);
    function zeroPad(num, length) {
        let str = num.toString();
        while (str.length < length) {
            str = "0" + str;
        }
        return str;
    }
    exports_1("zeroPad", zeroPad);
    return {
        setters: [],
        execute: function () {
            exports_1("default", {
                toOrdinal,
                toOrdinalSuffix,
                toNumberOrNull,
                asFormattedString
            });
        }
    };
});
//# sourceMappingURL=number.js.map