System.register([], function (exports_1, context_1) {
    "use strict";
    var __moduleName = context_1 && context_1.id;
    function isEmail(val) {
        if (typeof val === 'string') {
            var re = /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
            return re.test(val.toLowerCase());
        }
        return false;
    }
    exports_1("isEmail", isEmail);
    return {
        setters: [],
        execute: function () {
        }
    };
});
//# sourceMappingURL=Email.js.map