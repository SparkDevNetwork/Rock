System.register([], function (exports_1, context_1) {
    "use strict";
    var __moduleName = context_1 && context_1.id;
    function set(key, value, expiration) {
        if (expiration === void 0) { expiration = null; }
        if (!expiration) {
            var now = new Date();
            var oneMinute = 60000;
            expiration = new Date(now.getTime() + oneMinute);
        }
        var cache = { expiration: expiration, value: value };
        var cacheJson = JSON.stringify(cache);
        sessionStorage.setItem(key, cacheJson);
    }
    function get(key) {
        var cacheJson = sessionStorage.getItem(key);
        if (!cacheJson) {
            return null;
        }
        var cache = JSON.parse(cacheJson);
        if (!cache || !cache.expiration) {
            return null;
        }
        var expiration = new Date(cache.expiration);
        if (!expiration || expiration < new Date()) {
            return null;
        }
        return cache.value;
    }
    return {
        setters: [],
        execute: function () {
            exports_1("default", {
                set: set,
                get: get
            });
        }
    };
});
//# sourceMappingURL=Cache.js.map