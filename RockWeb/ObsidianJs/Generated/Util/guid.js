define(["require", "exports"], function (require, exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.areEqual = exports.normalize = exports.newGuid = void 0;
    /**
    * Generates a new Guid
    */
    function newGuid() {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = Math.random() * 16 | 0;
            var v = c === 'x' ? r : r & 0x3 | 0x8;
            return v.toString(16);
        });
    }
    exports.newGuid = newGuid;
    /**
     * Returns a normalized Guid that can be compared with string equality (===)
     * @param a
     */
    function normalize(a) {
        return a.toLowerCase();
    }
    exports.normalize = normalize;
    /**
     * Are the guids equal?
     * @param a
     * @param b
     */
    function areEqual(a, b) {
        return normalize(a) === normalize(b);
    }
    exports.areEqual = areEqual;
});
//# sourceMappingURL=Guid.js.map