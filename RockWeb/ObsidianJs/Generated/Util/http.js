System.register(["../Vendor/Axios/index.js"], function (exports_1, context_1) {
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
    var index_js_1;
    var __moduleName = context_1 && context_1.id;
    /**
     * Make an API call. This is only place Axios (or AJAX library) should be referenced to allow tools like performance metrics to provide
    * better insights.
     * @param method
     * @param url
     * @param params
     * @param data
     */
    function doApiCallRaw(method, url, params, data) {
        return __awaiter(this, void 0, void 0, function () {
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0: return [4 /*yield*/, index_js_1.default({
                            method: method,
                            url: url,
                            params: params,
                            data: data
                        })];
                    case 1: return [2 /*return*/, _a.sent()];
                }
            });
        });
    }
    /**
    * Make an API call
    * @param {string} method The HTTP method, such as GET
    * @param {string} url The endpoint to access, such as /api/campuses/
    * @param {object} params Query parameter object.  Will be converted to ?key1=value1&key2=value2 as part of the URL.
    * @param {any} data This will be the body of the request
    */
    function doApiCall(method, url, params, data) {
        if (params === void 0) { params = undefined; }
        if (data === void 0) { data = undefined; }
        return __awaiter(this, void 0, void 0, function () {
            var result, e_1;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        _a.trys.push([0, 2, , 3]);
                        return [4 /*yield*/, doApiCallRaw(method, url, params, data)];
                    case 1:
                        result = _a.sent();
                        return [2 /*return*/, {
                                data: result.data,
                                isError: false,
                                statusCode: result.status
                            }];
                    case 2:
                        e_1 = _a.sent();
                        if (e_1 && e_1.response && e_1.response.data && e_1.response.data.Message) {
                            return [2 /*return*/, {
                                    data: null,
                                    isError: true,
                                    statusCode: e_1.response.status,
                                    errorMessage: e_1.response.data.Message
                                }];
                        }
                        return [2 /*return*/, {
                                data: null,
                                isError: true,
                                statusCode: e_1.response.status,
                                errorMessage: null
                            }];
                    case 3: return [2 /*return*/];
                }
            });
        });
    }
    exports_1("doApiCall", doApiCall);
    /**
    * Make a GET HTTP request
    * @param {string} url The endpoint to access, such as /api/campuses/
    * @param {object} params Query parameter object.  Will be converted to ?key1=value1&key2=value2 as part of the URL.
    */
    function get(url, params) {
        if (params === void 0) { params = undefined; }
        return __awaiter(this, void 0, void 0, function () {
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0: return [4 /*yield*/, doApiCall('GET', url, params, undefined)];
                    case 1: return [2 /*return*/, _a.sent()];
                }
            });
        });
    }
    exports_1("get", get);
    /**
    * Make a POST HTTP request
    * @param {string} url The endpoint to access, such as /api/campuses/
    * @param {object} params Query parameter object.  Will be converted to ?key1=value1&key2=value2 as part of the URL.
    * @param {any} data This will be the body of the request
    */
    function post(url, params, data) {
        if (params === void 0) { params = undefined; }
        if (data === void 0) { data = undefined; }
        return __awaiter(this, void 0, void 0, function () {
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0: return [4 /*yield*/, doApiCall('POST', url, params, data)];
                    case 1: return [2 /*return*/, _a.sent()];
                }
            });
        });
    }
    exports_1("post", post);
    return {
        setters: [
            function (index_js_1_1) {
                index_js_1 = index_js_1_1;
            }
        ],
        execute: function () {
            exports_1("default", {
                doApiCall: doApiCall,
                post: post,
                get: get
            });
        }
    };
});
//# sourceMappingURL=http.js.map