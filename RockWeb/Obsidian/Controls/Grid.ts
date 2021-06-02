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
import JavaScriptAnchor from '../Elements/JavaScriptAnchor';

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
    components: {
        JavaScriptAnchor
    },
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
        pageSize: {
            type: Number as PropType<number>,
            default: 50
        },
        currentPageIndex: {
            type: Number as PropType<number>,
            default: 1
        },
        rowItemText: {
            type: String as PropType<string>,
            default: 'Entity'
        },
        rowCountOverride: {
            type: Number as PropType<number>,
            default: 0
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
    computed: {
        /** The number of rows in the dataset */
        rowCount (): number
        {
            if ( this.rowCountOverride )
            {
                return this.rowCountOverride;
            }

            return this.gridData.length;
        },

        /** How many pages are needed to display all of the rows */
        pageCount (): number
        {
            return Math.ceil( this.rowCount / this.pageSize );
        },

        currentPageSet (): number[]
        {
            const pagesPerSet = 10;
            const firstNumber = Math.floor( this.currentPageIndex / pagesPerSet ) * pagesPerSet + 1;
            const set: number[] = [];

            for ( let i = 0; i < pagesPerSet; i++ )
            {
                const pageIndex = firstNumber + i;

                if ( pageIndex <= this.pageCount )
                {
                    set.push( pageIndex );
                }
            }

            return set;
        }
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
        },

        /**
         * Set the number of rows per page
         * @param pageSize
         */
        setPageSize ( pageSize: number )
        {
            this.$emit( 'update:pageSize', pageSize );
        },

        /**
         * Set the current page index
         * @param pageIndex
         */
        setPageIndex ( pageIndex: number )
        {
            this.$emit( 'update:currentPageIndex', pageIndex );
        },

        /** Go to the previous page set */
        goToPreviousPageSet ()
        {
            const lowestPageInCurrentSet = this.currentPageSet[ 0 ] || 0;

            if ( lowestPageInCurrentSet <= 1 )
            {
                return;
            }

            this.setPageIndex( lowestPageInCurrentSet - 1 );
        },

        /** Go to the next page set */
        goToNextPageSet ()
        {
            const lastIndex = this.currentPageSet.length - 1;
            const highestPageInCurrentSet = this.currentPageSet[ lastIndex ] || 0;

            if ( highestPageInCurrentSet <= 1 )
            {
                return;
            }

            if ( highestPageInCurrentSet >= this.pageCount )
            {
                return;
            }

            this.setPageIndex( highestPageInCurrentSet + 1 );
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
        <tfoot>
            <tr>
                <td class="grid-paging" colspan="6">
                    <ul class="grid-pagesize pagination pagination-sm">
                        <li :class="pageSize === 50 ? 'active' : ''">
                            <JavaScriptAnchor @click="setPageSize(50)">50</JavaScriptAnchor>
                        </li>
                        <li :class="pageSize === 500 ? 'active' : ''">
                            <JavaScriptAnchor @click="setPageSize(500)">500</JavaScriptAnchor>
                        </li>
                        <li :class="pageSize === 5000 ? 'active' : ''">
                            <JavaScriptAnchor @click="setPageSize(5000)">5000</JavaScriptAnchor>
                        </li>
                    </ul>
                    <div class="grid-itemcount">{{rowCount}} {{rowItemText}}</div>
                    <ul v-if="pageCount > 1" class="grid-pager pagination pagination-sm">
                        <li class="prev disabled">
                            <JavaScriptAnchor @click="goToPreviousPageSet" class="aspNetDisabled">«</JavaScriptAnchor>
                        </li>
                        <li v-for="pageIndex in currentPageSet" :key="pageIndex" :class="pageIndex === currentPageIndex ? 'active' : ''">
                            <JavaScriptAnchor @click="setPageIndex(pageIndex)">{{pageIndex}}</JavaScriptAnchor>
                        </li>
                        <li class="next disabled">
                            <JavaScriptAnchor @click="goToNextPageSet" class="aspNetDisabled">»</JavaScriptAnchor>
                        </li>
                    </ul>
                </td>
            </tr>
            <tr>
                <td class="grid-actions" colspan="6">
                    <JavaScriptAnchor title="Communicate" class="btn btn-grid-action btn-communicate btn-default btn-sm"><i class="fa fa-comment fa-fw"></i></JavaScriptAnchor>
                    <JavaScriptAnchor title="Merge Person Records" class="btn btn-grid-action btn-merge btn-default btn-sm"><i class="fa fa-users fa-fw"></i></JavaScriptAnchor>
                    <JavaScriptAnchor title="Bulk Update" class="btn btn-grid-action btn-bulk-update btn-default btn-sm"><i class="fa fa-truck fa-fw"></i></JavaScriptAnchor>
                    <JavaScriptAnchor title="Launch Workflow" class="btn-grid-action btn-launch-workflow btn btn-default btn-sm"><i class="fa fa-cog fa-fw"></i></JavaScriptAnchor>
                    <JavaScriptAnchor title="Export to Excel" class="btn btn-grid-action btn-excelexport btn-default btn-sm"><i class="fa fa-table fa-fw"></i></JavaScriptAnchor>
                    <JavaScriptAnchor title="Merge Records into Merge Template" class="btn btn-grid-action btn-merge-template btn-default btn-sm"><i class="fa fa-files-o fa-fw"></i></JavaScriptAnchor>
                    <JavaScriptAnchor accesskey="n" title="Alt+N" class="btn btn-grid-action btn-add btn-default btn-sm"><i class="fa fa-plus-circle fa-fw"></i></JavaScriptAnchor>
                </td>
            </tr>
        </tfoot>
    </table>
</div>`
} );
