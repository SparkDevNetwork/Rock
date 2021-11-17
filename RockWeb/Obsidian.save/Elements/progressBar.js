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
                name: "ProgressBar",
                props: {
                    percent: {
                        type: Number,
                        required: true
                    }
                },
                computed: {
                    boundedPercent() {
                        if (this.percent < 0) {
                            return 0;
                        }
                        if (this.percent > 100) {
                            return 100;
                        }
                        return this.percent;
                    },
                    roundedBoundedPercent() {
                        return Math.round(this.boundedPercent);
                    },
                    style() {
                        return `width: ${this.boundedPercent}%;`;
                    }
                },
                template: `
<div class="progress">
    <div class="progress-bar" role="progressbar" :aria-valuenow="roundedBoundedPercent" aria-valuemin="0" aria-valuemax="100" :style="style">
        <span class="sr-only">{{roundedBoundedPercent}}% Complete</span>
    </div>
</div>`
            }));
        }
    };
});
//# sourceMappingURL=progressBar.js.map