Obsidian.Controls.registerControl({
    name: 'GridSelectColumn',
    components: {
        GridColumn: Obsidian.Controls.GridColumn
    },
    inject: [
        'gridContext',
        'rowContext'
    ],
    data() {
        return {
            isSelected: this.gridContext.selectAllRows || this.gridContext.selectedRowIds[this.rowId]
        };
    },
    computed: {
        rowId() {
            return this.rowContext.rowId;
        },
        isHeader() {
            return this.rowContext.isHeader;
        }
    },
    watch: {
        'gridContext.selectAllRows'() {
            if (!this.isHeader) {
                this.isSelected = this.gridContext.selectAllRows;
                this.gridContext.selectedRowIds[this.rowId] = this.isSelected;
            }
        },
        'gridContext.selectedRowIds'() {
            if (!this.isHeader) {
                this.isSelected = this.gridContext.selectedRowIds[this.rowId];
            }
        },
        isSelected() {
            if (!this.isHeader) {
                this.gridContext.selectedRowIds[this.rowId] = this.isSelected;
            }
        }
    },
    template:
`<GridColumn class="grid-select-field" align="center">
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
});
