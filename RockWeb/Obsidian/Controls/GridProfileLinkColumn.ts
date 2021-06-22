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
import { defineComponent, PropType, inject } from 'vue';
import { RowContext } from './Grid';
import GridColumn from './GridColumn';

export default defineComponent( {
    name: 'GridProfileLinkColumn',
    components: {
        GridColumn
    },
    setup ()
    {
        return {
            rowContext: inject( 'rowContext' ) as RowContext
        };
    },
    props: {
        property: {
            type: String as PropType<string>,
            default: 'PersonId'
        },
        urlTemplate: {
            type: String as PropType<string>,
            default: '/person/{id}'
        }
    },
    computed: {
        personId (): number | null
        {
            return ( this.rowContext.rowData[ this.property ] as number ) || null;
        },
        url (): string
        {
            if ( this.personId )
            {
                return this.urlTemplate.replace( '{id}', this.personId.toString() );
            }

            return '';
        }
    },
    template: `
<GridColumn :rowContext="rowContext" class="grid-columncommand" align="center">
    <a v-if="url" @click.stop class="btn btn-default btn-sm" :href="url">
        <i class="fa fa-user"></i>
    </a>
</GridColumn>`
} );
