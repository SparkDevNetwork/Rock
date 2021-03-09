System.register(["../Services/DateKey.js", "../Services/Email.js", "../Services/String.js", "../Vendor/VeeValidate/vee-validate.js"], function (exports_1, context_1) {
    "use strict";
    var DateKey_js_1, Email_js_1, String_js_1, vee_validate_js_1;
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
            function (DateKey_js_1_1) {
                DateKey_js_1 = DateKey_js_1_1;
            },
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
                if (typeof value === 'string' && String_js_1.isNullOrWhitespace(value)) {
                    return 'is required';
                }
                if (typeof value === 'number' && value === 0) {
                    return 'is required';
                }
                if (!value) {
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
            vee_validate_js_1.defineRule('notequal', (function (value, _a) {
                var compare = _a[0];
                return value !== compare;
            }));
            vee_validate_js_1.defineRule('greaterthan', (function (value, _a) {
                var compare = _a[0];
                return value > compare;
            }));
            vee_validate_js_1.defineRule('datekey', (function (value) {
                var asString = value;
                if (!DateKey_js_1.default.getYear(asString)) {
                    return 'must have a year';
                }
                if (!DateKey_js_1.default.getMonth(asString)) {
                    return 'must have a month';
                }
                if (!DateKey_js_1.default.getDay(asString)) {
                    return 'must have a day';
                }
                return true;
            }));
        }
    };
});
//# sourceMappingURL=Index.js.map