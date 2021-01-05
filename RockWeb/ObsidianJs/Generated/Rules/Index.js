define(["require", "exports", "../Filters/Email.js", "../Filters/String.js", "../Vendor/VeeValidate/vee-validate.js"], function (require, exports, Email_js_1, String_js_1, vee_validate_js_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.ruleArrayToString = exports.ruleStringToArray = void 0;
    function ruleStringToArray(rulesString) {
        return rulesString.split('|');
    }
    exports.ruleStringToArray = ruleStringToArray;
    function ruleArrayToString(rulesArray) {
        return rulesArray.join('|');
    }
    exports.ruleArrayToString = ruleArrayToString;
    vee_validate_js_1.defineRule('required', (function (value) {
        if (String_js_1.isNullOrWhitespace(value)) {
            return 'This field is required';
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
            return 'This field must be a valid email';
        }
        return true;
    }));
});
//# sourceMappingURL=Index.js.map