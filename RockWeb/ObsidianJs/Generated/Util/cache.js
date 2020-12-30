define(["require", "exports"], function (require, exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    /**
    * Stores the value using the given key. The cache will expire at the expiration or in
    * 1 minute if none is provided
    * @param key
    * @param value
    * @param expiration
    */
    function set(key, value, expiration) {
        if (expiration === void 0) { expiration = null; }
        if (!expiration) {
            // Default to one minute
            var now = new Date();
            var oneMinute = 60000; // One minute: 1 * 60 * 1000
            expiration = new Date(now.getTime() + oneMinute);
        }
        var cache = { expiration: expiration, value: value };
        var cacheJson = JSON.stringify(cache);
        sessionStorage.setItem(key, cacheJson);
    }
    /**
     * Gets a stored cache value if there is one that has not yet expired.
     * @param key
     */
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
    exports.default = {
        set: set,
        get: get
    };
});
//# sourceMappingURL=cache.js.map