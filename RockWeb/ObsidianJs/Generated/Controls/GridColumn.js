System.register(["../Elements/JavaScriptAnchor", "vue", "./Grid"], function (exports_1, context_1) {
    "use strict";
    var JavaScriptAnchor_1, vue_1, Grid_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (JavaScriptAnchor_1_1) {
                JavaScriptAnchor_1 = JavaScriptAnchor_1_1;
            },
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (Grid_1_1) {
                Grid_1 = Grid_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'GridColumn',
                components: {
                    JavaScriptAnchor: JavaScriptAnchor_1.default
                },
                props: {
                    title: {
                        type: String,
                        default: ''
                    },
                    property: {
                        type: String,
                        default: ''
                    },
                    sortExpression: {
                        type: String,
                        default: ''
                    }
                },
                setup: function () {
                    return {
                        gridContext: vue_1.inject('gridContext'),
                        rowContext: vue_1.inject('rowContext')
                    };
                },
                computed: {
                    mySortExpression: function () {
                        return this.sortExpression || this.property;
                    },
                    canSort: function () {
                        return !!this.sortProperty;
                    },
                    sortProperty: function () {
                        return this.gridContext.sortProperty;
                    },
                    isCurrentlySorted: function () {
                        var _a;
                        return !!this.mySortExpression && ((_a = this.sortProperty) === null || _a === void 0 ? void 0 : _a.Property) === this.mySortExpression;
                    },
                    isCurrentlySortedDesc: function () {
                        var _a;
                        return this.isCurrentlySorted && ((_a = this.sortProperty) === null || _a === void 0 ? void 0 : _a.Direction) === Grid_1.SortDirection.Descending;
                    },
                    isCurrentlySortedAsc: function () {
                        var _a;
                        return this.isCurrentlySorted && ((_a = this.sortProperty) === null || _a === void 0 ? void 0 : _a.Direction) === Grid_1.SortDirection.Ascending;
                    }
                },
                methods: {
                    onHeaderClick: function () {
                        this.$emit('click:header', this.property);
                        if (this.mySortExpression && this.sortProperty) {
                            if (this.isCurrentlySortedAsc) {
                                this.sortProperty.Direction = Grid_1.SortDirection.Descending;
                            }
                            else {
                                this.sortProperty.Property = this.mySortExpression;
                                this.sortProperty.Direction = Grid_1.SortDirection.Ascending;
                            }
                        }
                    },
                },
                template: "\n<th\n    v-if=\"rowContext.isHeader\"\n    scope=\"col\"\n    @click=\"onHeaderClick\"\n    :class=\"isCurrentlySortedAsc ? 'ascending' : isCurrentlySortedDesc ? 'descending' : ''\">\n    <JavaScriptAnchor v-if=\"mySortExpression && canSort\">\n        <slot name=\"header\">\n            {{title}}\n        </slot>\n    </JavaScriptAnchor>\n    <template v-else>\n        <slot name=\"header\">\n            {{title}}\n        </slot>\n    </template>\n</th>\n<td v-else class=\"grid-select-cell\">\n    <slot>\n        {{rowContext.rowData[property]}}\n    </slot>\n</td>"
            }));
        }
    };
});
//# sourceMappingURL=GridColumn.js.map