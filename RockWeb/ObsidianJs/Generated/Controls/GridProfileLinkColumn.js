System.register(["../Vendor/Vue/vue.js", "./GridColumn.js"], function (exports_1, context_1) {
    "use strict";
    var vue_js_1, GridColumn_js_1;
    var __moduleName = context_1 && context_1.id;
    function OfType() {
        return vue_js_1.defineComponent({
            name: 'GridProfileLinkColumn',
            components: {
                GridColumn: GridColumn_js_1.default()
            },
            setup: function () {
                return {
                    rowContext: vue_js_1.inject('rowContext')
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
            template: "<GridColumn :rowContext=\"rowContext\" class=\"grid-columncommand\" align=\"center\">\n    <a v-if=\"url\" @click.stop class=\"btn btn-default btn-sm\" :href=\"url\">\n        <i class=\"fa fa-user\"></i>\n    </a>\n</GridColumn>"
        });
    }
    exports_1("default", OfType);
    return {
        setters: [
            function (vue_js_1_1) {
                vue_js_1 = vue_js_1_1;
            },
            function (GridColumn_js_1_1) {
                GridColumn_js_1 = GridColumn_js_1_1;
            }
        ],
        execute: function () {
        }
    };
});
//# sourceMappingURL=GridProfileLinkColumn.js.map