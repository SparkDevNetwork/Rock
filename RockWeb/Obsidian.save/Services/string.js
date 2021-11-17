System.register([], function (exports_1, context_1) {
    "use strict";
    var escapeHtmlRegExp, escapeHtmlMap;
    var __moduleName = context_1 && context_1.id;
    function isEmpty(val) {
        if (typeof val === "string") {
            return val.length === 0;
        }
        return false;
    }
    exports_1("isEmpty", isEmpty);
    function isWhiteSpace(val) {
        if (typeof val === "string") {
            return val.trim().length === 0;
        }
        return false;
    }
    exports_1("isWhiteSpace", isWhiteSpace);
    function isNullOrWhiteSpace(val) {
        return isWhiteSpace(val) || val === undefined || val === null;
    }
    exports_1("isNullOrWhiteSpace", isNullOrWhiteSpace);
    function splitCamelCase(val) {
        return val.replace(/([a-z])([A-Z])/g, "$1 $2");
    }
    exports_1("splitCamelCase", splitCamelCase);
    function asCommaAnd(strs) {
        if (strs.length === 0) {
            return "";
        }
        if (strs.length === 1) {
            return strs[0];
        }
        if (strs.length === 2) {
            return `${strs[0]} and ${strs[1]}`;
        }
        const last = strs.pop();
        return `${strs.join(", ")}, and ${last}`;
    }
    exports_1("asCommaAnd", asCommaAnd);
    function toTitleCase(str) {
        if (!str) {
            return "";
        }
        return str.replace(/\w\S*/g, (word) => {
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
            return `${str.substring(0, 3)}-${str.substring(3, 7)}`;
        }
        if (str.length === 10) {
            return `(${str.substring(0, 3)}) ${str.substring(3, 6)}-${str.substring(6, 10)}`;
        }
        return str;
    }
    exports_1("formatPhoneNumber", formatPhoneNumber);
    function stripPhoneNumber(str) {
        if (!str) {
            return "";
        }
        return str.replace(/\D/g, "");
    }
    exports_1("stripPhoneNumber", stripPhoneNumber);
    function padLeft(str, length, padCharacter = " ") {
        if (padCharacter == "") {
            padCharacter = " ";
        }
        else if (padCharacter.length > 1) {
            padCharacter = padCharacter.substr(0, 1);
        }
        if (!str) {
            return Array(length).join(padCharacter);
        }
        if (str.length >= length) {
            return str;
        }
        return Array(length - str.length + 1).join(padCharacter) + str;
    }
    exports_1("padLeft", padLeft);
    function padRight(str, length, padCharacter = " ") {
        if (padCharacter == "") {
            padCharacter = " ";
        }
        else if (padCharacter.length > 1) {
            padCharacter = padCharacter.substr(0, 1);
        }
        if (!str) {
            return Array(length).join(padCharacter);
        }
        if (str.length >= length) {
            return str;
        }
        return str + Array(length - str.length + 1).join(padCharacter);
    }
    exports_1("padRight", padRight);
    function truncate(str, limit, options) {
        if (str.length <= limit) {
            return str;
        }
        const trimmable = "\u0009\u000A\u000B\u000C\u000D\u0020\u00A0\u1680\u180E\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200A\u202F\u205F\u2028\u2029\u3000\uFEFF";
        const reg = new RegExp(`(?=[${trimmable}])`);
        const words = str.split(reg);
        let count = 0;
        if (options && options.ellipsis === true) {
            limit -= 3;
        }
        const visibleWords = words.filter(function (word) {
            count += word.length;
            return count <= limit;
        });
        return `${visibleWords.join("")}...`;
    }
    exports_1("truncate", truncate);
    function escapeHtml(str) {
        return str.replace(escapeHtmlRegExp, (ch) => {
            return escapeHtmlMap[ch];
        });
    }
    exports_1("escapeHtml", escapeHtml);
    return {
        setters: [],
        execute: function () {
            escapeHtmlRegExp = /["'&<>]/g;
            escapeHtmlMap = {
                '"': "&quot;",
                "&": "&amp;",
                "'": "&#39;",
                "<": "&lt;",
                ">": "&gt;"
            };
            exports_1("default", {
                asCommaAnd,
                escapeHtml,
                splitCamelCase,
                isNullOrWhiteSpace,
                isWhiteSpace,
                isEmpty,
                toTitleCase,
                pluralConditional,
                formatPhoneNumber,
                stripPhoneNumber,
                padLeft,
                padRight,
                truncate
            });
        }
    };
});
//# sourceMappingURL=string.js.map