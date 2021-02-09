// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
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
    sortProperty: SortProperty | null;
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
                type: Object as PropType<SortProperty | null>,
                default: null
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
