import { defineComponent, PropType } from '../Vendor/Vue/vue.js';

export default defineComponent({
    name: 'Alert',
    props: {
        dismissible: {
            type: Boolean,
            default: false
        },
        default: {
            type: Boolean as PropType<boolean>,
            default: false
        },
        success: {
            type: Boolean as PropType<boolean>,
            default: false
        },
        info: {
            type: Boolean as PropType<boolean>,
            default: false
        },
        danger: {
            type: Boolean as PropType<boolean>,
            default: false
        },
        warning: {
            type: Boolean as PropType<boolean>,
            default: false
        },
        primary: {
            type: Boolean as PropType<boolean>,
            default: false
        },
        validation: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },
    emits: [
        'dismiss'
    ],
    methods: {
        onDismiss: function () {
            this.$emit('dismiss');
        }
    },
    computed: {
        typeClass(): string {
            if (this.danger) {
                return 'alert-danger';
            }

            if (this.warning) {
                return 'alert-warning';
            }

            if (this.success) {
                return 'alert-success';
            }

            if (this.info) {
                return 'alert-info';
            }

            if (this.primary) {
                return 'alert-primary';
            }

            if (this.validation) {
                return 'alert-validation';
            }

            return 'btn-default';
        },
    },
    template: `
<div class="alert" :class="typeClass">
    <button v-if="dismissible" type="button" class="close" @click="onDismiss">
        <span>&times;</span>
    </button>
    <slot />
</div>`
});
