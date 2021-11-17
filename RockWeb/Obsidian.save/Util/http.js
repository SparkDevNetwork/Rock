System.register(["axios"], function (exports_1, context_1) {
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
    var axios_1;
    var __moduleName = context_1 && context_1.id;
    function doApiCallRaw(method, url, params, data) {
        return __awaiter(this, void 0, void 0, function* () {
            return yield axios_1.default({
                method,
                url,
                params,
                data
            });
        });
    }
    function doApiCall(method, url, params = undefined, data = undefined) {
        var _a, _b, _c, _d, _e, _f, _g, _h;
        return __awaiter(this, void 0, void 0, function* () {
            try {
                const result = yield doApiCallRaw(method, url, params, data);
                return {
                    data: result.data,
                    isError: false,
                    isSuccess: true,
                    statusCode: result.status,
                    errorMessage: null
                };
            }
            catch (e) {
                if ((_c = (_b = (_a = e === null || e === void 0 ? void 0 : e.response) === null || _a === void 0 ? void 0 : _a.data) === null || _b === void 0 ? void 0 : _b.Message) !== null && _c !== void 0 ? _c : (_e = (_d = e === null || e === void 0 ? void 0 : e.response) === null || _d === void 0 ? void 0 : _d.data) === null || _e === void 0 ? void 0 : _e.message) {
                    return {
                        data: null,
                        isError: true,
                        isSuccess: false,
                        statusCode: e.response.status,
                        errorMessage: (_h = (_g = (_f = e === null || e === void 0 ? void 0 : e.response) === null || _f === void 0 ? void 0 : _f.data) === null || _g === void 0 ? void 0 : _g.Message) !== null && _h !== void 0 ? _h : e.response.data.message
                    };
                }
                return {
                    data: null,
                    isError: true,
                    isSuccess: false,
                    statusCode: e.response.status,
                    errorMessage: null
                };
            }
        });
    }
    exports_1("doApiCall", doApiCall);
    function get(url, params = undefined) {
        return __awaiter(this, void 0, void 0, function* () {
            return yield doApiCall("GET", url, params, undefined);
        });
    }
    exports_1("get", get);
    function post(url, params = undefined, data = undefined) {
        return __awaiter(this, void 0, void 0, function* () {
            return yield doApiCall("POST", url, params, data);
        });
    }
    exports_1("post", post);
    return {
        setters: [
            function (axios_1_1) {
                axios_1 = axios_1_1;
            }
        ],
        execute: function () {
            exports_1("default", {
                doApiCall,
                post,
                get
            });
        }
    };
});
//# sourceMappingURL=http.js.map