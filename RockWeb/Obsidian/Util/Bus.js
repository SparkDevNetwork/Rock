System.register(["mitt"], function (exports_1, context_1) {
    "use strict";
    var mitt_1, bus, log, writeLog;
    var __moduleName = context_1 && context_1.id;
    function publish(eventName, payload) {
        writeLog("Published " + eventName);
        bus.emit(eventName, payload);
    }
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
            function (mitt_1_1) {
                mitt_1 = mitt_1_1;
            }
        ],
        execute: function () {
            bus = mitt_1.default();
            log = [];
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