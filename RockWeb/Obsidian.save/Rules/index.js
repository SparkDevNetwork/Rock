System.register(["../Services/dateKey", "../Services/email", "../Services/string", "vee-validate", "../Services/number"], function (exports_1, context_1) {
    "use strict";
    var dateKey_1, email_1, string_1, vee_validate_1, number_1;
    var __moduleName = context_1 && context_1.id;
    function convertToNumber(value) {
        if (typeof value === "number") {
            return value;
        }
        if (typeof value === "string") {
            return number_1.toNumberOrNull(value) || 0;
        }
        return 0;
    }
    function isNumeric(value) {
        if (typeof value === "number") {
            return true;
        }
        if (typeof value === "string") {
            return number_1.toNumberOrNull(value) !== null;
        }
        return false;
    }
    function ruleStringToArray(rulesString) {
        return rulesString.split("|");
    }
    exports_1("ruleStringToArray", ruleStringToArray);
    function ruleArrayToString(rulesArray) {
        return rulesArray.join("|");
    }
    exports_1("ruleArrayToString", ruleArrayToString);
    return {
        setters: [
            function (dateKey_1_1) {
                dateKey_1 = dateKey_1_1;
            },
            function (email_1_1) {
                email_1 = email_1_1;
            },
            function (string_1_1) {
                string_1 = string_1_1;
            },
            function (vee_validate_1_1) {
                vee_validate_1 = vee_validate_1_1;
            },
            function (number_1_1) {
                number_1 = number_1_1;
            }
        ],
        execute: function () {
            vee_validate_1.defineRule("required", ((value, [optionsJson]) => {
                const options = typeof optionsJson === "string" ? JSON.parse(optionsJson) : {};
                if (typeof value === "string") {
                    const allowEmptyString = !!(options.allowEmptyString);
                    if (!allowEmptyString && string_1.isNullOrWhiteSpace(value)) {
                        return "is required";
                    }
                    return true;
                }
                if (typeof value === "number" && value === 0) {
                    return "is required";
                }
                if (typeof value === "boolean") {
                    return true;
                }
                if (!value) {
                    return "is required";
                }
                return true;
            }));
            vee_validate_1.defineRule("email", (value => {
                if (string_1.isNullOrWhiteSpace(value)) {
                    return true;
                }
                if (!email_1.isEmail(value)) {
                    return "must be a valid email";
                }
                return true;
            }));
            vee_validate_1.defineRule("notequal", ((value, [compare]) => {
                if (isNumeric(value) && isNumeric(compare)) {
                    if (convertToNumber(value) !== convertToNumber(compare)) {
                        return true;
                    }
                }
                else if (value !== compare) {
                    return true;
                }
                return `must not equal ${compare}`;
            }));
            vee_validate_1.defineRule("equal", ((value, [compare]) => {
                if (isNumeric(value) && isNumeric(compare)) {
                    if (convertToNumber(value) === convertToNumber(compare)) {
                        return true;
                    }
                }
                else if (value === compare) {
                    return true;
                }
                return `must equal ${compare}`;
            }));
            vee_validate_1.defineRule("gt", ((value, [compare]) => {
                if (string_1.isNullOrWhiteSpace(value)) {
                    return true;
                }
                if (isNumeric(value) && isNumeric(compare)) {
                    if (convertToNumber(value) > convertToNumber(compare)) {
                        return true;
                    }
                }
                return `must be greater than ${compare}`;
            }));
            vee_validate_1.defineRule("gte", ((value, [compare]) => {
                if (string_1.isNullOrWhiteSpace(value)) {
                    return true;
                }
                if (isNumeric(value) && isNumeric(compare)) {
                    if (convertToNumber(value) >= convertToNumber(compare)) {
                        return true;
                    }
                }
                return `must not be less than ${compare}`;
            }));
            vee_validate_1.defineRule("lt", ((value, [compare]) => {
                if (string_1.isNullOrWhiteSpace(value)) {
                    return true;
                }
                if (isNumeric(value) && isNumeric(compare)) {
                    if (convertToNumber(value) < convertToNumber(compare)) {
                        return true;
                    }
                }
                return `must be less than ${compare}`;
            }));
            vee_validate_1.defineRule("lte", ((value, [compare]) => {
                if (string_1.isNullOrWhiteSpace(value)) {
                    return true;
                }
                if (isNumeric(value) && isNumeric(compare)) {
                    if (convertToNumber(value) <= convertToNumber(compare)) {
                        return true;
                    }
                }
                return `must not be more than ${compare}`;
            }));
            vee_validate_1.defineRule("datekey", (value => {
                const asString = value;
                if (!dateKey_1.default.getYear(asString)) {
                    return "must have a year";
                }
                if (!dateKey_1.default.getMonth(asString)) {
                    return "must have a month";
                }
                if (!dateKey_1.default.getDay(asString)) {
                    return "must have a day";
                }
                return true;
            }));
            vee_validate_1.defineRule("integer", (value) => {
                if (string_1.isNullOrWhiteSpace(value)) {
                    return true;
                }
                if (/^-?[0-9]+$/.test(String(value))) {
                    return true;
                }
                return "must be an integer value.";
            });
            vee_validate_1.defineRule("decimal", (value) => {
                if (string_1.isNullOrWhiteSpace(value)) {
                    return true;
                }
                if (/^-?[0-9]+(\.[0-9]+)?$/.test(String(value))) {
                    return true;
                }
                return "must be a decimal value.";
            });
            vee_validate_1.defineRule("ssn", (value) => {
                if (string_1.isNullOrWhiteSpace(value)) {
                    return true;
                }
                if (/^[0-9]{3}-[0-9]{2}-[0-9]{4}$/.test(String(value))) {
                    return true;
                }
                if (/^[0-9]{9}$/.test(String(value))) {
                    return true;
                }
                return "must be a valid social security number";
            });
        }
    };
});
//# sourceMappingURL=index.js.map