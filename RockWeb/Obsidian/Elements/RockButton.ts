import { defineComponent, PropType } from '../Vendor/Vue/vue.js';

export default defineComponent({
    name: 'RockButton',
    props: {
        isLoading: {
            type: Boolean as PropType<boolean>,
            default: false
        },
        loadingText: {
            type: String as PropType<string>,
            default: 'Loading...'
        },
        type: {
            type: String as PropType<string>,
            default: 'button'
        },
        disabled: {
            type: Boolean,
            default: false
        }
    },
    emits: [
        'click'
    ],
    methods: {
        handleClick: function (event: Event) {
            this.$emit('click', event);
        }
    },
    template:
`<button class="btn" :disabled="isLoading || disabled" @click="handleClick" :type="type">
    <template v-if="isLoading">
        {{loadingText}}
    </template>
    <slot v-else />
</button>`
});
