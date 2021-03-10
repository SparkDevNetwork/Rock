System.register(["../Services/DateKey", "../Services/Email", "../Services/String", "vee-validate"], function (exports_1, context_1) {
    "use strict";
    var DateKey_1, Email_1, String_1, vee_validate_1;
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
            function (DateKey_1_1) {
                DateKey_1 = DateKey_1_1;
            },
            function (Email_1_1) {
                Email_1 = Email_1_1;
            },
            function (String_1_1) {
                String_1 = String_1_1;
            },
            function (vee_validate_1_1) {
                vee_validate_1 = vee_validate_1_1;
            }
        ],
        execute: function () {
            vee_validate_1.defineRule('required', (function (value) {
                if (typeof value === 'string' && String_1.isNullOrWhitespace(value)) {
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
            vee_validate_1.defineRule('email', (function (value) {
                // Field is empty, should pass
                if (String_1.isNullOrWhitespace(value)) {
                    return true;
                }
                // Check if email
                if (!Email_1.isEmail(value)) {
                    return 'must be a valid email';
                }
                return true;
            }));
            vee_validate_1.defineRule('notequal', (function (value, _a) {
                var compare = _a[0];
                return value !== compare;
            }));
            vee_validate_1.defineRule('greaterthan', (function (value, _a) {
                var compare = _a[0];
                return value > compare;
            }));
            vee_validate_1.defineRule('datekey', (function (value) {
                var asString = value;
                if (!DateKey_1.default.getYear(asString)) {
                    return 'must have a year';
                }
                if (!DateKey_1.default.getMonth(asString)) {
                    return 'must have a month';
                }
                if (!DateKey_1.default.getDay(asString)) {
                    return 'must have a day';
                }
                return true;
            }));
        }
    };
});
//# sourceMappingURL=Index.js.map