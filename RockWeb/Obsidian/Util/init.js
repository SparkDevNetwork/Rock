Obsidian.Util = (function () {

    const newGuid = () => 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
        const r = Math.random() * 16 | 0;
        const v = c === 'x' ? r : r & 0x3 | 0x8;
        return v.toString(16);
    });

    const isSuccessStatusCode = (statusCode) => statusCode && statusCode / 100 === 2;

    return {
        newGuid,
        isSuccessStatusCode
    };
})();
