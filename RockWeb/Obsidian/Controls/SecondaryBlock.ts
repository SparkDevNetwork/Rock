import { defineComponent } from '../Vendor/Vue/vue.js';
import store from '../Store/Index.js';

export default defineComponent({
    name: 'SecondaryBlock',
    computed: {
        isVisible() {
            return store.state.areSecondaryBlocksShown;
        }
    },
    template:
`<div class="secondary-block">
    <slot v-if="isVisible" />
</div>`
});
