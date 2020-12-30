import { defineComponent, inject, ref } from '../Vendor/Vue/vue.js';
import { GridContext, RowContext } from './Grid.js';
import GridColumn from './GridColumn.js';

export default function OfType<T>() {
    return defineComponent({
        name: 'GridSelectColumn',
        components: {
            GridColumn: GridColumn<T>()
        },
        setup() {
            const gridContext = inject('gridContext') as GridContext;
            const rowContext = inject('rowContext') as RowContext<T>;

            const selectAllRows = gridContext.selectAllRows;
            const isThisRowSelected = gridContext.selectedRowIds[rowContext.rowId];
            const isSelected = ref(selectAllRows || isThisRowSelected);

            return {
                gridContext,
                rowContext,
                isSelected
            };
        },
        computed: {
            rowId(): number {
                return this.rowContext.rowId;
            },
            isHeader(): boolean {
                return this.rowContext.isHeader;
            }
        },
        watch: {
            'gridContext.selectAllRows'(): void {
                if (!this.isHeader) {
                    this.isSelected = this.gridContext.selectAllRows;
                    this.gridContext.selectedRowIds[this.rowId] = this.isSelected;
                }
            },
            'gridContext.selectedRowIds'(): void {
                if (!this.isHeader) {
                    this.isSelected = this.gridContext.selectedRowIds[this.rowId];
                }
            },
            isSelected(): void {
                if (!this.isHeader) {
                    this.gridContext.selectedRowIds[this.rowId] = this.isSelected;
                }
            }
        },
        template: `
<GridColumn class="grid-select-field" align="center">
    <template #header>
        <div @click.stop class="checkbox">
            <label title="">
                <input type="checkbox" class="select-all" v-model="gridContext.selectAllRows" />
                <span class="label-text">&nbsp;</span>
            </label>
        </div>
    </template>
    <template #default>
        <div @click.stop class="checkbox">
            <label title="">
                <input type="checkbox" class="select-all" v-model="isSelected" />
                <span class="label-text">&nbsp;</span>
            </label>
        </div>
    </template>
</GridColumn>`
    });
}
