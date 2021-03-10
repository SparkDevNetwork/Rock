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
System.register([], function (exports_1, context_1) {
    "use strict";
    var __moduleName = context_1 && context_1.id;
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
    exports_1("asBooleanOrNull", asBooleanOrNull);
    /**
     * Transform the value into true or false
     * @param val
     */
    function asBoolean(val) {
        return !!asBooleanOrNull(val);
    }
    exports_1("asBoolean", asBoolean);
    /** Transform the value into the strings "Yes", "No", or null */
    function asYesNoOrNull(val) {
        var boolOrNull = asBooleanOrNull(val);
        if (boolOrNull === null) {
            return null;
        }
        return boolOrNull ? 'Yes' : 'No';
    }
    exports_1("asYesNoOrNull", asYesNoOrNull);
    /** Transform the value into the strings "True", "False", or null */
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