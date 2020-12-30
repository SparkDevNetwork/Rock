import { defineComponent, PropType } from '../Vendor/Vue/vue.js';
import { RowContext } from './Grid.js';

export default function OfType<T>() {
    return defineComponent({
        name: 'GridRow',
        props: {
            rowContext: {
                type: Object as PropType<RowContext<T>>,
                required: true
            }
        },
        provide() {
            return {
                rowContext: this.rowContext
            };
        },
        methods: {
            onRowClick() {
                if (!this.rowContext.isHeader) {
                    this.$emit('click:body', this.rowContext);
                }
                else {
                    this.$emit('click:header', this.rowContext);
                }
            }
        },
        template:
`<tr @click="onRowClick">
    <slot />
</tr>`
    });
}
