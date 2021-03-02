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
import { defineComponent } from '../../../Vendor/Vue/vue.js';
import PaneledBlockTemplate from '../../../Templates/PaneledBlockTemplate.js';
import store from '../../../Store/Index.js';

export default defineComponent({
    name: 'com_rocksolidchurchdemo.PageDebug.ContextGroup',
    components: {
        PaneledBlockTemplate
    },
    computed: {
        contextGroup() {
            return store.getters.groupContext || {};
        }
    },
    template:
`<PaneledBlockTemplate>
    <template v-slot:title>
        <i class="fa fa-grin-tongue-squint"></i>
        Context Group (TS Plugin)
    </template>
    <template v-slot:default>
        <dl>
            <dt>Group</dt>
            <dd>{{contextGroup.Name || '<none>'}}</dd>
        </dl>
    </template>
</PaneledBlockTemplate>`
});