System.register(["vue", "../../../Controls/saveFinancialAccountForm"], function (exports_1, context_1) {
    "use strict";
    var vue_1, saveFinancialAccountForm_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (saveFinancialAccountForm_1_1) {
                saveFinancialAccountForm_1 = saveFinancialAccountForm_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: "Event.RegistrationEntry.Success",
                components: {
                    SaveFinancialAccountForm: saveFinancialAccountForm_1.default
                },
                setup() {
                    return {
                        registrationEntryState: vue_1.inject("registrationEntryState")
                    };
                },
                computed: {
                    registrationTerm() {
                        return this.registrationEntryState.viewModel.registrationTerm.toLowerCase();
                    },
                    messageHtml() {
                        var _a;
                        return ((_a = this.registrationEntryState.successViewModel) === null || _a === void 0 ? void 0 : _a.messageHtml) || `You have successfully completed this ${this.registrationTerm}`;
                    },
                    gatewayGuid() {
                        return this.registrationEntryState.viewModel.gatewayGuid;
                    },
                    transactionCode() {
                        var _a;
                        return this.registrationEntryState.viewModel.isRedirectGateway ?
                            "" :
                            ((_a = this.registrationEntryState.successViewModel) === null || _a === void 0 ? void 0 : _a.transactionCode) || "";
                    },
                    gatewayPersonIdentifier() {
                        var _a;
                        return ((_a = this.registrationEntryState.successViewModel) === null || _a === void 0 ? void 0 : _a.gatewayPersonIdentifier) || "";
                    },
                    enableSaveAccount() {
                        return this.registrationEntryState.viewModel.enableSaveAccount && this.registrationEntryState.savedAccountGuid === null;
                    }
                },
                template: `
<div>
    <div v-html="messageHtml"></div>
    <SaveFinancialAccountForm v-if="gatewayGuid && transactionCode && gatewayPersonIdentifier && enableSaveAccount"
        :gatewayGuid="gatewayGuid"
        :transactionCode="transactionCode"
        :gatewayPersonIdentifier="gatewayPersonIdentifier"
        class="well">
        <template #header>
            <h3>Make Payments Even Easier</h3>
        </template>
    </SaveFinancialAccountForm>
</div>`
            }));
        }
    };
});
//# sourceMappingURL=success.js.map