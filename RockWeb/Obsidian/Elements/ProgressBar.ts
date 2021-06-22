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
import { defineComponent, PropType } from 'vue';

export default defineComponent({
    name: 'ProgressBar',
    props: {
        percent: {
            type: Number as PropType<number>,
            required: true
        }
    },
    computed: {
        boundedPercent(): number {
            if (this.percent < 0) {
                return 0;
            }

            if (this.percent > 100) {
                return 100;
            }

            return this.percent;
        },
        roundedBoundedPercent(): number {
            return Math.round(this.boundedPercent);
        },
        style(): string {
            return `width: ${this.boundedPercent}%;`;
        }
    },
    template: `
<div class="progress">
    <div class="progress-bar" role="progressbar" :aria-valuenow="roundedBoundedPercent" aria-valuemin="0" aria-valuemax="100" :style="style">
        <span class="sr-only">{{roundedBoundedPercent}}% Complete</span>
    </div>
</div>`
});