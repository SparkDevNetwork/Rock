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
                name: "GridProfileLinkColumn",
                components: {
                    GridColumn: gridColumn_1.default
                },
                setup() {
                    return {
                        rowContext: vue_1.inject("rowContext")
                    };
                },
                props: {
                    property: {
                        type: String,
                        default: "PersonId"
                    },
                    urlTemplate: {
                        type: String,
                        default: "/person/{id}"
                    }
                },
                computed: {
                    personId() {
                        return this.rowContext.rowData[this.property] || null;
                    },
                    url() {
                        if (this.personId) {
                            return this.urlTemplate.replace("{id}", this.personId.toString());
                        }
                        return "";
                    }
                },
                template: `
<GridColumn :rowContext="rowContext" class="grid-columncommand" align="center">
    <a v-if="url" @click.stop class="btn btn-default btn-sm" :href="url">
        <i class="fa fa-user"></i>
    </a>
</GridColumn>`
            }));
        }
    };
});
//# sourceMappingURL=gridProfileLinkColumn.js.map