System.register(["vue", "../Elements/alert", "../Elements/loadingIndicator"], function (exports_1, context_1) {
    "use strict";
    var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
        function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
        return new (P || (P = Promise))(function (resolve, reject) {
            function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
            function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
            function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
            step((generator = generator.apply(thisArg, _arguments || [])).next());
        });
    };
    var vue_1, alert_1, loadingIndicator_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (alert_1_1) {
                alert_1 = alert_1_1;
            },
            function (loadingIndicator_1_1) {
                loadingIndicator_1 = loadingIndicator_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: "ComponentFromUrl",
                components: {
                    LoadingIndicator: loadingIndicator_1.default,
                    Alert: alert_1.default
                },
                props: {
                    url: {
                        type: String,
                        required: true
                    }
                },
                data() {
                    return {
                        control: null,
                        loading: true,
                        error: ""
                    };
                },
                created() {
                    return __awaiter(this, void 0, void 0, function* () {
                        if (!this.url) {
                            this.error = `Could not load the control because no URL was provided`;
                            this.loading = false;
                            return;
                        }
                        try {
                            const controlComponentModule = yield context_1.import(this.url);
                            const control = controlComponentModule ?
                                (controlComponentModule.default || controlComponentModule) :
                                null;
                            if (control) {
                                this.control = vue_1.markRaw(control);
                            }
                        }
                        catch (e) {
                            console.error(e);
                            this.error = `Could not load the control for '${this.url}'`;
                        }
                        finally {
                            this.loading = false;
                            if (!this.control) {
                                this.error = `Could not load the control for '${this.url}'`;
                            }
                        }
                    });
                },
                template: `
<Alert v-if="error" alertType="danger">{{error}}</Alert>
<LoadingIndicator v-else-if="loading" />
<component v-else :is="control" />`
            }));
        }
    };
});
//# sourceMappingURL=componentFromUrl.js.map