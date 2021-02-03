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
import { ComponentPublicInstance, defineComponent, PropType } from 'vue';
import JavaScriptAnchor from './JavaScriptAnchor';

export default defineComponent({
    name: 'RockLabel',
    components: {
        JavaScriptAnchor
    },
    props: {
        help: {
            type: String as PropType<string>,
            default: ''
        }
    },
    mounted() {
        if (this.help) {
            const helpAnchor = this.$refs.help as ComponentPublicInstance;
            const jQuery = window['$'] as (el: unknown) => { tooltip: () => void };
            jQuery(helpAnchor.$el).tooltip();
        }
    },
    template: `
<label class="control-label">
    <slot />
    <JavaScriptAnchor v-if="help" ref="help" class="help" tabindex="-1" data-toggle="tooltip" data-placement="auto" data-container="body" data-html="true" title="" :data-original-title="help">
        <i class="fa fa-info-circle"></i>
    </JavaScriptAnchor>
</label>`
});
