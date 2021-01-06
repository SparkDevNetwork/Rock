import { defineComponent } from '../../../Vendor/Vue/vue.js';
import PaneledBlockTemplate from '../../../Templates/PaneledBlockTemplate.js';
import store from '../../../Store/Index.js';

export default defineComponent({
    name: 'org_rocksolidchurch.PageDebug.ContextGroup',
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
            <dd>{{contextGroup.Name}}</dd>
        </dl>
    </template>
</PaneledBlockTemplate>`
});