Obsidian.Bus = (function () {
    const _bus = new Vue();

    const publish = (eventName, payload) => {
        writeLog(`Published ${eventName}`);
        _bus.$emit(eventName, payload);
    };

    const subscribe = (eventName, callback) => {
        writeLog(`Subscribed to ${eventName}`);
        _bus.$on(eventName, callback);
    };

    const writeLog = (msg) => {
        log.push({
            date: new Date(),
            message: msg
        });
    };

    const log = [];

    return {
        publish,
        subscribe,
        log
    };
})();
