System.register(["../Services/DateKey", "../Services/Email", "../Services/String", "vee-validate", "../Services/Number"], function (exports_1, context_1) {
    "use strict";
    var DateKey_1, Email_1, String_1, vee_validate_1, Number_1, convertToNumber, isNumeric;
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
            },
            function (Number_1_1) {
                Number_1 = Number_1_1;
            }
        ],
        execute: function () {
            vee_validate_1.defineRule('required', (function (value, _a) {
                var optionsJson = _a[0];
                var options = optionsJson ? JSON.parse(optionsJson) : {};
                if (typeof value === 'string') {
                    var allowEmptyString = !!(options.allowEmptyString);
                    if (!allowEmptyString && String_1.isNullOrWhitespace(value)) {
                        return 'is required';
                    }
                    return true;
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
                if (String_1.isNullOrWhitespace(value)) {
                    return true;
                }
                if (!Email_1.isEmail(value)) {
                    return 'must be a valid email';
                }
                return true;
            }));
            vee_validate_1.defineRule('notequal', (function (value, _a) {
                var compare = _a[0];
                if (isNumeric(value) && isNumeric(compare)) {
                    if (convertToNumber(value) !== convertToNumber(compare)) {
                        return true;
                    }
                }
                else if (value !== compare) {
                    return true;
                }
                return "must not equal " + compare;
            }));
            vee_validate_1.defineRule('equal', (function (value, _a) {
                var compare = _a[0];
                if (isNumeric(value) && isNumeric(compare)) {
                    if (convertToNumber(value) === convertToNumber(compare)) {
                        return true;
                    }
                }
                else if (value === compare) {
                    return true;
                }
                return "must equal " + compare;
            }));
            vee_validate_1.defineRule('gt', (function (value, _a) {
                var compare = _a[0];
                if (isNumeric(value) && isNumeric(compare)) {
                    if (convertToNumber(value) > convertToNumber(compare)) {
                        return true;
                    }
                }
                else if (value > compare) {
                    return true;
                }
                return "must be greater than " + compare;
            }));
            vee_validate_1.defineRule('gte', (function (value, _a) {
                var compare = _a[0];
                if (isNumeric(value) && isNumeric(compare)) {
                    if (convertToNumber(value) >= convertToNumber(compare)) {
                        return true;
                    }
                }
                else if (value >= compare) {
                    return true;
                }
                return "must not be less than " + compare;
            }));
            vee_validate_1.defineRule('lt', (function (value, _a) {
                var compare = _a[0];
                if (isNumeric(value) && isNumeric(compare)) {
                    if (convertToNumber(value) < convertToNumber(compare)) {
                        return true;
                    }
                }
                else if (value < compare) {
                    return true;
                }
                return "must be less than " + compare;
            }));
            vee_validate_1.defineRule('lte', (function (value, _a) {
                var compare = _a[0];
                if (isNumeric(value) && isNumeric(compare)) {
                    if (convertToNumber(value) <= convertToNumber(compare)) {
                        return true;
                    }
                }
                else if (value <= compare) {
                    return true;
                }
                return "must not be more than " + compare;
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
            convertToNumber = function (value) {
                if (typeof value === 'number') {
                    return value;
                }
                if (typeof value === 'string') {
                    return Number_1.toNumberOrNull(value) || 0;
                }
                return 0;
            };
            isNumeric = function (value) {
                if (typeof value === 'number') {
                    return true;
                }
                if (typeof value === 'string') {
                    return Number_1.toNumberOrNull(value) !== null;
                }
                return false;
            };
        }
    };
});
//# sourceMappingURL=Index.js.map