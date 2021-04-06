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
import { defineComponent } from 'vue';

/** Provides a generic Rock Block structure */
const PaneledBlockTemplate = defineComponent( {
    name: 'PaneledBlockTemplate',
    data()
    {
        return {
            isDrawerOpen: false
        };
    },
    methods: {
        onDrawerPullClick()
        {
            this.isDrawerOpen = !this.isDrawerOpen;
        }
    },
    template: `
<div class="panel panel-block">
    <div class="panel-heading rollover-container">
        <h1 class="panel-title pull-left">
            <slot name="title" />
        </h1>
        <slot name="titleAside" />
    </div>
    <div v-if="$slots.drawer" class="panel-drawer rock-panel-drawer" :class="isDrawerOpen ? 'open' : ''">
        <div class="drawer-content" v-show="isDrawerOpen">
            <slot name="drawer" />
        </div>
        <div class="drawer-pull" @click="onDrawerPullClick">
            <i :class="isDrawerOpen ? 'fa fa-chevron-up' : 'fa fa-chevron-down'"></i>
        </div>
    </div>
    <div class="panel-body">
        <div class="block-content">
            <slot />
        </div>
    </div>
</div>`
} );

export default PaneledBlockTemplate;