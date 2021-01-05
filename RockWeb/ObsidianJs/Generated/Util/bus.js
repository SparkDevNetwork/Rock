System.register(["../Vendor/Mitt/index.js"], function (exports_1, context_1) {
    "use strict";
    var index_js_1, bus, log, writeLog;
    var __moduleName = context_1 && context_1.id;
    /**
    * Send the payload to subscribers listening for the event name
    */
    function publish(eventName, payload) {
        writeLog("Published " + eventName);
        bus.emit(eventName, payload);
    }
    /**
    * Whenever an event is received of eventName, the callback is executed with the message
    * payload as a parameter.
    */
    function subscribe(eventName, callback) {
        writeLog("Subscribed to " + eventName);
        bus.on(eventName, function (payload) {
            if (payload) {
                callback(payload);
            }
        });
    }
    return {
        setters: [
            function (index_js_1_1) {
                index_js_1 = index_js_1_1;
            }
        ],
        execute: function () {
            bus = index_js_1.default();
            log = [];
            /**
            * Write a log entry that a payload was sent or received.
            */
            writeLog = function (msg) {
                log.push({
                    date: new Date(),
                    message: msg
                });
            };
            exports_1("default", {
                publish: publish,
                subscribe: subscribe,
                log: log
            });
        }
    };
});
//# sourceMappingURL=Bus.js.map