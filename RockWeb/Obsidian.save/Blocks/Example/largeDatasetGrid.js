System.register(["../../Templates/paneledBlockTemplate", "vue", "../../Controls/gridRow", "../../Controls/gridColumn", "../../Controls/gridProfileLinkColumn", "../../Controls/blockActionSourcedGrid", "../../Controls/dialog"], function (exports_1, context_1) {
    "use strict";
    var paneledBlockTemplate_1, vue_1, gridRow_1, gridColumn_1, gridProfileLinkColumn_1, blockActionSourcedGrid_1, dialog_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (paneledBlockTemplate_1_1) {
                paneledBlockTemplate_1 = paneledBlockTemplate_1_1;
            },
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (gridRow_1_1) {
                gridRow_1 = gridRow_1_1;
            },
            function (gridColumn_1_1) {
                gridColumn_1 = gridColumn_1_1;
            },
            function (gridProfileLinkColumn_1_1) {
                gridProfileLinkColumn_1 = gridProfileLinkColumn_1_1;
            },
            function (blockActionSourcedGrid_1_1) {
                blockActionSourcedGrid_1 = blockActionSourcedGrid_1_1;
            },
            function (dialog_1_1) {
                dialog_1 = dialog_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: "Example.LargeDatasetGrid",
                components: {
                    PaneledBlockTemplate: paneledBlockTemplate_1.default,
                    BlockActionSourcedGrid: blockActionSourcedGrid_1.default,
                    GridColumn: gridColumn_1.default,
                    GridRow: gridRow_1.default,
                    GridProfileLinkColumn: gridProfileLinkColumn_1.default,
                    Dialog: dialog_1.default
                },
                data() {
                    return {
                        rowContextClicked: null,
                        isRowClickedDialogOpen: false
                    };
                },
                methods: {
                    onRowClick(rowContext) {
                        this.rowContextClicked = rowContext;
                        this.isRowClickedDialogOpen = true;
                    }
                },
                template: `
<PaneledBlockTemplate>
    <template #title>
        <i class="fa fa-dumbbell"></i>
        Large Dataset Grid
    </template>
    <template #default>
        <div class="grid grid-panel">
            <BlockActionSourcedGrid blockActionName="GetAttributeValues" #default="rowContext" rowItemText="Attribute Values" rowIdKey="Id">
                <GridRow :rowContext="rowContext" @click:body="onRowClick">
                    <GridColumn title="Id" property="Id" sortExpression="Id" />
                    <GridColumn title="Guid" property="Guid" sortExpression="Guid" />
                    <GridColumn title="Attribute" property="Attribute" sortExpression="Attribute.Id" />
                    <GridColumn title="Value" property="Value" sortExpression="Value" />
                </GridRow>
            </BlockActionSourcedGrid>
        </div>
        <Dialog v-model="isRowClickedDialogOpen">
            <template #header>
                <h3>Row Clicked</h3>
            </template>
            <template #default>
                <pre>{{ JSON.stringify( rowContextClicked, null, 2 ) }}</pre>
            </template>
        </Dialog>
    </template>
</PaneledBlockTemplate>`
            }));
        }
    };
});
//# sourceMappingURL=largeDatasetGrid.js.map