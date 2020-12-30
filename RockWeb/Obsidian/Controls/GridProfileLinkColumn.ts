import { defineComponent, PropType, inject } from '../Vendor/Vue/vue.js';
import { RowContext } from './Grid.js';
import GridColumn from './GridColumn.js';

export default function OfType<T>() {
    return defineComponent({
        name: 'GridProfileLinkColumn',
        components: {
            GridColumn: GridColumn<T>()
        },
        setup() {
            return {
                rowContext: inject('rowContext') as RowContext<T>
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
            personId(): number | null {
                return this.rowContext.rowData[this.property] || null;
            },
            url(): string {
                if (this.personId) {
                    return this.urlTemplate.replace('{id}', this.personId.toString());
                }

                return '';
            }
        },
        template:
`<GridColumn :rowContext="rowContext" class="grid-columncommand" align="center">
    <a v-if="url" @click.stop class="btn btn-default btn-sm" :href="url">
        <i class="fa fa-user"></i>
    </a>
</GridColumn>`
    });
}
