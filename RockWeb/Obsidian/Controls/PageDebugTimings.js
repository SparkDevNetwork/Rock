System.register(["../Services/Number", "vue", "../Store/Index"], function (exports_1, context_1) {
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
    var Number_1, vue_1, Index_1, PageDebugTimingRow;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (Number_1_1) {
                Number_1 = Number_1_1;
            },
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (Index_1_1) {
                Index_1 = Index_1_1;
            }
        ],
        execute: function () {
            PageDebugTimingRow = vue_1.defineComponent({
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
                    totalMs: {
                        type: Number,
                        required: true
                    }
                },
                methods: {
                    numberAsFormattedString: Number_1.asFormattedString
                },
                computed: {
                    indentStyle: function () {
                        if (!this.viewModel.IndentLevel) {
                            return '';
                        }
                        var pixels = this.viewModel.IndentLevel * 24;
                        return "padding-left: " + pixels + "px";
                    },
                    waterfallTitle: function () {
                        var timestampString = this.numberAsFormattedString(this.viewModel.TimestampMs, 2);
                        var durationString = this.numberAsFormattedString(this.viewModel.DurationMs, 2);
                        return "Started at " + timestampString + " ms / Duration " + durationString + " ms";
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
                template: "\n<tr>\n    <td class=\"debug-timestamp\">{{numberAsFormattedString(viewModel.TimestampMs, 2)}} ms</td>\n    <td :style=\"indentStyle\">\n        <strong v-if=\"viewModel.IsTitleBold\">\n            {{viewModel.Title}}\n        </strong>\n        <template v-else>\n            {{viewModel.Title}}\n        </template>\n        <small v-if=\"viewModel.SubTitle\" style=\"color:#A4A4A4; padding-left: 3px;\">\n            {{viewModel.SubTitle}}\n        </small>\n    </td>\n    <td class=\"debug-timestamp\">{{numberAsFormattedString(viewModel.DurationMs, 2)}} ms</td>\n    <td class=\"debug-waterfall\">\n        <span class=\"debug-chart-bar\" :title=\"waterfallTitle\" :style=\"waterfallStyle\"></span>\n    </td>\n</tr>"
            });
            exports_1("default", vue_1.defineComponent({
                name: 'PageDebugTimings',
                components: {
                    PageDebugTimingRow: PageDebugTimingRow
                },
                props: {
                    serverViewModels: {
                        type: Array,
                        required: true
                    }
                },
                computed: {
                    serverStartTimeMs: function () {
                        if (!this.serverViewModels.length) {
                            return 0;
                        }
                        return this.serverViewModels[0].TimestampMs;
                    },
                    serverEndTimeMs: function () {
                        if (!this.serverViewModels.length) {
                            return 0;
                        }
                        var lastIndex = this.serverViewModels.length - 1;
                        var lastViewModel = this.serverViewModels[lastIndex];
                        return lastViewModel.TimestampMs + lastViewModel.DurationMs;
                    },
                    firstClientRelativeStartTimeMs: function () {
                        if (!this.relativeClientViewModels.length) {
                            return 0;
                        }
                        var viewModel = this.relativeClientViewModels[0];
                        return viewModel.TimestampMs;
                    },
                    clientRelativeEndTimeMs: function () {
                        if (!this.relativeClientViewModels.length) {
                            return 0;
                        }
                        var lastIndex = this.relativeClientViewModels.length - 1;
                        var lastViewModel = this.relativeClientViewModels[lastIndex];
                        return lastViewModel.TimestampMs + lastViewModel.DurationMs;
                    },
                    totalMs: function () {
                        return this.clientRelativeEndTimeMs - this.serverStartTimeMs;
                    },
                    clientViewModels: function () {
                        return Index_1.default.state.debugTimings;
                    },
                    relativeClientViewModels: function () {
                        var _this = this;
                        return this.clientViewModels.map(function (vm) { return (__assign(__assign({}, vm), { TimestampMs: _this.serverEndTimeMs + vm.TimestampMs })); });
                    },
                    clientHeader: function () {
                        return {
                            DurationMs: this.firstClientRelativeStartTimeMs - this.serverEndTimeMs,
                            IndentLevel: 0,
                            IsTitleBold: true,
                            Title: 'Client Mount Blocks',
                            TimestampMs: this.serverEndTimeMs,
                            SubTitle: ''
                        };
                    }
                },
                template: "\n<span>\n    <table class=\"table table-bordered table-striped debug-timings\" style=\"width:100%; margin-bottom: 48px;\">\n        <thead>\n            <tr>\n                <th class=\"debug-timestamp\">Timestamp</th>\n                <th>Event</th>\n                <th class=\"debug-timestamp\">Duration</th>\n                <th class=\"debug-waterfall\">Waterfall</th>\n            </tr>\n        </thead>\n        <tbody>\n            <PageDebugTimingRow v-for=\"(vm, i) in serverViewModels\" :key=\"`s${i}-${vm.TimestampMs}`\" :viewModel=\"vm\" :startTimeMs=\"serverStartTimeMs\" :totalMs=\"totalMs\" />\n            <PageDebugTimingRow :viewModel=\"clientHeader\" :startTimeMs=\"serverStartTimeMs\" :totalMs=\"totalMs\" />\n            <PageDebugTimingRow v-for=\"(vm, i) in relativeClientViewModels\" :key=\"`c${i}-${vm.TimestampMs}`\" :viewModel=\"vm\" :startTimeMs=\"serverStartTimeMs\" :totalMs=\"totalMs\" />\n        </tbody>\n    </table>\n</span>"
            }));
        }
    };
});
//# sourceMappingURL=PageDebugTimings.js.map