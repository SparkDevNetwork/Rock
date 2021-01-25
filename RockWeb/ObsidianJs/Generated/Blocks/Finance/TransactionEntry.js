System.register(["../../Controls/CampusPicker.js", "../../Controls/DefinedValuePicker.js", "../../Elements/CurrencyBox.js", "../../Vendor/Vue/vue.js", "../../SystemGuid/DefinedType.js", "../../Elements/DatePicker.js", "../../Elements/RockButton.js"], function (exports_1, context_1) {
    "use strict";
    var CampusPicker_js_1, DefinedValuePicker_js_1, CurrencyBox_js_1, vue_js_1, DefinedType_js_1, DatePicker_js_1, RockButton_js_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (CampusPicker_js_1_1) {
                CampusPicker_js_1 = CampusPicker_js_1_1;
            },
            function (DefinedValuePicker_js_1_1) {
                DefinedValuePicker_js_1 = DefinedValuePicker_js_1_1;
            },
            function (CurrencyBox_js_1_1) {
                CurrencyBox_js_1 = CurrencyBox_js_1_1;
            },
            function (vue_js_1_1) {
                vue_js_1 = vue_js_1_1;
            },
            function (DefinedType_js_1_1) {
                DefinedType_js_1 = DefinedType_js_1_1;
            },
            function (DatePicker_js_1_1) {
                DatePicker_js_1 = DatePicker_js_1_1;
            },
            function (RockButton_js_1_1) {
                RockButton_js_1 = RockButton_js_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_js_1.defineComponent({
                name: 'Finance.TransactionEntry',
                components: {
                    CurrencyBox: CurrencyBox_js_1.default,
                    CampusPicker: CampusPicker_js_1.default,
                    DefinedValuePicker: DefinedValuePicker_js_1.default,
                    DatePicker: DatePicker_js_1.default,
                    RockButton: RockButton_js_1.default
                },
                data: function () {
                    return {
                        frequencyDefinedTypeGuid: DefinedType_js_1.FINANCIAL_FREQUENCY,
                        amounts: [null, null],
                        campusGuid: null,
                        frequencyTypeGuid: '',
                        giftDate: '2021-01-25'
                    };
                },
                template: "\n<div class=\"transaction-entry-v2\">\n    <h2>Your Generosity Changes Lives</h2>\n    <CurrencyBox label=\"General Fund\" v-model=\"amounts[0]\" />\n    <CurrencyBox label=\"Building Fund\" v-model=\"amounts[1]\" />\n    <CampusPicker v-model=\"campusGuid\" />\n    <DefinedValuePicker :definedTypeGuid=\"frequencyDefinedTypeGuid\" v-model=\"frequencyTypeGuid\" />\n    <DatePicker label=\"Process Gift On\" v-model=\"giftDate\" />\n    <RockButton primary>\n        Give Now\n    </RockButton>\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=TransactionEntry.js.map