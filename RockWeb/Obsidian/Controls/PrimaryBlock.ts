import { defineComponent, PropType } from '../Vendor/Vue/vue.js';
import store from '../Store/Index.js';

export default defineComponent({
    name: 'PrimaryBlock',
    props: {
        hideSecondaryBlocks: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },
    methods: {
        setAreSecondaryBlocksShown(isVisible): void {
            store.commit('setAreSecondaryBlocksShown', { areSecondaryBlocksShown: isVisible });
        }
    },
    watch: {
        hideSecondaryBlocks() {
            this.setAreSecondaryBlocksShown(!this.hideSecondaryBlocks);
        }
    },
    template: `<slot />`
});
