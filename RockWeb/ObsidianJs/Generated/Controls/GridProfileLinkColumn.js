define(["require", "exports", "../Vendor/Vue/vue.js", "./GridColumn.js"], function (require, exports, vue_js_1, GridColumn_js_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
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
    exports.default = OfType;
});
//# sourceMappingURL=GridProfileLinkColumn.js.map