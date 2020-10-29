Obsidian.Bus = (function () {
    const _bus = new Vue();

    const publish = (eventName, payload) => _bus.$emit(eventName, payload);
    const subscribe = (eventName, callback) => _bus.$on(eventName, callback);

    return {
        publish,
        subscribe
    };
})();
