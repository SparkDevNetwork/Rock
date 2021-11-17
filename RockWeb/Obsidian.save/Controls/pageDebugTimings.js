System.register(["../Services/number", "vue", "../Store/index"], function (exports_1, context_1) {
    "use strict";
    var number_1, vue_1, index_1, store, pageDebugTimingRow;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (number_1_1) {
                number_1 = number_1_1;
            },
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (index_1_1) {
                index_1 = index_1_1;
            }
        ],
        execute: function () {
            store = index_1.useStore();
            pageDebugTimingRow = vue_1.defineComponent({
                name: "PageDebugTimingRow",
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
                    numberAsFormattedString: number_1.asFormattedString
                },
                computed: {
                    indentStyle() {
                        if (!this.viewModel.indentLevel) {
                            return "";
                        }
                        const pixels = this.viewModel.indentLevel * 24;
                        return `padding-left: ${pixels}px`;
                    },
                    waterfallTitle() {
                        const timestampString = this.numberAsFormattedString(this.viewModel.timestampMs, 2);
                        const durationString = this.numberAsFormattedString(this.viewModel.durationMs, 2);
                        return `Started at ${timestampString} ms / Duration ${durationString} ms`;
                    },
                    getPercentFromMs() {
                        return (ms) => {
                            if (!this.totalMs) {
                                return 0;
                            }
                            const msFromStart = ms - this.startTimeMs;
                            return (msFromStart / this.totalMs) * 100;
                        };
                    },
                    waterfallStyle() {
                        const leftPercent = this.getPercentFromMs(this.viewModel.timestampMs);
                        const widthPercent = this.getPercentFromMs(this.viewModel.durationMs);
                        return `left: ${leftPercent}%; width: ${widthPercent}%;`;
                    }
                },
                template: `
<tr>
    <td class="debug-timestamp">{{numberAsFormattedString(viewModel.timestampMs, 2)}} ms</td>
    <td :style="indentStyle">
        <strong v-if="viewModel.isTitleBold">
            {{viewModel.title}}
        </strong>
        <template v-else>
            {{viewModel.title}}
        </template>
        <small v-if="viewModel.subTitle" style="color:#A4A4A4; padding-left: 3px;">
            {{viewModel.subTitle}}
        </small>
    </td>
    <td class="debug-timestamp">{{numberAsFormattedString(viewModel.durationMs, 2)}} ms</td>
    <td class="debug-waterfall">
        <span class="debug-chart-bar" :title="waterfallTitle" :style="waterfallStyle"></span>
    </td>
</tr>`
            });
            exports_1("default", vue_1.defineComponent({
                name: "PageDebugTimings",
                components: {
                    PageDebugTimingRow: pageDebugTimingRow
                },
                props: {
                    serverViewModels: {
                        type: Array,
                        required: true
                    }
                },
                computed: {
                    serverStartTimeMs() {
                        if (!this.serverViewModels.length) {
                            return 0;
                        }
                        return this.serverViewModels[0].timestampMs;
                    },
                    serverEndTimeMs() {
                        if (!this.serverViewModels.length) {
                            return 0;
                        }
                        const lastIndex = this.serverViewModels.length - 1;
                        const lastViewModel = this.serverViewModels[lastIndex];
                        return lastViewModel.timestampMs + lastViewModel.durationMs;
                    },
                    firstClientRelativeStartTimeMs() {
                        if (!this.relativeClientViewModels.length) {
                            return this.serverEndTimeMs;
                        }
                        const viewModel = this.relativeClientViewModels[0];
                        return viewModel.timestampMs;
                    },
                    clientRelativeEndTimeMs() {
                        if (!this.relativeClientViewModels.length) {
                            return this.serverEndTimeMs;
                        }
                        const lastIndex = this.relativeClientViewModels.length - 1;
                        const lastViewModel = this.relativeClientViewModels[lastIndex];
                        return lastViewModel.timestampMs + lastViewModel.durationMs;
                    },
                    totalMs() {
                        return this.clientRelativeEndTimeMs - this.serverStartTimeMs;
                    },
                    clientViewModels() {
                        return store.state.debugTimings;
                    },
                    relativeClientViewModels() {
                        return this.clientViewModels.map(vm => (Object.assign(Object.assign({}, vm), { timestampMs: this.serverEndTimeMs + vm.timestampMs })));
                    },
                    clientHeader() {
                        return {
                            durationMs: this.firstClientRelativeStartTimeMs - this.serverEndTimeMs,
                            indentLevel: 0,
                            isTitleBold: true,
                            title: "Client Mount Blocks",
                            timestampMs: this.serverEndTimeMs,
                            subTitle: ""
                        };
                    }
                },
                template: `
<span>
    <table class="table table-bordered table-striped debug-timings" style="width:100%; margin-bottom: 48px;">
        <thead>
            <tr>
                <th class="debug-timestamp">Timestamp</th>
                <th>Event</th>
                <th class="debug-timestamp">Duration</th>
                <th class="debug-waterfall">Waterfall</th>
            </tr>
        </thead>
        <tbody>
            <PageDebugTimingRow v-for="(vm, i) in serverViewModels" :key="\`s\${i}-\${vm.timestampMs}\`" :viewModel="vm" :startTimeMs="serverStartTimeMs" :totalMs="totalMs" />
            <PageDebugTimingRow :viewModel="clientHeader" :startTimeMs="serverStartTimeMs" :totalMs="totalMs" />
            <PageDebugTimingRow v-for="(vm, i) in relativeClientViewModels" :key="\`c\${i}-\${vm.timestampMs}\`" :viewModel="vm" :startTimeMs="serverStartTimeMs" :totalMs="totalMs" />
        </tbody>
    </table>
</span>`
            }));
        }
    };
});
//# sourceMappingURL=pageDebugTimings.js.map