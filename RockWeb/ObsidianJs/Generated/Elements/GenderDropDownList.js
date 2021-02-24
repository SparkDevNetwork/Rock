System.register(["vue", "./DropDownList"], function (exports_1, context_1) {
    "use strict";
    var vue_1, DropDownList_1, Gender;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
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
                computed: {
                    options: function () {
                        return [
                            { key: Gender.Unknown.toString(), text: '', value: Gender.Unknown.toString() },
                            { key: Gender.Male.toString(), text: 'Male', value: Gender.Male.toString() },
                            { key: Gender.Female.toString(), text: 'Female', value: Gender.Female.toString() }
                        ];
                    },
                },
                template: "\n<DropDownList label=\"Gender\" :options=\"options\" :showBlankItem=\"false\" />"
            }));
        }
    };
});
//# sourceMappingURL=GenderDropDownList.js.map