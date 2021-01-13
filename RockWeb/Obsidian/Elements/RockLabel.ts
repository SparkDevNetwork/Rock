import { ComponentPublicInstance, defineComponent, PropType } from '../Vendor/Vue/vue.js';
import JavaScriptAnchor from './JavaScriptAnchor.js';

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
