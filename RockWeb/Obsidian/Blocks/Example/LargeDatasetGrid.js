System.register(["../../Templates/PaneledBlockTemplate", "vue", "../../Controls/GridRow", "../../Controls/GridColumn", "../../Controls/GridProfileLinkColumn", "../../Controls/BlockActionSourcedGrid", "../../Controls/Dialog"], function (exports_1, context_1) {
    "use strict";
    var PaneledBlockTemplate_1, vue_1, GridRow_1, GridColumn_1, GridProfileLinkColumn_1, BlockActionSourcedGrid_1, Dialog_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (PaneledBlockTemplate_1_1) {
                PaneledBlockTemplate_1 = PaneledBlockTemplate_1_1;
            },
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (GridRow_1_1) {
                GridRow_1 = GridRow_1_1;
            },
            function (GridColumn_1_1) {
                GridColumn_1 = GridColumn_1_1;
            },
            function (GridProfileLinkColumn_1_1) {
                GridProfileLinkColumn_1 = GridProfileLinkColumn_1_1;
            },
            function (BlockActionSourcedGrid_1_1) {
                BlockActionSourcedGrid_1 = BlockActionSourcedGrid_1_1;
            },
            function (Dialog_1_1) {
                Dialog_1 = Dialog_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'Example.LargeDatasetGrid',
                components: {
                    PaneledBlockTemplate: PaneledBlockTemplate_1.default,
                    BlockActionSourcedGrid: BlockActionSourcedGrid_1.default,
                    GridColumn: GridColumn_1.default,
                    GridRow: GridRow_1.default,
                    GridProfileLinkColumn: GridProfileLinkColumn_1.default,
                    Dialog: Dialog_1.default
                },
                data: function () {
                    return {
                        rowContextClicked: null,
                        isRowClickedDialogOpen: false
                    };
                },
                methods: {
                    onRowClick: function (rowContext) {
                        this.rowContextClicked = rowContext;
                        this.isRowClickedDialogOpen = true;
                    }
                },
                template: "\n<PaneledBlockTemplate>\n    <template #title>\n        <i class=\"fa fa-dumbbell\"></i>\n        Large Dataset Grid\n    </template>\n    <template #default>\n        <div class=\"grid grid-panel\">\n            <BlockActionSourcedGrid blockActionName=\"GetAttributeValues\" #default=\"rowContext\" rowItemText=\"Attribute Values\" rowIdKey=\"Id\">\n                <GridRow :rowContext=\"rowContext\" @click:body=\"onRowClick\">\n                    <GridColumn title=\"Id\" property=\"Id\" sortExpression=\"Id\" />\n                    <GridColumn title=\"Guid\" property=\"Guid\" sortExpression=\"Guid\" />\n                    <GridColumn title=\"Attribute\" property=\"Attribute\" sortExpression=\"Attribute.Id\" />\n                    <GridColumn title=\"Value\" property=\"Value\" sortExpression=\"Value\" />\n                </GridRow>\n            </BlockActionSourcedGrid>\n        </div>\n        <Dialog v-model=\"isRowClickedDialogOpen\">\n            <template #header>\n                <h3>Row Clicked</h3>\n            </template>\n            <template #default>\n                <pre>{{ JSON.stringify( rowContextClicked, null, 2 ) }}</pre>\n            </template>\n        </Dialog>\n    </template>\n</PaneledBlockTemplate>"
            }));
        }
    };
});
//# sourceMappingURL=LargeDatasetGrid.js.map