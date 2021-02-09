System.register(["../Vendor/Vue/vue.js"], function (exports_1, context_1) {
    "use strict";
    var vue_js_1, SortDirection;
    var __moduleName = context_1 && context_1.id;
    function OfType() {
        return vue_js_1.defineComponent({
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
                    default: null
                },
                rowItemText: {
                    type: String,
                    default: 'Entity'
                }
            },
            data: function () {
                return {
                    gridContext: {
                        selectedRowIds: {},
                        selectAllRows: false,
                        sortProperty: this.sortProperty
                    }
                };
            },
            watch: {
                gridData: function () {
                    this.gridContext.selectedRowIds = {};
                    for (var _i = 0, _a = this.gridData; _i < _a.length; _i++) {
                        var rowData = _a[_i];
                        var rowId = rowData[this.rowIdKey];
                        this.gridContext.selectedRowIds[rowId] = false;
                    }
                },
                'gridContext.sortProperty': {
                    deep: true,
                    handler: function () {
                        this.$emit('update:sortProperty', this.gridContext.sortProperty);
                    }
                },
            },
            methods: {
                getRowContext: function (rowData, isHeader) {
                    return {
                        rowData: rowData,
                        isHeader: isHeader,
                        rowId: rowData[this.rowIdKey]
                    };
                }
            },
            provide: function () {
                return {
                    gridContext: this.gridContext
                };
            },
            template: "<div class=\"table-responsive\">\n    <table class=\"grid-table table table-bordered table-striped table-hover\">\n        <thead>\n            <slot :rowData=\"null\" :isHeader=\"true\" :rowId=\"null\" />\n        </thead>\n        <tbody>\n            <template v-if=\"!gridData.length\">\n                <tr data-original-title=\"\" title=\"\">\n                    <td colspan=\"28\">\n                        <span class=\"table-empty\">\n                            No {{rowItemText}}s Found\n                        </span>\n                    </td>\n                </tr>\n            </template>\n            <template v-else v-for=\"rowData in gridData\" :key=\"rowData[rowIdKey]\" >\n                <slot v-bind=\"getRowContext(rowData, false, )\" />\n            </template>\n        </tbody>\n    </table>\n</div>"
        });
    }
    exports_1("default", OfType);
    return {
        setters: [
            function (vue_js_1_1) {
                vue_js_1 = vue_js_1_1;
            }
        ],
        execute: function () {
            (function (SortDirection) {
                SortDirection[SortDirection["Ascending"] = 0] = "Ascending";
                SortDirection[SortDirection["Descending"] = 1] = "Descending";
            })(SortDirection || (SortDirection = {}));
            exports_1("SortDirection", SortDirection);
        }
    };
});
//# sourceMappingURL=Grid.js.map