System.register(["../Filters/Email.js", "../Filters/String.js", "../Vendor/VeeValidate/vee-validate.js"], function (exports_1, context_1) {
    "use strict";
    var Email_js_1, String_js_1, vee_validate_js_1;
    var __moduleName = context_1 && context_1.id;
    function ruleStringToArray(rulesString) {
        return rulesString.split('|');
    }
    exports_1("ruleStringToArray", ruleStringToArray);
    function ruleArrayToString(rulesArray) {
        return rulesArray.join('|');
    }
    exports_1("ruleArrayToString", ruleArrayToString);
    return {
        setters: [
            function (Email_js_1_1) {
                Email_js_1 = Email_js_1_1;
            },
            function (String_js_1_1) {
                String_js_1 = String_js_1_1;
            },
            function (vee_validate_js_1_1) {
                vee_validate_js_1 = vee_validate_js_1_1;
            }
        ],
        execute: function () {
            vee_validate_js_1.defineRule('required', (function (value) {
                if (String_js_1.isNullOrWhitespace(value)) {
                    return 'is required';
                }
                return true;
            }));
            vee_validate_js_1.defineRule('email', (function (value) {
                // Field is empty, should pass
                if (String_js_1.isNullOrWhitespace(value)) {
                    return true;
                }
                // Check if email
                if (!Email_js_1.isEmail(value)) {
                    return 'must be a valid email';
                }
                return true;
            }));
        }
    };
});
//# sourceMappingURL=Index.js.map