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
                    registrationTerm: function () {
                        return this.registrationEntryState.ViewModel.registrationTerm.toLowerCase();
                    },
                    messageHtml: function () {
                        var _a;
                        return ((_a = this.registrationEntryState.SuccessViewModel) === null || _a === void 0 ? void 0 : _a.messageHtml) || "You have successfully completed this " + this.registrationTerm;
                    },
                    gatewayGuid: function () {
                        return this.registrationEntryState.ViewModel.gatewayGuid;
                    },
                    transactionCode: function () {
                        var _a;
                        return this.registrationEntryState.ViewModel.isRedirectGateway ?
                            '' :
                            ((_a = this.registrationEntryState.SuccessViewModel) === null || _a === void 0 ? void 0 : _a.transactionCode) || '';
                    },
                    gatewayPersonIdentifier: function () {
                        var _a;
                        return ((_a = this.registrationEntryState.SuccessViewModel) === null || _a === void 0 ? void 0 : _a.gatewayPersonIdentifier) || '';
                    }
                },
                template: "\n<div>\n    <div v-html=\"messageHtml\"></div>\n    <SaveFinancialAccountForm v-if=\"gatewayGuid && transactionCode && gatewayPersonIdentifier\" :gatewayGuid=\"gatewayGuid\" :transactionCode=\"transactionCode\" :gatewayPersonIdentifier=\"gatewayPersonIdentifier\" class=\"well\">\n        <template #header>\n            <h3>Make Payments Even Easier</h3>\n        </template>\n    </SaveFinancialAccountForm>\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=Success.js.map