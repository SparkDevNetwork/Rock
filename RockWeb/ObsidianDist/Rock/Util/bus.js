define(["require", "exports", "../Vendor/Mitt/index.js"], function (require, exports, index_js_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    var bus = index_js_1.default();
    var log = [];
    /**
    * Write a log entry that a payload was sent or received.
    */
    var writeLog = function (msg) {
        log.push({
            date: new Date(),
            message: msg
        });
    };
    /**
    * Send the payload to subscribers listening for the event name
    */
    function publish(eventName, payload) {
        writeLog("Published " + eventName);
        bus.emit(eventName, payload);
    }
    ;
    /**
    * Whenever an event is received of eventName, the callback is executed with the message
    * payload as a parameter.
    */
    function subscribe(eventName, callback) {
        writeLog("Subscribed to " + eventName);
        bus.on(eventName, function (T) {
            callback(T);
        });
    }
    ;
    exports.default = {
        publish: publish,
        subscribe: subscribe,
        log: log
    };
});
//# sourceMappingURL=bus.js.map