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
