System.register(["vue", "./Grid"], function (exports_1, context_1) {
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
    var __generator = (this && this.__generator) || function (thisArg, body) {
        var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
        return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
        function verb(n) { return function (v) { return step([n, v]); }; }
        function step(op) {
            if (f) throw new TypeError("Generator is already executing.");
            while (_) try {
                if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
                if (y = 0, t) op = [op[0] & 2, t.value];
                switch (op[0]) {
                    case 0: case 1: t = op; break;
                    case 4: _.label++; return { value: op[1], done: false };
                    case 5: _.label++; y = op[1]; op = [0]; continue;
                    case 7: op = _.ops.pop(); _.trys.pop(); continue;
                    default:
                        if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                        if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                        if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                        if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                        if (t[2]) _.ops.pop();
                        _.trys.pop(); continue;
                }
                op = body.call(thisArg, _);
            } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
            if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
        }
    };
    var vue_1, Grid_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (Grid_1_1) {
                Grid_1 = Grid_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'BlockActionSourcedGrid',
                components: {
                    Grid: Grid_1.default
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
                setup: function () {
                    return {
                        invokeBlockAction: vue_1.inject('invokeBlockAction')
                    };
                },
                data: function () {
                    return {
                        pageSize: 50,
                        totalRowCount: 0,
                        currentPageIndex: 1,
                        isLoading: false,
                        errorMessage: '',
                        sortProperty: {
                            Direction: Grid_1.SortDirection.Ascending,
                            Property: this.rowIdKey
                        },
                        currentPageData: []
                    };
                },
                computed: {
                    sortString: function () {
                        return this.sortProperty.Property + " " + this.sortProperty.Direction;
                    }
                },
                methods: {
                    fetchData: function () {
                        return __awaiter(this, void 0, void 0, function () {
                            var result, e_1;
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0:
                                        if (this.isLoading) {
                                            return [2];
                                        }
                                        this.isLoading = true;
                                        this.errorMessage = '';
                                        _a.label = 1;
                                    case 1:
                                        _a.trys.push([1, 3, 4, 5]);
                                        return [4, this.invokeBlockAction(this.blockActionName, {
                                                filterOptions: {
                                                    Take: this.pageSize,
                                                    Skip: (this.currentPageIndex - 1) * this.pageSize
                                                },
                                                sortProperty: this.sortProperty
                                            })];
                                    case 2:
                                        result = _a.sent();
                                        if (result.data && result.data.CurrentPageData) {
                                            this.currentPageData = result.data.CurrentPageData;
                                            this.totalRowCount = result.data.TotalCount;
                                        }
                                        else {
                                            this.currentPageData = [];
                                        }
                                        return [3, 5];
                                    case 3:
                                        e_1 = _a.sent();
                                        this.errorMessage = "An exception occurred: " + e_1;
                                        return [3, 5];
                                    case 4:
                                        this.isLoading = false;
                                        return [7];
                                    case 5: return [2];
                                }
                            });
                        });
                    }
                },
                watch: {
                    pageSize: function () {
                        return __awaiter(this, void 0, void 0, function () {
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0:
                                        if (!(this.currentPageIndex > 1)) return [3, 1];
                                        this.currentPageIndex = 1;
                                        return [3, 3];
                                    case 1: return [4, this.fetchData()];
                                    case 2:
                                        _a.sent();
                                        _a.label = 3;
                                    case 3: return [2];
                                }
                            });
                        });
                    },
                    currentPageIndex: function () {
                        return __awaiter(this, void 0, void 0, function () {
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0: return [4, this.fetchData()];
                                    case 1:
                                        _a.sent();
                                        return [2];
                                }
                            });
                        });
                    },
                    'sortString': function () {
                        return __awaiter(this, void 0, void 0, function () {
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0: return [4, this.fetchData()];
                                    case 1:
                                        _a.sent();
                                        return [2];
                                }
                            });
                        });
                    }
                },
                mounted: function () {
                    return __awaiter(this, void 0, void 0, function () {
                        return __generator(this, function (_a) {
                            switch (_a.label) {
                                case 0: return [4, this.fetchData()];
                                case 1:
                                    _a.sent();
                                    return [2];
                            }
                        });
                    });
                },
                template: "\n<Grid\n    :gridData=\"currentPageData\"\n    #default=\"rowContext\"\n    v-model:sortProperty=\"sortProperty\"\n    v-model:pageSize=\"pageSize\"\n    v-model:currentPageIndex=\"currentPageIndex\"\n    rowItemText=\"Group Member\"\n    :rowCountOverride=\"totalRowCount\"\n    :rowIdKey=\"rowIdKey\">\n    <slot v-bind=\"rowContext\" />\n</Grid>"
            }));
        }
    };
});
//# sourceMappingURL=BlockActionSourcedGrid.js.map