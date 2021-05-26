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
System.register(["vue", "../../../Controls/SaveFinancialAccountForm"], function (exports_1, context_1) {
    "use strict";
    var vue_1, SaveFinancialAccountForm_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (SaveFinancialAccountForm_1_1) {
                SaveFinancialAccountForm_1 = SaveFinancialAccountForm_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'Event.RegistrationEntry.Success',
                components: {
                    SaveFinancialAccountForm: SaveFinancialAccountForm_1.default
                },
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
                    },
                    /** The financial gateway record's guid */
                    gatewayGuid: function () {
                        return this.registrationEntryState.ViewModel.GatewayGuid;
                    },
                    /** The transaction code that can be used to create a saved account */
                    transactionCode: function () {
                        var _a;
                        return this.registrationEntryState.ViewModel.IsRedirectGateway ?
                            '' :
                            ((_a = this.registrationEntryState.SuccessViewModel) === null || _a === void 0 ? void 0 : _a.TransactionCode) || '';
                    },
                    /** The token returned for the payment method */
                    gatewayPersonIdentifier: function () {
                        var _a;
                        return ((_a = this.registrationEntryState.SuccessViewModel) === null || _a === void 0 ? void 0 : _a.GatewayPersonIdentifier) || '';
                    }
                },
                template: "\n<div>\n    <div v-html=\"messageHtml\"></div>\n    <SaveFinancialAccountForm v-if=\"gatewayGuid && transactionCode && gatewayPersonIdentifier\" :gatewayGuid=\"gatewayGuid\" :transactionCode=\"transactionCode\" :gatewayPersonIdentifier=\"gatewayPersonIdentifier\" class=\"well\">\n        <template #header>\n            <h3>Make Payments Even Easier</h3>\n        </template>\n    </SaveFinancialAccountForm>\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=Success.js.map