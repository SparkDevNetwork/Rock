System.register(["vue", "./gridColumn"], function (exports_1, context_1) {
    "use strict";
    var vue_1, gridColumn_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (gridColumn_1_1) {
                gridColumn_1 = gridColumn_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: "GridSelectColumn",
                components: {
                    GridColumn: gridColumn_1.default
                },
                setup() {
                    const gridContext = vue_1.inject("gridContext");
                    const rowContext = vue_1.inject("rowContext");
                    const selectAllRows = gridContext.selectAllRows;
                    const isThisRowSelected = gridContext.selectedRowIds[rowContext.rowId];
                    const isSelected = vue_1.ref(selectAllRows || isThisRowSelected);
                    return {
                        gridContext,
                        rowContext,
                        isSelected
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
                    "gridContext.selectAllRows"() {
                        if (!this.isHeader) {
                            this.isSelected = this.gridContext.selectAllRows;
                            this.gridContext.selectedRowIds[this.rowId] = this.isSelected;
                        }
                    },
                    "gridContext.selectedRowIds"() {
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
            }));
        }
    };
});
//# sourceMappingURL=gridSelectColumn.js.map