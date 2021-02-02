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
import { asFormattedString } from '../Filters/Number.js';
import { defineComponent, PropType } from '../Vendor/Vue/vue.js';

export type DebugTimingViewModel = {
    TimestampMs: number;
    Title: string;
    SubTitle: string;
    IndentLevel: number;
    DurationMs: number;
    IsTitleBold: boolean;
};

const PageDebugTimingRow = defineComponent({
    name: 'PageDebugTimingRow',
    props: {
        viewModel: {
            type: Object as PropType<DebugTimingViewModel>,
            required: true
        },
        startTimeMs: {
            type: Number as PropType<number>,
            required: true
        },
        endTimeMs: {
            type: Number as PropType<number>,
            required: true
        }
    },
    methods: {
        numberAsFormattedString: asFormattedString
    },
    computed: {
        indentStyle(): string {
            const pixels = this.viewModel.IndentLevel * 24;
            return `padding-left: ${pixels}px`;
        },
        waterfallTitle(): string {
            const timestampString = this.numberAsFormattedString(this.viewModel.TimestampMs, 2);
            const durationString = this.numberAsFormattedString(this.viewModel.DurationMs, 2);
            return `Started at ${timestampString} ms / Duration ${durationString} ms`;
        },
        totalMs(): number {
            return this.endTimeMs - this.startTimeMs;
        },
        getPercentFromMs(): (ms: number) => number {
            return (ms: number) => {
                if (!this.totalMs) {
                    return 0;
                }

                const msFromStart = ms - this.startTimeMs;
                return (msFromStart / this.totalMs) * 100;
            };
        },
        waterfallStyle(): string {
            const leftPercent = this.getPercentFromMs(this.viewModel.TimestampMs);
            const widthPercent = this.getPercentFromMs(this.viewModel.DurationMs);
            return `left: ${leftPercent}%; width: ${widthPercent}%;`;
        }
    },
    template: `
<tr>
    <td class="debug-timestamp">{{numberAsFormattedString(viewModel.TimestampMs, 2)}} ms</td>
    <td :style="indentStyle">
        <strong v-if="viewModel.IsTitleBold">{{viewModel.Title}}</strong>
        <template v-else>{{viewModel.Title}}</template>
        <small v-if="viewModel.SubTitle" style="color:#A4A4A4">{{viewModel.SubTitle}}</small>
    </td>
    <td class="debug-timestamp">{{numberAsFormattedString(viewModel.DurationMs, 2)}} ms</td>
    <td class="debug-waterfall">
        <span class="debug-chart-bar" :title="waterfallTitle" :style="waterfallStyle"></span>
    </td>
</tr>`
});

export default defineComponent({
    name: 'PageDebugTimings',
    components: {
        PageDebugTimingRow
    },
    props: {
        viewModels: {
            type: Array as PropType<DebugTimingViewModel[]>,
            required: true
        }
    },
    computed: {
        startTimeMs(): number {
            if (!this.viewModels.length) {
                return 0;
            }

            return this.viewModels[0].TimestampMs;
        },
        endTimeMs(): number {
            if (!this.viewModels.length) {
                return 0;
            }

            const lastIndex = this.viewModels.length - 1;
            const lastViewModel = this.viewModels[lastIndex];
            return lastViewModel.TimestampMs + lastViewModel.DurationMs;
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
            <PageDebugTimingRow v-for="(vm, i) in viewModels" :key="\`\${i}-\${vm.TimestampMs}\`" :viewModel="vm" :startTimeMs="startTimeMs" :endTimeMs="endTimeMs" />
        </tbody>
    </table>
</span>`
});
