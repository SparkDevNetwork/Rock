Obsidian.Controls.registerControl({
    name: 'Grid',
    props: {
        gridData: {
            type: Array,
            required: true
        },
        rowIdKey: {
            type: String,
            required: true
        },
        sortProperty: {
            type: Object,
            required: true
        },
        rowItemText: {
            type: String,
            default: 'Entity'
        }
    },
    data() {
        return {
            gridContext: {
                selectedRowIds: {},
                selectAllRows: false,
                sortProperty: this.sortProperty
            }
        };
    },
    watch: {
        gridData() {
            this.gridContext.selectedRowIds = {};

            for (const rowData of this.gridData) {
                const rowId = rowData[this.rowIdKey];
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
                <slot :rowData="rowData" :isHeader="false" :rowId="rowData[rowIdKey]" />
            </template>
        </tbody>
    </table>
</div>`
});
