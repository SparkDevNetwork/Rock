System.register(["../Elements/Alert", "vue"], function (exports_1, context_1) {
    "use strict";
    var Alert_1, vue_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (Alert_1_1) {
                Alert_1 = Alert_1_1;
            },
            function (vue_1_1) {
                vue_1 = vue_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'RockValidation',
                components: {
                    Alert: Alert_1.default
                },
                props: {
                    errors: {
                        type: Object,
                        required: true
                    },
                    submitCount: {
                        type: Number,
                        required: true
                    }
                },
                data: function () {
                    return {
                        errorsToShow: {},
                        lastSubmitCount: 0,
                        lastErrorChangeMs: 0
                    };
                },
                computed: {
                    hasErrors: function () {
                        return Object.keys(this.errorsToShow).length > 0;
                    }
                },
                watch: {
                    errors: {
                        immediate: true,
                        handler: function () {
                            // There are errors that come in at different cycles. We don't want the screen jumping around as the
                            // user fixes errors. But, we do want the validations from the submit cycle to all get through even
                            // though they come at different times. The "debounce" 1000ms code is to try to allow all of those
                            // through, but then prevent changes once the user starts fixing the form.
                            var now = new Date().getTime();
                            var msSinceLastChange = now - this.lastErrorChangeMs;
                            this.lastErrorChangeMs = now;
                            var wasSubmitted = this.lastSubmitCount < this.submitCount;
                            if (msSinceLastChange > 1000 || !wasSubmitted) {
                                return;
                            }
                            this.errorsToShow = this.errors;
                            this.lastSubmitCount = this.submitCount;
                        }
                    }
                },
                template: "\n<Alert v-show=\"hasErrors\" alertType=\"validation\">\n    Please correct the following:\n    <ul>\n        <li v-for=\"(error, fieldLabel) of errorsToShow\">\n            <strong>{{fieldLabel}}</strong>\n            {{error}}\n        </li>\n    </ul>\n</Alert>"
            }));
        }
    };
});
//# sourceMappingURL=RockValidation.js.map