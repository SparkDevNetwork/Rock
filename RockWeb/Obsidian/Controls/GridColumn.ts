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
import JavaScriptAnchor from '../Elements/JavaScriptAnchor';
import { defineComponent, PropType, inject } from 'vue';
import { GridContext, RowContext, SortDirection, SortProperty } from './Grid';

export default defineComponent( {
    name: 'GridColumn',
    components: {
        JavaScriptAnchor
    },
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
    setup ()
    {
        return {
            gridContext: inject( 'gridContext' ) as GridContext,
            rowContext: inject( 'rowContext' ) as RowContext
        };
    },
    computed: {
        mySortExpression (): string
        {
            return this.sortExpression || this.property;
        },
        canSort (): boolean
        {
            return !!this.sortProperty;
        },
        sortProperty (): SortProperty | null
        {
            return this.gridContext.sortProperty;
        },
        isCurrentlySorted (): boolean
        {
            return !!this.mySortExpression && this.sortProperty?.Property === this.mySortExpression;
        },
        isCurrentlySortedDesc (): boolean
        {
            return this.isCurrentlySorted && this.sortProperty?.Direction === SortDirection.Descending;
        },
        isCurrentlySortedAsc (): boolean
        {
            return this.isCurrentlySorted && this.sortProperty?.Direction === SortDirection.Ascending;
        }
    },
    methods: {
        onHeaderClick ()
        {
            this.$emit( 'click:header', this.property );

            if ( this.mySortExpression && this.sortProperty )
            {
                if ( this.isCurrentlySortedAsc )
                {
                    this.sortProperty.Direction = SortDirection.Descending;
                }
                else
                {
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
    <JavaScriptAnchor v-if="mySortExpression && canSort">
        <slot name="header">
            {{title}}
        </slot>
    </JavaScriptAnchor>
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
} );
