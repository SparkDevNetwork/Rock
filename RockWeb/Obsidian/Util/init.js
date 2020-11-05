Obsidian.Util = (function () {

    /**
     * Stores the value using the given key. The cache will expire at the expiration or in
     * 1 minute if none is provided
     * @param {string} key
     * @param {any} value
     * @param {Date} expiration
     */
    const setCache = (key, value, expiration = null) => {
        if (!expiration) {
            // Default to one minute
            const now = new Date();
            const oneMinute = 60000; // One minute: 1 * 60 * 1000
            expiration = new Date(now.getTime() + oneMinute);
        }

        const cache = { expiration, value };
        const cacheJson = JSON.stringify(cache);
        sessionStorage.setItem(key, cacheJson);
    };

    /**
     * Gets a stored cache value if there is one that has not yet expired.
     * @param {string} key
     */
    const getCache = (key) => {
        const cacheJson = sessionStorage.getItem(key);

        if (!cacheJson) {
            return null;
        }

        const cache = JSON.parse(cacheJson);

        if (!cache || !cache.expiration) {
            return null;
        }

        const expiration = new Date(cache.expiration);

        if (!expiration || expiration < new Date()) {
            return null;
        }

        return cache.value;
    };

    /**
     * Generates a new Guid
     */
    const newGuid = () => 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
        const r = Math.random() * 16 | 0;
        const v = c === 'x' ? r : r & 0x3 | 0x8;
        return v.toString(16);
    });

    /**
     * Is the status code a 200 success code?
     * @param {number} statusCode
     */
    const isSuccessStatusCode = (statusCode) => statusCode && statusCode / 100 === 2;

    // Return "public" properties
    return {
        newGuid,
        isSuccessStatusCode,
        setCache,
        getCache
    };
})();
