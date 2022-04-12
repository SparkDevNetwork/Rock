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
import { asFormattedString } from "../Services/number";
import { defineComponent, PropType } from "vue";
import { useStore } from "../Store/index";
import { DebugTiming } from "@Obsidian/ViewModels/Utility/debugTiming";

const store = useStore();

const pageDebugTimingRow = defineComponent({
    name: "PageDebugTimingRow",
    props: {
        viewModel: {
            type: Object as PropType<DebugTiming>,
            required: true
        },
        startTimeMs: {
            type: Number as PropType<number>,
            required: true
        },
        totalMs: {
            type: Number as PropType<number>,
            required: true
        }
    },
    methods: {
        numberAsFormattedString: asFormattedString
    },
    computed: {
        indentStyle(): string {
            if (!this.viewModel.indentLevel) {
                return "";
            }

            const pixels = this.viewModel.indentLevel * 24;
            return `padding-left: ${pixels}px`;
        },
        waterfallTitle(): string {
            const timestampString = this.numberAsFormattedString(this.viewModel.timestampMs, 2);
            const durationString = this.numberAsFormattedString(this.viewModel.durationMs, 2);
            return `Started at ${timestampString} ms / Duration ${durationString} ms`;
        },
        getPercentFromMs(): (ms: number) => number {
            return (ms: number): number => {
                if (!this.totalMs) {
                    return 0;
                }

                const msFromStart = ms - this.startTimeMs;
                return (msFromStart / this.totalMs) * 100;
            };
        },
        waterfallStyle(): string {
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

export default defineComponent({
    name: "PageDebugTimings",
    components: {
        PageDebugTimingRow: pageDebugTimingRow
    },
    props: {
        serverViewModels: {
            type: Array as PropType<DebugTiming[]>,
            required: true
        }
    },
    computed: {
        serverStartTimeMs(): number {
            if (!this.serverViewModels.length) {
                return 0;
            }

            return this.serverViewModels[0].timestampMs;
        },
        serverEndTimeMs(): number {
            if (!this.serverViewModels.length) {
                return 0;
            }

            const lastIndex = this.serverViewModels.length - 1;
            const lastViewModel = this.serverViewModels[lastIndex];
            return lastViewModel.timestampMs + lastViewModel.durationMs;
        },
        firstClientRelativeStartTimeMs(): number {
            if (!this.relativeClientViewModels.length) {
                return this.serverEndTimeMs;
            }

            const viewModel = this.relativeClientViewModels[0];
            return viewModel.timestampMs;
        },
        clientRelativeEndTimeMs(): number {
            if (!this.relativeClientViewModels.length) {
                return this.serverEndTimeMs;
            }

            const lastIndex = this.relativeClientViewModels.length - 1;
            const lastViewModel = this.relativeClientViewModels[lastIndex];
            return lastViewModel.timestampMs + lastViewModel.durationMs;
        },
        totalMs(): number {
            return this.clientRelativeEndTimeMs - this.serverStartTimeMs;
        },
        clientViewModels(): DebugTiming[] {
            return store.state.debugTimings;
        },
        relativeClientViewModels(): DebugTiming[] {
            // Add the server end time so they appear after the server
            return this.clientViewModels.map(vm => ({
                ...vm,
                timestampMs: this.serverEndTimeMs + vm.timestampMs
            } as DebugTiming));
        },
        clientHeader(): DebugTiming {
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
});
