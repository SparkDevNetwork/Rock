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
System.register(["vue"], function (exports_1, context_1) {
    "use strict";
    var vue_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'Event.RegistrationEntry.Success',
                setup: function () {
                    return {
                        registrationEntryState: vue_1.inject('registrationEntryState')
                    };
                },
                computed: {
                    /** The term to refer to a registrant */
                    registrationTerm: function () {
                        return this.registrationEntryState.ViewModel.RegistrationTerm.toLowerCase();
                    },
                    /** The success lava markup */
                    messageHtml: function () {
                        var _a;
                        return ((_a = this.registrationEntryState.SuccessViewModel) === null || _a === void 0 ? void 0 : _a.MessageHtml) || "You have successfully completed this " + this.registrationTerm;
                    }
                },
                template: "\n<div>\n    <div v-html=\"messageHtml\"></div>\n    <pre>{{JSON.stringify(registrationEntryState, null, 2)}}</pre>\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=Success.js.map