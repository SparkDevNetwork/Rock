import { defineComponent, PropType, inject } from '../Vendor/Vue/vue.js';
import { GridContext, RowContext, SortDirection, SortProperty } from './Grid.js';

export default function OfType<T>() {
    return defineComponent({
        name: 'GridColumn',
        props: {
            title: {
                type: String as PropType<string>,
                default: ''
            },
            property: {
                type: String as PropType<string>,
                default: ''
            },
            sortExpression: {
                type: String as PropType<string>,
                default: ''
            }
        },
        setup() {
            return {
                gridContext: inject('gridContext') as GridContext,
                rowContext: inject('rowContext') as RowContext<T>
            };
        },
        computed: {
            mySortExpression(): string {
                return this.sortExpression || this.property;
            },
            sortProperty(): SortProperty {
                return this.gridContext.sortProperty;
            },
            isCurrentlySorted(): boolean {
                return !!this.mySortExpression && this.sortProperty.Property === this.mySortExpression;
            },
            isCurrentlySortedDesc(): boolean {
                return this.isCurrentlySorted && this.sortProperty.Direction === SortDirection.Descending;
            },
            isCurrentlySortedAsc(): boolean {
                return this.isCurrentlySorted && this.sortProperty.Direction === SortDirection.Ascending;
            }
        },
        methods: {
            onHeaderClick() {
                this.$emit('click:header', this.property);

                if (this.mySortExpression) {
                    if (this.isCurrentlySortedAsc) {
                        this.sortProperty.Direction = SortDirection.Descending;
                    }
                    else {
                        this.sortProperty.Property = this.mySortExpression;
                        this.sortProperty.Direction = SortDirection.Ascending;
                    }
                }
            },
        },
        template: `
<th
    v-if="rowContext.isHeader"
    scope="col"
    @click="onHeaderClick"
    :class="isCurrentlySortedAsc ? 'ascending' : isCurrentlySortedDesc ? 'descending' : ''">
    <a v-if="mySortExpression" href="javascript:void(0);">
        <slot name="header">
            {{title}}
        </slot>
    </a>
    <template v-else>
        <slot name="header">
            {{title}}
        </slot>
    </template>
</th>
<td v-else class="grid-select-cell">
    <slot>
        {{rowContext.rowData[property]}}
    </slot>
</td>`
    });
}
