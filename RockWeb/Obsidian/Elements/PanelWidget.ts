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
import RockButton from './RockButton';

export default defineComponent({
    name: 'PanelWidget',
    components: {
        RockButton
    },
    props: {
        isDefaultOpen: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },
    data() {
        return {
            isOpen: this.isDefaultOpen
        };
    },
    methods: {
        toggle() {
            this.isOpen = !this.isOpen;
        }
    },
    template: `
<section class="panel panel-widget rock-panel-widget">
    <header class="panel-heading clearfix clickable" @click="toggle">
        <div class="pull-left">
            <slot name="header" />
        </div>
        <div class="pull-right">
            <RockButton btnType="link" btnSize="xs">
                <i v-if="isOpen" class="fa fa-chevron-up"></i>
                <i v-else class="fa fa-chevron-down"></i>
            </RockButton>
        </div>
    </header>
    <div v-if="isOpen" class="panel-body">
        <slot />
    </div>
</section>`
});