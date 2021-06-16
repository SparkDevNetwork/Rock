System.register(["vue", "./GridColumn"], function (exports_1, context_1) {
    "use strict";
    var vue_1, GridColumn_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (GridColumn_1_1) {
                GridColumn_1 = GridColumn_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'GridProfileLinkColumn',
                components: {
                    GridColumn: GridColumn_1.default
                },
                setup: function () {
                    return {
                        rowContext: vue_1.inject('rowContext')
                    };
                },
                props: {
                    property: {
                        type: String,
                        default: 'PersonId'
                    },
                    urlTemplate: {
                        type: String,
                        default: '/person/{id}'
                    }
                },
                computed: {
                    personId: function () {
                        return this.rowContext.rowData[this.property] || null;
                    },
                    url: function () {
                        if (this.personId) {
                            return this.urlTemplate.replace('{id}', this.personId.toString());
                        }
                        return '';
                    }
                },
                template: "\n<GridColumn :rowContext=\"rowContext\" class=\"grid-columncommand\" align=\"center\">\n    <a v-if=\"url\" @click.stop class=\"btn btn-default btn-sm\" :href=\"url\">\n        <i class=\"fa fa-user\"></i>\n    </a>\n</GridColumn>"
            }));
        }
    };
});
//# sourceMappingURL=GridProfileLinkColumn.js.map