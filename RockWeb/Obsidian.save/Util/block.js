System.register(["vue"], function (exports_1, context_1) {
    "use strict";
    var vue_1;
    var __moduleName = context_1 && context_1.id;
    function useConfigurationValues() {
        const result = vue_1.inject("configurationValues");
        if (result === undefined) {
            throw "Attempted to access block configuration outside of a RockBlock.";
        }
        return result;
    }
    exports_1("useConfigurationValues", useConfigurationValues);
    function useInvokeBlockAction() {
        const result = vue_1.inject("invokeBlockAction");
        if (result === undefined) {
            throw "Attempted to access block action invocation outside of a RockBlock.";
        }
        return result;
    }
    exports_1("useInvokeBlockAction", useInvokeBlockAction);
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            }
        ],
        execute: function () {
        }
    };
});
//# sourceMappingURL=block.js.map