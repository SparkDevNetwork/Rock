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
import { defineComponent, inject, ref } from 'vue';
import { GridContext, RowContext, RowId } from './Grid';
import GridColumn from './GridColumn';

export default defineComponent( {
    name: 'GridSelectColumn',
    components: {
        GridColumn
    },
    setup ()
    {
        const gridContext = inject( 'gridContext' ) as GridContext;
        const rowContext = inject( 'rowContext' ) as RowContext;

        const selectAllRows = gridContext.selectAllRows;
        const isThisRowSelected = gridContext.selectedRowIds[ rowContext.rowId ];
        const isSelected = ref( selectAllRows || isThisRowSelected );

        return {
            gridContext,
            rowContext,
            isSelected
        };
    },
    computed: {
        rowId (): RowId
        {
            return this.rowContext.rowId;
        },
        isHeader (): boolean
        {
            return this.rowContext.isHeader;
        }
    },
    watch: {
        'gridContext.selectAllRows' (): void
        {
            if ( !this.isHeader )
            {
                this.isSelected = this.gridContext.selectAllRows;
                this.gridContext.selectedRowIds[ this.rowId ] = this.isSelected;
            }
        },
        'gridContext.selectedRowIds' (): void
        {
            if ( !this.isHeader )
            {
                this.isSelected = this.gridContext.selectedRowIds[ this.rowId ];
            }
        },
        isSelected (): void
        {
            if ( !this.isHeader )
            {
                this.gridContext.selectedRowIds[ this.rowId ] = this.isSelected;
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
} );
