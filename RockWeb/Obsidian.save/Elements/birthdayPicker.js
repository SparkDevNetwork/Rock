System.register(["vue", "./datePartsPicker"], function (exports_1, context_1) {
    "use strict";
    var vue_1, datePartsPicker_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (datePartsPicker_1_1) {
                datePartsPicker_1 = datePartsPicker_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: "BirthdayPicker",
                components: {
                    DatePartsPicker: datePartsPicker_1.default
                },
                template: `
<DatePartsPicker :allowFutureDates="false" :requireYear="false" />`
            }));
        }
    };
});
//# sourceMappingURL=birthdayPicker.js.map