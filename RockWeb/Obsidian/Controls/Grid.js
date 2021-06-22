System.register(["vue", "../Elements/JavaScriptAnchor"], function (exports_1, context_1) {
    "use strict";
    var vue_1, JavaScriptAnchor_1, SortDirection;
    var __moduleName = context_1 && context_1.id;
    function getRowId(rowData, rowIdKey) {
        return "" + rowData[rowIdKey];
    }
    exports_1("getRowId", getRowId);
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (JavaScriptAnchor_1_1) {
                JavaScriptAnchor_1 = JavaScriptAnchor_1_1;
            }
        ],
        execute: function () {
            (function (SortDirection) {
                SortDirection[SortDirection["Ascending"] = 0] = "Ascending";
                SortDirection[SortDirection["Descending"] = 1] = "Descending";
            })(SortDirection || (SortDirection = {}));
            exports_1("SortDirection", SortDirection);
            exports_1("default", vue_1.defineComponent({
                name: 'Grid',
                components: {
                    JavaScriptAnchor: JavaScriptAnchor_1.default
                },
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
                    pageSize: {
                        type: Number,
                        default: 50
                    },
                    currentPageIndex: {
                        type: Number,
                        default: 1
                    },
                    rowItemText: {
                        type: String,
                        default: 'Entity'
                    },
                    rowCountOverride: {
                        type: Number,
                        default: 0
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
                computed: {
                    rowCount: function () {
                        if (this.rowCountOverride) {
                            return this.rowCountOverride;
                        }
                        return this.gridData.length;
                    },
                    pageCount: function () {
                        return Math.ceil(this.rowCount / this.pageSize);
                    },
                    currentPageSet: function () {
                        var pagesPerSet = 10;
                        var firstNumber = Math.floor(this.currentPageIndex / pagesPerSet) * pagesPerSet + 1;
                        var set = [];
                        for (var i = 0; i < pagesPerSet; i++) {
                            var pageIndex = firstNumber + i;
                            if (pageIndex <= this.pageCount) {
                                set.push(pageIndex);
                            }
                        }
                        return set;
                    }
                },
                watch: {
                    gridData: function () {
                        this.gridContext.selectedRowIds = {};
                        for (var _i = 0, _a = this.gridData; _i < _a.length; _i++) {
                            var rowData = _a[_i];
                            var rowId = getRowId(rowData, this.rowIdKey);
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
                    getRowId: getRowId,
                    getRowContext: function (rowData, isHeader) {
                        var rowId = getRowId(rowData, this.rowIdKey);
                        return {
                            rowData: rowData,
                            isHeader: isHeader,
                            rowId: rowId
                        };
                    },
                    setPageSize: function (pageSize) {
                        this.$emit('update:pageSize', pageSize);
                    },
                    setPageIndex: function (pageIndex) {
                        this.$emit('update:currentPageIndex', pageIndex);
                    },
                    goToPreviousPageSet: function () {
                        var lowestPageInCurrentSet = this.currentPageSet[0] || 0;
                        if (lowestPageInCurrentSet <= 1) {
                            return;
                        }
                        this.setPageIndex(lowestPageInCurrentSet - 1);
                    },
                    goToNextPageSet: function () {
                        var lastIndex = this.currentPageSet.length - 1;
                        var highestPageInCurrentSet = this.currentPageSet[lastIndex] || 0;
                        if (highestPageInCurrentSet <= 1) {
                            return;
                        }
                        if (highestPageInCurrentSet >= this.pageCount) {
                            return;
                        }
                        this.setPageIndex(highestPageInCurrentSet + 1);
                    }
                },
                provide: function () {
                    return {
                        gridContext: this.gridContext
                    };
                },
                template: "\n<div class=\"table-responsive\">\n    <table class=\"grid-table table table-bordered table-striped table-hover\">\n        <thead>\n            <slot :rowData=\"null\" :isHeader=\"true\" :rowId=\"null\" />\n        </thead>\n        <tbody>\n            <template v-if=\"!gridData.length\">\n                <tr data-original-title=\"\" title=\"\">\n                    <td colspan=\"28\">\n                        <span class=\"table-empty\">\n                            No {{rowItemText}}s Found\n                        </span>\n                    </td>\n                </tr>\n            </template>\n            <template v-else v-for=\"rowData in gridData\" :key=\"getRowId(rowData, rowIdKey)\" >\n                <slot v-bind=\"getRowContext(rowData, false, )\" />\n            </template>\n        </tbody>\n        <tfoot>\n            <tr>\n                <td class=\"grid-paging\" colspan=\"6\">\n                    <ul class=\"grid-pagesize pagination pagination-sm\">\n                        <li :class=\"pageSize === 50 ? 'active' : ''\">\n                            <JavaScriptAnchor @click=\"setPageSize(50)\">50</JavaScriptAnchor>\n                        </li>\n                        <li :class=\"pageSize === 500 ? 'active' : ''\">\n                            <JavaScriptAnchor @click=\"setPageSize(500)\">500</JavaScriptAnchor>\n                        </li>\n                        <li :class=\"pageSize === 5000 ? 'active' : ''\">\n                            <JavaScriptAnchor @click=\"setPageSize(5000)\">5000</JavaScriptAnchor>\n                        </li>\n                    </ul>\n                    <div class=\"grid-itemcount\">{{rowCount}} {{rowItemText}}</div>\n                    <ul v-if=\"pageCount > 1\" class=\"grid-pager pagination pagination-sm\">\n                        <li class=\"prev disabled\">\n                            <JavaScriptAnchor @click=\"goToPreviousPageSet\" class=\"aspNetDisabled\">\u00AB</JavaScriptAnchor>\n                        </li>\n                        <li v-for=\"pageIndex in currentPageSet\" :key=\"pageIndex\" :class=\"pageIndex === currentPageIndex ? 'active' : ''\">\n                            <JavaScriptAnchor @click=\"setPageIndex(pageIndex)\">{{pageIndex}}</JavaScriptAnchor>\n                        </li>\n                        <li class=\"next disabled\">\n                            <JavaScriptAnchor @click=\"goToNextPageSet\" class=\"aspNetDisabled\">\u00BB</JavaScriptAnchor>\n                        </li>\n                    </ul>\n                </td>\n            </tr>\n            <tr>\n                <td class=\"grid-actions\" colspan=\"6\">\n                    <JavaScriptAnchor title=\"Communicate\" class=\"btn btn-grid-action btn-communicate btn-default btn-sm\"><i class=\"fa fa-comment fa-fw\"></i></JavaScriptAnchor>\n                    <JavaScriptAnchor title=\"Merge Person Records\" class=\"btn btn-grid-action btn-merge btn-default btn-sm\"><i class=\"fa fa-users fa-fw\"></i></JavaScriptAnchor>\n                    <JavaScriptAnchor title=\"Bulk Update\" class=\"btn btn-grid-action btn-bulk-update btn-default btn-sm\"><i class=\"fa fa-truck fa-fw\"></i></JavaScriptAnchor>\n                    <JavaScriptAnchor title=\"Launch Workflow\" class=\"btn-grid-action btn-launch-workflow btn btn-default btn-sm\"><i class=\"fa fa-cog fa-fw\"></i></JavaScriptAnchor>\n                    <JavaScriptAnchor title=\"Export to Excel\" class=\"btn btn-grid-action btn-excelexport btn-default btn-sm\"><i class=\"fa fa-table fa-fw\"></i></JavaScriptAnchor>\n                    <JavaScriptAnchor title=\"Merge Records into Merge Template\" class=\"btn btn-grid-action btn-merge-template btn-default btn-sm\"><i class=\"fa fa-files-o fa-fw\"></i></JavaScriptAnchor>\n                    <JavaScriptAnchor accesskey=\"n\" title=\"Alt+N\" class=\"btn btn-grid-action btn-add btn-default btn-sm\"><i class=\"fa fa-plus-circle fa-fw\"></i></JavaScriptAnchor>\n                </td>\n            </tr>\n        </tfoot>\n    </table>\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=Grid.js.map