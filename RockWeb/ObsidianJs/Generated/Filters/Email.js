define(["require", "exports"], function (require, exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.isEmail = void 0;
    /**
     * Is the value a valid email address?
     * @param val
     */
    function isEmail(val) {
        if (typeof val === 'string') {
            var re = /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
            return re.test(val.toLowerCase());
        }
        return false;
    }
    exports.isEmail = isEmail;
});
//# sourceMappingURL=Email.js.map