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
System.register(["vue", "./Registrant"], function (exports_1, context_1) {
    "use strict";
    var vue_1, Registrant_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (Registrant_1_1) {
                Registrant_1 = Registrant_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'Event.RegistrationEntry.Registrant',
                components: {
                    Registrant: Registrant_1.default
                },
                props: {
                    registrants: {
                        type: Array,
                        required: true
                    }
                },
                data: function () {
                    return {
                        currentRegistrantIndex: 0
                    };
                },
                template: "\n<div class=\"registrationentry-registrant\">\n    <h1>{{currentRegistrantTitle}}</h1>\n    <ProgressBar :percent=\"completionPercentInt\" />\n\n    <Registrant v-for=\"(r, i) in registrants\" v-if=\"currentRegistrantIndex === i\" :currentRegistrantIndex=\"i\" :key=\"r.Guid\" />\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=Registrants.js.map