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

import { defineComponent, inject, PropType } from 'vue';
import Grid, { FilterOptions, RowData, SortDirection, SortProperty } from './Grid';
import { InvokeBlockActionFunc } from './RockBlock';

interface BlockActionGridResponse
{
    TotalCount: number,
    CurrentPageData: RowData[]
}

export default defineComponent( {
    name: 'BlockActionSourcedGrid',
    components: {
        Grid
    },
    props: {
        blockActionName: {
            type: String as PropType<string>,
            required: true
        },
        rowIdKey: {
            type: String as PropType<string>,
            required: true
        }
    },
    setup ()
    {
        return {
            invokeBlockAction: inject( 'invokeBlockAction' ) as InvokeBlockActionFunc
        };
    },
    data ()
    {
        return {
            pageSize: 50,
            totalRowCount: 0,
            currentPageIndex: 1,
            isLoading: false,
            errorMessage: '',
            sortProperty: {
                Direction: SortDirection.Ascending,
                Property: this.rowIdKey
            } as SortProperty,
            currentPageData: [] as RowData[]
        };
    },
    computed: {
        sortString (): string
        {
            return `${this.sortProperty.Property} ${this.sortProperty.Direction}`;
        }
    },
    methods: {
        async fetchData ()
        {
            if ( this.isLoading )
            {
                return;
            }

            this.isLoading = true;
            this.errorMessage = '';

            try
            {
                const result = await this.invokeBlockAction<BlockActionGridResponse>( this.blockActionName, {
                    filterOptions: {
                        Take: this.pageSize,
                        Skip: (this.currentPageIndex - 1) * this.pageSize
                    } as FilterOptions,
                    sortProperty: this.sortProperty
                } );

                if ( result.data && result.data.CurrentPageData )
                {
                    this.currentPageData = result.data.CurrentPageData;
                    this.totalRowCount = result.data.TotalCount;
                }
                else
                {
                    this.currentPageData = [];
                }
            }
            catch ( e )
            {
                this.errorMessage = `An exception occurred: ${e}`;
            }
            finally
            {
                this.isLoading = false;
            }
        }
    },
    watch: {
        async pageSize ()
        {
            if ( this.currentPageIndex > 1 )
            {
                this.currentPageIndex = 1;
            }
            else
            {
                await this.fetchData();
            }
        },
        async currentPageIndex ()
        {
            await this.fetchData();
        },
        async 'sortString' ()
        {
            await this.fetchData();
        }
    },
    async mounted ()
    {
        await this.fetchData();
    },
    template: `
<Grid
    :gridData="currentPageData"
    #default="rowContext"
    v-model:sortProperty="sortProperty"
    v-model:pageSize="pageSize"
    v-model:currentPageIndex="currentPageIndex"
    rowItemText="Group Member"
    :rowCountOverride="totalRowCount"
    :rowIdKey="rowIdKey">
    <slot v-bind="rowContext" />
</Grid>`
} );
