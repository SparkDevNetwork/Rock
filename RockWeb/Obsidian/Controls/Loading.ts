import LoadingIndicator from '../Elements/LoadingIndicator.js';
import { defineComponent, PropType } from '../Vendor/Vue/vue.js';

export default defineComponent({
    name: 'Loading',
    components: {
        LoadingIndicator
    },
    props: {
        isLoading: {
            type: Boolean as PropType<boolean>,
            required: true
        }
    },
    template: `
<div>
    <slot v-if="!isLoading" />
    <LoadingIndicator v-else />
</div>`
});
