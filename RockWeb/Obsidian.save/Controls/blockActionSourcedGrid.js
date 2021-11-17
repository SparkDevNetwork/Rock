System.register(["vue", "./grid"], function (exports_1, context_1) {
    "use strict";
    var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
        function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
        return new (P || (P = Promise))(function (resolve, reject) {
            function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
            function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
            function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
            step((generator = generator.apply(thisArg, _arguments || [])).next());
        });
    };
    var vue_1, grid_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (grid_1_1) {
                grid_1 = grid_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: "BlockActionSourcedGrid",
                components: {
                    Grid: grid_1.default
                },
                props: {
                    blockActionName: {
                        type: String,
                        required: true
                    },
                    rowIdKey: {
                        type: String,
                        required: true
                    }
                },
                setup() {
                    return {
                        invokeBlockAction: vue_1.inject("invokeBlockAction")
                    };
                },
                data() {
                    return {
                        pageSize: 50,
                        totalRowCount: 0,
                        currentPageIndex: 1,
                        isLoading: false,
                        errorMessage: "",
                        sortProperty: {
                            direction: grid_1.SortDirection.Ascending,
                            property: this.rowIdKey
                        },
                        currentPageData: []
                    };
                },
                computed: {
                    sortString() {
                        return `${this.sortProperty.property} ${this.sortProperty.direction}`;
                    }
                },
                methods: {
                    fetchData() {
                        return __awaiter(this, void 0, void 0, function* () {
                            if (this.isLoading) {
                                return;
                            }
                            this.isLoading = true;
                            this.errorMessage = "";
                            try {
                                const result = yield this.invokeBlockAction(this.blockActionName, {
                                    filterOptions: {
                                        take: this.pageSize,
                                        skip: (this.currentPageIndex - 1) * this.pageSize
                                    },
                                    sortProperty: this.sortProperty
                                });
                                if (result.data && result.data.currentPageData) {
                                    this.currentPageData = result.data.currentPageData;
                                    this.totalRowCount = result.data.totalCount;
                                }
                                else {
                                    this.currentPageData = [];
                                }
                            }
                            catch (e) {
                                this.errorMessage = `An exception occurred: ${e}`;
                            }
                            finally {
                                this.isLoading = false;
                            }
                        });
                    }
                },
                watch: {
                    pageSize() {
                        return __awaiter(this, void 0, void 0, function* () {
                            if (this.currentPageIndex > 1) {
                                this.currentPageIndex = 1;
                            }
                            else {
                                yield this.fetchData();
                            }
                        });
                    },
                    currentPageIndex() {
                        return __awaiter(this, void 0, void 0, function* () {
                            yield this.fetchData();
                        });
                    },
                    "sortString"() {
                        return __awaiter(this, void 0, void 0, function* () {
                            yield this.fetchData();
                        });
                    }
                },
                mounted() {
                    return __awaiter(this, void 0, void 0, function* () {
                        yield this.fetchData();
                    });
                },
                template: `
<Grid
    :gridData="currentPageData"
    #default="rowContext"
    v-model:sortProperty="sortProperty"
    v-model:pageSize="pageSize"
    v-model:currentPageIndex="currentPageIndex"
    rowItemText="Group Member"
    :rowCountOverride="totalRowCount"
    :rowIdKey="rowIdKey">
    <slot v-bind="rowContext" />
</Grid>`
            }));
        }
    };
});
//# sourceMappingURL=blockActionSourcedGrid.js.map