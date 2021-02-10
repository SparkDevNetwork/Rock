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
     * Convert a date to a RockDate
     * @param d
     */
    function toRockDate(d) {
        return d.toISOString().split('T')[0];
    }
    exports_1("toRockDate", toRockDate);
    /**
    * Generates a new Rock Date
    */
    function newDate() {
        return toRockDate(new Date());
    }
    exports_1("newDate", newDate);
    return {
        setters: [],
        execute: function () {
            exports_1("default", {
                newDate: newDate,
                toRockDate: toRockDate
            });
        }
    };
});
//# sourceMappingURL=RockDate.js.map