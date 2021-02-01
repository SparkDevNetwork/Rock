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
import { defineComponent, PropType } from '../Vendor/Vue/vue.js';
import JavaScriptAnchor from './JavaScriptAnchor.js';

export default defineComponent({
    name: 'Toggle',
    components: {
        JavaScriptAnchor
    },
    props: {
        modelValue: {
            type: Boolean as PropType<boolean>,
            required: true
        }
    },
    data() {
        return {
            selectedClasses: 'active btn btn-primary',
            unselectedClasses: 'btn btn-default'
        };
    },
    methods: {
        onClick(isOn: boolean) {
            this.$emit('update:modelValue', isOn);
        }
    },
    template: `
<div class="btn-group btn-toggle btn-group-justified">
    <JavaScriptAnchor :class="modelValue ? selectedClasses : unselectedClasses" @click="onClick(true)">
        <slot name="on">On</slot>
    </JavaScriptAnchor>
    <JavaScriptAnchor :class="modelValue ? unselectedClasses : selectedClasses" @click="onClick(false)">
        <slot name="off">Off</slot>
    </JavaScriptAnchor>
</div>`
});
