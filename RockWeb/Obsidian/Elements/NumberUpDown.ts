import { defineComponent, PropType } from '../Vendor/Vue/vue.js';

export default defineComponent({
    name: 'NumberUpDown',
    props: {
        modelValue: {
            type: Number as PropType<number>,
            required: true
        },
        min: {
            type: Number as PropType<number>,
            default: 1
        },
        max: {
            type: Number as PropType<number>,
            default: 9
        }
    },
    methods: {
        goUp() {
            if (!this.isUpDisabled) {
                this.$emit('update:modelValue', this.modelValue + 1);
            }
        },
        goDown() {
            if (!this.isDownDisabled) {
                this.$emit('update:modelValue', this.modelValue - 1);
            }
        }
    },
    computed: {
        isUpDisabled(): boolean {
            return this.modelValue >= this.max;
        },
        isDownDisabled(): boolean {
            return this.modelValue <= this.min;
        }
    },
    template: `
<div class="numberincrement">
    <a @click="goDown" class="numberincrement-down" :class="{disabled: isDownDisabled}" :disabled="isDownDisabled">
        <i class="fa fa-minus "></i>
    </a>
    <span class="numberincrement-value">{{modelValue}}</span>
    <a @click="goUp" class="numberincrement-up" :class="{disabled: isUpDisabled}" :disabled="isUpDisabled">
        <i class="fa fa-plus "></i>
    </a>
</div>`
});
