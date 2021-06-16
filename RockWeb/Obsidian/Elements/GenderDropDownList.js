System.register(["vue", "../Rules/Index", "./DropDownList"], function (exports_1, context_1) {
    "use strict";
    var vue_1, Index_1, DropDownList_1, Gender;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (Index_1_1) {
                Index_1 = Index_1_1;
            },
            function (DropDownList_1_1) {
                DropDownList_1 = DropDownList_1_1;
            }
        ],
        execute: function () {
            (function (Gender) {
                Gender[Gender["Unknown"] = 0] = "Unknown";
                Gender[Gender["Male"] = 1] = "Male";
                Gender[Gender["Female"] = 2] = "Female";
            })(Gender || (Gender = {}));
            exports_1("Gender", Gender);
            exports_1("default", vue_1.defineComponent({
                name: 'GenderDropDownList',
                components: {
                    DropDownList: DropDownList_1.default
                },
                props: {
                    rules: {
                        type: String,
                        default: ''
                    }
                },
                data: function () {
                    return {
                        blankValue: "" + Gender.Unknown
                    };
                },
                computed: {
                    options: function () {
                        return [
                            { key: Gender.Male.toString(), text: 'Male', value: Gender.Male.toString() },
                            { key: Gender.Female.toString(), text: 'Female', value: Gender.Female.toString() }
                        ];
                    },
                    computedRules: function () {
                        var rules = Index_1.ruleStringToArray(this.rules);
                        var notEqualRule = "notequal:" + Gender.Unknown;
                        if (rules.indexOf('required') !== -1 && rules.indexOf(notEqualRule) === -1) {
                            rules.push(notEqualRule);
                        }
                        return Index_1.ruleArrayToString(rules);
                    }
                },
                template: "\n<DropDownList label=\"Gender\" :options=\"options\" :showBlankItem=\"true\" :blankValue=\"blankValue\" :rules=\"computedRules\" />"
            }));
        }
    };
});
//# sourceMappingURL=GenderDropDownList.js.map