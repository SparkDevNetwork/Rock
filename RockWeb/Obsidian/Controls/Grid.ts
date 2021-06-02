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
import { defineComponent, PropType } from 'vue';

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
    selectedRowIds: Record<string, boolean>;
    selectAllRows: boolean;
    sortProperty: SortProperty | null;
};

export type RowData = Record<string, unknown>;
export type RowId = string;

export type RowContext = {
    rowData: RowData;
    isHeader: boolean;
    rowId: RowId;
};

export function getRowId ( rowData: RowData, rowIdKey: string ): RowId
{
    return `${rowData[ rowIdKey ]}`;
}

export default defineComponent( {
    name: 'Grid',
    props: {
        gridData: {
            type: Array as PropType<RowData[]>,
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
    data ()
    {
        return {
            gridContext: {
                selectedRowIds: {},
                selectAllRows: false,
                sortProperty: this.sortProperty
            } as GridContext
        };
    },
    watch: {
        gridData ()
        {
            this.gridContext.selectedRowIds = {};

            for ( const rowData of this.gridData )
            {
                const rowId = getRowId( rowData, this.rowIdKey );
                this.gridContext.selectedRowIds[ rowId ] = false;
            }
        },
        'gridContext.sortProperty': {
            deep: true,
            handler ()
            {
                this.$emit( 'update:sortProperty', this.gridContext.sortProperty );
            }
        },
    },
    methods: {
        getRowId,
        getRowContext ( rowData: RowData, isHeader: boolean ): RowContext
        {
            const rowId = getRowId( rowData, this.rowIdKey );

            return {
                rowData,
                isHeader,
                rowId
            };
        }
    },
    provide ()
    {
        return {
            gridContext: this.gridContext
        };
    },
    template: `
<div class="table-responsive">
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
            <template v-else v-for="rowData in gridData" :key="getRowId(rowData, rowIdKey)" >
                <slot v-bind="getRowContext(rowData, false, )" />
            </template>
        </tbody>
    </table>
</div>`
} );
