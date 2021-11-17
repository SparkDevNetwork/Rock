System.register(["vue", "../Rules/index", "./dropDownList"], function (exports_1, context_1) {
    "use strict";
    var vue_1, index_1, dropDownList_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (index_1_1) {
                index_1 = index_1_1;
            },
            function (dropDownList_1_1) {
                dropDownList_1 = dropDownList_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: "GenderDropDownList",
                components: {
                    DropDownList: dropDownList_1.default
                },
                props: {
                    rules: {
                        type: String,
                        default: ""
                    }
                },
                data() {
                    return {
                        blankValue: `${0}`
                    };
                },
                computed: {
                    options() {
                        return [
                            { text: "Male", value: 1..toString() },
                            { text: "Female", value: 2..toString() }
                        ];
                    },
                    computedRules() {
                        const rules = index_1.ruleStringToArray(this.rules);
                        const notEqualRule = `notequal:${0}`;
                        if (rules.indexOf("required") !== -1 && rules.indexOf(notEqualRule) === -1) {
                            rules.push(notEqualRule);
                        }
                        return index_1.ruleArrayToString(rules);
                    }
                },
                template: `
<DropDownList label="Gender" :options="options" :showBlankItem="true" :blankValue="blankValue" :rules="computedRules" />`
            }));
        }
    };
});
//# sourceMappingURL=genderDropDownList.js.map