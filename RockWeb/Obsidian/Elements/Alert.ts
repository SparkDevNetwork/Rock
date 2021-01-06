import { defineComponent } from '../Vendor/Vue/vue.js';

export default defineComponent({
    name: 'Alert',
    props: {
        dismissible: {
            type: Boolean,
            default: false
        },
    },
    emits: [
        'dismiss'
    ],
    methods: {
        onDismiss: function () {
            this.$emit('dismiss');
        }
    },
    template:
`<div class="alert">
    <button v-if="dismissible" type="button" class="close" @click="onDismiss">
        <span>&times;</span>
    </button>
    <slot />
</div>`
});
