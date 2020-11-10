/**
 * The bus allows page components to send and receive arbitrary data from other page components.
 */
Obsidian.Bus = (function () {
    const _bus = mitt();

    /**
     * Send the payload to subscribers listening for the event name
     * @param {string} eventName
     * @param {any} payload
     */
    const publish = (eventName, payload) => {
        writeLog(`Published ${eventName}`);
        _bus.emit(eventName, payload);
    };

    /**
     * Whenever an event is received of eventName, the callback is executed with the message
     * payload as a parameter.
     * @param {string} eventName
     * @param {(payload: any) => {}} callback
     */
    const subscribe = (eventName, callback) => {
        writeLog(`Subscribed to ${eventName}`);
        _bus.on(eventName, callback);
    };

    /**
     * Write a log entry that a payload was sent or received.
     * @param {string} msg
     */
    const writeLog = (msg) => {
        log.push({
            date: new Date(),
            message: msg
        });
    };
    const log = [];

    // Return "public" members
    return {
        publish,
        subscribe,
        log
    };
})();
