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
System.register(["../Util/RockDate"], function (exports_1, context_1) {
    "use strict";
    var RockDate_1;
    var __moduleName = context_1 && context_1.id;
    /**
     * Transform the value into a date or null
     * @param val
     */
    function asDateOrNull(val) {
        if (val === undefined || val === null) {
            return null;
        }
        if (val instanceof Date) {
            return val;
        }
        if (typeof val === 'string') {
            var ms = Date.parse(val);
            if (isNaN(ms)) {
                return null;
            }
            return RockDate_1.default.stripTimezone(new Date(ms));
        }
        return null;
    }
    exports_1("asDateOrNull", asDateOrNull);
    /**
     * To a RockDate value.  Mon Dec 2 => 2000-12-02
     * @param val
     */
    function toRockDateOrNull(val) {
        var date = asDateOrNull(val);
        if (date === null) {
            return null;
        }
        return RockDate_1.default.toRockDate(date);
    }
    exports_1("toRockDateOrNull", toRockDateOrNull);
    /**
     * Transforms the value into a string like '9/13/2001'
     * @param val
     */
    function asDateString(val) {
        var dateOrNull = asDateOrNull(val);
        if (!dateOrNull) {
            return '';
        }
        return dateOrNull.toLocaleDateString();
    }
    exports_1("asDateString", asDateString);
    return {
        setters: [
            function (RockDate_1_1) {
                RockDate_1 = RockDate_1_1;
            }
        ],
        execute: function () {
        }
    };
});
//# sourceMappingURL=Date.js.map