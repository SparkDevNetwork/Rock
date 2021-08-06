System.register(["../Elements/Alert", "vue"], function (exports_1, context_1) {
    "use strict";
    var __assign = (this && this.__assign) || function () {
        __assign = Object.assign || function(t) {
            for (var s, i = 1, n = arguments.length; i < n; i++) {
                s = arguments[i];
                for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p))
                    t[p] = s[p];
            }
            return t;
        };
        return __assign.apply(this, arguments);
    };
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
                        default: -1
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
                    submitCount: function () {
                        var wasSubmitted = this.lastSubmitCount < this.submitCount;
                        if (wasSubmitted) {
                            var now = new Date().getTime();
                            this.errorsToShow = __assign({}, this.errors);
                            this.lastErrorChangeMs = now;
                            this.lastSubmitCount = this.submitCount;
                        }
                    },
                    errors: {
                        immediate: true,
                        handler: function () {
                            if (this.submitCount === -1) {
                                this.errorsToShow = __assign({}, this.errors);
                                return;
                            }
                            var now = new Date().getTime();
                            var msSinceLastChange = now - this.lastErrorChangeMs;
                            if (msSinceLastChange < 500) {
                                this.errorsToShow = __assign({}, this.errors);
                                this.lastErrorChangeMs = now;
                            }
                        }
                    }
                },
                template: "\n<Alert v-show=\"hasErrors\" alertType=\"validation\">\n    Please correct the following:\n    <ul>\n        <li v-for=\"(error, fieldLabel) of errorsToShow\">\n            <strong>{{fieldLabel}}</strong>\n            {{error}}\n        </li>\n    </ul>\n</Alert>"
            }));
        }
    };
});
//# sourceMappingURL=RockValidation.js.map