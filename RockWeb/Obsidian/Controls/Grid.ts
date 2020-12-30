import { defineComponent, PropType } from '../Vendor/Vue/vue.js';

export type FilterOptions = {
    Take: number;
    Skip: number;
};

export enum SortDirection {
    Ascending = 0,
    Descending = 1
}

export type SortProperty = {
    Property: string;
    Direction: SortDirection;
};

export type GridContext = {
    selectedRowIds: Record<number, boolean>;
    selectAllRows: boolean;
    sortProperty: SortProperty;
};

export type RowContext<T> = {
    rowData: T;
    isHeader: boolean;
    rowId: number;
};

export default function OfType<T>() {
    return defineComponent({
        name: 'Grid',
        props: {
            gridData: {
                type: Array as PropType<T[]>,
                required: true
            },
            rowIdKey: {
                type: String as PropType<string>,
                required: true
            },
            sortProperty: {
                type: Object as PropType<SortProperty>,
                required: true
            },
            rowItemText: {
                type: String as PropType<string>,
                default: 'Entity'
            }
        },
        data() {
            return {
                gridContext: {
                    selectedRowIds: {},
                    selectAllRows: false,
                    sortProperty: this.sortProperty
                } as GridContext
            };
        },
        watch: {
            gridData() {
                this.gridContext.selectedRowIds = {};

                for (const rowData of this.gridData) {
                    const rowId = rowData[this.rowIdKey] as number;
                    this.gridContext.selectedRowIds[rowId] = false;
                }
            },
            'gridContext.sortProperty': {
                deep: true,
                handler() {
                    this.$emit('update:sortProperty', this.gridContext.sortProperty);
                }
            },
        },
        methods: {
            getRowContext(rowData: T, isHeader: boolean): RowContext<T> {
                return {
                    rowData,
                    isHeader,
                    rowId: rowData[this.rowIdKey]
                };
            }
        },
        provide() {
            return {
                gridContext: this.gridContext
            };
        },
        template:
`<div class="table-responsive">
    <table class="grid-table table table-bordered table-striped table-hover">
        <thead>
            <slot :rowData="null" :isHeader="true" :rowId="null" />
        </thead>
        <tbody>
            <template v-if="!gridData.length">
                <tr data-original-title="" title="">
                    <td colspan="28">
                        <span class="table-empty">
                            No {{rowItemText}}s Found
                        </span>
                    </td>
                </tr>
            </template>
            <template v-else v-for="rowData in gridData" :key="rowData[rowIdKey]" >
                <slot v-bind="getRowContext(rowData, false, )" />
            </template>
        </tbody>
    </table>
</div>`
    });
}
