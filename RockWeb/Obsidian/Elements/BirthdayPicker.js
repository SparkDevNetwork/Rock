System.register(["vue", "./DatePartsPicker"], function (exports_1, context_1) {
    "use strict";
    var vue_1, DatePartsPicker_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (DatePartsPicker_1_1) {
                DatePartsPicker_1 = DatePartsPicker_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'BirthdayPicker',
                components: {
                    DatePartsPicker: DatePartsPicker_1.default
                },
                template: "\n<DatePartsPicker :allowFutureDates=\"false\" :requireYear=\"false\" />"
            }));
        }
    };
});
//# sourceMappingURL=BirthdayPicker.js.map