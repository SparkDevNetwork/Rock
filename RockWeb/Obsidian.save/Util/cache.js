System.register(["./rockDateTime"], function (exports_1, context_1) {
    "use strict";
    var rockDateTime_1;
    var __moduleName = context_1 && context_1.id;
    function set(key, value, expiration = null) {
        if (!expiration) {
            expiration = rockDateTime_1.RockDateTime.now().addMinutes(1);
        }
        const cache = { expiration, value };
        const cacheJson = JSON.stringify(cache);
        sessionStorage.setItem(key, cacheJson);
    }
    function get(key) {
        const cacheJson = sessionStorage.getItem(key);
        if (!cacheJson) {
            return null;
        }
        const cache = JSON.parse(cacheJson);
        if (!cache || !cache.expiration) {
            return null;
        }
        if (cache.expiration < rockDateTime_1.RockDateTime.now()) {
            return null;
        }
        return cache.value;
    }
    return {
        setters: [
            function (rockDateTime_1_1) {
                rockDateTime_1 = rockDateTime_1_1;
            }
        ],
        execute: function () {
            exports_1("default", {
                set,
                get
            });
        }
    };
});
//# sourceMappingURL=cache.js.map