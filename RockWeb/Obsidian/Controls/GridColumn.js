Obsidian.Controls.registerControl({
    name: 'GridColumn',
    inject: [
        'gridContext',
        'rowContext'
    ],
    props: {
        title: {
            type: String,
            default: ''
        },
        property: {
            type: String,
            default: ''
        },
        sortExpression: {
            type: String,
            default: ''
        }
    },
    computed: {
        mySortExpression() {
            return this.sortExpression || this.property;
        },
        sortProperty() {
            return this.gridContext.sortProperty;
        },
        isCurrentlySorted() {
            return this.mySortExpression && this.sortProperty.property === this.mySortExpression;
        },
        isCurrentlySortedDesc() {
            return this.isCurrentlySorted && this.sortProperty.direction === 1;
        },
        isCurrentlySortedAsc() {
            return this.isCurrentlySorted && this.sortProperty.direction === 0;
        }
    },
    methods: {
        onHeaderClick() {
            this.$emit('click:header', this.property);

            if (this.mySortExpression) {
                if (this.isCurrentlySortedAsc) {
                    this.sortProperty.direction = 1;
                }
                else {
                    this.sortProperty.property = this.mySortExpression;
                    this.sortProperty.direction = 0;
                }
            }
        },
    },
    template:
`<th
    v-if="rowContext.isHeader"
    scope="col"
    @click="onHeaderClick"
    :class="isCurrentlySortedAsc ? 'ascending' : isCurrentlySortedDesc ? 'descending' : ''">
    <a v-if="mySortExpression" href="javascript:void(0);">
        <slot name="header">
            {{title}}
        </slot>
    </a>
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
});
