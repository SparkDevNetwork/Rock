System.register([], function (exports_1, context_1) {
    "use strict";
    var __moduleName = context_1 && context_1.id;
    // <copyright>
    // Copyright by the Spark Development Network
    //
    // Licensed under the Rock Community License (the "License");
    // you may not use this file except in compliance with the License.
    // You may obtain a copy of the License at
    //
    // http://www.rockrms.com/license
    //
    // Unless required by applicable law or agreed to in writing, software
    // distributed under the License is distributed on an "AS IS" BASIS,
    // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    // See the License for the specific language governing permissions and
    // limitations under the License.
    // </copyright>
    //
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
    /**
     * Returns an English comma-and fragment.
     * Ex: ['a', 'b', 'c'] => 'a, b, and c'
     * @param strs
     */
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
    /**
     * Convert the string to the title case.
     * hellO worlD => Hello World
     * @param str
     */
    function toTitleCase(str) {
        if (!str) {
            return '';
        }
        return str.replace(/\w\S*/g, function (word) {
            return word.charAt(0).toUpperCase() + word.substr(1).toLowerCase();
        });
    }
    exports_1("toTitleCase", toTitleCase);
    /**
     * Returns a singular or plural phrase depending on if the number is 1.
     * (0, Cat, Cats) => 0 Cats
     * (1, Cat, Cats) => Cat
     * (2, Cat, Cats) => 2 Cats
     * @param num
     * @param singular
     * @param plural
     */
    function pluralPhrase(num, singular, plural) {
        return num === 1 ? singular : num + " " + plural;
    }
    exports_1("pluralPhrase", pluralPhrase);
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
                pluralPhrase: pluralPhrase
            });
        }
    };
});
//# sourceMappingURL=String.js.map