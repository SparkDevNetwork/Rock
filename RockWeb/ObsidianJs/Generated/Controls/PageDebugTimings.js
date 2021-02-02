System.register(["../Filters/Number.js", "../Vendor/Vue/vue.js"], function (exports_1, context_1) {
    "use strict";
    var Number_js_1, vue_js_1, PageDebugTimingRow;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (Number_js_1_1) {
                Number_js_1 = Number_js_1_1;
            },
            function (vue_js_1_1) {
                vue_js_1 = vue_js_1_1;
            }
        ],
        execute: function () {
            PageDebugTimingRow = vue_js_1.defineComponent({
                name: 'PageDebugTimingRow',
                props: {
                    viewModel: {
                        type: Object,
                        required: true
                    },
                    startTimeMs: {
                        type: Number,
                        required: true
                    },
                    endTimeMs: {
                        type: Number,
                        required: true
                    }
                },
                methods: {
                    numberAsFormattedString: Number_js_1.asFormattedString
                },
                computed: {
                    indentStyle: function () {
                        var pixels = this.viewModel.IndentLevel * 24;
                        return "padding-left: " + pixels + "px";
                    },
                    waterfallTitle: function () {
                        var timestampString = this.numberAsFormattedString(this.viewModel.TimestampMs, 2);
                        var durationString = this.numberAsFormattedString(this.viewModel.DurationMs, 2);
                        return "Started at " + timestampString + " ms / Duration " + durationString + " ms";
                    },
                    totalMs: function () {
                        return this.endTimeMs - this.startTimeMs;
                    },
                    getPercentFromMs: function () {
                        var _this = this;
                        return function (ms) {
                            if (!_this.totalMs) {
                                return 0;
                            }
                            var msFromStart = ms - _this.startTimeMs;
                            return (msFromStart / _this.totalMs) * 100;
                        };
                    },
                    waterfallStyle: function () {
                        var leftPercent = this.getPercentFromMs(this.viewModel.TimestampMs);
                        var widthPercent = this.getPercentFromMs(this.viewModel.DurationMs);
                        return "left: " + leftPercent + "%; width: " + widthPercent + "%;";
                    }
                },
                template: "\n<tr>\n    <td class=\"debug-timestamp\">{{numberAsFormattedString(viewModel.TimestampMs, 2)}} ms</td>\n    <td :style=\"indentStyle\">\n        <strong v-if=\"viewModel.IsTitleBold\">{{viewModel.Title}}</strong>\n        <template v-else>{{viewModel.Title}}</template>\n        <small v-if=\"viewModel.SubTitle\" style=\"color:#A4A4A4\">{{viewModel.SubTitle}}</small>\n    </td>\n    <td class=\"debug-timestamp\">{{numberAsFormattedString(viewModel.DurationMs, 2)}} ms</td>\n    <td class=\"debug-waterfall\">\n        <span class=\"debug-chart-bar\" :title=\"waterfallTitle\" :style=\"waterfallStyle\"></span>\n    </td>\n</tr>"
            });
            exports_1("default", vue_js_1.defineComponent({
                name: 'PageDebugTimings',
                components: {
                    PageDebugTimingRow: PageDebugTimingRow
                },
                props: {
                    viewModels: {
                        type: Array,
                        required: true
                    }
                },
                computed: {
                    startTimeMs: function () {
                        if (!this.viewModels.length) {
                            return 0;
                        }
                        return this.viewModels[0].TimestampMs;
                    },
                    endTimeMs: function () {
                        if (!this.viewModels.length) {
                            return 0;
                        }
                        var lastIndex = this.viewModels.length - 1;
                        var lastViewModel = this.viewModels[lastIndex];
                        return lastViewModel.TimestampMs + lastViewModel.DurationMs;
                    }
                },
                template: "\n<span>\n    <table class=\"table table-bordered table-striped debug-timings\" style=\"width:100%; margin-bottom: 48px;\">\n        <thead>\n            <tr>\n                <th class=\"debug-timestamp\">Timestamp</th>\n                <th>Event</th>\n                <th class=\"debug-timestamp\">Duration</th>\n                <th class=\"debug-waterfall\">Waterfall</th>\n            </tr>\n        </thead>\n        <tbody>\n            <PageDebugTimingRow v-for=\"(vm, i) in viewModels\" :key=\"`${i}-${vm.TimestampMs}`\" :viewModel=\"vm\" :startTimeMs=\"startTimeMs\" :endTimeMs=\"endTimeMs\" />\n        </tbody>\n    </table>\n</span>"
            }));
        }
    };
});
//# sourceMappingURL=PageDebugTimings.js.map